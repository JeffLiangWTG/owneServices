using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5203;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5203MessageHelper : TWMessageHelper
	{
		public N5203MessageHelper(TWMessage message) : base(message)
		{
			Declaration = new TWOutgoingMessageKeyInfomation(message.EM_MessageType, message.EM_MessageText).Result as Declaration;
		}

		Declaration Declaration { get; }

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (Declaration != null)
			{
				var goodsShipment = Declaration.GoodsShipment;
				var consignment = goodsShipment.Consignment;

				WriteMainDetails(table, goodsShipment, consignment);
				WritePartyDetails(table, goodsShipment, consignment);
				WriteBondedGoods(table, goodsShipment, consignment);
				WriteDetail(table, goodsShipment);
			}
		}

		void WriteMainDetails(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment, DeclarationGoodsShipmentConsignment consignment)
		{
			WriteTitleRow(table, Titles.HeaderTitle, FirstHeadStyle);
			WriteTitleRow(table, Titles.MainTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.DeclarationID, Declaration.Id?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationAcceptanceDateTime, Declaration.AcceptanceDateTime);
			WriteRowWithTwoCells(table, Titles.DeclarationTypeCode, Declaration.TypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansTypeCode, Declaration.BorderTransportMeans?.TypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationAssociatedGovernmentProcedureCode, Declaration.TwAssociatedGovernmentProcedureCode?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationFunctionCode, Declaration.FunctionCode?.Value);
			WriteTitleRow(table, Titles.AmountTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.DeclarationInvoiceAmount, Declaration.InvoiceAmount?.Value);
			WriteRowWithTwoCells(table, Titles.CustomsValuationExitToEntryChargeAmount, goodsShipment?.CustomsValuation?.ExitToEntryChargeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.CustomsValuationFreightChargeAmount, goodsShipment?.CustomsValuation?.FreightChargeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.CustomsValuationOtherChargeAmount, goodsShipment?.CustomsValuation?.TwOtherChargeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.CustomsValuationOtherDeductionAmount, goodsShipment?.CustomsValuation?.TwOtherDeductionAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.GoodsShipmentItemChargeAmount, goodsShipment?.TwItemChargeAmount?.Value);
			WriteRowWithTwoCells(table, Titles.TradeTermsConditionCode, goodsShipment?.TradeTerms?.ConditionCode?.Value);
			WriteRowWithTwoCells(table, Titles.CurrencyExchangeCurrencyTypeCode, Declaration.CurrencyExchange?.CurrencyTypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.CurrencyExchangeRateNumeric, Declaration.CurrencyExchange?.RateNumeric);
			WriteRowWithTwoCells(table, Titles.DutyTaxRateDutyMethodCode, Declaration.DutyTaxFee?.TwDutyMethodCode?.Value);
			WriteTitleRow(table, Titles.TransportTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansJourneyID, consignment?.BorderTransportMeans?.JourneyId?.Value);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansCallSignID, consignment?.BorderTransportMeans?.TwCallSignId?.Value);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansRegistration, consignment?.BorderTransportMeans?.TwRegistration?.Value);
			WriteRowWithTwoCells(table, Titles.DepartureTransportMeansID, consignment?.DepartureTransportMeans?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.DepartureTransportMeansName, consignment?.DepartureTransportMeans?.Name?.Value);
			WriteRowWithTwoCells(table, Titles.DepartureTransportMeansTypeCode, consignment?.DepartureTransportMeans?.TypeCode?.Value);
			if (consignment?.TransportEquipment?.Any() ?? false)
			{
				foreach (var transportEquipment in consignment.TransportEquipment)
				{
					WriteRowWithTwoCells(table, Titles.TransportEquipmentCharacteristicCode, transportEquipment?.CharacteristicCode?.Value);
					WriteRowWithTwoCells(table, Titles.TransportEquipmentID, transportEquipment?.Id?.Value);
					WriteRowWithTwoCells(table, Titles.TransportEquipmentUsedCapacityCode, transportEquipment?.TwUsedCapacityCode?.Value);
					if (transportEquipment?.TwSeal?.Any() ?? false)
					{
						foreach (var seal in transportEquipment.TwSeal)
						{
							WriteRowWithTwoCells(table, Titles.SealSealID, seal?.TwSealId?.Value);
						}
					}
				}
			}

			WriteRowWithTwoCells(table, Titles.GoodsLocationID, consignment?.GoodsLocation?.FirstOrDefault()?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.LoadingLocationID, consignment?.LoadingLocation?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.UnloadingLocationID, consignment?.UnloadingLocation?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.DeliveryDestinationName, goodsShipment?.DeliveryDestination?.Name?.Value);
			if (Declaration.BorderTransportMeans?.Itinerary?.Any() ?? false)
			{
				foreach (var itinerary in Declaration.BorderTransportMeans.Itinerary)
				{
					WriteRowWithTwoCells(table, Titles.ItineraryRoutingCountryType, itinerary.RoutingCountryCode?.Value);
				}
			}

			WriteRowWithTwoCells(table, Titles.ConsignmentShippingOrderNumber, consignment?.TwShippingOrderNumber?.Value);
			var transportContractDocument = consignment?.TransportContractDocument;
			if (transportContractDocument?.Any() ?? false)
			{
				WriteRowWithTwoCells(table, Titles.AirHBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._703)?.Id?.Value, CellStyle, false);
				WriteRowWithTwoCells(table, Titles.SeaMBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._704)?.Id?.Value, CellStyle, false);
				WriteRowWithTwoCells(table, Titles.SeaHBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._714)?.Id?.Value, CellStyle, false);
				WriteRowWithTwoCells(table, Titles.AirMBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._741)?.Id?.Value, CellStyle, false);
				foreach (var container in transportContractDocument?.Where(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._976))
				{
					WriteRowWithTwoCells(table, Titles.TransportContractDocumentID, container?.Id?.Value);
				}
			}

			WriteRowWithTwoCells(table, Titles.UCRID, goodsShipment?.Ucr?.Id?.Value);
			WriteTitleRow(table, Titles.SummaryTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.GovernmentProcedureDescription, Declaration.GovernmentProcedure?.Select(x => x.Description.Value));

			WriteRowWithTwoCells(table, Titles.PackagingMarksNumbers, Declaration.Packaging?.MarksNumbers?.Value);
			WriteRowWithTwoCells(table, Titles.PackagingPackagingMaterialDescription, Declaration.Packaging?.PackagingMaterialDescription?.Value);
			WriteRowWithTwoCells(table, Titles.PackagingCombination, Declaration.Packaging?.TwCombination?.Value);
			WriteRowWithTwoCells(table, Titles.PackagingTypeCode, Declaration.Packaging?.TypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationTotalGrossMassMeasure, Declaration.TotalGrossMassMeasure?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationTotalPackageQuantity, Declaration.TotalPackageQuantity?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationAuthentication, Declaration.Authentication?.Value);
			WriteRowWithTwoCells(table, Titles.AdditionalInformationCopyQuantity, Declaration.AdditionalInformation?.TwCopyQuantity?.Value);
			if (Declaration.AdditionalDocument?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.AdditionalDocumentTypeTitle, TitleStyle);
				foreach (var additionaldocument in Declaration.AdditionalDocument)
				{
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentTypeID, additionaldocument?.Id?.Value);
				}
			}

			if (consignment?.AdditionalInformation?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.StandbyTitle, TitleStyle);
				foreach (var additionalInformation in consignment.AdditionalInformation)
				{
					WriteRowWithTwoCells(table, Titles.AdditionalInformationStatementCode, additionalInformation?.StatementCode?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalInformationStatementDescription, additionalInformation?.StatementDescription?.Value);
				}
			}
		}

		void WritePartyDetails(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment, DeclarationGoodsShipmentConsignment consignment)
		{
			WriteTitleRow(table, Titles.PartyDetailTitle, HeadStyle);
			var agent = Declaration.Agent;
			var representativePerson = Declaration.RepresentativePerson;
			if (agent != null || representativePerson != null)
			{
				WriteTitleRow(table, Titles.AgentTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.AgentID, agent?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.AgentSubBoxID, agent?.TwSubBoxId?.Value);
				WriteRowWithTwoCells(table, Titles.AgentRoleCode, agent?.RoleCode?.Value);
				WriteRowWithTwoCells(table, Titles.AgentLPCOAuthorizedPartyID, agent?.LpcoAuthorizedParty?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.RepresentativePersonName, Declaration.RepresentativePerson?.Name?.Value);
			}

			var exporter = goodsShipment?.Exporter;
			if (exporter != null)
			{
				WriteTitleRow(table, Titles.ExporterTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.ExporterID, exporter.Id?.Value);
				WriteRowWithTwoCells(table, Titles.ExporterName, exporter.Name?.Value);
				WriteRowWithTwoCells(table, Titles.ExporterCustomsControlID, exporter.TwCustomsControlId?.Value);
				WriteRowWithTwoCells(table, Titles.ExporterPaymentOnAccountBusinessID, exporter.TwPaymentOnAccountBusinessId?.Value);
				WriteRowWithTwoCells(table, Titles.ExporterTypeCode, exporter.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.ExporterAddressLine, exporter.Address?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.ExporterLPCOAuthorizedPartyID, exporter.LpcoAuthorizedParty?.Id?.Value);
			}

			var buyer = goodsShipment?.Buyer;
			if (buyer != null)
			{
				WriteTitleRow(table, Titles.BuyerTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.BuyerID, buyer.Id?.Value);
				WriteRowWithTwoCells(table, Titles.BuyerName, buyer.Name?.Value);
				WriteRowWithTwoCells(table, Titles.BuyerCustomsControlID, buyer.TwCustomsControlId?.Value);
				WriteRowWithTwoCells(table, Titles.BuyerTypeCOde, buyer.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.BuyerAddressCountryCode, buyer.Address?.CountryCode?.Value);
				WriteRowWithTwoCells(table, Titles.BuyerAddressLine, buyer.Address?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.BuyerLPCOAuthorizedPartyID, buyer.LpcoAuthorizedParty?.Id?.Value);
			}

			var consignee = goodsShipment?.Consignee;
			if (consignee != null)
			{
				WriteTitleRow(table, Titles.ConsigneeTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.ConsigneeID, consignee.Id?.Value);
				WriteRowWithTwoCells(table, Titles.ConsigneeName, consignee.Name?.Value);
				WriteRowWithTwoCells(table, Titles.ConsigneeChineseName, consignee.TwChineseName?.Value);
				WriteRowWithTwoCells(table, Titles.ConsigneeTypeCode, consignee.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.ConsigneeAddressLine, consignee.Address?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.ConsigneeAddressChineseLine, consignee.Address?.TwChineseLine?.Value);
			}

			var consignor = goodsShipment?.Consignor;
			if (consignor != null)
			{
				WriteTitleRow(table, Titles.ConsignorTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.ConsignorID, consignor.Id?.Value);
				WriteRowWithTwoCells(table, Titles.ConsignorName, consignor.Name?.Value);
				WriteRowWithTwoCells(table, Titles.ConsignorChineseName, consignor.TwChineseName?.Value);
				WriteRowWithTwoCells(table, Titles.ConsignorTypeCode, consignor.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.ConsignorAddressLine, consignor.Address?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.ConsignorAddressChineseLine, consignor.Address?.TwChineseLine?.Value);
			}

			var notifyParty = goodsShipment?.NotifyParty;
			if (notifyParty != null)
			{
				WriteTitleRow(table, Titles.NotifyPartyTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.NotifyPartyID, notifyParty.Id?.Value);
				WriteRowWithTwoCells(table, Titles.NotifyPartyName, notifyParty.Name?.Value);
				WriteRowWithTwoCells(table, Titles.NotifyPartyChineseName, notifyParty.TwChineseName?.Value);
				WriteRowWithTwoCells(table, Titles.NotifyPartyTypeCode, notifyParty.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.NotifyPartyAddressLine, notifyParty.Address?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.NotifyPartyAddressChineseLine, notifyParty.Address?.TwChineseLine?.Value);
			}

			var carrier = consignment?.Carrier;
			if (carrier != null)
			{
				WriteTitleRow(table, Titles.CarrierTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.CarrierID, carrier.Id?.Value);
				WriteRowWithTwoCells(table, Titles.CarrierTypeCode, carrier.TwTypeCode?.Value);
			}
		}

		void WriteDetail(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			WriteTitleRow(table, Titles.DetailTitle, HeadStyle);
			if (goodsShipment?.GovernmentAgencyGoodsItem?.Any() ?? false)
			{
				foreach (var governmentAgencyGoodsItem in goodsShipment.GovernmentAgencyGoodsItem)
				{
					WriteTitleRow(table, GetTtile(Titles.GovernmentAgencyGoodsItemSequenceNumeric, governmentAgencyGoodsItem.SequenceNumeric.ToString()), TitleStyle);
					WriteTitleRow(table, Titles.MainTitle, SubTitleStyle);
					WriteRowWithTwoCells(table, Titles.CommodityComercialCategorizationID, governmentAgencyGoodsItem?.Commodity?.CommercialCategorizationId?.Value);
					WriteRowWithTwoCells(table, Titles.CommodityDescription, governmentAgencyGoodsItem?.Commodity?.Description?.Value);
					WriteRowWithTwoCells(table, Titles.CommodityName, governmentAgencyGoodsItem?.Commodity?.Name?.Value);
					WriteRowWithTwoCells(table, Titles.CommodityBondedNoteCode, governmentAgencyGoodsItem?.Commodity?.TwBondedNoteCode?.Value);
					WriteRowWithTwoCells(table, Titles.GovernmentProcedureCurrentCode, governmentAgencyGoodsItem?.GovernmentProcedure?.CurrentCode?.Value);
					WriteRowWithTwoCells(table, Titles.OriginCountryCode, governmentAgencyGoodsItem?.Origin?.CountryCode?.Value);

					var classifications = governmentAgencyGoodsItem.Commodity?.Classification;
					WriteRowWithTwoCells(table, Titles.ClassificationID, classifications?.FirstOrDefault(x => x.IdentificationTypeCode.Value == MessageConstants.IdentificationTypeCodes.HS)?.Id?.Value);

					WriteRowWithTwoCells(table, Titles.ConstituentElementDescription, governmentAgencyGoodsItem?.Commodity?.Constituent?.ElementDescription?.Value);
					WriteRowWithTwoCells(table, Titles.InvoiceLineItemChargeAmount, governmentAgencyGoodsItem?.Commodity?.InvoiceLine?.ItemChargeAmount?.Value);
					WriteRowWithTwoCells(table, Titles.InvoiceLineUnitPriceAmount, governmentAgencyGoodsItem?.Commodity?.InvoiceLine?.TwUnitPriceAmount?.Value);
					WriteRowWithTwoCells(table, Titles.GoodsMeasureTariffQuantity, governmentAgencyGoodsItem?.GoodsMeasure?.TariffQuantity?.Value);
					WriteRowWithTwoCells(table, Titles.GoodsMeasureUnitCode, governmentAgencyGoodsItem?.GoodsMeasure?.TwUnitCode?.Value);
					WriteRowWithTwoCells(table, Titles.GoodsMeasureNetWeightMeasure, governmentAgencyGoodsItem?.GoodsMeasure?.NetWeightMeasure?.Value);
					WriteRowWithTwoCells(table, Titles.GoodsStatisticalMeasureTariffQuantity, governmentAgencyGoodsItem?.TwGoodsStatisticalMeasure?.TwTariffQuantity?.Value);
					WriteRowWithTwoCells(table, Titles.GoodsStatisticalMeasureStatisticalUnitCode, governmentAgencyGoodsItem?.TwGoodsStatisticalMeasure?.TwStatisticalUnitCode?.Value);

					var packaging = governmentAgencyGoodsItem?.Packaging;
					if (packaging != null)
					{
						WriteTitleRow(table, Titles.PackagingTitle, SubTitleStyle);
						WriteRowWithTwoCells(table, Titles.PackagingQuantityQuantiy, packaging.QuantityQuantity?.Value);
						WriteRowWithTwoCells(table, Titles.PackagingTypeCode, packaging.TypeCode?.Value);
					}

					var commodityNumber = governmentAgencyGoodsItem?.Commodity?.TwCommodityNumber;
					if (commodityNumber?.Any() ?? false)
					{
						WriteTitleRow(table, Titles.CommodityNumberTitle, SubTitleStyle);
						WriteRowWithTwoCells(table, Titles.BuyerCommodityNumberID, commodityNumber.FirstOrDefault(x => x.TwIdentifierTypeCode?.Value == MessageConstants.IdentificationTypeCodes.BP)?.TwId?.Value, CellStyle, false);
						WriteRowWithTwoCells(table, Titles.SellerCommodityNumberID, commodityNumber.FirstOrDefault(x => x.TwIdentifierTypeCode?.Value == MessageConstants.IdentificationTypeCodes.SA)?.TwId?.Value, CellStyle, false);
					}

					var originAdditionalDocument = governmentAgencyGoodsItem?.Origin?.AdditionalDocument;
					if (originAdditionalDocument != null)
					{
						WriteTitleRow(table, Titles.CertificateOfOriginTitle, SubTitleStyle);
						WriteRowWithTwoCells(table, Titles.CertificateOfOriginID, originAdditionalDocument.Id?.Value);
						WriteRowWithTwoCells(table, Titles.AdditionalDocumentSequenceNumeric, originAdditionalDocument.TwSequenceNumeric);
					}

					if (governmentAgencyGoodsItem.Commodity?.AdditionalDocument?.Any() ?? false)
					{
						WriteTitleRow(table, Titles.ImportAndExportLicenseTitle, SubTitleStyle);
						foreach (var additionalDocument in governmentAgencyGoodsItem.Commodity.AdditionalDocument)
						{
							WriteRowWithTwoCells(table, Titles.ImportAndExportLicenseID, additionalDocument?.Id?.Value);
							WriteRowWithTwoCells(table, Titles.AdditionalDocumentSequenceNumeric, additionalDocument?.TwSequenceNumeric);
						}
					}
					if (governmentAgencyGoodsItem.AdditionalDocument?.Any() ?? false)
					{
						WriteTitleRow(table, Titles.AdditionalDocumentTitle, SubTitleStyle);
						foreach (var additionalDocument in governmentAgencyGoodsItem.AdditionalDocument)
						{
							WriteRowWithTwoCells(table, Titles.AdditionalDocumentID, additionalDocument?.Id?.Value);
						}
					}

					var previousDocument = governmentAgencyGoodsItem?.PreviousDocument;
					var preBondedDocument = governmentAgencyGoodsItem?.TwPreBondedDocument;
					if (previousDocument != null || preBondedDocument != null)
					{
						WriteTitleRow(table, Titles.PreviousDocumentTitle, SubTitleStyle);
						WriteRowWithTwoCells(table, Titles.PreviousDocumentID, previousDocument?.Id?.Value);
						WriteRowWithTwoCells(table, Titles.PreviousDocumentLineNumeric, previousDocument?.LineNumeric);
						WriteRowWithTwoCells(table, Titles.PreBondedDocumentID, preBondedDocument?.TwId?.Value);
						WriteRowWithTwoCells(table, Titles.PreBondedDocumentLineNumeric, preBondedDocument?.TwLineNumeric);
					}

					if (governmentAgencyGoodsItem.Commodity?.TwVehicleId?.Any() ?? false)
					{
						WriteTitleRow(table, Titles.VehicleTitle, SubTitleStyle);
						foreach (var vehicleID in governmentAgencyGoodsItem.Commodity.TwVehicleId)
						{
							WriteRowWithTwoCells(table, Titles.VehicleVINID, vehicleID.TwVinid?.Value);
						}
					}
					if (governmentAgencyGoodsItem.Commodity?.TwDutyOtherTaxFee?.Any() ?? false)
					{
						WriteTitleRow(table, Titles.DutyOtherTaxFeeTitle, SubTitleStyle);
						foreach (var dutyOtherTaxFee in governmentAgencyGoodsItem.Commodity.TwDutyOtherTaxFee)
						{
							WriteRowWithTwoCells(table, Titles.DutyOtherTaxFeeTaxRateNumeric, dutyOtherTaxFee?.TwTaxRateNumeric);
							WriteRowWithTwoCells(table, Titles.DutyOtherTaxFeeTypeCode, dutyOtherTaxFee?.TwTypeCode?.Value);
						}
					}

					var manufacturer = governmentAgencyGoodsItem?.Manufacturer;
					if (manufacturer != null)
					{
						WriteTitleRow(table, Titles.ManufacturerTitle, SubTitleStyle);
						WriteRowWithTwoCells(table, Titles.ManufacturerID, manufacturer.Id?.Value);
						WriteRowWithTwoCells(table, Titles.ManufacturerName, manufacturer.Name?.Value);
						WriteRowWithTwoCells(table, Titles.ManufacturerTypeCode, manufacturer.TwTypeCode?.Value);
					}

					if (governmentAgencyGoodsItem.AdditionalInformation?.Any() ?? false)
					{
						WriteTitleRow(table, Titles.AdditionalInformationStatementTitle, SubTitleStyle);
						foreach (var additionalInformation in governmentAgencyGoodsItem.AdditionalInformation)
						{
							WriteRowWithTwoCells(table, Titles.AdditionalInformationStatementCode, additionalInformation?.StatementCode?.Value);
							WriteRowWithTwoCells(table, Titles.AdditionalInformationStatementDescription, additionalInformation?.StatementDescription?.Value);
						}
					}

					if (governmentAgencyGoodsItem.Commodity?.Classification?.Any() ?? false)
					{
						var unClassificationID = classifications.FirstOrDefault(x => x.IdentificationTypeCode.Value == MessageConstants.IdentificationTypeCodes.SSO)?.Id?.Value;
						var iataClassificationID = classifications.FirstOrDefault(x => x.IdentificationTypeCode.Value == MessageConstants.IdentificationTypeCodes.ZZZ)?.Id?.Value;

						if (!string.IsNullOrEmpty(unClassificationID) || !string.IsNullOrEmpty(iataClassificationID))
						{
							WriteTitleRow(table, Titles.OtherTitle, SubTitleStyle);
							WriteRowWithTwoCells(table, Titles.UNClassificationID, unClassificationID);
							WriteRowWithTwoCells(table, Titles.IATAClassificationID, iataClassificationID);
						}
					}
				}
			}
		}

		void WriteBondedGoods(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment, DeclarationGoodsShipmentConsignment consignment)
		{
			WriteTitleRow(table, Titles.BondedGoodsTitle, HeadStyle);
			var bondedGoods = consignment?.TwBondedGoods;
			if (bondedGoods?.TwBondedFactory?.Any() ?? false)
			{
				foreach (var bondedFactory in consignment.TwBondedGoods.TwBondedFactory)
				{
					WriteTitleRow(table, Titles.BondedFactoryTitle, TitleStyle);
					WriteRowWithTwoCells(table, Titles.BondedFactoryCustomsControlID, bondedFactory?.TwCustomsControlId?.Value);
					WriteRowWithTwoCells(table, Titles.BondedFactoryID, bondedFactory?.TwId?.Value);
					WriteRowWithTwoCells(table, Titles.BondedFactoryTypeCode, bondedFactory?.TwTypeCode?.Value);
				}
			}

			var inBondedParty = bondedGoods?.TwInBondedParty;
			if (inBondedParty != null)
			{
				WriteTitleRow(table, Titles.InBondPartyTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.InBondPartyBondedID, inBondedParty.TwBondedId?.Value);
				WriteRowWithTwoCells(table, Titles.InBondPartyID, inBondedParty.TwId?.Value);
				WriteRowWithTwoCells(table, Titles.InBondPartyTypeCode, inBondedParty.TwTypeCode?.Value);
			}

			var outBondedParty = bondedGoods?.TwOutBondedParty;
			if (outBondedParty != null)
			{
				WriteTitleRow(table, Titles.OutBondedPartyTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.OutBondedPartyBondedID, outBondedParty.TwBondedId?.Value);
				WriteRowWithTwoCells(table, Titles.OutBondedPartyID, outBondedParty.TwId?.Value);
				WriteRowWithTwoCells(table, Titles.OutBondedPartyTypeCode, outBondedParty.TwTypeCode?.Value);
			}

			var documentCode = bondedGoods?.TwDocumentCode;
			var refundable = bondedGoods?.TwRefundable;
			if (documentCode != null || refundable != null)
			{
				WriteTitleRow(table, Titles.BluntTaxRebatesTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.BondedGoodsDocumentCode, documentCode?.Value);
				WriteRowWithTwoCells(table, Titles.BondedGoodsRefundable, refundable?.Value);
			}

			var bondedGoodsInvoices = bondedGoods?.TwBondedGoodsInvoice;
			var bondedGoodsMonthlyReport = bondedGoods?.TwBondedGoodsMonthlyReport;
			var hasBondedGoodsInvoices = bondedGoodsInvoices?.Any() ?? false;
			if (hasBondedGoodsInvoices || bondedGoodsMonthlyReport != null)
			{
				WriteTitleRow(table, Titles.BondedGoodsMonthlyReportTitle, TitleStyle);
				if (hasBondedGoodsInvoices)
				{
					foreach (var bondedGoodsInvoice in bondedGoodsInvoices)
					{
						WriteRowWithTwoCells(table, Titles.BondedGoodsInvoiceID, bondedGoodsInvoice?.TwId?.Value);
						WriteRowWithTwoCells(table, Titles.BondedGoodsInvoiceValueAmount, bondedGoodsInvoice?.TwValueAmount?.Value);
					}
				}
				WriteRowWithTwoCells(table, Titles.BondedGoodsMonthlyReportMonthNumeric, bondedGoodsMonthlyReport?.TwMonthNumeric);
				WriteRowWithTwoCells(table, Titles.BondedGoodsMonthlyReportTraderReferenceID, bondedGoodsMonthlyReport?.TwTraderReferenceId?.Value);
			}

			if (goodsShipment?.AdditionalDocument?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.ResponsibleGovernmentAgencyTitle, HeadStyle);
				foreach (var additionalDocument in goodsShipment.AdditionalDocument)
				{
					WriteTitleRow(table, GetTtile(Titles.AdditionalDocumentID, additionalDocument?.Id?.Value), TitleStyle);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentContent, additionalDocument?.TwContent?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentImageFileFormat, additionalDocument?.TwImageFileFormat?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentImageFileName, additionalDocument?.TwImageFileName?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentSizeMeasure, additionalDocument?.TwSizeMeasure?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentTypeCode, additionalDocument?.TypeCode?.Value);
					WriteRowWithTwoCells(table, Titles.ResponsibleGovernmentAgencyID, additionalDocument?.ResponsibleGovernmentAgency?.Id?.Value);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		public override string ToHtml()
		{
			var table = new HtmlTableCreator(new NameValueCollection { { (NoResString)"border", "0" }, { (NoResString)"cellpadding", "0" }, { "cellspacing", "0" }, { "style", "margin: 5pt;" } }) { EnableHTMLEncoding = false };
			WriteTable(table);
			return table?.ToHtml();
		}

		sealed class Titles
		{
			#region SuppressResourceStringsCheckRegion

			internal const string HeaderTitle = "表頭資料";

			internal const string TransportTitle = "運輸資料";

			internal const string AmountTitle = "金額資料";

			internal const string SummaryTitle = "彙總資料";

			internal const string AdditionalDocumentTypeTitle = "檢附文件號碼資料";

			internal const string StandbyTitle = "備用欄位資料";

			internal const string PartyDetailTitle = "相關單位資料";

			internal const string AgentTitle = "報關業者";

			internal const string ExporterTitle = "出口人(貨物輸出人)";

			internal const string BuyerTitle = "買方";

			internal const string ConsigneeTitle = "收貨人";

			internal const string ConsignorTitle = "發貨人";

			internal const string NotifyPartyTitle = "受通知人";

			internal const string CarrierTitle = "運輸業者";

			internal const string DetailTitle = "明細資料";

			internal const string MainTitle = "主要資料";

			internal const string PackagingTitle = "包裝資料";

			internal const string CommodityNumberTitle = "料號資料";

			internal const string CertificateOfOriginTitle = "產地證明書資料";

			internal const string ImportAndExportLicenseTitle = "輸出入許可文件資料";

			internal const string AdditionalDocumentTitle = "主管機關指定代碼";

			internal const string PreviousDocumentTitle = "原報單號碼";

			internal const string VehicleTitle = "車輛資料";

			internal const string DutyOtherTaxFeeTitle = "其他稅費資料";

			internal const string AdditionalInformationStatementTitle = "備用欄位資料";

			internal const string OtherTitle = "其他資料";

			internal const string ManufacturerTitle = "製造廠商資料";

			internal const string BondedGoodsTitle = "保稅資料";

			internal const string BondedFactoryTitle = "保稅工廠";

			internal const string InBondPartyTitle = "進倉保稅倉庫業者資料";

			internal const string OutBondedPartyTitle = "出倉保稅倉庫業者資料";

			internal const string BluntTaxRebatesTitle = "沖退稅資料";

			internal const string BondedGoodsMonthlyReportTitle = "按月彙報資料";

			internal const string ResponsibleGovernmentAgencyTitle = "檢附文件資料";

			internal const string DeclarationID = "報單號碼：";

			internal const string DeclarationAcceptanceDateTime = "報關日期：";

			internal const string DeclarationTypeCode = "報單類別：";

			internal const string BorderTransportMeansTypeCode = "海空運別：";

			internal const string DeclarationAssociatedGovernmentProcedureCode = "申請審驗方式：";

			internal const string DeclarationFunctionCode = "訊息功能代碼：";

			internal const string DeclarationInvoiceAmount = "發票總金額：";

			internal const string CustomsValuationExitToEntryChargeAmount = "保險費：";

			internal const string CustomsValuationFreightChargeAmount = "運費：";

			internal const string CustomsValuationOtherChargeAmount = "應加費用：";

			internal const string CustomsValuationOtherDeductionAmount = "應減費用：";

			internal const string GoodsShipmentItemChargeAmount = "總離岸價格(新台幣)：";

			internal const string TradeTermsConditionCode = "交易條件代碼：";

			internal const string CurrencyExchangeCurrencyTypeCode = "幣別代碼：";

			internal const string CurrencyExchangeRateNumeric = "外幣匯率：";

			internal const string DutyTaxRateDutyMethodCode = "海關稅費繳納方式代碼：";

			internal const string BorderTransportMeansJourneyID = "船舶航次(海)/航機班次(空)：";

			internal const string BorderTransportMeansCallSignID = "船舶呼號：";

			internal const string BorderTransportMeansRegistration = "海關通關號碼：";

			internal const string DepartureTransportMeansID = "出口船(機)代碼：";

			internal const string DepartureTransportMeansName = "出口船舶名稱：";

			internal const string DepartureTransportMeansTypeCode = "出口運輸方式代碼：";

			internal const string TransportEquipmentCharacteristicCode = "貨櫃種類：";

			internal const string TransportEquipmentID = "貨櫃號碼：";

			internal const string TransportEquipmentUsedCapacityCode = "貨櫃運裝方式：";

			internal const string SealSealID = "封條號碼：";

			internal const string GoodsLocationID = "卸存地點代碼：";

			internal const string LoadingLocationID = "裝貨港代碼：";

			internal const string UnloadingLocationID = "目的地代碼：";

			internal const string DeliveryDestinationName = "運送抵達地：";

			internal const string ItineraryRoutingCountryType = "過境國家代碼：";

			internal const string ConsignmentShippingOrderNumber = "裝貨單號碼：";

			internal const string AirHBL = "空運託運單分號：";

			internal const string SeaMBL = "海運託運單主號：";

			internal const string SeaHBL = "海運託運單分號：";

			internal const string AirMBL = "空運託運單主號：";

			internal const string TransportContractDocumentID = "運送單號碼：";

			internal const string UCRID = "貨物唯一追蹤號碼：";

			internal const string GovernmentProcedureDescription = "其他申報事項：";

			internal const string PackagingMarksNumbers = "標記：";

			internal const string PackagingPackagingMaterialDescription = "包裝說明：";

			internal const string PackagingCombination = "合成註記：";

			internal const string PackagingTypeCode = "件數單位：";

			internal const string DeclarationTotalGrossMassMeasure = "總毛重：";

			internal const string DeclarationTotalPackageQuantity = "總件數：";

			internal const string DeclarationAuthentication = "簽證情形：";

			internal const string AdditionalInformationCopyQuantity = "申請報單複本份數：";

			internal const string AdditionalDocumentID = "檢附文件號碼：";

			internal const string AdditionalInformationStatementCode = "備用欄位代碼：";

			internal const string AdditionalInformationStatementDescription = "備用欄位描述：";

			internal const string AgentID = "報關業者箱號：";

			internal const string AgentSubBoxID = "報關業者箱號附碼：";

			internal const string AgentRoleCode = "報關業者類別代碼：";

			internal const string AgentLPCOAuthorizedPartyID = "報關業者AEO編號：";

			internal const string RepresentativePersonName = "專責報關人員代號：";

			internal const string ExporterID = "出口人(貨物輸出人)統一編號：";

			internal const string ExporterName = "出口人(貨物輸出人)英文名稱：";

			internal const string ExporterCustomsControlID = "出口人(貨物輸出人)海關監管編號：";

			internal const string ExporterPaymentOnAccountBusinessID = "出口人(貨物輸出人)營業稅記帳廠商編號：";

			internal const string ExporterTypeCode = "出口人(貨物輸出人)身分識別代碼：";

			internal const string ExporterAddressLine = "出口人(貨物輸出人)英文地址：";

			internal const string ExporterLPCOAuthorizedPartyID = "出口人(貨物輸出人)AEO號碼：";

			internal const string BuyerID = "買方統一編號：";

			internal const string BuyerName = "買方英文名稱：";

			internal const string BuyerCustomsControlID = "買方海關監管編號：";

			internal const string BuyerTypeCOde = "買方身分識別代碼：";

			internal const string BuyerAddressCountryCode = "買方國家地址：";

			internal const string BuyerAddressLine = "買方英文地址：";

			internal const string BuyerLPCOAuthorizedPartyID = "買方AEO編號：";

			internal const string ConsigneeID = "收貨人代碼：";

			internal const string ConsigneeName = "收貨人英文名稱：";

			internal const string ConsigneeChineseName = "收貨人中文名稱：";

			internal const string ConsigneeTypeCode = "收貨人身分識別代碼：";

			internal const string ConsigneeAddressLine = "收貨人英文地址：";

			internal const string ConsigneeAddressChineseLine = "收貨人中文地址：";

			internal const string ConsignorID = "發貨人代碼：";

			internal const string ConsignorName = "發貨人英文名稱：";

			internal const string ConsignorChineseName = "發貨人中文名稱：";

			internal const string ConsignorTypeCode = "發貨人身分識別代碼：";

			internal const string ConsignorAddressLine = "發貨人英文地址：";

			internal const string ConsignorAddressChineseLine = "發貨人中文地址：";

			internal const string NotifyPartyID = "受通知人代碼：";

			internal const string NotifyPartyName = "受通知人英文名稱：";

			internal const string NotifyPartyChineseName = "受通知人中文名稱：";

			internal const string NotifyPartyTypeCode = "受通知人身分識別代碼：";

			internal const string NotifyPartyAddressLine = "受通知人英文地址：";

			internal const string NotifyPartyAddressChineseLine = "受通知人中文地址：";

			internal const string CarrierID = "運輸業者/代理行代碼：";

			internal const string CarrierTypeCode = "運輸業者/代理行類別代碼：";

			internal const string BondedFactoryCustomsControlID = "保稅工廠海關監管編號：";

			internal const string BondedFactoryID = "保稅工廠統一編號：";

			internal const string BondedFactoryTypeCode = "保稅工廠身分識別代碼：";

			internal const string InBondPartyBondedID = "進倉保稅倉庫業者統一編號：";

			internal const string InBondPartyID = "進倉保稅倉庫代碼：";

			internal const string InBondPartyTypeCode = "進倉保稅倉庫業者身分識別代碼：";

			internal const string OutBondedPartyBondedID = "出倉保稅倉庫業者統一編號：";

			internal const string OutBondedPartyID = "出倉保稅倉庫代碼：";

			internal const string OutBondedPartyTypeCode = "出倉保稅倉庫業者身分識別代碼：";

			internal const string BondedGoodsDocumentCode = "檢附外銷品使用原料及其供應商資料表：";

			internal const string BondedGoodsRefundable = "申請沖退原料稅：";

			internal const string BondedGoodsInvoiceID = "保稅貨品按月彙報之逐批統一發票號碼：";

			internal const string BondedGoodsInvoiceValueAmount = "保稅貨品按月彙報之逐批交易金額：";

			internal const string BondedGoodsMonthlyReportMonthNumeric = "進出口保稅貨物之月份：";

			internal const string BondedGoodsMonthlyReportTraderReferenceID = "保稅貨品按月彙報交易對方向海關申報之參考號碼：";

			internal const string AdditionalDocumentTypeID = "檢附文件字號";

			internal const string AdditionalDocumentContent = "檢附文件說明：";

			internal const string AdditionalDocumentImageFileFormat = "檢附影像文件格式：";

			internal const string AdditionalDocumentImageFileName = "檢附影像文件名稱：";

			internal const string AdditionalDocumentSizeMeasure = "檢附文件檔大小：";

			internal const string AdditionalDocumentTypeCode = "檢附文件種類：";

			internal const string ResponsibleGovernmentAgencyID = "核發機關代碼：";

			internal const string GovernmentAgencyGoodsItemSequenceNumeric = "項次";

			internal const string CommodityComercialCategorizationID = "型號：";

			internal const string CommodityDescription = "貨物名稱：";

			internal const string CommodityName = "商標(牌名)：";

			internal const string CommodityBondedNoteCode = "保稅貨物註記：";

			internal const string GovernmentProcedureCurrentCode = "統計方式代碼：";

			internal const string OriginCountryCode = "生產國別：";

			internal const string ClassificationID = "貨品分類號列：";

			internal const string ConstituentElementDescription = "規格：";

			internal const string InvoiceLineItemChargeAmount = "離岸金額(新台幣)：";

			internal const string InvoiceLineUnitPriceAmount = "單價金額：";

			internal const string GoodsMeasureTariffQuantity = "數量：";

			internal const string GoodsMeasureUnitCode = "數量單位：";

			internal const string GoodsMeasureNetWeightMeasure = "淨重：";

			internal const string GoodsStatisticalMeasureTariffQuantity = "統計數量：";

			internal const string GoodsStatisticalMeasureStatisticalUnitCode = "統計數量單位：";

			internal const string PackagingQuantityQuantiy = "件數：";

			internal const string BuyerCommodityNumberID = "買方料號：";

			internal const string SellerCommodityNumberID = "賣方料號：";

			internal const string CertificateOfOriginID = "產地證明書號碼：";

			internal const string AdditionalDocumentSequenceNumeric = "產地證明書項次：";

			internal const string ImportAndExportLicenseID = "輸出入許可文件號碼：";

			internal const string ImportAndExportLicenseSequenceNumeric = "輸出入許可文件項次：";

			internal const string ResponsibleInstitutionID = "主管機關指定代碼：";

			internal const string PreviousDocumentID = "原進口報單號碼：";

			internal const string PreviousDocumentLineNumeric = "原進口報單項次：";

			internal const string PreBondedDocumentID = "原進倉報單號碼：";

			internal const string PreBondedDocumentLineNumeric = "原進倉報單項次：";

			internal const string VehicleVINID = "車身號碼：";

			internal const string DutyOtherTaxFeeTaxRateNumeric = "其他稅費率：";

			internal const string DutyOtherTaxFeeTypeCode = "其他稅費代碼：";

			internal const string ManufacturerID = "製造廠商統一編號：";

			internal const string ManufacturerName = "製造廠商名稱：";

			internal const string ManufacturerTypeCode = "製造廠商身分識別代碼：";

			internal const string UNClassificationID = "UN危險貨物代碼：";

			internal const string IATAClassificationID = "IATA危險貨物代碼：";

			#endregion
		}
	}
}
