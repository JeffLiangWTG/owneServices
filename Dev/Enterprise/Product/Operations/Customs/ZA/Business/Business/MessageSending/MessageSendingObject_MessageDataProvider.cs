using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Inst = Enterprise.Customs.ZA.Business.MessageDataProviderInstruction;

namespace Enterprise.Customs.ZA.Business
{
	public partial class MessageSendingObject : ICUSDECMessageDataProvider
	{
		#region Cached Value for MessageDataProviderInstruction

		protected JobDeclaration Declaration => Header.Declaration;
		CusEntryLine RandomEntryLine => Header.RandomEntryLine;
		CusEntryInstruction EntryInstruction => Header.EntryInstruction;

		internal MessageDataProviderKeyFactor MessageKeyFactor
		{
			get
			{
				if (messageKeyFactor == null)
				{
					messageKeyFactor = new MessageDataProviderKeyFactor()
					{
						ShipmentType = Declaration.JE_MessageType,
						TransportMode = Declaration.JE_TransportMode,
						ProcedureCategory = RandomEntryLine?.ProcedureCategory ?? ZString.Empty,
						CPC = Header.CustomsProcedureCode,
						PPC = RandomEntryLine?.PreviousProcedureCode ?? ZString.Empty,
						FirstNonSpecificTariffTypeConcession = RandomEntryLine?.CusProcedure?.Concessions?.FirstNonSpecificTariffType(Factory) ?? ZString.Empty,
						MessageType = MessageType,
						RemovalTransportMode = Declaration.JE_RemovalTransportCode,
						CountryOfDestination = Declaration.FinalDestination?.Country,
						CountryOfOrigin = Declaration.Origin?.Country,
						RelationshipIndicator = RelationshipIndicator,
						DeclarationType = ((ICUSDECMessageDataProvider)this).DeclarationType,
						VDN = Header.RandomHeader?.JZ_VDN ?? ZString.Empty,
						Remover = Header?.RemoverLocalCustomsCarrierCode ?? ZString.Empty,
						SubContractor = Header?.SubContractorRemoverCarrierCode ?? ZString.Empty
					};
				}
				return messageKeyFactor;
			}
		}
		MessageDataProviderKeyFactor messageKeyFactor;

		#endregion

		#region ICUSDECMessageDataProvider
		ZString ICUSDECMessageDataProvider.LocalReferenceNumber => LocalReferenceNumber;

		ZString ICUSDECMessageDataProvider.ShipmentType => Declaration.IsImport ? MessageTypeList.Codes.Import : MessageTypeList.Codes.Export;

		ZString ICUSDECMessageDataProvider.MessageType => MessageType.TranslateToEDIFACTMessageFunctionCode();

		ZString ICUSDECMessageDataProvider.DeclarationType => TwoStepDeclarationHelper.IsTwoStepClearingValid ? DeclarationType : (ZString)ZA.Business.DeclarationTypeList.Codes.RegularCompleteDeclarationDefault;

		ZInt ICUSDECMessageDataProvider.PartClearanceQuantity => Inst.ShouldOutputPartClearanceQuantity(MessageKeyFactor) ? Header.CH_TotalEntries : ZInt.Zero;

		ZString ICUSDECMessageDataProvider.CustomsProcedureCategory => EntryInstruction?.CusProcedure?.ZZ6_Category ?? ZString.Empty;

		ZString ICUSDECMessageDataProvider.TransportDocumentIssuedAt => Inst.ShouldOutputTransportDocumentIssuedAt(MessageKeyFactor) ? Declaration.JE_RL_NKMasterBillIssuedAt : ZString.Empty;

		ZString ICUSDECMessageDataProvider.LocationOfGoods => Inst.ShouldOutputLocationOfGoods(messageKeyFactor) ? Declaration.JE_LocationOfGoods : ZString.Empty;

