using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class ProtestWorkflowProviderTest : WorkflowProviderTest<JobDeclaration, JobDeclarationProcessTaskCollection<JobDeclaration>>
	{
		public void TestGetTemplateSelectionCriteria_ForImporter()
		{
			Protest.Protestant.E2_OA_Address = GetNewOrgAddress().PK;
			Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)Protest).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { Protest.Protestant.Address.OA_OH, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ProtestWorkflowDescriptorCode;

		protected override JobDeclaration GetNewBusinessObject(BusinessObjectFactory factory) => Protest.Declaration;

		OrgAddress GetNewOrgAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "abc";

			OrgAddress address = Factory.New<OrgAddress>();
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "XXABC";
			uNLOCO.RL_RN_NKCountryCode = Country.Code;
			address.OA_OH = header.PK;
			return address;
		}

		RefCountry country;
		RefCountry Country
		{
			get
			{
				if (country == null)
				{
					country = Factory.New<RefCountry>();
					country.RN_Code = "XX";
				}
				return country;
			}
		}

		Protest protest;
		Protest Protest => protest ?? (protest = new Protest(Declaration));

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
