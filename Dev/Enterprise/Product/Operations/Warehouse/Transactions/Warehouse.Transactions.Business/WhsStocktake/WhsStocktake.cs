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
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[CodeProperty(WhsStocktakeSchema.Constants.WS_StocktakeNumber), DescriptionProperty(WhsStocktakeSchema.Constants.WS_StocktakeNumber)]
	public sealed partial class WhsStocktake :
		AutoWhsStocktake,
		Enterprise.Integration.Warehouse.IWhsStocktake,
		IJobInvoicingPlugIn,
		IDocManagerSupport,
		IDocumentSupportable,
		ILocationConsumer,
		IEDocsProvider,
		ISendEmailSource,
		IMasterStaffAssigner,
		IWorkflowProvider,
		IWhsLogEventParent,
		INumberFountainConsumer,
		ILocationCapacityMaster<WhsStocktakeLine>,
		IValidateParentWithLines
	{
		#region Constructors

		public WhsStocktake(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Default values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.New;
			WS_StocktakeDate = ZDateTime.Now;
			WS_StocktakeType = WarehouseDataRegistry.Instance.StocktakeTypes.Value.DefaultCode;
		}

		#endregion

		#region Related Entities

		#region ClientAddress

		public OrgAddress ClientAddress
		{
			get { return Client != null ? Client.MainAddress : null; }
		}

		#endregion

		#region ClosingLines

		internal IEnumerable<WhsStocktakeLine> ClosingLines
		{
			get { return closingLines ?? Enumerable.Empty<WhsStocktakeLine>(); }
			private set { closingLines = value; }
		}

		IEnumerable<WhsStocktakeLine> closingLines;

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WS_WW_Whs); }
		}

		#endregion

		#region SelectedRow

		public WhsRow SelectedRow
		{
			get { return Factory.Load<WhsRow>(WS_WR_Row); }
		}

		#endregion

		#region SelectedArea

		public WhsArea SelectedArea
		{
			get { return Factory.Load<WhsArea>(WS_WA_Area); }
		}

		#endregion

		#region Lines

		[ChildEditable(false)]
		public WhsStocktakeLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new WhsStocktakeLineCollection(this);
					RegisterEditableChildObject(lines);
					lines.CountChanged += delegate
					{ CheckAndUpdateStatus(); };
				}
				return lines;
			}
		}

		WhsStocktakeLineCollection lines;

		#endregion

		#region StocktakeLinesForFilter

		[ChildEditable(false)]
		public WhsStocktakeLineCollection StocktakeLinesForFilter
		{
			get
			{
				if (stocktakeLinesForFilter == null)
				{
					stocktakeLinesForFilter = new WhsStocktakeLineCollection(this);
				}

				return stocktakeLinesForFilter;
			}
		}
		WhsStocktakeLineCollection stocktakeLinesForFilter;

		#endregion

		#region InvoicingSupporter

		public WhsStocktakeInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new WhsStocktakeInvoicingSupporter(this)); }
		}

		WhsStocktakeInvoicingSupporter invoicingSupporter;

		#endregion

		#region LineClosingSemaphore

		public Semaphore LineClosingSemaphore
		{
			get { return lineClosingSemaphore ?? (lineClosingSemaphore = new Semaphore()); }
		}
		Semaphore lineClosingSemaphore;

		#endregion

		#region IsAutoCreatingStocktake

		internal bool IsAutoCreatingStocktake { get; set; }

		#endregion

		#region CurrentCountColumnNumber

		public ZByte CurrentCountColumnNumber
		{
			get
			{
				var result = new ZByte(1);
				if (Lines != null && Lines.Count > 0)
				{
					result = Lines.Max(l => l.WU_TotalCounts);
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region JobHeader

		public JobHeader Job => new JobHeader.Loader(this).Load();

		#endregion

		#region Properties

		// persistent

		#region WS_ABCAnalysisCategory

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.ABCAnalysisCategories")]
		public override ZString WS_ABCAnalysisCategory
		{
			get { return base.WS_ABCAnalysisCategory; }
			set
			{
				base.WS_ABCAnalysisCategory = value;
				ValidateCountEmptyLocationsCategory();
			}
		}

		#endregion

		#region WS_CountEmptyLocationsCategory

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.CountEmptyLocationCategory")]
		public override ZString WS_CountEmptyLocationsCategory
		{
			get { return base.WS_CountEmptyLocationsCategory; }
			set { base.WS_CountEmptyLocationsCategory = value; }
		}

		#endregion

		#region WS_WW_Whs

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WS_WW_Whs
		{
			get { return base.WS_WW_Whs; }
			set
			{
				if (!base.WS_WW_Whs.IsValid || base.WS_WW_Whs != value)
				{
					base.WS_WW_Whs = value;
					LocationString = "";
				}
			}
		}

		#endregion

		#region WS_OH_Client

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.Clients")]
		public override ZGuid WS_OH_Client
		{
			get { return base.WS_OH_Client; }
			set
			{
				base.WS_OH_Client = value;
				Lines.MarkAsNeedingValidation();
				var productsToRemove = ProductFilterCollection.Where(pf => pf.Product.RelatedOrganisations.Cast<OrgPartRelation>()
					.Where(op => op.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || op.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
					.All(org => org.OU_OH != value))
					.ToArray();
				foreach (var stocktakeProductFilter in productsToRemove)
				{
					ProductFilterCollection.Delete(stocktakeProductFilter);
				}
				ProductFilterCollection.RefreshBinding();
				ValidateCountEmptyLocationsCategory();
			}
		}

		#endregion

		#region WS_StocktakeCycle

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.StockTakeCycles")]
		public override ZString WS_StocktakeCycle
		{
			get { return base.WS_StocktakeCycle; }
			set
			{
				base.WS_StocktakeCycle = value;
				ValidateCountEmptyLocationsCategory();
			}
		}

		#endregion

		#region WS_RH_NKCommodityCode

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.CommodityCodes")]
		public override ZString WS_RH_NKCommodityCode
		{
			get { return base.WS_RH_NKCommodityCode; }
			set
			{
				base.WS_RH_NKCommodityCode = value;
				ValidateCountEmptyLocationsCategory();
			}
		}

		#endregion

		#region WS_WA_Area

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.Areas")]
		public override ZGuid WS_WA_Area
		{
			get { return base.WS_WA_Area; }
			set { base.WS_WA_Area = value; }
		}

		#endregion

		#region WS_WR_Row

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.Rows")]
		public override ZGuid WS_WR_Row
		{
			get { return base.WS_WR_Row; }
			set { base.WS_WR_Row = value; }
		}

		#endregion

		#region WS_PickMethod

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.PickMethods")]
		public override ZString WS_PickMethod
		{
			get { return base.WS_PickMethod; }
			set
			{
				base.WS_PickMethod = value;
			}
		}

		#endregion

		#region WS_StocktakeNumber

		[ReadOnly(true)]
		public override ZString WS_StocktakeNumber
		{
			get { return base.WS_StocktakeNumber; }
			set { base.WS_StocktakeNumber = value; }
		}

		#endregion

		#region WS_StocktakeDate

		[ReadOnly(true)]
		public override ZDateTime WS_StocktakeDate
		{
			get { return base.WS_StocktakeDate; }
			set { base.WS_StocktakeDate = value; }
		}

		#endregion

		#region WS_WL_Location

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.Locations")]
		[MaxLength(36)]
		[RelatedBusinessObject("Location")]
		public override ZGuid WS_WL_Location
		{
			get { return base.WS_WL_Location; }
			set
			{
				if (base.WS_WL_Location != value)
				{
					base.WS_WL_Location = value;
				}
			}
		}

		#endregion

		#region Location

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WS_WL_Location); }
		}

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.Locations")]
		[MaxLength(36)]
		public ZString LocationString
		{
			get { return Location?.WLV_LocationString_UserFriendly ?? locationString; }
			set
			{
				// prevalidation
				CheckMaximumLength(LocationStringInfo, value);
				locationString = value;

				WS_WL_Location = WhsLocation.FindLocationPK(Factory, value, WS_WW_Whs);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLocationString();
				}
				LocationStringInfo.RefreshBinding();
			}
		}

		ZString locationString;

		public ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(nameof(LocationString)); }
		}

		#endregion

		#region WS_StocktakeType

		[ReadOnlyMember(nameof(FilterMustBeReadonly))]
		[List("Lookups.StocktakeTypes")]
		public override ZString WS_StocktakeType
		{
			get { return base.WS_StocktakeType; }
			set
			{
				base.WS_StocktakeType = value;
				ValidateCountEmptyLocationsCategory();
			}
		}

		#endregion

		#region ProductFilterCollection

		[ChildEditable(true)]
		public WhsStocktakeProductFilterCollection ProductFilterCollection
		{
			get
			{
				if (productFilterCollection == null)
				{
					productFilterCollection = new WhsStocktakeProductFilterCollection(Factory, this);
					productFilterCollection.CollectionCountChange += ProductFilterCollection_CollectionCountChange;
					RegisterEditableChildObject(productFilterCollection);
				}
				return productFilterCollection;
			}
		}

		void ProductFilterCollection_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			ValidateCountEmptyLocationsCategory();
		}

		WhsStocktakeProductFilterCollection productFilterCollection;

		#endregion

		// calculated

		#region CountryCode

		public ZString CountryCode
		{
			get
			{
				var result = ZString.Empty;

				var warehouse = Warehouse;
				if (warehouse != null)
				{
					result = warehouse.CountryCode;
				}

				return result;
			}
		}

		#endregion

		#endregion

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableList(new[]
			{
				base.GetValidationDataSuspender(),
				new DisposableAction(
					() => cachedProductPackageTotals = GetProductPackageTotals(Lines.Where(line => !line.IsClosed)),
					() => cachedProductPackageTotals = null)
			});
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsStocktakeFetchStrategy(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return WS_StocktakeNumber.IsEmpty ? Res.GetString("d722687d-6147-4089-a645-700213391755", "Warehouse Stocktake") : Res.GetString("a03d502c-962d-45fb-94f7-f8dd63395ace", "Warehouse Stocktake {0}", WS_StocktakeNumber); }
		}

		#endregion

		#region Flags

		#region IsBondedWarehouse

		public bool IsBondedWarehouse
		{
			get
			{
				bool result = false;

				var warehouse = Warehouse;
				if (warehouse != null)
				{
					result = warehouse.IsWarehouseBondEnabled;
				}

				return result;
			}
		}

		#endregion

		#region ValidateCountEmptyLocationsCategory

		void ValidateCountEmptyLocationsCategory()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateWS_CountEmptyLocationsCategory();
				WS_CountEmptyLocationsCategoryInfo.RefreshBinding();
			}
		}

		#endregion

		#region IsLoaded

		public bool IsLoaded
		{
			get { return WS_StocktakeStatus == StocktakeStatus.Codes.Loaded; }
		}

		#endregion

		#region IsFinalised

		public bool IsFinalised
		{
			get { return WS_StocktakeStatus == StocktakeStatus.Codes.Finalised; }
		}

		#endregion

		#region IsNew

		public bool IsNew
		{
			get { return WS_StocktakeStatus == StocktakeStatus.Codes.New; }
		}

		#endregion

		#region FilterMustBeReadonly

		public bool FilterMustBeReadonly
		{
			get { return IsLoaded || IsFinalised; }
		}

		#endregion

		#region IsMaximumAmountOfColumnsAdded

		public ZBool IsMaximumAmountOfColumnsAdded
		{
			get { return (CurrentCountColumnNumber == MaximumNumberOfColumns); }
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#endregion

		#region Load

		public bool Load(INotificationSubscriberQueryUser notify = null)
		{
			userAgreedToLoadLines = false;
			var stocktakeLines = CreateStocktakeLines(notify);
			Lines.AddRange(stocktakeLines);

			if (Lines.Count > 0)
			{
				WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
			}
			return userAgreedToLoadLines;
		}

		public const int StocktakeLineCountLimitToAskConfirmation = 1000;

		public IEnumerable<WhsStocktakeLine> CreateStocktakeLines(INotificationSubscriberQueryUser notify = null)
		{
			var stocktakeLines = new List<WhsStocktakeLine>();

			var rawSqlQuery = "";

			if (WS_CountEmptyLocationsCategory == CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations)
			{
				rawSqlQuery = GetEmptyLocationQuery();
			}
			else
			{
				rawSqlQuery = GetNotEmptyLocationQuery();

				if (WS_CountEmptyLocationsCategory == CountEmptyLocationCategory.Codes.IncludeEmptyLocations)
				{
					rawSqlQuery += @" UNION ";
					rawSqlQuery += GetEmptyLocationQuery();
				}
			}

			//WI_F3_NKPackType, this parameter may be added as group criteria when Pack Type field usage will be implemented into Stocktake module

			var productPKs = ProductFilterCollection.Select(pf => pf.WSP_OP_Product);

			var summaryList = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WS_WW_Whs", WS_WW_Whs, WhsStocktakeSchema.WS_WW_Whs);
			sqlParams.Add("@WS_OH_Client", WS_OH_Client, WhsStocktakeSchema.WS_OH_Client);
			sqlParams.Add(ZSqlParameter.New("@ProductPKs", productPKs.ToArray(), WhsDocketLineSchema.WE_OP, isTableValued: true));
			sqlParams.Add("@WS_RH_NKCommodityCode", WS_RH_NKCommodityCode, WhsStocktakeSchema.WS_RH_NKCommodityCode);
			sqlParams.Add("@WS_ABCAnalysisCategory", WS_ABCAnalysisCategory, WhsStocktakeSchema.WS_ABCAnalysisCategory);
			sqlParams.Add("@WS_PickMethod", WS_PickMethod, WhsStocktakeSchema.WS_PickMethod);
			sqlParams.Add("@WS_WL_Location", WS_WL_Location, WhsStocktakeSchema.WS_WL_Location);
			sqlParams.Add("@WS_WR_Row", WS_WR_Row, WhsStocktakeSchema.WS_WR_Row);
			sqlParams.Add("@WS_WA_Area", WS_WA_Area, WhsStocktakeSchema.WS_WA_Area);
			sqlParams.Add("@WS_StocktakeCycle", WS_StocktakeCycle, WhsStocktakeSchema.WS_StocktakeCycle);

			userAgreedToLoadLines = true;
			summaryList.Load(rawSqlQuery, sqlParams);
			if (notify != null)
			{
				if (summaryList.Count > StocktakeLineCountLimitToAskConfirmation)
				{
					var caption = Res.GetString("A01DC88C-CD24-4F88-BA46-9934902D76C3", "Proceed");
					var message = Res.GetString("C6046EE8-3054-4CEA-BEAD-BFFFF64C390D", "Stock take with {0} lines is going to be created. You can choose to proceed with the results (this may take some time) or click No to add filters to narrow your search criteria.", summaryList.Count);
					var yesNoEventArgs = new QueryUserYesNoEventArgs(caption, message, defaultResponse: false);
					notify.QueryUser(yesNoEventArgs);
					if (!yesNoEventArgs.Response)
					{
						userAgreedToLoadLines = false;
					}
				}
			}

			if (userAgreedToLoadLines)
			{
				int lineNo = 0;
				foreach (DynamicBusinessObject row in summaryList)
				{
					Factory.AddFetchHint(WhsLocationViewSchema.PK, (ZGuid)row[WhsInventoryViewSchema.WI_WL]);
					Factory.AddFetchHint(OrgSupplierPartSchema.PK, (ZGuid)row[WhsInventoryViewSchema.WI_OP]);
				}

				for (int index = 0; index < summaryList.Count; index++)
				{
					var line = Factory.New<WhsStocktakeLine>();
					line.WU_WS = PK;
					line.WU_OH_Client = (ZGuid)summaryList[index][WhsInventoryViewSchema.WI_OH_Client];
					line.WU_OP = (ZGuid)summaryList[index][WhsInventoryViewSchema.WI_OP];
					line.WU_WL = (ZGuid)summaryList[index][WhsInventoryViewSchema.WI_WL];
					line.WU_Status = CodeLists.StocktakeLineStatus.Codes.Open;
					line.WU_SystemUnits = (ZDecimal)summaryList[index]["WI_AvailableQuantity"];
					line.WU_BondedEntryKey = (ZString)summaryList[index][WhsInventoryViewSchema.WI_BondedEntryKey];
					line.WU_ExpiryDate = ((ZDateTime)summaryList[index][WhsInventoryViewSchema.WI_ExpiryDate]).Date;
					line.WU_PackingDate = ((ZDateTime)summaryList[index][WhsInventoryViewSchema.WI_PackingDate]).Date;
					line.WU_PartAttrib1 = (ZString)summaryList[index][WhsInventoryViewSchema.WI_PartAttrib1];
					line.WU_PartAttrib2 = (ZString)summaryList[index][WhsInventoryViewSchema.WI_PartAttrib2];
					line.WU_PartAttrib3 = (ZString)summaryList[index][WhsInventoryViewSchema.WI_PartAttrib3];
					line.WU_SerialNumber = (ZString)summaryList[index][WhsInventoryViewSchema.WI_SerialNumber];
					line.WU_PalletID = (ZString)summaryList[index][WhsInventoryViewSchema.WI_PalletID];
					line.WU_InventoryStatus = (ZString)summaryList[index]["InventoryStatus"];
					line.WU_PackageGroupId = (ZString)summaryList[index][WhsDocketLineSchema.WE_PackageGroupId];
					line.WU_PerPackageQty = (ZDecimal)summaryList[index][WhsDocketLineSchema.WE_PerPackageQty];
					stocktakeLines.Add(line);
				}

				stocktakeLines.Sort(new SortStocktakeLines());
				foreach (var line in stocktakeLines)
				{
					line.WU_LineNo = ++lineNo;
				}
			}

			return stocktakeLines;
		}

		#region GetEmptyLocationQuery

		string GetEmptyLocationQuery()
		{
			var emptyLocationsQuery = $@"
SELECT DISTINCT
	CONVERT(UNIQUEIDENTIFIER, NULL) WI_OP, 
	WL_PK WI_WL,
	CONVERT(varchar, NULL) WI_PartAttrib1,
	CONVERT(varchar, NULL) WI_PartAttrib2, 
	CONVERT(varchar, NULL) WI_PartAttrib3,
	CONVERT(varchar, NULL) WI_SerialNumber,
	CONVERT(Datetime , NULL) WI_ExpiryDate, 
	CONVERT(Datetime , NULL) WI_PackingDate, 
	CONVERT(varchar , NULL) WI_PalletID,
	CONVERT(decimal , NULL) WI_AvailableQuantity, 
	CONVERT(UNIQUEIDENTIFIER, NULL)  WI_OH_Client,
	CONVERT(varchar , NULL)  WI_BondedEntryKey,
	'EMP' as InventoryStatus,
	CONVERT(varchar , NULL) WE_PackageGroupId,
	CONVERT(decimal , NULL) WE_PerPackageQty
FROM dbo.WhsLocation
	JOIN dbo.WhsLocationType ON WL_WLT_LocationType = WLT_PK
	LEFT JOIN dbo.WhsRow ON WR_PK = WL_WR
	WHERE WR_WW_Whs = @WS_WW_Whs AND 
		Not WL_PK in (
			SELECT WI_WL FROM dbo.WhsInventoryView 
			JOIN dbo.WhsDocketLine ON WE_PK = WI_WE_InDocketLine
			WHERE WI_WL = WL_PK AND ISNULL(WI_TotalUnits,0) > 0 AND WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}')
		AND WL_LocationStatus <> '{LocationStatus.Codes.Void}'
		AND WLT_LocationClass NOT IN ('{LocationClasses.Codes.DDL}', '{LocationClasses.Codes.PST}', '{LocationClasses.Codes.CON}')";

			if (!WS_PickMethod.IsEmpty)
			{
				emptyLocationsQuery += " AND WL_PickMethod = @WS_PickMethod";
			}
			if (!WS_WR_Row.IsEmpty)
			{
				emptyLocationsQuery += " AND WR_PK = @WS_WR_Row";
			}
			if (!WS_WA_Area.IsEmpty)
			{
				emptyLocationsQuery += " AND WL_WA_PickingArea = @WS_WA_Area";
			}
			if (!WS_WL_Location.IsEmpty)
			{
				emptyLocationsQuery += " AND WL_PK = @WS_WL_Location";
			}

			return emptyLocationsQuery;
		}

		#endregion

		#region GetNotEmptyLocationQuery

		string GetNotEmptyLocationQuery()
		{
			string rawSqlQuery = $@"
SELECT
	WI_OP,
	WI_WL,
	{SelectPartAttribsQuery()},
	WI_ExpiryDate,
	WI_PackingDate,
	WI_PalletID,
	SUM(WI_TotalUnits) as WI_AvailableQuantity,
	WI_OH_Client,
	WI_BondedEntryKey,
	CASE
		WHEN WE_WHC_NKCurrentInventoryHeldCode = '' THEN WI_InventoryStatus
		WHEN WE_WHC_NKCurrentInventoryHeldCode = 'DAM' THEN WE_WHC_NKCurrentInventoryHeldCode
		ELSE 'HEL'
	END as InventoryStatus,
	WE_PackageGroupId,
	WE_PerPackageQty
FROM
	dbo.WhsInventoryView
	JOIN dbo.WhsDocketLine on WE_PK = WI_WE_InDocketLine
	JOIN dbo.WhsDocket on WD_PK = WI_WD
	JOIN dbo.OrgPartRelation on OU_OH = WD_OH_Client and OU_OP = WE_OP
	LEFT JOIN dbo.WhsLocation on WL_PK = WI_WL
	LEFT JOIN dbo.WhsLocationType ON WL_WLT_LocationType = WLT_PK
	LEFT JOIN dbo.WhsRow on WR_PK = WL_WR";

			if (!WS_WA_Area.IsEmpty)
			{
				rawSqlQuery += " left join dbo.WhsArea on WA_PK = WL_WA_PickingArea";
			}

			if (!WS_RH_NKCommodityCode.IsEmpty)
			{
				rawSqlQuery += " left join dbo.OrgSupplierPart on OP_PK = WE_OP";
			}

			rawSqlQuery += $@"
WHERE
	WI_TotalUnits > 0
	AND WE_CurrentInventoryStatus <> '{InventoryStatus.Codes.Staged}'
	AND WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
	AND WR_WW_Whs = @WS_WW_Whs
	AND (WLT_LocationClass IS NULL OR WLT_LocationClass NOT IN ('{LocationClasses.Codes.DDL}', '{LocationClasses.Codes.PST}', '{LocationClasses.Codes.CON}'))";

			if (!WS_OH_Client.IsEmpty)
			{
				rawSqlQuery += " AND WD_OH_Client = @WS_OH_Client";
			}
			if (!WS_WL_Location.IsEmpty)
			{
				rawSqlQuery += " AND WI_WL = @WS_WL_Location";
			}
			if (ProductFilterCollection.Count > 0)
			{
				rawSqlQuery += " AND WI_OP IN (SELECT Value FROM @ProductPKs)"; // raw sql statement
			}
			if (!WS_RH_NKCommodityCode.IsEmpty)
			{
				rawSqlQuery += " AND OP_RH_NKCommodityCode = @WS_RH_NKCommodityCode";
			}

			if (!WS_ABCAnalysisCategory.IsEmpty)
			{
				rawSqlQuery += @" AND
	(
		SELECT TOP 1 WJ_Category
		FROM dbo.WhsABCCategory
		WHERE WJ_OP_Product = WI_OP AND WJ_OH_Client = WI_OH_Client AND WJ_WW_Warehouse = WR_WW_Whs
		ORDER BY WJ_AnalysisDateTo DESC
	) = @WS_ABCAnalysisCategory";
			}

			if (!WS_PickMethod.IsEmpty)
			{
				rawSqlQuery += " AND WL_PickMethod = @WS_PickMethod";
			}
			if (!WS_WR_Row.IsEmpty)
			{
				rawSqlQuery += " AND WR_PK = @WS_WR_Row";
			}
			if (!WS_WA_Area.IsEmpty)
			{
				rawSqlQuery += " AND WA_PK = @WS_WA_Area";
			}
			if (!WS_StocktakeCycle.IsEmpty)
			{
				rawSqlQuery += @" AND EXISTS (
SELECT NULL FROM dbo.WhsProductParamsByWhsAndClient
WHERE W3_WW = WD_WW_Whs
AND W3_OH = WD_OH_Client
AND W3_OP = WI_OP
AND W3_StockTakeCycle = @WS_StockTakeCycle)";
			}

			rawSqlQuery += $@"
GROUP BY
	WI_OP, 
	WI_WL, 
	{GroupPartAttribsQuery()},
	WI_ExpiryDate, 
	WI_PackingDate,
	WI_PalletID,
	WI_OH_Client,
	WI_BondedEntryKey,
	CASE
		WHEN WE_WHC_NKCurrentInventoryHeldCode = '' THEN WI_InventoryStatus
		WHEN WE_WHC_NKCurrentInventoryHeldCode = 'DAM' THEN WE_WHC_NKCurrentInventoryHeldCode
		ELSE 'HEL' 
	END,
	WE_PackageGroupId,
	WE_PerPackageQty";
			return rawSqlQuery;
		}

		string SelectPartAttribsQuery()
		{
			return $@"	WI_PartAttrib1,
	WI_PartAttrib2,
	WI_PartAttrib3,
	CASE WHEN OU_PICKMODE = '{WhsPickMode.Codes.AttributeNeutral}' AND OU_RFAttributeConfirm = '{RFAttributeConfirmCode.Codes.SerialNumber}' THEN '{AttributeNeutral}' ELSE WI_SerialNumber END AS WI_SerialNumber";
		}

		string GroupPartAttribsQuery()
		{
			return $@"	WI_PartAttrib1,
	WI_PartAttrib2, 
	WI_PartAttrib3,
	CASE WHEN OU_PICKMODE = '{WhsPickMode.Codes.AttributeNeutral}' AND OU_RFAttributeConfirm = '{RFAttributeConfirmCode.Codes.SerialNumber}' THEN '{AttributeNeutral}' ELSE WI_SerialNumber END";
		}

		#endregion

		#endregion

		#region CheckStockTakeCycle

		public bool CheckStockTakeCycle(ZGuid whs, ZGuid client, OrgSupplierPart supplierPart)
		{
			var product = WhsProduct.GetWhsProduct(supplierPart);
			return product.ParamsByWhsAndClient.Cast<WhsProductParamsByWhsAndClient>().Any(p => p.W3_StockTakeCycle == WS_StocktakeCycle && p.W3_WW == whs && p.W3_OH == client);
		}

		#endregion

		#region CloseLines

		public bool CloseLines(WhsStocktakeLine[] linesToClose)
		{
			using (new SemaphoreManager(LineClosingSemaphore))
			{
				try
				{
					ClosingLines = linesToClose.Where(l => l.IsOpen).ToArray();

					foreach (var line in closingLines)
					{
						if (line.CloseLine())
						{
							if (line.CurrentCount != line.WU_SystemUnits)
							{
								CreateAdjustment(line, line.CurrentCount - line.WU_SystemUnits);
							}
						}

						CheckAndUpdateStatus();
					}
				}
				finally
				{
					ClosingLines = null;
				}
			}

			return linesToClose.All(line => !line.HasErrors);
		}

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WS_StocktakeNumber;
			set => WS_StocktakeNumber = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehouseStocktakeNumber;

		#endregion

		#region Save

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			AddLogsIfRequired();
		}

		void PopulateWS_StocktakeNumberIfRequired()
		{
			if (!IsInDatabase && !IsDeleted && WS_StocktakeNumber.IsEmpty)
			{
				WS_StocktakeNumber = Env.NumberFountains.WarehouseStocktakeNumber.GetNextFormatted(Factory);
			}
		}

		void AddLogsIfRequired()
		{
			if (!IsInDatabase)
			{
				Logs.AddNew(Events.WarehouseJobEntered, ZDateTimeOffset.Now);
			}

			if (IsFinalised && !WS_StocktakeStatusInfo.OriginalValue.Equals(StocktakeStatus.Codes.Finalised))
			{
				Logs.AddNew(Events.ItemDocumentJobFinalised, ZDateTimeOffset.Now);
			}
		}

		#endregion

		#region Statuses

		public StocktakeStatus Statuses
		{
			get { return new StocktakeStatus(); }
		}

		public ZString StatusDesc
		{
			get { return Statuses.GetDescriptionFromCode(WS_StocktakeStatus); }
		}

		public ZPropertyInfo StatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDesc)); }
		}

		void CheckAndUpdateStatus()
		{
			if (Lines.Count > 0 && Lines.All(l => l.IsClosed))
			{
				WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			}
		}

		#endregion

		#region Adjustments

		public WhsAdjustmentCollection Adjustments
		{
			get
			{
				if (adjustments == null)
				{
					var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, AdjustmentExternalReferenceCommonPrefix());
					adjustments = new WhsAdjustmentCollection(Factory, query);
				}

				return adjustments;
			}
		}

		void CreateAdjustment(WhsStocktakeLine line, ZDecimal adjustmentCount)
		{
			if (unfinalisedAdjustment == null ||
				unfinalisedAdjustment.IsDeleted ||
				unfinalisedAdjustment.IsFinalisedOrCancelled ||
				unfinalisedAdjustment.WD_OH_Client != line.WU_OH_Client ||
				unfinalisedAdjustment.WD_WW_Whs != line.Stocktake.WS_WW_Whs)
			{
				unfinalisedAdjustment = FindUnfinalizedAdjustment(line.WU_OH_Client, line.Stocktake.WS_WW_Whs);
				if (unfinalisedAdjustment == null)
				{
					unfinalisedAdjustment = CreateNewAdjustment(line);
				}
			}

			var relation = line.Product.Parent.RelatedOrganisations.FindFirstByOrganisationPK(line.WU_OH_Client);

			if (!line.WU_IsManuallyAdded
				&& relation.OU_PickMode == WhsPickMode.Codes.AttributeNeutral
				&& line.Product.IsSerialNumberUsed(relation.Organisation))
			{
				var absoluteAdjustmentCount = Math.Abs(adjustmentCount);
				var lastCount = adjustmentCount > 0 ? 1 : -1;

				for (int count = 1; count <= absoluteAdjustmentCount; count++)
				{
					CreateAdjustmentLine(unfinalisedAdjustment, line, relation, lastCount);
				}
			}
			else
			{
				CreateAdjustmentLine(unfinalisedAdjustment, line, relation, adjustmentCount);
			}
		}

		WhsAdjustment FindUnfinalizedAdjustment(ZGuid clientPk, ZGuid warehousePk)
		{
			return Adjustments.Cast<WhsAdjustment>().FirstOrDefault(a => !a.IsDeleted && !a.IsFinalisedOrCancelled
				&& a.WD_OH_Client == clientPk && a.WD_WW_Whs == warehousePk);
		}

		WhsAdjustment CreateNewAdjustment(WhsStocktakeLine line)
		{
			var nextAdjustmentExternalReference = NewAdjustmentExternalReference();
			var newAdjustment = Adjustments.AddNew();
			newAdjustment.WD_ExternalReference = nextAdjustmentExternalReference;
			newAdjustment.WD_OH_Client = line.WU_OH_Client;
			newAdjustment.WD_WW_Whs = line.Stocktake.WS_WW_Whs;
			return newAdjustment;
		}

		static void CreateAdjustmentLine(WhsAdjustment unfinalisedAdjustment, WhsStocktakeLine line, OrgPartRelation relation, ZDecimal adjustmentCount)
		{
			var isAttributeNeutral = relation.OU_PickMode == WhsPickMode.Codes.AttributeNeutral;
			var adjLine = unfinalisedAdjustment.Lines.AddNew();
			adjLine.WE_OP = line.WU_OP;
			adjLine.WE_WL = line.WU_WL;
			adjLine.WE_F3_NKPackType = GetPackType(line);
			adjLine.WE_LineComment = line.WU_LineComment.IsEmpty
				? Res.GetString("6db76354-4a02-4b1a-8616-3b8a6caa5cd4", "Auto Stocktake Adjustment")
				: (string)line.WU_LineComment;
			adjLine.WE_ReasonCode = adjLine.Lookups.AdjustmentReasonCodes.DefaultCode;
			adjLine.WE_TransactionQuantity = adjustmentCount;
			adjLine.WE_PartAttrib1 = line.WU_PartAttrib1;
			adjLine.WE_PartAttrib2 = line.WU_PartAttrib2;
			adjLine.WE_PartAttrib3 = line.WU_PartAttrib3;
			adjLine.WE_SerialNumber = (isAttributeNeutral && !line.WU_IsManuallyAdded && relation.OU_RFAttributeConfirm == RFAttributeConfirmCode.Codes.SerialNumber) ? ZString.Empty : line.WU_SerialNumber;
			adjLine.WE_PackingDate = line.WU_PackingDate;
			adjLine.WE_ExpiryDate = line.WU_ExpiryDate;
			adjLine.WE_PalletID = line.WU_PalletID;
			adjLine.WE_PackageGroupId = line.WU_PackageGroupId;
			adjLine.WE_PerPackageQty = line.WU_PerPackageQty;

			if (line.WU_InventoryStatus.EqualsIgnoringCase(StocktakeInventoryStatus.Codes.Available))
			{
				adjLine.WE_OriginalInventoryStatus = line.WU_InventoryStatus;
			}
			else
			{
				var heldCodeToSet = adjustmentCount < 0 ? string.Empty : InventoryHoldCodes.Codes.Held; // Adjust in with "Held", adjust out *any* held (not damaged) inventory
				adjLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
				adjLine.WE_WHC_NKOriginalInventoryHeldCode = line.WU_InventoryStatus.EqualsIgnoringCase(StocktakeInventoryStatus.Codes.Damaged) ? InventoryHoldCodes.Codes.Damaged : heldCodeToSet;
			}
		}

		static ZString GetPackType(WhsStocktakeLine line)
		{
			var packType = Constants.PkgUnit.Unit;
			if (!line.WU_F3_NKPackType.IsEmpty)
			{
				packType = line.WU_F3_NKPackType;
			}
			else if (!line.Product.Parent.OP_StockKeepingUnit.IsEmpty)
			{
				packType = line.Product.Parent.OP_StockKeepingUnit;
			}

			return packType;
		}

		ZString NewAdjustmentExternalReference()
		{
			var nextNumber = Adjustments.Count + 1;
			return AdjustmentExternalReferenceCommonPrefix() + nextNumber.ToString(Culture.Invariant);
		}

		ZString AdjustmentExternalReferenceCommonPrefix()
		{
			return stocktakeReferencePrefix + WS_StocktakeNumber + "-";
		}

		WhsAdjustmentCollection adjustments;
		WhsAdjustment unfinalisedAdjustment;
		const string stocktakeReferencePrefix = "STOCKTAKE ";

		#endregion

		#region Notes

