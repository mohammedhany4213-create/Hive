using Hive.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hive.Infrastructure.Persistence.Migrations;

public partial class EnforcePermissionTargetType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddCheckConstraint(
            name: "CK_Permissions_TargetMatchesPermissionType",
            table: "Permissions",
            sql: "([CollectionId] IS NOT NULL AND [PermissionType] BETWEEN 1 AND 4) OR ([FileId] IS NOT NULL AND [PermissionType] BETWEEN 5 AND 7)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Permissions_TargetMatchesPermissionType",
            table: "Permissions");
    }
}
