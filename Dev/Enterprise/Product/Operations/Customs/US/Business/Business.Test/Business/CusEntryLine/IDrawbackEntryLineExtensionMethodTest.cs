using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IDrawbackEntryLineExtensionMethodTest : TestCaseWithFactory
	{
		public void TestGetImportUQToDefault()
		{
			entryLine.RandomLine.JI_InvoiceUQ = "KG";
			AssertEquals(ZString.Empty, entryLine.GetImportUQToDefault());
			entryLine.RandomLine.JI_InvoiceQuantity = 100m;
			AssertEquals("KG", entryLine.GetImportUQToDefault());
			entryLine.RandomLine.JI_InvoiceQuantity = 0m;
			entryLine.RandomLine.JI_CustomsUnitQty = "T";
			AssertEquals("T", entryLine.GetImportUQToDefault());
			entryLine.RandomLine.JI_InvoiceUQ = ZString.Empty;
			AssertEquals("T", entryLine.GetImportUQToDefault());
			entryLine.RandomLine.JI_CustomsQuantity = 50m;
			AssertEquals("T", entryLine.GetImportUQToDefault());
		}

		public void TestGetImportQuantityToDefault()
		{
			entryLine.RandomLine.JI_InvoiceUQ = "KG";
			entryLine.RandomLine.JI_InvoiceQuantity = 100m;
			entryLine.RandomLine.JI_CustomsUnitQty = "T";
			entryLine.RandomLine.JI_CustomsQuantity = 50m;
			AssertEquals(0m, entryLine.GetImportQuantityToDefault("G"));
			AssertEquals(100m, entryLine.GetImportQuantityToDefault("KG"));
			AssertEquals(50m, entryLine.GetImportQuantityToDefault("T"));
		}

		public void TestGetImportSecondUQAndQuantityToDefault()
		{
			entryLine.RandomLine.US_SupUQ2 = "KG";
			entryLine.RandomLine.US_SupQty2 = 100m;
			entryLine.RandomLine.JI_CustomsSecondUnitQty = "T";
			entryLine.RandomLine.JI_CustomsSecondQuantity = 50m;
			entryLine.US_SupLine = false;
			AssertEquals("T", entryLine.GetImportSecondUQToDefault());
			AssertEquals(50m, entryLine.GetImportSecondQuantityToDefault());
			entryLine.US_SupLine = true;
			AssertEquals("KG", entryLine.GetImportSecondUQToDefault());
			AssertEquals(100m, entryLine.GetImportSecondQuantityToDefault());
		}

		public void TestGetImportThirdUQAndQuantityToDefault()
		{
			entryLine.RandomLine.US_SupUQ3 = "KG";
			entryLine.RandomLine.US_SupQty3 = 100m;
			entryLine.RandomLine.JI_CustomsThirdUnitQty = "T";
			entryLine.RandomLine.JI_CustomsThirdQuantity = 50m;
			entryLine.US_SupLine = false;
			AssertEquals("T", entryLine.GetImportThirdUQToDefault());
			AssertEquals(50m, entryLine.GetImportThirdQuantityToDefault());
			entryLine.US_SupLine = true;
			AssertEquals("KG", entryLine.GetImportThirdUQToDefault());
			AssertEquals(100m, entryLine.GetImportThirdQuantityToDefault());
		}

		public void TestTotalSomeFeesAmountIncludingSecondaryLines()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = dec.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_PayableMPF = 30m;
			var line2 = line1.AddSecondaryInvoiceLine();
			line2.US_PayableMPF = 300m;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var parentLine = entryHeader.MergedLines.AddNew();
			parentLine.CL_AdValoremTariff = "8466939585";
			parentLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 100m);
			parentLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 200m);
			parentLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			parentLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 200m);
			parentLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 300m);
			line1.JI_CL = parentLine.PK;
			var childLine = entryHeader.MergedLines.AddNew();
			line2.JI_CL = childLine.PK;
			childLine.US_CL_ParentLine = parentLine.PK;
			childLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Tobacco, 300m);
			childLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.OtherExcise, 400m);
			childLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 1000m);
			childLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 2000m);
			childLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 3000m);
			Factory.Save();
			var loadedParentLine = NewFactory().Load<CusEntryLine>(parentLine.PK);
			var drawbackEntryLine = (IDrawbackEntryLine)loadedParentLine;
			AssertEquals("Total tax including child lines", 1000m, parentLine.TotalTaxIncludingSecondaryLines);
			AssertEquals("Total MPF including child lines", 1100m, parentLine.TotalMPFIncludingSecondaryLines);
			AssertEquals("Total payable MPF including child lines", 330m, parentLine.TotalPayableMPFIncludingSecondaryLines);
			AssertEquals("Total HMF including child lines", 2200m, parentLine.TotalHMFIncludingSecondaryLines);
			AssertEquals("Total fees including child lines", 7600m, parentLine.TotalFeeAmountIncludingSecondaryLines);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = declaration.ActiveEntryHeaders[0].EntryLines.FirstOrDefault();
		}

		CusEntryLine entryLine;
	}
}
