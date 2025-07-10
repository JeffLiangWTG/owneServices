using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusStatementLineCharge))]
	class CusStatementLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => statementLineCharge;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => statementLineCharge;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => statementLineCharge;

		public void TestB4_ReferenceNumberAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLineCharge), nameof(CusStatementLineCharge.B4_ReferenceNumber), false, attr => attr.Caption == "Stamp Duty Ledger No");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLineCharge), nameof(CusStatementLineCharge.B4_ReferenceNumber), false, attr => attr.ShortCaption == "S.D.Ledger No");
		}

		public void TestB4_ChargeTypeAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLineCharge), nameof(CusStatementLineCharge.B4_ChargeType), false, attr => attr.Caption == "Charge Type");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementLineCharge), nameof(CusStatementLineCharge.B4_ChargeType), false, attr => attr.ListDataSourceMember == (nameof(CusStatementLineCharge.Lookups) + "." + nameof(CusStatementLineChargeLookups.TaxOrFeeCodeList)));
		}

		public void TestB4_ChargeAmountAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLineCharge), nameof(CusStatementLineCharge.B4_ChargeAmount), false, attr => attr.Caption == "Charge Amount");
		}

		public void TestTotalChargeAmountChangedOrDeleted()
		{
			statementLineCharge.B4_ChargeAmount = 10;
			var statementLineCharge2 = statementLine.Charges.AddNew();
			statementLineCharge2.B4_ChargeAmount = 2;
			Factory.Save();

			CombineAssertions("Test B2_StatementAmount", () =>
			{
				AssertEquals("TotalChargeAmount saved", 12m, statementHeader.TotalChargeAmount);
				statementLineCharge2.B4_ChargeAmount = 4;
				AssertEquals("TotalChargeAmount changed", 14m, statementHeader.TotalChargeAmount);
				statementLineCharge2.Delete();
				AssertEquals("TotalChargeAmount deleted", 10m, statementHeader.TotalChargeAmount);
			});
		}

		public void TestB4_ChargeAmountValues()
		{
			PrepareReferenceTestData();

			statementLine.B3_EntryType = "MAN";
			statementLineCharge.B4_ChargeAmount = 0;
			statementLineCharge.B4_ChargeType = "ABS";

			CombineAssertions(() =>
			{
				AssertEquals("ABS", 4.8000m, statementLineCharge.B4_ChargeAmount);

				statementLineCharge.B4_ChargeAmount = 0;
				statementLineCharge.B4_ChargeType = "GMS";
				AssertEquals("GMS", 100.4000m, statementLineCharge.B4_ChargeAmount);

				statementLineCharge.B4_ChargeAmount = 0;
				statementLineCharge.B4_ChargeType = "OBS";
				AssertEquals("OBS", 4.8000m, statementLineCharge.B4_ChargeAmount);

				statementLineCharge.B4_ChargeAmount = 0;
				statementLineCharge.B4_ChargeType = "SBS";
				AssertEquals("SBS", 136.1000m, statementLineCharge.B4_ChargeAmount);

				statementLineCharge.B4_ChargeAmount = 0;
				statementLineCharge.B4_ChargeType = "SBS";
				AssertEquals("B4_ChargeType Value Not Changed", 0m, statementLineCharge.B4_ChargeAmount);

				statementLine.B3_EntryType = "ETR";
				statementLineCharge.B4_ChargeAmount = 0;
				statementLineCharge.B4_ChargeType = "89";
				AssertEquals("89", 898.2000m, statementLineCharge.B4_ChargeAmount);

				statementLine.B3_EntryType = "MAN";
				statementLineCharge.B4_ChargeAmount = 123;
				statementLineCharge.B4_ChargeType = "ABS";
				AssertEquals("B4_ChargeAmount not empty", 123m, statementLineCharge.B4_ChargeAmount);
			});
		}

		void PrepareReferenceTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("89", 898.2000m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("SBS", 136.1000m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("OBS", 4.8000m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("GMS", 100.4000m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTaxOrFee("ABS", 4.8000m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
			statementLineCharge = statementLine.Charges.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;
		CusStatementLineCharge statementLineCharge;
	}
}
