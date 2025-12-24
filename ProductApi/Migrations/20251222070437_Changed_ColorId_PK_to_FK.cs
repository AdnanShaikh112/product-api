using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class Changed_ColorId_PK_to_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_ColorId",
                table: "ProductColors",
                column: "ColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "ColorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors");

            migrationBuilder.DropIndex(
                name: "IX_ProductColors_ColorId",
                table: "ProductColors");
        }
    }
}
