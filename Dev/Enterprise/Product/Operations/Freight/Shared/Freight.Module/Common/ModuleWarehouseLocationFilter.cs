using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public delegate ZQuery GetWarehouseLocationQuery(ZQuery trueLocationFilter);

	public class ModuleWarehouseLocationFilter : ModuleFilter
	{
		#region Schema

		public static class Schema
		{
			public const string Location = "Location";
			public const string Warehouse = "Warehouse";
			public const string WarehouseList = "WarehouseList";
		}

		#endregion

		protected ModuleWarehouseLocationFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleWarehouseLocationFilter(ZString description, GetWarehouseLocationQuery queryDelegate, IWhsWarehouseCollection warehouseList)
			: base(description, queryDelegate)
		{
			this.warehouseList = warehouseList;
		}

		#region Location

		[MaxLength(36)]
		public ZString Location
		{
			[DebuggerStepThrough]
			get { return location; }
			set
			{
				value = value.TrimEnd();
				if (value != location)
				{
					CheckMaximumLength(LocationInfo, value);

					Match match = LocationPattern.Match(value);

					if (match.Success)
					{
						row = (string.IsNullOrEmpty(match.Groups[2].Value)) ? null : match.Groups[2].Value;
						column = (string.IsNullOrEmpty(match.Groups[4].Value)) ? null : (ZByte?)ZByte.ParseSafe(match.Groups[4].Value, 1);
						level = (string.IsNullOrEmpty(match.Groups[6].Value)) ? null : (ZByte?)ZByte.ParseSafe(match.Groups[6].Value, 1);
						tray = (string.IsNullOrEmpty(match.Groups[8].Value)) ? null : (ZByte?)ZByte.ParseSafe(match.Groups[8].Value, 1);

						StringBuilder builder = new StringBuilder(row);
						if (column != null)
						{
							builder.AppendFormat("-{0}", column);
						}

						if (level != null)
						{
							builder.AppendFormat("-{0}", level);
						}

						if (tray != null)
						{
							builder.AppendFormat("-{0}", tray);
						}

						location = builder.ToString();
					}
					else
					{
						location = value;
						row = null;
						column = null;
						level = null;
						tray = null;
					}

					LocationInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateLocation();
					}

					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(Schema.Location); }
		}

		ZString location;

		string row;
		byte? column;
		byte? level;
		byte? tray;

		#endregion

		#region Warehouse

		[List("WarehouseList")]
		public ZGuid Warehouse
		{
			[DebuggerStepThrough]
			get { return warehouse; }
			set { SetNonPersistentPropertyValue(WarehouseInfo, ref warehouse, value); }
		}

		public ZPropertyInfo WarehouseInfo
		{
			get { return GetZPropertyInfo(Schema.Warehouse); }
		}

		ZGuid warehouse;

		#endregion

		#region WarehouseList

		public IWhsWarehouseCollection WarehouseList
		{
			[DebuggerStepThrough]
			get { return warehouseList; }
		}

		readonly IWhsWarehouseCollection warehouseList;

		#endregion

		#region GetTrueWarehouseLocationFilter

		ZQuery GetTrueWarehouseLocationFilter()
		{
			var rowFilter = new ZDBOnlySubQuery(typeof(IWhsRow), WhsLocationViewSchema.WLV_WR);

			if (!Warehouse.IsEmpty)
			{
				rowFilter.AddToFilter(WhsRowSchema.WR_WW_Whs, Warehouse);
			}

			if (!ReferenceEquals(row, null))
			{
				rowFilter.AddToFilter(WhsRowSchema.WR_Name, row);
			}

			var whsLocationFilter = new ZDBOnlyQuery(typeof(IWhsLocation));

			if (!rowFilter.IsEmpty)
			{
				whsLocationFilter.AddSubQuery(rowFilter, JoinCondition.And);
			}

			if (column != null)
			{
				whsLocationFilter.AddToFilter(WhsLocationViewSchema.WLV_Column, column);
			}

			if (level != null)
			{
				whsLocationFilter.AddToFilter(WhsLocationViewSchema.WLV_Level, level);
			}

			if (tray != null)
			{
				whsLocationFilter.AddToFilter(WhsLocationViewSchema.WLV_Tray, tray);
			}

			return whsLocationFilter;
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleWarehouseLocationFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			ModuleWarehouseLocationFilter filter = filterToCopyFrom as ModuleWarehouseLocationFilter;
			warehouse = filter.warehouse;
			location = filter.location;
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			Location = ZString.Empty;
			Warehouse = ZGuid.Empty;
		}

		protected override bool IsEmptyCore => warehouse.IsEmpty
											   && row == null
											   && column == null
											   && level == null
											   && tray == null;

		#endregion

		#region Validaiton

		public new ModuleWarehouseLocationFilterValidation Validation
		{
			get { return (ModuleWarehouseLocationFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleWarehouseLocationFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { GetTrueWarehouseLocationFilter() }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new InvalidOperationException("Not Happening");
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Warehouse", Warehouse.ToString());
			writer.WriteElementString("Location", Location);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			Warehouse = new ZGuid(reader.ReadElementString("Warehouse"));
			Location = reader.ReadElementString("Location");
		}

		#endregion

		#region LocationPattern

		[ThreadStatic]
		static Regex locationPattern;

		public static Regex LocationPattern
		{
			get
			{
				if (locationPattern == null)
				{
					// Matches:
					// ABC1
					// ABC1-1
					// ABC1-1-1
					// ABC1-1-1-1
					locationPattern = new Regex(@"^(([A-Z0-9]+)([\s-,.]([0-9]+)([\s-,.]([0-9]+)([\s-,.]([0-9]+))?)?)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
				}
				return locationPattern;
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Location = RandomString(MaxLength);
			Warehouse = ZGuid.NewZGuid();
		}

#endif
		#endregion
	}

	public class ModuleWarehouseLocationFilterValidation : ModuleFilterValidation
	{
		public ModuleWarehouseLocationFilterValidation(ModuleWarehouseLocationFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidateWarehouse

		public void ValidateWarehouse()
		{
			ValidateCalculatedProperty(Parent.WarehouseInfo);
		}

		protected virtual void CheckWarehouse()
		{
			TypeValidation.CheckValidGuid(Parent.WarehouseInfo);
			ListValidation.ErrorIfInvalidPK(Parent.WarehouseInfo, Parent.WarehouseList);
		}

		#endregion

		#region ValidateLocation

		public void ValidateLocation()
		{
			ValidateCalculatedProperty(Parent.LocationInfo);
		}

		protected virtual void CheckLocation()
		{
			if (!Parent.Location.IsEmpty)
			{
				Match match = ModuleWarehouseLocationFilter.LocationPattern.Match(Parent.Location);
				if (!match.Success)
				{
					Parent.LocationInfo.AddError(Res.GetString("6206530a-c15b-4bf1-b34d-27bd755c8f57", "This location is incorrectly formatted.\r\nSome examples of valid formats are:\r\n ROW1\r\n ROW1-2\r\n ROW1-2-3\r\n ROW1-2-3-4"));
				}
			}
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(ModuleWarehouseLocationFilterValidation); }
		}

		public override void ValidateAll()
		{
			ValidateWarehouse();
			ValidateLocation();
		}

		readonly ModuleWarehouseLocationFilter Parent;

		#endregion
	}
}
