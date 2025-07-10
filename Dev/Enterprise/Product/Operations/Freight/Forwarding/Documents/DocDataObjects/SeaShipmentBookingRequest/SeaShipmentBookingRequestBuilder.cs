using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CarrierMessageValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class SeaShipmentBookingRequestBuilder
	{
		public const int PackLineMarksAndNumbersFieldCapacity = 100;

		readonly ForwardingShipment shipment;
		readonly IContext context;
		readonly DangerousGoodBuilder dgBuilder;
		readonly ForwardingConsol consol;
		readonly IStmALogProvider logProvider;

		public SeaShipmentBookingRequestBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
			dgBuilder = new DangerousGoodBuilder();
			consol = GetNVOConsol();
			logProvider = parameters?.LogProvider;
		}

		public SeaShipmentBookingRequest Build()
		{
			var bookingRequest = new SeaShipmentBookingRequest(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);

			bookingRequest.IsConsolAttached = shipment.Consols.Any();

			PopulateIsRequiredSendAttachment(bookingRequest);
			PopulatePickupRelatedProperties(bookingRequest);
			PopulateDeliveryRelatedProperties(bookingRequest);
			PopulateOtherShipmentProperties(bookingRequest);

			var weightUnit = GetWeightUnit(shipment.OuterPackLines);
			var volumeUnit = GetVolumeUnit(shipment.OuterPackLines);

			PopulatePackLines(bookingRequest, weightUnit, volumeUnit);

			PopulateTotalVolume(bookingRequest, volumeUnit);
			PopulateTotalWeight(bookingRequest, weightUnit);

			bookingRequest.Numbers = ReferenceNumber.Create(context, shipment.Numbers);

			AddValidation(bookingRequest);

			bookingRequest.ValidateAllIncludingChildren();

			return bookingRequest;
		}

		#region Population

		void PopulateIsRequiredSendAttachment(SeaShipmentBookingRequest bookingRequest)
		{
			var coLoadNVOCCConsol = shipment.Consols.OfType<ForwardingConsol>().FirstOrDefault(c => c.IsCoLoad && c.CreditorIsNVOCC);
			bookingRequest.IsRequiredSendAttachment =
				OrgHeaderExtensions.GetShippingLineMessagingRequirement(coLoadNVOCCConsol?.Creditor ?? shipment.BookedShippingLine, ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage)?.RSR_IsBookingRequest ?? false;
		}

		void PopulateOtherShipmentProperties(SeaShipmentBookingRequest bookingRequest)
		{
			bookingRequest.CurrentUser = AddressBuilder.CreateForCurrentUser(context);

			bookingRequest.Shipper = consol != null ? AddressBuilder.Create(context, consol?.SendingForwarderWithContact) :
				 AddressBuilder.Create(context, GlbBranch.CurrentBranch?.OrgProxy?.MainAddress);
			bookingRequest.Shipper.Unloco = Unloco.Create(context, bookingRequest.Shipper?.Unloco).WithCustomNameProvider(GetDetailedPortName);

			bookingRequest.Consignee = LastSeaConsol != null ? AddressBuilder.Create(context, LastSeaConsol?.ReceivingForwarderWithContact) :
				AddressBuilder.Create(context, shipment.DeliveryAgent?.MainAddress);

			bookingRequest.Recipient = AddressBuilder.Create(context, consol?.CreditorAddress ?? shipment.BookedShippingLineAddress);
			bookingRequest.Recipient.Unloco = Unloco.Create(context, bookingRequest.Recipient.Unloco).WithCustomNameProvider(GetDetailedPortName);

			bookingRequest.CarrierBookingOffice = consol != null
				? Unloco.Create(context, consol.CarrierBookingOffice).WithCustomNameProvider(GetDetailedPortName)
				: Unloco.Create(context, shipment.BookedShippingLineAddress?.RelatedPortCode).WithCustomNameProvider(GetDetailedPortName);

			bookingRequest.BookingReference = shipment.GetCarrierBookingReference();

			bookingRequest.ContainerMode = new CodeDescription(new CodeDescriptionPairList(OLookUpEditType.ContainerMode))
			{
				Code = shipment.JS_PackingMode
			};

			bookingRequest.Origin = Unloco.Create(context, shipment.Origin);
			bookingRequest.Destination = Unloco.Create(context, shipment.Destination);
			bookingRequest.PortOfLoading = consol != null ? Unloco.Create(context, consol.LoadPort) : Unloco.Create(context, shipment.LoadPort);
			bookingRequest.PortOfDischarge = consol != null ? Unloco.Create(context, consol.DischargePort) : Unloco.Create(context, shipment.DischargePort);

			bookingRequest.EarliestDepartureDate = shipment.JS_E_DEP;
			bookingRequest.LatestDeliveryDate = shipment.JS_E_ARV;

			bookingRequest.ShipperReference = shipment.JS_UniqueConsignRef;

			bookingRequest.CarrierContractNumbersFormatted = shipment.GetCarrierContractNumber();

			bookingRequest.GoodsHandlingInstructions = shipment.GetGoodsHandlingInstructions();

			PopulatePaymentTerms(bookingRequest);

			bookingRequest.AdditionalTerms = shipment.JS_AdditionalTerms;

			bookingRequest.TransportMode = new CodeDescription(context.TransportModes) { Code = shipment.TransportMode };

			PopulateReleaseType(bookingRequest);

			var mainTransportLeg = Transports.Create(context, ConsolTransports).Main ?? Transports.Create(context, ShipmentTransports).Main;
			if (mainTransportLeg != null)
			{
				bookingRequest.VesselName = mainTransportLeg.Vessel?.Name ?? ZString.Empty;
				bookingRequest.LloydsIMO = mainTransportLeg.Vessel?.LloydsIMO ?? ZString.Empty;
				bookingRequest.VoyageNumber = mainTransportLeg.VoyageFlightNumber;
				bookingRequest.ETD = mainTransportLeg.ETD;
				bookingRequest.ETA = mainTransportLeg.ETA;
				bookingRequest.PortOfLoading = mainTransportLeg.PortOfLoading;
				bookingRequest.PortOfDischarge = mainTransportLeg.PortOfDischarge;
				bookingRequest.LegTransportMode = mainTransportLeg.Mode?.Code ?? ZString.Empty;
				bookingRequest.LegOrder = ZByte.ParseSafe(mainTransportLeg.LegOrder.ToString(), ZByte.Zero);
				bookingRequest.LegType = mainTransportLeg.Type?.Code ?? ZString.Empty;
			}

			bookingRequest.OperationalPort = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = consol == null || consol.JK_RL_NKLoadPort.IsEmpty
					? shipment.JS_RL_NKLoadPort
					: consol.JK_RL_NKLoadPort
			}.WithCustomNameProvider(GetDetailedPortName);

			bookingRequest.FreightPayableAt = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = consol == null
					? ZString.Empty
					: FreightPayer?.Unloco != null && !FreightPayer.Unloco.Code.IsEmpty
						? FreightPayer.Unloco.Code
						: consol.JK_PrepaidCollect == Constants.PaymentType.Collect
							? consol.JK_RL_NKDischargePort
							: consol.JK_RL_NKLoadPort
			}.WithCustomNameProvider(GetDetailedPortName);
		}

		Address FreightPayer
		{
			get
			{
				if (consol == null)
				{
					return null;
				}

				var freightPayer = AddressBuilder.Create(context,
					consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid
						? consol.SendingForwarderWithContact
						: consol.ReceivingForwarderWithContact);
				freightPayer.Unloco = Unloco.Create(context, freightPayer.Unloco).WithCustomNameProvider(GetDetailedPortName);

				return freightPayer;
			}
		}

		void PopulateReleaseType(SeaShipmentBookingRequest bookingRequest)
		{
			if (FirstSeaConsol != null)
			{
				bookingRequest.ReleaseType = new CodeDescription(FirstSeaConsol?.JK_ReleaseType_List)
				{
					Code = FirstSeaConsol.JK_ReleaseType
				};
			}
		}

		void PopulatePaymentTerms(SeaShipmentBookingRequest bookingRequest)
		{
			bookingRequest.PaymentTerms = new OptionalCharge()
			{
				IsPrepaid = FirstSeaConsol?.JK_PrepaidCollect.ToString() == Constants.PaymentType.Prepaid,
				IsCollect = FirstSeaConsol?.JK_PrepaidCollect.ToString() == Constants.PaymentType.Collect
			};
		}

		void PopulateDeliveryRelatedProperties(SeaShipmentBookingRequest bookingRequest)
		{
			if (consol != null)
			{
				bookingRequest.IsDoorDelivery = consol.IsDoorDelivery();
			}

			bookingRequest.DeliverTo = AddressBuilder.Create(context, null);
			bookingRequest.PlaceOfDelivery = Unloco.Create(context, (IUnloco)null);

			SetDefaultDeliveryRelatedProperties(bookingRequest);

			bookingRequest.IsDoorDeliveryInfo.ValueChanged += (s, e) =>
			{
				SetDefaultDeliveryRelatedProperties(bookingRequest);
			};

			bookingRequest.DeliverTo.Unloco = Unloco.Create(context, bookingRequest.DeliverTo.Unloco).WithCustomNameProvider(GetDetailedPortName);
		}

		void SetDefaultDeliveryRelatedProperties(SeaShipmentBookingRequest bookingRequest)
		{
			if (bookingRequest.IsDoorDelivery)
			{
				if (consol != null)
				{
					SetAddressDetails(bookingRequest.DeliverTo, consol.UnpackDepotAddress);
					SetUnloco(bookingRequest.PlaceOfDelivery, consol.DischargePort);
				}
				else
				{
					SetAddressDetails(bookingRequest.DeliverTo, shipment.ImportReleaseDepot, shipment.ConsigneeDeliveryAddress);
					SetUnloco(bookingRequest.PlaceOfDelivery, shipment.ImportReleaseDepot?.HeaderClosestPort, shipment.ConsigneeDeliveryAddress?.Organisation?.ClosestPort, shipment.Destination);
				}
			}
			else
			{
				SetAddressDetails(bookingRequest.DeliverTo);
				SetUnloco(bookingRequest.PlaceOfDelivery);
			}
		}

		void PopulatePickupRelatedProperties(SeaShipmentBookingRequest bookingRequest)
		{
			if (consol != null)
			{
				bookingRequest.IsDoorPickup = consol.IsDoorPickup();
			}

			bookingRequest.PickupFrom = AddressBuilder.Create(context, null);
			bookingRequest.PlaceOfReceipt = Unloco.Create(context, (IUnloco)null);

			SetDefaultPickupRelatedProperties(bookingRequest);

			bookingRequest.IsDoorPickupInfo.ValueChanged += (s, e) =>
			{
				SetDefaultPickupRelatedProperties(bookingRequest);
			};

			bookingRequest.PickupFrom.Unloco = Unloco.Create(context, bookingRequest.PickupFrom.Unloco).WithCustomNameProvider(GetDetailedPortName);
		}

		void SetDefaultPickupRelatedProperties(SeaShipmentBookingRequest bookingRequest)
		{
			if (bookingRequest.IsDoorPickup)
			{
				bookingRequest.EstCargoPickupDateTime = shipment.DocsAndCartage?.JP_PickupRequiredBy ?? ZDateTime.Empty;

				if (consol != null)
				{
					SetAddressDetails(bookingRequest.PickupFrom, consol.PackDepotAddress);
					SetUnloco(bookingRequest.PlaceOfReceipt, consol.LoadPort);
				}
				else
				{
					SetAddressDetails(bookingRequest.PickupFrom, shipment.ExportReceivingDepot, shipment.ConsignorPickupAddress);
					SetUnloco(bookingRequest.PlaceOfReceipt, shipment.ExportReceivingDepot?.HeaderClosestPort, shipment.ConsignorPickupAddress?.Organisation?.ClosestPort, shipment.Origin);
				}
			}
			else
			{
				bookingRequest.EstCargoPickupDateTime = ZDateTime.Empty;
				SetAddressDetails(bookingRequest.PickupFrom);
				SetUnloco(bookingRequest.PlaceOfReceipt);
			}
		}

		void SetAddressDetails(Address address, OrgAddress orgAddress1 = null, JobDocAddress orgAddress2 = null)
		{
			var originalAddress = orgAddress1 != null ? AddressBuilder.Create(context, orgAddress1) : AddressBuilder.Create(context, orgAddress2);

			//For Excel macro only support string change
			address.CompanyName = originalAddress.CompanyName;
			address.AddressLine1 = originalAddress.AddressLine1;
			address.AddressLine2 = originalAddress.AddressLine2;
			address.City = originalAddress.City;
			address.State = originalAddress.State;
			address.Postcode = originalAddress.Postcode;

			address.Unloco.Code = originalAddress.Unloco?.Code ?? ZString.Empty;
			address.Unloco.Name = originalAddress.Unloco?.Name ?? ZString.Empty;

			address.Country.Code = originalAddress.Country?.Code ?? ZString.Empty;
			address.Country.Name = originalAddress.Country?.Name ?? ZString.Empty;

			address.Contact = originalAddress.Contact;
			address.Phone = originalAddress.Phone;
			address.Fax = originalAddress.Fax;
			address.Email = originalAddress.Email;
		}

		void SetUnloco(Unloco unloco, IRefUNLOCO refUNLOCO1 = null, IRefUNLOCO refUNLOCO2 = null, IRefUNLOCO refUNLOCO3 = null)
		{
			var originalunloco = Unloco.Create(context, refUNLOCO1 ?? refUNLOCO2 ?? refUNLOCO3);

			//For Excel macro only support string change
			unloco.Code = originalunloco.Code;
			unloco.Name = originalunloco.Name;
		}

		string GetDetailedPortName(IRefUNLOCO refUnloco)
		{
			if (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.Value)
			{
				return UnlocoExtensions.GetDetailedPortName(refUnloco);
			}

			return refUnloco?.RL_PortName ?? string.Empty;
		}

		void PopulatePackLines(SeaShipmentBookingRequest bookingRequest, ZString weightUnit, ZString volumeUnit)
		{
			var totalPacks = 0;
			if (shipment.OuterPackLines != null)
			{
				var listPackLines = new List<SeaShipmentBookingRequestPackLine>();
				foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
				{
					var requestPackLine = CreatePackLine(packLine, weightUnit, volumeUnit);

					listPackLines.Add(requestPackLine);
					totalPacks += packLine.JL_PackageCount;
				}

				bookingRequest.GoodsAndEquipmentDetails = listPackLines.ToArray();
			}
			bookingRequest.TotalPacks = totalPacks;
		}

		SeaShipmentBookingRequestPackLine CreatePackLine(ForwardingPackLine packLine, ZString weightUnit, ZString volumeUnit)
		{
			var requestPackLine = new SeaShipmentBookingRequestPackLine
			{
				GoodsDescription = GetGoodsDescription(packLine),
				MarksAndNumbersOnPackages = GetMarksAndNumbers(packLine),
				PackType = new CodeDescription(packLine.Lookups.PackTypes)
				{
					Code = packLine.PackType?.F3_Code ?? ZString.Empty
				},
				PacksQuantity = packLine.JL_PackageCount,
				Packs = $"{packLine.JL_PackageCount} {packLine.PackType?.F3_Code}", // non-translatable
			};

			requestPackLine.DangerousGoods = packLine
				.UNDGs
				.Select(undg => dgBuilder.Build(undg, context))
				.ToArray();

			(var harmonizedCodes, var harmonizedCodesString) = GetHarmonizedCodesListAndString(packLine);
			requestPackLine.HarmonizedCodesCollection = harmonizedCodes;
			requestPackLine.HarmonizedCodes = harmonizedCodesString;

			requestPackLine.CargoWeight = new Measurement()
			{
				Value = Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, weightUnit),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = weightUnit
				},
			};

			requestPackLine.CargoVolume = new Measurement()
			{
				Value = Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, volumeUnit),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = volumeUnit
				}
			};
			return requestPackLine;
		}

		void PopulateTotalWeight(SeaShipmentBookingRequest bookingRequest, ZString weightUnit)
		{
			var weight = bookingRequest.GoodsAndEquipmentDetails.Sum(d =>
				Utilities.Round(Constants.Weight.Convert(d.CargoWeight.Value, d.CargoWeight.Unit.Code, weightUnit), 3)
			);
			bookingRequest.TotalCargoWeight = new Measurement
			{
				Value = weight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = weightUnit,
				}
			};
		}

		void PopulateTotalVolume(SeaShipmentBookingRequest bookingRequest, ZString volumeUnit)
		{
			var totalVolume = bookingRequest.GoodsAndEquipmentDetails.Sum(dim => Utilities.Round(Constants.Volume.Convert(dim.CargoVolume.Value, dim.CargoVolume.Unit.Code, volumeUnit), 3));

			bookingRequest.TotalCargoVolume = new Measurement
			{
				Value = totalVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = volumeUnit
				}
			};
		}

		ZString GetWeightUnit(ForwardingPackLineCollection packLines)
		{
			var weightUnits = packLines
				.Select(packLine => ((ForwardingPackLine)packLine).JL_ActualWeightUQ)
				.Where(unit => Constants.Weight.ContainsCode(unit))
				.ToArray();

			var useImperialUnits = weightUnits.Length > 0 && weightUnits.All(unit => Constants.Weight.IsImperial(unit));

			return useImperialUnits ? Constants.Weight.Pounds : Constants.Weight.Kilograms;
		}

		ZString GetVolumeUnit(ForwardingPackLineCollection packLines)
		{
			var volumeUnits = packLines
				.Select(packLine => ((ForwardingPackLine)packLine).JL_ActualVolumeUQ)
				.Where(unit => Constants.Volume.ContainsCode(unit))
				.ToArray();

			var useImperialUnits = volumeUnits.Length > 0 && volumeUnits.All(unit => Constants.Volume.IsImperial(unit));

			return useImperialUnits
				? Constants.Volume.CubicFeet
				: Constants.Volume.CubicMetres;
		}

		ZString GetMarksAndNumbers(ForwardingPackLine packLine)
		{
			var marksAndNumbers = packLine.JL_MarksAndNumbers.IsEmpty
				? shipment.JS_MarksAndNumbers
				: packLine.JL_MarksAndNumbers;

			return marksAndNumbers.SubstringSafe(0, PackLineMarksAndNumbersFieldCapacity);
		}

		(List<HarmonizedCode> codesList, string codesString) GetHarmonizedCodesListAndString(ForwardingPackLine packLine)
		{
			var harmonizedCodesList = new List<HarmonizedCode>();
			var harmonizedCodesStrings = new List<string>();
			if (!packLine.JL_HarmonisedCode.IsEmpty)
			{
				var harmonizedCode = CreateHarmonizedCode(packLine.JL_RN_NKOrigin, packLine.JL_HarmonisedCode);
				harmonizedCodesList.Add(harmonizedCode);
				harmonizedCodesStrings.Add(GetHarmonizedCodeString(harmonizedCode));
			}
			foreach (var packLineHarmonisedCode in packLine.HarmonisedCodes)
			{
				if (packLineHarmonisedCode != null)
				{
					var harmonizedCode = CreateHarmonizedCode(packLineHarmonisedCode.JLH_RN_NKCountry, packLineHarmonisedCode.JLH_Code);
					harmonizedCodesList.Add(harmonizedCode);
					harmonizedCodesStrings.Add(GetHarmonizedCodeString(harmonizedCode, false));
				}
			}
			return (harmonizedCodesList, string.Join(", ", harmonizedCodesStrings));
		}

		HarmonizedCode CreateHarmonizedCode(ZString country, ZString code)
		{
			return new HarmonizedCode()
			{
				Country = new Country(context.Factory, context.Countries)
				{
					Code = country
				},
				Code = code,
			};
		}

		public string GetHarmonizedCodeString(HarmonizedCode harmonizedCode, bool isGeneric = true)
		{
			if (string.IsNullOrEmpty(harmonizedCode?.Code))
			{
				return ZString.Empty;
			}
			else if (isGeneric)
			{
				return $"HC: {harmonizedCode.Code}"; // non-translatable
			}
			else if (string.IsNullOrEmpty(harmonizedCode?.Country?.Code))
			{
				return $"HS: {harmonizedCode.Code}"; // non-translatable
			}
			else
			{
				return harmonizedCode.ToString();
			}
		}

		ZString GetGoodsDescription(ForwardingPackLine packLine)
		{
			return
				!string.IsNullOrWhiteSpace(packLine.JL_DetailedDescription) ? packLine.JL_DetailedDescription :
				!string.IsNullOrWhiteSpace(packLine.JL_Description) ? packLine.JL_Description :
				!string.IsNullOrWhiteSpace(packLine.Shipment?.DetailedGoodsDescriptionNoteText) ? packLine.Shipment.DetailedGoodsDescriptionNoteText :
				packLine.Shipment?.JS_GoodsDescription ?? ZString.Empty;
		}

		#endregion

		#region Consol

		ForwardingConsol GetNVOConsol()
		{
			return shipment.Consols?.Cast<ForwardingConsol>().FirstOrDefault(c => c.CreditorIsNVOCC);
		}

		#endregion

		#region Transports

		protected IReadOnlyCollection<Freight.Business.Transport> ConsolTransports => consoTtransports ?? (consoTtransports = GetTransports(FirstSeaConsol?.Transports));
		IReadOnlyCollection<Freight.Business.Transport> consoTtransports;

		protected IReadOnlyCollection<Freight.Business.Transport> ShipmentTransports => shipmenttransports ?? (shipmenttransports = GetTransports(shipment?.Transports));
		IReadOnlyCollection<Freight.Business.Transport> shipmenttransports;

		IReadOnlyCollection<Freight.Business.Transport> GetTransports(TransportCollection transports)
		{
			transports?.Sort(MovementLegComparer.PortsAndDatesBased(transports));
			return transports?
				.OfType<Freight.Business.Transport>()
				.ToArray();
		}

		#endregion

		#region Consols

		protected ForwardingConsol FirstSeaConsol => firstSeaConsol ?? (firstSeaConsol = MovementLegComparer.FirstOrDefaultLegForTransportMode(shipment.Consols.Cast<ForwardingConsol>(), Constants.TransportModes.Sea));
		ForwardingConsol firstSeaConsol;

		protected ForwardingConsol LastSeaConsol => lastSeaConsol ?? (lastSeaConsol = MovementLegComparer.LastOrDefaultLegForTransportMode(shipment.Consols.Cast<ForwardingConsol>(), Constants.TransportModes.Sea));
		ForwardingConsol lastSeaConsol;

		#endregion

		#region Validation

		protected virtual void AddValidation(SeaShipmentBookingRequest bookingRequest)
		{
			AddAddressesValidation(bookingRequest);
			AddPortsValidation(bookingRequest);
			AddPackLinesValidation(bookingRequest);
			AddCarrierBookingReferenceValidation(bookingRequest);
			AddCarrierBookingOfficeValidation(bookingRequest);

			if (shipment.Consols.Any(c => c.MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(ConsolDocumentNames.BookingRequest)))
			{
				bookingRequest.ErrorPlaceHolderInfo.AddMessageError(() => true, (NoResString)"A Booking Request already been sent from Consol. A Booking Request can only be sent from Consol or Shipment!"); // non-translatable validation message
			}

			bookingRequest.ShipperReferenceInfo.AddMessageErrorIfEmpty((NoResString)"Shipper reference is required."); // non-translatable validation message
			bookingRequest.ContainerMode.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Mode (Container Mode) is required."); // non-translatable validation message
			bookingRequest.EarliestDepartureDateInfo.AddMessageErrorIfEmpty((NoResString)"Earliest Departure Date (ETD) is required."); // non-translatable validation message
			bookingRequest.LatestDeliveryDateInfo.AddMessageErrorIfEmpty((NoResString)"Latest Delivery Date (ETA) is required."); // non-translatable validation message
			bookingRequest.EstCargoPickupDateTimeInfo.AddMessageError(() => bookingRequest.IsDoorPickup && bookingRequest.EstCargoPickupDateTime.IsEmpty, (NoResString)"Est. Cargo Pickup Date Time (Pickup > Pickup Required By) is required."); // non-translatable validation message
			bookingRequest.AddValidationDependencies(bookingRequest.EstCargoPickupDateTimeInfo, bookingRequest.IsDoorPickupInfo);
			bookingRequest.TotalPacksInfo.AddMessageErrorIfEmpty((NoResString)"Total Packs cannot be zero."); // non-translatable validation message
			bookingRequest.TotalCargoWeight.ValueInfo.AddMessageErrorIfEmpty((NoResString)"Total cargo weight cannot be zero."); // non-translatable validation message
			bookingRequest.TotalCargoVolume.ValueInfo.AddMessageErrorIfEmpty((NoResString)"Total cargo volume cannot be zero."); // non-translatable validation message
			bookingRequest.ETDInfo.AddMessageErrorIfEmpty((NoResString)"ETD is required."); // non-translatable validation message
			bookingRequest.ETAInfo.AddMessageErrorIfEmpty((NoResString)"ETA is required."); // non-translatable validation message

			var requirePPDOrCCXForFreightChargesMessageError = Res.GetString("34CFF868-804A-493A-B5D8-C86EFCF35C05", "Either Prepaid or Collect payment type must be selected for 'Payment Terms'.");
			OptionalCharge paymentTerms = (OptionalCharge)bookingRequest.PaymentTerms;
			paymentTerms.IsPrepaidInfo.AddMessageError(() => !paymentTerms.IsPrepaid && !paymentTerms.IsCollect, requirePPDOrCCXForFreightChargesMessageError);
			paymentTerms.IsCollectInfo.AddMessageError(() => !paymentTerms.IsPrepaid && !paymentTerms.IsCollect, requirePPDOrCCXForFreightChargesMessageError);
			paymentTerms.AddValidationDependencies(paymentTerms.IsPrepaidInfo, paymentTerms.IsCollectInfo);
			paymentTerms.AddValidationDependencies(paymentTerms.IsCollectInfo, paymentTerms.IsPrepaidInfo);
		}

		#region Ports Validation

		void AddPortsValidation(SeaShipmentBookingRequest bookingRequest)
		{
			var enterValidUnlocoMessageError = Res.GetString("A305BD95-AD2E-40F0-944B-65310BEA4270", "You have not entered a valid un loco.");

			bookingRequest.PortOfLoading.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.PortOfLoading.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			bookingRequest.PortOfDischarge.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.PortOfDischarge.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			bookingRequest.Origin.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Origin is required."); // non-translatable validation message
			bookingRequest.Origin.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.Origin.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			bookingRequest.Destination.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Destination is required."); // non-translatable validation message
			bookingRequest.Destination.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.Destination.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			bookingRequest.PlaceOfReceipt?.CodeInfo.AddMessageError(() => bookingRequest.IsDoorPickup && string.IsNullOrEmpty(bookingRequest.PlaceOfReceipt?.Code), (NoResString)"Place of Receipt is required."); // non-translatable validation message
			bookingRequest.PlaceOfReceipt?.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.PlaceOfReceipt?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			bookingRequest.PlaceOfDelivery?.CodeInfo.AddMessageError(() => bookingRequest.IsDoorDelivery && string.IsNullOrEmpty(bookingRequest.PlaceOfDelivery?.Code), (NoResString)"Place of Delivery is required."); // non-translatable validation message
			bookingRequest.PlaceOfDelivery?.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.PlaceOfDelivery?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
			bookingRequest.PlaceOfReceipt.AddValidationDependencies(bookingRequest.PlaceOfReceipt.CodeInfo, bookingRequest.IsDoorPickupInfo);
			bookingRequest.PlaceOfDelivery.AddValidationDependencies(bookingRequest.PlaceOfDelivery.CodeInfo, bookingRequest.IsDoorDeliveryInfo);

			bookingRequest.OperationalPort?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("101B4CC5-743B-4B17-8B81-3CA139ED0A19", "Operational Port is required."));
			bookingRequest.OperationalPort?.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.OperationalPort?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			bookingRequest.FreightPayableAt?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("1E5800B6-811D-4480-82D4-DFA93892FADF", "Freight Payable At is required."));
			bookingRequest.FreightPayableAt?.CodeInfo.AddAsciiCharactersValidation();
			bookingRequest.FreightPayableAt?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
		}

		#endregion

		void AddAddressesValidation(SeaShipmentBookingRequest bookingRequest)
		{
			bookingRequest.Shipper
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Shipper", () => false) // non-translatable validation message
				.AddToOrderSupport(); // non-translatable validation message

			bookingRequest.Shipper.CompanyNameInfo.AddMessageError(() => (bookingRequest.Shipper.IsToOrder()
				|| (string.IsNullOrWhiteSpace(bookingRequest.Shipper.CompanyName)
				|| string.IsNullOrWhiteSpace(bookingRequest.Shipper.AddressLine1)
				|| string.IsNullOrWhiteSpace(bookingRequest.Shipper.Country?.Name)))
				, (NoResString)"Shipper name and address information is required."); // non-translatable validation message

			bookingRequest.Recipient
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Carrier", () => false) // non-translatable validation message
				.AddToOrderSupport(); // non-translatable validation message

			bookingRequest.Recipient?.CompanyNameInfo.AddMessageError(() => (bookingRequest.Recipient.IsToOrder()
				|| (string.IsNullOrWhiteSpace(bookingRequest.Recipient.CompanyName)
				|| string.IsNullOrWhiteSpace(bookingRequest.Recipient.AddressLine1)
				|| string.IsNullOrWhiteSpace(bookingRequest.Recipient.Country?.Name)))
				, (NoResString)"Carrier name and address information is required."); // non-translatable validation message

			bookingRequest.PickupFrom?
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Pickup From", () => false) // non-translatable validation message
				.AddToOrderSupport(); // non-translatable validation message

			bookingRequest.PickupFrom?.CompanyNameInfo.AddMessageError(() => (bookingRequest.IsDoorPickup
				&& (bookingRequest.PickupFrom.IsToOrder()
				|| (string.IsNullOrWhiteSpace(bookingRequest.PickupFrom.CompanyName)
				|| string.IsNullOrWhiteSpace(bookingRequest.PickupFrom.AddressLine1)
				|| string.IsNullOrWhiteSpace(bookingRequest.PickupFrom.Country?.Name))))
				, (NoResString)"Pickup From name and address information is required."); // non-translatable validation message

			bookingRequest.DeliverTo?
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation("DeliverTo", () => false) // non-translatable validation message
				.AddToOrderSupport(); // non-translatable validation message

			bookingRequest.DeliverTo?.CompanyNameInfo.AddMessageError(() => (bookingRequest.IsDoorDelivery
				&& (bookingRequest.DeliverTo.IsToOrder()
				|| (string.IsNullOrWhiteSpace(bookingRequest.DeliverTo.CompanyName)
				|| string.IsNullOrWhiteSpace(bookingRequest.DeliverTo.AddressLine1)
				|| string.IsNullOrWhiteSpace(bookingRequest.DeliverTo.Country?.Name))))
				, (NoResString)"Deliver To name and address information is required."); // non-translatable validation message

			bookingRequest.PickupFrom.AddValidationDependencies(bookingRequest.PickupFrom.CompanyNameInfo, bookingRequest.IsDoorPickupInfo);
			bookingRequest.DeliverTo.AddValidationDependencies(bookingRequest.DeliverTo.CompanyNameInfo, bookingRequest.IsDoorDeliveryInfo);
		}

		void AddPackLinesValidation(SeaShipmentBookingRequest bookingRequest)
		{
			foreach (var item in bookingRequest.GoodsAndEquipmentDetails)
			{
				item.GoodsDescriptionInfo.AddMessageErrorIfEmpty((NoResString)"Goods Description is required"); // non-translatable validation message
				item.PacksInfo.AddMessageErrorIfEmpty((NoResString)"Pack/Pack type is required"); // non-translatable validation message
				item.PacksInfo.AddMessageError(() => !string.IsNullOrEmpty(item.Packs) && item.PacksQuantity <= 0, (NoResString)"Package count is required"); // non-translatable validation message
				item.PacksInfo.AddMessageError(() => !string.IsNullOrEmpty(item.Packs) && (item.PackType == null || string.IsNullOrEmpty(item.PackType.Code)), (NoResString)"Package type is required"); // non-translatable validation message

				item.CargoWeight?.ValueInfo.AddMessageErrorIfEmpty((NoResString)"Weight is required"); // non-translatable validation message
				item.CargoWeight?.ValueInfo.AddMessageError(() => (item.CargoWeight != null && item.CargoWeight.Value <= 0), (NoResString)"Weight is required"); // non-translatable validation message

				item.CargoVolume?.ValueInfo.AddMessageErrorIfEmpty((NoResString)"Volume is required"); // non-translatable validation message
				item.CargoVolume?.ValueInfo.AddMessageError(() => (item.CargoVolume != null && item.CargoVolume.Value <= 0), (NoResString)"Volume is required"); // non-translatable validation message

				AddDangerousGoodsValidation(item.DangerousGoods);
			}
		}

		void AddDangerousGoodsValidation(IReadOnlyCollection<DangerousGood> dangerousGoods)
		{
			foreach (var dangerousGood in dangerousGoods)
			{
				dangerousGood.Validator = () => AddDangerousGoodsValidation(dangerousGood);
			}
		}

		static IEnumerable<string> AddDangerousGoodsValidation(DangerousGood dangerousGood)
		{
			if (dangerousGood.IMOClass.IsEmpty || dangerousGood.Code.IsEmpty || dangerousGood.ProperShippingName.IsEmpty)
			{
				yield return (NoResString)"DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance."; // non-translatable registration number
			}
		}

		void AddCarrierBookingReferenceValidation(SeaShipmentBookingRequest bookingRequest)
		{
			var carrierBookingReferenceShouldBeEmptyIfFirstTimeMessageWarning = Res.GetString("a87e7ee5-aa14-4ed1-851c-5c68d5c48d67", @"Carrier Booking Request Number is populated during booking confirmation.
If you need to send Carrier Booking Number at the time of a Booking Request, please ensure this is the number pre-assigned by the carrier in advance.");

			bool IsApplicable(StmALog log)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
					case Events.StatusUpdatedCode:
						return (string.Compare(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType), ShipmentDocumentNames.BookingRequest, StringComparison.OrdinalIgnoreCase) == 0);
					default:
						return false;
				}
			}

			bool IsThisMessageBeingSentForTheFirstTime()
			{
				var lastRelevantLog = logProvider?
					.Logs?
					.GetAllLogs()
					.OfType<StmALog>()
					.Where(log => !log.IsCancelled && IsApplicable(log))
					.OrderByDescending(log => log.SL_PostedTimeUtc)
					.FirstOrDefault();

				return (lastRelevantLog != null && lastRelevantLog.SL_SE_NKEvent == Events.StatusUpdatedCode || lastRelevantLog == null);
			}

			bookingRequest.BookingReferenceInfo.AddWarning(() =>
				(!string.IsNullOrEmpty(bookingRequest.BookingReference)) && IsThisMessageBeingSentForTheFirstTime(),
				carrierBookingReferenceShouldBeEmptyIfFirstTimeMessageWarning);
		}

		void AddCarrierBookingOfficeValidation(SeaShipmentBookingRequest bookingRequest)
		{
			var carrierBookingOfficeMandatoryErrorMessage = bookingRequest.IsConsolAttached
				? Res.GetString("FD6EDDDE-A112-4281-AC0B-679A707B3B18", "The Carrier Booking Office is mandatory.\r\nPlease provide it on Consol > Details > Docs > Carrier Booking Office.")
				: Res.GetString("8C0059C6-5880-4702-9203-6C88F4623A0F", "The Carrier Booking Office is mandatory.\r\nPlease provide a valid UNLOCO on selected address of Shipment > Additional Detail > Planned Carrier.");
			bookingRequest.CarrierBookingOffice.CodeInfo.AddMessageErrorIfEmpty(carrierBookingOfficeMandatoryErrorMessage);

			bookingRequest.CarrierBookingOffice.CodeInfo.AddWarning(() =>
				!bookingRequest.CarrierBookingOffice.Code.IsEmpty
				&& bookingRequest.CarrierBookingOffice.Code.SubstringSafe(0, 2) != bookingRequest.PlaceOfReceipt.Code.SubstringSafe(0, 2)
				&& bookingRequest.CarrierBookingOffice.Code.SubstringSafe(0, 2) != bookingRequest.PortOfLoading.Code.SubstringSafe(0, 2)
				&& bookingRequest.CarrierBookingOffice.Code.SubstringSafe(0, 2) != bookingRequest.Origin.Code.SubstringSafe(0, 2),
				Res.GetString("530CFBBA-A773-4B2A-BA8B-FBBDFD535952", "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading/Origin.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier."));

			bookingRequest.CarrierBookingOffice.AddValidationDependencies(bookingRequest.CarrierBookingOffice.CodeInfo, bookingRequest.PlaceOfReceipt.CodeInfo, bookingRequest.PortOfLoading.CodeInfo, bookingRequest.Origin.CodeInfo);
		}

		#endregion
	}
}
