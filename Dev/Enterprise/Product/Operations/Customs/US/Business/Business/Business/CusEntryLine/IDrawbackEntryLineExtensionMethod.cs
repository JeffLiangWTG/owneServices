using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class IDrawbackEntryLineExtensionMethod
	{
		public static ZString GetImportUQToDefault(this IDrawbackEntryLine entryLine)
		{
			var result = ZString.Empty;
			if (entryLine != null)
			{
				if (entryLine.InvoiceQuantity > 0m && !entryLine.InvoiceUQ.IsEmpty)
				{
					result = entryLine.InvoiceUQ;
				}

				if (result.IsEmpty && !entryLine.CustomsUnitQty.IsEmpty)
				{
					result = entryLine.CustomsUnitQty;
				}
			}

			return result;
		}

		public static ZDecimal GetImportQuantityToDefault(this IDrawbackEntryLine entryLine, ZString unitsToMatchAgainst)
		{
			var result = ZDecimal.Zero;
			if (entryLine != null)
			{
				if (entryLine.InvoiceUQ == unitsToMatchAgainst)
				{
					result = entryLine.InvoiceQuantity;
				}

				if (result == ZDecimal.Zero && entryLine.CustomsUnitQty == unitsToMatchAgainst)
				{
					result = entryLine.CustomsQuantity;
				}
			}

			return result;
		}

		public static ZString GetImportSecondUQToDefault(this IDrawbackEntryLine entryLine)
		{
			return entryLine?.SecondCustomsUnitQty ?? ZString.Empty;
		}

		public static ZDecimal GetImportSecondQuantityToDefault(this IDrawbackEntryLine entryLine)
		{
			return entryLine?.SecondCustomsQuantity ?? ZDecimal.Zero;
		}

		public static ZString GetImportThirdUQToDefault(this IDrawbackEntryLine entryLine)
		{
			return entryLine?.ThirdCustomsUnitQty ?? ZString.Empty;
		}

		public static ZDecimal GetImportThirdQuantityToDefault(this IDrawbackEntryLine entryLine)
		{
			return entryLine?.ThirdCustomsQuantity ?? ZDecimal.Zero;
		}
	}
}
