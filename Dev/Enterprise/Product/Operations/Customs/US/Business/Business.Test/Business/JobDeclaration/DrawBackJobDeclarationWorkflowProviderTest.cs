using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class DrawBackWorkflowProviderTest : WorkflowProviderTest<JobDeclaration, Customs.Business.JobDeclarationProcessTaskCollection<JobDeclaration>>
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)Declaration).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { Declaration.JE_OH_Importer, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("Drawback is non persistent BO and milestones and Events tested on proper classes as a part of whole messaging process", true);
		}

		public new void TestProcessTasksCascadeDeleted()
		{
			Assert("Drawback is non persistent BO. Workflow Items will be deleted from ReconWrappedDeclaration.Delete()", true);
		}

		public void TestIDISHostImplementation()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var disHost = Declaration as IUSDISHost;
			Assert("Show DIS button", disHost.ShowDISFeatures);
			Assert("Don't need to do merger for drawback", !disHost.NeedToDoPreFormAction());
			Assert("Don't need to do merger for drawback", !disHost.DoPreFormAction());
			AssertEquals(1, disHost.ErrorMessages.Count());

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "SV9" });
			AssertEquals(0, disHost.ErrorMessages.Count());
		}

		#region Implementation

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DrawBackWorkflowDescriptorCode; }
		}

		protected override JobDeclaration GetNewBusinessObject(BusinessObjectFactory factory)
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			return Declaration;
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		protected override IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, JobDeclaration workFlowProvider)
		{
			var reloadedDeclaration = factory.Load<JobDeclaration>(workFlowProvider.PK);
			reloadedDeclaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			return reloadedDeclaration;
		}

		#endregion
	}
}
