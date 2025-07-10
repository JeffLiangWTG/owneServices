using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCarrierConsortiumRefVesselDependentCollection))]
	sealed class RefCarrierConsortiumRefVesselDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			RefCarrierConsortium carrierConsortium = Factory.New<RefCarrierConsortium>();
			return new RefCarrierConsortiumRefVesselDependentCollection(carrierConsortium, Factory);
		}

		public void TestRemoveAndDeleteAll()
		{
			VesselsCollection.RemoveAndDeleteAll();
			AssertEquals("Unable to delete all vessels.", 0, VesselsCollection.Count);
		}

		#region Contains
		public void TestContainsVessel()
		{
			RefVessel vessel3 = Factory.New<RefVessel>();

			Assert("Collection contains Vessel", VesselsCollection.Contains(Vessel1.PK));
			Assert("Collection does not Vessel '" + vessel3.PK.ToString() + "'", !VesselsCollection.Contains(vessel3.PK));
		}
		#endregion

		#region Load
		public void TestLoadDetectsNewVessel()
		{
			RefVessel vessel3 = Factory.New<RefVessel>();
			vessel3.RV_RG = Consortium.PK;
			vessel3.RV_Code = "Vessel3";

			VesselsCollection.Load();

			AssertEquals("Load failed to detect Vessel3.", 3, VesselsCollection.Count);
		}
		#endregion

		#region Implementation

		RefCarrierConsortium Consortium;
		RefCarrierConsortiumRefVesselDependentCollection VesselsCollection;
		RefVessel Vessel1;
		internal RefVessel Vessel2;

		protected override void SetUp()
		{
			base.SetUp();
			Consortium = Factory.New<RefCarrierConsortium>();
			VesselsCollection = Consortium.Vessels;
			Vessel1 = AddVesselToCollection("vessel1");
			Vessel2 = AddVesselToCollection("vessel2");
			AssertEquals("RefCarrierConsortiumRefVesselDependentCollection.Count", 2, VesselsCollection.Count);
		}

		RefVessel AddVesselToCollection(ZString code)
		{
			RefVessel vessel = VesselsCollection.AddNew();
			vessel.RV_Code = code;
			return vessel;
		}

		#endregion
	}
}
