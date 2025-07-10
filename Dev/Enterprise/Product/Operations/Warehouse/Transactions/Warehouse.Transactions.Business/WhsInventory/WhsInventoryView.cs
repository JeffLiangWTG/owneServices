using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[RowFetchStrategy(FetchStrategyType = typeof(WhsInventoryViewTypeFetchStrategy))]
	[CodeProperty(WhsInventoryView.Schema.Code), DescriptionProperty(WhsInventoryView.Schema.Description)]
	[UniversalCopyIgnoreElement("Docket")]
	public class WhsInventoryView :
		AutoWhsInventoryView,
		IWhsInventoryView,
		IWhsInventoryInternals,
		ILocationConsumer,
		IDocManagerSupport,
		IParentDocManagerSupport,
		ILineAttributes,
		ILineCustomAttributes,
		ICodeDescription,
		IPartAttributeValidationConsumer,
		IDocumentSupportable,
		ICustomLabelsConfigOrgProvider,
		IReservableInventory,
		ISupportTemporaryProduct,
		IExternalValidationMessages,
		ICanPerformReceiveActions,
		ICalculateProductPackageTotals
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventOverReduceStockViaInventoryLineMessageID = "Attempt to over-reduce TotalUnits.";

		public void DeleteForDataRefreshFromView()
		{
			DeleteForDataRefresh();
		}

		#region Constructors

		public WhsInventoryView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		protected override bool IsForcedPublish
		{
			get
			{
				return true;
			}
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoWhsInventoryView.Schema
		{
			public const string WI_ClientUQ = "WI_ClientUQ";
			public const string WI_ExpectedReceiptQuantity = "WI_ExpectedReceiptQuantity";
			public const string WI_ReceiveCrossDockOrderNo = "WI_ReceiveCrossDockOrderNo";
			public const string ConsigneeNameOrPK = "ConsigneeNameOrPK";
			public const string ConsigneeFieldType = "ConsigneeFieldType";
			public const string WI_CrossDockQuantity = "WI_CrossDockQuantity";
			public const string WI_AvailableForCrossDockQuantity = "WI_AvailableForCrossDockQuantity";
			public const string CommittedToTransactionQuantity = "CommittedToTransactionQuantity";
			public const string WI_AvailableToPickQuantity = "WI_AvailableToPickQuantity";
			public const string WI_SplitQuantity = "WI_SplitQuantity";
			public const string WI_UnitsUQ = "WI_UnitsUQ";
			public const string WI_ArrivalDateOrETA = "WI_ArrivalDateOrETA";
			public const string Warehouse = "Warehouse";
			public const string Client = "Client";
			public const string WI_LineNo = "WI_LineNo";
			public const string WI_SubLineNo = "WI_SubLineNo";
			public const string Details = "Details";
			public const string CustomsData = "CustomsData";
			public const string LocationString = "LocationString";
			public const string LocationStatus = "LocationStatus";
			public const string LocationClass = "LocationClass";
			public const string Code = "Code";
			public const string Description = "Description";
			public const string WI_OP_PartNum = "WI_OP_PartNum";
			public const string WI_OP_Desc = "WI_OP_Desc";
			public const string WI_CustomAttrib1 = "WI_CustomAttrib_1";
			public const string WI_CustomAttrib2 = "WI_CustomAttrib_2";
			public const string WI_CustomAttrib3 = "WI_CustomAttrib_3";
			public const string WI_CustomAttrib4 = "WI_CustomAttrib4";
			public const string WI_CustomAttrib5 = "WI_CustomAttrib5";
			public const string WI_CustomAttrib6 = "WI_CustomAttrib6";
			public const string WI_CustomDate1 = "WI_CustomDate1";
			public const string WI_CustomDate2 = "WI_CustomDate2";
			public const string WI_CustomDate3 = "WI_CustomDate3";
			public const string WI_CustomDate4 = "WI_CustomDate4";
			public const string WI_CustomDate5 = "WI_CustomDate5";
			public const string WI_CustomDecimal1 = "WI_CustomDecimal1";
			public const string WI_CustomDecimal2 = "WI_CustomDecimal2";
			public const string WI_CustomDecimal3 = "WI_CustomDecimal3";
			public const string WI_CustomDecimal4 = "WI_CustomDecimal4";
			public const string WI_CustomDecimal5 = "WI_CustomDecimal5";
			public const string WI_CustomFlag1 = "WI_CustomFlag1";
			public const string WI_CustomFlag2 = "WI_CustomFlag2";
			public const string WI_CustomFlag3 = "WI_CustomFlag3";
			public const string WI_CustomFlag4 = "WI_CustomFlag4";
			public const string WI_CustomFlag5 = "WI_CustomFlag5";
			public const string WI_CustomTextBlob1 = "WI_CustomTextBlob1";
			public const string WI_TotalValue = "WI_TotalValue";
			public const string WI_Currency = "WI_Currency";
			public const string WI_LastCost = "WI_LastCost";
			public const string CommodityCode = "CommodityCode";
			public const string LocationPickAreaName = "LocationPickAreaName";
			public const string LocationPickAreaType = "LocationPickAreaType";
			public const string CurrentLocationPickAreaName = "CurrentLocationPickAreaName";
			public const string CurrentLocationPickAreaType = "CurrentLocationPickAreaType";
			public const string PackageGroupId = "PackageGroupId";
			public const string PerPackageQty = "PerPackageQty";
			public const string PutawayTransferLine = "PutawayTransferLine";
			public const string OriginalInventoryStatus = "OriginalInventoryStatus";
			public const string OriginalInventoryHeldCode = "OriginalInventoryHeldCode";
			public const string OriginalHeldCode = "OriginalHeldCode";
			public const string CurrentHeldCode = "CurrentHeldCode";
			public const string CurrentLocation = "CurrentLocation";
			public const string CurrentLocationType = "CurrentLocationType";
			public const string CurrentLocationStatusCode = "CurrentLocationStatusCode";
			public const string CurrentLocationStatus = "CurrentLocationStatus";
			public const string CurrentLocationString = "CurrentLocationString";
			public const string CurrentLocationClass = "CurrentLocationClass";
			public const string CurrentPickMethod = "CurrentPickMethod";
			public const string CustomsTariffLookup = "CustomsTariffLookup";
			public const string CustomsTariffItem = "CustomsTariffItem";
			public const string CustomsTariffDesc = "CustomsTariffDesc";
			public const string HasEDocsOrNotesAttached = "HasEDocsOrNotesAttached";
			public const string TouchesUntilStocktake = "TouchesUntilStocktake";
			public const string InternalsProxy = "InternalsProxy";
			public const string InDocketLine = "InDocketLine";
			public const string Product = "Product";
			public const string ReceiptReference = "ReceiptReference";
			public const string IsExpired = nameof(WhsInventoryView.IsExpired);
		}

		#endregion

		#region Customs Stuff

		public bool IsCustomsDataActive
		{
			get { return customsData != null && !customsData.IsDeleted; }
		}

		public WhsBondedWarehouseAttribute CustomsData
		{
			get
			{
				if (!IsDeleted && (customsData == null || customsData.IsDeleted))
				{
					customsData = (InDocketLine == null ? Factory.New<WhsBondedWarehouseAttribute>() : InDocketLine.CustomsData);
				}
				return customsData;
			}
		}

		WhsBondedWarehouseAttribute customsData;

		BusinessObject[] CustomsClassificationPivots
		{
			get
			{
				ZQuery query = new ZQuery(CusClassPartPivotSchema.CI_OP, WI_OP);
				return (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.IBaseCusClassPartPivot>(query);
			}
		}

		BusinessObject CustomsClassification
		{
			get
			{
				BusinessObject result = null;
				ZString typeCode = "IMP";

				if (CustomsClassificationPivots.Length > 0)
				{
					foreach (BusinessObject pivot in CustomsClassificationPivots)
					{
						BusinessObject classification = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseCusClassification>((ZGuid)pivot[CusClassPartPivotSchema.CI_CC.Name]);
						if (classification != null && (ZString)classification[CusClassificationSchema.CC_ClassificationType] == typeCode)
						{
							result = classification;
							break;
						}
					}
				}

				return result;
			}
		}

		public ZString CustomsTariffLookup
		{
			get
			{
				BusinessObject classification = CustomsClassification;
				return classification == null ? ZString.Empty : (ZString)classification[CusClassificationSchema.CC_LookupCode.Name];
			}
		}

		public ZString CustomsTariffItem
		{
			get
			{
				BusinessObject classification = CustomsClassification;
				return classification == null ? ZString.Empty : (ZString)classification[CusClassificationSchema.CC_TariffNum.Name];
			}
		}

		public ZString CustomsTariffDesc
		{
			get
			{
				BusinessObject classification = CustomsClassification;
				return classification == null ? ZString.Empty : (ZString)classification[CusClassificationSchema.CC_Description.Name];
			}
		}

		public ZPropertyInfo CustomsTariffLookupInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsTariffLookup)); }
		}

		public ZPropertyInfo CustomsTariffItemInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsTariffItem)); }
		}

		public ZPropertyInfo CustomsTariffDescInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsTariffDesc)); }
		}

		#region BondInfo

		#region BondedInfo Class

		protected BondedInfo BondInfo
		{
			get
			{
				if (bondInfo == null)
				{
					bondInfo = new BondedInfo(Factory, WI_BondedEntryKey);
				}
				return bondInfo;
			}
		}

		BondedInfo bondInfo;

		protected class BondedInfo
		{
			#region Constructors

			public BondedInfo(BusinessObjectFactory factory, string bondedEntryKey)
			{
				var parser = new EntryLineCodeParser(bondedEntryKey);
				calculator = new WhsBondedCalculator(factory, parser.EntryNumber, parser.LineNumber);
			}

			#endregion

			#region AvailableBondedWhsQty

			public ZDecimal AvailableBondedWhsQty
			{
				get { return calculator.GetAvailableBondedWhsQty(); }
			}

			#endregion

			#region Implementation

			readonly WhsBondedCalculator calculator;

			#endregion
		}

		#endregion

		#region AvailableBondedWhsQty

		public ZDecimal BondedInfo_AvailableBondedWhsQty
		{
			get { return BondInfo.AvailableBondedWhsQty; }
		}

		public ZPropertyInfo BondedInfo_AvailableBondedWhsQtyInfo
		{
			get { return GetZPropertyInfo(nameof(BondedInfo_AvailableBondedWhsQty)); }
		}

		#endregion

		#endregion

		#endregion

		#region Business Object Overrides

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = base.BusinessObjectsWithRelatedEventsCore;

				var docketLine = InDocketLine;
				if (docketLine != null)
				{
					result = result.Append(docketLine).ToArray();
				}

				return result;
			}
		}

		#endregion

		#region IsInDatabase

		public override bool IsInDatabase
		{
			get
			{
				// WhsInventoryView is not persisted. Check IsInDatabase for docket line if you really need to.
				return false;
			}
		}

		#endregion

		#region GetLogsParentTableName

		protected override string GetLogsParentTableName() => WhsDocketLineSchema.Constants.TableName;

		#endregion

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			// These previously got set by side effect of the view sharing column names with WhsInventory table. Now that it's removed we set these manually for identical behaviour.
			var row = ((IBusinessObjectInternals)this).Row;
			row[WhsInventoryViewSchema.Constants.WI_InventoryStatus] = "PND";
			row[WhsInventoryViewSchema.Constants.WI_IsOriginalReceiptLine] = true;
			//

			OriginalInventoryStatus = InventoryStatus.Codes.Pending;
		}

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get
			{
				var docketLine = InDocketLine;
				var docket = Docket;
				if (docketLine != null && docket != null)
				{
					if ((docketLine as WhsReceiveLine != null) && !docketLine.IsDeleted && (docket == null || !docket.IsFinalisedOrCancelled))
					{
						return false;
					}
				}
				return base.CanDelete;
			}
		}

		public override void Delete()
		{
			// Tracking will call delete twice because it has more than 1 BizO for the same DataRow.
			var docket = (!IsDeleted) ? Docket : null;
			var docketLine = (!IsDeleted) ? InDocketLine : null;
			var part = (!IsDeleted) ? SupplierPart : null;
			var inventoryQuantity = (!IsDeleted) ? WI_InDocketLineUnits : ZDecimal.Zero;
			var inventoryLineNo = (!IsDeleted) ? WI_LineNo : ZShort.Zero;

			if (docketLine != null)
			{
				ReservedPickLines.DeleteAll();
			}

			base.Delete();

			// Docket Line will automatically update the Changes cache on delete. However in the case that Inventory has the Product set,
			// but the Docket Line does not, then we should still update the Changes cache.
			if (docket != null && part != null && (docketLine == null || (!docketLine.IsDeleted && docketLine.SupplierPart == null)))
			{
				docket.UpdateChangesCacheOnRemove(part, inventoryQuantity, inventoryLineNo);
			}

			// docketline won't exist until runpresavevalidation is invoked - yuck
			if (docketLine != null && !docketLine.DocketLineDeletionSemaphore.IsSuspended)
			{
				// Special case since in ReceiveEntryForm we can see only Inventories and never deal with WhsReceiveLine directly.
				if ((docketLine as WhsReceiveLine != null) && !docketLine.IsDeleted && (docket == null || !docket.IsFinalisedOrCancelled))
				{
					if (docketLine.WE_TransactionQuantity != inventoryQuantity)
					{
						// need to set WE_TransactionQuantity because deleting the docketline updates TotalsCacheChanges using WE_TransactionQuantity & InDocketLineUnits might be different
						SetValueOnRow(docketLine, (decimal)inventoryQuantity, WhsDocketLineSchema.Constants.WE_TransactionQuantity);
					}
					docketLine.Delete();
				}
			}
			else if (docketLine == null && docket != null)
			{
				docket.UpdateChangesCacheOnRemove(part, inventoryQuantity, inventoryLineNo);
			}

			if (CustomsData != null)
			{
				CustomsData.Delete();
			}
		}

		void SetValueOnRow<T>(T entity, IConvertible value, string columnName)
			where T : IBusinessObjectInternals
		{
			entity.Row[columnName] = value;
		}

		#endregion

		#region EnableLightValidationIfAvailable

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsInventoryViewFetchStrategy(this);
		}

		#endregion

		#region Human Readable Shortcut Name

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Empty;

				if (Client != null)
				{
					result += (string.IsNullOrEmpty(result) ? string.Empty : " - ") + Client.OH_Code;
				}

				if (!WI_OP_PartNum.IsEmpty)
				{
					result += (string.IsNullOrEmpty(result) ? string.Empty : " - ") + WI_OP_PartNum;
				}

				if (!WI_ArrivalDate.IsEmpty)
				{
					result += (string.IsNullOrEmpty(result) ? string.Empty : " - ") + WI_ArrivalDate.ToShortDateString();
				}

				return string.IsNullOrEmpty(result) ? Res.GetString("602c6e52-ea71-491b-9553-d078f9c45a86", "Inventory") : result;
			}
		}

		#endregion

		#region TableName

		public override string TableName
		{
			get { return Schema.TableName; }
		}

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		#region Related Entities

		#region Product

		public WhsProduct Product
		{
			get
			{
				if (WI_OP.IsValid)
				{
					if (product == null || WI_OP != product.Parent.PK)
					{
						product = WhsProduct.GetWhsProduct(Factory, WI_OP);
					}
				}
				else
				{
					product = null;
				}
				return product;
			}
		}

		#endregion

		#region Supplier

		public OrgHeader Supplier
		{
			get
			{
				var docket = DocketOriginal;
				return docket != null ? docket.Supplier : null;
			}
		}

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get
			{
				WhsWarehouse result = null;

				var location = Location;
				if (location != null)
				{
					var warehouse = location.Warehouse;
					if (warehouse != null)
					{
						result = warehouse;
					}
				}

				return result ?? WarehouseOnDocket;
			}
		}

		protected WhsWarehouse WarehouseOnDocket
		{
			get
			{
				var docket = Docket;
				return (docket != null) ? docket.Warehouse : null;
			}
		}

		#endregion

		#region InDocketLine

		public virtual WhsDocketLine InDocketLine
		{
			get
			{
				WhsDocketLine result = null;

				if (WI_WE_InDocketLine.IsValid)
				{
					switch (WI_InDocketLineType)
					{
						case DocketType.Codes.Receive:
							result = Factory.Load<WhsReceiveLine>(WI_WE_InDocketLine);
							break;

						case DocketType.Codes.Adjustment:
							result = Factory.Load<WhsAdjustmentLine>(WI_WE_InDocketLine);
							break;

						case DocketType.Codes.Transfer:
							result = Factory.Load<WhsTransferLine>(WI_WE_InDocketLine);
							break;

						case DocketType.Codes.Order:
							throw new InvalidOperationException("Inventory should not be attached to Order Lines");

						case DocketType.Codes.WorkOrder:
							throw new InvalidOperationException("Inventory should not be attached to Work Order Lines");

						default:
							throw new InvalidOperationException("Inventory attached to a DocketLine but an invalid Docket Type has been specified");
					}
				}

				return result;
			}
		}

		#endregion

		#region InternalsProxy

		public IWhsInventoryInternals InternalsProxy
		{
			get { return this; }
		}

		#endregion

		#region Docket

		public virtual WhsDocket Docket
		{
			get { return Factory.Load<WhsDocket>(WI_WD_Proxy); }
		}

		public WhsDocket DocketOriginal
		{
			get
			{
				WhsDocket result = null;

				if (!WI_WE_OriginalInDocketLineForRating.IsEmpty)
				{
					WhsDocketLine line = Factory.Load<WhsDocketLine>(WI_WE_OriginalInDocketLineForRating);
					if (line != null)
					{
						result = line.Docket;
					}
				}
				if (result == null && !WI_WE_InDocketLine.IsEmpty)
				{
					WhsDocketLine line = Factory.Load<WhsDocketLine>(WI_WE_InDocketLine);
					if (line != null)
					{
						result = line.Docket;
					}
				}
				if (result == null)
				{
					result = Docket;
				}

				return result;
			}
		}

		public ZGuid WI_WD_Proxy
		{
			get { return WI_WD.IsEmpty && InDocketLine != null ? InDocketLine.WE_WD : WI_WD; }
		}

		#endregion

		#region CommittedPickLines

		public IEnumerable<WhsPickLine> CommittedPickLines
		{
			get
			{
				IEnumerable<WhsPickLine> result;

				if (WI_WE_InDocketLine.IsEmpty)
				{
					result = Enumerable.Empty<WhsPickLine>();
				}
				else
				{
					var query = new ZQuery();
					query.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, WI_WE_InDocketLine);
					query.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);
					result = Factory.Load<WhsPickLine>(query);
				}

				return result;
			}
		}

		#endregion

		#region AllPickLines

		public IEnumerable<WhsPickLine> AllPickLines
		{
			get
			{
				IEnumerable<WhsPickLine> result;

				if (WI_WE_InDocketLine.IsEmpty)
				{
					result = Enumerable.Empty<WhsPickLine>();
				}
				else
				{
					var docketLineInMemory = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.PK, WI_WE_InDocketLine) { FetchOnlyFromLocalCache = true });
					var shouldLoadPickLinesFromDb = docketLineInMemory?.IsInDatabase ?? true;

					var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, WI_WE_InDocketLine);
					query.FetchOnlyFromLocalCache = !shouldLoadPickLinesFromDb;
					result = Factory.Load<WhsPickLine>(query);
				}

				return result;
			}
		}

		#endregion

		#region ReservedPickLines

		[ChildEditable]
		[ChildEditableTestExclude]
		public WhsPickLineCollection ReservedPickLines
		{
			get
			{
				var result = reservedPickLines;

				if (reservedPickLines == null)
				{
					if (InDocketLine == null)
					{
						result = new WhsPickLineCollection(Factory);
					}
					else
					{
						result = reservedPickLines = new ReservedPickLineCollection(this);
						RegisterEditableChildObject(reservedPickLines);
					}
				}

				return result;
			}
		}

		WhsPickLineCollection reservedPickLines;

		#endregion

		#region PackageDetails

		public WhsInventoryPackageDetailCollection PackageDetails
		{
			get
			{
				if (packageDetails == null)
				{
					packageDetails = new WhsInventoryPackageDetailCollection(Factory);

					var allPackages = AllPackages.ToArray();

					var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
					packageJobSubQuery.AddToFilter(PkgPackageJobSchema.PK, allPackages.Select(p => p.KP_KJ_ParentPackageJob));

					var orderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
					orderQuery.AddSubQuery(packageJobSubQuery, JoinCondition.And);
					Factory.AddFetchHint(typeof(WhsDocket), orderQuery);

					foreach (var package in allPackages)
					{
						var order = (WhsOrder)package.PackageJob.ParentJob;
						var packageDetail = new WhsInventoryPackageDetail(order, package.KP_PackageID, package.GetIsTote());
						packageDetails.Add(packageDetail);
					}
				}

				return packageDetails;
			}
		}

		WhsInventoryPackageDetailCollection packageDetails;

		#region AllPackages

		IEnumerable<PkgPackage> AllPackages
		{
			get
			{
				var pickLineInventorySubQueryForPickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), PkgPackageItemDivotSchema.KI_ParentID);
				pickLineInventorySubQueryForPickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, PK);

				var packageItemDivotPickLineSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_KP_Package);
				packageItemDivotPickLineSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_ParentTableCode, WhsPickLineSchema.Constants.Prefix);
				packageItemDivotPickLineSubQuery.AddSubQuery(pickLineInventorySubQueryForPickLineSubQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(PkgPackage));
				query.AddSubQuery(packageItemDivotPickLineSubQuery, JoinCondition.Or);

				var packageJobQuery = new ZDBOnlyQuery(typeof(PkgPackageJob));
				var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				packageSubQuery.AddToFilter(query);
				packageJobQuery.AddSubQuery(packageSubQuery, JoinCondition.And);
				Factory.AddFetchHint(PkgPackageJobSchema.Instance, packageJobQuery);

				return Factory.Load<PkgPackage>(query);
			}
		}

		#endregion

		#endregion

		#endregion

		#region Cloning

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			WhsInventoryView result = null;
			var docketLine = InDocketLine;
			if (docketLine != null)
			{
				var clone = (WhsDocketLine)docketLine.Clone();
				clone.WE_WD = docketLine.WE_WD;

				if (clone.Inventory.Count > 0)
				{
					result = clone.Inventory[0];
					result.WI_WD = WI_WD;
				}
			}

			return result;
		}

		#endregion

		#region Properties

		#region WI_LineNo

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public ZShort WI_LineNo
		{
			get
			{
				if (lineNo == ZShort.Zero && InDocketLine != null)
				{
					lineNo = InDocketLine.WE_LineNo;
				}
				return lineNo;
			}
			set
			{
				SetNonPersistentPropertyValue(WI_LineNoInfo, ref lineNo, value);
				var inDocketLine = this.InDocketLine;
				if (inDocketLine != null)
				{
					inDocketLine.WE_LineNo = value;
				}
			}
		}

		public virtual ZPropertyInfo WI_LineNoInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_LineNo);
				return info;
			}
		}

		ZShort lineNo;

		#endregion

		#region WI_SubLineNo

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public ZShort WI_SubLineNo
		{
			get
			{
				if (subLineNo == ZShort.Zero && InDocketLine != null)
				{
					subLineNo = InDocketLine.WE_SubLineNo;
				}
				return subLineNo;
			}
			set
			{
				SetNonPersistentPropertyValue(WI_SubLineNoInfo, ref subLineNo, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_SubLineNo = value;
				}
			}
		}

		public virtual ZPropertyInfo WI_SubLineNoInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_SubLineNo);
				return info;
			}
		}

		ZShort subLineNo;

		#endregion

		#region CountryCode

		public ZString CountryCode => Warehouse?.CountryCode ?? ZString.Empty;

		#endregion

		#region Flags

		#region  HasStockIncludingNotYetFinalised

		public bool HasStockIncludingNotYetFinalised
		{
			get
			{
				var docketLine = InDocketLine;
				return docketLine != null && (((docketLine.IsFinalised || IsInTransit) && WI_TotalUnits != 0m) || (!docketLine.IsFinalised && WI_InDocketLineUnits != 0m));
			}
		}

		#endregion

		#region IsAvailable

		public bool IsAvailable => WI_InventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.Available);

		#endregion

		#region IsHeld

		public bool IsHeld => WI_InventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.Held);

		#endregion

		#region IsPickByBOMKitInventory

		public bool IsPickByBOMKitInventory => (WI_InventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.Pending) || WI_InventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.Putaway))
			&& (InDocketLine?.IsCreatedFromPickByBOM ?? false);

		#endregion

		#region IsInTransit

		public bool IsInTransit => InTransitStatusCodeList.Any(i => i == WI_InventoryStatus);

		public static IEnumerable<ZString> InTransitStatusCodeList => new ZString[] { InventoryStatus.Codes.InTransit, InventoryStatus.Codes.PuttingAway };

		#endregion

		#region IsDocketLineFinalised

		public bool IsDocketLineFinalised => InDocketLine?.IsFinalised ?? false;

		#endregion

		#region IsAllocatedToPickFace

		public bool IsAllocatedToPickFace()
		{
			var whsProduct = Product;
			if (whsProduct != null)
			{
				foreach (WhsPickFace pickFace in whsProduct.PickFaces)
				{
					if (pickFace.WF_WL == WI_WL)
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region HasEDocsOrNotesAttached

		public ZBool HasEDocsOrNotesAttached => InDocketLine?.HasEDocsOrNotesAttached ?? ZBool.False;

		public ZPropertyInfo HasEDocsOrNotesAttachedInfo => GetZPropertyInfo(nameof(HasEDocsOrNotesAttached));

		#endregion

		#region IsTemporaryProduct

		public bool IsTemporaryProduct => !WI_OP.IsValid && !WI_OP_PartNum.IsEmpty;

		#endregion

		#region IsReceiveLineInventory

		public bool IsReceiveLineInventory => WI_InDocketLineType == DocketType.Codes.Receive;

		#endregion

		#region HasPutawayTransfer

		public bool HasPutawayTransfer => (InDocketLine as WhsReceiveLine)?.HasPutawayTransfer ?? false;

		#endregion

		#region IsReceivedIntoDockDoor

		public bool IsReceivedIntoDockDoor => WI_InventoryStatus == InventoryStatus.Codes.Received;

		#endregion

		#endregion

		#region Used Attributes Count

		public int UsedAttributesAndPalletIdCount
		{
			get
			{
				int result = 0;
				if (!WI_PartAttrib1.IsEmpty)
				{
					result++;
				}
				if (!WI_PartAttrib2.IsEmpty)
				{
					result++;
				}
				if (!WI_PartAttrib3.IsEmpty)
				{
					result++;
				}
				if (!WI_SerialNumber.IsEmpty)
				{
					result++;
				}
				if (!WI_PackingDate.IsEmpty)
				{
					result++;
				}
				if (!WI_ExpiryDate.IsEmpty)
				{
					result++;
				}
				if (!WI_PalletID.IsEmpty)
				{
					result++;
				}
				return result;
			}
		}

		#endregion

		#region CommittedQuantityIncludingUnfinalisedReceipt

		/// <summary>
		/// The amount committed to this Inventory, which also takes into account whether this
		/// inventory is finalised or not.
		/// </summary>
		public ZDecimal CommittedQuantityIncludingUnfinalisedReceipt
		{
			get { return CommittedToTransactionQuantity + CommittedToReceiveQuantity; }
		}

		#endregion

		#region CommittedToTransactionQuantity

		/// <summary>
		/// *** DO NOT USE UNLESS YOU KNOW WHAT YOU ARE DOING ***
		///
		/// This will return the amount committed to this inventory but without considering whether this
		/// inventory is finalised or not. This property primarily exists for the GUI. Generally speaking,
		/// 'CommittedQuantityIncludingUnfinalisedReceipt' should be used instead.
		/// </summary>
		ZDecimal IWhsInventoryInternals.CommittedToTransactionQuantity
		{
			get { return CommittedToTransactionQuantity; }
		}

		/// <summary>
		/// *** DO NOT USE UNLESS YOU KNOW WHAT YOU ARE DOING ***
		///
		/// This will return the amount committed to this inventory but without considering whether this
		/// inventory is finalised or not. This property primarily exists for the GUI. Generally speaking,
		/// 'CommittedQuantityIncludingUnfinalisedReceipt' should be used instead.
		/// </summary>
		ZDecimal CommittedToTransactionQuantity
		{
			get
			{
				var result = 0m;

				foreach (var pickLine in GetPickLinesLoadedWithFetchHints())
				{
					var docketLine = pickLine.DocketLine;
					result += GetCommittedToPickOrAdjustmentQuantity(pickLine, docketLine);
					result += GetCommittedToTransferQuantityCore(pickLine, docketLine);
				}

				return result;
			}
		}

		ZPropertyInfo IWhsInventoryInternals.CommittedToTransactionQuantityInfo
		{
			get { return GetZPropertyInfo("InternalsProxy+" + Schema.CommittedToTransactionQuantity); }
		}

		#endregion

		#region CommittedToReceiveQuantity

		ZDecimal CommittedToReceiveQuantity
		{
			get
			{
				var result = 0m;
				if (IsReceiveLineInventory && !InDocketLine.IsFinalised)
				{
					result = WI_TotalUnits;
				}

				return result;
			}
		}

		#endregion

		#region CommittedToPickOrAdjustmentQuantity

		static decimal GetCommittedToPickOrAdjustmentQuantity(WhsPickLine pickLine, WhsDocketLine docketLine)
		{
			var result = 0m;

			if (docketLine is WhsAdjustmentLine)
			{
				result = pickLine.WZ_Units;
			}
			else
			{
				var pickableDocket = docketLine is WhsPickableDocketLine pickableDocketLine ? pickableDocketLine.PickableDocket : null;
				if (pickableDocket != null)
				{
					if (!pickableDocket.WD_WP.IsEmpty)
					{
						result = pickLine.WZ_Units;
					}
					// There is a known bug whereby a reserved pickline has an OriginalReservedQty of 0. We can identify this as a reserve line because any pickline
					// that exists on an unpicked order cannot be anything but.
					//
					// We do not yet know how this occurs, but here we handle it as there is no known ill effect in doing so.
					// Additional error reporting is also added on pickline save.
					else if (pickLine.WZ_OriginalReservedQty == 0m && !pickableDocket.IsCancelled)
					{
						result = pickLine.WZ_Units;
					}
				}
			}

			return result;
		}

		// Tested in DbHits tests in ReleaseEntryForm.cs
		public void LoadFetchHintsForPickLines()
		{
			GetPickLinesLoadedWithFetchHints();
		}

		IEnumerable<WhsPickLine> GetPickLinesLoadedWithFetchHints()
		{
			var pickLines = CommittedPickLines.ToArray();

			if (!FetchHintsAddedForDocketWhenLoadingPickLines)
			{
				if (pickLines.Length > 0)
				{
					foreach (var line in pickLines)
					{
						Factory.AddFetchHint(WhsDocketLineSchema.PK, line.WZ_WE_TransactionLine);
						Factory.AddFetchHint(WhsDocketLineSchema.PK, line.WZ_WE_InventoryLine);
					}

					// add fetch hint for Docket
					var docketLinePKs = pickLines.Select(p => p.WZ_WE_TransactionLine)
						.Where(p => Factory.GetBizOsForPK(p.ToGuid()).Length == 0).ToArray(); // Don't want to load docketline PKs if they are already in the factory.

					if (docketLinePKs.Length > 0)
					{
						var query = new ZDBOnlyQuery(typeof(WhsDocket));
						query.AddSubQuery(GetDocketLineZDBOnlySubQuery(docketLinePKs), JoinCondition.And);

						Factory.AddFetchHint(WhsDocketSchema.Instance, query);
					}
				}

				FetchHintsAddedForDocketWhenLoadingPickLines = true;
			}

			return pickLines;
		}

		public static ZDBOnlySubQuery GetDocketLineZDBOnlySubQuery(ZGuid[] docketLinePKs)
		{
			var docketLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD) { AllowTableValuedParameters = true };
			docketLineQuery.AddToFilter(WhsDocketLineSchema.PK, docketLinePKs);
			return docketLineQuery;
		}

		bool FetchHintsAddedForDocketWhenLoadingPickLines;

		#endregion

		#region WI_CommittedToTransferQuantity

		public ZDecimal WI_CommittedToTransferQuantity
		{
			get
			{
				ZDecimal result = 0m;

				foreach (var pickLine in GetPickLinesLoadedWithFetchHints())
				{
					result += GetCommittedToTransferQuantityCore(pickLine, pickLine.DocketLine);
				}

				return result;
			}
		}

		static ZDecimal GetCommittedToTransferQuantityCore(WhsPickLine pickLine, WhsDocketLine docketLine)
		{
			return docketLine is WhsTransferLine ? pickLine.WZ_Units : ZDecimal.Zero;
		}

		#endregion

		#region WI_CrossDockQuantity

		public ZDecimal WI_CrossDockQuantity
		{
			get { return ReservedPickLines.GetQtyCommitted(); }
		}

		public ZPropertyInfo WI_CrossDockQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.WI_CrossDockQuantity); }
		}

		#endregion

		#region WI_AvailableToPickQuantity

		/// <summary>
		/// Quantity that is Available as Finalised Stock. This does not include Inventory that is Damaged or Held.
		/// </summary>
		public ZDecimal WI_AvailableToPickQuantity
		{
			get
			{
				ZDecimal result = 0m;

				if (IsAvailable)
				{
					var location = Location;
					if (location != null && location.WLV_LocationStatus == Environment.CodeLists.LocationStatus.Codes.Normal)
					{
						result = WI_AvailableToTransferQuantity;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo WI_AvailableToPickQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.WI_AvailableToPickQuantity); }
		}

		#endregion

		#region WI_AvailableToTransferQuantity

		/// <summary>
		/// Quantity that is Available as finalised Stock. This includes Inventory that is Damaged or Held.
		/// </summary>
		public ZDecimal WI_AvailableToTransferQuantity
		{
			get
			{
				var availableQtyType = IsReceivedIntoDockDoor
					? AvailableQtyType.IgnoreUnfinalisedReceipt | AvailableQtyType.IgnoreCrossDock
					: AvailableQtyType.Default;

				return GetAvailableQuantity(availableQtyType, WI_TotalUnits);
			}
		}

		#endregion

		#region WI_AvailableForCrossDockQuantity

		public ZDecimal WI_AvailableForCrossDockQuantity
		{
			// You can cross dock unfinalised inventory, so unfinalised inventory is 'available' for reservation.
			get => GetAvailableQuantity(AvailableQtyType.IgnoreUnfinalisedReceipt, QuantityToConsiderForCrossDocking);
		}

		public ZPropertyInfo WI_AvailableForCrossDockQuantityInfo => GetZPropertyInfo(Schema.WI_AvailableForCrossDockQuantity);

		ZDecimal QuantityToConsiderForCrossDocking =>
			InDocketLine is WhsReceiveLine receiveLine && receiveLine.CanReserveClientOrderedUnits
			? WI_ExpectedReceiptQuantity
			: WI_TotalUnits;

		#endregion

		#region GetAvailableQuantity

		ZDecimal GetAvailableQuantity(AvailableQtyType availableQtyType, ZDecimal quantityToConsider)
		{
			bool ignoreUnfinalisedReceipt = availableQtyType.HasFlag(AvailableQtyType.IgnoreUnfinalisedReceipt);
			var committedQty = ignoreUnfinalisedReceipt
				? CommittedToTransactionQuantity
				: CommittedQuantityIncludingUnfinalisedReceipt;

			bool ignoreCrossDockQty = availableQtyType.HasFlag(AvailableQtyType.IgnoreCrossDock);
			return quantityToConsider - committedQty - (ignoreCrossDockQty ? ZDecimal.Zero : WI_CrossDockQuantity); // Exclude cross dock quantity when creating putaway transfer lines
		}

		[Flags]
		enum AvailableQtyType
		{
			Default = 0,
			IgnoreUnfinalisedReceipt = 1,
			IgnoreCrossDock = 2
		}

		#endregion

		#region WI_SplitQuantity

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDecimal WI_SplitQuantity
		{
			get { return wi_SplitQuantity; }
			set
			{
				SetNonPersistentPropertyValue(WI_SplitQuantityInfo, ref wi_SplitQuantity, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateWI_SplitQuantity();
				}
			}
		}
		ZDecimal wi_SplitQuantity;

		public ZPropertyInfo WI_SplitQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.WI_SplitQuantity); }
		}

		#endregion

		#region WI_ExpectedReceiptQuantity

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		public ZDecimal WI_ExpectedReceiptQuantity
		{
			get
			{
				var docketLine = InDocketLine;
				return docketLine != null ? docketLine.WE_ClientOrderedUnits : ZDecimal.Zero;
			}
			set
			{
				SetValueToDocketLine(WhsDocketLineSchema.WE_ClientOrderedUnits.Name, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateWI_ExpectedReceiptQuantity();
					Validation.ValidateWI_InDocketLineUnits();
					ValidateSerialNumberPartAttribute();
				}
				WI_ExpectedReceiptQuantityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WI_ExpectedReceiptQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.WI_ExpectedReceiptQuantity); }
		}

		#endregion

		#region WI_ReceiveCrossDockOrderNo

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[MaxLength(WhsDocketLine.Schema.WE_ReceiveCrossDockOrderNoMaxLength)]
		public ZString WI_ReceiveCrossDockOrderNo
		{
			get
			{
				if (!wi_ReceiveCrossDockOrderNoLoaded && InDocketLine != null)
				{
					wi_ReceiveCrossDockOrderNo = InDocketLine.WE_ReceiveCrossDockOrderNo;
					wi_ReceiveCrossDockOrderNoLoaded = true;
				}
				return wi_ReceiveCrossDockOrderNo;
			}
			set
			{
				var inDocketLine = this.InDocketLine;

				SetNonPersistentPropertyValue(WI_ReceiveCrossDockOrderNoInfo, ref wi_ReceiveCrossDockOrderNo, value);
				if (inDocketLine != null)
				{
					inDocketLine.WE_ReceiveCrossDockOrderNo = value;
				}
				wi_ReceiveCrossDockOrderNoLoaded = true;
			}
		}

		ZString wi_ReceiveCrossDockOrderNo;
		bool wi_ReceiveCrossDockOrderNoLoaded;

		public ZPropertyInfo WI_ReceiveCrossDockOrderNoInfo
		{
			get { return GetZPropertyInfo(Schema.WI_ReceiveCrossDockOrderNo); }
		}

		#endregion

		#region CommodityCode

		[ReadOnlyMember(nameof(TempProductReadOnly))]
		[List("SupplierPart.Lookups.CommodityCodes")]
		[MaxLength(RefCommodityCode.Schema.RH_CodeMaxLength)]
		public ZString CommodityCode
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_RH_NKCommodityCode : commodityCode;
			}
			set
			{
				if (commodityCode != value)
				{
					CheckMaximumLength(CommodityCodeInfo, value);
					commodityCode = value;
					SetValueToDocketLine(WhsDocketLine.Schema.CommodityCode, value);
					CommodityCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CommodityCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CommodityCode); }
		}

		ZString commodityCode;

		#endregion

		#region Location

		#region WI_WL

		[List("Lookups.Locations")]
		[ReadOnlyMember(nameof(IsLocationReadOnly))]
		[RelatedBusinessObject("Location")]
		[ResourceStringData("WhsInventoryView|WI_WL", Caption = "Location", ShortCaption = "Loc.")]
		public override ZGuid WI_WL
		{
			get { return base.WI_WL; }
			set
			{
				if (value != base.WI_WL)
				{
					base.WI_WL = value;

					SetValueToDocketLine(WhsDocketLineSchema.WE_WL.Name, value);

					SetLocationStringToEmptyIfLocationIsSetToEmpty();
					SetReceiveDocketStatus();
				}
			}
		}

		void SetLocationStringToEmptyIfLocationIsSetToEmpty()
		{
			if (WI_WL.IsEmpty)
			{
				LocationString = "";

				var docketLine = InDocketLine;
				if (docketLine != null)
				{
					docketLine.LocationString = "";
				}
			}
		}

		#endregion

		public virtual WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WI_WL); }
		}

		[ReadOnlyMember(nameof(IsLocationReadOnly))]
		[MaxLength(36)]
		public ZString LocationString
		{
			get { return Location?.WLV_LocationString_UserFriendly ?? locationString; }
			set
			{
				var location = Location;
				if (location != null && location.WLV_LocationString != value || locationString != value || !WI_WL.IsValid)
				{
					// Prevalidation
					CheckMaximumLength(LocationStringInfo, value);

					if (Docket != null)
					{
						LocationWhsGuid = Docket.WD_WW_Whs;
					}

					locationString = value;
					WI_WL = WhsLocation.FindLocationPK(Factory, value, locationWhsGuid);

					if (!IsValidationSuspended)
					{
						var docketLine = InDocketLine;
						if (docketLine == null || !docketLine.WE_WLInfo.HasErrors())
						{
							SetReceiveDocketStatus();
						}
					}
					LocationStringInfo.RefreshBinding();
				}
			}
		}

		ZString locationString;

		public virtual ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(Schema.LocationString); }
		}

		public virtual ZGuid LocationWhsGuid
		{
			get { return Location?.WLV_WW_Whs ?? locationWhsGuid; }
			set
			{
				locationWhsGuid = value;
				LocationWhsGuidInfo.RefreshBinding();
			}
		}

		ZGuid locationWhsGuid;

		public ZPropertyInfo LocationWhsGuidInfo
		{
			get { return GetZPropertyInfo(nameof(LocationWhsGuid)); }
		}

		public ZString LocationStatus
		{
			get
			{
				var location = Location;
				return (location != null) ? location.LocationStatuses.GetDescriptionFromCode(location.WLV_LocationStatus) : "";
			}
		}

		#region CurrentLocationStatus

		public ZString CurrentLocationStatus
		{
			get { return InDocketLine?.CurrentLocationStatus ?? ZString.Empty; }
		}

		public ZString CurrentLocationString
		{
			get { return InDocketLine?.CurrentLocationString ?? ZString.Empty; }
		}

		public WhsLocation CurrentLocation
		{
			get { return InDocketLine?.CurrentLocation; }
		}

		public static ZDBOnlyQuery GetInventoryQueryByLocation(Action<ZDBOnlySubQuery> subQuery = null)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));

			var nonInTransitInventorySubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsInventoryViewSchema.PK);
			var warehouseLocationQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsDocketLineSchema.WE_WL);
			if (subQuery != null)
			{
				subQuery(warehouseLocationQuery);
			}
			nonInTransitInventorySubQuery.AddSubQuery(warehouseLocationQuery, JoinCondition.And);
			nonInTransitInventorySubQuery.AddToFilter(WhsDocketLineSchema.WE_OriginalInventoryStatus, SQLComparisonOperator.NotEqual, InventoryStatus.Codes.InTransit);
			nonInTransitInventorySubQuery.AddToFilter(WhsDocketLineSchema.WE_OriginalInventoryStatus, SQLComparisonOperator.NotEqual, InventoryStatus.Codes.PuttingAway);
			query.AddSubQuery(nonInTransitInventorySubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		public ZPropertyInfo LocationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.LocationStatus); }
		}

		#region LocationClass

		public ZString LocationClass
		{
			get
			{
				var location = Location;
				return (location != null) ? location.LocationType.WLT_LocationClass : ZString.Empty;
			}
		}

		public ZPropertyInfo LocationClassInfo
		{
			get { return GetZPropertyInfo(Schema.LocationClass); }
		}

		#endregion

		#region CurrentLocationClass

		public ZString CurrentLocationClass
		{
			get { return (InDocketLine as WhsReceiveLine)?.CurrentLocationClass ?? LocationClass; }
		}

		#endregion

		#region PutawayLocation

		internal WhsTransferLine PutawayTransferLine
		{
			get
			{
				WhsTransferLine result = null;

				if (WI_InDocketLineType == DocketType.Codes.Receive && !WI_WL.IsEmpty)
				{
					var pickLinesWithTransferLines = IEnumerableExtensions.DistinctBy(AllPickLines, l => l.WZ_WE_TransactionLine).Select(l => l.DocketLine).WhereNotNull().OfType<WhsTransferLine>();
					result = pickLinesWithTransferLines.FirstOrDefault(l => l.IsPutawayTransferLine);
				}

				return result;
			}
		}

		#endregion

		void SetReceiveDocketStatus()
		{
			var receive = Docket as WhsReceive;
			if (receive != null && !receive.IsFinalisedOrCancelled)
			{
				UpdateReceiveLineOriginalInventoryStatus((WhsReceiveLine)InDocketLine);
				var isPutaway = receive.WD_DocketStatus == DocketStatus.Codes.Putaway;

				if ((WI_WL.IsEmpty && isPutaway)
					|| (!WI_WL.IsEmpty && (!isPutaway || CurrentLocationClass == LocationClasses.Codes.DDL)))
				{
					receive.UpdateDocketStatus();
				}
			}
		}

		internal void UpdateReceiveLineOriginalInventoryStatus(WhsReceiveLine receiveLine)
		{
			var status = string.Empty;

			WhsLocation location;
			if (!WI_WL.IsEmpty && (location = Location) != null)
			{
				status = !location.IsDockDoorLocation || (receiveLine != null && receiveLine.IsCrossDockLocation(location))
					? InventoryStatus.Codes.Putaway
					: InventoryStatus.Codes.Received;
			}
			else
			{
				status = (WI_ArrivalDate.IsEmpty ? InventoryStatus.Codes.Pending : InventoryStatus.Codes.Arrived);
			}

			OriginalInventoryStatus = status;
		}

		#endregion

		#region WI_WW_Whs

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[RelatedBusinessObject("Warehouse")]
		[List("Lookups.Warehouses")]
		[ResourceStringData("WhsInventoryView|WI_WW_Whs", Caption = "Warehouse")]
		public override ZGuid WI_WW_Whs
		{
			get { return base.WI_WW_Whs; }
			set { base.WI_WW_Whs = value; }
		}

		#endregion

		#region WI_OP

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[RelatedBusinessObject("SupplierPart")]
		[List("Lookups.SupplierParts")]
		[ResourceStringData("WhsInventoryView|WI_OP", Caption = "Product", FullDescription = "Enter the Product Code from the Warehouse Product Files. Product Codes contain Product and Warehouse data elements. Product Codes are created and maintained by the user in the Warehouse Product Files.")]
		public override ZGuid WI_OP
		{
			get { return base.WI_OP; }
			set
			{
				if (value != base.WI_OP)
				{
					base.WI_OP = value;
					SetValueToDocketLine(WhsDocketLineSchema.WE_OP.Name, value);

					if (!IsCopying)
					{
						SetPackTypeFromProductParams();
						SetLineDataBasedOnProduct();
					}
				}
				OnProductChanged();
			}
		}

		public virtual OrgSupplierPart SupplierPart
		{
			get { return Factory.Load<OrgSupplierPart>(WI_OP); }
		}

		void OnProductChanged()
		{
			var receive = Docket as WhsReceive;
			if (receive != null)
			{
				receive.CallOnProductChanged(this, EventArgs.Empty);
			}
		}

		#region SetPackTypeFromProductParams

		void SetPackTypeFromProductParams()
		{
			var productParams = Docket?.GetProductParams(Product);
			if (productParams != null && !productParams.W3_F3_NKReceivedPackType.IsEmpty)
			{
				WI_F3_NKPackType = productParams.W3_F3_NKReceivedPackType;
			}
		}

		#endregion

		#region SetLineDataBasedOnProduct

		protected void SetLineDataBasedOnProduct()
		{
			var part = SupplierPart; // for performance reasons
			if (part != null)
			{
				if (WI_F3_NKPackType.IsEmpty)
				{
					WI_F3_NKPackType = part.OP_StockKeepingUnit;
				}

				var client = Client;
				if (client != null)
				{
					var manager = client.PartAttributeManager;
					if (!manager.IsExpiryDateUsedByProduct(part))
					{
						WI_ExpiryDate = ZDate.Empty;
					}

					if (!manager.IsPackingDateUsedByProduct(part))
					{
						WI_PackingDate = ZDate.Empty;
					}

					if (!manager.IsPartAttributeUsedByProduct(part, 1))
					{
						WI_PartAttrib1 = "";
					}

					if (!manager.IsPartAttributeUsedByProduct(part, 2))
					{
						WI_PartAttrib2 = "";
					}

					if (!manager.IsPartAttributeUsedByProduct(part, 3))
					{
						WI_PartAttrib3 = "";
					}

					if (!manager.IsSerialNumberUsedByProduct(part))
					{
						WI_SerialNumber = "";
					}
				}
			}
		}

		#endregion

		#endregion

		#region WI_OP_PartNum

		[MaxLength(OrgSupplierPart.Schema.OP_PartNumMaxLength)]
		public ZString WI_OP_PartNum
		{
			get
			{
				var part = this.SupplierPart;
				return part != null ? part.OP_PartNum : temporaryProductCode;
			}
			set
			{
				if (temporaryProductCode != value)
				{
					CheckMaximumLength(WI_OP_PartNumInfo, value);
					temporaryProductCode = value;
					SetValueToDocketLine(WhsDocketLine.Schema.ProductCode, value);
					WI_OP_PartNumInfo.RefreshBinding();
				}
			}
		}

		ZString temporaryProductCode;

		public virtual ZPropertyInfo WI_OP_PartNumInfo
		{
			get { return GetZPropertyInfo(Schema.WI_OP_PartNum); }
		}

		#endregion

		#region WI_OH_Client

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[RelatedBusinessObject("Client")]
		[List("Lookups.Clients")]
		[ResourceStringData("WhsInventoryView|WI_OH_Client", Caption = "Client")]
		public override ZGuid WI_OH_Client
		{
			get { return base.WI_OH_Client; }
			set { base.WI_OH_Client = value; }
		}

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(WI_OH_Client); }
		}

		#endregion

		#region WI_ClientUQ

		// used on web :P
		[MaxLength(OrgPartRelation.Schema.OU_ClientUQMaxLength)]
		public ZString WI_ClientUQ
		{
			get { return InDocketLine != null ? InDocketLine.WE_ClientUQ : WI_UnitsUQ; }
		}

		public ZPropertyInfo WI_ClientUQInfo
		{
			get { return GetZPropertyInfo(Schema.WI_ClientUQ); }
		}

		// TODO : Move this to TrackingInventory when the web team create one.
		public ZString Details
		{
			get { return Res.GetString("05a48409-2b55-40cf-b036-6894085ed412", "Details"); }
		}

		public ZPropertyInfo DetailsInfo
		{
			get { return GetZPropertyInfo(Schema.Details); }
		}

		#endregion

		#region WI_OP_Desc

		[ReadOnlyMember(nameof(TempProductReadOnly))]
		[MaxLength(OrgSupplierPart.Schema.OP_DescMaxLength)]
		public ZString WI_OP_Desc
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_Desc : wi_OP_Desc;
			}
			set
			{
				if (wi_OP_Desc != value)
				{
					CheckMaximumLength(WI_OP_DescInfo, value);
					wi_OP_Desc = value;
					SetValueToDocketLine(WhsDocketLine.Schema.ProductDesc, value);
					WI_OP_DescInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo WI_OP_DescInfo
		{
			get { return GetZPropertyInfo(Schema.WI_OP_Desc); }
		}

		ZString wi_OP_Desc;

		#endregion

		#region WI_UnitsUQ

		[ReadOnlyMember(nameof(TempProductReadOnly))]
		[List("Lookups.PackTypesWithStandardUnits")]
		[MaxLength(OrgSupplierPart.Schema.OP_StockKeepingUnitMaxLength)]
		[ResourceStringData("WhsInventoryView|WI_UnitsUQ", Caption = "Units")]
		public ZString WI_UnitsUQ
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_StockKeepingUnit : temporaryProductUQ;
			}
			set
			{
				if (temporaryProductUQ != value)
				{
					CheckMaximumLength(WI_UnitsUQInfo, value);
					temporaryProductUQ = value;
					SetValueToDocketLine(WhsDocketLine.Schema.ProductUQ, value);
					WI_UnitsUQInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo WI_UnitsUQInfo
		{
			get { return GetZPropertyInfo(Schema.WI_UnitsUQ); }
		}

		ZString temporaryProductUQ = Constants.PkgUnit.Unit;

		#endregion

		#region WI_F3_NKPackType

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.PackTypesWithStandardUnits")]
		[ResourceStringData("WhsInventoryView|WI_F3_NKPackType", Caption = "Pack UQ")]
		public override ZString WI_F3_NKPackType
		{
			get { return base.WI_F3_NKPackType; }
			set
			{
				base.WI_F3_NKPackType = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_F3_NKPackType.Name, value);
			}
		}

		#endregion

		#region WI_InventoryStatus

		public InventoryStatus Statuses
		{
			get { return new InventoryStatus(); }
		}

		public ZString StatusDesc
		{
			get { return Statuses.GetDescriptionFromCode(WI_InventoryStatus); }
		}

		public ZPropertyInfo StatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDesc)); }
		}

		#endregion

		#region WI_ArrivalDate

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_ArrivalDate", Caption = "Arrival Date")]
		public override ZDateTimeOffset WI_ArrivalDate
		{
			get { return base.WI_ArrivalDate; }
			set
			{
				base.WI_ArrivalDate = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_AdjustmentArrivalDate.Name, value);

				var receive = Docket as WhsReceive;
				if (receive != null && !receive.IsFinalising && !receive.IsFinalisedOrCancelled)
				{
					UpdateReceiveLineOriginalInventoryStatus((WhsReceiveLine)InDocketLine);
				}
			}
		}

		#endregion

		#region WI_ArrivalDateOrETA

		public ZDateTimeOffset WI_ArrivalDateOrETA
		{
			get { return WI_InventoryStatus != InventoryStatus.Codes.Pending || Docket == null ? WI_ArrivalDate : Docket.WD_ETA; }
		}

		public ZPropertyInfo WI_ArrivalDateOrETAInfo
		{
			get { return GetZPropertyInfo(Schema.WI_ArrivalDateOrETA); }
		}

		#endregion

		#region WI_TotalUnits

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		[ResourceStringData("WhsInventoryView|WI_TotalUnits", Caption = "Total Units")]
		public override ZDecimal WI_TotalUnits
		{
			get { return base.WI_TotalUnits; }
			set
			{
				base.WI_TotalUnits = value;

				SetValueToDocketLine(WhsDocketLineSchema.WE_StockOnHand.Name, value);
#if DEBUG
				OnInventoryCreatedFromTransferLine_ForTest(this);
#endif
				bondInfo = null;
			}
		}

		#endregion

		#region WI_InDocketLineUnits

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_InDocketLineUnits", Caption = "Units")]
		public override ZDecimal WI_InDocketLineUnits
		{
			get { return base.WI_InDocketLineUnits; }
			set
			{
				var oldValue = WI_InDocketLineUnits;
				base.WI_InDocketLineUnits = value;

				var docketLine = InDocketLine;
				if (docketLine != null && docketLine is WhsReceiveLine && docketLine.WE_IsOriginalInventory)
				{
					docketLine.SuspendingUpdatingTotalWeightAndVolume();
					try
					{
						SetValueToDocketLine(WhsDocketLineSchema.WE_TransactionQuantity.Name, value);
					}
					finally
					{
						docketLine.ResumeUpdatingTotalWeightAndVolume();
					}

					WI_TotalUnits = value;
				}

				var docket = Docket;
				if (docket != null)
				{
					var delta = value - oldValue;
					docket.UpdateTotalWeightAndVolume(SupplierPart, delta);
					if (!docket.RefreshTotalUnitsFromLinesSemaphore.IsSuspended)
					{
						docket.WD_TotalUnitsFromLinesInfo.RefreshBinding();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateWI_ExpectedReceiptQuantity(); // tested in WhsInventoryValidation
					ValidateSerialNumberPartAttribute();
				}
			}
		}

		void ValidateSerialNumberPartAttribute()
		{
			if (!WI_SerialNumber.IsEmpty)
			{
				Validation.ValidateWI_SerialNumber();
			}
		}

		#endregion

		#region WI_WE_InDocketLine

		[BusinessObjectTestExclude]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject InDocketLine (WhsDocketLine) is an abstract class")]
		[RelatedBusinessObject("InDocketLine")]
		public override ZGuid WI_WE_InDocketLine
		{
			get { return base.WI_WE_InDocketLine; }
			set { base.WI_WE_InDocketLine = value; }
		}

		#endregion

		#region WI_WD

		[BusinessObjectTestExclude]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject Docket (WhsDocket) is an abstract class")]
		[RelatedBusinessObject("Docket")]
		public override ZGuid WI_WD
		{
			get { return base.WI_WD; }
			set
			{
				RemoveFromOldDocket(Docket);
				base.WI_WD = value;
				SetDefaultsFromDocket();
				SetValueToDocketLine(WhsDocketLineSchema.WE_WD.Name, value);
			}
		}

		void RemoveFromOldDocket(WhsDocket docket)
		{
			var receive = docket as WhsReceive;
			if (receive != null && receive.Inventory.Contains(this))
			{
				receive.Inventory.Remove(this);
			}
		}

		void SetDefaultsFromDocket()
		{
			var docket = Docket;
			if (docket != null)
			{
				WI_OH_Client = docket.WD_OH_Client;
				WI_WW_Whs = docket.WD_WW_Whs;
				WI_InDocketLineType = docket.WD_DocketType;
				if (WI_ArrivalDate.IsEmpty)
				{
					WI_ArrivalDate = docket.WD_ArrivalDate;
				}

				var receive = docket as WhsReceive;
				if (receive != null && WI_IsOriginalReceiptLine && !AddToDocketInventoryCollectionSemaphore.IsSuspended && !receive.Inventory.Contains(this))
				{
					receive.Inventory.Add(this);
					UpdateReceiveLineOriginalInventoryStatus((WhsReceiveLine)InDocketLine);
				}
			}
		}

		public Semaphore AddToDocketInventoryCollectionSemaphore
		{
			get { return addToDocketInventoryCollectionSemaphore ?? (addToDocketInventoryCollectionSemaphore = new Semaphore()); }
		}

		Semaphore addToDocketInventoryCollectionSemaphore;

		#endregion

		#region WI_WE_OriginalInDocketLineForRating

		[BusinessObjectTestExclude]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject InDocketLineForRating (WhsDocketLine) is an abstract class")]
		[RelatedBusinessObject("InDocketLineForRating")]
		public override ZGuid WI_WE_OriginalInDocketLineForRating
		{
			get { return base.WI_WE_OriginalInDocketLineForRating; }
			set
			{
				base.WI_WE_OriginalInDocketLineForRating = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_WE_OriginalDocketLineForRating.Name, value);
			}
		}

		// put this in to make bizo test case pass, as we don't care about the related rating docket line
		protected WhsDocketLine InDocketLineForRating
		{
			get { return null; }
		}

		#endregion

		#region WI_InDocketLineType

		[ActionField(ReadOnly = true)]
		public override ZString WI_InDocketLineType
		{
			get { return base.WI_InDocketLineType; }
			set { base.WI_InDocketLineType = value; }
		}

		#endregion

		#region WI_InventoryStatus

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Statuses")]
		[ResourceStringData("WhsInventoryView|WI_InventoryStatus", Caption = "Inventory Status", ShortCaption = "Status")]
		public override ZString WI_InventoryStatus
		{
			get { return base.WI_InventoryStatus; }
			set
			{
				base.WI_InventoryStatus = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_CurrentInventoryStatus.Name, value);
			}
		}

		#endregion

		#region OriginalInventoryStatus

		[ReadOnly(true)]
		[List("Statuses")]
		[MaxLength(Schema.WI_InventoryStatusMaxLength)]
		public ZString OriginalInventoryStatus
		{
			get
			{
				var inDocketLine = InDocketLine;
				return (inDocketLine != null) ? inDocketLine.WE_OriginalInventoryStatus : originalInventoryStatus;
			}
			set
			{
				WI_InventoryStatus = value;
				SetNonPersistentPropertyValue(OriginalInventoryStatusInfo, ref originalInventoryStatus, value);
				SetValueToDocketLine(WhsDocketLineSchema.WE_OriginalInventoryStatus.Name, value);
			}
		}

		ZString originalInventoryStatus;

		public ZPropertyInfo OriginalInventoryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalInventoryStatus); }
		}

		#endregion

		#region WI_HeldCode

		[ReadOnly(true)]
		[List("Lookups.InventoryHeldCodeCollection")]
		[RelatedBusinessObject("CurrentHeldCode")]
		[ResourceStringData("WhsInventory|InventoryHeldCode", Caption = "Hold Code")]
		public override ZString WI_HeldCode
		{
			get { return base.WI_HeldCode; }
			set
			{
				base.WI_HeldCode = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode.Name, value);
			}
		}

		[ActionFieldFollow(false)]
		public WhsInventoryHeldCode CurrentHeldCode
		{
			get { return Factory.LoadFromNaturalKey<WhsInventoryHeldCode>(WhsInventoryHeldCodeSchema.WHC_Code, WI_HeldCode); }
		}

		#endregion

		#region OriginalInventoryHeldCode

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.InventoryHeldCodeCollection")]
		[MaxLength(WhsDocketLine.Schema.WE_WHC_NKOriginalInventoryHeldCodeMaxLength)]
		[RelatedBusinessObject(Schema.OriginalHeldCode)]
		[ResourceStringData("WhsInventory|InventoryHeldCode", Caption = "Hold Code")]
		public ZString OriginalInventoryHeldCode
		{
			get
			{
				var inDocketLine = InDocketLine;
				return inDocketLine != null ? inDocketLine.WE_WHC_NKOriginalInventoryHeldCode : originalInventoryHeldCode;
			}
			set
			{
				SetNonPersistentPropertyValue(OriginalInventoryHeldCodeInfo, ref originalInventoryHeldCode, value);

				var inDocketLine = InDocketLine;
				if (inDocketLine != null)
				{
					inDocketLine.WE_WHC_NKOriginalInventoryHeldCode = value;
				}
			}
		}

		public ZPropertyInfo OriginalInventoryHeldCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalInventoryHeldCode); }
		}

		[ActionFieldFollow(false)]
		public WhsInventoryHeldCode OriginalHeldCode
		{
			get { return Factory.LoadFromNaturalKey<WhsInventoryHeldCode>(WhsInventoryHeldCodeSchema.WHC_Code, OriginalInventoryHeldCode); }
		}

		ZString originalInventoryHeldCode;

		#endregion

		#region WI_IsOriginalReceiptLine

		[ActionField(ReadOnly = true)]
		public override ZBool WI_IsOriginalReceiptLine
		{
			get { return base.WI_IsOriginalReceiptLine; }
			set
			{
				base.WI_IsOriginalReceiptLine = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_IsOriginalInventory.Name, value);
			}
		}

		#endregion

		#region WI_PalletID

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_PalletID", Caption = "Pallet ID")]
		public override ZString WI_PalletID
		{
			get { return base.WI_PalletID; }
			set
			{
				base.WI_PalletID = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_PalletID.Name, value);
				ValidationPalletIDWarningMessage = "";
			}
		}

		#endregion

		#region WI_BondedEntryKey

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_BondedEntryKey", Caption = "Entry Key")]
		public override ZString WI_BondedEntryKey
		{
			get { return base.WI_BondedEntryKey; }
			set
			{
				base.WI_BondedEntryKey = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_BondedEntryKey.Name, value);
				bondInfo = null;
			}
		}

		#endregion

		#region WI_BondedEntryKey

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		[ResourceStringData("WhsInventoryView|WI_AllocationKey", Caption = "Allocation Key")]
		public override ZString WI_AllocationKey
		{
			get => base.WI_AllocationKey;
			set
			{
				base.WI_AllocationKey = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_AllocationKey.Name, value);
			}
		}

		#endregion

		#region WI_TotalValue

		[ReadOnly(true)]
		public ZDecimal WI_TotalValue
		{
			get
			{
				WhsProduct whsProduct = Product; // for performance reasons
				return (whsProduct != null) ? whsProduct.Parent.OP_LastCost * WI_TotalUnits : 0m;
			}
		}

		public ZPropertyInfo WI_TotalValueInfo
		{
			get { return GetZPropertyInfo(Schema.WI_TotalValue); }
		}

		#endregion

		#region WI_Currency

		[ReadOnly(true)]
		public ZString WI_Currency
		{
			get
			{
				WhsProduct whsProduct = Product; // for performance reasons
				return (whsProduct != null) ? whsProduct.Parent.OP_RX_NKLastWeightedCostCurr : ZString.Empty;
			}
		}

		public ZPropertyInfo WI_CurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.WI_Currency); }
		}

		#endregion

		#region WI_LastCost

		[ReadOnly(true)]
		public ZDecimal WI_LastCost
		{
			get
			{
				WhsProduct whsProduct = Product; // for performance reasons
				return (whsProduct != null) ? whsProduct.Parent.OP_LastCost : (ZDecimal)0m;
			}
		}

		public ZPropertyInfo WI_LastCostInfo
		{
			get { return GetZPropertyInfo(Schema.WI_LastCost); }
		}

		#endregion

		#region Part Attributes

		#region WI_PartAttrib1

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PartAttrib1InfoReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_PartAttrib1", Caption = "Attribute 1")]
		public override ZString WI_PartAttrib1
		{
			get { return base.WI_PartAttrib1; }
			set
			{
				base.WI_PartAttrib1 = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_PartAttrib1.Name, value);
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WI_PartAttrib1, 1);
			}
		}

		#endregion

		#region WI_PartAttrib2

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PartAttrib2InfoReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_PartAttrib2", Caption = "Attribute 2")]
		public override ZString WI_PartAttrib2
		{
			get { return base.WI_PartAttrib2; }
			set
			{
				base.WI_PartAttrib2 = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_PartAttrib2.Name, value);
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WI_PartAttrib2, 2);
			}
		}

		#endregion

		#region WI_PartAttrib3

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PartAttrib3InfoReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_PartAttrib3", Caption = "Attribute 3")]
		public override ZString WI_PartAttrib3
		{
			get { return base.WI_PartAttrib3; }
			set
			{
				base.WI_PartAttrib3 = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_PartAttrib3.Name, value);
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WI_PartAttrib3, 3);
			}
		}

		#endregion

		#region WI_ExpiryDate

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(ExpiryDateInfoReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_ExpiryDate", Caption = "Expiry Date")]
		public override ZDate WI_ExpiryDate
		{
			get { return base.WI_ExpiryDate; }
			set
			{
				base.WI_ExpiryDate = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_ExpiryDate.Name, value);
			}
		}

		#endregion

		#region WI_PackingDate

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackingDateInfoReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_PackingDate", Caption = "Packing Date")]
		public override ZDate WI_PackingDate
		{
			get { return base.WI_PackingDate; }
			set
			{
				base.WI_PackingDate = value;
				SetValueToDocketLine(WhsDocketLineSchema.WE_PackingDate.Name, value);
			}
		}

		#endregion

		#region WI_SerialNumber

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(SerialNumberReadOnly))]
		[ResourceStringData("WhsInventoryView|WI_SerialNumber", Caption = "Serial Number", ShortCaption = "Serial #")]
		public override ZString WI_SerialNumber
		{
			get => base.WI_SerialNumber;
			set
			{
				var previousValue = base.WI_SerialNumber;
				base.WI_SerialNumber = value;

				if (!IsValidationSuspended && previousValue != value)
				{
					Validation.ValidateWI_ExpectedReceiptQuantity();
					Validation.ValidateWI_InDocketLineUnits();
				}
				SetValueToDocketLine(WhsDocketLineSchema.WE_SerialNumber.Name, value);
			}
		}

		#endregion

		#region SetExpiryDateIfJulianBatchNumberAttributeIsUsed

		void SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(ZString partAttribute, int partAttributeNumber)
		{
			var whsProduct = Product;
			var docket = Docket;
			if (whsProduct != null && docket != null)
			{
				var client = docket.Client;
				if (client != null && whsProduct.IsPartAttributeAJulianBatchNumberAndUsed(client, partAttributeNumber))
				{
					WI_PackingDate = whsProduct.GetPackingDateFromJulianBatchNumber(client, docket.Warehouse, partAttribute);
					WI_ExpiryDate = whsProduct.CalculateExpiryDate(client, docket.Warehouse, partAttribute);
				}
			}
		}

		#endregion

		#endregion

		#region Custom Attributes

		ZString wi_CustomAttrib1;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		[MaxLength(15)]
		public ZString WI_CustomAttrib_1
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomAttrib1 : wi_CustomAttrib1; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomAttrib_1Info, ref wi_CustomAttrib1, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomAttrib1 = value;
				}
			}
		}

		ZString wi_CustomAttrib2;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		[MaxLength(15)]
		public ZString WI_CustomAttrib_2
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomAttrib2 : wi_CustomAttrib2; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomAttrib_2Info, ref wi_CustomAttrib2, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomAttrib2 = value;
				}
			}
		}

		ZString wi_CustomAttrib3;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		[MaxLength(15)]
		public ZString WI_CustomAttrib_3
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomAttrib3 : wi_CustomAttrib3; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomAttrib_3Info, ref wi_CustomAttrib3, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomAttrib3 = value;
				}
			}
		}

		ZString wi_CustomAttrib4;
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[MaxLength(15)]
		public ZString WI_CustomAttrib4
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomAttrib4 : wi_CustomAttrib4; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomAttrib4Info, ref wi_CustomAttrib4, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomAttrib4 = value;
				}
			}
		}

		ZString wi_CustomAttrib5;
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[MaxLength(15)]
		public ZString WI_CustomAttrib5
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomAttrib5 : wi_CustomAttrib5; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomAttrib5Info, ref wi_CustomAttrib5, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomAttrib5 = value;
				}
			}
		}

		ZString wi_CustomAttrib6;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		[MaxLength(15)]
		public ZString WI_CustomAttrib6
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomAttrib6 : wi_CustomAttrib6; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomAttrib6Info, ref wi_CustomAttrib6, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomAttrib6 = value;
				}
			}
		}

		ZDateTime wi_CustomDate1;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDateTime WI_CustomDate1
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDate1 : wi_CustomDate1; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDate1Info, ref wi_CustomDate1, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDate1 = value;
				}
			}
		}

		ZDateTime wi_CustomDate2;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDateTime WI_CustomDate2
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDate2 : wi_CustomDate2; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDate2Info, ref wi_CustomDate2, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDate2 = value;
				}
			}
		}

		ZDateTime wi_CustomDate3;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDateTime WI_CustomDate3
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDate3 : wi_CustomDate3; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDate3Info, ref wi_CustomDate3, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDate3 = value;
				}
			}
		}

		ZDateTime wi_CustomDate4;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDateTime WI_CustomDate4
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDate4 : wi_CustomDate4; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDate4Info, ref wi_CustomDate4, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDate4 = value;
				}
			}
		}

		ZDateTime wi_CustomDate5;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDateTime WI_CustomDate5
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDate5 : wi_CustomDate5; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDate5Info, ref wi_CustomDate5, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDate5 = value;
				}
			}
		}

		ZDecimal wi_CustomDecimal1;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDecimal WI_CustomDecimal1
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDecimal1 : wi_CustomDecimal1; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDecimal1Info, ref wi_CustomDecimal1, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDecimal1 = value;
				}
			}
		}

		ZDecimal wi_CustomDecimal2;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDecimal WI_CustomDecimal2
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDecimal2 : wi_CustomDecimal2; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDecimal2Info, ref wi_CustomDecimal2, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDecimal2 = value;
				}
			}
		}

		ZDecimal wi_CustomDecimal3;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDecimal WI_CustomDecimal3
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDecimal3 : wi_CustomDecimal3; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDecimal3Info, ref wi_CustomDecimal3, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDecimal3 = value;
				}
			}
		}

		ZDecimal wi_CustomDecimal4;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDecimal WI_CustomDecimal4
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDecimal4 : wi_CustomDecimal4; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDecimal4Info, ref wi_CustomDecimal4, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDecimal4 = value;
				}
			}
		}

		ZDecimal wi_CustomDecimal5;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZDecimal WI_CustomDecimal5
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomDecimal5 : wi_CustomDecimal5; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomDecimal5Info, ref wi_CustomDecimal5, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomDecimal5 = value;
				}
			}
		}

		ZBool wi_CustomFlag1;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZBool WI_CustomFlag1
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomFlag1 : wi_CustomFlag1; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomFlag1Info, ref wi_CustomFlag1, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomFlag1 = value;
				}
			}
		}

		ZBool wi_CustomFlag2;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZBool WI_CustomFlag2
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomFlag2 : wi_CustomFlag2; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomFlag2Info, ref wi_CustomFlag2, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomFlag2 = value;
				}
			}
		}

		ZBool wi_CustomFlag3;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZBool WI_CustomFlag3
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomFlag3 : wi_CustomFlag3; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomFlag3Info, ref wi_CustomFlag3, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomFlag3 = value;
				}
			}
		}

		ZBool wi_CustomFlag4;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZBool WI_CustomFlag4
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomFlag4 : wi_CustomFlag4; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomFlag4Info, ref wi_CustomFlag4, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomFlag4 = value;
				}
			}
		}

		ZBool wi_CustomFlag5;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		public ZBool WI_CustomFlag5
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomFlag5 : wi_CustomFlag5; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomFlag5Info, ref wi_CustomFlag5, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomFlag5 = value;
				}
			}
		}

		ZString wi_CustomTextBlob1;
		[ReadOnlyMember(nameof(AttributesReadOnly))]
		[MaxLength(WhsDocketLine.Schema.WE_CustomTextBlob1MaxLength)]
		public ZString WI_CustomTextBlob1
		{
			get { return InDocketLine != null ? InDocketLine.WE_CustomTextBlob1 : wi_CustomTextBlob1; }
			set
			{
				SetNonPersistentPropertyValue(WI_CustomTextBlob1Info, ref wi_CustomTextBlob1, value);
				if (InDocketLine != null)
				{
					InDocketLine.WE_CustomTextBlob1 = value;
				}
			}
		}

		#region Custom Infos

		public ZPropertyInfo WI_CustomAttrib_1Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomAttrib1);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomAttrib_2Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomAttrib2);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomAttrib_3Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomAttrib3);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomAttrib4Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomAttrib4);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomAttrib5Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomAttrib5);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomAttrib6Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomAttrib6);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDate1Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDate1);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDate2Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDate2);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDate3Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDate3);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDate4Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDate4);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDate5Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDate5);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDecimal1Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDecimal1);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDecimal2Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDecimal2);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDecimal3Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDecimal3);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDecimal4Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDecimal4);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomDecimal5Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomDecimal5);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomFlag1Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomFlag1);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomFlag2Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomFlag2);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomFlag3Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomFlag3);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomFlag4Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomFlag4);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomFlag5Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomFlag5);
				return info;
			}
		}

		public ZPropertyInfo WI_CustomTextBlob1Info
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WI_CustomTextBlob1);
				return info;
			}
		}

		#endregion

		#endregion

		#region PackageGroupId

		[ResourceStringData("WhsInventory|PackageGroupId", Caption = "Grouping ID")]
		public ZString PackageGroupId
		{
			get
			{
				var docketLine = InDocketLine;
				return (docketLine != null && !docketLine.WE_PackageGroupId.IsEmpty) ? docketLine.WE_PackageGroupId : ZString.Empty;
			}
		}

		public ZPropertyInfo PackageGroupIdInfo
		{
			get { return GetZPropertyInfo(Schema.PackageGroupId); }
		}

		#endregion

		#region PerPackageQty

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[ResourceStringData("WhsInventory|PerPackageQty", Caption = "Per Group Qty")]
		public ZDecimal PerPackageQty
		{
			get
			{
				var docketLine = InDocketLine;
				return (docketLine != null && !docketLine.WE_PerPackageQty.IsEmpty) ? docketLine.WE_PerPackageQty : perPackageQty;
			}
			set
			{
				SetNonPersistentPropertyValue(PerPackageQtyInfo, ref perPackageQty, value);

				var docketLine = InDocketLine;
				if (docketLine != null)
				{
					docketLine.WE_PerPackageQty = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateWI_InDocketLineUnits(); // tested in WhsInventoryValidation
				}
			}
		}

		public ZPropertyInfo PerPackageQtyInfo
		{
			get { return GetZPropertyInfo(Schema.PerPackageQty); }
		}

		ZDecimal perPackageQty;

		#endregion

		// calculated

		#region IsDamaged

		public bool IsDamaged
		{
			get { return !IsAvailable && (CurrentHeldCode?.IsDamaged ?? false); }
		}

		#endregion

		#region IsExpired

		[ResourceStringData("WhsInventory|IsExpired", Caption = "Is Expired", ShortCaption = "Expired")]
		public ZBool IsExpired => WI_ExpiryDate <= ZDate.Today;

		#endregion

		#region ReceiptReference

		[ResourceStringData("WhsInventory|ReceiptReference", Caption = "Receipt Reference")]
		public ZString ReceiptReference
		{
			get { return InDocketLine?.ReceiptReference ?? ZString.Empty; }
		}

		#endregion

		#region TouchesUntilStocktake

		[ResourceStringData("WhsInventory|TouchesUntilStocktake", Caption = "Touches Until Stocktake", ShortCaption = "Touches Left")]
		public ZInt TouchesUntilStocktake
		{
			get
			{
				var location = CurrentLocation;
				return location != null ? location.WLV_MaximumPickCountBeforeAutomatedStocktake - location.WLV_FinalisedPickCount : 0;
			}
		}

		#endregion

		//

		#region SetValueToDocketLine

		void SetValueToDocketLine(ZString propertyName, IZType newValue)
		{
			var docketLine = InDocketLine;
			if (docketLine != null && !docketLine[propertyName].Equals(newValue))
			{
				var semaphore = docketLine.GetSyncingDocketLineInventoryViewSemaphore(propertyName);
				if (!semaphore.IsSuspended)
				{
					using (new SemaphoreManager(semaphore))
					{
						docketLine[propertyName] = newValue;
					}
				}
			}
		}

		#endregion

		#endregion

		#region Areas

		#region LocationPickArea

		public WhsArea LocationPickArea => Location?.PickingArea;

		#endregion

		#region LocationPickAreaName

		public ZString LocationPickAreaName => LocationPickArea != null ? LocationPickArea.WA_NameMultilingual : ZString.Empty;

		public ZPropertyInfo LocationPickAreaNameInfo => GetZPropertyInfo(Schema.LocationPickAreaName);

		#endregion

		#region LocationPickAreaType

		public ZString LocationPickAreaType => Location?.WLV_PickingAreaType ?? ZString.Empty;

		public ZPropertyInfo LocationPickAreaTypeInfo => GetZPropertyInfo(Schema.LocationPickAreaType);

		#endregion

		#region LocationPutawayAreaType

		public ZString LocationPutawayAreaType => Location?.WLV_PutawayAreaType ?? ZString.Empty;

		#endregion

		#region CurrentLocationPickAreaName

		public ZString CurrentLocationPickAreaName => (InDocketLine as WhsReceiveLine)?.CurrentLocationPickAreaName ?? LocationPickAreaName;

		#endregion

		#region CurrentLocationPickAreaType

		public ZString CurrentLocationPickAreaType => (InDocketLine as WhsReceiveLine)?.CurrentLocationPickAreaType ?? LocationPickAreaType;

		#endregion

		#endregion

		#region CurrentLocationType

		public ZString CurrentLocationType
		{
			get { return CurrentLocation?.LocationType?.WLT_Code ?? ZString.Empty; }
		}

		public ZPropertyInfo CurrentLocationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentLocationType); }
		}

		#endregion

		#region CurrentLocationStatusCode

		public ZString CurrentLocationStatusCode
		{
			get { return CurrentLocation?.WLV_LocationStatus ?? ZString.Empty; }
		}

		public ZPropertyInfo CurrentLocationStatusCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentLocationStatusCode); }
		}

		#endregion

		#region CurrentLocationPutawayPathSequence

		public ZInt CurrentLocationPutawayPathSequence
		{
			get { return CurrentLocation?.WLV_PutawayPathSequence ?? int.MaxValue; }
		}

		#endregion

		#region CurrentPickMethod

		public ZString CurrentPickMethod
		{
			get { return CurrentLocation?.WLV_PickMethod ?? ZString.Empty; }
		}

		public ZPropertyInfo CurrentPickMethodInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentPickMethod); }
		}

		#endregion

		#region Validation

		protected override WhsInventoryViewValidation GetNewValidation()
		{
			switch (CountryCode)
			{
				case Constants.CountryCodes.UnitedStates:
					return new US.WhsInventoryValidation(this);

				default:
					return base.GetNewValidation();
			}
		}

		public ZString ValidationProductWarningMessage
		{
			get { return validationProductWarningMessage; }
			set
			{
				if (value != validationProductWarningMessage)
				{
					validationProductWarningMessage = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateWI_OP();
						InDocketLine.Validation.ValidateWE_OP();
					}
				}
			}
		}
		ZString validationProductWarningMessage = "";

		public ZString ValidationPalletIDWarningMessage
		{
			get { return validationPalletIDWarningMessage; }
			set
			{
				if (value != validationPalletIDWarningMessage)
				{
					validationPalletIDWarningMessage = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateWI_PalletID();

						var docketLine = InDocketLine;
						if (docketLine != null)
						{
							docketLine.Validation.ValidateWE_PalletID();
						}
					}
				}
			}
		}
		ZString validationPalletIDWarningMessage = "";

		public void SetValidationPartAttribWarningMessage(int attributeNumber, string message)
		{
			if (attributeNumber < 1 || attributeNumber > 3)
			{
				throw new ArgumentException("Valid range for attributes is 1-3.");
			}
			if (message != validationPartAttribWarningMessage[attributeNumber])
			{
				validationPartAttribWarningMessage[attributeNumber] = message;
				if (!IsValidationSuspended)
				{
					if (attributeNumber == 1)
					{
						Validation.ValidateWI_PartAttrib1();
					}
					else if (attributeNumber == 2)
					{
						Validation.ValidateWI_PartAttrib2();
					}
					else if (attributeNumber == 3)
					{
						Validation.ValidateWI_PartAttrib3();
					}
				}
			}
		}

		public IReadOnlyList<ZString> ValidationPartAttribWarningMessage
		{
			get { return validationPartAttribWarningMessage; }
		}

		readonly ZString[] validationPartAttribWarningMessage = new ZString[4];

		public ZString ValidationSerialNumberWarningMessage
		{
			get => validationSerialNumberWarningMessage;
			set
			{
				if (value != validationSerialNumberWarningMessage)
				{
					validationSerialNumberWarningMessage = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateWI_SerialNumber();
					}
				}
			}
		}
		ZString validationSerialNumberWarningMessage = "";

		public bool CanCommitToTransactionLine(ILineWithCommittedPickLines transactionLine)
		{
			return CanCommitToTransactionLineIfVasOrderTransferInInventory(transactionLine);
		}

		bool CanCommitToTransactionLineIfVasOrderTransferInInventory(ILineWithCommittedPickLines transactionLine)
		{
			var result = true;
			if (WI_InDocketLineType.EqualsIgnoringCase(DocketType.Codes.Transfer)
				&& InDocketLine is WhsTransferLine transferLine
				&& !transferLine.IsPutawayTransferLine)
			{
				var vasOrder = Factory.LoadTop1<WhsVASOrder>(new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, transferLine.WE_WD));
				if (vasOrder != null)
				{
					result = vasOrder.IsFinalised || transactionLine is WhsTransferLine transferTransactionLine && transferTransactionLine.IsOutOfServiceAreaTransferForVASOrder;
				}
			}

			return result;
		}

		#endregion

		#region Find Attributes

		public void UseChosenInventoryRow(WhsInventoryView inventory)
		{
			Argument.NotNull(inventory, "inventory");

			if (CanUpdateFromInventory)
			{
				WI_OP = inventory.WI_OP;
				SetAttributes(inventory);
				SetCustomAttributes(inventory);

				if (WI_WL.IsEmpty)
				{
					WI_WL = inventory.WI_WL;
				}
			}
			else
			{
				Docket.NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			}
		}

		bool CanUpdateFromInventory
		{
			get { return !IsDocketLineFinalised && !IsDocketCancelled; }
		}

		bool IsDocketCancelled
		{
			get
			{
				var docket = Docket;
				return docket != null && docket.IsCancelled;
			}
		}

		#endregion

		#region ReadOnly Control

		#region PartAttrib1InfoReadOnly

		protected bool PartAttrib1InfoReadOnly
		{
			get
			{
				return Docket == null ||
					AttributesReadOnly ||
					(SupplierPart != null && !Product.IsPartAttributeUsed(Docket.Client, 1) && WI_PartAttrib1.IsEmpty) ||
					Client == null ||
					(IsTemporaryProduct && Client.MiscServ.OM_IMPartAttrib1Type.IsEmpty);
			}
		}

		#endregion

		#region PartAttrib2InfoReadOnly

		protected bool PartAttrib2InfoReadOnly
		{
			get
			{
				return Docket == null ||
					AttributesReadOnly ||
					(SupplierPart != null && !Product.IsPartAttributeUsed(Docket.Client, 2) && WI_PartAttrib2.IsEmpty) ||
					Client == null ||
					(IsTemporaryProduct && Client.MiscServ.OM_IMPartAttrib2Type.IsEmpty);
			}
		}

		#endregion

		#region PartAttrib3InfoReadOnly

		protected bool PartAttrib3InfoReadOnly
		{
			get
			{
				return Docket == null ||
					AttributesReadOnly ||
					(SupplierPart != null && !Product.IsPartAttributeUsed(Docket.Client, 3) && WI_PartAttrib3.IsEmpty) ||
					Client == null ||
					(IsTemporaryProduct && Client.MiscServ.OM_IMPartAttrib3Type.IsEmpty);
			}
		}

		#endregion

		#region SerialNumberReadOnly

		// must be protected for Tracking to see this property
		protected bool SerialNumberReadOnly
		{
			get
			{
				var client = Client;
				var docket = Docket;
				return docket == null ||
					AttributesReadOnly ||
					(SupplierPart != null && !Product.IsSerialNumberUsed(docket.Client) && WI_SerialNumber.IsEmpty) ||
					client == null ||
					(IsTemporaryProduct && !client.MiscServ.OM_IMUseSerialNumber);
			}
		}

		#endregion

		#region ExpiryDateInfoReadOnly

		protected bool ExpiryDateInfoReadOnly
		{
			get
			{
				var result = true;
				var docket = Docket;
				if (docket != null && !AttributesReadOnly)
				{
					var whsProduct = Product;
					var client = Client;
					result = client == null || (whsProduct != null && !whsProduct.IsExpiryDateUsed(client) && WI_ExpiryDate.IsEmpty)
						|| (IsTemporaryProduct && !client.MiscServ.OM_IMUseExpiryDate)
						|| (whsProduct != null && whsProduct.IsAJulianBatchNumberAttributeUsed(client));
				}
				return result;
			}
		}

		#endregion

		#region PackingDateInfoReadOnly

		protected bool PackingDateInfoReadOnly
		{
			get
			{
				var result = true;
				var docket = Docket;
				if (docket != null && !AttributesReadOnly)
				{
					var whsProduct = Product;
					var client = Client;
					result = client == null || (whsProduct != null && !whsProduct.IsPackingDateUsed(client) && WI_PackingDate.IsEmpty)
						|| (IsTemporaryProduct && !client.MiscServ.OM_IMUsePackingDate)
						|| (whsProduct != null && whsProduct.IsAJulianBatchNumberAttributeUsed(client));
				}
				return result;
			}
		}

		#endregion

		#region IsInventoryEditForm

		public bool IsInventoryEditForm
		{
			get { return isInventoryEditForm; }
			set
			{
				isInventoryEditForm = value;

				var bondedWarehouseCustomsData = CustomsData;
				if (bondedWarehouseCustomsData != null)
				{
					bondedWarehouseCustomsData.SetReadOnlyIncludingChildren(isInventoryEditForm);
				}
			}
		}

		bool isInventoryEditForm;

		#endregion

		#region StandardReadOnly

		protected virtual bool StandardReadOnly
		{
			get { return AttributesReadOnly || IsReceiveCreatedFromWorkOrder; }
		}

		bool IsReceiveCreatedFromWorkOrder
		{
			get
			{
				var receive = Docket as WhsReceive;
				return (receive != null && receive.IsCreatedFromWorkOrder);
			}
		}

		#endregion

		#region AttributesReadOnly

		protected bool AttributesReadOnly
		{
			get { return IsInventoryEditForm || IsDocketLineFinalised || IsDocketFinalised || IsDocketCancelled; }
		}

		bool IsDocketFinalised
		{
			get
			{
				var docket = Docket;
				return docket != null && docket.IsFinalised;
			}
		}

		#endregion

		#region IsLocationReadOnly

		protected bool IsLocationReadOnly
		{
			get { return WarehouseOnDocket == null || AttributesReadOnly; }
		}

		#endregion

		#region TempProductReadOnly

		protected bool TempProductReadOnly
		{
			get { return StandardReadOnly || !IsTemporaryProduct; }
		}

		#endregion

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		public ZString Code
		{
			get { return "!FINDBOX!" + PK.ToString(); } // May be an identifier
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		public ZString Description
		{
			get { return "UNUSED"; }
		}

		#endregion

		#region ILineAttributes Members

		ZDate IPartAttributes.ExpiryDate => WI_ExpiryDate;
		ZDate IPartAttributes.PackingDate => WI_PackingDate;
		ZString IPartAttributes.PartAttrib1 => WI_PartAttrib1;
		ZString IPartAttributes.PartAttrib2 => WI_PartAttrib2;
		ZString IPartAttributes.PartAttrib3 => WI_PartAttrib3;
		ZString IPartAttributes.SerialNumber => WI_SerialNumber;
		ZString ILineAttributes.BondedEntryKey => WI_BondedEntryKey;
		ZString ILineAttributes.AllocationKey => WI_AllocationKey;

		public void SetAttributes(ILineAttributes source)
		{
			WI_BondedEntryKey = source.BondedEntryKey.ToUpper();
			WI_AllocationKey = source.AllocationKey;
			WI_ExpiryDate = source.ExpiryDate;
			WI_PackingDate = source.PackingDate;
			WI_PartAttrib1 = source.PartAttrib1;
			WI_PartAttrib2 = source.PartAttrib2;
			WI_PartAttrib3 = source.PartAttrib3;
			WI_SerialNumber = source.SerialNumber;
		}

		#endregion

		#region ILineCustomAttributes Members

		ZString ILineCustomAttributes.CustomAttrib1
		{
			get { return WI_CustomAttrib_1; }
		}

		ZString ILineCustomAttributes.CustomAttrib2
		{
			get { return WI_CustomAttrib_2; }
		}

		ZString ILineCustomAttributes.CustomAttrib3
		{
			get { return WI_CustomAttrib_3; }
		}

		ZString ILineCustomAttributes.CustomAttrib4
		{
			get { return WI_CustomAttrib4; }
		}

		ZString ILineCustomAttributes.CustomAttrib5
		{
			get { return WI_CustomAttrib5; }
		}

		ZString ILineCustomAttributes.CustomAttrib6
		{
			get { return WI_CustomAttrib6; }
		}

		ZDecimal ILineCustomAttributes.CustomDecimal1
		{
			get { return WI_CustomDecimal1; }
		}

		ZDecimal ILineCustomAttributes.CustomDecimal2
		{
			get { return WI_CustomDecimal2; }
		}

		ZDecimal ILineCustomAttributes.CustomDecimal3
		{
			get { return WI_CustomDecimal3; }
		}

		ZDecimal ILineCustomAttributes.CustomDecimal4
		{
			get { return WI_CustomDecimal4; }
		}

		ZDecimal ILineCustomAttributes.CustomDecimal5
		{
			get { return WI_CustomDecimal5; }
		}

		ZDateTime ILineCustomAttributes.CustomDate1
		{
			get { return WI_CustomDate1; }
		}

		ZDateTime ILineCustomAttributes.CustomDate2
		{
			get { return WI_CustomDate2; }
		}

		ZDateTime ILineCustomAttributes.CustomDate3
		{
			get { return WI_CustomDate3; }
		}

		ZDateTime ILineCustomAttributes.CustomDate4
		{
			get { return WI_CustomDate4; }
		}

		ZDateTime ILineCustomAttributes.CustomDate5
		{
			get { return WI_CustomDate5; }
		}

		ZBool ILineCustomAttributes.CustomFlag1
		{
			get { return WI_CustomFlag1; }
		}

		ZBool ILineCustomAttributes.CustomFlag2
		{
			get { return WI_CustomFlag2; }
		}

		ZBool ILineCustomAttributes.CustomFlag3
		{
			get { return WI_CustomFlag3; }
		}

		ZBool ILineCustomAttributes.CustomFlag4
		{
			get { return WI_CustomFlag4; }
		}

		ZBool ILineCustomAttributes.CustomFlag5
		{
			get { return WI_CustomFlag5; }
		}

		ZString ILineCustomAttributes.CustomTextBlob1
		{
			get { return WI_CustomTextBlob1; }
		}

		public void SetCustomAttributes(ILineCustomAttributes source)
		{
			WI_CustomAttrib_1 = source.CustomAttrib1;
			WI_CustomAttrib_2 = source.CustomAttrib2;
			WI_CustomAttrib_3 = source.CustomAttrib3;
			WI_CustomAttrib4 = source.CustomAttrib4;
			WI_CustomAttrib5 = source.CustomAttrib5;
			WI_CustomAttrib6 = source.CustomAttrib6;

			WI_CustomDecimal1 = source.CustomDecimal1;
			WI_CustomDecimal2 = source.CustomDecimal2;
			WI_CustomDecimal3 = source.CustomDecimal3;
			WI_CustomDecimal4 = source.CustomDecimal4;
			WI_CustomDecimal5 = source.CustomDecimal5;

			WI_CustomDate1 = source.CustomDate1;
			WI_CustomDate2 = source.CustomDate2;
			WI_CustomDate3 = source.CustomDate3;
			WI_CustomDate4 = source.CustomDate4;
			WI_CustomDate5 = source.CustomDate5;

			WI_CustomFlag1 = source.CustomFlag1;
			WI_CustomFlag2 = source.CustomFlag2;
			WI_CustomFlag3 = source.CustomFlag3;
			WI_CustomFlag4 = source.CustomFlag4;
			WI_CustomFlag5 = source.CustomFlag5;

			WI_CustomTextBlob1 = source.CustomTextBlob1;
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		public bool IsRegisteredForUniqueSerialNumberChecking
		{
			get
			{
				var result = true;
				var receive = Docket as WhsReceive;
				if (receive != null)
				{
					result = WI_InDocketLineUnits > 0 || (!receive.StartedReceiving && WI_ExpectedReceiptQuantity > 0);
				}

				return result;
			}
		}

		public bool IsValidForUniqueSerialNumberChecking(ZString serialNumber)
		{
			bool result = false;
			if (!serialNumber.IsEmpty)
			{
				var client = Client;
				if (client != null && Docket != null)
				{
					var whsProduct = Product;
					if (whsProduct != null)
					{
						result = product.IsSerialNumberUsed(client) && !product.IsSerialNumberReleaseCaptured(client);
					}
				}
			}
			return result;
		}

		public bool IsSerialNumberUsedOnThis(ZString serialNumber)
		{
			return WI_SerialNumber == serialNumber && IsValidForUniqueSerialNumberChecking(serialNumber);
		}

		public bool IsSerialNumberUsedOnSiblings(ZString serialNumber)
		{
			bool result = false;
			WhsReceive receive = Docket as WhsReceive;
			if (receive != null)
			{
				bool shouldBeUniqueByProduct = WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct;
				ZGuid partPK = WI_OP; // for performance reasons

				foreach (WhsInventoryView line in receive.Inventory)
				{
					if (line.PK != PK && line.IsRegisteredForUniqueSerialNumberChecking)
					{
						result = shouldBeUniqueByProduct
							? (line.WI_OP == partPK && line.IsSerialNumberUsedOnThis(serialNumber))
							: line.IsSerialNumberUsedOnThis(serialNumber);

						if (result)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		public bool IsInventoryAdjustedOutOnSiblings(WhsInventoryView inventory)
		{
			return false;
		}

		#endregion

		#region ILocationConsumer Members

		ZGuid ILocationConsumer.LocationPK
		{
			get { return WI_WL; }
		}

		ZGuid ILocationConsumer.WarehousePK
		{
			get { return LocationWhsGuid; }
		}

		ZString ILocationConsumer.LocationTypeForMessages
		{
			get { return Res.GetString("e9128929-eed8-4c8f-baf6-99fcf533bd2a", "Inventory"); }
		}

		ZString ILocationConsumer.LocationTitle { get; set; }

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.WarehouseInventory)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IParentDocManagerSupport Members

		public ZGuid ParentGuid
		{
			get
			{
				return InDocketLine?.PK ?? ZGuid.Empty;
			}
		}

		public ZString ParentTableName
		{
			get
			{
				return InDocketLine?.TableName ?? ZString.Empty;
			}
		}

		#endregion

		#region IDocumentSupportable Members

		public event EventHandler<WhsDocumentInventoryEventArgs> OnInventoryPrint;

		public void CallOnInventoryPrint(object sender, WhsDocumentInventoryEventArgs e)
		{
			OnInventoryPrint?.Invoke(sender, e);
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsInventoryDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return ((ICustomLabelsConfigOrgProvider)InDocketLine)?.ConfigOrg; }
		}

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				var docketLineConfigOrgProvider = ((ICustomLabelsConfigOrgProvider)InDocketLine);
				if (docketLineConfigOrgProvider != null)
				{
					docketLineConfigOrgProvider.ConfigOrgChanged += value;
				}
			}

			remove
			{
				var docketLineConfigOrgProvider = ((ICustomLabelsConfigOrgProvider)InDocketLine);
				if (docketLineConfigOrgProvider != null)
				{
					docketLineConfigOrgProvider.ConfigOrgChanged -= value;
				}
			}
		}

		#endregion

		#region ICustomLabelsProvider Members

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				ConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider { get; }

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result;
				if (configOrg != null)
				{
					var key = string.Format(Culture.Invariant, "WhsInventory.CustomLabelsProvider:{0}", configOrg.PK.ToStringKey()); // Key used for Factory Cache
					result = configOrg.Factory.GetCachedValue(key, () => GetCustomFieldsNoCache(configOrg, factory));
				}
				else
				{
					result = GetCustomFieldsNoCache(configOrg, factory);
				}

				return result;
			}

			CustomLabelInfoList GetCustomFieldsNoCache(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var result = new CustomLabelInfoList(typeof(WhsInventoryView), configOrg, ResString.GetMultilingualString("cdb592ef-7c57-4a7f-b082-01504bc70730", "the client"), factory);
				result.Capacity = 22;

				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute1, WhsInventoryView.Schema.WI_CustomAttrib1, ResString.GetMultilingualString("84980dba-20d1-4573-80a9-e1f33d0cb2f6", "Custom Attribute 1"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute2, WhsInventoryView.Schema.WI_CustomAttrib2, ResString.GetMultilingualString("9c48841d-b990-4e00-902d-560bf3871e4f", "Custom Attribute 2"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute3, WhsInventoryView.Schema.WI_CustomAttrib3, ResString.GetMultilingualString("3d561dcc-844c-4c99-aa93-abc378df0ec5", "Custom Attribute 3"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute4, WhsInventoryView.Schema.WI_CustomAttrib4, ResString.GetMultilingualString("82295251-d897-43be-bdbb-d5f08774ec42", "Custom Attribute 4"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute5, WhsInventoryView.Schema.WI_CustomAttrib5, ResString.GetMultilingualString("8530c8a6-655e-46c4-b8e5-d01953fd7e1e", "Custom Attribute 5"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute6, WhsInventoryView.Schema.WI_CustomAttrib6, ResString.GetMultilingualString("81e3f3c1-fc0f-4ad5-bfac-eb4d1610cbb6", "Custom Attribute 6"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate1, WhsInventoryView.Schema.WI_CustomDate1, ResString.GetMultilingualString("13a6d695-274e-4f2f-9fdf-eec1446047a5", "Custom Date 1"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate2, WhsInventoryView.Schema.WI_CustomDate2, ResString.GetMultilingualString("3926f3e0-03ec-497e-a024-79b0560bf762", "Custom Date 2"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate3, WhsInventoryView.Schema.WI_CustomDate3, ResString.GetMultilingualString("1eab5310-a54e-4aeb-aa77-8577e1fc9536", "Custom Date 3"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate4, WhsInventoryView.Schema.WI_CustomDate4, ResString.GetMultilingualString("044762c2-885f-4d56-9dbb-dd39aae47281", "Custom Date 4"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate5, WhsInventoryView.Schema.WI_CustomDate5, ResString.GetMultilingualString("47aa5180-05cd-4486-957a-17bfc0c177ef", "Custom Date 5"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal1, WhsInventoryView.Schema.WI_CustomDecimal1, ResString.GetMultilingualString("60064340-7c89-431b-ad17-e09ef88b6038", "Custom Number 1"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal2, WhsInventoryView.Schema.WI_CustomDecimal2, ResString.GetMultilingualString("381df3f4-6bcd-46eb-9d05-f6e8b9386fbd", "Custom Number 2"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal3, WhsInventoryView.Schema.WI_CustomDecimal3, ResString.GetMultilingualString("a05072d8-d7a7-43e7-a9b4-94d6ec7ec1d4", "Custom Number 3"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal4, WhsInventoryView.Schema.WI_CustomDecimal4, ResString.GetMultilingualString("6b63c5ed-c484-4174-b8fa-efbc66a4893e", "Custom Number 4"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal5, WhsInventoryView.Schema.WI_CustomDecimal5, ResString.GetMultilingualString("b49c0090-6104-491e-9825-943c820e59c1", "Custom Number 5"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag1, WhsInventoryView.Schema.WI_CustomFlag1, ResString.GetMultilingualString("08053fbf-f70a-44fb-a116-93026bc814d0", "Custom Flag 1"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag2, WhsInventoryView.Schema.WI_CustomFlag2, ResString.GetMultilingualString("6e440b9a-64bd-443d-ad18-9e59488c7c40", "Custom Flag 2"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag3, WhsInventoryView.Schema.WI_CustomFlag3, ResString.GetMultilingualString("47fb1e50-14af-42b2-9055-d46057d59fec", "Custom Flag 3"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag4, WhsInventoryView.Schema.WI_CustomFlag4, ResString.GetMultilingualString("1e863418-b8ab-4009-93c0-91880e97dcc7", "Custom Flag 4"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag5, WhsInventoryView.Schema.WI_CustomFlag5, ResString.GetMultilingualString("abac26b6-1bc5-4619-8dfa-2d59585249b0", "Custom Flag 5"));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomTextBlob1, WhsInventoryView.Schema.WI_CustomTextBlob1, ResString.GetMultilingualString("68d062e4-a07b-45ad-b63a-32d5eb3103b4", "Custom Text 1"));

				return result;
			}
		}

		#endregion

		#region IOrgSupplierPartDefaults Members

		public void SetupSupplierPart(OrgSupplierPart newPart)
		{
			if (newPart.OP_PartNum.IsEmpty)
			{
				newPart.OP_PartNum = WI_OP_PartNum;
			}

			if (!CommodityCode.IsEmpty)
			{
				newPart.OP_RH_NKCommodityCode = CommodityCode;
			}

			SetNewPartAttributesUse(newPart);
		}

		void SetNewPartAttributesUse(OrgSupplierPart newPart)
		{
			var clientMiscServ = Client?.MiscServ;
			if (clientMiscServ != null)
			{
				foreach (OrgPartRelation partRelation in newPart.RelatedOrganisations)
				{
					partRelation.OU_UsePartAttrib1 = !clientMiscServ.OM_IMPartAttrib1Type.IsEmpty && !WI_PartAttrib1.IsEmpty;
					partRelation.OU_UsePartAttrib2 = !clientMiscServ.OM_IMPartAttrib2Type.IsEmpty && !WI_PartAttrib2.IsEmpty;
					partRelation.OU_UsePartAttrib3 = !clientMiscServ.OM_IMPartAttrib3Type.IsEmpty && !WI_PartAttrib3.IsEmpty;
					partRelation.OU_UseSerialNumber = clientMiscServ.OM_IMUseSerialNumber && !WI_SerialNumber.IsEmpty;
					partRelation.OU_UseExpiryDate = clientMiscServ.OM_IMUseExpiryDate && !WI_ExpiryDate.IsEmpty;
					partRelation.OU_UsePackingDate = clientMiscServ.OM_IMUsePackingDate && !WI_PackingDate.IsEmpty;
				}
			}
		}

		#endregion

		#region ISupportTemporaryProduct Members

		ISupportTemporaryProduct[] ISupportTemporaryProduct.GetSiblingsWithTheSameProduct()
		{
			var receive = Docket as WhsReceive;
			return receive != null
				? receive.Inventory.Cast<WhsInventoryView>().Where(i => i.WI_OP_PartNum == WI_OP_PartNum).Cast<ISupportTemporaryProduct>().ToArray()
				: Array.Empty<ISupportTemporaryProduct>();
		}

		ZString ISupportTemporaryProduct.ProductUQ
		{
			get { return WI_UnitsUQ; }
			set { WI_UnitsUQ = value; }
		}

		ZPropertyInfo ISupportTemporaryProduct.ProductUQInfo
		{
			get { return WI_UnitsUQInfo; }
		}

		#endregion

		#region PalletLabelAutoPrinter

		public void PrintPalletIdLabel(Guid printerPK, int numberOfLabelsToPrint) => PalletLabelPrinter.PrintPalletLabel(printerPK, numberOfLabelsToPrint);

		public event EventHandler<PrintFailedEventArgs> PalletLabelPrintFailed
		{
			add { PalletLabelPrinter.PrintFailed += value; }
			remove { PalletLabelPrinter.PrintFailed -= value; }
		}

		PalletLabelAutoPrinter PalletLabelPrinter => palletLabelPrinter ?? (palletLabelPrinter = new PalletLabelAutoPrinter(this));
		PalletLabelAutoPrinter palletLabelPrinter;

		#endregion

		#region ICalculateProductPackageTotals

		ZString ICalculateProductPackageTotals.PackageGroupID => PackageGroupId;

		ZDecimal ICalculateProductPackageTotals.PerPackageQty => PerPackageQty;

		ZGuid ICalculateProductPackageTotals.ProductPK => WI_OP;

		ZGuid ICalculateProductPackageTotals.LocationPK => WI_WL;

		ZGuid ICalculateProductPackageTotals.ClientPK => WI_OH_Client;

		ZDecimal ICalculateProductPackageTotals.Units => WI_InDocketLineUnits;

		#endregion

		#region Implementation

		internal WhsInventoryView Split(ZDecimal quantityForNewLine)
		{
			if (quantityForNewLine >= WI_InDocketLineUnits)
			{
				throw new ArgumentException("Should not be splitting Inventory by an amount greater than or equal to its Quantity.");
			}

			var newLine = (WhsInventoryView)Clone();
			((WhsReceive)Docket).Inventory.Add(newLine);
			newLine.WI_ExpectedReceiptQuantity = newLine.WI_TotalUnits = newLine.WI_InDocketLineUnits = quantityForNewLine;

			ZDecimal saveExpectedReceiptQuantity = WI_ExpectedReceiptQuantity;

			// Splitting should have validation suspended because invalid errors may occur such as:
			// When Splitting Reserved Inventory, we re-allocate the Reserved Pick Lines after setting
			// the Inventory Quantity so the Validation will be run too early. Thus we need to suspend.
			using (GetValidationSuspender())
			{
				WI_InDocketLineUnits -= quantityForNewLine;
				WI_TotalUnits = WI_InDocketLineUnits;
				WI_ExpectedReceiptQuantity = saveExpectedReceiptQuantity - quantityForNewLine;
				if (WI_ExpectedReceiptQuantity < 0)
				{
					newLine.WI_ExpectedReceiptQuantity += WI_ExpectedReceiptQuantity;
					WI_ExpectedReceiptQuantity = 0;
				}
			}

			return newLine;
		}

#if DEBUG
		#region FillWithValidTestData

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var helper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var randomizer = new Random(ZGuid.BrettsGuid.GetHashCode());
			var warehouse = (WhsWarehouse)helper.CreateWarehouse("1", "A", 1, 1);
			var org = (OrgHeader)helper.CreateClient("111", "111");
			org.OH_Code = randomizer.Next(100000).ToString(Culture.Invariant);
			warehouse.WW_WarehouseCode = randomizer.Next(1000).ToString(Culture.Invariant);

			var receivePK = helper.CreateWhsReceive(org.PK, warehouse.PK, randomizer.Next(100000).ToString(Culture.Invariant), new Environment.Business.Testing.TestNotificationBuffer());
			var receive = Factory.Load<WhsReceive>(receivePK);
			var receiveLine = receive.Lines.AddNew();

			var product = helper.CreateProduct(org.PK, "P1");
			WI_OH_Client = org.PK;
			WI_WD = receivePK;
			WI_OP = product.PK;
			WI_InDocketLineType = DocketType.Codes.Receive;
			WI_WE_InDocketLine = receiveLine.PK;
			WI_WE_OriginalInDocketLineForRating = receiveLine.PK;
			InDocketLine.WE_OP = product.PK;
			WI_F3_NKPackType = receiveLine.WE_F3_NKPackType;

			receive.RunPreSaveValidation(); // Create/sync docket line

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		#endregion

		#region OnInventoryCreatedFromTransferLine_ForTest

		void OnInventoryCreatedFromTransferLine_ForTest(WhsInventoryView childLine)
		{
			if (InventoryCreatedFromTransferLine_ForTest != null)
			{
				InventoryCreatedFromTransferLine_ForTest(childLine, EventArgs.Empty);
			}
		}

		public event EventHandler InventoryCreatedFromTransferLine_ForTest;

		#endregion

		#region ClearProductForTesting

		public void ClearProductForTesting()
		{
			product = null;
		}

		#endregion
#endif

		WhsProduct product;

		#endregion
	}

	#region Document Supporter

	public class WhsInventoryDocumentSupporter : DocumentSupporter
	{
		public WhsInventoryDocumentSupporter(WhsInventoryView inventory)
			: base(inventory)
		{
		}

		protected WhsInventoryView Inventory
		{
			get { return (WhsInventoryView)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsInventory; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsInventoryCustomiseDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.WhsInventory,
				Constants.DataContext.WhsReceive,
				Constants.DataContext.GenericFreightJob
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			switch (dataContext)
			{
				case Constants.DataContext.GenericProductLabel:
					result = GetProductLabelDocumentWrappers();
					break;
				case Constants.DataContext.WhsInventory:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.WhsInventory, Inventory.InDocketLine) };
					break;
				case Constants.DataContext.GenericFreightJob:
					var docket = Inventory.Docket;
					if (docket != null)
					{
						docket.InventoryToPrintPalletLabelFor = Inventory;
					}
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, docket);
					break;
				case Constants.DataContext.WhsReceive:
					var receive = Inventory.DocketOriginal as WhsReceive;
					if (receive != null)
					{
						receive.InventoryToPrintPalletLabelFor = Inventory;
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.WhsReceive, receive) };
					}
					break;
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);

			if (dataContextValue.DataContext == Constants.DataContext.GenericProductLabel)
			{
				message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoProductLabelToPrint", "Cannot find Product Label to print.");
			}
			else if (dataContextValue.DataContext == Constants.DataContext.WhsInventory)
			{
				message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoAssociatedDocketLine", "Cannot find Docket Line.");
			}
			else if (dataContextValue.DataContext == Constants.DataContext.GenericFreightJob)
			{
				message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoDocket", "Cannot find Docket.");
			}
			else if (dataContextValue.DataContext == Constants.DataContext.WhsReceive)
			{
				message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoDocketOriginal", "Cannot find Original Docket.");
			}
			return message;
		}

		DocumentWrapper[] GetProductLabelDocumentWrappers()
		{
			var inventories = new WhsInventoryViewCollection(Factory);
			inventories.Add(Inventory);

			var result = Array.Empty<DocumentWrapper>();
			var options = new WhsDocumentInventoryOptions(inventories);
			var optionArgs = new WhsDocumentInventoryEventArgs(options);

			Inventory.CallOnInventoryPrint(this, optionArgs);
			if (optionArgs.ContinueToPrint)
			{
				var wrappers = new List<DocumentWrapper>();

				foreach (WhsDocumentInventory documentInventory in options.DocumentInventories)
				{
					if (documentInventory.LabelsToPrint > 0)
					{
						var wrapper = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, GetDocket(Inventory))[0];
						var iWrapper = (IWhsDocumentInventory)wrapper;
						iWrapper.SetInventory(documentInventory);
						wrappers.Add(wrapper);
					}
				}

				result = wrappers.ToArray();
			}

			return result;
		}

		WhsDocket GetDocket(WhsInventoryView inventory)
		{
			var docket = inventory.Docket;
			var transfer = docket as WhsTransfer;
			if (transfer != null && !transfer.IsInterWarehouseTransfer)
			{
				docket = inventory.DocketOriginal;
			}

			return docket;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact result = new OrgHeaderContact(Inventory.Client, null);
			return result;
		}

		protected override bool GetIsNonPersistent()
		{
			return true;
		}
	}

	#endregion
}
