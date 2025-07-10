 using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This is a Rating Adapter class based on RateQueryBusinessObject.
	/// </summary>
	public class RateQueryRatingAdapter : RatingAdapter<RateQueryBusinessObject>, IAutoRatingFreightConditionsSupportable, IAutoRatingCompanyTariffLevelProvider, IAutoRatingCustomsInfo
	{
		public RateQueryRatingAdapter(RateQueryBusinessObject rateQuery) : base(rateQuery)
		{
			this.rateQueryBusinessObject = Argument.NotNull(rateQuery, nameof(rateQuery));
		}

		readonly RateQueryBusinessObject rateQueryBusinessObject;

		/// <summary>
		/// AdapterType, which considers RateQuery as a shipment.
		/// </summary>
		public override AdapterType AdapterType => AdapterType.Shipment;

		public override IEnumerable<ZString> CarrierContractNumbers => rateQueryBusinessObject.CarrierContractNumbers;

		public override IEnumerable<ZString> ClientContractNumbers => rateQueryBusinessObject.ClientContractNumbers;

		public override IJobDatesProvider JobDatesProvider => new RateQueryJobDatesProvider(rateQueryBusinessObject.RateQuery);

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return chargeCodeGroups ?? (chargeCodeGroups = GetChargeCodeGroups()); }
		}

		ChargeCodeGroupCollection chargeCodeGroups;

		ChargeCodeGroupCollection GetChargeCodeGroups()
		{
			var result = new ChargeCodeGroupCollection();
			result.AddRange(Env.Registry.Rating.FreightRatedCodes);
			return result;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Shipment; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override ILocation Origin => rateQueryBusinessObject.Origin;

		public override IDocAddress PickupAddress => rateQueryBusinessObject.PickupAddress;

		public override IDocAddress DeliveryAddress => rateQueryBusinessObject.DeliveryAddress;

		public override OrgAddress WharfCTOAddress => Shipment.RatingAdapter?.WharfCTOAddress;

		public override OrgHeader Carrier => rateQueryBusinessObject.Carrier;

		public override IEnumerable<OrgHeader> PossibleCarriers => rateQueryBusinessObject.PossibleCarriers;

		public override ILocation GetVia(CostSell costOrSell) => rateQueryBusinessObject.Via;

		public override ILocation Destination => rateQueryBusinessObject.Destination;

		public override ILocation GetFirstLoad(CostSell costOrSell) => rateQueryBusinessObject.GetRelatedLocation(RateEntryLookups.LocationSourceOption.FirstLoad.Code);
		public override ILocation GetLastDischarge(CostSell costOrSell) => rateQueryBusinessObject.GetRelatedLocation(RateEntryLookups.LocationSourceOption.LastDischarge.Code);
		public override ILocation GetFirstRouteSetLoad(CostSell costOrSell) => rateQueryBusinessObject.GetRelatedLocation(RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code);
		public override ILocation GetLastRouteSetDischarge(CostSell costOrSell) => rateQueryBusinessObject.GetRelatedLocation(RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Source strings")]
		public override Creditors Creditors
		{
			get
			{
				if (creditors == null)
				{
					if (rateQueryBusinessObject.SourceEndpoint != SourceEndpoint.JobCharges)
					{
						creditors = Creditors.New(rateQueryBusinessObject.ServiceProviders.Select(sp => OrgWithSource.New(sp, new List<string>(new[] { "Rates API", "Rate Query" }))));
					}
					else
					{
						creditors = rateQueryBusinessObject.GetCreditors();
					}
				}

				return creditors;
			}
		}
		Creditors creditors;

		public override FreightMode FreightMode => rateQueryBusinessObject.FreightMode;

		public override ZString ContainerMode => rateQueryBusinessObject.ContainerMode;

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				var paymentTerm = new PaymentTermInfos();

				if (!string.IsNullOrWhiteSpace(Parent.RateQuery.Incoterm))
				{
					var type = this.IsDomestic() ? PaymentTermType.DomesticPaymentTerm : PaymentTermType.Incoterm;
					paymentTerm.AddOrReplace(new PaymentTermInfo(type, CostSell.Cost, Parent.RateQuery.Incoterm));
					paymentTerm.AddOrReplace(new PaymentTermInfo(type, CostSell.Revenue, Parent.RateQuery.Incoterm));
				}

				return paymentTerm;
			}
		}

		public override ZString PaymentTermOverride
		{
			get
			{
				return new ZString(rateQueryBusinessObject.RateQuery.PaymentTermOverride);
			}
		}

		public override ZString HBLDeliveryMode => rateQueryBusinessObject.HBLDeliveryMode;

		public override ZString FMCTariffID => rateQueryBusinessObject.FMCTariffID;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			var consolPaymentTerm = Parent.ConsolPaymentTerms;
			var consolPrepaidCollect = consolPaymentTerm.GetPrepaidCollect(costOrSell, chargeCodeGroup);
			if (string.IsNullOrEmpty(consolPrepaidCollect))
			{
				return false;
			}

			var shipmentPrepaidCollect = PaymentTerm.GetPrepaidCollect(CostSell.Revenue, chargeCodeGroup);
			if (string.IsNullOrEmpty(shipmentPrepaidCollect))
			{
				return false;
			}

			return consolPrepaidCollect == shipmentPrepaidCollect;
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				if (measureSet == null)
				{
					var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(rateQueryBusinessObject.Factory);
					measureSet = converter.Convert(rateQueryBusinessObject, AdapterType);
				}

				return measureSet;
			}
		}

		IRateableMeasureSet measureSet;

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var allServiceLevels = new List<ServiceLevelInfo>();
				allServiceLevels.AddRange(rateQueryBusinessObject.CarrierServiceLevels);
				allServiceLevels.AddRange(rateQueryBusinessObject.ClientServiceLevels);
				allServiceLevels.AddRange(rateQueryBusinessObject.GatewayServiceLevels);
				allServiceLevels.AddRange(rateQueryBusinessObject.ShipmentGatewayServiceLevels);

				return new ServiceLevelRatingInformation(allServiceLevels.ToArray());
			}
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				if (debtorOrgs == null)
				{
					debtorOrgs = new DebtorOrgCollection();

					if (Parent.RateQuery.RateParties != null)
					{
						var rateParties = Parent.GetResolvedRateParties();

						if (rateParties.ContainsKey(OrganisationRole.Roles.LC))
						{
							debtorOrgs[Registry.Business.RatingDebtorOrgTypes.LC] = rateParties[OrganisationRole.Roles.LC];
						}

						if (rateParties.ContainsKey(OrganisationRole.Roles.AG))
						{
							debtorOrgs[Registry.Business.RatingDebtorOrgTypes.AG] = rateParties[OrganisationRole.Roles.AG];
						}

						if (rateParties.ContainsKey(OrganisationRole.Roles.CNE))
						{
							debtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE] = rateParties[OrganisationRole.Roles.CNE];
						}

						if (rateParties.ContainsKey(OrganisationRole.Roles.CNR))
						{
							debtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR] = rateParties[OrganisationRole.Roles.CNR];
						}

						if (rateParties.ContainsKey(OrganisationRole.Roles.CCUS))
						{
							debtorOrgs[Registry.Business.RatingDebtorOrgTypes.CCUS] = rateParties[OrganisationRole.Roles.CCUS];
						}
					}
				}

				return debtorOrgs;
			}
		}
		DebtorOrgCollection debtorOrgs;

		public override ILocation PlannedLoad(CostSell costSell) => rateQueryBusinessObject.PlannedLoad;

		public override ILocation PlannedDischarge(CostSell costSell) => rateQueryBusinessObject.PlannedDischarge;

		public override ILocation RateOrigin => rateQueryBusinessObject.RateOrigin;

		public override ILocation RateDestination => rateQueryBusinessObject.RateDestination;

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				if (rateQueryBusinessObject.GoodsValue > 0 && rateQueryBusinessObject.GoodsValueCurrency != null)
				{
					result.Add(MoneyType.ValueType.GoodsValue, new Money(rateQueryBusinessObject.GoodsValue, rateQueryBusinessObject.GoodsValueCurrency));
				}

				if (rateQueryBusinessObject.InsuranceValue > 0 && rateQueryBusinessObject.InsuranceValueCurrency != null)
				{
					result.Add(MoneyType.ValueType.InsuranceValue, new Money(rateQueryBusinessObject.InsuranceValue, rateQueryBusinessObject.InsuranceValueCurrency));
				}

				return result;
			}
		}

		public override ZString DeliveryCartageEquipment
		{
			get
			{
				return rateQueryBusinessObject.RateQuery?.JobInfo?.DeliveryDropMode ?? ZString.Empty;
			}
		}

		public override ZString PickupCartageEquipment
		{
			get
			{
				return rateQueryBusinessObject.RateQuery?.JobInfo?.PickupDropMode ?? ZString.Empty;
			}
		}

		public override OrgHeader ImportBroker => Shipment.ImportBroker;

		public override OrgHeader ExportBroker => Shipment.ExportBroker;

		public RateLineConditionsSupporter ConditionsSupporter
		{
			get
			{
				return new ShipmentRateLineConditionsSupporter(Shipment);
			}
		}

		public int TariffLevel
		{
			get
			{
				return Convert.ToInt32(rateQueryBusinessObject.CompanyTariffLevelOverride);
			}
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				return (Shipment.RatingAdapter as ForwardingShipmentRatingAdapter).JobServices;
			}
		}

		CommonShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = RateQueryToShipmentConverter.CreateSingleConsolidatedShipment(Parent, true);
				}

				return shipment;
			}
		}

		CommonShipment shipment;

		public JobHeader Job
		{
			get
			{
				if (job == null)
				{
					job = new Job.Loader(Shipment).TryCreate();
				}

				return job;
			}
		}

		Job job;

		ZString IAutoRatingCustomsInfo.MessageType => ZString.Empty;

		ZString IAutoRatingCustomsInfo.MessageSubType => ZString.Empty;

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get
			{
				if (entries == null)
				{
					entries = new EntryInfoCollection();

					if (rateQueryBusinessObject.CustomsValue > 0)
					{
						entries.AddNew(0, 0, rateQueryBusinessObject.CustomsValue);
					}
				}

				return entries;
			}
		}

		EntryInfoCollection entries;

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices => new InvoiceInfoCollection();

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice => new InvoiceInfoCollection();

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment => new InvoiceInfoCollection();

		ZInt IAutoRatingCustomsInfo.SubHeaderCount => 0;
	}
}
