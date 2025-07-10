using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVAirCargoAdvanceScreeningMessageSender
	{
		ZString ACASDocumentaryName { get; }

		bool TrySendACASReports(ACASReportAction acasReportAction, out string message);

		bool TrySendACASReport(IHVLVConsignment consignment, ACASReportAction acasReportAction, out string errorMessage);

		string ValidateACASReportBasicRequirments();
	}
}
