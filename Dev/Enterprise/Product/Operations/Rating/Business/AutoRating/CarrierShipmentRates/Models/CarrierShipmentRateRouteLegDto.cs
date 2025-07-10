namespace Enterprise.Rating.Business;

public sealed class CarrierShipmentRateRouteLegDto
{
	public string FromAddress { get; set; }

	public string ToAddress { get; set; }

	public decimal? Distance { get; set; }

	public string UnitOfDistance { get; set; }

	public string Supplier { get; set; }

	public string Service { get; set; }

	public string TransportMode { get; set; }
}
