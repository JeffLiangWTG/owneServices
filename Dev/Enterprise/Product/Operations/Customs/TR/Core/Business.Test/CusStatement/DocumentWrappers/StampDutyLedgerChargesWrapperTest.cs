using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(StampDutyLedgerChargesWrapper))]
	public class StampDutyLedgerChargesWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapper()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_CustomsFeesTotal = 600m;
			var statementLineCharge = statementLine.Charges.AddNew();
			statementLineCharge.B4_ReferenceNumber = "TEST01";
			statementLineCharge.B4_ChargeType = "OBS";
			statementLineCharge.B4_ChargeAmount = 50m;

			var wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);

			CombineAssertions(() =>
			{
				AssertEquals("B4_ReferenceNumber", statementLineCharge.B4_ReferenceNumber, wrapper.ChargeReferenceNumber);
				AssertEquals("Turu", StampDutyChargeTypeList.Codes.Ordino, wrapper.ChargeKind);
				AssertEquals("Tipi", StampDutyChargeTypeList.Descriptions.Ordino, wrapper.ChargeType);
				AssertEquals("DamgaVergisiMatrahi", "MAKTU", wrapper.StampDutyBaseAmount);
				AssertEquals("B4_ChargeAmount", statementLineCharge.B4_ChargeAmount, wrapper.StampDutyAmount);
				AssertEquals("DamgaVergisiOrani", "8.33333", wrapper.StampDutyRate);
			});
		}

		public void TestGetStampDutyPercentage()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_CustomsFeesTotal = CargoWise.Types.ZDecimal.Zero;

			var statementLineCharge = statementLine.Charges.AddNew();
			statementLineCharge.B4_ChargeType = "OBS";
			statementLineCharge.B4_ChargeAmount = 50m;
			var wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);

			CombineAssertions(() =>
			{
				AssertEquals("B4_ChargeAmount", "0.00000", wrapper.StampDutyRate);

				statementLine.B3_CustomsFeesTotal = 50m;
				wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);
				AssertEquals("DamgaVergisiOrani", "100.00000", wrapper.StampDutyRate);

				statementLineCharge.B4_ChargeType = "ABS";
				statementLine.B3_CustomsFeesTotal = 60m;
				wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);
				AssertEquals("DamgaVergisiOrani", "83.33333", wrapper.StampDutyRate);
			});
		}

		public void TestGetChargeTypeCustomsCodeOrDescription()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			var statementLineCharge = statementLine.Charges.AddNew();
			statementLineCharge.B4_ChargeType = "89";
			var wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);

			CombineAssertions(() =>
			{
				AssertEquals("Turu", StampDutyChargeTypeList.Codes.CustomsDec, wrapper.ChargeKind);
				AssertEquals("Tipi", StampDutyChargeTypeList.Descriptions.CustomsDec, wrapper.ChargeType);

				statementLineCharge.B4_ChargeType = "GMS";
				wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);
				AssertEquals("Turu", StampDutyChargeTypeList.Codes.SummaryDec, wrapper.ChargeKind);
				AssertEquals("Tipi", StampDutyChargeTypeList.Descriptions.SummaryDec, wrapper.ChargeType);

				statementLineCharge.B4_ChargeType = "89";
				wrapper = new StampDutyLedgerChargesWrapper(statementLineCharge);
				AssertEquals("Turu", StampDutyChargeTypeList.Codes.CustomsDec, wrapper.ChargeKind);
				AssertEquals("Tipi", StampDutyChargeTypeList.Descriptions.CustomsDec, wrapper.ChargeType);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			var statementLineCharge = statementLine.Charges.AddNew();
			return new StampDutyLedgerChargesWrapper(statementLineCharge);
		}
	}
}
