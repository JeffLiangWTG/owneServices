using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconWorkflowHelperTest : TestCaseWithFactory
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var recon = new ReconDeclaration(declaration);
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			var ranker = (ColumnValueRanker)((IWorkflowProvider)recon).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { declaration.JE_OH_Importer, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public void TestWorkflowType()
		{
			AssertEquals(WorkflowDescriptors.ReconWorkflowDescriptorCode, ReconWorkflowHelper.WorkflowType);
		}
	}
}
