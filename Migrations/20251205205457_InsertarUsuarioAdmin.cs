using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStoreSA.Migrations
{
    /// <inheritdoc />
    public partial class InsertarUsuarioAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "EsAdministrador", "NombreCompleto", "NombreUsuario", "PasswordHash" },
                values: new object[] { 1, true, "Administrador Sistema", "admin", "1234" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
