using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;
using FRPortHelpers = Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Helpers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	abstract class CresaBuilderForTransitWarehouse<TSource> where TSource : EnterpriseBusinessObject, IConsignment
	{
		public CresaBuilderForTransitWarehouse(TSource consignment)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			this.context = new CommonContext(consignment.Factory.GetCachedReadOnlyFactory());
		}

		protected readonly TSource consignment;
		readonly IContext context;

		protected abstract Cresa CreateCresaMessage();

		public Cresa Build()
		{
			var cresa = CreateCresaMessage();
			PopulateOperationalPort(cresa);

			PopulateECVAndCRESAReferences(cresa);

			PopulateReferences(cresa);
			PopulateParties(cresa);
			PopulateTransportDetails(cresa);
			PopulatePortLocation(cresa);
			PopulatePortArea(cresa);
			PopulatePortServiceRef(cresa);

			PopulateConsignmentDetails(cresa);

			PopulatePackLines(cresa);
			PopulateGoodsInDateTime(cresa);
			PopulateGoodsDescription(cresa);
			PopulateTotals(cresa);

			PopulateCargoReceiptDate(cresa);
			PopulateGoodsReceiptNotes(cresa);
			PopulateGoodsSealed(cresa);
			PopulateCurrentUser(cresa);

			PopulateNote(cresa);

			AddValidations(cresa);
			cresa.ValidateAllIncludingChildren();

			return cresa;
		}

		protected abstract IPortReferenceCollection PortReferences { get; }

		protected abstract string ExpectedPortReferenceType { get; }

		void PopulateECVAndCRESAReferences(Cresa cresa)
		{
			var ecvReference = PortReferences?.Cast<CusEntryNumber>().FirstOrDefault(e => e.CE_EntryType == ExpectedPortReferenceType);
			if (ecvReference != null)
			{
				cresa.ECVReference = ecvReference.CE_EntryNum;
				cresa.CRESAReference = ecvReference.CE_EntryLineReference;
			}
		}

		protected abstract void PopulateReferences(Cresa cresa);

		void PopulateParties(Cresa cresa)
		{
			var sendingParty = consignment.Warehouse.WarehouseAddress as OrgAddress;
			cresa.PCS = sendingParty.RelatedPortCode?.GetPCS() ?? ZString.Empty;
			cresa.SendingParty = AddressBuilder.Create(context, sendingParty);
			cresa.SendingPartySOW = sendingParty.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			cresa.FormattedSendingPartyProviderID = cresa.PCS == FrenchPortsConstants.PCS.Soget && !cresa.SendingPartySOW.ValueInfo.Value.IsEmpty ? Res.GetString("dee1b753-14d3-4347-b5d0-6fd130c28191", "{0}: {1}", cresa.SendingPartySOW.ValueInfo, cresa.SendingPartySOW.ValueInfo.Value) : string.Empty;

			cresa.Buyer = AddressBuilder.Create(context, ConsigneeDocAddress);
			cresa.BuyerSON = ConsigneeDocAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.BuyerCI5 = ConsigneeDocAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedBuyerProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, cresa.BuyerSON.ValueInfo, cresa.BuyerCI5.ValueInfo);

			cresa.Supplier = AddressBuilder.Create(context, ConsignorDocAddress);
			cresa.SupplierSON = ConsignorDocAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.SupplierCI5 = ConsignorDocAddress.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedSupplierProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, cresa.SupplierSON.ValueInfo, cresa.SupplierCI5.ValueInfo);

			var sendingForwarder = consignment.BookingPartyDocAddress as JobDocAddress;
			cresa.SendingForwarder = AddressBuilder.Create(context, sendingForwarder);
			cresa.SendingForwarderSON = sendingForwarder.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.SendingForwarderSOA = sendingForwarder.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			cresa.SendingForwarderSOW = sendingForwarder.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			cresa.SendingForwarderCI5 = sendingForwarder.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedSendingForwarderProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SON, cresa.SendingForwarderSON.ValueInfo, cresa.SendingForwarderCI5.ValueInfo);

			var agent = consignment.BookingPartyDocAddress as JobDocAddress;
			cresa.Agent = AddressBuilder.Create(context, agent);
			cresa.AgentSON = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			cresa.AgentSOA = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOA);
			cresa.AgentSOW = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SOW);
			cresa.AgentCI5 = agent.Address.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
			cresa.FormattedAgentProviderID = FRPortHelpers.CreateFormattedProviderID(cresa.PCS, OrgCusCode.FranceCodeTypes.SOA, cresa.AgentSOA.ValueInfo, cresa.AgentCI5.ValueInfo);
		}

		protected abstract JobDocAddress ConsigneeDocAddress { get; }
		protected abstract JobDocAddress ConsignorDocAddress { get; }

		void PopulateTransportDetails(Cresa cresa)
		{
			cresa.PortOfArrival = Unloco.Create(context, NextDischargePort);
			cresa.PortOfTranshipment = Unloco.Create(context, NextDischargePort);
			cresa.TransportMode = "RTE";
			cresa.ETA = GetETA();
		}

		protected abstract IRefUNLOCO NextDischargePort { get; }

		protected abstract ZDateTime GetETA();

		void PopulatePortLocation(Cresa cresa)
		{
			cresa.PortLocation = (consignment.BookingPartyDocAddress as JobDocAddress).Address.GetPortLocation();
		}

		void PopulatePortArea(Cresa cresa)
		{
			cresa.PortArea = (consignment.BookingPartyDocAddress as JobDocAddress).Address.GetPortArea();
		}

		void PopulatePortServiceRef(Cresa cresa)
		{
			cresa.PortServiceCodeReference = (consignment.BookingPartyDocAddress as JobDocAddress).Address.GetRegistrationNumberWithFallback(context, OrgCusCode.CodeTypes.PortServiceReference).Value;
		}

		protected abstract void PopulateConsignmentDetails(Cresa cresa);

		void PopulatePackLines(Cresa cresa)
		{
			cresa.PackingLines = new List<BookingPackingLine>();
			cresa.PackingLines = GetPackingLines(OuterPackageStatesForBuildingPackingLines).ToArray();
		}

		protected abstract IEnumerable<WhsItemPackageState> OuterPackageStatesForBuildingPackingLines { get; }

		IEnumerable<BookingPackingLine> GetPackingLines(IEnumerable<WhsItemPackageState> packageStates)
		{
			var packLineBuilder = new BookingPackingLineBuilder();

			var ovps = packageStates.Where(p => p.WPS_UnitType == PackageStateUnitType.Codes.Overpack);
			foreach (var ovp in ovps)
			{
				if (ovp.WPS_UnitType == PackageStateUnitType.Codes.Overpack && ovp.Package.KP_GoodsDescription.IsEmpty)
				{
					var inners = ovp.Package.Packages;
					var fristGoodsDescription = inners.First().KP_GoodsDescription;
					if (!fristGoodsDescription.IsEmpty)
					{
						if (inners.All(inner => inner.KP_GoodsDescription == fristGoodsDescription))
						{
							ovp.Package.KP_GoodsDescription = fristGoodsDescription;
						}
					}
				}
			}

			return packageStates
				.Select(p => packLineBuilder.Build(p))
				.OrderBy(p => p.PackingLineID);
		}

		void PopulateGoodsInDateTime(Cresa cresa)
		{
			var unloadedTimes = PackageStates.Where(p => p.WPS_UnloadedTime.IsValid).Select(p => p.WPS_UnloadedTime).Distinct().ToArray();
			if (unloadedTimes.Any())
			{
				var unloadedTime = unloadedTimes.OrderByDescending(t => t).First();
				cresa.GoodsInDateTime = unloadedTime.ToZDateTime();
			}
		}

		void PopulateGoodsDescription(Cresa cresa)
		{
			var goodsDescriptions = cresa.PackingLines.Where(p => !string.IsNullOrEmpty(p.GoodsDescription.Trim())).Select(p => p.GoodsDescription.Trim()).Distinct().OrderBy(g => g).ToArray();
			cresa.GoodsDescription = ZString.Join(", ", goodsDescriptions);
		}

		void PopulateTotals(Cresa cresa)
		{
			if (cresa.PackingLines.Any())
			{
				cresa.PackType = cresa.PackingLines.First().PackageType;
				if (cresa.PackingLines.Select(p => p.PackageType.Code).Distinct().Count() > 1)
				{
					cresa.PackType.Code = "PKG";
				}

				var unitOfWeight = Core.Constants.Weight.Kilograms;
				var unitOfVolume = Core.Constants.Volume.CubicMetres;

				cresa.TotalPackCount = cresa.PackingLines.Sum(x => x.Quantity);

				cresa.TotalWeight = new Measurement
				{
					Value = decimal.Round(cresa.PackingLines.Sum(x => x.Weight.Value), MidpointRounding.AwayFromZero),
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
			else
			{
				cresa.PackType = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = ""
				};
			}
		}

		void PopulateCargoReceiptDate(Cresa cresa)
		{
			var gatedInTimes = PackageStates.Where(p => p.ReceiveTransportationUnit != null && p.ReceiveTransportationUnit.WRH_GateInTime.IsValid)
												.Select(p => p.ReceiveTransportationUnit.WRH_GateInTime).Distinct().ToArray();
			if (gatedInTimes.Any())
			{
				var gatedInTime = gatedInTimes.OrderByDescending(t => t).First();
				cresa.CargoReceiptDate = gatedInTime.ToZDateTime();
			}
		}

		void PopulateGoodsReceiptNotes(Cresa cresa)
		{
			cresa.GoodsReceiptNotes = consignment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description)?.FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;
		}

		void PopulateGoodsSealed(Cresa cresa)
		{
			var hasSealNumber = PackageStates.Any()
								&& PackageStates.All(p => p.WPS_UnloadedTime.IsValid && p.ReceiveTransportationUnit.HasContainerEquipmentDetails && p.ReceiveTransportationUnit.HasSealNumber);
			cresa.GoodsSealed = hasSealNumber;
		}

		protected abstract WhsItemPackageStateCollection PackageStates { get; }

		void PopulateOperationalPort(Cresa cresa)
		{
			cresa.OperationalPort = Unloco.Create(context, (consignment.Warehouse.WarehouseAddress as OrgAddress).RelatedPortCode);
		}

		void PopulateCurrentUser(Cresa cresa)
		{
			cresa.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
		}

		void PopulateNote(Cresa cresa)
		{
			var noteBuilder = new ZStringBuilder();

			var noteHelper = new CresaNoteColumnProvider();
			var noteHeader = noteHelper.GetTable(Res.GetString("141cde7d-4dc9-4b62-9a03-f65b186e4c21", "CRESA Message Details:"), new Cresa[] { cresa },
				TransitLogColumnIDs.CRESAColumn.OperationalPort,
				TransitLogColumnIDs.CRESAColumn.PCS,
				TransitLogColumnIDs.CRESAColumn.TransportMode,
				TransitLogColumnIDs.CRESAColumn.TranshipmentPort,
				TransitLogColumnIDs.CRESAColumn.PortOfArrival,
				TransitLogColumnIDs.CRESAColumn.PortArea,
				TransitLogColumnIDs.CRESAColumn.PortServiceReference,
				TransitLogColumnIDs.CRESAColumn.PortLocation,
				TransitLogColumnIDs.CRESAColumn.CargoReceiptDate,
				TransitLogColumnIDs.CRESAColumn.ETAatPortOfArrival);
			noteBuilder.AppendLine(noteHeader);

			var organizations = noteHelper.GetTable(Res.GetString("2dd0b87b-5ddf-4e8b-b66b-586841fe58d9", "Organization Details:"), new Cresa[] { cresa },
				TransitLogColumnIDs.CRESAColumn.Buyer,
				TransitLogColumnIDs.CRESAColumn.Supplier,
				TransitLogColumnIDs.CRESAColumn.SendingParty,
				TransitLogColumnIDs.CRESAColumn.Forwarder,
				TransitLogColumnIDs.CRESAColumn.Agent);
			noteBuilder.AppendLine(organizations);

			var additionalReferences = noteHelper.GetTable(Res.GetString("708c31e5-9e85-4e3f-bb81-a3e43e45ef65", "Additional References:"), new Cresa[] { cresa },
				TransitLogColumnIDs.CRESAColumn.BookingReference,
				TransitLogColumnIDs.CRESAColumn.WarehouseEntryNumber,
				TransitLogColumnIDs.CRESAColumn.ECVReference,
				TransitLogColumnIDs.CRESAColumn.CRESAReference);
			noteBuilder.AppendLine(additionalReferences);

			var packingLineNoteHelper = new CresaNotePackingLineColumnProvider();
			var packingLineNote = packingLineNoteHelper.GetTable(Res.GetString("26491cd1-4bad-49d7-a568-6eef3b8e7c23", "Goods Details:"), cresa.PackingLines,
				TransitLogColumnIDs.CRESABookingPackingLine.Packs,
				TransitLogColumnIDs.CRESABookingPackingLine.Weight,
				TransitLogColumnIDs.CRESABookingPackingLine.Volume,
				TransitLogColumnIDs.CRESABookingPackingLine.GoodsDescription);
			noteBuilder.AppendLine(packingLineNote);

			noteBuilder.AppendLine();
			cresa.NoteForTransitWarehouse = noteBuilder.ToString();
		}

		#region Validations

		void AddValidations(Cresa cresa)
		{
			AddReferenceValidations(cresa);
			AddPartyAddressValidations(cresa);
			AddPortLocationAndAreaValidations(cresa);
			AddOperationalPortValidation(cresa);
			AddHeaderDetailValidations(cresa);
			AddPortOfArrivalAndTranshipmentValidations(cresa);
			AddCargoReceiptDateValidation(cresa);
			AddGoodsDetailValidations(cresa);
			AddECVAndCRESAReferenceValidations(cresa);
		}

		void AddReferenceValidations(Cresa cresa)
		{
			if (ConsignmentIsRCN)
			{
				cresa.CommodityReferenceInfo.AddMessageError(() => cresa.CommodityReference.Length > 17, Res.GetString("a44d230e-d7ba-444f-a6cb-3d21ee17e560", "Commodity Reference (RCN ID) is too long. Maximum 17 characters allowed."));
				cresa.CommodityReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("f8c7b3a6-f46d-494d-b52d-62b27d63df65", "Commodity Reference (RCN ID) is required"));
				cresa.ShipmentNumberInfo.AddMessageError(() => cresa.ShipmentNumber.Length > 17, Res.GetString("829bcac3-85f6-4d05-87e8-e4799b8010c8", "Shipment Number (RCN ID) is too long. Maximum 17 characters allowed."));
			}
			else
			{
				cresa.CommodityReferenceInfo.AddMessageError(() => cresa.CommodityReference.Length > 17, Res.GetString("85eadeeb-2043-4839-9e44-0a9e99b428e3", "Commodity Reference (DCN ID) is too long. Maximum 17 characters allowed."));
				cresa.CommodityReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("5b652cb8-ce17-4047-9ffb-73dad0671f18", "Commodity Reference (DCN ID) is required"));
				cresa.ShipmentNumberInfo.AddMessageError(() => cresa.ShipmentNumber.Length > 17, Res.GetString("00e5c0ed-de40-4255-b36a-850c28a305a6", "Shipment Number (DCN ID) is too long. Maximum 17 characters allowed."));
			}
		}

		void AddPartyAddressValidations(Cresa cresa)
		{
			var sendingParty = Res.GetString("16e0182d-b68b-4668-8377-2ed01f1d8e73", "Sending Party");
			var forwarder = Res.GetString("8df43755-b5a6-47c0-8b45-c0a7f0059a24", "Forwarder");
			var agent = Res.GetString("54075c7a-4661-4b6e-8a12-e56cac21a9a0", "Agent");

			cresa.SendingParty.AddPartyNameAndAddressValidation(sendingParty);
			cresa.SendingForwarder.AddPartyNameAndAddressValidation(forwarder);
			cresa.Agent.AddPartyNameAndAddressValidation(agent);

			AddProviderIDValidations(sendingParty, OrgCusCode.FranceCodeTypes.SOW, cresa.SendingPartySOW.ValueInfo);
			AddProviderIDValidations(forwarder, OrgCusCode.FranceCodeTypes.SON, cresa.SendingForwarderSON.ValueInfo, cresa.SendingForwarderCI5.ValueInfo);
			AddProviderIDValidations(agent, OrgCusCode.FranceCodeTypes.SOA, cresa.AgentSOA.ValueInfo, cresa.AgentCI5.ValueInfo);
		}

		void AddProviderIDValidations(string partyName, string sPropertyType, ZPropertyInfo sPropertyInfo, ZPropertyInfo ci5PropertyInfo)
		{
			var propertyIsNullOrEmpty = ci5PropertyInfo.Value.IsEmpty && sPropertyInfo.Value.IsEmpty;
			sPropertyInfo.AddMessageError(() => propertyIsNullOrEmpty, Res.GetString("1D6C40F5-3D49-4424-82B8-2971F0B3B3EC", "{0} Provider ID is missing from this organization > Config > Registration Numbers/Codes - type {1}.", partyName, sPropertyType));
			ci5PropertyInfo.AddMessageError(() => propertyIsNullOrEmpty, Res.GetString("849FB2E0-F71F-4253-9D9C-A9F6340A1AD2", "{0} Provider ID is missing from this organization > Config > Registration Numbers/Codes - type CI5.", partyName));
		}

		void AddProviderIDValidations(string partyName, string sPropertyType, ZPropertyInfo sPropertyInfo)
		{
			sPropertyInfo.AddMessageError(() => sPropertyInfo.Value.IsEmpty, Res.GetString("973839CA-E55F-4A5B-9BEF-0A6293D3E3F5", "{0} Provider ID is missing from this organization > Config > Registration Numbers/Codes - type {1}.", partyName, sPropertyType));
		}

		void AddPortLocationAndAreaValidations(Cresa cresa)
		{
			if (ConsignmentIsRCN)
			{
				cresa.PortLocationInfo.AddMessageErrorIfEmpty(Res.GetString("f394bd2f-2bf9-4ff1-81f5-cdead01eaab7", "Port Location is missing from organization RCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code after '\\')"));
				cresa.PortLocationInfo.AddAsciiCharactersValidation();

				cresa.PortAreaInfo.AddMessageErrorIfEmpty(Res.GetString("7456168d-b1e0-479a-8cd8-8cb00c501be8", "Port Area is missing from organization RCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code before '\\')"));
				cresa.PortAreaInfo.AddAsciiCharactersValidation();
			}
			else
			{
				cresa.PortLocationInfo.AddMessageErrorIfEmpty(Res.GetString("1a96914a-717b-4dc1-b547-ddbfdf29323c", "Port Location is missing from organization DCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code after '\\')"));
				cresa.PortLocationInfo.AddAsciiCharactersValidation();

				cresa.PortAreaInfo.AddMessageErrorIfEmpty(Res.GetString("96b35443-269e-4bf9-8e52-73beaf22378b", "Port Area is missing from organization DCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code before '\\')"));
				cresa.PortAreaInfo.AddAsciiCharactersValidation();
			}
		}

		void AddOperationalPortValidation(Cresa cresa)
		{
			cresa.ErrorPlaceHolderInfo.AddMessageError(() => cresa.OperationalPort.IsEmpty(), Res.GetString("e931f657-f5a9-45e3-b043-3c6a2a6062cd", "Operational port is required."));
		}

		void AddHeaderDetailValidations(Cresa cresa)
		{
			cresa.TransportModeInfo.AddMessageError(() => cresa.TransportMode.Length > 3, Res.GetString("b70ef730-3095-4761-b4a1-e2aa20896fb7", "Mode has maximum three characters."));
			cresa.TransportModeInfo.AddMessageErrorIfEmpty(Res.GetString("895721b6-0774-4f82-b739-9216d4444356", "Transport mode is required."));

			if (ConsignmentIsRCN)
			{
				cresa.PortServiceCodeReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("78a18ac8-f25c-41c5-9759-1893fe8b6e3a", "Port Service Reference is missing from RCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSR."));
				cresa.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("48025c38-669e-48b5-a52e-0e30b8d03227", "Booking Reference (RCN ID) is required."));
			}
			else
			{
				cresa.PortServiceCodeReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("76e27166-ae5c-47e3-b5c1-d04940d4bd4a", "Port Service Reference is missing from DCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSR."));
				cresa.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("08843c6e-6caa-45e4-befb-a95225a3b7a7", "Booking Reference (DCN ID) is required."));
			}
		}

		void AddPortOfArrivalAndTranshipmentValidations(Cresa cresa)
		{
			((Unloco)cresa.PortOfArrival).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("f0e70160-8f59-4de2-992b-8e6b29c23f4d", "Port of Arrival (Next Discharge Port) is required."));
			((Unloco)cresa.PortOfArrival).CodeInfo.AddAsciiCharactersValidation();
			((Unloco)cresa.PortOfTranshipment).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("009b4e51-45e0-40a2-90bb-b3521eb1dd48", "Port of Transhipment (Next Discharge Port) is required."));
			((Unloco)cresa.PortOfTranshipment).CodeInfo.AddAsciiCharactersValidation();
		}

		void AddCargoReceiptDateValidation(Cresa cresa)
		{
			cresa.CargoReceiptDateInfo.AddMessageErrorIfEmpty(Res.GetString("b2a15aa1-e5fd-4cf6-8d69-66d0719d5cd2", "Cargo Receipt Date is required."));
			cresa.GoodsInDateTimeInfo.AddMessageErrorIfEmpty(Res.GetString("41ffd54a-be1d-44e3-ba75-5bf1d5c90022", "Date/Time goods arrived at the warehouse is required."));
		}

		void AddGoodsDetailValidations(Cresa cresa)
		{
			cresa.TotalPackCountInfo.AddMessageError(() => cresa.TotalPackCount <= 0, Res.GetString("ed8717e4-184b-4d0d-b058-9d04508bab79", "Packs are required."));
			((CodeDescription)cresa.PackType)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("090aaa8c-c5e5-44fb-a64d-c8c066509873", "Package Type are required."));
			((Measurement)cresa.TotalWeight)?.ValueInfo.AddMessageError(() => cresa.TotalWeight.Value <= 0, Res.GetString("ba73c890-fb94-49a8-914b-dd9709ece5fa", "The Ci5 and S)One systems supports only integer value of weight. The weight entered or rounded to nearest kilogram value is zero. Please review the pack line weight and input a valid weight."));
			((Measurement)cresa.TotalVolume)?.ValueInfo.AddMessageError(() => cresa.TotalVolume.Value <= 0, Res.GetString("d5b44318-3637-4e81-8789-14f9b1ffba1e", "Total Volume is required."));
			cresa.GoodsDescriptionInfo?.AddMessageErrorIfEmpty(Res.GetString("7798930f-7ea0-4db7-9234-6c778cf262ae", "Goods Description is required"));

			foreach (BookingPackingLine packingLine in cresa.PackingLines)
			{
				AddPackingLineValidation(packingLine);
			}
			cresa.ErrorPlaceHolderInfo.AddMessageError(() => !cresa.PackingLines.Any(), Res.GetString("391f552e-fd51-4a50-b5fd-8951f3ff78f7", "No Packages have been received."));
		}

		void AddPackingLineValidation(BookingPackingLine packingLine)
		{
			packingLine.PackingLineIDInfo.AddMessageErrorIfEmpty(Res.GetString("a960bf2e-b796-4b8f-b66b-a19ff145efcd", "Package ID is required."));
			packingLine.PackingLineIDInfo.AddMessageError(() => packingLine.PackingLineID.Length > 17, Res.GetString("0b948601-b3d8-4e65-a709-a2a2c1bda705", "Package ID is more than 17 characters."));
			packingLine.QuantityInfo.AddMessageError(() => (packingLine.Quantity.IsEmpty || packingLine.Quantity <= 0), Res.GetString("dbc985f5-1595-4613-8eab-4fcbf8c48a67", "Packline ID: {0} Packs are required.", packingLine.PackingLineID));
			((CodeDescription)packingLine.PackageType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("f7bd4b63-15e9-48d0-828f-30943ef8cd0b", "Packline ID: {0} Package Type is required.", packingLine.PackingLineID));
			packingLine.Weight.ValueInfo.AddMessageError(() => (packingLine.Weight.IsNull || packingLine.Weight.Value <= 0), Res.GetString("1a29aca0-56e5-4316-8f5b-a8b18ffa53e1", "Packline ID: {0} Weight is required.", packingLine.PackingLineID));
			packingLine.Volume.ValueInfo.AddMessageError(() => (packingLine.Volume.IsNull || packingLine.Volume.Value <= 0), Res.GetString("b00952cb-01ad-49cd-9e40-c083db7bc992", "Packline ID: {0} Volume is required.", packingLine.PackingLineID));
			packingLine.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("12325049-3dda-414e-b16f-6ee533192c38", "Packline ID: {0} Goods Description is required, OVP cannot auto full Goods Description as inner has different Goods Description.", packingLine.PackingLineID));
		}

		void AddECVAndCRESAReferenceValidations(Cresa cresa)
		{
			var ecvReference = PortReferences?.Cast<CusEntryNumber>().FirstOrDefault(e => e.CE_EntryType == ExpectedPortReferenceType);
			if (ecvReference != null)
			{
				if (ExpectedPortReferenceType == TransitWarehousePortReferenceTypes.Codes.PortAuthority)
				{
					cresa.ECVReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("c92d25a7-b01f-4434-9b56-13dfe01fcb2c", "ECV Reference (PAN Reference) is required when sending an amendment or withdrawal."));
					cresa.CRESAReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("c69f589e-66f7-40ce-96a0-15023cd2704e", "CRESA Reference (PAN Line Reference) is required when sending an amendment or withdrawal."));
				}
				else
				{
					cresa.ECVReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("0b9e6d76-40dd-43ff-a5cb-739d04dd4703", "ECV Reference (PEN Reference) is required when sending an amendment or withdrawal."));
					cresa.CRESAReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("0ba4e47d-518f-4b09-a558-67cbb61157a3", "CRESA Reference (PEN Line Reference) is required when sending an amendment or withdrawal."));
				}
			}
		}

		protected abstract bool ConsignmentIsRCN { get; }

		#endregion
	}
}
