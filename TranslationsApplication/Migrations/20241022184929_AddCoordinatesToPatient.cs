using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedService.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Availabilities");

            migrationBuilder.AlterColumn<int>(
                name: "PatientSex",
                table: "Patients",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorPhoto",
                table: "Doctors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "DoctorPassword",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DoctorEmail",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "PatientId",
                table: "DoctorAvailabilities",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Availabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_PatientId",
                table: "DoctorAvailabilities",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailabilities_Patients_PatientId",
                table: "DoctorAvailabilities",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailabilities_Patients_PatientId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilities_PatientId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Availabilities");

            migrationBuilder.AlterColumn<int>(
                name: "PatientSex",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DoctorPhoto",
                table: "Doctors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DoctorPassword",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DoctorEmail",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Availabilities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
