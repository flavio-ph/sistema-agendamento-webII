using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAgendamentoWebII.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Agendamentos",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Agendamentos");
        }
    }
}
