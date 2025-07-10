using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolRatingAdapter : ConsolRatingAdapter<ForwardingConsol>
		, IAutoRatingNamedAccountPlugin
		, IAutoRatingStandardFreightCost
		, IGateway
		, IJobDataUpdater
		, IManualRateSelectionSupporter
		, IAutoRatingSpotChargeInfo
	{
		public ForwardingConsolRatingAdapter(IRatingRoute<IRoutingSupport> ratingRoute, bool dontAutorateServices = false) : base(ratingRoute)
		{
			this.dontAutorateServices = dontAutorateServices;

			if (ratingRoute is RouteSetRatingRoute routeSet)
			{
				RouteSet = routeSet;
			}
			else if (ratingRoute is ConsolRatingRoute consolRouteSet)
			{
				ConsolRouteSet = consolRouteSet;
			}
		}

		readonly bool dontAutorateServices;

		public RouteSetRatingRoute RouteSet { get; private set; }
		public ConsolRatingRoute ConsolRouteSet { get; private set; }

		public override JobServicesCollection JobServices
		{
			get
			{
				if (dontAutorateServices)
				{
					var emptyServicesCollection = new JobServicesCollection();
					foreach (var service in base.JobServices)
					{
						emptyServicesCollection.Add(JobServiceInfo.Empty(service.ChargeCodeGroup, service.ServiceCode));
					}

					return emptyServicesCollection;
				}

				return base.JobServices;
			}
		}

		public override bool SkipFreightCharge
		{
			get
			{
				var line = Parent.Factory.Load<RatingContractAllocationLine>(Parent.JK_RCA_AllocationLine);

				return line?.RCA_AllowFreightSpotRate ?? false;
			}
		}

		public override ZString AircraftType
		{
			get
			{
				if (TransportMode != Constants.TransportModes.Air)
				{
					return ZString.Empty;
				}

				if (RatingDataRegistry.Instance.MultiModalRatingCost.Value)
				{
					return Parent.Transports.Cast<Transport>().Where(x => x.RouteSetNumber == RouteSetNumber).All(c => c.JW_IsCargoOnly)
						? Constants.AircraftType.CAO
						: Constants.AircraftType.PAX;
				}

				return Parent.JK_Calc_IsCargoOnly
					? Constants.AircraftType.CAO
					: Constants.AircraftType.PAX;
			}
		}

		#region Contract Numbers

		/// <summary>
		/// Returns the single consol carrier contract number if it is not blank or if it's blank and CONs has an empty number.
		/// Returns collection of consol carrier contract number if it is not blank and string.empty if CONs has an empty number
		/// An empty list is returned otherwise.
		/// </summary>
		public override IEnumerable<ZString> CarrierContractNumbers
		{
			get
			{
				//if you change this and the result returns more than one non empty contract number, make sure 
				//to update RemoveRateLinesOutOfDateRange in NotApplicableRateLineRemover.cs to use correct contract number
				var carrierContractNumber = Parent.JK_CarrierContractNumber;
				var hasNumberWithEmptyCon = Parent.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
											.Any(number => number.IsEmpty);

				if (hasNumberWithEmptyCon)
				{
					if (string.IsNullOrEmpty(carrierContractNumber))
					{
						return new[] { ZString.Empty };
					}

					return new[] { carrierContractNumber, ZString.Empty };
				}

				if (!string.IsNullOrEmpty(carrierContractNumber))
				{
					return new[] { carrierContractNumber };
				}

				return Enumerable.Empty<ZString>();
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

			/// <summary>
			/// When autorating and the according registry item is off, should limit the rates by job's contract numbers.
			/// </summary>
			public bool ShouldAddContractNumberQueryFilter =>
				costOrSell == CostSell.Cost
					? !ShouldIgnoreJobCarrierContractNumbers
					: !ShouldIgnoreJobClientContractNumbers;

			public bool ShouldApplySpecificAdapterContractNumberFilter => true;

			public bool ShouldIgnoreJobClientContractNumbers => FreightConfigurationRegistry.Instance.IgnoreAndReplaceClientContractNumbersDuringAutorating.Value;

			public bool ShouldIgnoreJobCarrierContractNumbers => FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.Value;

			public bool ShouldMatchJobBlankContractNumber => false;

			public bool ShouldUseCarrierContractDateFilter => costOrSell == CostSell.Cost && ObjectFactory.Get<IContractPermissions>().IsTariffAndRatesVisible();
		}

		#endregion

		public bool Enabled => StandardFreightCostEnabled;

		public virtual bool StandardFreightCostEnabled => ((FreightMode & FreightMode.AIR) == FreightMode.AIR);

		public void Set(Money cost)
		{
			if (cost.IsValid && Enabled)
			{
				var oldValue = Parent.IsSettingChargeableRateFromAutoRating;
				try
				{
					Parent.IsSettingChargeableRateFromAutoRating = true;
					var awbCurrency = Parent.AWBCurrency;
					ZDecimal? result = null;

					if (cost.Currency != null && awbCurrency != null && awbCurrency.RX_Code != cost.Currency.Code)
					{
						result = Parent.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(cost.Amount, cost.Currency.Code, awbCurrency.RX_Code);
					}

					Parent.JK_ConsolChargeableRate = result ?? cost.Amount;
				}
				finally
				{
					Parent.IsSettingChargeableRateFromAutoRating = oldValue;
				}
			}
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups => ChargeCodeGroupsOverride ?? (ChargeCodeGroupsOverride = base.ChargeCodeGroups);

		public ChargeCodeGroupCollection ChargeCodeGroupsOverride { get; set; }

		public BusinessObject GetHost()
		{
			return Parent;
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get
			{
				return Parent.InvoicingSupporter;
			}
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get
			{
				return Parent.InvoicingSupporter.ConsumerType;
			}
		}

		protected override IEnumerable<CommonShipment> GetAllShipmentsCore()
		{
			if (Parent.JK_AgentType == Constants.AgentType.AWBMaster)
			{
				return Parent.ColoadConsolsShipments.Cast<CommonShipment>();
			}

			return base.GetAllShipmentsCore();
		}

		void IAutoRatingNamedAccountPlugin.AddOrUpdateNamedAccount(string namedAccount)
		{
			const string namedAccountEntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			var namedAccountEntry = Parent.Numbers.GetFirstReferenceNumberByType(namedAccountEntryType);

			if (namedAccountEntry == null)
			{
				namedAccountEntry = Parent.Numbers.AddNew();
				namedAccountEntry.CE_EntryType = namedAccountEntryType;
			}

			namedAccountEntry.CE_EntryNum = namedAccount;
		}

		#region IGateway

		public IGatewayBillingSupporter GatewayBillingSupporter => ((IGateway)Parent).GatewayBillingSupporter;

		public virtual bool IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell)
		{
			if (!RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value || !Parent.IsGateway())
			{
				return false;
			}

			return costSell == CostSell.Revenue;
		}

		public virtual ZBool IsGatewaySellApplicableToGatewayConsol(CostSell costSell) => true;
		public virtual ZBool IsContainerNegotiatedCostApplicable(CostSell costSell) => true;
		public virtual List<ZGuid> SortedGatewayAgentPKs => new List<ZGuid>();
		public virtual List<ZGuid> SortedControllingCustomerPKs => new List<ZGuid>();
		public virtual List<ZGuid> GatewayAgentPKsForIntercompanyTariff => new List<ZGuid>();
		public virtual IDictionary<ZString, IList<ZGuid>> LoginGatewayAgentRoles => new Dictionary<ZString, IList<ZGuid>>();
		public virtual ZString GatewayLoginRole => ZString.Empty;
		public virtual List<ILocation> SortedOverridenPlannedLoad => new List<ILocation>();
		public virtual List<ILocation> SortedOverridenPlannedDischarge => new List<ILocation>();
		public virtual ZString GatewayAgentTypeFilteredReason(string agentType, ZGuid gatewayAgentPk, BillingType billingType, CostSell costSell) => ZString.Empty;

		public virtual ZString GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk)
			=> ((IGateway)Parent).GatewayServiceLevelFilteredReason(gatewayServiceLevel, gatewayAgentPk);
		public virtual ZString ShipmentGatewayServiceLevel => ((IGateway)Parent).ShipmentGatewayServiceLevel;

		public virtual bool ContinueWithDefaultCosting(BillingType billingType) => Parent.ContinueAutorateCosting(billingType);
		public virtual ZBool ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType billingType) => false;

		#endregion

		#region IJobDataUpdater

		public bool CanUpdateDate => true;

		public void UpdateRateCommodityCodeAndFMCTariffID(ZString newRateCommodityCode, ZString newFMCTariffID) { }
		public void UpdateDetailedGoodsDescription(ZString newDescription, bool append) { }

		public void UpdateServiceLevel(ZString newServiceLevel)
		{
			if (RouteSet != null)
			{
				return;
			}

			Parent.JK_AWBServiceLevel = newServiceLevel;
		}

		public void UpdateCarrier(OrgHeader newCarrier)
		{
			if (RouteSet != null)
			{
				RouteSet.ParentRouteSet.ReferenceLeg.JW_IsLinked = false;
				RouteSet.ParentRouteSet.ReferenceLeg.CarrierPK = newCarrier.PK;
			}
			else
			{
				Parent.SetShippingLine(newCarrier.MainAddress.PK, Res.GetString(
					"2087de38-3255-4621-a190-a1e6f1bf9f51",
					"Selected rate from Rate Selector had different service provider compare to Consol"));
			}
		}

		public bool UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
		{
			if (Carrier == null)
			{
				confirmationMessage = null;
				return true;
			}

			confirmationMessage = this.IsMultiRouteEnabled()
				? ResString.GetMultilingualString(
					"05e98059-afc0-440d-b1ce-7ddc86deb1d1",
					@"The Service Provider '{0}' of the selected rate is different from the Carrier / Creditor on your Consol and the Routing Legs in current Autorating process.

For the Routing Legs you are Autorating, they will be unlinked by un-ticking 'Is Linked' checkbox and their Carrier will be replaced with '{0}'.

You may need to review and also adjust the information of Routing Legs, Containers Info and Costing Charges if they are no longer valid.

Do you wish to continue?",
					newServiceProvider)
				: ResString.GetMultilingualString(
					"7d2c54c8-f6a4-4e27-89ae-a4c300a69aa2",
					@"During this operation, Service Provider '{0}' of the chosen rates will be populated as the Carrier of your Consol. 

You may need to manually adjust/remove any of the following if they are no longer valid:
	- Routing Legs
	- Containers Info
	- Consol Costing Charges

If your chosen rate is a spot rate linked with schedule, routing legs will be updated by the chosen schedule.

Do you wish to continue?",
					newServiceProvider);

			return true;
		}

		public void UpdateOrigin(ZString newOrigin)
		{
			if (RouteSet != null)
			{
				return;
			}

			Parent.JK_RL_NKLoadPort = newOrigin;
		}

		public bool UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
		{
			if (Parent.JK_RL_NKLoadPort != newOrigin)
			{
				confirmationMessage = ResString.GetMultilingualString(
					"8536af29-4f5e-4e80-adce-f25f1c99c4fd",
					"During this operation, would you like to update the 1st Load of the Consol to be the Origin '{0}' of the chosen rates?",
					newOrigin);

				return true;
			}

			confirmationMessage = string.Empty;

			return false;
		}

		public void UpdateDestination(ZString newDestination)
		{
			if (RouteSet != null)
			{
				return;
			}

			Parent.JK_RL_NKDischargePort = newDestination;
		}

		public virtual bool UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
		{
			if (Parent.JK_RL_NKDischargePort != newDestination)
			{
				confirmationMessage = ResString.GetMultilingualString(
					"c4256ffa-6748-4bac-84c8-007963267f36",
					"During this operation, would you like to update the Last Discharge of the Consol to be the Destination '{0}' of the chosen rates?",
					newDestination);

				return true;
			}

			confirmationMessage = string.Empty;

			return false;
		}

		public void UpdatePaymentTerms(ZString newPaymentTerms)
		{
			if (RouteSet != null)
			{
				return;
			}

			Parent.JK_PrepaidCollect = newPaymentTerms;
		}

		public void UpdateNamedAccount(ZString namedAccount)
		{
			var existingNamedAccount = Parent.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount);
			if (existingNamedAccount != null)
			{
				existingNamedAccount.CE_EntryNum = new ZString(namedAccount).SubstringSafe(0, CusEntryNumSchema.CE_EntryNum.MaxLength);
			}
			else
			{
				Parent.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, namedAccount);
			}
		}

		public bool UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			confirmationMessage = string.Empty;

			var confirmationIsNeeded = false;

			foreach (var group in newContainerPenalties.GroupBy(p => p.RefContainerCode))
			{
				foreach (var container in Parent.Containers.Cast<ForwardingContainer>().Where(c => c.Container.RC_Code == group.Key))
				{
					foreach (var penalty in group.Select(p => p))
					{
						ContainerPenaltyCollection penaltiesToWorkOn;

						if (penalty.CPY_ProcessType == ContainerPenaltyProcessType.Import)
						{
							penaltiesToWorkOn = container.ImportPenalties;
						}
						else
						{
							penaltiesToWorkOn = container.ExportPenalties;
						}

						var newPenalty = penaltiesToWorkOn.AddNew();
						newPenalty.CPY_PenaltyType = penalty.CPY_PenaltyType;
						newPenalty.CPY_CreditorType = penalty.CPY_CreditorType;

						var duplicateExists = GetExistingDuplicatedContainerPenalties(penaltiesToWorkOn, newPenalty).Any();

						penaltiesToWorkOn.Delete(newPenalty);

						if (duplicateExists)
						{
							confirmationIsNeeded = true;
							break;
						}
					}

					if (confirmationIsNeeded)
					{
						break;
					}
				}

				if (confirmationIsNeeded)
				{
					break;
				}
			}

			if (confirmationIsNeeded)
			{
				confirmationMessage = ResString.GetMultilingualString("6f65c19d-3497-46b1-b668-b8bb32fee429", @"During the operation Penalty Types entries of the selected rate to be populated will be duplicated with the existing entries under Container > Import Process / Export Process > Penalties grid.

Would you like to delete those existing duplicated Penalty Type entries?

Click 'Yes' to continue the process and delete the existing duplicated Penalty Type entries from the grid.
Click 'No' to continue the process and those Penalty Type entries will be duplicated.
Click 'Cancel' to stop the process and return to the Rate Selector.");
			}
			return confirmationIsNeeded;
		}
		public void UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates)
		{
			foreach (var group in containerPenalties.GroupBy(p => p.RefContainerCode))
			{
				foreach (var container in Parent.Containers.Cast<ForwardingContainer>().Where(c => c.Container.RC_Code == group.Key))
				{
					foreach (var penalty in group.Select(p => p))
					{
						ContainerPenaltyCollection penaltiesToWorkOn;

						if (penalty.CPY_ProcessType == ContainerPenaltyProcessType.Import)
						{
							penaltiesToWorkOn = container.ImportPenalties;
						}
						else
						{
							penaltiesToWorkOn = container.ExportPenalties;
						}

						var newPenalty = penaltiesToWorkOn.AddNew();
						newPenalty.CPY_PenaltyType = penalty.CPY_PenaltyType;
						newPenalty.CPY_CreditorType = penalty.CPY_CreditorType;
						newPenalty.CPY_FreeTime = penalty.CPY_FreeTime;
						newPenalty.CPY_TimeUnit = penalty.CPY_TimeUnit;
						newPenalty.CPY_PerUnitCost = penalty.CPY_PerUnitCost;
						newPenalty.CPY_RX_NKCurrency = penalty.CPY_RX_NKCurrency;

						if (deleteExistingDuplicates)
						{
							foreach (var item in GetExistingDuplicatedContainerPenalties(penaltiesToWorkOn, newPenalty))
							{
								penaltiesToWorkOn.Delete(item);
							}

							newPenalty.Validation.ValidateAll();
						}
					}
				}
			}
		}

		static IEnumerable<ContainerPenalty> GetExistingDuplicatedContainerPenalties(ContainerPenaltyCollection collection, ContainerPenalty penalty)
		{
			return
				collection
				.Where
					(c =>
						c.PK != penalty.PK
						&& c.CPY_ProcessType == penalty.CPY_ProcessType
						&& c.CPY_PenaltyType == penalty.CPY_PenaltyType
						&& c.CPY_OH_Creditor == penalty.CPY_OH_Creditor
						&& c.CPY_CreditorType == penalty.CPY_CreditorType
						&& c.CPY_RL_NKLocation == penalty.CPY_RL_NKLocation
					)
				.ToList();
		}

		public virtual void UpdateTransports(IEnumerable<ITransport> transports)
		{
			if (transports?.Any() != true)
			{
				return;
			}

			using (SailingScheduleDataVendor.SuppressVoyageOriginUpdate(Parent.Factory))
			using (SailingScheduleDataVendor.SuppressVoyageDestinationUpdate(Parent.Factory))
			{
				UpdateTransportsCore(transports);
			}
		}

		void UpdateTransportsCore(IEnumerable<ITransport> transports)
		{
			if (RouteSet == null)
			{
				GetUpdatedTransports(transports);
			}
			else
			{
				/*
				 *  When RouteSet is not null, it means that Multi-route autorating mode is enabled and we are autorating for current RouteSet.
				 *  In this case :
				 *  1 - We remove only current Routeset's legs.
				 *  2 - We replace them with new legs provided to this method.
				 *  3 - We shift the Leg Order of subsequent legs accordingly.
				 *
				 *  When we sort the legs by LegOrder, after finding the first leg of Routeset, subsequent legs would either belong to Routeset or we should update their LegOrder.
				 */

				var sortedExistingTransports =
					Parent
					.Transports
					.Cast<Transport>()
					.OrderBy(t => t.JW_LegOrder)
					.ToArray();

				var transportsToDelete = new List<Transport>();

				ZByte startingLegOrderOfNewTransports = 1;

				var foundTheFirstLegInRouteSet = false;

				var numberOfShiftsSubsequentLegsShouldMove = transports.Count() - (RouteSet?.ParentRouteSet?.NumberOfLegs ?? 0);

				foreach (var item in sortedExistingTransports)
				{
					if (RouteSet.ParentRouteSet.ContainsLeg(item.PK))
					{
						transportsToDelete.Add(item);

						if (!foundTheFirstLegInRouteSet)
						{
							foundTheFirstLegInRouteSet = true;
							startingLegOrderOfNewTransports = item.JW_LegOrder;
						}
					}
					else
					{
						if (foundTheFirstLegInRouteSet)
						{
							var newLegOrder = numberOfShiftsSubsequentLegsShouldMove + item.JW_LegOrder;
							item.JW_LegOrder = new ZByte(newLegOrder.ToString());
						}
					}
				}

				var createdNewTransports = GetUpdatedTransports(transports, startingLegOrderOfNewTransports, transportsToDelete);
				RouteSet = new RouteSetRatingRoute(new RouteSet(Parent.Factory, RouteSet.RouteSetNumber, createdNewTransports.ToArray()), Parent);
				OverrideRatingRoute(RouteSet);
			}
		}

		IEnumerable<Transport> GetUpdatedTransports(IEnumerable<ITransport> newTransports, int newTransportsStartingLegOrder = 1, List<Transport> transportsToDelete = null)
		{
			var result = new List<Transport>();

			var useFirstTransportAsNew = true;
			ZByte legOrderOfNewTransports = new ZByte(newTransportsStartingLegOrder.ToString());

			var carrierReferenceOfDeletedLegs = ZString.Empty;

			if (transportsToDelete == null)
			{
				Parent.Transports.RemoveAndDeleteAll();
			}
			else
			{
				carrierReferenceOfDeletedLegs = transportsToDelete.FirstOrDefault()?.JW_CarrierBookingReference ?? ZString.Empty;
				useFirstTransportAsNew = Parent.Transports.Count == transportsToDelete.Count;

				foreach (var item in transportsToDelete)
				{
					Parent.Transports.RemoveAndDelete(item);
				}
			}

			foreach (var item in newTransports)
			{
				Transport newTransport = null;

				if (useFirstTransportAsNew)
				{
					newTransport = Parent.Transports[0];
					useFirstTransportAsNew = false;
				}
				else
				{
					newTransport = Parent.Transports.AddNew();
				}

				using (newTransport.GetValidationSuspender())
				{
					newTransport.JW_LegOrder = legOrderOfNewTransports;
					legOrderOfNewTransports++;

					newTransport.JW_Status = item.JW_Status;
					newTransport.JW_IsLinked = item.JW_IsLinked;
					newTransport.JW_TransportMode = item.JW_TransportMode;
					newTransport.JW_VoyageFlight = item.JW_VoyageFlight;
					newTransport.JW_Vessel = item.JW_Vessel;
					newTransport.JW_OA_CarrierAddress = item.JW_OA_CarrierAddress;
					newTransport.JW_RL_NKLoadPort = item.JW_RL_NKLoadPort;
					newTransport.JW_RL_NKDiscPort = item.JW_RL_NKDiscPort;
					newTransport.JW_ETA = item.JW_ETA;
					newTransport.JW_ATA = item.JW_ATA;
					newTransport.JW_ETD = item.JW_ETD;
					newTransport.JW_LegNotes = item.JW_LegNotes;
					newTransport.JW_DocumentaryCutOff = item.JW_DocumentaryCutOff;
					newTransport.JW_TerminalCutOff = item.JW_TerminalCutOff;
					newTransport.JW_VGMCutOff = item.JW_VGMCutOff;
					newTransport.JW_CarrierBookingReference = carrierReferenceOfDeletedLegs;
				}
				newTransport.RunPreSaveValidation();

				result.Add(newTransport);
			}

			Parent.Transports.UpdateTransportTypes(true);

			return result;
		}

		public void UpdateCarrierQuoteNumber(ZString carrierQuoteNumber)
		{
			Parent.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber, carrierQuoteNumber);
		}

		public void UpdateContainersCarrierQuoteNumber(ZGuid containerRefPK, ZString carrierQuoteNumber)
		{
			foreach (var container in Parent.Containers.Cast<ForwardingContainer>().Where(c => c.JC_RC == containerRefPK))
			{
				container.AdditionalReferenceNumbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber, carrierQuoteNumber);
			}
		}

		public virtual void UpdateSpotBookingTerms(ZString termsAsText)
		{
			if (!string.IsNullOrEmpty(termsAsText))
			{
				StmNote note = Parent.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description).FirstOrDefault()
					?? Parent.Notes.AddNew();

				note.ST_Description = PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description;
				note.ST_NoteText = termsAsText;
				note.Validation.ValidateAll();
			}
		}

		/// <summary>
		/// Check if JK_CarrierContractNumber can be updated.
		/// Prompt interactive user (if needed) for selecting a single contract number or not populating.
		/// </summary>
		/// <param name="dialogService">Optional Rating DialogService (interactive mode) for selecting a contract number among many</param>
		/// <returns>
		/// A CanUpdateCarrierContractNumberResult consists of:
		/// - CanUpdate is false when user cancels popup dialog. We should stop Autorating with RSL in this case. Autorating without RSL can continue but doesn't update JK_CarrierContractNumber.
		///   CanUpdate is true in all other cases. Meaning: Autorating continues and JK_CarrierContractNumber can be updated with the included token from this result.
		/// - A confirmation message for overriding non-blank JK_CarrierContractNumber if the new number is different from it.
		/// - An update token (<see cref="UpdateCarrierContractNumberToken"/>) to pass to <see cref="UpdateCarrierContractNumber"/> to update JK_CarrierContractNumber.
		/// </returns>
		public CanUpdateCarrierContractNumberResult CanUpdateCarrierContractNumber(IEnumerable<string> newContractNumbers, IDialogService dialogService = null, bool isManualCostSelected = false)
		{
			if (!newContractNumbers.Any() || !ZArchitecture.Environment.Globals.IsUserInteractive)
			{
				return new CanUpdateCarrierContractNumberResult(newContractNumbers);
			}

			var oldNumber = Parent.JK_CarrierContractNumber;
			var criteriaUsesNonBlankNumber = !oldNumber.IsEmpty && !FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.Value;
			var conNumbersHasBlankEntry = Parent.Numbers.ListContractNumbers().Any(cusEntryNumber => cusEntryNumber.CE_EntryNum.IsEmpty);
			var shouldNotPopulateWhenBlank = oldNumber.IsEmpty
				&& (conNumbersHasBlankEntry || !FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.Value);
			if (!isManualCostSelected && (criteriaUsesNonBlankNumber || shouldNotPopulateWhenBlank))
			{
				// CanUpdate is false
				return default(CanUpdateCarrierContractNumberResult);
			}

			// The only new single number which is the same as existing number can be returned early as selected.
			if (newContractNumbers.Distinct().Count() == 1 && string.Compare(newContractNumbers.First(), oldNumber.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return new CanUpdateCarrierContractNumberResult
				{
					CanUpdate = true,
					Token = new UpdateCarrierContractNumberToken(newContractNumbers)
					{
						SelectionResult = new SingleCarrierContractNumberSelectionResult(oldNumber)
					}
				};
			}

			var selectionResult = SelectSingleCarrierContractNumber(newContractNumbers, dialogService);
			switch (selectionResult.Result)
			{
				case ContractNumberSelectionResult.Cancelled:
					return new CanUpdateCarrierContractNumberResult { CanUpdate = false };

				case ContractNumberSelectionResult.Default:
				case ContractNumberSelectionResult.KeepJobCarrierContractNumber:
					return new CanUpdateCarrierContractNumberResult
					{
						CanUpdate = true,
						Token = new UpdateCarrierContractNumberToken(newContractNumbers)
						{
							SelectionResult = selectionResult
						}
					};
			}

			// A number is selected from the popup
			string confirmationMessage = null;
			var selectedNumber = selectionResult.Number;
			if (!oldNumber.IsEmpty && string.Compare(oldNumber, selectedNumber, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				confirmationMessage = Res.GetString("DF07C34B-68C2-4D8F-A5F1-ADC643DCA158", @"During the operation, Contract Number '{0}' of the chosen rates will be populated as the Carrier Contract Number of your Consol.
You may need to manually adjust the following if they are no longer valid:
- Container > Import/Export Process > Penalty Grid > Free Time for Penalty Type of 'DET/STO' and Creditor Type of 'CAR'.
Do you wish to continue?", selectedNumber);
			}

			return new CanUpdateCarrierContractNumberResult
			{
				CanUpdate = true,
				ConfirmationMessageForOverridingJobContractNumber = confirmationMessage,
				Token = new UpdateCarrierContractNumberToken(newContractNumbers)
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
			return dialogService?.SelectSingleCarrierContractNumber(contractNumbers.Distinct())
				?? default(SingleCarrierContractNumberSelectionResult);
		}

		public void UpdateAutoratingDate(ZDate autoratingDate, bool isCosting)
		{
			if (isCosting)
			{
				Parent.AutoratingDate = autoratingDate;
			}
		}

		public bool IsMultipleCarrierContractNumberSupported => false;

		public DataUpdateResult UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token)
		{
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
						var canReplace = FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.Value;
						var canPopulateWhenBlank = FreightConfigurationRegistry.Instance.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating.Value && Parent.JK_CarrierContractNumber.IsEmpty;
						if (distinctNumbers.Length == 1 && (canReplace || canPopulateWhenBlank))
						{
							numberToUpdate = distinctNumbers[0];
						}
						break;
					}
			}

			if (numberToUpdate == null || Parent.JK_CarrierContractNumber == numberToUpdate)
			{
				return DataUpdateResult.NoAction;
			}

			Parent.JK_CarrierContractNumber = numberToUpdate.Length > JobConsolSchema.JK_CarrierContractNumber.MaxLength
				? numberToUpdate.Substring(0, JobConsolSchema.JK_CarrierContractNumber.MaxLength)
				: numberToUpdate;

			return DataUpdateResult.Updated;
		}

		public bool IsMultipleClientContractNumberSupported => true;
		public DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers) => DataUpdateResult.NoAction;
		public void UpdateChargeable(ZDecimal newChargeable) { }

		#endregion

		#region IManualRateSelectionSupporter

		bool IManualRateSelectionSupporter.SupportsManualRateSelection
		{
			get
			{
				if (RouteSet != null)
				{
					return RouteSet.SupportsManualRateSelection;
				}

				return ConsolRouteSet.SupportsManualRateSelection;
			}
		}

		PaymentTermInfos IManualRateSelectionSupporter.DefaultFilterValueForPaymentTerm => PaymentTerm;
		ILocation IManualRateSelectionSupporter.DefaultFilterValueForOrigin => Origin;
		ILocation IManualRateSelectionSupporter.DefaultFilterValueForDestination => Destination;
		bool IManualRateSelectionSupporter.ContinueAutoratingWithoutRateSelector => false;
		string IManualRateSelectionSupporter.OriginMissingMessage => null;
		string IManualRateSelectionSupporter.DestinationMissingMessage => null;

		#endregion

		#region IAutoRatingSpotChargeInfo
		public IDictionary<string, IEnumerable<string>> GetJobSpotCharges()
		{
			var costs = GetConsolCosts();
			return costs
				.Where(c => RatingBehaviours.IsAutoRatingOverriderSpotBehaviour(c.RatingBehaviour))
				.GroupBy(c => c.RatingBehaviour)
				.ToDictionary(g => g.Key, g => g.Select(c => c.AC_Code));
		}

		IEnumerable<IRatingJobConsolCost> GetConsolCosts()
		{
			var results = new List<IRatingJobConsolCost>();

			var costQuery = new ZQuery();
			costQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, Parent.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
			results.AddRange(Parent.Factory.Load<IRatingJobConsolCost>(costQuery));

			return results;
		}

		public void SendBookingInformationToCarrier()
		{
			Parent?.Factory.Save();
			var provider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();

			var query = new ZQuery(StmMenuItemSchema.PK, ConsolSystemFormMenuItems.DocumentMenuBookingRequestPK);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, CargoWise.Definitions.BusinessContext.Consol);

			var menuItem = Parent?.Factory.LoadTop1<StmMenuItemBase>(query);
			var command = provider.GetCommand(Parent, menuItem, ModuleIDs.JobConsol);
			command?.Execute();
		}

		#endregion

		#region IAutoratingAccountingInfo

		public override IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false)
		{
			var query = new ZQuery();
			query.AddToFilter(JobConsolCostSchema.E6_ParentID, Parent.PK);
			query.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);

			if (!fromAllCompanies)
			{
				query.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
			}

			var jobConsolCosts = Parent.Factory
				.Load<IJobConsolCost>(query)
				.OfType<IAutoRatingChargeInfo>()
				.ToArray();

			return jobConsolCosts;
		}

		#endregion

		public override IEnumerable<OrgHeader> PossibleServiceProviders
		{
			get
			{
				var serviceProviders = new List<OrgHeader>
				{
					Parent.ShippingLine,
					Parent.Creditor,
				};

				if (RatingDataRegistry.Instance.MultiModalRatingCost.Value)
				{
					var route =
						(Parent as IRoutingSupport)?
						.TransportsIncludingRelated.RouteSets
						.FirstOrDefault(x => x.RouteSetNumber == RouteSetNumber);

					serviceProviders.Add(route?.Carrier);
					serviceProviders.Add(route?.ReferenceLeg?.Creditor);
				}
				else
				{
					serviceProviders.Add(Parent.Transports.MostInterestingTransport?.Carrier);
					serviceProviders.Add(Parent.Transports.MostInterestingTransport?.Creditor);
				}

				return serviceProviders.WhereNotNull().Distinct();
			}
		}
	}
}
