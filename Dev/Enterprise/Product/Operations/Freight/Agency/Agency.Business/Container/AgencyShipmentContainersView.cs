using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainersView : BusinessObjectCollectionView<AgencyShipmentContainer>, IPackLineParentChangeNotifiable
	{
		public AgencyShipmentContainersView(AgencyShipmentContainerDependentCollection containersCollection, Func<AgencyShipmentContainer, bool> isPartOfThisCollectionPredicate = null)
			: base(containersCollection)
		{
			this.isPartOfThisCollectionPredicate = isPartOfThisCollectionPredicate ?? (container => true);

			Rebuild();

			if (containersCollection.Master != null)
			{
				containersCollection.Master.JS_PackingModeInfo.ValueChanged += (s, e) => Rebuild();
			}
		}

		readonly Func<AgencyShipmentContainer, bool> isPartOfThisCollectionPredicate;

		public new AgencyShipmentContainerDependentCollection CollectionToFilter
		{
			get { return (AgencyShipmentContainerDependentCollection)base.CollectionToFilter; }
		}

		protected override void RebuildOnConstruction()
		{
			// do not rebuild on construction
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return isPartOfThisCollectionPredicate((AgencyShipmentContainer)element);
		}

		#region IPackLineParentChangeNotifiable

		public void NotifyWeightChanged(ZDecimal oldWeight, ZDecimal newWeight)
		{
			CollectionToFilter.NotifyWeightChanged(oldWeight, newWeight);
		}

		public void NotifyWeightUQChanged(ZString oldWeightUQ, ZString newWeightUQ)
		{
			CollectionToFilter.NotifyWeightUQChanged(oldWeightUQ, newWeightUQ);
		}

		public void NotifyVolumeChanged(ZDecimal oldVolume, ZDecimal newVolume)
		{
			CollectionToFilter.NotifyVolumeChanged(oldVolume, newVolume);
		}

		public void NotifyVolumeUQChanged(ZString oldVolumeUQ, ZString newVolumeUQ)
		{
			CollectionToFilter.NotifyVolumeUQChanged(oldVolumeUQ, newVolumeUQ);
		}

		public void NotifyLoadingMetersChanged(ZDecimal oldValue, ZDecimal newValue)
		{
		}

		public void NotifyPackageCountChanged(ZInt oldCount, ZInt newCount)
		{
			CollectionToFilter.NotifyPackageCountChanged(oldCount, newCount);
		}

		public void NotifyPackageTypeChanged(ZString oldType, ZString newType)
		{
			CollectionToFilter.NotifyPackageTypeChanged(oldType, newType);
		}

		public void NotifyDescriptionChanged(ZString oldValue, ZString newValue)
		{
			CollectionToFilter.NotifyDescriptionChanged(oldValue, newValue);
		}

		#endregion

		#region Totals

		#region Container Count

		public ZInt TotalContainers
		{
			get { return this.Cast<AgencyShipmentContainer>().Sum(container => (int)container.JC_ContainerCount); }
		}

		#endregion

		#region Weight

		public ZString TotalWeightUnit
		{
			get
			{
				AgencyShipmentContainer firstContainer = this.Cast<AgencyShipmentContainer>().FirstOrDefault();
				if (firstContainer != null
					&& FreightUtilities.IsValidWeightUnit(firstContainer.JC_GrossWeightUQ)
					&& this.Cast<AgencyShipmentContainer>().All(container => container.JC_GrossWeightUQ == firstContainer.JC_GrossWeightUQ))
				{
					return firstContainer.JC_GrossWeightUQ;
				}

				return GetShipmentWeightUnit();
			}
		}

		public ZDecimal TotalWeight
		{
			get
			{
				if (this.Any())
				{
					ZDecimal result = 0;
					ZString unitToCalculateIn = TotalWeightUnit;

					foreach (AgencyShipmentContainer container in this)
					{
						if (unitToCalculateIn == container.JC_GrossWeightUQ)
						{
							result += container.JC_GrossWeight;
						}
						else
						{
							ZString weightUnit = FreightUtilities.IsValidWeightUnit(container.JC_GrossWeightUQ) ? container.JC_GrossWeightUQ : unitToCalculateIn;
							result += Constants.Weight.Convert(container.JC_GrossWeight, weightUnit, unitToCalculateIn);
						}
					}

					return Utilities.Round(result, JobContainerSchema.JC_GrossWeight.Scale);
				}

				return 0m;
			}
		}

		public ZDecimal TotalWeightInShipmentWeightUnit
		{
			get
			{
				if (this.Any())
				{
					ZString targetWeightUnit = GetShipmentWeightUnit();
					ZDecimal totalWeightInTargetUnit = Constants.Weight.Convert(TotalWeight, TotalWeightUnit, targetWeightUnit);

					return Utilities.Round(totalWeightInTargetUnit, JobContainerSchema.JC_GrossWeight.Scale);
				}

				return 0m;
			}
		}

		#endregion

		#region Volume

		public ZString TotalVolumeUnit
		{
			get
			{
				AgencyShipmentContainer firstContainer = this.Cast<AgencyShipmentContainer>().FirstOrDefault();
				if (firstContainer != null
					&& FreightUtilities.IsValidVolumeUnit(firstContainer.JC_GrossVolumeUQ)
					&& this.Cast<AgencyShipmentContainer>().All(container => container.JC_GrossVolumeUQ == firstContainer.JC_GrossVolumeUQ))
				{
					return firstContainer.JC_GrossVolumeUQ;
				}

				return GetShipmentVolumeUnit();
			}
		}

		public ZDecimal TotalVolume
		{
			get
			{
				if (this.Any())
				{
					ZDecimal result = 0;
					ZString unitToCalculateIn = TotalVolumeUnit;

					foreach (AgencyShipmentContainer container in this)
					{
						if (unitToCalculateIn == container.JC_GrossVolumeUQ)
						{
							result += container.JC_GrossVolume;
						}
						else
						{
							ZString volumeUnit = FreightUtilities.IsValidVolumeUnit(container.JC_GrossVolumeUQ) ? container.JC_GrossVolumeUQ : unitToCalculateIn;
							result += Constants.Volume.Convert(container.JC_GrossVolume, volumeUnit, unitToCalculateIn);
						}
					}

					return Utilities.Round(result, JobContainerSchema.JC_GrossVolume.Scale);
				}

				return 0m;
			}
		}

		public ZDecimal TotalVolumeInShipmentVolumeUnit
		{
			get
			{
				if (this.Any())
				{
					ZString targeVolumeUnit = GetShipmentVolumeUnit();
					ZDecimal totalVolumeInTargetUnit = Constants.Volume.Convert(TotalVolume, TotalVolumeUnit, targeVolumeUnit);

					return Utilities.Round(totalVolumeInTargetUnit, JobContainerSchema.JC_GrossVolume.Scale);
				}

				return 0m;
			}
		}

		public ZString TotalPackagesUnit
		{
			get
			{
				var result = ZString.Empty;

				foreach (AgencyShipmentContainer container in this)
				{
					if (result.IsEmpty)
					{
						result = container.JC_F3_NKPackType;
					}
					else if (container.JC_F3_NKPackType != result)
					{
						result = Constants.PkgUnit.Package;
						break;
					}
				}

				return result.IsEmpty ? Master.JS_F3_NKPackType : result;
			}
		}

		#endregion

		#region Implementation

		AgencyShipment Master
		{
			get { return CollectionToFilter.Master; }
		}

		ZString GetShipmentWeightUnit()
		{
			return FreightUtilities.IsValidWeightUnit(Master.JS_UnitOfWeight) ? Master.JS_UnitOfWeight : Master.RegistryWeightUnit;
		}

		ZString GetShipmentVolumeUnit()
		{
			return FreightUtilities.IsValidVolumeUnit(Master.JS_UnitOfVolume) ? Master.JS_UnitOfVolume : Master.RegistryVolumeUnit;
		}

		#endregion

		#endregion
	}
}



