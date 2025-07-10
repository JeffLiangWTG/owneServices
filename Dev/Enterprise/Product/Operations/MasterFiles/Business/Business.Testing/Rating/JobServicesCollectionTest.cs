using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobServicesCollectionTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			var collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "GROUP", "CODE", "DESCRIPTION"));

			var serviceInfo = collection.First();
			AssertEquals(true, serviceInfo.IsEnabled);
			AssertEquals("GROUP", serviceInfo.ChargeCodeGroup);
			AssertEquals("CODE", serviceInfo.ServiceCode);
			AssertEquals("DESCRIPTION", serviceInfo.ServiceDescription);

			collection.Add(new JobServiceInfo(false, "AAA", "BBB", "CCC"));
			serviceInfo = collection.Last();
			AssertEquals(false, serviceInfo.IsEnabled);
			AssertEquals("AAA", serviceInfo.ChargeCodeGroup);
			AssertEquals("BBB", serviceInfo.ServiceCode);
			AssertEquals("CCC", serviceInfo.ServiceDescription);
		}

		public void TestContains()
		{
			var collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "AAA", "Code1", ""));
			collection.Add(new JobServiceInfo(false, "AAA", "Code2", ""));
			collection.Add(new JobServiceInfo(true, "", "Code3", ""));
			collection.Add(new JobServiceInfo(false, "BBB", "Code1", ""));
			collection.Add(new JobServiceInfo(true, "BBB", "Code4", ""));

			Assert(collection.Contains("AAA", "Code1"));
			Assert(collection.Contains("AAA", "Code2"));
			Assert(collection.Contains("AAA", "Code3"));
			Assert(collection.Contains("", "Code3"));
			Assert(collection.Contains("XXX", "Code3"));
			Assert(collection.Contains("BBB", "Code3"));

			Assert(!collection.Contains("", ""));
			Assert(!collection.Contains("", "XXX"));
			Assert(!collection.Contains("AAA", ""));
			Assert(!collection.Contains("AAA", "Code4"));
			Assert(!collection.Contains("BBB", ""));
			Assert(!collection.Contains("BBB", "Code5"));
		}

		public void TestIsEnabled()
		{
			var collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "AAA", "Code1", ""));
			collection.Add(new JobServiceInfo(false, "AAA", "Code2", ""));
			collection.Add(new JobServiceInfo(true, "", "Code3", ""));
			collection.Add(new JobServiceInfo(false, "BBB", "Code1", ""));
			collection.Add(new JobServiceInfo(true, "BBB", "Code4", ""));

			Assert(collection.IsEnabled("AAA", "Code1"));
			Assert(!collection.IsEnabled("AAA", "Code2"));
			Assert(collection.IsEnabled("", "Code3"));
			Assert(!collection.IsEnabled("BBB", "Code1"));
			Assert(collection.IsEnabled("BBB", "Code4"));
		}

		public void TestFindService()
		{
			var serviceInfo1 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.OriginBrokerage, Core.Constants.FreightServiceType.Codes.Fumigation, "OriginBrokerage Fumigation");
			var serviceInfo2 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor, "Origin Labor");
			var serviceInfo3 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeSubGroupList.Labor, "OriginBrokerage Labor");

			var collection = new JobServicesCollection();
			collection.Add(serviceInfo1);

			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerage;
			chargeCode1.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerage;
			chargeCode2.AC_ChargeSubGroup = ChargeCodeSubGroupList.Labor;

			var foundServices = collection.FindServices(chargeCode1).ToList();
			AssertEquals(1, foundServices.Count);
			AssertEquals(serviceInfo1, foundServices.FirstOrDefault());
			AssertEquals(false, collection.FindServices(chargeCode2).Any());

			collection.Add(serviceInfo2);

			foundServices = collection.FindServices(chargeCode1).ToList();
			AssertEquals(1, foundServices.Count);
			AssertEquals(serviceInfo1, foundServices.FirstOrDefault());

			foundServices = collection.FindServices(chargeCode2).ToList();
			AssertEquals(1, foundServices.Count);
			AssertEquals("Fallback to GeneralizeChargeCodeGroup when given group not found", serviceInfo2, foundServices.FirstOrDefault());

			collection.Add(serviceInfo3);
			foundServices = collection.FindServices(chargeCode1).ToList();
			AssertEquals(1, foundServices.Count);
			AssertEquals(serviceInfo1, foundServices.FirstOrDefault());

			foundServices = collection.FindServices(chargeCode2).ToList();
			AssertEquals(1, foundServices.Count);
			AssertEquals("No fallback to GeneralizeChargeCodeGroup when given group is found", serviceInfo3, foundServices.FirstOrDefault());
		}

		public void TestIsEnabledOrServiceInactive()
		{
			//Previously when Service was not found for Group-SubGroup combination it was considered enabled.
			//This was done for spot quotes which should be autorated always and for Warehose guys who want to autorate STG service as measure and don't add it to IAutoRating.SpecialServices collection.
			//
			//Since now we also say that Service is disabled when it is not found for Group-SubGroup combination but IS found for SubGroup only match (so there's another Group in the pair).
			//This is done because, for example, user can add FUM Service only once to the Services grid. And it is impossible to specify charge Group in the UI. Shipment will later decide is it performed at ORG or DST. Both ORG-FUM and DST-FUM rates can be present while autorating. But we don't want fumigation to be charged twice (both rates apply) in this case.
			//If one would like one Service to be charged for serevral groups (for example FUM to be charged both at ORG and at DST) then both pairs will need to be added to this collection. Atm this can only be done in code.

			JobServicesCollection specialServices = new JobServicesCollection();
			specialServices.Add(new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal, "Origin Demurrage"));
			specialServices.Add(new JobServiceInfo(false, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage"));

			AccChargeCode code1 = Factory.New<AccChargeCode>();
			code1.AC_Code = "CH1";
			code1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			code1.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;

			AccChargeCode code2 = Factory.New<AccChargeCode>();
			code2.AC_Code = "CH2";
			code2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			code2.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			AccChargeCode code3 = Factory.New<AccChargeCode>();
			code3.AC_Code = "CH3";
			code3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			code3.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			AccChargeCode code4 = Factory.New<AccChargeCode>();
			code4.AC_Code = "CH4";
			code4.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			code4.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;

			AccChargeCode code5 = Factory.New<AccChargeCode>();
			code5.AC_Code = "CH4";
			code5.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			code5.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			Assert(specialServices.IsEnabledOrServiceInactive(code1));
			Assert(!specialServices.IsEnabledOrServiceInactive(code2));
			Assert(!specialServices.IsEnabledOrServiceInactive(code3));
			Assert(!specialServices.IsEnabledOrServiceInactive(code4));
			Assert(specialServices.IsEnabledOrServiceInactive(code5));
		}
	}
}
