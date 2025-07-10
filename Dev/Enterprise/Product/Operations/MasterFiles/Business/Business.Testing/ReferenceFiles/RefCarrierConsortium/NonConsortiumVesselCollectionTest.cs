using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NonConsortiumVesselCollection))]
	sealed class NonConsortiumVesselCollectionTest : ActiveBusinessObjectCollectionTestCase<NonConsortiumVesselCollection>
	{
		public void TestFilter()
		{
			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			RefVessel vessel1 = Factory.New<RefVessel>();
			RefVessel vessel2 = Factory.New<RefVessel>();

			vessel1.RV_RG = consortium.PK;

			NonConsortiumVesselCollection list = new NonConsortiumVesselCollection(Factory);

			Assert("Vessel1 should not be in list", !list.Contains(vessel1));
			Assert("Vessel2 should be in list", !ShippingLines.Contains(vessel2));
		}

		#region Implementation

		SeaShippingProviderCollection ShippingLines;

		protected override void SetUp()
		{
			base.SetUp();
			ShippingLines = new SeaShippingProviderCollection(Factory);
		}

		#endregion
	}
}
