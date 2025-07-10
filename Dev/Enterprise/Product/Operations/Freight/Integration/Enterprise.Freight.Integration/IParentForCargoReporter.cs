using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Integration
{
	public interface IParentForCargoReporter
	{
		Event CargoReportSentEvent { get; }
		Event CargoReportRejectedEvent { get; }
		Event CargoReportAcceptedEvent { get; }
		Event CargoReportWithdrawEvent { get; }
		Logs Logs { get; }
		ZString CargoReporterReferenceText { get; }
	}
}
