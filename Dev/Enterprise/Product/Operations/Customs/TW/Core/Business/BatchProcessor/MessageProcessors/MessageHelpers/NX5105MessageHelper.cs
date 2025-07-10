using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.NX5105;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX5105MessageHelper : TWMessageHelper
	{
		public NX5105MessageHelper(TWMessage message) : base(message)
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
				WritePartyDetails(table, goodsShipment);
				WriteBondedGoods(table, consignment);
				WriteAttachDocuments(table, goodsShipment);
				WriteDetail(table, goodsShipment);
			}
		}

		#region Declaration

		void WriteMainDetails(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment, DeclarationGoodsShipmentConsignment consignment)
		{
			var dutyTaxFee = Declaration.DutyTaxFee;
			WriteTitleRow(table, Titles.HeaderTitle, FirstHeadStyle);
			WriteMain(table);
			WriteAmountDetails(table, goodsShipment, dutyTaxFee);
			WriteTaxes(table, goodsShipment);
			WriteTransport(table, goodsShipment, consignment);
			WriteSummary(table, consignment, dutyTaxFee);
			WriteStandby(table, consignment);
		}

		void WritePartyDetails(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			WritePartyDetail(table);
			WriteSeller(table, goodsShipment);
			WriteImporter(table);
			WriteConsignee(table, goodsShipment);
			WriteConsignor(table, goodsShipment);
			WriteNotifyParty(table, goodsShipment);
		}

		void WriteBondedGoods(HtmlTableCreator table, DeclarationGoodsShipmentConsignment consignment)
		{
			WriteTitleRow(table, Titles.BondedGoodsTitle, HeadStyle);
			var bondedGoods = consignment?.TwBondedGoods;
			WriteBondedFactory(table, bondedGoods);
			WriteInBondParty(table, bondedGoods);
			WriteOutBondedParty(table, bondedGoods);
			WriteBondedGoodsMonthlyReport(table, bondedGoods);
		}

		void WriteAttachDocuments(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			if (goodsShipment?.AdditionalDocument?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.ResponsibleGovernmentAgencyTitle, HeadStyle);
				foreach (var additionalDocument in goodsShipment.AdditionalDocument)
				{
					WriteTitleRow(table, GetTtile(Titles.AdditionalDocumentTypeID, additionalDocument?.Id?.Value), TitleStyle);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentContent, additionalDocument?.TwContent?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentImageFileFormat, additionalDocument?.TwImageFileFormat?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentImageFileName, additionalDocument?.TwImageFileName?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentSizeMeasure, additionalDocument?.TwSizeMeasure?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalDocumentTypeCode, additionalDocument?.TypeCode?.Value);
					WriteRowWithTwoCells(table, Titles.ResponsibleGovernmentAgencyID, additionalDocument?.ResponsibleGovernmentAgency?.Id?.Value);
				}
			}
		}

		void WriteDetail(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			if (goodsShipment?.GovernmentAgencyGoodsItem?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.DetailTitle, HeadStyle);
				foreach (var governmentAgencyGoodsItem in goodsShipment.GovernmentAgencyGoodsItem)
				{
					var commodity = governmentAgencyGoodsItem.Commodity;
					var classifications = commodity?.Classification;
					WriteGovernmentAgencyGoodsItemSequenceNumeric(table, governmentAgencyGoodsItem, commodity, classifications);
					WriteDutyOtherTaxFee(table, governmentAgencyGoodsItem);
					WriteTaxRate(table, commodity);
					WriteCommodityNumber(table, commodity);
					WriteCertificateOfOrigin(table, governmentAgencyGoodsItem);
					WriteImportAndExportLicense(table, governmentAgencyGoodsItem);
					WriteAdditionalDocument(table, governmentAgencyGoodsItem);
					WritePreviousDocument(table, governmentAgencyGoodsItem, commodity);
					WriteVehicle(table, governmentAgencyGoodsItem);
					WriteAdditionalInformationStatement(table, governmentAgencyGoodsItem);
					WriteOther(table, governmentAgencyGoodsItem, classifications);
				}
			}
		}

		#endregion

		#region MainDetails

		void WriteMain(HtmlTableCreator table)
		{
			WriteTitleRow(table, Titles.MainTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.DeclarationID, Declaration.Id?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationAcceptanceDateTime, Declaration.AcceptanceDateTime);
			WriteRowWithTwoCells(table, Titles.DeclarationTypeCode, Declaration.TypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansTypeCode, Declaration.BorderTransportMeans?.TypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.ArrivalDateTime, Declaration.BorderTransportMeans?.ArrivalDateTime);
			WriteRowWithTwoCells(table, Titles.ExitDateTime, Declaration.GoodsShipment?.ExitDateTime);
			WriteRowWithTwoCells(table, Titles.DeclarationAssociatedGovernmentProcedureCode, Declaration.TwAssociatedGovernmentProcedureCode?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationFunctionCode, Declaration.FunctionCode?.Value);
		}

		void WriteAmountDetails(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment, DeclarationDutyTaxFee dutyTaxFee)
		{
			WriteTitleRow(table, Titles.AmountTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.DeclarationInvoiceAmount, Declaration.InvoiceAmount?.Value);

			var shipmentCustomsValuation = goodsShipment?.CustomsValuation;
			var declarationCurrencyExchange = Declaration.CurrencyExchange;
			WriteRowWithTwoCells(table, Titles.CustomsValuationExitToEntryChargeAmount, shipmentCustomsValuation?.ExitToEntryChargeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.CustomsValuationFreightChargeAmount, shipmentCustomsValuation?.FreightChargeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.CustomsValuationOtherChargeAmount, shipmentCustomsValuation?.TwOtherChargeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.CustomsValuationOtherDeductionAmount, shipmentCustomsValuation?.TwOtherDeductionAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.GoodsShipmentItemChargeAmount, goodsShipment?.TwItemChargeAmount?.Value);
			WriteRowWithTwoCells(table, Titles.GoodsShipmentItemChargeAmountNewTaiwanDollar, goodsShipment?.TwTotalCifAmount?.Value);
			WriteRowWithTwoCells(table, Titles.OtherChargeDeductionAmount, shipmentCustomsValuation?.OtherChargeDeductionAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.TotalDutyTaxFeeAmount, dutyTaxFee?.TwTotalDutyTaxFeeAmount?.Value, true);
			WriteRowWithTwoCells(table, Titles.TradeTermsConditionCode, goodsShipment?.TradeTerms?.ConditionCode?.Value);
			WriteRowWithTwoCells(table, Titles.CurrencyExchangeCurrencyTypeCode, declarationCurrencyExchange?.CurrencyTypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.CurrencyExchangeRateNumeric, declarationCurrencyExchange?.RateNumeric);
			WriteRowWithTwoCells(table, Titles.DutyTaxRateDutyMethodCode, dutyTaxFee?.TwDutyMethodCode?.Value);
			WriteRowWithTwoCells(table, Titles.PartyRelationship, shipmentCustomsValuation?.PartyRelationshipCode?.Value);
		}

		void WriteTaxes(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			if (goodsShipment?.DutyTaxFee?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.TaxesTitle, TitleStyle);
				foreach (var dutyTaxFee in goodsShipment.DutyTaxFee)
				{
					var amount = dutyTaxFee.AdValoremTaxBaseAmount?.Value;
					if (amount != 0)
					{
						WriteRowWithTwoCells(table, Titles.DutyTaxFeeTypeCode, dutyTaxFee.TypeCode?.Value, CellBoldStyle);
						WriteRowWithTwoCells(table, Titles.AdValoremTaxBaseAmount, amount);
					}
				}
			}
		}

		void WriteTransport(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment, DeclarationGoodsShipmentConsignment consignment)
		{
			WriteTitleRow(table, Titles.TransportTitle, TitleStyle);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansJourneyID, consignment?.BorderTransportMeans?.JourneyId?.Value);
			WriteRowWithTwoCells(table, Titles.BorderTransportMeansRegistration, consignment?.BorderTransportMeans?.TwRegistration?.Value);
			WriteRowWithTwoCells(table, Titles.DepartureTransportMeansID, consignment?.BorderTransportMeans?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.ArrivalTransportMeansTypeCode, consignment?.ArrivalTransportMeans?.TypeCode?.Value);

			consignment?.TransportEquipment?.Where(x => x != null).ForEach(x =>
			{
				WriteRowWithTwoCells(table, Titles.TransportEquipmentCharacteristicCode, x?.CharacteristicCode?.Value);
				WriteRowWithTwoCells(table, Titles.TransportEquipmentID, x?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.TransportEquipmentUsedCapacityCode, x?.TwUsedCapacityCode?.Value);
				x.TwSeal?.Where(y => y != null).ForEach(y => WriteRowWithTwoCells(table, Titles.SealSealID, y?.TwSealId?.Value));
			});

			WriteRowWithTwoCells(table, Titles.GoodsLocationID, consignment?.GoodsLocation?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.LoadingLocationID, consignment?.LoadingLocation?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.ManifestSerialNumber, consignment?.TwManifestSerialNumber?.Value);

			var transportContractDocument = consignment?.TransportContractDocument;
			if (transportContractDocument?.Any() ?? false)
			{
				WriteRowWithTwoCells(table, Titles.SeaMBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._704)?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.SeaHBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._714)?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.AirMBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._741)?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.AirHBL, transportContractDocument?.FirstOrDefault(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._703)?.Id?.Value);
				foreach (var container in transportContractDocument?.Where(x => x.TypeCode?.Value == MessageConstants.TransportContractDocumentTypeCodes._976))
				{
					WriteRowWithTwoCells(table, Titles.TransportContractDocumentID, container?.Id?.Value);
				}
			}
			WriteRowWithTwoCells(table, Titles.UCRID, goodsShipment?.Ucr?.Id?.Value);
		}

		void WriteSummary(HtmlTableCreator table, DeclarationGoodsShipmentConsignment consignment, DeclarationDutyTaxFee dutyTaxFee)
		{
			WriteTitleRow(table, Titles.SummaryTitle, TitleStyle);

			var declarationPackaging = Declaration.Packaging;
			WriteRowWithTwoCells(table, Titles.GovernmentProcedureDescription, Declaration.GovernmentProcedure?.Select(x => x.Description.Value));
			WriteRowWithTwoCells(table, Titles.PackagingMarksNumbers, declarationPackaging?.MarksNumbers?.Value);
			WriteRowWithTwoCells(table, Titles.PackagingPackagingMaterialDescription, declarationPackaging?.PackagingMaterialDescription?.Value);
			WriteRowWithTwoCells(table, Titles.PackagingCombination, declarationPackaging?.TwCombination?.Value);
			WriteRowWithTwoCells(table, Titles.ConsignmentSplit, consignment?.ConsignmentItem?.TwSplit?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationTotalGrossMassMeasure, Declaration.TotalGrossMassMeasure?.Value);
			WriteRowWithTwoCells(table, Titles.DeclarationTotalPackageQuantity, Declaration.TotalPackageQuantity?.Value);
			WriteRowWithTwoCells(table, Titles.PackagingTypeCode, declarationPackaging?.TypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.DutyExemptionWaiverNote, dutyTaxFee?.TwDutyExemptionWaiverNote?.Value);
			WriteRowWithTwoCells(table, Titles.DutyMemoPrinted, dutyTaxFee?.TwDutyMemoPrinted?.Value);
			WriteRowWithTwoCells(table, Titles.ObligationGuaranteeReferenceID, dutyTaxFee?.Payment?.ObligationGuarantee?.ReferenceId?.Value);
			WriteRowWithTwoCells(table, Titles.AdditionalInformationCopyQuantity, Declaration.AdditionalInformation?.TwCopyQuantity?.Value);
		}

		void WriteStandby(HtmlTableCreator table, DeclarationGoodsShipmentConsignment consignment)
		{
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

		#endregion

		#region PartyDetails

		void WritePartyDetail(HtmlTableCreator table)
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
				WriteRowWithTwoCells(table, Titles.RepresentativePersonName, representativePerson?.Name?.Value);
			}
		}

		void WriteSeller(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			var seller = goodsShipment?.Seller;
			if (seller != null)
			{
				var sellerAddress = seller.Address;
				WriteTitleRow(table, Titles.SellerTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.SellerID, seller.Id?.Value);
				WriteRowWithTwoCells(table, Titles.SellerEnglishName, seller.Name?.Value);
				WriteRowWithTwoCells(table, Titles.SellerChineseName, seller.TwChineseName?.Value);
				WriteRowWithTwoCells(table, Titles.SellerCustomsControlID, seller.TwCustomsControlId?.Value);
				WriteRowWithTwoCells(table, Titles.SellerTypeCode, seller.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.SellerCountryCode, sellerAddress?.CountryCode?.Value);
				WriteRowWithTwoCells(table, Titles.SellerAddressLine, sellerAddress?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.SellerAddressChineseLine, sellerAddress?.TwChineseLine?.Value);
				WriteRowWithTwoCells(table, Titles.SellerCommunicationID, seller.Communication?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.SellerContactName, seller.Contact?.Name?.Value);
				WriteRowWithTwoCells(table, Titles.SellerLPCOAuthorizedPartyID, seller.LpcoAuthorizedParty?.Id?.Value);
			}
		}

		void WriteImporter(HtmlTableCreator table)
		{
			var importer = Declaration.Importer;
			if (importer != null)
			{
				WriteTitleRow(table, Titles.ImporterTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.ImpoterID, importer.Id?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterEngilshName, importer.Name?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterChineseName, importer.TwChineseName?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterCustomsControlID, importer.TwCustomsControlId?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterPaymentOnAccountBusinessID, importer.TwPaymentOnAccountBusinessId?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterTypeCode, importer.TwTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterAddressLine, importer.Address?.Line?.Value);
				WriteRowWithTwoCells(table, Titles.ImpoterAddressChineseLine, importer.Address?.TwChineseLine?.Value);
				var communication = importer.Communication;
				if (communication?.Any() ?? false)
				{
					WriteRowWithTwoCells(table, Titles.ImpoterCommunicationPhoneNumber, communication.FirstOrDefault(x => x.TypeId?.Value == MessageConstants.CommunicationTypeIDs.TE)?.Id?.Value);
					WriteRowWithTwoCells(table, Titles.ImpoterCommunicationEmail, communication.FirstOrDefault(x => x.TypeId?.Value == MessageConstants.CommunicationTypeIDs.MA)?.Id?.Value);
				}
				WriteRowWithTwoCells(table, Titles.ImpoterLPCOAuthorizedPartyID, importer.LpcoAuthorizedParty?.Id?.Value);
			}
		}

		void WriteConsignee(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
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
		}

		void WriteConsignor(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
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
		}

		void WriteNotifyParty(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
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
		}

		#endregion

		#region BondedGoods

		void WriteBondedFactory(HtmlTableCreator table, DeclarationGoodsShipmentConsignmentTwBondedGoods bondedGoods)
		{
			if (bondedGoods?.TwPreBondedParty?.Any() ?? false)
			{
				foreach (var preBondedParty in bondedGoods.TwPreBondedParty)
				{
					WriteTitleRow(table, Titles.BondedFactoryTitle, TitleStyle);
					WriteRowWithTwoCells(table, Titles.BondedFactoryCustomsControlID, preBondedParty?.TwCustomsControlId?.Value);
					WriteRowWithTwoCells(table, Titles.BondedFactoryID, preBondedParty?.TwId?.Value);
					WriteRowWithTwoCells(table, Titles.BondedFactoryTypeCode, preBondedParty?.TwTypeCode?.Value);
				}
			}
		}

		void WriteInBondParty(HtmlTableCreator table, DeclarationGoodsShipmentConsignmentTwBondedGoods bondedGoods)
		{
			var inBondedParty = bondedGoods?.TwInBondedParty;
			if (inBondedParty != null)
			{
				WriteTitleRow(table, Titles.InBondPartyTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.InBondPartyBondedID, inBondedParty.TwBondedId?.Value);
				WriteRowWithTwoCells(table, Titles.InBondPartyID, inBondedParty.TwId?.Value);
				WriteRowWithTwoCells(table, Titles.InBondPartyTypeCode, inBondedParty.TwTypeCode?.Value);
			}
		}

		void WriteOutBondedParty(HtmlTableCreator table, DeclarationGoodsShipmentConsignmentTwBondedGoods bondedGoods)
		{
			var outBondedParty = bondedGoods?.TwOutBondedParty;
			if (outBondedParty != null)
			{
				WriteTitleRow(table, Titles.OutBondedPartyTitle, TitleStyle);
				WriteRowWithTwoCells(table, Titles.OutBondedPartyBondedID, outBondedParty.TwBondedId?.Value);
				WriteRowWithTwoCells(table, Titles.OutBondedPartyID, outBondedParty.TwId?.Value);
				WriteRowWithTwoCells(table, Titles.OutBondedPartyTypeCode, outBondedParty.TwTypeCode?.Value);
			}
		}

		void WriteBondedGoodsMonthlyReport(HtmlTableCreator table, DeclarationGoodsShipmentConsignmentTwBondedGoods bondedGoods)
		{
			var bondedGoodsInvoices = bondedGoods?.TwBondedGoodsInvoice;
			var bondedGoodsMonthlyReport = bondedGoods?.TwBondedGoodsMonthlyReport;
			var hasBondedGoodsInvoices = bondedGoodsInvoices?.Any() ?? false;
			var bondedGoodsAddDutyReasonCode = bondedGoods?.TwAddDutyReasonCode;
			if (bondedGoodsMonthlyReport != null || hasBondedGoodsInvoices || bondedGoodsAddDutyReasonCode != null)
			{
				WriteTitleRow(table, Titles.BondedGoodsMonthlyReportTitle, TitleStyle);
				if (hasBondedGoodsInvoices)
				{
					WriteRowWithTwoCells(table, Titles.BondedGoodsAddDutyReasonCode, bondedGoodsAddDutyReasonCode?.Value);
					foreach (var bondedGoodsInvoice in bondedGoodsInvoices)
					{
						WriteRowWithTwoCells(table, Titles.BondedGoodsInvoiceID, bondedGoodsInvoice?.TwId?.Value);
						WriteRowWithTwoCells(table, Titles.BondedGoodsInvoiceValueAmount, bondedGoodsInvoice?.TwValueAmount?.Value);
					}
				}
				if (bondedGoodsMonthlyReport != null)
				{
					WriteRowWithTwoCells(table, Titles.BondedGoodsMonthlyReportMonthNumeric, bondedGoodsMonthlyReport.TwMonthNumeric);
					WriteRowWithTwoCells(table, Titles.BondedGoodsMonthlyReportTraderReferenceID, bondedGoodsMonthlyReport.TwTraderReferenceId?.Value);
				}
			}
		}

		#endregion

		#region governmentAgencyGoodsItem

		void WriteGovernmentAgencyGoodsItemSequenceNumeric(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem, DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> classifications)
		{
			WriteTitleRow(table, GetTtile(Titles.GovernmentAgencyGoodsItemSequenceNumeric, governmentAgencyGoodsItem.SequenceNumeric.ToString()), TitleStyle);
			WriteTitleRow(table, Titles.MainTitle, SubTitleStyle);

			var commodityDutyTaxFee = commodity?.DutyTaxFee;
			var goodsMeasure = governmentAgencyGoodsItem?.GoodsMeasure;
			var commodityInvoiceLine = commodity?.InvoiceLine;
			var twGoodsStatisticalMeasure = governmentAgencyGoodsItem?.TwGoodsStatisticalMeasure;
			WriteRowWithTwoCells(table, Titles.CommodityComercialCategorizationID, commodity?.CommercialCategorizationId?.Value);
			WriteRowWithTwoCells(table, Titles.CommodityName, commodity?.Name?.Value);
			WriteRowWithTwoCells(table, Titles.ConstituentElementDescription, commodity?.Constituent?.ElementDescription?.Value);
			WriteRowWithTwoCells(table, Titles.CommodityDescription, commodity?.Description?.Value);
			WriteRowWithTwoCells(table, Titles.CommodityChineseDescription, commodity?.TwChineseDescription?.Value);
			WriteRowWithTwoCells(table, Titles.CommodityEnglishDescription, commodity?.TwEnglishDescription?.Value);
			WriteRowWithTwoCells(table, Titles.CITESImportPermitID, commodity?.TwCitesImportPermitId?.Value);
			WriteRowWithTwoCells(table, Titles.FTATariffCode, commodity?.TwFtaTariffCode?.Value);
			WriteRowWithTwoCells(table, Titles.SHTCImportPermitID, commodity?.TwShtcImportPermitId?.Value);
			WriteRowWithTwoCells(table, Titles.OriginCountryCode, governmentAgencyGoodsItem?.Origin?.CountryCode?.Value);
			WriteRowWithTwoCells(table, Titles.ClassificationID, classifications?.FirstOrDefault(x => x.IdentificationTypeCode.Value == MessageConstants.IdentificationTypeCodes.HS)?.Id?.Value);
			WriteRowWithTwoCells(table, Titles.DutyRegimeCode, commodityDutyTaxFee?.DutyRegimeCode?.Value);
			WriteRowWithTwoCells(table, Titles.GovernmentProcedureCurrentCode, commodity?.GovernmentProcedure?.CurrentCode?.Value);
			WriteRowWithTwoCells(table, Titles.InvoiceLineChargesTypeCode, commodityInvoiceLine?.TwChargesTypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.InvoiceLineCurrencyTypeCode, commodityInvoiceLine?.TwCurrencyTypeCode?.Value);
			WriteRowWithTwoCells(table, Titles.InvoiceLineUnitPriceAmount, commodityInvoiceLine?.TwUnitPriceAmount?.Value);
			WriteRowWithTwoCells(table, Titles.GovernmentAgencyGoodsItemAdValoremTaxBaseAmount, commodityDutyTaxFee?.AdValoremTaxBaseAmount?.Value);
			WriteRowWithTwoCells(table, Titles.GoodsMeasureTariffQuantity, goodsMeasure?.TariffQuantity?.Value);
			WriteRowWithTwoCells(table, Titles.GoodsMeasureUnitCode, goodsMeasure?.TwUnitCode?.Value);
			WriteRowWithTwoCells(table, Titles.CommodityDutyTaxFeeSpecificTaxBaseQuantity, commodityDutyTaxFee?.SpecificTaxBaseQuantity?.Value);
			WriteRowWithTwoCells(table, Titles.CommodityDutyTaxFeePercentageNumeric, commodityDutyTaxFee?.TwPercentageNumeric);
			WriteRowWithTwoCells(table, Titles.GoodsMeasureNetWeightMeasure, goodsMeasure?.NetWeightMeasure?.Value);
			WriteRowWithTwoCells(table, Titles.GoodsStatisticalMeasureStatisticalUnitCode, twGoodsStatisticalMeasure?.TwStatisticalUnitCode?.Value);
			WriteRowWithTwoCells(table, Titles.GoodsStatisticalMeasureTariffQuantity, twGoodsStatisticalMeasure?.TwTariffQuantity?.Value);
			WriteRowWithTwoCells(table, Titles.AlcoholContentNumeric, commodity?.TwWine?.TwAlcoholContentNumeric);
		}

		void WriteDutyOtherTaxFee(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem)
		{
			if (governmentAgencyGoodsItem.Commodity?.TwDutyOtherTaxFee?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.DutyOtherTaxFeeTitle, SubTitleStyle);
				foreach (var dutyOtherTaxFee in governmentAgencyGoodsItem.Commodity.TwDutyOtherTaxFee)
				{
					WriteRowWithTwoCells(table, Titles.DutyOtherTaxFeeTypeCode, dutyOtherTaxFee?.TwTypeCode?.Value, CellBoldStyle);
					WriteRowWithTwoCells(table, Titles.DutyOtherTaxFeeMethodCode, dutyOtherTaxFee?.TwMethodCode?.Value);
					WriteRowWithTwoCells(table, Titles.DutyOtherTaxFeePercentageNumeric, dutyOtherTaxFee?.TwPercentageNumeric);
					WriteRowWithTwoCells(table, Titles.DutyOtherTaxFeeTaxRateNumeric, dutyOtherTaxFee?.TwTaxRateNumeric);
				}
			}
		}

		void WriteTaxRate(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity)
		{
			var dutyTaxFeeAmount = commodity?.TwDutyTaxFeeAmount;
			var dutyTaxFeeQuantity = commodity?.TwDutyTaxFeeQuantity;
			if (dutyTaxFeeAmount != null || dutyTaxFeeQuantity != null)
			{
				WriteTitleRow(table, Titles.TaxRate, SubTitleStyle);
				WriteRowWithTwoCells(table, Titles.ImportTaxRateNumeric, dutyTaxFeeAmount?.TwTaxRateNumeric, true);
				WriteRowWithTwoCells(table, Titles.ImportPercentageNumeric, dutyTaxFeeAmount?.TwPercentageNumeric);
				WriteRowWithTwoCells(table, Titles.DutyTaxFeeQuantityDutyUnitCode, dutyTaxFeeQuantity?.TwDutyUnitCode?.Value);
				WriteRowWithTwoCells(table, Titles.DutyTaxFeeQuantityPercentageNumeric, dutyTaxFeeQuantity?.TwPercentageNumeric);
				WriteRowWithTwoCells(table, Titles.DutyTaxFeeQuantityTaxRateNumeric, dutyTaxFeeQuantity?.TwTaxRateNumeric);
			}
		}

		void WriteCommodityNumber(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity)
		{
			var commodityNumber = commodity?.TwCommodityNumber;
			if (commodityNumber?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.CommodityNumberTitle, SubTitleStyle);
				WriteRowWithTwoCells(table, Titles.BuyerCommodityNumberID, commodityNumber.FirstOrDefault(x => x.TwIdentifierTypeCode?.Value == MessageConstants.IdentificationTypeCodes.BP)?.TwId?.Value);
				WriteRowWithTwoCells(table, Titles.SellerCommodityNumberID, commodityNumber.FirstOrDefault(x => x.TwIdentifierTypeCode?.Value == MessageConstants.IdentificationTypeCodes.SA)?.TwId?.Value);
			}
		}

		void WriteCertificateOfOrigin(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem)
		{
			var originAdditionalDocument = governmentAgencyGoodsItem?.Origin?.AdditionalDocument;
			if (originAdditionalDocument != null)
			{
				WriteTitleRow(table, Titles.CertificateOfOriginTitle, SubTitleStyle);
				WriteRowWithTwoCells(table, Titles.CertificateOfOriginID, originAdditionalDocument.Id?.Value);
				WriteRowWithTwoCells(table, Titles.AdditionalDocumentSequenceNumeric, originAdditionalDocument.TwSequenceNumeric);
			}
		}

		void WriteImportAndExportLicense(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem)
		{
			if (governmentAgencyGoodsItem?.AdditionalDocument?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.ImportAndExportLicenseTitle, SubTitleStyle);
				foreach (var additionalDocument in governmentAgencyGoodsItem.AdditionalDocument)
				{
					WriteRowWithTwoCells(table, Titles.ImportAndExportLicenseID, additionalDocument?.Id?.Value, CellBoldStyle);
					WriteRowWithTwoCells(table, Titles.ImportAndExportLicenseSequenceNumeric, additionalDocument?.TwSequenceNumeric);
				}
			}
		}

		void WriteAdditionalDocument(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem)
		{
			if (governmentAgencyGoodsItem.Commodity.AdditionalDocument?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.AdditionalDocumentTitle, SubTitleStyle);
				foreach (var additionalDocument in governmentAgencyGoodsItem.Commodity.AdditionalDocument)
				{
					WriteRowWithTwoCells(table, Titles.CommodityAdditionalDocumentID, additionalDocument?.Id?.Value);
				}
			}
		}

		void WritePreviousDocument(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem, DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity)
		{
			var previousDocument = governmentAgencyGoodsItem?.PreviousDocument;
			var preBondedDocument = governmentAgencyGoodsItem?.TwPreBondedDocument;
			var commodityPreBondedDocument = commodity?.PreviousDocument;
			if (previousDocument != null || preBondedDocument != null || commodityPreBondedDocument != null)
			{
				WriteTitleRow(table, Titles.PreviousDocumentTitle, SubTitleStyle);
				WriteRowWithTwoCells(table, Titles.PreviousExportDocumentID, previousDocument?.Id?.Value);
				WriteRowWithTwoCells(table, Titles.PreviousExportDocumentLineNumeric, previousDocument?.LineNumeric);
				WriteRowWithTwoCells(table, Titles.PreBondedDocumentID, preBondedDocument?.TwId?.Value);
				WriteRowWithTwoCells(table, Titles.PreBondedDocumentLineNumeric, preBondedDocument?.TwLineNumeric);
				WriteRowWithTwoCells(table, Titles.CommodityPreviousDocumentID, commodityPreBondedDocument?.Id?.Value);
			}
		}

		void WriteVehicle(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem)
		{
			var vehicle = governmentAgencyGoodsItem.Commodity?.TwVehicle;
			if (vehicle != null)
			{
				WriteTitleRow(table, Titles.VehicleTitle, SubTitleStyle);
				WriteRowWithTwoCells(table, Titles.VehicleCatalyst, vehicle.TwCatalyst?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleClassificationCode, vehicle.TwClassificationCode?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleCylinderQuantity, vehicle.TwCylinderQuantity?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleDisplaceQuantity, vehicle.TwDisplaceQuantity?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleDoorQuantity, vehicle.TwDoorQuantity?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleDrivingSide, vehicle.TwDrivingSide?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleFuelTypeCode, vehicle.TwFuelTypeCode?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleModelYearNumeric, vehicle.TwModelYearNumeric);
				WriteRowWithTwoCells(table, Titles.VehicleSeatQuantity, vehicle.TwSeatQuantity?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleStatusCode, vehicle.TwStatusCode?.Value);
				WriteRowWithTwoCells(table, Titles.VehicleTransmissionTypeCode, vehicle.TwTransmissionTypeCode?.Value);
				vehicle.TwVehicleId?.Where(y => y != null).ForEach(y =>
				{
					WriteRowWithTwoCells(table, Titles.VehicleVINID, y.TwVinid?.Value);
				});
			}
		}

		void WriteAdditionalInformationStatement(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem)
		{
			if (governmentAgencyGoodsItem.AdditionalInformation?.Any() ?? false)
			{
				WriteTitleRow(table, Titles.AdditionalInformationStatementTitle, SubTitleStyle);
				foreach (var additionalInformation in governmentAgencyGoodsItem.AdditionalInformation)
				{
					WriteRowWithTwoCells(table, Titles.AdditionalInformationStatementCode, additionalInformation?.StatementCode?.Value);
					WriteRowWithTwoCells(table, Titles.AdditionalInformationStatementDescription, additionalInformation?.StatementDescription?.Value);
				}
			}
		}

		void WriteOther(HtmlTableCreator table, DeclarationGoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem, Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> classifications)
		{
			if (governmentAgencyGoodsItem.Commodity?.Classification?.Any() ?? false)
			{
				var unClassificationID = classifications.FirstOrDefault(x => x.IdentificationTypeCode?.Value == MessageConstants.IdentificationTypeCodes.SSO)?.Id?.Value;
				var iataClassificationID = classifications.FirstOrDefault(x => x.IdentificationTypeCode?.Value == MessageConstants.IdentificationTypeCodes.ZZZ)?.Id?.Value;

				if (!string.IsNullOrEmpty(unClassificationID) || !string.IsNullOrEmpty(iataClassificationID))
				{
					WriteTitleRow(table, Titles.OtherTitle, SubTitleStyle);
					WriteRowWithTwoCells(table, Titles.UNClassificationID, unClassificationID);
					WriteRowWithTwoCells(table, Titles.IATAClassificationID, iataClassificationID);
				}
			}
		}

		#endregion

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

			internal const string TaxesTitle = "稅費資料";

			internal const string SummaryTitle = "彙總資料";

			internal const string StandbyTitle = "備用欄位資料";

			internal const string PartyDetailTitle = "相關單位資料";

			internal const string AgentTitle = "報關業者";

			internal const string SellerTitle = "出口人";

			internal const string ImporterTitle = "進口人";

			internal const string ConsigneeTitle = "收貨人";

			internal const string ConsignorTitle = "發貨人";

			internal const string NotifyPartyTitle = "受通知人";

			internal const string DetailTitle = "明細資料";

			internal const string MainTitle = "主要資料";

			internal const string CommodityNumberTitle = "料號資料";

			internal const string CertificateOfOriginTitle = "產地證明書資料";

			internal const string ImportAndExportLicenseTitle = "輸出入許可文件資料";

			internal const string AdditionalDocumentTitle = "主管機關指定代碼";

			internal const string PreviousDocumentTitle = "原報單號碼";

			internal const string VehicleTitle = "車輛資料";

			internal const string DutyOtherTaxFeeTitle = "其他稅費資料";

			internal const string TaxRate = "稅率";

			internal const string AdditionalInformationStatementTitle = "備用欄位資料";

			internal const string OtherTitle = "其他資料";

			internal const string BondedGoodsTitle = "保稅資料";

			internal const string BondedFactoryTitle = "保稅工廠";

			internal const string InBondPartyTitle = "進倉保稅倉庫業者資料";

			internal const string OutBondedPartyTitle = "出倉保稅倉庫業者資料";

			internal const string BondedGoodsMonthlyReportTitle = "按月彙報資料";

			internal const string ResponsibleGovernmentAgencyTitle = "檢附文件資料";

			internal const string DeclarationID = "報單號碼：";

			internal const string DeclarationAcceptanceDateTime = "報關日期：";

			internal const string DeclarationTypeCode = "報單類別：";

			internal const string BorderTransportMeansTypeCode = "海空運別：";

			internal const string ArrivalDateTime = "進口日期：";

			internal const string ExitDateTime = "國外出口日期：";

			internal const string DeclarationAssociatedGovernmentProcedureCode = "申請審驗方式：";

			internal const string DeclarationFunctionCode = "訊息功能代碼：";

			internal const string DeclarationInvoiceAmount = "發票總金額：";

			internal const string CustomsValuationExitToEntryChargeAmount = "保險費：";

			internal const string CustomsValuationFreightChargeAmount = "運費：";

			internal const string CustomsValuationOtherChargeAmount = "應加費用：";

			internal const string CustomsValuationOtherDeductionAmount = "應減費用：";

			internal const string GoodsShipmentItemChargeAmount = "總離岸價格：";

			internal const string GoodsShipmentItemChargeAmountNewTaiwanDollar = "總起岸價格(新台幣)：";

			internal const string OtherChargeDeductionAmount = "營業稅稅基：";

			internal const string TotalDutyTaxFeeAmount = "稅費合計：";

			internal const string TradeTermsConditionCode = "交易條件代碼：";

			internal const string CurrencyExchangeCurrencyTypeCode = "幣別代碼：";

			internal const string CurrencyExchangeRateNumeric = "外幣匯率：";

			internal const string DutyTaxRateDutyMethodCode = "海關稅費繳納方式代碼：";

			internal const string PartyRelationship = "特殊關係：";

			internal const string AdValoremTaxBaseAmount = "稅費金額：";

			internal const string DutyTaxFeeTypeCode = "稅費代號：";

			internal const string BorderTransportMeansJourneyID = "船舶航次(海)/航機班次(空)：";

			internal const string BorderTransportMeansRegistration = "海關通關號碼：";

			internal const string DepartureTransportMeansID = "船(機)代碼：";

			internal const string ArrivalTransportMeansTypeCode = "進口運輸方式代碼：";

			internal const string TransportEquipmentCharacteristicCode = "貨櫃種類：";

			internal const string TransportEquipmentID = "貨櫃號碼：";

			internal const string TransportEquipmentUsedCapacityCode = "貨櫃運裝方式：";

			internal const string SealSealID = "封條號碼：";

			internal const string GoodsLocationID = "卸存地點代碼：";

			internal const string LoadingLocationID = "裝貨港代碼：";

			internal const string ManifestSerialNumber = "艙單號碼：";

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

			internal const string ConsignmentSplit = "分批註記：";

			internal const string PackagingTypeCode = "件數單位：";

			internal const string DutyExemptionWaiverNote = "排除低價免稅註記：";

			internal const string DutyMemoPrinted = "申請稅單列印：";

			internal const string ObligationGuaranteeReferenceID = "先放後稅核准之案號：";

			internal const string DeclarationTotalGrossMassMeasure = "總毛重：";

			internal const string DeclarationTotalPackageQuantity = "總件數：";

			internal const string AdditionalInformationCopyQuantity = "申請報單副本份數：";

			internal const string CommodityAdditionalDocumentID = "主管機關指定代號：";

			internal const string AdditionalInformationStatementCode = "備用欄位代碼：";

			internal const string AdditionalInformationStatementDescription = "備用欄位描述：";

			internal const string AgentID = "報關業者箱號：";

			internal const string AgentSubBoxID = "報關業者箱號附碼：";

			internal const string AgentRoleCode = "報關業者類別代碼：";

			internal const string AgentLPCOAuthorizedPartyID = "報關業者AEO編號：";

			internal const string RepresentativePersonName = "專責報關人員代號：";

			internal const string SellerID = "出口人(或賣方)統一編號或國外廠商英文名稱縮寫：";

			internal const string SellerEnglishName = "出口人(或賣方)英文名稱：";

			internal const string SellerChineseName = "出口人(或賣方)中文名稱：";

			internal const string SellerCustomsControlID = "海關監管編號：";

			internal const string SellerTypeCode = "身分識別代碼：";

			internal const string SellerCountryCode = "出口人(或賣方)國家代碼：";

			internal const string SellerAddressLine = "出口人(或賣方)英文地址：";

			internal const string SellerAddressChineseLine = "出口人(或賣方)中文地址：";

			internal const string SellerCommunicationID = "出口人(或賣方)電話：";

			internal const string SellerContactName = "出口人(或賣方)聯絡人：";

			internal const string SellerLPCOAuthorizedPartyID = "AEO編號：";

			internal const string ImpoterID = "進口人(納稅義務人)統一編號：";

			internal const string ImpoterEngilshName = "進口人(納稅義務人)英文名稱：";

			internal const string ImpoterChineseName = "進口人(納稅義務人)中文名稱：";

			internal const string ImpoterCustomsControlID = "海關監管編號：";

			internal const string ImpoterPaymentOnAccountBusinessID = "營業稅記帳廠商編號：";

			internal const string ImpoterTypeCode = "身分識別代碼：";

			internal const string ImpoterAddressLine = "進口人(納稅義務人)英文地址：";

			internal const string ImpoterAddressChineseLine = "進口人(納稅義務人)中文地址：";

			internal const string ImpoterCommunicationPhoneNumber = "進口人(納稅義務人)電話：";

			internal const string ImpoterCommunicationEmail = "進口人(納稅義務人)電子郵件：";

			internal const string ImpoterLPCOAuthorizedPartyID = "AEO編號：";

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

			internal const string BondedFactoryCustomsControlID = "保稅工廠海關監管編號：";

			internal const string BondedFactoryID = "保稅工廠統一編號：";

			internal const string BondedFactoryTypeCode = "保稅工廠身分識別代碼：";

			internal const string InBondPartyBondedID = "進倉保稅倉庫業者統一編號：";

			internal const string InBondPartyID = "進倉保稅倉庫代碼：";

			internal const string InBondPartyTypeCode = "進倉保稅倉庫業者身分識別代碼：";

			internal const string OutBondedPartyBondedID = "出倉保稅倉庫業者統一編號：";

			internal const string OutBondedPartyID = "出倉保稅倉庫代碼：";

			internal const string OutBondedPartyTypeCode = "出倉保稅倉庫業者身分識別代碼：";

			internal const string BondedGoodsAddDutyReasonCode = "保稅貨物內銷補稅原因代碼：";

			internal const string BondedGoodsInvoiceID = "統一發票號碼：";

			internal const string BondedGoodsInvoiceValueAmount = "交易金額：";

			internal const string BondedGoodsMonthlyReportMonthNumeric = "月份：";

			internal const string BondedGoodsMonthlyReportTraderReferenceID = "交易對方參考編號：";

			internal const string AdditionalDocumentTypeID = "檢附文件字號";

			internal const string AdditionalDocumentContent = "檢附文件說明：";

			internal const string AdditionalDocumentImageFileFormat = "檢附影像文件格式：";

			internal const string AdditionalDocumentImageFileName = "檢附影像文件名稱：";

			internal const string AdditionalDocumentSizeMeasure = "檢附文件檔大小：";

			internal const string AdditionalDocumentTypeCode = "檢附文件種類：";

			internal const string ResponsibleGovernmentAgencyID = "核發機關代碼：";

			internal const string GovernmentAgencyGoodsItemSequenceNumeric = "項次";

			internal const string CommodityComercialCategorizationID = "型號：";

			internal const string CommodityName = "商標(牌名)：";

			internal const string ConstituentElementDescription = "規格：";

			internal const string CommodityDescription = "貨物名稱：";

			internal const string CommodityChineseDescription = "貨物中文名稱(品名)：";

			internal const string CommodityEnglishDescription = "貨物英文名稱(品名)：";

			internal const string CITESImportPermitID = "華盛頓公約進口許可證號碼：";

			internal const string FTATariffCode = "自由貿易協定優惠關稅註記：";

			internal const string SHTCImportPermitID = "戰略性高科技貨品國際進口證明號碼：";

			internal const string OriginCountryCode = "生產國別代碼：";

			internal const string ClassificationID = "貨品分類號列：";

			internal const string DutyRegimeCode = "稅則增註：";

			internal const string GovernmentProcedureCurrentCode = "納稅辦法代碼：";

			internal const string InvoiceLineChargesTypeCode = "單價條件：";

			internal const string InvoiceLineCurrencyTypeCode = "單價幣別代碼：";

			internal const string InvoiceLineUnitPriceAmount = "單價金額：";

			internal const string GovernmentAgencyGoodsItemAdValoremTaxBaseAmount = "完稅價格：";

			internal const string GoodsMeasureTariffQuantity = "數量：";

			internal const string GoodsMeasureUnitCode = "數量單位：";

			internal const string CommodityDutyTaxFeeSpecificTaxBaseQuantity = "完稅數量：";

			internal const string CommodityDutyTaxFeePercentageNumeric = "折算率：";

			internal const string GoodsMeasureNetWeightMeasure = "淨重：";

			internal const string GoodsStatisticalMeasureTariffQuantity = "統計數量：";

			internal const string GoodsStatisticalMeasureStatisticalUnitCode = "統計數量單位：";

			internal const string AlcoholContentNumeric = "酒精成分：";

			internal const string BuyerCommodityNumberID = "買方料號：";

			internal const string SellerCommodityNumberID = "賣方料號：";

			internal const string CertificateOfOriginID = "產地證明書號碼：";

			internal const string AdditionalDocumentSequenceNumeric = "產地證明書項次：";

			internal const string ImportAndExportLicenseID = "輸出入許可文件號碼：";

			internal const string ImportAndExportLicenseSequenceNumeric = "輸出入許可文件項次：";

			internal const string PreviousExportDocumentID = "原出口報單號碼：";

			internal const string PreviousExportDocumentLineNumeric = "原出口報單項次：";

			internal const string PreBondedDocumentID = "原進倉報單號碼：";

			internal const string PreBondedDocumentLineNumeric = "原進倉報單項次：";

			internal const string CommodityPreviousDocumentID = "原輸出入許可文件號碼：";

			internal const string VehicleCatalyst = "觸媒轉換器：";

			internal const string VehicleClassificationCode = "車型：";

			internal const string VehicleCylinderQuantity = "汽缸數：";

			internal const string VehicleDisplaceQuantity = "排氣量：";

			internal const string VehicleDoorQuantity = "車門數：";

			internal const string VehicleDrivingSide = "左駕駛：";

			internal const string VehicleFuelTypeCode = "燃料/動力代碼：";

			internal const string VehicleModelYearNumeric = "型式年份：";

			internal const string VehicleSeatQuantity = "座位數：";

			internal const string VehicleStatusCode = "車況：";

			internal const string VehicleTransmissionTypeCode = "排檔代碼：";

			internal const string VehicleVINID = "車身號碼：";

			internal const string DutyOtherTaxFeeMethodCode = "從價或從量註記：";

			internal const string DutyOtherTaxFeePercentageNumeric = "折算率：";

			internal const string DutyOtherTaxFeeTaxRateNumeric = "其他稅費率：";

			internal const string DutyOtherTaxFeeTypeCode = "其他稅費代碼：";

			internal const string ImportTaxRateNumeric = "進口稅率：";

			internal const string ImportPercentageNumeric = "折算率：";

			internal const string DutyTaxFeeQuantityDutyUnitCode = "稅額單位：";

			internal const string DutyTaxFeeQuantityPercentageNumeric = "折算率：";

			internal const string DutyTaxFeeQuantityTaxRateNumeric = "單位稅額：";

			internal const string UNClassificationID = "UN危險貨物代碼：";

			internal const string IATAClassificationID = "IATA危險貨物代碼：";

			#endregion
		}
	}
}
