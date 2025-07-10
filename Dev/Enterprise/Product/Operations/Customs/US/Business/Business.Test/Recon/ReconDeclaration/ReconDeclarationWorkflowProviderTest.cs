using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class ReconDeclarationWorkflowProviderTest : WorkflowProviderTest<JobDeclaration, JobDeclarationProcessTaskCollection<JobDeclaration>>
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)Recon).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { Declaration.JE_OH_Importer, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ReconWorkflowDescriptorCode;

		protected override JobDeclaration GetNewBusinessObject(BusinessObjectFactory factory) => Recon.ReconWrappedJobDeclaration;

		protected override string RealTableNameForNonPersistentIWorkflowProvider => JobDeclarationSchema.Constants.TableName;

		ReconDeclaration recon;
		ReconDeclaration Recon => recon ?? (recon = new ReconDeclaration(Declaration));

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
