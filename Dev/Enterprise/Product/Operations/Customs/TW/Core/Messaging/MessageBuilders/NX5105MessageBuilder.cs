using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX5105;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX5105MessageBuilder : BaseTWMessageBuilder<INX5105Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX5105Declaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.AcceptanceDateTime = input.AcceptanceDateTime.ToISO8601ShortDateString();
				PopulateValueIfNodeValueIsNotEmpty(input.Authentication, () => newItem.Authentication = new DeclarationAuthentication { Value = input.Authentication });
				newItem.FunctionCode = new DeclarationFunctionCode { Value = functionCode };
				newItem.Id = new DeclarationId { Value = input.ID };
				newItem.InvoiceAmount = new DeclarationInvoiceAmount { Value = input.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(input.InvoiceAmount)) };
				newItem.TotalGrossMassMeasure = new DeclarationTotalGrossMassMeasure { Value = input.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(input.TotalGrossMassMeasure)) };
				newItem.TotalPackageQuantity = new DeclarationTotalPackageQuantity { Value = input.TotalPackageQuantity };
				PopulateValueIfNodeValueIsNotEmpty(input.AssociatedGovernmentProcedureCode, () => newItem.TwAssociatedGovernmentProcedureCode = new DeclarationTwAssociatedGovernmentProcedureCode { Value = input.AssociatedGovernmentProcedureCode });
				PopulateValueIfNodeValueIsNotEmpty(input.CombinedNote, () => newItem.TwCombinedNote = new DeclarationTwCombinedNote { Value = input.CombinedNote });
				newItem.TypeCode = new DeclarationTypeCode { Value = input.TypeCode };
				PopulateAdditionalInformation(newItem, input.AdditionalInformation);
				PopulateAgent(newItem, input.Agent);
				PopulateBorderTransportMeans(newItem, input.BorderTransportMeans);
				PopulateCurrencyExchange(newItem, input.CurrencyExchange);
				PopulateDutyTaxFee(newItem, input.DutyTaxFee);
				PopulateGoodsShipment(newItem, input.GoodsShipment);
				PopulateDeclarationGovernmentProcedure(newItem, input.GovernmentProcedureDescriptions);
				PopulateImporter(newItem, input.Importer);
				PopulateDeclarationPackaging(newItem, input.Packaging);
				PopulateRepresentativePerson(newItem, input.RepresentativePersonName);
				PopulateApplication(newItem, input.Applications);
			}
			return newItem;
		}
		public void PopulateAdditionalInformation(Declaration bo, IAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationAdditionalInformation() { TwCopyQuantity = new DeclarationAdditionalInformationTwCopyQuantity { Value = obj.CopyQuantity } };
			}
		}

		public void PopulateAgent(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationAgentId { Value = obj.ID };
				newItem.RoleCode = new DeclarationAgentRoleCode { Value = obj.RoleCode };
				newItem.TwSubBoxId = new DeclarationAgentTwSubBoxId { Value = obj.SubBoxID };
				PopulateLPCOAuthorizedParty(newItem, obj.LPCOAuthorizedParty);
			}
		}

		public void PopulateLPCOAuthorizedParty(DeclarationAgent bo, ILPCOAuthorizedParty obj)
		{
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.LpcoAuthorizedParty = new DeclarationAgentLpcoAuthorizedParty() { Id = new DeclarationAgentLpcoAuthorizedPartyId { Value = obj.ID.Left(20) } };
			}
		}

		public void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				newItem.ArrivalDateTime = obj.ArrivalDateTime.ToISO8601ShortDateString();
				newItem.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
				PopulateItinerary(newItem, obj.ItineraryRoutingCountryCodes);
			}
		}

		public void PopulateItinerary(DeclarationBorderTransportMeans bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationBorderTransportMeansItinerary>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationBorderTransportMeansItinerary() { RoutingCountryCode = new DeclarationBorderTransportMeansItineraryRoutingCountryCode { Value = item } };
					collection.Add(newItem);
				}
				bo.Itinerary = collection;
			}
		}

		public void PopulateCurrencyExchange(Declaration bo, ICurrencyExchange obj)
		{
			if (obj != null)
			{
				bo.CurrencyExchange = new DeclarationCurrencyExchange();
				var newItem = bo.CurrencyExchange;
				newItem.CurrencyTypeCode = new DeclarationCurrencyExchangeCurrencyTypeCode { Value = obj.CurrencyTypeCode };
				newItem.RateNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.RateNumeric));
			}
		}

		public void PopulateDutyTaxFee(Declaration bo, IDutyTaxFee obj)
		{
			if (obj != null)
			{
				bo.DutyTaxFee = new DeclarationDutyTaxFee();
				var newItem = bo.DutyTaxFee;
				PopulateValueIfNodeValueIsNotEmpty(obj.DutyExemptionWaiverNote, () => newItem.TwDutyExemptionWaiverNote = new DeclarationDutyTaxFeeTwDutyExemptionWaiverNote { Value = obj.DutyExemptionWaiverNote });
				PopulateValueIfNodeValueIsNotEmpty(obj.DutyMemoPrinted, () => newItem.TwDutyMemoPrinted = new DeclarationDutyTaxFeeTwDutyMemoPrinted { Value = obj.DutyMemoPrinted });
				newItem.TwDutyMethodCode = new DeclarationDutyTaxFeeTwDutyMethodCode { Value = obj.DutyMethodCode };
				newItem.TwTotalDutyTaxFeeAmount = new DeclarationDutyTaxFeeTwTotalDutyTaxFeeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TotalDutyTaxFeeAmount)) };
				PopulatePayment(newItem, obj.PaymentObligationGuaranteeReferenceID);
			}
		}

		public void PopulatePayment(DeclarationDutyTaxFee bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Payment = new DeclarationDutyTaxFeePayment() { ObligationGuarantee = new DeclarationDutyTaxFeePaymentObligationGuarantee() { ReferenceId = new DeclarationDutyTaxFeePaymentObligationGuaranteeReferenceId { Value = obj } } };
			}
		}

		public void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				newItem.ExitDateTime = obj.ExitDateTime.ToISO8601ShortDateString();
				newItem.TwItemChargeAmount = new DeclarationGoodsShipmentTwItemChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ItemChargeAmount)) };
				newItem.TwTotalCifAmount = new DeclarationGoodsShipmentTwTotalCifAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TotalCIFAmount)) };
				PopulateAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateConsignee(newItem, obj.Consignee);
				PopulateConsignment(newItem, obj.Consignment);
				PopulateConsignor(newItem, obj.Consignor);
				PopulateCustomsValuation(newItem, obj.CustomsValuation);
				PopulateDeliveryDestination(newItem, obj.DeliveryDestinationName);
				PopulateGoodsShipmentDutyTaxFee(newItem, obj.DutyTaxFees);
				PopulateGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
				PopulateNotifyParty(newItem, obj.NotifyParty);
				PopulateSeller(newItem, obj.Seller);
				PopulateTradeTerms(newItem, obj.TradeTermsConditionCode);
				PopulateUCR(newItem, obj.UCR);
			}
		}

		public void PopulateAdditionalDocument(DeclarationGoodsShipment bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentAdditionalDocument();
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationGoodsShipmentAdditionalDocumentId { Value = item.ID });
					PopulateValueIfNodeValueIsNotEmpty(item.Content, () => newItem.TwContent = new DeclarationGoodsShipmentAdditionalDocumentTwContent { Value = item.Content });
					newItem.TwImageFileFormat = new DeclarationGoodsShipmentAdditionalDocumentTwImageFileFormat { Value = item.ImageFileFormat };
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileName, () => newItem.TwImageFileName = new DeclarationGoodsShipmentAdditionalDocumentTwImageFileName { Value = item.ImageFileName });
					newItem.TwSizeMeasure = new DeclarationGoodsShipmentAdditionalDocumentTwSizeMeasure { Value = item.SizeMeasure };
					newItem.TypeCode = new DeclarationGoodsShipmentAdditionalDocumentTypeCode { Value = item.TypeCode };
					PopulateResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		public void PopulateResponsibleGovernmentAgency(DeclarationGoodsShipmentAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationGoodsShipmentAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationGoodsShipmentAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		public void PopulateConsignee(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Consignee = new DeclarationGoodsShipmentConsignee();
				var newItem = bo.Consignee;
				newItem.Id = new DeclarationGoodsShipmentConsigneeId { Value = obj.ID };
				newItem.Name = new DeclarationGoodsShipmentConsigneeName { Value = obj.Name.Left(80) };
				newItem.TwChineseName = new DeclarationGoodsShipmentConsigneeTwChineseName { Value = obj.ChineseName.Left(70) };
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentConsigneeTwTypeCode { Value = obj.TypeCode });
				PopulateAddress(newItem, obj.Address);
			}
		}

		public void PopulateAddress(DeclarationGoodsShipmentConsignee bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentConsigneeAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationGoodsShipmentConsigneeAddressLine { Value = obj.Line.Left(120) };
				newItem.TwChineseLine = new DeclarationGoodsShipmentConsigneeAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		public void PopulateConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.ManifestSerialNumber, () => newItem.TwManifestSerialNumber = new DeclarationGoodsShipmentConsignmentTwManifestSerialNumber { Value = obj.ManifestSerialNumber });

				PopulateConsignmentAdditionalInformation(newItem, obj.AdditionalInformations);
				PopulateArrivalTransportMeans(newItem, obj.ArrivalTransportMeansTypeCode);
				PopulateConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateCarrier(newItem, obj.Carrier);
				PopulateConsignmentItem(newItem, obj.ConsignmentItem);
				PopulateGoodsLocation(newItem, obj.GoodsLocation);
				PopulateLoadingLocation(newItem, obj.LoadingLocation);
				PopulateTransportContractDocument(newItem, obj.TransportContractDocuments);
				PopulateTransportEquipment(newItem, obj.TransportEquipments);
				PopulateBondedGoods(newItem, obj.BondedGoods);
			}
		}

		public void PopulateConsignmentAdditionalInformation(DeclarationGoodsShipmentConsignment bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentAdditionalInformation();
					newItem.StatementCode = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementCode { Value = item.StatementCode };
					newItem.StatementDescription = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementDescription { Value = item.StatementDescription };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		public void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ArrivalTransportMeans = new DeclarationGoodsShipmentConsignmentArrivalTransportMeans() { TypeCode = new DeclarationGoodsShipmentConsignmentArrivalTransportMeansTypeCode { Value = obj } };
			}
		}

		public void PopulateConsignmentBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				newItem.Id = new DeclarationGoodsShipmentConsignmentBorderTransportMeansId { Value = obj.ID };
				newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID };
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => newItem.TwRegistration = new DeclarationGoodsShipmentConsignmentBorderTransportMeansTwRegistration { Value = obj.Registration });
			}
		}

		public void PopulateCarrier(DeclarationGoodsShipmentConsignment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Carrier = new DeclarationGoodsShipmentConsignmentCarrier();
				var newItem = bo.Carrier;
				newItem.Id = new DeclarationGoodsShipmentConsignmentCarrierId { Value = obj.ID };
				newItem.TwTypeCode = new DeclarationGoodsShipmentConsignmentCarrierTwTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateConsignmentItem(DeclarationGoodsShipmentConsignment bo, IConsignmentItem obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.Split, () => bo.ConsignmentItem = new DeclarationGoodsShipmentConsignmentConsignmentItem() { TwSplit = new DeclarationGoodsShipmentConsignmentConsignmentItemTwSplit { Value = obj.Split } });
			}
		}

		public void PopulateGoodsLocation(DeclarationGoodsShipmentConsignment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = obj } };
			}
		}

		public void PopulateLoadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			var id = obj?.ID ?? ZString.Empty;
			if (!id.IsEmpty)
			{
				bo.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation() { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = id } };
			}
		}

		public void PopulateTransportContractDocument(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportContractDocument();
					newItem.Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId { Value = item.ID };
					newItem.TypeCode = new DeclarationGoodsShipmentConsignmentTransportContractDocumentTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TransportContractDocument = collection;
			}
		}

		public void PopulateTransportEquipment(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportEquipment> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipment>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportEquipment();
					newItem.CharacteristicCode = new DeclarationGoodsShipmentConsignmentTransportEquipmentCharacteristicCode { Value = item.CharacteristicCode };
					newItem.Id = new DeclarationGoodsShipmentConsignmentTransportEquipmentId { Value = item.ID };
					newItem.TwUsedCapacityCode = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwUsedCapacityCode { Value = item.UsedCapacityCode };
					PopulateSeal(newItem, item.Seals);
					collection.Add(newItem);
				}
				bo.TransportEquipment = collection;
			}
		}

		public void PopulateSeal(DeclarationGoodsShipmentConsignmentTransportEquipment bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentTwSeal>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwSeal() { TwSealId = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwSealTwSealId { Value = item } };
					collection.Add(newItem);
				}
				bo.TwSeal = collection;
			}
		}

		public void PopulateBondedGoods(DeclarationGoodsShipmentConsignment bo, IBondedGoods obj)
		{
			if (obj != null)
			{
				bo.TwBondedGoods = new DeclarationGoodsShipmentConsignmentTwBondedGoods();
				var newItem = bo.TwBondedGoods;
				PopulateValueIfNodeValueIsNotEmpty(obj.AddDutyReasonCode, () => newItem.TwAddDutyReasonCode = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwAddDutyReasonCode { Value = obj.AddDutyReasonCode });
				PopulateBondedGoodsInvoice(newItem, obj.BondedGoodsInvoices);
				PopulateBondedGoodsMonthlyReport(newItem, obj.BondedGoodsMonthlyReport);
				PopulateInBondedParty(newItem, obj.InBondedParty);
				PopulateOutBondedParty(newItem, obj.OutBondedParty);
				PopulatePreBondedParty(newItem, obj.PreBondedParties);
			}
		}

		public void PopulateBondedGoodsInvoice(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IEnumerable<IBondedGoodsInvoice> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsInvoice>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsInvoice();
					newItem.TwId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsInvoiceTwId { Value = item.ID };
					newItem.TwValueAmount = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsInvoiceTwValueAmount { Value = item.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.ValueAmount)) };
					collection.Add(newItem);
				}
				bo.TwBondedGoodsInvoice = collection;
			}
		}

		public void PopulateBondedGoodsMonthlyReport(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IBondedGoodsMonthlyReport obj)
		{
			if (obj != null)
			{
				bo.TwBondedGoodsMonthlyReport = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsMonthlyReport();
				var newItem = bo.TwBondedGoodsMonthlyReport;
				PopulateValueIfNodeValueIsNotEmpty(obj.MonthNumeric, () =>
				{
					newItem.TwMonthNumericValueSpecified = true;
					newItem.TwMonthNumeric = obj.MonthNumeric;
				});
				PopulateValueIfNodeValueIsNotEmpty(obj.TraderReferenceID, () => newItem.TwTraderReferenceId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsMonthlyReportTwTraderReferenceId { Value = obj.TraderReferenceID });
			}
		}

		public void PopulateInBondedParty(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IBondedParty obj)
		{
			if (obj != null)
			{
				bo.TwInBondedParty = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwInBondedParty();
				var newItem = bo.TwInBondedParty;
				newItem.TwBondedId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwInBondedPartyTwBondedId { Value = obj.BondedID };
				newItem.TwId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwInBondedPartyTwId { Value = obj.ID };
				newItem.TwTypeCode = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwInBondedPartyTwTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateOutBondedParty(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IBondedParty obj)
		{
			if (obj != null)
			{
				bo.TwOutBondedParty = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwOutBondedParty();
				var newItem = bo.TwOutBondedParty;
				newItem.TwBondedId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwOutBondedPartyTwBondedId { Value = obj.BondedID };
				newItem.TwId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwOutBondedPartyTwId { Value = obj.ID };
				newItem.TwTypeCode = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwOutBondedPartyTwTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulatePreBondedParty(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IEnumerable<IBondedParty> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTwBondedGoodsTwPreBondedParty>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwPreBondedParty();
					newItem.TwCustomsControlId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwPreBondedPartyTwCustomsControlId { Value = item.CustomsControlID };
					newItem.TwId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwPreBondedPartyTwId { Value = item.ID };
					newItem.TwTypeCode = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwPreBondedPartyTwTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TwPreBondedParty = collection;
			}
		}

		public void PopulateConsignor(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Consignor = new DeclarationGoodsShipmentConsignor();
				var newItem = bo.Consignor;
				newItem.Id = new DeclarationGoodsShipmentConsignorId { Value = obj.ID };
				newItem.Name = new DeclarationGoodsShipmentConsignorName { Value = obj.Name.Left(80) };
				newItem.TwChineseName = new DeclarationGoodsShipmentConsignorTwChineseName { Value = obj.ChineseName.Left(70) };
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentConsignorTwTypeCode { Value = obj.TypeCode });
				PopulateConsignorAddress(newItem, obj.Address);
			}
		}

		public void PopulateConsignorAddress(DeclarationGoodsShipmentConsignor bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentConsignorAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationGoodsShipmentConsignorAddressLine { Value = obj.Line.Left(120) };
				newItem.TwChineseLine = new DeclarationGoodsShipmentConsignorAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		public void PopulateCustomsValuation(DeclarationGoodsShipment bo, ICustomsValuation obj)
		{
			if (obj != null)
			{
				bo.CustomsValuation = new DeclarationGoodsShipmentCustomsValuation();
				var newItem = bo.CustomsValuation;
				newItem.ExitToEntryChargeAmount = new DeclarationGoodsShipmentCustomsValuationExitToEntryChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ExitToEntryChargeAmount)) };
				newItem.FreightChargeAmount = new DeclarationGoodsShipmentCustomsValuationFreightChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.FreightChargeAmount)) };
				newItem.OtherChargeDeductionAmount = new DeclarationGoodsShipmentCustomsValuationOtherChargeDeductionAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherChargeDeductionAmount)) };
				newItem.PartyRelationshipCode = new DeclarationGoodsShipmentCustomsValuationPartyRelationshipCode { Value = obj.PartyRelationshipCode };
				newItem.TwOtherChargeAmount = new DeclarationGoodsShipmentCustomsValuationTwOtherChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherChargeAmount)) };
				newItem.TwOtherDeductionAmount = new DeclarationGoodsShipmentCustomsValuationTwOtherDeductionAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherDeductionAmount)) };
			}
		}

		public void PopulateDeliveryDestination(DeclarationGoodsShipment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.DeliveryDestination = new DeclarationGoodsShipmentDeliveryDestination() { Name = new DeclarationGoodsShipmentDeliveryDestinationName { Value = obj } };
			}
		}

		public void PopulateGoodsShipmentDutyTaxFee(DeclarationGoodsShipment bo, IEnumerable<IGoodsShipmentDutyTaxFee> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentDutyTaxFee>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentDutyTaxFee();
					newItem.AdValoremTaxBaseAmount = new DeclarationGoodsShipmentDutyTaxFeeAdValoremTaxBaseAmount { Value = item.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.AdValoremTaxBaseAmount)) };
					newItem.TypeCode = new DeclarationGoodsShipmentDutyTaxFeeTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.DutyTaxFee = collection;
			}
		}

		public void PopulateGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateGovernmentAgencyGoodsItemAdditionalDocument(newItem, item.AdditionalDocuments);
					PopulateGovernmentAgencyGoodsItemAdditionalInformation(newItem, item.AdditionalInformations);
					PopulateCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateManufacturer(newItem, item.Manufacturer);
					PopulateOrigin(newItem, item.Origin);
					PopulatePackaging(newItem, item.Packaging);
					PopulateGovernmentAgencyGoodsItemPreviousDocument(newItem, item.PreviousDocument);
					PopulateApprovalDocument(newItem, item.ApprovalDocument);
					PopulateCommoditySpecification(newItem, item.CommoditySpecification);
					PopulateGoodsLicensingStatisticalMeasure(newItem, item.GoodsLicensingStatisticalMeasure);
					PopulateGoodsStatisticalMeasure(newItem, item.GoodsStatisticalMeasure);
					PopulateMedicalInstrument(newItem, item.MedicalInstrument);
					PopulatePreBondedDocument(newItem, item.PreBondedDocument);
					PopulateShippingIdentification(newItem, item.ShippingIdentifications);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		public void PopulateGovernmentAgencyGoodsItemAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument();
					newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentId { Value = item.ID };
					PopulateValueIfNodeValueIsNotEmpty(item.SequenceNumeric, () =>
					{
						newItem.TwSequenceNumericValueSpecified = true;
						newItem.TwSequenceNumeric = item.SequenceNumeric;
					});
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		public void PopulateGovernmentAgencyGoodsItemAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
					newItem.StatementCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementCode { Value = item.StatementCode };
					newItem.StatementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementDescription { Value = item.StatementDescription };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		public void PopulateCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID.Left(80) });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description.Left(512) };
				PopulateValueIfNodeValueIsNotEmpty(obj.GoodsGroupNameCode, () => newItem.GoodsGroupNameCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsGroupNameCode { Value = obj.GoodsGroupNameCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name.Left(50) });
				PopulateValueIfNodeValueIsNotEmpty(obj.BarCode, () => newItem.TwBarCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwBarCode { Value = obj.BarCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseDescription, () => newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription { Value = obj.ChineseDescription.Left(512) });
				PopulateValueIfNodeValueIsNotEmpty(obj.CITESImportPermitID, () => newItem.TwCitesImportPermitId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCitesImportPermitId { Value = obj.CITESImportPermitID });
				PopulateValueIfNodeValueIsNotEmpty(obj.EnglishDescription, () => newItem.TwEnglishDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwEnglishDescription { Value = obj.EnglishDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.FTATariffCode, () => newItem.TwFtaTariffCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFtaTariffCode { Value = obj.FTATariffCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.SHTCImportPermitID, () => newItem.TwShtcImportPermitId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwShtcImportPermitId { Value = obj.SHTCImportPermitID });
				PopulateValueIfNodeValueIsNotEmpty(obj.TariffCodeExtensionCode, () => newItem.TwTariffCodeExtensionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwTariffCodeExtensionCode { Value = obj.TariffCodeExtensionCode });
				PopulateCommodityAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateClassification(newItem, obj.Classifications);
				PopulateCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
				PopulateConstituent(newItem, obj.Constituent);
				PopulateCommodityDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateGovernmentProcedure(newItem, obj.GovernmentProcedure);
				PopulateHandling(newItem, obj.HandlingInstructionsCodes);
				PopulateInvoiceLine(newItem, obj.InvoiceLine);
				PopulatePreviousDocument(newItem, obj.PreviousDocument);
				PopulateCommodityNumber(newItem, obj.CommodityNumbers);
				PopulateDutyOtherTaxFee(newItem, obj.DutyOtherTaxFees);
				PopulateDutyTaxFeeAmount(newItem, obj.DutyTaxFeeAmount);
				PopulateDutyTaxFeeQuantity(newItem, obj.DutyTaxFeeQuantity);
				PopulateFood(newItem, obj.Food);
				PopulateQuarantine(newItem, obj.Quarantine);
				PopulateVehicle(newItem, obj.Vehicle);
				PopulateWine(newItem, obj.Wine);
			}
		}

		public void PopulateCommodityAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument() { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentId { Value = item.ID } };
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		public void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<IClassification> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification();
					newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationId { Value = item.ID };
					newItem.IdentificationTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationIdentificationTypeCode { Value = item.IdentificationTypeCode };
					collection.Add(newItem);
				}
				bo.Classification = collection;
			}
		}

		public void PopulateCommodityRelatedPackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityRelatedPackaging obj)
		{
			if (obj != null && (!obj.PackingMethodDescription.IsEmpty || !obj.MaterialCode.IsEmpty || !obj.Specification.IsEmpty))
			{
				bo.CommodityRelatedPackaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackaging();
				var newItem = bo.CommodityRelatedPackaging;
				PopulateValueIfNodeValueIsNotEmpty(obj.PackingMethodDescription, () => newItem.PackingMethodDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingPackingMethodDescription { Value = obj.PackingMethodDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.MaterialCode, () => newItem.TwMaterialCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwMaterialCode { Value = obj.MaterialCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.Specification, () => newItem.TwSpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwSpecification { Value = obj.Specification });
			}
		}

		public void PopulateConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			if (obj != null && (!obj.ElementDescription.IsEmpty || !obj.LevelID.IsEmpty || !obj.Thickness.IsEmpty))
			{
				bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
				var newItem = bo.Constituent;
				PopulateValueIfNodeValueIsNotEmpty(obj.ElementDescription, () => newItem.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription.Left(256) });
				PopulateValueIfNodeValueIsNotEmpty(obj.LevelID, () => newItem.TwLevelId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwLevelId { Value = obj.LevelID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Thickness, () => newItem.TwThickness = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwThickness { Value = obj.Thickness });
			}
		}

		public void PopulateCommodityDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityDutyTaxFee obj)
		{
			if (obj != null)
			{
				bo.DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee();
				var newItem = bo.DutyTaxFee;
				newItem.AdValoremTaxBaseAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeAdValoremTaxBaseAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.AdValoremTaxBaseAmount)) };
				PopulateValueIfNodeValueIsNotEmpty(obj.DutyRegimeCode, () => newItem.DutyRegimeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeDutyRegimeCode { Value = obj.DutyRegimeCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.SpecificTaxBaseQuantity, () => newItem.SpecificTaxBaseQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeSpecificTaxBaseQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.SpecificTaxBaseQuantity)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.PercentageNumeric, () =>
				{
					newItem.TwPercentageNumericValueSpecified = true;
					newItem.TwPercentageNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.PercentageNumeric));
				});
			}
		}

		public void PopulateGovernmentProcedure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IGovernmentProcedure obj)
		{
			if (obj != null)
			{
				bo.GovernmentProcedure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure() { CurrentCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureCurrentCode { Value = obj.CurrentCode } };
			}
		}

		public void PopulateHandling(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<ZString> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandling>();
				foreach (var item in obj.Take(9))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandling() { InstructionsCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandlingInstructionsCode { Value = item } };
					collection.Add(newItem);
				}
				bo.Handling = collection;
			}
		}

		public void PopulateInvoiceLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IInvoiceLine obj)
		{
			if (obj != null)
			{
				bo.InvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine();
				var newItem = bo.InvoiceLine;
				newItem.TwChargesTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwChargesTypeCode { Value = obj.ChargesTypeCode };
				newItem.TwCurrencyTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwCurrencyTypeCode { Value = obj.CurrencyTypeCode };
				newItem.TwUnitPriceAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwUnitPriceAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.UnitPriceAmount)) };
			}
		}

		public void PopulatePreviousDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IPreviousDocument obj)
		{
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.PreviousDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument();
				var newItem = bo.PreviousDocument;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocumentId { Value = obj.ID.Left(14) };
			}
		}

		public void PopulateCommodityNumber(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<ICommodityNumber> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumber>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumber();
					newItem.TwId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumberTwId { Value = item.ID };
					newItem.TwIdentifierTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumberTwIdentifierTypeCode { Value = item.IdentifierTypeCode };
					collection.Add(newItem);
				}
				bo.TwCommodityNumber = collection;
			}
		}

		public void PopulateDutyOtherTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<IDutyOtherTaxFee> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFee>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFee();
					newItem.TwMethodCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFeeTwMethodCode { Value = item.MethodCode };
					newItem.TwTaxRateNumeric = item.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.TaxRateNumeric));
					newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFeeTwTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TwDutyOtherTaxFee = collection;
			}
		}

		public void PopulateDutyTaxFeeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IDutyTaxFeeAmount obj)
		{
			if (obj != null)
			{
				bo.TwDutyTaxFeeAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyTaxFeeAmount();
				var newItem = bo.TwDutyTaxFeeAmount;
				newItem.TwTaxRateNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TaxRateNumeric));
			}
		}

		public void PopulateDutyTaxFeeQuantity(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IDutyTaxFeeQuantity obj)
		{
			if (obj != null)
			{
				bo.TwDutyTaxFeeQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyTaxFeeQuantity();
				var newItem = bo.TwDutyTaxFeeQuantity;
				newItem.TwDutyUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyTaxFeeQuantityTwDutyUnitCode { Value = obj.DutyUnitCode };
				newItem.TwTaxRateNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TaxRateNumeric));
			}
		}

		public void PopulateFood(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IFood obj)
		{
			if (obj != null)
			{
				bo.TwFood = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood();
				var newItem = bo.TwFood;
				if (obj.PHValueNumeric != null)
				{
					newItem.TwPhValueNumericValueSpecified = true;
					newItem.TwPhValueNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.PHValueNumeric));
				}
				if (obj.SterilizationValueNumeric != null)
				{
					newItem.TwSterilizationValueNumericValueSpecified = true;
					newItem.TwSterilizationValueNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.SterilizationValueNumeric));
				}
				PopulateFoodConstituent(newItem, obj.Constituents);
			}
		}

		public void PopulateFoodConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood bo, IEnumerable<IFoodConstituent> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFoodConstituent>();
				foreach (var item in obj.Take(99))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFoodConstituent();
					newItem.ElementName = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFoodConstituentElementName { Value = item.ElementName };
					PopulateValueIfNodeValueIsNotEmpty(item.ElementPercentNumeric, () =>
					{
						newItem.ElementPercentNumericValueSpecified = true;
						newItem.ElementPercentNumeric = item.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.ElementPercentNumeric));
					});
					collection.Add(newItem);
				}
				bo.Constituent = collection;
			}
		}

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/tw_Quarantine
		public void PopulateQuarantine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IQuarantine obj)
		{
			if (obj != null && (!obj.ObjectFeature.IsEmpty || !obj.Treatment.IsEmpty || CanAddAnimalNode(obj.Animal)
				|| (obj.Packing?.Any() ?? false) || (obj.AdditionalInformation?.Any() ?? false) || (obj.AdditionalDocument?.Any() ?? false)))
			{
				bo.TwQuarantine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine();
				var newItem = bo.TwQuarantine;
				PopulateValueIfNodeValueIsNotEmpty(obj.ObjectFeature, () => newItem.TwObjectFeature = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwObjectFeature { Value = obj.ObjectFeature });
				PopulateValueIfNodeValueIsNotEmpty(obj.Treatment, () => newItem.TwTreatment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwTreatment { Value = obj.Treatment });
				PopulateQuarantineAdditionalDocument(newItem, obj.AdditionalDocument);
				PopulateQuarantineAdditionalInformation(newItem, obj.AdditionalInformation);
				PopulateQuarantinePacking(newItem, obj.Packing);
				PopulateAnimal(newItem, obj.Animal);
			}
		}

		void PopulateQuarantineAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IEnumerable<IAdditionalDocument> additionalDocuments)
		{
			if (additionalDocuments != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalDocument>();
				foreach (var additionalDocument in additionalDocuments.Take(5))
				{
					collection.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalDocument { TwSlaughterDateTime = additionalDocument.SlaughterDateTime.ToISO8601ShortDateString() });
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateQuarantineAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IEnumerable<IAdditionalInformation> additionalInformations)
		{
			if (additionalInformations != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalInformation>();
				foreach (var additionalInformation in additionalInformations.Take(5))
				{
					collection.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalInformation { TwPackingHouse = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalInformationTwPackingHouse { Value = additionalInformation.PackingHouse } });
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateQuarantinePacking(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IEnumerable<IPackaging> packings)
		{
			if (packings != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantinePackaging>();
				foreach (var packing in packings.Take(5))
				{
					collection.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantinePackaging { TwPackingDateTime = packing.PackingDateTime.ToISO8601ShortDateString() });
				}
				bo.Packaging = collection;
			}
		}

		void PopulateAnimal(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IAnimal obj)
		{
			if (CanAddAnimalNode(obj))
			{
				bo.TwAnimal = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimal();
				var newItem = bo.TwAnimal;
				PopulateValueIfNodeValueIsNotEmpty(obj.AgeMonthNumeric, () =>
				{
					newItem.TwAgeMonthNumericValueSpecified = true;
					newItem.TwAgeMonthNumeric = obj.AgeMonthNumeric;
				});
				PopulateValueIfNodeValueIsNotEmpty(obj.AgeYearNumeric, () =>
				{
					newItem.TwAgeYearNumericValueSpecified = true;
					newItem.TwAgeYearNumeric = obj.AgeYearNumeric;
				});
				PopulateValueIfNodeValueIsNotEmpty(obj.FemaleQuantity, () => newItem.TwFemaleQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwFemaleQuantity { Value = obj.FemaleQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.MaleQuantity, () => newItem.TwMaleQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMaleQuantity { Value = obj.MaleQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.MicrochipID, () => newItem.TwMicrochipId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMicrochipId { Value = obj.MicrochipID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Vaccination, () => newItem.TwVaccination = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwVaccination { Value = obj.Vaccination });
			}
		}

		bool CanAddAnimalNode(IAnimal obj) => obj != null && (!obj.AgeYearNumeric.IsEmpty || !obj.AgeMonthNumeric.IsEmpty || !obj.FemaleQuantity.IsEmpty || !obj.MaleQuantity.IsEmpty || !obj.MicrochipID.IsEmpty || !obj.Vaccination.IsEmpty);
		#endregion

		public void PopulateVehicle(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IVehicle obj)
		{
			if (obj != null)
			{
				bo.TwVehicle = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicle();
				var newItem = bo.TwVehicle;
				newItem.TwCatalyst = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwCatalyst { Value = obj.Catalyst };
				newItem.TwClassificationCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwClassificationCode { Value = obj.ClassificationCode };
				newItem.TwCylinderQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwCylinderQuantity { Value = obj.CylinderQuantity };
				newItem.TwDisplaceQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwDisplaceQuantity { Value = ZDecimal.ParseSafe(obj.Displace, 0) };
				newItem.TwDoorQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwDoorQuantity { Value = obj.DoorQuantity };
				newItem.TwDrivingSide = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwDrivingSide { Value = obj.DrivingSide };
				newItem.TwFuelTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwFuelTypeCode { Value = obj.FuelTypeCode };
				newItem.TwModelYearNumeric = obj.ModelYearNumeric;
				newItem.TwSeatQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwSeatQuantity { Value = obj.SeatQuantity };
				newItem.TwStatusCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwStatusCode { Value = obj.StatusCode };
				newItem.TwTransmissionTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwTransmissionTypeCode { Value = obj.TransmissionTypeCode };
				PopulateVehicleID(newItem, obj.VehicleIDs);
			}
		}

		public void PopulateVehicleID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicle bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwVehicleId>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwVehicleId() { TwVinid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleTwVehicleIdTwVinid { Value = item } };
					collection.Add(newItem);
				}
				bo.TwVehicleId = collection;
			}
		}

		public void PopulateWine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IWine obj)
		{
			if (obj != null)
			{
				bo.TwWine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwWine();
				var newItem = bo.TwWine;
				PopulateValueIfNodeValueIsNotEmpty(obj.AgeNumeric, () =>
				{
					newItem.TwAgeNumericValueSpecified = true;
					newItem.TwAgeNumeric = obj.AgeNumeric;
				});
				newItem.TwAlcoholContentNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.AlcoholContentNumeric));
				PopulateValueIfNodeValueIsValid(obj.BottledDate, () =>
				{
					newItem.TwBottledDateValueSpecified = true;
					newItem.TwBottledDate = obj.BottledDate.ToDateTime();
				});
				PopulateValueIfNodeValueIsNotEmpty(obj.CoverLotNumberAmount, () => newItem.TwCoverLotNumberAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwWineTwCoverLotNumberAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.CoverLotNumberAmount)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.GeographicRegion, () => newItem.TwGeographicRegion = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwWineTwGeographicRegion { Value = obj.GeographicRegion.Left(100) });
				PopulateValueIfNodeValueIsNotEmpty(obj.OriginalNonLotNumberAmount, () => newItem.TwOriginalNonLotNumberAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwWineTwOriginalNonLotNumberAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OriginalNonLotNumberAmount)) });
				PopulateValueIfNodeValueIsValid(obj.ProductBestBeforeDateTime, () => newItem.TwProductBestBeforeDateTime = obj.ProductBestBeforeDateTime.ToISO8601ShortDateString());
				PopulateValueIfNodeValueIsValid(obj.ProductExpiryDateTime, () => newItem.TwProductExpiryDateTime = obj.ProductExpiryDateTime.ToISO8601ShortDateString());
				PopulateValueIfNodeValueIsNotEmpty(obj.RemoveLotNumberAmount, () => newItem.TwRemoveLotNumberAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwWineTwRemoveLotNumberAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.RemoveLotNumberAmount)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.YearNumeric, () =>
				{
					newItem.TwYearNumericValueSpecified = true;
					newItem.TwYearNumeric = obj.YearNumeric;
				});
			}
		}

		public void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsMeasure obj)
		{
			if (obj != null)
			{
				bo.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure();
				var newItem = bo.GoodsMeasure;
				newItem.NetWeightMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureNetWeightMeasure { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.NetWeightMeasure)) };
				newItem.TariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTariffQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TariffQuantity)) };
				newItem.TwUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwUnitCode { Value = obj.UnitCode };
			}
		}

		public void PopulateManufacturer(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () =>
				{
					bo.Manufacturer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer();
					var newItem = bo.Manufacturer;
					PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerId { Value = obj.ID });
					newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerName { Value = obj.Name };
					PopulateManufacturerAddress(newItem, obj.Address);
					PopulateCommunication(newItem, obj.Communications?.FirstOrDefault());
					PopulateContact(newItem, obj.ContactName);
				});
			}
		}

		public void PopulateManufacturerAddress(DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer bo, IAddress obj)
		{
			if (obj != null)
			{
				var id = obj.CountrySubDivisionID;
				var name = obj.CountrySubDivisionName;
				var line = obj.Line;
				if (!id.IsEmpty || !name.IsEmpty || !line.IsEmpty)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddress();
					PopulateValueIfNodeValueIsNotEmpty(id, () => newItem.CountrySubDivisionId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddressCountrySubDivisionId { Value = id });
					PopulateValueIfNodeValueIsNotEmpty(name, () => newItem.CountrySubDivisionName = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddressCountrySubDivisionName { Value = name });
					PopulateValueIfNodeValueIsNotEmpty(line, () => newItem.Line = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddressLine { Value = line });
					bo.Address = newItem;
				}
			}
		}

		public void PopulateCommunication(DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer bo, ICommunication obj)
		{
			if (obj != null)
			{
				var id = obj.ID;
				if (!id.IsEmpty)
				{
					bo.Communication = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerCommunication
					{
						Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerCommunicationId { Value = id },
						TypeId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerCommunicationTypeId { Value = obj.TypeID },
					};
				}
			}
		}

		public void PopulateContact(DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Contact = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerContact() { Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerContactName { Value = obj } };
			}
		}

		public void PopulateOrigin(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IOrigin obj)
		{
			if (obj != null)
			{
				bo.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin() { CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = obj.CountryCode } };
				PopulateOriginAdditionalDocument(bo.Origin, obj.AdditionalDocument);
			}
		}

		public void PopulateOriginAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin bo, IAdditionalDocument obj)
		{
			if (obj != null)
			{
				bo.AdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginAdditionalDocument() { TwSequenceNumericValueSpecified = true };
				var newItem = bo.AdditionalDocument;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginAdditionalDocumentId { Value = obj.ID };
				PopulateValueIfNodeValueIsNotEmpty(obj.SequenceNumeric, () =>
				{
					newItem.TwSequenceNumericValueSpecified = true;
					newItem.TwSequenceNumeric = obj.SequenceNumeric;
				});
			}
		}

		public void PopulatePackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging();
				var newItem = bo.Packaging;
				newItem.QuantityQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingQuantityQuantity { Value = obj.QuantityQuantity };
				newItem.TypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateGovernmentAgencyGoodsItemPreviousDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPreviousDocument obj)
		{
			if (obj != null && (!obj.ID.IsEmpty && !obj.LineNumeric.IsEmpty))
			{
				bo.PreviousDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument();
				var newItem = bo.PreviousDocument;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentId { Value = obj.ID };
				newItem.LineNumeric = obj.LineNumeric;
			}
		}

		public void PopulateApprovalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ILPCODetail obj)
		{
			if (obj != null)
			{
				var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument();
				PopulateValueIfNodeValueIsNotEmpty(obj.LPCOExemptionCode, () => newItem.TwLpcoExemptionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentTwLpcoExemptionCode { Value = obj.LPCOExemptionCode.Left(1) });
				PopulateValueIfNodeValueIsNotEmpty(obj.LPCOID, () => newItem.TwLpcoid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentTwLpcoid { Value = obj.LPCOID.Left(14) });
				PopulateApprovalDocumentLPCOAuthorizedParty(newItem, obj.LPCOAuthorizedParty);
				if (newItem.LpcoAuthorizedParty != null || newItem.TwLpcoid != null || newItem.TwLpcoExemptionCode != null)
				{
					bo.TwApprovalDocument = newItem;
				}
			}
		}

		public void PopulateApprovalDocumentLPCOAuthorizedParty(DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument bo, ILPCOAuthorizedParty obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.TypeCode.IsEmpty))
			{
				bo.LpcoAuthorizedParty = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedParty();
				var newItem = bo.LpcoAuthorizedParty;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedPartyId { Value = obj.ID.Left(14) };
				newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedPartyTwTypeCode { Value = obj.TypeCode.Left(3) };
			}
		}

		public void PopulateCommoditySpecification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommoditySpecification obj)
		{
			if (obj != null)
			{
				bo.TwCommoditySpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecification();
				var newItem = bo.TwCommoditySpecification;
				newItem.TwCharacteristicQualifierCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecificationTwCharacteristicQualifierCode { Value = obj.CharacteristicQualifierCode };
				newItem.TwElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecificationTwElementDescription { Value = obj.ElementDescription };
			}
		}

		public void PopulateGoodsLicensingStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsLicensingStatisticalMeasure obj)
		{
			if (obj != null && (!obj.LicensingQuantity.IsEmpty || !obj.StatisticalUnitCode.IsEmpty || !obj.ResponsibleGovernmentAgency.IsEmpty))
			{
				bo.TwGoodsLicensingStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasure();
				var newItem = bo.TwGoodsLicensingStatisticalMeasure;
				newItem.TwLicensingQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureTwLicensingQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.LicensingQuantity)) };
				newItem.TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode };
				PopulateGoodsLicensingStatisticalMeasureResponsibleGovernmentAgency(newItem, obj.ResponsibleGovernmentAgency);
			}
		}

		public void PopulateGoodsLicensingStatisticalMeasureResponsibleGovernmentAgency(DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasure bo, ZString obj)
		{
			bo.ResponsibleGovernmentAgency = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureResponsibleGovernmentAgency() { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureResponsibleGovernmentAgencyId { Value = obj } };
		}

		public void PopulateGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsStatisticalMeasure obj)
		{
			if (obj != null)
			{
				bo.TwGoodsStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasure();
				var newItem = bo.TwGoodsStatisticalMeasure;
				newItem.TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode };
				newItem.TwTariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwTariffQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TariffQuantity)) };
			}
		}

		public void PopulateMedicalInstrument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ILPCODetail obj)
		{
			if (obj != null)
			{
				var medicalInstrument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument();
				if (!obj.LPCOID.IsEmpty)
				{
					medicalInstrument.TwLpcoid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentTwLpcoid { Value = obj.LPCOID.Left(14) };
				}
				PopulateMedicalInstrumentLPCOAuthorizedParty(medicalInstrument, obj.LPCOAuthorizedParty);
				if (medicalInstrument.TwLpcoid != null || medicalInstrument.LpcoAuthorizedParty != null)
				{
					bo.TwMedicalInstrument = medicalInstrument;
				}
			}
		}

		public void PopulateMedicalInstrumentLPCOAuthorizedParty(DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument bo, ILPCOAuthorizedParty obj)
		{
			if (obj != null)
			{
				var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedParty();
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedPartyId { Value = obj.ID.Left(14) });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedPartyTwTypeCode { Value = obj.TypeCode.Left(3) });
				if (newItem.Id != null || newItem.TwTypeCode != null)
				{
					bo.LpcoAuthorizedParty = newItem;
				}
			}
		}

		public void PopulatePreBondedDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPreviousDocument obj)
		{
			if (obj != null && (!obj.ID.IsEmpty && !obj.LineNumeric.IsEmpty))
			{
				bo.TwPreBondedDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwPreBondedDocument();
				var newItem = bo.TwPreBondedDocument;
				newItem.TwId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwPreBondedDocumentTwId { Value = obj.ID };
				newItem.TwLineNumeric = obj.LineNumeric;
			}
		}

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/tw_ShippingIdentification
		public void PopulateShippingIdentification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IShippingIdentification> obj)
		{
			if (obj?.Any() ?? false)
			{
				var list = obj.Where(x => CanPopulateShippingIdentification(x)).Take(99).Select(y => NewShippingIdentification(y)).ToArray();
				if (list.Any())
				{
					bo.TwShippingIdentification = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification>(list);
				}
			}
		}

		bool CanPopulateShippingIdentification(IShippingIdentification obj) => obj != null && (!obj.LotNumberID.IsEmpty || obj.ProductBestBeforeDateTime.IsValid || !obj.ProductLotNumberAmount.IsEmpty || obj.ProductManufacturedDate.IsValid);

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification NewShippingIdentification(IShippingIdentification obj)
		{
			var result = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification();
			PopulateValueIfNodeValueIsNotEmpty(obj.LotNumberID, () => result.TwLotNumberId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwLotNumberId { Value = obj.LotNumberID.Left(150) });
			PopulateValueIfNodeValueIsValid(obj.ProductBestBeforeDateTime, () => result.TwProductBestBeforeDateTime = obj.ProductBestBeforeDateTime.ToISO8601ShortDateString());
			PopulateValueIfNodeValueIsNotEmpty(obj.ProductLotNumberAmount, () => result.TwProductLotNumberAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwProductLotNumberAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ProductLotNumberAmount)) });
			PopulateValueIfNodeValueIsValid(obj.ProductManufacturedDate, () =>
			{
				result.TwProductManufacturedDateValueSpecified = true;
				result.TwProductManufacturedDate = obj.ProductManufacturedDate.ToDateTime();
			});
			return result;
		}
		#endregion

		public void PopulateNotifyParty(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.NotifyParty = new DeclarationGoodsShipmentNotifyParty();
				var newItem = bo.NotifyParty;
				newItem.Id = new DeclarationGoodsShipmentNotifyPartyId { Value = obj.ID };
				newItem.Name = new DeclarationGoodsShipmentNotifyPartyName { Value = obj.Name.Left(80) };
				newItem.TwChineseName = new DeclarationGoodsShipmentNotifyPartyTwChineseName { Value = obj.ChineseName.Left(70) };
				newItem.TwTypeCode = new DeclarationGoodsShipmentNotifyPartyTwTypeCode { Value = obj.TypeCode };
				PopulateNotifyPartyAddress(newItem, obj.Address);
			}
		}

		public void PopulateNotifyPartyAddress(DeclarationGoodsShipmentNotifyParty bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentNotifyPartyAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationGoodsShipmentNotifyPartyAddressLine { Value = obj.Line.Left(120) };
				newItem.TwChineseLine = new DeclarationGoodsShipmentNotifyPartyAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		public void PopulateSeller(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Seller = new DeclarationGoodsShipmentSeller();
				var newItem = bo.Seller;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentSellerId { Value = obj.ID.Left(14) });
				newItem.Name = new DeclarationGoodsShipmentSellerName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentSellerTwChineseName { Value = obj.ChineseName.Left(70) });
				PopulateValueIfNodeValueIsNotEmpty(obj.CustomsControlID, () => newItem.TwCustomsControlId = new DeclarationGoodsShipmentSellerTwCustomsControlId { Value = obj.CustomsControlID.Left(8) });
				newItem.TwTypeCode = new DeclarationGoodsShipmentSellerTwTypeCode { Value = obj.TypeCode.Left(3) };
				PopulateSellerAddress(newItem, obj.Address);
				PopulateSellerCommunication(newItem, obj.Communications?.FirstOrDefault());
				PopulateSellerContact(newItem, obj.ContactName);
				PopulateSellerLPCOAuthorizedParty(newItem, obj.LPCOAuthorizedParty, obj.Address);
			}
		}

		public void PopulateSellerAddress(DeclarationGoodsShipmentSeller bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentSellerAddress();
				var newItem = bo.Address;
				newItem.CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.CountryCode };
				newItem.Line = new DeclarationGoodsShipmentSellerAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentSellerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		public void PopulateSellerCommunication(DeclarationGoodsShipmentSeller bo, ICommunication obj)
		{
			if (obj != null)
			{
				bo.Communication = new DeclarationGoodsShipmentSellerCommunication();
				var newItem = bo.Communication;
				newItem.Id = new DeclarationGoodsShipmentSellerCommunicationId { Value = obj.ID.Left(20) };
				newItem.TypeId = new DeclarationGoodsShipmentSellerCommunicationTypeId { Value = obj.TypeID };
			}
		}

		public void PopulateSellerContact(DeclarationGoodsShipmentSeller bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Contact = new DeclarationGoodsShipmentSellerContact() { Name = new DeclarationGoodsShipmentSellerContactName { Value = obj.Left(70) } };
			}
		}

		public void PopulateSellerLPCOAuthorizedParty(DeclarationGoodsShipmentSeller bo, ILPCOAuthorizedParty obj, IAddress address)
		{
			if (obj != null && address != null && MessageBuilderHelper.EffectiveAEOCountryCodes.Contains(address.CountryCode))
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () =>
					bo.LpcoAuthorizedParty = new DeclarationGoodsShipmentSellerLpcoAuthorizedParty() { Id = new DeclarationGoodsShipmentSellerLpcoAuthorizedPartyId { Value = obj.ID.Left(20) } });
			}
		}

		public void PopulateTradeTerms(DeclarationGoodsShipment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.TradeTerms = new DeclarationGoodsShipmentTradeTerms() { ConditionCode = new DeclarationGoodsShipmentTradeTermsConditionCode { Value = obj } };
			}
		}

		public void PopulateUCR(DeclarationGoodsShipment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Ucr = new DeclarationGoodsShipmentUcr() { Id = new DeclarationGoodsShipmentUcrId { Value = obj } };
			}
		}

		public void PopulateDeclarationGovernmentProcedure(Declaration bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGovernmentProcedure>();
				foreach (var item in obj)
				{
					if (!item.IsEmpty)
					{
						var newItem = new DeclarationGovernmentProcedure() { Description = new DeclarationGovernmentProcedureDescription { Value = item } };
						collection.Add(newItem);
					}
				}
				if (collection.Any())
				{
					bo.GovernmentProcedure = collection;
				}
			}
		}

		public void PopulateImporter(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Importer = new DeclarationImporter();
				var newItem = bo.Importer;
				newItem.Id = new DeclarationImporterId { Value = obj.ID };
				newItem.Name = new DeclarationImporterName { Value = obj.Name.Left(80) };
				newItem.TwChineseName = new DeclarationImporterTwChineseName { Value = obj.ChineseName.Left(70) };
				PopulateValueIfNodeValueIsNotEmpty(obj.CustomsControlID, () => newItem.TwCustomsControlId = new DeclarationImporterTwCustomsControlId { Value = obj.CustomsControlID });
				PopulateValueIfNodeValueIsNotEmpty(obj.PaymentOnAccountBusinessID, () => newItem.TwPaymentOnAccountBusinessId = new DeclarationImporterTwPaymentOnAccountBusinessId { Value = obj.PaymentOnAccountBusinessID });
				newItem.TwTypeCode = new DeclarationImporterTwTypeCode { Value = obj.TypeCode };
				PopulateImporterAddress(newItem, obj.Address);
				PopulateImporterCommunication(newItem, obj.Communications);
				PopulateImporterLPCOAuthorizedParty(newItem, obj.LPCOAuthorizedParty);
			}
		}

		public void PopulateImporterAddress(DeclarationImporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationImporterAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationImporterAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		public void PopulateImporterCommunication(DeclarationImporter bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationImporterCommunication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationImporterCommunication();
					newItem.Id = new DeclarationImporterCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationImporterCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}

		public void PopulateImporterLPCOAuthorizedParty(DeclarationImporter bo, ILPCOAuthorizedParty obj)
		{
			if (obj != null)
			{
				bo.LpcoAuthorizedParty = new DeclarationImporterLpcoAuthorizedParty() { Id = new DeclarationImporterLpcoAuthorizedPartyId { Value = obj.ID } };
			}
		}

		public void PopulateDeclarationPackaging(Declaration bo, IDeclarationPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationPackaging();
				var newItem = bo.Packaging;
				newItem.MarksNumbers = new DeclarationPackagingMarksNumbers { Value = obj.MarksNumbers.Left(512) };
				PopulateValueIfNodeValueIsNotEmpty(obj.PackagingMaterialDescription, () => newItem.PackagingMaterialDescription = new DeclarationPackagingPackagingMaterialDescription { Value = obj.PackagingMaterialDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.Combination, () => newItem.TwCombination = new DeclarationPackagingTwCombination { Value = obj.Combination });
				newItem.TypeCode = new DeclarationPackagingTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateRepresentativePerson(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.RepresentativePerson = new DeclarationRepresentativePerson() { Name = new DeclarationRepresentativePersonName { Value = obj } };
			}
		}

		public void PopulateApplication(Declaration bo, IEnumerable<IApplication> obj)
		{
			if (obj?.Any() ?? false)
			{
				var collection = new Collection<DeclarationTwApplication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationTwApplication();
					newItem.TwFunctionalReferenceId = new DeclarationTwApplicationTwFunctionalReferenceId { Value = item.FunctionalReferenceID };
					newItem.TwId = new DeclarationTwApplicationTwId { Value = item.ID };
					newItem.TwPurposeCode = new DeclarationTwApplicationTwPurposeCode { Value = item.PurposeCode };
					newItem.TwTypeCode = new DeclarationTwApplicationTwTypeCode { Value = item.TypeCode };
					PopulateApplicationAdditionalDocument(newItem, item.AdditionalDocuments);
					PopulateApplicationAdditionalInformation(newItem, item.AdditionalInformation);
					PopulateApplicationAgent(newItem, item.Agent);
					PopulateBankAccount(newItem, item.BankAccount);
					PopulateContactOffice(newItem, item.ContactOffice);
					PopulateApplicationPayment(newItem, item.Payment);
					PopulateApplicationResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					PopulateAppointment(newItem, item.Appointment);
					PopulateApprovalAuthenticationInformation(newItem, item.ApprovalAuthenticationInformation);
					PopulateAuthorizedInformation(newItem, item.AuthorizedInformation);
					PopulateDeclarer(newItem, item.Declarer);
					PopulateItemGroupReference(newItem, item.ItemGroupReferenceSequenceNumerics);
					PopulateLabel(newItem, item.Labels);
					PopulateLocalManufacturer(newItem, item.LocalManufacturer);
					PopulateApplicationWine(newItem, item.Wine);
					collection.Add(newItem);
				}
				bo.TwApplication = collection;
			}
		}

		public void PopulateApplicationAdditionalDocument(DeclarationTwApplication bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationTwApplicationAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationTwApplicationAdditionalDocument();
					newItem.Id = new DeclarationTwApplicationAdditionalDocumentId { Value = item.ID };
					newItem.TwContent = new DeclarationTwApplicationAdditionalDocumentTwContent { Value = item.Content };
					newItem.TwImageFileFormat = new DeclarationTwApplicationAdditionalDocumentTwImageFileFormat { Value = item.ImageFileFormat };
					newItem.TwImageFileName = new DeclarationTwApplicationAdditionalDocumentTwImageFileName { Value = item.ImageFileName };
					newItem.TypeCode = new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = item.TypeCode };
					PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		public void PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(DeclarationTwApplicationAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		public void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription { Value = obj.StatementDescription };
				newItem.TwDeductionSample = new DeclarationTwApplicationAdditionalInformationTwDeductionSample { Value = obj.DeductionSample };
				newItem.TwElectronicReceipt = new DeclarationTwApplicationAdditionalInformationTwElectronicReceipt { Value = obj.ElectronicReceipt };
				newItem.TwProvedPaper = new DeclarationTwApplicationAdditionalInformationTwProvedPaper { Value = obj.ProvedPaper };
				newItem.TwReturnSample = new DeclarationTwApplicationAdditionalInformationTwReturnSample { Value = obj.ReturnSample };
				PopulateApplicationAdditionalInformationAddress(newItem, obj);
			}
		}

		public void PopulateApplicationAdditionalInformationAddress(DeclarationTwApplicationAdditionalInformation bo, IApplicationAdditionalInformation obj)
		{
			if (bo != null)
			{
				bo.Address = new DeclarationTwApplicationAdditionalInformationAddress { TwChineseLine = new DeclarationTwApplicationAdditionalInformationAddressTwChineseLine { Value = obj.AddressChineseLine.Left(100) } };
			}
		}

		public void PopulateApplicationAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationTwApplicationAgentId { Value = obj.ID };
				newItem.Name = new DeclarationTwApplicationAgentName { Value = obj.Name.Left(70) };
				newItem.TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode };
				PopulateApplicationAgentAddress(newItem, obj.Address);
				PopulateApplicationAgentCommunication(newItem, obj.Communications);
			}
		}

		public void PopulateApplicationAgentAddress(DeclarationTwApplicationAgent bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationAgentAddress() { TwChineseLine = new DeclarationTwApplicationAgentAddressTwChineseLine { Value = obj.ChineseLine.Left(100) } };
			}
		}

		public void PopulateApplicationAgentCommunication(DeclarationTwApplicationAgent bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationTwApplicationAgentCommunication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationTwApplicationAgentCommunication();
					newItem.Id = new DeclarationTwApplicationAgentCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationTwApplicationAgentCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}

		#region NX5105_CM: Declaration/tw_Application/BankAccount
		public void PopulateBankAccount(DeclarationTwApplication bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.BankAccount = new DeclarationTwApplicationBankAccount() { Id = new DeclarationTwApplicationBankAccountId { Value = obj.Left(16) } };
			}
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/ContactOffice
		public void PopulateContactOffice(DeclarationTwApplication bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ContactOffice = new DeclarationTwApplicationContactOffice() { Id = new DeclarationTwApplicationContactOfficeId { Value = obj.Left(17) } };
			}
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/Payment
		public void PopulateApplicationPayment(DeclarationTwApplication bo, IPayment obj)
		{
			if (obj != null)
			{
				bo.Payment = new DeclarationTwApplicationPayment() { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.MethodCode.Left(2) } };
			}
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/ResponsibleGovernmentAgency
		public void PopulateApplicationResponsibleGovernmentAgency(DeclarationTwApplication bo, ZString obj)
		{
			bo.ResponsibleGovernmentAgency = new DeclarationTwApplicationResponsibleGovernmentAgency() { Id = new DeclarationTwApplicationResponsibleGovernmentAgencyId { Value = obj.Left(5) } };
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_Appointment
		public void PopulateAppointment(DeclarationTwApplication bo, IAppointment obj)
		{
			if (obj != null && (obj.ReservationDate.IsValid || !obj.ReservationPeriodCode.IsEmpty))
			{
				bo.TwAppointment = new DeclarationTwApplicationTwAppointment();
				var newItem = bo.TwAppointment;
				PopulateValueIfNodeValueIsValid(obj.ReservationDate, () =>
				{
					newItem.TwReservationDateValueSpecified = true;
					newItem.TwReservationDate = obj.ReservationDate.ToDateTime();
				});
				PopulateValueIfNodeValueIsNotEmpty(obj.ReservationPeriodCode, () => newItem.TwReservationPeriodCode = new DeclarationTwApplicationTwAppointmentTwReservationPeriodCode { Value = obj.ReservationPeriodCode.Left(1) });
			}
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_ApprovalAuthenticationInformation
		public void PopulateApprovalAuthenticationInformation(DeclarationTwApplication bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.TwApprovalAuthenticationInformation = new DeclarationTwApplicationTwApprovalAuthenticationInformation() { TwId = new DeclarationTwApplicationTwApprovalAuthenticationInformationTwId { Value = obj.Left(6) } };
			}
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_AuthorizedInformation
		public void PopulateAuthorizedInformation(DeclarationTwApplication bo, IAuthorizedInformation obj)
		{
			if (obj != null && !obj.AuthorizedTypeCode.IsEmpty)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.AuthorizedTypeCode, () => bo.TwAuthorizedInformation = new DeclarationTwApplicationTwAuthorizedInformation() { TwAuthorizedTypeCode = new DeclarationTwApplicationTwAuthorizedInformationTwAuthorizedTypeCode { Value = obj.AuthorizedTypeCode.Left(1) } });
				PopulateAuthorizedInformationAdditionalDocument(bo.TwAuthorizedInformation, obj.AdditionalDocument);
			}
		}

		public void PopulateAuthorizedInformationAdditionalDocument(DeclarationTwApplicationTwAuthorizedInformation bo, IAdditionalDocument obj)
		{
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.AdditionalDocument = new DeclarationTwApplicationTwAuthorizedInformationAdditionalDocument() { Id = new DeclarationTwApplicationTwAuthorizedInformationAdditionalDocumentId { Value = obj.ID.Left(10) } };
			}
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_Declarer
		public void PopulateDeclarer(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (CanPopulateDeclarer(obj))
			{
				bo.TwDeclarer = new DeclarationTwApplicationTwDeclarer();
				var newItem = bo.TwDeclarer;
				newItem.TwChineseName = new DeclarationTwApplicationTwDeclarerTwChineseName { Value = obj.ChineseName.Left(70) };
				newItem.TwId = new DeclarationTwApplicationTwDeclarerTwId { Value = obj.ID.Left(14) };
				newItem.TwName = new DeclarationTwApplicationTwDeclarerTwName { Value = obj.Name.Left(80) };
				newItem.TwTypeCode = new DeclarationTwApplicationTwDeclarerTwTypeCode { Value = obj.TypeCode.Left(3) };
				PopulateDeclarerAddress(newItem, obj.Address);
				PopulateDeclarerCommunication(newItem, obj.Communications);
			}
		}

		bool CanPopulateDeclarer(IPartyDetails obj) => obj != null && (!obj.ChineseName.IsEmpty || !obj.ID.IsEmpty || !obj.Name.IsEmpty || !obj.TypeCode.IsEmpty);

		public void PopulateDeclarerAddress(DeclarationTwApplicationTwDeclarer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationTwDeclarerAddress() { TwChineseLine = new DeclarationTwApplicationTwDeclarerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) } };
			}
		}

		#region NX5105_CM: Declaration/tw_Application/tw_Declarer/Communication
		public void PopulateDeclarerCommunication(DeclarationTwApplicationTwDeclarer bo, IEnumerable<ICommunication> obj)
		{
			var collection = new Collection<DeclarationTwApplicationTwDeclarerCommunication>();
			if (obj?.Any() ?? false)
			{
				AddNewDeclarerCommunication(obj, DeclarerCommunicationTypeID.TE, collection, 20);
				AddNewDeclarerCommunication(obj, DeclarerCommunicationTypeID.MA, collection, 60);
			}
			if (!collection.Any())
			{
				AddNewDeclarerCommunication(ZString.Empty, ZString.Empty, collection);
			}
			bo.Communication = collection;
		}

		void AddNewDeclarerCommunication(IEnumerable<ICommunication> obj, ZString filterTypeId, Collection<DeclarationTwApplicationTwDeclarerCommunication> collection, int idMaxLength)
		{
			var item = obj.FirstOrDefault(x => x.TypeID == filterTypeId);
			if (item != null)
			{
				AddNewDeclarerCommunication(item.ID, item.TypeID, collection, idMaxLength);
			}
		}

		void AddNewDeclarerCommunication(ZString id, ZString typeId, Collection<DeclarationTwApplicationTwDeclarerCommunication> collection, int idMaxLength = -1)
		{
			var result = new DeclarationTwApplicationTwDeclarerCommunication();
			result.Id = new DeclarationTwApplicationTwDeclarerCommunicationId { Value = id.Left(idMaxLength) };
			result.TypeId = new DeclarationTwApplicationTwDeclarerCommunicationTypeId { Value = typeId.Left(2) };
			collection.Add(result);
		}
		#endregion
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_ItemGroupReference
		public void PopulateItemGroupReference(DeclarationTwApplication bo, IEnumerable<ZInt> obj)
		{
			var collection = new Collection<DeclarationTwApplicationTwItemGroupReference>();
			obj?.Take(9999).ToList().ForEach(x => collection.Add(NewItemGroupReference(x)));
			if (!collection.Any())
			{
				collection.Add(NewItemGroupReference(ZInt.Zero));
			}
			bo.TwItemGroupReference = collection;
		}

		DeclarationTwApplicationTwItemGroupReference NewItemGroupReference(ZInt obj) => new DeclarationTwApplicationTwItemGroupReference() { TwSequenceNumeric = obj };
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_Label
		public void PopulateLabel(DeclarationTwApplication bo, IEnumerable<ILabel> obj)
		{
			if (obj?.Any() ?? false)
			{
				var collection = new Collection<DeclarationTwApplicationTwLabel>();
				obj.Take(99).ToList().ForEach(x =>
				{
					var newItem = new DeclarationTwApplicationTwLabel();
					PopulateStatus(newItem, x.StatusNameCode);
					PopulateLabelDetail(newItem, x.LabelDetails);
					collection.Add(newItem);
				});
				bo.TwLabel = collection;
			}
		}

		public void PopulateStatus(DeclarationTwApplicationTwLabel bo, ZString obj)
		{
			bo.Status = new DeclarationTwApplicationTwLabelStatus() { NameCode = new DeclarationTwApplicationTwLabelStatusNameCode { Value = obj.Left(1) } };
		}

		#region NX5105_CM: Declaration/tw_Application/tw_Label/tw_LabelDetail
		public void PopulateLabelDetail(DeclarationTwApplicationTwLabel bo, IEnumerable<ILabelDetail> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationTwApplicationTwLabelTwLabelDetail>();
				obj.Where(x => CanPopulateLabelDetail(x)).Take(99).ToList().ForEach(labelDetail => collection.Add(NewLabelDetail(labelDetail)));
				if (collection.Any())
				{
					bo.TwLabelDetail = collection;
				}
			}
		}

		bool CanPopulateLabelDetail(ILabelDetail obj) => obj != null && (!obj.EndNumber.IsEmpty || !obj.StartNumber.IsEmpty || !obj.Track.IsEmpty || !obj.Year.IsEmpty);

		DeclarationTwApplicationTwLabelTwLabelDetail NewLabelDetail(ILabelDetail labelDetail)
		{
			var result = new DeclarationTwApplicationTwLabelTwLabelDetail();
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.EndNumber, () => result.TwEndNumber = new DeclarationTwApplicationTwLabelTwLabelDetailTwEndNumber { Value = labelDetail.EndNumber.Left(8) });
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.StartNumber, () => result.TwStartNumber = new DeclarationTwApplicationTwLabelTwLabelDetailTwStartNumber { Value = labelDetail.StartNumber.Left(8) });
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.Track, () => result.TwTrack = new DeclarationTwApplicationTwLabelTwLabelDetailTwTrack { Value = labelDetail.Track.Left(3) });
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.Year, () => result.TwYear = new DeclarationTwApplicationTwLabelTwLabelDetailTwYear { Value = labelDetail.Year.Left(3) });
			return result;
		}
		#endregion
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_LocalManufacturer
		public void PopulateLocalManufacturer(DeclarationTwApplication bo, IPartyDetails obj)
		{
			var chineseName = obj?.ChineseName.Left(70) ?? ZString.Empty;
			if (!chineseName.IsEmpty)
			{
				bo.TwLocalManufacturer = new DeclarationTwApplicationTwLocalManufacturer();
				var newItem = bo.TwLocalManufacturer;
				newItem.TwChineseName = new DeclarationTwApplicationTwLocalManufacturerTwChineseName { Value = chineseName };
				PopulateLocalManufacturerAddress(newItem, obj.Address);
				PopulateLocalManufacturerCommunication(newItem, obj.Communications?.FirstOrDefault());
			}
		}

		public void PopulateLocalManufacturerAddress(DeclarationTwApplicationTwLocalManufacturer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationTwLocalManufacturerAddress() { TwChineseLine = new DeclarationTwApplicationTwLocalManufacturerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) } };
			}
		}

		public void PopulateLocalManufacturerCommunication(DeclarationTwApplicationTwLocalManufacturer bo, ICommunication obj)
		{
			var id = obj?.ID.Left(20) ?? ZString.Empty;
			bo.Communication = new DeclarationTwApplicationTwLocalManufacturerCommunication() { Id = new DeclarationTwApplicationTwLocalManufacturerCommunicationId { Value = id } };
		}
		#endregion

		#region NX5105_CM: Declaration/tw_Application/tw_Wine
		public void PopulateApplicationWine(DeclarationTwApplication bo, IApplicationWine obj)
		{
			if (obj != null)
			{
				var newItem = new DeclarationTwApplicationTwWine();
				PopulateApplicationWineAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateApplicationWineGovernmentProcedure(newItem, obj.GovernmentProcedurePreviousCode);
				PopulateApplicationWinePreviousDocument(newItem, obj.PreviousDocument);
				if (newItem.AdditionalDocument.Any() || newItem.GovernmentProcedure != null || newItem.PreviousDocument != null)
				{
					bo.TwWine = newItem;
				}
			}
		}

		#region NX5105_CM: Declaration/tw_Application/tw_Wine/AdditionalDocument
		public void PopulateApplicationWineAdditionalDocument(DeclarationTwApplicationTwWine bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (CanPopulateApplicationWineAdditionalDocument(obj))
			{
				var collection = new Collection<DeclarationTwApplicationTwWineAdditionalDocument>();
				obj.Where(x => !x.ID.IsEmpty).Take(10).ToList().ForEach(y => collection.Add(NewWineAdditionalDocument(y)));
				bo.AdditionalDocument = collection;
			}
		}

		bool CanPopulateApplicationWineAdditionalDocument(IEnumerable<IAdditionalDocument> obj) => obj != null && obj.Any(x => !x.ID.IsEmpty);

		DeclarationTwApplicationTwWineAdditionalDocument NewWineAdditionalDocument(IAdditionalDocument obj) =>
			new DeclarationTwApplicationTwWineAdditionalDocument() { Id = new DeclarationTwApplicationTwWineAdditionalDocumentId { Value = obj.ID.Left(14) } };
		#endregion

		public void PopulateApplicationWineGovernmentProcedure(DeclarationTwApplicationTwWine bo, ZString obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj, () =>
				bo.GovernmentProcedure = new DeclarationTwApplicationTwWineGovernmentProcedure() { PreviousCode = new DeclarationTwApplicationTwWineGovernmentProcedurePreviousCode { Value = obj.Left(1) } });
		}

		public void PopulateApplicationWinePreviousDocument(DeclarationTwApplicationTwWine bo, IPreviousDocument obj)
		{
			var id = obj?.ID.Left(14) ?? ZString.Empty;
			PopulateValueIfNodeValueIsNotEmpty(id, () =>
				bo.PreviousDocument = new DeclarationTwApplicationTwWinePreviousDocument() { Id = new DeclarationTwApplicationTwWinePreviousDocumentId { Value = id } });
		}
		#endregion
	}
}
