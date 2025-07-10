using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	class CommercialInvoiceHeaderDataObjectReaderForWOTTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestBondedWarehouseProperties()
		{
			var declaration = Factory.New<JobDeclaration>();

			var commercialInvoiceLine1 = SetupCommercialInvoiceLine(1, "GOODS 1", 1.1m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bag }, 1500m, ZString.Empty, 1.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 150m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
				new List<EntryReference>(new[] {
					SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423")
				}));
			var commercialInvoiceLine2 = SetupCommercialInvoiceLine(1, "GOODS 1", 1.1m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bag }, 1500m, ZString.Empty, 1.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 150m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
				new List<EntryReference>(new[] {
					SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423")
				}));
			const string guid = "8f8f79df-f1f5-44d2-9a0b-e3b0714adbb3";
			commercialInvoiceLine1.DataImportMatchingKey = guid;
			commercialInvoiceLine2.DataImportMatchingKey = guid;
			var commercialInvoiceHeader = SetupCommercialInvoiceHeaderData("INV1", null, null, 10000m, new Currency() { Code = Core.Constants.CurrencyCodes.SouthAfrica }, new ZDateTime(2023, 4, 23), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.FreeOnBoard }, 10m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1000m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, ZDecimal.Zero, ZDecimal.Zero, 100m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, null, null, null,
				new DataObjectList<CommercialInvoiceLine>(new[] { commercialInvoiceLine1, commercialInvoiceLine2 }));

			var reader = new CommercialInvoiceHeaderDataObjectReaderForWOT(commercialInvoiceHeader, logger, new UniversalDataObjectReaderHelper(Factory), declaration.JobComInvoiceGroupHeaders[0]);
			var dataObject = reader.ReadIntoBusinessObject();

			HtmlAssertNotNull("ReadIntoBusinessObject() returned null. This should work with multiple uses of the same DataImportMatchingKey for ZAA Warehouse Operator Transaction shipments" +
				(logger.HasErrors ? System.Environment.NewLine + logger.Logs : string.Empty), dataObject, isHtmlMessage: false);
			AssertEquals(2, dataObject.InvoiceLines.Count);
			var line1 = dataObject.InvoiceLines[0];
			var line2 = dataObject.InvoiceLines[1];

			AssertEquals("InvoiceLines[0].JI_ParentTableCode", "WOL", line1.JI_ParentTableCode);
			AssertEquals("InvoiceLines[0].JI_ParentID", guid, line1.JI_ParentID.ToString());
			AssertEquals("InvoiceLines[0].JI_MatchingKey", ZString.Empty, line1.JI_MatchingKey);
			AssertEquals("InvoiceLines[1].JI_ParentTableCode", "WOL", line2.JI_ParentTableCode);
			AssertEquals("InvoiceLines[1].JI_ParentID", guid, line2.JI_ParentID.ToString());
			AssertEquals("InvoiceLines[1].JI_MatchingKey", ZString.Empty, line2.JI_MatchingKey);
		}
	}
}
