using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgTradeDetailLookups;

namespace Enterprise.MasterFiles.Business
{
	[UserDefinedValues]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgSales : AutoOrgSales, IOrgSales, ISalesValue
	{
		#region Schema

		public new class Schema : AutoOrgSales.Schema
		{
			public const string OW_ServiceDescription = "OW_ServiceDescription";
			public const string DestinationLocationType = "DestinationLocationType";
			public const string OriginLocationType = "OriginLocationType";

			public const string OW_Calc_TotalAnnualCount = "OW_Calc_TotalAnnualCount";
			public const string OW_Calc_TotalAnnualChargeable = "OW_Calc_TotalAnnualChargeable";
			public const string OW_Calc_TotalAnnualTEU = "OW_Calc_TotalAnnualTEU";
			public const string OW_Calc_TotalAnnualWeight = "OW_Calc_TotalAnnualWeight";
			public const string OW_Calc_TotalAnnualVolume = "OW_Calc_TotalAnnualVolume";
			public const string OW_Calc_TotalAnnualPalletCount = "OW_Calc_TotalAnnualPalletCount";
			public const string OW_Calc_TotalAnnualMetricVolume = "OW_Calc_TotalAnnualMetricVolume";
		}

		#endregion

		public OrgSales(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Logging

		protected override AutologState AutoLoggingState => IsActual ? AutologState.NotLogged : AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				if (ImpExpMode == Constants.Sales.Mode.Import)
				{
					return Res.GetString("4a047102-816c-4e72-956b-449a9b5d9e33", "{0} Trade Lane for {1} {2} {3}", ImpExpMode, DestinationCode, "<-", OriginCode);
				}
				else
				{
					return Res.GetString("4a047102-816c-4e72-956b-449a9b5d9e33", "{0} Trade Lane for {1} {2} {3}", ImpExpMode, OriginCode, "->", DestinationCode);
				}
			}
		}

		#endregion

