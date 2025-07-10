using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobTWComInvoiceLine))]
	sealed class JobTWComInvoiceLineTest : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<JobTWComInvoiceLine>
	{
		public void TestInteractionsBetweenAllDocumentaryUnitPriceCalulationRelatedProperties()
		{
			const string pce = "PCE";
			const string dzn = "DZN";

			var pce_dzn = Factory.New<CusRefPacks>();
			pce_dzn.RP_ConversionFactor = 12m;
			pce_dzn.RP_CommercialPack = pce;
			pce_dzn.RP_CustomsPack = dzn;

			var dzn_pce = Factory.New<CusRefPacks>();
			dzn_pce.RP_ConversionFactor = 0.083333333m;
			dzn_pce.RP_CommercialPack = dzn;
			dzn_pce.RP_CustomsPack = pce;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine1.JI_InvoiceQuantity = 218m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, "", 0m, 0m, "", 0m, 0m);

			invoiceLine1.JI_InvoiceUQ = pce;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, pce, 0m, 218m, pce, 0m, 0m);

			invoiceLine1.JI_EnteredUnitPrice = 105.4545m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, pce, 105.4545m, 218m, pce, 105.4545m, 22989.08m);

			invoiceLine1.JI_LinePrice = 22989.08;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, pce, 105.4545m, 218m, pce, 105.4545m, 22989.08m);

			invoiceLine1.AddInfoChild.TWL_DocumentaryUnitPrice = 105.45;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, pce, 105.4545m, 218m, pce, 105.45m, 22989.08m);

			invoiceLine1.AddInfoChild.TWL_DocumentaryUQ = dzn;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, pce, 105.4545m, 18.1667m, dzn, 1265.454005m, 22989.08m);

			invoiceLine1.AddInfoChild.TWL_DocumentaryUnitPrice = 1265.45;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 218m, pce, 105.4545m, 18.1667m, dzn, 1265.45m, 22989.08m);

			invoiceLine1.JI_InvoiceQuantity = 436m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 436m, pce, 105.4545m, 36.3333m, dzn, 1265.45m, 45978.16m);

			invoiceLine1.AddInfoChild.TWL_DocumentaryQty = 436m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 436m, pce, 105.4545m, 436m, dzn, 105.454495m, 45978.16m);

			invoiceLine1.AddInfoChild.TWL_DocumentaryUnitPrice = 105.4545;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 436m, pce, 105.4545m, 436m, dzn, 105.4545m, 45978.16m);

			invoiceLine1.AddInfoChild.TWL_DocumentaryQty = 0m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine1, 436m, pce, 105.4545m, 0m, dzn, 105.4545m, 45978.16m);

			invoiceLine2.JI_InvoiceQuantity = 218m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, "", 0m, 0m, "", 0m, 0m);

			invoiceLine2.JI_InvoiceUQ = pce;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, pce, 0m, 218m, pce, 0m, 0m);

			invoiceLine2.JI_LinePrice = 22989m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, pce, 105.454128m, 218m, pce, 105.454128m, 22989m);

			invoiceLine2.JI_EnteredUnitPrice = 105.4545m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, pce, 105.4545m, 218m, pce, 105.4545m, 22989.08m);

			invoiceLine2.JI_InvoiceUQ = dzn;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, dzn, 105.4545m, 2616m, pce, 8.787875m, 22989.08m);

			invoiceLine2.JI_EnteredUnitPrice = 105.45m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, dzn, 105.45m, 2616m, pce, 8.7875m, 22988.10m);

			invoiceLine2.AddInfoChild.TWL_DocumentaryUnitPrice = 8.79;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, dzn, 105.45m, 2616m, pce, 8.79m, 22988.10m);

			invoiceLine2.JI_LinePrice = 22989m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 218m, dzn, 105.45m, 2616m, pce, 8.7875m, 22989m);

			invoiceLine2.JI_InvoiceQuantity = 436m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 436m, dzn, 105.45m, 5232m, pce, 8.7875m, 45976.20m);

			invoiceLine2.JI_EnteredUnitPrice = 105.4545m;
			AssertAllDocumentaryUnitPriceCalulationRelatedProperties(invoiceLine2, 436m, dzn, 105.4545m, 5232m, pce, 8.787875m, 45978.16m);
		}

		void AssertAllDocumentaryUnitPriceCalulationRelatedProperties(JobComInvoiceLine invoiceLine, ZDecimal quantity, ZString unit, ZDecimal unitPrice, ZDecimal documentaryQuantity, ZString documentaryUnit, ZDecimal documentaryUnitPrice, ZDecimal linePrice)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(invoiceLine.JI_InvoiceQuantity), quantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals(nameof(invoiceLine.JI_InvoiceUQ), unit, invoiceLine.JI_InvoiceUQ);
				AssertEquals(nameof(invoiceLine.JI_EnteredUnitPrice), unitPrice, invoiceLine.JI_EnteredUnitPrice);
				AssertEquals(nameof(invoiceLine.AddInfoChild.TWL_DocumentaryQty), documentaryQuantity, invoiceLine.AddInfoChild.TWL_DocumentaryQty);
				AssertEquals(nameof(invoiceLine.AddInfoChild.TWL_DocumentaryUQ), documentaryUnit, invoiceLine.AddInfoChild.TWL_DocumentaryUQ);
				AssertEquals(nameof(invoiceLine.AddInfoChild.TWL_DocumentaryUnitPrice), documentaryUnitPrice, invoiceLine.AddInfoChild.TWL_DocumentaryUnitPrice);
				AssertEquals(nameof(invoiceLine.JI_LinePrice), linePrice, invoiceLine.JI_LinePrice);
			});
		}

		public void TestTWL_DocumentaryUQAttributes()
		{
			var line = Factory.New<JobTWComInvoiceLine>();
			var property = line.TWL_DocumentaryUQInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Documentary Quantity Unit", resStrings.Caption);
				AssertEquals("Medium Caption", "Doc. Qty Unit", resStrings.MediumCaption);
				AssertEquals("Short Caption", "UQ", resStrings.ShortCaption);
				AssertHasCustomAttribute<ListAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.ListDataSourceMember == "Lookups.CustomsUQList");
			});
		}

		public void TestTWL_DocumentaryQtyAttributes()
		{
			var line = Factory.New<JobTWComInvoiceLine>();
			var property = line.TWL_DocumentaryQtyInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Documentary Quantity", resStrings.Caption);
				AssertEquals("Medium Caption", "Doc. Quantity", resStrings.MediumCaption);
				AssertEquals("Short Caption", "Doc. Qty", resStrings.ShortCaption);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPlaces == 4);
				AssertHasCustomAttribute<DecimalPrecisionAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPrecision == 16);
			});
		}

		public void TestTWL_DocumentaryUnitPriceAttributes()
		{
			var line = Factory.New<JobTWComInvoiceLine>();
			var property = line.TWL_DocumentaryUnitPriceInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Documentary Unit Price", resStrings.Caption);
				AssertEquals("Short Caption", "Doc. Unit Price", resStrings.ShortCaption);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPlaces == 6);
				AssertHasCustomAttribute<DecimalPrecisionAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPrecision == 18);
			});
		}
		public void TestTWL_DeclarationUnitPriceAttributes()
		{
			var line = Factory.New<JobTWComInvoiceLine>();
			var property = line.TWL_DeclarationUnitPriceInfo;
			var resStrings = DataBoundResourceStrings.GetDataForProperty(property);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Declaration Unit Price", resStrings.Caption);
				AssertEquals("Short Caption", "Decl. Unit Price", resStrings.ShortCaption);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPlaces == 6);
				AssertHasCustomAttribute<DecimalPrecisionAttribute>(typeof(JobTWComInvoiceLine), property.Name, true, attribute
					=> attribute.DecimalPrecision == 18);
			});
		}
		public void TestTWL_AircraftPartsCategory()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			twLine.TWL_AircraftPartsCategory = "1";
			twLine.TWL_AircraftPartsCode = "12";
			twLine.TWL_AircraftPartsCategory = "2";
			AssertEquals(ZString.Empty, twLine.TWL_AircraftPartsCode);
		}

		public void TestTWL_AircraftPartsCategory_Caption()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			AssertEquals("Caption", "Category", DataBoundResourceStrings.GetDataForProperty(twLine.TWL_AircraftPartsCategoryInfo).Caption);
		}

		public void TestReportAircraftPartsIsNeeded()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			CombineAssertions(() =>
			{
				AssertEquals(false, twLine.ReportAircraftPartsIsNeeded);

				twLine.TWL_AircraftPartsCategory = "1";
				AssertEquals(true, twLine.ReportAircraftPartsIsNeeded);

				twLine.TWL_AircraftPartsCategory = ZString.Empty;
				AssertEquals(false, twLine.ReportAircraftPartsIsNeeded);

				twLine.TWL_AircraftPartsCode = "1";
				AssertEquals(true, twLine.ReportAircraftPartsIsNeeded);

				twLine.TWL_AircraftPartsCode = ZString.Empty;
				AssertEquals(false, twLine.ReportAircraftPartsIsNeeded);

				twLine.TWL_AircraftIPC = "1";
				AssertEquals(true, twLine.ReportAircraftPartsIsNeeded);

				twLine.TWL_AircraftIPC = ZString.Empty;
				AssertEquals(false, twLine.ReportAircraftPartsIsNeeded);
			});
		}

		public void TestTWL_AircraftPartsCategory_List()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			AssertHasCustomAttribute<ListAttribute>(twLine.GetType(), "TWL_AircraftPartsCategory", true, attrib => attrib.ListDataSourceMember == "Lookups.CategoryCodesOfCAAAircraftPartsList");
		}

		public void TestTWL_AircraftPartsCode_List()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			AssertHasCustomAttribute<ListAttribute>(twLine.GetType(), "TWL_AircraftPartsCode", true, attrib => attrib.ListDataSourceMember == "Lookups.CAAAircraftPartsCodesList");
		}

		public void TestTWL_AircraftPartsCode_Caption()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(twLine.TWL_AircraftPartsCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sequence", resourceStringData.Caption);
				AssertEquals("Short Caption", "Seq.", resourceStringData.ShortCaption);
			});
		}

		public void TestTWL_AircraftPartsCode_ReadOnly()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			twLine.TWL_AircraftPartsCategory = "1";
			AssertEquals(false, twLine.TWL_AircraftPartsCodeInfo.ReadOnly);

			twLine.TWL_AircraftPartsCategory = ZString.Empty;
			AssertEquals(true, twLine.TWL_AircraftPartsCodeInfo.ReadOnly);
		}

		public void TestAircraftPartsCodeReadOnly()
		{
			var twLine = Factory.New<JobTWComInvoiceLine>();
			twLine.TWL_AircraftPartsCategory = "1";
			AssertEquals(false, twLine.AircraftPartsCodeReadOnly);

			twLine.TWL_AircraftPartsCategory = ZString.Empty;
			AssertEquals(true, twLine.AircraftPartsCodeReadOnly);
		}

		public void TestInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var twLine = Factory.New<JobTWComInvoiceLine>();
			twLine.TWL_JI = line.PK;
			AssertEquals(line.PK, twLine.InvoiceLine.PK);
			AssertType<JobComInvoiceLine>(twLine.InvoiceLine);
		}

		public void TestIJobTWComInvoiceLineIsCorrectlySetup()
		{
			var data = (BusinessObject)Factory.New<Integration.Customs.TW.IJobTWComInvoiceLine>();
			AssertType<JobTWComInvoiceLine>(data);
			AssertType<JobTWComInvoiceLine>(Factory.Load(data.TablePrefix, data.PK));
		}

		protected override string ExpectedUniqueIndexName => Enterprise.ZArchitecture.Schema.JobTWComInvoiceLineSchema.Constants.Indexes.FK_UX__TWL_JI;
		protected override EnterpriseBusinessObject GetParent(JobTWComInvoiceLine bizObj) => bizObj.InvoiceLine;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.AddInfoChild;
		}
	}

	[TestedType(typeof(JobTWComInvoiceLine))]
	sealed class JobTWComInvoiceLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.AddInfoChild;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}
	}
}