#if DEBUG
		public StmNoteContexts GetNoteContextsForRelatedNotes()
		{
			return NoteContextsForRelatedNotes;
		}
#endif

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.S;

				return result;
			}
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var client = Client;
				if (client != null)
				{
					result.Add(client);
				}

				var warehouse = Warehouse;
				if (warehouse != null)
				{
					result.Add(warehouse);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Lines.DeleteAll();
			DeleteRelatedJobHeaders();
			WorkflowItems.RemoveAndDeleteAll();
			ProductFilterCollection.DeleteAll();

			base.Delete();
		}

		void DeleteRelatedJobHeaders()
		{
			if (InvoicingSupporter != null)
			{
				InvoicingSupporter.DeleteRelatedJobHeaders();
			}
		}

		#endregion

		#region LocationCapacityValidationManager

		internal WhsLocationCapacityValidationManager<WhsStocktake, WhsStocktakeLine> LocationCapacityValidationManager
		{
			get { return locationCapacityValidationManager ?? (locationCapacityValidationManager = new WhsLocationCapacityValidationManager<WhsStocktake, WhsStocktakeLine>(this)); }
		}
		WhsLocationCapacityValidationManager<WhsStocktake, WhsStocktakeLine> locationCapacityValidationManager;

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;
		}
#endif
		#endregion

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return InvoicingSupporter; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return WS_StocktakeNumber; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateWS_StocktakeNumberIfRequired();
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.WarehouseStocktake);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsStocktakeDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(Client);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("c939b3bf-76ce-40fb-8db4-fb43259d3a83", "Stocktake") + " - " + WS_StocktakeNumber; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.WarehouseStocktakeCycleCount; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return null; }
		}

		#endregion

		#region ILocationCapacityMaster Members

		ZGuid ILocationCapacityMaster<WhsStocktakeLine>.PKToExclude => ZGuid.Empty;

		IEnumerable<WhsStocktakeLine> ILocationCapacityMaster<WhsStocktakeLine>.GetValidationLines(WhsLocation location)
		{
			return Lines.Where(l => l.WU_WL == location.PK);
		}

		ZBool ILocationCapacityMaster<WhsStocktakeLine>.ShowRFMessage => false;

		ZString ILocationCapacityMaster<WhsStocktakeLine>.GetLocationQuantityExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location)
		{
			return Res.GetString("4BE94979-0776-475A-8F79-9C47E5F26EC0",
											"The Stocktake quantity exceeds the maximum available quantity for location {0} by {1} unit(s).",
											location.WLV_LocationString,
											(requiredValue - availableValue));
		}

		ZString ILocationCapacityMaster<WhsStocktakeLine>.GetLocationWeightExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location)
		{
			return Res.GetString("9D56B06C-0678-4FD1-891E-1E497D1CFB84",
											"The Stocktake weight exceeds the maximum available weight for location {0} by {1} {2}.",
											location.WLV_LocationString,
											((ZDecimal)(requiredValue - availableValue)).ToString(2), // 2 decimal places
											location.WLV_MaxWeightUnit);
		}

		ZString ILocationCapacityMaster<WhsStocktakeLine>.GetLocationVolumeExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location)
		{
			return Res.GetString("B2C3916E-B94A-4379-933F-E4D4D940D8AB",
											"The Stocktake volume exceeds the maximum available volume for location {0} by {1} {2}.",
											location.WLV_LocationString,
											((ZDecimal)(requiredValue - availableValue)).ToString(3), // 3 decimal places
											location.WLV_MaxCubicUnit);
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.WhsStocktakeWorkflowDescriptorCode; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsStocktakeProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, WS_OH_Client, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_WW, WS_WW_Whs, ZGuid.Empty);
			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IWhsLogEventParent Memebers

		ZString IWhsLogEventParent.EventFreeTextReference
		{
			get { return WS_StocktakeNumber; }
		}

		string IWhsLogEventParent.EventReferenceParameterType
		{
			get { return Constants.EventReferenceParameterTypes.Stocktake; }
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new WarehouseStocktakeNumberFountainUniqueIndexFailureHandler(this)); }
		}

		class WarehouseStocktakeNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public WarehouseStocktakeNumberFountainUniqueIndexFailureHandler(WhsStocktake stocktake)
			: base(WhsStocktakeSchema.Constants.Indexes.NR_UC__WS_StocktakeNumber, stocktake)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Env.NumberFountains.WarehouseStocktakeNumber; }
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#region ILocationConsumer Member

		ZGuid ILocationConsumer.LocationPK
		{
			get { return WS_WL_Location; }
		}

		ZGuid ILocationConsumer.WarehousePK
		{
			get { return WS_WW_Whs; }
		}

		ZString ILocationConsumer.LocationTypeForMessages
		{
			get { return Res.GetString("cb506b65-2a73-4a33-a339-cfda5f034b19", "Stocktake"); }
		}

		ZString ILocationConsumer.LocationTitle { get; set; }

		#endregion

		public const int MaximumNumberOfColumns = 3;

		bool userAgreedToLoadLines;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A constant string")]
