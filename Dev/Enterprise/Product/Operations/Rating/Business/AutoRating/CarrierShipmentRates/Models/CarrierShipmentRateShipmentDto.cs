using System;
using System.Collections.Generic;

namespace Enterprise.Rating.Business;

public sealed class CarrierShipmentRateShipmentDto
{
	public IEnumerable<CarrierShipmentRateRatePartyDto> RateParties { get; set; }

	public string Carrier { get; set; }

	public string ContractNumber { get; set; }

	public string ReceiptType { get; set; }

	public string ReceiptTerm { get; set; }

	public string DeliveryType { get; set; }

	public string DeliveryTerm { get; set; }

	public string ReceiptDrayage { get; set; }

	public string DeliveryDrayage { get; set; }

	public DateTime ReadyDate { get; set; }
}
