using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageContainer : AutoPkgPackageContainer, IPackingHasChanges, ITemperatureSettings
	{
		public PkgPackageContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public abstract new class Schema : AutoPkgPackageContainer.Schema
		{
			public const string RefContainerLength = "RefContainerLength";
			public const string RefContainerWidth = "RefContainerWidth";
			public const string RefContainerHeight = "RefContainerHeight";

			public const string OverhangLength = "OverhangLength";
			public const string OverhangWidth = "OverhangWidth";
			public const string OverhangHeight = "OverhangHeight";

			public const string PackageWeightUQ = "PackageWeightUQ";
			public const string PackageWeight = "PackageWeight";
		}

		#endregion

		#region Related Entities

		#region Package

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(K0_KP_Package); }
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region K0_ContainerMode

		[List("Lookups.ContainerModes")]
		public override ZString K0_ContainerMode
		{
			get { return base.K0_ContainerMode; }
			set { base.K0_ContainerMode = value; }
		}

		#endregion

		#region K0_KP_Package

		[RelatedBusinessObject("Package")]
		public override ZGuid K0_KP_Package
		{
			get { return base.K0_KP_Package; }
			set { base.K0_KP_Package = value; }
		}

		#endregion

		#region K0_Quality

		[List("Lookups.ContainerQualities")]
		public override ZString K0_Quality
		{
			get { return base.K0_Quality; }
			set { base.K0_Quality = value; }
		}

		#endregion

		#region K0_RC_ContainerType

		public override ZGuid K0_RC_ContainerType
		{
			get { return base.K0_RC_ContainerType; }
			set
			{
				base.K0_RC_ContainerType = value;

				if (!IsCopying)
				{
					DefaultDetailsFromRefContainer();
				}
			}
		}

		void DefaultDetailsFromRefContainer()
		{
			var refContainer = ContainerType;
			var package = Package;

			if (refContainer != null)
			{
				package.KP_Length = RefContainerLength;
				package.KP_Width = RefContainerWidth;
				package.KP_Height = RefContainerHeight;

				// RefContainer Weight Unit is KG. Convert to the Package Weight Unit.
				package.KP_TareWeight = (package.Lookups.WeightUQs.ContainsCode(package.KP_WeightUQ)
					? Constants.Weight.Convert(ContainerType.RC_TareWeight, Constants.Weight.Kilograms, package.KP_WeightUQ)
					: (decimal)refContainer.RC_TareWeight) * package.KP_PackageQty;
			}
			else
			{
				package.KP_Length = 0;
				package.KP_Width = 0;
				package.KP_Height = 0;
				package.KP_TareWeight = 0m;
			}
		}

		#endregion

		#region K0_Status

		[List("Lookups.ContainerStatuses")]
		public override ZString K0_Status
		{
			get { return base.K0_Status; }
			set { base.K0_Status = value; }
		}

		#endregion

		#region Package Proxys

		#region PackageWeightUQ

		[List("Package.Lookups.WeightUQs")]
		public ZString PackageWeightUQ
		{
			get { return Package != null ? Package.KP_WeightUQ : ZString.Empty; }
			set
			{
				if (Package != null)
				{
					Package.KP_WeightUQ = value;
				}
			}
		}

		#endregion

		#region PackageWeight

		[MeasureUnit(Schema.PackageWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal PackageWeight
		{
			get { return Package != null ? Package.KP_Weight : ZDecimal.Zero; }
			set
			{
				if (Package != null)
				{
					Package.KP_Weight = value;
				}
			}
		}

		#endregion

		#endregion

		// persistent - temperature details

		#region K0_AirVentFlowRate

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZDecimal K0_AirVentFlowRate
		{
			get { return base.K0_AirVentFlowRate; }
			set { base.K0_AirVentFlowRate = value; }
		}

		#endregion

		#region K0_AirVentFlowRateUnit

		[List("Lookups.AirVentFlowRateUnits")]
		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZString K0_AirVentFlowRateUnit
		{
			get { return base.K0_AirVentFlowRateUnit; }
			set { base.K0_AirVentFlowRateUnit = value; }
		}

		#endregion

		#region K0_HumidityPercent

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZByte K0_HumidityPercent
		{
			get { return base.K0_HumidityPercent; }
			set { base.K0_HumidityPercent = value; }
		}

		#endregion

		#region K0_RefrigGeneratorID

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZString K0_RefrigGeneratorID
		{
			get { return base.K0_RefrigGeneratorID; }
			set { base.K0_RefrigGeneratorID = value; }
		}

		#endregion

		#region K0_SetPointTemp

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZDecimal K0_SetPointTemp
		{
			get { return base.K0_SetPointTemp; }
			set { base.K0_SetPointTemp = value; }
		}

		#endregion

		#region K0_SetPointTempUnit

		[List("Package.Lookups.TemperatureUnits")]
		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZString K0_SetPointTempUnit
		{
			get { return base.K0_SetPointTempUnit; }
			set { base.K0_SetPointTempUnit = value; }
		}

		#endregion

		#region K0_TempRecorderSerialNumber

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZString K0_TempRecorderSerialNumber
		{
			get { return base.K0_TempRecorderSerialNumber; }
			set { base.K0_TempRecorderSerialNumber = value; }
		}

		#endregion

		#region TemperatureDetailsReadOnly

		protected bool TemperatureDetailsReadOnly
		{
			get { return !K0_IsControlledAtmosphere; }
		}

		#endregion

		#region SealParty

		[List("Lookups.SealParty_List")]
		public override ZString K0_Seal1PartyType
		{
			get { return base.K0_Seal1PartyType; }
			set { base.K0_Seal1PartyType = value; }
		}

		[List("Lookups.SealParty_List")]
		public override ZString K0_Seal2PartyType
		{
			get { return base.K0_Seal2PartyType; }
			set { base.K0_Seal2PartyType = value; }
		}

		[List("Lookups.SealParty_List")]
		public override ZString K0_Seal3PartyType
		{
			get { return base.K0_Seal3PartyType; }
			set { base.K0_Seal3PartyType = value; }
		}

		#endregion

		// calculated

		#region RefContainerLength

		public ZDecimal RefContainerLength
		{
			get { return GetRefContainerMeasurementInPackageUQ(RefContainerSchema.RC_Length); }
		}

		#endregion

		#region RefContainerWidth

		public ZDecimal RefContainerWidth
		{
			get { return GetRefContainerMeasurementInPackageUQ(RefContainerSchema.RC_Width); }
		}

		#endregion

		#region RefContainerHeight

		public ZDecimal RefContainerHeight
		{
			get { return GetRefContainerMeasurementInPackageUQ(RefContainerSchema.RC_Height); }
		}

		#endregion

		#region OverhangLength

		public ZDecimal OverhangLength
		{
			get { return GetRefContainerOverhangInPackageUQ(RefContainerSchema.RC_Length, PkgPackageSchema.KP_Length); }
		}

		public ZPropertyInfoDecimal OverhangLengthInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.OverhangLength); }
		}

		#endregion

		#region OverhangWidth

		public ZDecimal OverhangWidth
		{
			get { return GetRefContainerOverhangInPackageUQ(RefContainerSchema.RC_Width, PkgPackageSchema.KP_Width); }
		}

		public ZPropertyInfoDecimal OverhangWidthInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.OverhangWidth); }
		}

		#endregion

		#region OverhangHeight

		public ZDecimal OverhangHeight
		{
			get { return GetRefContainerOverhangInPackageUQ(RefContainerSchema.RC_Height, PkgPackageSchema.KP_Height); }
		}

		public ZPropertyInfoDecimal OverhangHeightInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.OverhangHeight); }
		}

		#endregion

		#region GoodsWeight

		[MeasureUnit(Schema.PackageWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal GoodsWeight
		{
			get => Package?.GoodsWeight ?? 0m;
			set
			{
				if (!IsCopying && Package != null)
				{
					Package.GoodsWeight = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateGoodsWeight();
				}

				GoodsWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfoDecimal GoodsWeightInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(nameof(GoodsWeight)); }
		}

		#endregion

		#region TemperatureSummary

		public ZString TemperatureSummary
		{
			get { return Res.GetString("9715f922-ad95-45e8-a029-3ebdbd5afd35", "{0} °{1}", K0_SetPointTemp.ToStringTrimZeros("N"), K0_SetPointTempUnit); }
		}

		#endregion

		#region Measurement Helpers

		ZDecimal GetRefContainerOverhangInPackageUQ(SchemaDecimalColumn refContainerDimensionSchemaColumn, SchemaDecimalColumn packageDimensionSchemaColumn)
		{
			ZDecimal result = 0m;

			if (ContainerType != null)
			{
				var length = GetRefContainerMeasurementInPackageUQ(refContainerDimensionSchemaColumn);
				var packageDimension = (ZDecimal)Package[packageDimensionSchemaColumn];
				result = packageDimension - length;
			}

			return result;
		}

		ZDecimal GetRefContainerMeasurementInPackageUQ(SchemaDecimalColumn refContainerDimensionSchemaColumn)
		{
			var result = 0m;

			var refContainer = ContainerType;
			if (refContainer != null)
			{
				var package = Package;
				var refContainerDimension = (ZDecimal)refContainer[refContainerDimensionSchemaColumn];

				// RefContainer Dimension Unit is Feet. Convert to the Package Dimension Unit.
				result = package.Lookups.DimensionUQs.ContainsCode(package.KP_DimensionUQ)
					? Constants.Length.Convert(refContainerDimension, Constants.Length.Feet, package.KP_DimensionUQ)
					: (decimal)refContainerDimension;
			}

			return result;
		}

		#endregion

		#endregion

		#region Flags

		#region HasAllDefaultValues

		public bool HasAllDefaultValues
		{
			get
			{
				foreach (var info in ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => !PropertiesToExcludeFromHasAllDefaultValues.Contains(info)))
				{
					if (!info.Value.Equals(info.DefaultValue))
					{
						return false;
					}
				}

				return true;
			}
		}

		ZPropertyInfo[] PropertiesToExcludeFromHasAllDefaultValues
		{
			get { return new ZPropertyInfo[] { K0_KP_PackageInfo }; } //, PackageWeightUQInfo, PackageWeightInfo }; }
		}

		#endregion

		// temperature

		#region IsNotTemperatureControlled

		public ZBool IsNotTemperatureControlled
		{
			get { return !K0_IsControlledAtmosphere; }
			set { SetTemperatureFlagExclusive(value, SetIsNotTemperatureControlledCore); }
		}

		void SetIsNotTemperatureControlledCore(bool value)
		{
			K0_IsControlledAtmosphere = !value;
			if (value)
			{
				ResetTemperatureDetails();
			}
		}

		void ResetTemperatureDetails()
		{
			K0_AirVentFlowRate = 0m;
			K0_AirVentFlowRateUnit = "";
			K0_HumidityPercent = 0;
			K0_RefrigGeneratorID = "";
			K0_SetPointTemp = 0m;
			K0_SetPointTempUnit = "";
			K0_TempRecorderSerialNumber = "";
		}

		#endregion

		#region IsChiller

		public ZBool IsChiller
		{
			get { return this.IsChiller(); }
			set { SetTemperatureFlagExclusive(value, (isChiller) => this.SetIsChiller(isChiller)); }
		}

		#endregion

		#region IsFreezer

		public ZBool IsFreezer
		{
			get { return this.IsFreezer(); }
			set { SetTemperatureFlagExclusive(value, (isFreezer) => this.SetIsFreezer(isFreezer)); }
		}

		#endregion

		#region SetTemperatureFlagExclusive

		void SetTemperatureFlagExclusive(bool value, Action<bool> setTemperatureFlag)
		{
			if (value || IsResettingTemperatureFlags.IsSuspended)
			{
				using (new SemaphoreManager(IsResettingTemperatureFlags))
				{
					setTemperatureFlag(value);
				}
			}
		}

		/// <summary>
		/// NoTemperatureControl/Chiller/Freezer all set each other to
		/// false, we must lock during this to prevent incorrect results.
		/// </summary>
		Semaphore IsResettingTemperatureFlags
		{
			get { return isResettingTemperatureFlags ?? (isResettingTemperatureFlags = new Semaphore()); }
		}

		Semaphore isResettingTemperatureFlags;

		#endregion

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IPackingHasChanges Members

		bool IPackingHasChanges.HasChangesThatAreInvalidIfFinalised
		{
			// tested in IBusinessExtensionsTest.TestHasChangesOnChildrenNotValidIfFinalised()
			get { return ((IBusiness)this).HasChangesNotIncludingChildren; }
		}

		#endregion

		#region ITemperatureSettings Members

		bool ITemperatureSettings.IsTemperatureControlled
		{
			get { return K0_IsControlledAtmosphere; }
			set { K0_IsControlledAtmosphere = value; }
		}

		ZDecimal ITemperatureSettings.TemperatureMin
		{
			get { return K0_SetPointTemp; }
			set { K0_SetPointTemp = value; }
		}

		ZDecimal ITemperatureSettings.TemperatureMax
		{
			get { return K0_SetPointTemp; }
			set { K0_SetPointTemp = value; }
		}

		ZString ITemperatureSettings.TemperatureUnit
		{
			get { return K0_SetPointTempUnit; }
			set { K0_SetPointTempUnit = value; }
		}

		#endregion

		#region Validation

		protected override PkgPackageContainerValidation GetNewValidation()
		{
			var package = Package;
			return package == null || package.PackageJob == null || !package.PackageJob.KJ_IsFinalized
					? new PkgPackageContainerValidationForUnfinalisedPackageJob(this)
					: new PkgPackageContainerValidation(this);
		}

		#endregion
	}
}
