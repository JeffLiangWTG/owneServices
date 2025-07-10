using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RangeJobMawbTest : JobMawbTest
	{
		public void TestRangeJobMawbCarrierServiceLevels_NeutralAirWaybillServiceLevelList()
		{
			RangeJobMawb mawb = Factory.NewWithValidTestData<RangeJobMawb>();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "191").PK;
			OrgCarrierServiceLevel lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "XXX";
			lvl.PL_CarrierServiceLevelDescription = "XXX";
			lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "202").PK;
			lvl = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "ZUB";
			lvl.PL_CarrierServiceLevelDescription = "ZUBIN";

			Factory.Save();

			AssertEquals("STD + ALL", 2, mawb.NeutralAirWaybillServiceLevels.Count);

			mawb.JM_Airline3DigitPrefix = "191";
			AssertEquals("Two defined service levels + STD + ALL", 4, mawb.NeutralAirWaybillServiceLevels.Count);

			mawb.JM_Airline3DigitPrefix = "202";
			AssertEquals("One defined service level + STD + ALL", 3, mawb.NeutralAirWaybillServiceLevels.Count);

			mawb.JM_Airline3DigitPrefix = "";
			AssertEquals("STD + ALL", 2, mawb.NeutralAirWaybillServiceLevels.Count);
		}

		public void TestNumberRangeStart_FixCheckDigit()
		{
			RangeMawb.MawbCount = 10;
			RangeMawb.NumberRangeStart = "10000002";

			AssertHasErrors("Expect an errors ", RangeMawb.NumberRangeStartInfo);
			AssertEquals("Should not create an end number if start number is invalid", "", RangeMawb.NumberRangeEnd);

			RangeMawb.NumberRangeStart = "10000001";

			AssertNoErrors("Expect no errors or we won't get a number", RangeMawb.NumberRangeStartInfo);
			AssertEquals("10000093", RangeMawb.NumberRangeEnd);
		}

		public void TestSave()
		{
			RangeMawb.MawbCount = 2;
			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "777";
			RangeMawb.JM_ServiceLevel = "STD";
			RangeMawb.Save();

			ZQuery filter = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, "777");
			filter.AddToFilter(JobMawbSchema.JM_ServiceLevel, "STD");
			int result = Factory.GetDatabaseCount(typeof(JobMawb), filter);

			AssertEquals(2, result);
		}

		public void TestDuplicatesExist()
		{
			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "777";

			AssertEquals("", RangeMawb.DuplicatesInSaveRange());
			RangeMawb.Save();

			Assert(!RangeMawb.DuplicatesInSaveRange().IsEmpty);
		}

		public void TestDuplicatesExistInConsol()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C000001";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = OrgCarrierServiceLevel.StandardCode;
			consol.JK_MasterBillNum = "08100000002";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			Factory.Save();

			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "081";

			Assert(!RangeMawb.DuplicatesInSaveRange().IsEmpty);
			var expectationMessage = $"Master Bill Numbers in the range '{rangeMawb.NumberRangeStart}' to '{rangeMawb.NumberRangeEnd}' have already been used in consol or shipment for Airline: {rangeMawb.JM_Airline3DigitPrefix}\r\n\r\nPlease choose a range of new numbers that does not include numbers already used.\r\n\r\nFirst Master Bill Number that exists:\r\n\t {consol.JK_MasterBillNum}";
			AssertEquals(expectationMessage, RangeMawb.DuplicatesInSaveRange());

			consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value).AddMonths(-2);
			Factory.Save();
			Assert(RangeMawb.DuplicatesInSaveRange().IsEmpty);
		}

		public void TestDuplicatesExistInShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_AWBServiceLevel = OrgCarrierServiceLevel.StandardCode;
			shipment.JS_RL_NKLoadPort = "AUBNE";
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_HouseBill = "08100000002";
			shipment.JS_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(-2);
			Factory.Save();

			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "081";

			Assert(!RangeMawb.DuplicatesInSaveRange().IsEmpty);
			var expectationMessage = $"Master Bill Numbers in the range '{rangeMawb.NumberRangeStart}' to '{rangeMawb.NumberRangeEnd}' have already been used in consol or shipment for Airline: {rangeMawb.JM_Airline3DigitPrefix}\r\n\r\nPlease choose a range of new numbers that does not include numbers already used.\r\n\r\nFirst Master Bill Number that exists:\r\n\t {shipment.JS_HouseBill}";
			AssertEquals(expectationMessage, RangeMawb.DuplicatesInSaveRange());

			shipment.JS_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value).AddMonths(-2);
			Factory.Save();
			Assert(RangeMawb.DuplicatesInSaveRange().IsEmpty);
		}

		public void TestNoDuplicatedExistWhenSameBOLExistInConsol()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C000001";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "08100000002";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			Factory.Save();

			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "081";

			Assert("No duplicated MAWB when same BOL number already existed", RangeMawb.DuplicatesInSaveRange().IsEmpty);
		}

		public void TestNoDuplicatedExistWhenSameBOLExistInShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "AUBNE";
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_HouseBill = "08100000002";
			shipment.JS_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(-2);
			Factory.Save();

			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "081";

			Assert("No duplicated MAWB when same BOL number already existed", RangeMawb.DuplicatesInSaveRange().IsEmpty);
		}

		public void TestDuplicatedExistAfterMawbRecycleDate()
		{
			var jobMawb = Factory.New<JobMawb>();

			jobMawb.JM_Airline3DigitPrefix = "777";
			jobMawb.JM_MAWB = "00000011";

			Factory.Save();

			RangeMawb.NumberRangeStart = "00000000";
			RangeMawb.NumberRangeEnd = "00000011";
			RangeMawb.JM_Airline3DigitPrefix = "777";

			Assert("Pre-condition", RangeMawb.DuplicatesInSaveRange() != "");

			jobMawb.JM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value - 1);
			Factory.Save();

			AssertEquals("No duplicates as JobMawb created event is before recycle period", "", RangeMawb.DuplicatesInSaveRange());
		}

		#region Implementation

		RangeJobMawb RangeMawb
		{
			get { return rangeMawb ?? (rangeMawb = Factory.NewWithValidTestData<RangeJobMawb>()); }
		}

		RangeJobMawb rangeMawb;

		#endregion
	}
}
