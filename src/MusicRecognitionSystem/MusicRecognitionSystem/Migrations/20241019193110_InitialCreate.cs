using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicRecognitionSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hashes",
                columns: table => new
                {
                    hashID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hashValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashes", x => x.hashID);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    songID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.songID);
                });

            migrationBuilder.CreateTable(
                name: "SongHashes",
                columns: table => new
                {
                    songID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hashID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    timestamp = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongHashes", x => new { x.songID, x.hashID });
                    table.ForeignKey(
                        name: "FK_SongHashes_Hashes_hashID",
                        column: x => x.hashID,
                        principalTable: "Hashes",
                        principalColumn: "hashID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SongHashes_Songs_songID",
                        column: x => x.songID,
                        principalTable: "Songs",
                        principalColumn: "songID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongHashes_hashID",
                table: "SongHashes",
                column: "hashID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongHashes");

            migrationBuilder.DropTable(
                name: "Hashes");

            migrationBuilder.DropTable(
                name: "Songs");
        }
    }
}
