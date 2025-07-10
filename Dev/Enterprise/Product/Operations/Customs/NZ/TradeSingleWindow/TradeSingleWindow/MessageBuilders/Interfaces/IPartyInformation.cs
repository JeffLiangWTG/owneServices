using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public interface IPartyInformation
	{
		ZString Name { get; }
		ZString City { get; }
		ZString CountryCode { get; }
		ZString CountryRegion { get; }
		ZString Address { get; }
		ZString PostCode { get; }
		ZString CustomsClientCode { get; }
		IEnumerable<ICommunication> Communications { get; }
		bool IsEmpty { get; }
	}
}
