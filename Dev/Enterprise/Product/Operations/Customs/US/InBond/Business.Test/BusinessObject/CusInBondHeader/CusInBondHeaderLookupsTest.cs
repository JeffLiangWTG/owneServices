using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHeaderTypeList()
		{
			var header = Factory.New<CusInBondHeader>();
			var list = header.Lookups.HeaderTypeList;
			AssertEquals(3, list.Count);
			AssertEquals("AMS", InBondHeaderTypeList.Descriptions.AMS, list.GetDescriptionFromCode("A"));
			AssertEquals("Full Data", InBondHeaderTypeList.Descriptions.FullData, list.GetDescriptionFromCode("N"));
			AssertEquals("Document Only", InBondHeaderTypeList.Descriptions.DocumentOnly, list.GetDescriptionFromCode("D"));
		}

		public void TestSuppliers()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("Suppliers", typeof(ConsignorCollection), header.Lookups.Suppliers.GetType());
		}

		public void TestCarrierCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.CarrierCollection;
			AssertNotNull(collection);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			collection = header.Lookups.CarrierCollection;
			AssertNotNull(collection);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirContainer;
			collection = header.Lookups.CarrierCollection;
			AssertNotNull(collection);
		}

		public void TestAirlineCollection()
		{
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";
			var refAirline2 = Factory.New<RefAirline>();
			refAirline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "";
			refAirline2.RM_TwoCharacterCode = "A2";
			refAirline2.RM_ThreeLetterCode = "BBB";
			var refAirline3 = Factory.New<RefAirline>();
			refAirline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "789";
			refAirline3.RM_TwoCharacterCode = "A3";
			refAirline3.RM_ThreeLetterCode = "";
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.AirlineCollection;
			AssertNotNull(collection);
			var airline1 = collection.FirstOrDefault(airline => airline.RM_ThreeLetterCode == "AAA");
			AssertNotNull("Airlines with a Numeric Code and Three Letter Code should be returned", airline1);
			Assert("Airline should be of the derived type 'InbondRefAirline' to support the Three Letter Code FindBox lookup", airline1 is ThreeLetterRefAirline);
			var airline2 = collection.FirstOrDefault(airline => airline.RM_ThreeLetterCode == "BBB");
			AssertNull("Airlines without a Numeric Code should not be returned", airline2);
			var airline3 = collection.FirstOrDefault(airline => airline.RM_TwoCharacterCode == "A3");
			AssertNull("Airlines without a Three Letter Code should not be returned", airline2);
		}

		public void TestCountries()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.Countries;
			AssertNotNull(collection);
		}

		public void TestFIRMSCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.FIRMSCollection;
			AssertNotNull(collection);
		}

		public void TestImportingConveyanceList()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.ImportingConveyanceList;
			AssertNotNull(collection);
		}

		public void TestScheduleDCodes()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.ScheduleDCodes;
			AssertNotNull(collection);
		}

		public void TestScheduleKCodes()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.ScheduleKCodes;
			AssertNotNull(collection);
			AssertEquals(true, collection.Where(x => x.ZZD_Code == "97103") != null);
			AssertEquals(true, collection.Where(x => x.ZZD_Code == "97102") != null);
		}

		public void TestTransportModeCodes()
		{
			var header = Factory.New<CusInBondHeader>();
			var codes = header.Lookups.TransportModeCodes;
			AssertEquals(6, codes.Count);
			AssertEquals("Vessel, non container", true, codes.ContainsCode("10"));
			AssertEquals("Vessel Containerized", true, codes.ContainsCode("11"));
			AssertEquals("Rail", true, codes.ContainsCode("20"));
			AssertEquals("Truck", true, codes.ContainsCode("30"));
			AssertEquals("Air", true, codes.ContainsCode("40"));
			AssertEquals("Pipeline", true, codes.ContainsCode("70"));
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_FTZMove = ZBool.False;
			codes = header.Lookups.TransportModeCodes;
			AssertEquals(3, codes.Count);
			AssertEquals("Truck, Non-container", true, codes.ContainsCode("30"));
			AssertEquals("Air, Non-container", true, codes.ContainsCode("40"));
			AssertEquals("Fixed Transport Installations(Includes pipeline and powerhouse)", true, codes.ContainsCode("70"));
		}

		public void TestImporterList()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Lookups.ImporterList;
			AssertNotNull(collection);
		}
	}
}
