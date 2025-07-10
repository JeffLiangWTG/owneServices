using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.DebuggerDisplay("Code = {RC_Code}, ISO = {RC_ISOType}")]
	public class RefContainer : AutoRefContainer, Integration.IRefContainer, IDocManagerSupport, IZUnit
	{
		#region Schema

		public new class Schema : AutoRefContainer.Schema
		{
			public const string LengthInchesOnly = "LengthInchesOnly";
			public const string LengthFeetOnly = "LengthFeetOnly";
			public const string LengthMetres = "LengthMetres";
			public const string LengthInches = "LengthInches";
			public const string LengthCentimetres = "LengthCentimetres";
			public const string WidthInchesOnly = "WidthInchesOnly";
			public const string WidthFeetOnly = "WidthFeetOnly";
			public const string WidthMetres = "WidthMetres";
			public const string WidthInches = "WidthInches";
			public const string WidthCentimetres = "WidthCentimetres";
			public const string HeightInchesOnly = "HeightInchesOnly";
			public const string HeightFeetOnly = "HeightFeetOnly";
			public const string HeightMetres = "HeightMetres";
			public const string HeightInches = "HeightInches";
			public const string HeightCentimetres = "HeightCentimetres";
			public const string InsideLengthInches = "InsideLengthInches";
			public const string InsideLengthCentimetres = "InsideLengthCentimetres";
			public const string InsideWidthInches = "WidthInches";
			public const string InsideWidthCentimetres = "WidthCentimetres";
			public const string InsideHeightInches = "InsideHeightInches";
			public const string InsideHeightCentimetres = "InsideHeightCentimetres";
			public const string GrossWeightPounds = "GrossWeightPounds";
			public const string TareWeightPounds = "TareWeightPounds";
			public const string NetWeightPounds = "NetWeightPounds";
			public const string CubicCapacityFeet = "CubicCapacityFeet";
		}

		#endregion

		public RefContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static RefContainer New(BusinessObjectFactory factory)
		{
			return factory.New<RefContainer>();
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefContainer LoadFromCode(ZString rC_Code)
			{
				return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, rC_Code);
			}

			public RefContainer LoadFromISOType(ZString rC_ISOType)
			{
				RefContainer result = null;
				var refContainers = Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, rC_ISOType));

				var activeRefContainers = refContainers.Where(x => x.RC_IsActive).ToArray();
				if (activeRefContainers.Length == 1)
				{
					result = activeRefContainers[0];
				}

				return result;
			}

			public IEnumerable<RefContainer> LoadFromISOTypeWithGroupFallback(ZString rC_ISOType)
			{
				var query = new ZQuery(RefContainerSchema.RC_ISOType, rC_ISOType);
				query.AddToFilter(RefContainerSchema.RC_IsActive, true);
				var activeRefContainers = Factory.Load<RefContainer>(query).ToArray();

				return activeRefContainers.Length == 0
					? LoadFromISOTypeGroupCode(rC_ISOType)
					: activeRefContainers;
			}

			public IEnumerable<RefContainer> LoadFromISOTypeGroupCode(ZString rC_ISOType)
			{
				List<RefContainer> result = new List<RefContainer>();

				var isoTypeGroupCode = ContainerISOType.GroupCodeDescriptionList.TryGetValue(rC_ISOType.SubstringSafe(2, 2), out var groupCodeDescriptionPair)
					? groupCodeDescriptionPair.Code
					: string.Empty;

				if (!string.IsNullOrEmpty(isoTypeGroupCode))
				{
					var refContainersWithSameSize = Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, SQLComparisonOperator.StartsWith, rC_ISOType.SubstringSafe(0, 2))).ToArray();

					if (refContainersWithSameSize.Length > 0)
					{
						result.AddRange(refContainersWithSameSize.Where(x => x.RC_IsActive && x.ISOType.GroupCode == isoTypeGroupCode));
					}
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefContainer);
			}
		}

		#endregion

		#region Default Values and Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RC_IsIso = true;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			using (SuspendSettingHasChanges())
			{
				if (isoType != null)
				{
					ISOType.ISOCode = RC_ISOType;
				}

				SetLengthFromPersistentLength();
				SetHeightFromPersistentHeight();
				SetWidthFromPersistentWidth();

				if (IsAirContainer)
				{
					SetInsideLengthFromPersistentInsideLength();
					SetInsideHeightFromPersistentInsideHeight();
					SetInsideWidthFromPersistentInsideWidth();
				}

				SetGrossWeightFromPersistentGrossWeight();
				SetTareWeightFromPersistentTareWeight();
				SetNetWeightFromPersistentNetWeight();
				SetCubicCapacityFromPersistentCubicCapacity();
			}
		}

		public void SetDefaultForISO()
		{
			SetIsHighCube();
			SetTEU();
			SetContainerTypeForISOType();
		}

		#endregion

		#region Saving and Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Properties

		#region Container Type

		#region Is20GP

		public ZBool Is20GP
		{
			get
			{
				return (RC_ContainerType == Constants.ContainerTypes.DryStorage
						&& RC_Length >= new ZDecimal(TwentyFootMinLength)
						&& RC_Length <= new ZDecimal(TwentyFootMaxLength));
			}
		}

		#endregion

		#region Is40GP

		public ZBool Is40GP
		{
			get
			{
				return (RC_ContainerType == Constants.ContainerTypes.DryStorage
						&& RC_Length >= new ZDecimal(FortyFootMinLength)
						&& RC_Length <= new ZDecimal(FortyFootMaxLength));
			}
		}

		#endregion

		#region Is20RE

		public ZBool Is20RE
		{
			get
			{
				return (IsRefrigerated
						&& RC_Length >= new ZDecimal(TwentyFootMinLength)
						&& RC_Length <= new ZDecimal(TwentyFootMaxLength));
			}
		}

		#endregion

		#region Is40RE

		public ZBool Is40RE
		{
			get
			{
				return (IsRefrigerated
						&& RC_Length >= new ZDecimal(FortyFootMinLength)
						&& RC_Length <= new ZDecimal(FortyFootMaxLength));
			}
		}

		#endregion

		#region RC_Calc_StorageClass

		public ZString RC_Calc_StorageClass
		{
			get
			{
				var storageClass = RC_StorageClass;
				if (RC_ShippingMode == RefContainerLookups.ShippingModes.Air
					&& IsRefrigerated
					&& !storageClass.IsEmpty)
				{
					storageClass += "R";
				}

				return storageClass;
			}
		}

		bool IsRefrigerated => RC_ContainerType == Constants.ContainerTypes.Refrigerated;

		#endregion

		#region IsOtherContainerType

		public ZBool IsOtherContainerType
		{
			get { return !Is20GP && !Is20RE && !Is40GP && !Is40RE; }
		}

		#endregion

		public const double TwentyFootMinLength = 19.5;
		public const double TwentyFootMaxLength = 20.5;
		public const double FortyFootMinLength = 39.5;
		public const double FortyFootMaxLength = 40.5;

		#endregion

		#region RC_Shipping Mode
		[List("Lookups.ShippingModesList")]
		public override ZString RC_ShippingMode
		{
			get { return base.RC_ShippingMode; }
			set
			{
				base.RC_ShippingMode = value;
				if (IsSeaContainer)
				{
					RC_IATARateClass = ZString.Empty;
					RC_StorageClass = ZString.Empty;
				}
				else if (IsAirContainer)
				{
					RC_IsIso = false;
					RC_ISOType = ZString.Empty;
					RC_TEU = 0;
					RC_HasTynes = false;
					RC_HasVents = false;
					RC_IsHighCube = false;
					RC_ContainerType = ZString.Empty;
					RC_StorageClass = ZString.Empty;
				}

				Validation.ValidateRC_Code();
			}
		}

		/// <summary>
		/// Ie, a ULD
		/// </summary>
		public bool IsAirContainer
		{
			get { return RC_ShippingMode == RefContainerLookups.ShippingModes.Air; }
		}

		public bool IsSeaContainer
		{
			get { return RC_ShippingMode == RefContainerLookups.ShippingModes.Sea; }
		}

		public bool IsRoadTruckContainer
		{
			get { return RC_ShippingMode == RefContainerLookups.ShippingModes.Road; }
		}

		#endregion

		#region RC_ContainerType
		[List("Lookups.ContainerTypes")]
		public override ZString RC_ContainerType
		{
			get { return base.RC_ContainerType; }
			set { base.RC_ContainerType = value; }
		}

		void SetContainerTypeForISOType()
		{
			if (base.RC_ISOType.SubstringSafe(2, 1) == "P")
			{
				RC_ContainerType = Constants.ContainerTypes.FlatRack;
			}
		}

		#endregion

		#region RC_ISOEquipmentSizeTypeCode
		[List("Lookups.EquipmentSizeTypeList")]
		public override ZString RC_ISOEquipmentSizeTypeCode
		{
			get
			{
				return base.RC_ISOEquipmentSizeTypeCode;
			}
			set
			{
				base.RC_ISOEquipmentSizeTypeCode = value;
			}
		}
		#endregion

		#region RC_HandlingRateClass
		[List("Lookups.HandlingRateClassList")]
		public override ZString RC_HandlingRateClass
		{
			get
			{
				return base.RC_HandlingRateClass;
			}
			set
			{
				base.RC_HandlingRateClass = value;
			}
		}
		#endregion

		#region RC_FreightRateClass
		[List("Lookups.FreightRateClassList")]
		public override ZString RC_FreightRateClass
		{
			get
			{
				return base.RC_FreightRateClass;
			}
			set
			{
				base.RC_FreightRateClass = value;
			}
		}
		#endregion

		#region RC_StorageClass
		[List("Lookups.StorageClassList")]
		public override ZString RC_StorageClass
		{
			get
			{
				return base.RC_StorageClass;
			}
			set
			{
				base.RC_StorageClass = value;
			}
		}
		#endregion

		#region RC_TEU

		[ReadOnlyMember(nameof(IsAirContainer))]
		public override ZDecimal RC_TEU
		{
			get { return base.RC_TEU; }
			set { base.RC_TEU = value; }
		}

		void SetTEU()
		{
			switch (base.RC_ISOType.SubstringSafe(0, 1))
			{
				case "1":
				case "2":
					base.RC_TEU = 1;
					break;
				case "3":
				case "4":
				case "B":
				case "C":
					base.RC_TEU = 2;
					break;
				default:
					base.RC_TEU = 0;
					break;
			}
		}

		#endregion

		#region RC_HasTynes

		[ReadOnlyMember(nameof(IsAirContainer))]
		public override ZBool RC_HasTynes
		{
			get { return base.RC_HasTynes; }
			set { base.RC_HasTynes = value; }
		}

		#endregion

		#region RC_HasVents

		[ReadOnlyMember(nameof(IsAirContainer))]
		public override ZBool RC_HasVents
		{
			get { return base.RC_HasVents; }
			set { base.RC_HasVents = value; }
		}

		#endregion

		#region RC_IsHighCube

		[ReadOnlyMember(nameof(IsAirContainer))]
		public override ZBool RC_IsHighCube
		{
			get { return base.RC_IsHighCube; }
			set { base.RC_IsHighCube = value; }
		}

		void SetIsHighCube()
		{
			if ((new List<string>() { "5", "6" }).Contains(base.RC_ISOType.SubstringSafe(1, 1)))
			{
				base.RC_IsHighCube = true;
			}
			else
			{
				base.RC_IsHighCube = false;
			}
		}

		#endregion

		#region RC_IATARateClass

		[List("Lookups.IATARateClasses")]
		[ReadOnlyMember(nameof(IsSeaContainer))]
		public override ZString RC_IATARateClass
		{
			get { return base.RC_IATARateClass; }
			set { base.RC_IATARateClass = value; }
		}

		#endregion

		#region RC_IsIso

		[ReadOnly(true)]
		public override ZBool RC_IsIso
		{
			get { return base.RC_IsIso; }
			set { base.RC_IsIso = value; }
		}

		#endregion

		#region RC_ISOType

		[List("Lookups.ISOTypes")]
		public override ZString RC_ISOType
		{
			get { return base.RC_ISOType; }
			set
			{
				base.RC_ISOType = value;
				ISOType.ISOCode = value;
				RC_IsIso = !RC_ISOType.IsEmpty && Lookups.ISOTypes.Any(x => ((ContainerISOType)x).ISOCode == RC_ISOType);
			}
		}

		protected bool RC_ISOType_ReadOnly
		{
			get { return IsAirContainer; }
		}

		#endregion

		#region Length Properties

		protected bool UpdatingLength;

		#region RC_Length

		public override ZDecimal RC_Length
		{
			get { return base.RC_Length; }
			set
			{
				base.RC_Length = value;

				if (!UpdatingLength)
				{
					SetLengthFromPersistentLength();
				}
			}
		}

		void SetLengthFromPersistentLength()
		{
			UpdatingLength = true;
			UpdateLengthNonPersistentProperties();
			UpdatingLength = false;
		}

		void UpdateLengthNonPersistentProperties()
		{
			LengthFeetOnly = Constants.Length.GetFeetFromFeetDecimal(RC_Length);
			LengthFeetOnlyInfo.RefreshBinding();
			LengthInchesOnly = Constants.Length.GetInchesFromFeetDecimal(RC_Length);
			LengthInchesOnlyInfo.RefreshBinding();
			LengthMetres = Constants.Length.Convert(RC_Length, Constants.Length.Feet, Constants.Length.Metres);
			LengthMetresInfo.RefreshBinding();
			LengthInches = Constants.Length.Convert(RC_Length, Constants.Length.Feet, Constants.Length.Inches);
			LengthInchesInfo.RefreshBinding();
			LengthCentimetres = Constants.Length.Convert(RC_Length, Constants.Length.Feet, Constants.Length.Centimetres);
			LengthCentimetresInfo.RefreshBinding();
		}

		#endregion

		#region LengthInchesOnly

		protected ZDecimal fLengthInchesOnly;
		public ZDecimal LengthInchesOnly
		{
			get { return fLengthInchesOnly; }
			set
			{
				SetNonPersistentPropertyValue(LengthInchesOnlyInfo, ref fLengthInchesOnly, value);

				if (!UpdatingLength)
				{
					UpdatingLength = true;

					RC_Length = LengthFeetOnly + Constants.Length.Convert(fLengthInchesOnly, Constants.Length.Inches, Constants.Length.Feet);
					UpdateLengthNonPersistentProperties();

					UpdatingLength = false;
				}
			}
		}

		public ZPropertyInfo LengthInchesOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.LengthInchesOnly); }
		}

		#endregion

		#region LengthFeetOnly

		protected ZDecimal fLengthFeetOnly;
		public ZDecimal LengthFeetOnly
		{
			get { return fLengthFeetOnly; }
			set
			{
				SetNonPersistentPropertyValue(LengthFeetOnlyInfo, ref fLengthFeetOnly, value);

				if (!UpdatingLength)
				{
					UpdatingLength = true;

					RC_Length = fLengthFeetOnly + Constants.Length.Convert(LengthInchesOnly, Constants.Length.Inches, Constants.Length.Feet);
					UpdateLengthNonPersistentProperties();

					UpdatingLength = false;
				}
			}
		}

		public ZPropertyInfo LengthFeetOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.LengthFeetOnly); }
		}

		#endregion

		#region LengthMetres

		protected ZDecimal fLengthMetres;
		public ZDecimal LengthMetres
		{
			get { return fLengthMetres; }
			set
			{
				SetNonPersistentPropertyValue(LengthMetresInfo, ref fLengthMetres, value);

				if (!UpdatingLength)
				{
					UpdatingLength = true;

					RC_Length = Constants.Length.Convert(fLengthMetres, Constants.Length.Metres, Constants.Length.Feet);
					UpdateLengthNonPersistentProperties();

					UpdatingLength = false;
				}
			}
		}

		public ZPropertyInfo LengthMetresInfo
		{
			get { return GetZPropertyInfo(Schema.LengthMetres); }
		}

		#endregion

		#region LengthInches

		protected ZDecimal fLengthInches;
		public ZDecimal LengthInches
		{
			get { return fLengthInches; }
			set
			{
				SetNonPersistentPropertyValue(LengthInchesInfo, ref fLengthInches, value);

				if (!UpdatingLength)
				{
					UpdatingLength = true;

					RC_Length = Constants.Length.Convert(fLengthInches, Constants.Length.Inches, Constants.Length.Feet);
					UpdateLengthNonPersistentProperties();

					UpdatingLength = false;
				}
			}
		}

		public ZPropertyInfo LengthInchesInfo
		{
			get { return GetZPropertyInfo(Schema.LengthInches); }
		}

		#endregion

		#region LengthCentimetres

		protected ZDecimal fLengthCentimetres;
		public ZDecimal LengthCentimetres
		{
			get { return fLengthCentimetres; }
			set
			{
				SetNonPersistentPropertyValue(LengthInchesInfo, ref fLengthCentimetres, value);

				if (!UpdatingLength)
				{
					UpdatingLength = true;

					RC_Length = Constants.Length.Convert(fLengthCentimetres, Constants.Length.Centimetres, Constants.Length.Feet);
					UpdateLengthNonPersistentProperties();

					UpdatingLength = false;
				}
			}
		}

		public ZPropertyInfo LengthCentimetresInfo
		{
			get { return GetZPropertyInfo(Schema.LengthCentimetres); }
		}

		#endregion

		#endregion

		#region Height Properties

		protected bool UpdatingHeight;

		#region RC_Height

		public override ZDecimal RC_Height
		{
			get { return base.RC_Height; }
			set
			{
				base.RC_Height = value;

				if (!UpdatingHeight)
				{
					SetHeightFromPersistentHeight();
				}
			}
		}

		void SetHeightFromPersistentHeight()
		{
			UpdatingHeight = true;
			UpdateHeightNonPersistentProperties();
			UpdatingHeight = false;
		}

		void UpdateHeightNonPersistentProperties()
		{
			HeightFeetOnly = Constants.Length.GetFeetFromFeetDecimal(RC_Height);
			HeightFeetOnlyInfo.RefreshBinding();
			HeightInchesOnly = Constants.Length.GetInchesFromFeetDecimal(RC_Height);
			HeightInchesOnlyInfo.RefreshBinding();
			HeightMetres = Constants.Length.Convert(RC_Height, Constants.Length.Feet, Constants.Length.Metres);
			HeightMetresInfo.RefreshBinding();
			HeightInches = Constants.Length.Convert(RC_Height, Constants.Length.Feet, Constants.Length.Inches);
			HeightInchesInfo.RefreshBinding();
			HeightCentimetres = Constants.Length.Convert(RC_Height, Constants.Length.Feet, Constants.Length.Centimetres);
			HeightCentimetresInfo.RefreshBinding();
		}

		#endregion

		#region HeightInchesOnly

		protected ZDecimal fHeightInchesOnly;
		public ZDecimal HeightInchesOnly
		{
			get { return fHeightInchesOnly; }
			set
			{
				SetNonPersistentPropertyValue(HeightInchesOnlyInfo, ref fHeightInchesOnly, value);

				if (!UpdatingHeight)
				{
					UpdatingHeight = true;

					RC_Height = HeightFeetOnly + Constants.Length.Convert(fHeightInchesOnly, Constants.Length.Inches, Constants.Length.Feet);
					UpdateHeightNonPersistentProperties();

					UpdatingHeight = false;
				}
			}
		}

		public ZPropertyInfo HeightInchesOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.HeightInchesOnly); }
		}

		#endregion

		#region HeightFeetOnly

		protected ZDecimal fHeightFeetOnly;
		public ZDecimal HeightFeetOnly
		{
			get { return fHeightFeetOnly; }
			set
			{
				SetNonPersistentPropertyValue(HeightFeetOnlyInfo, ref fHeightFeetOnly, value);

				if (!UpdatingHeight)
				{
					UpdatingHeight = true;

					RC_Height = fHeightFeetOnly + Constants.Length.Convert(HeightInchesOnly, Constants.Length.Inches, Constants.Length.Feet);
					UpdateHeightNonPersistentProperties();

					UpdatingHeight = false;
				}
			}
		}

		public ZPropertyInfo HeightFeetOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.HeightFeetOnly); }
		}

		#endregion

		#region HeightMetres

		protected ZDecimal fHeightMetres;
		public ZDecimal HeightMetres
		{
			get { return fHeightMetres; }
			set
			{
				SetNonPersistentPropertyValue(HeightMetresInfo, ref fHeightMetres, value);

				if (!UpdatingHeight)
				{
					UpdatingHeight = true;

					RC_Height = Constants.Length.Convert(fHeightMetres, Constants.Length.Metres, Constants.Length.Feet);
					UpdateHeightNonPersistentProperties();

					UpdatingHeight = false;
				}
			}
		}

		public ZPropertyInfo HeightMetresInfo
		{
			get { return GetZPropertyInfo(Schema.HeightMetres); }
		}

		#endregion

		#region HeightInches

		protected ZDecimal fHeightInches;
		public ZDecimal HeightInches
		{
			get { return fHeightInches; }
			set
			{
				SetNonPersistentPropertyValue(HeightInchesInfo, ref fHeightInches, value);

				if (!UpdatingHeight)
				{
					UpdatingHeight = true;

					RC_Height = Constants.Length.Convert(fHeightInches, Constants.Length.Inches, Constants.Length.Feet);
					UpdateHeightNonPersistentProperties();

					UpdatingHeight = false;
				}
			}
		}

		public ZPropertyInfo HeightInchesInfo
		{
			get { return GetZPropertyInfo(Schema.HeightInches); }
		}

		#endregion

		#region HeightCentimetres

		protected ZDecimal fHeightCentimetres;
		public ZDecimal HeightCentimetres
		{
			get { return fHeightCentimetres; }
			set
			{
				SetNonPersistentPropertyValue(HeightCentimetresInfo, ref fHeightCentimetres, value);

				if (!UpdatingHeight)
				{
					UpdatingHeight = true;

					RC_Height = Constants.Length.Convert(fHeightCentimetres, Constants.Length.Centimetres, Constants.Length.Feet);
					UpdateHeightNonPersistentProperties();

					UpdatingHeight = false;
				}
			}
		}

		public ZPropertyInfo HeightCentimetresInfo
		{
			get { return GetZPropertyInfo(Schema.HeightCentimetres); }
		}

		#endregion

		#endregion

		#region Width Properties

		protected bool UpdatingWidth;

		#region RC_Width

		public override ZDecimal RC_Width
		{
			get { return base.RC_Width; }
			set
			{
				base.RC_Width = value;

				if (!UpdatingWidth)
				{
					SetWidthFromPersistentWidth();
				}
			}
		}

		void SetWidthFromPersistentWidth()
		{
			UpdatingWidth = true;
			UpdateWidthNonPersistentProperties();
			UpdatingWidth = false;
		}

		void UpdateWidthNonPersistentProperties()
		{
			WidthFeetOnly = Constants.Length.GetFeetFromFeetDecimal(RC_Width);
			WidthFeetOnlyInfo.RefreshBinding();
			WidthInchesOnly = Constants.Length.GetInchesFromFeetDecimal(RC_Width);
			WidthInchesOnlyInfo.RefreshBinding();
			WidthMetres = Constants.Length.Convert(RC_Width, Constants.Length.Feet, Constants.Length.Metres);
			WidthMetresInfo.RefreshBinding();
			WidthInches = Constants.Length.Convert(RC_Width, Constants.Length.Feet, Constants.Length.Inches);
			WidthInchesInfo.RefreshBinding();
			WidthCentimetres = Constants.Length.Convert(RC_Width, Constants.Length.Feet, Constants.Length.Centimetres);
			WidthCentimetresInfo.RefreshBinding();
		}

		#endregion

		#region WidthInchesOnly

		protected ZDecimal fWidthInchesOnly;
		public ZDecimal WidthInchesOnly
		{
			get { return fWidthInchesOnly; }
			set
			{
				SetNonPersistentPropertyValue(WidthInchesOnlyInfo, ref fWidthInchesOnly, value);

				if (!UpdatingWidth)
				{
					UpdatingWidth = true;

					RC_Width = WidthFeetOnly + Constants.Length.Convert(fWidthInchesOnly, Constants.Length.Inches, Constants.Length.Feet);
					UpdateWidthNonPersistentProperties();

					UpdatingWidth = false;
				}
			}
		}

		public ZPropertyInfo WidthInchesOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.WidthInchesOnly); }
		}

		#endregion

		#region WidthFeetOnly

		protected ZDecimal fWidthFeetOnly;
		public ZDecimal WidthFeetOnly
		{
			get { return fWidthFeetOnly; }
			set
			{
				SetNonPersistentPropertyValue(WidthFeetOnlyInfo, ref fWidthFeetOnly, value);

				if (!UpdatingWidth)
				{
					UpdatingWidth = true;

					RC_Width = fWidthFeetOnly + Constants.Length.Convert(WidthInchesOnly, Constants.Length.Inches, Constants.Length.Feet);
					UpdateWidthNonPersistentProperties();

					UpdatingWidth = false;
				}
			}
		}

		public ZPropertyInfo WidthFeetOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.WidthFeetOnly); }
		}

		#endregion

		#region WidthMetres

		protected ZDecimal fWidthMetres;
		public ZDecimal WidthMetres
		{
			get { return fWidthMetres; }
			set
			{
				SetNonPersistentPropertyValue(WidthMetresInfo, ref fWidthMetres, value);

				if (!UpdatingWidth)
				{
					try
					{
						UpdatingWidth = true;

						RC_Width = Constants.Length.Convert(fWidthMetres, Constants.Length.Metres, Constants.Length.Feet);
						UpdateWidthNonPersistentProperties();
					}
					finally
					{
						UpdatingWidth = false;
					}
				}
			}
		}

		public ZPropertyInfo WidthMetresInfo
		{
			get { return GetZPropertyInfo(Schema.WidthMetres); }
		}

		#endregion

		#region WidthInches

		protected ZDecimal fWidthInches;
		public ZDecimal WidthInches
		{
			get { return fWidthInches; }
			set
			{
				SetNonPersistentPropertyValue(WidthInchesInfo, ref fWidthInches, value);

				if (!UpdatingWidth)
				{
					try
					{
						UpdatingWidth = true;

						RC_Width = Constants.Length.Convert(fWidthInches, Constants.Length.Inches, Constants.Length.Feet);
						UpdateWidthNonPersistentProperties();
					}
					finally
					{
						UpdatingWidth = false;
					}
				}
			}
		}

		public ZPropertyInfo WidthInchesInfo
		{
			get { return GetZPropertyInfo(Schema.WidthInches); }
		}

		#endregion

		#region WidthCentimetres

		protected ZDecimal fWidthCentimetres;
		public ZDecimal WidthCentimetres
		{
			get { return fWidthCentimetres; }
			set
			{
				SetNonPersistentPropertyValue(WidthCentimetresInfo, ref fWidthCentimetres, value);

				if (!UpdatingWidth)
				{
					try
					{
						UpdatingWidth = true;

						RC_Width = Constants.Length.Convert(fWidthCentimetres, Constants.Length.Centimetres, Constants.Length.Feet);
						UpdateWidthNonPersistentProperties();
					}
					finally
					{
						UpdatingWidth = false;
					}
				}
			}
		}

		public ZPropertyInfo WidthCentimetresInfo
		{
			get { return GetZPropertyInfo(Schema.WidthCentimetres); }
		}

		#endregion

		#endregion

		#region GrossWeight Properties

		#region RC_GrossWeight

		public override ZDecimal RC_GrossWeight
		{
			get { return base.RC_GrossWeight; }
			set
			{
				base.RC_GrossWeight = value;
				if (!UpdatingGrossWeight)
				{
					SetGrossWeightFromPersistentGrossWeight();
					if (!IsAirContainer)
					{
						RC_NetWeight = Math.Max(0, RC_GrossWeight - RC_TareWeight);
					}
				}
			}
		}

		#endregion

		#region GrossWeightPounds

		public ZDecimal GrossWeightPounds
		{
			get { return fGrossWeightPounds; }
			set
			{
				SetNonPersistentPropertyValue(GrossWeightPoundsInfo, ref fGrossWeightPounds, value);

				if (!UpdatingGrossWeight)
				{
					UpdatingGrossWeight = true;
					RC_GrossWeight = Utilities.Round(Constants.Weight.Convert(fGrossWeightPounds, Constants.Weight.Pounds, Constants.Weight.Kilograms), 3);
					UpdatingGrossWeight = false;
				}
			}
		}
		ZDecimal fGrossWeightPounds;

		public ZPropertyInfo GrossWeightPoundsInfo
		{
			get { return GetZPropertyInfo(Schema.GrossWeightPounds); }
		}

		#endregion

		void SetGrossWeightFromPersistentGrossWeight()
		{
			UpdatingGrossWeight = true;
			GrossWeightPounds = Utilities.Round(Constants.Weight.Convert(RC_GrossWeight, Constants.Weight.Kilograms, Constants.Weight.Pounds), 3);
			GrossWeightPoundsInfo.RefreshBinding();
			UpdatingGrossWeight = false;
		}
		bool UpdatingGrossWeight;

		#endregion

		#region NetWeight Properties

		#region RC_NetWeight

		public override ZDecimal RC_NetWeight
		{
			get { return base.RC_NetWeight; }
			set
			{
				base.RC_NetWeight = value;
				if (!UpdatingNetWeight)
				{
					SetNetWeightFromPersistentNetWeight();
				}
			}
		}

		protected bool RC_NetWeight_ReadOnly => !IsAirContainer;

		#endregion

		#region NetWeightPounds

		public ZDecimal NetWeightPounds
		{
			get { return fNetWeightPounds; }
			set
			{
				SetNonPersistentPropertyValue(NetWeightPoundsInfo, ref fNetWeightPounds, value);

				if (!UpdatingNetWeight)
				{
					UpdatingNetWeight = true;
					RC_NetWeight = Utilities.Round(Constants.Weight.Convert(fNetWeightPounds, Constants.Weight.Pounds, Constants.Weight.Kilograms), 3);
					UpdatingNetWeight = false;
				}
			}
		}
		ZDecimal fNetWeightPounds;

		public ZPropertyInfo NetWeightPoundsInfo
		{
			get { return GetZPropertyInfo(Schema.NetWeightPounds); }
		}

		protected bool NetWeightPounds_ReadOnly => RC_NetWeight_ReadOnly;

		#endregion

		void SetNetWeightFromPersistentNetWeight()
		{
			UpdatingNetWeight = true;
			NetWeightPounds = Utilities.Round(Constants.Weight.Convert(RC_NetWeight, Constants.Weight.Kilograms, Constants.Weight.Pounds), 3);
			NetWeightPoundsInfo.RefreshBinding();
			UpdatingNetWeight = false;
		}
		bool UpdatingNetWeight;

		#endregion

		#region TareWeight Properties

		#region RC_TareWeight

		public override ZDecimal RC_TareWeight
		{
			get { return base.RC_TareWeight; }
			set
			{
				base.RC_TareWeight = value;
				if (!UpdatingTareWeight)
				{
					SetTareWeightFromPersistentTareWeight();
					if (!IsAirContainer)
					{
						RC_NetWeight = Math.Max(0, RC_GrossWeight - RC_TareWeight);
					}
				}
			}
		}

		#endregion

		#region TareWeightPounds

		public ZDecimal TareWeightPounds
		{
			get { return fTareWeightPounds; }
			set
			{
				SetNonPersistentPropertyValue(TareWeightPoundsInfo, ref fTareWeightPounds, value);

				if (!UpdatingTareWeight)
				{
					UpdatingTareWeight = true;
					RC_TareWeight = Utilities.Round(Constants.Weight.Convert(fTareWeightPounds, Constants.Weight.Pounds, Constants.Weight.Kilograms), 3);
					UpdatingTareWeight = false;
				}
			}
		}
		ZDecimal fTareWeightPounds;

		public ZPropertyInfo TareWeightPoundsInfo
		{
			get { return GetZPropertyInfo(Schema.TareWeightPounds); }
		}

		#endregion

		void SetTareWeightFromPersistentTareWeight()
		{
			UpdatingTareWeight = true;
			TareWeightPounds = Utilities.Round(Constants.Weight.Convert(RC_TareWeight, Constants.Weight.Kilograms, Constants.Weight.Pounds), 3);
			TareWeightPoundsInfo.RefreshBinding();
			UpdatingTareWeight = false;
		}
		bool UpdatingTareWeight;

		#endregion

		#region CubicCapacity Properties

		#region RC_CubicCapacity

		public override ZDecimal RC_CubicCapacity
		{
			get { return base.RC_CubicCapacity; }
			set
			{
				base.RC_CubicCapacity = value;
				if (!UpdatingCubicCapacity)
				{
					SetCubicCapacityFromPersistentCubicCapacity();
				}
			}
		}

		#endregion

		#region CubicCapacityFeet

		public ZDecimal CubicCapacityFeet
		{
			get { return fCubicCapacityFeet; }
			set
			{
				SetNonPersistentPropertyValue(CubicCapacityFeetInfo, ref fCubicCapacityFeet, value);

				if (!UpdatingCubicCapacity)
				{
					UpdatingCubicCapacity = true;
					RC_CubicCapacity = Utilities.Round(Constants.Volume.Convert(fCubicCapacityFeet, Constants.Volume.CubicFeet, Constants.Volume.CubicMetres), 3);
					UpdatingCubicCapacity = false;
				}
			}
		}
		ZDecimal fCubicCapacityFeet;

		public ZPropertyInfo CubicCapacityFeetInfo
		{
			get { return GetZPropertyInfo(Schema.CubicCapacityFeet); }
		}

		#endregion

		void SetCubicCapacityFromPersistentCubicCapacity()
		{
			UpdatingCubicCapacity = true;
			CubicCapacityFeet = Utilities.Round(Constants.Volume.Convert(RC_CubicCapacity, Constants.Volume.CubicMetres, Constants.Volume.CubicFeet), 3);
			CubicCapacityFeetInfo.RefreshBinding();
			UpdatingCubicCapacity = false;
		}
		bool UpdatingCubicCapacity;

		#endregion

		#region Inside Length Properties

		protected bool UpdatingInsideLength;

		#region RC_InsideLength

		public override ZDecimal RC_InsideLength
		{
			get { return base.RC_InsideLength; }
			set
			{
				base.RC_InsideLength = value;

				if (!UpdatingInsideLength)
				{
					SetInsideLengthFromPersistentInsideLength();
				}
			}
		}

		void SetInsideLengthFromPersistentInsideLength()
		{
			UpdatingInsideLength = true;
			UpdateInsideLengthNonPersistentProperties();
			UpdatingInsideLength = false;
		}

		void UpdateInsideLengthNonPersistentProperties()
		{
			InsideLengthInches = Constants.Length.Convert(RC_InsideLength, Constants.Length.Feet, Constants.Length.Inches);
			InsideLengthInchesInfo.RefreshBinding();
			InsideLengthCentimetres = Constants.Length.Convert(RC_InsideLength, Constants.Length.Feet, Constants.Length.Centimetres);
			InsideLengthCentimetresInfo.RefreshBinding();
		}

		#endregion

		#region InsideLengthInches

		protected ZDecimal fInsideLengthInches;
		public ZDecimal InsideLengthInches
		{
			get { return fInsideLengthInches; }
			set
			{
				SetNonPersistentPropertyValue(InsideLengthInchesInfo, ref fInsideLengthInches, value);

				if (!UpdatingInsideLength)
				{
					UpdatingInsideLength = true;

					RC_InsideLength = Constants.Length.Convert(fInsideLengthInches, Constants.Length.Inches, Constants.Length.Feet);
					UpdateInsideLengthNonPersistentProperties();

					UpdatingInsideLength = false;
				}
			}
		}

		public ZPropertyInfo InsideLengthInchesInfo
		{
			get { return GetZPropertyInfo(Schema.InsideLengthInches); }
		}

		#endregion

		#region InsideLengthCentimetres

		protected ZDecimal fInsideLengthCentimetres;
		public ZDecimal InsideLengthCentimetres
		{
			get { return fInsideLengthCentimetres; }
			set
			{
				SetNonPersistentPropertyValue(InsideLengthCentimetresInfo, ref fInsideLengthCentimetres, value);

				if (!UpdatingInsideLength)
				{
					UpdatingInsideLength = true;

					RC_InsideLength = Constants.Length.Convert(fInsideLengthCentimetres, Constants.Length.Centimetres, Constants.Length.Feet);
					UpdateInsideLengthNonPersistentProperties();

					UpdatingInsideLength = false;
				}
			}
		}

		public ZPropertyInfo InsideLengthCentimetresInfo
		{
			get { return GetZPropertyInfo(Schema.InsideLengthCentimetres); }
		}

		#endregion

		#endregion

		#region Inside Height Properties

		protected bool UpdatingInsideHeight;

		#region RC_InsideHeight

		public override ZDecimal RC_InsideHeight
		{
			get { return base.RC_InsideHeight; }
			set
			{
				base.RC_InsideHeight = value;

				if (!UpdatingInsideHeight)
				{
					SetInsideHeightFromPersistentInsideHeight();
				}
			}
		}

		void SetInsideHeightFromPersistentInsideHeight()
		{
			UpdatingInsideHeight = true;
			UpdateInsideHeightNonPersistentProperties();
			UpdatingInsideHeight = false;
		}

		void UpdateInsideHeightNonPersistentProperties()
		{
			InsideHeightInches = Constants.Length.Convert(RC_InsideHeight, Constants.Length.Feet, Constants.Length.Inches);
			InsideHeightInchesInfo.RefreshBinding();
			InsideHeightCentimetres = Constants.Length.Convert(RC_InsideHeight, Constants.Length.Feet, Constants.Length.Centimetres);
			InsideHeightCentimetresInfo.RefreshBinding();
		}

		#endregion

		#region InsideHeightInches

		protected ZDecimal fInsideHeightInches;
		public ZDecimal InsideHeightInches
		{
			get { return fInsideHeightInches; }
			set
			{
				SetNonPersistentPropertyValue(InsideHeightInchesInfo, ref fInsideHeightInches, value);

				if (!UpdatingInsideHeight)
				{
					UpdatingInsideHeight = true;

					RC_InsideHeight = Constants.Length.Convert(InsideHeightInches, Constants.Length.Inches, Constants.Length.Feet);
					UpdateInsideHeightNonPersistentProperties();

					UpdatingInsideHeight = false;
				}
			}
		}

		public ZPropertyInfo InsideHeightInchesInfo
		{
			get { return GetZPropertyInfo(Schema.InsideHeightInches); }
		}

		#endregion

		#region InsideHeightCentimetres

		protected ZDecimal fInsideHeightCentimetres;
		public ZDecimal InsideHeightCentimetres
		{
			get { return fInsideHeightCentimetres; }
			set
			{
				SetNonPersistentPropertyValue(InsideHeightCentimetresInfo, ref fInsideHeightCentimetres, value);

				if (!UpdatingInsideHeight)
				{
					UpdatingInsideHeight = true;

					RC_InsideHeight = Constants.Length.Convert(fInsideHeightCentimetres, Constants.Length.Centimetres, Constants.Length.Feet);
					UpdateInsideHeightNonPersistentProperties();

					UpdatingInsideHeight = false;
				}
			}
		}

		public ZPropertyInfo InsideHeightCentimetresInfo
		{
			get { return GetZPropertyInfo(Schema.InsideHeightCentimetres); }
		}

		#endregion

		#endregion

		#region Inside Width Properties

		protected bool UpdatingInsideWidth;

		#region RC_InsideWidth

		public override ZDecimal RC_InsideWidth
		{
			get { return base.RC_InsideWidth; }
			set
			{
				base.RC_InsideWidth = value;

				if (!UpdatingInsideWidth)
				{
					SetInsideWidthFromPersistentInsideWidth();
				}
			}
		}

		void SetInsideWidthFromPersistentInsideWidth()
		{
			UpdatingInsideWidth = true;
			UpdateInsideWidthNonPersistentProperties();
			UpdatingInsideWidth = false;
		}

		void UpdateInsideWidthNonPersistentProperties()
		{
			InsideWidthInches = Constants.Length.Convert(RC_InsideWidth, Constants.Length.Feet, Constants.Length.Inches);
			InsideWidthInchesInfo.RefreshBinding();
			InsideWidthCentimetres = Constants.Length.Convert(RC_InsideWidth, Constants.Length.Feet, Constants.Length.Centimetres);
			InsideWidthCentimetresInfo.RefreshBinding();
		}

		#endregion

		#region InsideWidthInches

		protected ZDecimal fInsideWidthInches;
		public ZDecimal InsideWidthInches
		{
			get { return fInsideWidthInches; }
			set
			{
				SetNonPersistentPropertyValue(InsideWidthInchesInfo, ref fInsideWidthInches, value);

				if (!UpdatingInsideWidth)
				{
					try
					{
						UpdatingInsideWidth = true;

						RC_InsideWidth = Constants.Length.Convert(fInsideWidthInches, Constants.Length.Inches, Constants.Length.Feet);
						UpdateInsideWidthNonPersistentProperties();
					}
					finally
					{
						UpdatingInsideWidth = false;
					}
				}
			}
		}

		public ZPropertyInfo InsideWidthInchesInfo
		{
			get { return GetZPropertyInfo(Schema.InsideWidthInches); }
		}

		#endregion

		#region InsideWidthCentimetres

		protected ZDecimal fInsideWidthCentimetres;
		public ZDecimal InsideWidthCentimetres
		{
			get { return fInsideWidthCentimetres; }
			set
			{
				SetNonPersistentPropertyValue(InsideWidthCentimetresInfo, ref fInsideWidthCentimetres, value);

				if (!UpdatingInsideWidth)
				{
					try
					{
						UpdatingInsideWidth = true;

						RC_InsideWidth = Constants.Length.Convert(fInsideWidthCentimetres, Constants.Length.Centimetres, Constants.Length.Feet);
						UpdateInsideWidthNonPersistentProperties();
					}
					finally
					{
						UpdatingInsideWidth = false;
					}
				}
			}
		}

		public ZPropertyInfo InsideWidthCentimetresInfo
		{
			get { return GetZPropertyInfo(Schema.InsideWidthCentimetres); }
		}

		#endregion

		#endregion

		#region ISO Type

		public ContainerISOType ISOType
		{
			get
			{
				if (isoType == null)
				{
					isoType = new ContainerISOType(this);
					ISOType.ISOCode = RC_ISOType;
				}
				return isoType;
			}
		}
		ContainerISOType isoType;

		#endregion

		#endregion

		#region Related Objects

		#region Rate Class

		public enum RateClassType
		{
			Freight,
			Handling,
			Storage
		}

		public RefContainerCollection ContainersInSameFreightRateClass
		{
			get { return GetContainerInSameClass(RateClassType.Freight); }
		}

		public RefContainerCollection ContainersInSameHandlingRateClass
		{
			get { return GetContainerInSameClass(RateClassType.Handling); }
		}

		public RefContainerCollection ContainersInSameStorageRateClass
		{
			get { return GetContainerInSameClass(RateClassType.Storage); }
		}

		RefContainerCollection GetContainerInSameClass(RateClassType classType)
		{
			SchemaColumn fieldname = null;
			ZString value = "";
			if (classType == RateClassType.Freight)
			{
				fieldname = RefContainerSchema.RC_FreightRateClass;
				value = RC_FreightRateClass;
			}
			else if (classType == RateClassType.Handling)
			{
				fieldname = RefContainerSchema.RC_HandlingRateClass;
				value = RC_HandlingRateClass;
			}
			else if (classType == RateClassType.Storage)
			{
				fieldname = RefContainerSchema.RC_StorageClass;
				value = RC_StorageClass;
			}

			RefContainerCollection results = new RefContainerCollection(Factory);
			if (value.IsEmpty || fieldname == null)
			{
				results.AdditionalFilter = ZQuery.NoResultQuery;
			}
			else
			{
				ZQuery filter = new ZQuery();
				if (fieldname != null)
				{
					filter.AddToFilter(fieldname, value);
				}

				filter.AddToFilter(RefContainerSchema.PK, SQLComparisonOperator.NotEqual, PK);
				results.AdditionalFilter = filter;
			}

			return results;
		}

		RefContainerCodeMapCollection codeMapCollection;

		[ChildEditable(true)]
		public RefContainerCodeMapCollection CodeMapCollection
		{
			get
			{
				RefContainerCodeMapCollection localCodeMapCollection;
				if (codeMapCollection == null)
				{
					localCodeMapCollection = new RefContainerCodeMapCollection(this);
					RegisterEditableChildObject(localCodeMapCollection);
					localCodeMapCollection.IsManagedForDataRefresh = true;
				}
				else
				{
					localCodeMapCollection = codeMapCollection;
				}
				if (!localCodeMapCollection.IsLoaded)
				{
					localCodeMapCollection.Load();
				}
				codeMapCollection = localCodeMapCollection;
				return codeMapCollection;
			}
		}

		#endregion

		#endregion

		#region Container Class

		public bool MatchesClass(string otherCode)
		{
			if (otherCode == RC_Code)
			{
				return true;
			}

			var other = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, otherCode));
			return MatchesClass(other);
		}

		public bool MatchesClass(RefContainer other)
		{
			return other != null && other.RC_FreightRateClass == RC_FreightRateClass;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ContainerReferenceFiles);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region ZUnit

		ZString IZUnit.Code => RC_Code;
		ZUnitType IZUnit.Type => ZUnitType.RefContainer;

		public static implicit operator ZUnit(RefContainer container) => container == null ? ZUnit.RefContainer.Any : new ZUnit(container);
		public ZUnit ToZUnit() => this;

		#endregion

		[TranslatableDataField(Schema.TableName, Schema.RC_Description, DataXmlFilePaths.RefTables, Type = typeof(RefContainer), SecurityCheckpoint = "ContainersModify", Asmid = ResString.AssemblyId)]
		public override ZString RC_Description
		{
			get { return base.RC_Description; }
			set { base.RC_Description = value; }
		}

		public MultilingualString RC_DescriptionMultilingual
		{
			get { return GetMultilingual(RC_DescriptionInfo); }
		}

		public void DefaultSizing()
		{
		}

		RefContainerCodeMap GetCountrySpecificContainerCodeMap(ZString countryCode, ZString usage)
		{
			return CodeMapCollection.Cast<RefContainerCodeMap>().FirstOrDefault(codeMap => codeMap.RCM_RN_NKCountry == countryCode && codeMap.RCM_Usage == usage);
		}

		public void SetCountrySpecificContainerCode(ZString value, ZString countryCode, string usage = "")
		{
			var codeMap = GetCountrySpecificContainerCodeMap(countryCode, usage);
			if (value.IsEmpty)
			{
				if (codeMap != null)
				{
					CodeMapCollection.RemoveAndDelete(codeMap);
				}
			}
			else
			{
				var usCodeMap = codeMap;
				if (usCodeMap == null)
				{
					usCodeMap = CodeMapCollection.AddNew();
					usCodeMap.RCM_RN_NKCountry = countryCode;
					usCodeMap.RCM_RC_Container = PK;
					usCodeMap.RCM_Usage = usage;
				}
				usCodeMap.RCM_Code = value.Left(usCodeMap.RCM_CodeInfo.MaxLength);
			}
			RC_USContainerCodeInfo.RefreshBinding();
		}

		public ZString GetCountrySpecificContainerCode(ZString countryCode, string usage = "")
		{
			return GetCountrySpecificContainerCodeMap(countryCode, usage)?.RCM_Code ?? ZString.Empty;
		}

		public static ZDBOnlySubQuery GetContainerCodeFilter(ZString country, IEnumerable<ZString> containerCodes)
		{
			var refContainerCodeMapQuery = new ZDBOnlySubQuery(typeof(RefContainerCodeMap), RefContainerCodeMapSchema.RCM_RC_Container);
			refContainerCodeMapQuery.AddToFilter(RefContainerCodeMapSchema.RCM_Code, containerCodes);
			refContainerCodeMapQuery.AddToFilter(RefContainerCodeMapSchema.RCM_RN_NKCountry, country);
			refContainerCodeMapQuery.AddToFilter(RefContainerCodeMapSchema.RCM_Usage, ZString.Empty);
			return refContainerCodeMapQuery;
		}

		public static ZDBOnlySubQuery GetContainerCodeFilter(ZString country, params ZString[] containerCodes)
		{
			return GetContainerCodeFilter(country, (IEnumerable<ZString>)containerCodes);
		}

		public override void Delete()
		{
			CodeMapCollection.RemoveAndDeleteAll();
			base.Delete();
		}
	}
}
