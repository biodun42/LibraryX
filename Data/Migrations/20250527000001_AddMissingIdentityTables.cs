using Microsoft.EntityFrameworkCore.Migrations;

namespace LibraryX.Data.Migrations
{
    public partial class AddMissingIdentityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the UserClaims table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS UserClaims (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    UserId TEXT NOT NULL,
                    ClaimType TEXT NULL,
                    ClaimValue TEXT NULL,
                    CONSTRAINT FK_UserClaims_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
                );
            ");

            // Create index on UserId
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_UserClaims_UserId ON UserClaims (UserId);
            ");

            // Check if the UserLogins table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS UserLogins (
                    LoginProvider TEXT NOT NULL,
                    ProviderKey TEXT NOT NULL,
                    ProviderDisplayName TEXT NULL,
                    UserId TEXT NOT NULL,
                    CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),
                    CONSTRAINT FK_UserLogins_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
                );
            ");
            
            // Create index on UserId
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_UserLogins_UserId ON UserLogins (UserId);
            ");

            // Check if the UserTokens table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS UserTokens (
                    UserId TEXT NOT NULL,
                    LoginProvider TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Value TEXT NULL,
                    CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),
                    CONSTRAINT FK_UserTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
                );
            ");

            // Check if the RoleClaims table exists
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS RoleClaims (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    RoleId TEXT NOT NULL,
                    ClaimType TEXT NULL,
                    ClaimValue TEXT NULL,
                    CONSTRAINT FK_RoleClaims_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES Roles (Id) ON DELETE CASCADE
                );
            ");
            
            // Create index on RoleId
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_RoleClaims_RoleId ON RoleClaims (RoleId);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // We don't want to drop tables in the Down method for safety
        }
    }
}
