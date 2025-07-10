using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsInventory : WhsInventoryView, IWebDocumentsWithUploadSupport, IWebUserVisibleNotesSupport
	{
		public TrackingWhsInventory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema
		{
			public const string TableName = AutoWhsInventoryView.Schema.TableName;
			public const string PK = "WI_PK";
			public const string TrackingWarehouseCode = "TrackingWarehouseCode";
			public const string TrackingWarehouseName = "TrackingWarehouseName";
			public const string TrackingProductCode = "TrackingProductCode";
			public const string TrackingProductDescription = "TrackingProductDescription";
			public const string Quantity = "Quantity";
			public const string TrackingTotalWeight = "TrackingTotalWeight";
			public const string TrackingTotalVolume = "TrackingTotalVolume";
			public const string TrackingClientQuantity = "TrackingClientQuantity";
			public const string TrackingArrivalDateOrETA = "TrackingArrivalDateOrETA";
			public const string TrackingInventoryStatus = "TrackingInventoryStatus";
			public const string TrackingHeldCode = "TrackingHeldCode";
			public const string TrackingCurrentLocation = "TrackingCurrentLocation";
			public const string TrackingCommittedQuantity = "TrackingCommittedQuantity";
			public const string TrackingCrossDockQuantity = "TrackingCrossDockQuantity";
			public const string TrackingTotalValue = "TrackingTotalValue";
			public const string TrackingHasEDocsAttached = "TrackingHasEDocsAttached";
			public const string TrackingCurrency = "TrackingCurrency";
			public const string TrackingReceiptReference = "TrackingReceiptReference";
			public const string TrackingArrivalDate = "TrackingArrivalDate";
			public const string TrackingPalletID = "TrackingPalletID";
			public const string TrackingPartAttrib1 = "TrackingPartAttrib1";
			public const string TrackingPartAttrib2 = "TrackingPartAttrib2";
			public const string TrackingPartAttrib3 = "TrackingPartAttrib3";
			public const string TrackingSerialNumber = "TrackingSerialNumber";
			public const string TrackingPackingDate = "TrackingPackingDate";
			public const string TrackingExpiryDate = "TrackingExpiryDate";
			public const string TrackingAvailableToPickQuantity = "TrackingAvailableToPickQuantity";
			public const string TrackingUnitsUQ = "TrackingUnitsUQ";
			public const string TrackingTotalUnits = "TrackingTotalUnits";
			public const string TrackingClientUQ = "TrackingClientUQ";
		}

		#endregion

		#region Docket

		public override WhsDocket Docket => docket ?? (docket = GetDocket());

		WhsDocket GetDocket()
		{
			var query = new ZQuery();
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, WI_WD_Proxy);

			return Factory.LoadTop1<WhsReceive>(query) ?? base.Docket;
		}

		WhsDocket docket;

		#endregion

		#region InDocketLine

		public override WhsDocketLine InDocketLine => inDocketLine ?? (inDocketLine = GetInDocketLine());

		WhsDocketLine GetInDocketLine()
		{
			if (WI_WE_InDocketLine.IsValid && WI_InDocketLineType == DocketType.Codes.Receive)
			{
				return Factory.Load<WhsReceiveLine>(WI_WE_InDocketLine);
			}

			return base.InDocketLine;
		}

		WhsDocketLine inDocketLine;

		#endregion

		#region SupplierPart

		public override OrgSupplierPart SupplierPart => supplierPart ?? (supplierPart = base.SupplierPart);

		public void SetSupplierPart(OrgSupplierPart part) => supplierPart = part;

		OrgSupplierPart supplierPart;

		#endregion

		#region New Properties

		#region TrackingClientUQ

		public ZString TrackingClientUQ => GetTrackingValue(ref trackingClientUQ, () => WI_ClientUQ);

		ZString? trackingClientUQ;

		public ZPropertyInfo TrackingClientUQInfo => GetZPropertyInfo(Schema.TrackingClientUQ);

		#endregion

		#region TrackingTotalUnits

		public ZDecimal TrackingTotalUnits => GetTrackingValue(ref trackingTotalUnits, () => WI_TotalUnits);

		ZDecimal? trackingTotalUnits;

		public ZPropertyInfo TrackingTotalUnitsInfo => GetZPropertyInfo(Schema.TrackingTotalUnits);

		#endregion

		#region TrackingUnitsUQ

		public ZString TrackingUnitsUQ => GetTrackingValue(ref trackingUnitsUQ, () => WI_UnitsUQ);

		ZString? trackingUnitsUQ;

		public ZPropertyInfo TrackingUnitsUQInfo => GetZPropertyInfo(Schema.TrackingUnitsUQ);

		#endregion

		#region TrackingSerialNumber

		public ZString TrackingSerialNumber => GetTrackingValue(ref trackingSerialNumber, () => WI_SerialNumber);

		ZString? trackingSerialNumber;

		public ZPropertyInfo TrackingSerialNumberInfo => GetZPropertyInfo(Schema.TrackingSerialNumber);

		#endregion

		#region TrackingPartAttrib1

		public ZString TrackingPartAttrib1 => GetTrackingValue(ref trackingPartAttrib1, () => WI_PartAttrib1);

		ZString? trackingPartAttrib1;

		public ZPropertyInfo TrackingPartAttrib1Info => GetZPropertyInfo(Schema.TrackingPartAttrib1);

		#endregion

		#region TrackingPartAttrib2

		public ZString TrackingPartAttrib2 => GetTrackingValue(ref trackingPartAttrib2, () => WI_PartAttrib2);

		ZString? trackingPartAttrib2;

		public ZPropertyInfo TrackingPartAttrib2Info => GetZPropertyInfo(Schema.TrackingPartAttrib2);

		#endregion

		#region TrackingPartAttrib3

		public ZString TrackingPartAttrib3 => GetTrackingValue(ref trackingPartAttrib3, () => WI_PartAttrib3);

		ZString? trackingPartAttrib3;

		public ZPropertyInfo TrackingPartAttrib3Info => GetZPropertyInfo(Schema.TrackingPartAttrib3);

		#endregion

		#region TrackingPalletID

		public ZString TrackingPalletID => GetTrackingValue(ref trackingPalletID, () => WI_PalletID);

		ZString? trackingPalletID;

		public ZPropertyInfo TrackingPalletIDInfo => GetZPropertyInfo(Schema.TrackingPalletID);

		#endregion

		#region TrackingCurrency

		public ZString TrackingCurrency => GetTrackingValue(ref trackingCurrency, () => WI_Currency);

		ZString? trackingCurrency;

		public ZPropertyInfo TrackingCurrencyInfo => GetZPropertyInfo(Schema.TrackingCurrency);

		#endregion

		#region TrackingHasEDocsAttached

		public ZBool TrackingHasEDocsAttached => GetTrackingValue(ref trackingHasEDocsAttached, () => HasEDocsOrNotesAttached);

		ZBool? trackingHasEDocsAttached;

		public ZPropertyInfo TrackingHasEDocsAttachedInfo => GetZPropertyInfo(Schema.TrackingHasEDocsAttached);

		#endregion

		#region TrackingTotalValue

		public ZDecimal TrackingTotalValue => GetTrackingValue(ref trackingTotalValue, () => WI_TotalValue);

		ZDecimal? trackingTotalValue;

		public ZPropertyInfo TrackingTotalValueInfo => GetZPropertyInfo(Schema.TrackingTotalValue);

		#endregion

		#region TrackingCrossDockQuantity

		public ZDecimal TrackingCrossDockQuantity => GetTrackingValue(ref trackingCrossDockQuantity, () => WI_CrossDockQuantity);

		ZDecimal? trackingCrossDockQuantity;

		public ZPropertyInfo TrackingCrossDockQuantityInfo => GetZPropertyInfo(Schema.TrackingCrossDockQuantity);

		#endregion

		#region TrackingCommittedQuantity

		public ZDecimal TrackingCommittedQuantity => GetTrackingValue(ref trackingCommittedQuantity, () => InternalsProxy.CommittedToTransactionQuantity);

		ZDecimal? trackingCommittedQuantity;

		public ZPropertyInfo TrackingCommittedQuantityInfo => GetZPropertyInfo(Schema.TrackingCommittedQuantity);

		#endregion

		#region TrackingCurrentLocation

		public ZString TrackingCurrentLocation => GetTrackingValue(ref trackingCurrentLocation, () => CurrentLocationString);

		ZString? trackingCurrentLocation;

		public ZPropertyInfo TrackingCurrentLocationInfo => GetZPropertyInfo(Schema.TrackingCurrentLocation);

		#endregion

		#region TrackingHeldCode

		public ZString TrackingHeldCode => GetTrackingValue(ref trackingHeldCode, () => WI_HeldCode);

		ZString? trackingHeldCode;

		public ZPropertyInfo TrackingHeldCodeInfo => GetZPropertyInfo(Schema.TrackingHeldCode);

		#endregion

		#region TrackingInventoryStatus

		public ZString TrackingInventoryStatus => GetTrackingValue(ref trackingInventoryStatus, () => WI_InventoryStatus);

		ZString? trackingInventoryStatus;

		public ZPropertyInfo TrackingInventoryStatusInfo => GetZPropertyInfo(Schema.TrackingInventoryStatus);

		#endregion

		#region TrackingArrivalDateOrETA

		public ZDateTimeOffset TrackingArrivalDateOrETA => GetTrackingValue(ref trackingArrivalDateOrETA, () => WI_ArrivalDateOrETA);

		ZDateTimeOffset? trackingArrivalDateOrETA;

		public ZPropertyInfo TrackingArrivalDateOrETAInfo => GetZPropertyInfo(Schema.TrackingArrivalDateOrETA);

		#endregion

		#region TrackingArrivalDate

		public ZDateTimeOffset TrackingArrivalDate => GetTrackingValue(ref trackingArrivalDate, () => WI_ArrivalDate);

		ZDateTimeOffset? trackingArrivalDate;

		public ZPropertyInfo TrackingArrivalDateInfo => GetZPropertyInfo(Schema.TrackingArrivalDate);

		#endregion

		#region TrackingPackingDate

		public ZDateTime TrackingPackingDate => GetTrackingValue(ref trackingPackingDate, () => WI_PackingDate);

		ZDateTime? trackingPackingDate;

		public ZPropertyInfo TrackingPackingDateInfo => GetZPropertyInfo(Schema.TrackingPackingDate);

		#endregion

		#region TrackingExpiryDate

		public ZDateTime TrackingExpiryDate => GetTrackingValue(ref trackingExpiryDate, () => WI_ExpiryDate);

		ZDateTime? trackingExpiryDate;

		public ZPropertyInfo TrackingExpiryDateInfo => GetZPropertyInfo(Schema.TrackingExpiryDate);

		#endregion

		#region TrackingWarehouseCode

		[MaxLength(AutoWhsWarehouse.Schema.WW_WarehouseCodeMaxLength)]
		public ZString TrackingWarehouseCode => Warehouse?.WW_WarehouseCode ?? ZString.Empty;

		public ZPropertyInfo TrackingWarehouseCodeInfo => GetZPropertyInfo(Schema.TrackingWarehouseCode);

		#endregion

		#region TrackingWarehouseName

		[MaxLength(AutoWhsWarehouse.Schema.WW_WarehouseNameMaxLength)]
		public ZString TrackingWarehouseName => Warehouse?.WW_WarehouseNameMultilingual;

		public ZPropertyInfo TrackingWarehouseNameInfo => GetZPropertyInfo(Schema.TrackingWarehouseName);

		#endregion

		#region TrackingProductCode

		[MaxLength(AutoOrgSupplierPart.Schema.OP_PartNumMaxLength)]
		public ZString TrackingProductCode => SupplierPart?.OP_PartNum ?? ZString.Empty;

		public ZPropertyInfo TrackingProductCodeInfo => GetZPropertyInfo(Schema.TrackingProductCode);

		#endregion

		#region TrackingProductDescription

		[MaxLength(AutoOrgSupplierPart.Schema.OP_DescMaxLength)]
		public ZString TrackingProductDescription => SupplierPart?.OP_Desc ?? ZString.Empty;

		public ZPropertyInfo TrackingProductDescriptionInfo => GetZPropertyInfo(Schema.TrackingProductDescription);

		#endregion

		#region Quantity

		public ZDecimal Quantity
		{
			get { return quantity; }
			set
			{
				quantity = value;
				QuantityInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateQuantity();
				}
			}
		}

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(Schema.Quantity);

		ZDecimal quantity;

		#endregion

		#region TrackingClientQuantity

		public ZDecimal TrackingClientQuantity => GetTrackingValue(ref trackingClientQuantity, () => SupplierPart?.UnitConverter.Convert(WI_TotalUnits, SupplierPart.OP_StockKeepingUnit, WI_ClientUQ) ?? ZDecimal.Zero);

		ZDecimal? trackingClientQuantity;

		public ZPropertyInfo TrackingClientQuantityInfo => GetZPropertyInfo(Schema.TrackingClientQuantity);

		#endregion

		#region TrackingAvailableToPickQuantity

		public ZDecimal TrackingAvailableToPickQuantity => GetTrackingValue(ref trackingAvailableToPickQuantity, () => WI_AvailableToPickQuantity);

		ZDecimal? trackingAvailableToPickQuantity;

		public ZPropertyInfo TrackingAvailableToPickQuantityInfo => GetZPropertyInfo(Schema.TrackingAvailableToPickQuantity);

		#endregion

		#region TrackingTotalWeight

		public ZDecimal TrackingTotalWeight => GetTrackingValue(ref trackingTotalWeight, () => WI_TotalUnits * (SupplierPart?.OP_Weight ?? ZDecimal.Zero));

		ZDecimal? trackingTotalWeight;

		public ZPropertyInfo TrackingTotalWeightInfo => GetZPropertyInfo(Schema.TrackingTotalWeight);

		#endregion

		#region TrackingTotalVolume

		public ZDecimal TrackingTotalVolume => GetTrackingValue(ref trackingTotalVolume, () => WI_TotalUnits * (SupplierPart?.OP_Cubic ?? ZDecimal.Zero));

		ZDecimal? trackingTotalVolume;

		public ZPropertyInfo TrackingTotalVolumeInfo => GetZPropertyInfo(Schema.TrackingTotalVolume);

		#endregion

		#region TrackingReceiptReference

		public ZString TrackingReceiptReference => GetTrackingValue(ref trackingReceiptReference, () => ReceiptReference);

		ZString? trackingReceiptReference;

		public ZPropertyInfo TrackingReceiptReferenceInfo => GetZPropertyInfo(Schema.TrackingReceiptReference);

		#endregion

		public ZGuid WarehousePK
		{
			get
			{
				var warehousePK = Location?.WLV_WW_Whs ?? ZGuid.Empty;

				return !warehousePK.IsEmpty ? warehousePK : Docket.WD_WW_Whs;
			}
		}

		public TrackingWhsOrder ShoppingCart { get; set; }

		#endregion

		#region Overrides

		public override ZGuid WI_WD
		{
			get => base.WI_WD;
			set
			{
				base.WI_WD = value;
				docket = null;
			}
		}

		public override ZString WI_InDocketLineType
		{
			get => base.WI_InDocketLineType;
			set
			{
				base.WI_InDocketLineType = value;
				inDocketLine = null;
			}
		}

		public override ZGuid WI_WE_InDocketLine
		{
			get => base.WI_WE_InDocketLine;
			set
			{
				base.WI_WE_InDocketLine = value;
				inDocketLine = null;
			}
		}

		public override ZGuid WI_OP
		{
			get => base.WI_OP;
			set
			{
				base.WI_OP = value;
				supplierPart = null;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Lookups

		public new TrackingWhsInventoryLookups Lookups
		{
			get { return (TrackingWhsInventoryLookups)base.Lookups; }
		}

		protected override WhsInventoryViewLookups GetNewLookups()
		{
			return new TrackingWhsInventoryLookups(this);
		}

		#endregion

		public CustomLabelInfoList GetAdditionalInformationFields()
		{
			CustomLabelsProvider provider = new CustomLabelsProvider(this);
			CustomLabelInfoList customLabels = provider.GetCustomFields(provider.ConfigOrgProvider.ConfigOrg, Factory);

			CustomLabelInfoList customLabelInfoList = new CustomLabelInfoList(typeof(WhsInventoryView), provider.ConfigOrgProvider.ConfigOrg, ResString.GetMultilingualString("a4d5c107-f4bc-413f-a7c4-a0114c847a3c", "the client of the warehouse"), Factory);

			foreach (CustomLabelInfo field in customLabels)
			{
				if (field.IsEnabled && field.LabelName.StartsWith("WhsDocketLine.")) // Its an identifier
				{
					customLabelInfoList.Add(field);
				}
			}

			return customLabelInfoList;
		}

		public ZString PickAllocationsAsString
		{
			get { return (InDocketLine != null) ? InDocketLine.PickAllocationsAsString : ZString.Empty; }
		}

		static T GetTrackingValue<T>(ref T? localVariable, Func<T> getValue) where T : struct
		{
			if (!localVariable.HasValue)
			{
				localVariable = getValue();
			}

			return localVariable.Value;
		}

		#region IWebDocumentsWithUploadSupport Members

		public DocumentSupport DocumentHelper
		{
			get
			{
				return fDocumentHelper ?? (fDocumentHelper = new DocumentSupport(this));
			}
		}
		DocumentSupport fDocumentHelper;

		public ZGuid DocParentPK { get { return PK; } }
		public List<ZGuid> DocRelatedPKs { get { return new List<ZGuid>(); } }
		public OrgContact LoggedInContact { get { return SiteUser.LoggedInUser; } }

		public TrackingSiteUser SiteUser
		{
			get { return fSiteUser ?? (fSiteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser); }
#if DEBUG
			set { fSiteUser = value; }
#endif
		}
		TrackingSiteUser fSiteUser;

		public DocumentUploadSupport DocumentUploadHelper => documentUploadHelper ?? (documentUploadHelper = new DocumentUploadSupport(Factory));
		DocumentUploadSupport documentUploadHelper;

		public void ResetDocumentHelper()
		{
			fDocumentHelper = null;
		}

		#endregion

		#region IWebUserVisibleNotesSupport Members

		public WebUserVisibleNotes NotesHelper
		{
			get
			{
				return fNotesHelper ?? (fNotesHelper = new WebUserVisibleNotes(this));
			}
		}
		WebUserVisibleNotes fNotesHelper;

		public bool ShowAgentNotes { get { return false; } }

		public IStmNoteParent NotesParentBO { get { return this.InDocketLine; } }

		#endregion

		#region Validation

		public new TrackingWhsInventoryValidation Validation
		{
			get { return (TrackingWhsInventoryValidation)base.Validation; }
		}

		protected override WhsInventoryViewValidation GetNewValidation()
		{
			return new TrackingWhsInventoryValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			if (!isOnlyAllocateColumnValidationEnabledOnPreSave)
			{
				base.RunPreSaveValidationCore();
			}

			Validation.ValidateQuantity();
		}

		public void EnableOnlyAllocateColumnValidationOnPreSave()
		{
			isOnlyAllocateColumnValidationEnabledOnPreSave = true;
		}

		bool isOnlyAllocateColumnValidationEnabledOnPreSave;

		#endregion
	}
}