#if DEBUG
		public
#endif
		const string AttributeNeutral = "Attribute Neutral";

		// interfaces

		#region IMasterAssigner

		IEnumerable<ILineStaffAssigner> IMasterStaffAssigner.Lines => Lines;

		bool IMasterStaffAssigner.IsJobAssignable => IsLoaded;

		bool IMasterStaffAssigner.CanAssignOrUnAssignAnyLines => IsLoaded && Lines.Any<ILineStaffAssigner>(l => l.CanAssignOrUnAssignLine());

		#endregion

		#region IValidateParentWithLines

		ProductPackageTotals IValidateParentWithLines.GetParentProductPackageTotals(IEnumerable<ICalculateProductPackageTotals> lines) => cachedProductPackageTotals ?? GetProductPackageTotals(lines);
		ProductPackageTotals cachedProductPackageTotals;

		ProductPackageTotals GetProductPackageTotals(IEnumerable<ICalculateProductPackageTotals> lines) => new ProductPackageTotals(lines);

		#endregion
	}

	#region Invoicing Supporter

	public class WhsStocktakeInvoicingSupporter : WhsJobInvoicingSupporter<WhsStocktake>
	{
		public WhsStocktakeInvoicingSupporter(WhsStocktake parent)
			: base(parent)
		{
		}

		public override OrgHeader Consignor
		{
			get { return Parent.Client; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseStocktake; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Parent.IsInDatabaseIncludingChildren; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return (Parent.Warehouse != null) ? Parent.Warehouse.RelatedCompanyBranch : null; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WhsStocktakeJobInvoicing;
		}
	}

	#endregion

	#region Document Supporter

	public class WhsStocktakeDocumentSupporter : DocumentSupporter
	{
		public WhsStocktakeDocumentSupporter(WhsStocktake stocktake)
			: base(stocktake)
		{
		}

		protected WhsStocktake Stocktake
		{
			get { return (WhsStocktake)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsStocktake; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsStocktakeCustomiseDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
				{
					Core.Constants.DataContext.WhsStocktake,
					Core.Constants.DataContext.GenericFreightJob
				};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Stocktake);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsStocktake, Stocktake) };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact result = new OrgHeaderContact(Stocktake.Client, null);
			return result;
		}
	}

	#endregion
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Business
{
	partial class WhsStocktake
	{
		public WhsAdjustment UnfinalizedAdjustmentForTesting
		{
			get { return unfinalisedAdjustment; }
		}
	}
}

#endif
#endregion
