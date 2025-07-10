using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentRateSelectorEnabledRatingAdapter : ForwardingShipmentRatingAdapter, IManualRateSelectionSupporter, IJobDataUpdater
	{
		protected internal ForwardingShipmentRateSelectorEnabledRatingAdapter(ForwardingShipment parent) : base(parent)
		{
			if (parent.Consols.Count != 1)
			{
				throw new ArgumentException("Only shipments with 1 consol are allowed for this Rating Adapter", nameof(parent));
			}
		}

		ForwardingConsol ParentConsol => (ForwardingConsol)Parent.GetFirstOrCorrectConsol();
		ForwardingConsolRatingAdapter ParentConsolRatingAdapter => (ForwardingConsolRatingAdapter)ParentConsol.RatingAdapter;

		public override IEnumerable<OrgHeader> PossibleServiceProviders => ParentConsolRatingAdapter.PossibleServiceProviders;
		public override ZString NamedAccount => ParentConsolRatingAdapter.NamedAccount;

		#region IManualRateSelectionSupporter

		bool IManualRateSelectionSupporter.SupportsManualRateSelection => true;
		PaymentTermInfos IManualRateSelectionSupporter.DefaultFilterValueForPaymentTerm => ParentConsolRatingAdapter.PaymentTerm;
		ILocation IManualRateSelectionSupporter.DefaultFilterValueForOrigin => Origin;
		ILocation IManualRateSelectionSupporter.DefaultFilterValueForDestination => Destination;
		bool IManualRateSelectionSupporter.ContinueAutoratingWithoutRateSelector => false;
		string IManualRateSelectionSupporter.OriginMissingMessage => null;
		string IManualRateSelectionSupporter.DestinationMissingMessage => null;

		#endregion

		#region IJobDataUpdater

		bool IJobDataUpdater.CanUpdateDate => false;

		bool IJobDataUpdater.UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
		{
			confirmationMessage = ResString.GetMultilingualString(
					"8f6b928e-2477-4b69-b980-2cc9bb842b8e",
					@"During this operation, Service Provider '{0}' of the chosen rates will be populated as the Carrier of the Consol of your shipment. 

You may need to manually adjust/remove any of the following if they are no longer valid:
	- Routing Legs
	- Containers Info
	- Consol Costing Charges

Do you wish to continue?",
					newServiceProvider);

			return true;
		}
		bool IJobDataUpdater.UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
		{
			if (newDestination != ParentConsol.JK_RL_NKDischargePort)
			{
				confirmationMessage = ResString.GetMultilingualString(
					"28275329-c0fd-4f40-9e24-3a1d62e14b02",
					"During this operation, would you like to update the Last Discharge of the Consol of your shipment to be the Destination '{0}' of the chosen rates?",
					newDestination);

				return true;
			}

			confirmationMessage = null;
			return false;
		}

		bool IJobDataUpdater.UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
		{
			if (newOrigin != ParentConsol.JK_RL_NKLoadPort)
			{
				confirmationMessage = ResString.GetMultilingualString(
					"dd97f96f-baf1-4f1f-8483-1cef239c24c8",
					"During this operation, would you like to update the 1st Load of the Consol of your shipment to be the Origin '{0}' of the chosen rates?",
					newOrigin);

				return true;
			}

			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateCarrier(OrgHeader newCarrier)
		{
			if (ParentConsolRatingAdapter.RouteSet != null)
			{
				ParentConsolRatingAdapter.RouteSet.ParentRouteSet.ReferenceLeg.JW_IsLinked = false;
				ParentConsolRatingAdapter.RouteSet.ParentRouteSet.ReferenceLeg.CarrierPK = newCarrier.PK;
			}
			else
			{
				ParentConsol.SetShippingLine(newCarrier.MainAddress.PK, Res.GetString("7d8ff71f-5919-4f1c-95af-01dae0421aba", "Selected rate from Rate Selector had different service provider compare to Consol"));
			}
		}

		void IJobDataUpdater.UpdateDestination(ZString newDestination)
		{
			if (ParentConsolRatingAdapter.RouteSet != null)
			{
				return;
			}

			if (newDestination != ParentConsol.JK_RL_NKDischargePort)
			{
				ParentConsol.JK_RL_NKDischargePort = newDestination;
			}
		}

		void IJobDataUpdater.UpdateNamedAccount(ZString namedAccount)
		{
			var existingNamedAccount = ParentConsol.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount);
			if (existingNamedAccount != null)
			{
				existingNamedAccount.CE_EntryNum = new ZString(namedAccount).SubstringSafe(0, CusEntryNumSchema.CE_EntryNum.MaxLength);
			}
			else
			{
				ParentConsol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, namedAccount);
			}
		}

		void IJobDataUpdater.UpdateOrigin(ZString newOrigin)
		{
			if (ParentConsolRatingAdapter.RouteSet != null)
			{
				return;
			}

			if (newOrigin != ParentConsol.JK_RL_NKLoadPort)
			{
				ParentConsol.JK_RL_NKLoadPort = newOrigin;
			}
		}

		void IJobDataUpdater.UpdatePaymentTerms(ZString newPaymentTerms)
		{
			if (ParentConsolRatingAdapter.RouteSet != null)
			{
				return;
			}

			ParentConsol.JK_PrepaidCollect = newPaymentTerms;
		}

		void IJobDataUpdater.UpdateServiceLevel(ZString newServiceLevel)
		{
			if (ParentConsolRatingAdapter.RouteSet != null)
			{
				return;
			}

			ParentConsol.JK_AWBServiceLevel = newServiceLevel;
		}

		void IJobDataUpdater.UpdateCarrierQuoteNumber(ZString carrierQuoteNumber)
		{
			ParentConsol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber, carrierQuoteNumber);
		}

		bool IJobDataUpdater.UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			if (newContainerPenalties != null && newContainerPenalties.Any())
			{
				return ParentConsolRatingAdapter.UpdateContainerPenaltiesConfirmationIsNeeded(newContainerPenalties, out confirmationMessage);
			}
			confirmationMessage = string.Empty;
			return false;
		}

		void IJobDataUpdater.UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates)
		{
			ParentConsolRatingAdapter.UpdateContainerPenalties(containerPenalties, deleteExistingDuplicates);
		}

		void IJobDataUpdater.UpdateSpotBookingTerms(ZString termsAsText)
		{
			ParentConsolRatingAdapter.UpdateSpotBookingTerms(termsAsText);
		}

		CanUpdateCarrierContractNumberResult IJobDataUpdater.CanUpdateCarrierContractNumber(IEnumerable<string> newContractNumbers, IDialogService dialogService, bool isManualCostSelected)
		{
			var consolAdapter = ParentConsolRatingAdapter;
			return consolAdapter.RouteSet == null
				? consolAdapter.CanUpdateCarrierContractNumber(newContractNumbers, dialogService, isManualCostSelected)
				: new CanUpdateCarrierContractNumberResult(newContractNumbers);
		}

		public override bool IsMultipleCarrierContractNumberSupported => false;

		DataUpdateResult IJobDataUpdater.UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token)
		{
			var consolAdapter = ParentConsolRatingAdapter;
			return consolAdapter.RouteSet == null
				? consolAdapter.UpdateCarrierContractNumber(token)
				: DataUpdateResult.NoAction;
		}

		void IJobDataUpdater.UpdateTransports(IEnumerable<ITransport> transports)
		{
			ParentConsolRatingAdapter.UpdateTransports(transports);
		}

		bool IJobDataUpdater.IsMultipleClientContractNumberSupported => true;
		DataUpdateResult IJobDataUpdater.UpdateClientContractNumber(IEnumerable<string> newNumbers) => DataUpdateResult.NoAction;

		#endregion

		#region RatingAdapter

		public override bool SkipFreightCharge
		{
			get
			{
				var line = Parent.Factory.Load<RatingContractAllocationLine>(ParentConsol.JK_RCA_AllocationLine);

				return line?.RCA_AllowFreightSpotRate ?? false;
			}
		}

		#endregion
	}
}
