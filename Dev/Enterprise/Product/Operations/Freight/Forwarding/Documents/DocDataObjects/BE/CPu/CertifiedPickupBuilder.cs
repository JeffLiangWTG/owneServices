using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class CertifiedPickupBuilder
	{
		public CertifiedPickupBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));

			this.consol = consol;
			this.parameters = parameters;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public CertifiedPickup Build()
		{
			var certifiedPickup = new CertifiedPickup(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			certifiedPickup.OperationalPort = Unloco.Create(context, consol.DischargePort);
			certifiedPickup.Terminal = consol.ArrivalCTOAddress.GetPortSystemNumber(context).Value;
			certifiedPickup.BillOfLading = consol.JK_MasterBillNum;

			certifiedPickup.FormMode = GetFormMode();

			PopulateParties(certifiedPickup);
			PopulateContainers(certifiedPickup);
			PopulateDataObjectWriterFields(certifiedPickup);

			AddValidations(certifiedPickup);
			certifiedPickup.ValidateAllIncludingChildren();

			return certifiedPickup;
		}

		void PopulateParties(CertifiedPickup certifiedPickup)
		{
			certifiedPickup.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			certifiedPickup.SendingParty = AddressBuilder.CreateForCurrentUser(context);
			certifiedPickup.SendingPartyId = proxyMainAddress.GetPartyIdWithFallback(context);

			certifiedPickup.Forwarder = AddressBuilder.Create(context, consol.ReceivingForwarderWithContact);
			certifiedPickup.ForwarderId = ((OrgAddress)consol.ReceivingForwarderWithContact?.OrgAddress).GetPartyIdWithFallback(context);

			certifiedPickup.TransportCompany = AddressBuilder.Create(context, consol.ArrivalUnpackCFSTransportAddress);
			certifiedPickup.TransportCompanyId = consol.ArrivalUnpackCFSTransportAddress.GetPartyIdWithFallback(context);

			certifiedPickup.CarrierIdentificationId = consol.ShippingLineAddress.GetPartyIdWithFallback(context);
		}

		void PopulateContainers(CertifiedPickup certifiedPickup)
		{
			var containerBuilder = new CertifiedPickupContainerBuilder();
			var containerDOs = new List<CertifiedPickupContainer>();
			var selectedContainerPKs = GetSelectedContainerPKs();

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				containerDOs.Add(containerBuilder.Build(containerBO, selectedContainerPKs.Contains(containerBO.PK) ? certifiedPickup.FormMode : CertifiedPickup.FormModeNotApplicable));
			}

			certifiedPickup.Containers = containerDOs.OrderBy(container => container.Number).ToArray();
		}

		void PopulateDataObjectWriterFields(CertifiedPickup certifiedPickup)
		{
			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			certifiedPickup.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};
		}

		#region Validations

		void AddValidations(CertifiedPickup certifiedPickup)
		{
			((Unloco)certifiedPickup.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("1ED84BD9-2321-43C6-9FB7-2FDE8AB7FC12", "Operational port is required."));
			certifiedPickup.BillOfLadingInfo.AddMessageErrorIfEmpty(Res.GetString("c9e9d3bc-394b-40a8-85fc-39fd5e93e47c", "BOL Number is required."));
			certifiedPickup.TerminalInfo.AddMessageErrorIfEmpty(Res.GetString("c662f39d-61a5-47bd-aa40-8a1b706aab5d", "Terminal is required."));

			AddPartiesValidations(certifiedPickup);
			AddContainersValidations(certifiedPickup);
		}

		void AddPartiesValidations(CertifiedPickup certifiedPickup)
		{
			certifiedPickup.SendingParty.AddPartyNameAndAddressValidation(Res.GetString("AA056261-F620-4F3E-99B6-201774E12CF9", "Sending Party"));
			certifiedPickup.Forwarder.AddPartyNameAndAddressValidation(Res.GetString("89B2A493-CAA9-42A2-9D4F-5EC8FB20DB64", "Receiving Agent"));

			certifiedPickup.SendingPartyId.ValueInfo.AddMessageErrorIfEmpty(Res.GetString("d137e139-70dd-45f1-86eb-499b5d8f11dd", "Sender Identification is required. Please provide  organization > Config > Registration Numbers / Codes - type 'PSN', 'DUN', 'EOR' or 'BTW'."));
			certifiedPickup.CarrierIdentificationId.ValueInfo.AddMessageErrorIfEmpty(Res.GetString("84686040-5ce0-4d39-bc28-eecde89e5a0c", "Carrier Identification is required in case of Transfer or Revoke action. Please provide  organization > Config > Registration Numbers / Codes - type 'PSN', 'DUN' or 'EOR'."));
		}

		void AddContainersValidations(CertifiedPickup certifiedPickup)
		{
			foreach (var container in certifiedPickup.SelectedContainers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("24BC460C-2DCD-4A38-9777-E95890C43129", "Container number is required."));
				container.ReleaseIdentificationInfo.AddMessageErrorIfEmpty(Res.GetString("f689a0a5-5095-492f-9698-12bca8afb850", "Release Identification is required."));

				container.ErrorPlaceHolderInfo.AddMessageError(()
					=> container.Action.ObjectValue.IsEmpty && !(certifiedPickup.IsTransferMode && container.CurrentStatus == CertifiedPickupConstants.Status.TransferSentAwaitingResponse)
					, certifiedPickup.IsTransferMode ? Res.GetString("5d874b20-84d5-47e0-8031-6d793f9e04db", "Please select Transfer To Party.") : Res.GetString("a0da103e-0b79-48cc-8a99-14cff1b134ed", "Please select a send action."));

				container.ReleaseFromNameInfo.AddMessageErrorIfEmpty(Res.GetString("4817E181-372E-443D-BEEC-8A40791C7E72", "Received From Name is required."));
				container.ReleaseFromIdInfo.AddMessageErrorIfEmpty(Res.GetString("F3EED8E6-2B24-4C99-BDD6-0C4159AE904C", "Received From ID is required."));
				container.ReleaseFromCodeInfo.AddMessageErrorIfEmpty(Res.GetString("AB74421B-C1FD-4635-9ED3-843CB96A74A2", "Received From Code is required."));

				if (certifiedPickup.IsTransferMode && container.CurrentStatus != CertifiedPickupConstants.Status.TransferSentAwaitingResponse)
				{
					certifiedPickup.ForwarderId.ValueInfo.AddMessageError(() => container.Action.IsTransferToForwarder && certifiedPickup.ForwarderId.Value.IsEmpty, Res.GetString("5339e835-d92d-4a08-a4c7-08f4d83dfbc5", "Receiving Forwarder Identification is required. Please provide organization > Config > Registration Numbers / Codes - type 'PSN', 'DUN', 'EOR' or 'BTW'."));
					certifiedPickup.TransportCompanyId.ValueInfo.AddMessageError(() => container.Action.IsTransferToTransporter && certifiedPickup.TransportCompanyId.Value.IsEmpty, Res.GetString("afec5fa2-cc27-47d3-bd61-f2edbe4bf6e7", "Transport Company identification code is required. Please provide organization > Config > Registration Numbers / Codes - type 'PSN', 'DUN', 'EOR' or 'BTW'."));

					container.Action.ObjectValueInfo.ValueChanged += (object sender, System.EventArgs e) =>
					{
						certifiedPickup.ForwarderId.ValidateAll();
						certifiedPickup.TransportCompanyId.ValidateAll();
					};
				}
			}
		}

		#endregion

		#region Implement

		IEnumerable<ZGuid> GetSelectedContainerPKs()
		{
			if (parameters?.Data is IReadOnlyCollection<ForwardingContainer> containers)
			{
				return containers.Select(c => c.PK);
			}

			return new List<ZGuid>();
		}

		string GetFormMode()
		{
			switch (parameters.DocumentTitle)
			{
				case BelgianPortsConstants.DocumentNames.CPuReleaseRightAcceptDecline:
					return CertifiedPickup.FormModeAcceptDecline;
				case BelgianPortsConstants.DocumentNames.CPuReleaseRightTransfer:
					return CertifiedPickup.FormModeTransfer;
				case BelgianPortsConstants.DocumentNames.CPuReleaseRightRevoke:
					return CertifiedPickup.FormModeRevoke;
				default:
					return CertifiedPickup.FormModeAcceptDecline;
			}
		}

		#endregion
	}
}
