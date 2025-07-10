using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class NonPersistentExportAWBOtherChargeTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new NonPersistentExportAWBOtherCharge(null));
			AssertNoExceptionThrown(() => new NonPersistentExportAWBOtherCharge(Factory.New<MockExportAWBHeader>()));
		}

		public void TestToPersistentCharge()
		{
			NonPersistentExportAWBOtherCharge charge = new NonPersistentExportAWBOtherCharge(Factory.New<MockExportAWBHeader>());

			charge.ChargeCode = Core.Constants.AWB.ChargeCodes.RA;
			charge.EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			charge.PPDCLT = Core.Constants.AWB.PPDCollect.Prepaid;
			charge.Amount = 200m;

			ExportAWBOtherCharges persistentCharge = charge.ToPersistentCharge();
			AssertEquals(Factory, persistentCharge.Factory);
			AssertEquals(Core.Constants.AWB.ChargeCodes.RA, persistentCharge.EO_ChargeCode);
			AssertEquals("Dangerous goods physical/ documentary inspection", persistentCharge.EO_ChargeDescription);
			AssertEquals(Core.Constants.AWB.EntitlementCode.Agent, persistentCharge.EO_EntitlementCode);
			AssertEquals(Core.Constants.AWB.PPDCollect.Prepaid, persistentCharge.EO_PPDCLT);
			AssertEquals(200m, persistentCharge.EO_Amount);
			AssertEquals(false, persistentCharge.HasChanges);

			charge.ChargeCode = Core.Constants.AWB.ChargeCodes.XD;
			charge.EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			charge.PPDCLT = Core.Constants.AWB.PPDCollect.Collect;
			charge.Amount = 100m;

			persistentCharge = charge.ToPersistentCharge();
			AssertEquals(Factory, persistentCharge.Factory);
			AssertEquals(Core.Constants.AWB.ChargeCodes.XD, persistentCharge.EO_ChargeCode);
			AssertEquals("War risk", persistentCharge.EO_ChargeDescription);
			AssertEquals(Core.Constants.AWB.EntitlementCode.Carrier, persistentCharge.EO_EntitlementCode);
			AssertEquals(Core.Constants.AWB.PPDCollect.Collect, persistentCharge.EO_PPDCLT);
			AssertEquals(100m, persistentCharge.EO_Amount);
			AssertEquals(false, persistentCharge.HasChanges);
		}

		public void TestToPersistentChargeOverflowingDescription()
		{
			NonPersistentExportAWBOtherCharge charge = new NonPersistentExportAWBOtherCharge(Factory.New<MockExportAWBHeader>());
			charge.ChargeDescription = "Very loooooooooooooooooooooooooooog charge description with another few loooooooooooooooooooooooooooong words";

			ExportAWBOtherCharges persistentCharge = charge.ToPersistentCharge();
			AssertEquals("prerequisite", 80, persistentCharge.EO_ChargeDescriptionInfo.MaxLength);
			AssertEquals("Very loooooooooooooooooooooooooooog charge description with another few looooooo", persistentCharge.EO_ChargeDescription);
		}

		public void TestEquals()
		{
			AssertEquals<ShipmentExportAWBOtherCharges>();
			AssertEquals<ConsolExportAWBOtherCharges>();
		}

		void AssertEquals<T>() where T : ExportAWBOtherCharges
		{
			NonPersistentExportAWBOtherCharge charge = new NonPersistentExportAWBOtherCharge(Factory.New<MockExportAWBHeader>());

			charge.ChargeCode = Core.Constants.AWB.ChargeCodes.RA;
			charge.ChargeDescription = "ZZZ";
			charge.EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			charge.PPDCLT = Core.Constants.AWB.PPDCollect.Prepaid;
			charge.Amount = 200m;

			T persistentCharge = Factory.New<T>();
			persistentCharge.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.RA;
			persistentCharge.EO_ChargeDescription = "ZZZ";
			persistentCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			persistentCharge.EO_PPDCLT = Core.Constants.AWB.PPDCollect.Prepaid;
			persistentCharge.EO_Amount = 200m;

			AssertEquals(true, charge.Equals(persistentCharge));

			charge.ChargeDescription = "XXX";
			AssertEquals(true, charge.Equals(persistentCharge));

			charge.ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			AssertEquals(false, charge.Equals(persistentCharge));

			charge.ChargeCode = persistentCharge.EO_ChargeCode;
			charge.EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			AssertEquals(false, charge.Equals(persistentCharge));

			charge.EntitlementCode = persistentCharge.EO_EntitlementCode;
			charge.PPDCLT = Core.Constants.AWB.PPDCollect.Collect;
			AssertEquals(false, charge.Equals(persistentCharge));

			charge.PPDCLT = persistentCharge.EO_PPDCLT;
			charge.Amount = 99m;
			AssertEquals(false, charge.Equals(persistentCharge));

			charge.Amount = persistentCharge.EO_Amount;
			AssertEquals(true, charge.Equals(persistentCharge));
		}

		public void TestSettingChargeCodeSetsIATADescription()
		{
			NonPersistentExportAWBOtherCharge charge = new NonPersistentExportAWBOtherCharge(Factory.New<MockExportAWBHeader>());
			AssertEquals(ZString.Empty, charge.ChargeDescription);

			charge.ChargeCode = Core.Constants.AWB.ChargeCodes.RA;
			AssertEquals("Dangerous goods physical/ documentary inspection", charge.ChargeDescription);

			charge.ChargeCode = Core.Constants.AWB.ChargeCodes.XD;
			AssertEquals("War risk", charge.ChargeDescription);
		}
	}
}
