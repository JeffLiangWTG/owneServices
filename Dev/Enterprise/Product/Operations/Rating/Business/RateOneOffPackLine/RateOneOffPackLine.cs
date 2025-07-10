
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Business
{
	[UniversalCopyWithExtendedEntities]
	[UniversalCopyIgnoreElement(nameof(WidthImperial), nameof(HeightImperial), nameof(LengthImperial))]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class RateOneOffPackLine : AutoRateOneOffPackLine,
		IPackTypeDafaultable,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn
	{
		[CodeAlive("is going to used to store the Loose Cargo in One Off Quote")]
		public RateOneOffPackLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TPL_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
			TPL_DimensionUQ = Env.Registry.OuterPacklinesMeasurementDefaultUnit;
			TPL_VolumeUQ = Env.Registry.FreightVolumeUnit;
			TPL_WeightUQ = Env.Registry.FreightWeightUnit;
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("Parent")]
		public override ZGuid TPL_TT_RateOneOffShipment
		{
			get => base.TPL_TT_RateOneOffShipment;
			set => base.TPL_TT_RateOneOffShipment = value;
		}

		[ResourceStringData("RateOneOffPackLine|TPL_PackLineCount", Caption = "Count", FullDescription = "Count of Pack Lines.")]
		public override ZShort TPL_PackLineCount
		{
			get { return base.TPL_PackLineCount; }
			set
			{
				if (base.TPL_PackLineCount != value)
				{
					base.TPL_PackLineCount = value;
					HandleLooseDimensionChange();

					if (!TPL_F3_NKPackTypeInfo.HasErrors())
					{
						SetupDefaultWeightAndDimensionsForPackType();
					}
				}
			}
		}

		[List("Lookups.RefPackTypes")]
		[ResourceStringData("RateOneOffPackLine|TPL_F3_NKPackType", Caption = "Package Type", FullDescription = "Package Type of Loose Cargo.")]
		public override ZString TPL_F3_NKPackType
		{
			get { return base.TPL_F3_NKPackType; }
			set
			{
				base.TPL_F3_NKPackType = value;
				if (!TPL_F3_NKPackTypeInfo.HasErrors())
				{
					SetupDefaultWeightAndDimensionsForPackType();
				}
			}
		}

		[MeasureUnit(Schema.TPL_DimensionUQ, MeasureUnitType.Length)]
		[ResourceStringData("RateOneOffPackLine|TPL_Height", Caption = "Height")]
		public override ZDecimal TPL_Height
		{
			get { return base.TPL_Height; }
			set
			{
				if (base.TPL_Height != value)
				{
					base.TPL_Height = value;
					HandleLooseDimensionChange();
					Validation.ValidateTPL_DimensionUQ();
				}
			}
		}

		[MeasureUnit(Schema.TPL_DimensionUQ, MeasureUnitType.Length)]
		[ResourceStringData("RateOneOffPackLine|TPL_Width", Caption = "Width")]
		public override ZDecimal TPL_Width
		{
			get { return base.TPL_Width; }
			set
			{
				if (base.TPL_Width != value)
				{
					base.TPL_Width = value;
					HandleLooseDimensionChange();
					Validation.ValidateTPL_DimensionUQ();
				}
			}
		}

		[MeasureUnit(Schema.TPL_DimensionUQ, MeasureUnitType.Length)]
		[ResourceStringData("RateOneOffPackLine|TPL_Length", Caption = "Length")]
		public override ZDecimal TPL_Length
		{
			get { return base.TPL_Length; }
			set
			{
				if (base.TPL_Length != value)
				{
					base.TPL_Length = value;
					HandleLooseDimensionChange();
					Validation.ValidateTPL_DimensionUQ();
				}
			}
		}

		[List("Lookups.DimensionUnits")]
		[ResourceStringData("RateOneOffPackLine|TPL_DimensionUQ", Caption = "Dimension Units")]
		public override ZString TPL_DimensionUQ
		{
			get { return base.TPL_DimensionUQ; }
			set
			{
				base.TPL_DimensionUQ = value;
				HandleLooseDimensionChange();
			}
		}

		[MeasureUnit(Schema.TPL_VolumeUQ, MeasureUnitType.Volume)]
		[ResourceStringData("RateOneOffPackLine|TPL_Volume", Caption = "Volume")]
		public override ZDecimal TPL_Volume
		{
			get { return base.TPL_Volume; }
			set
			{
				var roundedValue = this.GetRoundedValue(RateOneOffPackLineSchema.TPL_Volume, TPL_VolumeInfo, value);
				base.TPL_Volume = roundedValue;
				HandleVolumeChange();
				Validation.ValidateTPL_VolumeUQ();
			}
		}

		[List("Lookups.VolumeUnits")]
		[ResourceStringData("RateOneOffPackLine|TPL_VolumeUQ", Caption = "Volume Units")]
		public override ZString TPL_VolumeUQ
		{
			get { return base.TPL_VolumeUQ; }
			set
			{
				base.TPL_VolumeUQ = value;
				HandleVolumeChange();
			}
		}

		[MeasureUnit(Schema.TPL_WeightUQ, MeasureUnitType.Weight)]
		[ResourceStringData("RateOneOffPackLine|TPL_Weight", Caption = "Weight")]
		public override ZDecimal TPL_Weight
		{
			get { return base.TPL_Weight; }
			set
			{
				var roundedValue = this.GetRoundedValue(RateOneOffPackLineSchema.TPL_Weight, TPL_WeightInfo, value);
				base.TPL_Weight = roundedValue;
				HandleWeightChange();
				Validation.ValidateTPL_WeightUQ();
			}
		}

		[List("Lookups.WeightUnits")]
		[ResourceStringData("RateOneOffPackLine|TPL_WeightUQ", Caption = "Weight Units")]
		public override ZString TPL_WeightUQ
		{
			get { return base.TPL_WeightUQ; }
			set
			{
				base.TPL_WeightUQ = value;
				HandleWeightChange();
			}
		}

		#region Vehicle

		[ResourceStringData("RateOneOffPackLine|TPL_RefVehicleIdentificationNumber", Caption = "Vehicle VIN", FullDescription = "Identification Number of Vehicle.")]
		public override ZString TPL_RefVehicleIdentificationNumber { get => base.TPL_RefVehicleIdentificationNumber; set => base.TPL_RefVehicleIdentificationNumber = value; }

		[ResourceStringData("RateOneOffPackLine|TPL_VehicleColor", Caption = "Vehicle Color", FullDescription = "Color of Vehicle.")]
		public override ZString TPL_VehicleColor { get => base.TPL_VehicleColor; set => base.TPL_VehicleColor = value; }

		[ResourceStringData("RateOneOffPackLine|TPL_VehicleMake", Caption = "Vehicle Make", FullDescription = "Make of Vehicle.")]
		public override ZString TPL_VehicleMake { get => base.TPL_VehicleMake; set => base.TPL_VehicleMake = value; }

		[ResourceStringData("RateOneOffPackLine|TPL_VehicleModel", Caption = "Vehicle Model", FullDescription = "Model of Vehicle.")]
		public override ZString TPL_VehicleModel { get => base.TPL_VehicleModel; set => base.TPL_VehicleModel = value; }

		[ResourceStringData("RateOneOffPackLine|TPL_VehicleNumberOfDoors", Caption = "Vehicle Number of Doors", FullDescription = "Number of Doors of Vehicle.")]
		public override ZByte TPL_VehicleNumberOfDoors { get => base.TPL_VehicleNumberOfDoors; set => base.TPL_VehicleNumberOfDoors = value; }

		[ResourceStringData("RateOneOffPackLine|TPL_VehicleTransmission", Caption = "Vehicle Transmission", FullDescription = "Transmission of Vehicle.")]
		[List("Lookups.TPL_VehicleTransmission_List")]
		public override ZString TPL_VehicleTransmission { get => base.TPL_VehicleTransmission; set => base.TPL_VehicleTransmission = value; }

		[ResourceStringData("RateOneOffPackLine|TPL_VehicleYear", Caption = "Vehicle Year", FullDescription = "Year of Vehicle.")]
		public override ZShort TPL_VehicleYear { get => base.TPL_VehicleYear; set => base.TPL_VehicleYear = value; }

		#endregion

		#endregion

		#region New Properties

		[BusinessObjectMaxLengthTestExclude]
		[List("Lookups.LooseCargoContainerTypes")]
		[MaxLength(10)]
		public ZString LooseCargoContainerType
		{
			get
			{
				if (RefContainer != null)
				{
					return RefContainer.RC_Code;
				}

				return ZString.Empty;
			}

			set
			{
				var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, value);
				if (container != null)
				{
					TPL_RC_RefContainer = container.PK;
				}
				else
				{
					TPL_RC_RefContainer = ZGuid.Empty;
				}

				TPL_RC_RefContainerInfo.RefreshBinding();
				LooseCargoContainerTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LooseCargoContainerTypeInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(LooseCargoContainerType)); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("RateOneOffContainers|HeightImperial", Caption = "Height (ft/in)", FullDescription = "The Height in feet and inches.")]
		[MaxLength(15)]
		public ZString HeightImperial
		{
			get { return ToImperialLengthString(TPL_Height, TPL_DimensionUQ); }
			set
			{
				var newValue = FromImperialLengthString(value, TPL_DimensionUQ, RateOneOffPackLineSchema.TPL_Height.Scale);

				if (newValue.HasValue)
				{
					TPL_Height = newValue.Value;
				}

				HeightImperialInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HeightImperialInfo
		{
			get { return GetZPropertyInfo(nameof(HeightImperial)); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("RateOneOffContainers|WidthImperial", Caption = "Width (ft/in)", FullDescription = "The Width in feet and inches.")]
		[MaxLength(15)]
		public ZString WidthImperial
		{
			get { return ToImperialLengthString(TPL_Width, TPL_DimensionUQ); }
			set
			{
				var newValue = FromImperialLengthString(value, TPL_DimensionUQ, RateOneOffPackLineSchema.TPL_Width.Scale);

				if (newValue.HasValue)
				{
					TPL_Width = newValue.Value;
				}

				WidthImperialInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WidthImperialInfo
		{
			get { return GetZPropertyInfo(nameof(WidthImperial)); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("RateOneOffContainers|LengthImperial", Caption = "Length (ft/in)", FullDescription = "The Length in feet and inches.")]
		[MaxLength(15)]
		public ZString LengthImperial
		{
			get { return ToImperialLengthString(TPL_Length, TPL_DimensionUQ); }
			set
			{
				var newValue = FromImperialLengthString(value, TPL_DimensionUQ, RateOneOffPackLineSchema.TPL_Length.Scale);

				if (newValue.HasValue)
				{
					TPL_Length = newValue.Value;
				}

				LengthImperialInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LengthImperialInfo
		{
			get { return GetZPropertyInfo(nameof(LengthImperial)); }
		}

		#endregion

		#region Parent One Off Shipment

		public RateOneOffShipment Parent
		{
			get { return fParent ?? Factory.Load<RateOneOffShipment>(TPL_TT_RateOneOffShipment); }
			internal set { fParent = value; }
		}

		RateOneOffShipment fParent;

		#endregion

		#region Override Delete Method

		public override void Delete()
		{
			var parent = Parent;

			base.Delete();

			RecalculateParentWeightAndVolume(parent);
		}

		void RecalculateParentWeightAndVolume(RateOneOffShipment parent)
		{
			if (parent != null)
			{
				parent.RecalculateVolume();
				parent.RecalculateWeight();
				parent.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Unit Conversion

		ZString ToImperialLengthString(decimal value, string unit)
		{
			var result = ZString.Empty;

			if (IsLengthUnitValid(unit))
			{
				var inches = Convert.ToInt32(ZArchitecture.Core.Utilities.Round(Core.Constants.Length.Convert(value, unit, Core.Constants.Length.Inches), 0));

				result = RatingDataRegistry.Instance.OneOffQuoteImperialUnitsInchesOnly.Value
					? string.Format("{0}\"", inches)
					: string.Format("{0}'{1}\"", inches / 12, inches % 12);
			}

			return result;
		}

		ZDecimal? FromImperialLengthString(string value, string unit, int precision)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return 0m;
			}

			if (!IsLengthUnitValid(unit))
			{
				return null;
			}

			var regex = new Regex(@"^(?<feet>[0-9]+)\'{1}(?<inches>[0-9]+)\'{2}$|^(?<feet>[0-9]+)\'{1}(?<inches>[0-9]+)\""{1}$|^(?<feet>[0-9]+)\'{1}(?<inches>[0-9]+)$|^(?<feet>[0-9]+)\'{1}$|^(?<inches>[0-9]+)\'{2}$|^(?<inches>[0-9]+)\""{1}$|^(?<inches>[0-9]+)$");

			var match = regex.Match(value.Trim());

			if (!match.Success)
			{
				return null;
			}

			decimal feet = ZDecimal.ParseSafe(match.Groups["feet"].Value, 0);
			decimal inches = ZDecimal.ParseSafe(match.Groups["inches"].Value, 0);

			var totalInches = Core.Constants.Length.Convert(feet, Core.Constants.Length.Feet, Core.Constants.Length.Inches) + inches;

			return ZArchitecture.Core.Utilities.Round(Core.Constants.Length.Convert(totalInches, Core.Constants.Length.Inches, unit), precision);
		}

		static bool IsLengthUnitValid(string unit)
		{
			return Core.Constants.Length.Codes.Contains(unit);
		}

		#endregion

		#region IDefaultValuesForPackType Members

		ZDecimal IPackTypeDafaultable.Height
		{
			set { TPL_Height = value; }
		}

		ZDecimal IPackTypeDafaultable.Length
		{
			set { TPL_Length = value; }
		}

		ZDecimal IPackTypeDafaultable.Width
		{
			set { TPL_Width = value; }
		}

		ZDecimal IPackTypeDafaultable.Weight
		{
			set { TPL_Weight = value; }
		}

		ZString IPackTypeDafaultable.UnitOfDimension
		{
			set { TPL_DimensionUQ = value; }
		}

		ZString IPackTypeDafaultable.UnitOfWeight
		{
			set { TPL_WeightUQ = value; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode =>
			Parent is IDefaultNumberOfDecimalsSupporter defaultNumberOfDecimalsSupporter
				? defaultNumberOfDecimalsSupporter.TransportMode
				: ZString.Empty;

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;

			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);
			switch (propertyName)
			{
				case Schema.TPL_Weight:
					unitOfMeasure = TPL_WeightUQ;
					break;
				case Schema.TPL_Volume:
					unitOfMeasure = TPL_VolumeUQ;
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

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(RateOneOffPackLineSchema.TPL_Weight, TPL_WeightInfo);
			this.SetRoundedValue(RateOneOffPackLineSchema.TPL_Volume, TPL_VolumeInfo);
		}

		#endregion

		void SetupDefaultWeightAndDimensionsForPackType()
		{
			OrgBuyerSupplierLinkPackPivot packageDetails = null;

			if (Parent != null)
			{
				var buyerSupplierLink = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Parent.PickUpDocAddress.Organisation, Parent.DeliveryDocAddress.Organisation, Parent.TT_RL_NKDeliveryLocation.Left(2));
				if (buyerSupplierLink != null)
				{
					packageDetails = buyerSupplierLink.PackPivots.GetDetailsForPackType(TPL_F3_NKPackType);
				}
			}

			var packType = Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, TPL_F3_NKPackType);
			this.SetupDefaultWeightAndDimensions(packageDetails, packType, TPL_PackLineCount);
		}

		void HandleLooseDimensionChange()
		{
			if (TPL_Height > 0 && TPL_Width > 0 && TPL_Length > 0 && TPL_PackLineCount > 0 &&
				Constants.Length.ContainsCode(TPL_DimensionUQ) && Constants.Volume.ContainsCode(TPL_VolumeUQ))
			{
				var length = TPL_Length;
				var width = TPL_Width;
				var height = TPL_Height;

				//Convert dimensions to Metres
				if (TPL_DimensionUQ != Constants.Length.Metres)
				{
					length = Constants.Length.Convert(length, TPL_DimensionUQ, Constants.Length.Metres);
					width = Constants.Length.Convert(width, TPL_DimensionUQ, Constants.Length.Metres);
					height = Constants.Length.Convert(height, TPL_DimensionUQ, Constants.Length.Metres);
				}

				//Calculate Volume in M3
				var unroundedVolume = length * width * height * TPL_PackLineCount;

				//Convert to different Volume unit if required
				if (TPL_VolumeUQ != Constants.Volume.CubicMetres)
				{
					unroundedVolume = Constants.Volume.Convert(unroundedVolume, Constants.Volume.CubicMetres, TPL_VolumeUQ);
				}

				TPL_Volume = unroundedVolume;
			}
		}

		#region HandleVolumeChange

		void HandleVolumeChange()
		{
			if (Parent != null)
			{
				Parent.RecalculateVolume();
				Parent.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region HandleWeightChange

		void HandleWeightChange()
		{
			if (Parent != null)
			{
				Parent.RecalculateWeight();
				Parent.MarkAsNeedingValidation();
			}
		}

		#endregion

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return base.GetPropertiesToExcludeFromCloning().Concat(new[]
			{
				RateOneOffPackLineSchema.Constants.TPL_TT_RateOneOffShipment
			});
		}
	}
}
