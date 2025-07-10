using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Adapt a RateEntry so it can be the input to autorating.
	/// The values on the rate are treated like values on a job, such as rate Origin becomes the job Origin.
	/// Weight, Volume and Package measures are added with amounts of zero, and commodity set to rate commodity.
	/// If the rate has a container type, a single container is added to the container list with container type, commodity, palletized and ownership from the rate.
	///
	/// Used by PricingPageLineSetFactory (somehow).
	/// Used to find company tariff rates that match a rate.
	/// Used in a few other places.
	/// </summary>
	class RateEntryAdapter : RatingAdapter<RateEntry>, IAutoRatingCustomsInfo, IAutoRatingWarehouseInfo
	{
		internal RateEntryAdapter(RateEntry entry)
			: base(entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException(nameof(entry));
			}
		}

		internal RateEntryAdapter(RateLine rateLine)
			: base(rateLine.Parent)
		{
			RateLine = rateLine;
			if (rateLine == null || rateLine.Parent == null)
			{
				throw new ArgumentNullException(nameof(rateLine));
			}
		}

		#region Properties

		RatingHeader RatingHeader
		{
			get { return ratingHeader ?? (ratingHeader = Parent.Parent); }
		}

		RatingHeader ratingHeader;

		readonly RateLine RateLine;

		#endregion

		#region RatingAdapter

		public override AdapterType AdapterType => AdapterType.RateEntry;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return null; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new RateEntryJobDatesProider(Parent); }
		}

		protected ChargeCodeGroupCollection fChargeCodeGroups;

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				if (fChargeCodeGroups == null)
				{
					fChargeCodeGroups = new ChargeCodeGroupCollection();
					foreach (ICodeDescription codeDesc in new ChargeCodeGroupList())
					{
						fChargeCodeGroups.Add(codeDesc.Code);
					}
				}

				return fChargeCodeGroups;
			}
		}

		public override JobServicesCollection JobServices
		{
			get { return new JobServicesCollection(); }
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get { return new AutoRatingStatusInfo(true, ZString.Empty); }
		}

		public override RateType RateTypeToUse
		{
			get { return Parent.RateType(); }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		OrgHeader GetConsignee()
		{
			if (DeliveryAddress?.Organisation != null)
			{
				return (OrgHeader)DeliveryAddress.Organisation;
			}

			return Parent.Consignee;
		}

		OrgHeader GetConsignor()
		{
			if (PickupAddress?.Organisation != null)
			{
				return (OrgHeader)PickupAddress.Organisation;
			}

			return Parent.Consignor;
		}

		OrgHeader GetLocalClient()
		{
			if (RatingHeader != null && (RatingHeader.IsClientRate() || RatingHeader.IsQuote()))
			{
				return RatingHeader.Header;
			}

			return null;
		}

		#endregion

		#region IAutoRatingLocations Members

		public override ILocation GetVia(CostSell costSell) => Parent.Via();

		public override ILocation Destination
		{
			get { return Parent.Destination(); }
		}

		public override ILocation Origin
		{
			get { return Parent.Origin(); }
		}

		public override ILocation PlannedDischarge(CostSell costSell) => Parent.PlannedDischarge();
		public override ILocation PlannedLoad(CostSell costSell) => Parent.PlannedLoad();
		public override ILocation RateOrigin => Parent.RateOrigin();
		public override ILocation RateDestination => Parent.RateDestination();
		public override ILocation GetFirstLoad(CostSell costSell) => Parent.FirstLoad();
		public override ILocation GetLastDischarge(CostSell costSell) => Parent.LastDischarge();
		public override ILocation GetFirstRouteSetLoad(CostSell costSell) => Parent.FirstRouteSetLoad();
		public override ILocation GetLastRouteSetDischarge(CostSell costSell) => Parent.LastRouteSetDischarge();

		#endregion

		#region IAutoRatingOrganisations Members

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.CartageDeliveryAddressOverride; }
		}

		public override IDocAddress PickupAddress
		{
			get { return Parent.CartagePickupAddressOverride; }
		}

		public override ZString DeliveryCartageEquipment
		{
			get
			{
				if (RateLine != null)
				{
					var equipment = RateLine.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType);
					if (equipment != null)
					{
						return equipment.TM_Text;
					}
				}

				return ZString.Empty;
			}
		}

		public override ZString PickupCartageEquipment
		{
			get
			{
				if (RateLine != null)
				{
					var equipment = RateLine.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType);
					if (equipment != null)
					{
						return equipment.TM_Text;
					}
				}
				return ZString.Empty;
			}
		}

		public override OrgHeader Carrier
		{
			get { return Parent.TransportProvider; }
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(OrgWithSource.NewFrom<OrgHeader>(Parent.TI_OH_SupplierInfo), OrgWithSource.NewFrom<OrgHeader>(Parent.TI_OH_TransportProviderInfo)); }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;

				result[RatingDebtorOrgTypes.CNE] = GetConsignee();
				result[RatingDebtorOrgTypes.CNR] = GetConsignor();
				result[RatingDebtorOrgTypes.LC] = GetLocalClient();
				result[RatingDebtorOrgTypes.CCUS] = Parent.ControllingCustomer;

				return result;
			}
		}

		public override IEnumerable<ZString> ClientContractNumbers
		{
			get
			{
				return [Parent.TI_ContractNumber];
			}
		}

		public override IEnumerable<ZString> CarrierContractNumbers
		{
			get
			{
				return [Parent.TI_ContractNumber];
			}
		}

		public override ZString FMCTariffID => Parent.TI_FMCTariffID;

		#endregion

		#region IAutoRatingFreightInfo Members

		public override FreightMode FreightMode
		{
			get { return Parent.FreightMode(); }
		}

		public ZString CommodityCode
		{
			get { return Parent.TI_RH_NKCommodityCode; }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				if (Parent.Container != null)
				{
					var isPalletized = ZBool.False;
					var containerOwnership = ZString.Empty;

					if (RateLine != null && RateLine.TL_WeightVolume == QuantityUnit.CN)
					{
						isPalletized = RateLine.TL_IsOnPallets;
						containerOwnership = RateLine.TL_ContainerOwnership;
					}

					result.AddContainerGroup(
						Parent.TI_RC,
						Parent.TI_RH_NKCommodityCode,
						isPalletized,
						containerOwnership,
						new[] {
							new MeasureInfo.ContainerInfo(isNonOperatedReefer: new ZBool(Parent.TI_IsNonOperatedReefer))
						}
					);
				}

				result.SetWeightAndVolumeWithCommodity(0, 0, Parent.TI_RH_NKCommodityCode);
				result.SetPackageCountWithCommodity(0, Parent.TI_RH_NKCommodityCode);

				return result;
			}
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var result = new List<ServiceLevelInfo>
				{
					new ServiceLevelInfo(Parent.TI_RS_NKServiceLevel_NI, ServiceLevelType.Client),
					new ServiceLevelInfo(Parent.TI_PL_NKCarrierServiceLevel, ServiceLevelType.Carrier)
				};

				return new ServiceLevelRatingInformation(result.ToArray());
			}
		}

		public override Directions JobDirection
		{
			get { return ((IImportExport)Parent).JobDirection; }
		}

		public Money FreightSellRate
		{
			get { return Money.Invalid; }
		}

		public ZString SellAutoratingMode
		{
			get { return Constants.FreightRateAutoratingModes.Code.StandardRate; }
		}

		public Money FreightCostRate
		{
			get { return Money.Invalid; }
		}

		public ZString CostAutoratingMode
		{
			get { return Constants.FreightRateAutoratingModes.Code.StandardRate; }
		}

		#endregion

		#region IAutoRatingCustomsInfo Members

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get { return new EntryInfoCollection(); }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get { return new InvoiceInfoCollection(); }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
		{
			get { return new InvoiceInfoCollection(); }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
		{
			get { return new InvoiceInfoCollection(); }
		}

		ZString IAutoRatingCustomsInfo.MessageType
		{
			get
			{
				if (RateLine != null)
				{
					var messageType = RateLine.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType);
					if (messageType != null)
					{
						return messageType.TM_Text;
					}
				}

				return ZString.Empty;
			}
		}

		ZString IAutoRatingCustomsInfo.MessageSubType
		{
			get
			{
				if (RateLine != null)
				{
					var messageSubType = RateLine.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType);
					if (messageSubType != null)
					{
						return messageSubType.TM_Text;
					}
				}

				return ZString.Empty;
			}
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}

		#endregion

		#region IAutoRatingWarehouseInfo Members

		public ZGuid WarehousePK => Parent?.TI_WW_Warehouse ?? ZGuid.Empty;

		OrgHeader IAutoRatingWarehouseInfo.WarehouseFallbackConsignorForFilterOnly => null;

		#endregion
	}
}
