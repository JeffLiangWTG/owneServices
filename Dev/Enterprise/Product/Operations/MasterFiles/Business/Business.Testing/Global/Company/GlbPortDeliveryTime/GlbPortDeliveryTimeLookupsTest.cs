using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPortDeliveryTimeLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Implementation

		GlbPortDeliveryTime fDeliveryTime;
		GlbPortDeliveryTime DeliveryTime
		{
			get
			{
				if (fDeliveryTime == null)
				{
					fDeliveryTime = Factory.New<GlbPortDeliveryTime>();
				}
				return fDeliveryTime;
			}
		}

		#endregion

		public void TestFreightMode()
		{
			Assert("Freight Mode List should have at least one element", DeliveryTime.Lookups.FreightMode.Count > 0);
			AssertNotNull("Freight Mode Not Null", DeliveryTime.Lookups.FreightMode[0].Code);
		}

		public void TestJobMode()
		{
			Assert("Job Mode List should have at least one element", DeliveryTime.Lookups.JobMode.Count > 0);
			AssertNotNull("Job Mode Not Null", DeliveryTime.Lookups.JobMode[0].Code);
		}

		public void TestCompany()
		{
			AssertNotNull(Factory.New<GlbCompany>());
		}

		public void TestPorts()
		{
			AssertNotNull(Factory.New<RefUNLOCO>());
		}
	}
}
