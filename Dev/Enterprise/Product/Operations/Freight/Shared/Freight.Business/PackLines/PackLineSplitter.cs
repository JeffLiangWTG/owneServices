using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class PackLineSplitter : NonPersistentBusinessObject, IObsoleteValidation, IDefaultNumberOfDecimalsSupporter
	{
		public PackLineSplitter(PackLine line, CommonContainer container) : base()
		{
			this.Line = line;
			this.Container = container;
			line.ReadOnly = true;
			SplitPackages = line.JL_PackageCount;
			SplitWeight = line.JL_ActualWeight;
			SplitVolume = line.JL_ActualVolume;
		}

		public abstract class Schema
		{
			public const string SplitWeight = "SplitWeight";
			public const string SplitVolume = "SplitVolume";
		}

		#region Properties

		#region SplitPackages

		ZInt fSplitPackages;
		public ZInt SplitPackages
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fSplitPackages; }
			set
			{
				SetNonPersistentPropertyValue(SplitPackagesInfo, ref fSplitPackages, value);
				UpdateWeightVolumePercentages();
				ValidateSplitPackages();
			}
		}

		public ZPropertyInfo SplitPackagesInfo
		{
			get { return GetZPropertyInfo(nameof(SplitPackages)); }
		}

		#endregion

		#region SplitWeight

		ZDecimal fSplitWeight;
		public ZDecimal SplitWeight
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fSplitWeight; }
			set
			{
				var roundedValue = this.GetRoundedValue(SplitWeightInfo, value);

				SetNonPersistentPropertyValue(SplitWeightInfo, ref fSplitWeight, roundedValue);
				ValidateSplitWeight();
			}
		}

		public ZPropertyInfo SplitWeightInfo
		{
			get { return GetZPropertyInfo(nameof(SplitWeight)); }
		}

		#endregion

		#region SplitVolume

		ZDecimal fSplitVolume;
		public ZDecimal SplitVolume
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fSplitVolume; }
			set
			{
				var roundedValue = this.GetRoundedValue(SplitVolumeInfo, value);

				SetNonPersistentPropertyValue(SplitVolumeInfo, ref fSplitVolume, roundedValue);
				ValidateSplitVolume();
			}
		}

		public ZPropertyInfo SplitVolumeInfo
		{
			get { return GetZPropertyInfo(nameof(SplitVolume)); }
		}

		#endregion

		#endregion

		#region Validation

		public void ValidateSplitPackages()
		{
			if (!IsValidationSuspended)
			{
				SplitPackagesInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(SplitPackagesInfo);
				if (Line.JL_PackageCount < SplitPackages)
				{
					SplitPackagesInfo.AddError(Res.GetString("81ba2922-be6b-4687-87f5-9dcb453a3878", "Number of packages split cannot be more than the number of packages of the existing packline."));
				}
				else if (Line.JL_PackageCount == SplitPackages)
				{
					SplitPackagesInfo.AddError(Res.GetString("d9ec5717-0dd5-4fa4-a34a-7f009ff3b83b", "Split amount is the same as the packline total, no split will occur."));
				}

				ValidateConfirmDivotPackagesDelivered();
			}
		}

		void ValidateConfirmDivotPackagesDelivered()
		{
			var divotConfirmTypes = Line.ConfirmDivots
				.Where(d => d.J8_PackagesDelivered != Line.JL_PackageCount)
				.Select(c => c.Confirm != null ? c.Confirm.EU_PickupDeliveryType : ZString.Empty)
				.Distinct()
				.ToArray();

			var pathList = new List<string>();

			foreach (var type in divotConfirmTypes)
			{
				switch (type)
				{
					case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:

						pathList.Add(Res.GetString("7d9db585-00d8-4bba-8b03-eb4e232f475f", "Forwarding > Shipment > Pickup Or Delivery > Confirmations"));
						break;

					case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:

						pathList.Add(Res.GetString("b4c7b419-50c0-47e2-9b69-43bd6f02c2b4", "CFS > Shipment > Arrival"));
						break;

					case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:

						pathList.Add(Res.GetString("4de40f83-73ba-4f07-9d67-457291ea0d77", "CFS > Gate Pass > Delivery Information"));
						break;
				}
			}

			if (pathList.Any())
			{
				var path = string.Join(System.Environment.NewLine, pathList);

				var errorMessage = Res.GetString("c44873dd-e419-4c78-839c-3b9a80c31b90",
					@"Packline cannot be split because it has multiple confirmations or Packline packs quantity does not correspond to confirmed packs quantity.
Please remove these redundant confirmations or change the confirmed packs quantity via:

{0}", path);

				SplitPackagesInfo.AddError(errorMessage);
			}
		}

		public void ValidateSplitWeight()
		{
			if (!IsValidationSuspended)
			{
				SplitWeightInfo.ClearAllNotifications();
				if (Line.JL_ActualWeight == 0)
				{
					CompareValidation.CheckEqual(SplitWeightInfo, Line.JL_ActualWeightInfo);
				}
				else
				{
					CompareValidation.CheckNumberGreaterThanZero(SplitWeightInfo);
				}
				if (Line.JL_ActualWeight < SplitWeight)
				{
					SplitWeightInfo.AddError(Res.GetString("22c6f1d0-3121-40d7-a5d5-80125d1b9348", "Split weight cannot be more than the weight of the existing packline."));
				}
			}
		}

		public void ValidateSplitVolume()
		{
			if (!IsValidationSuspended)
			{
				SplitVolumeInfo.ClearAllNotifications();
				if (Line.JL_ActualVolume == 0)
				{
					CompareValidation.CheckEqual(SplitVolumeInfo, Line.JL_ActualVolumeInfo);
				}
				else
				{
					CompareValidation.CheckNumberGreaterThanZero(SplitVolumeInfo);
				}
				if (Line.JL_ActualVolume < SplitVolume)
				{
					SplitVolumeInfo.AddError(Res.GetString("c917ad17-48d6-4ea7-8360-92d0ce50acd6", "Split volume cannot be more than the volume of the existing packline."));
				}
			}
		}

		#endregion

		protected void UpdateWeightVolumePercentages()
		{
			if (Line.JL_PackageCount != 0 && SplitPackages < Line.JL_PackageCount)
			{
				ZDecimal singleWeight = ((decimal)Line.JL_ActualWeight) / ((decimal)Line.JL_PackageCount);
				SplitWeight = ((decimal)singleWeight) * ((decimal)SplitPackages);
				SplitWeightInfo.RefreshBinding();
				ZDecimal singleVolume = ((decimal)Line.JL_ActualVolume) / ((decimal)Line.JL_PackageCount);
				SplitVolume = ((decimal)singleVolume) * ((decimal)SplitPackages);
				SplitVolumeInfo.RefreshBinding();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSplitPackages();
			ValidateSplitWeight();
			ValidateSplitVolume();
		}

		public ZBool Split()
		{
			if (Line.IsDeleted)
			{
				this.AddRowError(Res.GetString("c9d23985-18c3-4122-a3fc-fe9494eaebcd", "The packline you are trying to split has been deleted or does not exist."));
				return false;
			}
			RunPreSaveValidation();
			if (!this.HasErrors)
			{
				var originalPackageCount = Line.JL_PackageCount;
				var originalVolume = Line.JL_ActualVolume;
				var originalWeight = Line.JL_ActualWeight;

				var splitLine = (PackLine)Line.Clone();

				Line.JL_PackageCount = originalPackageCount - SplitPackages;
				Line.JL_ActualVolume = originalVolume - SplitVolume;
				Line.JL_ActualWeight = originalWeight - SplitWeight;

				UpdatePackagesDelivered(Line);

				//additional calculation to ensure totals still add up -  - we get rounding errors in division occasionally
				var splitVolume = SplitVolume + (originalVolume - Line.JL_ActualVolume - SplitVolume);
				var splitWeight = SplitWeight + (originalWeight - Line.JL_ActualWeight - SplitWeight);

				splitLine.JL_PackageCount = SplitPackages;
				splitLine.JL_ActualVolume = splitVolume;
				splitLine.JL_ActualWeight = splitWeight;

				splitLine.JL_OriginTransitWarehouseStatus = Line.JL_OriginTransitWarehouseStatus;
				splitLine.JL_DepartureTransitWarehouseExcluded = Line.JL_DepartureTransitWarehouseExcluded;
				splitLine.JL_OA_LastKnownTransitWarehouseAddress = Line.JL_OA_LastKnownTransitWarehouseAddress;
				splitLine.JL_LastKnownTransitWarehouseStatus = Line.JL_LastKnownTransitWarehouseStatus;
				splitLine.JL_LastKnownTransitWarehouseStatusDateTime = Line.JL_LastKnownTransitWarehouseStatusDateTime;

				if (!Line.Shipment.OuterPackLines.Contains(splitLine.PK))
				{
					Line.Shipment.OuterPackLines.Add(splitLine);
				}

				Container.AddPackLine(splitLine);

				if (!splitLine.IsDeleted)
				{
					UpdatePackagesDelivered(splitLine);
				}

				return true;
			}
			return false;
		}

		void UpdatePackagesDelivered(PackLine packLine)
		{
			packLine.ConfirmDivots.ForEach(divot => divot.J8_PackagesDelivered = packLine.JL_PackageCount);
		}

		#region PackLine

		PackLine fLine;
		public PackLine Line
		{
			get { return fLine; }
			set { fLine = value; }
		}

		#endregion

		#region CommonContainer

		CommonContainer fContainer;
		public CommonContainer Container
		{
			get { return fContainer; }
			set { fContainer = value; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parent = null;

				if (Line != null)
				{
					parent = Line;
				}
				else if (Container != null)
				{
					parent = Container;
				}

				var result = (parent != null) ? parent.TransportMode : ZString.Empty;

				return result;
			}
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.SplitWeight:
					unitOfMeasure = Line.JL_ActualWeightUQ;
					break;

				case Schema.SplitVolume:
					unitOfMeasure = Line.JL_ActualVolumeUQ;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// no change in transport mode expected during the lifecycle of this bizObj
		}

		#endregion
	}
}
