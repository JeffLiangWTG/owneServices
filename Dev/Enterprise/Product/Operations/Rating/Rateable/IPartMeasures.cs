using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Measurable quantities on a part such as weight.
	/// 
	/// Most quantities have their own property for type safety, and fast, direct access.
	/// Any one part will likely only have a few properties actually set.
	/// Properties that do not apply or have not been set are null, meaning undefined.
	/// </summary>
	public interface IPartMeasures
	{
		/// <summary>
		/// <see cref="MeasureType.Weight"/>
		/// <see cref="MeasureType.JobWeight"/>
		/// <see cref="MeasureType.StorageWeight"/>
		/// </summary>
		IClientProviderValues WeightMeasure { get; }

		/// <summary>
		/// Indicates the WeightMeasure has no corresponding volume defined.
		/// This part cannot contribute to volume to weight conversion.
		/// The chargeable amount can only be the weight.
		/// </summary>
		bool IsWeightWithoutVolume { get; }

		/// <summary>
		/// <see cref="MeasureType.Volume"/>
		/// <see cref="MeasureType.JobVolume"/>
		/// <see cref="MeasureType.StorageVolume"/>
		/// </summary>
		IClientProviderValues VolumeMeasure { get; }

		/// <summary>
		/// <see cref="MeasureType.Area"/>
		/// </summary>
		IClientProviderValues AreaMeasure { get; }

		/// <summary>
		/// <see cref="MeasureType.Length"/>
		/// </summary>
		IClientProviderValues LengthMeasure { get; }

		/// <summary>
		/// <see cref="MeasureType.Chargeable"/>
		/// </summary>
		IClientProviderValues ChargeableMeasure { get; }

		/// <summary>
		/// Package count. A decimal for historical reasons, but mostly likely is always a whole number.
		/// <see cref="MeasureType.Package"/>
		/// </summary>
		decimal? PackageCount { get; }

		/// <summary>
		/// Package reference for the PackageCount.
		/// Usually formatted as {ContainerNumber}/{PackLine RefNumber}.
		/// </summary>
		string PackageCountReference { get; }

		/// <summary>
		/// Unit count (when defined seems always to be the same as PackageCount - maybe they can be merged)
		/// <see cref="MeasureType.Unit"/>
		/// <see cref="MeasureType.JobUnit"/>
		/// </summary>
		decimal? UnitCount { get; }

		/// <summary>
		/// Loading meters. Matches unit "LM = Loading Meter".
		/// <see cref="MeasureType.LoadingMeters"/>
		/// </summary>
		IClientProviderValues LoadingMeterMeasure { get; }

		/// <summary>
		/// The number of shipments on the job. Always job level.
		/// <see cref="MeasureType.Shipment"/>
		/// Used only for rating when the unit is HB - House Bill
		/// Null means the part is not of a kind that has shipments.
		/// Zero means the house bill is not charged to this job, e.g., it is charged to a co-load master shipment.
		/// </summary>
		decimal? ShipmentCount { get; }

		/// <summary>
		/// Lowest Level Bill count.
		/// <see cref="MeasureType.LowestBill"/>
		/// Some customs jobs are rated on the number of Lowest Level Bills
		/// Lowest Level Bills are Master, House and Sub-house bills that have no children.
		/// </summary>
		decimal? LowestBillCount { get; }

		/// <summary>
		/// <see cref="MeasureType.ChargeablePallet"/>
		/// </summary>
		decimal? ChargeablePalletCount { get; }

		/// <summary>
		/// <see cref="MeasureType.LocationPallet"/>
		/// </summary>
		decimal? LocationPalletCount { get; }

		/// <summary>
		/// Warehouse order lines
		/// <see cref="MeasureType.Line"/>
		/// </summary>
		decimal? LineCount { get; }

		/// <summary>
		/// Warehouse order lines
		/// <see cref="MeasureType.Package"/>
		/// </summary>
		bool IsPackageLoaded { get; }

		/// <summary>
		/// Unidentified measure. 
		/// <see cref="MeasureType.Unidentified"/>
		/// Mostly used to allow a job to match a line with a unit of "SV - Service".
		/// Could probably be implemented more cleanly.
		/// </summary>
		int? UnidentifiedCount { get; }

		/// <summary>
		/// <see cref="MeasureType.PickupDistance"/>
		/// </summary>
		decimal PickupDistance { get; }

		/// <summary>
		/// <see cref="MeasureType.DeliveryDistance"/>
		/// </summary>
		decimal DeliveryDistance { get; }

		/// <summary>
		/// <see cref="MeasureType.Time"/>
		/// </summary>
		TimeInfo Time { get; }

		/// <summary>
		/// For the 40 or so different customs declaration line counts.
		/// <see cref="MeasureType.CFIALine"/> etc
		/// </summary>
		int GetDeclarationLineCount(MeasureType measureType);
	}
}
