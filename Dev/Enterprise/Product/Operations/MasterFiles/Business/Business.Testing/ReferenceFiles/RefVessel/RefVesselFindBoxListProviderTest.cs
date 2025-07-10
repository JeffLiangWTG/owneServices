using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class RefVesselFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestUsingLloyds()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "ABC LINERS";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_IsActive = true;
			Factory.Save();
			var collection = new RefVesselCollection(Factory, true);
			var provider = new RefVesselFindBoxListProvider(collection);
			AssertSame("GetBusinessObjectFromCode", vessel, provider.GetBusinessObjectFromCode("1234567"));
			AssertEquals("CodeFromDescription", "1234567", provider.CodeFromDescription("ABC LINERS"));
			AssertEquals("CodeFromPrimaryKey", "1234567", provider.CodeFromPrimaryKey(vessel.PK));
			AssertEquals("DescriptionFromCode", "ABC LINERS", provider.DescriptionFromCode("1234567"));
			AssertEquals("DescriptionFromPrimaryKey", "ABC LINERS", provider.DescriptionFromPrimaryKey(vessel.PK));
		}

		public void TestICodePropertyNameProviderMembers()
		{
			var collection = new RefVesselCollection(Factory, false);
			var provider = new RefVesselFindBoxListProvider(collection);
			AssertEquals(RefVessel.Schema.RV_Code, ((ICodePropertyNameProvider)provider).GetCodePropertyName(typeof(RefVessel)));

			collection = new RefVesselCollection(Factory, true);
			provider = new RefVesselFindBoxListProvider(collection);
			AssertEquals(RefVessel.Schema.RV_LloydsNumber, ((ICodePropertyNameProvider)provider).GetCodePropertyName(typeof(RefVessel)));
		}

		public void TestRefVesselFindBoxListProvider_ObtainsActiveVessel()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "ABC LINERS";
			vessel.RV_IsActive = true;

			Factory.Save();

			RefVesselCollection collection = new RefVesselCollection(Factory);
			RefVesselFindBoxListProvider provider = new RefVesselFindBoxListProvider(collection);

			var vesselList = provider.GetBusinessObjectsFromCodeWithoutFilter("ABC LINERS");
			var obtainedVessel = vesselList.Single() as RefVessel;

			AssertNotNull(obtainedVessel);
			Assert("Vessel is active", obtainedVessel.RV_IsActive);
			AssertEquals("The two vessels are the same", vessel.RV_Code, obtainedVessel.RV_Code);
		}

		public void TestRefVesselFindBoxListProvider_ObtainsInactiveVessel()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "ABC LINERS";
			vessel.RV_IsActive = false;

			Factory.Save();

			RefVesselCollection collection = new RefVesselCollection(Factory);
			RefVesselFindBoxListProvider provider = new RefVesselFindBoxListProvider(collection);

			var vesselList = provider.GetBusinessObjectsFromCodeWithoutFilter("ABC LINERS");
			var obtainedVessel = vesselList.Single() as RefVessel;

			AssertNotNull(obtainedVessel);
			Assert("Vessel is inactive", !obtainedVessel.RV_IsActive);
			AssertEquals("The two vessels are the same", vessel.RV_Code, obtainedVessel.RV_Code);
		}
	}
}
