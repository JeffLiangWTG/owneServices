using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business;

sealed class CarrierShipmentRateQueryBusinessObject(CarrierShipmentRateQueryDto parent, string origin, string destination) : NonPersistentBusinessObject
{
	public string Origin => origin;

	public string Destination => destination;

	public FreightMode FreightMode => FreightMode.FCL;

	public string Carrier => parent.Shipment.Carrier;

	public DateTime ReadyDate => parent.Shipment.ReadyDate;

	public string Consignee => parent.Shipment.RateParties?.FirstOrDefault(p => p.Role == DocAddressTypes.Codes.ConsigneeDocumentaryAddress)?.Code;

	public string Consignor => parent.Shipment.RateParties?.FirstOrDefault(p => p.Role == DocAddressTypes.Codes.ConsignorDocumentaryAddress)?.Code;

	public string BookingParty => parent.Shipment.RateParties?.FirstOrDefault(p => p.Role == DocAddressTypes.Codes.BookingPartyDocumentaryAddress)?.Code;

	public RateType RateType
	{
		get
		{
			return RateType.Shipping | RateType.ShippingImportDetention | RateType.ShippingExportDetention;
		}
	}

	public IEnumerable<CarrierShipmentRateCargoDto> Cargo => parent.Cargo;

	public bool HasMeasures => Cargo?.Any() ?? false;
}
