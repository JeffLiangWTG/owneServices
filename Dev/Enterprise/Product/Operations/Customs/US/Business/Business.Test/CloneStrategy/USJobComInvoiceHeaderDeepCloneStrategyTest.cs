using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USJobComInvoiceHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestInvoiceHeaderDeepClone_EnsureUSOrganisationAreLoadedIfNeeded()
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "VIC";
			var impAddress1 = importer1.MainAddress;
			impAddress1.OA_Address1 = "TEST IMPORTER 1 ADDRESS";

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "CIV";
			var impAddress2 = importer2.MainAddress;
			impAddress2.OA_Address1 = "TEST IMPORTER 2 ADDRESS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_OH_Importer = importer1.PK;

			Factory.Save();

			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(importer1.PK, invoiceHeader.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals(impAddress1.PK, invoiceHeader.US_ExportUltimateConsignee.ZO_OA_Address);

			var newDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals(JobMessageTypeList.Codes.Export, newDec.JE_MessageType);
			AssertEquals(1, newDec.Invoices.Count);
			var newInvoiceHeader = newDec.Invoices[0];

			newDec.JE_OH_Importer = importer2.PK;

			AssertEquals(importer2.PK, newInvoiceHeader.US_ExportUltimateConsignee.ZO_OH_Organisation);
			AssertEquals(false, impAddress1.PK == newInvoiceHeader.US_ExportUltimateConsignee.ZO_OA_Address);
			AssertEquals(impAddress2.PK, newInvoiceHeader.US_ExportUltimateConsignee.ZO_OA_Address);
		}
	}
}