		#region Associated Sales Calls / Opportunities / Visits

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (!IsSettingHasChangesSuspended && value)
				{
					UpdateTradeLanes();
				}
			}
		}

		void UpdateTradeLanes()
		{
			if (ParentOrganisation != null)
			{
				ParentOrganisation.SalesCollection.SetTradeLanesAsChanged();
			}
		}

		#endregion

		#region Trade Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgTradeDetailCollection TradeDetails
		{
			get
			{
				if (fTradeDetails == null)
				{
					fTradeDetails = new OrgTradeDetailCollection(this);
					fTradeDetails.Load();
					RegisterEditableChildObject(fTradeDetails);
					if (ParentOrganisation != null)
					{
						fTradeDetails.SetReadOnlyIncludingChildren(!ParentOrganisation.SecurityProvider.HasModifySalesTradeProfileSecurity);
					}
				}

				return fTradeDetails;
			}
		}
		OrgTradeDetailCollection fTradeDetails;

		#endregion

		#region Delete

		public override void Delete()
		{
			TradeDetails.RemoveAndDeleteAll();

			if (!IsDeleted)
			{
				DeleteAllAssociations();
			}
			base.Delete();
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && IsEditable; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("de095814-9964-405b-8afc-ea2dfbca0c4c", "This trade lane is linked to another association"); }
		}

		void DeleteAllAssociations()
		{
			if (!IsActual)
			{
				OrgSalesValueAssociationPivot.DeleteAll(this);
			}
		}

		#endregion

		#region Properties

		#region OW_MP_Product

		[List("Lookups.SalesProducts")]
		public override ZGuid OW_MP_Product
		{
			get { return base.OW_MP_Product; }
			set { base.OW_MP_Product = value; }
		}

		public ZString ProductCode
		{
			get
			{
				var product = Product;
				return product != null ? product.MP_Code : ZString.Empty;
			}
		}

		public IOrgSalesProduct Product
		{
			get { return Factory.Load<IOrgSalesProduct>(OW_MP_Product); }
		}

		#endregion

		#region OW_OriginID

		[List("Lookups.Locations")]
		public override ZGuid OW_OriginID
		{
			get
			{
				return base.OW_OriginID;
			}
			set
			{
				if (base.OW_OriginID != value)
				{
					base.OW_OriginID = value;
					var origin = Origin;
					OW_OriginTableCode = origin != null ? origin.VLO_TableCode : ZString.Empty;

					if (ShouldDefaultProperties)
					{
						var product = Product;
						if (product != null && (!product.MP_IsSystemDefined || product.MP_Code == SystemDefinedSalesProductList.Codes.CustomsBrokerage))
						{
							DefaultParentOrganisationAsBuyerIfIsInLocation(origin);
						}
						else
						{
							DefaultParentOrganisationAsSupplierIfIsInLocation(origin);
						}
					}
				}
			}
		}

		public void SetOriginIdAndTableCodeDirectly(ZGuid id, ZString tableCode)
		{
			base.OW_OriginID = id;
			base.OW_OriginTableCode = tableCode;
		}

		public bool OW_OriginID_ReadOnly
		{
			get { return !IsEditable; }
		}

		public ViewLocation Origin
		{
			get { return ViewLocationHelper.GetLocationFromPk(Factory, OW_OriginID); }
		}

		public ZString OriginCode
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.VLO_Code : ZString.Empty;
			}
		}

		public ZString OriginDescription
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.VLO_Description : ZString.Empty;
			}
		}

		public ZString OriginLocationType
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.LocationType : ZString.Empty;
			}
		}

		public ZString OriginCountryCode
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.VLO_CountryCode : ZString.Empty;
			}
		}

		public ZString OriginCountryDescription
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.VLO_CountryDescription : ZString.Empty;
			}
		}

		public ZString OriginStateCode
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.VLO_StateCode : ZString.Empty;
			}
		}

		public ZString OriginStateDescription
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.VLO_StateDescription : ZString.Empty;
			}
		}

		public ZString OriginUnlocoCode
		{
			get
			{
				var origin = Origin;
				return origin != null ? origin.UnlocoCode : ZString.Empty;
			}
		}

		#endregion

		#region OW_DestinationID

		[List("Lookups.Locations")]
		public override ZGuid OW_DestinationID
		{
			get
			{
				return base.OW_DestinationID;
			}
			set
			{
				if (base.OW_DestinationID != value)
				{
					base.OW_DestinationID = value;
					var destination = Destination;
					OW_DestinationTableCode = destination != null ? destination.VLO_TableCode : ZString.Empty;

					if (ShouldDefaultProperties)
					{
						DefaultParentOrganisationAsBuyerIfIsInLocation(destination);
					}
				}
			}
		}

		public void SetDestinationIdAndTableCodeDirectly(ZGuid id, ZString tableCode)
		{
			base.OW_DestinationID = id;
			base.OW_DestinationTableCode = tableCode;
		}

		public bool OW_DestinationID_ReadOnly
		{
			get { return !IsEditable; }
		}

		public ViewLocation Destination
		{
			get { return ViewLocationHelper.GetLocationFromPk(Factory, OW_DestinationID); }
		}

		public ZString DestinationCode
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.VLO_Code : ZString.Empty;
			}
		}

		public ZString DestinationDescription
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.VLO_Description : ZString.Empty;
			}
		}

		public ZString DestinationLocationType
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.LocationType : ZString.Empty;
			}
		}

		public ZString DestinationCountryCode
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.VLO_CountryCode : ZString.Empty;
			}
		}

		public ZString DestinationCountryDescription
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.VLO_CountryDescription : ZString.Empty;
			}
		}

		public ZString DestinationStateCode
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.VLO_StateCode : ZString.Empty;
			}
		}

		public ZString DestinationStateDescription
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.VLO_StateDescription : ZString.Empty;
			}
		}

		public ZString DestinationUnlocoCode
		{
			get
			{
				var destination = Destination;
				return destination != null ? destination.UnlocoCode : ZString.Empty;
			}
		}

		#endregion

		#region OW_WW

		[List("Lookups.Warehouses")]
		public override ZGuid OW_WW
		{
			get { return base.OW_WW; }
			set
			{
				if (base.OW_WW != value)
				{
					if (ShouldDefaultProperties)
					{
						var previousWarehouseHomePort = GetWarehouseHomePort(Warehouse);
						var originIsTheSameAsPreviousWarehouseHomePort = previousWarehouseHomePort != null && previousWarehouseHomePort.PK == OW_OriginID;

						base.OW_WW = value;

						if (OW_OriginID.IsEmpty || originIsTheSameAsPreviousWarehouseHomePort)
						{
							var newWarehouseHomePort = GetWarehouseHomePort(Warehouse);
							if (newWarehouseHomePort != null)
							{
								OW_OriginID = newWarehouseHomePort.PK;
							}
						}
					}
					else
					{
						base.OW_WW = value;
					}
				}
			}
		}
		public bool OW_WW_ReadOnly
		{
			get { return !IsEditable; }
		}

		public void SetWarehousePkDirectly(ZGuid warehousePk)
		{
			base.OW_WW = warehousePk;
		}

		public IWhsWarehouse Warehouse
		{
			get { return Factory.Load<IWhsWarehouse>(OW_WW); }
		}

		public ZString WarehouseDescription
		{
			get
			{
				var warehouse = Warehouse;
				if (warehouse == null)
				{
					return Res.GetString("f0f16e76-33b5-4ff4-85b1-ab363b45e555", "Unknown Warehouse");
				}

				return warehouse.WW_WarehouseName;
			}
		}

		public static RefUNLOCO GetWarehouseHomePort(IWhsWarehouse warehouse)
		{
			if (warehouse == null)
			{
				return null;
			}

			var factory = ((IBusiness)warehouse).Factory;
			var companyBranch = factory.Load<GlbBranch>(warehouse.WW_GB_RelatedCompanyBranch);
			if (companyBranch == null)
			{
				return null;
			}

			return companyBranch.HomePort;
		}

		#endregion

		#region OW_OH_Buyer

		void DefaultParentOrganisationAsBuyerIfIsInLocation(ViewLocation location)
		{
			if (location != null && OW_OH_Buyer.IsEmpty && IsOW_OH_BuyerAllowed)
			{
				var parentOrganisation = ParentOrganisation;
				if (parentOrganisation != null
					&& OW_OH_Supplier != parentOrganisation.PK
					&& location.Contains(parentOrganisation))
				{
					OW_OH_Buyer = parentOrganisation.PK;
				}
			}
		}

		public bool OW_OH_Buyer_ReadOnly
		{
			get
			{
				if (!IsEditable)
				{
					return true;
				}

				return !IsOW_OH_BuyerAllowed;
			}
		}

		bool IsOW_OH_BuyerAllowed
		{
			get { return Product == null || Product.IsBuyerAllowed(this); }
		}

		#endregion

		#region OW_OH_Supplier

		void DefaultParentOrganisationAsSupplierIfIsInLocation(ViewLocation location)
		{
			if (location != null && OW_OH_Supplier.IsEmpty && IsOW_OH_SupplierAllowed)
			{
				var parentOrganisation = ParentOrganisation;
				if (parentOrganisation != null
					&& OW_OH_Buyer != parentOrganisation.PK
					&& location.Contains(parentOrganisation))
				{
					OW_OH_Supplier = parentOrganisation.PK;
				}
			}
		}

		public bool OW_OH_Supplier_ReadOnly
		{
			get
			{
				if (!IsEditable)
				{
					return true;
				}

				return !IsOW_OH_SupplierAllowed;
			}
		}

		bool IsOW_OH_SupplierAllowed
		{
			get { return Product == null || Product.IsSupplierAllowed(this); }
		}

		#endregion

		#region OW_Service

		[List("Lookups.ServiceTypeList")]
		public override ZString OW_Service
		{
			get { return base.OW_Service; }
			set
			{
				if (base.OW_Service != value)
				{
					base.OW_Service = value;

					if (Product != null)
					{
						if (!Product.IsBuyerAllowed(this))
						{
							OW_OH_Buyer = ZGuid.Empty;
						}

						if (!Product.IsSupplierAllowed(this))
						{
							OW_OH_Supplier = ZGuid.Empty;
						}
					}
				}
			}
		}

		public bool OW_Service_ReadOnly
		{
			get { return !IsEditable; }
		}

		public void SetWarehouseServiceDirectly(ZString service)
		{
			base.OW_Service = service;
		}

		[List("Lookups.ServiceTypeInverseList")]
		public ZString OW_ServiceDescription
		{
			get { return Lookups.ServiceTypeInverseList.GetCodeFromDescription(OW_Service); }
			set
			{
				OW_Service = Lookups.ServiceTypeInverseList.GetDescriptionFromCode(value);
			}
		}

		public ZPropertyInfo OW_ServiceDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OW_ServiceDescription, x => OW_ServiceInfo); }
		}

		protected int OW_ServiceDescription_MaxLength
		{
			get { return Lookups.ServiceTypeInverseList.MaxCodeLength; }
		}

		#endregion

		#region IsDomestic

		public bool IsDomestic
		{
			get { return Origin != null && Destination != null && Origin.VLO_CountryCode == Destination.VLO_CountryCode; }
		}

		#endregion

		#region ImpExpMode

		public ZString ImpExpMode
		{
			get
			{
				if (Product != null && Product.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
				{
					return ImpExpMode_Warehouse;
				}
				else if (ParentOrganisation != null)
				{
					if (OW_OH_Buyer == ParentOrganisation.PK)
					{
						return Constants.Sales.Mode.Import;
					}
					else if (OW_OH_Supplier == ParentOrganisation.PK)
					{
						return Constants.Sales.Mode.Export;
					}
				}

				return ZString.Empty;
			}
		}

		ZString ImpExpMode_Warehouse
		{
			get
			{
				if (OW_Service == OrgSalesWarehouseServiceTypesList.Codes.Orders)
				{
					return Constants.Sales.Mode.Export;
				}
				else if (OW_Service == OrgSalesWarehouseServiceTypesList.Codes.Receipts)
				{
					return Constants.Sales.Mode.Import;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region OW_Calc_OtherPort

		public ZString OW_Calc_OtherPort
		{
			get
			{
				switch (ImpExpMode)
				{
					case Constants.Sales.Mode.Import:
						return OriginCode;

					case Constants.Sales.Mode.Export:
						return DestinationCode;

					default:
						return ZString.Empty;
				}
			}
		}

		#endregion

		#region Totals

		public void RefreshTotalAnnualStats()
		{
			OW_Calc_TotalAnnualChargeableInfo.RefreshBinding();
			OW_Calc_TotalAnnualCountInfo.RefreshBinding();
			OW_Calc_TotalAnnualPalletCountInfo.RefreshBinding();
			OW_Calc_TotalAnnualTEUInfo.RefreshBinding();
			OW_Calc_TotalAnnualVolumeInfo.RefreshBinding();
			OW_Calc_TotalAnnualMetricVolumeInfo.RefreshBinding();
			OW_Calc_TotalAnnualWeightInfo.RefreshBinding();
		}

		#region OW_Calc_TotalAnnualCount

		public ZDecimal OW_Calc_TotalAnnualCount
		{
			get { return TotalsCalculator.GetTotalAnnualCount(TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualCountInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualCount); }
		}

		#endregion

		#region OW_Calc_TotalAnnualChargeable

		public ZDecimal OW_Calc_TotalAnnualChargeable
		{
			get { return TotalsCalculator.GetTotalAnnualChargeable(IsDomestic, TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualChargeableInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualChargeable); }
		}

		[List("Lookups.UnitOfWeightList")]
		public ZString OW_Calc_TotalAnnualChargeableUQ
		{
			get { return TotalsCalculator.GetTotalAnnualChargeableUQ(TradeDetailsToIncludeInTotal); }
		}

		#endregion

		#region OW_Calc_TotalAnnualTEU

		public ZDecimal OW_Calc_TotalAnnualTEU
		{
			get { return TotalsCalculator.GetTotalAnnualTEU(TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualTEUInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualTEU); }
		}

		#endregion

		#region OW_Calc_TotalAnnualWeight

		public ZDecimal OW_Calc_TotalAnnualWeight
		{
			get { return TotalsCalculator.GetTotalAnnualWeight(TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualWeightInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualWeight); }
		}

		public ZString OW_Calc_TotalAnnualWeightUQ
		{
			get { return TotalsCalculator.GetTotalAnnualWeightUQ(TradeDetailsToIncludeInTotal); }
		}

		#endregion

		#region OW_Calc_TotalAnnualVolume

		public ZDecimal OW_Calc_TotalAnnualVolume
		{
			get { return TotalsCalculator.GetTotalAnnualVolume(TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualVolume); }
		}

		public ZString OW_Calc_TotalAnnualVolumeUQ
		{
			get { return TotalsCalculator.GetTotalAnnualVolumeUQ(TradeDetailsToIncludeInTotal); }
		}

		#endregion

		#region OW_Calc_TotalAnnualMetricVolume
		public ZDecimal OW_Calc_TotalAnnualMetricVolume
		{
			get { return TotalsCalculator.GetTotalAnnualMetricVolume(TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualMetricVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualMetricVolume); }
		}

		public ZString OW_Calc_TotalAnnualMetricVolumeUQ
		{
			get { return TotalsCalculator.GetTotalAnnualMetricVolumeUQ(); }
		}

		#endregion

		#region OW_Calc_TotalAnnualPalletCount

		public ZDecimal OW_Calc_TotalAnnualPalletCount
		{
			get { return TotalsCalculator.GetTotalAnnualPalletCount(TradeDetailsToIncludeInTotal); }
		}

		public ZPropertyInfo OW_Calc_TotalAnnualPalletCountInfo
		{
			get { return GetZPropertyInfo(Schema.OW_Calc_TotalAnnualPalletCount); }
		}

		#endregion

		protected virtual IEnumerable<OrgTradeDetail> TradeDetailsToIncludeInTotal
		{
			get { return TradeDetails.Cast<OrgTradeDetail>(); }
		}

		OrgTradeDetailTotalsCalculator TotalsCalculator
		{
			get { return totalsCalculator ?? (totalsCalculator = new OrgTradeDetailTotalsCalculator()); }
		}
		OrgTradeDetailTotalsCalculator totalsCalculator;

		#endregion

		#region OW_IsCustomRevenue

		public override ZBool OW_IsCustomRevenue
		{
			get { return base.OW_IsCustomRevenue; }
			set
			{
				if (base.OW_IsCustomRevenue != value)
				{
					base.OW_IsCustomRevenue = value;

					if (!value)
					{
						OW_MonthlyRevenue = 0;
						OW_AnnualRevenue = 0;
						OW_RX_NKRevenueCurrency = "";
					}
				}
			}
		}

		#endregion

		#region TradeLaneDescription

		/// <summary>
		/// Used by OrgSalesCall.TradeProfileDescriptionList AND the DocWrapper for Organisation Client Visits Document
		/// </summary>
		/// <returns>String representation of OrgSales call in format [UNLOCO] ([Import]/[Export]) </returns>
		public ZString TradeLaneDescription
		{
			get
			{
				var impExpModeDescription = ImpExpMode == Constants.Sales.Mode.Import ? Res.GetString("2aed190a-88a1-48d9-8f19-084b46e56303", "Import") : Res.GetString("039639d6-2763-4047-8449-b1fab2e4d723", "Export");
				return OW_Calc_OtherPort + " (" + impExpModeDescription + ")";
			}
		}

		public ZString TradeLaneDetailedDescription
		{
			get
			{
				var result = new ZStringBuilder();

				if (Product != null)
				{
					result.Append(Product.MP_NameMultilingual);
					if (Product.ServiceIsMandatory)
					{
						result.Append(" " + string.Format(CultureInfo.InvariantCulture, "({0})", OW_ServiceDescription));
					}
					result.Append(":");

					if (Product.LocationArrangement == OrgSalesProductLocationArrangement.OriginDestination)
					{
						result.Append(" " + string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", OriginCode, DestinationCode));
					}
					else if (Product.LocationArrangement == OrgSalesProductLocationArrangement.SingleLocation)
					{
						if (Product.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse && Warehouse != null)
						{
							result.Append(" " + Warehouse.WW_WarehouseName);
						}
						else
						{
							result.Append(" " + OriginCode);
						}
					}

					if (Product.MP_Code == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
					{
						var isImport = TradeDetails.OfType<OrgTradeDetail>().Any(x => x.PA_TradeType == CustomsBrokerageTradeTypes.Import);
						var isExport = TradeDetails.OfType<OrgTradeDetail>().Any(x => x.PA_TradeType == CustomsBrokerageTradeTypes.Export);

						if (isImport && !isExport)
						{
							result.Append(" " + Res.GetString("d5cb4de8-112b-4080-94cd-475127104fc1", "(Import)"));
						}
						else if (!isImport && isExport)
						{
							result.Append(" " + Res.GetString("dad20c18-17db-4310-aec4-6e8e0af24652", "(Export)"));
						}
						else if (isImport && isExport)
						{
							result.Append(" " + Res.GetString("03588361-4e6f-4a99-b4d3-7067d15776a4", "(Import/Export)"));
						}
					}
				}

				var shouldShowConsignor = (Supplier != null && Supplier != ParentOrganisation);
				var shouldShowConsignee = (Buyer != null && Buyer != ParentOrganisation);

				if (shouldShowConsignor && !shouldShowConsignee)
				{
					result.Append(" " + Res.GetString("3d3b5c8f-8559-4fc6-a226-67d338dd0f48", "(Consignor: {0})", Supplier.OH_Code));
				}
				else if (!shouldShowConsignor && shouldShowConsignee)
				{
					result.Append(" " + Res.GetString("1baf7535-3981-4fd2-a445-1993e2a9e845", "(Consignee: {0})", Buyer.OH_Code));
				}
				else if (shouldShowConsignor && shouldShowConsignee)
				{
					result.Append(" " + Res.GetString("03170575-6452-4ebf-9804-6bccdfd84a49", "(Consignor: {0}, Consignee: {1})", Supplier.OH_Code, Buyer.OH_Code));
				}

				return result.ToString();
			}
		}

		#endregion

		#region ShouldDefaultProperties

		public bool ShouldDefaultProperties
		{
			get { return !IsDeleted && !IsActual && !isDefaultingPropertiesSuspended; }
		}

		public IDisposable GetDefaultPropertySuspender()
		{
			isDefaultingPropertiesSuspended = true;
			return new DisposableAction(() =>
			{
				isDefaultingPropertiesSuspended = false;
			});
		}

		bool isDefaultingPropertiesSuspended;

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		#endregion

		#region Is Actual

		public bool IsActual
		{
			get { return OW_IsTraded; }
		}

		#endregion

		#region IsEditable

		public virtual bool IsEditable
		{
			get
			{
				var product = Product;
				if (product == null)
				{
					return true;
				}

				if (product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales) && HasMultipleSalesAssociations)
				{
					return false;
				}

				if (product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail) && TradeDetails.Cast<OrgTradeDetail>().Any(x => x.HasMultipleSalesAssociations))
				{
					return false;
				}

				return true;
			}
		}

		#endregion

		#region IsFreight

		public bool IsFreight
		{
			get { return Product != null && Product.IsFreight; }
		}

		#endregion

		#region ParentOrganisation

		public OrgHeader ParentOrganisation
		{
			get
			{
				if (parentOrganisation != null)
				{
					return parentOrganisation;
				}
				else
				{
					HashSet<OrgHeader> possibleParents = new HashSet<OrgHeader>();
					foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						var salesCollection = GetMainSalesCollection(collection);
						if (salesCollection != null)
						{
							possibleParents.Add(salesCollection.Master);
						}
					}

					if (possibleParents.Count > 1)
					{
						foreach (var parent in possibleParents.ToList())
						{
							if (!parent.IsBoundToOrganisationForm)
							{
								possibleParents.Remove(parent);
							}
						}
					}

					return possibleParents.FirstOrDefault();
				}
			}
			set
			{
				parentOrganisation = value;
			}
		}
		OrgHeader parentOrganisation;

		OrgSalesCollection GetMainSalesCollection(BusinessObjectCollection collection)
		{
			var salesCollection = collection as OrgSalesCollection;
			if (salesCollection != null)
			{
				return salesCollection;
			}

			var subsetCollection = collection as ISubsetBusinessObjectCollection;
			if (subsetCollection != null)
			{
				return GetMainSalesCollection(subsetCollection.CollectionToFilter);
			}

			return null;
		}

		#endregion

		#region Sales Related Business Objects

		public IEnumerable<ISalesRelatedBusinessObject> SalesRelatedBusinessObjects
		{
			get { return SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity).OfType<ISalesRelatedBusinessObject>(); }
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgSalesFetchStrategy(this);
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			OrgHeader parentOrganisation = ParentOrganisation;
			if (parentOrganisation == null)
			{
				return CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}
			return !parentOrganisation.SecurityProvider.HasModifySalesTradeProfileSecurity || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		/// <summary>
		/// Update all periods' org
		/// Can not use OrgSales.TradeDetails / OrgTradeDetail.ProspectPeriods directly as they may not contain all records.
		/// </summary>
		/// <param name="newOrgPk"></param>
		public void UpdateAllTradePeriods(ZGuid newOrgPk)
		{
			var details = new OrgTradeDetailCollection(this);
			details.Load();
			var periods = details.OfType<OrgTradeDetail>().SelectMany(x => new OrgTradePeriodCollection(x, false)).Where(x => x.PAS_OH_Client != newOrgPk).ToArray();
			Array.ForEach(periods, (x) => x.PAS_OH_Client = newOrgPk);
		}

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public virtual SalesValueAssociationPivotCollection SalesAssociationPivotCollectionCompanyView
		{
			get
			{
				if (salesAssociationPivotCollectionCompanyView == null)
				{
					salesAssociationPivotCollectionCompanyView = new SalesValueAssociationPivotCollection(this, true);
					RegisterEditableChildObject(salesAssociationPivotCollectionCompanyView);
				}
				return salesAssociationPivotCollectionCompanyView;
			}
		}
		SalesValueAssociationPivotCollection salesAssociationPivotCollectionCompanyView;

		public SalesValueAssociationPivotCollection SalesAssociationPivotCollectionGlobal
		{
			get
			{
				if (salesAssociationPivotCollectionGlobal == null)
				{
					salesAssociationPivotCollectionGlobal = new SalesValueAssociationPivotCollection(this, false);
				}
				return salesAssociationPivotCollectionGlobal;
			}
		}
		SalesValueAssociationPivotCollection salesAssociationPivotCollectionGlobal;

		public bool HasMultipleSalesAssociations
		{
			get { return SalesAssociationPivotCollectionGlobal.Where(x => x.SVP_ActivityTableCode != OrgHeaderSchema.Constants.Prefix).Skip(1).Any(); }
		}

		#endregion

		#region Lost Value Analysis

		public static ZDate GetLostCutoffDate()
		{
			return ZDate.Today.AddYears(-1);
		}

		public ZDate LastTraded
		{
			get
			{
				var periods = TradeDetails.Cast<OrgTradeDetail>().SelectMany(x => x.TradedPeriods.Cast<OrgTradePeriod>());
				return (periods.Any() ? periods.Max(x => x.PAS_LastTraded).Date : ZDate.Empty);
			}
		}

		#endregion

		#region IAuditDetails Members

		ZDateTime IAuditDetails.SystemCreateTimeUtc
		{
			get { return OW_SystemCreateTimeUtc; }
		}

		ZString IAuditDetails.SystemCreateUser
		{
			get { return OW_SystemCreateUser; }
		}

		ZDateTime IAuditDetails.SystemLastEditTimeUtc
		{
			get { return ZDateTime.Empty; }
		}

		ZString IAuditDetails.SystemLastEditUser
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
