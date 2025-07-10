using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Messaging.Interface
{
	public interface IInBondQXHeader : IMessageAttacheeWithCBPSenderReference
	{
		ZString EntryType { get; }
		ZString InBondNumber { get; }
		ZString CarrierCode { get; }
		ZString USPortOfDestination { get; }
		ZString ForeignDestination { get; }
		ZInt Value { get; }
		ZString InBondCarrierID { get; }
		ZString ForeignEntryNumber { get; }

		ZString ImportingCarrierCode { get; }
		ZString ImportingCarrierFlightNumber { get; }
		ZString DistrictPortOfImportingConveyanceArrival { get; }
		ZDateTime EstimatedDateOfArrival { get; }

		IEnumerable<IInBondQXBillDetails> Bills { get; }

		ZString JobNumber { get; }
	}
}