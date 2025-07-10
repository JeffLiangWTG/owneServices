using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class OtherChargeTemplateTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new OtherChargeTemplate(null));
			AssertNoExceptionThrown(() => new OtherChargeTemplate(Factory.New<MockExportAWBHeader>()));
		}

		public void TestChargeDescription()
		{
			OtherChargeTemplate template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			AssertEquals(ZString.Empty, template.ChargeDescription);

			template.AccChargeCode = Factory.New<AccChargeCode>();
			template.AccChargeCode.AC_Desc = "Shazam";
			AssertEquals("Shazam", template.ChargeDescription);

			template.AccChargeCode.AC_Desc = "Pumba";
			AssertEquals("Pumba", template.ChargeDescription);
		}

		public void TestIATAChargeCode()
		{
			OtherChargeTemplate template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			AssertEquals(ZString.Empty, template.IATAChargeCode);

			template.AccChargeCode = Factory.New<AccChargeCode>();
			template.IATAChargeCodeProvider = () => "XX";

			AssertEquals("XX", template.IATAChargeCode);

			template.IATAChargeCodeProvider = () => "ZZ";
			AssertEquals("ZZ", template.IATAChargeCode);
		}

		public void TestIATAChargeCode_Override()
		{
			OtherChargeTemplate template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			AssertEquals(ZString.Empty, template.IATAChargeCode);

			template.AccChargeCode = Factory.New<AccChargeCode>();
			template.AccChargeCode.AC_IATA_ChargeCodeMap = "AA";
			template.IATAChargeCodeProvider = () => "XX";
			AssertEquals("XX", template.IATAChargeCode);

			template.IATAChargeCodeProvider = () => "ZZ";
			AssertEquals("ZZ", template.IATAChargeCode);
		}

		public void TestEntitlementCode()
		{
			OtherChargeTemplate template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			template.PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("C", template.EntitlementCode);

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Agent;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = "AS";
			template.AccChargeCode = chargeCode;
			template.IATAChargeCodeProvider = () => chargeCode.AC_IATA_ChargeCodeMap;

			AssertEquals(Core.Constants.AWB.EntitlementCode.Agent, template.EntitlementCode);

			collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			template.PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			template.AccChargeCode = chargeCode;
			template.IATAChargeCodeProvider = () => chargeCode.AC_IATA_ChargeCodeMap;

			AssertEquals(Core.Constants.AWB.EntitlementCode.Carrier, template.EntitlementCode);

			collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Split;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			template.PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			template.AccChargeCode = chargeCode;
			template.IATAChargeCodeProvider = () => chargeCode.AC_IATA_ChargeCodeMap;

			AssertEquals(Core.Constants.AWB.EntitlementCode.Split, template.EntitlementCode);
		}

		public void TestIsValid()
		{
			OtherChargeTemplate template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			AssertEquals(false, template.IsValid);

			AccChargeCode asChargeCode = Factory.New<AccChargeCode>();
			asChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AS;
			template.AccChargeCode = asChargeCode;
			template.PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			AssertEquals(true, template.IsValid);

			AccChargeCode freightChargeCode = Factory.New<AccChargeCode>();
			asChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AS;
			template.AccChargeCode = freightChargeCode;
			Env.Registry.FreightChargeCode = freightChargeCode.PK.ToGuid();

			AssertEquals(false, template.IsValid);

			template.AccChargeCode = asChargeCode;
			template.IATAChargeCodeProvider = () => asChargeCode.AC_IATA_ChargeCodeMap;

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals(false, template.IsValid);

			collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals(false, template.IsValid);

			template.PrepaidCollect = Core.Constants.PaymentType.Collect;

			AssertEquals(true, template.IsValid);
		}

		public void TestCreateCharges()
		{
			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Hide);

			collection[Core.Constants.AWB.ChargeCodes.CJ].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			collection[Core.Constants.AWB.ChargeCodes.CJ].Entitlement = Core.Constants.AWB.EntitlementCode.Agent;

			collection[Core.Constants.AWB.ChargeCodes.SS].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			collection[Core.Constants.AWB.ChargeCodes.SS].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;

			collection[Core.Constants.AWB.ChargeCodes.AC].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			collection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = Core.Constants.AWB.EntitlementCode.Split;

			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AccChargeCode asChargeCode = Factory.New<AccChargeCode>();
			asChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AS;
			asChargeCode.AC_Desc = "AS charge";

			AccChargeCode cjChargeCode = Factory.New<AccChargeCode>();
			cjChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CJ;
			cjChargeCode.AC_Desc = "CJ charge";

			AccChargeCode ssChargeCode = Factory.New<AccChargeCode>();
			ssChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.SS;
			ssChargeCode.AC_Desc = "SS charge";

			AccChargeCode acChargeCode = Factory.New<AccChargeCode>();
			acChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			acChargeCode.AC_Desc = "AC charge";

			OtherChargeTemplate template = new OtherChargeTemplate(Factory.New<MockExportAWBHeader>());
			template.PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			template.ChargeAmountProvider = () => new ZDecimal(100);
			template.CostAmountProvider = () => new ZDecimal(80);
			template.ProfitAmountProvider = () => template.ChargeAmount - template.CostAmount;
			template.AccChargeCode = asChargeCode;
			template.IATAChargeCodeProvider = () => asChargeCode.AC_IATA_ChargeCodeMap;

			AssertEquals(0, template.CreateCharges().Length);

			template.AccChargeCode = cjChargeCode;
			template.IATAChargeCodeProvider = () => cjChargeCode.AC_IATA_ChargeCodeMap;

			AssertContainsExactElementsInAnyOrder(new[] { "CJ|CJ charge|PPD|A|100" },
				GetFormattedCharges(template.CreateCharges()));

			template.AccChargeCode = ssChargeCode;
			template.IATAChargeCodeProvider = () => ssChargeCode.AC_IATA_ChargeCodeMap;

			AssertContainsExactElementsInAnyOrder(new[] { "SS|SS charge|PPD|C|100" },
				GetFormattedCharges(template.CreateCharges()));

			template.AccChargeCode = acChargeCode;
			template.IATAChargeCodeProvider = () => acChargeCode.AC_IATA_ChargeCodeMap;

			AssertContainsExactElementsInAnyOrder(new[] { "AC|AC charge|PPD|C|80", "AC|AC charge|PPD|A|20" },
				GetFormattedCharges(template.CreateCharges()));
		}

		#region Implementation

		string[] GetFormattedCharges(NonPersistentExportAWBOtherCharge[] charges)
		{
			return charges.Select(charge => string.Format("{0}|{1}|{2}|{3}|{4}",
				charge.ChargeCode, charge.ChargeDescription, charge.PPDCLT, charge.EntitlementCode, charge.Amount))
				.ToArray();
		}

		#endregion
	}
}
