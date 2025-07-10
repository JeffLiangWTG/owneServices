using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsReceiveLineDataObjectReader : WhsDocketLineDataObjectReader<WhsReceive, WhsReceiveLine>
	{
		internal WhsReceiveLineDataObjectReader(OrderLine inventoryLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsReceive parent, IEnumerable<WhsReceiveLine> matchedLines, ExtraOrderLineDetails extraOrderLineDetails = null)
			: base(inventoryLineDataObject, logger, factory, parent)
		{
			MatchedLines = matchedLines;
			ExtraOrderLineDetails = extraOrderLineDetails;
		}

		protected override bool IsDataSourceCustoms => (CustomsHelper != null && CustomsHelper.IsDataSourceCustoms);
		readonly IEnumerable<WhsReceiveLine> MatchedLines;
		readonly ExtraOrderLineDetails ExtraOrderLineDetails;

		#region Matching

		protected override WhsReceiveLine GetExistingBusinessObject()
		{
			WhsReceiveLine line;
			if (Parent.IsInDatabase)
			{
				if (IsDataSourceCustoms)
				{
					line = ObjectFactory.Get<IWhsReceiveLineMatcher>().FindExistingBizOForCustoms(Parent, dataObject, GetProduct(), MatchedLines);
				}
				else
				{
					line = ObjectFactory.Get<IWhsReceiveLineMatcher>().FindExistingBizOForNonCustoms(Parent, dataObject, MatchedLines);
				}
			}
			else
			{
				line = null;
			}
			return line;
		}

		protected override WhsReceiveLine GetNewBusinessObject()
		{
			return Parent.Lines.AddNew();
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObjectCore(WhsReceiveLine receiveLine)
		{
			if (receiveLine.IsInDatabase && (Parent.StartedReceiving || receiveLine.HasPutawayTransfer))
			{
				var message = Res.GetString("f3d35ee8-c24f-4492-9f4c-56394579d3c9", "Cannot update Receive Line after receiving started.");
				throw new DataObjectReadFailureException(message);
			}
			PopulateOrgs(receiveLine);

			if (receiveLine.Inventory.Count == 0)
			{
				receiveLine.ClearInventoryCache();
			}

			PopulateAttributes(receiveLine);

			SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_PalletID, dataObject.PalletID);

			if (dataObject.PackageQtyUnit != null && dataObject.PackageQtyUnit.Code.HasValue && !dataObject.PackageQtyUnit.Code.Value.IsEmpty)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_F3_NKPackType, receiveLine.Lookups.PackTypes, dataObject.PackageQtyUnit);
			}

			SetValue(receiveLine, WhsDocketLineSchema.WE_AllocationKey, ExtraOrderLineDetails?.AllocationKey ?? ZString.Empty);

			PopulateExpectedAndActualQty(receiveLine);
			PopulateLocation(receiveLine);

			SetValue(receiveLine, WhsDocketLineSchema.WE_LineNo, dataObject.LineNumber);
			SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_ReceiveCrossDockOrderNo, dataObject.CrossDockOrderNumber);

			PopulateRequiredBy(receiveLine);
			PopulateHoldCode(receiveLine);

			SetValue(receiveLine, WhsDocketLineSchema.WE_SubLineNo, dataObject.SubLineNumber);
			PopulatePackageGroupID(receiveLine);

			PopulateExtraClassificationDetails(receiveLine);
			PopulateExtraCustomsDetails(receiveLine);
			PopulateStockOnHand(receiveLine);
			CreateASNLine(receiveLine);
		}

		#region GetProductCode

		protected override Product GetProduct()
		{
			Product result;

			if (IsWarehouseBondedChangeOfOwnership)
			{
				result = new Product { Code = ExtraOrderLineDetails?.NewProductCode.GetValueOrDefault().ToUpper() ?? "" };
			}
			else
			{
				result = base.GetProduct();
			}

			return result;
		}

		#endregion

		#region PopulateLocation

		void PopulateLocation(WhsReceiveLine line)
		{
			if (IsWarehouseBondedChangeOfInventory)
			{
				if (!IsChangeOfWarehouse && !(IsWarehouseBondedChangeOfRegime && IntoRegimeType != OutOfRegimeType))
				{
					var location = dataObject.Location;
					if (location != null)
					{
						var row = Parent.Warehouse.Rows.Find(new ZQuery(WhsRowSchema.WR_Name, location.Row.GetValueOrDefault())).FirstOrDefault();
						if (row != null)
						{
							var locationBO = row.Locations.FindByColumnLevelTray(location.Column.GetValueOrDefault(), location.Level.GetValueOrDefault(), location.Tray.GetValueOrDefault());
							if (locationBO != null)
							{
								line.WE_WL = locationBO.PK;
							}
						}
					}
				}
			}
		}

		#endregion

		#region PopulateHoldCode

		void PopulateHoldCode(WhsReceiveLine receiveLine)
		{
			if (!receiveLine.IsFinalised)
			{
				if (dataObject.OriginalHoldCode != null && dataObject.OriginalHoldCode.Code.HasValue)
				{
					var holdCode = dataObject.OriginalHoldCode.Code.Value;
					SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_WHC_NKOriginalInventoryHeldCode, holdCode);

					if (!receiveLine.Lookups.InventoryHeldCodeCollection.ContainsCode(holdCode))
					{
						var message = Res.GetString("ddb4305e-bff8-48a0-9554-d26e79c801f5", "Imported Hold Code '{0}' is not valid on this system.", holdCode);
						logger.Log(LogType.Warning, message);
					}
				}

				SetValue(receiveLine, WhsDocketLineSchema.WE_CurrentHoldReason, dataObject.CurrentHoldReason);
			}
		}

		#endregion

		#region PopulateExtraClassificationDetails

		void PopulateExtraClassificationDetails(WhsReceiveLine receiveLine)
		{
			if (receiveLine.IsCustomsTransaction)
			{
				if (ExtraOrderLineDetails?.ExtraClassificationDetails != null)
				{
					foreach (var extraClassificationDetail in ExtraOrderLineDetails.ExtraClassificationDetails)
					{
						var addInfo = factory.BOFactory.New<Integration.IWarehouseCustomsAddInfo>();
						addInfo.B7_AddInfoData = extraClassificationDetail;
						addInfo.B7_ParentID = receiveLine.PK;
						addInfo.B7_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
					}
				}
			}
		}

		#endregion

		#region PopulateExtraCustomsDetails

		void PopulateExtraCustomsDetails(WhsReceiveLine receiveLine)
		{
			if (receiveLine.IsCustomsTransaction)
			{
				var extraCustomsDetails = ExtraOrderLineDetails?.ExtraCustomsDetails;
				if (extraCustomsDetails != null)
				{
					var customsData = receiveLine.CustomsData;
					var customsDataPK = customsData.PK;
					var customsDataTablePrefix = customsData.TablePrefix;
					var query = new ZQuery(CusAddInfoSchema.B7_ParentID, customsDataPK);
					query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, customsDataTablePrefix);
					query.FetchOnlyFromLocalCache = !customsData.IsInDatabase;
					if (extraCustomsDetails.Any())
					{
						query.AddToFilter(CusAddInfoSchema.B7_Type, extraCustomsDetails.Select(x => x.Type).Distinct());
					}
					((BusinessObject[])customsData.Factory.Load<Integration.IWarehouseCustomsAttributeAddInfo>(query)).DeleteAll();
					foreach (var extraCustomsDetail in extraCustomsDetails)
					{
						var addInfo = factory.BOFactory.New<Integration.IWarehouseCustomsAttributeAddInfo>();
						addInfo.B7_ParentID = customsDataPK;
						addInfo.B7_ParentTableCode = customsDataTablePrefix;
						addInfo.B7_Type = extraCustomsDetail.Type;
						addInfo.B7_AddInfoData = extraCustomsDetail.Data;
					}
				}
			}
		}

		#endregion

		#region PopulateOrgs

		void PopulateOrgs(WhsReceiveLine receiveLine)
		{
			if (dataObject.Consignee != null)
			{
				new OrganisationDataObjectReader(dataObject.Consignee, logger, factory).GetMatchedOrNew(receiveLine);
			}
		}

		#endregion

		#region PopulateAttributes

		void PopulateAttributes(WhsReceiveLine receiveLine)
		{
			SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_ExpiryDate, dataObject.ExpiryDate);
			SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_PackingDate, dataObject.PackingDate);
			if (receiveLine.SupplierPart.IsInDatabase)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_PartAttrib1, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewPartAttribute1 : dataObject.PartAttribute1);
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_PartAttrib2, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewPartAttribute2 : dataObject.PartAttribute2);
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_PartAttrib3, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewPartAttribute3 : dataObject.PartAttribute3);
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_SerialNumber, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewSerialNumber : dataObject.SerialNumber);
			}
			else
			{
				SetValue(receiveLine, WhsDocketLineSchema.WE_PartAttrib1, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewPartAttribute1 : dataObject.PartAttribute1);
				SetValue(receiveLine, WhsDocketLineSchema.WE_PartAttrib2, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewPartAttribute2 : dataObject.PartAttribute2);
				SetValue(receiveLine, WhsDocketLineSchema.WE_PartAttrib3, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewPartAttribute3 : dataObject.PartAttribute3);
				SetValue(receiveLine, WhsDocketLineSchema.WE_SerialNumber, IsWarehouseBondedChangeOfOwnership ? ExtraOrderLineDetails?.NewSerialNumber : dataObject.SerialNumber);
			}
		}

		#endregion

		#region PopulateExpectedAndActualQty

		void PopulateExpectedAndActualQty(WhsReceiveLine receiveLine)
		{
			var transactionQty = GetWE_TransactionQuantity(receiveLine).GetValueOrDefault();
			var expectedQty = dataObject.ExpectedQuantity.GetValueOrDefault();

			if (expectedQty > 0 && transactionQty > 0 && transactionQty != expectedQty)
			{
				var message = Res.GetString("5c63e681-7dfb-41d5-850c-c6c308d29267", "Cannot import Receive Line if Ordered Qty, Expected Quantity or Package Qty (when converted into appropriate units) have a value greater than 0 that are not equal.");
				throw new DataObjectReadFailureException(message);
			}

			var unitsToSetTo = Math.Max(expectedQty, transactionQty);
			receiveLine.WE_ClientOrderedUnits = unitsToSetTo;
			if (ShouldClearTransactionQuantity(receiveLine))
			{
				SetValue(receiveLine, WhsDocketLineSchema.WE_TransactionQuantity, 0);
			}
			else
			{
				if (unitsToSetTo < 0)
				{
					throw new DataObjectReadFailureException(Res.GetString("69ab0f1c-6026-4092-a6ba-e1721fda460f", "Cannot Import Receive Line {0}:\r\nQuantity cannot be negative.", dataObject.LineNumber.GetValueOrDefault()));
				}
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_TransactionQuantity, unitsToSetTo);
			}
		}

		#endregion

		#region PopulateRequiredBy

		void PopulateRequiredBy(WhsReceiveLine receiveLine)
		{
			if ((dataObject.RequiredBy.HasValue && dataObject.RequiredBy.Value.IsValid) || Shipment == null || Shipment.LocalProcessing == null)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_RequiredByDate, dataObject.RequiredBy);
			}
			else
			{
				var dataObjectHelper = new WhsDataObjectReaderHelper(receiveLine.Warehouse);
				SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_RequiredByDate, dataObjectHelper.ConvertToZDateTimeOffset(Shipment.LocalProcessing.DeliveryRequiredBy));
			}
		}

		#endregion

		#region PopulateCustomsData

		protected override void PopulateCustomsDataCore(WhsReceiveLine line)
		{
			base.PopulateCustomsDataCore(line);

			var warehouseAttributeDataObjectReader = new WhsBondedWarehouseAttributeDataObjectReader(logger);
			if (IsWarehouseBondedChangeOfInventory)
			{
				warehouseAttributeDataObjectReader.ReadMainAndSecondaryInwardsProcessedItemIntoBusinessObject(dataObject.CustomsData, line.CustomsData);
			}
			warehouseAttributeDataObjectReader.CalculateTotalLiabilityAmount(line.CustomsData, Parent.CountryCode, GetArrivalDate(line), factory.BOFactory);
		}

		ZDateTime GetArrivalDate(WhsReceiveLine line)
		{
			var result = ZDateTime.Empty;
			if (!line.WE_AdjustmentArrivalDate.IsEmpty)
			{
				result = line.WE_AdjustmentArrivalDate.ToZDateTime();
			}
			else if (!Parent.WD_ArrivalDate.IsEmpty)
			{
				result = Parent.WD_ArrivalDate.ToDateTime();
			}
			return result;
		}

		protected override void SetBondedEntryKey(WhsReceiveLine line)
		{
			var bondedEntryKey = dataObject.CustomsData?.GetFormattedCustomsEntryKeyWithLineNo();

			if (string.IsNullOrWhiteSpace(bondedEntryKey) && IsWarehouseBondedChangeOfRegime)
			{
				var message = Res.GetString("5e4f3d13-415b-48c3-b8e2-a93f222dd973", "Bonded Entry Key must be provided for Change of Regime import.");
				throw new DataObjectReadFailureException(message);
			}

			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_BondedEntryKey, bondedEntryKey);
		}

		#endregion

		#region PopulatePackageGroupID

		void PopulatePackageGroupID(WhsReceiveLine receiveLine)
		{
			SetValueWhenNotReadOnlyOrCustomsTransaction(receiveLine, WhsDocketLineSchema.WE_PerPackageQty, dataObject.PerPackageQty);
			if (!string.Equals(dataObject.PackageGroupId, receiveLine.WE_PackageGroupId, StringComparison.OrdinalIgnoreCase))
			{
				Parent.AddReceiveLineToGeneratePackageGroupId(receiveLine, dataObject.PackageGroupId);
			}
		}

		#endregion

		#region PopulateProduct

		protected override void SetProduct(WhsReceiveLine docketLine, OrgSupplierPart product)
		{
			var warehouse = Parent.Warehouse;
			if (warehouse != null && warehouse.WW_IsVirtualWarehouse && docketLine.IsCustomsTransaction)
			{
				var whsProduct = WhsProduct.GetWhsProduct(factory.BOFactory, product.PK);
				if (whsProduct != null && whsProduct.IsAnyPartAttribReleaseCaptured(docketLine.Docket.Client))
				{
					var message = Res.GetString("d5ca86bb-3f3b-47c3-b4a8-e2f2dd752ea0", "Product: {0} cannot be received as Bonded receive cannot be processed in a virtual warehouse: {1} with release captured attributes.", product.HumanReadableName, warehouse.HumanReadableName);
					throw new DataObjectReadFailureException(message);
				}
			}
			SetValue(docketLine, WhsDocketLineSchema.WE_OP, product.PK);
		}

		protected override string DocketLineType => WhsReceiveDataObjectReader.ReceiveDocketType;

		#endregion

		#region PopulateStockOnHand

		void PopulateStockOnHand(WhsReceiveLine receiveLine)
		{
			// tested in WhsReceiveDataObjectReaderTest
			//	(Test_CustomsSource_ImportOfExistingFinalisedJobWithStockIncreaseInVirtualWhsAppliesChanges_EndToEnd)
			//	(Test_CustomsSource_ImportOfExistingFinalisedJobWithStockDecreaseInVirtualWhsAppliesChanges_EndToEnd)
			//	(Test_CustomsSource_ImportOfExistingFinalisedJobWithStockDecreasedNotBelowAvailableInVirtualWhsShouldNotFail_EndToEnd)
			if (ShouldClearTransactionQuantity(receiveLine))
			{
				SetValue(receiveLine, WhsDocketLineSchema.WE_StockOnHand, 0);
			}
			else
			{
				var unitsToSetTo = receiveLine.WE_TransactionQuantity;
				var originalValue = (ZDecimal)receiveLine.WE_TransactionQuantityInfo.OriginalValue;
				var originalTotalUnits = (ZDecimal)receiveLine.WE_StockOnHandInfo.OriginalValue;
				SetValue(receiveLine, WhsDocketLineSchema.WE_StockOnHand, originalTotalUnits + (unitsToSetTo - originalValue));
			}
		}

		#endregion

		#region CreateASNLine

		void CreateASNLine(WhsReceiveLine receiveLine)
		{
			if (Parent.StartedReceiving)
			{
				var newAsnLine = Parent.AsnLines.AddNew();
				newAsnLine.WN_OP = receiveLine.WE_OP;
				newAsnLine.WN_PartAttrib1 = receiveLine.WE_PartAttrib1;
				newAsnLine.WN_PartAttrib2 = receiveLine.WE_PartAttrib2;
				newAsnLine.WN_PartAttrib3 = receiveLine.WE_PartAttrib3;
				newAsnLine.WN_SerialNumber = receiveLine.WE_SerialNumber;
				newAsnLine.WN_PackingDate = receiveLine.WE_PackingDate;
				newAsnLine.WN_ExpiryDate = receiveLine.WE_ExpiryDate;
				newAsnLine.WN_PalletId = receiveLine.WE_PalletID;
				newAsnLine.WN_Quantity = receiveLine.WE_ClientOrderedUnits;
				newAsnLine.WN_QuantityUQ = receiveLine.ProductUQ;
				newAsnLine.WN_LineNo = receiveLine.WE_LineNo;
				newAsnLine.WN_SubLineNo = receiveLine.WE_SubLineNo;
				newAsnLine.WN_AddedAfterReceiveStarted = true;
			}
		}

		#endregion

		#endregion

		#region IsWarehouseBondedChangeOfOwnership

		bool IsWarehouseBondedChangeOfOwnership
		{
			get { return CustomsHelper?.IsWarehouseBondedChangeOfOwnership ?? false; }
		}

		#endregion

		#region IsWarehouseBondedChangeOfRegime

		bool IsWarehouseBondedChangeOfRegime
		{
			get { return CustomsHelper?.IsWarehouseBondedChangeOfRegime ?? false; }
		}

		CustomsRegime IntoRegimeType => Shipment.GetWarehouseCustomsDetailsChangeOfRegime(logger.TopLevelDataContext).IntoRegimeType;

		CustomsRegime OutOfRegimeType => Shipment.GetWarehouseCustomsDetailsChangeOfRegime(logger.TopLevelDataContext).OutOfRegimeType;

		#endregion

		#region IsChangeOfWarehouse

		bool IsChangeOfWarehouse
		{
			get
			{
				var isChangeOfWarehouse = false;

				if (IsWarehouseBondedChangeOfOwnership)
				{
					isChangeOfWarehouse = Shipment.GetWarehouseCustomsDetailsChangeOfOwnership(logger.TopLevelDataContext).NewWarehouse != null;
				}
				else if (IsWarehouseBondedChangeOfRegime)
				{
					isChangeOfWarehouse = Shipment.GetWarehouseCustomsDetailsChangeOfRegime(logger.TopLevelDataContext).NewWarehouse != null;
				}

				return isChangeOfWarehouse;
			}
		}

		#endregion

		#region IsWarehouseBondedChangeOfInventory

		bool IsWarehouseBondedChangeOfInventory => IsWarehouseBondedChangeOfOwnership || IsWarehouseBondedChangeOfRegime;

		#endregion

		#region ShouldClearTransactionQuantity

		bool ShouldClearTransactionQuantity(WhsReceiveLine receiveLine)
			=> !receiveLine.IsInDatabase
			&& factory.GetCachedValue("WhsReceiveLineDataObjectReader|ShouldClearTransactionQuantity|" + Parent.WD_DocketID, Parent.HasBeenLoadedInRF);

		#endregion

		#region ClientDescriptionForErrorMessage

		protected override ZString ClientDescriptionForErrorMessage => CustomsHelper?.IsWarehouseBondedChangeOfOwnership ?? false ? (ZString)Res.GetString("WhsReceiveLineDataObjectReader|ChangeOfOwnership|OwnerDescription", "New Owner") : base.ClientDescriptionForErrorMessage;

		#endregion

		#region Shipment

		Shipment Shipment
		{
			get
			{
				var shipment = (Shipment)logger?.TopLevelDataObject;
				return shipment != null ? Shipment.GetSourceDataObject(shipment) : shipment;
			}
		}

		#endregion

		#region CustomsHelper

		CustomsDataSourceHelper<WhsReceive> CustomsHelper
		{
			get
			{
				if (customsHelper == null && Shipment != null && logger?.TopLevelDataContext != null)
				{
					customsHelper = new CustomsDataSourceHelperForReceive(Shipment, logger.TopLevelDataContext);
				}

				return customsHelper;
			}
		}

		CustomsDataSourceHelper<WhsReceive> customsHelper;

		#endregion
	}
}
