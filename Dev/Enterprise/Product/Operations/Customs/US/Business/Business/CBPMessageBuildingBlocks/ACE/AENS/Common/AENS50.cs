using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS50 : Abstract.AENS50, IACEBIRDSecondaryLineRecord
	{
		#region IACEBIRDSecondaryLineRecord Members

		void IACEBIRDSecondaryLineRecord.Update(JobComInvoiceLine invoiceLine, bool isSupplementaryTariff, INotifications notifications)
		{
			this.SetTariffRelatedDetails(invoiceLine, Quantity1.ParseForABI(12, 2), UnitOfMeasureCode1, Quantity2.ParseForABI(12, 2), UnitOfMeasureCode2, Quantity3.ParseForABI(12, 2), UnitOfMeasureCode3);
			this.UpdateValue(invoiceLine, ValueOfGoodsAmount);
			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, DutyAmount);

			if (isSupplementaryTariff && CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(HTSNumber))
			{
				invoiceLine.US_98GoodsValue = invoiceLine.JI_LinePrice;
				invoiceLine.JI_LinePrice = 0m;
			}

			var parentTariffLine = invoiceLine.ParentTariffLine;
			if (invoiceLine.IsSecondaryTariffLine && parentTariffLine != null)
			{
				invoiceLine.JI_Weight = parentTariffLine.JI_Weight;
				parentTariffLine.JI_Weight = ZDecimal.Zero;

				var totalFTZPackQty = parentTariffLine.US_ManifestQty;
				if (totalFTZPackQty > 0)
				{
					var halfPackQty = totalFTZPackQty / 2;
					invoiceLine.US_ManifestQty = halfPackQty;
					parentTariffLine.US_ManifestQty = totalFTZPackQty - halfPackQty;
				}

				if (parentTariffLine.JI_LinePrice > 0m && new ImportLinePriceValidator().ShouldBeDeclaredAtSecondary(parentTariffLine))
				{
					invoiceLine.JI_LinePrice = parentTariffLine.JI_LinePrice;
					parentTariffLine.JI_LinePrice = 0m;
				}
			}
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return HTSNumber; }
		}

		#endregion

	}
}

namespace Enterprise.Customs.US.Business
{
	public static class ZTypeExtensionMethods
	{
		public static ZString ToStringForABI(this ZDecimal quantity, ZString uq, short totalLength, short numberOfDecimalPlaces)
		{
			var result = ZString.Empty;

			if (!uq.IsEmpty || quantity > 0)
			{
				quantity = quantity.Round(numberOfDecimalPlaces);
				result = quantity.ToString(numberOfDecimalPlaces).Replace(".", "").PadLeft(totalLength, '0');
				if (result.Length > totalLength)
				{
					result = new string(InvalidPaddingCharacter, totalLength);
				}
			}
			return result;
		}
		const char InvalidPaddingCharacter = '*';

		public static ZDecimal ParseForABI(this ZString quantityString, short totalLength, short numberOfDecimalPlaces)
		{
			quantityString = quantityString.PadLeft(totalLength, '0');
			var quantityStringWithDot = quantityString.SubstringSafe(0, totalLength - numberOfDecimalPlaces) + "." + quantityString.SubstringSafe(totalLength - numberOfDecimalPlaces);
			return ZDecimal.ParseSafe(quantityStringWithDot, 0m);
		}
	}
}
