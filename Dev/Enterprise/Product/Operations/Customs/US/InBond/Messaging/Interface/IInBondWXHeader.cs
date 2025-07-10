using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Messaging.Interface
{
	public interface IInBondWXHeader : IMessageAttacheeWithCBPSenderReference
	{
		ZString InBondNumber { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }

		ZDateTime ArrivalDateTime { get; }
		ZDateTime ExportDateTime { get; }

		ZString ScheduleDPortOfArrival { get; }
		ZString PortOfExport { get; }

		ZString ExportMOT { get; }
		ZString ImportingCarrierCode { get; }
		ZString ImportingCarrierFlightNumber { get; }
		ZDateTime ImportingCarrierScheduleArrivalDate { get; }

		ZString JobNumber { get; }
	}
}