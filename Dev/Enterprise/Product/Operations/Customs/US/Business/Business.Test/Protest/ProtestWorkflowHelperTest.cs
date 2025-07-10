using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ProtestWorkflowHelperTest : TestCaseWithFactory
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			Protest.Protestant.E2_OA_Address = GetNewOrgAddress().PK;
			Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			var ranker = (ColumnValueRanker)((IWorkflowProvider)Protest).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { Protest.Protestant.Address.OA_OH, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public void TestWorkflowType()
		{
			AssertEquals(WorkflowDescriptors.ProtestWorkflowDescriptorCode, ProtestWorkflowHelper.WorkflowType);
		}

		Protest.Protest protest;
		Protest.Protest Protest => protest ?? (protest = new Protest.Protest(Declaration));

		OrgAddress GetNewOrgAddress()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "XX";

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "abc";

			OrgAddress address = Factory.New<OrgAddress>();
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "XXABC";
			uNLOCO.RL_RN_NKCountryCode = country.Code;
			address.OA_OH = header.PK;
			return address;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
