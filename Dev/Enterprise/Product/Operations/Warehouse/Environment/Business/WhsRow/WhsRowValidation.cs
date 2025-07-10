using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRowValidation : AutoWhsRowValidation
	{
		public WhsRowValidation(AutoWhsRow parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsRow Parent => (WhsRow)base.Parent;

		#endregion

		#region Warehouse

		protected WhsWarehouse Warehouse
		{
			get { return Parent.Warehouse; }
		}

		#endregion

		#region WR_PickPathSequence

		protected override void CheckWR_PickPathSequence()
		{
			base.CheckWR_PickPathSequence();

			MandatoryValidation.CheckNotNegative(Parent.WR_PickPathSequenceInfo);
			AddWarningIfPickPathSequenceIsZero();
		}

		void AddWarningIfPickPathSequenceIsZero()
		{
			if (!Parent.WR_PickPathSequenceInfo.HasErrors() && Parent.WR_PickPathSequence == 0)
			{
				Parent.WR_PickPathSequenceInfo.AddWarning(Res.GetString("da2937f6-1b19-45db-8cd0-36ecf099359a", "A value of Zero indicates this Row will be *Last* in the Pick Sequence."));
			}
		}

		#endregion

		#region WR_WW_Whs

		protected override void CheckWR_WW_Whs()
		{
			base.CheckWR_WW_Whs();

			var info = Parent.WR_WW_WhsInfo;
			MandatoryValidation.CheckEntered(info);
			CheckWR_WW_Whs_WarehouseCannotBeChangedAfterSave(info, Parent);
			CheckWR_WW_Whs_WarehouseHasDefaultsSet(info, Parent.Warehouse);
		}

		static void CheckWR_WW_Whs_WarehouseCannotBeChangedAfterSave(ZPropertyInfo info, WhsRow parent)
		{
			if (!info.HasErrors())
			{
				if (parent.IsInDatabase && parent.WR_WW_Whs != (ZGuid)info.OriginalValue)
				{
					info.AddError(Res.GetString("94db89af-eb17-47f8-8518-3a8c343b7539", "The Warehouse cannot be changed once the Row is saved."));
				}
			}
		}

		static void CheckWR_WW_Whs_WarehouseHasDefaultsSet(ZPropertyInfo info, WhsWarehouse warehouse)
		{
			if (!info.HasErrors() && warehouse != null)
			{
				if (warehouse.Areas.All(area => !area.WA_IsDefaultPickArea))
				{
					info.AddError(Res.GetString("092c54f2-fc02-4963-b5a8-84743158cd68", "Warehouse does not have a Default Pick Area set."));
				}

				if (warehouse.Areas.All(area => !area.WA_IsDefaultPutawayArea))
				{
					info.AddError(Res.GetString("967bacf1-89b3-455d-88a8-5e223c07c07e", "Warehouse does not have a Default Putaway Area set."));
				}
			}
		}

		#endregion

		#region WR_Name

		protected override void CheckWR_Name()
		{
			base.CheckWR_Name();

			MandatoryValidation.CheckEntered(Parent.WR_NameInfo);

			var warehouse = Warehouse;
			if (warehouse != null && (!Parent.IsInDatabase || Parent.WR_NameInfo.HasChanges))
			{
				if (!IsValidRowName)
				{
					Parent.WR_NameInfo.AddError(
						Res.GetString("942efbaf-4848-4b15-9aff-e39ef03e068a", "The") + " " + DataBoundResourceStrings.GetColumnDescriptiveName(Parent.TableName, Parent.WR_NameInfo.Name) + " " +
						Res.GetString("044dadac-7591-4621-b109-69d5525c97a0", "cannot contain the following spaces or the following characters:\r\n. , - {0}", warehouse.WW_LocationComponentDelimiter));
				}
				else if (warehouse.IsFixedWidthLocation && WarehouseDataRegistry.Instance.EnableRowNamePrefixValidationForFixedWidthWarehouses.Value)
				{
					CheckWR_NameForPrefixDuplicates();
				}
				else
				{
					CheckWR_NameForDuplicates();
				}
			}
		}

		bool IsValidRowName
		{
			get
			{
				var warehouse = Warehouse;
				return warehouse.WW_LocationComponentDelimiter.Length > 0 && !Parent.WR_Name.ContainsAnyChar(" .,-" + warehouse.WW_LocationComponentDelimiter[0]);
			}
		}

		protected void CheckWR_NameForDuplicates()
		{
			var dupKeyCheck = new ZQuery(WhsRowSchema.WR_WW_Whs, Parent.WR_WW_Whs);
			dupKeyCheck.AddToFilter(WhsRowSchema.WR_Name, Parent.WR_Name);
			dupKeyCheck.AddToFilter(WhsRowSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (!Parent.WR_NameInfo.HasErrors() && Parent.Factory.LoadTop1<WhsRow>(dupKeyCheck) != null)
			{
				Parent.WR_NameInfo.AddError(Res.GetString("07d1912d-52e8-4a4c-a93a-74cea038c7b3", "Duplicate Row names are not allowed"));
			}
		}

		protected void CheckWR_NameForPrefixDuplicates()
		{
			var subQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsRowSchema.PK);
			subQuery.AddToFilter(JoinCondition.Or, WhsRowSchema.WR_Name, SQLComparisonOperator.Like, Parent.WR_Name + "%");

			var parameters = new ZSqlParameterCollection
			{
				{ "@RowName", Parent.WR_Name, WhsRowSchema.WR_Name }
			};
			subQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "CHARINDEX({0}, @RowName) = 1", WhsRowSchema.WR_Name.Name), parameters, JoinCondition.Or);

			var query = new ZDBOnlyQuery(typeof(WhsRow));
			query.AddToFilter(WhsRowSchema.WR_WW_Whs, Parent.WR_WW_Whs);
			query.AddToFilter(WhsRowSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddSubQuery(WhsRowSchema.PK, subQuery, JoinCondition.And);

			if (!Parent.WR_NameInfo.HasErrors() && Parent.Factory.LoadTop1<WhsRow>(query) != null)
			{
				Parent.WR_NameInfo.AddError(Res.GetString("7fde437c-d6e7-4dc5-829b-ba14103c17cf", "With 'Fixed Width Location' set, Row names cannot be prefixes of other Row names from the same Warehouse."));
			}
		}

		#endregion

		#region WR_Columns

		protected override void CheckWR_Columns()
		{
			base.CheckWR_Columns();

			var warehouse = Warehouse;
			var maxColumns = warehouse != null
				? GetMaxLocationComponents(warehouse.WW_LocationColumnsAlpha, warehouse.WW_LocationColumnsFixedWidth, 999)
				: 999;

			if (Parent.WR_Columns < 1)
			{
				Parent.WR_ColumnsInfo.AddError(Res.GetString("2f327633-07c9-41db-8cb7-da02c9227f77", "A Row must have at least 1 Column."));
			}
			else if (Parent.WR_Columns > maxColumns)
			{
				Parent.WR_ColumnsInfo.AddError(Res.GetString("e6a495b0-1ceb-437d-9f4e-f4c3e5d91dc5", "A Row must not have more than {0} Columns.", maxColumns));
			}
			else if (Parent.IsInDatabase && Parent.WR_Columns < (ZShort)Parent.WR_ColumnsInfo.OriginalValue && IsLocationInUse(WhsRowSchema.WR_Columns, WhsLocationSchema.WL_Column.Name))
			{
				Parent.WR_ColumnsInfo.AddError(GetSavedLocationReductionError(Parent.WR_ColumnsInfo, Res.GetString("1e58d196-8d85-416c-a69c-90b2901e6f00", "Columns")));
			}
			else
			{
				CheckLocationCount(Parent.WR_ColumnsInfo);
			}
		}

		#endregion

		#region WR_Levels

		protected override void CheckWR_Levels()
		{
			base.CheckWR_Levels();

			var warehouse = Warehouse;
			var maxLevels = warehouse != null
				? GetMaxLocationComponents(warehouse.WW_LocationLevelsAlpha, warehouse.WW_LocationLevelsFixedWidth, 999)
				: 999;

			if (Parent.WR_Levels < 1)
			{
				Parent.WR_LevelsInfo.AddError(Res.GetString("88c814d2-2535-4311-902d-c64b6836233c", "A Row must have at least 1 Level."));
			}
			else if (Parent.WR_Levels > maxLevels)
			{
				Parent.WR_LevelsInfo.AddError(Res.GetString("8b099f05-2cb4-4039-8b43-f5d9319726b0", "A Row must not have more than {0} Levels.", maxLevels));
			}
			else if (Parent.IsInDatabase && Parent.WR_Levels < (ZShort)Parent.WR_LevelsInfo.OriginalValue && IsLocationInUse(WhsRowSchema.WR_Levels, WhsLocationSchema.WL_Level.Name))
			{
				Parent.WR_LevelsInfo.AddError(GetSavedLocationReductionError(Parent.WR_LevelsInfo, Res.GetString("e9a2bc9d-2bb2-476b-babf-15f38ce9c939", "Levels")));
			}
			else
			{
				CheckLocationCount(Parent.WR_LevelsInfo);
			}
		}

		#endregion

		#region WR_Trays

		protected override void CheckWR_Trays()
		{
			base.CheckWR_Trays();

			var warehouse = Warehouse;
			int maxTrays = warehouse != null
				? GetMaxLocationComponents(warehouse.WW_LocationTraysAlpha, warehouse.WW_LocationTraysFixedWidth, 99)
				: 99;

			if (Parent.WR_Trays < 1)
			{
				Parent.WR_TraysInfo.AddError(Res.GetString("2b57a540-8aae-44d7-991d-35bf35c3df0b", "A Row must have at least 1 Tray."));
			}
			else if (warehouse != null && warehouse.WW_WarehouseType == WarehouseTypes.Codes.ContainerYard && Parent.WR_Trays != 1)
			{
				Parent.WR_TraysInfo.AddError(Res.GetString("08b0aa0f-5736-4493-aa0a-62e5e4cacd18", "Container Yard do not use trays and the value must remain at 1."));
			}
			else if (Parent.WR_Trays > maxTrays)
			{
				Parent.WR_TraysInfo.AddError(Res.GetString("5f522f99-145b-4c2c-8adc-50de638023b4", "A Row must not have more than {0} Trays.", maxTrays));
			}
			else if (Parent.IsInDatabase && Parent.WR_Trays < (ZShort)Parent.WR_TraysInfo.OriginalValue && IsLocationInUse(WhsRowSchema.WR_Trays, WhsLocationSchema.WL_Tray.Name))
			{
				Parent.WR_TraysInfo.AddError(GetSavedLocationReductionError(Parent.WR_TraysInfo, Res.GetString("7e5525be-9408-4847-9765-784fc2377c03", "Trays")));
			}
			else
			{
				CheckLocationCount(Parent.WR_TraysInfo);
			}
		}

		#endregion

		#region Non Persistent

		#region ValidateSortCycleCountMethod

		public void ValidateSortCycleCountMethod()
		{
			ValidateCalculatedProperty(Parent.SortCycleCountMethodInfo);
		}

		protected void CheckSortCycleCountMethod()
		{
			CheckForValidSortPathMethod(Parent.SortCycleCountMethodInfo);
		}

		#endregion

		#region ValidateSortPickPathMethod

		public void ValidateSortPickPathMethod()
		{
			ValidateCalculatedProperty(Parent.SortPickPathMethodInfo);
		}

		protected void CheckSortPickPathMethod()
		{
			CheckForValidSortPathMethod(Parent.SortPickPathMethodInfo);
		}

		#endregion

		#region ValidateSortPutawayPathMethod

		public void ValidateSortPutawayPathMethod()
		{
			ValidateCalculatedProperty(Parent.SortPutawayPathMethodInfo);
		}

		protected void CheckSortPutawayPathMethod()
		{
			CheckForValidSortPathMethod(Parent.SortPutawayPathMethodInfo);
		}

		#endregion

		void CheckForValidSortPathMethod(ZPropertyInfo sortPathMethodInfo)
		{
			ListValidation.ErrorIfInvalidCode(sortPathMethodInfo);
		}

		#endregion

		#region Implementation

		int GetMaxLocationComponents(bool isLocationComponentAlpha, ZByte locationComponentFixedWidth, int maxLocationComponentSize)
		{
			var maxLocationComponents = maxLocationComponentSize;
			if (isLocationComponentAlpha)
			{
				maxLocationComponents = 26;
			}
			else if (locationComponentFixedWidth != 0)
			{
				maxLocationComponents = WhsLocationHelper.GetMaxLocationComponentValueWithFixedWidth(locationComponentFixedWidth);
			}

			return maxLocationComponents;
		}

		void CheckLocationCount(ZPropertyInfo info)
		{
			var currentLocationCount = Parent.WR_Columns * Parent.WR_Levels * Parent.WR_Trays;

			var maxLocationsPerRow = WarehouseDataRegistry.Instance.MaxNumberOfLocationsPerRow.Value;
			if (currentLocationCount > maxLocationsPerRow)
			{
				var locationCountInDB = (Parent.IsInDatabase)
					? (ZShort)Parent.WR_ColumnsInfo.OriginalValue * (ZShort)Parent.WR_LevelsInfo.OriginalValue * (ZShort)Parent.WR_TraysInfo.OriginalValue
					: 0;

				var message = Res.GetString("ae412bdc-b836-42cd-8611-e05e81bfa8b2",
					"The number of locations is over {0} with this combination of Columns, Levels and Trays.", maxLocationsPerRow);

				if (locationCountInDB > maxLocationsPerRow && currentLocationCount <= locationCountInDB)
				{
					info.AddWarning(message);
				}
				else
				{
					info.AddError(message);
				}
			}
		}

		#region IsLocationInUse

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ZBool IsLocationInUse(SchemaShortColumn column, string locationColumnName)
		{
			var locationInUse = false;
			var whereClause = string.Join("\r\nUNION ", ColumnsReferencingLocation // May be a part of SQL expression.
							  .Select(c => string.Format(CultureInfo.InvariantCulture, "SELECT NULL as Value FROM {0} x INNER JOIN dbo.WhsLocation wl ON x.{1} = wl.WL_PK WHERE wl.WL_WR = @WL_WR AND wl.{2} > @{2}", c.TableName, c.ColumnName, locationColumnName)));

			var parameter = ZSqlParameter.New(string.Format(CultureInfo.InvariantCulture, "@{0}", locationColumnName), Parent[column], column);
			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT 1 WHERE EXISTS ({0})", whereClause); // May be a part of SQL expression.

			using (var command = Db.Connection.Command(sql)) // No other way to easily Query multiple tables without access to all their types
			{
				command.AddParameterBasedOnDbColumn("@WL_WR", Parent.PK.ToGuid(), WhsRowSchema.PK);
				command.AddParameter(parameter);
				locationInUse = command.ExecuteScalar() != null;
			}

			return locationInUse;
		}

		(ZString ColumnName, ZString TableName)[] ColumnsReferencingLocation
		{
			get { return Parent.Factory.GetCachedValue("WhsRowValidation|ColumnsReferencingLocation", GetColumnsReferencingLocation().ToArray); }
		}

		IEnumerable<(ZString ColumnName, ZString TableName)> GetColumnsReferencingLocation()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT 
	OBJECT_NAME(ForeignKeys.parent_object_id) as TableName, 
	COL_NAME(ForeignKeyColumns.parent_object_id, ForeignKeyColumns.parent_column_id) as ColumnName 
FROM 
	sys.foreign_keys as ForeignKeys 
	JOIN sys.foreign_key_columns as ForeignKeyColumns ON ForeignKeys.OBJECT_ID = ForeignKeyColumns.constraint_object_id 
	JOIN sys.tables Tables ON Tables.OBJECT_ID = ForeignKeyColumns.referenced_object_id 
WHERE 
	OBJECT_NAME(ForeignKeys.referenced_object_id) = '{0}'
", WhsLocationSchema.Constants.TableName); // May be a part of SQL expression.

			var dynamicBizO = new DynamicBusinessObjectCollection(Parent.Factory);
			dynamicBizO.Load(sql); // direct SQL

			foreach (DynamicBusinessObject bizO in dynamicBizO)
			{
				var tableName = (ZString)bizO["TableName"];
				if (!IsTableExcludedFromRowTypeCheck(tableName))
				{
					yield return ((ZString)bizO["ColumnName"], tableName);
				}
			}
		}

		bool IsTableExcludedFromRowTypeCheck(string tableName)
		{
			return tableName == "WhsPutawayLocationCache"
				|| tableName == "GateTransportCFSDetail";
		}

		#endregion

		public static string CurrentStockInColumnErrorMsg
		{
			get
			{
				return Res.GetString("7e2241e6-cec0-4d14-8ffc-32867cc11b0c",
				"There are current stock quantities in columns you are attempting to remove.\r\nPlease remove the existing stock before attempting to reduce the number of columns on this row.");
			}
		}

		public static string CurrentStockInLevelErrorMsg
		{
			get
			{
				return Res.GetString("9a5ec9a6-5eb2-43ab-b702-bfd36c380cc5",
				"There are current stock quantities in levels you are attempting to remove.\r\nPlease remove the existing stock before attempting to reduce the number of levels on this row.");
			}
		}

		public static string CurrentStockInTrayErrorMsg
		{
			get
			{
				return Res.GetString("7aeabc72-14db-44fd-8c83-e164287366c0",
				"There are current stock quantities in trays you are attempting to remove.\r\nPlease remove the existing stock before attempting to reduce the number of trays on this row.");
			}
		}

		ZString GetSavedLocationReductionError(ZPropertyInfo dimensionInfo, ZString dimensionText)
		{
			return Res.GetString("20d31b58-6ef9-49bf-b1ec-21e51263a008",
				"The number of {0} cannot be reduced. Enter a value greater than or equal to {1}.\r\n\r\nLocations, however, can be given a void status.", dimensionText, dimensionInfo.OriginalValue);
		}

		#endregion
	}
}
