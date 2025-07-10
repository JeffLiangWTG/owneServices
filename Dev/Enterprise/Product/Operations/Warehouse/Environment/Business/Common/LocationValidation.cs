using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class LocationValidation : ValidationProvider
	{
		public LocationValidation(IBusiness parent = null)
		{
			this.parent = parent;
		}

		#region ValidateLocation

		public void ValidateLocation(ZPropertyInfo locationStringInfo)
		{
			var locationConsumer = (ILocationConsumer)parent;
			var factory = locationConsumer.Factory;
			var warehousePK = locationConsumer.WarehousePK;
			var warehouse = GetWarehouse(factory, warehousePK);
			if (warehouse == null)
			{
				AddValidationError(locationStringInfo, Res.GetString("babbfec7-d09d-49e5-b13b-76316a1ae8b5", "A valid warehouse has not been selected for this {0}.", locationConsumer.LocationTypeForMessages));
			}
			else
			{
				var locationString = ((ZString)locationStringInfo.Value).Trim();
				if (locationString.IsEmpty)
				{
					var locationTitle = locationConsumer.LocationTitle.IsEmpty
						? string.Empty
						: Res.GetString("b7b4e7e5-1c57-4faf-9894-c9072c92092d", "No {0} has been entered.", locationConsumer.LocationTitle) + " ";
					var errorMessage = locationTitle + Res.GetString("36eae0c0-3406-424d-a08b-c51a6c088018", "Please enter a valid Location.");
					AddValidationError(locationStringInfo, errorMessage);
				}
				else
				{
					ValidateLocationString(locationConsumer, locationStringInfo, warehouse);
				}
			}

			if (!locationStringInfo.HasErrors() && !locationConsumer.LocationPK.IsValid)
			{
				AddValidationError(locationStringInfo, Res.GetString("8d64e571-036c-4d06-a8b3-8751d13eb9d7", "Please re-enter a valid Location."));
			}
		}

		void ValidateLocationString(ILocationConsumer locationConsumer, ZPropertyInfo locationStringInfo, WhsWarehouse warehouse)
		{
			if (warehouse.IsFixedWidthLocation)
			{
				ValidateFixedWidthLocationString(locationConsumer, locationStringInfo, warehouse);
			}
			else
			{
				ValidateLocationStringCore(locationConsumer, locationStringInfo, warehouse);
			}
		}

		void ValidateLocationStringCore(ILocationConsumer locationConsumer, ZPropertyInfo locationStringInfo, WhsWarehouse warehouse)
		{
			var delimiters = new char[5] { ' ', '-', ',', '.', warehouse.WW_LocationComponentDelimiter[0] };
			var locationString = (ZString)locationStringInfo.Value;
			var locationParts = locationString.Split(delimiters);

			var rowFilter = new ZQuery(WhsRowSchema.WR_Name, locationParts[0]);
			rowFilter.AddToFilter(WhsRowSchema.WR_WW_Whs, warehouse.PK);
			var matchingRows = locationConsumer.Factory.Load<WhsRow>(rowFilter);
			var row = LookForExactMatchFirst(matchingRows, locationParts[0]);

			if (row != null)
			{
				var parser = new LocationParserWithInfoError(locationConsumer);

				if (locationParts.Length > 1)
				{
					parser.GetLocationComponent(row, locationStringInfo, locationParts[1], warehouse.WW_LocationColumnsAlpha, warehouse.WW_LocationColumnsZeroBased, row.WR_Columns, (NoResString)"Column"); // programmatic constant
					if (locationParts.Length > 2)
					{
						parser.GetLocationComponent(row, locationStringInfo, locationParts[2], warehouse.WW_LocationLevelsAlpha, warehouse.WW_LocationLevelsZeroBased, row.WR_Levels, (NoResString)"Level"); // programmatic constant
						if (locationParts.Length > 3)
						{
							parser.GetLocationComponent(row, locationStringInfo, locationParts[3], warehouse.WW_LocationTraysAlpha, warehouse.WW_LocationTraysZeroBased, row.WR_Trays, (NoResString)"Tray"); // programmatic constant
						}
					}
				}
			}
			else
			{
				var errorMessage = locationConsumer.LocationTitle.IsEmpty
					? Res.GetString("ce5f003b-a469-451e-9217-030f3b6e8efa", "Please enter a valid Location Row.")
					: Res.GetString("32674ae9-757a-4523-a591-774f018ff262", "Please enter a valid Location Row for {0}.", locationConsumer.LocationTitle);
				AddValidationError(locationStringInfo, errorMessage);
			}
		}

		void ValidateFixedWidthLocationString(ILocationConsumer locationConsumer, ZPropertyInfo locationStringInfo, WhsWarehouse warehouse)
		{
			if (locationConsumer.LocationPK.IsEmpty || !locationConsumer.LocationPK.IsValid)
			{
				AddValidationError(locationStringInfo, Res.GetString("340cdea0-b338-48e9-be61-bfdca89a9703", "Please enter a valid Location."));
			}
		}

		WhsWarehouse GetWarehouse(BusinessObjectFactory factory, ZGuid locationWhsGuid)
		{
			return factory.Load<WhsWarehouse>(locationWhsGuid);
		}

		void AddValidationError(ZPropertyInfo info, string errorMessage)
		{
			if (!IsValidationSuspendedOnParent)
			{
				info.AddError(errorMessage);
			}
		}

		bool IsValidationSuspendedOnParent
		{
			get { return parent != null && parent.IsValidationSuspended; }
		}

		#endregion

		#region LookForExactMatchFirst

		WhsRow LookForExactMatchFirst(WhsRow[] rows, string rowName)
		{
			WhsRow result = null;

			// if there is an exact match then use it, otherwise use first row found
			foreach (WhsRow row in rows)
			{
				if (row.WR_Name == rowName)
				{
					result = row;
					break;
				}
			}

			if (result == null && rows.Length >= 1)
			{
				result = rows[0];
			}

			return result;
		}

		#endregion

		readonly IBusiness parent;
	}
}
