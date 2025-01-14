using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicRecognitionSystem.Migrations
{
    /// <inheritdoc />
    public partial class timestampAddedAsPKFieldInSongHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongHashes",
                table: "SongHashes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongHashes",
                table: "SongHashes",
                columns: new[] { "songID", "hashID", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongHashes",
                table: "SongHashes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongHashes",
                table: "SongHashes",
                columns: new[] { "songID", "hashID" });
        }
    }
}
