using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	class JobDeclarationDataObjectReaderForWOTTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestApplicationCode()
		{
			var declarationDataObject = SetupDeclaration("OwnerRef", null, null);
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = ApplicationCodeList.Codes.ZATransactionOrders };

			var declarationBO = new JobDeclarationDataObjectReaderForWOT(declarationDataObject, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("BLT", declarationBO.JE_ApplicationCode);
		}

		public void TestDataImportMatchingKey()
		{
			var declarationDataObject = SetupDeclaration("OwnerRef", null, null);
			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = ApplicationCodeList.Codes.ZATransactionOrders };

			var line = SetupCommercialInvoiceLine(1, "GOODS 1", 1.1m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bag }, 1500m, ZString.Empty, 1.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 150m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
				new List<EntryReference>(new[] {
					SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423")
				}));
			line.DataImportMatchingKey = "85f5872e-6b7a-401c-984a-502f2098ca91";

			declarationDataObject.CommercialInfo = SetupCommercialInfo("All Invoices",
				null,
				new DataObjectList<CommercialInvoiceHeader>(new[]
				{
					SetupCommercialInvoiceHeaderData("INV1", null, null, 10000m, new Currency() { Code = Core.Constants.CurrencyCodes.SouthAfrica }, new ZDateTime(2023, 4, 23), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.FreeOnBoard }, 10m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1000m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, ZDecimal.Zero, ZDecimal.Zero, 100m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, null, null, null,
						new DataObjectList<CommercialInvoiceLine>(new[] { line }))
				})
			);

			var declarationBO = new JobDeclarationDataObjectReaderForWOT(declarationDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
			AssertEquals("InvoiceLines[0].JI_ParentTableCode", "WOL", declarationBO.InvoiceLines[0].JI_ParentTableCode);
			AssertEquals("InvoiceLines[0].JI_ParentID", line.DataImportMatchingKey, declarationBO.InvoiceLines[0].JI_ParentID.ToString());
			AssertEquals("InvoiceLines[0].JI_MatchingKey", ZString.Empty, declarationBO.InvoiceLines[0].JI_MatchingKey);
		}

		public void TestCommercialInvoiceHeader()
		{
			var declarationDataObject = SetupDeclaration("OwnerRef", null, null);
			var declarationReader = new JobDeclarationDataObjectReaderForWOTForTest(declarationDataObject, logger, Factory);
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeaderReader = declarationReader.CreateNewCommercialInvoiceHeaderDataObjectReader(declaration.JobComInvoiceGroupHeaders[0], new CommercialInvoiceHeader(), declarationDataObject, null);
			AssertType(typeof(CommercialInvoiceHeaderDataObjectReaderForWOT), invoiceHeaderReader);
		}

		class JobDeclarationDataObjectReaderForWOTForTest : JobDeclarationDataObjectReaderForWOT
		{
			internal JobDeclarationDataObjectReaderForWOTForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
			{
			}

			public new CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
			{
				return base.CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader);
			}
		}
	}
}
