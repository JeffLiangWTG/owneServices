using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRateCommodityDefaultingRule))]
	public class OrgRateCommodityDefaultingRuleTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgRateCommodityDefaultingRule>();
		}
	}
}
