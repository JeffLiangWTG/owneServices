using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Test
{
	internal class RateOneOffCarrierValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTTC_TransitTime()
		{
			var carrierOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg1.OH_IsShippingProvider = true;

			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();

			var possibleCarrier1 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = carrierOrg1.PK;
			possibleCarrier1.TTC_TransitTime = "abc";
			AssertHasErrors("Please enter a valid transit time from the available list.", possibleCarrier1.TTC_TransitTimeInfo);

			foreach (CodeDescriptionPair transitTimePair in possibleCarrier1.Lookups.TransitTimesList)
			{
				possibleCarrier1.TTC_TransitTime = transitTimePair.Code;
				AssertNoErrors(possibleCarrier1.TTC_TransitTimeInfo);
			}
		}

		public void TestTTC_Frequency()
		{
			var carrierOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg1.OH_IsShippingProvider = true;

			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();

			var possibleCarrier1 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = carrierOrg1.PK;
			possibleCarrier1.TTC_Frequency = -1;
			possibleCarrier1.TTC_FrequencyUnit = "daily";
			AssertHasErrors("Frequency cannot be less than 0.", possibleCarrier1.TTC_FrequencyInfo);

			possibleCarrier1.TTC_Frequency = 0;
			AssertHasErrors("Since you have specified the frequency unit, you need to also specify the frequency.", possibleCarrier1.TTC_FrequencyInfo);

			possibleCarrier1.TTC_Frequency = 1;
			AssertNoErrors(possibleCarrier1.TTC_FrequencyInfo);
		}

		public void TestTTC_FrequencyUnit()
		{
			var carrierOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg1.OH_IsShippingProvider = true;

			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();

			var possibleCarrier1 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = carrierOrg1.PK;
			possibleCarrier1.TTC_Frequency = 1;
			possibleCarrier1.TTC_FrequencyUnit = "abc";
			AssertHasErrors("Enter a valid frequency unit.", possibleCarrier1.TTC_FrequencyUnitInfo);

			possibleCarrier1.TTC_FrequencyUnit = "";
			AssertHasErrors("Please specify what frequency unit your frequency represents.", possibleCarrier1.TTC_FrequencyUnitInfo);

			foreach (CodeDescriptionPair frequencyUnitPair in possibleCarrier1.Lookups.FrequencyUnits)
			{
				if (!string.IsNullOrEmpty(frequencyUnitPair.Code))
				{
					possibleCarrier1.TTC_FrequencyUnit = frequencyUnitPair.Code;
					AssertNoErrors($"Frequency Unit: {frequencyUnitPair.Code}", possibleCarrier1.TTC_FrequencyUnitInfo);
				}
			}
		}

		public void TestTTC_OH_Carrier()
		{
			var quote = Factory.New<Quote>();
			var carrierOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg1.OH_IsShippingProvider = true;
			var carrierOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg2.OH_IsShippingProvider = true;

			var oneOffShipment = quote.OneOffQuote.AddNew();
			var possibleCarrier1 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = carrierOrg1.PK;
			AssertNoErrors(possibleCarrier1.TTC_OH_CarrierInfo);

			var possibleCarrier2 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_OH_Carrier = carrierOrg1.PK;
			CombineAssertions("GIVEN duplicate carriers THEN error message 'Carrier has already been added' should be shown", () =>
			{
				AssertNoErrors("possibleCarrier1", possibleCarrier1.TTC_OH_CarrierInfo);
				AssertHasError("possibleCarrier2", possibleCarrier2.TTC_OH_CarrierInfo, "Carrier has already been added.");
			});

			possibleCarrier2.TTC_OH_Carrier = carrierOrg2.PK;
			AssertNoErrors(possibleCarrier1.TTC_OH_CarrierInfo);

			possibleCarrier1.TTC_OH_Carrier = CargoWise.Types.ZGuid.Empty;
			possibleCarrier2.TTC_OH_Carrier = CargoWise.Types.ZGuid.Empty;
			CombineAssertions("GIVEN duplicate empty carrier THEN error message 'Carrier has already been added' should not be shown", () =>
			{
				AssertNoError("possibleCarrier1 NoError", possibleCarrier1.TTC_OH_CarrierInfo, "Carrier has already been added.");
				AssertNoError("possibleCarrier2 NoError", possibleCarrier2.TTC_OH_CarrierInfo, "Carrier has already been added.");

				AssertHasError("possibleCarrier1 HasError", possibleCarrier1.TTC_OH_CarrierInfo, "Please enter a Carrier.");
				AssertHasError("possibleCarrier2 HasError", possibleCarrier2.TTC_OH_CarrierInfo, "Please enter a Carrier.");
			});
		}
	}
}
