using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAgendamentoWebII.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Address_Establishments_EstablishmentId",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Professionals_ProfessionalId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Service_ServiceId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Users_ClientId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Service_Category_CategoryId",
                table: "Service");

            migrationBuilder.DropForeignKey(
                name: "FK_Service_Professionals_ProfessionalId",
                table: "Service");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Service",
                table: "Service");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Address",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Service",
                newName: "Services");

            migrationBuilder.RenameTable(
                name: "Notification",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "Appointment",
                newName: "Agendamentos");

            migrationBuilder.RenameTable(
                name: "Address",
                newName: "Addresses");

            migrationBuilder.RenameIndex(
                name: "IX_Service_ProfessionalId",
                table: "Services",
                newName: "IX_Services_ProfessionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Service_CategoryId",
                table: "Services",
                newName: "IX_Services_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Agendamentos",
                newName: "ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_ServiceId",
                table: "Agendamentos",
                newName: "IX_Agendamentos_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_ProfessionalId",
                table: "Agendamentos",
                newName: "IX_Agendamentos_ProfessionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_ClientId",
                table: "Agendamentos",
                newName: "IX_Agendamentos_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Address_EstablishmentId",
                table: "Addresses",
                newName: "IX_Addresses_EstablishmentId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Professionals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Professionals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Establishments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Services",
                table: "Services",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Agendamentos",
                table: "Agendamentos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Establishments_EstablishmentId",
                table: "Addresses",
                column: "EstablishmentId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Professionals_ProfessionalId",
                table: "Agendamentos",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Services_ServiceId",
                table: "Agendamentos",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Users_ClienteId",
                table: "Agendamentos",
                column: "ClienteId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Category_CategoryId",
                table: "Services",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Professionals_ProfessionalId",
                table: "Services",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Establishments_EstablishmentId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Professionals_ProfessionalId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Services_ServiceId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Users_ClienteId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Category_CategoryId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Professionals_ProfessionalId",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Services",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Agendamentos",
                table: "Agendamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Professionals");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Professionals");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Establishments");

            migrationBuilder.RenameTable(
                name: "Services",
                newName: "Service");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notification");

            migrationBuilder.RenameTable(
                name: "Agendamentos",
                newName: "Appointment");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Address");

            migrationBuilder.RenameIndex(
                name: "IX_Services_ProfessionalId",
                table: "Service",
                newName: "IX_Service_ProfessionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Services_CategoryId",
                table: "Service",
                newName: "IX_Service_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notification",
                newName: "IX_Notification_UserId");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "Appointment",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Agendamentos_ServiceId",
                table: "Appointment",
                newName: "IX_Appointment_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Agendamentos_ProfessionalId",
                table: "Appointment",
                newName: "IX_Appointment_ProfessionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Agendamentos_ClienteId",
                table: "Appointment",
                newName: "IX_Appointment_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_EstablishmentId",
                table: "Address",
                newName: "IX_Address_EstablishmentId");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Service",
                table: "Service",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Address",
                table: "Address",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Address_Establishments_EstablishmentId",
                table: "Address",
                column: "EstablishmentId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Professionals_ProfessionalId",
                table: "Appointment",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Service_ServiceId",
                table: "Appointment",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Users_ClientId",
                table: "Appointment",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Category_CategoryId",
                table: "Service",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Professionals_ProfessionalId",
                table: "Service",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
