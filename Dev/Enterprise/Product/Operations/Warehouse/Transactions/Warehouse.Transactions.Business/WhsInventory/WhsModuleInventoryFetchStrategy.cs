using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsModuleInventoryFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsModuleInventoryFetchStrategy(WhsModuleInventory inventory)
			: base(inventory)
		{
		}

		#region FetchForViewCore

		// Test in InventoryFilterControl.cs
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var requireDocket = false;
			var requireStmNote = false;
			var requireLocation = false;
			var requireWhs = false;
			var requireArea = false;
			var requireClient = false;
			var requireProduct = false;
			var requirePickLine = false;
			var requireUNDG = false;
			var requireStyle = false;
			var requireColour = false;
			var requireClassification = false;
			var requireSize = false;
			var requireCustomsData = false;
			var requireCustomsTariff = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case WhsInventoryView.Schema.ReceiptReference:
						requireDocket = true;
						break;
					case WhsInventoryView.Schema.Warehouse + "+WW_WarehouseNameMultilingual":
						requireWhs = true;
						requireLocation = true;
						break;
					case WhsInventoryView.Schema.CurrentLocationClass:
					case WhsInventoryView.Schema.CurrentLocationStatus:
					case WhsInventoryView.Schema.CurrentLocationStatusCode:
					case WhsInventoryView.Schema.CurrentLocationString:
					case WhsInventoryView.Schema.CurrentLocationType:
					case WhsInventoryView.Schema.CurrentPickMethod:
					case WhsInventoryView.Schema.TouchesUntilStocktake:
					case WhsInventoryView.Schema.CurrentLocation + "+" + WhsLocation.Schema.WLV_MaxCubic:
					case WhsInventoryView.Schema.CurrentLocation + "+" + WhsLocation.Schema.WLV_MaxCubicUnit:
					case WhsInventoryView.Schema.CurrentLocation + "+" + WhsLocation.Schema.WLV_MaxWeight:
					case WhsInventoryView.Schema.CurrentLocation + "+" + WhsLocation.Schema.WLV_MaxWeightUnit:
					case WhsInventoryView.Schema.CurrentLocation + "+" + WhsLocation.Schema.WLV_MaxQuantity:
					case WhsInventoryView.Schema.CurrentLocation + "+" + nameof(WhsLocation.WLV_LastInventoryChangeDateForBinding):
						requireLocation = true;
						break;
					case WhsInventoryView.Schema.CurrentLocationPickAreaName:
					case WhsInventoryView.Schema.CurrentLocationPickAreaType:
						requireArea = true;
						requireLocation = true;
						break;
					case WhsInventoryView.Schema.WI_OH_Client:
					case WhsInventoryView.Schema.Client + "+" + OrgHeaderSchema.Constants.OH_FullName:
						requireClient = true;
						break;
					case WhsInventoryView.Schema.WI_OP_PartNum:
					case WhsInventoryView.Schema.WI_OP_Desc:
					case WhsInventoryView.Schema.CommodityCode:
					case WhsInventoryView.Schema.WI_TotalUnits:
					case WhsInventoryView.Schema.WI_LastCost:
					case WhsInventoryView.Schema.WI_TotalValue:
					case WhsInventoryView.Schema.WI_Currency:
						requireProduct = true;
						break;
					case WhsInventoryView.Schema.WI_AvailableToPickQuantity:
						requirePickLine = true;
						requireLocation = true;
						requireProduct = true;
						break;
					case WhsInventoryView.Schema.WI_CrossDockQuantity:
						requirePickLine = true;
						requireProduct = true;
						break;
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.FirstUNDG + "+" + UNDGDataItemSchema.Constants.DI_DG:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.FirstUNDG + "+Subs+" + UNDGSubstanceSchema.Constants.DG_SubLabel1:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.FirstUNDG + "+Subs+" + UNDGSubstanceSchema.Constants.DG_SubLabel2:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.FirstUNDG + "+Subs+" + UNDGSubstanceSchema.Constants.DG_PSN:
						requireProduct = true;
						requireUNDG = true;
						break;
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyle + "+" + WhsProductStyle.Schema.WST_Code:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyle + "+" + WhsProductStyle.Schema.WST_Description:
						requireProduct = true;
						requireStyle = true;
						requireColour = true;
						break;
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyleColour + "+" + WhsProductStyleColour.Schema.WSC_Code:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyleColour + "+" + WhsProductStyleColour.Schema.WSC_Description:
						requireProduct = true;
						requireColour = true;
						break;
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyleClassification + "+" + WhsProductStyleClassification.Schema.WSS_Code:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyleClassification + "+" + WhsProductStyleClassification.Schema.WSS_Description:
						requireProduct = true;
						requireClassification = true;
						break;
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyleSize + "+" + WhsProductStyleSize.Schema.WSZ_Size:
					case WhsInventoryView.Schema.Product + "+" + WhsProduct.Schema.ProductStyleSize + "+" + WhsProductStyleSize.Schema.WSZ_Sequence:
						requireProduct = true;
						requireSize = true;
						break;
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_EntryDate:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_EntryLineNo:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_DeclarationReference:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_RN_NKCountryOfOrigin:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsQty:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsUnitOfQty:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_ValueForDuty:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_TILV:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_AddInfo:
					case WhsInventoryView.Schema.CustomsData + "+" + nameof(WhsBondedWarehouseAttribute.ManufacturerCode):
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsDeadline:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_InwardStyle:
					case WhsInventoryView.Schema.CustomsData + "+" + WhsBondedWarehouseAttributeSchema.Constants.WB_InwardProcedure:
						requireCustomsData = true;
						requireDocket = true;
						break;
					case WhsInventoryView.Schema.CustomsTariffLookup:
					case WhsInventoryView.Schema.CustomsTariffItem:
					case WhsInventoryView.Schema.CustomsTariffDesc:
						requireDocket = true;
						requireCustomsTariff = true;
						break;
					case WhsInventoryView.Schema.HasEDocsOrNotesAttached:
						requireStmNote = true;
						break;
					case WhsInventoryView.Schema.InternalsProxy + "+" + WhsInventoryView.Schema.CommittedToTransactionQuantity:
						requireProduct = true;
						requirePickLine = true;
						break;
				}
			}

			if (requireDocket)
			{
				Factory.AddFetchHint(WhsDocketSchema.Constants.TableName, Inventory.WI_WD);
			}

			if (requireStmNote)
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Inventory.WI_WE_InDocketLine);
			}

			if (requireLocation)
			{
				Factory.AddFetchHint(WhsLocationViewSchema.PK, Inventory.WI_WL);
			}

			if (requireWhs)
			{
				var locationForWhsSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsLocationViewSchema.WLV_WW_Whs);
				locationForWhsSubQuery.AddToFilter(WhsLocationViewSchema.PK, Inventory.WI_WL);
				var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
				warehouseQuery.AddSubQuery(WhsWarehouseSchema.PK, locationForWhsSubQuery, JoinCondition.And);
				Factory.AddFetchHint(WhsWarehouseSchema.Instance, warehouseQuery);
			}

			if (requireArea)
			{
				var locationForAreaSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsLocationViewSchema.WLV_WA_PickingArea);
				locationForAreaSubQuery.AddToFilter(WhsLocationViewSchema.PK, Inventory.WI_WL);
				var areaQuery = new ZDBOnlyQuery(typeof(WhsArea));
				areaQuery.AddSubQuery(WhsAreaSchema.PK, locationForAreaSubQuery, JoinCondition.And);
				Factory.AddFetchHint(WhsAreaSchema.Instance, areaQuery);
			}

			if (requireClient)
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, Inventory.WI_OH_Client);
			}

			if (requireProduct)
			{
				Factory.AddFetchHint(OrgSupplierPartSchema.PK, Inventory.WI_OP);
			}

			if (requirePickLine)
			{
				Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, Inventory.WI_WE_InDocketLine);
			}

			if (requireUNDG)
			{
				Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, Inventory.WI_OP);
			}

			if (requireStyle)
			{
				AddProductStyleFetchHint();
			}

			if (requireColour)
			{
				AddProductStyleColourFetchHint();
			}

			if (requireClassification)
			{
				AddProductStyleClassificationFetchHint();
			}

			if (requireSize)
			{
				AddProductStyleSizeFetchHint();
			}

			if (requireCustomsData)
			{
				Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, Inventory.WI_WE_InDocketLine);
			}

			if (requireCustomsTariff)
			{
				Factory.AddFetchHint(CusClassPartPivotSchema.CI_OP, Inventory.WI_OP);
			}
		}

		void AddProductStyleFetchHint()
		{
			var colourPartSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour);
			colourPartSubQuery.AddToFilter(OrgSupplierPartSchema.PK, Inventory.WI_OP); // Product references ProductStyle via Style Colour

			var colourSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleColour), WhsProductStyleColourSchema.WSC_WST_ProductStyle);
			colourSubQuery.AddSubQuery(WhsProductStyleColourSchema.PK, colourPartSubQuery, JoinCondition.And);

			var styleQuery = new ZDBOnlyQuery(typeof(WhsProductStyle));
			styleQuery.AddSubQuery(WhsProductStyleSchema.PK, colourSubQuery, JoinCondition.And);

			Factory.AddFetchHint(WhsProductStyleSchema.Instance, styleQuery);
		}

		void AddProductStyleColourFetchHint()
		{
			var colourPartSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour);
			colourPartSubQuery.AddToFilter(OrgSupplierPartSchema.PK, Inventory.WI_OP);

			var colourQuery = new ZDBOnlyQuery(typeof(WhsProductStyleColour));
			colourQuery.AddSubQuery(WhsProductStyleColourSchema.PK, colourPartSubQuery, JoinCondition.And);

			Factory.AddFetchHint(WhsProductStyleColourSchema.Instance, colourQuery);
		}

		void AddProductStyleClassificationFetchHint()
		{
			var classificationPartSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification);
			classificationPartSubQuery.AddToFilter(OrgSupplierPartSchema.PK, Inventory.WI_OP);

			var classificationQuery = new ZDBOnlyQuery(typeof(WhsProductStyleClassification));
			classificationQuery.AddSubQuery(WhsProductStyleClassificationSchema.PK, classificationPartSubQuery, JoinCondition.And);

			Factory.AddFetchHint(WhsProductStyleClassificationSchema.Instance, classificationQuery);
		}

		void AddProductStyleSizeFetchHint()
		{
			var sizePartSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize);
			sizePartSubQuery.AddToFilter(OrgSupplierPartSchema.PK, Inventory.WI_OP);

			var sizeQuery = new ZDBOnlyQuery(typeof(WhsProductStyleSize));
			sizeQuery.AddSubQuery(WhsProductStyleSizeSchema.PK, sizePartSubQuery, JoinCondition.And);

			Factory.AddFetchHint(WhsProductStyleSizeSchema.Instance, sizeQuery);
		}

		#endregion

		#region Inventory

		WhsModuleInventory Inventory
		{
			get { return (WhsModuleInventory)BusinessObject; }
		}

		#endregion
	}
}
