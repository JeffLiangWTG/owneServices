using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgWhsChgAttribGrpBy))]
	sealed class OrgWhsChgAttribGrpByTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCompany()
		{
			OrgCompanyData company = Factory.New<OrgCompanyData>();
			OrgWhsChgAttribGrpBy groupby = (OrgWhsChgAttribGrpBy)GetNewBusinessObject();

			groupby.PX_OB = company.PK;
			AssertEquals(company, groupby.Company);

			groupby.PX_OB = ZGuid.Empty;
			AssertNull(groupby.Company);
		}
	}
}
