using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBranches()
		{
			var branches = header.Lookups.Branches;
			AssertEquals(typeof(GlbBranchNotCurrentCompanyRelatedCollection), branches.GetType());
		}

		public void TestScheduleDList()
		{
			var collection = header.Lookups.ScheduleDList;
			AssertNotNull(collection);
		}

		public void TestFIRMSList()
		{
			var collection = header.Lookups.FIRMSList;
			AssertNotNull(collection);
		}

		public void TestSCACList()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_ModeOfTransportation = "1";

			var collection = header.Lookups.SCACList;
			AssertNotNull(collection);
			AssertNull(collection.AddNotificationWhenAdditionalFilterNotMetOverride);
			AssertEquals(true, collection.AdditionalFilter.IsEmpty);

			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			collection = header.Lookups.SCACList;
			AssertNotNull(collection);
			AssertNotNull(collection.AddNotificationWhenAdditionalFilterNotMetOverride);
			AssertEquals(false, collection.AdditionalFilter.IsEmpty);
			AssertEquals(true, carrier.MatchesFilter(collection.AdditionalFilter));
			AssertEquals("This carrier's mode of transportation is different to the import carrier's transport mode.", ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(carrier));
		}

		public void TestConveyanceTransportTypeList()
		{
			var list = header.Lookups.ConveyanceTransportTypeList;
			AssertEquals(3, list.Count);
			AssertEquals(TransportTypeList.Descriptions.VesselNonContainer, list.GetDescriptionFromCode(TransportTypeList.Codes.VesselNonContainer));
			AssertEquals(TransportTypeList.Descriptions.VesselContainer, list.GetDescriptionFromCode(TransportTypeList.Codes.VesselContainer));
			AssertEquals(TransportTypeList.Descriptions.Rail, list.GetDescriptionFromCode(TransportTypeList.Codes.Rail));
		}

		public void TestRefVessels()
		{
			var collection = header.Lookups.RefVessels;
			AssertNotNull(collection);
		}

		public void TestCountries()
		{
			var collection = header.Lookups.Countries;
			AssertNotNull(collection);
		}

		CusInBondHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
		}
	}
}
