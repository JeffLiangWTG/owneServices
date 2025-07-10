
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IDrawbackImportClaim
	{
		ZString DrawbackImportEntry { get; }
		ZString DrawbackImportEntryPort { get; }
		ZDate DrawbackImportEntryDate { get; }
		ZString CMCDIndicator { get; }
		ZDecimal DrawbackClaimDuty { get; }
		ZDecimal DrawbackClaimTax { get; }
	}
}
