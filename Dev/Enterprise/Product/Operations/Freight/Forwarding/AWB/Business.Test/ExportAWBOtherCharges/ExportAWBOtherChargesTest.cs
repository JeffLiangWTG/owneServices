using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBOtherCharges))]
	sealed class ExportAWBOtherChargesTest : EnterpriseBusinessObjectTestCaseWithListChecking<ExportAWBOtherCharges>
	{
		public void TestEO_ChargeCode()
		{
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			AssertEquals(Core.Constants.AWB.ChargeCodes.AC, OtherCharges.EO_ChargeCode);
			AssertEquals("Animal container", OtherCharges.EO_ChargeDescription);

			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.NS;
			AssertEquals(Core.Constants.AWB.ChargeCodes.NS, OtherCharges.EO_ChargeCode);
		}

		public void TestEO_Amount()
		{
			OtherCharges.EO_Amount = 12.45M;
			AssertEquals(12.45M, OtherCharges.EO_Amount);
		}

		public void TestEO_EntitlementCode()
		{
			OtherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertEquals(Core.Constants.AWB.EntitlementCode.Agent, OtherCharges.EO_EntitlementCode);
		}

		public void TestChargeCodesList()
		{
			AssertEquals(OLookUpEditType.AWBChargeCodes, OtherCharges.IATAChargeCodesList.LookupEditType);
		}

		public void TestEntitlementCodesList()
		{
			AssertEquals(OLookUpEditType.AWBEntitlementCodes, OtherCharges.EntitlementCodesList.LookupEditType);
		}

		public void TestPrepaidCollectList()
		{
			AssertEquals(OLookUpEditType.CustomType, OtherCharges.PrepayCollectList.LookupEditType);
			AssertEquals(2, OtherCharges.PrepayCollectList.Count);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, OtherCharges.PrepayCollectList[0].Code);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, OtherCharges.PrepayCollectList[1].Code);
		}

		public void TestIATADescriptionsAreNotTranslated()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(delegate(string key)
				{ return new ResourceStringData(key, "do not use this translation"); }));
				OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			}

			AssertEquals("should use English IATA description", "Animal container", OtherCharges.EO_ChargeDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ExportAWBOtherCharges>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<ExportAWBHeader>();
			var result = factory.NewWithValidTestData<ExportAWBOtherCharges>();
			result.EO_EH = header.PK;

			return result;
		}

		ExportAWBOtherCharges OtherCharges;
		protected override void SetUp()
		{
			OtherCharges = Factory.New<ExportAWBOtherCharges>();
			OtherCharges.EO_EH = Factory.New<ExportAWBHeader>().PK;
		}

		#endregion
	}
}
