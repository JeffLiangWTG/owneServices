using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ITAndSplitDetailsCollection))]
	sealed class ITAndSplitDetailsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTotalNoOfPacks()
		{
			Bill.ITAndSplitDetails.AddNew().US_NoOfPacks = 46;
			Bill.ITAndSplitDetails.AddNew().US_NoOfPacks = 4;
			Bill.ITAndSplitDetails.AddNew().US_NoOfPacks = 10;
			AssertEquals(60, Bill.ITAndSplitDetails.TotalNoOfPacks);
		}

		public void TestFindByItNumber()
		{
			var no1 = Bill.ITAndSplitDetails.AddNew();
			no1.US_ITNumber = "1";
			var no2 = Bill.ITAndSplitDetails.AddNew();
			no2.US_ITNumber = "2";
			var no3 = Bill.ITAndSplitDetails.AddNew();
			no3.US_ITNumber = "3";
			AssertEquals(1, Bill.ITAndSplitDetails.FindByItNumber("1").Length);
			AssertEquals(no1, Bill.ITAndSplitDetails.FindByItNumber("1")[0]);
			AssertEquals(1, Bill.ITAndSplitDetails.FindByItNumber("2").Length);
			AssertEquals(no2, Bill.ITAndSplitDetails.FindByItNumber("2")[0]);
			AssertEquals(1, Bill.ITAndSplitDetails.FindByItNumber("3").Length);
			AssertEquals(no3, Bill.ITAndSplitDetails.FindByItNumber("3")[0]);
		}

		public void TestGetListOfUniqueVITNumbers()
		{
			var no1 = Bill.ITAndSplitDetails.AddNew();
			no1.US_ITNumber = "V45632131";
			var no2 = Bill.ITAndSplitDetails.AddNew();
			no2.US_ITNumber = "2";
			var no3 = Bill.ITAndSplitDetails.AddNew();
			no3.US_ITNumber = "V987652315";
			AssertEquals(2, Bill.ITAndSplitDetails.GetListOfUniqueVITNumbers().Count);
			var no4 = Bill.ITAndSplitDetails.AddNew();
			no4.US_ITNumber = "V987652315";
			var no5 = Bill.ITAndSplitDetails.AddNew();
			no5.US_ITNumber = "V987652315";
			var no6 = Bill.ITAndSplitDetails.AddNew();
			no6.US_ITNumber = "2";
			AssertEquals(2, Bill.ITAndSplitDetails.GetListOfUniqueVITNumbers().Count);
			var no7 = Bill.ITAndSplitDetails.AddNew();
			no7.US_ITNumber = "V987600015";
			AssertEquals(3, Bill.ITAndSplitDetails.GetListOfUniqueVITNumbers().Count);
		}

		public void TestRemoveSplitDetails()
		{
			var details = Bill.ITAndSplitDetails.AddNew();
			details.US_ArrivalDate = ZDateTime.Today.AddDays(2);
			details.US_CarrierCode = "APLU";
			details.US_FlightNumber = "001S";
			Bill.ITAndSplitDetails.RemoveSplitDetails();
			AssertEquals(ZDateTime.Empty, details.US_ArrivalDate);
			AssertEquals(ZString.Empty, details.US_CarrierCode);
			AssertEquals(ZString.Empty, details.US_FlightNumber);
		}

		public void TestSetDefaultsForNewElement()
		{
			var declaration = Bill.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			declaration.US_UI_NKCarrierSCAC = "APLU";
			declaration.JE_VoyageFlightNo = "001S";
			Bill.US_SESplitShip = true;
			var details = Bill.ITAndSplitDetails.AddNew();
			AssertEquals(ZDateTime.Today.AddDays(2), details.US_ArrivalDate);
			AssertEquals("APLU", details.US_CarrierCode);
			AssertEquals("001S", details.US_FlightNumber);
			var details2 = Bill.ITAndSplitDetails.AddNew();
			AssertEquals(ZDateTime.Empty, details2.US_ArrivalDate);
			AssertEquals(ZString.Empty, details2.US_CarrierCode);
			AssertEquals(ZString.Empty, details2.US_FlightNumber);
		}

		public void TestSetDefaultsForNewElementWhenFTZ()
		{
			var declaration = Bill.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			declaration.US_UI_NKCarrierSCAC = "APLU";
			declaration.JE_VoyageFlightNo = "001S";
			Bill.US_SESplitShip = true;
			var details = Bill.ITAndSplitDetails.AddNew();
			AssertEquals(ZDateTime.Today.AddDays(2), details.US_ArrivalDate);
			AssertEquals("APLU", details.US_CarrierCode);
			AssertEquals("001S", details.US_FlightNumber);
			var details2 = Bill.ITAndSplitDetails.AddNew();
			AssertEquals(ZDateTime.Empty, details2.US_ArrivalDate);
			AssertEquals(ZString.Empty, details2.US_CarrierCode);
			AssertEquals(ZString.Empty, details2.US_FlightNumber);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ITAndSplitDetailsCollection(Bill);

		Bill bill;
		Bill Bill
		{
			get
			{
				var declaration = Factory.New<JobDeclaration>();
				return bill ?? (bill = declaration.Bills.AddNew());
			}
		}
	}
}
