using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMiscRequestHeaderCollection))]
	sealed class CusMiscRequestHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusMiscRequestHeaderCollection>
	{
		protected override CusMiscRequestHeaderCollection GetCollectionToTest() => new CusMiscRequestHeaderCollection(Factory, Company);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusMiscRequestHeader>();
			result.CMR_GB = Company.Branches[0].PK;
			return result;
		}
		GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = Factory.NewWithValidTestData<GlbCompany>();
					var branch = company.Branches.AddNew();
					branch.GB_Code = "~GB";
				}
				return company;
			}
		}
		GlbCompany company;
	}
}
