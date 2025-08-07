using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Com.Efrata.Service.Finance.Accounting.Lib.Migrations
{
    public partial class add_table_vbdocumentfile_and_alter_vbrealization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaimType",
                table: "VBRealizationDocuments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispositionType",
                table: "VBRealizationDocuments",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClaim",
                table: "VBRealizationDocuments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNo",
                table: "VBRealizationDocumentExpenditureItems",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VBRealizationDocumentFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DocumentsFileName = table.Column<string>(nullable: true),
                    DocumentsPath = table.Column<string>(nullable: true),
                    DocumentAmount = table.Column<decimal>(nullable: false),
                    VBRealizationDocumentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VBRealizationDocumentFiles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VBRealizationDocumentFiles");

            migrationBuilder.DropColumn(
                name: "ClaimType",
                table: "VBRealizationDocuments");

            migrationBuilder.DropColumn(
                name: "DispositionType",
                table: "VBRealizationDocuments");

            migrationBuilder.DropColumn(
                name: "IsClaim",
                table: "VBRealizationDocuments");

            migrationBuilder.DropColumn(
                name: "InvoiceNo",
                table: "VBRealizationDocumentExpenditureItems");
        }
    }
}
