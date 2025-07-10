using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class RateOneOffShipmentRatingAdapter : RatingAdapter<RateOneOffShipment>,
		IAutoRatingCustomsInfo,
		IAutoRatingGlbCompany
	{
		public RateOneOffShipmentRatingAdapter(RateOneOffShipment parent)
			: base(parent)
		{
		}

		#region RatingAdapter

		public override ZString FMCTariffID
		{
			get
			{
				return Parent.TT_FMCTariffID;
			}
		}

		public override IEnumerable<RefCommodityCode> OverriddenCommodity
		{
			get
			{
				if (Parent.Commodity != null)
				{
					return new[] { Parent.Commodity };
				}
				return Enumerable.Empty<RefCommodityCode>();
			}
		}

		public override AdapterType AdapterType => AdapterType.OneOffQuote;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get
			{
				return invoicingSupporter ??
					   (invoicingSupporter = QuotedBooking.New(Parent.ParentQuote.PK, ZGuid.Empty, Parent.Factory).InvoicingSupporter);
			}
		}
		IJobInvoicingSupporter invoicingSupporter;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new JobDatesProvider<RateOneOffShipment>(Parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				if (chargeCodeGroups == null)
				{
					chargeCodeGroups = new ChargeCodeGroupCollection();
					chargeCodeGroups.AddRange(Env.Registry.Rating.FreightRatedCodes);
					chargeCodeGroups.AddRange(Env.Registry.Rating.BrokerageRatedCodes);
					chargeCodeGroups.AddRange(Env.Registry.Rating.OriginBrokerageRatedCodes);
				}
				chargeCodeGroups.CostChargesFilter = ChargeCodeFilter.AutorateNothing;

				return chargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection chargeCodeGroups;

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.OneOffQuotation; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override ILocation GetVia(CostSell costOrSell) => Parent.ViaLocation;

		public override ILocation Destination
		{
			get { return Parent.DeliveryLocation; }
		}

		public override ILocation Origin
		{
			get { return Parent.ReceivalLocation; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;

				if (DeliveryAddress != null && DeliveryAddress.Organisation != null)
				{
					result[RatingDebtorOrgTypes.CNE] = (OrgHeader)DeliveryAddress.Organisation;
				}

				if (PickupAddress != null && PickupAddress.Organisation != null)
				{
					result[RatingDebtorOrgTypes.CNE] = (OrgHeader)PickupAddress.Organisation;
				}

				if (Parent.ParentQuote != null && Parent.ParentQuote.Header != null)
				{
					if (Parent.TT_OrgRole == Core.Constants.OrgRoles.LocalClient)
					{
						result[RatingDebtorOrgTypes.LC] = Parent.ParentQuote.Header;
					}
					else
					{
						result[RatingDebtorOrgTypes.AG] = Parent.ParentQuote.Header;
					}
				}

				return result;
			}
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.DeliveryDocAddress; }
		}

		public override ZString DeliveryCartageEquipment
		{
			get { return Parent.TT_DeliveryEquipment; }
		}

		public override IDocAddress PickupAddress
		{
			get { return Parent.PickUpDocAddress; }
		}

		public override ZString PickupCartageEquipment
		{
			get { return Parent.TT_PickupEquipment; }
		}

		public override OrgHeader Carrier
		{
			get { return Parent.Carrier; }
		}

		public override IEnumerable<OrgHeader> PossibleCarriers =>
			Parent.PossibleCarriers.Cast<RateOneOffCarrier>().Select(x => x.Carrier).WhereNotNull();

		public override IJobExRateCurrencyConverter CurrencyConverter =>
			currencyConverter ?? (currencyConverter = base.CurrencyConverter ?? NonOrgSpecificExRateCurrencyConverter.Default(Parent.Factory));

		IJobExRateCurrencyConverter currencyConverter;

		public override Creditors Creditors
		{
			get
			{
				var creditors = new Creditors();
				foreach (var chargeCodeGroup in ChargeCodeGroups)
				{
					var creditorSource = OrgWithSource.NewFrom<OrgHeader>(Parent.TT_OH_CreditorInfo);
					var creditorSourceList = new OrgPrioritizedList { { 1, creditorSource } };
					creditors.Add(chargeCodeGroup, creditorSourceList);
					var carrierSource = OrgWithSource.NewFrom<OrgHeader>(Parent.TT_OH_CarrierInfo);
					creditors[chargeCodeGroup].Merge(new OrgPrioritizedList { { 2, carrierSource } });

					foreach (RateOneOffCarrier possibleCarrier in Parent.PossibleCarriers)
					{
						var priority = 1;
						if (!possibleCarrier.TTC_OH_Creditor.IsEmpty && possibleCarrier.TTC_OH_Creditor != Parent.TT_OH_Creditor)
						{
							var possibleCreditorSource = OrgWithSource.NewFrom<OrgHeader>(possibleCarrier.TTC_OH_CreditorInfo);
							creditors[chargeCodeGroup].Merge(new OrgPrioritizedList { { priority++, possibleCreditorSource } });
						}
						if (possibleCarrier.TTC_OH_Carrier != Parent.TT_OH_Carrier)
						{
							var possibleCarrierSource = OrgWithSource.NewFrom<OrgHeader>(possibleCarrier.TTC_OH_CarrierInfo);
							creditors[chargeCodeGroup].Merge(new OrgPrioritizedList { { priority, possibleCarrierSource } });
						}
					}
				}
				return creditors;
			}
		}

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				result.Add(MoneyType.ValueType.GoodsValue, new Money(Parent.TT_ValueOfGoods, Parent.GoodsCurrency));
				result.Add(MoneyType.ValueType.InsuranceValue, new Money(Parent.TT_InsureVal, Parent.InsureValCurr));

				return result;
			}
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				result.AddPartList(MeasureType.Chargeable, new JobLevelPart
				{
					CommodityCode = Parent.TT_RH_NKCommodity,
					ChargeableUnit = Parent.TT_ChargeableUnit,
					ChargeableMeasure = new ClientProviderValues(Parent.TT_Chargeable, Parent.TT_Chargeable, Parent.TT_Chargeable)
				});

				result.AddPartList(MeasureType.Shipment, new JobLevelPart { ShipmentCount = 1 });

				AddPackages(result);
				AddContainers(result);

				// Weight, Volume
				if (Parent.TT_ContainerMode == Constants.ContainerModes.ULD && Parent.LooseCargo.Any())
				{
					// For ULD rates we want to get autorate measures from pack lines rather than from goods.
					// Using goods measures for non-FRT charges will be wrong (c) Katherine
					AddPackLineMeasures(Parent.LooseCargo.Cast<RateOneOffPackLine>(), result);
				}
				else
				{
					AddGoodsMeasures(result);
				}

				OverrideCommodityIfNeeded(result);

				return result;
			}
		}

		void OverrideCommodityIfNeeded(RateableMeasureSet result)
		{
			var overriddenCommodity = OverriddenCommodity.FirstOrDefault();
			if (overriddenCommodity != null)
			{
				result.UpdateCommodities(overriddenCommodity.RH_Code);
			}
		}

		void AddGoodsMeasures(RateableMeasureSet measures)
		{
			measures.AddPartList(MeasureType.Weight, new JobLevelPart
			{
				CommodityCode = Parent.TT_RH_NKCommodity,
				Weight = Parent.TT_ActualWeight,
				WeightUnit = Parent.TT_UnitOfWeight
			});

			measures.AddPartList(MeasureType.Volume, new JobLevelPart
			{
				CommodityCode = Parent.TT_RH_NKCommodity,
				Volume = Parent.TT_ActualVolume,
				VolumeUnit = Parent.TT_UnitOfVolume
			});
		}

		void AddPackLineMeasures(IEnumerable<RateOneOffPackLine> packLines, RateableMeasureSet measures)
		{
			var partsList = new RateablePartList
			{
				WeightUnit = Parent.TT_UnitOfWeight,
				VolumeUnit = Parent.TT_UnitOfVolume,
				HasCommodity = true,
				HasContainerType = true
			};

			foreach (var packLine in packLines)
			{
				partsList.AddPart(new RateablePart
				{
					Weight = FreightRatingHelper.Convert(packLine.TPL_Weight, packLine.TPL_WeightUQ, Parent.TT_UnitOfWeight),
					Volume = FreightRatingHelper.Convert(packLine.TPL_Volume, packLine.TPL_VolumeUQ, Parent.TT_UnitOfVolume),
					ContainerTypePk = packLine.TPL_RC_RefContainer.IsEmpty ? null : NullableHelper.ToNullable(packLine.TPL_RC_RefContainer.ToGuid()),
					CommodityCode = Parent.TT_RH_NKCommodity,
				});
			}

			measures.AddPartList(MeasureType.Weight, partsList);
			measures.AddPartList(MeasureType.Volume, partsList);
		}

		void AddPackages(RateableMeasureSet measures)
		{
			var partsList = new RateablePartList
			{
				HasCommodity = true,
				HasPackageType = true
			};

			foreach (var packLine in Parent.LooseCargo.Cast<RateOneOffPackLine>())
			{
				partsList.AddPart(new RateablePart
				{
					CommodityCode = Parent.TT_RH_NKCommodity,
					PackageCount = packLine.TPL_PackLineCount,
					UnitCount = packLine.TPL_PackLineCount,
					PackageType = packLine.TPL_F3_NKPackType,
				});
			}

			measures.AddPartList(MeasureType.Package, partsList);
			measures.AddPartList(MeasureType.Unit, partsList);
		}

		void AddContainers(RateableMeasureSet rateableMeasures)
		{
			foreach (var containersByType in Parent.Containers.Cast<RateOneOffContainers>().GroupBy(c => c.TC_RC))
			{
				var packs = Parent.LooseCargo.Cast<RateOneOffPackLine>()
					.Where(lc => lc.TPL_RC_RefContainer == containersByType.Key)
					.ToList();

				// We need to convert weight and volume of all packs to the same unit.
				// It doesn't matter which one as it will be converted to KG/M3 later anyway (this is how autorating works with containers),
				// so, we convert to KG/M3.
				var weight = packs.Sum(p => FreightRatingHelper.Convert(p.TPL_Weight, p.TPL_WeightUQ, Constants.Weight.Kilograms));
				var volume = packs.Sum(p => FreightRatingHelper.Convert(p.TPL_Volume, p.TPL_VolumeUQ, Constants.Volume.CubicMetres));

				var containerInfo = new MeasureInfo.ContainerInfo(
					weight: weight, weightUnit: Constants.Weight.Kilograms,
					volume: volume, volumeUnit: Constants.Volume.CubicMetres,
					containerCount: containersByType.Sum(c => c.TC_ContainerCount),
					teu: containersByType.Sum(c => c.Calc_TEUCount));

				rateableMeasures.AddContainerGroup(containersByType.Key, Parent.TT_RH_NKCommodity, new[] { containerInfo });
			}

			// for mixed-mode shipments (e.g SEA (FCL/LCL), ROA (FRO/LRO/FTL), RAI (FRA/LRA/FWL)), the LCL information needs to be passed in to the
			// container collection with weight/volume information (container type and count are irrelevant)
			if (Parent.Mode == Core.Constants.RateMode.SEA || Parent.Mode == Core.Constants.RateMode.ROA || Parent.Mode == Core.Constants.RateMode.RAI)
			{
				rateableMeasures.AddLCL(Parent.TT_RH_NKCommodity, Parent.TT_ActualWeight, Parent.TT_UnitOfWeight, Parent.TT_ActualVolume, Parent.TT_UnitOfVolume);
			}
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				var infos = new PaymentTermInfos();
				if (!string.IsNullOrWhiteSpace(Parent.TT_IncoTerm))
				{
					var type = Parent.IsDomesticFreight ? PaymentTermType.DomesticPaymentTerm : PaymentTermType.Incoterm;
					infos.AddOrReplace(new PaymentTermInfo(type, CostSell.Revenue, Parent.TT_IncoTerm));
					infos.AddOrReplace(new PaymentTermInfo(type, CostSell.Cost, Parent.TT_IncoTerm));
				}

				return infos;
			}
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			if (costOrSell == CostSell.Cost && chargeCodeGroup == ChargeCodeGroupList.Codes.Freight)
			{
				return false;
			}
			return true;
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var result = new List<ServiceLevelInfo>
				{
					new ServiceLevelInfo(Parent.TT_RS_NKServiceLevel, ServiceLevelType.Client),
					new ServiceLevelInfo(Parent.TT_PL_NKCarrierServiceLevel, ServiceLevelType.Carrier)
				};
				return new ServiceLevelRatingInformation(result.ToArray());
			}
		}

		public override FreightMode FreightMode
		{
			get { return RateOneOffShipment.FreightModeConverter.GetFreightMode(Parent.TT_TransportMode, Parent.TT_ContainerMode, Parent); }
		}

		public override ZString ContainerMode
		{
			get { return Parent?.TT_ContainerMode ?? ZString.Empty; }
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		public override IEnumerable<ZString> CarrierContractNumbers
		{
			get
			{
				return Parent.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);
			}
		}

		public override ZString NamedAccount
		{
			get
			{
				return Parent.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)?.CE_EntryNum ?? ZString.Empty;
			}
		}

		#endregion

		#region IAutoRatingCustomsInfo Members

		ZString IAutoRatingCustomsInfo.MessageType
		{
			get { return ZString.Empty; }
		}

		ZString IAutoRatingCustomsInfo.MessageSubType
		{
			get { return ZString.Empty; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get
			{
				var results = new InvoiceInfoCollection();
				results.AddNew(Money.Empty, Parent.TT_NumberOfEntryLines);
				return results;
			}
		}

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get
			{
				var results = new EntryInfoCollection();
				for (int i = 0; i < Parent.TT_NumberOfEntries; i++)
				{
					results.AddNew(Parent.TT_NumberOfEntryLines, Parent.TT_NumberOfEntryLines, 0m);
				}
				return results;
			}
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
		{
			get { return new InvoiceInfoCollection(); }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
		{
			get { return new InvoiceInfoCollection(); }
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}

		#endregion

		#region IAutoRatingGlbCompany Members

		GlbCompany IAutoRatingGlbCompany.Company
		{
			get { return Parent.ParentQuote.Company; }
		}

		#endregion

		#region IAutoRatingAccountingInfo Members

		public override ZString QuoteNumber
		{
			get { return Parent.ParentQuote.TH_QuoteNumber; }
		}

		#endregion
	}
}
