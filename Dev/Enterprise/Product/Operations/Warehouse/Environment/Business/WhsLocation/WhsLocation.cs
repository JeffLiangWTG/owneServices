using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[ActionFieldFollow(false)]
	[CodeProperty(AutoWhsLocationView.Schema.WLV_LocationString_UserFriendly), DescriptionProperty(AutoWhsLocationView.Schema.WLV_LocationString_UserFriendly)]
	public sealed class WhsLocation : AutoWhsLocationView, IWhsLocation
	{
		public const string DefaultQuantityUnit = "UNT";
		public const byte EmptyCheckDigit = 255;

		public WhsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LocationString), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LocationString_UserFriendly), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_FormattedColumn), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_FormattedLevel), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_FormattedTray), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LastInventoryChangeDate), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LastAllocatedOrChangedDateUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_WarehouseType), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_IsVirtualWarehouse), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LocationClass), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LocationTypeCode), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_PickingAreaType), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_PutawayAreaType), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LastAllocatedOrChangedID), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_IsValidLocationForProductWarehousePutaway), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WLV_LastConfigChangedUtc), ConcurrencyPolicy.Ignore);
		}

		public abstract new class Schema : AutoWhsLocationView.Schema
		{
			public const string FormattedColumn = nameof(FormattedColumn);
			public const string FormattedLevel = nameof(FormattedLevel);
			public const string FormattedTray = nameof(FormattedTray);
			public const string FormattedCheckDigit = nameof(FormattedCheckDigit);
			public const string RowName = nameof(RowName);
			public const string RowLocationSequence = nameof(RowLocationSequence);
			public const string PickArea = nameof(PickArea);
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WLV_Column = 1;
			WLV_Level = 1;
			WLV_Tray = 1;
			WLV_CheckDigit = EmptyCheckDigit;
			WLV_LocationStatus = LocationStatus.Codes.Normal;
			WLV_PickMethod = PickMethods.DefaultCode;
			WLV_ApprovedKnownLocation = CodeLists.ApprovedKnownStatus.Codes.NO;
			WLV_MaxQuantityUnit = DefaultQuantityUnit;
		}

		#endregion

		#region Related Entities

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WLV_WW_Whs); }
		}

		IWhsWarehouse IWhsLocation.Warehouse => Warehouse;

		public WhsArea PickingArea => Factory.Load<WhsArea>(WLV_WA_PickingArea);

		public WhsArea PutawayArea => Factory.Load<WhsArea>(WLV_WA_PutawayArea);

		public WhsRow Row
		{
			get { return Factory.Load<WhsRow>(WLV_WR); }
		}

		public WhsLocationType LocationType
		{
			get { return Factory.Load<WhsLocationType>(WLV_WLT_LocationType); }
		}

		#endregion

		#region Properties

		#region WLV_RS_NKTransitServiceLevel

		public override ZString WLV_RS_NKTransitServiceLevel
		{
			get { return base.WLV_RS_NKTransitServiceLevel; }

			set
			{
				base.WLV_RS_NKTransitServiceLevel = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_TransitDischargeLRC();
				}
			}
		}

		#endregion

		#region WLV_TransitDischargeLRC

		[List("Lookups.Locations")]
		public override ZString WLV_TransitDischargeLRC
		{
			get { return base.WLV_TransitDischargeLRC; }
			set
			{
				base.WLV_TransitDischargeLRC = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_RS_NKTransitServiceLevel();
				}
			}
		}

		#endregion

		#region WLV_WarehouseType

		[BusinessObjectTestExclude] // We do not set WLV_WarehouseType. The setter throws an exception.
		public override ZString WLV_WarehouseType
		{
			get => Warehouse?.WW_WarehouseType ?? string.Empty;
			set => throw new NotSupportedException("WLV_WarehouseType should not be set."); // Exception message
		}

		#endregion

		#region WLV_LocationClass

		[BusinessObjectTestExclude] // We do not set WLV_LocationClass. The setter throws an exception.
		public override ZString WLV_LocationClass
		{
			get => LocationType?.WLT_LocationClass ?? ZString.Empty;
			set => throw new NotSupportedException("WLV_LocationClass should not be set."); // Exception message
		}

		#endregion

		#region WLV_LocationTypeCode

		[BusinessObjectTestExclude] // We do not set WLV_LocationTypeCode. The setter throws an exception.
		public override ZString WLV_LocationTypeCode
		{
			get => base.WLV_LocationTypeCode;
			set => throw new NotSupportedException("WLV_LocationTypeCode should not be set."); // Exception message
		}

		#endregion

		#region PickingAreaName

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		public ZString PickingAreaName => PickingArea?.WA_Name ?? ZString.Empty;

		#endregion

		#region WLV_IsVirtualWarehouse

		[BusinessObjectTestExclude] // We do not set WLV_IsVirtualWarehouse. The setter throws an exception.
		public override ZBool WLV_IsVirtualWarehouse
		{
			get => Warehouse?.WW_IsVirtualWarehouse ?? false;
			set => throw new NotSupportedException("WLV_IsVirtualWarehouse should not be set."); // Exception message
		}

		#endregion

		#region WLV_LocationString

		[BusinessObjectTestExclude] // We do not set WLV_LocationString. The setter throws an exception.
		public override ZString WLV_LocationString
		{
			get => IsInDatabase
					? base.WLV_LocationString
					: GetLocationString(false);
			set => throw new NotSupportedException("WLV_LocationString should not be set.");  // Exception message
		}

		[BusinessObjectTestExclude] // We do not set WLV_LocationString_UserFriendly. The setter throws an exception.
		public override ZString WLV_LocationString_UserFriendly
		{
			get => IsInDatabase
					? base.WLV_LocationString_UserFriendly
					: GetLocationString(true);
			set => throw new NotSupportedException("WLV_LocationString_UserFriendly should not be set.");  // Exception message
		}

		ZString GetLocationString(bool includeDelimiterIfFixedWidth)
		{
			var locationFromDB = string.Empty;
			var row = Row;
			var warehouse = row?.Warehouse;
			if (warehouse != null)
			{
				locationFromDB = GetLocationStringFromDB(warehouse.WW_LocationComponentDelimiter, row, warehouse, includeDelimiterIfFixedWidth);
			}

			return locationFromDB;
		}

		ZString GetLocationStringFromDB(string delimiter, WhsRow row, WhsWarehouse warehouse, bool includeDelimiterIfFixedWidth)
		{
			var locationString = "";
			if (row != null && warehouse != null)
			{
				var result = new DynamicBusinessObjectCollection(Factory);
				var sql = string.Format(CultureInfo.InvariantCulture, @"
Select dbo.WhsLocationFormatter(@WR_Name, @WLV_Column, @WLV_Level, @WLV_Tray, @WR_Columns, @WR_Levels, @WR_Trays, @WW_LocationColumnsAlpha, @WW_LocationLevelsAlpha, @WW_LocationTraysAlpha, 
@WW_LocationComponentDelimiter, @WW_LocationsHaveLeadingZeros, @WW_LocationColumnsZeroBased, @WW_LocationLevelsZeroBased, @WW_LocationTraysZeroBased,
@WW_LocationColumnsFixedWidth, @WW_LocationLevelsFixedWidth, @WW_LocationTraysFixedWidth, @IncludeDelimiterIfFixedWidth) as LocationString");

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@WR_Name", row.WR_Name, WhsRowSchema.WR_Name);
				sqlParams.Add("@WLV_Column", WLV_Column, WhsLocationViewSchema.WLV_Column);
				sqlParams.Add("@WLV_Level", WLV_Level, WhsLocationViewSchema.WLV_Level);
				sqlParams.Add("@WLV_Tray", WLV_Tray, WhsLocationViewSchema.WLV_Tray);
				sqlParams.Add("@WR_Columns", row.WR_Columns, WhsRowSchema.WR_Columns);
				sqlParams.Add("@WR_Levels", row.WR_Levels, WhsRowSchema.WR_Levels);
				sqlParams.Add("@WR_Trays", row.WR_Trays, WhsRowSchema.WR_Trays);
				sqlParams.Add("@WW_LocationColumnsAlpha", warehouse.WW_LocationColumnsAlpha, WhsWarehouseSchema.WW_LocationColumnsAlpha);
				sqlParams.Add("@WW_LocationLevelsAlpha", warehouse.WW_LocationLevelsAlpha, WhsWarehouseSchema.WW_LocationLevelsAlpha);
				sqlParams.Add("@WW_LocationTraysAlpha", warehouse.WW_LocationTraysAlpha, WhsWarehouseSchema.WW_LocationTraysAlpha);
				sqlParams.Add("@WW_LocationComponentDelimiter", delimiter, WhsWarehouseSchema.WW_LocationComponentDelimiter);
				sqlParams.Add("@WW_LocationsHaveLeadingZeros", warehouse.WW_LocationsHaveLeadingZeros, WhsWarehouseSchema.WW_LocationsHaveLeadingZeros);
				sqlParams.Add("@WW_LocationColumnsZeroBased", warehouse.WW_LocationColumnsZeroBased, WhsWarehouseSchema.WW_LocationColumnsZeroBased);
				sqlParams.Add("@WW_LocationLevelsZeroBased", warehouse.WW_LocationLevelsZeroBased, WhsWarehouseSchema.WW_LocationLevelsZeroBased);
				sqlParams.Add("@WW_LocationTraysZeroBased", warehouse.WW_LocationTraysZeroBased, WhsWarehouseSchema.WW_LocationTraysZeroBased);
				sqlParams.Add("@WW_LocationColumnsFixedWidth", warehouse.WW_LocationColumnsFixedWidth, WhsWarehouseSchema.WW_LocationColumnsFixedWidth);
				sqlParams.Add("@WW_LocationLevelsFixedWidth", warehouse.WW_LocationLevelsFixedWidth, WhsWarehouseSchema.WW_LocationLevelsFixedWidth);
				sqlParams.Add("@WW_LocationTraysFixedWidth", warehouse.WW_LocationTraysFixedWidth, WhsWarehouseSchema.WW_LocationTraysFixedWidth);
				sqlParams.Add("@IncludeDelimiterIfFixedWidth", includeDelimiterIfFixedWidth, WhsWarehouseSchema.WW_LocationsHaveLeadingZeros);

				result.Load(sql, sqlParams);
				if (result.Count == 1)
				{
					var dynamicBizO = result[0];
					locationString = (ZString)dynamicBizO["LocationString"];
				}
			}

			return locationString;
		}

		#endregion

		#region WLV_FormattedColumn

		[BusinessObjectTestExclude] // We do not set WLV_FormattedColumn. The setter throws an exception.
		public override ZString WLV_FormattedColumn
		{
			get { return base.WLV_FormattedColumn; }
			set { throw new NotSupportedException("WLV_FormattedColumn should not be set."); } // Exception message
		}

		#endregion

		#region WLV_FormattedLevel

		[BusinessObjectTestExclude] // We do not set WLV_FormattedLevel. The setter throws an exception.
		public override ZString WLV_FormattedLevel
		{
			get { return base.WLV_FormattedLevel; }
			set { throw new NotSupportedException("WLV_FormattedLevel should not be set."); } // Exception message
		}

		#endregion

		#region WLV_FormattedTray

		[BusinessObjectTestExclude] // We do not set WLV_FormattedTray. The setter throws an exception.
		public override ZString WLV_FormattedTray
		{
			get { return base.WLV_FormattedTray; }
			set { throw new NotSupportedException("WLV_FormattedTray should not be set."); } // Exception message
		}

		#endregion

		#region RowPathSequence

		public ZShort RowPathSequence
		{
			get { return RowPathSequenceCore.GetValueOrDefault(); }
		}

		ZShort? RowPathSequenceCore
		{
			get
			{
				var row = Row;
				return row != null ? row.WR_PickPathSequence : null;
			}
		}

		#endregion

		#region RowLocationSequence

		public ZString RowLocationSequence
		{
			get
			{
				var rowPathSequence = RowPathSequenceCore;
				return rowPathSequence.HasValue ? ZString.Format("{0}-{1}", rowPathSequence, WLV_PickPathSequence) : ZString.Empty;
			}
		}

		#endregion

		#region WLV_MaxWeight

		public override ZDecimal WLV_MaxWeight
		{
			get { return base.WLV_MaxWeight; }
			set
			{
				base.WLV_MaxWeight = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_MaxWeightUnit();
					Validation.ValidateWLV_WLT_LocationType();
				}
			}
		}

		#endregion

		#region WLV_MaxCubic

		public override ZDecimal WLV_MaxCubic
		{
			get { return base.WLV_MaxCubic; }
			set
			{
				base.WLV_MaxCubic = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_MaxCubicUnit();
					Validation.ValidateWLV_WLT_LocationType();
				}
			}
		}

		#endregion

		#region WLV_MaxHeight

		public override ZDecimal WLV_MaxHeight
		{
			get { return base.WLV_MaxHeight; }
			set
			{
				base.WLV_MaxHeight = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_MaxDimensionUnit();
					Validation.ValidateWLV_MaxWidth();
					Validation.ValidateWLV_MaxDepth();
				}
			}
		}

		#endregion

		#region WLV_MaxDepth

		public override ZDecimal WLV_MaxDepth
		{
			get { return base.WLV_MaxDepth; }
			set
			{
				base.WLV_MaxDepth = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_MaxDimensionUnit();
					Validation.ValidateWLV_MaxHeight();
					Validation.ValidateWLV_MaxWidth();
				}
			}
		}

		#endregion

		#region WLV_MaxWidth

		public override ZDecimal WLV_MaxWidth
		{
			get { return base.WLV_MaxWidth; }
			set
			{
				base.WLV_MaxWidth = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_MaxDimensionUnit();
					Validation.ValidateWLV_MaxDepth();
					Validation.ValidateWLV_MaxHeight();
				}
			}
		}

		#endregion

		#region WLV_MaxWeightUnit

		[List(nameof(Lookups) + "." + nameof(WhsLocationViewLookups.WeightUnits))]
		public override ZString WLV_MaxWeightUnit
		{
			get => base.WLV_MaxWeightUnit;
			set => base.WLV_MaxWeightUnit = value;
		}

		#endregion

		#region WLV_MaxCubicUnit

		[List(nameof(Lookups) + "." + nameof(WhsLocationViewLookups.CubicUnits))]
		public override ZString WLV_MaxCubicUnit
		{
			get => base.WLV_MaxCubicUnit;
			set => base.WLV_MaxCubicUnit = value;
		}

		#endregion

		#region WLV_MaxDimensionUnit

		[List(nameof(Lookups) + "." + nameof(WhsLocationViewLookups.DimensionUnits))]
		public override ZString WLV_MaxDimensionUnit
		{
			get => base.WLV_MaxDimensionUnit;
			set => base.WLV_MaxDimensionUnit = value;
		}

		#endregion

		#region WLV_PalletFloorSpaces

		public override ZByte WLV_PalletFloorSpaces
		{
			get => base.WLV_PalletFloorSpaces;
			set
			{
				base.WLV_PalletFloorSpaces = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_PalletStackHeight();
				}
			}
		}

		#endregion

		#region WLV_PalletStackHeight

		public override ZByte WLV_PalletStackHeight
		{
			get => base.WLV_PalletStackHeight;
			set
			{
				base.WLV_PalletStackHeight = value;

				// Tested in WhsLocationValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_PalletFloorSpaces();
				}
			}
		}

		#endregion

		#region IsEmpty

		public bool IsEmpty
		{
			get
			{
				var query = new ZQuery(WhsInventoryViewSchema.WI_WL, PK);
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

				return Factory.LoadTop1<IWhsInventoryView>(query) == null; // LoadTop1 for row caching vs db count or IF EXISTS()
			}
		}

		#endregion

		#region LocationString

		public ZString ToLocationString()
		{
			return WLV_LocationString;
		}

		public ZString ToSpaceDelimitedLocationString()
		{
			var row = Row;
			return GetLocationStringFromDB(" ", row, row?.Warehouse, true);
		}

		#endregion

		#region FormattedColumn

		public ZString FormattedColumn
		{
			get { return WLV_FormattedColumn; }
		}

		public ZPropertyInfo FormattedColumnInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedColumn); }
		}

		#endregion

		#region FormattedLevel

		public ZString FormattedLevel
		{
			get { return WLV_FormattedLevel; }
		}

		public ZPropertyInfo FormattedLevelInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedLevel); }
		}

		#endregion

		#region FormattedTray

		public ZString FormattedTray
		{
			get { return WLV_FormattedTray; }
		}

		public ZPropertyInfo FormattedTrayInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedTray); }
		}

		#endregion

		#region FormattedCheckDigit

		[ResourceStringData("WhsLocationView|FormattedCheckDigit", Caption = "Check Digit", ShortCaption = "Check")]
		[MaxLength(2)]
		public ZString FormattedCheckDigit
		{
			get => WLV_CheckDigit == EmptyCheckDigit ? string.Empty : WLV_CheckDigit.ToString("00");
			set
			{
				WLV_CheckDigit = (ZByte)(byte.TryParse(value, out var checkDigit) ? checkDigit : EmptyCheckDigit);
			}
		}

		public ZWrappedPropertyInfo FormattedCheckDigitInfo => GetWrappedZPropertyInfo(Schema.WLV_CheckDigit, _ => WLV_CheckDigitInfo);

		#endregion

		#region OldBarcode

		/// <summary>
		/// Do not use, only retained incase a warehouse uses the old label format
		/// </summary>
		public ZString OldBarcode
		{
			get
			{
				return (Row != null && Row.Warehouse != null)
					? (NoResString)"#W" + Row.Warehouse.WW_WarehouseCode + WLV_LocationString // non-semantic text
					: "";
			}
		}

		#endregion

		#region IsApprovedKnownLocation

		public bool IsApprovedKnownLocation
		{
			get
			{
				bool result = false;
				switch (CountryCode)
				{
					case Enterprise.Core.Constants.CountryCodes.UnitedStates:
						if (Row != null)
						{
							if (Row.Warehouse != null)
							{
								if (Row.Warehouse.IsApprovedKnown)
								{
									result = (WLV_ApprovedKnownLocation == CodeLists.US.TSAStatus.Codes.Known);
								}
							}
						}
						break;
				}
				return result;
			}
		}

		#endregion

		#region CountryCode

		public ZString CountryCode
		{
			get { return Row != null ? Row.CountryCode : ZString.Empty; }
		}

		#endregion

		#region RowName

		public ZString RowName => Row?.WR_Name ?? ZString.Empty;

		public ZPropertyInfo RowNameInfo
		{
			get { return GetZPropertyInfo(nameof(RowName)); }
		}

		#endregion

		#region WLV_WW_Whs

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WLV_WW_Whs
		{
			get { return base.WLV_WW_Whs; }
			set { base.WLV_WW_Whs = value; }
		}

		#endregion

		#region WLV_WA_PickingArea

		[List("Lookups.PickingAreas")]
		[RelatedBusinessObject("PickingArea")]
		public override ZGuid WLV_WA_PickingArea
		{
			get { return base.WLV_WA_PickingArea; }
			set
			{
				if (base.WLV_WA_PickingArea != value)
				{
					var pickingArea = Factory.Load<WhsArea>(value);
					base.WLV_PickingAreaType = pickingArea?.WA_AreaType ?? ZString.Empty;
					base.WLV_WA_PickingArea = value;

					if (!IsValidationSuspended)
					{
						// Tested in WhsLocationViewValidationTestCase.cs
						Validation.ValidateWLV_WLT_LocationType();
						Validation.ValidateWLV_WA_PutawayArea();
					}
				}
			}
		}

		#endregion

		#region WLV_PickingAreaType

		[BusinessObjectTestExclude] // We do not set WLV_PickingAreaType. The setter throws an exception.
		public override ZString WLV_PickingAreaType
		{
			get => base.WLV_PickingAreaType;
			set => throw new NotSupportedException("WLV_PickingAreaType should not be set."); // Exception message
		}

		#endregion

		#region WLV_WA_PutawayArea

		[List("Lookups.PutawayAreas")]
		[RelatedBusinessObject("PutawayArea")]
		public override ZGuid WLV_WA_PutawayArea
		{
			get => base.WLV_WA_PutawayArea;
			set
			{
				if (base.WLV_WA_PutawayArea != value)
				{
					var putawayArea = Factory.Load<WhsArea>(value);
					base.WLV_PutawayAreaType = putawayArea?.WA_AreaType ?? ZString.Empty;
					base.WLV_WA_PutawayArea = value;

					if (!IsValidationSuspended)
					{
						// Tested in WhsLocationViewValidationTestCase.cs
						Validation.ValidateWLV_WA_PickingArea();
					}
				}
			}
		}

		#endregion

		#region WLV_PutawayAreaType

		[BusinessObjectTestExclude] // We do not set WLV_PutawayAreaType. The setter throws an exception.
		public override ZString WLV_PutawayAreaType
		{
			get => base.WLV_PutawayAreaType;
			set => throw new NotSupportedException("WLV_PutawayAreaType should not be set."); // Exception message
		}

		#endregion

		#region WLV_IsValidLocationForProductWarehousePutaway

		[BusinessObjectTestExclude] // We do not set WLV_IsValidLocationForProductWarehousePutaway. The setter throws an exception.
		public override ZBool WLV_IsValidLocationForProductWarehousePutaway
		{
			get => base.WLV_IsValidLocationForProductWarehousePutaway;
			set => throw new NotSupportedException("WLV_IsValidLocationForProductWarehousePutaway should not be set.");
		}

		#endregion

		#region WLV_LastConfigChangedUtc

		[SettingInvalidDateOnDateTimePropertyTestExclude]
		[BusinessObjectTestExclude] // We do not set WLV_LastConfigChangedUtc. The setter throws an exception.
		public override ZDateTime WLV_LastConfigChangedUtc
		{
			get => base.WLV_LastConfigChangedUtc;
			set => throw new NotSupportedException("WLV_LastConfigChangedUtc should not be set.");
		}

		#endregion

		#region WLV_WR

		[RelatedBusinessObject("Row")]
		public override ZGuid WLV_WR
		{
			get { return base.WLV_WR; }
			set
			{
				base.WLV_WR = value;

				var whs = Row?.Warehouse;
				if (whs != null)
				{
					WLV_WLT_LocationType = whs.WW_WLT_DefaultLocationType;
					WLV_WW_Whs = whs.PK;
				}
			}
		}

		#endregion

		#region WLV_WLT_LocationType

		[List("Lookups.LocationTypes")]
		[RelatedBusinessObject("LocationType")]
		public override ZGuid WLV_WLT_LocationType
		{
			get { return base.WLV_WLT_LocationType; }
			set
			{
				base.WLV_WLT_LocationType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_WA_PickingArea();
					Validation.ValidateWLV_MaxWeight();
					Validation.ValidateWLV_MaxCubic();
					Validation.ValidateWLV_MaxQuantity();
					Validation.ValidateWLV_CheckDigit();
				}
			}
		}

		#endregion

		#region LocationClass

		public bool IsDockDoorLocation => WLV_LocationClass == LocationClasses.Codes.DDL;

		public bool IsPackingStationLocation => WLV_LocationClass == LocationClasses.Codes.PST;

		public bool IsFixedLocation => WLV_LocationClass == LocationClasses.Codes.FIX;

		public bool IsDynamicPickFaceLocation => WLV_LocationClass == LocationClasses.Codes.DPF;

		public bool IsPackingConsolidationLocation => WLV_LocationClass == LocationClasses.Codes.CON;

		public bool IsEligibleForCheckDigits => !IsDockDoorLocation && !IsPackingConsolidationLocation && !IsPackingStationLocation;

		#endregion

		#region LocationStatuses

		public LocationStatus LocationStatuses
		{
			get { return new LocationStatus(); }
		}

		#endregion

		#region IsInBondedArea

		public bool IsInBondedArea => WLV_PutawayAreaType == AreaTypes.Codes.Bonded || WLV_PickingAreaType == AreaTypes.Codes.Bonded;

		#endregion

		#region IsInInwardProcessingArea

		public bool IsInInwardProcessingArea => WLV_PutawayAreaType == AreaTypes.Codes.InwardProcessing || WLV_PickingAreaType == AreaTypes.Codes.InwardProcessing;

		#endregion

		#region PickMethods

		public ICodeDescriptionPairListWithDefaultCode PickMethods
		{
			get { return WarehouseDataRegistry.Instance.PickMethod.Value; }
		}

		#endregion

		#region IsMaximumTouchCountUsed

		public bool IsMaximumTouchCountUsed
		{
			get { return WLV_MaximumPickCountBeforeAutomatedStocktake > 0; }
		}

		#endregion

		#region HasCapacityLimitation

		public bool HasCapacityLimitation
		{
			get { return WLV_MaxWeight != 0 || WLV_MaxCubic != 0 || WLV_MaxQuantity != 0; }
		}

		#endregion

		#region WLV_FinalisedPickCount

		public override ZInt WLV_FinalisedPickCount
		{
			get { return base.WLV_FinalisedPickCount; }
			set
			{
				base.WLV_FinalisedPickCount = value;
				WLV_LastInventoryChangeDate = ZDateTimeOffset.Now;
			}
		}

		#endregion

		#region WLV_MaxQuantity

		public override ZDecimal WLV_MaxQuantity
		{
			get { return base.WLV_MaxQuantity; }
			set
			{
				base.WLV_MaxQuantity = value;
				WLV_MaxQuantityUnit = value > 0 ? DefaultQuantityUnit : "";

				if (!IsValidationSuspended)
				{
					Validation.ValidateWLV_WLT_LocationType();
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		protected override WhsLocationViewLookups GetNewLookups()
		{
			switch (CountryCode)
			{
				case Enterprise.Core.Constants.CountryCodes.UnitedStates:
					return new US.WhsLocationLookups(this);

				default:
					return base.GetNewLookups();
			}
		}

		#endregion

		#region Validation

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && CheckPropertyIsEnabledForValidation(propertyInfo.Name);
		}

		bool CheckPropertyIsEnabledForValidation(string propertyName)
		{
			switch (propertyName)
			{
				case WhsLocationViewSchema.Constants.WLV_RowName:
					return false;
				case WhsLocationViewSchema.Constants.WLV_WW_Whs:
					return false;
				default:
					return true;
			}
		}

		protected override WhsLocationViewValidation GetNewValidation()
		{
			switch (CountryCode)
			{
				case Constants.CountryCodes.UnitedStates:
					return new US.WhsLocationValidation(this);

				default:
					return base.GetNewValidation();
			}
		}

		#region ValidationLocation

		public static void ValidateLocation(IBusiness parent, ZPropertyInfo locationStringInfo)
		{
			new LocationValidation(parent).ValidateLocation(locationStringInfo);
		}

		#endregion

		#region CanTransferToOrFromAreaType

		public static bool CanTransferToOrFromAreaType(string areaFromLocationType, string areaToCompareLocationType)
		{
			var result = false;

			if (areaToCompareLocationType == AreaTypes.Codes.DynamicPickFace)
			{
				result = areaFromLocationType != AreaTypes.Codes.Bonded && areaFromLocationType != AreaTypes.Codes.Excise;
			}
			else if (areaFromLocationType == AreaTypes.Codes.DynamicPickFace)
			{
				result = areaToCompareLocationType != AreaTypes.Codes.Bonded && areaToCompareLocationType != AreaTypes.Codes.Excise;
			}
			else
			{
				result = areaFromLocationType == areaToCompareLocationType ||
					(
						areaFromLocationType != AreaTypes.Codes.FreeStore &&
						areaToCompareLocationType != AreaTypes.Codes.FreeStore
					);
			}

			return result;
		}

		#endregion

		#endregion

		#region Update Current Touch Count

		public bool UpdateCurrentTouchCount()
		{
			bool result = false;

			if (IsMaximumTouchCountUsed)
			{
				WLV_FinalisedPickCount++;

				if (WLV_MaximumPickCountBeforeAutomatedStocktake <= WLV_FinalisedPickCount)
				{
					WLV_FinalisedPickCount = 0;
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region FindLocation

		public static WhsLocation FindLocation(BusinessObjectFactory factory, ZString locationString, ZGuid whsPK)
		{
			Argument.NotNull(factory, nameof(factory));
			WhsLocation location = null;
			if (!locationString.IsEmpty)
			{
				var filter = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whsPK);
				var locationStringQuery = new ZQuery(WhsLocationViewSchema.WLV_LocationString, SQLComparisonOperator.StartsWith, locationString);
				var userFriendlyLocationQuery = new ZQuery(WhsLocationViewSchema.WLV_LocationString_UserFriendly, SQLComparisonOperator.StartsWith, locationString);
				var locationStringAndUserFriendlyLocationQuery = new ZQuery();
				locationStringAndUserFriendlyLocationQuery.AddToFilter(locationStringQuery, JoinCondition.Or);
				locationStringAndUserFriendlyLocationQuery.AddToFilter(userFriendlyLocationQuery, JoinCondition.Or);

				filter.AddToFilter(locationStringAndUserFriendlyLocationQuery, JoinCondition.And);
				filter.OrderBy = string.Join(", ", WhsLocationViewSchema.Constants.WLV_RowName, WhsLocationViewSchema.Constants.WLV_Column, WhsLocationViewSchema.Constants.WLV_Level, WhsLocationViewSchema.Constants.WLV_Tray);
				location = factory.LoadTop1<WhsLocation>(filter);
			}
			return location;
		}

		public static ZGuid FindLocationPK(BusinessObjectFactory factory, ZString locationString, ZGuid whsPK)
		{
			return FindLocation(factory, locationString, whsPK)?.PK ?? ZGuid.Empty;
		}

		#endregion

		#region LastInventoryChangeDate

		public ZDateTime WLV_LastInventoryChangeDateForBinding => WLV_LastInventoryChangeDate.ToLocalZDateTime();

		public ZPropertyInfo WLV_LastInventoryChangeDateForBindingInfo => GetZPropertyInfo(nameof(WLV_LastInventoryChangeDateForBinding));

		public void OnStockOnHandChanged()
		{
			var now = ZDateTimeOffset.Now;
			if (WLV_LastInventoryChangeDate.IsEmpty || now.CompareTo(WLV_LastInventoryChangeDate) > 0)
			{
				WLV_LastInventoryChangeDate = now;
			}
		}

		public void RollbackChangeLastInventoryChangeDate()
		{
			if (IsInDatabase)
			{
				WLV_LastInventoryChangeDate = (ZDateTimeOffset)WLV_LastInventoryChangeDateInfo.OriginalValue;
			}
			else
			{
				WLV_LastInventoryChangeDate = ZDateTimeOffset.Empty;
			}
		}

		public void UpdateWLV_LastAllocatedOrChangedDateUtc()
		{
			var date = ZDateTime.UtcNow;
			UpdateWLV_LastAllocatedOrChangedDateUtc(date);
		}

		public void UpdateWLV_LastAllocatedOrChangedDateUtc(ZDateTime date)
		{
			if (date.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("LastAllocatedOrChangedDate must be a UTC DateTime.");
			}

			if (WLV_LastAllocatedOrChangedDateUtc.IsEmpty || date.CompareTo(WLV_LastAllocatedOrChangedDateUtc) > 0)
			{
				WLV_LastAllocatedOrChangedDateUtc = date;
			}

			if (!WLV_LastAllocatedOrChangedIDInfo.HasChanges)
			{
				WLV_LastAllocatedOrChangedID = ZGuid.NewZGuid();
			}
		}

		#endregion

		#region CycleCountLastPerformed

		public ZDateTime WLV_CycleCountLastPerformedForBinding => WLV_CycleCountLastPerformed.ToLocalZDateTime();

		public ZPropertyInfo WLV_CycleCountLastPerformedForBindingInfo => GetZPropertyInfo(nameof(WLV_CycleCountLastPerformedForBinding));

		#endregion

		#region UniqueIndexFailureHandlers

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers => base.UniqueIndexFailureHandlers
			.Append(new LocationStringUniqueIndexValidationHandler(WhsRow.LocationStringUniqueIndexFailureMessage));

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (kind.HasFlag(TestBusinessObjectKind.MinimumRequiredToSave))
			{
				if (Warehouse == null)
				{
					WLV_WW_Whs = Factory.NewWithValidTestData<WhsWarehouse>().PK;
				}
				if (Row == null)
				{
					WLV_WR = Factory.NewWithValidTestData<WhsRow>().PK;
				}
				if (PickingArea == null)
				{
					WLV_WA_PickingArea = Factory.NewWithValidTestData<WhsArea>().PK;
				}
			}

			if (WLV_WLT_LocationType.IsEmpty)
			{
				WLV_WLT_LocationType = Row.Warehouse.WW_WLT_DefaultLocationType;
			}

			if (LocationType != null)
			{
				base.WLV_LocationTypeCode = LocationType.WLT_Code;
			}

			var now = ZDateTime.UtcNow;
			WLV_PutawayPathSequence = 1;
			WLV_SystemCreateTimeUtc = now;
			WLV_SystemLastEditTimeUtc = now;
			WLV_SystemLastEditUser = "E";
			WLV_SystemCreateUser = "E";
			base.WLV_LastConfigChangedUtc = ZDateTime.UtcNow;

			base.FillWithValidTestDataCore(kind, propertyPath);
			Row.UpdateComponentsPreEdit_ForTest();
		}

#endif

		#endregion
	}
}
