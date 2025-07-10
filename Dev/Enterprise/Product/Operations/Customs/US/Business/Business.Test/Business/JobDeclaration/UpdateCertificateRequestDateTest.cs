using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class UpdateCertificateRequestDateTest : TestCaseWithFactory
	{
		JobDeclaration GetDeclarationforENS()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			declaration.Factory.Save();
			return declaration;
		}

		public void TestUpdateCertificateRequestDate()
		{
			var declaration = GetDeclarationforENS();
			var expectedCertReqDate2 = declaration.US_CertReqDate;
			declaration.US_CertReqDate = ZDateTime.Empty;
			Factory.Save();

			var dataProvider = ObjectFactory.Get<Integration.Customs.US.IUpdateCertificateRequestDate>();
			dataProvider.Update(null, new ZGuid[] { declaration.PK });
			AssertEquals(expectedCertReqDate2, declaration.US_CertReqDate);
		}

		public void TestDoNotCreateTasksFromTemplate()
		{
			var declaration = GetDeclarationforENS();
			var expectedCertReqDate2 = declaration.US_CertReqDate;
			declaration.US_CertReqDate = ZDateTime.Empty;
			Factory.Save();

			var templateIMP = Factory.NewWithValidTestData<MasterFiles.Business.ProcessTaskTemplate>();
			templateIMP.P0_ProcessType = Enterprise.MasterFiles.Business.JobInvoicingConsumerTypes.Brokerage.Code;
			templateIMP.P0_SubType2 = Enterprise.MasterFiles.Business.ImportExportCodeList.Codes.Import;

			var workflowIMP = templateIMP.WorkflowItems.AddNew();
			workflowIMP.P9_Description = "IMPWORK";

			Factory.Save();

			var dataProvider = ObjectFactory.Get<Integration.Customs.US.IUpdateCertificateRequestDate>();
			dataProvider.Update(null, new ZGuid[] { declaration.PK });
			AssertEquals(expectedCertReqDate2, declaration.US_CertReqDate);

			var newFactory = new BusinessObjectFactory();
			var loaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(0, loaded.WorkflowItems.Tasks.Count);
		}
	}
}
