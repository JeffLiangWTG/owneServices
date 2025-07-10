using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using FRPortHelpers = Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Helpers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ShipmentDossierBuilder
	{
		public ShipmentDossierBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.parameters = Argument.NotNull(parameters, nameof(parameters));
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
			consol = GetConsolFromFrance();
		}

		readonly ForwardingConsol consol;
		readonly ForwardingShipment shipment;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public Dossier Build()
		{
			var dossier = new Dossier(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);

			dossier.IsImport = string.Compare(parameters.DataStoreName, ShipmentDocumentDataStoreNames.DossierImport, StringComparison.OrdinalIgnoreCase) == 0;
			dossier.ShipmentNumber = shipment.JS_UniqueConsignRef;
			dossier.ThirdPartyReference = shipment.JS_UniqueConsignRef;
			dossier.BillOfLading = dossier.IsImport ? shipment.JS_HouseBill : ZString.Empty;

			if (consol != null)
			{
				dossier.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List) { Code = consol.JK_ConsolMode };
				dossier.ShipmentType = new CodeDescription(consol.JK_AgentType_List) { Code = consol.JK_AgentType };
			}

			dossier.ImplicitAcknowledgement = true;
			dossier.ImplicitBAET = true;

			var operationalPort = dossier.IsImport
							? shipment.ImportReleaseDepot?.RelatedPortCode
							: shipment.ExportReceivingDepot?.RelatedPortCode;

			dossier.PCS = operationalPort?.GetPCS() ?? ZString.Empty;
			dossier.OperationalPort = Unloco.Create(context, operationalPort);

			dossier.ConfirmationReference = GetConfirmationReferenceFromEvents();
			dossier.ECVReference = GetECVReference();
			dossier.AgentReference = dossier.IsImport ? shipment.JS_UniqueConsignRef : GetAgentReference();

			PopulateParties(dossier);

			AddValidations(dossier);
			dossier.ValidateAllIncludingChildren();

			return dossier;
		}

		ZString GetECVReference()
		{
			var notSendAgentRefs = GetNotSendAgentRefs();

			IEnumerable<ZString> notSendECVRefs = null;
			if (notSendAgentRefs.Any())
			{
				notSendECVRefs = shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(packLine => packLine.AdditionalReferenceNumbers
								.Cast<CusEntryNumber>()
								.Any(e => e.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC && notSendAgentRefs.Contains(e.CE_EntryNum)))
						.Select(p => p.JL_ExportRefNumber)
						.Where(x => !x.IsEmpty)
						.Distinct();
			}
			else if (MessageExtensions.HasSentDocument(shipment, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA))
			{
				notSendECVRefs = shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ExportRefNumber).Where(x => !x.IsEmpty).Distinct();
			}

			return notSendECVRefs == null ? ZString.Empty : (ZString)string.Join(", ", notSendECVRefs);
		}

		ZString GetAgentReference()
		{
			return MessageExtensions.HasSentDocument(shipment, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA)
				? shipment.JS_UniqueConsignRef
				: (ZString)string.Join(", ", GetNotSendAgentRefs());
		}

		List<ZString> GetNotSendAgentRefs()
		{
			if (parameters?.Data is List<ZString> notSendAgentRefs)
			{
				return notSendAgentRefs;
			}

			return new List<ZString>();
		}

		ForwardingConsol GetConsolFromFrance()
		{
			if (string.Compare(parameters.DataStoreName, ShipmentDocumentDataStoreNames.DossierImport, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.Transports
					.Cast<Freight.Business.Transport>()
					.Any(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea
							  && Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKDiscPort.SubstringSafe(0, 2))));
			}

			return shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.Transports
				.Cast<Freight.Business.Transport>()
				.Any(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea
						  && Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKLoadPort.SubstringSafe(0, 2))));
		}

		#region PopulateParties

		void PopulateParties(Dossier dossier)
		{
			if (consol != null)
			{
				dossier.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
				dossier.CarrierSON = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
				dossier.CarrierCI5 = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

				dossier.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderAddress);
				dossier.SendingForwarderSON = consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
				dossier.SendingForwarderCI5 = consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

				dossier.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderAddress);
				dossier.ReceivingForwarderSON = consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
				dossier.ReceivingForwarderCI5 = consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);

				dossier.ThirdParty = dossier.IsImport ? dossier.ReceivingForwarder : dossier.SendingForwarder;
				dossier.ThirdPartySON = dossier.IsImport
					? consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort)
					: consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort);
				dossier.ThirdPartyCI5 = dossier.IsImport
					? consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort)
					: consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort);
			}
			else
			{
				dossier.ThirdParty = AddressBuilder.Create(context, dossier.IsImport ? shipment.DeliveryAgent?.MainAddress : shipment.PickupAgent?.MainAddress);
				var deliveryAgentAddress = shipment.DeliveryAgent?.MainAddress;
				var pickupAgentAddress = shipment.PickupAgentDocumentaryAddress?.Address;

				dossier.ThirdPartySON = dossier.IsImport
					? deliveryAgentAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort)
					: pickupAgentAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort);

				dossier.ThirdPartyCI5 = dossier.IsImport
					? deliveryAgentAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort)
					: pickupAgentAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort);
			}
			dossier.FormattedThirdPartyProviderID = FRPortHelpers.CreateFormattedProviderID(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, dossier.ThirdPartySON.ValueInfo, dossier.ThirdPartyCI5.ValueInfo);

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress;
			if (proxyMainAddress == null || proxyMainAddress.Address1.IsEmpty)
			{
				proxyMainAddress = GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			}

			dossier.SendingParty = AddressBuilder.CreateForCurrentUser(context);
			dossier.SendingPartySON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, dossier.OperationalPort);
			dossier.SendingPartyCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, dossier.OperationalPort);
			dossier.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, dossier.SendingPartySON.ValueInfo, dossier.SendingPartyCI5.ValueInfo);
			dossier.Agent = AddressBuilder.CreateForCurrentUser(context);
			dossier.AgentSOA = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA, false, dossier.OperationalPort);
			dossier.AgentCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, false, dossier.OperationalPort);
			dossier.FormattedAgentProviderID = FRPortHelpers.CreateFormattedProviderID(dossier.PCS, OrgCusCode.FranceCodeTypes.SOA, dossier.AgentSOA.ValueInfo, dossier.AgentCI5.ValueInfo);
		}

		#endregion

		#region ConfirmationReference

		string GetConfirmationReferenceFromEvents()
		{
			foreach (var log in GetEventLogsInDescendingOrder())
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

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			foreach (var log in shipment.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);
				var documentName = string.Compare(parameters.DataStoreName, ShipmentDocumentDataStoreNames.DossierImport, System.StringComparison.OrdinalIgnoreCase) == 0
					? FrenchPortsConstants.DocumentNames.DOSImport
					: FrenchPortsConstants.DocumentNames.DOSExport;

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
				dossier.BillOfLadingInfo.AddMessageErrorIfEmpty(Res.GetString("9cbea4a7-4e55-4281-ae0d-924d7ce9c968", "House Bill of Lading (HBL) Number is required."));
			}

			dossier.ECVReferenceInfo.AddMessageError(() => !dossier.IsImport && dossier.ECVReference.IsEmpty, Res.GetString("DA0B6DAC-1BEF-44E7-B31B-067CBF9E0FE1", "ECV Ref is required when dossier's status is not imported.\r\nExport Ref Number missing from Shipment > Packing > Pack Line."));

			dossier.AgentReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("47D65AF8-FF02-448A-838F-D6BCC3D8D419", "Agent Reference is required."));
			dossier.PCSInfo.AddPCSValidation();
			((Unloco)dossier.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(dossier.IsImport ?
																																					Res.GetString("993eb899-e611-49b1-a864-10c9c61c0453", "Operational Port is required, UNLOCO missing from Shipment > Delivery > CFS.") :
																																					Res.GetString("0a9a0b7d-e029-47e5-bfcf-93bf011c4b29", "Operational Port is required, UNLOCO missing from Shipment > Pickup > CFS."));

			var sendingPartyName = Res.GetString("86afdb6d-af04-431b-a801-005c6edac23a", "Sending Party");
			var sendingPartyCodeType = Res.GetString("0c32dbaf-390d-4b29-be26-37cfa05b88d8", "Forwarder");
			var sendingPartyConfigPath = Res.GetString("d588267f-bf20-42f0-ba05-d9ebc2fc293b", "Org. Proxy");

			var agentPartyName = Res.GetString("fa1e713c-a869-4e05-b4c7-409afab81c93", "Agent");

			var thirdPartyName = Res.GetString("2c460b02-4899-473a-9787-66419fb3ac81", "Third Party");
			var extraThirdPartyConfigPath = dossier.IsImport ? Res.GetString("C3009315-FB9A-480E-8E5E-5232D54DEEF6", "Consol > Receiving Agent") : Res.GetString("6FAC402F-F8C6-4422-965D-5277CEB7B118", "Consol > Sending Agent");
			var thirdPartyConfigPath = dossier.IsImport ? Res.GetString("01450ea3-0e96-4fa8-ac23-c01c741d345b", "Shipment > Delivery > Delivery Agent") : Res.GetString("53054438-6a9b-4d77-87ce-157203da11b3", "Shipment > Pickup > Pickup Agent");

			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");

			dossier.SendingParty.AddPartyNameAndAddressValidation(Res.GetString("8160b0c9-837f-49d1-969c-a1f434af2659", "Sending Party"), messageValidation: messageValidation);
			dossier.Agent.AddPartyNameAndAddressValidation(Res.GetString("2D5010B8-BC95-4310-A503-1FD3D6ED6855", "Agent"), messageValidation: messageValidation);
			dossier.ThirdParty.AddPartyNameAndAddressValidation(Res.GetString("cd7c7298-ee03-4d33-898b-f18cdb3f4fff", "Third Party"), messageValidation: messageValidation);

			dossier.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, sendingPartyName, sendingPartyCodeType, sendingPartyConfigPath);
			dossier.FormattedAgentProviderIDInfo.AddFormattedProviderIDValidations(dossier.PCS, OrgCusCode.FranceCodeTypes.SOA, agentPartyName, agentPartyName, sendingPartyConfigPath);
			dossier.FormattedThirdPartyProviderIDInfo.AddFormattedProviderIDValidations(dossier.PCS, OrgCusCode.FranceCodeTypes.SON, thirdPartyName, sendingPartyCodeType, thirdPartyConfigPath, extraConfigPath: extraThirdPartyConfigPath);

			dossier.ThirdPartyReferenceInfo.AddMessageError(() => dossier.ThirdPartyReference.Length > 17, Res.GetString("bde2bf0d-14fd-461e-88b3-8e1f2f676400", "Shipment Number (which is used as a Reference for this message) is too long.  Maximum 17 characters are allowed."));
			dossier.NotesInfo.AddMessageError(() => dossier.Notes.Length > 210, Res.GetString("05d2a9a3-c1ed-4bd4-90b9-656abab265ff", "Notes field value is too long.  Maximum 210 characters allowed."));
		}

		#endregion
	}
}