		ZString ICUSDECMessageDataProvider.FromWarehouse
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputFromWarehouse(MessageKeyFactor))
				{
					result = EntryInstruction?.Warehouse?.GetRegNoWithOrganisationAddress(OrgCusCode.CodeTypes.WarehouseControlledPremisesID) ?? ZString.Empty;
				}
				return result;
			}
		}

		ZString ICUSDECMessageDataProvider.ToWarehouse
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputToWarehouse(MessageKeyFactor))
				{
					result = EntryInstruction?.Warehouse2?.GetRegNoWithOrganisationAddress(OrgCusCode.CodeTypes.WarehouseControlledPremisesID) ?? ZString.Empty;
				}
				return result;
			}
		}

		// TODO: ToBeImplemented : this if for 122, Import Only, source not specified in CPC1100 #Victor 20160422
		ZString ICUSDECMessageDataProvider.Consignee => Inst.ShouldOutputConsignee(MessageKeyFactor) ? ZString.Empty : ZString.Empty;

		ZString ICUSDECMessageDataProvider.PortOfExit => EntryInstruction?.CEI_PortOfExit ?? ZString.Empty;
		ZString ICUSDECMessageDataProvider.CountryOfExport => Inst.ShouldOutputCountryOfExport(MessageKeyFactor) ? Declaration.Origin?.Country?.Code ?? ZString.Empty : ZString.Empty;

		ZString ICUSDECMessageDataProvider.CountryOfDestination => Declaration.FinalDestination?.Country?.Code ?? ZString.Empty;

		ZString ICUSDECMessageDataProvider.CustomsOfficeCode => Header.CustomsOffice;

		ZDateTime ICUSDECMessageDataProvider.DateOfAssessment => Inst.ShouldOutputDateOfAssessment(MessageKeyFactor) && EntryInstruction != null ? CusEntryInstruction.GetEffectiveAssessmentDate(EntryInstruction, this.Factory) : ZDateTime.Empty;

		ZDateTime ICUSDECMessageDataProvider.DateOfArrival => Inst.ShouldOutputDateOfArrival(MessageKeyFactor) ? Declaration.JE_DateOfArrival : ZDateTime.Empty;

		ZDateTime ICUSDECMessageDataProvider.DateOfDepartureOrDateOfFlight
		{
			get
			{
				var result = ZDateTime.Empty;
				if (Inst.ShouldOutputDateOfDepartureOrDateOfFlight(MessageKeyFactor))
				{
					if (Inst.ShouldOutputDateOfFlight(MessageKeyFactor))
					{
						result = Declaration.JE_DateOfArrival;
					}
					else if (Inst.ShouldOutputDateOfDeparture(MessageKeyFactor))
					{
						result = Declaration.JE_ExportDate;
					}
				}
				return result;
			}
		}

		ZDateTime ICUSDECMessageDataProvider.ShippedOnBoardDate => Declaration.JE_MasterBillIssuedDate;

		ZString ICUSDECMessageDataProvider.RelatedPartyIndicator
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputRelatedIndicatorAndValuationCode(MessageKeyFactor))
				{
					result = RelationshipIndicator;
					result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.SouthAfrica, "REL", result, CusEntryInstruction.GetEffectiveAssessmentDate(EntryInstruction, Factory));
				}
				return result;
			}
		}

		ZString ICUSDECMessageDataProvider.RelatedPartyIndicatorForSADDocumentPackPurposes => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.SouthAfrica, "REL", RelationshipIndicator, CusEntryInstruction.GetEffectiveAssessmentDate(EntryInstruction, Factory));

		ZString RelationshipIndicator => Header.RandomHeader?.JZ_RelatedIndicator ?? ZString.Empty;

		ZString ICUSDECMessageDataProvider.ValuationCode => Inst.ShouldOutputRelatedIndicatorAndValuationCode(MessageKeyFactor) ? Header.RandomHeader?.JZ_ValuationCode ?? ZString.Empty : ZString.Empty;

		ZString ICUSDECMessageDataProvider.ValuationCodeForSADDocumentPackPurposes => Header.RandomHeader?.JZ_ValuationCode ?? ZString.Empty;

		ZString ICUSDECMessageDataProvider.PaymentMethod => Inst.ShouldOutputFreeForPaymentMethod(MessageKeyFactor) ? new ZString(PaymentMethodCodeList.Codes.Free) : Header.CH_PaymentMethod;

		ZString ICUSDECMessageDataProvider.RefundAcknowledgementIndicator => Inst.ShouldOutputRefundAcknowledgementIndicator(MessageKeyFactor) && AmountDueDifference < ZDecimal.Zero ? new ZString("A") : ZString.Empty;

		ZDecimal ICUSDECMessageDataProvider.GrossWeightInKG
		{
			get
			{
				if (Header.GrossWeight.Amount != ZDecimal.Zero && Header.GrossWeight.Unit != ZString.Empty)
				{
					return Header.GrossWeight.ConvertTo(Core.Constants.Weight.Kilograms).Round(2);
				}
				else
				{
					return ZDecimal.Zero;
				}
			}
		}

		IEnumerable<IContainerInformation> ICUSDECMessageDataProvider.Containers
		{
			get
			{
				var result = Array.Empty<IContainerInformation>();
				if (Inst.ShouldOutputContainers(MessageKeyFactor))
				{
					if (Declaration.HasMultiCustomsEntryInstructions)
					{
						result = EntryInstruction?.InvoiceLines?.SelectMany(invoiceLine => invoiceLine?.ContainersPivot)?.Select(pivot => (pivot as CusContainerInvoiceLinePivot)?.Container as CusContainer)?.Distinct()?.ToArray();
					}
					else
					{
						result = Declaration.CusContainers.OfType<IContainerInformation>().ToArray();
					}
				}
				return result;
			}
		}

		ZInt ICUSDECMessageDataProvider.TotalLineCount => Inst.ShouldOutputLineLevelInformation(MessageKeyFactor) ? new ZInt(Header.MergedLines.Count) : ZInt.Zero;

		ZString ICUSDECMessageDataProvider.CreditTerms
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputCreditTerms(MessageKeyFactor))
				{
					var entryInstruction = EntryInstruction;
					if (!entryInstruction?.CEI_CreditTerms.IsEmpty ?? false)
					{
						result = entryInstruction.CEI_CreditTerms.PadLeft(3, '0');
					}
				}
				return result;
			}
		}

		ZString ICUSDECMessageDataProvider.VATIndicator => Declaration.JE_VATClaimBackIndicator;

		ZString ICUSDECMessageDataProvider.TransactionBankCode => Inst.ShouldOutputTransactionBankCode(MessageKeyFactor) ? EntryInstruction?.CEI_BankCode ?? ZString.Empty : ZString.Empty;

		ZString ICUSDECMessageDataProvider.ChangeAcknowledgementIndicator => Inst.ShouldOutputChangeAcknowledgementIndicator(MessageKeyFactor) ? ChangeAcknowledgementIndicator : ZString.Empty;

		#region SG1

		ZString ICUSDECMessageDataProvider.HouseBill
		{
			get
			{
				var result = Header.HAWBOverride;
				var carrier = Header.CargoCarrierOverride;
				if (!carrier.IsEmpty)
				{
					if (!result.IsEmpty)
					{
						carrier = carrier.PadRight(8);
					}
					result = carrier + result;
				}
				return result;
			}
		}

		ZString ICUSDECMessageDataProvider.OriginalMRN => Inst.ShouldOutputOriginalMRN(MessageKeyFactor) ? MovementReferenceNumber : ZString.Empty;

		ZString ICUSDECMessageDataProvider.MRNToBeReplaced => Inst.ShouldOutputMRNToBeReplaced(MessageKeyFactor) ? EntryInstruction?.CEI_MRNToBeReplaced ?? ZString.Empty : ZString.Empty;

		ZString ICUSDECMessageDataProvider.TransportDocumentNumber => Inst.ShouldOutputTransportDocumentNumber(MessageKeyFactor) ? Declaration.TransportDocumentNumber : ZString.Empty;

		ZString ICUSDECMessageDataProvider.FinancialAccountNumber
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputFinancialAccountNumberFromOriginalMessage(MessageKeyFactor))
				{
					var message = LastAcceptedSendMessage;
					if (message != null)
					{
						result = CUSDECMessageHelper.New(message).FinancialAccountNumber;
					}
				}
				if (result.IsEmpty)
				{
					result = Header.GetFinancialAccountNumber();
				}
				return result;
			}
		}

		ZBool ICUSDECMessageDataProvider.ShouldOutputFinancialAccountNumber
		{
			get
			{
				var header = Header;
				var isExciseEntry = header.RandomEntryLine?.RandomLine?.IsExcise ?? ZBool.False;

				return !isExciseEntry || header.CH_PaymentMethod != PaymentMethodCodeList.Codes.Free;
			}
		}

		ZString ICUSDECMessageDataProvider.UniqueConsignmentReference => Header?.UniqueConsignmentReference ?? ZString.Empty;

		ZString ICUSDECMessageDataProvider.MessageNumber => EDIMessage.MessageNumberPlaceHolder;

		ZString ICUSDECMessageDataProvider.CaseNumber => Inst.ShouldOutputCaseNumber(MessageKeyFactor) ? CaseNumber : ZString.Empty;

		ZDateTime ICUSDECMessageDataProvider.HouseBillIssuedDate => Header.HawbDateOverride;

		ZDateTime ICUSDECMessageDataProvider.TransportDocumentDate => Inst.ShouldOutputTransportDocumentDate(MessageKeyFactor) ? Declaration.JE_MasterBillIssuedDate : ZDateTime.Empty;

		#region SG2

		ZString ICUSDECMessageDataProvider.TotalNoOfPacks => Inst.ShouldOutputTotalNoOfPacks(MessageKeyFactor) ? new ZString(Header.CH_Packages.ToString()) : ZString.Empty;

		#region SG3

		IEnumerable<ZString> ICUSDECMessageDataProvider.MarksAndNumbers => Inst.ShouldOutputMarksAndNumbers(MessageKeyFactor) ? Declaration.JE_MarksAndNumbers.Split(new char[] { '\r', '\n' }) : Array.Empty<ZString>();

		#endregion

		#endregion

		#endregion

		#region SG4

		ZString ICUSDECMessageDataProvider.VoyageFlightNo => Inst.ShouldOutputVoyageFlightNo(MessageKeyFactor) ? Declaration.JE_VoyageFlightNo : ZString.Empty;

		ZString ICUSDECMessageDataProvider.TransportMode => Inst.ShouldOutputTransportMode(MessageKeyFactor) ? TransportModeTranslator.TranslateToWCOCode(Declaration.JE_TransportMode) : ZString.Empty;

		ZString ICUSDECMessageDataProvider.RemovalTransportMode
		{
			get
			{
				if (Inst.ShouldOutputUnknownRemovalTransportMode(MessageKeyFactor))
				{
					return UniversalReferenceConstants.TransportMode.Unknown;
				}
				return Inst.ShouldOutputRemovalTransportMode(MessageKeyFactor) ? TransportModeTranslator.TranslateToWCOCode(Declaration.JE_RemovalTransportCode) : ZString.Empty;
			}
		}

		ZString ICUSDECMessageDataProvider.TransportName
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputTransportName(MessageKeyFactor))
				{
					var declaration = this.Declaration;
					if (declaration != null)
					{
						if (Inst.ShouldOutputVessel(MessageKeyFactor))
						{
							result = ZString.Format("{0,-4}{1,-9}{2}", declaration.JE_Carrier, declaration.JE_RadioCallSign, declaration.JE_VesselName).SubstringSafe(0, 35);
						}
						else if (Inst.ShouldOutputRoadVehicle(MessageKeyFactor))
						{
							result = ZString.Format("{0,-10}{1,-10}{2,-10}", declaration.JE_VoyageFlightNo, declaration.JE_Trailer1, declaration.JE_Trailer2);
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region SG5

		IEnumerable<IInvoiceInformation> ICUSDECMessageDataProvider.InvoiceInformations
		{
			get
			{
				IEnumerable<IInvoiceInformation> result = null;
				if (Inst.ShouldOutputInvoiceInformations(MessageKeyFactor))
				{
					if (MessageKeyFactor.MessageType == MessageSubTypeCodes.Codes.Cancellation)
					{
						var message = LastAcceptedSendMessage;
						if (message != null)
						{
							result = CUSDECMessageHelper.New(message).InvoiceInformations;
						}
					}
					if (result == null)
					{
						result = Header.InvoiceHeaders.ToArray();
					}
				}

				return result ?? Array.Empty<IInvoiceInformation>();
			}
		}

		CUSDECEDIMessage LastAcceptedSendMessage
		{
			get { return lastAcceptedSendMessage ?? (lastAcceptedSendMessage = GetLastAcceptedSendMessage(Header)); }
		}
		CUSDECEDIMessage lastAcceptedSendMessage;

		CUSDECEDIMessage GetLastAcceptedSendMessage(CusEntryHeader entryHeader)
		{
			var lastAcceptedMessage = entryHeader.Messages.GetMatchingMessages(SARSEDIMessage.MessageTypes.CUSRES, EDIMessage.Direction.Receive, (ZAMessage msg) => !msg.EntryStatus.HasEntryStatusGotAttribute(Factory, entryHeader.EntryInstructionAssessmentDate, RefCusCodeListAttributeTypes.Codes.CustomsRejected))?.OrderByDescending(x => x.EM_MessageDateTime).FirstOrDefault();
			return lastAcceptedMessage == null ? null : entryHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageNum == lastAcceptedMessage.ParentMessageNumber) as CUSDECEDIMessage;
		}

		#endregion

		#region SG6

		IAddressInformation ICUSDECMessageDataProvider.Importer
		{
			get
			{
				IAddressInformation result = null;
				if (Inst.ShouldOutputImporter(MessageKeyFactor))
				{
					var importer = Declaration.Importer;
					result = OrgHeaderAddressInformationWrapper.Wrap(importer, OrgCusCode.CodeTypes.CustomsClientCode);
				}
				return result;
			}
		}

		IAddressInformation ICUSDECMessageDataProvider.ImporterForExportJob
		{
			get
			{
				IAddressInformation result = null;
				if (Inst.ShouldOutputImporterForExportJob(MessageKeyFactor))
				{
					var importer = Declaration.Importer;
					result = OrgHeaderAddressInformationWrapper.Wrap(importer, OrgCusCode.CodeTypes.CustomsClientCode);
				}
				return result;
			}
		}

		ZString ICUSDECMessageDataProvider.AgentCode => Header.AgentCode;

		ZString ICUSDECMessageDataProvider.RemoverTransporterCode => Inst.ShouldOutputRemoverTransporterCode(MessageKeyFactor) ? Header.RemoverCarrierCodeForEDI : ZString.Empty;

		IAddressInformation ICUSDECMessageDataProvider.Supplier
		{
			get
			{
				if (Inst.ShouldOutputSupplier(MessageKeyFactor))
				{
					return OrgHeaderAddressInformationWrapper.Wrap(Header?.Declaration?.Supplier,
						Inst.ShouldOutputSupplierCode(MessageKeyFactor) ? OrgCusCode.CodeTypes.SupplierCode : null);
				}
				return null;
			}
		}

		IAddressInformation ICUSDECMessageDataProvider.Exporter => Inst.ShouldOutputExporter(MessageKeyFactor) ? OrgHeaderAddressInformationWrapper.Wrap(Declaration.Supplier, OrgCusCode.CodeTypes.SupplierCode) : null;

		ZString ICUSDECMessageDataProvider.AgentDualProfileCode => Header.AgentDualProfileCode;

		ZString ICUSDECMessageDataProvider.OwnerCode
		{
			get
			{
				var result = ZString.Empty;
				if (Inst.ShouldOutputOwnerCode(MessageKeyFactor))
				{
					var entryInstruction = EntryInstruction;
					var importer = entryInstruction != null && entryInstruction.HasBothOutOfAndIntoRegimeProcedure ? entryInstruction.Owner : Declaration.Importer;
					result = OrgHeaderAddressInformationWrapper.Wrap(importer, OrgCusCode.CodeTypes.CustomsClientCode)?.OrganizationCode ?? ZString.Empty;
				}

				return result;
			}
		}

		IAddressInformation ICUSDECMessageDataProvider.UnregisteredTrader
		{
			get
			{
				IAddressInformation result = null;
				if (Inst.ShouldOutputImporterUnregisteredTrader(MessageKeyFactor) && ValidationConstants.Declaration.UnregisteredTraderCustomsCode.Equals(Declaration.Importer?.LocalCustomsClientCode ?? ZString.Empty))
				{
					result = OrgHeaderAddressInformationWrapper.WrapDeclarant(Declaration.Importer);
				}
				else if (Inst.ShouldOutputSupplierUnregisteredTrader(MessageKeyFactor) && ValidationConstants.Declaration.UnregisteredTraderCustomsCode.Equals(Declaration.Supplier?.LocalCustomsSupplierCode ?? ZString.Empty))
				{
					result = OrgHeaderAddressInformationWrapper.WrapDeclarant(Declaration.Supplier);
				}
				return result;
			}
		}

		ZString ICUSDECMessageDataProvider.VesselAgent => TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(Declaration)
			? Declaration.JE_VesselAgent
			: ZString.Empty;

		ZString ICUSDECMessageDataProvider.MasterCargoCarrier => TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(Declaration)
			? Declaration.JE_CarrierCode
			: ZString.Empty;

		#endregion

		#region SG10

		ZBool ICUSDECMessageDataProvider.ShouldOutputInvoiceDetails => Inst.ShouldOutputInvoiceDetails(MessageKeyFactor);

		ZDateTime ICUSDECMessageDataProvider.ExchangeRateDateTime => Declaration.DateOfValuation;

		ZDateTime ICUSDECMessageDataProvider.DateForDuty => EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;

		ZString ICUSDECMessageDataProvider.PaymentTerms =>
			(ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today)
			? Header.RandomHeader?.JZ_PaymentTerms
			: EntryInstruction?.CEI_CreditTerms) ?? ZString.Empty;

		IEnumerable<IInvoiceHeaderInformation> ICUSDECMessageDataProvider.InvoiceHeaderInformations => Inst.ShouldOutputInvoiceDetails(MessageKeyFactor)
			? Header.InvoiceHeaders
			: null;

		#endregion

		#region SG30

		IEnumerable<ILineLevelInformation> ICUSDECMessageDataProvider.LineLevelDetails
		{
			get
			{
				List<ILineLevelInformation> details = null;
				if (Inst.ShouldOutputLineLevelInformation(MessageKeyFactor))
				{
					details = new List<ILineLevelInformation>();
					foreach (CusEntryLine line in Header.MergedLines)
					{
						line.MessageKeyFactor.DeclarationType = MessageKeyFactor.DeclarationType;
						details.Add(line);
					}
				}

				return details;
			}
		}

		#endregion

		#region SG49

		ZDecimal ICUSDECMessageDataProvider.TotalCIFCAmount => Inst.ShouldOutputTotalCIFCAmount(MessageKeyFactor) ? Header.CIFInLocalCurrencyRounded : ZDecimal.Zero;

		ZDecimal ICUSDECMessageDataProvider.TotalTransactionValue => Inst.ShouldOutputTotalTransactionValueAndCurrency(MessageKeyFactor) ? EntryInstruction?.CEI_TransactionValue ?? ZDecimal.Zero : ZDecimal.Zero;

		ZString ICUSDECMessageDataProvider.TotalTransactionValueCurrency => Inst.ShouldOutputTotalTransactionValueAndCurrency(MessageKeyFactor) ? EntryInstruction?.CEI_RX_NKTransactionValueCurrency ?? ZString.Empty : ZString.Empty;

		ZBool ICUSDECMessageDataProvider.ShouldOutputDutiesDueWhenZero => Inst.ShouldOutputTotalDutiesDueWhenZero(MessageKeyFactor);

		ZDecimal ICUSDECMessageDataProvider.TotalDutiesDue => Inst.ShouldOutputDutiesAndFees(MessageKeyFactor) ? new ZDecimal(Math.Abs(CustomsDutyNoS1P2BDifference + S1P2BDutyDifference + ProvisionalPaymentAmountDifference + PenaltyAmountDifference + DiamondLevyAmount)) : ZDecimal.Zero;

		ZBool ICUSDECMessageDataProvider.ShouldOutputVATDueWhenZero => Inst.ShouldOutputTotalVATDueWhenZero(MessageKeyFactor);

		ZDecimal ICUSDECMessageDataProvider.TotalVATDue => Inst.ShouldOutputDutiesAndFees(MessageKeyFactor) ? new ZDecimal(Math.Abs(ValueAddedTaxDifference)) : ZDecimal.Zero;

		ZBool ICUSDECMessageDataProvider.IsIntoWarehouseWarehousing => Header.IsIntoWarehouseWarehousing;

		ZDecimal ICUSDECMessageDataProvider.OverpaidExcise => ZDecimal.Zero;

		ZDecimal ICUSDECMessageDataProvider.UnderpaidExcise => ZDecimal.Zero;

		ZDecimal ICUSDECMessageDataProvider.TotalCustomsValue => Header.CustomsValue;

		ZBool ICUSDECMessageDataProvider.ShouldOutputTotalTransactionValueAndCurrency => Inst.ShouldOutputTotalTransactionValueAndCurrency(MessageKeyFactor);

		#endregion

		#endregion

		TransportModeTranslator TransportModeTranslator => transportModeTranslator ?? (transportModeTranslator = new TransportModeTranslator());
		TransportModeTranslator transportModeTranslator;
	}
}
