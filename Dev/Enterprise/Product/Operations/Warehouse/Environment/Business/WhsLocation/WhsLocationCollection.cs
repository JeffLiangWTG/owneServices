using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	#region class WhsLocationCollection

	[ModuleID(ModuleId.WhsConfigLocation)]
	public class WhsLocationCollection : ActiveBusinessObjectCollection<WhsLocation>, ICompositeCollection, IWhsLocationCollection
	{
		public const int LocationStringMaxLength = 36;

		#region Constructors

		public WhsLocationCollection(BusinessObjectFactory factory)
			: this(factory, dockDoorLocationsOnly: false)
		{
		}

		public WhsLocationCollection(BusinessObjectFactory factory, bool dockDoorLocationsOnly)
			: base(factory)
		{
			ApplyVoidStatusFilter();
			AddDockDoorLocationsFilterBusinessObjectDefaults(dockDoorLocationsOnly);
		}

		public WhsLocationCollection(WhsWarehouse warehouse)
			: base(warehouse.Factory, new WarehouseLocationCollectionRelationship(warehouse))
		{
			AddFilterBusinessObjectDefaults();
			ApplyVoidStatusFilter();
		}

		public WhsLocationCollection(WhsWarehouse warehouse, bool dockDoorLocationsOnly)
			: this(warehouse)
		{
			AddDockDoorLocationsFilterBusinessObjectDefaults(dockDoorLocationsOnly);
		}

		public WhsLocationCollection(WhsRow row)
			: base(row)
		{
		}

		public static WhsLocationCollection GetEmptyLocationCollection(BusinessObjectFactory factory)
		{
			return new WhsLocationCollection(factory, new NoResultRelationship(typeof(WhsLocation)));
		}

		WhsLocationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		void AddFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterSchema.Warehouse, "Property",
				RelationshipForWarehouse.Warehouse.PK, false));
		}

		void AddDockDoorLocationsFilterBusinessObjectDefaults(bool dockDoorLocationsOnly)
		{
			if (dockDoorLocationsOnly)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterSchema.DockDoorLocation, "Property0",
					ZBool.True, isRemovable: false));
			}
		}

		void ApplyVoidStatusFilter()
		{
			AdditionalFilter = new ZQuery(WhsLocationViewSchema.WLV_LocationStatus, SQLComparisonOperator.NotEqual,
				LocationStatus.Codes.Void);
		}

		#endregion

		#region AllowNew

		protected override bool AllowNew => false;

		#endregion

		#region FindByColumnLevelTray

		public WhsLocation FindByColumnLevelTray(int column, int level, int tray)
		{
			var row = Relationship.Master as WhsRow;
			Argument.NotNull(row, "Cannot call FindByColumnLevelTray() on a WhsLocationCollection whose parent is a WhsRow.");

			WhsLocation result = null;
			if (column > 0 && column <= row.WR_Columns && level > 0 && level <= row.WR_Levels && tray > 0 && tray <= row.WR_Trays)
			{
				// first try to find from calc (only works if collection sorted by column, level, tray
				var index = ((column - 1) * row.WR_Levels * row.WR_Trays) + ((level - 1) * row.WR_Trays) + tray - 1;
				var location = this[index];

				if (LocationMatch(location, column, level, tray))
				{
					result = location;
				}
				else
				{
					// can't calc index so now do full collection scan
					foreach (var loc in this)
					{
						if (LocationMatch(loc, column, level, tray))
						{
							result = location;
							break;
						}
					}
				}
			}

			return result;
		}

		bool LocationMatch(WhsLocation location, int column, int level, int tray)
		{
			return (location.WLV_Column == column && location.WLV_Level == level && location.WLV_Tray == tray);
		}

		#endregion

		#region Sorting

		#region GetSortComparerForProperty

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			switch (property.Name)
			{
				case WhsLocationViewSchema.Constants.WLV_LocationString:
					return new LocationComparer<WhsLocation>(property, direction, location => location);
				case WhsLocationViewSchema.Constants.WLV_PickPathSequence:
					return new PickPathSequenceComparer(property, direction);
				case nameof(WhsLocation.FormattedColumn):
					return new ColLevelTrayComparer(property, direction, location => location.WLV_Column);
				case nameof(WhsLocation.FormattedLevel):
					return new ColLevelTrayComparer(property, direction, location => location.WLV_Level);
				case nameof(WhsLocation.FormattedTray):
					return new ColLevelTrayComparer(property, direction, location => location.WLV_Tray);
				case nameof(WhsLocation.RowLocationSequence):
					return new ColLevelTrayComparer(property, direction, location => location.WLV_PickPathSequence);
				default:
					return base.GetSortComparerForProperty(property, direction);
			}
		}

		class PickPathSequenceComparer : PropertyComparer
		{
			public PickPathSequenceComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				int result;
				var firstPickPathSequenceIsEmpty = ((WhsLocation)x).WLV_PickPathSequence.IsEmpty;
				var secondPickPathSequenceIsEmpty = ((WhsLocation)y).WLV_PickPathSequence.IsEmpty;

				if (firstPickPathSequenceIsEmpty || secondPickPathSequenceIsEmpty)
				{
					result = Direction == ListSortDirection.Ascending
						? firstPickPathSequenceIsEmpty.CompareTo(secondPickPathSequenceIsEmpty)
						: secondPickPathSequenceIsEmpty.CompareTo(firstPickPathSequenceIsEmpty);
				}
				else
				{
					result = base.Compare(x, y);
				}

				return result;
			}
		}

		class ColLevelTrayComparer : PropertyComparer
		{
			public ColLevelTrayComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction, Func<WhsLocation, IComparable> getColLevelTrayValue)
				: base(propertyDescriptor, direction)
			{
				GetColLevelTrayValue = getColLevelTrayValue;
			}

			Func<WhsLocation, IComparable> GetColLevelTrayValue { get; }

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				var propertyValueToCompare1 = GetColLevelTrayValue((WhsLocation)x);
				var propertyValueToCompare2 = GetColLevelTrayValue((WhsLocation)y);

				return Direction == ListSortDirection.Ascending
						? propertyValueToCompare1.CompareTo(propertyValueToCompare2)
						: propertyValueToCompare2.CompareTo(propertyValueToCompare1);
			}
		}

		#endregion

		#region class SortByColLevelTray

		public class SortByColLevelTray : IComparer
		{
			public int Compare(object o1, object o2)
			{
				var loc1 = (WhsLocation)o1;
				var loc2 = (WhsLocation)o2;

				if (loc1 == null)
				{
					return 1;
				}

				if (loc2 == null)
				{
					return -1;
				}

				if (loc1.WLV_Column < loc2.WLV_Column)
				{
					return -1;
				}

				if (loc1.WLV_Column > loc2.WLV_Column)
				{
					return 1;
				}

				if (loc1.WLV_Level < loc2.WLV_Level)
				{
					return -1;
				}

				if (loc1.WLV_Level > loc2.WLV_Level)
				{
					return 1;
				}

				if (loc1.WLV_Tray < loc2.WLV_Tray)
				{
					return -1;
				}

				if (loc1.WLV_Tray > loc2.WLV_Tray)
				{
					return 1;
				}

				return 0;
			}

			// to enable flyweight caching of the ActiveBizOCollection Indexer (tests will fail without this)
			public override bool Equals(object obj)
			{
				return obj.GetType() == GetType();
			}

			// to enable flyweight caching of the ActiveBizOCollection Indexer (tests will fail without this)
			public override int GetHashCode()
			{
				return GetType().GetHashCode();
			}
		}

		#endregion

		#endregion

		#region SetValues

		public void SetValues(WhsLocation[] locations)
		{
			var row = Relationship.Master as WhsRow;
			Argument.NotNull(row, "Cannot call SetValues() on a WhsLocationCollection whose parent is not a WhsRow.");

			row.UpdateLocationValues(locations);
		}

		#endregion

		#region FilterSchema

		// used by filter bizos
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name")]
		public static class FilterSchema
		{
			public const string Warehouse = "Warehouse";
			public const string LocationEmptyStatus = "Location Empty Status";
			public const string DockDoorLocation = "Dock Door Locations";
			public const string BondedLocation = "Bonded Pick Locations";
			public const string PackingStationLocation = "Packing Station Locations";
			public const string LocationStatus = "Location Status";
			public const string Row = "Row";
			public const string Level = "Level";
			public const string Column = "Column";
			public const string Tray = "Tray";
			public const string PickingArea = "Picking Area";
			public const string PutawayArea = "Putaway Area";
			public const string CycleCountLastPerformed = "Cycle Count Last Performed Date";
			public const string CycleCountPathSequence = "Cycle Count Path Sequence";
			public const string LastInventoryChangeDate = "Last Inventory Change Date";
			public const string PickPathSequence = "Pick Path Sequence";
			public const string PutawayPathSequence = "Putaway Path Sequence";
		}

		#endregion

		#region Relationship

		public WarehouseLocationCollectionRelationship RelationshipForWarehouse => base.Relationship as WarehouseLocationCollectionRelationship;

		#region class WarehouseLocationCollectionRelationship

		public class WarehouseLocationCollectionRelationship : CollectionRelationship
		{
			public WarehouseLocationCollectionRelationship(WhsWarehouse warehouse)
				: base(typeof(WhsLocation))
			{
				Warehouse = warehouse;
			}

			public readonly WhsWarehouse Warehouse;

			public override BusinessObject Master => Warehouse;

			protected override ZQuery RelationshipFilterCore => new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, Warehouse.PK);

			protected override bool SupportsAddToRelationshipCore() => false;
		}

		#endregion

		#endregion

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return findBoxListProvider ?? (findBoxListProvider = new LocationsFindBoxListProvider(this)); }
		}

		LocationsFindBoxListProvider findBoxListProvider;

		#region class LocationsFindBoxListProvider

		/// <summary>
		/// Tested in WhsInventory.WI_WL.
		/// </summary>
		internal class LocationsFindBoxListProvider : FindBoxListProvider
		{
			public LocationsFindBoxListProvider(WhsLocationCollection collection)
				: base(collection)
			{
			}

			public new WhsLocationCollection List => (WhsLocationCollection)base.List;

			public override ZGuid PrimaryKeyFromCode(string code)
			{
				var result = ZGuid.Invalid;

				if (List.RelationshipForWarehouse != null)
				{
					var warehouse = List.RelationshipForWarehouse.Warehouse;
					var location = warehouse?.FindLocation(code);
					if (location != null)
					{
						result = location.PK;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region ICompositeCollection Members

		/// <summary>
		/// This is here to work around a bug in ZGuidFindBox that causes the entire location list to be loaded
		/// when the user tabs into the Location field. On client sites this was causing delays of up to 10 minutes.
		/// Brett will be fixing the arc, in the meantime the max length here eliminates the need to load the list:
		/// </summary>
		int ICompositeCollection.MaxLength => LocationStringMaxLength;

		Type ICompositeCollection.TypeOfElementFromCode(ZString code) => typeof(WhsLocation);

		Type ICompositeCollection.TypeOfElementFromPK(ZGuid pk) => typeof(WhsLocation);

		#endregion
	}

	#endregion

	#region class WhsLocationAreaCollection

	public class WhsLocationAreaCollection : ActiveBusinessObjectCollection<WhsLocation>
	{
		public WhsLocationAreaCollection(WhsArea master, BusinessObjectFactory factory, SchemaColumn relationshipColumn)
			: base(factory, master, null, relationshipColumn)
		{
		}

		protected override bool AllowNew => false;
	}

	#endregion
}
