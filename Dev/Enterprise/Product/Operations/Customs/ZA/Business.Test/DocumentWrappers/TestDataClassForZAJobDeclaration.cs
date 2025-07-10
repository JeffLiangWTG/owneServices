using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	abstract class TestDataClassForZAJobDeclaration : TestCaseWithFactory
	{
		public JobDeclaration Declaration;
		public ICusEntryLine EntryLine1;
		public ICusEntryLine EntryLine2;
		protected ZString storedCountry;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.SouthAfrica);
			SetUpDeclaration();
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}
		void SetUpDeclaration()
		{
			Declaration = Factory.New<JobDeclaration>();
			Declaration.JE_ExportDate = ZDateTime.Today;
			Declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			var testInstruction = Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			JobComInvoiceGroupHeader invoiceGroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			invoiceHeader.JZ_ROOCert = "123";

			//Invoice Line 1
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction.PK;
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "00";
			invoiceLine1.JI_LinePrice = 150.59M;

			var classification1pk = GetNewClassification("BTH", "Test Lookup");
			var classification2pk = GetNewClassification("BTH", "Test Lookup2");

			invoiceLine1.JI_CC = classification1pk;
			invoiceLine1.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Namibia;
			invoiceLine1.JI_PreviousEntryLineNumber = 1;

			//Invoice Line 2
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction.PK;
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "11";
			invoiceLine2.JI_LinePrice = 897.59M;

			invoiceLine2.JI_CC = classification2pk;
			invoiceLine2.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Namibia;
			invoiceLine2.JI_PreviousEntryLineNumber = 2;

			Factory.Save();

			LineMerger merger = new LineMerger(Declaration);
			merger.DoMerge();

			AssertEquals("One Customs header", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two mereged lines", 2, Declaration.CustomsEntryHeaders[0].MergedLines.Count);
			EntryLine1 = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			EntryLine2 = Declaration.CustomsEntryHeaders[0].MergedLines[1];
		}

		ZGuid GetNewClassification(ZString type, ZString code)
		{
			var classification1 = Factory.New<CusClassification>();
			classification1.CC_LookupCode = code;
			classification1.CC_ClassificationType = type;
			return classification1.PK;
		}
	}
}
