using System;
using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.DataTransfer
{
	public class SysMergeWarehouseValueObjectDataAdapter : ValueObjectDataAdapter<WhsWarehouse, SystemMergeWarehouse>
	{
		#region ExportToValueObjectCore

		protected override void ExportToValueObjectCore(WhsWarehouse warehouse, SystemMergeWarehouse warehouseXML, IValueObjectExportContext context)
		{
			if (warehouse != null && warehouseXML != null)
			{
				ExportWarehouseDetails(warehouse, warehouseXML);
				ExportWarehouseRows(warehouse, warehouseXML);
				ExportWarehouseAreas(warehouse, warehouseXML);
			}
		}

		#region ExportWarehouseDetails

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void ExportWarehouseDetails(WhsWarehouse warehouse, SystemMergeWarehouse warehouseXML)
		{
			warehouseXML.PK = warehouse.PK.ToString();
			warehouseXML.WarehouseCode = warehouse.WW_WarehouseCode;
			warehouseXML.WarehouseName = warehouse.WW_WarehouseName;
			warehouseXML.LocationComponentDelimiter = warehouse.WW_LocationComponentDelimiter;
			warehouseXML.PackingSlipTitle = warehouse.PackingSlipTitle;
			warehouseXML.WarehouseAddressPK = warehouse.WW_OA_WarehouseAddress.ToString();
			warehouseXML.DefaultLocationType = warehouse.LocationType.WLT_Code;
			warehouseXML.DefaultInboundDockDoorPK = warehouse.WW_DefaultInboundDockDoor.ToString();
			warehouseXML.DefaultOutboundDockDoorPK = warehouse.WW_DefaultOutboundDockDoor.ToString();

			var branch = warehouse.RelatedCompanyBranch;
			if (branch != null)
			{
				warehouseXML.RelatedCompanyBranchCode = branch.GB_Code;
			}

			if (warehouse.WW_AutoPrintOrderCopyForMOPOnPick)
			{ warehouseXML.AutoPrintOrderCopyForMOPOnPick = warehouseXML.AutoPrintOrderCopyForMOPOnPickSpecified = true; }
			if (warehouse.WW_AutoPrintOrderSummaryOnPick)
			{ warehouseXML.AutoPrintOrderSummaryOnPick = warehouseXML.AutoPrintOrderSummaryOnPickSpecified = true; }
			if (warehouse.WW_AutoPrintPackingSlip)
			{ warehouseXML.AutoPrintPackingSlip = warehouseXML.AutoPrintPackingSlipSpecified = true; }
			if (warehouse.WW_AutoPrintPickingNonPickedItems)
			{ warehouseXML.AutoPrintPickingNonPickedItems = warehouseXML.AutoPrintPickingNonPickedItemsSpecified = true; }
			if (warehouse.WW_AutoPrintPickingShortfallItems)
			{ warehouseXML.AutoPrintPickingShortfallItems = warehouseXML.AutoPrintPickingShortfallItemsSpecified = true; }
			if (warehouse.WW_AutoPrintPickingSlip)
			{ warehouseXML.AutoPrintPickingSlip = warehouseXML.AutoPrintPickingSlipSpecified = true; }
			if (warehouse.WW_IsActive)
			{ warehouseXML.IsActive = warehouseXML.IsActiveSpecified = true; }
			if (warehouse.WW_IsBondedWarehouse)
			{ warehouseXML.IsBondedWarehouse = warehouseXML.IsBondedWarehouseSpecified = true; }
			if (warehouse.WW_IsVirtualWarehouse)
			{ warehouseXML.IsVirtualWarehouse = warehouseXML.IsVirtualWarehouseSpecified = true; }
			if (warehouse.WW_LocationTraysAlpha)
			{ warehouseXML.LocaitonTraysAlpha = warehouseXML.LocaitonTraysAlphaSpecified = true; }
			if (warehouse.WW_LocationColumnsAlpha)
			{ warehouseXML.LocationColumnsAlpha = warehouseXML.LocationColumnsAlphaSpecified = true; }
			if (warehouse.WW_LocationColumnsZeroBased)
			{ warehouseXML.LocationColumnsZeroBased = warehouseXML.LocationColumnsZeroBasedSpecified = true; }
			if (warehouse.WW_LocationLevelsAlpha)
			{ warehouseXML.LocationLevelsAlpha = warehouseXML.LocationLevelsAlphaSpecified = true; }
			if (warehouse.WW_LocationLevelsZeroBased)
			{ warehouseXML.LocationLevelsZeroBased = warehouseXML.LocationLevelsZeroBasedSpecified = true; }
			if (warehouse.WW_LocationsHaveLeadingZeros)
			{ warehouseXML.LocationsHaveLeadingZeros = warehouseXML.LocationsHaveLeadingZerosSpecified = true; }
			if (warehouse.WW_LocationTraysZeroBased)
			{ warehouseXML.LocationTraysZeroBased = warehouseXML.LocationTraysZeroBasedSpecified = true; }
			if (warehouse.WW_UseArrivalDateForInwardsFinalisedDate)
			{ warehouseXML.UseArrivalDateForInwardsFinalisedDate = warehouseXML.UseArrivalDateForInwardsFinalisedDateSpecified = true; }
			if (warehouse.WW_UseRequiredDateForOutwardsFinalisedDate)
			{ warehouseXML.UseRequiredDateForOutwardsFinalisedDate = warehouseXML.UseRequiredDateForOutwardsFinalisedDateSpecified = true; }

			warehouseXML.LocationColumnsFixedWidth = warehouse.WW_LocationColumnsFixedWidth;
			warehouseXML.LocationLevelsFixedWidth = warehouse.WW_LocationLevelsFixedWidth;
			warehouseXML.LocationTraysFixedWidth = warehouse.WW_LocationTraysFixedWidth;
		}

		#endregion

		#region ExportWarehouseRows

		void ExportWarehouseRows(WhsWarehouse warehouse, SystemMergeWarehouse warehouseXML)
		{
			foreach (WhsRow row in warehouse.Rows)
			{
				var rowXML = warehouseXML.Rows.AddNew();
				rowXML.PK = row.PK.ToString();
				rowXML.Name = row.WR_Name;
				rowXML.Columns = row.WR_Columns;
				rowXML.Levels = row.WR_Levels;
				rowXML.Trays = row.WR_Trays;

				ExportWarehouseLocations(rowXML, row);
			}
		}

		void ExportWarehouseLocations(SystemMergeRow rowXML, WhsRow row)
		{
			foreach (var location in row.Locations)
			{
				var locationXML = rowXML.Locations.AddNew();
				locationXML.PK = location.PK.ToString();
				locationXML.ApprovedKnownLocation = location.WLV_ApprovedKnownLocation;
				locationXML.PickingAreaPK = location.WLV_WA_PickingArea.ToString();
				locationXML.PutawayAreaPK = location.WLV_WA_PutawayArea.ToString();
				locationXML.Column = location.WLV_Column;
				locationXML.Level = location.WLV_Level;
				locationXML.Tray = location.WLV_Tray;
				locationXML.MaxDepth = location.WLV_MaxDepth;
				locationXML.MaxHeight = location.WLV_MaxHeight;
				locationXML.MaxWidth = location.WLV_MaxWidth;
				locationXML.MaxDimensionUQ = location.WLV_MaxDimensionUnit;
				locationXML.LocationStatus = location.WLV_LocationStatus;
				locationXML.LocationType = location.LocationType.WLT_Code;
				locationXML.MaxVolume = location.WLV_MaxCubic;
				locationXML.MaxVolumeUQ = location.WLV_MaxCubicUnit;
				locationXML.MaxWeight = location.WLV_MaxWeight;
				locationXML.MaxWeightUQ = location.WLV_MaxWeightUnit;
				locationXML.PalletFloorSpaces = location.WLV_PalletFloorSpaces;
				locationXML.PalletStackHeight = location.WLV_PalletStackHeight;
				locationXML.PickMethod = location.WLV_PickMethod;
			}
		}

		#endregion

		#region ExportWarehouseAreas

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void ExportWarehouseAreas(WhsWarehouse warehouse, SystemMergeWarehouse warehouseXML)
		{
			foreach (var area in warehouse.Areas)
			{
				var areaXML = warehouseXML.Areas.AddNew();
				areaXML.PK = area.PK.ToString();
				areaXML.Name = area.WA_Name;
				areaXML.AreaType = area.WA_AreaType;
			}
		}

		#endregion

		#endregion

		#region ImportFromValueObjectCore

		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context)
		{
			var errorContext = Res.GetString("af1c0024-99ad-4b95-b14e-8e4e82f5ccf6", "Warehouse: [({0}) - {1} - {2}]", xsdWarehouse.PK, xsdWarehouse.WarehouseCode, xsdWarehouse.WarehouseName);

			ImportWarehouseDetails(warehouse, xsdWarehouse, context, errorContext);
			ImportWarehouseAreas(warehouse, xsdWarehouse, context, errorContext);
			ImportWarehouseRows(warehouse, xsdWarehouse, context);
		}

		#region ImportWarehouseDetails

		void ImportWarehouseDetails(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context, string errorContext)
		{
			ValidateAndImportWarehouseAddress(warehouse, xsdWarehouse, context, errorContext);
			ValidateAndImportBranch(warehouse, xsdWarehouse, context, errorContext);
			ValidateAndImportWarehouseLocationType(warehouse, xsdWarehouse, context, errorContext);
			ValidateAndImportDefaultDockDoor(warehouse, xsdWarehouse, errorContext, true);
			ValidateAndImportDefaultDockDoor(warehouse, xsdWarehouse, errorContext, false);

			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_WarehouseCodeInfo, xsdWarehouse.WarehouseCode);
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_WarehouseNameInfo, xsdWarehouse.WarehouseName);
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationComponentDelimiterInfo, xsdWarehouse.LocationComponentDelimiter);
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_AutoPrintOrderCopyForMOPOnPickInfo, xsdWarehouse.AutoPrintOrderCopyForMOPOnPick.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_AutoPrintOrderSummaryOnPickInfo, xsdWarehouse.AutoPrintOrderSummaryOnPick.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_AutoPrintPackingSlipInfo, xsdWarehouse.AutoPrintPackingSlip.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_AutoPrintPickingNonPickedItemsInfo, xsdWarehouse.AutoPrintPickingNonPickedItems.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_AutoPrintPickingShortfallItemsInfo, xsdWarehouse.AutoPrintPickingShortfallItems.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_AutoPrintPickingSlipInfo, xsdWarehouse.AutoPrintPickingSlip.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_IsActiveInfo, xsdWarehouse.IsActive.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_IsBondedWarehouseInfo, xsdWarehouse.IsBondedWarehouse.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_IsVirtualWarehouseInfo, xsdWarehouse.IsVirtualWarehouse.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationTraysAlphaInfo, xsdWarehouse.LocaitonTraysAlpha.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationColumnsAlphaInfo, xsdWarehouse.LocationColumnsAlpha.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationColumnsZeroBasedInfo, xsdWarehouse.LocationColumnsZeroBased.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationLevelsAlphaInfo, xsdWarehouse.LocationLevelsAlpha.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationLevelsZeroBasedInfo, xsdWarehouse.LocationLevelsZeroBased.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationsHaveLeadingZerosInfo, xsdWarehouse.LocationsHaveLeadingZeros.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationTraysZeroBasedInfo, xsdWarehouse.LocationTraysZeroBased.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_UseArrivalDateForInwardsFinalisedDateInfo, xsdWarehouse.UseArrivalDateForInwardsFinalisedDate.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_UseRequiredDateForOutwardsFinalisedDateInfo, xsdWarehouse.UseRequiredDateForOutwardsFinalisedDate.ToString());

			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationColumnsFixedWidthInfo, xsdWarehouse.LocationColumnsFixedWidth.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationLevelsFixedWidthInfo, xsdWarehouse.LocationLevelsFixedWidth.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(warehouse.WW_LocationTraysFixedWidthInfo, xsdWarehouse.LocationTraysFixedWidth.ToString());
		}

		#region ValidateAndImportWarehouseAddress

		void ValidateAndImportWarehouseAddress(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context, string errorContext)
		{
			var addressPK = new ZGuid(xsdWarehouse.WarehouseAddressPK);
			var address = context.Factory.Load<OrgAddress>(addressPK);
			if (address != null)
			{
				warehouse.WW_OA_WarehouseAddress = address.PK;
			}
			else
			{
				var errorMessage = Res.GetString("f093663d-c29b-4380-9ade-a83eb24972b7", "{0}\r\nCould not find Address with PK = ({1}).\r\nPlease import it first and then retry the import operation.",
					errorContext, addressPK) + "\r\n";
				warehouse.Delete(); // delete Warehouse to not allow DataTransfer architecture to save it to DB.
				throw new ArgumentException(errorMessage);
			}
		}

		#endregion

		#region ValidateAndImportWarehouseLocationType

		void ValidateAndImportWarehouseLocationType(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context, string errorContext)
		{
			var locationTypeCode = !xsdWarehouse.DefaultLocationType.IsEmpty
				? xsdWarehouse.DefaultLocationType
				: (ZString)WhsLocationType.SystemWideDefaultLocationTypeCode;

			var locationType = context.Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, locationTypeCode));
			if (locationType != null)
			{
				warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			}
			else
			{
				var errorMessage = Res.GetString("045F459B-2A6E-4BD3-BB52-08E6D0A94F12", "{0}\r\nCould not find Location Type with Code '{1}'.\r\nPlease import or create it first and then retry the import operation.",
	errorContext, locationTypeCode) + "\r\n";
				warehouse.Delete(); // delete Warehouse to not allow DataTransfer architecture to save it to DB.
				throw new ArgumentException(errorMessage);
			}
		}

		#endregion

		#region ValidateAndImportBranch

		void ValidateAndImportBranch(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context, string errorContext)
		{
			if (!xsdWarehouse.RelatedCompanyBranchCode.IsEmpty)
			{
				var branch = warehouse.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, xsdWarehouse.RelatedCompanyBranchCode);
				if (branch != null)
				{
					warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
				}
				else
				{
					var message = Res.GetString("b5275ec5-4bf8-4574-b6a4-78bccac06291", "{0}: branch skipped. Reason: branch [{1}] doesn't exist.", errorContext, xsdWarehouse.RelatedCompanyBranchCode);
					context.Notify(new InfoNotification(message));
				}
			}
		}

		#endregion

		#region ValidateAndImportDefaultDockDoor

		void ValidateAndImportDefaultDockDoor(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, string errorContext, bool inbound)
		{
			var defaultDockDoorString = inbound ? xsdWarehouse.DefaultInboundDockDoorPK : xsdWarehouse.DefaultOutboundDockDoorPK;
			if (!defaultDockDoorString.IsEmpty)
			{
				var defaultDockDoor = new ZGuid(defaultDockDoorString);
				if (defaultDockDoor.IsValid)
				{
					var dockDoorArea = xsdWarehouse.Areas.Cast<SystemMergeArea>().Where(a => a.AreaType == AreaTypes.Codes.DockDoor);
					if (xsdWarehouse.Rows.Cast<SystemMergeRow>().SelectMany(r => r.Locations.Cast<SystemMergeLocation>())
						.Any(c => c.PK == defaultDockDoorString && dockDoorArea.Any(dd => dd.PK == c.PickingAreaPK)))
					{
						if (inbound)
						{
							warehouse.WW_DefaultInboundDockDoor = defaultDockDoor;
						}
						else
						{
							warehouse.WW_DefaultOutboundDockDoor = defaultDockDoor;
						}
					}
					else
					{
						warehouse.Delete(); // delete Warehouse to not allow DataTransfer architecture to save it to DB.
						var dockDoorType = inbound ? WhsLocationViewValidation.Inbound : WhsLocationViewValidation.Outbound;
						var errorMessage = Res.GetString("f58db6f9-e729-4e2c-b6cc-f302639f8e2d", "{0}\r\nCould not find default {1} dock door Location.\r\nPlease fix it first and then retry the import operation.", errorContext, dockDoorType) + "\r\n";
						throw new ArgumentException(errorMessage);
					}
				}
			}
		}

		#endregion

		#endregion

		#region ImportWarehouseAreas

		void ImportWarehouseAreas(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context, string errorContext)
		{
			foreach (SystemMergeArea systemMergeArea in xsdWarehouse.Areas)
			{
				var area = context.Factory.NewWithPrimaryKey<WhsArea>(new Guid(systemMergeArea.PK));
				area.WA_WW_Whs = warehouse.PK;

				context.SetPropertyInfoValueIfValueNotEmpty(area.WA_NameInfo, systemMergeArea.Name);
				context.SetPropertyInfoValueIfValueNotEmpty(area.WA_AreaTypeInfo, systemMergeArea.AreaType);
			}
		}

		#endregion

		#region ImportWarehouseRows

		void ImportWarehouseRows(WhsWarehouse warehouse, SystemMergeWarehouse xsdWarehouse, IValueObjectImportContext context)
		{
			foreach (SystemMergeRow systemMergeRow in xsdWarehouse.Rows)
			{
				var row = context.Factory.NewWithPrimaryKey<WhsRow>(new Guid(systemMergeRow.PK));
				row.WR_WW_Whs = warehouse.PK;

				context.SetPropertyInfoValueIfValueNotEmpty(row.WR_ColumnsInfo, systemMergeRow.Columns.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(row.WR_LevelsInfo, systemMergeRow.Levels.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(row.WR_TraysInfo, systemMergeRow.Trays.ToString());
				row.StorePreeditLocationSetting();

				context.SetPropertyInfoValueIfValueNotEmpty(row.WR_NameInfo, systemMergeRow.Name);
				ImportWarehouseLocations(row, systemMergeRow, context);
			}
		}

		void ImportWarehouseLocations(WhsRow row, SystemMergeRow systemMergeRow, IValueObjectImportContext context)
		{
			foreach (SystemMergeLocation systemMergeLocation in systemMergeRow.Locations)
			{
				var location = context.Factory.NewWithPrimaryKey<WhsLocation>(new Guid(systemMergeLocation.PK));
				location.WLV_WR = row.PK;
				location.WLV_WA_PickingArea = new ZGuid(systemMergeLocation.PickingAreaPK);
				location.WLV_WA_PutawayArea = new ZGuid(systemMergeLocation.PutawayAreaPK);

				var locationType = context.Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, systemMergeLocation.LocationType));
				if (locationType != null)
				{
					location.WLV_WLT_LocationType = locationType.PK;
				}

				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_ApprovedKnownLocationInfo, systemMergeLocation.ApprovedKnownLocation);
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxHeightInfo, systemMergeLocation.MaxHeight.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxWidthInfo, systemMergeLocation.MaxWidth.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxDepthInfo, systemMergeLocation.MaxDepth.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxDimensionUnitInfo, systemMergeLocation.MaxDimensionUQ.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_ColumnInfo, systemMergeLocation.Column.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_LevelInfo, systemMergeLocation.Level.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_TrayInfo, systemMergeLocation.Tray.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_LocationStatusInfo, systemMergeLocation.LocationStatus);
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxCubicInfo, systemMergeLocation.MaxVolume.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxCubicUnitInfo, systemMergeLocation.MaxVolumeUQ);
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxWeightInfo, systemMergeLocation.MaxWeight.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_MaxWeightUnitInfo, systemMergeLocation.MaxWeightUQ);
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_PalletFloorSpacesInfo, systemMergeLocation.PalletFloorSpaces.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_PalletStackHeightInfo, systemMergeLocation.PalletStackHeight.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_PickMethodInfo, systemMergeLocation.PickMethod);
				context.SetPropertyInfoValueIfValueNotEmpty(location.WLV_PutawayPathSequenceInfo, "1");
			}
		}

		#endregion

		#endregion

		#region FindBusinessObject

		protected override WhsWarehouse FindBusinessObject(SystemMergeWarehouse value, IValueObjectImportContext context)
		{
			return context.Factory.Load<WhsWarehouse>(new Guid(value.PK));
		}

		#endregion

		#region NewBusinessObject

		protected override WhsWarehouse NewBusinessObject(SystemMergeWarehouse systemMergeWarehouse, IValueObjectImportContext context)
		{
			return context.Factory.NewWithPrimaryKey<WhsWarehouse>(new Guid(systemMergeWarehouse.PK));
		}

		#endregion

		#region ConfirmUpdateOfExistingBusinessObject

		protected override bool ConfirmUpdateOfExistingBusinessObject(WhsWarehouse warehouse, INotifications notifications)
		{
			return false;
		}

		#endregion

		#region OnUserDeclinedImport

		protected override void OnUserDeclinedImport(WhsWarehouse warehouse, SystemMergeWarehouse systemMergeWarehouse, IValueObjectImportContext context)
		{
			string message = Res.GetString("a54c3568-b68d-4b2b-a816-633619b1e712", "Import of warehouse [({0}) - {1} - {2}] skipped. Reason: Warehouse already exists.", warehouse.PK, warehouse.WW_WarehouseCode, warehouse.WW_WarehouseNameMultilingual) + "\r\n";
			context.Notify(new InfoNotification(message));
		}

		#endregion

		#region NotifyBizObjCreatedOrUpdated

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		#endregion

		#endregion

		#region Overrides

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override string RootCollectionElementName
		{
			get { return "SystemMergeWarehouses"; }
		}

		public override string RootElementName
		{
			get { return "SystemMergeWarehouse"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		#endregion
	}
}
