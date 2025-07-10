using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCodeCarrierIataMapping))]
	sealed class AccChargeCodeCarrierIataMappingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationAndLookups()
		{
			var accChargeCodeCarrierIataMapping = Factory.New<AccChargeCodeCarrierIataMapping>();
			AssertEquals(typeof(AccChargeCodeCarrierIataMappingValidation), accChargeCodeCarrierIataMapping.Validation.GetType());
			AssertEquals(typeof(AccChargeCodeCarrierIataMappingLookups), accChargeCodeCarrierIataMapping.Lookups.GetType());
		}

		#region AirLine Name and Code

		public void TestAirLineNameAndCode()
		{
			var carrier = Factory.New<OrgHeader>();
			var miscServ = carrier.MiscServ;

			carrier.OH_RL_NKClosestPort = "USLAX";
			carrier.OH_FullName = "TestCarrier";
			carrier.MainAddress.OA_Address1 = "TestAddress";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsAirLine = true;

			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_AirlineName1 = "Qantas Airways Limited";
			airline.RM_TwoCharacterCode = "QF";

			miscServ.OM_RM_Airline = airline.PK;
			var match = Factory.New<AccChargeCodeCarrierIataMapping>();
			match.ACI_OH_Carrier = carrier.PK;

			AssertEquals("Qantas Airways Limited", match.AirLineName);
			AssertEquals("QF", match.AirLine2CharCode);
		}

		#endregion
	}
}
