using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.Environment;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ShipmentExportAWBOtherChargesValidationTest : TestExportAWBOtherChargesValidation
	{
		public void TestEntitlementCodeValidation()
		{
			var otherCharges = Factory.New<ShipmentExportAWBOtherCharges>();

			otherCharges.EO_EntitlementCode = "";
			AssertHasWarning(otherCharges.EO_EntitlementCodeInfo, "You have not entered an Entitlement.");
			AssertNoMessageErrors(otherCharges.EO_EntitlementCodeInfo);

			otherCharges.EO_EntitlementCode = "X";
			AssertHasWarning(otherCharges.EO_EntitlementCodeInfo, "You have not entered a valid code.");
			AssertNoMessageErrors(otherCharges.EO_EntitlementCodeInfo);

			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertNoWarnings(otherCharges.EO_EntitlementCodeInfo);
			AssertNoMessageErrors(otherCharges.EO_EntitlementCodeInfo);
		}

		public void TestChargeCodeValidation()
		{
			ShipmentExportAWBOtherCharges otherCharges = Factory.New<ShipmentExportAWBOtherCharges>();

			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;
			otherCharges.RunPreSaveValidation();
			AssertHasWarnings(otherCharges.EO_ChargeCodeInfo);
			AssertNoMessageErrors(otherCharges.EO_ChargeCodeInfo);

			otherCharges.EO_ChargeCode = "XX";
			otherCharges.RunPreSaveValidation();
			AssertHasWarnings(otherCharges.EO_ChargeCodeInfo);
			AssertNoMessageErrors(otherCharges.EO_ChargeCodeInfo);

			otherCharges.EO_ChargeCode = Constants.AWB.ChargeCodes.MA;
			otherCharges.RunPreSaveValidation();
			AssertNoWarnings(otherCharges.EO_ChargeCodeInfo);
			AssertNoMessageErrors(otherCharges.EO_ChargeCodeInfo);

			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = false;
			otherCharges.EO_ChargeCode = "";
			otherCharges.RunPreSaveValidation();
			AssertNoWarnings(otherCharges.EO_ChargeCodeInfo);
			AssertNoMessageErrors(otherCharges.EO_ChargeCodeInfo);

			otherCharges.EO_ChargeCode = "XX";
			otherCharges.RunPreSaveValidation();
			AssertHasWarnings(otherCharges.EO_ChargeCodeInfo);
			AssertNoMessageErrors(otherCharges.EO_ChargeCodeInfo);
		}

		public void TestCheckEO_PPDCLT()
		{
			ShipmentExportAWBOtherCharges otherCharges = Factory.New<ShipmentExportAWBOtherCharges>();
			otherCharges.EO_PPDCLT = "";
			otherCharges.Validation.ValidateEO_PPDCLT();
			AssertEquals("Should not have any errors", false, otherCharges.EO_PPDCLTInfo.HasErrors());

			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			AssertEquals("Should not have any errors", false, otherCharges.EO_PPDCLTInfo.HasErrors());

			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			AssertEquals("Should not have any errors", false, otherCharges.EO_PPDCLTInfo.HasErrors());

			otherCharges.EO_PPDCLT = "XXX";
			AssertEquals("Should have any errors", true, otherCharges.EO_PPDCLTInfo.HasErrors());
		}
	}
}
