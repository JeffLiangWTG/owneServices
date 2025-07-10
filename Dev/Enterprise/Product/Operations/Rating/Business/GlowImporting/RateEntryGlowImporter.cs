using System;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateEntryGlowImporter : IGlowCustomImporter
	{
		public bool ImportChildlessChildren(BusinessObject parent, INotifications logger, int rowIndex, string[] childHeaders, ImportPreviewLineDetails[] childValues)
		{
			logger.AddWarning($"Row {rowIndex}, RateEntry: RateLines without RateLineItems are not supported.");
			return false;
		}

		public void ConvertCustomLine(BusinessObject parent, string code, INotifications logger, int rowIndex, string propertyName)
		{
			var rateEntry = parent as RateEntry ?? throw new ArgumentException("Expected a RateEntry", nameof(parent));
			var warehouse = parent.Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, code));

			if (warehouse != null)
			{
				if (rateEntry.TI_ParentID != ZGuid.Empty)
				{
					var previousSetterProperty = propertyName == "Yard" ? (NoResString)"Warehouse" : (NoResString)"Yard";
					logger.AddWarning($"A previous value has been set, for {previousSetterProperty} and has been overwritten with a {propertyName}, with code: {code}");
				}

				if (propertyName == "Yard" && warehouse.WW_WarehouseType == "CYD")
				{
					rateEntry.TI_CYC_WW_Facility = warehouse.PK;
				}
				else
				{
					rateEntry.TI_WW_Warehouse = warehouse.PK;
				}
			}
			else if (string.IsNullOrEmpty(code)) // This is for all Warehouses
			{
				rateEntry.AllWarehouses = true;
			}
			else
			{
				logger.AddError($"Row {rowIndex}, No {propertyName} exists with the code: {code}");
			}
		}

		public bool IsEmptyValueAllowed(string propertyName)
		{
			if (propertyName == "Yard" || propertyName == "Warehouse")
			{
				return true;
			}

			return false;
		}

		bool IGlowCustomImporter.ShouldCustomizeChildrenImport => true;
	}
}
