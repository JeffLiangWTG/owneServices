using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ImportLinePriceValidator
	{
		public void ValidateForValueToBeDeclaredAtSecondary(JobComInvoiceLine invoiceLine)
		{
			if (ShouldBeDeclaredAtSecondary(invoiceLine))
			{
				invoiceLine.JI_LinePriceInfo.AddMessageError(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);
			}
		}

		public bool ShouldBeDeclaredAtSecondary(JobComInvoiceLine invoiceLine)
		{
			var result = false;
			var dutyDate = invoiceLine.EffectiveDateForDutyRate;

			IDutyData dutyData = ((IDutyData)invoiceLine).ParentTariffLine ?? invoiceLine;
			var tariff = dutyData.ImportTariff;

			if (tariff != null && tariff.IsValueToBeDeclaredInAlternateTariff(dutyDate))
			{
				//derived duty calculation still has a parent and children
				if (invoiceLine.JI_LinePrice > 0m && invoiceLine.IsParentLine && invoiceLine.HasEmptySupTariff ||
					invoiceLine.JI_LinePrice == 0m && invoiceLine.IsSecondaryTariffLine)
				{
					result = true;
				}
			}
			return result;
		}

		public void ValidateFor9802TransactionValues(JobComInvoiceLine invoiceLine, ZPropertyInfo info, ZDecimal value)
		{
			IDutyData dutyData = ((IDutyData)invoiceLine).ParentTariffLine ?? invoiceLine;
			USCTariff tariff = dutyData.ImportTariff;

			if (tariff != null && tariff.UE_Tariff.StartsWith("9802") && value == 0m)
			{
				var childLines = (invoiceLine.ParentTariffLine ?? invoiceLine).ChildLines;

				if (childLines.IsCountEqualTo(1) || childLines.IsCountEqualTo(0) && invoiceLine.HasSupplementary)
				{
					info.AddMessageError(ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
				}
			}
		}

		public void ValidateForSetXLine(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.JI_LinePrice > 0m && invoiceLine.IsSetXLine && !invoiceLine.ChildLines.IsNullOrEmpty())
			{
				ZDecimal total = 0m;

				foreach (JobComInvoiceLine child in invoiceLine.ChildLines)
				{
					total += child.JI_LinePrice;
				}

				if (invoiceLine.JI_LinePrice != total)
				{
					invoiceLine.JI_LinePriceInfo.AddWarning(LinePriceOfXWillBeIgnoredFor7501);
				}
			}
		}

		internal const string LinePriceOfXWillBeIgnoredFor7501 = "This amount is disregarded for 7501 purposes. The entered values of V lines will be declared to Customs in the 7501.";

		public void Validate98GoodsValueForSetXLine(ZPropertyInfo propertyInfo, JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.IsSetXLine && !invoiceLine.ChildLines.IsNullOrEmpty() && !propertyInfo.Value.IsEmpty)
			{
				ZDecimal total = 0m;

				foreach (JobComInvoiceLine child in invoiceLine.ChildLines)
				{
					total += child.TotalOriginalGoodsValueInUSD;
				}

				ZDecimal goodsValue = invoiceLine.US_98GoodsValue;

				if (invoiceLine.CurrencyConverter != null && invoiceLine.LinePriceRefCurrency != null && invoiceLine.Declaration != null)
				{
					goodsValue += invoiceLine.CurrencyConverter.ConvertExact(new Money(invoiceLine.US_98ValueInvCurr, invoiceLine.LinePriceRefCurrency), invoiceLine.Declaration.Country.LocalCurrency).Amount;
				}

				if (goodsValue != total)
				{
					propertyInfo.AddWarning(LinePriceOfXWillBeIgnoredFor7501);
				}
			}
		}
	}
}
