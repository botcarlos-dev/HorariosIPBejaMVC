using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorariosIPBejaMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSolucaoHorario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.CreateTable(
                name: "SOLUCAO_HORARIO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOLUCAO_HORARIO", x => x.Id);
                });



            migrationBuilder.AddColumn<int>(
            name: "solucaoHorarioId",
            table: "HORARIO_REFERENCIAL",
            nullable: true);

            // Adicione a chave estrangeira
            migrationBuilder.CreateIndex(
                name: "IX_HORARIO_REFERENCIAL_solucaoHorarioId",
                table: "HORARIO_REFERENCIAL",
                column: "solucaoHorarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_HORARIO_REFERENCIAL_SOLUCAO_HORARIO_solucaoHorarioId",
                table: "HORARIO_REFERENCIAL",
                column: "solucaoHorarioId",
                principalTable: "SOLUCAO_HORARIO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);



            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropForeignKey(
           name: "FK_HORARIO_REFERENCIAL_SOLUCAO_HORARIO_solucaoHorarioId",
           table: "HORARIO_REFERENCIAL");

            migrationBuilder.DropIndex(
                name: "IX_HORARIO_REFERENCIAL_solucaoHorarioId",
                table: "HORARIO_REFERENCIAL");

            migrationBuilder.DropColumn(
                name: "solucaoHorarioId",
                table: "HORARIO_REFERENCIAL");



            migrationBuilder.DropTable(
                name: "SOLUCAO_HORARIO");

            
        }
    }
}
