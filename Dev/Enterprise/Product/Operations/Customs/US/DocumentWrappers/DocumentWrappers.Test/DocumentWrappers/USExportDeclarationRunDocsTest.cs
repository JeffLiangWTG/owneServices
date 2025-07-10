using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	sealed class USExportDeclarationRunDocsTest : CustomsRunDocsTest
	{
		ZString storedCountry;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
				declaration.US_TransportReference = "Transportation Reference";
				declaration.US_DateOfExport = ZDateTime.Today;
				declaration.US_RN_NKCountryOfDestination = "GB";
				declaration.US_SchDExport = "3901";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 15000m;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice.US_TransactionsRelated = US.Business.YesNoDefaultList.Codes.Yes;
				invoice.US_ImportEntryNo = "12456";
				invoice.US_StateOfOrigin = "IL";
				invoice.US_RoutedTransaction = US.Business.YesNoDefaultList.Codes.Yes;
				invoice.US_HazardousCargo = US.Business.YesNoDefaultList.Codes.Yes;

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "1010101010";
				invoiceLine1.JI_Description = "DESCRIPTION 1";
				invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
				invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
				invoiceLine1.US_LicenseNo = "LCN1234";
				invoiceLine1.JI_Weight = 145m;
				invoiceLine1.JI_CustomsQuantity = 24m;
				invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
				invoiceLine1.JI_CustomsSecondQuantity = 46m;
				invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
				invoiceLine1.US_ECCN = "EC12";
				invoiceLine1.JI_LinePrice = 1500m;
				invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine1.JI_CL = entryLine.PK;

				return declaration;
			}
		}

		[ExpectNoExceptions]
		public void TestAESTransmissionRecordDocument()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "AES Transmission Record");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}
	}
}
