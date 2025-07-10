using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.AutoratingViaPortHelper;
using ContainerPenalty = Enterprise.Freight.Business.ContainerPenalty;

namespace Enterprise.Freight.QuotedBookings.Business
{
	class QuotedBookingRatingAdapter : RatingAdapter<QuotedBooking>,
										IAutoRatingGlbCompany,
										IAutoRatingCustomsInfo,
										IAutoRatingCompanyTariffLevelProvider,
										IAutoRatingFreightConditionsSupportable,
										ISpotRate,
										IManualRateSelectionSupporter,
										IJobDataUpdater
	{
		public QuotedBookingRatingAdapter(QuotedBooking parent)
			: base(parent)
		{
		}

		#region RatingAdapter

		public override bool SkipFreightCharge
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					return false;
				}

				var line = Parent.Factory.Load<RatingContractAllocationLine>(Parent.AllocationLinePK);

				return line?.RCA_AllowFreightSpotRate ?? false;
			}
		}

		public override ZString FMCTariffID
		{
			get
			{
				return Parent.FMCTariffID;
			}
		}

		public override IEnumerable<RefCommodityCode> OverriddenCommodity
		{
			get
			{
				if (!string.IsNullOrEmpty(Parent.Commodity))
				{
					var commodity = Parent.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, Parent.Commodity);
					if (commodity != null)
					{
						return new[] { commodity };
					}
				}
				return Enumerable.Empty<RefCommodityCode>();
			}
		}

		public override IEnumerable<ZString> ClientContractNumbers
		{
			get
			{
				var booking = Parent.Booking;
				if (booking == null)
				{
					return new List<ZString>();
				}

				if (!(booking.Numbers is Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection numbers))
				{
					return new List<ZString>();
				}

				// Get all numbers regardless of countries of issue
				return numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC);
			}
		}

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

			public bool ShouldApplySpecificAdapterContractNumberFilter => costOrSell == CostSell.Cost;

			public bool ShouldIgnoreJobClientContractNumbers => false;

			public bool ShouldIgnoreJobCarrierContractNumbers => false;

			public bool ShouldMatchJobBlankContractNumber => false;

			public bool ShouldUseCarrierContractDateFilter => false;
		}

		public override AdapterType AdapterType
		{
			get
			{
				switch (Parent.ObjectState)
				{
					case QuotedBookingState.BookingOnly:
						return AdapterType.Booking;

					case QuotedBookingState.QuoteOnly:
						return AdapterType.OneOffQuote;

					default:
						return AdapterType.BookingWithQuote;
				}
			}
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new QuotedBookingJobDatesProvider(Parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.AddRange(Env.Registry.Rating.FreightRatedCodes);

				if (Parent.QuoteNumberOfEntries > 0 || Parent.QuoteNumberOfEntryLines > 0)
				{
					result.AddRange(Env.Registry.Rating.BrokerageRatedCodes);
					result.AddRange(Env.Registry.Rating.OriginBrokerageRatedCodes);
				}

				return result;
			}
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return ((IJobInvoicingPlugIn)Parent).InvoicingSupporter.ConsumerType; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override JobServicesCollection JobServices
		{
			get { return Parent.Booking != null ? BookingRatingAdapter.JobServices : new JobServicesCollection(); }
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		public override ILocation Destination
		{
			get { return Parent.DestinationUNLOCO; }
		}

		public override ILocation Origin
		{
			get { return Parent.OriginUNLOCO; }
		}

		public override ILocation GetVia(CostSell costOrSell)
		{
			ILocation specifiedVia = Parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.Via);
			if (specifiedVia != null)
			{
				return specifiedVia;
			}

			var viaCode = RatingDataRegistry.Instance.AutoratingViaPort.GetViaForQuotedBooking(
				transportMode: Parent.TransportMode,
				direction:
					Parent.IsExport() ? DirectionOption.Export.Code :
					Parent.IsImport() ? DirectionOption.Import.Code :
					DirectionOption.All.Code,
				origin: Parent.Origin,
				destination: Parent.Destination,
				voyageLoad: Parent.LoadPort,
				voyageDischarge: Parent.DischargePort);

			return LocationHelper.GetCachedLocationFromString(viaCode, Parent.Factory);
		}

		public override ILocation GetFirstLoad(CostSell costOrSell) =>
			Parent.LoadPortUNLOCO;

		public override ILocation GetLastDischarge(CostSell costOrSell) =>
			Parent.DischargePortUNLOCO;

		public override ILocation GetFirstRouteSetLoad(CostSell costOrSell) =>
			Parent.ScheduleChooser?.Sailing?.PortOfLoading;

		public override ILocation GetLastRouteSetDischarge(CostSell costOrSell) =>
			Parent.ScheduleChooser?.Sailing?.PortOfDischarge;

		public override OrgHeader Carrier
		{
			get { return Parent.Carrier; }
		}

		public override OrgHeader ImportBroker
		{
			get { return Parent.ImportBroker; }
		}

		public override OrgHeader ExportBroker
		{
			get { return Parent.ExportBroker; }
		}

		public override ZString DeliveryCartageEquipment
		{
			get { return Parent.DeliveryEquipment; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[RatingDebtorOrgTypes.CNE] = Parent.Consignee;
				result[RatingDebtorOrgTypes.CNR] = Parent.Consignor;
				result[RatingDebtorOrgTypes.CCUS] = Parent.ControllingCustomer;

				return result;
			}
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.GetFromBooking ? Parent.Booking.ConsigneeDeliveryAddress : Parent.ConsigneeDocumentaryAddress; }
		}

		public override ZString PickupCartageEquipment
		{
			get { return Parent.PickupEquipment; }
		}

		public override IDocAddress PickupAddress
		{
			get { return Parent.GetFromBooking ? Parent.Booking.ConsignorPickupAddress : Parent.ConsignorDocumentaryAddress; }
		}

		public override Creditors Creditors
		{
			get { return Parent.GetFromBooking ? BookingRatingAdapter.Creditors : Parent.Quote.CurrentOneOffQuote.RatingAdapter.Creditors; }
		}

		IAutoRating RatingAdapter
		{
			get { return Parent.GetFromBooking ? Parent.Booking.RatingAdapter : Parent.Quote.CurrentOneOffQuote.RatingAdapter; }
		}

		public override FreightMode FreightMode
		{
			get { return RatingAdapter.FreightMode; }
		}

		public override ZString ContainerMode
		{
			get { return RatingAdapter.ContainerMode; }
		}

		public override ZString HousebillReleaseType
		{
			get { return Parent.GetFromBooking ? BookingRatingAdapter.HousebillReleaseType.ToString() : ""; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				if (overriddenPaymentTerm != null)
				{
					return overriddenPaymentTerm;
				}

				return RatingAdapter.PaymentTerm;
			}
			set
			{
				overriddenPaymentTerm = value;
			}
		}

		PaymentTermInfos overriddenPaymentTerm;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return RatingAdapter.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell);
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				if (Parent.GetFromBooking)
				{
					return BookingRatingAdapter.RateableMeasures;
				}
				else
				{
					return Parent.Quote.CurrentOneOffQuote.RatingAdapter.RateableMeasures;
				}
			}
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get { return GetStatusInformation(); }
		}

		AutoRatingStatusInfo GetStatusInformation()
		{
			var volumeAndWeightUnitStatusInformation = GetVolumeAndWeightUnitStatusInformation();
			if (volumeAndWeightUnitStatusInformation != null)
			{
				return volumeAndWeightUnitStatusInformation;
			}

			return base.StatusInformation;
		}

		AutoRatingStatusInfo GetVolumeAndWeightUnitStatusInformation()
		{
			if (!Constants.Weight.ContainsCode(Parent.WeightUnit))
			{
				return new AutoRatingStatusInfo(false, Res.GetString("51595014-0a7b-42ba-9fa4-e97ef2dd1940", "Invalid unit of weight: '{0}'.", Parent.WeightUnit));
			}

			if (!Constants.Volume.ContainsCode(Parent.VolumeUnit))
			{
				return new AutoRatingStatusInfo(false, Res.GetString("8b312359-9eb3-402a-a0a1-b9de0efec867", "Invalid unit of volume: '{0}'.", Parent.VolumeUnit));
			}

			return null;
		}

		public override MoneyType MonetaryValues
		{
			get { return RatingAdapter.MonetaryValues; }
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				if (serviceLevel == null)
				{
					var serviceLevelInfos = new List<ServiceLevelInfo>(RatingAdapter.ServiceLevel.ServiceLevelData);
					if (Parent.GetFromBooking)
					{
						foreach (var item in serviceLevelInfos.Where(i => i.ServiceLevelType == ServiceLevelType.Carrier).ToList())
						{
							serviceLevelInfos.Remove(item);
						}
						serviceLevelInfos.Add(new ServiceLevelInfo(Parent.Booking.JS_PL_NKCarrierServiceLevel, ServiceLevelType.Carrier));
					}

					serviceLevel = new ServiceLevelRatingInformation(serviceLevelInfos.ToArray());
				}

				return serviceLevel;
			}
		}

		ServiceLevelRatingInformation serviceLevel;

		public override OrgAddress WharfCTOAddress
		{
			get { return RatingAdapter.WharfCTOAddress; }
		}

		public override IEnumerable<OrgHeader> PossibleServiceProviders
		{
			get
			{
				var serviceProviders = new HashSet<OrgHeader>()
				{
					Parent.Carrier
				};
				var possibles = PossibleCarriers;
				if (possibles != null)
				{
					foreach (var possible in possibles)
					{
						serviceProviders.Add(possible);
					}
				}
				serviceProviders.Remove(null);
				return serviceProviders;
			}
		}

		public override IEnumerable<OrgHeader> PossibleCarriers
			=> RatingAdapter.PossibleCarriers;

		public override IEnumerable<ZString> CarrierContractNumbers
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					// return the numbers directly from OOQ, which are still CON refs at the moment.
					return RatingAdapter.CarrierContractNumbers;
				}

				if (!Parent.GetFromBooking)
				{
					return Enumerable.Empty<ZString>();
				}

				var contractNumbers = new HashSet<ZString>
				{
					// Dedicated CCA field on booking
					Parent.CarrierContractNumber,
					// According to the specifications, blank number rates should not be filtered out preliminarily.
					// To ensure that rates are retained, add a blank number.
					ZString.Empty,
				};
				return contractNumbers.ToList();
			}
		}

		public override ZString NamedAccount
		{
			get
			{
				if (Parent.GetFromBooking)
				{
					return Parent.Booking.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)?.CE_EntryNum ?? ZString.Empty;
				}

				if (Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.QuoteOnly)
				{
					return RatingAdapter.NamedAccount;
				}

				return ZString.Empty;
			}
		}

		/// <summary>
		/// One valid client usage is that even OneOffQuote cannot have service, we should still accept rate lines with service charge codes.
		/// OOQ will have a charge with either zero amounts or MIN price depending on calculator setup.
		/// </summary>
		public override bool ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(AccChargeCode chargeCode)
		{
			return chargeCode == null || chargeCode.AC_ChargeSubGroup.IsEmpty;
		}

		public override ZString HBLDeliveryMode => Parent.ContainerPackModeOverride;

		#endregion

		#region BookingRatingAdapter

		BookingRatingAdapter BookingRatingAdapter
		{
			get { return bookingRatingAdapter ?? (bookingRatingAdapter = new BookingRatingAdapter(Parent)); }
		}
		BookingRatingAdapter bookingRatingAdapter;

		#endregion

		#region IAutoRatingGlbCompany Members

		GlbCompany IAutoRatingGlbCompany.Company
		{
			get { return (Parent.GetFromBooking) ? GlbCompany.CurrentCompany : ((IAutoRatingGlbCompany)Parent.Quote.CurrentOneOffQuote.RatingAdapter).Company; }
		}

		#endregion

		#region IAutoRatingCustomsInfo Members

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get { return (Parent.Quote == null) ? new EntryInfoCollection() : ((IAutoRatingCustomsInfo)Parent.Quote.CurrentOneOffQuote.RatingAdapter).Entries; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get { return (Parent.Quote == null) ? new InvoiceInfoCollection() : ((IAutoRatingCustomsInfo)Parent.Quote.CurrentOneOffQuote.RatingAdapter).Invoices; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
		{
			get { return (Parent.Quote == null) ? new InvoiceInfoCollection() : ((IAutoRatingCustomsInfo)Parent.Quote.CurrentOneOffQuote.RatingAdapter).TariffsPerInvoice; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
		{
			get { return (Parent.Quote == null) ? new InvoiceInfoCollection() : ((IAutoRatingCustomsInfo)Parent.Quote.CurrentOneOffQuote.RatingAdapter).TariffsPerShipment; }
		}

		ZString IAutoRatingCustomsInfo.MessageSubType
		{
			get { return (Parent.Quote == null) ? ZString.Empty : ((IAutoRatingCustomsInfo)Parent.Quote.CurrentOneOffQuote.RatingAdapter).MessageSubType; }
		}

		ZString IAutoRatingCustomsInfo.MessageType
		{
			get { return (Parent.Quote == null) ? ZString.Empty : ((IAutoRatingCustomsInfo)Parent.Quote.CurrentOneOffQuote.RatingAdapter).MessageType; }
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}

		#endregion

		#region IAutoRatingCompanyTariffLevelProvider Members

		int IAutoRatingCompanyTariffLevelProvider.TariffLevel
		{
			get
			{
				if (Parent.CompanyTariffLevelInfo.ReadOnly)
				{
					var doNotOverrideCompanyTariffLevel = 0;
					return doNotOverrideCompanyTariffLevel;
				}

				ZByte.TryParse(Parent.CompanyTariffLevel, out var companyTariffLevelOverride);
				return Convert.ToInt32(companyTariffLevelOverride);
			}
		}

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return ((IAutoRatingFreightConditionsSupportable)Parent).ConditionsSupporter; }
		}

		#endregion

		#region ISpotRate

		SpotRateInfo ISpotRate.SellSpotRateInfo
		{
			get
			{
				if (!Parent.GetFromBooking)
				{
					return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate);
				}

				var iSpotRate = BookingRatingAdapter as ISpotRate;

				if (iSpotRate == null)
				{
					return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate);
				}

				return iSpotRate.SellSpotRateInfo;
			}
		}

		SpotRateInfo ISpotRate.CostSpotRateInfo
		{
			get
			{
				if (!Parent.GetFromBooking)
				{
					return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
				}

				var iSpotRate = BookingRatingAdapter as ISpotRate;

				if (iSpotRate == null)
				{
					return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
				}

				return iSpotRate.CostSpotRateInfo;
			}
		}

		#endregion

		#region IImportExport Members

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		#endregion

		#region IManualRateSelectionSupporter

		bool IManualRateSelectionSupporter.SupportsManualRateSelection
		{
			get
			{
				if
					(
						Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.QuoteOnly
						|| Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.UnacceptedBookingWithQuote
						|| Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.AcceptedBookingWithQuote
						|| Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.BookingOnly
					)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		PaymentTermInfos IManualRateSelectionSupporter.DefaultFilterValueForPaymentTerm => PaymentTerm;

		ILocation IManualRateSelectionSupporter.DefaultFilterValueForOrigin
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					return Origin;
				}

				return Parent.LoadPortUNLOCO;
			}
		}

		ILocation IManualRateSelectionSupporter.DefaultFilterValueForDestination
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					return Destination;
				}

				return Parent.DischargePortUNLOCO;
			}
		}

		bool IManualRateSelectionSupporter.ContinueAutoratingWithoutRateSelector
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					return false;
				}

				return true;
			}
		}

		string IManualRateSelectionSupporter.OriginMissingMessage
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					return null;
				}

				return ResString.GetMultilingualString("89cb154a-1e51-42a8-8bf9-8d921c0b61ef", "Load Port is mandatory for Rates Selector to be displayed. Autorating process will continue without Rates Selector");
			}
		}

		string IManualRateSelectionSupporter.DestinationMissingMessage
		{
			get
			{
				if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
				{
					return null;
				}

				return ResString.GetMultilingualString("d6e253ec-58c4-40e9-b21d-33576b63a961", "Discharge Port is mandatory for Rates Selector to be displayed. Autorating process will continue without Rates Selector");
			}
		}

		#endregion

		#region IJobDataUpdater

		public bool CanUpdateDate => false;

		public void UpdateRateCommodityCodeAndFMCTariffID(ZString newRateCommodityCode, ZString newFMCTariffID)
		{
			Parent.Commodity = newRateCommodityCode;
			Parent.FMCTariffID = newFMCTariffID;
		}

		public void UpdateDetailedGoodsDescription(ZString newDescription, bool append)
		{
			if (append && !Parent.DetailedGoodsDescriptionNoteText.IsEmpty)
			{
				Parent.DetailedGoodsDescriptionNoteText += System.Environment.NewLine + newDescription;
			}
			else
			{
				Parent.DetailedGoodsDescriptionNoteText = newDescription;
			}
		}

		public void UpdateServiceLevel(ZString newServiceLevel)
		{
			Parent.CarrierServiceLevel = newServiceLevel;
		}

		public void UpdateCarrier(OrgHeader newCarrier)
		{
			Parent.OH_Carrier = newCarrier.PK;
		}

		public bool UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
		{
			confirmationMessage = Parent.OH_Carrier.IsEmpty
				? null
				: ResString.GetMultilingualString("2c37c360-80a0-4bd3-9c01-d7d9c9ecef8e",
					@"During this operation, Service Provider '{0}' of the chosen rates will be populated as the Carrier of your {1}.

You may need to manually adjust/remove Charges if they are no longer valid.

Do you wish to continue?",
					newServiceProvider,
					Parent.HumanReadableNameWithoutID);

			return true;
		}

		public void UpdateOrigin(ZString newOrigin)
		{
			if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
			{
				Parent.Origin = newOrigin;
			}
			else if (Parent.LoadPort != newOrigin)
			{
				Parent.LoadPort = newOrigin;
			}
		}

		public bool UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
		{
			var originType = ResString.GetMultilingualString("3ac2c9b0-f105-4755-9531-3bbfa9747a51", "Load");
			var origin = Parent.LoadPort;
			if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
			{
				originType = ResString.GetMultilingualString("db7a8e4d-daf2-4126-b459-ad8fd42a6b3a", "Origin");
				origin = Parent.Origin;
			}

			if (newOrigin != origin)
			{
				confirmationMessage =
					ResString.GetMultilingualString("186ed58c-88c4-4183-8ef0-7f86fd6fe4b4",
						"During this operation, would you like to update the {2} of the {1} to be the Origin '{0}' of the chosen rates?",
						newOrigin,
						Parent.HumanReadableNameWithoutID,
						originType);
				return true;
			}

			confirmationMessage = null;
			return false;
		}

		public void UpdateDestination(ZString newDestination)
		{
			if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
			{
				Parent.Destination = newDestination;
			}
			else if (Parent.DischargePort != newDestination)
			{
				Parent.DischargePort = newDestination;
			}
		}

		public virtual bool UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
		{
			var destinationType = ResString.GetMultilingualString("11bd8beb-0b22-4d9c-99b5-df287c5a1a99", "Discharge");
			var destination = Parent.DischargePort;

			if (Parent.ObjectState == QuotedBookingState.QuoteOnly)
			{
				destinationType = ResString.GetMultilingualString("8d27f7f3-6477-462e-8f91-26d174ee41f0", "Destination");
				destination = Parent.Destination;
			}

			if (newDestination != destination)
			{
				confirmationMessage =
					ResString.GetMultilingualString("e61df905-8e27-4c51-a7bb-68c6b9401240",
						"During this operation, would you like to update the {2} of the {1} to be the Destination '{0}' of the chosen rates?",
						newDestination,
						Parent.HumanReadableNameWithoutID,
						destinationType);
				return true;
			}

			confirmationMessage = null;
			return false;
		}

		public void UpdatePaymentTerms(ZString newPaymentTerms)
		{
		}

		public void UpdateNamedAccount(ZString namedAccount)
		{
			if (Parent.GetFromBooking)
			{
				UpdateNamedAccount(Parent.Booking.Numbers, namedAccount);
			}

			if (Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.QuoteOnly)
			{
				UpdateNamedAccount(Parent.Quote.CurrentOneOffQuote.Numbers, namedAccount);
			}
		}

		void UpdateNamedAccount(CusEntryNumAdditionalReferenceCollection numbers, ZString namedAccount)
		{
			var existingNamedAccount = numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount);
			if (existingNamedAccount != null)
			{
				existingNamedAccount.CE_EntryNum = new ZString(namedAccount).SubstringSafe(0, CusEntryNumSchema.CE_EntryNum.MaxLength);
			}
			else
			{
				numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, namedAccount);
			}
		}

		public bool UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			confirmationMessage = string.Empty;
			return false;
		}

		public void UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExisting = true)
		{
			if (containerPenalties?.Any() == true)
			{
				var noteBuilder = new ZStringBuilder();
				var organisation = Parent.Consignor?.NameAndCode;
				var dischargePort = Parent.Origin;
				var description = PredefinedNoteTypes.Instance.SpotBookingPenaltiesFees.Description;

				var note = Parent.Notes.FindByDescription(description).SingleOrDefault(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany)
					?? Parent.Notes.AddNew();

				foreach (var group in containerPenalties.GroupBy(p => p.RefContainerCode))
				{
					foreach (var penalty in group)
					{
						noteBuilder.AppendLine(Res.GetString("6e05a69f-46e0-49e1-9008-b0f78908fc1d", "From {0} with Creditor Type {1} at {2} for {3} ", organisation, penalty.CPY_CreditorType, dischargePort, penalty.RefContainerCode));
						noteBuilder.AppendLine(Res.GetString("ffbf0f5f-fb68-4dab-acf0-7c00d5b25638", "Detention {0} - Free Time - {1} days with ", penalty.CPY_PenaltyType, ContainerPenalty.ConvertDateTimeToDays(penalty.CPY_FreeTime)));
						noteBuilder.AppendLine(Res.GetString("925f261a-7596-490e-b50c-529e4275a3ec", "break up at {0} {1} per Day ", penalty.CPY_PerUnitCost.ToString(), penalty.CPY_RX_NKCurrency));
						noteBuilder.AppendLine();
					}
				}

				note.ST_Description = description; //Method parameter deleteExisting is irrelevant here as we will always replace the previous note item with the new autorated note.
				note.ST_NoteText = noteBuilder.ToString();
				note.Validation.ValidateAll();
			}
		}

		public void UpdateTransports(IEnumerable<ITransport> transports)
		{
			if (transports?.Any() == false)
			{
				return;
			}

			var noteBuilder = new ZStringBuilder();
			var description = PredefinedNoteTypes.Instance.SpotBookingRouting.Description;
			var carrier = Parent.Carrier.OH_FullName;
			var note = Parent.Notes.FindByDescription(description).FirstOrDefault(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany)
				?? Parent.Notes.AddNew();

			foreach (var item in transports)
			{
				if (noteBuilder.IsEmpty)
				{
					noteBuilder.AppendLine(Res.GetString("57b07daf-2034-4f72-906e-d2067052f12d", "Schedule Information for the {0}-{1} - Leg {2}", item.JW_RL_NKLoadPort, item.JW_RL_NKDiscPort, item.JW_LegOrder.ToString()));
				}
				else
				{
					noteBuilder.AppendLine(Res.GetString("34c5b499-759d-40b4-b7c2-6f47b9abbb75", "Schedule Information for the Leg {0}", item.JW_LegOrder.ToString()));
				}

				noteBuilder.AppendLine(Res.GetString("41717ffe-24ea-4e9d-a6b4-04b10ef7e3fa", "Status : {0}", item.JW_Status));
				noteBuilder.AppendLine(Res.GetString("acfbb5b3-a62e-409a-b8a0-1fa5cabf90c3", "Load Port : {0}", item.JW_RL_NKLoadPort));
				noteBuilder.AppendLine(Res.GetString("6920d0b7-6567-45d3-8291-cf8dd5e46f56", "ETD : {0}", item.JW_ETD.ToString()));
				noteBuilder.AppendLine(Res.GetString("f3e9f26d-8be4-4f14-857a-b9f5f31b1aa0", "Discharge Port : {0}", item.JW_RL_NKDiscPort));
				noteBuilder.AppendLine(Res.GetString("332a7e89-ac30-4407-b859-06864044ba2e", "ETA : {0}", item.JW_ETA.ToString()));
				noteBuilder.AppendLine(Res.GetString("4a4a2b2b-cb00-4632-bda7-431b01961644", "Vessel Name : {0}", item.JW_Vessel));
				noteBuilder.AppendLine(Res.GetString("fcc5e1e2-e38a-4f93-9acf-03e1e91ecf1a", "Voyage Number : {0}", item.JW_VoyageFlight));
				noteBuilder.AppendLine(Res.GetString("2502e334-6b87-4ac3-9ac5-93b5dfcc6a75", "Carrier : {0}", carrier));
				noteBuilder.AppendLine();

				if (item.JW_LegNotes != ZString.Empty)
				{
					noteBuilder.AppendLine(Res.GetString("f966205e-7729-44b4-a8e6-ab404cdf72b4", "Documentation Deadlines for the Leg - {0}", item.JW_LegOrder.ToString()));
					noteBuilder.Append(Res.GetString("903c0740-8a26-4e2e-b547-ca2b373319ac", "{0}", item.JW_LegNotes));
				}

				noteBuilder.AppendLine(Res.GetString("e398f7cd-8ef3-41ec-8413-eb2f7d964cbd", "Shipping Instructions Deadline \r\r\r\r\r {0}", item.JW_DocumentaryCutOff));
				noteBuilder.AppendLine(Res.GetString("ff2d9f25-f2c4-4d88-8cea-ff539b77be11", "Commercial Verified Gross Mass Deadline \r\r\r\r\r {0}", item.JW_VGMCutOff));
				noteBuilder.AppendLine(Res.GetString("462d54bd-a82c-4360-9a48-546bec4b03f8", "Commercial Cargo Cutoff \r\r\r\r\r {0}", item.JW_TerminalCutOff));
				noteBuilder.AppendLine();
			}

			note.ST_Description = description;
			note.ST_NoteText = noteBuilder.ToString();
			note.Validation.ValidateAll();
		}

		public void UpdateSpotBookingTerms(ZString termsAsText)
		{
			if (!string.IsNullOrEmpty(termsAsText))
			{
				var description = PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description;
				var note = Parent.Notes.FindByDescription(description).FirstOrDefault(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany)
					?? Parent.Notes.AddNew();

				note.ST_Description = description;
				note.ST_NoteText = termsAsText;
				note.Validation.ValidateAll();
			}
		}

		public void UpdateCarrierQuoteNumber(ZString carrierQuoteNumber)
		{
			if (Parent.GetFromBooking)
			{
				Parent.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber, carrierQuoteNumber);
			}
			else if (Parent.ObjectState == Integration.QuotedBooking.QuotedBookingState.QuoteOnly)
			{
				Parent.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber, carrierQuoteNumber);
			}
		}

		public void UpdateContainersCarrierQuoteNumber(ZGuid containerRefPK, ZString carrierQuoteNumber) { }

		public void UpdateAutoratingDate(ZDate autoratingDate, bool isCosting) { }

		public CanUpdateCarrierContractNumberResult CanUpdateCarrierContractNumber(IEnumerable<string> newContractNumbers, IDialogService dialogService = null, bool isManualCostSelected = false)
		{
			var newNumbers = newContractNumbers.Distinct().ToList();

			if (newNumbers.Count <= 0 || !ZArchitecture.Environment.Globals.IsUserInteractive || Parent.ObjectState == QuotedBookingState.QuoteOnly)
			{
				return new CanUpdateCarrierContractNumberResult(newNumbers);
			}

			var oldNumber = Parent.CarrierContractNumber;

			// Retain the old number when:
			// - New numbers has only 1 number, and it equals to old number
			// - New numbers has 2 numbers, 1 of them is blank and the other one equals to the old number.
			var keepOldNumber = newNumbers.Count == 1 && oldNumber.EqualsIgnoringCase(newNumbers[0]);
			keepOldNumber = keepOldNumber ||
			(
				newNumbers.Count == 2 &&
				!oldNumber.IsEmpty &&
				newNumbers.Contains(string.Empty) &&
				newNumbers.Contains(oldNumber.ToString(), StringComparer.InvariantCultureIgnoreCase)
			);
			if (keepOldNumber)
			{
				return new CanUpdateCarrierContractNumberResult
				{
					CanUpdate = true,
					Token = new UpdateCarrierContractNumberToken(newNumbers)
					{
						SelectionResult = new SingleCarrierContractNumberSelectionResult(oldNumber)
					}
				};
			}

			// There is only 1 new number. Just update job blank number with that.
			if (newNumbers.Count == 1 && oldNumber.IsEmpty)
			{
				return new CanUpdateCarrierContractNumberResult
				{
					CanUpdate = true,
					Token = new UpdateCarrierContractNumberToken(newNumbers)
					{
						SelectionResult = new SingleCarrierContractNumberSelectionResult(newNumbers[0])
					}
				};
			}

			// Can't decide which new number to update. Ask user for one (if in interactive mode).
			var selectionResult = SelectSingleCarrierContractNumber(newNumbers, dialogService);

			// Return early if cancel choosing or keep the number.
			switch (selectionResult.Result)
			{
				case ContractNumberSelectionResult.Cancelled:
					return new CanUpdateCarrierContractNumberResult { CanUpdate = false };

				case ContractNumberSelectionResult.Default:
				case ContractNumberSelectionResult.KeepJobCarrierContractNumber:
					return new CanUpdateCarrierContractNumberResult
					{
						CanUpdate = true,
						Token = new UpdateCarrierContractNumberToken(newNumbers)
						{
							SelectionResult = selectionResult
						}
					};
			}

			// A number is selected from the popup
			string confirmationMessage = null;
			var selectedNumber = selectionResult.Number;

			if (!oldNumber.IsEmpty && !oldNumber.EqualsIgnoringCase(selectedNumber))
			{
				confirmationMessage = Res.GetString("5FD83AEA-52AB-413A-8F77-7CD7CEFE32C9", @"During the operation, Contract Number '{0}' of the chosen rates will be populated as the Carrier Contract Number.
Do you wish to continue?", selectedNumber);
			}

			return new CanUpdateCarrierContractNumberResult
			{
				CanUpdate = true,
				ConfirmationMessageForOverridingJobContractNumber = confirmationMessage,
				Token = new UpdateCarrierContractNumberToken(newNumbers)
				{
					SelectionResult = selectionResult
				}
			};
		}

		/// <summary>
		/// Select a single number among new numbers.
		/// If it's interactive and there is a dialog service, user is prompted for selecting a number or an option to not populate.
		/// Cancelling the dialog should also cancel autorating.
		/// If it's non-interactive or there is no dialog to help, return default result with null selected number.
		/// </summary>
		static SingleCarrierContractNumberSelectionResult SelectSingleCarrierContractNumber(IEnumerable<string> contractNumbers, IDialogService dialogService)
		{
			return dialogService?.SelectSingleCarrierContractNumber(contractNumbers.Distinct()) ?? default;
		}

		public bool IsMultipleCarrierContractNumberSupported => false;

		DataUpdateResult UpdateCarrierContractNumberForQuoteOnly(UpdateCarrierContractNumberToken token)
		{
			var numbersCollection = Parent.Quote?.CurrentOneOffQuote?.Numbers;
			if (numbersCollection == null)
			{
				return DataUpdateResult.NoAction;
			}

			var existingNumbers = numbersCollection
				.Cast<CusEntryNumber>()
				.Where(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
				.ToArray();
			foreach (var cusEntryNumber in existingNumbers)
			{
				numbersCollection.RemoveAndDelete(cusEntryNumber);
			}

			foreach (var newNumber in token.NewContractNumbers)
			{
				var truncatedNumber = newNumber.Length > CusEntryNumSchema.CE_EntryNum.MaxLength
					? newNumber.Substring(0, CusEntryNumSchema.CE_EntryNum.MaxLength)
					: newNumber;
				numbersCollection.AddOrSkipContractNumber(truncatedNumber);
			}

			return DataUpdateResult.Updated;
		}

		DataUpdateResult UpdateCarrierContractNumberForQuotedBookingOrQuickBooking(UpdateCarrierContractNumberToken token)
		{
			if (Parent.Booking == null)
			{
				return DataUpdateResult.NoAction;
			}

			var numberToUpdate = (string)null;
			var selectionResult = token.SelectionResult;

			switch (selectionResult.Result)
			{
				case ContractNumberSelectionResult.Cancelled:
					return DataUpdateResult.Cancelled;

				case ContractNumberSelectionResult.KeepJobCarrierContractNumber:
					return DataUpdateResult.NoAction;

				case ContractNumberSelectionResult.NumberSelected:
					numberToUpdate = selectionResult.Number;
					break;

				// non-interactive mode in which the result is returned with default values
				case ContractNumberSelectionResult.Default:
				{
					var distinctNumbers = token.NewContractNumbers.Distinct().ToArray();

					if (distinctNumbers.Length == 1)
					{
						numberToUpdate = distinctNumbers[0];
					}
					break;
				}
			}

			if (numberToUpdate == null || Parent.CarrierContractNumber == numberToUpdate)
			{
				return DataUpdateResult.NoAction;
			}

			if (numberToUpdate.Length <= CusEntryNumSchema.CE_EntryNum.MaxLength)
			{
				Parent.CarrierContractNumber = numberToUpdate;
				return DataUpdateResult.Updated;
			}

			// CusEntryNumSchema.CE_EntryNum.MaxLength == 35
			// JobShipmentSchema.JS_CarrierContractNumber.MaxLength == 50
			//
			// The 2 places are in sync. Change JS_CarrierContractNumber then CON ref also changes, and vice versa.
			// Except when the number length is over 35. In that case, CON ref does not change when changing JS_CarrierContractNumber.
			//
			// We will hack it for the long number case: set the CON ref first, then set the CCA field. Assume that we always have the max lengths as declared.

			var tempNumber = numberToUpdate.Substring(0, CusEntryNumSchema.CE_EntryNum.MaxLength);

			// Remove existing CON REFs, and add the new number.
			var existingNumbers = Parent.Booking.Numbers
				.Cast<CusEntryNumber>()
				.Where(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
				.ToArray();
			foreach (var cusEntryNumber in existingNumbers)
			{
				Parent.Booking.Numbers.RemoveAndDelete(cusEntryNumber);
			}
			Parent.Booking.Numbers.AddOrSkipContractNumber(tempNumber);

			Parent.CarrierContractNumber = numberToUpdate.Length > JobShipmentSchema.JS_CarrierContractNumber.MaxLength
				? numberToUpdate.Substring(0, JobShipmentSchema.JS_CarrierContractNumber.MaxLength)
				: numberToUpdate;

			return DataUpdateResult.Updated;
		}

		public DataUpdateResult UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token)
		{
			if (!token.NewContractNumbers.Any())
			{
				return DataUpdateResult.NoAction;
			}

			return Parent.ObjectState == QuotedBookingState.QuoteOnly
				? UpdateCarrierContractNumberForQuoteOnly(token)
				: UpdateCarrierContractNumberForQuotedBookingOrQuickBooking(token);
		}

		public bool IsMultipleClientContractNumberSupported => true;
		public DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers) => DataUpdateResult.NoAction;
		public void UpdateChargeable(ZDecimal newChargeable) { }

		public void SendBookingInformationToCarrier() { }

		#endregion
	}
}
