using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Business.RequiredTaxNumbers;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class HouseBillBuilder
	{
		public HouseBillBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.parameters = parameters;
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;
		enum TaxNumberType
		{
			Shipper,
			Consignee,
			NotifyParty
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Search string")]
		public HouseBill Build(bool enableValidations = true, bool isDraft = false)
		{
			var customBusinessObject = shipment is ICustomFieldProvider customFieldProvider ? customFieldProvider.GetCustomBusinessObject() : null;
			var houseBill = new HouseBill(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, customBusinessObject);

			if (shipment.IsEditingElectronicBOL && shipment.JS_HouseBillOfLadingType != HouseBillOfLadingTypes.Code.FIATAHBL)
			{
				if (isDraft)
				{
					houseBill.IsElectronicBOL = true;
				}
				else
				{
					houseBill = new ElectronicHouseBillBuilder(shipment).Build(houseBill);
				}

				enableValidations = false;
			}

			houseBill.IsOriginal = string.Compare(parameters?.DocumentTitle, "ORIGINAL", StringComparison.OrdinalIgnoreCase) == 0;
			houseBill.HouseBillNumber = shipment.JS_HouseBill;
			houseBill.ShipmentNumber = shipment.JS_UniqueConsignRef;
			houseBill.ShippersReference = shipment.JS_BookingReference;
			houseBill.CarrierBookingReference = DepartureConsol?.JK_BookingReference ?? ZString.Empty;
			houseBill.CoLoadBookingReference = DepartureConsol?.JK_CoLoadBookingReference ?? ZString.Empty;
			houseBill.CoLoadMasterBillNumber = DepartureConsol?.JK_CoLoadMasterBill ?? ZString.Empty;
			houseBill.IsToSpecificAfricanCountry = CountryCodes.IsSpecificAfricanCountryCode(shipment.JS_RL_NKDestination.SubstringSafe(0, 2));

			houseBill.CTKNumber = string.Join(", ", shipment.Numbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote && x.CE_RN_NKCountryCode == shipment.JS_RL_NKDestination.SubstringSafe(0, 2)).Select(x => x.CE_EntryNum));

			houseBill.Clause = FreightDataRegistry.Instance.BOLClause.Value;

			houseBill.NumberOfCopies = shipment.JS_NoCopyBills;
			houseBill.NumberOfOriginals = shipment.JS_NoOriginalBills;

			houseBill.TariffLineItemReference = RetrieveTariffLineItemReference();

			houseBill.ContainerMode = new CodeDescription(shipment.Lookups.JS_PackingMode_List)
			{
				Code = shipment.JS_PackingMode
			};

			houseBill.TotalWeight = new Measurement
			{
				Value = shipment.JS_ActualWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = shipment.JS_UnitOfWeight
				}
			};

			houseBill.TotalVolume = new Measurement
			{
				Value = shipment.JS_ActualVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = shipment.JS_UnitOfVolume
				}
			};

			var goodsDescription = shipment.DetailedGoodsDescriptionNoteText.IsEmpty
				? shipment.JS_GoodsDescription
				: shipment.DetailedGoodsDescriptionNoteText;

			try
			{
				houseBill.GoodsDescription = goodsDescription.ToString().Normalize(System.Text.NormalizationForm.FormKC);
			}
			catch (ArgumentException exception) when (exception.Message.StartsWith("Invalid Unicode"))
			{
				ErrorReporter.ReportOnce($"goodsDescription contains characters that are not valid unicode and so cannot be normalised: {goodsDescription}", exception);
			}

			houseBill.MarksAndNumbers = shipment.JS_MarksAndNumbers;

			houseBill.ShipperLoadAndCount = new CodeDescription(new ShipperLoadAndCountTypes())
			{
				Code = RetrieveShippersLoadAndCount(shipment)
			};

			houseBill.INCO = new CodeDescription(shipment.Lookups.JS_INCO_List)
			{
				Code = Constants.IncoTerms.GetMappedOfficialIncoterm(shipment.JS_INCO)
			};

			var moveType = shipment.JS_HBLContainerPackModeOverride.Split(packModeSeparator).ToArray();

			houseBill.MoveTypeFrom = moveType.FirstOrDefault();
			houseBill.MoveTypeTo = moveType.Skip(1).FirstOrDefault();
			houseBill.MoveTypeList = GetMoveTypeList();

			houseBill.DeclaredValueOfGoods = new Money
			{
				Currency = new CodeDescription(Lookups.Currencies)
			};

			houseBill.FreightAmount = new Money
			{
				Currency = new CodeDescription(Lookups.Currencies)
			};

			houseBill.ReleaseType = new CodeDescription(Lookups.ReleaseTypes)
			{
				Code = shipment.JS_ReleaseType
			};

			houseBill.CustomsEntryNumber = new CustomsEntryNumber
			{
				Value = shipment.CustomsEntryNumber,
				Type = new CodeDescription(shipment.ShipmentCustomsEntryNumber.EntryType_List)
				{
					Code = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates && shipment.CustomsEntryNumberType == CusEntryNumberTypes.UnitedStates.ITN ? CusEntryNumberTypes.UnitedStates.AES : shipment.CustomsEntryNumberType
				}
			};

			houseBill.IsPrepaid = !IsCollect;
			houseBill.PaymentTerms = new CodeDescription(Lookups.PaymentTerms)
			{
				Code = IsCollect
					? Constants.PaymentType.Collect
					: Constants.PaymentType.Prepaid
			};
			houseBill.IsPrepaidInfo.ValueChanged += (obj, args) =>
			{
				houseBill.PaymentTerms.Code = houseBill.IsPrepaid
					? Constants.PaymentType.Prepaid
					: Constants.PaymentType.Collect;
			};

			houseBill.ShippedOnBoard = new ShippedOnBoard(shipment.Lookups.JS_ShippedOnBoard_List)
			{
				Code = shipment.JS_ShippedOnBoard,
				Date = shipment.JS_ShippedOnBoardDate
			};

			houseBill.HouseBillOfLadingType = new CodeDescription(shipment.Lookups.JS_HouseBillOfLadingType_List)
			{
				Code = shipment.JS_HouseBillOfLadingType
			};

			houseBill.Logo = new HouseBillLogo(shipment, houseBill.IsOriginal);

			houseBill.TermsAndConditions = new HouseBillTermsAndConditions(shipment);

			houseBill.Transports = Transports.Create(context, SeaTransportsInPortOrder, shipment);

			houseBill.AsAgentOption = new CodeDescription(Lookups.AsAgentOptions)
			{
				Code = HouseBillLookups.AsAgentOption.AsCarrier
			};

			houseBill.ExcessValueDeclaration = new Money()
			{
				Currency = new CodeDescription(Lookups.Currencies)
			};

			PopulateOverrides(houseBill);
			PopulateDates(houseBill);
			PopulateLocations(houseBill);
			PopulateAddresses(houseBill);
			PopulateTaxNumbers(houseBill);
			PopulateContainers(houseBill);
			PopulateLoosePackingLines(houseBill);
			PopulateConsols(houseBill);
			PopulateOrderReferences(houseBill);
			PopulateOrders(houseBill);
			PopulateCharges(houseBill);
			PopulateReferenceNumbers(houseBill);
			PopulateNotes(houseBill);
			PopulateNumbers(houseBill);
			PopulateExportStatement(houseBill);

			if (shipment.JS_ShipmentType != ShipmentTypes.BuyersConsolLead)
			{
				var subs = shipment
					.CoLoadShipments
					.Cast<ForwardingShipment>()
					.Select(s => new HouseBillBuilder(s, parameters).Build(enableValidations))
					.ToArray();
				houseBill.SubHouseBills = subs;
			}

			if (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.Value && houseBill.MarksAndNumbers.IsEmpty)
			{
				houseBill.MarksAndNumbers = string.Join(System.Environment.NewLine, houseBill.Containers.SelectMany(container => container.PackingLines.Where(packLine => !packLine.MarksAndNumbers.IsEmpty)).Select(packLine => packLine.MarksAndNumbers));
			}

			if (enableValidations)
			{
				AddValidation(houseBill);
			}
			else if (shipment.IsEditingElectronicBOL || isDraft)
			{
				AddGeneralMandatoryFieldsValidation(houseBill);
			}

			return houseBill;
		}

		ZString RetrieveTariffLineItemReference()
		{
			if (!shipment.JS_FMCTariffID.IsEmpty && !shipment.JS_RH_NKRateCommodity.IsEmpty)
			{
				var rateCommodity = shipment.JS_RH_NKRateCommodity;
				var refCommodityCodeBO = shipment.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, rateCommodity);
				var localCode = refCommodityCodeBO?.RefCommodityCodeMaps?.Where(c => c.LC_LocalCodeProvider == "RAT").FirstOrDefault()?.LC_LocalCode;

				return string.Concat(localCode.HasValue ? localCode : rateCommodity, ".", shipment.JS_FMCTariffID);
			}

			return ZString.Empty;
		}

		ZString RetrieveShippersLoadAndCount(ForwardingShipment shipment)
		{
			var shipperLoadAndCount = ZString.Empty;

			if (!shipment.JS_HBLContainerPackModeOverride.IsEmpty)
			{
				if (shipment.JS_HBLContainerPackModeOverride.StartsWith("CY") || shipment.JS_HBLContainerPackModeOverride.StartsWith("DOOR"))
				{
					shipperLoadAndCount = ShipperLoadAndCountTypes.Codes.ShipperLoadAndCount;
				}
			}
			else if (shipment.Containers.Any(c => c.JC_ContainerMode == DocDataConstants.BillOfLadingTypes.Codes.Collect && (c.JC_DeliveryMode.StartsWith("CY") || c.JC_DeliveryMode.StartsWith("DR"))))
			{
				shipperLoadAndCount = ShipperLoadAndCountTypes.Codes.ShipperLoadAndCount;
			}

			return shipperLoadAndCount;
		}

		#region AddValidation

		void AddValidation(HouseBill houseBill)
		{
			AddBangladeshValidation(houseBill);
			AddIndiaValidation(houseBill);
			AddIndonesiaValidation(houseBill);
			AddEgyptValidation(houseBill);
			AddFIATAHBLValidation(houseBill);
			AddCTKNumberValidation(houseBill);

			if (houseBill.HouseBillOfLadingType.Code == HouseBillOfLadingTypes.Code.FIATAHBL
				|| FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration)
			{
				AddShipperAddressValidation(houseBill);
				AddConsigneeAddressToOrderSupportAndValidation(houseBill);
				AddNotifyPartyAddressSameAsConsigneeSupportAndValidation(houseBill);
			}

			AddPackingLinesValidation(houseBill);
			AddVesselNameAndVoyageValidation(houseBill);

			houseBill.ValidateAllIncludingChildren();
		}

		void AddGeneralMandatoryFieldsValidation(HouseBill houseBill)
		{
			AddShipperAddressValidation(houseBill);
			AddConsigneeAddressToOrderSupportAndValidation(houseBill);
			AddNotifyPartyAddressSameAsConsigneeSupportAndValidation(houseBill);
			AddPackingLinesValidation(houseBill);
			AddVesselNameAndVoyageValidation(houseBill);

			houseBill.ValidateAllIncludingChildren();
		}

		#endregion

		#region MoveTypeList

		const char packModeSeparator = '/';

		CodeDescriptionPairList GetMoveTypeList()
		{
			var codes = new HashSet<ZString>();

			foreach (var entry in shipment.Lookups.JS_HBLContainerPackModeOverride_List)
			{
				if (entry is CodeDescriptionPair codeDescription)
				{
					var moveType = codeDescription.Code.Split(packModeSeparator).ToArray();

					void AddCodeIfNotEmpty(string code)
					{
						if (!string.IsNullOrWhiteSpace(code))
						{
							codes.Add(code);
						}
					}

					AddCodeIfNotEmpty(moveType.FirstOrDefault());
					AddCodeIfNotEmpty(moveType.Skip(1).FirstOrDefault());
				}
			}

			var res = new CodeDescriptionPairList();

			foreach (var code in codes.OrderBy(c => c))
			{
				res.AddPair(code, code);
			}

			return res;
		}

		#endregion

		#region PopulateOverrides

		void PopulateOverrides(HouseBill houseBill)
		{
			StmNote goodsDetailsOverride = null;
			StmNote chargesOverride = null;
			StmNote followOnOverride = null;

			foreach (var note in shipment.Notes.GetAllNotes().Cast<StmNote>())
			{
				if (note.ST_Description == PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride.Code)
				{
					goodsDetailsOverride = note;
				}
				else if (note.ST_Description == PredefinedNoteTypes.Instance.HouseBillChargesOverride.Code)
				{
					chargesOverride = note;
				}
				else if (note.ST_Description == PredefinedNoteTypes.Instance.HouseBillFollowOnOverride.Code)
				{
					followOnOverride = note;
				}

				if (goodsDetailsOverride != null
					&& chargesOverride != null
					&& followOnOverride != null)
				{
					break;
				}
			}

			if (goodsDetailsOverride != null)
			{
				houseBill.GoodsDetailsTextOverride = goodsDetailsOverride.ST_NoteText;
				houseBill.HasGoodsDetailsTextOverride = ZBool.True;
			}

			if (chargesOverride != null)
			{
				houseBill.ChargesTextOverride = chargesOverride.ST_NoteText;
				houseBill.HasChargesTextOverride = ZBool.True;
			}

			if (followOnOverride != null)
			{
				houseBill.FollowOnTextOverride = followOnOverride.ST_NoteText;
				houseBill.HasFollowOnTextOverride = ZBool.True;
			}
		}

		#endregion

		#region Dates

		void PopulateDates(HouseBill houseBill)
		{
			houseBill.DateOfIssue = shipment.JS_HouseBillIssueDate.IsEmpty
				? MainSeaLeg?.JW_ATD ?? ZDateTime.Empty
				: shipment.JS_HouseBillIssueDate;
			houseBill.DepartureDate = shipment.JS_E_DEP;
			houseBill.ArrivalDate = shipment.JS_E_ARV;
		}

		#endregion

		#region Locations

		void PopulateLocations(HouseBill houseBill)
		{
			var manufacturerPortCode = shipment.ManufacturerDocAddress?.Address?.OA_RL_NKRelatedPortCode ?? ZString.Empty;

			houseBill.PortOfOrigin = Unloco.Create(context, shipment.Origin);
			houseBill.PortOfDestination = Unloco.Create(context, shipment.Destination);
			houseBill.PlaceOfIssue = Unloco.Create(context, GlbBranch.CurrentBranch.HomePort);
			houseBill.PlaceOfReceipt = Unloco.Create(context, shipment.Origin);
			houseBill.PlaceOfDelivery = Unloco.Create(context, shipment.Destination);
			houseBill.PortOfLoading = Unloco.Create(context, IsManufacturerHBL && !manufacturerPortCode.IsEmpty ? shipment.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, manufacturerPortCode) : FirstSeaLeg?.LoadPort);
			houseBill.PortOfDischarge = Unloco.Create(context, LastSeaLeg?.DiscPort);
			houseBill.FreightPayableAt = Unloco.Create(context, IsCollect || IsManufacturerHBL ? shipment.Destination : shipment.Origin);
		}

		#endregion

		#region Addresses

		void PopulateAddresses(HouseBill houseBill)
		{
			var shipper = AddressBuilder.Create(context, IsManufacturerHBL ? shipment.ManufacturerDocAddress : shipment.ConsignorDocumentaryAddress);
			houseBill.Shipper = shipper;

			var consignee = AddressBuilder.Create(context, IsManufacturerHBL ? shipment.ConsignorDocumentaryAddress : shipment.ConsigneeDocumentaryAddress);
			houseBill.Consignee = consignee;

			houseBill.ForwardingAgent = AddressBuilder.Create(context, GlbBranch.CurrentBranch.OrgProxy?.MainAddress);

			var deliveryAgent = shipment.DeliveryAgent?.MainAddress;
			houseBill.GoodsDelivery =
				deliveryAgent != null
				? AddressBuilder.Create(context, deliveryAgent) :
				(
					shipment.Consols.Count == 1
					? AddressBuilder.Create(context, shipment.Consols[0].ReceivingForwarderWithContact, true, true)
					: AddressBuilder.Create(context, GetLastConsolReceivingForwarderWithContact(), true, true)
				);

			var notifyParty = AddressBuilder.Create(context, shipment.NotifyPartyDocumentaryAddress);
			houseBill.NotifyParty = notifyParty;

			houseBill.NotifyParty2 = AddressBuilder.Create(context, shipment.NotifyParty2DocumentaryAddress);
			houseBill.NotifyParty3 = AddressBuilder.Create(context, shipment.NotifyParty3DocumentaryAddress);

			houseBill.SendingForwarder = AddressBuilder.Create(context, DepartureConsol?.SendingForwarderWithContact);
			houseBill.ReceivingForwarder = AddressBuilder.Create(context, DepartureConsol?.ReceivingForwarderWithContact);
			houseBill.ColoadWith = CreateColoadWith(false);

			houseBill.ConsignorPickup = AddressBuilder.Create(context, shipment.ConsignorPickupAddress);
			houseBill.ConsigneeDelivery = AddressBuilder.Create(context, shipment.ConsigneeDeliveryAddress);
		}

		ZAddressWithContact GetLastConsolReceivingForwarderWithContact()
		{
			if (shipment.LastDischargeConsol != null)
			{
				return shipment.LastDischargeConsol.ReceivingForwarderWithContact;
			}

			var lastTransport = shipment.TransportsInLegOrder.LastOrDefault() as Freight.Business.Transport;
			return lastTransport == null ? null : shipment.Consols.OfType<ForwardingConsol>().FirstOrDefault(consol => consol.Transports.Any(transport => transport.PK == lastTransport.PK))?.ReceivingForwarderWithContact;
		}

		Address CreateColoadWith(bool includeContactInfo)
		{
			if (DepartureConsol != null)
			{
				if (DepartureConsol.IsCoLoad)
				{
					return AddressBuilder.Create(context, DepartureConsol?.CreditorAddress, includeContactInfo);
				}

				if (DepartureConsol.ShippingLineIsNVOCC)
				{
					return AddressBuilder.Create(context, DepartureConsol?.ShippingLineAddress, includeContactInfo);
				}
			}

			return AddressBuilder.Create(context, true);
		}

		void AddCTKNumberValidation(HouseBill houseBill)
		{
			if (houseBill.IsToSpecificAfricanCountry)
			{
				houseBill.CTKNumberInfo.AddWarningIfEmpty(Res.GetString("58459876-15F0-48A4-A29D-6E6EF548CB1C", "The CTK – Cargo Tracking Note number is required for shipments destined to {0}.\r\nEnter the CTK in the Shipment > Additional Details > Reference Numbers.", GetCountryName(shipment.JS_RL_NKDestination)));
			}
		}

		void AddBangladeshValidation(HouseBill houseBill)
		{
			houseBill.ConsigneeTaxInfo.NumberInfo.AddMessageError(() => IsImportToBangladesh(houseBill) && houseBill.ConsigneeTaxInfo.Number.IsEmpty, BangladeshConsigneeMissingBIN);
			houseBill.NotifyPartyTaxInfo.NumberInfo.AddMessageError(() => IsImportToBangladesh(houseBill) && !houseBill.NotifyParty.CompanyName.IsEmpty && houseBill.NotifyPartyTaxInfo.Number.IsEmpty, BangladeshNotifyPartyMissingBIN);
		}

		void AddIndiaValidation(HouseBill houseBill)
		{
			var consignee = (Address)houseBill.Consignee;
			var notifyParty = (Address)houseBill.NotifyParty;
			var shipper = (Address)houseBill.Shipper;

			houseBill.ConsigneeTaxInfo.NumberInfo.AddWarning(() => !consignee.IsToOrder() && !AddressExtensions.IsToOrder(consignee.AddressFormatted) && HasIndiaConsigneeMissingIECAndPAN(consignee), IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			consignee.EmailInfo.AddWarning(() => HasBothOfIndiaConsigneeAndNotifyPartyMissingEmailAddress(consignee, notifyParty), IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);

			houseBill.NotifyPartyTaxInfo.NumberInfo.AddWarning(() => !notifyParty.IsSameAsConsignee() && !AddressExtensions.IsSameAsConsignee(notifyParty.AddressFormatted) && HasIndiaNotifyPartyMissingIECAndPAN(notifyParty), IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			notifyParty.EmailInfo.AddWarning(() => HasBothOfIndiaConsigneeAndNotifyPartyMissingEmailAddress(consignee, notifyParty), IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);

			houseBill.ShipperTaxInfo.NumberInfo.AddWarning(() => HasIndiaConsignorMissingIECAndPAN(shipper), IndiaConsignorMissingIECOrPanMessage);

			consignee.OnValueChanged(nameof(consignee.Email)).Do(ValidateAllChildren);
			consignee.OnValueChanged(nameof(consignee.CompanyName)).Do(ValidateAllChildren);
			consignee.OnValueChanged(nameof(consignee.AddressFormatted)).Do(ValidateAllChildren);

			notifyParty.OnValueChanged(nameof(notifyParty.Email)).Do(ValidateAllChildren);
			notifyParty.OnValueChanged(nameof(notifyParty.CompanyName)).Do(ValidateAllChildren);
			notifyParty.OnValueChanged(nameof(notifyParty.AddressFormatted)).Do(ValidateAllChildren);

			shipper.OnValueChanged(nameof(shipper.CompanyName)).Do(ValidateAllChildren);

			void ValidateAllChildren()
			{
				houseBill.ValidateAllIncludingChildren();
			}
		}

		void AddIndonesiaValidation(HouseBill houseBill)
		{
			if (houseBill.PortOfDestination.Country.Code == CountryCodes.Indonesia)
			{
				if (houseBill.Consignee.Country.Code == CountryCodes.Indonesia && houseBill.NotifyParty.Country.Code == CountryCodes.Indonesia)
				{
					houseBill.ConsigneeTaxInfo.NumberInfo.AddWarning(() => houseBill.ConsigneeTaxInfo.Number.IsEmpty && houseBill.NotifyPartyTaxInfo.Number.IsEmpty, Res.GetString("95310d4d-9f0d-4c73-aed9-c31e0a2e2567", "Consignee's or Notify party's PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017 for Indonesia, when Consignee and Notify party are in Indonesia."));
					houseBill.NotifyPartyTaxInfo.NumberInfo.AddWarning(() => houseBill.ConsigneeTaxInfo.Number.IsEmpty && houseBill.NotifyPartyTaxInfo.Number.IsEmpty, Res.GetString("f986d9ae-e5d7-4381-bf9c-1dc7bca44205", "Notify party's or Consignee's PPN(NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with  Regulation No. 158/PMK.04/2017 for Indonesia, when Notify party and Consignee are in Indonesia."));
				}
				else if (houseBill.Consignee.Country.Code == CountryCodes.Indonesia)
				{
					houseBill.ConsigneeTaxInfo.NumberInfo.AddWarningIfEmpty(Res.GetString("2403e22d-44bb-4369-9fc2-e159d06f50ce", "Consignee's PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017 for Indonesia, when Notify party is not in Indonesia."));
				}
				else if (houseBill.NotifyParty.Country.Code == CountryCodes.Indonesia)
				{
					houseBill.NotifyPartyTaxInfo.NumberInfo.AddWarningIfEmpty(Res.GetString("7eec7bd7-24ca-43bd-b9f9-f50c05c65f5a", "Notify party's PPN(NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with  Regulation No. 158/PMK.04/2017 for Indonesia, when Consignee is not in Indonesia."));
				}
			}
		}

		void AddEgyptValidation(HouseBill houseBill)
		{
			if (houseBill.PortOfDestination.Country.Code == CountryCodes.Egypt)
			{
				houseBill.ACIDNOInfo.AddWarningIfEmpty(Res.GetString("535D977D-C8D7-4DFB-802A-192E12ED3978", "It is recommended to capture the House ACID to comply with the Advance Cargo Information (ACI) for Egypt."));

				houseBill.ConsigneeTaxInfo.NumberInfo.AddWarningIfEmpty(Res.GetString("FEC2108A-EE18-4581-9116-184F446D41D7", "It is recommended to fill in the Commercial Registration Number to comply with Advance Cargo Information (ACI) for Egypt"));

				houseBill.ShipperTaxInfo.NumberInfo.AddWarningIfEmpty(Res.GetString("7CEFBB92-33CF-4C28-BF03-12C7EFC78374", "It is recommended to fill in the Export Registration Number to comply with Advance Cargo Information (ACI) for Egypt"));
			}
		}

		void AddPackingLinesValidation(HouseBill houseBill)
		{
			var goodsDescriptionErrorMessage = Res.GetString("233b4c8c-edfd-4ecc-980c-6aa899245712", "Goods Description is required.");
			var packageCountErrorMessage = Res.GetString("5debd29e-1848-409e-ae1f-6cccefc83e26", "Package Count is required and cannot be zero.");

			foreach (var container in houseBill.Containers.OfType<Container>())
			{
				container.PackCountInfo.AddMessageError(() => container.PackCount <= 0, packageCountErrorMessage);

				foreach (var packingLine in container.PackingLines.OfType<PackingLine>())
				{
					packingLine.ShortGoodsDescriptionInfo.AddMessageErrorIfEmpty(goodsDescriptionErrorMessage);
				}
			}

			foreach (var packingLine in houseBill.LoosePackingLines.OfType<PackingLine>())
			{
				packingLine.QuantityInfo.AddMessageError(() => packingLine.Quantity <= 0, packageCountErrorMessage);
				packingLine.ShortGoodsDescriptionInfo.AddMessageErrorIfEmpty(goodsDescriptionErrorMessage);
			}
		}

		void AddVesselNameAndVoyageValidation(HouseBill houseBill)
		{
			var transportModes = new List<string> { TransportModes.Sea, TransportModes.SeaAir, TransportModes.AirSea };

			if (transportModes.Contains(shipment.JS_TransportMode))
			{
				var mainTransport = (Transport)houseBill.Transports.Main;

				houseBill.ErrorPlaceHolderInfo.AddMessageError(() => mainTransport == null, Res.GetString("e1065463-3843-4caa-a8a9-303dfb434a6e", "Main Sea transport leg is missing in the Routing tab. Please enter it in Consol or Shipment."));

				if (mainTransport != null)
				{
					var errorMessage = Res.GetString("8224eb9e-1460-4c81-beee-f2bb9456e6fc", "Both Vessel Name and Voyage Number are required.");

					mainTransport.Vessel?.NameInfo.AddMessageErrorIfEmpty(errorMessage);
					mainTransport.VoyageFlightNumberInfo.AddMessageErrorIfEmpty(errorMessage);
				}
			}
		}

		bool IsImportToBangladesh(HouseBill houseBill)
		{
			return CountryCodes.Bangladesh == houseBill.PlaceOfDelivery.Country.Code;
		}

		bool HasIndiaConsignorMissingIECAndPAN(Address consignor)
		{
			return (shipment.Origin?.Country?.Code ?? ZString.Empty) == CountryCodes.India
					 && (consignor.Country?.Code ?? ZString.Empty) == CountryCodes.India
					 && !HasCountryRegNumber(consignor, shipment.Consignor, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN)
					 && !HasCountryRegNumber(consignor, shipment.Consignor, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.IEC);
		}

		bool HasIndiaConsigneeMissingIECAndPAN(Address consignee)
		{
			return (shipment.Destination?.Country?.Code ?? ZString.Empty) == CountryCodes.India
				&& (consignee.Country?.Code ?? ZString.Empty) == CountryCodes.India
				&& !HasCountryRegNumber(consignee, shipment.Consignee, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN)
				&& !HasCountryRegNumber(consignee, shipment.Consignee, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.IEC);
		}

		bool HasIndiaNotifyPartyMissingIECAndPAN(Address notifyParty)
		{
			return (shipment.Destination?.Country?.Code ?? ZString.Empty) == CountryCodes.India
				&& (notifyParty.Country?.Code ?? ZString.Empty) == CountryCodes.India
				&& !HasCountryRegNumber(notifyParty, shipment.NotifyParty, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN)
				&& !HasCountryRegNumber(notifyParty, shipment.NotifyParty, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.IEC);
		}

		bool HasBothOfIndiaConsigneeAndNotifyPartyMissingEmailAddress(Address consignee, Address notifyParty)
		{
			return (shipment.Destination?.Country?.Code ?? ZString.Empty) == CountryCodes.India
				&& consignee.Email.IsEmpty
				&& notifyParty.Email.IsEmpty;
		}

		bool HasCountryRegNumber(Address address, OrgHeader header, ZString countryCode, ZString type)
		{
			var regNum = address.RegistrationNumbers.FirstOrDefault(r => r.CountryOfIssue.Code == countryCode && r.Type.Code == type)?.Value;
			if (!regNum.HasValue)
			{
				if (header != null)
				{
					var govRegNum = header.PrimaryRegistrationNumber.CusCode;
					if (govRegNum != null && govRegNum.OK_CodeType == type)
					{
						regNum = govRegNum.OK_CustomsRegNo;
					}
				}
			}

			return regNum.HasValue && !regNum.Value.IsEmpty;
		}

		#endregion

		#region CountryName

		ZString GetCountryName(ZString unloco)
		{
			var countryCode = unloco.SubstringSafe(0, 2);
			return RefCountry.LoadFromCountryCode(shipment.Factory, countryCode)?.RN_Desc ?? ZString.Empty;
		}

		#endregion

		#region TaxNumbers

		void PopulateTaxNumbers(HouseBill houseBill)
		{
			houseBill.ShipperTaxInfo = GetTaxNumber(houseBill, houseBill.Shipper, new OrgHeaderRegistrationNumberProvider(IsManufacturerHBL ? GetSourceHeader(shipment.ManufacturerDocAddress) : GetSourceHeader(shipment.ConsignorDocumentaryAddress)), TaxNumberType.Shipper);
			houseBill.ConsigneeTaxInfo = GetConsigneeTaxInfo(houseBill);
			houseBill.ConsigneeTaxInfoOriginal = GetConsigneeTaxInfo(houseBill);
			houseBill.NotifyPartyTaxInfo = GetNotifyPartyTaxInfo(houseBill);
			houseBill.NotifyPartyTaxInfoOriginal = GetNotifyPartyTaxInfo(houseBill);
		}

		TaxInfo GetConsigneeTaxInfo(HouseBill houseBill)
		{
			return GetTaxNumber(houseBill, houseBill.Consignee, new OrgHeaderRegistrationNumberProvider(IsManufacturerHBL ? GetSourceHeader(shipment.ConsignorDocumentaryAddress) : GetSourceHeader(shipment.ConsigneeDocumentaryAddress)), TaxNumberType.Consignee);
		}

		TaxInfo GetNotifyPartyTaxInfo(HouseBill houseBill)
		{
			return GetTaxNumber(houseBill, houseBill.NotifyParty, new OrgHeaderRegistrationNumberProvider(GetSourceHeader(shipment.NotifyPartyDocumentaryAddress)), TaxNumberType.NotifyParty);
		}

		TaxInfo GetTaxNumber(HouseBill houseBill, IAddress address, OrgHeaderRegistrationNumberProvider registrationNumberProvider, TaxNumberType taxNumberType)
		{
			return GetTaxNumbers(houseBill, address, registrationNumberProvider, taxNumberType).FirstOrDefault() ?? new TaxInfo();
		}

		IEnumerable<TaxInfo> GetTaxNumbers(HouseBill houseBill, IAddress address, OrgHeaderRegistrationNumberProvider registrationNumberProvider, TaxNumberType taxNumberType)
		{
			if (address.IsEmpty())
			{
				return new List<TaxInfo>();
			}

			var regulatingCountry = taxNumberType == TaxNumberType.Shipper ? houseBill.PortOfOrigin?.Country : houseBill.PortOfDestination?.Country;
			var refDatas = GetTaxInfoFromRefTable(address.Country?.Code ?? ZString.Empty, registrationNumberProvider, shipment.Factory, regulatingCountry?.Code ?? ZString.Empty);
			var requiredTaxInfos = GetTaxInfoFromRefData(houseBill, refDatas, regulatingCountry, taxNumberType).ToList();

			if (houseBill.PlaceOfDelivery.Country.Code == CountryCodes.Egypt)
			{
				var secondaryRegulatingCountry = taxNumberType == TaxNumberType.Shipper ? houseBill.PortOfDestination?.Country : houseBill.PortOfOrigin?.Country;
				var secondaryRefDatas = GetTaxInfoFromRefTable(address.Country?.Code ?? ZString.Empty, registrationNumberProvider, shipment.Factory, secondaryRegulatingCountry?.Code ?? ZString.Empty);
				var secondaryRequiredTaxInfos = GetTaxInfoFromRefData(houseBill, secondaryRefDatas, secondaryRegulatingCountry, taxNumberType).ToList();

				requiredTaxInfos.AddRange(secondaryRequiredTaxInfos);
			}

			return requiredTaxInfos;
		}

		IEnumerable<TaxInfo> GetTaxInfoFromRefData(HouseBill houseBill, List<TaxCodeInformation> refTableData, ICountry country, TaxNumberType taxNumberType)
		{
			refTableData = refTableData?.Where(t => t.DocumentType == TaxRelatedDocumentType.HouseBill).ToList();
			if (refTableData == null || !refTableData.Any())
			{
				return Enumerable.Empty<TaxInfo>();
			}

			var taxCodeInformations = refTableData.OrderBy(t => t.Priority).ToList();
			var nonEmptyList = taxCodeInformations.Where(t => t.Number != ZString.Empty).ToList();
			if (nonEmptyList.Any())
			{
				taxCodeInformations = nonEmptyList;
			}
			var usePriority = taxCodeInformations.First().Priority;
			taxCodeInformations = taxCodeInformations.Where(t => t.Priority == usePriority).ToList();

			return taxCodeInformations.Select(t => GenerateTaxInfo(houseBill, t, country, taxNumberType));
		}

		TaxInfo GenerateTaxInfo(HouseBill houseBill, TaxCodeInformation taxCodeInformation, ICountry country, TaxNumberType taxNumberType)
		{
			return new TaxInfo
			{
				Code = taxCodeInformation.Code,
				Description = taxCodeInformation.Description,
				ShortLabel = taxCodeInformation.ShortLabel,
				LongLabel = taxCodeInformation.LongLabel,
				Number = GetAndFormatTaxNumber(houseBill, taxCodeInformation, country, taxNumberType),
				DisplayedLabel = GetTaxDisplayedLabel(houseBill, taxCodeInformation, taxNumberType),
				Country = (Country)country,
				IsChinaSpecific = taxCodeInformation.RegulatingCountryCode == CountryCodes.China,
				RegulatingCountry = Country.Create(context, RefCountry.LoadFromCountryCode(shipment.Factory, taxCodeInformation.RegulatingCountryCode))
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		ZString GetAndFormatTaxNumber(HouseBill houseBill, TaxCodeInformation taxCodeInformation, ICountry country, TaxNumberType taxNumberType)
		{
			if (houseBill.PlaceOfDelivery.Country.Code == CountryCodes.Egypt && taxCodeInformation.RegulatingCountryCode == CountryCodes.Egypt && taxNumberType == TaxNumberType.Shipper && !string.IsNullOrEmpty(taxCodeInformation.Number))
			{
				if (string.IsNullOrEmpty(taxCodeInformation.Comments))
				{
					return taxCodeInformation.Number;
				}
				var registerType = taxCodeInformation.Comments.EqualsIgnoringCase("Tax Id") ? "02" : "01";

				return $"{taxCodeInformation.CountryCode}-{registerType}-{taxCodeInformation.Number}"; // non-translatable validation message
			}

			return taxCodeInformation.Number;
		}

		ZString GetTaxDisplayedLabel(HouseBill houseBill, TaxCodeInformation taxCodeInformation, TaxNumberType taxNumberType)
		{
			if (!taxCodeInformation.ShortLabel.IsEmpty)
			{
				return taxCodeInformation.ShortLabel;
			}

			return taxCodeInformation.Code;
		}

		OrgHeader GetSourceHeader(object addressSource)
		{
			OrgHeader header = null;
			if (addressSource is JobDocAddress jobDocAddressSource)
			{
				header = jobDocAddressSource.Organisation;
			}
			else if (addressSource is OrgAddress orgAddressSource)
			{
				header = orgAddressSource.Header;
			}
			else if (addressSource is ZAddressWithContact zAddressWithContact)
			{
				header = zAddressWithContact.OrgHeader as OrgHeader;
			}

			return header;
		}

		#endregion

		#region Goods Details (containers, packinglines, dangerous goods)

		void PopulateContainers(HouseBill houseBill)
		{
			var consol = MovementLegComparer.FirstOrDefaultLegForTransportMode(shipment.Consols.Cast<CommonConsol>(), shipment.TransportMode);

			var containerBizObjs = consol
				?.Containers
				.OfType<ForwardingContainer>()
				.OrderBy(container => container.JC_ContainerNum)
				.ToArray();

			var containers = containerBizObjs?.Length > 0
				? CreateContainers(containerBizObjs)
				: Array.Empty<Container>();

			var allPackingLines = containers
				.SelectMany(c => c.PackingLines)
				.ToArray();

			houseBill.Containers = containers;

			if (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.Value)
			{
				houseBill.TotalPackCount = allPackingLines.Sum(p => p.Quantity);
			}
			else
			{
				houseBill.TotalPackCount = shipment.JS_OuterPacks;
			}

			if (!allPackingLines.TryGetPackTypeCode(out var packTypeCode))
			{
				packTypeCode = shipment.JS_F3_NKPackType;
			}

			houseBill.TotalPackType = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = packTypeCode
			};
		}

		Container[] CreateContainers(ForwardingContainer[] containerBizObjs)
		{
			var containers = new List<Container>();

			var containerBuilder = new ContainerBuilder();
			var packlineBuilder = new PackingLineBuilder();

			var shipmentPks = GetPksFromShipmentAndAllSubShipmentsWithoutChildren();

			foreach (var containerBizObj in containerBizObjs)
			{
				ForwardingPackLine[] packingLineBizObjs;

				packingLineBizObjs = containerBizObj
					.PackLines
					.OfType<ForwardingPackLine>()
					.Where(p => p.JL_JS.In(shipmentPks))
					.ToArray();

				if (packingLineBizObjs.Length == 0)
				{
					continue;
				}

				var containerID = $"{containerBizObj.PK}-{shipment.JS_UniqueConsignRef}"; // Non-translatable Identifier
				var container = containerBuilder.Build(containerBizObj, context, containerID: containerID);

				var packingLines = packingLineBizObjs
					.Select(packLine =>
					{
						var packingLine = packlineBuilder.Build(packLine, packingLineIdentifier: $"{packLine.PK}-{containerID}");
						packingLine.ContainerNumber = container.Number;

						return packingLine;
					});

				var orderedPackingLines = GetOrderedPackingLines(packingLines);
				container.PackingLines = orderedPackingLines;

				container.GoodsWeight = CreateContainerGoodsWeight(orderedPackingLines);
				container.Volume = CreateContainerVolume(orderedPackingLines);
				container.PackCount = orderedPackingLines.Sum(p => p.Quantity);
				container.PackType = new CodeDescription(shipment.Lookups.PackTypes)
				{
					Code = orderedPackingLines.GetPackTypeCode()
				};
				container.IsEmpty = false;

				containers.Add(container);
			}

			Container[] orderedContainers;

			if (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.Value == HBLPackLinesDisplayOrders.ShowDGCargoFirst)
			{
				orderedContainers = containers.OrderByDescending(container => container.PackingLines.FirstOrDefault()?.DangerousGoods.Any() ?? false)
					.ThenBy(container => container.Number).ToArray();
			}
			else
			{
				orderedContainers = containers.ToArray();
			}

			if (!FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.Value)
			{
				orderedContainers.ForEach(container => container.PackingLines = Array.Empty<PackingLine>());
			}

			return orderedContainers;
		}

		Measurement CreateContainerGoodsWeight(PackingLine[] packingLines)
		{
			var unitOfWeight = packingLines.HaveSameUnitOfWeight()
				? (packingLines[0].Weight?.Unit?.Code.ToString() ?? Constants.Weight.Kilograms)
				: Constants.Weight.Kilograms;

			var weight = packingLines.Sum(p => Constants.Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, unitOfWeight));

			return new Measurement
			{
				Value = weight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};
		}

		Measurement CreateContainerVolume(PackingLine[] packingLines)
		{
			var unitOfVolume = packingLines.HaveSameUnitOfVolume()
				? (packingLines[0].Volume?.Unit?.Code.ToString() ?? Constants.Volume.CubicMetres)
				: Constants.Volume.CubicMetres;

			var volume = packingLines.Sum(p => Constants.Volume.Convert(p.Volume.Value, p.Volume.Unit.Code, unitOfVolume));

			return new Measurement
			{
				Value = volume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};
		}

		void PopulateLoosePackingLines(HouseBill houseBill)
		{
			var packLineBuilder = new PackingLineBuilder();

			var shipmentPks = GetPksFromShipmentAndAllSubShipmentsWithoutChildren();

			var loosePackingLines = shipment
				.OuterPackLines
				.OfType<ForwardingPackLine>()
				.Where(p => !p.Containers.Any() && p.JL_JS.In(shipmentPks))
				.Select(packLine => packLineBuilder.Build(packLine));

			var orderedLoosePackingLines = GetOrderedPackingLines(loosePackingLines);
			houseBill.LoosePackingLines = orderedLoosePackingLines;
			houseBill.TotalLoosePackCount = orderedLoosePackingLines.Sum(p => p.Quantity);

			if (!orderedLoosePackingLines.TryGetPackTypeCode(out var packTypeCode))
			{
				packTypeCode = shipment.JS_F3_NKPackType;
			}

			houseBill.TotalLoosePackType = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = packTypeCode
			};

			if (houseBill.HouseBillOfLadingType.Code == HouseBillOfLadingTypes.Code.FIATAHBL)
			{
				houseBill.TotalPackCount += houseBill.TotalLoosePackCount;
			}
		}

		HashSet<ZGuid> GetPksFromShipmentAndAllSubShipmentsWithoutChildren()
		{
			var shipmentPks = new HashSet<ZGuid>();
			shipmentPks.Add(shipment.PK);

			foreach (var subShipmentPK in shipment.GetPksFromAllSubShipmentsWithoutChildren())
			{
				shipmentPks.Add(subShipmentPK);
			}

			return shipmentPks;
		}

		PackingLine[] GetOrderedPackingLines(IEnumerable<PackingLine> originalPackingLines)
		{
			if (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.Value == HBLPackLinesDisplayOrders.ShowDGCargoFirst)
			{
				return originalPackingLines.OrderByDescending(packingLine => packingLine.DangerousGoods.Any())
					.ThenBy(packingLine => packingLine.PackingOrder)
					.ThenBy(packingLine => packingLine.PackingLineID).ToArray();
			}

			return originalPackingLines
				.OrderBy(packingLine => packingLine.PackingOrder)
				.ThenBy(packingLine => packingLine.PackingLineID).ToArray();
		}

		#endregion

		#region Consols

		void PopulateConsols(HouseBill houseBill)
		{
			houseBill.Consols = new ForwardingConsolDataObjectsCollection(shipment);
		}

		#endregion

		#region Orders

		void PopulateOrderReferences(HouseBill houseBill)
		{
			houseBill.OrderReferences = shipment
				.DocsAndCartage
				.OrderItems
				.OfType<OrderItem>()
				.Select(item => item.JT_OrderReference.ToString())
				.ToArray();
		}

		void PopulateOrders(HouseBill houseBill)
		{
			houseBill.Orders = shipment
				.AttachedOrders
				.Select(order => CreateOrderDataObject(order))
				.ToArray();
		}

		OrderDataObject CreateOrderDataObject(Order order)
		{
			return new OrderDataObject(order.PK)
			{
				OrderNumber = order.JD_OrderNumber,
				OrderDate = order.JD_OrderDate,
				OrderLines = order?
				.OrderLines
				.Select(orderLine => CreateOrderLineDataObject(orderLine))
				.ToArray()
			};
		}

		OrderLineDataObject CreateOrderLineDataObject(OrderLine orderLine)
		{
			return new OrderLineDataObject(orderLine.PK)
			{
				LineNumber = orderLine.JO_LineNo,
				Description = orderLine.JO_Description,
				OuterPacks = orderLine.JO_OuterPacks,
				InnerPacks = orderLine.JO_InnerPacks,
				TotalInnerPacks = orderLine.JO_TotalInnerPacks,
				QuantityOrdered = new Measurement
				{
					Value = orderLine.JO_Quantity,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				QuantityInvoiced = new Measurement
				{
					Value = orderLine.JO_QtyInvoiced,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				QuantityReceived = new Measurement
				{
					Value = orderLine.JO_QtyReceived,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				QuantityRemaining = new Measurement
				{
					Value = orderLine.JO_QuantityRemaining,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				ItemPrice = new Money
				{
					Amount = orderLine.JO_ItemPrice,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = orderLine.Order.JD_Calc_Currency.Currency
					}
				},
				TotalLinePrice = new Money
				{
					Amount = orderLine.JO_LinePrice,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = orderLine.Order.JD_Calc_Currency.Currency
					}
				},
				RequiredDate = orderLine.JO_LineDropDate,
				Status = new CodeDescription(orderLine.JO_LineStatus_List)
				{
					Code = orderLine.JO_LineStatus
				},
				Product = new CodeDescription(orderLine.JO_Partno_List)
				{
					Code = orderLine.JO_Partno
				},
				OrderNumber = orderLine.Order.JD_OrderNumberAndSplit
			};
		}

		#endregion

		#region Charges

		void PopulateCharges(HouseBill houseBill)
		{
			houseBill.Charges = new ChargesCollection(shipment, Lookups, houseBill.IsOriginal, true);
		}

		#endregion

		#region ReferenceNumbers

		void PopulateReferenceNumbers(HouseBill houseBill)
		{
			var numberTypes = CreateNumberTypesLookup();

			houseBill.ReferenceNumbers = shipment
				.Numbers
				.OfType<CusEntryNumber>()
				.Select(n => CreateReferenceNumber(n, numberTypes))
				.ToArray();
		}

		IReferenceNumber CreateReferenceNumber(CusEntryNumber cusEntryNumber, ICodeDescriptionPairList numberTypes)
		{
			return new ReferenceNumber
			{
				Value = cusEntryNumber.CE_EntryNum,
				CountryOfIssue = new Country(shipment.Factory, lookups.Countries)
				{
					Code = cusEntryNumber.CE_RN_NKCountryCode
				},
				Type = new CodeDescription(numberTypes)
				{
					Code = cusEntryNumber.CE_EntryType
				}
			};
		}

		ICodeDescriptionPairList CreateNumberTypesLookup()
		{
			var list = new CodeDescriptionPairList();

			foreach (var number in shipment.Numbers.OfType<CusEntryNumber>())
			{
				list.AddPairIfNotExist(number.CE_EntryType, number.Lookups.AdditionalReferenceNumberTypes.GetDescriptionFromCode(number.CE_EntryType));
			}

			return list;
		}

		#endregion

		#region Notes

		void PopulateNotes(HouseBill houseBill)
		{
			houseBill.Notes = shipment
				.Notes
				.GetAllNotes()
				.Cast<StmNote>()
				.Select(note => new Note()
				{
					Text = note.ST_NoteDataAsText,
					Description = note.ST_Description
				})
				.ToArray();
		}

		#endregion

		#region Numbers

		void PopulateNumbers(HouseBill houseBill)
		{
			if (houseBill.PortOfDestination.Country.Code == CountryCodes.Egypt)
			{
				houseBill.ACIDNO = string.Join(", ", shipment?.Numbers?.GetAllReferenceNumbersByTypeAndCountry(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference, CountryCodes.Egypt));

				if (!string.IsNullOrEmpty(houseBill.ACIDNO))
				{
					houseBill.ACIDNO = (NoResString)"ACID No: " + houseBill.ACIDNO; // label
				}
			}
		}

		#endregion

		#region ExportStatement

		void PopulateExportStatement(HouseBill houseBill)
		{
			// User Defined
			var exportStatementSetting = shipment.ExportStatementSetting;
			if (exportStatementSetting != null && exportStatementSetting.UseOnHouseBillOfLading && !shipment.ExportStatement.IsEmpty)
			{
				houseBill.ExportStatement = shipment.ExportStatement;
			}

			// Mandatory
			var country = shipment.Origin?.Code.Left(2);
			if (!string.IsNullOrEmpty(country))
			{
				foreach (ExportStatementSetting mandatorySetting in FreightDataRegistry.Instance.ExportStatementSettings.Value.GetMandatoryStatements(country))
				{
					if (mandatorySetting.UseOnHouseBillOfLading && !mandatorySetting.Statement.IsEmpty)
					{
						if (!houseBill.ExportStatement.IsEmpty)
						{
							houseBill.ExportStatement += "\r\n";
						}
						houseBill.ExportStatement += mandatorySetting.Statement;
					}
				}
			}
		}

		#endregion

		#region Validation

		void AddFIATAHBLValidation(HouseBill houseBill)
		{
			if (houseBill.HouseBillOfLadingType.Code == HouseBillOfLadingTypes.Code.FIATAHBL)
			{
				houseBill.DateOfIssueInfo.AddMessageErrorIfEmpty(Res.GetString("4e7e2bbe-a8a3-4104-a706-3b14410b5dfb", @"Date of Issue is mandatory as per FIATA requirement.
Please verify in Shipment > Basic Registration > Issue Date."));

				if (houseBill.PlaceOfIssue is Unloco placeOfIssue)
				{
					placeOfIssue.CodeInfo.AddMessageError(() => !placeOfIssue.Name.IsEmpty && placeOfIssue.Code.IsEmpty
						, Res.GetString("40f21204-1498-467d-a1b5-f206b7da1f2d", "Port Code is required, if Port Name is entered."));
					placeOfIssue.NameInfo.AddMessageErrorIfEmpty(Res.GetString("42ad21be-562f-4a60-9bfd-ae5bed7cdbd5", @"Place of Issue is mandatory as per FIATA requirement.
Please enter this field or verify in current login Branch > Branch Details > Home Port."));
					placeOfIssue.AddValidationDependencies(placeOfIssue.CodeInfo, placeOfIssue.NameInfo);
				}

				AddFIATAStandardAddressValidation(houseBill.Consignee as Address);
				AddFIATAStandardAddressValidation(houseBill.NotifyParty as Address);
				AddFIATAStandardAddressValidation(houseBill.GoodsDelivery as Address);
				AddFIATAStandardAddressValidation(houseBill.Shipper as Address);
			}
		}

		void AddShipperAddressValidation(HouseBill houseBill)
		{
			var shipper = houseBill.Shipper as Address;
			shipper?.AddressFormattedInfo.AddMessageError(() => shipper.IsPartyNameAndAddressEmpty(), Res.GetString("9dee2703-5444-4850-a433-0c4b603f07bf", "Shipper party name and address information is required."));
		}

		void AddConsigneeAddressToOrderSupportAndValidation(HouseBill houseBill)
		{
			var consignee = houseBill.Consignee as Address;
			var notifyParty = houseBill.NotifyParty as Address;

			consignee.AddToOrderSupport();

			consignee.AddressFormattedInfo.AddMessageError(() => (consignee.IsToOrder() || (string.IsNullOrWhiteSpace(consignee.CompanyName) || string.IsNullOrWhiteSpace(consignee.AddressLine1) || string.IsNullOrWhiteSpace(consignee.Country?.Name)))
				&& (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || notifyParty.IsSameAsConsignee()), Res.GetString("40e01b6a-2241-4543-a8cf-d85af0455426", "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE."));
			consignee.AddValidationDependencies(consignee.AddressFormattedInfo, notifyParty.CompanyNameInfo);
			consignee.AddAddressFormattedValidationDependencies();

			consignee.CompanyNameInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				var args = e as ValueChangedEventArgs;

				if (args == null)
				{
					return;
				}

				var oldValue = args.OldValue.ToString();
				var newValue = args.NewValue.ToString();

				if (AddressExtensions.IsToOrder(oldValue) && !AddressExtensions.IsToOrder(newValue))
				{
					CopyTaxInfoOriginalToTaxInfo(houseBill.ConsigneeTaxInfo, houseBill.ConsigneeTaxInfoOriginal);
				}
				else if (!AddressExtensions.IsToOrder(oldValue) && AddressExtensions.IsToOrder(newValue))
				{
					CleanUpTaxInfoWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(houseBill);
				}
			};
		}

		void AddNotifyPartyAddressSameAsConsigneeSupportAndValidation(HouseBill houseBill)
		{
			var notifyParty = houseBill.NotifyParty as Address;
			var consignee = houseBill.Consignee as Address;

			notifyParty.AddSameAsConsigneeSupport();

			notifyParty.AddressFormattedInfo.AddMessageError(() => (notifyParty.IsSameAsConsignee() || (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || string.IsNullOrWhiteSpace(notifyParty.AddressLine1) || string.IsNullOrWhiteSpace(notifyParty.Country?.Name)))
				&& (string.IsNullOrWhiteSpace(consignee.CompanyName) || consignee.IsToOrder()), Res.GetString("1985b20a-2a78-4b60-8ed7-52a9140aa758", "Notify Party name and address information is required, when Consignee is empty or TO ORDER."));
			notifyParty.AddValidationDependencies(notifyParty.AddressFormattedInfo, consignee.CompanyNameInfo);
			notifyParty.AddAddressFormattedValidationDependencies();

			notifyParty.CompanyNameInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				var args = e as ValueChangedEventArgs;

				if (args == null)
				{
					return;
				}

				var oldValue = args.OldValue.ToString();
				var newValue = args.NewValue.ToString();

				if (AddressExtensions.IsSameAsConsignee(oldValue) && !AddressExtensions.IsSameAsConsignee(newValue))
				{
					CopyTaxInfoOriginalToTaxInfo(houseBill.NotifyPartyTaxInfo, houseBill.NotifyPartyTaxInfoOriginal);
				}
				else if (!AddressExtensions.IsSameAsConsignee(oldValue) && AddressExtensions.IsSameAsConsignee(newValue))
				{
					CleanUpTaxInfoWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(houseBill);
				}
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		void AddFIATAStandardAddressValidation(Address address)
		{
			if (address != null)
			{
				bool IsValidEmail()
				{
					// TODO: WI00749813 - Investigate usages of Email Validation RegEx
					const string pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
					return address.Email.Length > 6
						&& new Regex(pattern, RegexOptions.IgnoreCase).Match(address.Email).Success;
				}
				address.EmailInfo.AddMessageError(() => !address.Email.IsEmpty && !IsValidEmail()
					, Res.GetString("b31e509f-1d80-4657-b798-d4e96effc9ef", "Please enter a valid email. Email must contain at least 6 characters, at least one dot '.' after '@' with at least one character in between and at least 2 characters after the dot. Email can only contain alphanumeric characters and '_', '-', '@', '.'."));

				if (address.Country != null)
				{
					address.AddressFormattedInfo.AddMessageError(() => !address.Country.Name.IsEmpty && address.Country.Code.IsEmpty, Res.GetString("b9a3ef69-6e76-4a1b-9a5d-81ebb186eac9", "Country code is required, if Country is entered."));
					address.AddressFormattedInfo.AddMessageError(() => (!address.AddressLine1.IsEmpty || !address.AddressLine2.IsEmpty || !address.City.IsEmpty || !address.State.IsEmpty || !address.Country.Code.IsEmpty || !address.Postcode.IsEmpty)
						&& address.Country.Name.IsEmpty, Res.GetString("619d641b-a0c4-424f-9952-a3b35d7f86e6", "Country is mandatory as per FIATA requirement."));
				}

				address.AddAddressFormattedValidationDependencies();
			}
		}

		void CopyTaxInfoOriginalToTaxInfo(TaxInfo taxInfo, TaxInfo taxInfoOriginal)
		{
			taxInfo.Code = taxInfoOriginal.Code;
			if (taxInfo.Country != null && taxInfoOriginal.Country != null)
			{
				taxInfo.Country.Code = taxInfoOriginal.Country.Code;
				taxInfo.Country.Name = taxInfoOriginal.Country.Name;
			}

			if (taxInfo.RegulatingCountry != null && taxInfoOriginal.RegulatingCountry != null)
			{
				taxInfo.RegulatingCountry.Code = taxInfoOriginal.RegulatingCountry.Code;
				taxInfo.RegulatingCountry.Name = taxInfoOriginal.RegulatingCountry.Name;
			}

			taxInfo.Description = taxInfoOriginal.Description;
			taxInfo.LongLabel = taxInfoOriginal.LongLabel;
			taxInfo.ShortLabel = taxInfoOriginal.ShortLabel;
			taxInfo.Number = taxInfoOriginal.Number;
			taxInfo.DisplayedLabel = taxInfoOriginal.DisplayedLabel;
			taxInfo.IsChinaSpecific = taxInfoOriginal.IsChinaSpecific;
			taxInfo.IsPlaceHolder = taxInfoOriginal.IsPlaceHolder;
		}

		void CleanUpTaxInfoWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(HouseBill houseBill)
		{
			if ((houseBill.Consignee as Address).IsToOrder() && houseBill.ConsigneeTaxInfo != null)
			{
				houseBill.ConsigneeTaxInfo.Clear();
			}

			if ((houseBill.NotifyParty as Address).IsSameAsConsignee() && houseBill.NotifyPartyTaxInfo != null)
			{
				houseBill.NotifyPartyTaxInfo.Clear();
			}
		}

		#endregion

		#region Implementation

		HouseBillLookups Lookups => lookups ?? (lookups = new HouseBillLookups(shipment.Factory));
		HouseBillLookups lookups;

		bool IsCollect
		{
			get
			{
				if (isCollect == null)
				{
					var incoTerms = RatingDataRegistry.Instance.IncoTermDefinition.Value;
					isCollect = incoTerms
						.OfType<IncoTermChargeCodes>()
						.Any(incoChargeCode => shipment.JS_INCO == incoChargeCode.IncoTerm && incoChargeCode.Freight == Constants.PaymentParty.Consignee);
				}

				return isCollect.Value;
			}
		}

		bool? isCollect;

		bool IsManufacturerHBL => shipment.JS_ShipmentType == ShipmentTypes.ThirdPartyOwnershipHouse && parameters.DataStoreName.StartsWith("ManufacturerBillOfLading"); // special menu name

		ForwardingConsol DepartureConsol => departureConsol ?? (departureConsol = shipment.DepartureConsol);
		ForwardingConsol departureConsol;

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInPortOrder
		{
			get
			{
				if (seaTransportsInPortOrder == null)
				{
					var sortedTransports = shipment
						.TransportsIncludingRelated
						.OfType<Freight.Business.Transport>()
						.Where(transport => transport.JW_TransportMode == TransportModes.Sea)
						.ToArray();
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
					seaTransportsInPortOrder = sortedTransports;
				}

				return seaTransportsInPortOrder;
			}
		}
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInPortOrder;

		Freight.Business.Transport MainSeaLeg
		{
			get
			{
				if (mainSeaLeg == null)
				{
					mainSeaLeg = SeaTransportsInPortOrder
						.Where(t => !t.IsLegFromNonDirectConsolAttachedToDirectShipment(shipment)) // WI00495056 - Attach DRT shipments to AGT Consols, MainSeaLeg of DRT Shipment should be from DRT Consol.
						.FirstOrDefault(t => t.JW_TransportType == Constants.TransportPlanningType.MainVessel);
				}

				return mainSeaLeg;
			}
		}
		Freight.Business.Transport mainSeaLeg;

		Freight.Business.Transport FirstSeaLeg => firstSeaLeg ?? (firstSeaLeg = SeaTransportsInPortOrder.FirstOrDefault());
		Freight.Business.Transport firstSeaLeg;

		Freight.Business.Transport LastSeaLeg => lastSeaLeg ?? (lastSeaLeg = SeaTransportsInPortOrder.LastOrDefault());
		Freight.Business.Transport lastSeaLeg;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage = "For India, the Consignee and/or Notify Party require a contact email address. Ensure this information is included.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string IndiaConsignorMissingIECOrPanMessage = "For India, the Shipper requires the IEC or PAN Number. Ensure this information is included.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage = "For India, the Consignee and/or Notify Party requires the IEC or PAN Numbers together with a contact email address. Ensure this information is included.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string BangladeshConsigneeMissingBIN = "The Consignee VAT (BIN) number is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string BangladeshNotifyPartyMissingBIN = "The Notify Party VAT (BIN) number is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";

		#endregion
	}
}
