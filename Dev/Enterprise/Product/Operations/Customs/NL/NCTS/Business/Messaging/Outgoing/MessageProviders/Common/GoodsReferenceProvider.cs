using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class GoodsReferenceProvider : IGoodsReference
{
	readonly NctsContainerItem cusCodeData;
	readonly NctsCommonCargoDesc goodItem;

	public GoodsReferenceProvider(NctsContainerItem cusCodeData, int sequence)
	{
		SequenceNumeric = sequence;
		this.cusCodeData = Argument.NotNull(cusCodeData, nameof(cusCodeData));
	}

	public GoodsReferenceProvider(NctsCommonCargoDesc goodItem, ZInt sequence)
	{
		SequenceNumeric = sequence;
		this.goodItem = Argument.NotNull(goodItem, nameof(goodItem));
	}

	public int SequenceNumeric { get; }

	public int GoodsItemNumericValue => cusCodeData != null ? cusCodeData.CY_DataNumeric : goodItem.BY_DeclarationGoodsItemNumber;
}
