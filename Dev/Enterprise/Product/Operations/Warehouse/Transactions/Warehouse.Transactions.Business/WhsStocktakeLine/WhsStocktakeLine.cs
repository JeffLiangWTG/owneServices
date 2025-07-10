using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsStocktake), "Lines")]
	public class WhsStocktakeLine : AutoWhsStocktakeLine,
		ILocationConsumer,
		IPartAttributeValidationConsumer,
		ILineAttributes,
		ILineStaffAssigner,
		ILocationCapacityLine,
		ICalculateProductPackageTotals
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventInvalidClientAndProductForNonEmptyLocationTriggerID = "Client or Product cannot be null when exclude empty locations.";

		#region Constructors

		public WhsStocktakeLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoWhsStocktakeLine.Schema
		{
			public const string LocationString = "LocationString";
		}

		#endregion

		#region Related Entities

		public WhsStocktake Stocktake
		{
			get { return Factory.Load<WhsStocktake>(WU_WS); }
		}

		#endregion

		#region Properties

		#region WU_OP

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		[List("Lookups.SupplierParts")]
		public override ZGuid WU_OP
		{
			get { return base.WU_OP; }
			set
			{
				if (value != base.WU_OP)
				{
					base.WU_OP = value;
					SetLineDataBasedOnProduct();
				}
			}
		}

		void SetLineDataBasedOnProduct()
		{
			var part = SupplierPart;
			if (part != null)
			{
				if (Client != null)
				{
					var manager = Client.PartAttributeManager;
					if (!WU_ExpiryDate.IsEmpty && !manager.IsExpiryDateUsedByProduct(part))
					{
						WU_ExpiryDate = ZDate.Empty;
					}

					if (!WU_PackingDate.IsEmpty && !manager.IsPackingDateUsedByProduct(part))
					{
						WU_PackingDate = ZDate.Empty;
					}

					if (!WU_PartAttrib1.IsEmpty && !manager.IsPartAttributeUsedByProduct(part, 1))
					{
						WU_PartAttrib1 = "";
					}

					if (!WU_PartAttrib2.IsEmpty && !manager.IsPartAttributeUsedByProduct(part, 2))
					{
						WU_PartAttrib2 = "";
					}

					if (!WU_PartAttrib3.IsEmpty && !manager.IsPartAttributeUsedByProduct(part, 3))
					{
						WU_PartAttrib3 = "";
					}

					if (!WU_SerialNumber.IsEmpty && !manager.IsSerialNumberUsedByProduct(part))
					{
						WU_SerialNumber = "";
					}
				}
				else
				{
					// setting some default client
					WU_OH_Client = part.RelatedOrganisations.Cast<OrgPartRelation>().Where(ou => ou.IsBoth || ou.IsOwner).Select(ou => ou.OU_OH).FirstOrDefault();
				}
			}
		}

		#endregion

		#region WU_OP_Desc

		public ZString WU_OP_Desc
		{
			get { return (SupplierPart != null) ? SupplierPart.OP_Desc : ZString.Empty; }
		}

		public ZPropertyInfo WU_OP_DescInfo
		{
			get { return GetZPropertyInfo(nameof(WU_OP_Desc)); }
		}

		#endregion

		#region WU_OH_Client

		[ReadOnlyMember(nameof(ClientReadOnly))]
		public override ZGuid WU_OH_Client
		{
			get { return base.WU_OH_Client; }
			set { base.WU_OH_Client = value; }
		}

		#endregion

		#region Product

		public WhsProduct Product
		{
			get { return WhsProduct.GetWhsProduct(Factory, WU_OP); } // Since product can be changed, it should not be lazy loaded
		}

		#endregion

		#region CommodityCode

		public ZString CommodityCode
		{
			get { return SupplierPart != null ? SupplierPart.OP_RH_NKCommodityCode : ZString.Empty; }
		}

		public ZPropertyInfo CommodityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CommodityCode)); }
		}

		#endregion

		#region Location

		public bool ValidateLocation
		{
			get { return true; }
		}

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WU_WL); }
		}

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		[List("Lookups.Locations")]
		[MaxLength(36)]
		public ZString LocationString
		{
			get { return Location?.WLV_LocationString_UserFriendly ?? locationString; }
			set
			{
				var location = Location;
				if ((location != null && location.WLV_LocationString != value) || locationString != value || !WU_WL.IsValid)
				{
					// prevalidation
					CheckMaximumLength(LocationStringInfo, value);
					locationString = value;

					WU_WL = WhsLocation.FindLocationPK(Factory, value, LocationWhsGuid);

					if (!IsValidationSuspended)
					{
						Validation.ValidateLocationString();

						if (!LocationStringInfo.HasErrors())
						{
							Validation.ValidateLocationString();
						}
					}
					LocationStringInfo.RefreshBinding();
				}
			}
		}

		ZString locationString;

		#region LocationWhsGuid

		internal ZGuid LocationWhsGuid
		{
			get { return Stocktake?.WS_WW_Whs ?? (Location?.WLV_WW_Whs ?? ZGuid.Empty); }
		}

		#endregion

		public ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(Schema.LocationString); }
		}

		[List("Lookups.Locations")]
		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		[RelatedBusinessObject("Location")]
		public override ZGuid WU_WL
		{
			get { return base.WU_WL; }
			set
			{
				if (base.WU_WL != value)
				{
					base.WU_WL = value;
				}
			}
		}

		#endregion

		#region Packs

		public ZDecimal WU_PackQty
		{
			get { return ReCalcPackQty(); }
			set
			{
				ReCalcReqUnits(value);
				WU_PackQtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WU_PackQtyInfo
		{
			get { return GetZPropertyInfo(nameof(WU_PackQty)); }
		}

		public ZDecimal ReCalcPackQty()
		{
			if (SupplierPart != null)
			{
				return ZArchitecture.Core.Utilities.Round(SupplierPart.UnitConverter.Convert(CurrentCount, SupplierPart.OP_StockKeepingUnit, WU_F3_NKPackType), 2);
			}
			else
			{
				return 0;
			}
		}

		public ZDecimal ReCalcReqUnits(ZDecimal qty)
		{
			// we have to set base.WI_TotalUnits here so the
			// overridden prop doesnt go into an infinite loop
			if (SupplierPart != null)
			{
				CurrentCount = ZArchitecture.Core.Utilities.Round(SupplierPart.UnitConverter.Convert(qty, WU_F3_NKPackType, SupplierPart.OP_StockKeepingUnit), SupplierPart.OP_CountDecimalPlaces);
			}
			return CurrentCount;
		}

		#region CurrentCount

		public ZDecimal CurrentCount
		{
			get { return GetCurrentCountInfo().CurrentCount; }
			set { SetCurrentCountInfo(CurrentCountInfo.PropertyType.Count, value); }
		}

		#endregion

		#region CurrentCountVerifiedBy

		public ZString CurrentCountVerifiedBy
		{
			get { return GetCurrentCountInfo().CurrentCountVerifiedBy; }
			set { SetCurrentCountInfo(CurrentCountInfo.PropertyType.VerifiedBy, value); }
		}

		#endregion

		#region CurrentCountVerifiedDate

		public ZDateTime CurrentCountVerifiedDate
		{
			get { return GetCurrentCountInfo().CurrentCountVerifiedDate; }
			set { SetCurrentCountInfo(CurrentCountInfo.PropertyType.VerifiedDate, value); }
		}

		#endregion

		#region CurrentCountInfo

		#region GetCurrentCountInfo

		CurrentCountInfo GetCurrentCountInfo()
		{
			var result = new CurrentCountInfo();

			if (WU_TotalCounts > WhsStocktake.MaximumNumberOfColumns)
			{
				throw new NotImplementedException("CountColumnNumber is not implemented.");
			}

			var countColumns = CountRelatedSchemaCollection[CurrentCountInfo.PropertyType.Count];
			var verifiedByColumns = CountRelatedSchemaCollection[CurrentCountInfo.PropertyType.VerifiedBy];
			var verifiedDateColumns = CountRelatedSchemaCollection[CurrentCountInfo.PropertyType.VerifiedDate];

			var currentCountColumn = countColumns[WU_TotalCounts - 1];
			var currentVerifiedByColumn = verifiedByColumns[WU_TotalCounts - 1];
			var currentVerifiedDateColumn = verifiedDateColumns[WU_TotalCounts - 1];

			result.CurrentCount = (ZDecimal)this[currentCountColumn];
			result.CurrentCountVerifiedBy = (ZString)this[currentVerifiedByColumn];
			result.CurrentCountVerifiedDate = (ZDateTime)this[currentVerifiedDateColumn];

			return result;
		}

		#endregion

		#region SetCurrentCountInfo

		void SetCurrentCountInfo(CurrentCountInfo.PropertyType propertyType, IZType value)
		{
			if (!IsClosed)
			{
				if (Stocktake != null)
				{
					if (WU_TotalCounts > WhsStocktake.MaximumNumberOfColumns)
					{
						throw new NotImplementedException("Current count column number is not implemented.");
					}

					var countRelatedColumns = CountRelatedSchemaCollection[propertyType];
					var countRelatedColumnToChange = countRelatedColumns[WU_TotalCounts - 1];
					this[countRelatedColumnToChange] = value;
				}
			}
			else
			{
				throw new ArgumentException("Current Count cannot be set on a closed line.");
			}
		}

		IDictionary<CurrentCountInfo.PropertyType, SchemaColumn[]> GetSchemaColumnCollection()
		{
			return new Dictionary<CurrentCountInfo.PropertyType, SchemaColumn[]>()
			{
				{ CurrentCountInfo.PropertyType.Count, new SchemaColumn[]
					{ WhsStocktakeLineSchema.WU_LastCount, WhsStocktakeLineSchema.WU_Count2, WhsStocktakeLineSchema.WU_Count3 } },
				{ CurrentCountInfo.PropertyType.VerifiedBy, new SchemaColumn[]
					{ WhsStocktakeLineSchema.WU_GS_NKVerifiedBy, WhsStocktakeLineSchema.WU_Count2VerifiedBy, WhsStocktakeLineSchema.WU_Count3VerifiedBy } },
				{ CurrentCountInfo.PropertyType.VerifiedDate, new SchemaColumn[]
					{ WhsStocktakeLineSchema.WU_DateVerified, WhsStocktakeLineSchema.WU_Count2DateVerified, WhsStocktakeLineSchema.WU_Count3DateVerified } }
			};
		}

		IDictionary<CurrentCountInfo.PropertyType, SchemaColumn[]> CountRelatedSchemaCollection
		{
			get { return countRelatedSchemaCollection ?? (countRelatedSchemaCollection = GetSchemaColumnCollection()); }
		}
		IDictionary<CurrentCountInfo.PropertyType, SchemaColumn[]> countRelatedSchemaCollection;

		#endregion

		#endregion

		#region CurrentCountInfo Class

		class CurrentCountInfo
		{
			public enum PropertyType
			{
				Count,
				VerifiedBy,
				VerifiedDate
			}

			public ZDecimal CurrentCount { get; set; }
			public ZString CurrentCountVerifiedBy { get; set; }
			public ZDateTime CurrentCountVerifiedDate { get; set; }
		}

		#endregion

		#endregion

		#region Attributes

		#region WU_PartAttrib1

		[ReadOnlyMember(nameof(PartAttribute1ReadOnly))]
		public override ZString WU_PartAttrib1
		{
			get { return base.WU_PartAttrib1; }
			set
			{
				base.WU_PartAttrib1 = value;
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WU_PartAttrib1, 1);
			}
		}

		#endregion

		#region WU_PartAttrib2

		[ReadOnlyMember(nameof(PartAttribute2ReadOnly))]
		public override ZString WU_PartAttrib2
		{
			get { return base.WU_PartAttrib2; }
			set
			{
				base.WU_PartAttrib2 = value;
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WU_PartAttrib2, 2);
			}
		}

		#endregion

		#region WU_PartAttrib3

		[ReadOnlyMember(nameof(PartAttribute3ReadOnly))]
		public override ZString WU_PartAttrib3
		{
			get { return base.WU_PartAttrib3; }
			set
			{
				base.WU_PartAttrib3 = value;
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WU_PartAttrib3, 3);
			}
		}

		#endregion

		#region WU_SerialNumber

		[ReadOnlyMember(nameof(SerialNumberReadOnly))]
		public override ZString WU_SerialNumber
		{
			get => base.WU_SerialNumber;
			set
			{
				base.WU_SerialNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWU_LastCount();
					Validation.ValidateWU_Count2();
					Validation.ValidateWU_Count3();
				}
			}
		}

		#endregion

		#region WU_PackingDate

		[ReadOnlyMember(nameof(PackingDateReadOnly))]
		public override ZDate WU_PackingDate
		{
			get { return base.WU_PackingDate; }
			set { base.WU_PackingDate = value; }
		}

		#endregion

		#region WU_ExpiryDate

		[ReadOnlyMember(nameof(ExpiryDateReadOnly))]
		public override ZDate WU_ExpiryDate
		{
			get { return base.WU_ExpiryDate; }
			set { base.WU_ExpiryDate = value; }
		}

		#endregion

		#region SetExpiryDateIfJulianBatchNumberAttributeIsUsed

		void SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(ZString partAttribute, int partAttributeNumber)
		{
			var product = Product;
			var stocktake = Stocktake;
			var client = Client;
			if (product != null && stocktake != null && client != null && product.IsPartAttributeAJulianBatchNumberAndUsed(client, partAttributeNumber))
			{
				WU_PackingDate = product.GetPackingDateFromJulianBatchNumber(client, stocktake.Warehouse, partAttribute);
				WU_ExpiryDate = product.CalculateExpiryDate(client, stocktake.Warehouse, partAttribute);
			}
		}

		#endregion

		#endregion

		#endregion

		#region Flags

		#region IsStocktakeFinalised

		public bool IsStocktakeFinalised
		{
			get { return Stocktake != null && Stocktake.IsFinalised; }
		}

		#endregion

		#region IsClosed

		public bool IsClosed
		{
			get { return WU_Status == CodeLists.StocktakeLineStatus.Codes.Closed; }
		}

		#endregion

		#region IsOpen

		public bool IsOpen
		{
			get { return WU_Status == StocktakeLineStatus.Codes.Open; }
		}

		#endregion

		#region IsClosing

		public bool IsClosing { get { return Stocktake.LineClosingSemaphore.IsSuspended; } }

		#endregion

		#region EmptyLocation

		#region IsEmptyLocation

		public bool IsEmptyLocation
		{
			get { return WU_InventoryStatus == "EMP"; }
		}

		#endregion

		#endregion

		#region ProductIsEmpty

		bool ProductIsEmpty
		{
			get
			{
				return AutoLoadedReadOnly
					|| WU_OP.IsEmpty
					|| SupplierPart == null;
			}
		}

		#endregion

		#region CanContinueWithSaveCore

		protected override bool CanContinueWithSaveCore
		{
			get { return base.CanContinueWithSaveCore && !IsEmptyLocation; }
		}

		#endregion

		#endregion

		#region Properties

		#region DecimalPlaces

		public ZInt DecimalPlaces
		{
			get { return SupplierPart?.OP_CountDecimalPlaces ?? 0; }
		}

		#endregion

		#region WU_F3_NKPackType

		[ReadOnly(true)]
		public override ZString WU_F3_NKPackType
		{
			get { return base.WU_F3_NKPackType; }
			set { base.WU_F3_NKPackType = value; }
		}

		#endregion

		#region WU_LineComment

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		public override ZString WU_LineComment
		{
			get { return base.WU_LineComment; }
			set { base.WU_LineComment = value; }
		}

		#endregion

		#region WU_SystemUnits

		[ReadOnly(true)]
		public override ZDecimal WU_SystemUnits
		{
			get { return base.WU_SystemUnits; }
			set { base.WU_SystemUnits = value; }
		}

		#endregion

		#region Counts

		#region WU_LastCount

		[ReadOnlyMember(nameof(NonStandardOrEmptyReadOnly))]
		public override ZDecimal WU_LastCount
		{
			get { return base.WU_LastCount; }
			set
			{
				ClearLocationCapacityValidationCache(base.WU_LastCount, value);

				base.WU_LastCount = value;

				if (WU_DateVerified.IsEmpty)
				{
					WU_DateVerified = ZDateTime.Now;
				}

				ValidatePerPackageQty(); // tested in WhsStocktakeLineValidationForManuallyAddedLinesUS
			}
		}

		#endregion

		#region WU_Count2

		[ReadOnlyMember(nameof(NonStandardOrEmptyReadOnly))]
		public override ZDecimal WU_Count2
		{
			get { return base.WU_Count2; }
			set
			{
				ClearLocationCapacityValidationCache(base.WU_Count2, value);

				base.WU_Count2 = value;

				if (WU_Count2DateVerified.IsEmpty)
				{
					WU_Count2DateVerified = ZDateTime.Now;
				}

				ValidatePerPackageQty(); // tested in WhsStocktakeLineValidationForManuallyAddedLinesUS
			}
		}

		#endregion

		#region WU_Count3

		[ReadOnlyMember(nameof(NonStandardOrEmptyReadOnly))]
		public override ZDecimal WU_Count3
		{
			get { return base.WU_Count3; }
			set
			{
				ClearLocationCapacityValidationCache(base.WU_Count3, value);

				base.WU_Count3 = value;

				if (WU_Count3DateVerified.IsEmpty)
				{
					WU_Count3DateVerified = ZDateTime.Now;
				}

				ValidatePerPackageQty(); // tested in WhsStocktakeLineValidationForManuallyAddedLinesUS
			}
		}

		#endregion

		void ClearLocationCapacityValidationCache(ZDecimal previousValue, ZDecimal newValue)
		{
			if (!previousValue.Equals(newValue))
			{
				Stocktake.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			}
		}

		void ValidatePerPackageQty()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateWU_PerPackageQty();
			}
		}

		#endregion

		#region WU_LineNo

		[ReadOnly(true)]
		public override ZInt WU_LineNo
		{
			get { return base.WU_LineNo; }
			set { base.WU_LineNo = value; }
		}

		#endregion

		#region WU_Status

		[ReadOnly(true)]
		public override ZString WU_Status
		{
			get { return base.WU_Status; }
			set { base.WU_Status = value; }
		}

		#endregion

		#region WU_BondedEntryKey

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		public override ZString WU_BondedEntryKey
		{
			get { return base.WU_BondedEntryKey; }
			set { base.WU_BondedEntryKey = value; }
		}

		#endregion

		#region WU_PackageGroupId

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		public override ZString WU_PackageGroupId
		{
			get { return base.WU_PackageGroupId; }
			set
			{
				base.WU_PackageGroupId = value;

				ValidatePerPackageQty(); // tested in WhsStocktakeLineValidationForManuallyAddedLinesUS
				ValidateCounts(); // tested in WhsStocktakeLineValidationForManuallyAddedLinesUS
			}
		}

		#endregion

		#region WU_PalletID

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		public override ZString WU_PalletID
		{
			get { return base.WU_PalletID; }
			set { base.WU_PalletID = value; }
		}

		#endregion

		#region WU_PerPackageQty

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		public override ZDecimal WU_PerPackageQty
		{
			get { return base.WU_PerPackageQty; }
			set
			{
				base.WU_PerPackageQty = value;
				ValidateCounts(); // tested in WhsStocktakeLineValidationForManuallyAddedLinesUS
			}
		}

		#endregion

		#region WU_DateVerified

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		public override ZDateTime WU_DateVerified
		{
			get { return base.WU_DateVerified; }
			set { base.WU_DateVerified = value; }
		}

		#endregion

		#region WU_GS_NKVerifiedBy

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		[List("Lookups.VerifiedBys")]
		public override ZString WU_GS_NKVerifiedBy
		{
			get { return base.WU_GS_NKVerifiedBy; }
			set { base.WU_GS_NKVerifiedBy = value; }
		}

		#endregion

		#region WU_DateClosed

		[ReadOnly(true)]
		public override ZDateTime WU_DateClosed
		{
			get { return base.WU_DateClosed; }
			set { base.WU_DateClosed = value; }
		}

		#endregion

		#region WU_Count2DateVerified

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		public override ZDateTime WU_Count2DateVerified
		{
			get { return base.WU_Count2DateVerified; }
			set { base.WU_Count2DateVerified = value; }
		}

		#endregion

		#region WU_Count2VerifiedBy

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		[List("Lookups.VerifiedBys")]
		public override ZString WU_Count2VerifiedBy
		{
			get { return base.WU_Count2VerifiedBy; }
			set { base.WU_Count2VerifiedBy = value; }
		}

		#endregion

		#region WU_Count3DateVerified

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		public override ZDateTime WU_Count3DateVerified
		{
			get { return base.WU_Count3DateVerified; }
			set { base.WU_Count3DateVerified = value; }
		}

		#endregion

		#region WU_Count3VerifiedBy

		[ReadOnlyMember(nameof(NonStandardReadOnly))]
		[List("Lookups.VerifiedBys")]
		public override ZString WU_Count3VerifiedBy
		{
			get { return base.WU_Count3VerifiedBy; }
			set { base.WU_Count3VerifiedBy = value; }
		}

		#endregion

		#region WU_InventoryStatus

		[ReadOnlyMember(nameof(AutoLoadedReadOnly))]
		[List("Lookups.InventoryStatuses")]
		public override ZString WU_InventoryStatus
		{
			get { return base.WU_InventoryStatus; }
			set { base.WU_InventoryStatus = value; }
		}

		public ZString StatusDesc
		{
			get { return Lookups.InventoryStatuses.GetDescriptionFromCode(WU_InventoryStatus); }
		}

		public ZPropertyInfo StatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDesc)); }
		}

		#endregion

		void ValidateCounts()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateWU_LastCount();
				Validation.ValidateWU_Count2();
				Validation.ValidateWU_Count3();
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region ILocationConsumer Member

		ZGuid ILocationConsumer.LocationPK
		{
			get { return WU_WL; }
		}

		ZGuid ILocationConsumer.WarehousePK
		{
			get { return LocationWhsGuid; }
		}

		ZString ILocationConsumer.LocationTypeForMessages
		{
			get { return Res.GetString("cb506b65-2a73-4a33-a339-cfda5f034b19", "Stocktake"); }
		}

		ZString ILocationConsumer.LocationTitle { get; set; }

		#endregion

		#region ReadOnly Attribute Properties

		#region NonStandardOrEmptyReadOnly

		protected virtual bool NonStandardOrEmptyReadOnly
		{
			get { return IsEmptyLocation || NonStandardReadOnly; }
		}

		#endregion

		#region NonStandardReadOnly

		protected virtual bool NonStandardReadOnly
		{
			get { return IsClosed || Stocktake == null || !Stocktake.IsLoaded; }
		}

		#endregion

		#region AutoLoadedReadOnly

		protected bool AutoLoadedReadOnly
		{
			get { return NonStandardReadOnly || !WU_IsManuallyAdded; }
		}

		#endregion

		#region PartAttribute1ReadOnly

		protected bool PartAttribute1ReadOnly
		{
			get { return ProductIsEmpty || !Product.IsPartAttributeUsed(Client ?? Stocktake.Client, 1); }
		}

		#endregion

		#region PartAttribute2ReadOnly

		protected bool PartAttribute2ReadOnly
		{
			get { return ProductIsEmpty || !Product.IsPartAttributeUsed(Client ?? Stocktake.Client, 2); }
		}

		#endregion

		#region PartAttribute3ReadOnly

		protected bool PartAttribute3ReadOnly
		{
			get { return ProductIsEmpty || !Product.IsPartAttributeUsed(Client ?? Stocktake.Client, 3); }
		}

		#endregion

		#region SerialNumberReadOnly

		protected bool SerialNumberReadOnly
		{
			get
			{
				return ProductIsEmpty
					|| !Product.IsSerialNumberUsed(Client ?? Stocktake.Client);
			}
		}

		#endregion

		#region ExpiryDateReadOnly

		protected bool ExpiryDateReadOnly
		{
			get
			{
				var result = true;
				if (!ProductIsEmpty)
				{
					var product = Product;
					var client = Client ?? Stocktake.Client;
					result = product == null || client == null || !product.IsExpiryDateUsed(client) || product.IsAJulianBatchNumberAttributeUsed(client);
				}
				return result;
			}
		}

		#endregion

		#region PackingDateReadOnly

		protected bool PackingDateReadOnly
		{
			get
			{
				var result = true;
				if (!ProductIsEmpty)
				{
					var product = Product;
					var client = Client ?? Stocktake.Client;
					result = product == null || client == null || !product.IsPackingDateUsed(client) || product.IsAJulianBatchNumberAttributeUsed(client);
				}
				return result;
			}
		}

		#endregion

		#region ClientReadOnly

		protected bool ClientReadOnly
		{
			get
			{
				var result = AutoLoadedReadOnly;
				if (!result)
				{
					var stockTake = Stocktake;
					if (stockTake != null)
					{
						result = !stockTake.WS_OH_Client.IsEmpty;
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region CloseLine

		public bool CloseLine()
		{
			var result = false;
			Validation.ValidateAll();
			if (!HasErrors)
			{
				WU_Status = StocktakeLineStatus.Codes.Closed;
				WU_DateClosed = ZDateTime.Now;
				Stocktake.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
				result = true;
			}
			return result;
		}

		#endregion

		#region IsEqualLine

		public bool IsEqualLine(WhsStocktakeLine line)
		{
			var result = false;
			if (line != null && !line.IsDeleted)
			{
				result = WU_OP == line.WU_OP
					&& WU_WL == line.WU_WL
					&& WU_OH_Client == line.WU_OH_Client
					&& WU_InventoryStatus == line.WU_InventoryStatus
					&& IsMatchingStocktakeLine(line, line.WU_PalletID, line.WU_PackageGroupId, line.WU_PerPackageQty);
			}
			return result;
		}

		public bool IsMatchingStocktakeLine(ILineAttributes lineAttributes, ZString palletId, ZString packageGroupID, ZDecimal perPackageQty)
		{
			return AttributeComparer.Compare(this, lineAttributes)
				&& WU_PalletID == palletId
				&& WU_PackageGroupId == packageGroupID
				&& WU_PerPackageQty == perPackageQty;
		}

		#endregion

		#region Validation

		protected override WhsStocktakeLineValidation GetNewValidation()
		{
			WhsStocktakeLineValidation result;

			var stocktake = Stocktake;

			if (WU_InventoryStatus == StocktakeLineStatus.Codes.Empty)
			{
				result = new EmptyWhsStocktakeLineValidation(this);
			}
			else if (stocktake != null && (!WU_IsManuallyAdded || IsClosed))
			{
				result = new WhsStocktakeLineValidation(this);
			}
			else if (stocktake != null && WU_IsManuallyAdded)
			{
				result = stocktake.CountryCode == Constants.CountryCodes.UnitedStates
					? new WhsStocktakeLineValidationForManuallyAddedLinesUS(this)
					: new WhsStocktakeLineValidationForManuallyAddedLines(this);
			}
			else
			{
				result = base.GetNewValidation();
			}

			return result;
		}

		#endregion

		// interfaces

		#region IPartAttributeValidationConsumer Members

		public bool IsRegisteredForUniqueSerialNumberChecking
		{
			get { return true; }
		}

		public bool IsValidForUniqueSerialNumberChecking(ZString serialNumber)
		{
			var client = Client ?? Stocktake.Client;
			var product = Product;
			return
				!serialNumber.IsEmpty
				&& client != null
				&& SupplierPart != null
				&& WU_Status == StocktakeLineStatus.Codes.Open
				&& product.IsSerialNumberUsed(client) && !product.IsSerialNumberReleaseCaptured(client);
		}

		public bool IsSerialNumberUsedOnThis(ZString serialNumber)
		{
			return WU_SerialNumber == serialNumber && IsValidForUniqueSerialNumberChecking(serialNumber);
		}

		public bool IsSerialNumberUsedOnSiblings(ZString serialNumber)
		{
			bool result = false;
			if (Stocktake != null)
			{
				result = Stocktake.Lines
					.Where(l => l.PK != PK)
					.Any(l => WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct
						? (l.WU_OP == WU_OP && l.IsSerialNumberUsedOnThis(serialNumber))
						: l.IsSerialNumberUsedOnThis(serialNumber));
			}
			return result;
		}

		public bool IsInventoryAdjustedOutOnSiblings(WhsInventoryView inventory)
		{
			return false;
		}

		#endregion

		#region ILocationCapacityLine Members

		public ZDecimal GetQuantity(WhsLocation location)
		{
			return CurrentCount - WU_SystemUnits;
		}

		ZString ILocationCapacityLine.PalletID => WU_PalletID;

		ZGuid ILocationCapacityLine.ProductPK => WU_OP;

		#endregion

		#region ILineAttributes Members

		public ZString BondedEntryKey => WU_BondedEntryKey;

		public ZDate ExpiryDate => WU_ExpiryDate;

		public ZDate PackingDate => WU_PackingDate;

		public ZString PartAttrib1 => WU_PartAttrib1;

		public ZString PartAttrib2 => WU_PartAttrib2;

		public ZString PartAttrib3 => WU_PartAttrib3;

		public ZString SerialNumber => WU_SerialNumber;

		ZString ILineAttributes.AllocationKey => string.Empty;

		public void SetAttributes(ILineAttributes atributes)
		{
			WU_BondedEntryKey = atributes.BondedEntryKey.ToUpper();
			WU_ExpiryDate = atributes.ExpiryDate;
			WU_PackingDate = atributes.PackingDate;
			WU_PartAttrib1 = atributes.PartAttrib1;
			WU_PartAttrib2 = atributes.PartAttrib2;
			WU_PartAttrib3 = atributes.PartAttrib3;
			WU_SerialNumber = atributes.SerialNumber;
		}

		#endregion

		#region ILineAssigner Members

		void ILineStaffAssigner.AssignLine(GlbStaff staff) => CurrentCountVerifiedBy = staff.GS_Code;

		bool ILineStaffAssigner.CanAssignOrUnAssignLine() => !IsEmptyLocation && Stocktake.IsLoaded && IsOpen && CurrentCountVerifiedDate.IsEmpty;

		void ILineStaffAssigner.UnAssignLine(GlbStaff staff) => throw new NotImplementedException();

		#endregion

		#region ICalculateProductPackageTotals

		ZString ICalculateProductPackageTotals.PackageGroupID => WU_PackageGroupId;

		ZDecimal ICalculateProductPackageTotals.PerPackageQty => WU_PerPackageQty;

		ZGuid ICalculateProductPackageTotals.ProductPK => WU_OP;

		ZGuid ICalculateProductPackageTotals.LocationPK => WU_WL;
		ZGuid ICalculateProductPackageTotals.ClientPK => WU_OH_Client;
		ZDecimal ICalculateProductPackageTotals.Units => CurrentCount;

		#endregion
	}
}
