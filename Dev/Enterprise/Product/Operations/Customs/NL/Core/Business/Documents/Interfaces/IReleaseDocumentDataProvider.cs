using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IReleaseDocumentDataProvider
{
	IAddressDocumentInformation Declarant { get; }
	IAddressDocumentInformation Exporter { get; }
	ZDateTime ReleaseDate { get; }
	IShipmentDetails ShipmentDetails { get; }
	IEnumerable<IItemLineDetails> ItemLines { get; }
}
