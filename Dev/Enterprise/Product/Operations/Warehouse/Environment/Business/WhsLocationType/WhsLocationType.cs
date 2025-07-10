using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public sealed class WhsLocationType : AutoWhsLocationType, IWhsLocationType, IAffectLocationView
	{
		public WhsLocationType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public const string SystemWideDefaultLocationTypeCode = "RNO";

		#region Properties

		#region WLT_LocationClass

		[List(nameof(Lookups) + "." + nameof(LocationClasses))]
		public override ZString WLT_LocationClass
		{
			get { return base.WLT_LocationClass; }
			set
			{
				base.WLT_LocationClass = value;

				SetDefaultsForLocationClass();
			}
		}

		void SetDefaultsForLocationClass()
		{
			if (!IsFixedLocationClass)
			{
				WLT_MaximumNumberOfProducts = 0;
				WLT_RetainPalletIDsInFixedPickFaces = false;
			}

			SetDefaultTemperatureFields();

			WLT_DefaultCycleCountGranularity = IsCycleCountingSupportedForLocationClass(WLT_LocationClass)
				? CycleCountGranularities.Codes.PWA
				: string.Empty;

			if (IsDockDoorLocationClass)
			{
				WLT_IsPalletIDNeutral = false;
			}
		}

		void SetDefaultTemperatureFields()
		{
			if (IsTemperatureControlledLocationClass)
			{
				if (WLT_TemperatureUnit.IsEmpty)
				{
					WLT_TemperatureUnit = Enterprise.Core.Constants.Temperature.Centigrade;
				}
			}
			else
			{
				WLT_MinimumTemperature = 0;
				WLT_MaximumTemperature = 0;
				WLT_TemperatureUnit = string.Empty;
			}
		}

		#endregion

		#region WLT_DefaultCycleCountGranularity

		[ReadOnlyMember(nameof(DefaultCycleCountGranularityReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CycleCountGranularities))]
		public override ZString WLT_DefaultCycleCountGranularity
		{
			get { return base.WLT_DefaultCycleCountGranularity; }
			set
			{
				base.WLT_DefaultCycleCountGranularity = value;
			}
		}

		bool DefaultCycleCountGranularityReadOnly
			=> !IsCycleCountingSupportedForLocationClass(WLT_LocationClass);

		#endregion

		#region WLT_MaximumNumberOfProducts

		[ReadOnlyMember(nameof(IsNotFIXTypeReadOnly))]
		public override ZInt WLT_MaximumNumberOfProducts
		{
			get { return base.WLT_MaximumNumberOfProducts; }
			set { base.WLT_MaximumNumberOfProducts = value; }
		}

		#endregion

		#region WLT_IsPalletIDNeutral

		[ReadOnlyMember(nameof(IsDDLReadOnly))]
		public override ZBool WLT_IsPalletIDNeutral
		{
			get { return base.WLT_IsPalletIDNeutral; }
			set
			{
				base.WLT_IsPalletIDNeutral = value;
			}
		}

		[ReadOnlyMember(nameof(IsNotFIXTypeReadOnly))]
		public override ZBool WLT_RetainPalletIDsInFixedPickFaces
		{
			get { return base.WLT_RetainPalletIDsInFixedPickFaces; }
			set
			{
				base.WLT_RetainPalletIDsInFixedPickFaces = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWLT_IsPalletIDNeutral();
				}
			}
		}

		#endregion

		#region WLT_MinimumTemperature

		[ReadOnlyMember(nameof(IsNotTCLTypeReadOnly))]
		public override ZDecimal WLT_MinimumTemperature
		{
			get { return base.WLT_MinimumTemperature; }
			set
			{
				base.WLT_MinimumTemperature = value;

				// Tested in WhsLocationTypeValidationTestCase
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLT_MaximumTemperature();
				}
			}
		}

		#endregion

		#region WLT_MaximumTemperature

		[ReadOnlyMember(nameof(IsNotTCLTypeReadOnly))]
		public override ZDecimal WLT_MaximumTemperature
		{
			get { return base.WLT_MaximumTemperature; }
			set
			{
				base.WLT_MaximumTemperature = value;

				// Tested in WhsLocationTypeValidationTestCase
				if (!IsValidationSuspended)
				{
					Validation.ValidateWLT_MinimumTemperature();
				}
			}
		}

		#endregion

		#region WLT_TemperatureUnit

		[List(nameof(Lookups) + "." + nameof(WhsLocationTypeLookups.TemperatureUnits))]
		[ReadOnlyMember(nameof(IsNotTCLTypeReadOnly))]
		public override ZString WLT_TemperatureUnit
		{
			get { return base.WLT_TemperatureUnit; }
			set { base.WLT_TemperatureUnit = value; }
		}

		#endregion

		#region WLT_IsSystem

		[ReadOnly(true)]
		public override ZBool WLT_IsSystem
		{
			get { return base.WLT_IsSystem; }
			set { base.WLT_IsSystem = value; }
		}

		#endregion

		#region Flags

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		bool IsDockDoorLocationClass => WLT_LocationClass == LocationClasses.Codes.DDL;

		bool IsPackingStationLocationClass => WLT_LocationClass == LocationClasses.Codes.PST;

		bool IsFixedLocationClass => WLT_LocationClass == LocationClasses.Codes.FIX;

		bool IsTemperatureControlledLocationClass => WLT_LocationClass == LocationClasses.Codes.TCL;

		bool IsHighPriorityLocationClass => WLT_LocationClass == LocationClasses.Codes.HPL;

		bool IsDDLReadOnly => IsDockDoorLocationClass;

		bool IsNotFIXTypeReadOnly => !IsFixedLocationClass;

		bool IsNotTCLTypeReadOnly => !IsTemperatureControlledLocationClass;

		bool IsPackingConsolidationLocationClass => WLT_LocationClass == LocationClasses.Codes.CON;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WLT_Code.IsEmpty ?
					Res.GetString("576150C5-93CB-4C44-8F51-48824298532C", "Location Type") :
					Res.GetString("F7116075-B5E8-41B4-9CA8-FFFD895EED33", "Location Type {0}", WLT_Code);
			}
		}

		#endregion

		#endregion

		#region IsCycleCountingSupportedForLocationClass

		public static bool IsCycleCountingSupportedForLocationClass(string warehouseLocationClass)
			=> !unsupportedCycleCountLocations.Contains(warehouseLocationClass);

		static readonly ImmutableArray<string> unsupportedCycleCountLocations = ImmutableArray.Create(LocationClasses.Codes.TCL, LocationClasses.Codes.DDL, LocationClasses.Codes.PST, LocationClasses.Codes.CON);

		#endregion

		#region IsLocationTypeSupportedByThisWarehouseType

		public bool IsLocationTypeSupportedByThisWarehouseType(string warehouseType)
		{
			return
				(!IsTemperatureControlledLocationClass || warehouseType == WarehouseTypes.Codes.Transit || warehouseType == WarehouseTypes.Codes.ContainerYard)
				&& ((!IsPackingStationLocationClass && !IsPackingConsolidationLocationClass) || warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
				&& (!(IsDockDoorLocationClass || IsFixedLocationClass || IsHighPriorityLocationClass) || warehouseType != WarehouseTypes.Codes.ContainerYard);
		}

		#endregion

		#region Delete

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var reason = base.ReasonForNotAbleToDelete;
				if (reason.IsEmpty && !CanDelete)
				{
					reason = (WLT_IsSystem)
						? ResString.GetMultilingualString("7F952F65-1952-4EB8-83BF-765C3FBFE501", "System location types cannot be deleted.")
						: ResString.GetMultilingualString("1FFC2D71-B4ED-4EC1-A4D5-F25EEBB76938", "Location types which are in use cannot be deleted.");
				}

				return reason;
			}
		}

		public override bool CanDelete
		{
			get
			{
				var queryLocations = new ZQuery(WhsLocationViewSchema.WLV_WLT_LocationType, PK);
				var queryWarehouses = new ZQuery(WhsWarehouseSchema.WW_WLT_DefaultLocationType, PK);
				return base.CanDelete && !WLT_IsSystem
					&& Factory.LoadTop1<WhsLocation>(queryLocations) == null
					&& Factory.LoadTop1<WhsWarehouse>(queryWarehouses) == null;
			}
		}

		#endregion

		#region OutboundLocationClasses

		public static ImmutableArray<string> OutboundLocationClasses { get; } = ImmutableArray.Create(LocationClasses.Codes.DDL, LocationClasses.Codes.PST, LocationClasses.Codes.CON);

		#endregion

		#region IAffectLocationView

		SchemaColumn[] IAffectLocationView.GetColumnsThatAffectLocationView() => new SchemaColumn[]
		{
			WhsLocationTypeSchema.WLT_LocationClass,
			WhsLocationTypeSchema.WLT_Code,
		};

		void IAffectLocationView.ReloadLocationsFromDB()
		{
			var query = new ZQuery(WhsLocationViewSchema.WLV_WLT_LocationType, PK);
			query.ReLoadExistingRows = true;
			Factory.Load<WhsLocation>(query);
		}

		ZGuid IAffectLocationView.ParentThatMayReloadMyLocations => ZGuid.Empty;

		#endregion
	}
}
