using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefAirline
	{
		ZString HumanReadableName { get; }

		ZString RM_EagleAddedAirlinePrefixOrAccountingCode { get; }
	}
}
