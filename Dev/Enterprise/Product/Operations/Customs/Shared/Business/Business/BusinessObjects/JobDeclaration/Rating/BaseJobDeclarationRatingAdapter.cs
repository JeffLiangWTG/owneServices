using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class BaseJobDeclarationRatingAdapter<T> : RatingAdapter<T>, IAutoRatingCustomsInfo, IAutoRatingFreightConditionsSupportable, IJobDataUpdater
		where T : BaseJobDeclaration
	{
		public BaseJobDeclarationRatingAdapter(T parent)
			: base(parent)
		{
			if (!parent.HasMessageInitiator)
			{
				parent.MessageInitiator = new SendsMessagesToCustomsReturningResultsAsProperties(true);
			}
		}

		#region RatingAdapter

		public override AdapterType AdapterType => AdapterType.CustomsDeclaration;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new DeclarationJobDatesProvider(Parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();

				if (Parent.IsImport())
				{
					result.AddRange(Env.Registry.Rating.BrokerageRatedCodes);
				}
				else
				{
					result.AddRange(Env.Registry.Rating.OriginBrokerageRatedCodes);
				}

				if (Parent.Shipment != null)
				{
					foreach (string group in Env.Registry.Rating.FreightRatedCodes)
					{
						if (group == ChargeCodeGroupList.Codes.Brokerage ||
							group == ChargeCodeGroupList.Codes.CustomsDuty ||
							group == ChargeCodeGroupList.Codes.OriginBrokerage)
						{
							continue;
						}

						if (result.Contains(group))
						{
							result.Remove(group);
						}
					}

					if (result.Contains(ChargeCodeGroupList.Codes.BrokerageOnly))
					{
						result.Remove(ChargeCodeGroupList.Codes.BrokerageOnly);
					}

					if (result.Contains(ChargeCodeGroupList.Codes.OriginBrokerageOnly))
					{
						result.Remove(ChargeCodeGroupList.Codes.OriginBrokerageOnly);
					}
				}

				return result;
			}
		}

		#region Job Services

		public override JobServicesCollection JobServices
		{
			get
			{
				var jobServices = new JobServicesCollection();
				jobServices.Add(GetCartageDemurrage());
				jobServices.Add(GetLabor());
				jobServices.Add(GetBeyondCartage());
				jobServices.AddRange(GetJobServices());

				return jobServices;
			}
		}

		IEnumerable<JobServiceInfo> GetJobServices()
		{
			var servcies = new List<JobServiceInfo>();

			var containers = Parent.CusContainers
					.Cast<BaseCusContainer>()
					.Select(c => c.JobContainer)
					.ToArray();

			var docsAndCartageServices = Parent.DocsAndCartage.Services;
			var docsAndCartageServicesInfos = GetServiceInfosFromJobServices(docsAndCartageServices);

			foreach (var service in docsAndCartageServicesInfos)
			{
				if (service.IsEnabled)
				{
					servcies.Add(service);
				}
				else
				{
					var containerServices = FreightRatingHelper.GetServiceInfosFromContainers(containers, service.ServiceCode, FreightRatingHelper.GetServiceInfoDefault);
					servcies.AddRange(containerServices);
				}
			}

			return servcies;
		}

		#region Delegates

		JobServiceInfo GetCartageDemurrage()
		{
			var isEnabled = Parent.JE_PickupOrDeliveryTruckWaitTime.IsValid;
			var description = Res.GetString("e760f429-52de-4ec5-ab4b-3e956204ebae", "Port Transport Demurrage");
			var duration = isEnabled ? Parent.JE_PickupOrDeliveryTruckWaitTime.ToTimeSpan() : new TimeSpan();

			return new JobServiceInfo(isEnabled, "", ChargeCodeSubGroupList.CartageDemurrageTotal, description, null, duration);
		}

		JobServiceInfo GetLabor()
		{
			var isEnabled = Parent.JE_DeliveryOrPickupLabourTime.IsValid;
			var description = Res.GetString("79ac25c9-6059-4358-811f-db9a25751ee4", "Labor");
			var duration = isEnabled ? Parent.JE_DeliveryOrPickupLabourTime.ToTimeSpan() : new TimeSpan();

			return new JobServiceInfo(isEnabled, "", ChargeCodeSubGroupList.Labor, description, null, duration);
		}

		JobServiceInfo GetBeyondCartage()
		{
			var isEnabled = Parent.IsExport || Parent.IsNonTransportDeclarationType
				? HasBeyondCartage(Parent.SupplierPickupAddress, Parent.JE_RL_NKOrigin)
				: HasBeyondCartage(Parent.ImporterDeliveryAddress, Parent.JE_RL_NKFinalDestination);
			var description = Res.GetString("b256d76c-4d41-42cc-87e0-7a38695740c1", "Port Transport Beyond");

			return new JobServiceInfo(isEnabled, "", ChargeCodeSubGroupList.CartageBeyondPostcode, description);
		}

		ZBool HasBeyondCartage(JobDocAddress pickupOrDeliveryAddress, ZString locationCode)
		{
			ZBool result = false;
			var customsCountryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Parent.CountryCode);

			if (customsCountryOfJurisdiction == Core.Constants.CountryCodes.UnitedStates || customsCountryOfJurisdiction == Core.Constants.CountryCodes.Canada)
			{
				var aCIZone = RefDomesticCartageZone.GetZone(Parent.Factory, pickupOrDeliveryAddress.E2_Postcode, locationCode, pickupOrDeliveryAddress.E2_City);

				if (aCIZone != null)
				{
					result = aCIZone.F1_IsBeyond;
				}
			}
			else
			{
				IOrgHeader transportProvider = Parent.DeliveryOrPickupCartageCo;
				ILocation location = LocationHelper.GetLocationFromString(locationCode, Parent.Factory);

				string countryCode = pickupOrDeliveryAddress.E2_RN_NKCountryCode;
				string postCode = pickupOrDeliveryAddress.E2_Postcode;
				string citySuburb = pickupOrDeliveryAddress.E2_City;

				var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();
				result = rateTransportZoneHelper.IsBeyond(Parent.Factory, transportProvider, location, countryCode, postCode, citySuburb);
			}

			return result;
		}

		#endregion

		#endregion

		protected virtual AutoRatingStatusInfo GetStatusInformationCore()
		{
			var canExecute = !Parent.MergeManager.RequiresMerge || Parent.DoMerge();
			var messageInitiator = Parent.MessageInitiator as SendsMessagesToCustomsReturningResultsAsProperties;
			return new AutoRatingStatusInfo(canExecute, canExecute || messageInitiator == null ? ZString.Empty : messageInitiator.MergeResult);
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				var declarationCustomsCharges = (ICustomsCharges)Parent.GetService(typeof(ICustomsCharges));
				return declarationCustomsCharges != null && declarationCustomsCharges.IsActive ? GetStatusInformationCore() : new AutoRatingStatusInfo(true, ZString.Empty);
			}
		}

		public override RateType RateTypeToUse => RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.Value && Parent.Shipment == null
			? RateType.Customs
			: RateType.Forwarding;

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Brokerage; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override ILocation Origin
		{
			get { return Parent.Origin; }
		}

		public override ILocation Destination
		{
			get { return Parent.FinalDestination; }
		}

		public override OrgHeader Carrier
		{
			get { return Parent.ShippingLine; }
		}

		public override OrgHeader ImportBroker
		{
			get { return GlbBranch.CurrentBranch.OrgProxy; }
		}

		public override OrgHeader ExportBroker
		{
			get { return GlbBranch.CurrentBranch.OrgProxy; }
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(GetCreditorsForAutoRating()); }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;

				AddDebtorOrgIfNeed(result, Parent.Consignor, RatingDebtorOrgTypes.CNR);
				AddDebtorOrgIfNeed(result, Parent.Consignee, RatingDebtorOrgTypes.CNE);
				AddDebtorOrgIfNeed(result, Parent.ControllingCustomer, RatingDebtorOrgTypes.CCUS);

				return result;
			}
		}

		void AddDebtorOrgIfNeed(DebtorOrgCollection orgs, OrgHeader orgHeader, RatingDebtorOrgTypes debtorOrgType)
		{
			if (orgHeader != null && orgHeader.PK != OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation)
			{
				orgs[debtorOrgType] = orgHeader;
			}
		}

		protected virtual IEnumerable<OrgWithSource> GetCreditorsForAutoRating()
		{
			yield return OrgWithSource.NewFrom<OrgHeader>(Parent.JE_OH_ShippingLineInfo);
			yield return OrgWithSource.NewFrom<OrgHeader>(Parent.JE_OH_ForwarderInfo);
			yield return Parent.IsImport ? OrgWithSource.NewFrom<OrgAddress>(Parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo) : OrgWithSource.NewFrom<OrgAddress>(Parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
			yield return Parent.ContainerTerminalOperatorDocAddress.Address != null ? OrgWithSource.NewFrom<OrgAddress>(Parent.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo) : null;
			yield return Parent.ContainerYardDocAddress.Address != null ? OrgWithSource.NewFrom<OrgAddress>(Parent.ContainerYardDocAddress.E2_OA_AddressInfo) : null;
			yield return Parent.DepotDocAddress.Address != null ? OrgWithSource.NewFrom<OrgAddress>(Parent.DepotDocAddress.E2_OA_AddressInfo) : null;
			yield return Parent.ShouldAutoRatingForExternalBroker ? OrgWithSource.NewFrom<OrgHeader>(Parent.JE_OH_ExternalBrokerInfo) : null;
		}

		public override ZString DeliveryCartageEquipment
		{
			get { return Parent.JE_FCLDeliveryOrPickupEquipmentNeeded; }
		}

		public override ZString PickupCartageEquipment
		{
			get { return Parent.JE_FCLDeliveryOrPickupEquipmentNeeded; }
		}

		public override IDocAddress DeliveryAddress
		{
			get
			{
				if (Parent.IsImport)
				{
					var deliveryAddress = Parent.ClientPickupDeliveryAddress;
					if (IsValidOrgAddress(deliveryAddress))
					{
						return deliveryAddress;
					}
				}

				if (Parent.Importer != null && !Parent.Importer.IsMiscellaneous)
				{
					return Parent.Importer.MainAddress;
				}

				return null;
			}
		}

		public override IDocAddress PickupAddress
		{
			get
			{
				if (!Parent.IsImport)
				{
					var pickupAddress = Parent.ClientPickupDeliveryAddress;
					if (IsValidOrgAddress(pickupAddress))
					{
						return pickupAddress;
					}
				}

				if (Parent.Supplier != null && !Parent.Supplier.IsMiscellaneous)
				{
					return Parent.Supplier.MainAddress;
				}

				return null;
			}
		}

		static bool IsValidOrgAddress(JobDocAddress address)
		{
			return !address.IsEmpty && address.Organisation != null && !address.Organisation.IsMiscellaneous;
		}

		public override FreightMode FreightMode
			=> FreightRatingHelper.CalculateFreightMode
			(
				FreightTransportMode,
				ContainerMode,
				(RateableMeasureSet)RateableMeasures
			);

		// For BBK/BLK/ROR/BCN, consider parent.ContainerMode because Parent.FreightContainerMode doesn't has ROR (Product decision)
		public override ZString ContainerMode
			=> Parent.ContainerMode.In(ContainerModes.BreakBulk, ContainerModes.Bulk, ContainerModes.RollOnRollOff, ContainerModes.FCLMixedShipper)
				&& RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value
				? Parent.ContainerMode
				: Parent.FreightContainerMode;

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var result = new List<ServiceLevelInfo>();
				result.Add(new ServiceLevelInfo(Parent.JE_RS_NKServiceLevel, ServiceLevelType.Client));
				result.Add(new ServiceLevelInfo(Parent.JE_RS_NKServiceLevel, ServiceLevelType.Carrier));
				return new ServiceLevelRatingInformation(result.ToArray());
			}
		}

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				result.Add(MoneyType.ValueType.GoodsValue, Parent.GoodsValue);

				AddAdditionalRatingMoneyAmounts(result);

				return result;
			}
		}

		protected virtual void AddAdditionalRatingMoneyAmounts(MoneyType amounts)
		{
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				var commodity = GetUniqueCommodityFromShipmentPackLines();//get commodity from shipment first because commodity cannot be entered on a declaration, it comes straight from the container commodity - and that may not be correct for LCL. 

				if (commodity.IsEmpty)
				{
					commodity = GetUniqueCommodityFromDeclarationContainers();
				}

				if (!Parent.JE_TotalWeightUnit.IsEmpty)
				{
					result.SetWeightWithCommodity(Parent.JE_TotalWeight, Parent.JE_TotalWeightUnit, commodity);
				}

				if (!Parent.JE_TotalVolumeUnit.IsEmpty)
				{
					result.SetVolumeWithCommodity(Parent.JE_TotalVolume, Parent.JE_TotalVolumeUnit, commodity);
				}

				result.SetPackageCountWithCommodity((decimal)Parent.JE_TotalNoOfPacks, commodity);
				result.CreatePackageUnitList(true);
				SetAutoRatingContainers(result);
				result.LowestBill = Parent.LowestBills.Count;
				result.Shipments = Math.Max(1, ShipmentCountCore);

				AddAdditionalRatingMeasurements(result);

				return result;
			}
		}

		protected virtual ZDecimal ShipmentCountCore
		{
			get
			{
				var houseBills = Parent.Bills.FindByBillType(BillTypeList.Codes.HouseBill);
				var subHouseBills = Parent.Bills.FindByBillType(BillTypeList.Codes.SubHouseBill);
				return houseBills.Length + subHouseBills.Length;
			}
		}

		protected virtual void AddAdditionalRatingMeasurements(RateableMeasureSet measures)
		{
		}

		ZString GetUniqueCommodityFromDeclarationContainers()
		{
			var result = ZString.Empty;

			if (Parent.CusContainers.Count > 0)
			{
				var containerCommodity = Parent.CusContainers[0].JobContainer.JC_RH_NKContainerCommodityCode;

				foreach (BaseCusContainer container in Parent.CusContainers)
				{
					if (containerCommodity != container.JobContainer.JC_RH_NKContainerCommodityCode)
					{
						containerCommodity = ZString.Empty;
						break;
					}
				}

				if (!containerCommodity.IsEmpty)
				{
					result = containerCommodity;
				}
			}

			return result;
		}

		ZString GetUniqueCommodityFromShipmentPackLines()
		{
			var result = ZString.Empty;

			if (Parent.Shipment != null && Parent.Shipment.OuterPackLines.Count > 0)
			{
				var shipmentCommodity = Parent.Shipment.OuterPackLines[0].JL_RH_NKCommodityCode;

				foreach (PackLine packLine in Parent.Shipment.OuterPackLines)
				{
					if (shipmentCommodity != packLine.JL_RH_NKCommodityCode)
					{
						shipmentCommodity = ZString.Empty;
						break;
					}
				}

				result = shipmentCommodity;
			}

			return result;
		}

		void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			var commonContainersWithMassAndVolumeGetters = new List<ContainerAndMassAndVolumeHelper>();

			foreach (BaseCusContainer cusContainer in Parent.CusContainers)
			{
				commonContainersWithMassAndVolumeGetters.Add(
					new ContainerAndMassAndVolumeHelper(
						cusContainer.JobContainer,
						weight: cusContainer.GrossWeight,
						grossWeight: new Quantity(cusContainer.GrossWeight, cusContainer.GrossWeightUQ),
						volume: 0m));
			}

			FreightRatingHelper.SetContainers(measures, Parent.Shipment, commonContainersWithMassAndVolumeGetters, null);
		}

		public override OrgAddress WharfCTOAddress
		{
			get { return Parent.ContainerTerminalOperatorDocAddress.Address; }
		}

		protected virtual ZString FreightTransportMode
		{
			get { return Parent.GetFreightTransportMode(); }
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				var infos = new PaymentTermInfos();
				var incoTerm = Parent.IncoTerm;
				if (!string.IsNullOrWhiteSpace(incoTerm))
				{
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Cost, Parent.IncoTerm));
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, Parent.IncoTerm));
				}

				return infos;
			}
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return true;
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		#endregion

		#region IAutoRatingCustomsInfo Members

		ZString IAutoRatingCustomsInfo.MessageType
		{
			get { return Parent.JE_MessageType; }
		}

		ZString IAutoRatingCustomsInfo.MessageSubType
		{
			get { return Parent.JE_MessageSubType; }
		}

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get { return Parent.Entries; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get
			{
				var result = new InvoiceInfoCollection();

				foreach (BaseJobComInvoiceHeader invoice in Parent.Invoices)
				{
					result.AddNew(new Money(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency), invoice.JobComInvoiceLines.Count, invoice.Supplier);
				}

				return result;
			}
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
		{
			get
			{
				var result = new InvoiceInfoCollection();

				var invoiceLinesWithDistinctTariff = Parent.InvoiceLines.OfType<BaseJobComInvoiceLine>().Select(x => x.JI_Tariff).Distinct();
				result.AddNew(Money.Empty, invoiceLinesWithDistinctTariff.Count());
				return result;
			}
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
		{
			get
			{
				var result = new InvoiceInfoCollection();

				foreach (BaseJobComInvoiceHeader invoice in Parent.Invoices)
				{
					var tariffs = invoice.InvoiceLines.OfType<BaseJobComInvoiceLine>()
						.Select(x => x.JI_Tariff)
						.Where(c => !string.IsNullOrWhiteSpace(c))
						.ToArray();

					var invoiceLinesWithDistinctTariff = tariffs.Distinct();

					result.AddNew(Money.Empty, invoiceLinesWithDistinctTariff.Count(), null, tariffs.Length);
				}

				return result;
			}
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get
			{
				return 0;
			}
		}

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = new DeclarationRateLineConditionsSupporter(Parent)); }
		}

		RateLineConditionsSupporter conditionsSupporter;

		#endregion

		#region Contract Numbers

		public override IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell)
		{
			return new ContractNumberConfiguration(costOrSell);
		}

		class ContractNumberConfiguration : IContractNumberConfiguration
		{
			public ContractNumberConfiguration(CostSell costOrSell)
			{
				this.costOrSell = costOrSell;
			}

			readonly CostSell costOrSell;

			public bool ShouldAddContractNumberQueryFilter => true;

			public bool ShouldApplySpecificAdapterContractNumberFilter => costOrSell == CostSell.Revenue;

			public bool ShouldIgnoreJobClientContractNumbers => false;

			public bool ShouldIgnoreJobCarrierContractNumbers => false;

			public bool ShouldMatchJobBlankContractNumber => false;

			public bool ShouldUseCarrierContractDateFilter => false;
		}

		/// <summary>
		/// Returns the client contract number from the job header if it is not blank, or an empty list if blank or no job header.
		/// </summary>
		public override IEnumerable<ZString> ClientContractNumbers
		{
			get
			{
				var number = Parent.Job?.JH_ClientContractNumber ?? ZString.Empty;
				return !number.IsEmpty ? new[] { number } : Enumerable.Empty<ZString>();
			}
		}

		#endregion

		#region IJobDataUpdater

		bool IJobDataUpdater.CanUpdateDate => false;

		void IJobDataUpdater.UpdateRateCommodityCodeAndFMCTariffID(ZString newRateCommodityCode, ZString newFMCTariffID) { }

		void IJobDataUpdater.UpdateDetailedGoodsDescription(ZString newDescription, bool append) { }

		void IJobDataUpdater.UpdateServiceLevel(ZString newServiceLevel) { }

		void IJobDataUpdater.UpdateCarrier(OrgHeader newCarrier) { }

		bool IJobDataUpdater.UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateOrigin(ZString newOrigin) { }

		bool IJobDataUpdater.UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateDestination(ZString newDestination) { }

		bool IJobDataUpdater.UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates) { }

		bool IJobDataUpdater.UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdatePaymentTerms(ZString newPaymentTerms) { }

		void IJobDataUpdater.UpdateNamedAccount(ZString namedAccount) { }

		void IJobDataUpdater.UpdateCarrierQuoteNumber(ZString carrierQuoteNumber) { }

		void IJobDataUpdater.UpdateContainersCarrierQuoteNumber(ZGuid containerRefPK, ZString carrierQuoteNumber) { }

		void IJobDataUpdater.UpdateSpotBookingTerms(ZString termsAsText) { }

		public void UpdateAutoratingDate(ZDate autoratingDate, bool isCosting) { }

		CanUpdateCarrierContractNumberResult IJobDataUpdater.CanUpdateCarrierContractNumber(IEnumerable<string> newContractNumbers, IDialogService dialogService, bool isManualCostSelected)
		{
			return new CanUpdateCarrierContractNumberResult
			{
				Token = new UpdateCarrierContractNumberToken(newContractNumbers)
			};
		}

		bool IJobDataUpdater.IsMultipleCarrierContractNumberSupported => true;

		DataUpdateResult IJobDataUpdater.UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token) => DataUpdateResult.NoAction;

		public DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers)
		{
			var jobHeader = Parent.Job;
			if (jobHeader != null)
			{
				var nonBlankContractNumbers = newNumbers.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
				if (nonBlankContractNumbers.Length == 1)
				{
					var newContractNumber = nonBlankContractNumbers.First();
					if (!string.Equals(jobHeader.JH_ClientContractNumber, newContractNumber, StringComparison.OrdinalIgnoreCase))
					{
						jobHeader.JH_ClientContractNumber = newContractNumber;
						return DataUpdateResult.Updated;
					}
				}
			}

			return DataUpdateResult.NoAction;
		}

		void IJobDataUpdater.UpdateTransports(IEnumerable<ITransport> transports) { }

		void IJobDataUpdater.UpdateChargeable(ZDecimal newChargeable) { }

		void IJobDataUpdater.SendBookingInformationToCarrier() { }

		bool IJobDataUpdater.IsMultipleClientContractNumberSupported => false;

		#endregion
	}
}
