namespace Enterprise.Rating.Business;

public sealed class CarrierShipmentRateCargoDto
{
	public bool IsChargeable { get; set; }

	public int? PieceCount { get; set; }

	public string ContainerType { get; set; }

	public string ContainerNumber { get; set; }

	public string CarModel { get; set; }

	public string CarManufacturer { get; set; }

	public string Commodity { get; set; }

	public string PackageType { get; set; }

	public decimal? GrossWeight { get; set; }

	public string UnitOfWeight { get; set; }

	public decimal? RevenueTons { get; set; }

	public decimal? Length { get; set; }

	public decimal? Width { get; set; }

	public decimal? Height { get; set; }

	public string UnitOfDimension { get; set; }

	public decimal? Area { get; set; }

	public string UnitOfArea { get; set; }

	public decimal? Volume { get; set; }

	public string UnitOfVolume { get; set; }

	public bool IsEmptyContainer { get; set; }

	public string ContainerOwnership { get; set; }

	public string Propulsion { get; set; }

	public decimal? ReeferTemperature { get; set; }

	public string UnitOfReeferTemperature { get; set; }

	public decimal? LostSlots { get; set; }

	public string ImdgUNNumber { get; set; }

	public string ImdgClass { get; set; }

	public string CargoType { get; set; }

	public string ReceiptDrayage { get; set; }

	public string DeliveryDrayage { get; set; }
}
