using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Business
{
	public class PackLineCollectionCalculator
	{
		#region enum FilterMode

		public enum FilterMode
		{
			NoFilter = 0,
			RemoveColoadMasterPackLines = 1
		}

		#endregion

		public PackLineCollectionCalculator(IPackLineCollection packLineCollection, FilterMode filterMode)
		{
			if (!(packLineCollection is BusinessObjectCollection))
			{
				throw new NotSupportedException("IPackLineCollection must be implemented on a BusinessObjectCollection");
			}

			if (filterMode == FilterMode.RemoveColoadMasterPackLines)
			{
				this.PackLineCollection = new SubPackLineCollectionView(packLineCollection);
			}
			else
			{
				this.PackLineCollection = packLineCollection;
			}
		}

		protected internal readonly IPackLineCollection PackLineCollection;

		#region TotalPackages

		public ZInt TotalPackages
		{
			get
			{
				ZDecimal decimalValue = TotalCalculation.GetTotal((BusinessObjectCollection)PackLineCollection, PackLine.Schema.JL_PackageCount);
				return ZInt.ParseSafe(decimalValue.ToString(), 0);
			}
		}

		#endregion

		#region TotalPackagesToDeliver

		public ZInt TotalPackagesToDeliver
		{
			get
			{
				ZInt result = 0;
				foreach (PackLine packLine in PackLineCollection)
				{
					result += packLine.PackagesToDeliver;
				}
				return result;
			}
		}

		#endregion

		#region TotalPackagesByShipment

		public ZDecimal TotalPackagesByShipment(CommonShipment shipmentBO)
		{
			ZDecimal result = 0m;
			if (shipmentBO != null)
			{
				foreach (PackLine packLine in PackLineCollection)
				{
					if (packLine.Shipment != null && packLine.Shipment.PK == shipmentBO.PK)
					{
						result += packLine.JL_PackageCount;
					}
				}
			}

			return result;
		}

		#endregion

		#region TotalPackagesUnit

		public ZString TotalPackagesUnit
		{
			get
			{
				ZString result = "";
				foreach (PackLine packLine in PackLineCollection)
				{
					if (result.IsEmpty)
					{
						result = packLine.JL_F3_NKPackType;
					}
					else if (packLine.JL_F3_NKPackType != result)
					{
						result = Constants.PkgUnit.Package; //Combination
						break;
					}
				}

				return result.IsEmpty ? PackLineCollection.MasterPackagesUnit : result;
			}
		}

		#endregion

		#region TotalWeight

		public ZDecimal TotalWeight
		{
			get
			{
				ZDecimal result = 0;
				ZString unitToCalculateIn = TotalWeightUnit;

				foreach (PackLine packLine in PackLineCollection)
				{
					if (unitToCalculateIn == packLine.PackLineWeightUnit)
					{
						result += packLine.JL_ActualWeight;
					}
					else
					{
						result += Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.PackLineWeightUnit, unitToCalculateIn, false);
					}
				}

				return result;
			}
		}

		#endregion

		#region TotalWeightByShipment

		public ZDecimal TotalWeightByShipment(CommonShipment shipmentBO)
		{
			ZDecimal result = 0;
			ZString unitToCalculateIn = TotalWeightUnit;

			if (shipmentBO != null)
			{
				foreach (PackLine packLine in PackLineCollection)
				{
					if (packLine.Shipment != null && packLine.Shipment.PK == shipmentBO.PK)
					{
						if (unitToCalculateIn == packLine.PackLineWeightUnit)
						{
							result += packLine.JL_ActualWeight;
						}
						else
						{
							result += Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.PackLineWeightUnit, unitToCalculateIn, false);
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region TotalWeightToDeliver

		public ZDecimal TotalWeightToDeliver
		{
			get
			{
				ZDecimal result = 0;
				ZString unitToCalculateIn = TotalWeightUnit;

				foreach (PackLine packLine in PackLineCollection)
				{
					if (unitToCalculateIn == packLine.PackLineWeightUnit)
					{
						result += packLine.JL_Calc_WeightToDeliver;
					}
					else
					{
						result += Constants.Weight.Convert(packLine.JL_Calc_WeightToDeliver, packLine.PackLineWeightUnit, unitToCalculateIn, false);
					}
				}

				return result;
			}
		}

		#endregion

		#region TotalWeightUnit

		public ZString TotalWeightUnit
		{
			get
			{
				ZString result = "";

				foreach (PackLine packLine in PackLineCollection)
				{
					if (result.IsEmpty)
					{
						result = packLine.PackLineWeightUnit;
					}
					else if (packLine.PackLineWeightUnit != result)
					{
						result = PackLineCollection.MasterWeightUnit;
						break;
					}
				}

				return result.IsEmpty ? PackLineCollection.MasterWeightUnit : result;
			}
		}

		#endregion

		#region TotalVolume

		public ZDecimal TotalVolume
		{
			get
			{
				ZDecimal result = 0;
				ZString unitToCalculateIn = TotalVolumeUnit;

				foreach (PackLine packLine in PackLineCollection)
				{
					if (unitToCalculateIn == packLine.PackLineVolumeUnit)
					{
						result += packLine.JL_ActualVolume;
					}
					else
					{
						result += Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.PackLineVolumeUnit, unitToCalculateIn, false);
					}
				}

				return result;
			}
		}

		#endregion

		#region TotalVolumeByShipment

		public ZDecimal TotalVolumeByShipment(CommonShipment shipmentBO)
		{
			ZDecimal result = 0;
			ZString unitToCalculateIn = TotalVolumeUnit;

			if (shipmentBO != null)
			{
				foreach (PackLine packLine in PackLineCollection)
				{
					if (packLine.Shipment != null && packLine.Shipment.PK == shipmentBO.PK)
					{
						if (unitToCalculateIn == packLine.PackLineVolumeUnit)
						{
							result += packLine.JL_ActualVolume;
						}
						else
						{
							result += Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.PackLineVolumeUnit, unitToCalculateIn, false);
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region TotalVolumeToDeliver

		public ZDecimal TotalVolumeToDeliver
		{
			get
			{
				ZDecimal result = 0;
				ZString unitToCalculateIn = TotalVolumeUnit;

				foreach (PackLine packLine in PackLineCollection)
				{
					if (unitToCalculateIn == packLine.PackLineVolumeUnit)
					{
						result += packLine.JL_Calc_VolumeToDeliver;
					}
					else
					{
						result += Constants.Volume.Convert(packLine.JL_Calc_VolumeToDeliver, packLine.PackLineVolumeUnit, unitToCalculateIn, false);
					}
				}

				return result;
			}
		}

		#endregion

		#region TotalVolumeUnit

		public ZString TotalVolumeUnit
		{
			get
			{
				ZString result = "";

				foreach (PackLine packLine in PackLineCollection)
				{
					if (result.IsEmpty)
					{
						result = packLine.PackLineVolumeUnit;
					}
					else if (packLine.PackLineVolumeUnit != result)
					{
						result = PackLineCollection.MasterVolumeUnit;
						break;
					}
				}

				return result.IsEmpty ? PackLineCollection.MasterVolumeUnit : result;
			}
		}

		#endregion

		#region TotalLoadingMeters

		public ZDecimal TotalLoadingMeters
		{
			get { return PackLineCollection.Cast<PackLine>().Sum(packLine => packLine.JL_LoadingMeters); }
		}

		#endregion

		#region SubPackLineCollectionView
#if DEBUG
		public
#endif
		class SubPackLineCollectionView : BusinessObjectCollectionView<PackLine>, IPackLineCollection
		{
			public SubPackLineCollectionView(IPackLineCollection packLines)
				: base((BusinessObjectCollection)packLines)
			{
			}

			#region Overrides

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				PackLine packLine = (PackLine)element;
				return !packLine.IsOnColoadMasterShipment;
			}

			protected override void OnAdded(BusinessObject bizOAdded)
			{
				if (!CollectionToFilter.Contains(bizOAdded) && CollectionToFilter is PackLineManyToManyCollection packlineCollection)
				{
					if (IsRebuilding && bizOAdded.PK == rebuildAddingBizoPk)
					{
						ErrorReporter.ReportOnce("ReAddingBizoToCollectionToFilterWhileRebuilding",
							string.Format("{0} with PK '{1}' has already been in CollectionToFilter of type {2} before rebuilding of collection view of type {3}. Remove StackTrace: {4}",
							bizOAdded.GetType().FullName, bizOAdded.PK, CollectionToFilter.GetType().FullName, GetType().FullName, packlineCollection.RemoveStackTrace));
					}
				}
				base.OnAdded(bizOAdded);
			}

			#endregion

			#region IPackLineCollection Members

			PackLineCollectionCalculator IPackLineCollection.Totals
			{
				get { return totals ?? (totals = new PackLineCollectionCalculator(this, PackLineCollectionCalculator.FilterMode.NoFilter)); }
			}
			PackLineCollectionCalculator totals;

			BusinessObjectFactory IPackLineCollection.Factory
			{
				get { return Factory; }
			}

			int IPackLineCollection.Count
			{
				get { return Count; }
			}

			ZString IPackLineCollection.MasterPackagesUnit
			{
				get { return PackLines.MasterPackagesUnit; }
			}

			ZString IPackLineCollection.MasterWeightUnit
			{
				get { return PackLines.MasterWeightUnit; }
			}

			ZString IPackLineCollection.MasterVolumeUnit
			{
				get { return PackLines.MasterVolumeUnit; }
			}

			#endregion

			#region Implementation

			IPackLineCollection PackLines
			{
				get { return (IPackLineCollection)collectionToFilter; }
			}

			#endregion
		}

		#endregion
	}
}
