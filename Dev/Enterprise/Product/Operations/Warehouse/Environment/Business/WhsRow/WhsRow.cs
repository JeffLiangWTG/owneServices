using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Environment.Business
{
	[ActionFieldFollow(false)]
	[DependentBusinessObject(typeof(WhsWarehouse), "Rows")]
	[CodeProperty(AutoWhsRow.Schema.WR_Name), DescriptionProperty(WhsRow.Schema.WR_Name)]
	public sealed partial class WhsRow : AutoWhsRow, IWhsRow, WhsRowInternals, IDocManagerSupport, IDocumentSupportable, IAffectLocationView
	{
		#region Constructors

		public WhsRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		const int SortPathMethodMaxLength = 3;

		#region Business Object Overrides

		public override void Delete()
		{
			using (((IBusinessObjectCollection)Locations).SuspendListChanged())
			{
				Locations.DeleteAll();
				base.Delete();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WR_Columns = 1;
			WR_Levels = 1;
			WR_Trays = 1;

			fColsPreEdit = 0;
			fLevelsPreEdit = 0;
			fTraysPreEdit = 0;
			fApprovedKnownStatus = CodeLists.ApprovedKnownStatus.Codes.NO;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			StorePreeditLocationSetting();
		}

		// Only Call this if manually set these values
		public void StorePreeditLocationSetting()
		{
			fColsPreEdit = WR_Columns;
			fLevelsPreEdit = WR_Levels;
			fTraysPreEdit = WR_Trays;
		}

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableAction(
				() => locationValidationCache = new LocationValidationCache(Factory, this, Locations),
				() => locationValidationCache = null);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			((WhsRowInternals)this).GenerateLocations();
			AutoAssignPathSequenceNumbers();
			LocationViewReloaderService.AddLocationReloaderService(Factory);
		}

		void AutoAssignPathSequenceNumbers()
		{
			if (Locations.Count > 0)
			{
				AutoAssignPickPathSequence();
				AutoAssignPutawayPathSequence();
				AutoAssignCycleCountPathSequence();
			}
		}

		void AutoAssignPickPathSequence()
		{
			AutoAssignPathSequenceCore(nameof(WhsLocation.WLV_PickPathSequence));
		}

		void AutoAssignPutawayPathSequence()
		{
			AutoAssignPathSequenceCore(nameof(WhsLocation.WLV_PutawayPathSequence));
		}

		void AutoAssignCycleCountPathSequence()
		{
			AutoAssignPathSequenceCore(nameof(WhsLocation.WLV_CycleCountPathSequence));
		}

		void AutoAssignPathSequenceCore(string pathSequencePropertyName)
		{
			var defaultValue = pathSequencePropertyName == nameof(WhsLocation.WLV_PutawayPathSequence) ? (ZInt)1 : ZInt.Zero;
			var maxSequenceNumber = GetValidPathSequence(Locations.Max(location => (ZInt)location.FindPropertyInfo(pathSequencePropertyName).Value) + 1, defaultValue);
			foreach (var location in Locations.Where(location => (ZInt)location.FindPropertyInfo(pathSequencePropertyName).Value == 0))
			{
				location.FindPropertyInfo(pathSequencePropertyName).Value = maxSequenceNumber;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool IsLookupsCachedInBase => false;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsRowFetchStrategy(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WR_Name.IsEmpty ?
					Res.GetString("90375798-57E3-421D-8BFA-8F822EA01F47", "Row") :
					Res.GetString("96FDB8B6-8862-4880-8AC7-0B880B703F25", "Row {0}", WR_Name);
			}
		}

		#endregion

		#region Related Entities

		#region Warehouse

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WR_WW_Whs);

		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WR_WW_Whs
		{
			get => base.WR_WW_Whs;
			set
			{
				var valueChanged = (base.WR_WW_Whs != value);
				base.WR_WW_Whs = value;

				if (valueChanged)
				{
					Locations.MarkAsNeedingValidation();
					SetRowPathSequenceToNextUnusedNumberInWarehouse();
				}
			}
		}

		void SetRowPathSequenceToNextUnusedNumberInWarehouse()
		{
			if (WR_WW_Whs.IsValid)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsRowSchema.PK, SQLComparisonOperator.NotEqual, PK);
				query.AddToFilter(WhsRowSchema.WR_WW_Whs, WR_WW_Whs);
				query.OrderBy = WhsRowSchema.WR_PickPathSequence.Name + " desc";

				var rowWithHighestPickSequence = Factory.LoadTop1<WhsRow>(query);
				WR_PickPathSequence = rowWithHighestPickSequence != null ? rowWithHighestPickSequence.WR_PickPathSequence + 1 : new ZShort(1);
			}
		}

		#endregion

		#region Locations

		[ChildEditable(true)]
		public WhsLocationCollection Locations
		{
			get
			{
				if (locations == null)
				{
					locations = new WhsLocationCollection(this);
					locations.ApplySort(new WhsLocationCollection.SortByColLevelTray());
					RegisterEditableChildObject(locations);
				}
				return locations;
			}
		}

		#endregion

		#endregion

		#region Methods

		internal bool LocationHasDockDoorPick(WhsLocation location) => GetCache(location).HasDockDoorPick(location);

		internal bool LocationHasPackingStationPick(WhsLocation location) => GetCache(location).HasPackingStationPick(location);

		internal bool LocationHasUnfinalisedPick(WhsLocation location) => GetCache(location).HasUnfinalisedPick(location);

		internal bool LocationHasPickFace(WhsLocation location) => GetCache(location).HasPickFace(location);

		internal bool LocationHasNonPalletStock(WhsLocation location) => GetCache(location).HasNonPalletStock(location);

		internal bool LocationHasNonUniqueCheckDigit(WhsLocation location) => GetCache(location).HasNonUniqueCheckDigit(location);

		internal int GetLocationPalletCount(WhsLocation location) => GetCache(location).GetPalletCount(location);

		internal int GetLocationProductCount(WhsLocation location) => GetCache(location).GetProductCount(location);

		LocationValidationCache GetCache(WhsLocation location) => locationValidationCache ?? new LocationValidationCache(Factory, this, new[] { location });

		LocationValidationCache locationValidationCache;

		public bool IsFirstPackingStationLocationInThisWarehouse()
		{
			var result = (Locations.Any(location =>
				location.WLV_LocationClassInfo.HasChanges &&
				location.WLV_LocationClass == LocationClasses.Codes.PST)
			);

			if (result)
			{
				var query = new ZQuery(WhsLocationViewSchema.WLV_LocationClass, LocationClasses.Codes.PST);
				query.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, WR_WW_Whs);
				result = !Factory.ExistsInDatabase(WhsLocationViewSchema.Constants.TableName, query);
			}

			return result;
		}

		#region UpdateLocationValues

		public void UpdateLocationValues(WhsLocation[] locations)
		{
			try
			{
				locationValidationCache = new LocationValidationCache(Factory, this, locations);

				using (locationValidationCache.DeferCheckDigitCacheCreation())
				{
					var rowNameHash = ((string)WR_Name).Aggregate(7, (hash, c) => unchecked(31 * hash + c)); // GetHashCode() could vary between application runs

					foreach (var location in locations)
					{
						UpdateLocationValues(location, rowNameHash);
					}
				}

				if (GenerateCheckDigit)
				{
					foreach (var location in locations)
					{
						location.Validation.ValidateWLV_CheckDigit();
					}
				}
			}
			finally
			{
				locationValidationCache = null;
			}
		}

		void UpdateLocationValues(WhsLocation location, int rowNameHash)
		{
			if (location != null)
			{
				if (this.GenerateCheckDigit && location.IsEligibleForCheckDigits)
				{
					location.WLV_CheckDigit = GenerateCheckDigitForLocation(location, rowNameHash);
				}

				if (this.MaxQuantity != 0)
				{
					location.WLV_MaxQuantity = this.MaxQuantity;
				}

				if (this.MaxWeight != 0)
				{
					location.WLV_MaxWeight = this.MaxWeight;
				}

				if (!this.MaxWeightUnit.IsEmpty)
				{
					location.WLV_MaxWeightUnit = this.MaxWeightUnit;
				}

				if (this.MaxCubic != 0)
				{
					location.WLV_MaxCubic = this.MaxCubic;
				}

				if (!this.MaxCubicUnit.IsEmpty)
				{
					location.WLV_MaxCubicUnit = this.MaxCubicUnit;
				}

				if (this.MaxHeight != 0)
				{
					location.WLV_MaxHeight = this.MaxHeight;
				}

				if (this.MaxDepth != 0)
				{
					location.WLV_MaxDepth = this.MaxDepth;
				}

				if (this.MaxWidth != 0)
				{
					location.WLV_MaxWidth = this.MaxWidth;
				}

				if (!this.MaxDimensionUnit.IsEmpty)
				{
					location.WLV_MaxDimensionUnit = this.MaxDimensionUnit;
				}

				if (this.PalletFloorSpaces != 0)
				{
					location.WLV_PalletFloorSpaces = this.PalletFloorSpaces;
				}

				if (this.PalletStackHeight != 0)
				{
					location.WLV_PalletStackHeight = this.PalletStackHeight;
				}

				if (!this.LocationType.IsEmpty)
				{
					location.WLV_WLT_LocationType = this.LocationType;
				}

				if (!this.LocationStatus.IsEmpty)
				{
					location.WLV_LocationStatus = this.LocationStatus;
				}

				if (!this.PickMethod.IsEmpty)
				{
					location.WLV_PickMethod = this.PickMethod;
				}

				if (!this.PickingArea_MassUpdate.IsEmpty)
				{
					location.WLV_WA_PickingArea = this.PickingArea_MassUpdate;
				}

				if (!this.PutawayArea_MassUpdate.IsEmpty)
				{
					location.WLV_WA_PutawayArea = this.PutawayArea_MassUpdate;
				}

				if (!this.ApprovedKnownStatus.IsEmpty)
				{
					location.WLV_ApprovedKnownLocation = this.ApprovedKnownStatus;
				}

				if (this.MaximumTouchCount != 0)
				{
					location.WLV_MaximumPickCountBeforeAutomatedStocktake = this.MaximumTouchCount;
				}
			}
		}

		ZByte GenerateCheckDigitForLocation(WhsLocation location, int rowNameHash)
		{
			// Algorithm based on Microlistics' use of summed primes for check digits.
			// To minimize the predictability of the resulting check digits, we alternate between sets of coefficients.
			var coefficientSets = new[]
			{
				new[] { 5, 7, 11 },
				new[] { 13, 17, 19 },
			};

			var coefficientSetToUse = coefficientSets[(location.WLV_Column + location.WLV_Level + location.WLV_Tray) % 2];
			var checkDigit = unchecked(
				(rowNameHash * 3) +
				(location.WLV_Column * coefficientSetToUse[0]) +
				(location.WLV_Level * coefficientSetToUse[1]) +
				(location.WLV_Tray * coefficientSetToUse[2])
			) % 100;

			if (checkDigit < 0)
			{
				checkDigit += 100;
			}

			return (ZByte)checkDigit;
		}

		#endregion

		#region UpdatePathSequenceOnLocations

		public void UpdatePathSequenceOnLocations()
		{
			if (SortPickPathMethodInfo.HasErrors() || SortPutawayPathMethodInfo.HasErrors() || SortCycleCountMethodInfo.HasErrors())
			{
				NotificationSubscriber.AddError(Res.GetString("E13E8B74-3159-4BED-989B-F843137FFDCC", "Invalid sort path method specified."));
			}
			else
			{
				UpdatePathSequenceOnLocationsCore(WhsLocationViewSchema.Constants.WLV_PickPathSequence, SortPickPathMethod);
				UpdatePathSequenceOnLocationsCore(WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, SortPutawayPathMethod);
				UpdatePathSequenceOnLocationsCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, SortCycleCountMethod);
			}
		}

		void UpdatePathSequenceOnLocationsCore(ZString pathPropertyName, ZString sortPathMethodForBinding)
		{
			var sequenceColumnNames = new[] { WhsLocationViewSchema.Constants.WLV_PickPathSequence, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence };
			if (sequenceColumnNames.All(s => !s.Equals(pathPropertyName)))
			{
				throw new ArgumentException(string.Format(Culture.Invariant, "The Path Property {0} is not valid, please pass in {1}, {2} or {3}.", pathPropertyName, WhsLocationViewSchema.Constants.WLV_PickPathSequence, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence));
			}

			switch (sortPathMethodForBinding)
			{
				case SortPathMethods.Codes.ColumnThenLevel:
					UpdatePickSequenceWithColumnThenLevel(pathPropertyName);
					break;
				case SortPathMethods.Codes.LevelThenColumn:
					UpdatePickSequenceWithLevelThenColumn(pathPropertyName);
					break;
				case SortPathMethods.Codes.UserDefined:
				default:
					break;
			}
		}

		void UpdatePickSequenceWithColumnThenLevel(ZString pathPropertyName)
		{
			foreach (var location in Locations.ToArray())
			{
				var proposedPickSequence = ((location.WLV_Column - 1) * WR_Levels * WR_Trays) + ((location.WLV_Level - 1) * WR_Trays) + location.WLV_Tray;
				location.FindPropertyInfo(pathPropertyName).Value = GetValidPathSequence(proposedPickSequence, ZInt.Zero);
			}
		}

		void UpdatePickSequenceWithLevelThenColumn(ZString pathPropertyName)
		{
			foreach (var location in Locations.ToArray())
			{
				var proposedPickSequence = ((location.WLV_Level - 1) * WR_Columns * WR_Trays) + ((location.WLV_Column - 1) * WR_Trays) + location.WLV_Tray;
				location.FindPropertyInfo(pathPropertyName).Value = GetValidPathSequence(proposedPickSequence, ZInt.Zero);
			}
		}

		ZInt GetValidPathSequence(ZInt proposedPathSequence, ZInt defaultValue)
		{
			return proposedPathSequence > short.MaxValue ? defaultValue : proposedPathSequence;
		}

		#endregion

		#endregion

		#region Properties

		#region SortPathMethodsForBinding

		#region SortPickPathMethod

		[MaxLength(SortPathMethodMaxLength)]
		[List("Lookups.SortPathMethods")]
		public ZString SortPickPathMethod
		{
			get => sortPickPathMethod;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (sortPickPathMethod != value)
					{
						CheckMaximumLength(SortPickPathMethodInfo, value);
						SetNonPersistentPropertyValue(SortPickPathMethodInfo, ref sortPickPathMethod, value);
						if (!IsValidationSuspended)
						{
							Validation.ValidateSortPickPathMethod();
						}
					}
				}
			}
		}

		ZString sortPickPathMethod;

		public ZPropertyInfo SortPickPathMethodInfo => GetZPropertyInfo(nameof(SortPickPathMethod));

		#endregion

		#region SortPutawayPathMethod

		[MaxLength(SortPathMethodMaxLength)]
		[List("Lookups.SortPathMethods")]
		public ZString SortPutawayPathMethod
		{
			get => sortPutawayPathMethod;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (sortPutawayPathMethod != value)
					{
						CheckMaximumLength(SortPutawayPathMethodInfo, value);
						SetNonPersistentPropertyValue(SortPutawayPathMethodInfo, ref sortPutawayPathMethod, value);
						if (!IsValidationSuspended)
						{
							Validation.ValidateSortPutawayPathMethod();
						}
					}
				}
			}
		}

		ZString sortPutawayPathMethod;

		public ZPropertyInfo SortPutawayPathMethodInfo => GetZPropertyInfo(nameof(SortPutawayPathMethod));

		#endregion

		#region SortCycleCountMethod

		[MaxLength(SortPathMethodMaxLength)]
		[List("Lookups.SortPathMethods")]
		public ZString SortCycleCountMethod
		{
			get => sortCycleCountMethod;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (sortCycleCountMethod != value)
					{
						CheckMaximumLength(SortCycleCountMethodInfo, value);
						SetNonPersistentPropertyValue(SortCycleCountMethodInfo, ref sortCycleCountMethod, value);
						if (!IsValidationSuspended)
						{
							Validation.ValidateSortCycleCountMethod();
						}
					}
				}
			}
		}

		ZString sortCycleCountMethod;

		public ZPropertyInfo SortCycleCountMethodInfo => GetZPropertyInfo(nameof(SortCycleCountMethod));

		#endregion

		#endregion

		#region SelectedLocations

		public List<WhsLocation> SelectedLocations => (ParentForm != null ? ParentForm.SelectedLocations : new List<WhsLocation>());

		#endregion

		#region ParentForm

		public IRowForm ParentForm
		{
			get => parentForm;
			set => parentForm = value;
		}

		IRowForm parentForm;

		#endregion

		#region ApprovedKnownStatus

		[MaxLength(3)]
		[ReadOnlyMember(nameof(ApprovedKnownStatusReadOnly))]
		public ZString ApprovedKnownStatus
		{
			get => fApprovedKnownStatus;
			set
			{
				if (fApprovedKnownStatus != value)
				{
					CheckMaximumLength(ApprovedKnownStatusInfo, value);
					SetNonPersistentPropertyValue(ApprovedKnownStatusInfo, ref fApprovedKnownStatus, value);
				}
			}
		}

		public ZPropertyInfo ApprovedKnownStatusInfo => GetZPropertyInfo(nameof(ApprovedKnownStatus));

		bool ApprovedKnownStatusReadOnly
		{
			get
			{
				return CountryCode != Enterprise.Core.Constants.CountryCodes.UnitedStates
					|| Warehouse == null
					|| !Warehouse.IsApprovedKnown;
			}
		}

		#endregion

		#region CountryCode

		public ZString CountryCode => Warehouse != null ? Warehouse.CountryCode : ZString.Empty;

		#endregion

		#region Columns/Levels/Trays

		public override ZShort WR_Columns
		{
			get => base.WR_Columns;
			set
			{
				base.WR_Columns = value;
				Locations.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					Validation.ValidateWR_Levels();
					Validation.ValidateWR_Trays();
				}
			}
		}

		public override ZShort WR_Levels
		{
			get => base.WR_Levels;
			set
			{
				base.WR_Levels = value;
				Locations.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					Validation.ValidateWR_Columns();
					Validation.ValidateWR_Trays();
				}
			}
		}

		public override ZShort WR_Trays
		{
			get => base.WR_Trays;
			set
			{
				base.WR_Trays = value;
				Locations.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					Validation.ValidateWR_Columns();
					Validation.ValidateWR_Levels();
				}
			}
		}

		#endregion

		#region CheckDigit

		[ResourceStringData("WhsRow|GenerateCheckDigit", Caption = "Generate Check Digit")]
		public ZBool GenerateCheckDigit
		{
			get => fGenerateCheckDigit;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(GenerateCheckDigitInfo, ref fGenerateCheckDigit, value);
				}
			}
		}

		public ZPropertyInfo GenerateCheckDigitInfo => GetZPropertyInfo(nameof(GenerateCheckDigit));

		#endregion

		#region MaxQuantity

		[ResourceStringData("WhsRow|MaxQuantity", Caption = "Max Quantity")]
		public ZDecimal MaxQuantity
		{
			get => fMaxQuantity;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaxQuantityInfo, ref fMaxQuantity, value);
				}
			}
		}

		public ZPropertyInfo MaxQuantityInfo => GetZPropertyInfo(nameof(MaxQuantity));

		#endregion

		#region MaxWeight

		[ResourceStringData("WhsRow|MaxWeight", Caption = "Max Weight")]
		public ZDecimal MaxWeight
		{
			get => fMaxWeight;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaxWeightInfo, ref fMaxWeight, value);
				}
			}
		}

		public ZPropertyInfo MaxWeightInfo => GetZPropertyInfo(nameof(MaxWeight));

		#endregion

		#region MaxWeightUnit

		[List(nameof(Lookups) + "." + nameof(WhsRowLookups.WeightUnitTypes))]
		[MaxLength(2)]
		public ZString MaxWeightUnit
		{
			get => fMaxWeightUnit;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (fMaxWeightUnit != value)
					{
						CheckMaximumLength(MaxWeightUnitInfo, value);
						SetNonPersistentPropertyValue(MaxWeightUnitInfo, ref fMaxWeightUnit, value);
					}
				}
			}
		}

		public ZPropertyInfo MaxWeightUnitInfo => GetZPropertyInfo(nameof(MaxWeightUnit));

		#endregion

		#region MaxCubic

		[ResourceStringData("WhsRow|MaxCubic", Caption = "Max Cubic")]
		public ZDecimal MaxCubic
		{
			get => fMaxCubic;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaxCubicInfo, ref fMaxCubic, value);
				}
			}
		}

		public ZPropertyInfo MaxCubicInfo => GetZPropertyInfo(nameof(MaxCubic));

		#endregion

		#region MaxCubicUnit

		[List(nameof(Lookups) + "." + nameof(WhsRowLookups.CubicUnitTypes))]
		[MaxLength(2)]
		public ZString MaxCubicUnit
		{
			get => fMaxCubicUnit;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (fMaxCubicUnit != value)
					{
						CheckMaximumLength(MaxCubicUnitInfo, value);
						SetNonPersistentPropertyValue(MaxCubicUnitInfo, ref fMaxCubicUnit, value);
					}
				}
			}
		}

		public ZPropertyInfo MaxCubicUnitInfo => GetZPropertyInfo(nameof(MaxCubicUnit));

		#endregion

		#region MaxHeight

		[ResourceStringData("WhsRow|MaxHeight", Caption = "Max Height")]
		public ZDecimal MaxHeight
		{
			get => maxHeight;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaxHeightInfo, ref maxHeight, value);
				}
			}
		}

		ZDecimal maxHeight;

		public ZPropertyInfo MaxHeightInfo => GetZPropertyInfo(nameof(MaxHeight));

		#endregion

		#region MaxDepth

		[ResourceStringData("WhsRow|MaxDepth", Caption = "Max Depth")]
		public ZDecimal MaxDepth
		{
			get => maxDepth;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaxDepthInfo, ref maxDepth, value);
				}
			}
		}

		ZDecimal maxDepth;

		public ZPropertyInfo MaxDepthInfo => GetZPropertyInfo(nameof(MaxDepth));

		#endregion

		#region MaxWidth

		[ResourceStringData("WhsRow|MaxWidth", Caption = "Max Width")]
		public ZDecimal MaxWidth
		{
			get => maxWidth;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaxWidthInfo, ref maxWidth, value);
				}
			}
		}

		ZDecimal maxWidth;

		public ZPropertyInfo MaxWidthInfo => GetZPropertyInfo(nameof(MaxWidth));

		#endregion

		#region MaxDimensionUnit

		[ResourceStringData("WhsRow|MaxDimensionUnit", Caption = "Max Dimension Unit")]
		[List(nameof(Lookups) + "." + nameof(WhsRowLookups.DimensionUnitTypes))]
		[MaxLength(2)]
		public ZString MaxDimensionUnit
		{
			get => maxDimensionUnit;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (maxDimensionUnit != value)
					{
						CheckMaximumLength(MaxDimensionUnitInfo, value);
						SetNonPersistentPropertyValue(MaxDimensionUnitInfo, ref maxDimensionUnit, value);
					}
				}
			}
		}

		ZString maxDimensionUnit;

		public ZPropertyInfo MaxDimensionUnitInfo => GetZPropertyInfo(nameof(MaxDimensionUnit));

		#endregion

		#region PalletFloorSpaces

		[ResourceStringData("WhsRow|PalletFloorSpaces", Caption = "Pallet Floor Spaces", MediumCaption = "Plt. Floor Spaces", ShortCaption = "Plt. Spaces")]
		public ZByte PalletFloorSpaces
		{
			get => palletFloorSpaces;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(PalletFloorSpacesInfo, ref palletFloorSpaces, value);
				}
			}
		}

		ZByte palletFloorSpaces;

		public ZPropertyInfo PalletFloorSpacesInfo => GetZPropertyInfo(nameof(PalletFloorSpaces));

		#endregion

		#region PalletStackHeight

		[ResourceStringData("WhsRow|PalletStackHeight", Caption = "Pallet Stack Height", MediumCaption = "Plt. Stack Height")]
		public ZByte PalletStackHeight
		{
			get => palletStackHeight;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(PalletStackHeightInfo, ref palletStackHeight, value);
				}
			}
		}

		ZByte palletStackHeight;

		public ZPropertyInfo PalletStackHeightInfo => GetZPropertyInfo(nameof(PalletStackHeight));

		#endregion

		#region LocationType

		public ZGuid LocationType
		{
			get => fLocationType;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(LocationTypeInfo, ref fLocationType, value);
				}
			}
		}

		public WhsLocationTypeCollection LocationTypes => base.Factory.GetCachedValue("WhsLocationTypeCollection", () => new WhsLocationTypeCollection(Factory));

		public ZPropertyInfo LocationTypeInfo => GetZPropertyInfo(nameof(LocationType));

		#endregion

		#region LocationStatus

		public LocationStatus LocationStatuses => new LocationStatus();

		[MaxLength(3)]
		public ZString LocationStatus
		{
			get => fLocationStatus;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (fLocationStatus != value)
					{
						CheckMaximumLength(LocationStatusInfo, value);
						SetNonPersistentPropertyValue(LocationStatusInfo, ref fLocationStatus, value);
					}
				}
			}
		}

		public ZPropertyInfo LocationStatusInfo => GetZPropertyInfo(nameof(LocationStatus));

		#endregion

		#region MaximumTouchCount

		public ZInt MaximumTouchCount
		{
			get => maximumTouchCount;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(MaximumTouchCountInfo, ref maximumTouchCount, value);
				}
			}
		}

		public ZPropertyInfo MaximumTouchCountInfo => GetZPropertyInfo(nameof(MaximumTouchCount));

		#endregion

		#region PickingArea_MassUpdate

		public ZGuid PickingArea_MassUpdate
		{
			get => pickingArea;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(PickingArea_MassUpdateInfo, ref pickingArea, value);
				}
			}
		}

		public ZPropertyInfo PickingArea_MassUpdateInfo => GetZPropertyInfo(nameof(PickingArea_MassUpdate));

		#endregion

		#region PutawayArea_MassUpdate

		public ZGuid PutawayArea_MassUpdate
		{
			get => putawayArea;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(PutawayArea_MassUpdateInfo, ref putawayArea, value);
				}
			}
		}

		public ZPropertyInfo PutawayArea_MassUpdateInfo => GetZPropertyInfo(nameof(PutawayArea_MassUpdate));

		#endregion

		#region PickMethods

		public ICodeDescriptionPairListWithDefaultCode PickMethods => WarehouseDataRegistry.Instance.PickMethod.Value;

		[MaxLength(3)]
		public ZString PickMethod
		{
			get => fPickMethod;
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (fPickMethod != value)
					{
						CheckMaximumLength(PickMethodInfo, value);
						SetNonPersistentPropertyValue(PickMethodInfo, ref fPickMethod, value);
					}
				}
			}
		}

		public ZPropertyInfo PickMethodInfo => GetZPropertyInfo(nameof(PickMethod));

		#endregion

		#region RangeMessage

		public ZString RangeMessage(ZString componentName, ZShort max, ZBool isZeroBased)
		{
			ZString msg;

			ZShort firstElement = isZeroBased ? (ZShort)0 : (ZShort)1;
			ZShort lastElement = isZeroBased ? max - 1 : max;

			if (lastElement != firstElement)
			{
				msg = Res.GetString("8cbfe781-db01-46eb-b7ba-5947c40e25f7", "Please enter a Location {0} between {1} and {2} (Row {3} contains {4} {5})", componentName, firstElement, lastElement, WR_Name, max, Grammar.Instance.Pluralize(componentName));
			}
			else
			{
				msg = Res.GetString("dd821fcc-8252-4d5e-b86e-d492f76b55e5", "Row {0} only has 1 {1} so the {2} must be {3}.", WR_Name, componentName, componentName, firstElement);
			}
			return msg;
		}

		#endregion

		#endregion

		#region WhsRowInternals Members

		void WhsRowInternals.GenerateLocations()
		{
			// note this function handles the resizing of existing rows
			// that is, if the row is reduced in size, the extra locations are removed
			// and if the row is increased in size, existing locations are retained while
			// new locations are added

			var warehouse = Warehouse;
			if (warehouse == null)
			{
				return;
			}

			if (warehouse.Areas.Count < 1)
			{
				((IWhsWarehouseInternals)warehouse).CreateDefaultArea();
			}

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Locations.Factory))
			{
				// first delete old locations
				foreach (WhsLocation locn in new ArrayList(Locations))
				{
					if (locn.WLV_Column > WR_Columns ||
						locn.WLV_Level > WR_Levels ||
						locn.WLV_Tray > WR_Trays)
					{
						Locations.Delete(locn);
					}
				}

				var defaultPickArea = GetArea(warehouse, a => a.WA_IsPickingArea, a => a.WA_IsDefaultPickArea);
				var defaultPutawayArea = GetArea(warehouse, a => a.WA_IsPutawayArea, a => a.WA_IsDefaultPutawayArea);

				// generate a new location for each bay in row
				for (ZShort col = 1; col <= WR_Columns; col++)
				{
					for (ZShort level = 1; level <= WR_Levels; level++)
					{
						for (ZShort tray = 1; tray <= WR_Trays; tray++)
						{
							// only create if not already there
							if (col > fColsPreEdit || level > fLevelsPreEdit || tray > fTraysPreEdit)
							{
								var locn = Locations.AddNew();
								using (locn.GetValidationSuspender())
								{
									locn.WLV_WW_Whs = WR_WW_Whs;
									locn.WLV_Column = col;
									locn.WLV_Level = level;
									locn.WLV_Tray = tray;
									locn.WLV_WA_PickingArea = defaultPickArea.PK;
									locn.WLV_WA_PutawayArea = defaultPutawayArea.PK;
									if (warehouse.WW_WarehouseType == WarehouseTypes.Codes.ContainerYard)
									{
										locn.WLV_MaxQuantity = 1m;
									}
								}
							}
						}
					}
				}
			}

			fColsPreEdit = WR_Columns;
			fLevelsPreEdit = WR_Levels;
			fTraysPreEdit = WR_Trays;
			locations = null;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "No need to translate WA_Name")]
		static WhsArea GetArea(WhsWarehouse warehouse, Func<WhsArea, bool> isPickingOrPutawayArea, Func<WhsArea, bool> isDefaultArea)
			=> warehouse.Areas
				.Where(isPickingOrPutawayArea)
				.OrderByDescending(isDefaultArea)
				.ThenBy(a => a.WA_AreaType == AreaTypes.Codes.DockDoor)
				.ThenBy(a => a.WA_Name)
				.First();

		#endregion

		#region Lookups

		protected override WhsRowLookups GetNewLookups()
		{
			switch (CountryCode)
			{
				case Enterprise.Core.Constants.CountryCodes.UnitedStates:
					return new US.WhsRowLookups(this);

				default:
					return base.GetNewLookups();
			}
		}

		#endregion

		#region UniqueIndexFailureHandlers

		public const string DuplicateLocationStringError = "Cannot insert duplicate key row in object 'dbo.WhsLocationView_DoNotUse' with unique index 'NR_UC__WhsLocationView_DoNotUse'";

		public static string LocationStringUniqueIndexFailureMessage => Res.GetString("68f75b04-9cd5-4fb4-bb87-a38d6a9c9305", "The changes to the location configurations on the row have resulted in a duplicate Location Barcode in the warehouse. Ensure the Row Name or Number of Columns/Levels/Trays does not result in a duplicate Location Barcode for Fixed Width Locations.");

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers => base.UniqueIndexFailureHandlers
			.Append(new LocationStringUniqueIndexValidationHandler(LocationStringUniqueIndexFailureMessage));

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.WarehouseRow);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter => new WhsRowDocumentSupporter(this);

		#endregion

		#region IAffectLocationView

		SchemaColumn[] IAffectLocationView.GetColumnsThatAffectLocationView()
		{
			return new SchemaColumn[]
			{
				WhsRowSchema.WR_Name,
				WhsRowSchema.WR_Columns,
				WhsRowSchema.WR_Levels,
				WhsRowSchema.WR_Trays,
			};
		}

		void IAffectLocationView.ReloadLocationsFromDB() => Locations.RefreshFromDb();

		ZGuid IAffectLocationView.ParentThatMayReloadMyLocations => WR_WW_Whs;

		#endregion

		#region Notifications

		public NotificationManager NotificationManager
		{
			get
			{
				if (notificationManager == null)
				{
					notificationManager = new NotificationManager();
					notificationManager.Push(new NotificationBuffer());
				}
				return notificationManager;
			}
		}

		public INotifications NotificationSubscriber => NotificationManager.Peek;

		IWhsLocationCollection IWhsRow.Locations => Locations;

		NotificationManager notificationManager;

		#endregion

		#region Implementation

		ZShort fColsPreEdit;
		ZShort fLevelsPreEdit;
		ZShort fTraysPreEdit;
		ZBool fGenerateCheckDigit;
		ZDecimal fMaxQuantity;
		ZDecimal fMaxWeight;
		ZDecimal fMaxCubic;
		ZString fMaxWeightUnit;
		ZString fMaxCubicUnit;
		ZGuid fLocationType;
		ZString fLocationStatus;
		ZString fPickMethod;
		ZGuid pickingArea;
		ZGuid putawayArea;
		ZString fApprovedKnownStatus;
		ZInt maximumTouchCount;

		//WhsRowLayout fLayout;
		WhsLocationCollection locations;

		#endregion
	}

	#region Document Supporter

	public class WhsRowDocumentSupporter : DocumentSupporter
	{
		public WhsRowDocumentSupporter(WhsRow row)
			: base(row)
		{
		}

		protected WhsRow Row => (WhsRow)BusinessObject;

		#region BusinessContext

		public override BusinessContext BusinessContext => BusinessContext.WhsRow;

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] {
				Constants.DataContext.WhsLocationLabels
			};
		}

		#endregion

		#region CustomisationSecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsConfigWarehouseCustomiseDocuments;

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Enterprise.Core.Constants.DataContext.WhsLocationLabels)
			{
				List<WhsLocation> locationList = null;
				bool isDocumentRunFromRowFilterControl = (Row.ParentForm == null);
				bool isDocumentRunFromLocationForm = (Row.SelectedLocations != null);

				if (isDocumentRunFromRowFilterControl)
				{
					locationList = new List<WhsLocation>(Row.Locations);
				}
				else if (isDocumentRunFromLocationForm)
				{
					locationList = Row.SelectedLocations;
				}

				if (locationList != null && locationList.Count > 0)
				{
					var locationLabelList = new WhsLocationLabelList(locationList, Factory);
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.WhsLocationLabels, locationLabelList) };
				}
			}

			return result;
		}

		#endregion

		#region GetBODocDataProvidersNotFoundMessage

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);

			if (dataContextValue.DataContext == Enterprise.Core.Constants.DataContext.WhsLocationLabels)
			{
				message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoLocation", "Cannot find Warehouse Location.");
			}

			return message;
		}

		#endregion
	}

	#endregion

	#region Internals

	internal interface WhsRowInternals
	{
		void GenerateLocations();
	}

	#endregion
}

#region Test
#if DEBUG
namespace Enterprise.Warehouse.Environment.Business
{
	public partial class WhsRow
	{
		///<summary>
		///WhsLocation created from FillWithValidTestData does not update fColsPreEdit/fLevelsPreEdit/fTraysPreEdit
		///This method is used to avoid create Location with Same WarehousePK/Row/Column/Level/Tray
		///</summary>
		internal void UpdateComponentsPreEdit_ForTest()
		{
			fColsPreEdit = 1;
			fLevelsPreEdit = 1;
			fTraysPreEdit = 1;
		}
	}
}

#endif
#endregion
