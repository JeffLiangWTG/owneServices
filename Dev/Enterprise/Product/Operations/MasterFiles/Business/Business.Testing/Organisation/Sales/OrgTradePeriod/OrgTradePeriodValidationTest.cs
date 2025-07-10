using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradePeriodValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidation()
		{
			AssertNumberNotNegative(TradePeriod.PAS_CurrentRateInfo);
			AssertNumberNotNegative(TradePeriod.PAS_UnitsInfo);
			AssertNumberNotNegative(TradePeriod.PAS_RateOfferedInfo);
			AssertNumberNotNegative(TradePeriod.PAS_RepeatsMnthInfo);
			AssertNumberNotNegative(TradePeriod.PAS_ChargeableInfo);
			AssertNumberNotNegative(TradePeriod.PAS_WeightInfo);
			AssertNumberNotNegative(TradePeriod.PAS_VolumeInfo);
		}

		public void TestCheckPAS_RX_NKCurrency()
		{
			AssertInfo(TradePeriod.PAS_RX_NKCurrencyInfo, false, "AUD");
		}

		public void TestCheckPAS_WeightUQ()
		{
			AssertInfo(TradePeriod.PAS_WeightUQInfo, true, "KG");
		}

		public void TestCheckPAS_VolumeUQ()
		{
			AssertInfo(TradePeriod.PAS_VolumeUQInfo, true, "M3");
		}

		void AssertNumberNotNegative(ZPropertyInfo info)
		{
			SetValue(info, 1m);
			AssertNoErrors(info);

			SetValue(info, -1m);

			string prefix = Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName);
			AssertHasError(info, String.Format("Please enter {0}'{1}' greater than or equal to 0.", prefix, info.HumanReadableName));
		}

		void SetValue(ZPropertyInfo info, decimal value)
		{
			if (info.Value is ZDecimal)
			{
				info.Value = (ZDecimal)value;
			}
			else if (info.Value is ZShort)
			{
				info.Value = (ZShort)value;
			}
			else if (info.Value is ZLong)
			{
				info.Value = (ZLong)value;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		void AssertInfo(ZPropertyInfo info, bool emptyEnabled, ZString validValue)
		{
			info.Value = (ZString)"##";
			Assert(info.HasError("Enter a valid selection.") || info.HasError("Enter a valid code.") || info.HasError(String.Format("Enter a valid {0}.", info.Description)));

			info.Value = ZString.Empty;
			if (emptyEnabled)
			{
				AssertNoErrors(info);
			}
			else
			{
				Assert(info.HasError("Please enter a value.") || info.HasError(String.Format("Please enter a {0}.", info.Description)));
			}

			info.Value = validValue;
			AssertNoErrors(info);
		}

		OrgTradePeriod TradePeriod
		{
			get
			{
				if (tradePeriod == null)
				{
					var sales = Factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
					tradePeriod = sales.TradeDetails.AddNew().CurrentProspectPeriod;
				}
				return tradePeriod;
			}
		}
		OrgTradePeriod tradePeriod;
	}
}
