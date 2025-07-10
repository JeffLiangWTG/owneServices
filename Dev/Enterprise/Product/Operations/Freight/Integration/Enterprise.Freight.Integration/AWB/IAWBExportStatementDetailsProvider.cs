using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBExportStatementDetailsProvider
	{
		ZString Code { get; }
		ZString Statement { get; }
	}
}
