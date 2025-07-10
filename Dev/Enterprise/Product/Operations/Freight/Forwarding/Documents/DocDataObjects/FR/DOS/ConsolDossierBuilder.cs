using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using FRPortHelpers = Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Helpers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ConsolDossierBuilder
	{
		public ConsolDossierBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			this.parameters = Argument.NotNull(parameters, nameof(parameters));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public Dossier Build()
		{
			var dossier = new Dossier(nameof(ForwardingConsol), consol.JK_UniqueConsignRef);
			dossier.IsImport = string.Compare(parameters.DataStoreName, ConsolDocumentDataStoreNames.DossierImport, StringComparison.OrdinalIgnoreCase) == 0;

			dossier.ConsolNumber = consol.JK_UniqueConsignRef;
			dossier.ThirdPartyReference = consol.JK_UniqueConsignRef;

			dossier.CarrierBookingReference = consol.JK_BookingReference;
			dossier.BillOfLading = consol.JK_MasterBillNum;

			dossier.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List) { Code = consol.JK_ConsolMode };
			dossier.ShipmentType = new CodeDescription(consol.JK_AgentType_List) { Code = consol.JK_AgentType };

			var operationalPort = dossier.IsImport
							? consol.ArrivalCTOAddress?.RelatedPortCode ?? SeaTransports.LastOrDefault()?.DiscPort
							: consol.DepartureCTOAddress?.RelatedPortCode ?? SeaTransports.FirstOrDefault()?.LoadPort;

			dossier.PCS = operationalPort?.GetPCS() ?? ZString.Empty;
			dossier.OperationalPort = Unloco.Create(context, operationalPort);

			var firstDepartureTransport = SeaTransports.FirstOrDefault(t => Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKLoadPort));
			var lastDischargeTransport = SeaTransports.LastOrDefault(t => Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKDiscPort));

			dossier.ATP = dossier.IsImport
				? lastDischargeTransport?.Sailing?.Destination?.JB_ArrivalReference ?? ZString.Empty
				: firstDepartureTransport?.Sailing?.Origin?.JA_DepartReference ?? ZString.Empty;

			dossier.ConfirmationReference = GetConfirmationReferenceFromEvents(dossier);
			dossier.ImplicitAcknowledgement = true;
			dossier.ImplicitBAET = true;

			PopulateParties(dossier);

			AddValidations(dossier);
			dossier.ValidateAllIncludingChildren();

			return dossier;
		}

		#region PopulateParties

		void PopulateParties(Dossier dossier)
		{
			dossier.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			dossier.CarrierSON = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			dossier.CarrierCI5 = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			dossier.FormattedCarrierProviderID = FRPortHelpers.CreateFormattedProviderID(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, dossier.CarrierSON.ValueInfo, dossier.CarrierCI5.ValueInfo);

			dossier.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderAddress);
			dossier.SendingForwarderSON = consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			dossier.SendingForwarderCI5 = consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			dossier.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderAddress);
			dossier.ReceivingForwarderSON = consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			dossier.ReceivingForwarderCI5 = consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress;
			if (proxyMainAddress == null || proxyMainAddress.Address1.IsEmpty)
			{
				proxyMainAddress = GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			}

			dossier.SendingParty = AddressBuilder.CreateForCurrentUser(context);
			dossier.SendingPartySON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort);
			dossier.SendingPartyCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort);
			dossier.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, dossier.SendingPartySON.ValueInfo, dossier.SendingPartyCI5.ValueInfo);

			dossier.ThirdParty = dossier.IsImport ? dossier.ReceivingForwarder : dossier.SendingForwarder;
			dossier.ThirdPartySON = dossier.IsImport
				? consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort)
				: consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort);
			dossier.ThirdPartyCI5 = dossier.IsImport
				? consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort)
				: consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort);
			dossier.FormattedThirdPartyProviderID = FRPortHelpers.CreateFormattedProviderID(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, dossier.ThirdPartySON.ValueInfo, dossier.ThirdPartyCI5.ValueInfo);
		}

		#endregion

		#region ConfirmationReference

		string GetConfirmationReferenceFromEvents(Dossier dossier)
		{
			foreach (var log in GetEventLogsInDescendingOrder(dossier))
			{
				if (log.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode)
				{
					return string.Empty;
				}

				if (log.SL_SE_NKEvent == Events.MessageAcceptedCode)
				{
					if (log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber))
					{
						return referenceNumber;
					}
				}
			}
			return string.Empty;
		}

		IEnumerable<StmALog> GetEventLogsInDescendingOrder(Dossier dossier)
		{
			foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);
				var documentName = dossier.IsImport ? FrenchPortsConstants.DocumentNames.DOSImport : FrenchPortsConstants.DocumentNames.DOSExport;

				if (string.Compare(messageType, documentName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		#endregion

		#region AddValidations

		void AddValidations(Dossier dossier)
		{
			if (dossier.IsImport)
			{
				dossier.BillOfLadingInfo.AddMessageErrorIfEmpty(Res.GetString("fc9a51bb-84cd-41aa-b896-a12634605f7d", "Bill of Lading (BOL) Number is required."));
			}
			else
			{
				dossier.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("65f4ae6d-e1c7-4bc9-af4b-cfd8577faf09", "Carrier Booking Reference is required."));
				dossier.CarrierBookingReferenceInfo.AddMessageError(() => dossier.CarrierBookingReference.Length > 17, Res.GetString("97113b6a-c5b0-4dc9-a5f1-4843ec39bd38", "Carrier Booking Reference is too long. Maximum 17 characters allowed."));
			}

			dossier.ATPInfo.AddMessageError(() => dossier.ATP.Length > 17, dossier.IsImport ?
																												Res.GetString("53a5099c-6ce9-4732-bcfb-0465ab06c6f0", "ATP Reference (Arrival Reference from Sailing Schedule of Consol > Routing > Last SEA leg) value is too long. Maximum 17 characters allowed.") :
																												Res.GetString("362a8eb2-aa15-49e5-98b1-9403ffad6d94", "ATP Reference (Departure Reference from Sailing Schedule of Consol > Routing > First SEA leg) value is too long. Maximum 17 characters allowed."));

			dossier.PCSInfo.AddPCSValidation();
			((Unloco)dossier.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(dossier.IsImport ?
																																							Res.GetString("76508503-a599-4ac6-8e91-ee7038eb71cd", "Operational Port is required, UNLOCO missing from Consol > Arrival > CTO Address or Discharge Port of the last SEA leg.") :
																																							Res.GetString("40dd4db3-5452-4609-a34a-6dbfd10a3875", "Operational Port is required, UNLOCO missing from Consol > Departure > CTO Address or Load Port of the first SEA leg."));

			var carrierPartyName = Res.GetString("81fd2306-9c3f-4334-835e-6eabfe010543", "Carrier");
			var carrierConfigPath = Res.GetString("f4c64a29-4d2f-48e2-ad62-c962bf024df0", "Consol > Carrier");

			var sendingPartyName = Res.GetString("86afdb6d-af04-431b-a801-005c6edac23a", "Sending Party");
			var sendingPartyCodeType = Res.GetString("0c32dbaf-390d-4b29-be26-37cfa05b88d8", "Forwarder");
			var sendingPartyConfigPath = Res.GetString("d588267f-bf20-42f0-ba05-d9ebc2fc293b", "Org. Proxy");

			var thirdPartyName = Res.GetString("2c460b02-4899-473a-9787-66419fb3ac81", "Third Party");
			var thirdPartyConfigPath = dossier.IsImport ? Res.GetString("d9e44771-8f61-4aad-9164-b41dbec96b1d", "Consol > Receiving Agent") : Res.GetString("5f7e575c-5015-42ce-ba60-72f0b7445e99", "Consol > Sending Agent");

			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");

			dossier.Carrier.AddPartyNameAndAddressValidation(carrierPartyName, messageValidation: messageValidation);
			dossier.SendingParty.AddPartyNameAndAddressValidation(sendingPartyName, messageValidation: messageValidation);
			dossier.ThirdParty.AddPartyNameAndAddressValidation(thirdPartyName, messageValidation: messageValidation);

			dossier.FormattedCarrierProviderIDInfo.AddFormattedProviderIDValidations(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, carrierPartyName, configPath: carrierConfigPath, isOrganization: true);
			dossier.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, sendingPartyName, sendingPartyCodeType, sendingPartyConfigPath);
			dossier.FormattedThirdPartyProviderIDInfo.AddFormattedProviderIDValidations(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, thirdPartyName, sendingPartyCodeType, thirdPartyConfigPath);

			dossier.ThirdPartyReferenceInfo.AddMessageError(() => dossier.ThirdPartyReference.Length > 17, Res.GetString("34b1f9c7-33a5-434e-bcea-86efaa4bb315", "Consol Number (which is used as Reference for this message) is too long.  Maximum 17 characters are allowed."));
			dossier.NotesInfo.AddMessageError(() => dossier.Notes.Length > 210, Res.GetString("368d6d50-3914-41af-841e-07272e69719c", "Notes field value is too long.  Maximum 210 characters allowed."));
		}

		#endregion

		#region Transports

		public IReadOnlyCollection<Freight.Business.Transport> SeaTransports => seaTransports ?? (seaTransports = GetSeaTransports());
		IReadOnlyCollection<Freight.Business.Transport> seaTransports;

		IReadOnlyCollection<Freight.Business.Transport> GetSeaTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.TransportMode == Core.Constants.TransportModes.Sea)
				.ToArray();
		}

		#endregion
	}
}
