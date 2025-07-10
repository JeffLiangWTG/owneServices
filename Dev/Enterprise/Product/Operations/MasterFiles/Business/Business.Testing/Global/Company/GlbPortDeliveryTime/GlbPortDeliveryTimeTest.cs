using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPortDeliveryTime))]
	sealed class GlbPortDeliveryTimeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaults()
		{
			AssertEquals("Default company should be CurrentCompany", Env.CurrentCompany.PK, ((GlbPortDeliveryTime)GetNewBusinessObject()).G1_GC_Company);
		}

		public void TestHumanReadableNameCore()
		{
			var glbPortDeliverTime = Factory.NewWithValidTestData<GlbPortDeliveryTime>();
			glbPortDeliverTime.G1_RL_NKDischargePort = "SYD";
			glbPortDeliverTime.G1_RL_NKDestinationPort = "WHG";

			AssertEquals("Port Delivery Time - SYD - WHG", glbPortDeliverTime.HumanReadableName);
		}
	}
}
