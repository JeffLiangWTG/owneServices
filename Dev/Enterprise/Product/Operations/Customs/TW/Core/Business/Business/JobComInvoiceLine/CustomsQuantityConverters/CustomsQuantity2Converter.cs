using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class CustomsQuantity2Converter : BaseCustomsQuantityConverter
	{
		public CustomsQuantity2Converter(JobComInvoiceLine invoiceLine)
		: base(invoiceLine, invoiceLine.JI_CustomsSecondQuantityInfo, invoiceLine.JI_CustomsSecondUnitQtyInfo)
		{ }

		protected override void CalculateConversionFactor()
		{
			if (!IsClothMeterials)
			{
				base.CalculateConversionFactor();
			}
		}

		protected override ZDecimal CalculateCustomsQuantityCore()
		{
			ZDecimal result = 0m;
			if (IsClothMeterials)
			{
				var invoiceLine = InvoiceLine;
				if (!invoiceLine.JI_InvoiceQuantity.IsEmpty)
				{
					var effectiveQuantityUnit = GetEffectiveClothMaterialsQuantityUnit(invoiceLine.JI_InvoiceUQ);
					if (!effectiveQuantityUnit.IsEmpty && invoiceLine.Lookups.TextileWidthUQList.ContainsCode(invoiceLine.JI_TextileWidthUQ))
					{
						result = decimal.Round(Length.ConvertSafe(invoiceLine.JI_InvoiceQuantity, effectiveQuantityUnit, Length.Metres) * Length.ConvertSafe(invoiceLine.JI_TextileWidth, invoiceLine.JI_TextileWidthUQ, Length.Metres), CustomsQuantityDecimalPlace);
					}
				}
			}
			else
			{
				result = base.CalculateCustomsQuantityCore();
			}
			return result;
		}

		bool IsClothMeterials
		{
			get
			{
				var invoiceLine = InvoiceLine;
				var result = !invoiceLine.JI_TextileWidth.IsEmpty && !invoiceLine.JI_TextileWidthUQ.IsEmpty
					&& invoiceLine.JI_CustomsSecondUnitQty == Constants.CustomsUnitOfMeasureList.SquareMetre;
				if (result)
				{
					result = (invoiceLine.UniversalTariff?.GetSpecificUOM(UnitOfMeasureTypes.AdditionalUOMType) ?? ZString.Empty) == Constants.CustomsUnitOfMeasureList.SquareMetre;
				}

				return result;
			}
		}

		ZString GetEffectiveClothMaterialsQuantityUnit(ZString unit)
		{
			var quantityUnits = new Dictionary<string, string>()
			{
				{ Constants.UnitOfQuantityCodes.Centimeter, TextileWidthUQList.Codes.CM },
				{ Constants.UnitOfQuantityCodes.Meter, TextileWidthUQList.Codes.M },
				{ Constants.UnitOfQuantityCodes.MilliMeter, TextileWidthUQList.Codes.MM },
				{ Constants.UnitOfQuantityCodes.Inch, TextileWidthUQList.Codes.IN },
				{ Constants.UnitOfQuantityCodes.Foot, TextileWidthUQList.Codes.FT },
				{ Constants.UnitOfQuantityCodes.Yards, TextileWidthUQList.Codes.YD },
				{ Constants.UnitOfQuantityCodes.Yard, TextileWidthUQList.Codes.YD }
			};

			string result = ZString.Empty;
			if (!unit.IsEmpty)
			{
				quantityUnits.TryGetValue(unit, out result);
			}
			return result;
		}
	}
}
