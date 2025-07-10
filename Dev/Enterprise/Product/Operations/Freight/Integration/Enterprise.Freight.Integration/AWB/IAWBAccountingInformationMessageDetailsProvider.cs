using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBAccountingInformationMessageDetailsProvider
	{
		ZString InformationID { get; }
		ZString Information { get; }
		bool IsSkippedOnMessaging { get; }
	}
}
