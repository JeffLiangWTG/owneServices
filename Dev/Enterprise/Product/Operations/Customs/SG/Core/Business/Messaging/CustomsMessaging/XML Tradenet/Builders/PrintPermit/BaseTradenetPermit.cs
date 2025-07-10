using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	class BaseTradeNetPermit : IPrintPermitTN41
	{
		public BaseTradeNetPermit(BusinessObjectFactory factory, ITradeNetOutPermitSection section)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.section = Argument.NotNull(section, nameof(section));

			declaration = section.Declaration;
			permit = section.Permit;

			header = declaration?.Header;
			party = declaration?.Party;
			transport = declaration?.Transport;
			summary = declaration?.Summary;
			cargo = (declaration as ITradeNetInSectionWithCargo)?.Cargo;
			invoices = (declaration as ITradeNetInSectionWithInvoice)?.Invoice;
		}

		readonly BusinessObjectFactory factory;
		readonly ITradeNetOutPermitSection section;

		readonly ITradeNetInSection declaration;
		readonly Header header;
		readonly Permit permit;
		readonly Cargo cargo;
		readonly Transport transport;
		readonly Party party;
		readonly Summary summary;
		readonly Invoice[] invoices;

		#region IPrintPermit

		public ZString TradeNetVersion => SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne;

		public ZString PermitNumber => permit?.PermitNumber;

		public ZString UniqueRef
		{
			get
			{
				var uniqueReferenceNumber = header?.UniqueReferenceNumber;

				return uniqueReferenceNumber != null
				? string.Concat
				(
					(uniqueReferenceNumber.ID?.Trim() ?? string.Empty),
					" ",
					(uniqueReferenceNumber.Date?.Trim() ?? string.Empty),
					" ",
					(uniqueReferenceNumber.SequenceNumeric?.Trim() ?? string.Empty).PadLeft(4, '0')
				)
				: string.Empty;
			}
		}

		public ZString MessageType => GetDescriptionInUpperCase(factory.GetCachedValue<CommonAccessReferenceCodeList>(), header?.CommonAccessReference);

		public ZString DeclarationType => GetDescriptionInUpperCase(factory.GetCachedValue<DeclarationTypeCodeList>(), header?.DeclarationType);

		public ZString Importer => string.Empty;

		public ZString Exporter => string.Empty;

		public ZString HandlingAgent => string.Empty;

		public ZString InwardCarrierAgent => string.Empty;

		public ZString OutwardCarrierAgent => string.Empty;

		public ZString NameOfCompany
		{
			get
			{
				var result = GetPartyInfo(party?.DeclaringAgentParty);

				if (result.IsEmpty)
				{
					result = ImporterNameLine1.IsEmpty
						? FormattableString.Invariant($"{ExporterNameLine1} {ExporterNameLine2}")
						: FormattableString.Invariant($"{ImporterNameLine1} {ImporterNameLine2}");
				}

				return result;
			}
		}

		public ZString EntityIdentOfCompany => ZString.Empty;

		public ZString DeclarantName => party?.DeclarantParty?.PersonInformation?.Name;

		public ZString DeclarantCode
		{
			get
			{
				ZString code = party?.DeclarantParty?.PersonInformation?.CodeValue ?? string.Empty;
				return code.Right(5).PadLeft(code.Length, 'X');
			}
		}

		public ZString TelNb => party?.DeclarantParty?.Telephone;

		public ZString ManufacturerName
		{
			get
			{
				var result = string.Empty;

				var manufacturer = invoices?.FirstOrDefault(c => c.SupplierManufacturerParty != null)?.SupplierManufacturerParty;
				if (manufacturer != null)
				{
					result = string.Concat(manufacturer.Name.Take(2));
				}

				return result;
			}
		}

		public ZString PortOfLoading => GetMappingPort(transport?.InwardTransport?.LoadingPort);

		public ZString PortOfDischarge => GetMappingPort(transport?.OutwardTransport?.DischargePort);

		public ZString NextPortOfCall => GetMappingPort(transport?.OutwardTransport?.AdditionalVesselInformation?.LoadingNextPort);

		public ZString FinalPortOfCall => GetMappingPort(transport?.OutwardTransport?.AdditionalVesselInformation?.LoadingFinalPort);

		public ZString CountryOfFinalDest
		{
			get
			{
				var result = string.Empty;

				var finalDestinationCountry = transport?.OutwardTransport?.FinalDestinationCountry;

				if (!string.IsNullOrWhiteSpace(finalDestinationCountry))
				{
					var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, finalDestinationCountry);
					result = country?.RN_DescMultilingual.GetUnresolvedString().ToUpper();
				}

				return result;
			}
		}

		public ZString PlaceOfReleaseName => cargo?.ReleaseLocation?.LocationName;

		public ZString PlaceOfReleaseCode => cargo?.ReleaseLocation?.LocationCode;

		public ZString PlaceOfReceiptName => cargo?.ReceiptLocation?.LocationName;

		public ZString PlaceOfReceiptCode => cargo?.ReceiptLocation?.LocationCode;

		/// <summary>
		/// Not Used In XML Tradenet Message
		/// </summary>
		public ZString PlaceOfReceipt => string.Empty;

		/// <summary>
		/// Not Used In XML Tradenet Message
		/// </summary>
		public ZString PlaceOfRelease => string.Empty;

		public ZDate ValidityPeriodFrom => GetDate(permit?.PermitValidityPeriod?.StartDate);

		public ZDate ValidityPeriodTo => GetDate(permit?.PermitValidityPeriod?.EndDate);

		public ZString TotalGrossWt
		{
			get
			{
				var result = string.Empty;

				var totalGrossWeight = summary?.TotalGrossWeight;
				if (totalGrossWeight != null)
				{
					result = string.Join(@"/", new[] { totalGrossWeight.Value.ToString().PadLeft(15, ' '), totalGrossWeight.unitCode ?? string.Empty });
				}

				return result;
			}
		}

		public ZString TotalOuterPack
		{
			get
			{
				var result = string.Empty;

				var totalOuterPack = summary?.TotalOuterPack;
				if (totalOuterPack != null)
				{
					result = string.Join(@"/", new[] { totalOuterPack.Value.ToString().PadLeft(8, ' '), totalOuterPack.unitCode ?? string.Empty });
				}

				return result;
			}
		}

		public ZDecimal TotalCustomsDUTPayable => summary?.TotalTariff?.TotalCustomsDutyAmount ?? ZDecimal.Zero;

		public ZDecimal TotalOtherTaxPayable => summary?.TotalTariff?.TotalOtherTaxAmount ?? ZDecimal.Zero;

		public ZDecimal TotalExciseDUTPayable => summary?.TotalTariff?.TotalExciseDutyAmount ?? ZDecimal.Zero;

		public ZDecimal TotalGstAmount => summary?.TotalTariff?.TotalGoodsAndServicesTaxAmount ?? ZDecimal.Zero;

		public ZDecimal TotalAmountPayable => summary?.TotalTariff?.TotalAmountPayable ?? ZDecimal.Zero;

		public ZString CargoPackingType => GetDescriptionInUpperCase(factory.GetCachedValue<CargoPackingCodeList>(), cargo?.CargoPackingType);

		public ZString InVesName => transport?.InwardTransport?.TransportMeans?.TransportMode?.TransportIdentifier;

		public ZString InVoyageFlightNumber => transport?.InwardTransport?.TransportMeans?.TransportMode?.ConveyanceReferenceNumber;

		public ZString InOBLMawbNb
		{
			get
			{
				var result = transport?.InwardTransport?.TransportMeans?.MAWBOUCROBLNumber;
				var declarationType = header?.DeclarationType ?? string.Empty;

				if (string.IsNullOrWhiteSpace(result) && (declarationType != DeclarationTypeCodeList.Codes.TTI && declarationType != DeclarationTypeCodeList.Codes.TTF))
				{
					result = declaration?.Item?.Select(c => c.InMAWBOUCROBLNumber)?.FirstOrDefault(c => string.IsNullOrWhiteSpace(c));
				}

				return result;
			}
		}

		public ZDate ArrivalDate => GetDate(transport?.InwardTransport?.ArrivalDate);

		public ZString OutVesName => transport?.OutwardTransport?.TransportMeans?.TransportMode?.TransportIdentifier;

		/// <summary>
		/// Not Used In XML Tradenet Message
		/// </summary>
		public ZString OutVesLocation => ZString.Empty;

		public ZString OutVoyageFlightNumber => transport?.OutwardTransport?.TransportMeans?.TransportMode?.ConveyanceReferenceNumber;

		public ZString TowingVesselName => transport?.OutwardTransport?.AdditionalVesselInformation?.TowingVessel?.VesselName;

		public ZString OutOBLMawbNb
		{
			get
			{
				var result = transport?.OutwardTransport?.TransportMeans?.MAWBOUCROBLNumber;
				var declarationType = header?.DeclarationType ?? string.Empty;

				if (string.IsNullOrWhiteSpace(result) && (declarationType != DeclarationTypeCodeList.Codes.TTI && declarationType != DeclarationTypeCodeList.Codes.TTF))
				{
					result = declaration?.Item?.Select(c => c.OutMAWBOUCROBLNumber)?.FirstOrDefault(c => string.IsNullOrWhiteSpace(c));
				}

				return result;
			}
		}

		public ZDate DepartureDate => GetDate(transport?.OutwardTransport?.DepartureDate);

		public ZString LicenceNo => string.Concat((declaration as ITradeNetInSectionWithLicence)?.Licence?.Where(c => !string.IsNullOrWhiteSpace(c?.ReferenceID)).Select(c => c.ReferenceID) ?? Array.Empty<string>());

		public ZString CertificateNo => permit?.CertificateNumber;

		public ZString CustomsProcedureCodes
		{
			get
			{
				var result = new ZStringBuilder();

				var customsProcedureCodeInformation = header?.CustomsProcedureCodeInformation;

				if (customsProcedureCodeInformation != null)
				{
					foreach (var information in customsProcedureCodeInformation)
					{
						ZString cpc = information.CustomsProcedureCode;

						var procedureCode = cpc.SubstringSafe(0, 3);
						var concession = cpc.SubstringSafe(3, 4);

						var procedure = new RefCusProcedure.Loader(factory).LoadFromProcedureAndPreviousProcedureAndConcession(procedureCode, ZString.Empty, concession, ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today);
						if (procedure != null)
						{
							result.AppendIfNotEmpty(procedure.ZZ6_Description.Split('(').FirstOrDefault().Trim());
						}
					}
				}

				return result.ToString();
			}
		}

		public ZBool HideMawbLine => ConsignmentDetails.All(c => c.InwardMawbObl.IsEmpty && c.OutwardMawbObl.IsEmpty);

		public ZBool HideHawbLine => ConsignmentDetails.All(c => c.InwardHawbHbl.IsEmpty && c.OutwardHawbHbl.IsEmpty);

		public ZBool HideCifFobValue => ConsignmentDetails.All(c => c.CifFobLspValue.IsEmpty);

		public ZBool HideLspValue => ConsignmentDetails.All(c => c.LspAmount.IsEmpty);

		public ZBool HideGstValue => ConsignmentDetails.All(c => c.GstAmount.IsEmpty);

		public ZBool HideDutQtyWtVolValue => ConsignmentDetails.All(c => c.DutQuantity.IsEmpty);

		public ZBool HideUnitPriceValue => ConsignmentDetails.All(c => c.UnitPrice.IsEmpty);

		public ZBool HideExciseValue => ConsignmentDetails.All(c => c.ExciseDutyPayable.IsEmpty);

		public ZBool HideDutyValue => ConsignmentDetails.All(c => c.CustomsDutyPayable.IsEmpty);

		public ZBool HideOtherTaxValue => ConsignmentDetails.All(c => c.OtherTaxPayable.IsEmpty);

		public ZString[] TradersRemark => header?.Remarks != null ? header.Remarks.Select(r => (ZString)r).ToArray() : Array.Empty<ZString>();

		public ZDate AmendDate => AmendFields.Any() ? GetDate(permit?.PermitApprovalDatetime) : ZDate.Empty;

		public ZString[] AmendFields
		{
			get
			{
				if (amendFields == null)
				{
					var result = new List<ZString>();

					var informations = (section as ITradeNetOutUpdatePermitSection)?.Update?.Amendment?.SCUpdateInformation;

					if (informations != null && informations.Any())
					{
						var codes = informations.SelectMany(c => c.UpdateSummaryCode).WhereNotNull();
						var updateSummaryCode = new Business.UpdateSummaryCode();

						foreach (var code in codes)
						{
							var line = string.Concat(code.Value ?? string.Empty, code.type ?? string.Empty);

							if (!string.IsNullOrWhiteSpace(line))
							{
								var field = updateSummaryCode.GetTN41FieldDescriptionFromSummaryCode(line);
								result.Add(field);
							}
						}
					}

					amendFields = result.ToArray();
				}

				return amendFields;
			}
		}
		ZString[] amendFields;

		public IPrintPermitConsignment[] ConsignmentDetails
		{
			get
			{
				if (consignmentDetails == null)
				{
					var bondedIntoOrReleasedFromBond = BondedIntoOrReleasedFromBond;
					consignmentDetails = declaration?.Item?.Select(c => new BaseTradeNetPermitItem(c, invoices, bondedIntoOrReleasedFromBond)).ToArray() ?? Array.Empty<IPrintPermitConsignment>();
				}

				return consignmentDetails;
			}
		}
		IPrintPermitConsignment[] consignmentDetails;

		public IPrintPermitContainers[] ContainerIdentifiers => containerIdentifiers ?? (containerIdentifiers = cargo?.TransportEquipment?.Select(c => new BaseTradeNetPermitContainer(c)).ToArray() ?? Array.Empty<IPrintPermitContainers>());
		IPrintPermitContainers[] containerIdentifiers;

		protected bool BondedIntoOrReleasedFromBond => BondedIntoOrReleasedFromBondCore(PlaceOfReleaseCode) || BondedIntoOrReleasedFromBondCore(PlaceOfStorageCode);

		bool BondedIntoOrReleasedFromBondCore(ZString placeCode)
		{
			return !placeCode.IsEmpty && (SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(factory, placeCode)?.IsLicencedPremise() ?? false);
		}

		protected ZString PlaceOfStorageCode => cargo?.StorageLocation?.LocationCode;

		#endregion

		#region TN41

		public ZString ImporterNameLine1 => GetPartyName(party?.ImporterParty?.PartyName, 0);

		public ZString ImporterNameLine2 => GetPartyName(party?.ImporterParty?.PartyName, 1);

		public ZString ImporterUEN => party?.ImporterParty?.PartyIdentification?.ID;

		public ZString ExporterNameLine1 => GetPartyName(party?.ExporterParty?.PartyDetail.PartyName, 0);

		public ZString ExporterNameLine2 => GetPartyName(party?.ExporterParty?.PartyDetail.PartyName, 1);

		public ZString ExporterUEN => party?.ExporterParty?.PartyDetail?.PartyIdentification?.ID;

		public ZString HandlingAgentNameLine1 => GetPartyName(party?.HandlingAgentParty?.PartyName, 0, true);

		public ZString HandlingAgentNameLine2 => GetPartyName(party?.HandlingAgentParty?.PartyName, 1, true);

		public ZString HandlingAgentNameLine3 => GetPartyName(party?.HandlingAgentParty?.PartyName, 2, true);

		public ZString HandlingAgentUEN => party?.HandlingAgentParty?.PartyIdentification?.ID;

		public ZString InwardCarrierAgentNameLine1 => GetPartyName(party?.InwardCarrierAgentParty?.PartyName, 0, true);

		public ZString InwardCarrierAgentNameLine2 => GetPartyName(party?.InwardCarrierAgentParty?.PartyName, 1, true);

		public ZString InwardCarrierAgentNameLine3 => GetPartyName(party?.InwardCarrierAgentParty?.PartyName, 2, true);

		public ZString OutwardCarrierAgentNameLine1 => GetPartyName(party?.OutwardCarrierAgentParty?.PartyName, 0, true);

		public ZString OutwardCarrierAgentNameLine2 => GetPartyName(party?.OutwardCarrierAgentParty?.PartyName, 1, true);

		public ZString OutwardCarrierAgentNameLine3 => GetPartyName(party?.OutwardCarrierAgentParty?.PartyName, 2, true);

		public ITN41PermitConditions[] CAPermitConditions => caPermitConditions ?? (caPermitConditions = BaseTradeNetPermitCondition.GetPermitConditions(permit?.CAApprovalCondition).ToArray());
		ITN41PermitConditions[] caPermitConditions;

		public ITN41PermitConditions[] CustomsPermitConditions => customsPermitConditions ?? (customsPermitConditions = BaseTradeNetPermitCondition.GetPermitConditions(permit?.SCApprovalCondition).ToArray());
		ITN41PermitConditions[] customsPermitConditions;

		#endregion

		#region Implement

		ZString GetPartyName(string[] names, int index, bool needRearrange = false)
		{
			var array = names;

			if (array != null && needRearrange)
			{
				ZString fullText = string.Concat(array);
				array = fullText.SplitIntoArray(35, 3, false);
			}

			return array != null && array.Length > index ? array[index] : string.Empty;
		}

		ZString GetPartyInfo(ITradeNetParty party)
		{
			var result = new ZStringBuilder();

			if (party != null)
			{
				foreach (var name in party.PartyName ?? Array.Empty<string>())
				{
					result.Append(name);
				}
			}

			return result.ToString();
		}

		ZString GetMappingPort(ZString portName)
		{
			var result = portName;

			if (!string.IsNullOrWhiteSpace(result))
			{
				var mappingName = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, result, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
				if (mappingName != null)
				{
					result = mappingName.ZZD_Description.ToUpperInvariant();
				}
			}

			return result;
		}

		ZDate GetDate(ZString date)
		{
			date = date.KeepNumericCharacters();

			return string.IsNullOrWhiteSpace(date) || date.Length < 8
				? ZDate.Empty
				: new ZDate(Convert.ToInt32(date.SubstringSafe(0, 4)), Convert.ToInt32(date.SubstringSafe(4, 2)), Convert.ToInt32(date.SubstringSafe(6, 2)));
		}

		string GetDescriptionInUpperCase(CodeDescriptionPairList list, string code)
		{
			var result = string.IsNullOrWhiteSpace(code) ? string.Empty : list.GetDescriptionFromCode(code);
			return string.IsNullOrWhiteSpace(result) ? (code?.ToUpperInvariant() ?? string.Empty) : result.ToUpperInvariant();
		}

		#endregion
	}
}
