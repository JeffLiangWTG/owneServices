using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceHeader))]
	sealed class BaseJobComInvoiceHeaderWorkflowProviderTest : WorkflowProviderTest<BaseJobComInvoiceHeader, ProcessTaskCollection<BaseJobComInvoiceHeaderProcessTask, BaseJobComInvoiceHeader>>
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, InvoiceHeader.IsImport);
			Factory.Save();
			AssertGetTemplateFilterCriteria(InvoiceHeader.JobDeclaration.JE_OH_ImporterInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForSupplier()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, InvoiceHeader.IsExport);
			Factory.Save();
			AssertGetTemplateFilterCriteria(InvoiceHeader.JobDeclaration.JE_OH_SupplierInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForBranch()
		{
			InvoiceHeader.Factory.Save();
			AssertGetTemplateFilterCriteria(InvoiceHeader.JZ_GBInfo, ProcessTaskTemplate.P0_GBInfo, GlbBranch.CurrentBranch.PK, Branch.PK, ZGuid.Empty);
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			BusinessObject.HasChanges = true;
			Factory.Save();
			AssertEquals("Commercial invoices doesn't support Tasks & Milestones.", true, ((IWorkflowProvider)BusinessObject).WorkflowItems.Count == 0);
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode;

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => nameof(BaseJobComInvoiceHeader.JZ_IncoTerm);

		protected override bool WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory => true;

		BaseJobComInvoiceHeader InvoiceHeader => BusinessObject;
	}
}
