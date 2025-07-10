using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Business.RequiredTaxNumbers;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using Notes = Enterprise.ZArchitecture.Business.Notes;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippingInstructionBuilder : CarrierMessageDataBuilder
	{
		public ShippingInstructionBuilder(ForwardingConsol consol)
			: base(consol)
		{
		}

		protected override void GetGoodsHandlingInstructionsFromNotesWithFallingBack(CarrierMessageData wrapper)
		{
			wrapper.GoodsHandlingInstructions = GetNoteTextByDescription(consol.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);

			if (wrapper.GoodsHandlingInstructions.IsEmpty)
			{
				if (wrapper.IsDirect)
				{
					var visibleNotes = DirectShipment?.Notes.VisibleNotes;
					var shipmentNote = GetNoteByDescription(DirectShipment?.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);

					if (visibleNotes != null)
					{
						if (visibleNotes.Contains(shipmentNote))
						{
							wrapper.GoodsHandlingInstructions = shipmentNote?.ST_NoteDataAsText ?? ZString.Empty;
						}

						if (wrapper.GoodsHandlingInstructions.IsEmpty)
						{
							var shipmentConsignorNote = GetNoteByDescription(DirectShipment?.Consignor?.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);

							if (visibleNotes.Contains(shipmentConsignorNote))
							{
								wrapper.GoodsHandlingInstructions = shipmentConsignorNote?.ST_NoteDataAsText ?? ZString.Empty;
							}
						}
					}
				}
				else
				{
					var visibleNotes = consol.Notes.VisibleNotes;
					var shippingLineNote = GetNoteByDescription(consol.ShippingLine?.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);

					if (visibleNotes.Contains(shippingLineNote))
					{
						wrapper.GoodsHandlingInstructions = shippingLineNote?.ST_NoteDataAsText ?? ZString.Empty;
					}

					if (wrapper.GoodsHandlingInstructions.IsEmpty)
					{
						var sendingForwarderNote = GetNoteByDescription(consol.SendingForwarder?.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);

						if (visibleNotes.Contains(sendingForwarderNote))
						{
							wrapper.GoodsHandlingInstructions = sendingForwarderNote?.ST_NoteDataAsText ?? ZString.Empty;
						}
					}
				}
			}
		}

		protected override void PopulateSpecialInstructions(CarrierMessageData wrapper)
		{
			wrapper.SpecialInstructions = GetNoteTextByDescription(consol.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description);

			if (wrapper.SpecialInstructions.IsEmpty)
			{
				if (wrapper.IsDirect)
				{
					var visibleNotes = DirectShipment?.Notes.VisibleNotes;
					var shipmentNote = GetNoteByDescription(DirectShipment?.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description);

					if (visibleNotes != null)
					{
						if (visibleNotes.Contains(shipmentNote))
						{
							wrapper.SpecialInstructions = shipmentNote?.ST_NoteDataAsText ?? ZString.Empty;
						}

						if (wrapper.SpecialInstructions.IsEmpty)
						{
							var shipmentConsignorNote = GetNoteByDescription(DirectShipment?.Consignor?.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description);

							if (visibleNotes.Contains(shipmentConsignorNote))
							{
								wrapper.SpecialInstructions = shipmentConsignorNote?.ST_NoteDataAsText ?? ZString.Empty;
							}
						}
					}
				}
				else
				{
					var visibleNotes = consol.Notes.VisibleNotes;
					var shippingLineNote = GetNoteByDescription(consol.ShippingLine?.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description);

					if (visibleNotes.Contains(shippingLineNote))
					{
						wrapper.SpecialInstructions = shippingLineNote?.ST_NoteDataAsText ?? ZString.Empty;
					}

					if (wrapper.SpecialInstructions.IsEmpty)
					{
						var sendingForwarderNote = GetNoteByDescription(consol.SendingForwarder?.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description);

						if (visibleNotes.Contains(sendingForwarderNote))
						{
							wrapper.SpecialInstructions = sendingForwarderNote?.ST_NoteDataAsText ?? ZString.Empty;
						}
					}
				}
			}
		}

		StmNote GetNoteByDescription(Notes notes, string description)
		{
			return notes?.FindByDescription(description)?.FirstOrDefault();
		}

		protected override CarrierMessageData GetNewCarrierMessageData()
		{
			return new CarrierMessageData(nameof(ForwardingConsol),
				consol.CarrierShipperReferenceWithFallback,
				DataContext.ShippingInstruction);
		}

		protected override void PopulateCharges(CarrierMessageData wrapper)
		{
			base.PopulateCharges(wrapper);

			if (!consol.JK_PrepaidCollect.IsEmpty)
			{
				wrapper.IsFreightPrepaid = consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid;
				wrapper.IsFreightCollect = consol.JK_PrepaidCollect == Constants.PaymentType.Collect;
			}
		}

		protected override void PopulateAdditionalData(CarrierMessageData wrapper)
		{
			base.PopulateAdditionalData(wrapper);
			PopulateGoodsValue(wrapper);
			PopulateBillOfLadingNumber(wrapper);
			PopulateCountrySpecificFields(wrapper);
			PopulateOtherBillClauses(wrapper);
			PopulateRegistrationNumbers(wrapper);
			PopulatePorts(wrapper);
			PopulateICS2(wrapper);
			PopulatePackingLineHBLPaymentTypeEvents(wrapper);
		}

		void PopulateICS2(CarrierMessageData wrapper)
		{
			wrapper.IsShowICS2 = consol.IsICS2;

			if (wrapper.IsShowICS2)
			{
				var selfFilerAddress = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);
				var eoriNumber = selfFilerAddress?.Address.GetEoriNumber(context, useDefaultCountry: false);
				wrapper.ICS2DeclarantEORINumber = ZString.Empty;

				if (eoriNumber != null && !eoriNumber.Value.IsEmpty)
				{
					if (!eoriNumber.CountryOfIssue.Code.EqualsIgnoringCase(eoriNumber.Value.SubstringSafe(0, 2))
						&& !(eoriNumber.CountryOfIssue.Code.EqualsIgnoringCase(CountryCodes.UnitedKingdom) && eoriNumber.Value.StartsWith("XI", StringComparison.OrdinalIgnoreCase)))
					{
						wrapper.ICS2DeclarantEORINumber = (eoriNumber.CountryOfIssue.Code + eoriNumber.Value).ToUpper();
					}
					else
					{
						wrapper.ICS2DeclarantEORINumber = eoriNumber.Value.ToUpper();
					}
				}

				wrapper.OnValueChanged(nameof(wrapper.ICS2DeclarantEORINumber)).Do(wrapper.ValidateAllIncludingChildren);
			}
		}

		bool IsEORIMissingAgainstSelfFilerOrg()
		{
			var selfFilerAddress = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);
			if (selfFilerAddress == null)
			{
				return false;
			}

			var eoriNumber = selfFilerAddress.Address.GetEoriNumber(context, useDefaultCountry: false);
			return eoriNumber.Value.IsEmpty;
		}

		bool IsValidateEORI(ZString eori)
		{
			var isValidateEORI = false;
			if (Regex.IsMatch(eori, @"^[A-Z]{2}[\x21-\x7E]{1,15}$"))
			{
				var countryCode = eori.Left(2);
				if (consol.Factory.LoadFromNaturalKey<IRefCountry>(RefCountrySchema.RN_Code, countryCode) != null)
				{
					isValidateEORI = true;
				}
			}

			return isValidateEORI;
		}
		void AddICS2Validations(CarrierMessageData wrapper)
		{
			var warning1 = (NoResString)"Please verify ICS2 filing method before transmitting this Shipping Instruction to the carrier.\r\n - 'Carrier Filing' requires this field to be blank and necessary HBL data required by ICS2 will be shared with the carrier.\r\n - ‘Self Filing’ requires user to include the EORI of next filing party in this field and no HBL information will be shared with the carrier, but self-filing a partial ENS with ICS2 will be required."; // non-translatable validation message
			var warning2 = (NoResString)"The EORI number consists of:\r\n - A country code of the issuing Member State (2 letters); followed by\r\n - An identifier that is unique in the Member State (up to 15 alphanumeric characters)."; // non-translatable validation message
			var eorIsMissingAgainstSelfFilerOrgWarning = (NoResString)"EORI number is missing in your Self-Filer organization. Please verify in Consol > Addresses > Self-Filer > Config > Registration Numbers/Codes."; // non-translatable validation message

			if (IsEORIMissingAgainstSelfFilerOrg())
			{
				wrapper.ICS2DeclarantEORINumberInfo.AddWarning(() => IsValidateEORI(wrapper.ICS2DeclarantEORINumber), warning1 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);
				wrapper.ICS2DeclarantEORINumberInfo.AddWarning(() => !IsValidateEORI(wrapper.ICS2DeclarantEORINumber), warning1 + System.Environment.NewLine + warning2 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);
			}
			else
			{
				wrapper.ICS2DeclarantEORINumberInfo.AddWarning(() => IsValidateEORI(wrapper.ICS2DeclarantEORINumber), warning1);
				wrapper.ICS2DeclarantEORINumberInfo.AddWarning(() => !IsValidateEORI(wrapper.ICS2DeclarantEORINumber), warning1 + System.Environment.NewLine + warning2);
			}
		}

		protected override void AddValidation(CarrierMessageData wrapper)
		{
			base.AddValidation(wrapper);
			AddBookingReferenceValidation(wrapper);
			AddPlaceAndDateOfIssueValidation(wrapper);
			AddAddressValidation(wrapper);
			CleanUpTaxNumbersWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(wrapper);
			AddTaxNumberValidation(wrapper);
			AddContainerModeValidation(wrapper);
			AddTransportsValidation(wrapper);
			AddPortsValidation(wrapper);
			AddShipmentsValidation(wrapper);
			AddUSOrCAValidations(wrapper);
			AddContainersValidation(wrapper);
			AddGoodsValueValidationAndSuppression(wrapper);
			AddShippingLineMessagingRequirementsValidation(wrapper, x => x.RSR_IsShippingInstruction);
			AddNumberOfOriginalsAndReleaseTypeValidation(wrapper);
			AddExportStatementFieldsValidation(wrapper);
			AddICS2Validations(wrapper);
		}

		void PopulatePorts(CarrierMessageData wrapper)
		{
			wrapper.OperationalPort = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = consol.JK_RL_NKLoadPort
			}.WithCustomNameProvider(GetDetailedPortName);
			wrapper.FreightPayableAt = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = wrapper.FreightPayer?.Unloco != null && !wrapper.FreightPayer.Unloco.Code.IsEmpty
					? wrapper.FreightPayer.Unloco.Code
					: consol.JK_PrepaidCollect == Constants.PaymentType.Collect
						? consol.JK_RL_NKDischargePort
						: consol.JK_RL_NKLoadPort
			}.WithCustomNameProvider(GetDetailedPortName);
		}

		protected override void PopulateBOLDocumentationProvider(CarrierMessageData wrapper)
		{
			base.PopulateBOLDocumentationProvider(wrapper);

			if (wrapper.ElectronicBillOfLadingProviderMandatory)
			{
				if (consol.IsDirect && (!consol.DirectShipment?.Consignor?.MiscServ?.OM_FWRequiresElectronicBOLForDirectConsol ?? false) ||
					!consol.IsDirect && (!consol.SendingForwarder?.MiscServ?.OM_FWRequiresElectronicBOLForNonDirectConsol ?? false))
				{
					wrapper.EBLProvider.Code = EBLProviderConstants.Codes.NotListed;
				}
				else if (consol.IsDirect && (consol.DirectShipment?.Consignor?.MiscServ?.OM_FWRequiresElectronicBOLForDirectConsol ?? false) ||
					!consol.IsDirect && (consol.SendingForwarder?.MiscServ?.OM_FWRequiresElectronicBOLForNonDirectConsol ?? false))
				{
					wrapper.EBLProvider.Code = GetDefaultEBLProvider(GetCarrier());
				}
			}
		}

		ZString GetDefaultEBLProvider(OrgHeader org) => org?.ShippingLine?.ShippingLineEBLProviders?.OfType<RefShippingLineEBLProvider>().FirstOrDefault(x => x.RSE_IsAvailable && x.RSE_IsDefault)?.RSE_Name ?? EBLProviderConstants.Codes.NotListed;

		bool AtLeastOneAvailableEBLProvider(OrgHeader org) => org?.ShippingLine?.ShippingLineEBLProviders?.OfType<RefShippingLineEBLProvider>().Any(x => x.RSE_IsAvailable) ?? false;

		protected override void AddContainersSealNumberValidation(CarrierMessageData wrapper)
		{
			foreach (var container in wrapper.Containers)
			{
				container.IsSealNumberMandatory = true;
				container.SealInfo.AddMessageError(() => container.Seal.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForExists);
				container.SealInfo.AddMessageError(() => !container.Seal.IsEmpty && container.Seal.Length > 15, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForLength);
				container.SecondSealInfo.AddMessageError(() => !container.SecondSeal.IsEmpty && container.SecondSeal.Length > 15, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForLength);
				container.ThirdSealInfo.AddMessageError(() => !container.ThirdSeal.IsEmpty && container.ThirdSeal.Length > 15, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForLength);
			}
		}

		void PopulateRegistrationNumbers(CarrierMessageData wrapper)
		{
			var hirReference = consol?.Numbers?.GetFirstReferenceNumberByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR);
			if (hirReference != null)
			{
				wrapper.HIRReference = new RegistrationNumber()
				{
					Value = hirReference.CE_EntryNum,
					Type = new CodeDescription(new ShipmentNonCustomsAdditionalReferenceCodesCodeList())
					{
						Code = hirReference.CE_EntryType
					},
					CountryOfIssue = new Country(context.Factory, context.Countries)
					{
						Code = hirReference.CE_RN_NKCountryCode
					}
				};
			}
		}

		void AddContainersValidation(CarrierMessageData wrapper)
		{
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.MustHaveContainerPackline && consol.Containers.Count <= 0, (NoResString)"Container details are required for Shipping Instruction and Amendment messages."); // non-translatable validation message
		}

		void AddUSOrCAValidations(CarrierMessageData wrapper)
		{
			wrapper.IncludeHblInfo.AddWarning(() => wrapper.IncludeHbl, (NoResString)"House Bills will be included with electronic transmission only. Not every carrier/NVOCC can process House Bill details even if they are submitted electronically. Please consult carrier directly for more information."); // non-translatable validation message

			var usCanadaOrUsTerritoryCountryCodes = new string[]
			{
				CountryCodes.UnitedStates,
				CountryCodes.Canada,
				CountryCodes.PuertoRico,
				CountryCodes.Guam,
				CountryCodes.NorthernMarianaIslands,
				CountryCodes.AmericanSamoa,
				CountryCodes.VirginIslands
			};

			wrapper.IncludeHblInfo.AddMessageError(() => wrapper.IncludeHbl && usCanadaOrUsTerritoryCountryCodes.Contains<string>(wrapper.PortOfDischarge?.Country?.Code) && !string.IsNullOrWhiteSpace(wrapper.USCanadaManifestSelfFilerID), (NoResString)"House Bills can only be included if the carrier is to file US/Canada Manifest."); // non-translatable validation message
			wrapper.AddValidationDependencies(wrapper.IncludeHblInfo, wrapper.USCanadaManifestSelfFilerIDInfo);
		}

		void AddGoodsValueValidationAndSuppression(CarrierMessageData wrapper)
		{
			wrapper.SuppressGoodsValue = !FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.Value;

			if (EnableSCMTR)
			{
				if (consol.JK_RL_NKDischargePort.SubstringSafe(0, 2) == CountryCodes.India)
				{
					wrapper.GoodsValue.AmountInfo.AddMessageErrorIfEmpty((NoResString)"Goods Value is required to comply with SCMTR Manifest reporting for India."); // non-translatable validation message
					wrapper.SuppressGoodsValue = false;
				}

				if (IsTransitThroughCountry(wrapper, CountryCodes.India))
				{
					var currency = (CodeDescription)wrapper.GoodsValue.Currency;
					currency.CodeInfo.AddMessageError(() => wrapper.GoodsValue.Amount != 0 && wrapper.GoodsValue.Currency.Code.IsEmpty, (NoResString)"BOL Currency is required to comply with SCMTR Manifest reporting for India."); // non-translatable validation message
					currency.AddValidationDependencies(((CodeDescription)wrapper.GoodsValue.Currency).CodeInfo, wrapper.GoodsValue.AmountInfo);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		void AddNumberOfOriginalsAndReleaseTypeValidation(CarrierMessageData wrapper)
		{
			wrapper.NumberOfOriginalsInfo.AddMessageError(() => wrapper.ReleaseType.Code == ShippingInstructionReleaseTypes.Codes.BOLOriginal && wrapper.NumberOfOriginals < 1, "This Release Type requires at least one Original Bill");
			wrapper.AddValidationDependencies(wrapper.NumberOfOriginalsInfo, wrapper.ReleaseType.CodeInfo);
		}

		#region Population

		protected override ZAddressWithContact ForwarderAddressWithContact
		{
			get
			{
				if (consol.IsDirect || FreightConfigurationRegistry.Instance.SIDefaultForwarderForNonDirectConsols.Value)
				{
					return base.ForwarderAddressWithContact;
				}

				return null;
			}
		}

		void PopulateGoodsValue(CarrierMessageData wrapper)
		{
			var goodsValueCurrency = GetGoodsValueCurrency();

			wrapper.GoodsValue = new Money(context)
			{
				Amount = consol
					.Shipments
					.OfType<ForwardingShipment>()
					.Where(shipment => shipment.JS_RX_NKGoodsValueCurr == goodsValueCurrency || shipment.JS_RX_NKGoodsValueCurr == ZString.Empty)
					.Sum(x => x.JS_GoodsValue),
			};

			wrapper.GoodsValue.Currency.Code = goodsValueCurrency;
		}

		void PopulateBillOfLadingNumber(CarrierMessageData wrapper)
		{
			if (wrapper.ContainerMode.Code == Constants.ContainerModes.LCL
				&& consol.JK_AgentType != Constants.AgentType.Direct
				&& consol.Shipments.Count == 1)
			{
				wrapper.BillOfLadingNumber = consol.Shipments.OfType<ForwardingShipment>().First().JS_HouseBill;
			}
		}

		void PopulateCountrySpecificFields(CarrierMessageData wrapper)
		{
			var dischargeCountry = wrapper.PortOfDischarge.Country.Code;

			var isDischargeInCanadaUSOrUSTerritory = new string[]
			{
				CountryCodes.UnitedStates,
				CountryCodes.Canada,
				CountryCodes.PuertoRico,
				CountryCodes.Guam,
				CountryCodes.NorthernMarianaIslands,
				CountryCodes.VirginIslands,
				CountryCodes.AmericanSamoa
			}
			.Contains(dischargeCountry.ToString());

			if (isDischargeInCanadaUSOrUSTerritory)
			{
				wrapper.USCanadaManifestSelfFilerID = GetUSCanadaManifestSelfFilerID(dischargeCountry, consol.SendingForwarder);
				wrapper.USCanadaManifestSelfFilerIDInfo.AddAsciiCharactersValidation();
			}

			wrapper.IsTransitThroughBrazil = wrapper.Origin.Country.Code == CountryCodes.Brazil
				|| wrapper.Destination.Country.Code == CountryCodes.Brazil
				|| wrapper.PortOfLoading.Country.Code == CountryCodes.Brazil
				|| wrapper.PortOfDischarge.Country.Code == CountryCodes.Brazil
				|| Transports.Any(x => x.JW_RL_NKLoadPort.Substring(0, 2) == CountryCodes.Brazil);

			wrapper.BRWoodenPackageProcessType = new CodeDescription(Lookups.BRWoodenPackageProcessTypes)
			{
				Code = DocDataConstants.WoodenPackageProcessTypes.NotApplicable
			};
			wrapper.BRWoodenPackageProcessType.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Wooden Package is required."); // non-translatable validation message

			var portOfLoadingCountryCode = wrapper.Transports.FirstOrDefault()?.PortOfLoading?.Country?.Code ?? ZString.Empty;
			var portOfDischargeCountryCode = wrapper.Transports.LastOrDefault()?.PortOfDischarge?.Country?.Code ?? ZString.Empty;
			wrapper.IsUSOrUSTerritoryExport = CountryCodes.IsUsaOrTerritory(portOfLoadingCountryCode)
				&& portOfLoadingCountryCode != portOfDischargeCountryCode;
			wrapper.IsBrazilExport = portOfLoadingCountryCode == CountryCodes.Brazil
				&& portOfLoadingCountryCode != portOfDischargeCountryCode;
			wrapper.IsCanadaExport = portOfLoadingCountryCode == CountryCodes.Canada
				&& portOfLoadingCountryCode != portOfDischargeCountryCode;

			wrapper.IsUSCanadaManifestSelfFilerIDSupported = IsUSCanadaManifestSelfFilerIDSupportedCountryCode(consol.DischargePort?.Country?.Code ?? ZString.Empty);

			var consolRUCNumber = string.Join(",", consol.Numbers.Cast<Customs.Common.CusEntryNumber>()
				.Where(x => x.CE_EntryType == Customs.Common.BrazilAdditionalReferenceNumberTypes.Codes.RUC && x.CE_RN_NKCountryCode == CountryCodes.Brazil)
				.Select(x => x.CE_EntryNum).Distinct());
			wrapper.RUCNumber = !string.IsNullOrEmpty(consolRUCNumber) ? consolRUCNumber
				: string.Join(",", consol.Shipments.OfType<ForwardingShipment>().SelectMany(s => s.Numbers.Cast<Customs.Common.CusEntryNumber>())
					.Where(x => x.CE_EntryType == Customs.Common.BrazilAdditionalReferenceNumberTypes.Codes.RUC && x.CE_RN_NKCountryCode == CountryCodes.Brazil)
					.Select(x => x.CE_EntryNum).Distinct());
		}

		ZBool IsUSCanadaManifestSelfFilerIDSupportedCountryCode(ZString countryCode)
		{
			return CountryCodes.IsUsaOrTerritory(countryCode) || countryCode == CountryCodes.Canada;
		}

		ZString GetUSCanadaManifestSelfFilerID(ZString dischargeCountry, OrgHeader sendingForwarder)
		{
			if (consol.IsDirect)
			{
				return ZString.Empty;
			}
			var isToUS = dischargeCountry == CountryCodes.UnitedStates;
			var usSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode);

			if (isToUS)
			{
				return usSCACCode;
			}
			else
			{
				switch (dischargeCountry)
				{
					case CountryCodes.Canada:
						var caSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.Canada, OrgCusCode.CodeTypes.CarrierCode);
						return caSCACCode.IsEmpty ? usSCACCode : caSCACCode;
					case CountryCodes.PuertoRico:
						var prSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.PuertoRico, OrgCusCode.CodeTypes.CarrierCode);
						return prSCACCode.IsEmpty ? usSCACCode : prSCACCode;
					case CountryCodes.Guam:
						var guSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.Guam, OrgCusCode.CodeTypes.CarrierCode);
						return guSCACCode.IsEmpty ? usSCACCode : guSCACCode;
					case CountryCodes.NorthernMarianaIslands:
						var mpSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.NorthernMarianaIslands, OrgCusCode.CodeTypes.CarrierCode);
						return mpSCACCode.IsEmpty ? usSCACCode : mpSCACCode;
					case CountryCodes.VirginIslands:
						var viSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.VirginIslands, OrgCusCode.CodeTypes.CarrierCode);
						return viSCACCode.IsEmpty ? usSCACCode : viSCACCode;
					case CountryCodes.AmericanSamoa:
						var asSCACCode = sendingForwarder.GetRegistrationNumber(CountryCodes.AmericanSamoa, OrgCusCode.CodeTypes.CarrierCode);
						return asSCACCode.IsEmpty ? usSCACCode : asSCACCode;
					default:
						return ZString.Empty;
				}
			}
		}

		void PopulateOtherBillClauses(CarrierMessageData wrapper)
		{
			if (FreightConfigurationRegistry.Instance.SendContactCompanyIdInSIBillClauses.Value)
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(wrapper.OtherBillClauses);

				builder.AppendIfNotEmpty(WrapAddressForOtherBillClauses((NoResString)"Shipper", wrapper.Shipper, wrapper.ShipperTaxInfo)); // it is used to concat content
				builder.AppendIfNotEmpty(WrapAddressForOtherBillClauses((NoResString)"Consignee", wrapper.Consignee, wrapper.ConsigneeTaxInfo)); // it is used to concat content
				builder.AppendIfNotEmpty(WrapAddressForOtherBillClauses((NoResString)"Notify Party", wrapper.NotifyParty, wrapper.NotifyPartyTaxInfo)); // it is used to concat content

				wrapper.OtherBillClauses = builder.ToStringWithNewLineBetweenAppends();
			}
		}

		static string WrapAddressForOtherBillClauses(string addressType, Address address, IEnumerable<TaxInfo> taxinfos)
		{
			if (address == null)
			{
				return string.Empty;
			}

			var segmentBuilder = new ZStringBuilder();

			if (!address.Contact.IsEmpty)
			{
				segmentBuilder.AppendFormat("CONTACT:{0}", address.Contact); // it is used to concat content
			}

			if (!address.Phone.IsEmpty)
			{
				segmentBuilder.AppendFormat("TEL:{0}", address.Phone); // it is used to concat content
			}

			if (!address.Fax.IsEmpty)
			{
				segmentBuilder.AppendFormat("FAX:{0}", address.Fax); // it is used to concat content
			}

			if (!address.Email.IsEmpty)
			{
				segmentBuilder.AppendFormat("EMAIL:{0}", address.Email); // it is used to concat content
			}

			if (taxinfos.Any())
			{
				var infoFormatted = taxinfos
										.Select(t =>
											t.IsEmpty() || t.Number.IsEmpty
											? string.Empty
											: (t.IsChinaSpecific
												? string.Format(CultureInfo.InvariantCulture, "{0}+{1}", t.LongLabel, t.Number)
												: string.Format(CultureInfo.InvariantCulture, "{0}+{1}", t.ShortLabel, t.Number)))
										.Where(t => !string.IsNullOrWhiteSpace(t));

				if (infoFormatted.Any())
				{
					segmentBuilder.AppendFormat("TAXID:{0}", string.Join("; ", infoFormatted)); // it is used to concat content
				}
			}

			return segmentBuilder.IsEmpty ? string.Empty : (addressType + "- " + segmentBuilder.ToStringWithDelimiterBetweenAppends(", "));
		}

		#region Shipment

		protected override void PopulateShipment(ForwardingShipment shipment, Shipment shipmentDO)
		{
			base.PopulateShipment(shipment, shipmentDO);

			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				var buyerDocAddress = shipment.CoLoadMasterShipment != null ? shipment.CoLoadMasterShipment.BuyerDocAddress : shipment.BuyerDocAddress;
				if (buyerDocAddress != null)
				{
					shipmentDO.Buyer = AddressBuilder.Create(context, buyerDocAddress);
				}

				var supplierDocAddress = shipment.CoLoadMasterShipment != null ? (shipment.CoLoadMasterShipment as ForwardingShipment)?.SupplierDocAddress : shipment.SupplierDocAddress;
				if (supplierDocAddress != null)
				{
					shipmentDO.Supplier = AddressBuilder.Create(context, supplierDocAddress);
				}
			}
		}

		#endregion

		#region ExportStatement Fields

		protected override void PopulateExportStatementFields(CarrierMessageData wrapper)
		{
			var shipmentBOs = consol.Shipments.Cast<ForwardingShipment>().ToArray();
			foreach (var shipmentDO in wrapper.Shipments.Where(s => !s.ExportStatement.IsEmpty))
			{
				var shipmentBO = shipmentBOs.FirstOrDefault(s => s.JS_UniqueConsignRef == shipmentDO.ShipmentID);
				if (shipmentBO != null)
				{
					var originCountryCode = shipmentBO.JS_RL_NKOrigin.Left(2);
					if (Constants.TransportModes.Sea.Equals(shipmentBO.JS_TransportMode)
						&& CountryCodes.IsUsaOrTerritory(originCountryCode)
						&& originCountryCode != shipmentBO.JS_RL_NKDestination.Left(2))
					{
						var statements = FreightDataRegistry.Instance.ExportStatementSettings.Value[originCountryCode]?.Statements;
						if (statements != null)
						{
							var statement = statements.Cast<ExportStatementSetting>().FirstOrDefault(s => s.Code.Equals(shipmentBO.DocsAndCartage.JP_ExportStatement));
							if (statement != null && IsExportStatementFieldApplicable(statement))
							{
								shipmentDO.ExportStatementField1Type = statement.Field1;
								shipmentDO.ExportStatementField1Code = GetExportStatementFieldCode(shipmentBO, statement.Field1);
								shipmentDO.ExportStatementField2Type = statement.Field2;
								shipmentDO.ExportStatementField2Code = GetExportStatementFieldCode(shipmentBO, statement.Field2);
							}
						}
					}
				}
			}
		}

		bool IsExportStatementFieldApplicable(ExportStatementSetting statement)
		{
			return (consol.IsDirect && statement.UseOnDirectMasterBillOfLading)
				|| (!consol.IsDirect && statement.UseOnConsolidationMasterBillOfLading);
		}

		ZString GetExportStatementFieldCode(ForwardingShipment shipmentBO, ZString fieldType)
		{
			var countryCode = GlbCompany.CurrentCompany?.Country?.Code ?? ZString.Empty;

			switch (fieldType)
			{
				case SEDStatementFieldType.Codes.AgentEIN:
					return consol?.SendingForwarder?.GetRegistrationNumber(countryCode, OrgCusCode.USACodeTypes.EmployerIdentificationNumber) ?? ZString.Empty;

				case SEDStatementFieldType.Codes.DateOfExport:
					{
						foreach (var transport in Transports)
						{
							var loadPortCountryCode = transport.JW_RL_NKLoadPort.SubstringSafe(0, 2).ToUpper();
							if (CountryCodes.IsUsaOrTerritory(loadPortCountryCode) && !loadPortCountryCode.Equals(transport.JW_RL_NKDiscPort.SubstringSafe(0, 2).ToUpper()))
							{
								return transport.JW_ETD.ToString("MM-dd-yyyy");
							}
						}
						return string.Empty;
					}

				case SEDStatementFieldType.Codes.FilerID:
					return consol?.SendingForwarder?.GetRegistrationNumber(countryCode, OrgCusCode.USACodeTypes.EntryFilerCode) ?? ZString.Empty;

				case SEDStatementFieldType.Codes.ITN:
					return shipmentBO.CustomsEntryNumberType == Customs.Common.US.CusEntryNumberTypeList.Codes.ITN
						? shipmentBO.CustomsEntryNumber
						: ZString.Empty;

				case SEDStatementFieldType.Codes.ShipperEIN:
					return shipmentBO.Consignor?.GetRegistrationNumber(countryCode, OrgCusCode.USACodeTypes.EmployerIdentificationNumber) ?? ZString.Empty;

				case SEDStatementFieldType.Codes.ShipperEINAndFilerID:
					{
						var shipperEIN = shipmentBO.Consignor?.GetRegistrationNumber(countryCode, OrgCusCode.USACodeTypes.EmployerIdentificationNumber) ?? ZString.Empty;
						var filerID = shipmentBO.Consignor?.GetRegistrationNumber(countryCode, OrgCusCode.USACodeTypes.EntryFilerCode) ?? ZString.Empty;
						return filerID.IsEmpty
							? shipperEIN
							: $"{shipperEIN}, {filerID}";
					}

				case SEDStatementFieldType.Codes.SRN:
					return shipmentBO.JS_BookingReference.IsEmpty ? shipmentBO.JS_UniqueConsignRef : shipmentBO.JS_BookingReference;

				case SEDStatementFieldType.Codes.SplitShipments:
				case SEDStatementFieldType.Codes.XTN:
				case "":
				default:
					return ZString.Empty;
			}
		}

		void AddExportStatementFieldsValidation(CarrierMessageData wrapper)
		{
			var statementFieldTypeList = new SEDStatementFieldType();

			foreach (var shipmentDO in wrapper.Shipments)
			{
				var fieldWarningMessage = (NoResString)"{0} Export Statement {1} requires the {2}. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements."; // non-translatable validation message

				shipmentDO.ExportStatementField1CodeInfo.AddWarning(() => !shipmentDO.ExportStatementField1Type.IsEmpty && shipmentDO.ExportStatementField1Code.IsEmpty,
					string.Format(fieldWarningMessage, shipmentDO.ShipmentID, shipmentDO.ExportStatementCode, GetExportStatementFieldTypeDescription(shipmentDO.ExportStatementField1Type)));
				shipmentDO.ExportStatementField2CodeInfo.AddWarning(() => !shipmentDO.ExportStatementField2Type.IsEmpty && shipmentDO.ExportStatementField2Code.IsEmpty,
					string.Format(fieldWarningMessage, shipmentDO.ShipmentID, shipmentDO.ExportStatementCode, GetExportStatementFieldTypeDescription(shipmentDO.ExportStatementField2Type)));

				const int fieldSectionMaxLength = 35;
				var fieldErrorMessage = (NoResString)"This field exceeds the maximum 35 character code limit."; // non-translatable validation message

				shipmentDO.ExportStatementField1CodeInfo.AddMessageError(() => shipmentDO.ExportStatementField1Code.Split(",").Any(s => s.Length > fieldSectionMaxLength), fieldErrorMessage);
				shipmentDO.ExportStatementField2CodeInfo.AddMessageError(() => shipmentDO.ExportStatementField2Code.Split(",").Any(s => s.Length > fieldSectionMaxLength), fieldErrorMessage);

				foreach (var packLineDO in shipmentDO.PackingLines)
				{
					packLineDO.GroupExportStatementField1CodeInfo.AddWarning(() => !packLineDO.GroupExportStatementField1Type.IsEmpty && packLineDO.GroupExportStatementField1Code.IsEmpty,
						string.Format(fieldWarningMessage, shipmentDO.ShipmentID, packLineDO.GroupPOFCode, GetGroupExportStatementFieldTypeDescription(packLineDO.GroupExportStatementField1Type)));
					packLineDO.GroupExportStatementField2CodeInfo.AddWarning(() => !packLineDO.GroupExportStatementField2Type.IsEmpty && packLineDO.GroupExportStatementField2Code.IsEmpty,
						string.Format(fieldWarningMessage, shipmentDO.ShipmentID, packLineDO.GroupPOFCode, GetGroupExportStatementFieldTypeDescription(packLineDO.GroupExportStatementField2Type)));

					packLineDO.GroupExportStatementField1CodeInfo.AddMessageError(() => packLineDO.GroupExportStatementField1Code.Split(",").Any(s => s.Length > fieldSectionMaxLength), fieldErrorMessage);
					packLineDO.GroupExportStatementField2CodeInfo.AddMessageError(() => packLineDO.GroupExportStatementField2Code.Split(",").Any(s => s.Length > fieldSectionMaxLength), fieldErrorMessage);
				}
			}

			ZString GetExportStatementFieldTypeDescription(ZString exportStatementFieldType)
			{
				return statementFieldTypeList.ContainsCode(exportStatementFieldType)
					? (ZString)statementFieldTypeList[exportStatementFieldType].Description
					: ZString.Empty;
			}

			ZString GetGroupExportStatementFieldTypeDescription(ZString groupExportStatementFieldType)
			{
				var fieldTypeDescriptions = groupExportStatementFieldType.Split(",")
					.Select(t => GetExportStatementFieldTypeDescription(t));

				return string.Join(",", fieldTypeDescriptions);
			}
		}

		#endregion

		void PopulatePackingLineHBLPaymentTypeEvents(CarrierMessageData wrapper)
		{
			foreach (var shipment in wrapper.Shipments.Where(s => !s.PackingLines.IsNullOrEmpty()))
			{
				foreach (var packingLine in shipment.PackingLines)
				{
					packingLine.HBLPaymentTypeInfo.ValueChanged += (object sender, EventArgs e) =>
					{
						var args = e as ValueChangedEventArgs;
						if (args == null)
						{
							return;
						}
						var newValue = args.NewValue.ToString();
						foreach (var item in shipment.PackingLines)
						{
							if (item.HBLPaymentType != newValue)
							{
								item.HBLPaymentType = newValue;
							}
						}
					};
				}
			}
		}

		#endregion

		#region Addresses

		#region Consignee

		protected override object Consignee => CalculateConsignee();

		object CalculateConsignee()
		{
			if (!consol.IsDirect && consol.MasterBillConsigneeOverrideDocumentaryAddress.IsValidAddress)
			{
				return consol.MasterBillConsigneeOverrideDocumentaryAddress;
			}

			var ultimateConsigneeRule = GetUltimateConsigneeRule();
			if (ultimateConsigneeRule != null
				&& (ultimateConsigneeRule.R7_UltimateConsigneeRule == Constants.UltimateConsigneeRuleTypes.Codes.Mandatory || ultimateConsigneeRule.R7_UltimateConsigneeRule == Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities))
			{
				var shipments = consol.Shipments.Cast<ForwardingShipment>();
				if (shipments.Any(x => !x.ConsigneeDocumentaryAddress.IsEmpty)
					&& shipments.Select(x => x.ConsigneeDocumentaryAddress.AddressAsASingleLine).AllSame())
				{
					return consol.Shipments.Cast<ForwardingShipment>().First().ConsigneeDocumentaryAddress;
				}
			}

			return base.Consignee;
		}

		#endregion

		RefCountryRules GetUltimateConsigneeRule()
		{
			return RefCountryRulesHelper.FilterRules(consol.LoadPort?.Country, consol.DischargePort?.Country, consol.JK_TransportMode,
					consol.DischargePort?.Country?.Rules.Where(x => !x.R7_UltimateConsigneeRule.IsEmpty))
				.FirstOrDefault();
		}

		#endregion

		#region Goods Value

		ZString GetGoodsValueCurrency()
		{
			var currencyFromFreightCharges = GetGoodsValueCurrencyFromFreightCharges();
			return !currencyFromFreightCharges.IsEmpty ? currencyFromFreightCharges : GetGoodsValueCurrencyFromCurrentCompany();
		}

		ZString GetGoodsValueCurrencyFromFreightCharges()
		{
			var chargesCreditorPK = consol.Creditor?.PK ?? consol.ShippingLine?.PK ?? ZGuid.Empty;

			if (!chargesCreditorPK.IsEmpty)
			{
				var chargeCodeSubQuery = new ZDBOnlySubQuery(typeof(IAccChargeCode), JobConsolCostSchema.E6_AC_ChargeCode);
				chargeCodeSubQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);

				var consolCostQuery = new ZDBOnlyQuery(typeof(IJobConsolCost));
				consolCostQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
				consolCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, consol.PK);
				consolCostQuery.AddToFilter(JobConsolCostSchema.E6_OH_Creditor, chargesCreditorPK);
				consolCostQuery.AddSubQuery(chargeCodeSubQuery, JoinCondition.And);

				var consolCostsCurrencies = consol.Factory.Load<IJobConsolCost>(consolCostQuery)
					.Cast<BusinessObject>()
					.Select(x => x[JobConsolCostSchema.E6_RX_NKCurrency.Name])
					.Distinct();

				if (consolCostsCurrencies.Count() == 1)
				{
					return consolCostsCurrencies.First().ToString();
				}
			}

			return ZString.Empty;
		}

		ZString GetGoodsValueCurrencyFromCurrentCompany()
		{
			return GlbCompany.CurrentCompany?.GC_RX_NKLocalCurrency ?? ZString.Empty;
		}

		#endregion

		#region Release Type

		protected override ZString GetReleaseType(CarrierMessageData wrapper)
		{
			if (!IsSeaWaybill || consol.JK_ReleaseType == Constants.ShipmentReleaseTypes.NonNegotiable && wrapper.Consignee.IsToOrder())
			{
				return ShippingInstructionReleaseTypes.Codes.BOLOriginal;
			}

			return ShippingInstructionReleaseTypes.Codes.SeaWaybill;
		}

		#endregion

		#region Number Of Originals and Copies

		protected override void PopulateNumberOfOriginalsAndCopies(CarrierMessageData wrapper)
		{
			wrapper.NumberOfCopies = DirectShipment?.JS_NoCopyBills ?? consol.JK_NoCopyBills;
			wrapper.NumberOfOriginals = wrapper.ReleaseType.Code == ShippingInstructionReleaseTypes.Codes.SeaWaybill
				? wrapper.NumberOfCopies
				: (DirectShipment?.JS_NoOriginalBills ?? consol.JK_NoOriginalBills);
		}

		#endregion

		#region Container Mode Validation

		void AddContainerModeValidation(CarrierMessageData wrapper)
		{
			wrapper.ContainerMode.CodeInfo.AddAsciiCharactersValidation();

			var unsupportedContainerTypes = new List<string> { Constants.ContainerModes.BreakBulk, Constants.ContainerModes.Liquid, Constants.ContainerModes.Bulk, Constants.ContainerModes.RollOnRollOff };
			wrapper.ContainerMode.CodeInfo.AddWarning(() => unsupportedContainerTypes.Contains(wrapper.ContainerMode.Code), (NoResString)"Carrier may not support this type of consol."); // non-translatable validation message
		}

		#endregion

		#region Container Validation

		protected override void AddContainerValidation(CarrierMessageData wrapper, Container containerDO)
		{
			base.AddContainerValidation(wrapper, containerDO);

			containerDO.VerifiedMethod.DescriptionInfo.AddWarning(() => !wrapper.IsNonContainerized, (NoResString)"VGM details on this form are for information, print, fax or email only. VGM information to participating carriers should be transmitted separately as per Verified Gross Container Weight form or according to local authority requirements.");  // non-translatable validation message

			containerDO.NumberInfo.AddMessageError(() => containerDO.Number.IsEmpty && ((!wrapper.IsCoload && !wrapper.IsGatewayCoload && !wrapper.IsNVO) || wrapper.ContainerMode.Code != Core.Constants.ContainerModes.LCL), (NoResString)"Please enter a Container Number."); // non-translatable registration number

			var isContainerized = wrapper.ContainerMode.Code == Core.Constants.ContainerModes.LCL || wrapper.ContainerMode.Code == Core.Constants.ContainerModes.FCL;
			containerDO.NumberInfo.AddMessageError(() => isContainerized && !containerDO.PackingLines.Any(), (NoResString)"There are no packs in this container.");

			AddSealPartyTypeValidation(containerDO);
		}

		void AddSealPartyTypeValidation(Container containerDO)
		{
			((CodeDescription)containerDO.SealPartyType).CodeInfo.AddMessageError(() => !containerDO.Seal.IsEmpty && containerDO.SealPartyType.Code.IsEmpty, (NoResString)"Sealed By is mandatory when Seal Number exists."); // seal
			((CodeDescription)containerDO.SecondSealPartyType).CodeInfo.AddMessageError(() => !containerDO.SecondSeal.IsEmpty && containerDO.SecondSealPartyType.Code.IsEmpty, (NoResString)"Sealed By is mandatory when Seal Number exists."); // seal
			((CodeDescription)containerDO.ThirdSealPartyType).CodeInfo.AddMessageError(() => !containerDO.ThirdSeal.IsEmpty && containerDO.ThirdSealPartyType.Code.IsEmpty, (NoResString)"Sealed By is mandatory when Seal Number exists."); // seal
		}

		#endregion

		#region PackLine Validation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable registration number, non-translatable validation message, non-translatable validation message.")]
		protected override void AddPackingLineValidation(CarrierMessageData wrapper, PackingLine packingLine)
		{
			base.AddPackingLineValidation(wrapper, packingLine);

			var harmonizedCode = packingLine.HarmonizedCode as HarmonizedCode;
			var importHarmonizedCode = packingLine.ImportHarmonizedCode as HarmonizedCode;
			var exportHarmonizedCode = packingLine.ExportHarmonizedCode as HarmonizedCode;
			var harmonizedCodesHaveMandatoryMessageErrors = false;

			importHarmonizedCode?.CodeInfo.AddMessageError(() =>
			{
				if (importHarmonizedCode.Code.IsEmpty && ((HarmonizedCode)packingLine.HarmonizedCode).Code.IsEmpty && RequireImportHarmonizedCodeForCountry(wrapper.PortOfDischarge?.Country?.Code))
				{
					return harmonizedCodesHaveMandatoryMessageErrors = true;
				}
				else
				{
					return false;
				}
			}, ZString.Format((NoResString)"Harmonized System Code is required for imports to {0}.", wrapper.PortOfDischarge?.Country?.Code));

			exportHarmonizedCode?.CodeInfo.AddMessageError(() =>
			{
				if (exportHarmonizedCode.Code.IsEmpty && ((HarmonizedCode)packingLine.HarmonizedCode).Code.IsEmpty && RequireExportHarmonizedCodeForCountry(wrapper.PortOfLoading?.Country?.Code))
				{
					return harmonizedCodesHaveMandatoryMessageErrors = true;
				}
				else
				{
					return false;
				}
			}, ZString.Format((NoResString)"Harmonized System Code is required for exports to {0}.", wrapper.PortOfLoading?.Country?.Code));

			if (importHarmonizedCode != null)
			{
				AddHSCodeValidationForEuropeanUnion(wrapper, packingLine, importHarmonizedCode, ref harmonizedCodesHaveMandatoryMessageErrors);

				AddHSCodeValidationForTurkey(wrapper, packingLine, importHarmonizedCode, ref harmonizedCodesHaveMandatoryMessageErrors);

				AddHSCodeValidationForPhilippines(wrapper, packingLine, importHarmonizedCode, ref harmonizedCodesHaveMandatoryMessageErrors);
			}
			harmonizedCode?.CodeInfo.AddMessageError(() =>
			{
				if ((importHarmonizedCode?.Country?.Code ?? ZString.Empty) == CountryCodes.Malaysia && harmonizedCode.Code.IsEmpty && importHarmonizedCode.Code.IsEmpty
				|| (exportHarmonizedCode?.Country?.Code ?? ZString.Empty) == CountryCodes.Malaysia && harmonizedCode.Code.IsEmpty && exportHarmonizedCode.Code.IsEmpty)
				{
					return harmonizedCodesHaveMandatoryMessageErrors = true;
				}
				else
				{
					return false;
				}
			}, (NoResString)"HS Code is required for Sea exports from or imports to Malaysia.");

			harmonizedCode?.CodeInfo.AddMessageError(() =>
			{
				if ((importHarmonizedCode?.Country?.Code ?? ZString.Empty) == CountryCodes.Israel && harmonizedCode.Code.IsEmpty && importHarmonizedCode.Code.IsEmpty)
				{
					return harmonizedCodesHaveMandatoryMessageErrors = true;
				}
				else
				{
					return false;
				}
			}, "Harmonized System Code is required for imports to Israel to comply with Manifest reporting.");

			if (EnableSCMTR && IsTransitThroughCountry(wrapper, CountryCodes.India))
			{
				harmonizedCode?.CodeInfo.AddMessageError(() => !Regex.IsMatch(harmonizedCode.Code, @"^\d{6}$"), (NoResString)"A six digit HS Code is required to comply with SCMTR Manifest reporting for India.");
			}
			else
			{
				harmonizedCode?.CodeInfo.AddWarning(() => !harmonizedCodesHaveMandatoryMessageErrors && string.IsNullOrWhiteSpace(harmonizedCode.Code) && string.IsNullOrWhiteSpace(importHarmonizedCode?.Code) && string.IsNullOrWhiteSpace(exportHarmonizedCode?.Code),
(NoResString)"It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes.");
			}

			if (wrapper.IsShowICS2)
			{
				var hsCodeMustBe6DigitForICS2WarningMessage = Res.GetString("a1fab02f-bca3-40ab-87fd-dcf6e20ec095", "HS code(s) reported for ICS2 must be 6-digit only.");

				harmonizedCode?.CodeInfo.AddWarning(() => harmonizedCode.Code.Split(", ").Any(x => !x.IsEmpty && x.Length != 6), hsCodeMustBe6DigitForICS2WarningMessage);
				importHarmonizedCode?.CodeInfo.AddWarning(() => importHarmonizedCode.Code.Split(", ").Any(x => !x.IsEmpty && x.Length != 6), hsCodeMustBe6DigitForICS2WarningMessage);
				exportHarmonizedCode?.CodeInfo.AddWarning(() => exportHarmonizedCode.Code.Split(", ").Any(x => !x.IsEmpty && x.Length != 6), hsCodeMustBe6DigitForICS2WarningMessage);

				if (consol.JK_AgentType != Constants.AgentType.Direct)
				{
					packingLine.MarksAndNumbersInfo.AddMessageError(() => wrapper.ICS2DeclarantEORINumber.IsEmpty && packingLine.MarksAndNumbers.IsEmpty, (NoResString)"Marks & Numbers are mandatory when carrier is filing the House Bill. Please enter the ICS2 Declarant EORI field if Carrier Filing is not intended.");
					packingLine.MarksAndNumbersInfo.AddWarning(() => wrapper.ICS2DeclarantEORINumber.IsEmpty && packingLine.MarksAndNumbers.Length > 512, (NoResString)"Marks & Numbers over 512 characters will be truncated during transmission to ICS2 system.");
					packingLine.GoodsDescriptionInfo.AddWarning(() => wrapper.ICS2DeclarantEORINumber.IsEmpty && packingLine.GoodsDescription.Length > 512, (NoResString)"Goods Description over 512 characters will be truncated during transmission to ICS2 system.");
				}
			}

			AddCUSCodesValidation(packingLine);
		}

		void AddHSCodeValidationForPhilippines(CarrierMessageData wrapper, PackingLine packingLine, HarmonizedCode importHarmonizedCode, ref bool harmonizedCodesHaveMandatoryMessageErrors)
		{
			if (wrapper.PortOfDischarge.Country.Code == CountryCodes.Philippines)
			{
				var harmonizedCode = packingLine.HarmonizedCode?.Code ?? ZString.Empty;
				if (importHarmonizedCode.Code.IsEmpty && harmonizedCode.IsEmpty)
				{
					importHarmonizedCode.CodeInfo.AddMessageError(() => true, (NoResString)"Harmonized System Code is required for imports to Philippines in line with Customs Order No. 48-2019."); // non-translatable validation message
					harmonizedCodesHaveMandatoryMessageErrors = true;
				}
				else
				{
					if (packingLine.HarmonizedCodes.Any(c => (c.Code.Length < 6 || !c.Code.IsNumbersOnlyOrEmpty) && c.Country.Code == CountryCodes.Philippines) ||
						(!harmonizedCode.IsEmpty && (harmonizedCode.Length < 6 || !harmonizedCode.IsNumbersOnlyOrEmpty)))
					{
						importHarmonizedCode.CodeInfo.AddMessageError(() => true, (NoResString)"For imports to Philippines, a six digit HS Code is required, in line with Customs Order No. 48-2019."); // non-translatable validation message
						harmonizedCodesHaveMandatoryMessageErrors = true;
					}
				}
			}
		}

		void AddHSCodeValidationForTurkey(CarrierMessageData wrapper, PackingLine packingLine, HarmonizedCode importHarmonizedCode, ref bool harmonizedCodesHaveMandatoryMessageErrors)
		{
			if (RequireHSForTransshipmentsOrImports(wrapper, code => code == CountryCodes.Turkey))
			{
				var harmonizedCode = packingLine.HarmonizedCode?.Code ?? ZString.Empty;
				if (importHarmonizedCode.Code.IsEmpty && harmonizedCode.IsEmpty)
				{
					importHarmonizedCode.CodeInfo.AddMessageError(() => true, (NoResString)"Harmonized System Code is required for imports to Turkey for the carrier to complete mandatory ENS (Entry Summary Declaration) filing."); // non-translatable validation message
					harmonizedCodesHaveMandatoryMessageErrors = true;
				}
				else
				{
					if (packingLine.HarmonizedCodes.Any(c => (c.Code.Length < 6 || !c.Code.IsNumbersOnlyOrEmpty) && c.Country.Code == CountryCodes.Turkey) ||
						(!harmonizedCode.IsEmpty && (harmonizedCode.Length < 6 || !harmonizedCode.IsNumbersOnlyOrEmpty)))
					{
						importHarmonizedCode.CodeInfo.AddWarning(() => true, (NoResString)"At least four but recommended six digit Harmonized System Code is required for imports to Turkey for the carrier to complete mandatory ENS (Entry Summary Declaration) filing."); // non-translatable validation message
					}
				}
			}
		}

		void AddHSCodeValidationForEuropeanUnion(CarrierMessageData wrapper, PackingLine packingLine, HarmonizedCode importHarmonizedCode, ref bool harmonizedCodesHaveMandatoryMessageErrors)
		{
			if (RequireHSForTransshipmentsOrImports(wrapper, code => EuropeanUnionCustomsMembersProvider.IsMemberOfEU(code)))
			{
				var harmonizedCode = packingLine.HarmonizedCode?.Code ?? ZString.Empty;
				if (importHarmonizedCode.Code.IsEmpty && harmonizedCode.IsEmpty)
				{
					importHarmonizedCode.CodeInfo.AddMessageError(() => true, (NoResString)"Harmonized System Code is required for imports to the EU for the carrier to complete mandatory ENS (Entry Summary Declaration) filing."); // non-translatable validation message
					harmonizedCodesHaveMandatoryMessageErrors = true;
				}
			}
		}

		public bool RequireHSForTransshipmentsOrImports(CarrierMessageData wrapper, Func<string, bool> predict)
		{
			return !predict(wrapper.PlaceOfReceipt.Country.Code) && (predict(wrapper.PlaceOfDelivery.Country.Code) || Transports.Any(x => predict(x.JW_RL_NKDiscPort.Substring(0, 2))));
		}

		bool RequireImportHarmonizedCodeForCountry(string country)
		{
			return CountryCodes.IsUsaOrTerritory(country) || country == CountryCodes.Brazil || country == CountryCodes.VietNam || country == CountryCodes.Indonesia;
		}

		bool RequireExportHarmonizedCodeForCountry(string country)
		{
			return CountryCodes.IsUsaOrTerritory(country) || country == CountryCodes.Brazil;
		}

		bool IsTransitToCountry(CarrierMessageData wrapper, Func<string, bool> predict)
		{
			return predict(wrapper.PortOfDischarge.Country.Code) || Transports.Any(x => predict(x.JW_RL_NKDiscPort.Substring(0, 2)));
		}

		bool IsTransitThroughCountry(CarrierMessageData wrapper, string country)
		{
			return wrapper.Origin.Country.Code == country
				|| wrapper.Destination.Country.Code == country
				|| Transports.Any(x => x.JW_RL_NKLoadPort.Substring(0, 2) == country);
		}

		void AddCUSCodesValidation(PackingLine packingLine)
		{
			var invalidCusCodeErrorMessage = Res.GetString("60a4adaf-6642-42f8-8715-e5d8cbe1f991", "Invalid code has been detected. Please use the searching tile to lookup the correct CUS code.");

			new[] { packingLine.CUSCode1Info, packingLine.CUSCode2Info, packingLine.CUSCode3Info,
				packingLine.CUSCode4Info, packingLine.CUSCode5Info, packingLine.CUSCode6Info,
				packingLine.CUSCode7Info, packingLine.CUSCode8Info, packingLine.CUSCode9Info }
			.ForEach(cusCodeInfo => cusCodeInfo.AddMessageError(
				() =>
				{
					if (cusCodeInfo.Value.IsEmpty)
					{
						return false;
					}

					var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, RefCusCodeListType.Code_ECICS);
					query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, cusCodeInfo.Value);

					return !consol.Factory.Exists(typeof(Customs.Universal.ZZRefCusCodeListCombined), query);
				}, invalidCusCodeErrorMessage));
		}

		#endregion

		#region Transport Validation

		void AddTransportsValidation(CarrierMessageData wrapper)
		{
			var mainTransport = wrapper.Transports?.Main;
			mainTransport?.Vessel?.NameInfo.AddAsciiCharactersValidation(); // non-translatable validation message
			mainTransport?.Vessel?.NameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(mainTransport.Vessel.Name), (NoResString)"Vessel name is required."); // non-translatable validation message
			mainTransport?.VoyageFlightNumberInfo.AddAsciiCharactersValidation(); // non-translatable validation message
			mainTransport?.VoyageFlightNumberInfo.AddMessageError(() => string.IsNullOrWhiteSpace(mainTransport?.VoyageFlightNumber), (NoResString)"Voyage is required."); // non-translatable validation message
		}

		#endregion

		#region Booking Reference Validation

		void AddBookingReferenceValidation(CarrierMessageData wrapper)
		{
			wrapper.CoLoadBookingReferenceInfo.AddMessageError(() => wrapper.IsCoload && string.IsNullOrWhiteSpace(wrapper.CoLoadBookingReference), (NoResString)"At least one Co-Loader Booking Reference number is required."); // non-translatable validation message
			wrapper.BookingReferenceInfo.AddMessageError(() => wrapper.IsGatewayCoload && string.IsNullOrWhiteSpace(wrapper.BookingReference), (NoResString)"At least one Gateway Co-Loader Booking Reference number is required."); // non-translatable validation message

			var bkgReferenceNumbers = wrapper.Numbers.Where(x => x.Type != null && x.Type.Code == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG).Where(x => !x.Value.IsEmpty);
			wrapper.BookingReferenceInfo.AddMessageError(() => !wrapper.IsCoload && !wrapper.IsGatewayCoload && string.IsNullOrEmpty(wrapper.BookingReference) && !bkgReferenceNumbers.Any(), (NoResString)"At least one Carrier Booking Reference number is required."); // non-translatable validation message
			wrapper.BookingReferenceInfo.AddMessageError(() => !wrapper.IsCoload && !wrapper.IsGatewayCoload && (ValidationExtensions.HasNonAsciiCharacters(wrapper.BookingReference) || bkgReferenceNumbers.Any(x => ValidationExtensions.HasNonAsciiCharacters(x.Value))), (NoResString)"Most messaging providers do not support non ASCII characters."); // non-translatable validation message
		}

		#endregion

		#region Place and Date of Issue Validation

		public void AddPlaceAndDateOfIssueValidation(CarrierMessageData wrapper)
		{
			wrapper.DateOfIssueInfo.AddMessageError(
				() => !string.IsNullOrWhiteSpace(wrapper.PlaceOfIssue.Code) && (wrapper.DateOfIssue.IsEmpty || !wrapper.DateOfIssue.IsValid),
(NoResString)"Date of Issue is required if Place of Issue is entered."  // non-translatable validation message
			);

			wrapper.PlaceOfIssue.CodeInfo.AddMessageError(() => string.IsNullOrWhiteSpace(wrapper.PlaceOfIssue.Code) && !wrapper.DateOfIssue.IsEmpty, (NoResString)"Place of Issue is required if Date of Issue is entered."); // non-translatable validation message

			wrapper.AddValidationDependencies(wrapper.DateOfIssueInfo, wrapper.PlaceOfIssue.CodeInfo);
			wrapper.AddValidationDependencies(wrapper.PlaceOfIssue.CodeInfo, wrapper.DateOfIssueInfo);
		}

		#endregion

		#region Address Validation

		void AddAddressValidation(CarrierMessageData wrapper)
		{
			AddConsigneeAddressValidation(wrapper);
			AddNotifyPartyAddressValidation(wrapper);
			AddShipperAddressValidation(wrapper);
		}

		void AddConsigneeAddressValidation(CarrierMessageData wrapper)
		{
			var consignee = wrapper.Consignee;
			var notifyParty = wrapper.NotifyParty;

			consignee.Country.NameInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Brazil) && !consignee.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.Brazil)) && !notifyParty.Country.Name.EqualsIgnoringCase(nameof(Core.Constants.CountryCodes.Brazil)), (NoResString)"Either Consignee's or Notify Party's country code must be Brazil for Brazil imports."); // non-translatable validation message
			consignee.Country.NameInfo.AddWarning(() => !consignee.CompanyName.IsEmpty && !consignee.TaxNumber.IsEmpty && wrapper.Destination.Code.StartsWith(CountryCodes.China, StringComparison.OrdinalIgnoreCase) && !consignee.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.China)), (NoResString)"Consignee's country is different from Destination country."); // non-translatable validation message
			consignee.Country.AddValidationDependencies(consignee.Country.NameInfo, notifyParty.Country.NameInfo, wrapper.Destination.CodeInfo, consignee.CompanyNameInfo);

			consignee.ContactInfo.AddMessageError(() => consignee.Phone.IsEmpty && notifyParty.Phone.IsEmpty && consignee.Email.IsEmpty && notifyParty.Email.IsEmpty && (wrapper.PortOfLoading.IsInCountry(CountryCodes.China) || wrapper.PortOfDischarge.IsInCountry(CountryCodes.China)), errorForMissingConsigneeAndNotifyPartyContactDetails);
			consignee.AddValidationDependencies(consignee.ContactInfo, consignee.PhoneInfo, notifyParty.PhoneInfo, consignee.EmailInfo, notifyParty.EmailInfo);

			consignee.PostcodeInfo.AddMessageError(() => !consignee.IsToOrder() && consignee.Postcode.IsEmpty && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedStates), (NoResString)"Consignee's postcode is required for US imports."); // non-translatable validation message
			consignee.PostcodeInfo.AddMessageError(() => !consignee.IsToOrder() && consignee.Postcode.IsEmpty && wrapper.PortOfDischarge.IsInCountry(CountryCodes.Canada), (NoResString)"Consignee's postcode is required for Canadian imports."); // non-translatable validation message
			consignee.AddValidationDependencies(consignee.PostcodeInfo, consignee.CompanyNameInfo);

			if (EnableSCMTR)
			{
				consignee.Country.NameInfo.AddMessageError(() => !consignee.IsToOrder() && IsTransitToCountry(wrapper, code => code == CountryCodes.India) && consignee.Country.Name.IsEmpty, messageErrorForRequireFullAddressForIndia);
				consignee.PostcodeInfo.AddMessageError(() => !consignee.IsToOrder() && IsTransitToCountry(wrapper, code => code == CountryCodes.India) && consignee.Postcode.IsEmpty, messageErrorForRequireFullAddressForIndia);
				consignee.CityInfo.AddMessageError(() => !consignee.IsToOrder() && IsTransitToCountry(wrapper, code => code == CountryCodes.India) && consignee.City.IsEmpty, messageErrorForRequireFullAddressForIndia);
				consignee.AddValidationDependencies(consignee.CityInfo, consignee.CompanyNameInfo);
			}

			AddUltimateConsigneeValidation(wrapper);

			consignee.CompanyNameInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				var args = e as ValueChangedEventArgs;

				if (args == null)
				{
					return;
				}

				var oldValue = args.OldValue.ToString();
				var newValue = args.NewValue.ToString();

				wrapper.ReleaseType.Code = GetReleaseType(wrapper);
				PopulateNumberOfOriginalsAndCopies(wrapper);

				if (AddressExtensions.IsToOrder(oldValue) && !AddressExtensions.IsToOrder(newValue))
				{
					CopyTaxInfoOriginalCollectionToTaxInfoCollection(wrapper.ConsigneeTaxInfo, wrapper.ConsigneeTaxInfoOriginal);
				}
				else if (!AddressExtensions.IsToOrder(oldValue) && AddressExtensions.IsToOrder(newValue))
				{
					CleanUpTaxNumbersWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(wrapper);
				}
			};
		}

		void AddUltimateConsigneeValidation(CarrierMessageData wrapper)
		{
			var ultimateConsigneeRule = GetUltimateConsigneeRule();
			if (ultimateConsigneeRule != null)
			{
				var consignee = wrapper.Consignee;
				if (ultimateConsigneeRule.R7_UltimateConsigneeRule == Constants.UltimateConsigneeRuleTypes.Codes.Mandatory)
				{
					var mandatoryMessage = FormattableString.Invariant($"Ultimate consignee is required for Master Bill for destination of {wrapper.Destination.Country.Name}."); // non-translatable validation message
					consignee.CompanyNameInfo.AddMessageError(() => consignee.IsToOrder() || ConsigneeAddressIsFromReceivingForwarder(consignee),
						mandatoryMessage);

					consignee.CompanyNameInfo.AddWarning(() => ConsigneeAddressHasChanges(consignee), mandatoryMessage);
				}
				else if (ultimateConsigneeRule.R7_UltimateConsigneeRule == Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities)
				{
					var mayBeRequiredMessage = FormattableString.Invariant($"Ultimate consignee might be required for Master Bill for destination of {wrapper.Destination.Country.Name} - please check with the Carrier or local authorities at destination."); // non-translatable validation message
					consignee.CompanyNameInfo.AddWarning(() => consignee.IsToOrder()
							|| ConsigneeAddressHasChanges(consignee)
							|| ConsigneeAddressIsFromReceivingForwarder(consignee),
						mayBeRequiredMessage);
				}

				consignee.AddValidationDependencies(consignee.CompanyNameInfo, consignee.AddressLine1Info, consignee.AddressLine2Info, consignee.CityInfo, consignee.PostcodeInfo);
			}
		}

		bool ConsigneeAddressHasChanges(Address consignee)
		{
			var calculatedConsigneeAddress = AddressBuilder.Create(context, CalculateConsignee(), consol.IsDirect);
			return calculatedConsigneeAddress.CompanyName != consignee.CompanyName
				|| calculatedConsigneeAddress.AddressLine1 != consignee.AddressLine1
				|| calculatedConsigneeAddress.AddressLine2 != consignee.AddressLine2
				|| calculatedConsigneeAddress.City != consignee.City
				|| calculatedConsigneeAddress.Postcode != consignee.Postcode;
		}

		bool ConsigneeAddressIsFromReceivingForwarder(Address consignee)
		{
			var receivingForwarderAddress = AddressBuilder.Create(context, consol.ReceivingForwarderAddress, consol.IsDirect);
			return receivingForwarderAddress.CompanyName == consignee.CompanyName
				&& receivingForwarderAddress.AddressLine1 == consignee.AddressLine1
				&& receivingForwarderAddress.AddressLine2 == consignee.AddressLine2
				&& receivingForwarderAddress.City == consignee.City
				&& receivingForwarderAddress.Postcode == consignee.Postcode;
		}

		void AddNotifyPartyAddressValidation(CarrierMessageData wrapper)
		{
			var consignee = wrapper.Consignee;
			var notifyParty = wrapper.NotifyParty;

			notifyParty.CompanyNameInfo.AddMessageError(() => notifyParty.IsSameAsConsignee() && wrapper.PortOfDischarge.IsInCountry(CountryCodes.Brazil) && (consignee.IsToOrder() || !consignee.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.Brazil))), (NoResString)"Notify party is required for Brazil Imports when consignee is outside Brazil or 'To Order', 'Same As Consignee' text cannot be used."); // non-translatable validation message
			notifyParty.CompanyNameInfo.AddMessageError(() => notifyParty.IsSameAsConsignee() && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedStates), (NoResString)"'Same As Consignee' text cannot be used for US imports."); // non-translatable validation message

			notifyParty.Country.NameInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Brazil) && !notifyParty.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.Brazil)) && !consignee.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.Brazil)), (NoResString)"Either Consignee's or Notify Party's country code must be Brazil for Brazil imports."); // non-translatable validation message
			notifyParty.Country.NameInfo.AddWarning(() => !notifyParty.CompanyName.IsEmpty && !notifyParty.TaxNumber.IsEmpty && wrapper.Destination.Code.StartsWith(CountryCodes.China, StringComparison.OrdinalIgnoreCase) && !notifyParty.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.China)), (NoResString)"Notify Party's country is different from Destination country."); // non-translatable validation message

			notifyParty.ContactInfo.AddMessageError(() => consignee.Phone.IsEmpty && notifyParty.Phone.IsEmpty && consignee.Email.IsEmpty && notifyParty.Email.IsEmpty && (wrapper.PortOfLoading.IsInCountry(CountryCodes.China) || wrapper.PortOfDischarge.IsInCountry(CountryCodes.China)), errorForMissingConsigneeAndNotifyPartyContactDetails);
			notifyParty.AddValidationDependencies(notifyParty.ContactInfo, consignee.PhoneInfo, notifyParty.PhoneInfo, consignee.EmailInfo, notifyParty.EmailInfo);

			notifyParty.PostcodeInfo.AddMessageError(() => notifyParty.Postcode.IsEmpty && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedStates), (NoResString)"Notify Party's postcode is required for US imports."); // non-translatable validation message
			notifyParty.AddValidationDependencies(consignee.PostcodeInfo, consignee.CompanyNameInfo);

			if (EnableSCMTR)
			{
				notifyParty.CompanyNameInfo.AddMessageError(() => !notifyParty.IsSameAsConsignee() && IsTransitToCountry(wrapper, code => code == CountryCodes.India) && (consignee.IsToOrder() || (consignee.Country.Code != string.Empty && consignee.Country.Code != CountryCodes.India)) && notifyParty.CompanyName.IsEmpty, messageErrorForRequireFullAddressForIndia);
				notifyParty.AddValidationDependencies(notifyParty.CompanyNameInfo, notifyParty.Country.CodeInfo, consignee.CompanyNameInfo, consignee.Country.NameInfo);
				notifyParty.Country.NameInfo.AddMessageError(() => !notifyParty.IsSameAsConsignee() && IsTransitToCountry(wrapper, code => code == CountryCodes.India) && (consignee.IsToOrder() || (consignee.Country.Code != string.Empty && consignee.Country.Code != CountryCodes.India)) && notifyParty.Country.Name.IsEmpty, messageErrorForRequireFullAddressForIndia);
				notifyParty.Country.AddValidationDependencies(notifyParty.Country.NameInfo, consignee.Country.NameInfo, consignee.CompanyNameInfo, wrapper.Destination.CodeInfo, notifyParty.CompanyNameInfo);
				notifyParty.AddressLine1Info.AddMessageError(() => !notifyParty.IsSameAsConsignee()
					&& IsTransitToCountry(wrapper, code => code == CountryCodes.India)
					&& (consignee.IsToOrder() || (consignee.Country.Code != string.Empty && consignee.Country.Code != CountryCodes.India))
					&& notifyParty.AddressLine1.IsEmpty
					&& notifyParty.AddressLine2.IsEmpty, messageErrorForRequireFullAddressForIndia);

				notifyParty.AddValidationDependencies(notifyParty.AddressLine1Info, notifyParty.AddressLine2Info, consignee.CompanyNameInfo, consignee.Country.CodeInfo);
			}
			else
			{
				notifyParty.AddValidationDependencies(notifyParty.CompanyNameInfo, consignee.CompanyNameInfo, consignee.Country.NameInfo);
				notifyParty.Country.AddValidationDependencies(notifyParty.Country.NameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo, notifyParty.CompanyNameInfo);
			}

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
					CopyTaxInfoOriginalCollectionToTaxInfoCollection(wrapper.NotifyPartyTaxInfo, wrapper.NotifyPartyTaxInfoOriginal);
				}
				else if (!AddressExtensions.IsSameAsConsignee(oldValue) && AddressExtensions.IsSameAsConsignee(newValue))
				{
					CleanUpTaxNumbersWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(wrapper);
				}
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string errorForMissingConsigneeAndNotifyPartyContactDetails = "Please enter at least one communication method (phone or email) for the Consignee or Notify Party.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string messageErrorForRequireFullAddressForIndia = "Full address is required to comply with SCMTR Manifest reporting for India.";

		void AddShipperAddressValidation(CarrierMessageData wrapper)
		{
			var shipper = wrapper.Shipper;
			wrapper.Shipper.Country.NameInfo.AddWarning(() => !shipper.CompanyName.IsEmpty && !shipper.TaxNumber.IsEmpty && wrapper.Origin.Code.StartsWith(CountryCodes.China, StringComparison.OrdinalIgnoreCase) && !shipper.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.China)), (NoResString)"Shipper's country is different from Origin country."); // non-translatable validation message
			wrapper.Shipper.AddValidationDependencies(shipper.Country.NameInfo, shipper.CompanyNameInfo, shipper.TaxNumberInfo, wrapper.Origin.CodeInfo);
		}

		#endregion

		#region Charges Validation

		protected override void AddChargesValidation(CarrierMessageData wrapper)
		{
			base.AddChargesValidation(wrapper);

			var mustChoseOneMessageError = (NoResString)"A payment type must be selected for 'Freight Charges'.";  // non-translatable registration number
			wrapper.IsFreightPrepaidInfo.AddMessageError(() => !wrapper.IsFreightPrepaid && !wrapper.IsFreightCollect && !wrapper.IsFreightAsAgreed, mustChoseOneMessageError);
			wrapper.IsFreightCollectInfo.AddMessageError(() => !wrapper.IsFreightPrepaid && !wrapper.IsFreightCollect && !wrapper.IsFreightAsAgreed, mustChoseOneMessageError);
			wrapper.IsFreightAsAgreedInfo.AddMessageError(() => !wrapper.IsFreightPrepaid && !wrapper.IsFreightCollect && !wrapper.IsFreightAsAgreed, mustChoseOneMessageError);
			wrapper.AddValidationDependencies(wrapper.IsFreightPrepaidInfo, wrapper.IsFreightCollectInfo, wrapper.IsFreightAsAgreedInfo);
			wrapper.AddValidationDependencies(wrapper.IsFreightCollectInfo, wrapper.IsFreightPrepaidInfo, wrapper.IsFreightAsAgreedInfo);
			wrapper.AddValidationDependencies(wrapper.IsFreightAsAgreedInfo, wrapper.IsFreightCollectInfo, wrapper.IsFreightPrepaidInfo);

			wrapper.OtherBillClausesInfo.AddAsciiCharactersValidation();

			LinkChargesValue(wrapper);
		}

		void LinkChargesValue(CarrierMessageData wrapper)
		{
			var optionalChargeBasicFreight = (OptionalCharge)wrapper.OptionalChargeBasicFreight;

			optionalChargeBasicFreight.IsPrepaidInfo.ValueChanged += (s, e) =>
			{
				if (optionalChargeBasicFreight.IsPrepaid)
				{
					wrapper.IsFreightPrepaid = true;
				}
			};

			optionalChargeBasicFreight.IsCollectInfo.ValueChanged += (s, e) =>
			{
				if (optionalChargeBasicFreight.IsCollect)
				{
					wrapper.IsFreightCollect = true;
				}
			};

			optionalChargeBasicFreight.IsPayableElsewhereInfo.ValueChanged += (s, e) =>
			{
				if (optionalChargeBasicFreight.IsPayableElsewhere)
				{
					wrapper.IsFreightAsAgreed = true;
				}
			};
		}

		#endregion

		#region Tax Number Validation

		void CleanUpTaxNumbersWhenConsigneeIsToOrderOrNotifyPartyIsSameAsConsignee(CarrierMessageData wrapper)
		{
			if (wrapper.Consignee.IsToOrder() && wrapper.ConsigneeTaxInfo != null)
			{
				foreach (var taxInfo in wrapper.ConsigneeTaxInfo)
				{
					taxInfo.Clear();
				}
			}

			if (wrapper.NotifyParty.IsSameAsConsignee() && wrapper.NotifyPartyTaxInfo != null)
			{
				foreach (var taxInfo in wrapper.NotifyPartyTaxInfo)
				{
					taxInfo.Clear();
				}
			}
		}

		void CopyTaxInfoOriginalCollectionToTaxInfoCollection(IReadOnlyCollection<TaxInfo> taxInfoCollection, IReadOnlyCollection<TaxInfo> taxInfoCollectionOriginal)
		{
			var i = 0;
			foreach (var taxInfo in taxInfoCollection)
			{
				taxInfo.Code = taxInfoCollectionOriginal.ElementAt(i).Code;
				if (taxInfo.Country != null && taxInfoCollectionOriginal.ElementAt(i).Country != null)
				{
					taxInfo.Country.Code = taxInfoCollectionOriginal.ElementAt(i).Country.Code;
					taxInfo.Country.Name = taxInfoCollectionOriginal.ElementAt(i).Country.Name;
				}

				if (taxInfo.RegulatingCountry != null && taxInfoCollectionOriginal.ElementAt(i).RegulatingCountry != null)
				{
					taxInfo.RegulatingCountry.Code = taxInfoCollectionOriginal.ElementAt(i).RegulatingCountry.Code;
					taxInfo.RegulatingCountry.Name = taxInfoCollectionOriginal.ElementAt(i).RegulatingCountry.Name;
				}

				taxInfo.Description = taxInfoCollectionOriginal.ElementAt(i).Description;
				taxInfo.LongLabel = taxInfoCollectionOriginal.ElementAt(i).LongLabel;
				taxInfo.ShortLabel = taxInfoCollectionOriginal.ElementAt(i).ShortLabel;
				taxInfo.Number = taxInfoCollectionOriginal.ElementAt(i).Number;
				taxInfo.DisplayedLabel = taxInfoCollectionOriginal.ElementAt(i).DisplayedLabel;
				taxInfo.IsChinaSpecific = taxInfoCollectionOriginal.ElementAt(i).IsChinaSpecific;
				taxInfo.IsPlaceHolder = taxInfoCollectionOriginal.ElementAt(i).IsPlaceHolder;

				i++;
			}
		}

		void AddTaxNumberValidation(CarrierMessageData wrapper)
		{
			AddConsigneeTaxNumberValidation_RefDataTable(wrapper);
			AddNotifyPartyTaxNumberValidation_RefDataTable(wrapper);
			AddShipperTaxNumberValidation_RefDataTable(wrapper);
		}

		void AddConsigneeTaxNumberValidation_RefDataTable(CarrierMessageData wrapper)
		{
			var consignee = wrapper.Consignee;
			var notifyParty = wrapper.NotifyParty;

			void AddValidations(TaxInfo taxInfo)
			{
				taxInfo.NumberInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Bangladesh) && !consignee.IsToOrder() && wrapper.IsDirect && taxInfo.Code == OrgCusCode.CodeTypes.VATCode && taxInfo.Number.IsEmpty, bangladeshImportsConsigneeBINMissingErrorMessage);
				taxInfo.NumberInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Bangladesh) && !consignee.IsToOrder() && taxInfo.Code == OrgCusCode.BangladeshCodeTypes.AIN && !wrapper.IsDirect && taxInfo.Number.IsEmpty, bangladeshImportsConsigneeAINMissingErrorMessage);
				taxInfo.NumberInfo.AddWarning(() => (wrapper.PortOfLoading.IsInCountry(CountryCodes.China) || wrapper.PortOfDischarge.IsInCountry(CountryCodes.China)) && !consignee.IsToOrder() && !consignee.CompanyName.IsEmpty && IsChinaEmptyTaxNumber(taxInfo),
					string.Format(CultureInfo.InvariantCulture, chinaTaxNumberRequiredWarning, GetChinaTaxNumberTypes_RefDataTable(wrapper.ConsigneeTaxInfo, (NoResString)" or "))); // non-translatable validation message

				taxInfo.NumberInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Brazil) && !consignee.IsToOrder() && consignee.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.Brazil)) && HasMissingBrazilTaxNumbers_RefDataTable(wrapper.ConsigneeTaxInfo), brazilImportsCJNNumberMandatoryWarning);

				taxInfo.NumberInfo.AddWarning(() => wrapper.Destination.Code.StartsWith(CountryCodes.VietNam, StringComparison.OrdinalIgnoreCase) && !consignee.IsToOrder() && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.VietNam, OrgCusCode.CodeTypes.VATCode), vietnamVATNumberRequiredWarning);

				taxInfo.NumberInfo.AddMessageError(() => !consignee.IsToOrder() && wrapper.IsToEgypt && consignee.Country.Code == CountryCodes.Egypt && taxInfo.RegulatingCountry.Code == CountryCodes.Egypt && taxInfo.Number.IsEmpty, egConsigneeTaxNumberMessage);
				taxInfo.NumberInfo.AddMessageError(() => wrapper.IsToEgypt && consignee.Country.Code == CountryCodes.Egypt && taxInfo.RegulatingCountry.Code == CountryCodes.Egypt && taxInfo.Code == OrgCusCode.CodeTypes.VATCode && !Regex.IsMatch(taxInfo.Number, @"^\d{9}$"), egConsigneeVATNumberValidationMessage);

				taxInfo.NumberInfo.AddWarning(() => !consignee.IsToOrder() && wrapper.IsToKenya && consignee.Country.Code == CountryCodes.Kenya && taxInfo.RegulatingCountry.Code == CountryCodes.Kenya && taxInfo.Number.IsEmpty, keConsigneeTaxNumberMessage);
				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Indonesia) && taxInfo.Number.IsEmpty, indonesiaImportsTaxNumberRequiredWarning);
				AddTaxInfoNoneFoundValidations(taxInfo);

				taxInfo.NumberInfo.AddMessageError(() => consignee.Country.Code == CountryCodes.UnitedKingdom && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedKingdom) && taxInfo.Number.IsEmpty && taxInfo.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, gbConsigneeEoriTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => consignee.Country.Code == CountryCodes.Israel && wrapper.PlaceOfDelivery.Country.Code == CountryCodes.Israel && taxInfo.RegulatingCountry.Code == CountryCodes.Israel && taxInfo.Number.IsEmpty, israelConsigneeTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Guatemala) && !consignee.IsToOrder() && consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Guatemala) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Guatemala, OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria), guatemalaConsingeeTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Panama) && !consignee.IsToOrder() && consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Panama) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Panama, PanamaOrgCusCodeInfo.OrgCusCodes.RUC), panamaConsingeeTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Morocco) && !consignee.IsToOrder() && consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Morocco) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Morocco, OrgCusCode.MoroccoCodeTypes.ICE), moroccoConsingeeTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Jordan) && !consignee.IsToOrder() && consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Jordan) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Jordan, OrgCusCode.CodeTypes.GSTCode), jordanConsingeeTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.SaudiArabia) && !consignee.IsToOrder() && consignee.Country.Code.EqualsIgnoringCase(CountryCodes.SaudiArabia) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.SaudiArabia, OrgCusCode.CodeTypes.VATCode), saudiArabiaConsingeeTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => taxInfo.Number.Length > 26, taxNumberLengthWarningMessage);

				if (EnableSCMTR)
				{
					taxInfo.NumberInfo.AddMessageError(() => !consignee.IsToOrder() && !notifyParty.IsSameAsConsignee() && RequireConsigneeMissingIndiaTaxNumberMessageError(wrapper, taxInfo), indiaConsingeeTaxNumberMessage);
					taxInfo.NumberInfo.AddMessageError(() => !consignee.IsToOrder()
							&& notifyParty.IsSameAsConsignee()
							&& (wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN) == null || taxInfo.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN)
							&& wrapper.PortOfDischarge.IsInCountry(CountryCodes.India)
							&& wrapper.Consignee.Country.Code == CountryCodes.India
							&& !HasNonEmptyTaxNumberOfCountryAndCode(wrapper.ConsigneeTaxInfo, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN), indiaSameAsConsingeeTaxNumberMessage);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, notifyParty.CompanyNameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, wrapper.ConsigneeTaxInfo.Except(new[] { taxInfo }).Select(t => t.NumberInfo).ToArray());
					taxInfo.AddValidationDependencies(taxInfo.ShortLabelInfo, consignee.CompanyNameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo);
				}
				else
				{
					taxInfo.NumberInfo.AddWarning(() => !consignee.IsToOrder() && HasMissingIndiaTaxNumbersOrEmailAddress_RefDataTable(wrapper, consignee, notifyParty), indiaTaxNumberAndEmailMessage);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, consignee.Country.NameInfo, consignee.EmailInfo, notifyParty.EmailInfo, wrapper.Destination.CodeInfo);
					taxInfo.AddValidationDependencies(taxInfo.ShortLabelInfo, consignee.CompanyNameInfo, consignee.Country.NameInfo, consignee.EmailInfo, notifyParty.EmailInfo, wrapper.Destination.CodeInfo);
				}

				taxInfo.AddValidationDependencies(taxInfo.NumberInfo, consignee.CompanyNameInfo, consignee.Country.CodeInfo);
			}

			if (EnableSCMTR)
			{
				consignee.EmailInfo.AddMessageError(() => !consignee.IsToOrder() && consignee.Email.IsEmpty && notifyParty.Email.IsEmpty && wrapper.PortOfDischarge.IsInCountry(CountryCodes.India), indiaConsigneeContactEmailMessage);
				consignee.AddValidationDependencies(consignee.EmailInfo, notifyParty.EmailInfo, consignee.CompanyNameInfo, wrapper.Destination.CodeInfo);
			}
			else
			{
				consignee.EmailInfo.AddWarning(() => HasMissingIndiaTaxNumbersOrEmailAddress_RefDataTable(wrapper, consignee, notifyParty), indiaTaxNumberAndEmailMessage);
				consignee.AddValidationDependencies(consignee.EmailInfo, notifyParty.EmailInfo, wrapper.Destination.CodeInfo);
			}

			foreach (var taxInfo in wrapper.ConsigneeTaxInfo)
			{
				AddValidations(taxInfo);
			}

			wrapper.ConsigneeTaxInfo?.FirstOrDefault()?.NumberInfo.AddMessageError(() => consignee.Country.Code == CountryCodes.UnitedKingdom && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedKingdom) && wrapper.ConsigneeTaxInfo?.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) == null, gbConsigneeEoriTaxNumberMessage);
		}

		bool RequireConsigneeMissingIndiaTaxNumberMessageError(CarrierMessageData wrapper, TaxInfo taxInfo)
		{
			return (wrapper.ConsigneeTaxInfo.Count == 1 || taxInfo.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN || taxInfo.Code == IndiaOrgCusCodeInfo.OrgCusCodes.IEC)
					&& wrapper.Consignee.Country.Code == CountryCodes.India
					&& wrapper.PortOfDischarge.IsInCountry(CountryCodes.India)
					&& HasMissingIndiaTaxNumbers_RefDataTable(wrapper, wrapper.Consignee, wrapper.ConsigneeTaxInfo);
		}

		void AddNotifyPartyTaxNumberValidation_RefDataTable(CarrierMessageData wrapper)
		{
			var consignee = wrapper.Consignee;
			var notifyParty = wrapper.NotifyParty;

			void AddValidations(TaxInfo taxInfo)
			{
				taxInfo.NumberInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Bangladesh) && !notifyParty.IsSameAsConsignee() && wrapper.IsDirect && taxInfo.Number.IsEmpty, bangladeshImportsNotifyPartyBINMissingErrorMessage);
				taxInfo.NumberInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Bangladesh) && !notifyParty.IsSameAsConsignee() && taxInfo.Code == OrgCusCode.BangladeshCodeTypes.AIN && !wrapper.IsDirect && taxInfo.Number.IsEmpty, bangladeshImportsNotifyPartyAINMissingErrorMessage);

				taxInfo.NumberInfo.AddMessageError(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Brazil) && (consignee.IsToOrder() || !consignee.Country.Name.EqualsIgnoringCase(nameof(CountryCodes.Brazil))) && HasMissingBrazilTaxNumbers_RefDataTable(wrapper.NotifyPartyTaxInfo), brazilImportsCJNNumberMandatoryWarning);

				taxInfo.NumberInfo.AddWarning(() => wrapper.Destination.Code.StartsWith(CountryCodes.VietNam, StringComparison.OrdinalIgnoreCase) && !notifyParty.IsSameAsConsignee() && consignee.IsToOrder() && consignee.TaxNumber.IsEmpty && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.VietNam, OrgCusCode.CodeTypes.VATCode), vietnamVATNumberRequiredWarning);
				taxInfo.NumberInfo.AddWarning(() => (wrapper.PortOfLoading.IsInCountry(CountryCodes.China) || wrapper.PortOfDischarge.IsInCountry(CountryCodes.China)) && !notifyParty.IsSameAsConsignee() && !notifyParty.CompanyName.IsEmpty && IsChinaEmptyTaxNumber(taxInfo),
					string.Format(CultureInfo.InvariantCulture, chinaTaxNumberRequiredWarning, GetChinaTaxNumberTypes_RefDataTable(wrapper.NotifyPartyTaxInfo, (NoResString)" or "))); // non-translatable validation message
				AddTaxInfoNoneFoundValidations(taxInfo);

				taxInfo.NumberInfo.AddMessageError(() => consignee.IsToOrder() && !notifyParty.IsSameAsConsignee() && wrapper.IsToEgypt && notifyParty.Country.Code == CountryCodes.Egypt && taxInfo.RegulatingCountry.Code == CountryCodes.Egypt && taxInfo.Number.IsEmpty, egNotifyPartyTaxNumberMessage);
				taxInfo.NumberInfo.AddWarning(() => consignee.IsToOrder() && !notifyParty.IsSameAsConsignee() && wrapper.IsToKenya && notifyParty.Country.Code == CountryCodes.Kenya && taxInfo.RegulatingCountry.Code == CountryCodes.Kenya && taxInfo.Number.IsEmpty, keNotifyPartyTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() =>
					(consignee.IsToOrder() || consignee.Country.Code != CountryCodes.Israel) && !notifyParty.IsSameAsConsignee() && wrapper.PlaceOfDelivery.Country.Code == CountryCodes.Israel && notifyParty.Country.Code == CountryCodes.Israel && taxInfo.RegulatingCountry.Code == CountryCodes.Israel && taxInfo.Number.IsEmpty,
					israelNotifyPartyTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Guatemala) && (consignee.IsToOrder() || !consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Guatemala)) && !notifyParty.IsSameAsConsignee() && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Guatemala, OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria), guatemalaNotifyTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Morocco) && (consignee.IsToOrder() || !consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Morocco)) && !notifyParty.IsSameAsConsignee() && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Morocco, OrgCusCode.MoroccoCodeTypes.ICE), moroccoNotifyTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Jordan) && (consignee.IsToOrder() || !consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Jordan)) && !notifyParty.IsSameAsConsignee() && notifyParty.Country.Code.EqualsIgnoringCase(CountryCodes.Jordan) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Jordan, OrgCusCode.CodeTypes.GSTCode), jordanNotifyTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.SaudiArabia) && (consignee.IsToOrder() || !consignee.Country.Code.EqualsIgnoringCase(CountryCodes.SaudiArabia)) && !notifyParty.IsSameAsConsignee() && notifyParty.Country.Code.EqualsIgnoringCase(CountryCodes.SaudiArabia) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.SaudiArabia, OrgCusCode.CodeTypes.VATCode), saudiArabiaNotifyTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => taxInfo.Number.Length > 26, taxNumberLengthWarningMessage);

				if (EnableSCMTR)
				{
					taxInfo.NumberInfo.AddMessageError(() => !notifyParty.IsSameAsConsignee() && RequireNotifyPartyMissingIndiaTaxNumberMessageError(wrapper, taxInfo), indiaNotifyPartyTaxNumberMessage);
					taxInfo.AddValidationDependencies(taxInfo.ShortLabelInfo, notifyParty.CompanyNameInfo, consignee.CompanyNameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, notifyParty.CompanyNameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, wrapper.NotifyPartyTaxInfo.Except(new[] { taxInfo }).Select(t => t.NumberInfo).ToArray());
				}
				else
				{
					taxInfo.NumberInfo.AddWarning(() => !notifyParty.IsSameAsConsignee() && HasMissingIndiaTaxNumbersOrEmailAddress_RefDataTable(wrapper, consignee, notifyParty), indiaTaxNumberAndEmailMessage);
					taxInfo.AddValidationDependencies(taxInfo.ShortLabelInfo, notifyParty.CompanyNameInfo, consignee.CompanyNameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo, consignee.EmailInfo, notifyParty.EmailInfo);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, notifyParty.CompanyNameInfo, consignee.Country.NameInfo, wrapper.Destination.CodeInfo, consignee.EmailInfo, notifyParty.EmailInfo);
				}

				taxInfo.NumberInfo.AddMessageError(() => notifyParty.Country.Code == CountryCodes.UnitedKingdom && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedKingdom) && taxInfo.Number.IsEmpty && taxInfo.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, gbNotifyPartyEoriTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfDischarge.IsInCountry(CountryCodes.Panama) && (consignee.IsToOrder() || !consignee.Country.Code.EqualsIgnoringCase(CountryCodes.Panama)) && !notifyParty.IsSameAsConsignee() && notifyParty.Country.Code.EqualsIgnoringCase(CountryCodes.Panama) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Panama, PanamaOrgCusCodeInfo.OrgCusCodes.RUC), panamaNotifyTaxNumberMessage);

				taxInfo.AddValidationDependencies(taxInfo.NumberInfo, consignee.CompanyNameInfo, consignee.Country.CodeInfo, notifyParty.CompanyNameInfo, notifyParty.Country.CodeInfo);
			}

			if (EnableSCMTR)
			{
				notifyParty.EmailInfo.AddMessageError(() => !notifyParty.IsSameAsConsignee() && consignee.Email.IsEmpty && notifyParty.Email.IsEmpty && wrapper.PortOfDischarge.IsInCountry(CountryCodes.India), indiaNotifyPartyContactEmailMessage);
				notifyParty.AddValidationDependencies(notifyParty.EmailInfo, consignee.EmailInfo, notifyParty.CompanyNameInfo, wrapper.Destination.CodeInfo);
			}
			else
			{
				notifyParty.EmailInfo.AddWarning(() => HasMissingIndiaTaxNumbersOrEmailAddress_RefDataTable(wrapper, consignee, notifyParty), indiaTaxNumberAndEmailMessage);
				notifyParty.AddValidationDependencies(notifyParty.EmailInfo, consignee.EmailInfo, wrapper.Destination.CodeInfo);
			}

			foreach (var taxInfo in wrapper.NotifyPartyTaxInfo)
			{
				AddValidations(taxInfo);
			}

			wrapper.NotifyPartyTaxInfo?.FirstOrDefault()?.NumberInfo.AddMessageError(() => notifyParty.Country.Code == CountryCodes.UnitedKingdom && wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedKingdom) && wrapper.NotifyPartyTaxInfo?.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) == null, gbNotifyPartyEoriTaxNumberMessage);
		}

		bool RequireNotifyPartyMissingIndiaTaxNumberMessageError(CarrierMessageData wrapper, TaxInfo taxInfo)
		{
			return (wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN) == null || taxInfo.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN)
				&& wrapper.PortOfDischarge.IsInCountry(CountryCodes.India)
				&& wrapper.NotifyParty.Country.Code == CountryCodes.India
				&& !HasNonEmptyTaxNumberOfCountryAndCode(wrapper.NotifyPartyTaxInfo, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN);
		}

		void AddShipperTaxNumberValidation_RefDataTable(CarrierMessageData wrapper)
		{
			var shipper = wrapper.Shipper;
			var firstLegLoadPort = Transports.FirstOrDefault()?.JW_RL_NKLoadPort ?? ZString.Empty;

			void AddValidations(TaxInfo taxInfo)
			{
				taxInfo.NumberInfo.AddWarning(() => (wrapper.PortOfLoading.IsInCountry(CountryCodes.China) || wrapper.PortOfDischarge.IsInCountry(CountryCodes.China)) && !shipper.CompanyName.IsEmpty && IsChinaEmptyTaxNumber(taxInfo),
					string.Format(CultureInfo.InvariantCulture, chinaTaxNumberRequiredWarning, GetChinaTaxNumberTypes_RefDataTable(wrapper.ShipperTaxInfo, (NoResString)" or "))); // non-translatable validation message

				taxInfo.NumberInfo.AddMessageError(() => firstLegLoadPort.StartsWith(CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase) && HasMissingBrazilTaxNumbers_RefDataTable(wrapper.ShipperTaxInfo), brazilExportsCJNNumberMandatoryMessage);

				taxInfo.NumberInfo.AddWarning(() => HasMissingIndonesiaExportsTaxNumbers_RefDataTable(wrapper), indonesiaExportsTaxNumberRequiredWarning);

				taxInfo.NumberInfo.AddMessageError(() => wrapper.IsToEgypt && taxInfo.RegulatingCountry.Code == CountryCodes.Egypt && taxInfo.Number.IsEmpty, egShipperTaxNumberMessage);
				taxInfo.NumberInfo.AddWarning(() => wrapper.IsToEgypt && taxInfo.RegulatingCountry.Code == CountryCodes.Egypt && taxInfo.Number.Length > 17, egShipperExportRegistrationNumberMaximumCharWarningMessage);
				taxInfo.NumberInfo.AddMessageError(() => wrapper.IsToEgypt && taxInfo.RegulatingCountry.Code == CountryCodes.Egypt && (!Regex.IsMatch(taxInfo.Number, @"^[A-Z]{2}-0[1|2]-[\s\S]{4,}$") || taxInfo.Number.Split("-")[0] != shipper.Country.Code), egShipperExportRegistrationNumberFormatErrorMessage);

				taxInfo.NumberInfo.AddWarning(() => wrapper.PortOfLoading.IsInCountry(CountryCodes.Panama) && shipper.Country.Code.EqualsIgnoringCase(CountryCodes.Panama) && IsEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Panama, PanamaOrgCusCodeInfo.OrgCusCodes.RUC), panamaShipperTaxNumberMessage);

				taxInfo.NumberInfo.AddWarning(() => taxInfo.Number.Length > 26, taxNumberLengthWarningMessage);

				AddTaxInfoNoneFoundValidations(taxInfo);
				taxInfo.AddValidationDependencies(taxInfo.NumberInfo, shipper.CompanyNameInfo);

				if (EnableSCMTR)
				{
					taxInfo.NumberInfo.AddMessageError(() => RequireShipperMissingIndiaTaxNumberMessageError(wrapper, taxInfo), indiaShipperTaxNumberMessage);
					taxInfo.AddValidationDependencies(taxInfo.NumberInfo, wrapper.ShipperTaxInfo.Except(new[] { taxInfo }).Select(t => t.NumberInfo).ToArray());
				}

				taxInfo.NumberInfo.AddMessageError(() => shipper.Country.Code == CountryCodes.UnitedKingdom && wrapper.PortOfLoading.IsInCountry(CountryCodes.UnitedKingdom) && taxInfo.Number.IsEmpty && taxInfo.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, gbShipperEoriTaxNumberMessage);
			}

			foreach (var taxInfo in wrapper.ShipperTaxInfo)
			{
				AddValidations(taxInfo);
			}

			wrapper.ShipperTaxInfo?.FirstOrDefault()?.NumberInfo.AddMessageError(() => shipper.Country.Code == CountryCodes.UnitedKingdom && wrapper.PortOfLoading.IsInCountry(CountryCodes.UnitedKingdom) && wrapper.ShipperTaxInfo?.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) == null, gbShipperEoriTaxNumberMessage);
		}

		bool RequireShipperMissingIndiaTaxNumberMessageError(CarrierMessageData wrapper, TaxInfo taxInfo)
		{
			return (wrapper.ShipperTaxInfo.Count == 1 || taxInfo.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN || taxInfo.Code == IndiaOrgCusCodeInfo.OrgCusCodes.IEC)
					&& wrapper.Shipper.Country.Code == CountryCodes.India
					&& wrapper.PortOfLoading.IsInCountry(CountryCodes.India)
					&& HasMissingIndiaTaxNumbers_RefDataTable(wrapper, wrapper.Shipper, wrapper.ShipperTaxInfo);
		}

		void AddTaxInfoNoneFoundValidations(TaxInfo taxInfo)
		{
			Func<ZString, bool> validLabel = label => label == "8888" || label == "9999";
			taxInfo.ShortLabelInfo.AddWarning(() =>
			{
				return taxInfo.IsPlaceHolder && validLabel(taxInfo.ShortLabel);
			}, taxInfoNoneFoundWarning);
			taxInfo.ShortLabelInfo.AddMessageError(() => taxInfo.IsPlaceHolder && !validLabel(taxInfo.ShortLabel), taxInfoNoneFoundInvalidValue);
		}

		bool IsEmptyTaxNumberOfCountryAndCode(TaxInfo taxInfo, ZString country, ZString code)
		{
			return (taxInfo.RegulatingCountry?.Code ?? ZString.Empty) == country && taxInfo.Code == code && taxInfo.Number.IsEmpty;
		}

		bool IsChinaEmptyTaxNumber(TaxInfo taxInfo)
		{
			return !taxInfo.Code.IsEmpty && taxInfo.Number.IsEmpty && (taxInfo.RegulatingCountry?.Code ?? ZString.Empty) == CountryCodes.China;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string bangladeshImportsConsigneeBINMissingErrorMessage = "The Consignee VAT (BIN – Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string bangladeshImportsConsigneeAINMissingErrorMessage = "The Sending Agent AIN (Agent Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string bangladeshImportsNotifyPartyBINMissingErrorMessage = "The Notify Party VAT (BIN – Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string bangladeshImportsNotifyPartyAINMissingErrorMessage = "The Notify Party AIN (Agent Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string brazilImportsCJNNumberMandatoryWarning = "CNPJ or CPF number is mandatory for Brazil imports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=BR, Type=CJN or CPF.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string brazilExportsCJNNumberMandatoryMessage = "CNPJ or CPF number is mandatory for Brazil exports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=BR, Type=CJN or CPF.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string vietnamVATNumberRequiredWarning = "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string chinaTaxNumberRequiredWarning = "Company ID, {0}\r\nis required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indonesiaExportsTaxNumberRequiredWarning = "Consignor PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indonesiaImportsTaxNumberRequiredWarning = "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string taxInfoNoneFoundWarning = "9999 is used for company, but you can modify it to 8888 for an individual";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string taxInfoNoneFoundInvalidValue = "Must be 9999 for company, or 8888 for an individual";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaTaxNumberAndEmailMessage = "For India, the Consignee or Notify Party requires the IEC and GST numbers together with a contact email address. Ensure this information is included if applicable.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaShipperTaxNumberMessage = "Shipper's IEC or PAN is required for exports from India.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaConsingeeTaxNumberMessage = "Consignee's IEC or PAN is required for imports to India.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaSameAsConsingeeTaxNumberMessage = "Consignee's PAN is required for imports to India when the Notify Party is 'Same as Consignee'.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaNotifyPartyTaxNumberMessage = "Notify Party PAN is required to comply with SCMTR Manifest reporting for India.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaConsigneeContactEmailMessage = "Consignee's Email is required for India imports.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string indiaNotifyPartyContactEmailMessage = "Notify Party's Email is required for India imports.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string gbConsigneeEoriTaxNumberMessage = "Consignee's EORI is required for Great Britain.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string gbNotifyPartyEoriTaxNumberMessage = "Notify Party's EORI is required for Great Britain.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string gbShipperEoriTaxNumberMessage = "Shipper's EORI is required for Great Britain.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string egShipperTaxNumberMessage = "Exporter registration number of the Shipper is mandatory for cargo destined to Egypt.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string egShipperExportRegistrationNumberMaximumCharWarningMessage = "To comply with requirements from certain carriers, Shipper’s Exporter Registration Number \r\nshould be a maximum of 17 alphanumerics including special characters.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string egShipperExportRegistrationNumberFormatErrorMessage = "Shipper’s Exporter Registration Number should consist of: \r\n- Shipper’s country code \r\n- Type of registration (01-Company Registry, 02-VAT) \r\n- Exporter registration number";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string guatemalaConsingeeTaxNumberMessage = "Consignee NIT is required for Guatemala Imports.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string guatemalaNotifyTaxNumberMessage = "Notify NIT is required for Guatemala Imports.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string moroccoConsingeeTaxNumberMessage = "Consignee ICE is required for Imports to Morocco.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string moroccoNotifyTaxNumberMessage = "Notify ICE is required for Imports to Morocco.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string saudiArabiaConsingeeTaxNumberMessage = "Consignee VAT is required for Imports to Saudi Arabia.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string saudiArabiaNotifyTaxNumberMessage = "Notify VAT is required for Imports to Saudi Arabia.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string taxNumberLengthWarningMessage = @"Company tax ID longer than 26 characters may result in rejection from carriers.
Please maintain proper ID in the Organization > Config > Registration Numbers/Codes tab.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string egConsigneeTaxNumberMessage = @"Consignee’s Egyptian Importer VAT Number is mandatory for imports to Egypt,
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=EG, Type=VAT.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string egConsigneeVATNumberValidationMessage = "Consignee’s Egyptian Importer VAT Number should contain 9 digits.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string egNotifyPartyTaxNumberMessage = @"Notify Party’s Egyptian Importer VAT Number is mandatory for imports to Egypt when Consignee is 'TO ORDER',
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=EG, Type=VAT.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string keConsigneeTaxNumberMessage = @"Consignee PIN (Personal Identification Number) is required to comply with Manifest reporting for Kenya,
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=KE, Type=PIN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string keNotifyPartyTaxNumberMessage = @"Notify Party PIN (Personal Identification Number) is required to comply with Manifest reporting for Kenya,
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=KE, Type=PIN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string israelConsigneeTaxNumberMessage = @"Consignee VAT Number is required for imports to Israel to comply with Manifest reporting.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string israelNotifyPartyTaxNumberMessage = @"Notify Party VAT Number is required when Consignee is ‘TO ORDER’ or its address is not in Israel for imports to Israel to comply with Manifest reporting.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string panamaConsingeeTaxNumberMessage = "Consignee RUC is required for Panama imports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=PA, Type=RUC.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string panamaNotifyTaxNumberMessage = "Notify Party RUC is required for Panama imports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=PA, Type=RUC.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string panamaShipperTaxNumberMessage = "Shipper RUC is required for Panama exports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=PA, Type=RUC.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string jordanConsingeeTaxNumberMessage = "Consignee GST is required for Jordan Imports.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string jordanNotifyTaxNumberMessage = "Notify GST is required for Jordan Imports.";

		bool HasMissingIndiaTaxNumbers_RefDataTable(CarrierMessageData wrapper, Address address, IReadOnlyCollection<TaxInfo> taxInfoCollection)
		{
			return !HasNonEmptyTaxNumberOfCountryAndCode(taxInfoCollection, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.PAN) && !HasNonEmptyTaxNumberOfCountryAndCode(taxInfoCollection, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.IEC);
		}

		bool HasMissingIndiaTaxNumbersOrEmailAddress_RefDataTable(CarrierMessageData wrapper, Address consignee, Address notifyParty)
		{
			return wrapper.PortOfDischarge.IsInCountry(CountryCodes.India)
				&& (!HasNonEmptyTaxNumberOfCountryAndCode(wrapper.ConsigneeTaxInfo, CountryCodes.India, OrgCusCode.CodeTypes.GSTCode) || !HasNonEmptyTaxNumberOfCountryAndCode(wrapper.ConsigneeTaxInfo, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.IEC) || consignee.Email.IsEmpty)
				&& (!HasNonEmptyTaxNumberOfCountryAndCode(wrapper.NotifyPartyTaxInfo, CountryCodes.India, OrgCusCode.CodeTypes.GSTCode) || !HasNonEmptyTaxNumberOfCountryAndCode(wrapper.NotifyPartyTaxInfo, CountryCodes.India, IndiaOrgCusCodeInfo.OrgCusCodes.IEC) || notifyParty.Email.IsEmpty);
		}

		bool HasNonEmptyTaxNumberOfCountryAndCode(IReadOnlyCollection<TaxInfo> taxInfoCollection, ZString countryCode, ZString code)
		{
			return taxInfoCollection.Any(taxInfo => (taxInfo.Country?.Code ?? ZString.Empty) == countryCode && (taxInfo.RegulatingCountry?.Code ?? ZString.Empty) == countryCode && taxInfo.Code == code && !taxInfo.Number.IsEmpty);
		}

		bool HasMissingIndonesiaExportsTaxNumbers_RefDataTable(CarrierMessageData wrapper)
		{
			return wrapper.Origin.IsInCountry(CountryCodes.Indonesia)
					 && !HasNonEmptyTaxNumberOfCountryAndCode(wrapper.ShipperTaxInfo, CountryCodes.Indonesia, OrgCusCode.IndonesiaCodeTypes.PPN)
					 && !HasNonEmptyTaxNumberOfCountryAndCode(wrapper.ShipperTaxInfo, CountryCodes.Indonesia, OrgCusCode.CodeTypes.PassportID);
		}

		bool HasMissingBrazilTaxNumbers_RefDataTable(IReadOnlyCollection<TaxInfo> taxInfo)
		{
			return !HasNonEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ)
				&& !HasNonEmptyTaxNumberOfCountryAndCode(taxInfo, CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration);
		}

		#endregion

		#region Ports Validation

		void AddPortsValidation(CarrierMessageData wrapper)
		{
			wrapper.PlaceOfIssue.CodeInfo.AddAsciiCharactersValidation();

			var enterValidUnlocoMessageError = Res.GetString("5A367CCC-DB84-4022-B69E-602C600BB810", "You have not entered a valid un loco.");

			wrapper.OperationalPort?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("314B9F4E-73C1-44D0-838E-CA5E4FC6B74C", "Operational Port is required."));
			wrapper.OperationalPort?.CodeInfo.AddAsciiCharactersValidation();
			wrapper.OperationalPort?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			wrapper.FreightPayableAt?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("8D1CF2F4-D871-491F-B539-FC97902DAB66", "Freight Payable At is required."));
			wrapper.FreightPayableAt?.CodeInfo.AddAsciiCharactersValidation();
			wrapper.FreightPayableAt?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
		}

		#endregion

		#region Shipments Validation

		void AddShipmentsValidation(CarrierMessageData wrapper)
		{
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => !wrapper.Shipments.Any(), (NoResString)"You need at least one shipment"); // non-translatable validation message

			var firstLegLoadPort = Transports.FirstOrDefault()?.JW_RL_NKLoadPort ?? ZString.Empty;
			var lastLegDischargePort = Transports.LastOrDefault()?.JW_RL_NKDiscPort ?? ZString.Empty;
			var hasUSOrUSTerritoryExportTransport = CountryCodes.IsUsaOrTerritory(firstLegLoadPort.SubstringSafe(0, 2).ToUpper())
				&& CountryCodes.IsUsaOrTerritory(wrapper.PortOfLoading.Country.Code)
				&& firstLegLoadPort.Substring(0, 2) != lastLegDischargePort.Substring(0, 2);

			if (hasUSOrUSTerritoryExportTransport)
			{
				foreach (Shipment shipment in wrapper.Shipments)
				{
					ValidateITNNumber(shipment);
				}
			}

			if (wrapper.IsToSpecificAfricanCountry)
			{
				foreach (var shipment in wrapper.Shipments)
				{
					ValidateCTKNumber(shipment);
				}
			}

			if (wrapper.IsCanadaExport)
			{
				foreach (var shipment in wrapper.Shipments)
				{
					ValidateCTNNumber(shipment);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void ValidateCTKNumber(Shipment shipment)
		{
			if (IsGroupAndConsolidatePackingLines)
			{
				foreach (var groupedPackingLine in shipment.PackingLines)
				{
					groupedPackingLine.GroupCTKNumberInfo.AddWarningIfEmpty(ZString.Format("The CTK - Cargo Tracking Note number is required for cargo destined to {0}.\r\nPlease enter CTK in either Consol > Details > Numbers > Reference Numbers,\r\nOr in Shipment ({1}) > Additional Details > Reference Numbers.", GetCountryName(consol.JK_RL_NKDischargePort), groupedPackingLine.ShipmentID));
				}
			}
			else
			{
				shipment.CTKNumberInfo.AddWarningIfEmpty(ZString.Format("The CTK - Cargo Tracking Note number is required for cargo destined to {0}.\r\nPlease enter CTK in either Consol > Details > Numbers > Reference Numbers,\r\nOr in Shipment ({1}) > Additional Details > Reference Numbers.", GetCountryName(consol.JK_RL_NKDischargePort), shipment.ShipmentID));
			}

			foreach (var subShipment in shipment.Shipments)
			{
				ValidateCTKNumber(subShipment);
			}
		}

		ZString GetCountryName(ZString unloco)
		{
			var countryCode = unloco.SubstringSafe(0, 2);
			return RefCountry.LoadFromCountryCode(consol.Factory, countryCode)?.RN_Desc ?? ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void ValidateITNNumber(Shipment shipment)
		{
			if (IsGroupAndConsolidatePackingLines)
			{
				foreach (var groupedPackingLine in shipment.PackingLines)
				{
					groupedPackingLine.GroupITNNumberInfo.AddMessageError(() => !groupedPackingLine.GroupITNNumber.IsEmpty && groupedPackingLine.GroupITNNumber.Split(",").Any(x => x.Trim().Length > 35), (NoResString)"Number with over 35 characters will not be accepted by the carrier.");
					groupedPackingLine.GroupITNNumberInfo.AddWarning(() => !groupedPackingLine.GroupITNNumber.IsEmpty && groupedPackingLine.GroupITNNumber.Split(",").All(x => x.Trim().Length <= 35) && !groupedPackingLine.GroupITNNumber.Split(",").All(x => IsValidITNNumber(x.Trim())), (NoResString)"Please enter valid ITN number(s), the number must start with the letter \"X\", \r\nfollowed by the year, month and day of acceptance in the AES, and six randomly assigned digits.");
					groupedPackingLine.GroupITNNumberInfo.AddMessageError(() => groupedPackingLine.GroupITNNumber.IsEmpty && groupedPackingLine.GroupPOFNumber.IsEmpty, ZString.Format((NoResString)"{0}: ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.", groupedPackingLine.ShipmentID));

					var originalITNNumber = groupedPackingLine.GroupITNNumber;

					var shipmentIDs = groupedPackingLine.ShipmentID.Split(", ");
					var packingLineShipments = shipmentIDs.Select(shipmentID => CarrierMessageDataExtensions.GetMatchedShipmentForPackLine(shipment, shipmentID));
					groupedPackingLine.GroupITNNumberInfo.AddMessageError(() => groupedPackingLine.GroupITNNumber == originalITNNumber && packingLineShipments.Any(x => x.ITNNumber.IsEmpty && x.ExportStatement.IsEmpty), ZString.Format("{0}: ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.", string.Join(", ", packingLineShipments.Where(x => x.ITNNumber.IsEmpty && x.ExportStatement.IsEmpty).Select(x => x.ShipmentID))));

					groupedPackingLine.GroupITNNumberInfo.AddWarning(() => !groupedPackingLine.GroupITNNumber.IsEmpty && !groupedPackingLine.GroupPOFNumber.IsEmpty, (NoResString)"When both the ITN and Export Statement are entered, only the ITN information will be transmitted to the carrier via electronic messaging.");
				}
			}
			else
			{
				if (shipment.ExportStatement.IsEmpty)
				{
					shipment.ITNNumberInfo.AddMessageErrorIfEmpty(ZString.Format((NoResString)"{0}: ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.", shipment.ShipmentID));
				}
				shipment.ITNNumberInfo.AddMessageError(() => !shipment.ITNNumber.IsEmpty && shipment.ITNNumber.Split(",").Any(x => x.Trim().Length > 35), (NoResString)"Number with over 35 characters will not be accepted by the carrier.");
				shipment.ITNNumberInfo.AddWarning(() => !shipment.ITNNumber.IsEmpty && shipment.ITNNumber.Split(",").All(x => x.Trim().Length <= 35) && !shipment.ITNNumber.Split(",").All(x => IsValidITNNumber(x.Trim())), (NoResString)"Please enter valid ITN number(s), the number must start with the letter \"X\", \r\nfollowed by the year, month and day of acceptance in the AES, and six randomly assigned digits.");
			}

			foreach (var subShipment in shipment.Shipments)
			{
				ValidateITNNumber(subShipment);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void ValidateCTNNumber(Shipment shipment)
		{
			var warningMessage = "Please enter valid CERS Proof of Report number(s). The number must start with Exporter’s Authorization ID (e.g., \"AA1234\"), \r\nfollowed by the Submission Date in the format YYYYMMDD, and Sequential Number from 1 to 99999999999.";
			var errorMessage = "Number with over 35 characters will not be accepted by the carrier.";

			if (IsGroupAndConsolidatePackingLines)
			{
				foreach (var packingLine in shipment.PackingLines)
				{
					packingLine.GroupCTNNumberInfo.AddMessageError(() => !packingLine.GroupCTNNumber.IsEmpty && packingLine.GroupCTNNumber.Split(",").Any(x => x.Trim().Length > 35), (NoResString)errorMessage);
					packingLine.GroupCTNNumberInfo.AddWarning(() => !packingLine.GroupCTNNumber.IsEmpty && packingLine.GroupCTNNumber.Split(",").All(x => x.Trim().Length <= 35) && !packingLine.GroupCTNNumber.Split(",").All(x => IsValidCTNNumber(x.Trim())), warningMessage);
				}
			}
			else
			{
				shipment.CTNNumberInfo.AddMessageError(() => !shipment.CTNNumber.IsEmpty && shipment.CTNNumber.Split(",").Any(x => x.Trim().Length > 35), (NoResString)errorMessage);
				shipment.CTNNumberInfo.AddWarning(() => !shipment.CTNNumber.IsEmpty && shipment.CTNNumber.Split(",").All(x => x.Trim().Length <= 35) && !shipment.CTNNumber.Split(",").All(x => IsValidCTNNumber(x.Trim())), warningMessage);
			}

			foreach (var subShipment in shipment.Shipments)
			{
				ValidateCTNNumber(subShipment);
			}
		}

		bool IsValidITNNumber(ZString itnNumber) => itnNumber.Length == 15 && itnNumber.StartsWith("X") && itnNumber.SubstringSafe(1).IsNumbersOnlyOrEmpty && ZDateTime.TryParseExact(itnNumber.SubstringSafe(1, 8), out _, "yyyyMMdd");

		bool IsValidCTNNumber(ZString ctnNumber) => ctnNumber.Length >= 15 && ctnNumber.Length <= 25
			&& ctnNumber.SubstringSafe(0,2).IsLettersOnlyOrEmpty
			&& ctnNumber.SubstringSafe(2).IsNumbersOnlyOrEmpty
			&& ZDateTime.TryParseExact(ctnNumber.SubstringSafe(6, 8), out _, "yyyyMMdd")
			&& ZInt.TryParse(ctnNumber.SubstringSafe(14), out var number) && number != 0;

		#endregion

		#region CarrierMessagingRequirements Validation

		protected override void AddCarrierMessagingRequirementsValidation(CarrierMessageData wrapper)
		{
			base.AddCarrierMessagingRequirementsValidation(wrapper);

			if (wrapper.ElectronicBillOfLadingProviderMandatory)
			{
				((CodeDescription)wrapper.EBLProvider).CodeInfo.AddMessageError(() => !wrapper.EBLProvider.Code.IsEmpty && !Lookups.EBLProviderList.ContainsCode(wrapper.EBLProvider.Code),
					Res.GetString("45B41009-43C5-453E-9AE5-483020C22810", "eBL Provider is not valid."));

				((CodeDescription)wrapper.EBLProvider).CodeInfo.AddMessageError(() => (wrapper.EBLProvider.Code.IsEmpty && (consol.IsDirect && (consol.DirectShipment?.Consignor?.MiscServ?.OM_FWRequiresElectronicBOLForDirectConsol ?? false) || !consol.IsDirect && (consol.SendingForwarder?.MiscServ?.OM_FWRequiresElectronicBOLForNonDirectConsol ?? false))),
					Res.GetString("C4331041-D631-4EA0-9FCD-5A3B4FD2C64E", "This carrier supports electronic bill of lading and the shipper is requesting an electronic bill of lading.\r\nPlease choose your preferred eBL Provider. Select <Not Listed> when the preferred provider is not listed."));

				((CodeDescription)wrapper.EBLProvider).CodeInfo.AddWarning(() => wrapper.EBLProvider.Code == EBLProviderConstants.Codes.NotListed &&
					(consol.IsDirect && (consol.DirectShipment?.Consignor?.MiscServ?.OM_FWRequiresElectronicBOLForDirectConsol ?? false) || !consol.IsDirect && (consol.SendingForwarder?.MiscServ?.OM_FWRequiresElectronicBOLForNonDirectConsol ?? false)),
					Res.GetString("3F321D94-686D-4710-A8FD-0C2B1681DC71", "This carrier supports electronic bill of lading and the shipper is requesting an electronic bill of lading.\r\nPlease choose your preferred eBL Provider. Select <Not Listed> when the preferred provider is not listed, no eBL will be issued by the carrier."));
			}
		}

		#endregion

		#region TaxInfo

		protected override void GetAdditionalTaxInfo(CarrierMessageData wrapper, OrgHeaderRegistrationNumberProvider provider, Country primaryCountry, List<TaxInfo> taxInfo, Country addressCountry)
		{
			if (primaryCountry != null && addressCountry != null
				&& primaryCountry.Code == CountryCodes.UnitedKingdom
				&& addressCountry.Code != CountryCodes.UnitedKingdom)
			{
				var refDatas = GetTaxInfoFromRefTable(CountryCodes.UnitedKingdom, provider, consol.Factory, CountryCodes.UnitedKingdom, addressCountry.Code, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var eoriInfo = GetTaxInfoFromRefData(wrapper, refDatas, addressCountry).FirstOrDefault();

				if (eoriInfo != null)
				{
					taxInfo.Add(eoriInfo);
				}
			}
			else if (primaryCountry != null
					&& addressCountry != null
					&& IsCountryThatHasToDistinguishDirectionForEori(primaryCountry.Code)
					&& IsCountryThatHasToDistinguishDirectionForEori(addressCountry.Code)
					&& taxInfo.FirstOrDefault(tax =>
						   tax.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
						&& tax.Country.Code == addressCountry.Code
						&& tax.RegulatingCountry.Code == addressCountry.Code
					) is var existingEoriInfo
					&& (existingEoriInfo == null || existingEoriInfo.Number.IsEmpty))
			{
				var countryIgnoreRegistrationNumberProvider = new CountryIgnoreRegistrationNumberProvider(provider);
				var refDatas = GetTaxInfoFromRefTable(addressCountry.Code, countryIgnoreRegistrationNumberProvider, consol.Factory, addressCountry.Code, addressCountry.Code, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var eoriRefDatas = refDatas.Where(t => t.DocumentType == Constants.TaxRelatedDocumentType.ShippingInstruction).ToList();
				if (eoriRefDatas.Any())
				{
					var countryCodeOfEoriRegistrationNumber = countryIgnoreRegistrationNumberProvider.FindCountryByTaxNumber(eoriRefDatas.First().Number, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
					var eoriRegistrationCountry = Country.Create(context, RefCountry.LoadFromCountryCode(consol.Factory, countryCodeOfEoriRegistrationNumber));
					var eoriInfos = GetTaxInfoFromRefData(wrapper, eoriRefDatas, eoriRegistrationCountry);

					var eoriInfo = eoriInfos.FirstOrDefault(tax => !tax.Number.IsEmpty);
					if (eoriInfo != null)
					{
						if (existingEoriInfo != null)
						{
							taxInfo.Remove(existingEoriInfo);
						}

						eoriInfo.RegulatingCountry = eoriRegistrationCountry;
						taxInfo.Add(eoriInfo);
					}
				}
			}
		}

		protected override TaxInfo GenerateTaxInfo(CarrierMessageData wrapper, TaxCodeInformation taxCodeInformation, Country country, bool useExportCountry)
		{
			var isChinaSpecific = taxCodeInformation.RegulatingCountryCode == Constants.CountryCodes.China;
			return new TaxInfo
			{
				Code = taxCodeInformation.Code,
				Description = taxCodeInformation.Description,
				ShortLabel = taxCodeInformation.ShortLabel,
				LongLabel = taxCodeInformation.LongLabel,
				Number = GetTaxNumber(wrapper, taxCodeInformation, country, useExportCountry),
				Country = country,
				IsChinaSpecific = isChinaSpecific,
				DisplayedLabel = GetDisplayedLabel(wrapper, taxCodeInformation, useExportCountry),
				RegulatingCountry = Country.Create(context, RefCountry.LoadFromCountryCode(consol.Factory, taxCodeInformation.RegulatingCountryCode))
			};
		}

		ZString GetDisplayedLabel(CarrierMessageData wrapper, TaxCodeInformation taxCodeInformation, bool useExportCountry)
		{
			if (taxCodeInformation.RegulatingCountryCode == CountryCodes.China)
			{
				return taxCodeInformation.LongLabel;
			}

			if (wrapper.IsToEgypt && taxCodeInformation.RegulatingCountryCode == CountryCodes.Egypt)
			{
				if ("VAT".Contains(taxCodeInformation.Code)) // non-translatable validation message
				{
					return (NoResString)"VAT Number"; // non-translatable validation message
				}
				if ("COM".Contains(taxCodeInformation.Code)) // non-translatable validation message
				{
					return (NoResString)"Commerical Register"; // non-translatable validation message
				}

				return useExportCountry ? (NoResString)"Exporter Register" : (NoResString)"Commercial Register"; // non-translatable validation message
			}

			return taxCodeInformation.ShortLabel;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		ZString GetTaxNumber(CarrierMessageData wrapper, TaxCodeInformation taxCodeInformation, Country country, bool useExportCountry)
		{
			var number = ZString.Empty;
			if (wrapper.IsToEgypt && taxCodeInformation.RegulatingCountryCode == CountryCodes.Egypt && useExportCountry && !string.IsNullOrEmpty(taxCodeInformation.Number))
			{
				if (string.IsNullOrEmpty(taxCodeInformation.Comments))
				{
					return taxCodeInformation.Number;
				}
				var registerType = taxCodeInformation.Comments.EqualsIgnoringCase("Tax Id") ? "02" : "01";
				return $"{taxCodeInformation.CountryCode}-{registerType}-{taxCodeInformation.Number}"; // non-translatable validation message
			}
			if (wrapper.IsToEgypt && taxCodeInformation.RegulatingCountryCode == CountryCodes.Egypt && taxCodeInformation.Code == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(taxCodeInformation.Number))
			{
				return Regex.Replace(taxCodeInformation.Number, @"[^0-9]+", "");
			}
			if (taxCodeInformation.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori && !taxCodeInformation.Number.IsEmpty)
			{
				if (country != null && !country.Code.EqualsIgnoringCase(taxCodeInformation.Number.SubstringSafe(0, 2))
					&& !(country.Code.EqualsIgnoringCase(CountryCodes.UnitedKingdom) && taxCodeInformation.Number.StartsWith("XI", StringComparison.OrdinalIgnoreCase)))
				{
					return (country.Code + taxCodeInformation.Number).ToUpper();
				}

				return taxCodeInformation.Number.ToUpper();
			}

			number += taxCodeInformation.Number;
			return number;
		}

		protected override bool ShouldFilterDirection(TaxCodeInformation taxCodeInformation, string countryCode, bool useExportCountry)
		{
			if (taxCodeInformation.Code == OrgCusCode.MoroccoCodeTypes.ICE && countryCode == Constants.CountryCodes.Morocco)
			{
				return true;
			}

			if (taxCodeInformation.Code == PanamaOrgCusCodeInfo.OrgCusCodes.RUC && countryCode == Constants.CountryCodes.Panama)
			{
				return true;
			}

			if (taxCodeInformation.Code == OrgCusCode.CodeTypes.GSTCode && countryCode == Constants.CountryCodes.Jordan)
			{
				return true;
			}

			if (taxCodeInformation.Code == OrgCusCode.CodeTypes.VATCode && countryCode == Constants.CountryCodes.SaudiArabia)
			{
				return true;
			}

			return base.ShouldFilterDirection(taxCodeInformation, countryCode, useExportCountry);
		}

		#endregion

		ZBool EnableSCMTR => FreightDataRegistry.Instance.SCMTREnableDate.Value <= ZDateTime.Now;

		#region CarrierMessagingRequirements

		protected override ZBool IsHarmonizedCodeMandatory() => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.HarmonisedCode)?.RSR_IsShippingInstruction ?? false;

		protected override ZBool IsElectronicBillOfLadingProviderMandatory(CarrierMessageData wrapper) =>
			(GetMessagingRequirement(ShippingLineMessagingRequirement.Types.BillOfLadingProvider)?.RSR_IsShippingInstruction ?? false) &&
			wrapper.ReleaseType.Code == ShippingInstructionReleaseTypes.Codes.BOLOriginal &&
			AtLeastOneAvailableEBLProvider(GetCarrier());

		protected override ZBool GetIsRequiredSendAttachment() => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage)?.RSR_IsShippingInstruction ?? false;

		protected override ZBool CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag() => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice)?.RSR_IsShippingInstruction ?? false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		protected override string IelMessageRequirementValidationContactErrorMessage => "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send Shipping Instruction.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.";

		#endregion
	}
}
