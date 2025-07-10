using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CommodityProvider(CusExitConsignmentItem exitConsignmentItem) : ICommodityBase
{
	readonly CusExitConsignmentItem cusExitConsignmentItem = Argument.NotNull(exitConsignmentItem, nameof(exitConsignmentItem));

	public IGoodsMeasure GoodsMeasure => goodsMeasure ??= new GoodsMeasureProvider(cusExitConsignmentItem);
	IGoodsMeasure goodsMeasure;
}
