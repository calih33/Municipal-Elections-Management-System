using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Municipal_Elections_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    MunicipalityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ElectionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    LogoImage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.MunicipalityId);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<string>(type: "TEXT", nullable: true),
                    URL = table.Column<string>(type: "TEXT", nullable: true),
                    Image = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    PositionId = table.Column<int>(type: "INTEGER", nullable: true),
                    MunicipalityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_Candidates_Municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "MunicipalityId");
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumberOfPositions = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    MunicipalityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Positions_Municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "MunicipalityId");
                });

            migrationBuilder.InsertData(
                table: "Candidates",
                columns: new[] { "CandidateId", "Description", "FirstName", "Image", "LastName", "MunicipalityId", "Position", "PositionId", "URL" },
                values: new object[] { 1, "As a UBC Sauder School of Business graduate specializing in Finance, I've spent 15 years leading and growing a local herbal tea manufacturing operation, navigating complex food safety, labelling, and import/export regulations while ensuring full compliance with all three levels of government. But my most important role is father to four school-aged children, all born and raised right here in Richmond, because I'm doing this for their future. I firmly believe that the people are the masters, and elected officials are their servants, not the other way around. That's why I personally practice servant leadership: leadership built on humility, accountability, and listening first. City Hall should never tell residents what to accept; it should listen, respond, and serve. I'm running because Richmond belongs to all of us: the parents, workers, seniors, and students who call it home. Together, we thrive. We Are Richmond. Vote Dickens CHEUNG for Mayor.", "Dickens", "dickens_cheung.png", "Cheung", null, "Mayor", 1, "https://www.wearerichmond.ca/" });

            migrationBuilder.InsertData(
                table: "Municipalities",
                columns: new[] { "MunicipalityId", "Description", "ElectionDate", "LogoImage", "Name" },
                values: new object[] { 1, "Richmond is a dynamic and ethnically diverse urban center, blending residential, commercial, agricultural, and industrial spaces. It’s a city where the Fraser River meets the Pacific Ocean, offering an ideal environment for business, nature, and community life.", new DateTime(2026, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "richmond_logo.png", "Richmond" });

            migrationBuilder.InsertData(
                table: "Positions",
                columns: new[] { "PositionId", "Description", "MunicipalityId", "NumberOfPositions", "Type" },
                values: new object[] { 1, "The Mayor is the head of the municipal government, responsible for leading the council, representing the municipality, and ensuring the effective administration of local policies and services.", 1, 1, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_MunicipalityId",
                table: "Candidates",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_MunicipalityId",
                table: "Positions",
                column: "MunicipalityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Municipalities");
        }
    }
}
