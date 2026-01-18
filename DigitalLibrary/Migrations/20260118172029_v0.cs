using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalLibrary.Migrations
{
    /// <inheritdoc />
    public partial class v0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RoleClaimId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Abstract = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublishYear = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Documents_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DownloadLogs",
                columns: table => new
                {
                    DownloadLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadLogs", x => x.DownloadLogId);
                    table.ForeignKey(
                        name: "FK_DownloadLogs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DownloadLogs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadLogs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavDocs",
                columns: table => new
                {
                    FavDocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavDocs", x => x.FavDocId);
                    table.ForeignKey(
                        name: "FK_FavDocs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FavDocs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavDocs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StarRate = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewLogs",
                columns: table => new
                {
                    ViewLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewLogs", x => x.ViewLogId);
                    table.ForeignKey(
                        name: "FK_ViewLogs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ViewLogs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewLogs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, "074ef205-f0df-4873-b5bd-5fe39a1ab448", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(658), "Quản trị viên hệ thống", "Admin", "ADMIN" },
                    { 2, "72f5eef3-14bb-480d-be0c-e55e3db747c8", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(723), "Thủ thư", "Librarian", "LIBRARIAN" },
                    { 3, "d7d40a46-a0f4-4455-b5ca-cbe464b51f5a", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(725), "Giáo viên", "Teacher", "TEACHER" },
                    { 4, "4e5f9e3e-9f87-4e49-98ab-1ab221c078b3", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(728), "Sinh viên", "Student", "STUDENT" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Công nghệ thông tin" },
                    { 2, "Khoa học tự nhiên" },
                    { 3, "Khoa học xã hội" },
                    { 4, "Văn học" },
                    { 5, "Kinh tế" },
                    { 6, "Y học" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RoleClaimId", "ClaimType", "ClaimValue", "CreatedAt", "IsGranted", "RoleId" },
                values: new object[,]
                {
                    { 1, "User", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(915), true, 1 },
                    { 2, "User", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(922), true, 1 },
                    { 3, "User", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(923), true, 1 },
                    { 4, "User", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(924), true, 1 },
                    { 5, "Document", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(924), true, 1 },
                    { 6, "Document", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(926), true, 1 },
                    { 7, "Document", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(926), true, 1 },
                    { 8, "Document", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(927), true, 1 },
                    { 9, "Document", "Download", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(928), true, 1 },
                    { 10, "Document", "Upload", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(929), true, 1 },
                    { 11, "Category", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(929), true, 1 },
                    { 12, "Category", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(929), true, 1 },
                    { 13, "Category", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(930), true, 1 },
                    { 14, "Category", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(930), true, 1 },
                    { 15, "Review", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(931), true, 1 },
                    { 16, "Review", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(931), true, 1 },
                    { 17, "Review", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(932), true, 1 },
                    { 18, "Review", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(933), true, 1 },
                    { 19, "Review", "Moderate", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(933), true, 1 },
                    { 20, "Dashboard", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(934), true, 1 },
                    { 21, "Dashboard", "Export", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(934), true, 1 },
                    { 22, "System", "Configure", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(935), true, 1 },
                    { 23, "System", "Backup", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(935), true, 1 },
                    { 24, "Document", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(939), true, 2 },
                    { 25, "Document", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(940), true, 2 },
                    { 26, "Document", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(941), true, 2 },
                    { 27, "Document", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(941), true, 2 },
                    { 28, "Document", "Download", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(942), true, 2 },
                    { 29, "Document", "Upload", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(942), true, 2 },
                    { 30, "Category", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(942), true, 2 },
                    { 31, "Category", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(943), true, 2 },
                    { 32, "Category", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(943), true, 2 },
                    { 33, "Category", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(944), true, 2 },
                    { 34, "Review", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(945), true, 2 },
                    { 35, "Review", "Moderate", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(945), true, 2 },
                    { 36, "Dashboard", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(946), true, 2 },
                    { 37, "Document", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(948), true, 3 },
                    { 38, "Document", "Download", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(949), true, 3 },
                    { 39, "Document", "Upload", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(950), true, 3 },
                    { 40, "Review", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(950), true, 3 },
                    { 41, "Review", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(950), true, 3 },
                    { 42, "Review", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(951), true, 3 },
                    { 43, "Review", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(951), true, 3 },
                    { 44, "Document", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(953), true, 4 },
                    { 45, "Document", "Download", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(954), true, 4 },
                    { 46, "Review", "View", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(955), true, 4 },
                    { 47, "Review", "Create", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(955), true, 4 },
                    { 48, "Review", "Edit", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(956), true, 4 },
                    { 49, "Review", "Delete", new DateTime(2026, 1, 19, 0, 20, 29, 346, DateTimeKind.Local).AddTicks(956), true, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CategoryId",
                table: "Documents",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Title",
                table: "Documents",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadLogs_ApplicationUserId",
                table: "DownloadLogs",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadLogs_DocumentId",
                table: "DownloadLogs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadLogs_UserId",
                table: "DownloadLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavDocs_ApplicationUserId",
                table: "FavDocs",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavDocs_DocumentId",
                table: "FavDocs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FavDocs_UserId",
                table: "FavDocs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ApplicationUserId",
                table: "Reviews",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_DocumentId",
                table: "Reviews",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_ClaimType_ClaimValue",
                table: "RolePermissions",
                columns: new[] { "RoleId", "ClaimType", "ClaimValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewLogs_ApplicationUserId",
                table: "ViewLogs",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewLogs_DocumentId",
                table: "ViewLogs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewLogs_UserId",
                table: "ViewLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DownloadLogs");

            migrationBuilder.DropTable(
                name: "FavDocs");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "ViewLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
