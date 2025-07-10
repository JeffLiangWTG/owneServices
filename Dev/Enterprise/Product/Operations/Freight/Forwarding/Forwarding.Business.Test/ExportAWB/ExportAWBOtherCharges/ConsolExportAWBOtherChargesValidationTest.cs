namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ConsolExportAWBOtherChargesValidationTest : TestExportAWBOtherChargesValidation
	{
		public void TestCheckNoDuplicateChargeCodeAndEntitlementCodeCombinations()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var charge = awbHeader.AWBOtherCharges.AddNew();
			charge.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.MY;
			charge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertNoNotifications(charge.EO_ChargeCodeInfo);
			AssertNoNotifications(charge.EO_EntitlementCodeInfo);

			var anotherCharge = awbHeader.AWBOtherCharges.AddNew();
			anotherCharge.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.PA;
			anotherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertNoNotifications(anotherCharge.EO_ChargeCodeInfo);
			AssertNoNotifications(anotherCharge.EO_EntitlementCodeInfo);

			anotherCharge.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.MY;
			anotherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			AssertNoNotifications(anotherCharge.EO_ChargeCodeInfo);
			AssertNoNotifications(anotherCharge.EO_EntitlementCodeInfo);

			anotherCharge.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.MY;
			anotherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			AssertHasNotifications("Duplicate combinations of Other Charge Code and Entitlement Code are not permitted when sending AWB electronically. Other charges can be grouped by switching the following registry setting to YES: Freight > AWB > MAWB > Other Charges > Group By IATA Code.", anotherCharge.EO_ChargeCodeInfo);
			AssertHasNotifications("Duplicate combinations of Other Charge Code and Entitlement Code are not permitted when sending AWB electronically. Other charges can be grouped by switching the following registry setting to YES: Freight > AWB > MAWB > Other Charges > Group By IATA Code.", anotherCharge.EO_EntitlementCodeInfo);
		}
	}
}
