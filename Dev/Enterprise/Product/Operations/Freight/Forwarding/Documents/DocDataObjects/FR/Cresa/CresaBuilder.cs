using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using FRPortHelpers = Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Helpers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CresaBuilder
	{
		public CresaBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
			this.isStandaloneShipment = !this.shipment.Consols.Any();
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;
		readonly bool isStandaloneShipment;

		public Cresa Build()
		{
			var cresa = new Cresa(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef);

			cresa.PCS = shipment.ExportReceivingDepot?.RelatedPortCode?.GetPCS() ?? ZString.Empty;
			cresa.OperationalPort = Unloco.Create(context, shipment.ExportReceivingDepot?.RelatedPortCode);

			PopulateEventReferences(cresa);

			PopulateGeneralFields(cresa);
			PopulateParties(cresa);
			PopulateTransportDetails(cresa);
			PopulatePortLocation(cresa);
			PopulatePortArea(cresa);
			PopulatePortServiceRef(cresa);
			PopulateCarrierBookingReference(cresa);

			PopulateShipmentDetails(cresa);

			PopulateGoodsPackLines(cresa);
			PopulateGoodsInDateTime(cresa);
			PopulateGoodsDescription(cresa);
			PopulateTotals(cresa);

			PopulateCargoReceiptDate(cresa);
			PopulateGoodsReceiptNotes(cresa);

			PopulateDataObjectWriterFields(cresa);

			AddValidations(cresa);
			cresa.ValidateAllIncludingChildren();

			return cresa;
		}

		void PopulateEventReferences(Cresa cresa)
		{
			var ecvReference = shipment.Numbers?.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, Constants.CountryCodes.France);
			if (ecvReference != null)
			{
				cresa.ECVReference = ecvReference.CE_EntryNum;
			}
			cresa.CRESAReference = GetParameterFromMAAEventReference(EventReferenceParameters.Codes.ReferenceNumber);
		}

		public string GetParameterFromMAAEventReference(String referenceType)
		{
			foreach (var log in GetCRESAEventLogsInDescendingOrder())
			{
				if (log.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode)
				{
					return "";
				}
				else
				{
					if (log.SL_SE_NKEvent == Events.MessageAcceptedCode)
					{
						if (log.Parameters.TryGetValue(referenceType, out var reference))
						{
							return reference;
						}
					}
				}
			}
			return "";
		}

		IEnumerable<StmALog> GetCRESAEventLogsInDescendingOrder()
		{
			foreach (var log in shipment.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(EventReferenceParameters.Codes.MessageType);
				var logMessageType = FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA;

				if (string.Compare(messageType, logMessageType, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		void PopulateGeneralFields(Cresa cresa)
		{
			cresa.EntryNumber = shipment.JS_UniqueConsignRef;
			cresa.CommodityReference = shipment.JS_UniqueConsignRef;
		}

		#region Header Details

		void PopulateParties(Cresa cresa)
		{
			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress;
			if (proxyMainAddress == null || proxyMainAddress.Address1.IsEmpty)
			{
				proxyMainAddress = GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			}

			var sendingParty = shipment.ExportReceivingDepot;
			cresa.SendingParty = AddressBuilder.Create(context, sendingParty);
			cresa.SendingPartySON = sendingParty.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.SendingPartySOA = sendingParty.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			cresa.SendingPartySOW = sendingParty.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			cresa.SendingPartyCI5 = sendingParty.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedSendingPartyProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SOW, cresa.SendingPartySOW.ValueInfo, cresa.SendingPartyCI5.ValueInfo);

			cresa.Buyer = AddressBuilder.Create(context, shipment.ConsigneeDocumentaryAddress);
			cresa.BuyerSON = shipment.ConsigneeDocumentaryAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.BuyerCI5 = shipment.ConsigneeDocumentaryAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedBuyerProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, cresa.BuyerSON.ValueInfo, cresa.BuyerCI5.ValueInfo);

			cresa.Supplier = AddressBuilder.Create(context, shipment.ConsignorDocumentaryAddress);
			cresa.SupplierSON = shipment.ConsignorDocumentaryAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.SupplierCI5 = shipment.ConsignorDocumentaryAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedSupplierProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, cresa.SupplierSON.ValueInfo, cresa.SupplierCI5.ValueInfo);

			cresa.SendingForwarder = AddressBuilder.CreateForCurrentUser(context);
			cresa.SendingForwarderSON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON, true, cresa.OperationalPort);
			cresa.SendingForwarderSOA = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			cresa.SendingForwarderSOW = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			cresa.SendingForwarderCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, true, cresa.OperationalPort);
			cresa.FormattedSendingForwarderProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, cresa.SendingForwarderSON.ValueInfo, cresa.SendingForwarderCI5.ValueInfo);

			var agent = shipment.PickupAgentDocumentaryAddress;
			cresa.Agent = AddressBuilder.Create(context, agent);
			cresa.AgentSON = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.AgentSOA = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA, false, cresa.OperationalPort);
			cresa.AgentSOW = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			cresa.AgentCI5 = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5, false, cresa.OperationalPort);
			cresa.FormattedAgentProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SOA, cresa.AgentSOA.ValueInfo, cresa.AgentCI5.ValueInfo);

			cresa.Transporter = AddressBuilder.Create(context, shipment.DocsAndCartage?.PickupCartageCoAddr);
			cresa.TransporterSON = shipment.DocsAndCartage?.PickupCartageCoAddr?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.TransporterCI5 = shipment.DocsAndCartage?.PickupCartageCoAddr?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
		}

		void PopulateTransportDetails(Cresa cresa)
		{
			if (isStandaloneShipment)
			{
				cresa.PortOfArrival = Unloco.Create(context, shipment.Destination);
				cresa.PortOfTranshipment = Unloco.Create(context, shipment.DischargePort);
				if (cresa.PortOfTranshipment.IsEmpty())
				{
					cresa.PortOfTranshipment = Unloco.Create(context, shipment.Destination);
				}

				cresa.ETA = shipment.JS_E_ARV.IsEmpty ? ZDateTime.Now : shipment.JS_E_ARV;
			}
			else
			{
				var firstSeaLeg = TransportsInLegOrder
					.OfType<Freight.Business.Transport>()
					.FirstOrDefault(t => t.TransportMode == Core.Constants.TransportModes.Sea && t.JW_RL_NKLoadPort.Left(2) != t.JW_RL_NKDiscPort.Left(2));
				var portOfTranshipment = Unloco.Create(context, firstSeaLeg?.DiscPort);
				cresa.PortOfTranshipment = portOfTranshipment;

				var lastSeaLeg = TransportsInLegOrder
					.OfType<Freight.Business.Transport>()
					.LastOrDefault(t => t.TransportMode == Core.Constants.TransportModes.Sea);
				var portOfArrival = Unloco.Create(context, lastSeaLeg?.DiscPort);
				cresa.PortOfArrival = portOfArrival;

				cresa.ETA = lastSeaLeg == null || lastSeaLeg.JW_ETA.IsEmpty ? ZDateTime.Now : lastSeaLeg.JW_ETA;
			}

			cresa.TransportMode = "RTE";
		}

		public ForwardingTransportCollection TransportsInLegOrder
		{
			get
			{
				if (transportLegs == null)
				{
					transportLegs = new ForwardingTransportCollection(shipment.Factory);

					foreach (ForwardingConsol consol in shipment.Consols)
					{
						foreach (Freight.Business.Transport transport in consol.Transports)
						{
							transportLegs.Add(transport);
						}
					}

					transportLegs.Sort(MovementLegComparer.PortsAndDatesBased(transportLegs));
				}
				return transportLegs;
			}
		}

		ForwardingTransportCollection transportLegs;

		void PopulatePortArea(Cresa cresa)
		{
			cresa.PortArea = shipment.ExportReceivingDepot.GetPortArea();
		}

		void PopulatePortLocation(Cresa cresa)
		{
			cresa.PortLocation = shipment.ExportReceivingDepot.GetPortLocation();
		}

		void PopulatePortServiceRef(Cresa cresa)
		{
			cresa.PortServiceCodeReference = shipment.ExportReceivingDepot.GetRegistrationNumberWithFallback(context, OrgCusCode.CodeTypes.PortServiceReference).Value;
		}

		#endregion

		void PopulateCarrierBookingReference(Cresa cresa)
		{
			cresa.CarrierBookingReference = shipment.JS_UniqueConsignRef;
		}

		void PopulateShipmentDetails(Cresa cresa)
		{
			cresa.ShipmentNumber = shipment.JS_UniqueConsignRef;

			cresa.ShipmentType = new CodeDescription(shipment.Lookups.JS_ShipmentType_List)
			{
				Code = shipment.JS_ShipmentType
			};
		}

		#region Goods Details

		void PopulateGoodsInDateTime(Cresa cresa)
		{
			cresa.GoodsInDateTime = ZDateTime.Now;
		}

		void PopulateTotals(Cresa cresa)
		{
			cresa.PackType = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = shipment.JS_F3_NKPackType
			};

			var unitOfWeight = Constants.Weight.Kilograms;
			var unitOfVolume = Constants.Volume.CubicMetres;

			cresa.TotalPackCount = cresa.PackingLines.Sum(x => x.Quantity);

			cresa.TotalWeight = new Measurement
			{
				Value = Decimal.Round(cresa.PackingLines.Sum(x => x.Weight.Value), MidpointRounding.AwayFromZero),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			cresa.TotalVolume = new Measurement
			{
				Value = cresa.PackingLines.Sum(x => x.Volume.Value),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};
		}

		void PopulateGoodsDescription(Cresa cresa)
		{
			cresa.GoodsDescription = shipment.JS_GoodsDescription;

			if (cresa.GoodsDescription.IsEmpty)
			{
				cresa.GoodsDescription = shipment.DetailedGoodsDescriptionNoteText;
			}
		}

		void PopulateGoodsPackLines(Cresa cresa)
		{
			cresa.PackingLines = GetPackingLines(shipment).ToArray();
		}

		IEnumerable<BookingPackingLine> GetPackingLines(ForwardingShipment shipmentBO)
		{
			var packLineBuilder = new BookingPackingLineBuilder();
			return shipmentBO.OuterPackLines.Cast<PackLine>().Where(x => !x.JL_LastKnownTransitWarehouseStatus.IsEmpty).Select(x => packLineBuilder.Build(x));
		}

		#endregion

		void PopulateCargoReceiptDate(Cresa cresa)
		{
			cresa.CargoReceiptDate = ZDateTime.Now;
		}

		void PopulateGoodsReceiptNotes(Cresa cresa)
		{
			cresa.GoodsReceiptNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description)?.FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;
		}

		void PopulateDataObjectWriterFields(Cresa cresa)
		{
			cresa.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			cresa.ContainerMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = shipment.JS_PackingMode
			};
		}

		#region Validations

		void AddValidations(Cresa cresa)
		{
			AddGeneralFieldsValidations(cresa);
			AddAddressValidations(cresa);
			AddPortLocationValidations(cresa);
			AddDataObjectWriterFieldsValidation(cresa);
			AddHeaderDetailValidations(cresa);
			AddCargoReceiptDateValidation(cresa);
			AddGoodsDetailValidations(cresa);
			AddEventReferenceValidations(cresa);
		}

		void AddGeneralFieldsValidations(Cresa cresa)
		{
			cresa.CommodityReferenceInfo.AddMessageError(() => cresa.CommodityReference.Length > 17, Res.GetString("D8A0A9DF-1823-4AAC-9ECE-2A4D9D47EFB3", "Commodity Reference is too long. Maximum 17 characters allowed."));
		}

		void AddAddressValidations(Cresa cresa)
		{
			var sendingPartyName = Res.GetString("4EDE4E26-B06B-447D-B489-46CAC41BECC3", "Sending Party");
			var sendingPartyConfigPath = Res.GetString("0c870073-22c5-4483-8668-34a099dce895", "Shipment > Pickup > CFS");

			var forwarderName = Res.GetString("3220D295-FADE-461B-BC73-EBD5327FDD5A", "Forwarder");
			var forwarderConfigPath = Res.GetString("3ed89eeb-db5e-4e8f-9627-4b7eab718c0d", "Org. Proxy");

			var agentName = Res.GetString("E00D5D88-A44F-430C-9FA1-62C6C1EE369D", "Agent");
			var agentConfigPath = Res.GetString("d5676913-89b5-4bc9-a763-de12087edccf", "Shipment > Pickup > Pickup Agent");

			var messageValidation = Res.GetString("c9981c74-475f-4988-aa4c-83cb025fb0aa", "name and address is required.");

			cresa.SendingParty.AddPartyNameAndAddressValidation(sendingPartyName, messageValidation: messageValidation);
			cresa.SendingForwarder.AddPartyNameAndAddressValidation(forwarderName, messageValidation: messageValidation);
			cresa.Agent.AddPartyNameAndAddressValidation(agentName, messageValidation: messageValidation);

			cresa.FormattedSendingPartyProviderIDInfo.AddFormattedProviderIDValidations(cresa.PCS, OrgCusCode.FranceCodeTypes.SOW, sendingPartyName, configPath: sendingPartyConfigPath, isOrganization: true);
			cresa.FormattedSendingForwarderProviderIDInfo.AddFormattedProviderIDValidations(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, forwarderName, forwarderName, forwarderConfigPath);
			cresa.FormattedAgentProviderIDInfo.AddFormattedProviderIDValidations(cresa.PCS, OrgCusCode.FranceCodeTypes.SOA, agentName, agentName, agentConfigPath);
		}

		void AddPortLocationValidations(Cresa cresa)
		{
			cresa.PortLocationInfo.AddMessageErrorIfEmpty(Res.GetString("6E0B1C31-D9EA-45DA-9A3A-46E726A6BB35", "Port Location is missing from Organization Shipment > Pickup > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')."));
			cresa.PortLocationInfo.AddAsciiCharactersValidation();
		}

		void AddDataObjectWriterFieldsValidation(Cresa cresa)
		{
			cresa.PCSInfo.AddPCSValidation();
			((Unloco)cresa.OperationalPort)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("af8f0358-6a30-42d8-987f-13d8ed9d704e", "Operational Port is required, UNLOCO missing from Shipment > Pickup > CFS."));
		}

		void AddHeaderDetailValidations(Cresa cresa)
		{
			cresa.TransportModeInfo.AddMessageError(() => cresa.TransportMode.Length > 3, Res.GetString("35E1F9A3-B66B-45D0-8E55-44DA324AB764", "Transport Mode must not exceed three (3) characters."));
			cresa.TransportModeInfo.AddMessageErrorIfEmpty(Res.GetString("0AA857DA-30E3-431B-9B51-290D739DE668", "Transport mode is required."));

			((Unloco)cresa.PortOfArrival).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("817A0DC3-A6E9-4C4D-8C47-39605B2C47AD", "Port of Arrival is required."));
			((Unloco)cresa.PortOfArrival).CodeInfo.AddAsciiCharactersValidation();
			((Unloco)cresa.PortOfTranshipment).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("6D5138D2-F1A4-4293-BCE3-54CA1AEC71FB", "Port of Transhipment is required."));
			((Unloco)cresa.PortOfTranshipment).CodeInfo.AddAsciiCharactersValidation();

			cresa.PortAreaInfo.AddMessageErrorIfEmpty(Res.GetString("5EFED7B9-5FC4-4BC4-AA08-40F8E9AF6532", "Port Area is missing from Organization Shipment > Pickup > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')."));
			cresa.PortAreaInfo.AddAsciiCharactersValidation();

			cresa.PortServiceCodeReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("11084AB1-B5E8-4A27-BF87-07B8E1E1BC65", "Port Service Reference is missing from Organization Shipment > Pickup > CFS > Config > Registration Numbers/Codes [Type=PSR]."));

			cresa.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("840402EB-FED4-4759-B57A-397DF2762BFD", "Booking Reference is required."));
		}

		void AddCargoReceiptDateValidation(Cresa cresa)
		{
			cresa.CargoReceiptDateInfo.AddMessageErrorIfEmpty(Res.GetString("5ca62539-0589-41aa-4147-8f7bf34ddb64", "Cargo Receipt Date is required."));
		}

		void AddGoodsDetailValidations(Cresa cresa)
		{
			cresa.ShipmentNumberInfo.AddMessageError(() => cresa.ShipmentNumber.Length > 17, Res.GetString("B2631B34-0BAB-44F4-A3B6-76C4E62536E5", "Shipment Number (which is used as a Document Reference for this message) is too long.  Maximum 17 characters are allowed."));
			cresa.CommodityReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("031D3498-0736-4DF8-8E53-9D0EDCAA6CD8", "Commodity Reference is required."));
			cresa.GoodsInDateTimeInfo.AddMessageErrorIfEmpty(Res.GetString("FC9D3968-B097-4544-B9C7-2A7EA8ACC09D", "Date/Time goods arrived at the warehouse is required"));
			cresa.TotalPackCountInfo.AddMessageError(() => cresa.TotalPackCount <= 0, Res.GetString("69e12b38-b2ca-4e2b-9bf6-54b5dc8b6273", "Total number of packs is required."));
			((CodeDescription)cresa.PackType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("7E386470-3AE4-42DF-BE50-9FD37FCAC925", "Pack Type is required."));
			((Measurement)cresa.TotalWeight).ValueInfo.AddMessageError(() => cresa.TotalWeight.Value <= 0, Res.GetString("82CCCB90-6339-4D98-9A26-A5D66945FCC2", "Weight cannot be zero.\r\nIt is either entered as zero or rounded to the nearest kilogram value to zero. The CI5 and S)One systems supports only integer value of weight. Please review the pack line weight and input a valid weight."));
			((Measurement)cresa.TotalVolume).ValueInfo.AddMessageError(() => cresa.TotalVolume.Value <= 0, Res.GetString("74524F59-F6AE-4206-9A12-675F7EA51539", "Total Volume is required."));
			cresa.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("73E78B05-D378-4752-89DD-C1B89BB20BE7", "Goods Description is required"));

			foreach (BookingPackingLine packingLine in cresa.PackingLines)
			{
				AddPackingLineValidation(packingLine);
			}
			cresa.ErrorPlaceHolderInfo.AddMessageError(() => !cresa.PackingLines.Any(), Res.GetString("4A118305-494C-40CC-9900-88A9951052A1", "There are no pack lines in the shipment or pack lines in the shipment are without the Last Know Transit Warehouse Status value. The Goods Received (CRESA) message cannot be sent."));

			cresa.ErrorPlaceHolderInfo.AddWarning(() => shipment.OuterPackLines.Cast<PackLine>().ToArray().Length > cresa.PackingLines.Count, Res.GetString("FFE8F2BF-4206-4B48-9B44-5505D258DFD9", "There are pack lines in the shipment without the Last Know Transit Warehouse Status value.  These pack lines will not be sent."));
		}

		void AddPackingLineValidation(BookingPackingLine packingLine)
		{
			packingLine.PackingLineIDInfo.AddMessageErrorIfEmpty(Res.GetString("203CF337-7E0A-4C6B-AA45-109B208F6CE2", "Pack Line ID is required."));
			packingLine.PackingLineIDInfo.AddMessageError(() => packingLine.PackingLineID.Length > 17, Res.GetString("e85eb7d7-781b-4a2d-bd02-3927b07ec7e3", "Packing Line ID is too long. Maximum 17 characters allowed."));
			packingLine.ExportReferenceNumberInfo.AddMessageErrorIfEmpty(Res.GetString("149562AC-F269-4575-83B8-C8E2E725DA21", "ECV Reference is required. Export Ref Number missing from Shipment > Packing > Pack Line."));
			packingLine.QuantityInfo.AddMessageError(() => (packingLine.Quantity.IsEmpty || packingLine.Quantity <= 0), Res.GetString("F0C1C3AF-45AB-4A40-A3C3-AA1904C75E3B", "Number of Packs is required."));
			((CodeDescription)packingLine.PackageType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("BCB09F0B-9043-4212-94FE-0CDA3BDACBE1", "Package Type is required."));
			packingLine.Weight.ValueInfo.AddMessageError(() => (packingLine.Weight.IsNull || packingLine.Weight.Value <= 0), Res.GetString("FACD4509-E124-4F4D-8FDA-DED1B46C0707", "Weight is required."));
			packingLine.Volume.ValueInfo.AddMessageError(() => (packingLine.Volume.IsNull || packingLine.Volume.Value <= 0), Res.GetString("197BA519-EF0F-42E5-B0C3-080A7093FC2B", "Volume is required."));
		}

		void AddEventReferenceValidations(Cresa cresa)
		{
			bool shouldReferencesBeFilledIn()
			{
				foreach (var log in GetCRESAEventLogsInDescendingOrder())
				{
					switch (log.SL_SE_NKEvent)
					{
						case Events.MessageWithdrawCancelAcceptedCode:
							return false;
						case Events.MessageAcceptedCode:
							return true;
					}
				}

				return false;
			}

			if (shouldReferencesBeFilledIn())
			{
				cresa.ECVReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("1E70BEAD-E9CB-4768-8E0C-308ED0D7AA02", "ECV Reference is required when sending an amendment or withdrawal. Export Ref Number missing from Shipment > Packing > Pack Line."));
				cresa.CRESAReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("C1FAACB6-BEAE-4FFE-8164-0E6F653899B0", "CRESA Reference is required when sending an amendment or withdrawal. Goods Received (CRESA) original message should be accepted by the the Port Community System before sending an amendment or withdrawal."));
			}
		}

		#endregion
	}
}
