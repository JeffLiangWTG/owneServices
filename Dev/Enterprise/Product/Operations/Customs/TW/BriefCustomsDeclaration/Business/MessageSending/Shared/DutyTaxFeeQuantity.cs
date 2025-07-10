using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class DutyTaxFeeQuantity : IDutyTaxFeeQuantity
	{
		public DutyTaxFeeQuantity(AsycudaPackedItem packedItem)
		{
			PackedItem = Argument.NotNull(packedItem, nameof(packedItem));
		}

		AsycudaPackedItem PackedItem { get; }

		ZDecimal IDutyTaxFeeQuantity.PercentageNumeric => default;

		ZDecimal IDutyTaxFeeQuantity.TaxRateNumeric => CommonHelper.GetDecimalForRateFormulaDerivedFrom(PackedItem.SpecificDutyRateFormulaDerivedFrom);

		ZString IDutyTaxFeeQuantity.DutyUnitCode => PackedItem.API_CustomsUQ2;
	}
}
