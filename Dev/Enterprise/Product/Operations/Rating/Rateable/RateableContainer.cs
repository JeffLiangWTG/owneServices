using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Rateable container quantities.
	/// Used to represent an air or sea container.
	/// Also used to represent packages that are in a container, e.g., a shipment packline.
	/// Future: Might be useful to use something else for packages to simplify things.
	/// </summary>
	public interface IRateableContainer : IRateablePart
	{
		int ContainerCount { get; }

		/// <summary>
		/// The container weight.
		/// The package weight when this is a package in a container.
		/// Equivalent to the old ContainerInfo.Weight.
		/// </summary>
		decimal ContainerWeightInKG { get; }

		/// <summary>
		/// The container gross weight.
		/// </summary>
		Quantity ContainerGrossWeight { get; }

		/// <summary>
		/// The container volume. The gross volume when this is a shipment/consol container.
		/// The package volume when this is a package in a container.
		/// Equivalent to the old ContainerInfo.Volume.
		/// </summary>
		decimal ContainerVolumeInM3 { get; }

		/// <summary>
		/// The calculated total weight for a shipment container (not the gross).
		/// Equivalent to the old MeasureDimension.ContainerCalculatedWeight.
		/// Introduced only for JobChargeQuickCalculateBusinessObject.
		/// </summary>
		decimal ContainerCalculatedWeightInKG { get; }

		/// <summary>
		/// The calculated total volume for a shipment container.
		/// Equivalent to the old MeasureDimension.ContainerCalculatedWeight.
		/// Introduced only for JobChargeQuickCalculateBusinessObject.
		/// </summary>
		decimal ContainerCalculatedVolumeInM3 { get; }

		/// <summary>
		/// Package count. Will be zero if this contains no packages.
		/// Used for LCL container package count.
		/// For package measures (Unit and PackageType) this number is not used as a count, but as a grouping key.
		/// It is set inconsistently, some jobs set 1 and others set the total number of packages in a container.
		/// Need to be made consistent...
		/// </summary>
		int ContainerPackages { get; }

		/// <summary>
		/// Comes from IForwardingContainer.JC_ContainerQuality where provided by job's containers
		/// </summary>
		string ContainerQuality { get; }

		string YardUnitType { get; }

		string YardUnitLoad { get; }

		Guid YardUnitClient { get; }

		decimal TEU { get; }

		/// <summary>
		/// Ref number - as of 2021-07 only used for pack line ref numbers (i.e., JobPackLines.JL_RefNumber)
		/// </summary>
		string RefNumber { get; }

		/// <summary>
		/// Combined reference.
		/// Format is ContainerNumber/RefNumber if both are defined, otherwise just the defined number.
		/// </summary>
		string Reference { get; }

		/// <summary>
		/// The fraction of the container that belongs to the current shipment.
		/// Only used for rating shipments.
		/// The fraction can be based on weight or volume, depending on which uses the most of the available capacity.
		///
		/// Note, the same container can contain multiple shipments, so in different shipment adapters
		/// there can be the same ContainerNumber and their own ShipmentShare.
		/// However, rating only calculates for one adapter at a time, so within a single adapter there should
		/// be only one RateableContainer per real container with a shipment share less than one.
		/// </summary>
		decimal ShipmentShare { get; }

		ContainerSpotRates ContainerSpotRates { get; }
	}

	public class RateableContainer : RateablePart, IRateableContainer
	{
		public static readonly ZGuid LCL = new Guid("{B44F89DC-D579-4770-8983-ED645D1B3C18}");

		public RateableContainer()
		{
		}

		public int ContainerCount { get; set; }

		/// <summary>
		/// <see cref="IRateableContainer.ContainerWeightInKG"/>
		/// </summary>
		public decimal ContainerWeightInKG
		{
			get => containerWeightInKG;
			set
			{
				containerWeightInKG = value;
				HasContainerWeightVolume = true;
			}
		}
		decimal containerWeightInKG;

		public void SetContainerWeight(decimal weight, string weightUnit)
		{
			ContainerWeightInKG = weightUnit == Constants.Weight.Kilograms
				? weight
				: Constants.Weight.Convert(weight, weightUnit, Constants.Weight.Kilograms);
		}

		public decimal ContainerVolumeInM3
		{
			get => containerVolumeInM3;
			set
			{
				containerVolumeInM3 = value;
				HasContainerWeightVolume = true;
			}
		}
		decimal containerVolumeInM3;

		public void SetContainerVolume(decimal volume, string volumeUnit)
		{
			ContainerVolumeInM3 = volumeUnit == Constants.Volume.CubicMetres
				? volume
				: Constants.Volume.Convert(volume, volumeUnit, Constants.Volume.CubicMetres);
		}

		public bool HasContainerWeightVolume { get; private set; }

		public decimal ContainerCalculatedWeightInKG { get; internal set; }
		public decimal ContainerCalculatedVolumeInM3 { get; internal set; }

		public Quantity ContainerGrossWeight
		{
			get =>
				containerGrossWeight == default
					? new Quantity(ContainerWeightInKG, Constants.Weight.Kilograms)
					: containerGrossWeight;

			set => containerGrossWeight = value;
		}

		Quantity containerGrossWeight;

		public int ContainerPackages { get; set; }

		public string ContainerQuality { get; internal set; }

		public string YardUnitType { get; set; }

		public string YardUnitLoad { get; set; }

		public Guid YardUnitClient { get; set; }

		public decimal TEU { get; set; }

		public string RefNumber { get; set; }

		public string Reference
		{
			get
			{
				if (!string.IsNullOrEmpty(ContainerNumber))
				{
					return !string.IsNullOrEmpty(RefNumber)
						? (ContainerNumber + "/" + RefNumber)
						: ContainerNumber;
				}
				else
				{
					return RefNumber;
				}
			}
		}

		public decimal ShipmentShare { get; internal set; } = 1m;

		#region Spot Rates

		public ContainerSpotRates ContainerSpotRates { get; internal set; }

		public void SetSpotRates(SpotRateInfo costSpotRate, SpotRateInfo sellSpotRate, ZGuid containerPK, ZGuid refContainerPK)
		{
			this.ContainerSpotRates = new ContainerSpotRates(costSpotRate, sellSpotRate, this.ContainerNumber, containerPK, refContainerPK);
		}

		#endregion
	}
}
