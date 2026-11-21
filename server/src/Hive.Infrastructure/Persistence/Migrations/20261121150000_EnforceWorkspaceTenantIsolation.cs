using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hive.Infrastructure.Persistence.Migrations;

public partial class EnforceWorkspaceTenantIsolation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1
                FROM dbo.Collections c
                LEFT JOIN dbo.WorkspaceMembers m
                    ON m.MemberId = c.CreatedByMemberId
                   AND m.WorkspaceId = c.WorkspaceId
                WHERE m.MemberId IS NULL
            )
                THROW 51000, 'Workspace isolation migration blocked: a collection creator belongs to another workspace.', 1;

            IF EXISTS (
                SELECT 1
                FROM dbo.Files f
                INNER JOIN dbo.Collections c ON c.CollectionId = f.CollectionId
                LEFT JOIN dbo.WorkspaceMembers m
                    ON m.MemberId = f.UploadedByMemberId
                   AND m.WorkspaceId = c.WorkspaceId
                WHERE m.MemberId IS NULL
            )
                THROW 51000, 'Workspace isolation migration blocked: a file uploader belongs to another workspace.', 1;

            IF EXISTS (
                SELECT 1
                FROM dbo.FileVersions v
                INNER JOIN dbo.Files f ON f.FileId = v.FileId
                INNER JOIN dbo.Collections c ON c.CollectionId = f.CollectionId
                LEFT JOIN dbo.WorkspaceMembers m
                    ON m.MemberId = v.UploadedByMemberId
                   AND m.WorkspaceId = c.WorkspaceId
                WHERE m.MemberId IS NULL
            )
                THROW 51000, 'Workspace isolation migration blocked: a file version uploader belongs to another workspace.', 1;

            IF EXISTS (
                SELECT 1
                FROM dbo.Permissions p
                LEFT JOIN dbo.Collections c ON c.CollectionId = p.CollectionId
                LEFT JOIN dbo.Files f ON f.FileId = p.FileId
                LEFT JOIN dbo.Collections fc ON fc.CollectionId = f.CollectionId
                INNER JOIN dbo.WorkspaceMembers targetMember ON targetMember.MemberId = p.WorkspaceMemberId
                INNER JOIN dbo.WorkspaceMembers grantor ON grantor.MemberId = p.GrantedByMemberId
                WHERE targetMember.WorkspaceId <> COALESCE(c.WorkspaceId, fc.WorkspaceId)
                   OR grantor.WorkspaceId <> COALESCE(c.WorkspaceId, fc.WorkspaceId)
            )
                THROW 51000, 'Workspace isolation migration blocked: a permission crosses workspace boundaries.', 1;
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER TRIGGER dbo.TR_Collections_EnforceWorkspaceIsolation
            ON dbo.Collections
            AFTER INSERT, UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    LEFT JOIN dbo.WorkspaceMembers m
                        ON m.MemberId = i.CreatedByMemberId
                       AND m.WorkspaceId = i.WorkspaceId
                    WHERE m.MemberId IS NULL
                )
                    THROW 51001, 'Collection creator must belong to the same workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Files f ON f.CollectionId = i.CollectionId
                    LEFT JOIN dbo.WorkspaceMembers uploader
                        ON uploader.MemberId = f.UploadedByMemberId
                       AND uploader.WorkspaceId = i.WorkspaceId
                    WHERE uploader.MemberId IS NULL
                )
                    THROW 51002, 'All file uploaders must belong to the collection workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Permissions p ON p.CollectionId = i.CollectionId
                    INNER JOIN dbo.WorkspaceMembers targetMember ON targetMember.MemberId = p.WorkspaceMemberId
                    INNER JOIN dbo.WorkspaceMembers grantor ON grantor.MemberId = p.GrantedByMemberId
                    WHERE targetMember.WorkspaceId <> i.WorkspaceId
                       OR grantor.WorkspaceId <> i.WorkspaceId
                )
                    THROW 51003, 'Collection permissions must belong to the collection workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Files f ON f.CollectionId = i.CollectionId
                    INNER JOIN dbo.Permissions p ON p.FileId = f.FileId
                    INNER JOIN dbo.WorkspaceMembers targetMember ON targetMember.MemberId = p.WorkspaceMemberId
                    INNER JOIN dbo.WorkspaceMembers grantor ON grantor.MemberId = p.GrantedByMemberId
                    WHERE targetMember.WorkspaceId <> i.WorkspaceId
                       OR grantor.WorkspaceId <> i.WorkspaceId
                )
                    THROW 51004, 'File permissions must belong to the parent collection workspace.', 1;
            END;
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER TRIGGER dbo.TR_Files_EnforceWorkspaceIsolation
            ON dbo.Files
            AFTER INSERT, UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Collections c ON c.CollectionId = i.CollectionId
                    LEFT JOIN dbo.WorkspaceMembers m
                        ON m.MemberId = i.UploadedByMemberId
                       AND m.WorkspaceId = c.WorkspaceId
                    WHERE m.MemberId IS NULL
                )
                    THROW 51005, 'File uploader must belong to the parent collection workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Collections c ON c.CollectionId = i.CollectionId
                    INNER JOIN dbo.FileVersions v ON v.FileId = i.FileId
                    LEFT JOIN dbo.WorkspaceMembers m
                        ON m.MemberId = v.UploadedByMemberId
                       AND m.WorkspaceId = c.WorkspaceId
                    WHERE m.MemberId IS NULL
                )
                    THROW 51006, 'File version uploader must belong to the parent collection workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Collections c ON c.CollectionId = i.CollectionId
                    INNER JOIN dbo.Permissions p ON p.FileId = i.FileId
                    INNER JOIN dbo.WorkspaceMembers targetMember ON targetMember.MemberId = p.WorkspaceMemberId
                    INNER JOIN dbo.WorkspaceMembers grantor ON grantor.MemberId = p.GrantedByMemberId
                    WHERE targetMember.WorkspaceId <> c.WorkspaceId
                       OR grantor.WorkspaceId <> c.WorkspaceId
                )
                    THROW 51007, 'File permissions must belong to the file workspace.', 1;
            END;
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER TRIGGER dbo.TR_FileVersions_EnforceWorkspaceIsolation
            ON dbo.FileVersions
            AFTER INSERT, UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Files f ON f.FileId = i.FileId
                    INNER JOIN dbo.Collections c ON c.CollectionId = f.CollectionId
                    LEFT JOIN dbo.WorkspaceMembers m
                        ON m.MemberId = i.UploadedByMemberId
                       AND m.WorkspaceId = c.WorkspaceId
                    WHERE m.MemberId IS NULL
                )
                    THROW 51008, 'File version uploader must belong to the file workspace.', 1;
            END;
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER TRIGGER dbo.TR_Permissions_EnforceWorkspaceIsolation
            ON dbo.Permissions
            AFTER INSERT, UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    LEFT JOIN dbo.Collections c ON c.CollectionId = i.CollectionId
                    LEFT JOIN dbo.Files f ON f.FileId = i.FileId
                    LEFT JOIN dbo.Collections fc ON fc.CollectionId = f.CollectionId
                    INNER JOIN dbo.WorkspaceMembers targetMember ON targetMember.MemberId = i.WorkspaceMemberId
                    INNER JOIN dbo.WorkspaceMembers grantor ON grantor.MemberId = i.GrantedByMemberId
                    WHERE targetMember.WorkspaceId <> COALESCE(c.WorkspaceId, fc.WorkspaceId)
                       OR grantor.WorkspaceId <> COALESCE(c.WorkspaceId, fc.WorkspaceId)
                )
                    THROW 51009, 'Permission target and grantor must belong to the target resource workspace.', 1;
            END;
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER TRIGGER dbo.TR_WorkspaceMembers_EnforceWorkspaceIsolation
            ON dbo.WorkspaceMembers
            AFTER UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Collections c ON c.CreatedByMemberId = i.MemberId
                    WHERE c.WorkspaceId <> i.WorkspaceId
                )
                    THROW 51010, 'A workspace member cannot be moved while they created collections in another workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Files f ON f.UploadedByMemberId = i.MemberId
                    INNER JOIN dbo.Collections c ON c.CollectionId = f.CollectionId
                    WHERE c.WorkspaceId <> i.WorkspaceId
                )
                    THROW 51011, 'A workspace member cannot be moved while they uploaded files in another workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.FileVersions v ON v.UploadedByMemberId = i.MemberId
                    INNER JOIN dbo.Files f ON f.FileId = v.FileId
                    INNER JOIN dbo.Collections c ON c.CollectionId = f.CollectionId
                    WHERE c.WorkspaceId <> i.WorkspaceId
                )
                    THROW 51012, 'A workspace member cannot be moved while they uploaded versions in another workspace.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM inserted i
                    INNER JOIN dbo.Permissions p
                        ON p.WorkspaceMemberId = i.MemberId
                        OR p.GrantedByMemberId = i.MemberId
                    LEFT JOIN dbo.Collections c ON c.CollectionId = p.CollectionId
                    LEFT JOIN dbo.Files f ON f.FileId = p.FileId
                    LEFT JOIN dbo.Collections fc ON fc.CollectionId = f.CollectionId
                    WHERE COALESCE(c.WorkspaceId, fc.WorkspaceId) <> i.WorkspaceId
                )
                    THROW 51013, 'A workspace member cannot be moved while they participate in permissions for another workspace.', 1;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_WorkspaceMembers_EnforceWorkspaceIsolation;");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Permissions_EnforceWorkspaceIsolation;");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_FileVersions_EnforceWorkspaceIsolation;");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Files_EnforceWorkspaceIsolation;");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Collections_EnforceWorkspaceIsolation;");
    }
}
