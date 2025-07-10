using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class GenericLandedCostingExtensions
	{
		public static T GetPropertyValue<T>(this BusinessObject businessObject, string propertyName)
		{
			Argument.NotNull(businessObject, "businessObject");
			Argument.NotNullOrEmpty(propertyName, "propertyName");

			T result = default(T);

			var value = businessObject[propertyName];
			if (value is T)
			{
				result = (T)value;
			}
			else
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "{0}[{1}] should be {2}", propertyName, businessObject.GetType().Name, typeof(T).Name));
			}

			return result;
		}

		public static ZDecimal GetAmountApportionedFromCusEntryHeader(this CusEntryLine entryLine, ZDecimal cusEntryHeaderAmount, ZString cusEntryLineApplicablePropertyName)
		{
			var result = ZDecimal.Zero;
			if (entryLine != null && entryLine.Header != null && !cusEntryHeaderAmount.IsEmpty)
			{
				if (cusEntryLineApplicablePropertyName.IsEmpty)
				{
					result = entryLine.GetAmountApportionedFromCusEntryHeader(cusEntryHeaderAmount);
				}
				else
				{
					var cusEntryLineApplicable = entryLine.GetPropertyValue<ZBool>(cusEntryLineApplicablePropertyName);
					if (cusEntryLineApplicable)
					{
						result = cusEntryHeaderAmount * GetProportionOfCusEntryHeader(entryLine, cusEntryLineApplicablePropertyName);
					}
				}
			}
			return result;
		}

		static ZDecimal GetProportionOfCusEntryHeader(CusEntryLine entryLine, ZString cusEntryLineApplicablePropertyName)
		{
			ZDecimal result = 1m;

			var entryHeader = entryLine.Header;
			if (entryHeader != null && entryHeader.MergedLines.IsCountMoreThan(1))
			{
				ZDecimal totalCustomsValue = entryHeader.MergedLines.Cast<CusEntryLine>().Where(x => GetPropertyValue<ZBool>(x, cusEntryLineApplicablePropertyName)).Sum(x => x.CL_CustomsValue);
				if (!totalCustomsValue.IsEmpty)
				{
					result = entryLine.CL_CustomsValue / totalCustomsValue;
				}
			}
			return result;
		}

		public static ZDecimal GetAmountApportionedFromCusEntryLine(this BaseJobComInvoiceLine invoiceLine, CusEntryLine entryLine, ZDecimal cusEntryLineAmount)
		{
			var result = ZDecimal.Zero;
			if (entryLine != null && !cusEntryLineAmount.IsEmpty)
			{
				result = cusEntryLineAmount * GetProportionOfCusEntryLine(invoiceLine, entryLine);
			}
			return result;
		}

		static ZDecimal GetProportionOfCusEntryLine(BaseJobComInvoiceLine invoiceLine, CusEntryLine entryLine)
		{
			ZDecimal result = 1m;

			if (entryLine != null && entryLine.InvoiceLines.IsCountMoreThan(1))
			{
				var cusEntryLineFOBInLocalCurrency = entryLine.FOBInLocalCurrency.Amount;
				if (!cusEntryLineFOBInLocalCurrency.IsEmpty)
				{
					result = invoiceLine.JI_Calc_FOB_InLocalCurrency / cusEntryLineFOBInLocalCurrency;
				}
			}
			return result;
		}
	}
}
