using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRateFeeChargeLevelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckORF_ServiceType()
		{
			ChargeLevelTestObject.ORF_ServiceType = ZString.Empty;
			AssertHasError(ChargeLevelTestObject.ORF_ServiceTypeInfo, "Please enter a Type.");

			ChargeLevelTestObject.ORF_ServiceType = "CCE";
			AssertHasError(ChargeLevelTestObject.ORF_ServiceTypeInfo, "Enter a valid Type.");

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "OCEAND"));

			var testChargeLevelObject2 = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();
			testChargeLevelObject2.ORF_OH = org.PK;
			testChargeLevelObject2.ORF_ServiceType = "TES";

			ChargeLevelTestObject.ORF_ServiceType = "TES";
			AssertHasError(ChargeLevelTestObject.ORF_ServiceTypeInfo, "The same type can only be configured once.");
		}

		public void TestCheckCheckORF_Level()
		{
			var registry = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var registryChargeType = registry.FeeChargeTypes.Cast<FeeChargeType>().First(t => t.Code == "TES");
			var level = registryChargeType.FeeChargeLevels.AddNew();

			ChargeLevelTestObject.ORF_ServiceType = "TES";
			ChargeLevelTestObject.ORF_Level = ZString.Empty;
			AssertHasError(ChargeLevelTestObject.ORF_LevelInfo, "Please enter a Level.");

			level.Code = "EEE";
			level.Description = (NoResString)"Test Description";
			level.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.Excess;
			level.Amount1 = 300;
			level.Amount1Currency = "AUD";
			level.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.Maximum;
			level.Amount2Currency = "CNY";
			level.Amount2 = 600;

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry);

			ChargeLevelTestObject.ORF_Level = "AAA";
			AssertHasError(ChargeLevelTestObject.ORF_LevelInfo, "Enter a valid Level.");
			ChargeLevelTestObject.ORF_Level = "EEE";
			AssertNoErrors(ChargeLevelTestObject.ORF_LevelInfo);
		}

		public void TestValidateORF_RX_NKAmount1Currency()
		{
			ChargeLevelTestObject.ORF_RX_NKAmount1Currency = ZString.Empty;
			AssertHasError(ChargeLevelTestObject.ORF_RX_NKAmount1CurrencyInfo, "Please enter a Currency.");

			ChargeLevelTestObject.ORF_RX_NKAmount1Currency = "AUD";
			AssertNoErrors(ChargeLevelTestObject.ORF_RX_NKAmount1CurrencyInfo);

			ChargeLevelTestObject.ORF_RX_NKAmount1Currency = "EEE";
			AssertHasError(ChargeLevelTestObject.ORF_RX_NKAmount1CurrencyInfo, "Enter a valid Currency.");
		}

		public void TestValidateORF_RX_NKAmount2Currency()
		{
			ChargeLevelTestObject.ORF_RX_NKAmount2Currency = ZString.Empty;
			AssertHasError(ChargeLevelTestObject.ORF_RX_NKAmount2CurrencyInfo, "Please enter a Currency.");

			ChargeLevelTestObject.ORF_RX_NKAmount2Currency = "AUD";
			AssertNoErrors(ChargeLevelTestObject.ORF_RX_NKAmount2CurrencyInfo);

			ChargeLevelTestObject.ORF_RX_NKAmount2Currency = "EEE";
			AssertHasError(ChargeLevelTestObject.ORF_RX_NKAmount2CurrencyInfo, "Enter a valid Currency.");
		}

		public void TestValidateORF_Amount1Type()
		{
			ChargeLevelTestObject.ORF_Amount1Type = ZString.Empty;
			AssertHasError(ChargeLevelTestObject.ORF_Amount1TypeInfo, "Please enter an Amount 1 Type.");

			ChargeLevelTestObject.ORF_Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.Excess;
			AssertNoErrors(ChargeLevelTestObject.ORF_Amount1TypeInfo);

			ChargeLevelTestObject.ORF_Amount1Type = "EEE";
			AssertHasError(ChargeLevelTestObject.ORF_Amount1TypeInfo, "Enter a valid Amount 1 Type.");

			ChargeLevelTestObject.ORF_Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.None;

			AssertEquals(ChargeLevelTestObject.ORF_Amount1, (ZDecimal)0);
			AssertEquals(ChargeLevelTestObject.ORF_RX_NKAmount1Currency, ZString.Empty);

			AssertNoErrors(ChargeLevelTestObject.ORF_RX_NKAmount1CurrencyInfo);
			AssertNoErrors(ChargeLevelTestObject.ORF_Amount1Info);

			Assert(ChargeLevelTestObject.ORF_Amount1Info.ReadOnly);
			Assert(ChargeLevelTestObject.ORF_RX_NKAmount1CurrencyInfo.ReadOnly);
		}

		public void TestValidateORF_Amount2Type()
		{
			ChargeLevelTestObject.ORF_Amount2Type = ZString.Empty;
			AssertHasError(ChargeLevelTestObject.ORF_Amount2TypeInfo, "Please enter an Amount 2 Type.");

			ChargeLevelTestObject.ORF_Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.Maximum;
			AssertNoErrors(ChargeLevelTestObject.ORF_Amount2TypeInfo);

			ChargeLevelTestObject.ORF_Amount2Type = "EEE";
			AssertHasError(ChargeLevelTestObject.ORF_Amount2TypeInfo, "Enter a valid Amount 2 Type.");

			ChargeLevelTestObject.ORF_Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None;

			AssertEquals(ChargeLevelTestObject.ORF_Amount2, (ZDecimal)0);
			AssertEquals(ChargeLevelTestObject.ORF_RX_NKAmount2Currency, ZString.Empty);

			AssertNoErrors(ChargeLevelTestObject.ORF_RX_NKAmount2CurrencyInfo);
			AssertNoErrors(ChargeLevelTestObject.ORF_Amount2Info);

			Assert(ChargeLevelTestObject.ORF_Amount2Info.ReadOnly);
			Assert(ChargeLevelTestObject.ORF_RX_NKAmount2CurrencyInfo.ReadOnly);
		}

		OrgRateFeeChargeLevel ChargeLevelTestObject;

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "OCEAND"));

			ChargeLevelTestObject = Factory.New<OrgRateFeeChargeLevel>();
			ChargeLevelTestObject.ORF_OH = org.PK;

			var registry = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			registry.FeeChargeTypes.AddFeeChargeType("TES",
				(NoResString)"Test Description", new FeeChargeLevelCollection());

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry);
		}
	}
}
