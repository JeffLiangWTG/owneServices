using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IPort
	{
		ZString DistrictPortOfUnladingCode { get; }
		ZDate OriginalEstimatedDate { get; }
		ZInt NumberOfBillsOfLadingForPort { get; }
		ZString FIRMSCode { get; }
		ZString Time { get; }
	}
}
