using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class QuantityCode
	{
		public const char FirstQuantity = '1';
		public const char SecondQuantity = '2';
		public const char ThirdQuantity = '3';
	}

	class ValueQuantityBoundValidator
	{
		public static class ValueEditType
		{
			public const string NoValueEdit = "0";
			public const string HTSRestriction = "1";
			public const string CensusRestriction = "2";
		}

		public void ValidateQuantityLowerAndUpperBound(ZPropertyInfo infoToAddNotification, char quantityCode, IDutyData invoiceLine)
		{
			USCTariff tariff = invoiceLine.ImportTariff;
			if (tariff != null)
			{
				ZString quantityEditCode = tariff.UE_QuantityEditCode;

				if (quantityEditCode.Length == 3 && quantityCode == quantityEditCode[0])//want to add notification to its numerator
				{
					ZDecimal numerator = GetQuantity(quantityEditCode[0], invoiceLine);
					ZDecimal denominator = GetQuantity(quantityEditCode[1], invoiceLine);

					if (denominator != 0m && !IsInsideRange(tariff.UE_QuantityEditLowerBound, tariff.UE_QuantityEditUpperBound, quantityEditCode[2], numerator / denominator))
					{
						string rangeDescription = GetRangeDescription(tariff.UE_QuantityEditLowerBound, tariff.UE_QuantityEditUpperBound, quantityEditCode[2]);

						string numeratorQuantityDesc = GetQuantityDescription(quantityEditCode[0]);
						string denominatorQuantityDesc = GetQuantityDescription(quantityEditCode[1]);

						infoToAddNotification.AddMessageError(string.Format(QuantityRatioOutOfBound, numeratorQuantityDesc, denominatorQuantityDesc, rangeDescription));
					}
				}
			}
		}

		/// <summary>
		/// This adds a message error to US_ComponentPrice if the unit price allowed for the tariff does not satify what users have entered
		/// </summary>
		/// <param name="invoiceLine"></param>
		public void ValidateUnitPriceLowerAndUpperBound(USCTariff tariff, IDutyData invoiceLine, ZDecimal totalCustomsValue, ZPropertyInfo linePriceInfo, ZString relatedTariffField)
		{
			if (tariff != null && invoiceLine != null)
			{
				ZString valueEditCode = tariff.UE_ValueEditCode;

				if (valueEditCode.StartsWith(ValueEditType.HTSRestriction) && valueEditCode.Length == 3)
				{
					ZDecimal quantity = GetQuantity(valueEditCode[1], invoiceLine);

					if (quantity != 0m)
					{
						ZDecimal unitPrice = totalCustomsValue / quantity;

						if (!IsInsideRange(tariff.UE_ValueLowBounds, tariff.UE_ValueHighBounds, valueEditCode[2], unitPrice))
						{
							string quantityRequired = GetQuantityDescription(valueEditCode[1]);

							string rangeDescription = GetRangeDescription(tariff.UE_ValueLowBounds, tariff.UE_ValueHighBounds, valueEditCode[2]);

							linePriceInfo.AddMessageError(string.Format(UnitPriceOutOfBound, quantityRequired, relatedTariffField, rangeDescription, unitPrice.Round(4)));
						}
					}
				}
			}
		}

		internal const string FirstQuantity = "first quantity";
		internal const string SecondQuantity = "second quantity";
		internal const string ThirdQuantity = "third quantity";
		internal const string UnitPriceOutOfBound = "The unit price ${3} calculated with the customs value divided by the {0} of entry lines is outside of the range the selected {1} allows, {2}. Please review all the invoice lines merged to this entry line. If you have modified line prices or quantities of any invoice line, please save or click Brokerage > Merge to refresh this validation.";
		internal const string QuantityRatioOutOfBound = "The ratio calculated with {0} divided by {1} is outside of the range the selected tariff allows, {2}";

		string GetRangeDescription(ZDecimal lowBounds, ZDecimal highBounds, char thirdPosition)
		{
			switch (thirdPosition)
			{
				case '1':
					return " > " + lowBounds.ToString(4) + " and <= " + highBounds.ToString(4);

				case '2':
					return " >= " + lowBounds.ToString(4) + " and < " + highBounds.ToString(4);

				case '3':
					return " > " + lowBounds.ToString(4);

				case '4':
					return " >= " + lowBounds.ToString(4);

				case '5':
					return " <= " + highBounds.ToString(4);

				case '6':
					return " < " + highBounds.ToString(4);

				default:
					return "";
			}
		}

		bool IsInsideRange(ZDecimal lowBounds, ZDecimal highBounds, char thirdPosition, ZDecimal valueToCheck)
		{
			switch (thirdPosition)
			{
				case '1':
					return valueToCheck > lowBounds && valueToCheck <= highBounds;

				case '2':
					return valueToCheck >= lowBounds && valueToCheck < highBounds;

				case '3':
					return valueToCheck > lowBounds;

				case '4':
					return valueToCheck >= lowBounds;

				case '5':
					return valueToCheck <= highBounds;

				case '6':
					return valueToCheck < highBounds;

				default:
					return true;
			}
		}

		ZDecimal GetQuantity(char editCode, IDutyData invoiceLine)
		{
			switch (editCode)
			{
				case QuantityCode.FirstQuantity:
					return invoiceLine.Quantity1;
				case QuantityCode.SecondQuantity:
					return invoiceLine.Quantity2;
				case QuantityCode.ThirdQuantity:
					return invoiceLine.Quantity3;
				default:
					return 0m;
			}
		}

		string GetQuantityDescription(char editCode)
		{
			switch (editCode)
			{
				case QuantityCode.FirstQuantity:
					return FirstQuantity;
				case QuantityCode.SecondQuantity:
					return SecondQuantity;
				case QuantityCode.ThirdQuantity:
					return ThirdQuantity;
				default:
					return "";
			}
		}
	}
}
