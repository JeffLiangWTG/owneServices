using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.TransportConsignment.Business
{
	public abstract class LinehaulAndRunSheetRatingAdapter<T> : RatingAdapter<T> where T : BusinessObject
	{
		protected LinehaulAndRunSheetRatingAdapter(T parent)
			: base(parent)
		{
		}

		#region IAutoRating_AdapterType

		public override AdapterType AdapterType => AdapterType.RunSheet;

		#endregion

		#region IAutoRating_ChargeCodeGroups

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var chargeCodeGroups = new ChargeCodeGroupCollection();
				chargeCodeGroups.Add(ChargeCodeGroupList.Codes.TransportBooking);
				chargeCodeGroups.SellChargesFilter = ChargeCodeFilter.AutorateNothing;
				chargeCodeGroups.CostChargesFilter = ChargeCodeFilter.AutorateConsolLevelOnly;

				return chargeCodeGroups;
			}
		}

		#endregion

		#region IAutoRating_MergeCharges

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		#endregion

		#region IAutoRating_RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.TransportBookings; }
		}

		#endregion

		#region IAutoRatingFreightInfo Members

		#region IAutoRatingFreightInfo_FreightMode

		public override FreightMode FreightMode
		{
			get { return FreightMode.FRO; }
		}

		#endregion

		#region IAutoRatingFreightInfo_Measures

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				result.CreatePackageUnitList(includeCommodity: true);
				result.CreateWeightAndVolumeListWithCommodityAndPackageType();

				foreach (var package in Packages)
				{
					var qtyRatio = 1;
					var weightInKG = Core.Constants.Weight.Convert(qtyRatio * package.KP_Weight, package.KP_WeightUQ, Core.Constants.Weight.Kilograms);
					var volumeInM3 = Core.Constants.Volume.Convert(qtyRatio * package.KP_Volume, package.KP_VolumeUQ, Core.Constants.Volume.CubicMetres);
					result.AddWeightAndVolumeWithCommodityAndPackageType(weightInKG, volumeInM3, packType: package.KP_F3_NKPackType, commodity: package.KP_RH_NKCommodityCode);

					var containerInfo = GetPackageAsContainerInfo(package);
					result.AddPackageUnitWithCommodity(containerInfo, packType: package.KP_F3_NKPackType, commodity: package.KP_RH_NKCommodityCode);
				}

				result.CopyUnitMeasureToPackage();

				SetContainersCore(result);
				return result;
			}
		}

		#endregion

		#region SetContainersCore

		protected abstract RefEquipment Vehicle { get; }

		void SetContainersCore(RateableMeasureSet measures)
		{
			measures.CreateContainerList(includeCommodity: false);

			if (Vehicle != null)
			{
				var roadContainerType = Vehicle.RoadContainerType;
				if (roadContainerType != null)
				{
					var containerInfo = new MeasureInfo.ContainerInfo(teu: roadContainerType.RC_TEU, containerCount: 1);
					measures.AddContainerGroup(roadContainerType.PK, new[] { containerInfo });
				}
			}
		}

		#endregion

		#region CalculateTotals

		protected abstract IEnumerable<PkgPackage> Packages { get; }

		MeasureInfo.ContainerInfo GetPackageAsContainerInfo(PkgPackage package)
		{
			if (package.KP_PackageQty.IsEmpty)
			{
				return null;
			}

			var innerPackageQuantity = package.Packages.Sum(p => p.KP_PackageQty);
			var containerNumber = package.IsContainer ? package.KP_PackageID : ZString.Empty;

			var info = new MeasureInfo.ContainerInfo(
				package.KP_Weight, package.KP_WeightUQ,
				package.KP_Volume, package.KP_VolumeUQ,
				innerPackageQuantity,
				package.Container?.ContainerType?.RC_TEU ?? 0,
				containerNumber,
				containerCount: package.KP_PackageQty);

			return info;
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo_ServiceLevel

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get { return new ServiceLevelRatingInformation(new ServiceLevelInfo(CarrierServiceLevel, ServiceLevelType.Carrier)); }
		}

		protected abstract ZString CarrierServiceLevel { get; }

		#endregion

		#endregion
	}
}
