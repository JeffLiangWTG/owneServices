using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5203;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class N5203MessageBuilder : BaseTWMessageBuilder<IN5203Declaration, Declaration>
	{
		public ZString PopulateXml(IN5203Declaration declaration, string functionCode)
		{
			return declaration != null ? XmlHelper.Serializer(typeof(Declaration), PopulateDeclaration(declaration, functionCode), true) : string.Empty;
		}

		public override Declaration PopulateDeclaration(IN5203Declaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.AcceptanceDateTime = input.AcceptanceDateTime.ToISO8601ShortDateString();
				PopulateValueIfNodeValueIsNotEmpty(input.Authentication, () => { newItem.Authentication = new DeclarationAuthentication { Value = input.Authentication }; });
				newItem.FunctionCode = new DeclarationFunctionCode { Value = functionCode };
				newItem.Id = new DeclarationId { Value = input.ID };
				newItem.InvoiceAmount = new DeclarationInvoiceAmount { Value = input.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(input.InvoiceAmount)) };
				newItem.TotalGrossMassMeasure = new DeclarationTotalGrossMassMeasure { Value = input.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(input.TotalGrossMassMeasure)) };
				newItem.TotalPackageQuantity = new DeclarationTotalPackageQuantity { Value = input.TotalPackageQuantity };
				PopulateValueIfNodeValueIsNotEmpty(input.AssociatedGovernmentProcedureCode, () => { newItem.TwAssociatedGovernmentProcedureCode = new DeclarationTwAssociatedGovernmentProcedureCode { Value = input.AssociatedGovernmentProcedureCode }; });
				newItem.TypeCode = new DeclarationTypeCode { Value = input.TypeCode };
				PopulateAdditionalDocument(newItem, input.AdditionalDocuments);
				PopulateAdditionalInformation(newItem, input.AdditionalInformation);
				PopulateAgent(newItem, input.Agent);
				PopulateBorderTransportMeans(newItem, input.BorderTransportMeans);
				PopulateCurrencyExchange(newItem, input.CurrencyExchange);
				PopulateDutyTaxFee(newItem, input.DutyTaxFee);
				PopulateGoodsShipment(newItem, input.GoodsShipment);
				PopulateDeclarationGovernmentProcedure(newItem, input.GovernmentProcedureDescriptions);
				PopulateDeclarationPackaging(newItem, input.Packaging);
				PopulateRepresentativePerson(newItem, input.RepresentativePersonName);
			}
			return newItem;
		}

		public void PopulateAdditionalDocument(Declaration bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationAdditionalDocument() { Id = new DeclarationAdditionalDocumentId { Value = item.ID } };
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
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
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { bo.LpcoAuthorizedParty = new DeclarationAgentLpcoAuthorizedParty() { Id = new DeclarationAgentLpcoAuthorizedPartyId { Value = obj.ID } }; });
			}
		}

		public void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationBorderTransportMeans() { TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode } };
				PopulateItinerary(bo.BorderTransportMeans, obj.ItineraryRoutingCountryCodes);
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
				PopulateValueIfNodeValueIsNotEmpty(obj.DutyMethodCode, () => { bo.DutyTaxFee = new DeclarationDutyTaxFee() { TwDutyMethodCode = new DeclarationDutyTaxFeeTwDutyMethodCode { Value = obj.DutyMethodCode } }; });
			}
		}

		public void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				newItem.TwItemChargeAmount = new DeclarationGoodsShipmentTwItemChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ItemChargeAmount)) };
				PopulateGoodsShipmentAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateBuyer(newItem, obj.Buyer);
				PopulateConsignee(newItem, obj.Consignee);
				PopulateConsignment(newItem, obj.Consignment);
				PopulateConsignor(newItem, obj.Consignor);
				PopulateCustomsValuation(newItem, obj.CustomsValuation);
				PopulateDeliveryDestination(newItem, obj.DeliveryDestinationName);
				PopulateExporter(newItem, obj.Exporter);
				PopulateGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
				PopulateNotifyParty(newItem, obj.NotifyParty);
				PopulateTradeTerms(newItem, obj.TradeTermsConditionCode);
				PopulateUCR(newItem, obj.UCR);
			}
		}

		public void PopulateGoodsShipmentAdditionalDocument(DeclarationGoodsShipment bo, IEnumerable<IAdditionalDocument> obj)
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

		public void PopulateBuyer(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Buyer = new DeclarationGoodsShipmentBuyer();
				var newItem = bo.Buyer;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { newItem.Id = new DeclarationGoodsShipmentBuyerId { Value = obj.ID.Left(14) }; });
				newItem.Name = new DeclarationGoodsShipmentBuyerName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.CustomsControlID, () => { newItem.TwCustomsControlId = new DeclarationGoodsShipmentBuyerTwCustomsControlId { Value = obj.CustomsControlID.Left(8) }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => { newItem.TwTypeCode = new DeclarationGoodsShipmentBuyerTwTypeCode { Value = obj.TypeCode.Left(3) }; });
				PopulateBuyerAddress(newItem, obj.Address);
				PopulateBuyerLPCOAuthorizedParty(newItem, obj.LPCOAuthorizedParty, obj.Address);
			}
		}

		public void PopulateBuyerAddress(DeclarationGoodsShipmentBuyer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentBuyerAddress();
				var newItem = bo.Address;
				newItem.CountryCode = new DeclarationGoodsShipmentBuyerAddressCountryCode { Value = obj.CountryCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => { newItem.Line = new DeclarationGoodsShipmentBuyerAddressLine { Value = obj.Line.Left(120) }; });
			}
		}

		public void PopulateBuyerLPCOAuthorizedParty(DeclarationGoodsShipmentBuyer bo, ILPCOAuthorizedParty obj, IAddress address)
		{
			if (obj != null && address != null && MessageBuilderHelper.EffectiveAEOCountryCodes.Contains(address.CountryCode))
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { bo.LpcoAuthorizedParty = new DeclarationGoodsShipmentBuyerLpcoAuthorizedParty() { Id = new DeclarationGoodsShipmentBuyerLpcoAuthorizedPartyId { Value = obj.ID.Left(20) } }; });
			}
		}

		public void PopulateConsignee(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Consignee = new DeclarationGoodsShipmentConsignee();
				var newItem = bo.Consignee;
				newItem.Id = new DeclarationGoodsShipmentConsigneeId { Value = obj.ID };
				newItem.Name = new DeclarationGoodsShipmentConsigneeName { Value = obj.Name.SubstringSafe(0, 80) };
				newItem.TwChineseName = new DeclarationGoodsShipmentConsigneeTwChineseName { Value = obj.ChineseName.Left(70) };
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentConsigneeTwTypeCode { Value = obj.TypeCode });
				PopulateConsigneeAddress(newItem, obj.Address);
			}
		}

		public void PopulateConsigneeAddress(DeclarationGoodsShipmentConsignee bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentConsigneeAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationGoodsShipmentConsigneeAddressLine { Value = obj.Line.SubstringSafe(0, 120) };
				newItem.TwChineseLine = new DeclarationGoodsShipmentConsigneeAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		public void PopulateConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.ShippingOrderNumber, () => { newItem.TwShippingOrderNumber = new DeclarationGoodsShipmentConsignmentTwShippingOrderNumber { Value = obj.ShippingOrderNumber }; });
				PopulateConsignmentAdditionalInformation(newItem, obj.AdditionalInformations);
				PopulateConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateCarrier(newItem, obj.Carrier);
				PopulateDepartureTransportMeans(newItem, obj.DepartureTransportMeans);
				PopulateGoodsLocation(newItem, obj.GoodsLocations);
				PopulateLoadingLocation(newItem, obj.LoadingLocation);
				PopulateTransportContractDocument(newItem, obj.TransportContractDocuments);
				PopulateTransportEquipment(newItem, obj.TransportEquipments);
				PopulateBondedGoods(newItem, obj.BondedGoods);
				PopulateUnloadingLocation(newItem, obj.UnloadingLocation);
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

		public void PopulateConsignmentBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID };
				PopulateValueIfNodeValueIsNotEmpty(obj.CallSignID, () => { newItem.TwCallSignId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansTwCallSignId { Value = obj.CallSignID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => { newItem.TwRegistration = new DeclarationGoodsShipmentConsignmentBorderTransportMeansTwRegistration { Value = obj.Registration }; });
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

		public void PopulateDepartureTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.DepartureTransportMeans = new DeclarationGoodsShipmentConsignmentDepartureTransportMeans();
				var newItem = bo.DepartureTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { newItem.Id = new DeclarationGoodsShipmentConsignmentDepartureTransportMeansId { Value = obj.ID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => { newItem.Name = new DeclarationGoodsShipmentConsignmentDepartureTransportMeansName { Value = obj.Name }; });
				newItem.TypeCode = new DeclarationGoodsShipmentConsignmentDepartureTransportMeansTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateGoodsLocation(DeclarationGoodsShipmentConsignment bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				int valueIsEmptyCount = 0;
				var collection = new Collection<DeclarationGoodsShipmentConsignmentGoodsLocation>();
				foreach (var item in obj)
				{
					if (item.IsEmpty)
					{
						valueIsEmptyCount++;
					}
					if (valueIsEmptyCount < 2 || !item.IsEmpty)
					{
						var newItem = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = item } };
						collection.Add(newItem);
					}
				}
				bo.GoodsLocation = collection;
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
				PopulateValueIfNodeValueIsNotEmpty(obj.DocumentCode, () => { newItem.TwDocumentCode = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwDocumentCode { Value = obj.DocumentCode }; });
				newItem.TwRefundable = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwRefundable { Value = obj.Refundable };
				PopulateBondedFactory(newItem, obj.BondedFactories);
				PopulateBondedGoodsInvoice(newItem, obj.BondedGoodsInvoices);
				PopulateBondedGoodsMonthlyReport(newItem, obj.BondedGoodsMonthlyReport);
				PopulateInBondedParty(newItem, obj.InBondedParty);
				PopulateOutBondedParty(newItem, obj.OutBondedParty);
			}
		}

		public void PopulateBondedFactory(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IEnumerable<IBondedParty> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedFactory>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedFactory();
					newItem.TwCustomsControlId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedFactoryTwCustomsControlId { Value = item.CustomsControlID };
					newItem.TwId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedFactoryTwId { Value = item.ID };
					newItem.TwTypeCode = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedFactoryTwTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TwBondedFactory = collection;
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
					newItem.TwValueAmount = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsInvoiceTwValueAmount { Value = item.ValueAmount };
					collection.Add(newItem);
				}
				bo.TwBondedGoodsInvoice = collection;
			}
		}

		public void PopulateBondedGoodsMonthlyReport(DeclarationGoodsShipmentConsignmentTwBondedGoods bo, IBondedGoodsMonthlyReport obj)
		{
			if (obj != null && !obj.MonthNumeric.IsEmpty)
			{
				bo.TwBondedGoodsMonthlyReport = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsMonthlyReport();
				var newItem = bo.TwBondedGoodsMonthlyReport;
				newItem.TwMonthNumeric = obj.MonthNumeric;
				PopulateValueIfNodeValueIsNotEmpty(obj.TraderReferenceID, () => { newItem.TwTraderReferenceId = new DeclarationGoodsShipmentConsignmentTwBondedGoodsTwBondedGoodsMonthlyReportTwTraderReferenceId { Value = obj.TraderReferenceID }; });
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

		public void PopulateUnloadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			var id = obj?.ID ?? ZString.Empty;
			if (!id.IsEmpty)
			{
				bo.UnloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation() { Id = new DeclarationGoodsShipmentConsignmentUnloadingLocationId { Value = id } };
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
			if (obj != null && (!obj.ExitToEntryChargeAmount.IsEmpty || !obj.FreightChargeAmount.IsEmpty || !obj.OtherChargeAmount.IsEmpty || !obj.OtherDeductionAmount.IsEmpty))
			{
				bo.CustomsValuation = new DeclarationGoodsShipmentCustomsValuation();
				var newItem = bo.CustomsValuation;
				PopulateValueIfNodeValueIsNotEmpty(obj.ExitToEntryChargeAmount, () => { newItem.ExitToEntryChargeAmount = new DeclarationGoodsShipmentCustomsValuationExitToEntryChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ExitToEntryChargeAmount)) }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.FreightChargeAmount, () => { newItem.FreightChargeAmount = new DeclarationGoodsShipmentCustomsValuationFreightChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.FreightChargeAmount)) }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.OtherChargeAmount, () => { newItem.TwOtherChargeAmount = new DeclarationGoodsShipmentCustomsValuationTwOtherChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherChargeAmount)) }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.OtherDeductionAmount, () => { newItem.TwOtherDeductionAmount = new DeclarationGoodsShipmentCustomsValuationTwOtherDeductionAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherDeductionAmount)) }; });
			}
		}

		public void PopulateDeliveryDestination(DeclarationGoodsShipment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.DeliveryDestination = new DeclarationGoodsShipmentDeliveryDestination() { Name = new DeclarationGoodsShipmentDeliveryDestinationName { Value = obj } };
			}
		}

		public void PopulateExporter(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Exporter = new DeclarationGoodsShipmentExporter();
				var newItem = bo.Exporter;
				newItem.Id = new DeclarationGoodsShipmentExporterId { Value = obj.ID.Left(14) };
				newItem.Name = new DeclarationGoodsShipmentExporterName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.CustomsControlID, () => { newItem.TwCustomsControlId = new DeclarationGoodsShipmentExporterTwCustomsControlId { Value = obj.CustomsControlID.Left(8) }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.PaymentOnAccountBusinessID, () => { newItem.TwPaymentOnAccountBusinessId = new DeclarationGoodsShipmentExporterTwPaymentOnAccountBusinessId { Value = obj.PaymentOnAccountBusinessID.Left(8) }; });
				newItem.TwTypeCode = new DeclarationGoodsShipmentExporterTwTypeCode { Value = obj.TypeCode.Left(3) };
				PopulateExporterAddress(newItem, obj.Address);
				PopulateExporterLPCOAuthorizedParty(newItem, obj.LPCOAuthorizedParty);
			}
		}

		public void PopulateExporterAddress(DeclarationGoodsShipmentExporter bo, IAddress obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => { bo.Address = new DeclarationGoodsShipmentExporterAddress() { Line = new DeclarationGoodsShipmentExporterAddressLine { Value = obj.Line.Left(120) } }; });
			}
		}

		public void PopulateExporterLPCOAuthorizedParty(DeclarationGoodsShipmentExporter bo, ILPCOAuthorizedParty obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { bo.LpcoAuthorizedParty = new DeclarationGoodsShipmentExporterLpcoAuthorizedParty() { Id = new DeclarationGoodsShipmentExporterLpcoAuthorizedPartyId { Value = obj.ID.Left(20) } }; });
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
					PopulateGovernmentProcedure(newItem, item.GovernmentProcedure);
					PopulateManufacturer(newItem, item.Manufacturer);
					PopulateOrigin(newItem, item.Origin);
					PopulatePackaging(newItem, item.Packaging);
					PopulatePreviousDocument(newItem, item.PreviousDocument);
					PopulateGoodsStatisticalMeasure(newItem, item.GoodsStatisticalMeasure);
					PopulatePreBondedDocument(newItem, item.PreBondedDocument);
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
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument() { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentId { Value = item.ID } };
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
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => { newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID.Left(80) }; });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description.Left(512) };
				newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name.Left(50) };
				PopulateValueIfNodeValueIsNotEmpty(obj.BondedNoteCode, () => { newItem.TwBondedNoteCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwBondedNoteCode { Value = obj.BondedNoteCode }; });
				PopulateCommodityAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateClassification(newItem, obj.Classifications);
				PopulateConstituent(newItem, obj.Constituent);
				PopulateInvoiceLine(newItem, obj.InvoiceLine);
				PopulateCommodityNumber(newItem, obj.CommodityNumbers);
				PopulateDutyOtherTaxFee(newItem, obj.DutyOtherTaxFees);
				PopulateVehicleID(newItem, obj.VehicleIDs);
			}
		}

		public void PopulateCommodityAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument();
					newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentId { Value = item.ID };
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

		public void PopulateConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ElementDescription, () => { bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent() { ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription.Left(256) } }; });
			}
		}

		public void PopulateInvoiceLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IInvoiceLine obj)
		{
			if (obj != null)
			{
				bo.InvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine();
				var newItem = bo.InvoiceLine;
				newItem.ItemChargeAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineItemChargeAmount { Value = obj.ItemChargeAmount.Normalize() };
				newItem.TwUnitPriceAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwUnitPriceAmount { Value = obj.UnitPriceAmount };
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
				var dutyOtherTaxFeeObj = obj.Where(x => !x.TaxRateNumeric.IsEmpty && !x.TypeCode.IsEmpty);
				if (dutyOtherTaxFeeObj.Any())
				{
					var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFee>();
					foreach (var item in dutyOtherTaxFeeObj)
					{
						var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFee();
						newItem.TwTaxRateNumeric = item.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.TaxRateNumeric));
						newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFeeTwTypeCode { Value = item.TypeCode };
						collection.Add(newItem);
					}
					bo.TwDutyOtherTaxFee = collection;
				}
			}
		}

		public void PopulateVehicleID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleId>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleId() { TwVinid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwVehicleIdTwVinid { Value = item } };
					collection.Add(newItem);
				}
				bo.TwVehicleId = collection;
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

		public void PopulateGovernmentProcedure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGovernmentProcedure obj)
		{
			if (obj != null)
			{
				bo.GovernmentProcedure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGovernmentProcedure() { CurrentCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGovernmentProcedureCurrentCode { Value = obj.CurrentCode } };
			}
		}

		public void PopulateManufacturer(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPartyDetails obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.Name.IsEmpty || !obj.TypeCode.IsEmpty))
			{
				bo.Manufacturer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer();
				var newItem = bo.Manufacturer;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerId { Value = obj.ID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => { newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerName { Value = obj.Name.Left(70) }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => { newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerTwTypeCode { Value = obj.TypeCode }; });
			}
		}

		public void PopulateOrigin(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IOrigin obj)
		{
			if (obj != null && (!obj.CountryCode.IsEmpty || !obj.AdditionalDocument.ID.IsEmpty || !obj.AdditionalDocument.SequenceNumeric.IsEmpty))
			{
				bo.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin();
				PopulateValueIfNodeValueIsNotEmpty(obj.CountryCode, () => { bo.Origin.CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = obj.CountryCode }; });
				PopulateOriginAdditionalDocument(bo.Origin, obj.AdditionalDocument);
			}
		}

		public void PopulateOriginAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin bo, IAdditionalDocument obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.SequenceNumeric.IsEmpty))
			{
				bo.AdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginAdditionalDocument() { TwSequenceNumericValueSpecified = true };
				var newItem = bo.AdditionalDocument;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginAdditionalDocumentId { Value = obj.ID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.SequenceNumeric, () => { newItem.TwSequenceNumeric = obj.SequenceNumeric; });
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

		public void PopulatePreviousDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPreviousDocument obj)
		{
			if (obj != null && (!obj.ID.IsEmpty && !obj.LineNumeric.IsEmpty))
			{
				bo.PreviousDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument();
				var newItem = bo.PreviousDocument;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentId { Value = obj.ID };
				newItem.LineNumeric = obj.LineNumeric;
			}
		}

		public void PopulateGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsStatisticalMeasure obj)
		{
			if (obj != null && (!obj.StatisticalUnitCode.IsEmpty && !obj.TariffQuantity.IsEmpty))
			{
				bo.TwGoodsStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasure();
				var newItem = bo.TwGoodsStatisticalMeasure;
				newItem.TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode };
				newItem.TwTariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwTariffQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TariffQuantity)) };
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

		public void PopulateNotifyParty(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.NotifyParty = new DeclarationGoodsShipmentNotifyParty();
				var newItem = bo.NotifyParty;
				newItem.Id = new DeclarationGoodsShipmentNotifyPartyId { Value = obj.ID };
				newItem.Name = new DeclarationGoodsShipmentNotifyPartyName { Value = obj.Name.SubstringSafe(0, 80) };
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
				newItem.Line = new DeclarationGoodsShipmentNotifyPartyAddressLine { Value = obj.Line.SubstringSafe(0, 120) };
				newItem.TwChineseLine = new DeclarationGoodsShipmentNotifyPartyAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
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
			PopulateValueIfNodeValueIsNotEmpty(obj, () => { bo.Ucr = new DeclarationGoodsShipmentUcr() { Id = new DeclarationGoodsShipmentUcrId { Value = obj } }; });
		}

		public void PopulateDeclarationGovernmentProcedure(Declaration bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGovernmentProcedure>();
				foreach (var item in obj)
				{
					PopulateValueIfNodeValueIsNotEmpty(item, () => { collection.Add(new DeclarationGovernmentProcedure() { Description = new DeclarationGovernmentProcedureDescription { Value = item } }); });
				}
				if (collection.Count > 0)
				{
					bo.GovernmentProcedure = collection;
				}
			}
		}

		public void PopulateDeclarationPackaging(Declaration bo, IDeclarationPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationPackaging();
				var newItem = bo.Packaging;
				newItem.MarksNumbers = new DeclarationPackagingMarksNumbers { Value = obj.MarksNumbers.Left(512) };
				PopulateValueIfNodeValueIsNotEmpty(obj.PackagingMaterialDescription, () => { newItem.PackagingMaterialDescription = new DeclarationPackagingPackagingMaterialDescription { Value = obj.PackagingMaterialDescription }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Combination, () => { newItem.TwCombination = new DeclarationPackagingTwCombination { Value = obj.Combination }; });
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
	}
}
