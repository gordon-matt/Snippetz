using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Snippetz.Data.MySql.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "app");

        migrationBuilder.AlterDatabase()
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Roles",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<string>(type: "varchar(255)", nullable: false),
                Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Roles", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Snippets",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "longtext", nullable: false),
                Language = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                Code = table.Column<string>(type: "longtext", nullable: false),
                Docs = table.Column<string>(type: "longtext", nullable: false),
                IsPinned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                OwnerUserId = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Snippets", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Tags",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                OwnerUserId = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Tags", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Users",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<string>(type: "varchar(255)", nullable: false),
                DisplayName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                LastSeenAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "RoleClaims",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                RoleId = table.Column<string>(type: "varchar(255)", nullable: false),
                ClaimType = table.Column<string>(type: "longtext", nullable: true),
                ClaimValue = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_RoleClaims_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "app",
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "SnippetTags",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                SnippetId = table.Column<int>(type: "int", nullable: false),
                TagId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SnippetTags", x => x.Id);
                table.ForeignKey(
                    name: "FK_SnippetTags_Snippets_SnippetId",
                    column: x => x.SnippetId,
                    principalSchema: "app",
                    principalTable: "Snippets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SnippetTags_Tags_TagId",
                    column: x => x.TagId,
                    principalSchema: "app",
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "UserClaims",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                ClaimType = table.Column<string>(type: "longtext", nullable: true),
                ClaimValue = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserClaims_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "UserLogins",
            schema: "app",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true),
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_UserLogins_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "UserRoles",
            schema: "app",
            columns: table => new
            {
                UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_UserRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "app",
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserRoles_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "UserTokens",
            schema: "app",
            columns: table => new
            {
                UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                Name = table.Column<string>(type: "varchar(255)", nullable: false),
                Value = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_UserTokens_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "app",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_RoleClaims_RoleId",
            schema: "app",
            table: "RoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            schema: "app",
            table: "Roles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Snippets_OwnerUserId_IsPinned_UpdatedAt",
            schema: "app",
            table: "Snippets",
            columns: new[] { "OwnerUserId", "IsPinned", "UpdatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Snippets_OwnerUserId_Language",
            schema: "app",
            table: "Snippets",
            columns: new[] { "OwnerUserId", "Language" });

        migrationBuilder.CreateIndex(
            name: "IX_SnippetTags_SnippetId_TagId",
            schema: "app",
            table: "SnippetTags",
            columns: new[] { "SnippetId", "TagId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SnippetTags_TagId",
            schema: "app",
            table: "SnippetTags",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_OwnerUserId_Name",
            schema: "app",
            table: "Tags",
            columns: new[] { "OwnerUserId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserClaims_UserId",
            schema: "app",
            table: "UserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserLogins_UserId",
            schema: "app",
            table: "UserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_RoleId",
            schema: "app",
            table: "UserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            schema: "app",
            table: "Users",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            schema: "app",
            table: "Users",
            column: "NormalizedUserName",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RoleClaims",
            schema: "app");

        migrationBuilder.DropTable(
            name: "SnippetTags",
            schema: "app");

        migrationBuilder.DropTable(
            name: "UserClaims",
            schema: "app");

        migrationBuilder.DropTable(
            name: "UserLogins",
            schema: "app");

        migrationBuilder.DropTable(
            name: "UserRoles",
            schema: "app");

        migrationBuilder.DropTable(
            name: "UserTokens",
            schema: "app");

        migrationBuilder.DropTable(
            name: "Snippets",
            schema: "app");

        migrationBuilder.DropTable(
            name: "Tags",
            schema: "app");

        migrationBuilder.DropTable(
            name: "Roles",
            schema: "app");

        migrationBuilder.DropTable(
            name: "Users",
            schema: "app");
    }
}