using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class GoodsMeasureProvider(CusExitConsignmentItem exitConsignmentItem) : IGoodsMeasure
{
	readonly CusExitConsignmentItem cusExitConsignmentItem = Argument.NotNull(exitConsignmentItem, nameof(exitConsignmentItem));

	public decimal? GrossMassValue => cusExitConsignmentItem.CCI_GrossMass;

	public decimal NetMass => cusExitConsignmentItem.CCI_NetMass;

	public decimal? SupplementaryUnitsValue => null;
}
