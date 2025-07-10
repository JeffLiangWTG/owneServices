using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceRiskStatusObject))]
	public class ComplianceRiskStatusObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var complianceRiskStatus = new ComplianceRiskStatusObject("PSK", "PSK", "CLR", "INC");
			AssertEquals("PSK", complianceRiskStatus.JobRisk);
			AssertEquals("PSK", complianceRiskStatus.PartyRisk);
			AssertEquals("CLR", complianceRiskStatus.LocationRisk);
			AssertEquals("INC", complianceRiskStatus.CommodityRisk);
		}

		protected override BusinessObject GetNewBusinessObject() => new ComplianceRiskStatusObject("PSK", "PSK", "CLR", "INC");
	}
}
