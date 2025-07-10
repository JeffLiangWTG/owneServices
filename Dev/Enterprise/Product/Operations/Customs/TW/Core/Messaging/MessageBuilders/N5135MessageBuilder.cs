using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5135;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class N5135MessageBuilder : BaseTWMessageBuilder<IN5135Declaration, Declaration>
	{
		public ZString PopulateXml(IN5135Declaration declaration, string functionCode)
		{
			return declaration != null ? XmlHelper.Serializer(typeof(Declaration), PopulateDeclaration(declaration, functionCode), true) : string.Empty;
		}

		public override Declaration PopulateDeclaration(IN5135Declaration obj, string functionCode = null)
		{
			var newItem = new Declaration();
			if (obj != null)
			{
				newItem.AcceptanceDateTime = obj.AcceptanceDateTime.ToISO8601ShortDateString();
				newItem.FunctionCode = new DeclarationFunctionCode { Value = obj.FunctionCode };
				newItem.Id = new DeclarationId { Value = obj.ID };
				newItem.TypeCode = new DeclarationTypeCode { Value = obj.TypeCode };
				PopulateAgent(newItem, obj.Agent);
				PopulateConsignment(newItem, obj.Consignment);
				PopulateGoodsShipments(newItem, obj.GoodsShipments);
				PopulateBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateImporter(newItem, obj.Importer);
				newItem.RepresentativePerson = new DeclarationRepresentativePerson { Name = new DeclarationRepresentativePersonName { Value = obj.RepresentativePersonName } };
				PopulateExpressCarrier(newItem, obj.ExpressCarrier);
				PopulateOnBoardCourier(newItem, obj.OnBoardCourier);
			}
			return newItem;
		}

		void PopulateOnBoardCourier(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwOnBoardCourier = new DeclarationTwOnBoardCourier();
				var newItem = bo.TwOnBoardCourier;
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationTwOnBoardCourierTwChineseName { Value = obj.ChineseName.Left(70) });
				newItem.TwId = new DeclarationTwOnBoardCourierTwId { Value = obj.ID };
				newItem.TwName = new DeclarationTwOnBoardCourierTwName { Value = obj.Name };
				newItem.TwTypeCode = new DeclarationTwOnBoardCourierTwTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateExpressCarrier(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwExpressCarrier = new DeclarationTwExpressCarrier();
				var newItem = bo.TwExpressCarrier;
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationTwExpressCarrierTwChineseName { Value = obj.ChineseName.Left(70) });
				newItem.TwId = new DeclarationTwExpressCarrierTwId { Value = obj.ID };
				newItem.TwName = new DeclarationTwExpressCarrierTwName { Value = obj.Name };
				newItem.TwTypeCode = new DeclarationTwExpressCarrierTwTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateImporter(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Importer = new DeclarationImporter();
				var newItem = bo.Importer;
				newItem.Id = new DeclarationImporterId { Value = obj.ID };
				newItem.Name = new DeclarationImporterName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationImporterTwChineseName { Value = obj.ChineseName.Left(70) });
				newItem.TwTypeCode = new DeclarationImporterTwTypeCode { Value = obj.TypeCode };
				PopulateImporterAddress(newItem, obj.Address);
			}
		}

		void PopulateImporterAddress(DeclarationImporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationImporterAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationImporterAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateDutyTaxFee(Declaration bo, IDutyTaxFee obj)
		{
			if (obj != null)
			{
				bo.DutyTaxFee = new DeclarationDutyTaxFee();
				var newItem = bo.DutyTaxFee;
				newItem.TwDutyMethodCode = new DeclarationDutyTaxFeeTwDutyMethodCode { Value = obj.DutyMethodCode };
				PopulatePayment(newItem, obj.PaymentObligationGuaranteeReferenceID);
			}
		}

		void PopulatePayment(DeclarationDutyTaxFee bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Payment = new DeclarationDutyTaxFeePayment() { ObligationGuarantee = new DeclarationDutyTaxFeePaymentObligationGuarantee() { ReferenceId = new DeclarationDutyTaxFeePaymentObligationGuaranteeReferenceId { Value = obj } } };
			}
		}

		void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			var result = new DeclarationBorderTransportMeans();
			if (obj != null)
			{
				result.ArrivalDateTime = obj.ArrivalDateTime.ToISO8601ShortDateString();
				result.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
			}
			bo.BorderTransportMeans = result;
		}

		void PopulateAgent(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationAgentId { Value = obj.ID };
				newItem.RoleCode = new DeclarationAgentRoleCode { Value = obj.RoleCode };
				newItem.TwSubBoxId = new DeclarationAgentTwSubBoxId { Value = obj.SubBoxID };
			}
		}

		void PopulateConsignmentBorderTransportMeans(DeclarationConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationConsignmentBorderTransportMeansId { Value = obj.ID });
				newItem.JourneyId = new DeclarationConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID };
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => newItem.TwRegistration = new DeclarationConsignmentBorderTransportMeansTwRegistration { Value = obj.Registration });
			}
		}

		void PopulateTransportContractDocument(DeclarationConsignment bo, ZString obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj, () => bo.TransportContractDocument = new DeclarationConsignmentTransportContractDocument { Id = new DeclarationConsignmentTransportContractDocumentId { Value = obj } });
		}

		void PopulateConsignment(Declaration bo, IBCDConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationConsignment();
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.BoardedQuantity, () => newItem.BoardedQuantity = new DeclarationConsignmentBoardedQuantity { Value = obj.BoardedQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.ManifestSerialNumber, () => newItem.TwManifestSerialNumber = new DeclarationConsignmentTwManifestSerialNumber { Value = obj.ManifestSerialNumber });
				PopulateValueIfNodeValueIsNotEmpty(obj.AssociatedTransportDocumentId, () => newItem.AssociatedTransportDocument = new DeclarationConsignmentAssociatedTransportDocument { Id = new DeclarationConsignmentAssociatedTransportDocumentId { Value = obj.AssociatedTransportDocumentId } });
				PopulateConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				newItem.GoodsLocation = new DeclarationConsignmentGoodsLocation() { Id = new DeclarationConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				PopulateTransportContractDocument(newItem, obj.TransportContractDocumentId);
			}
		}

		void PopulateResponsibleGovernmentAgency(DeclarationGoodsShipmentAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationGoodsShipmentAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationGoodsShipmentAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		void PopulateAdditionalDocument(DeclarationGoodsShipment bo, IEnumerable<IAdditionalDocument> obj)
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

		void PopulateConsigneeAddress(DeclarationGoodsShipmentConsignee bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentConsigneeAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationGoodsShipmentConsigneeAddressLine { Value = obj.Line.Left(120) });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentConsigneeAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateConsignee(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Consignee = new DeclarationGoodsShipmentConsignee();
				var newItem = bo.Consignee;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsigneeId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentConsigneeName { Value = obj.Name.Left(80) });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentConsigneeTwChineseName { Value = obj.ChineseName.Left(70) });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentConsigneeTwTypeCode { Value = obj.TypeCode });
				PopulateConsigneeAddress(newItem, obj.Address);
			}
		}

		void PopulateGoodsShipmentConsignmentItem(DeclarationGoodsShipmentConsignment bo, IConsignmentItem obj)
		{
			if (obj != null && !obj.AssociatedGovernmentProcedureCode.IsEmpty)
			{
				bo.ConsignmentItem = new DeclarationGoodsShipmentConsignmentConsignmentItem();
				bo.ConsignmentItem.AssociatedGovernmentProcedureCode = new DeclarationGoodsShipmentConsignmentConsignmentItemAssociatedGovernmentProcedureCode { Value = obj.AssociatedGovernmentProcedureCode };
			}
		}

		void PopulateGoodsShipmentGovernmentProcedures(DeclarationGoodsShipmentConsignment bo, IEnumerable<IGovernmentProcedure> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentGovernmentProcedure>();
				foreach (var item in obj.Take(2))
				{
					var newItem = new DeclarationGoodsShipmentConsignmentGovernmentProcedure();
					newItem.Description = new DeclarationGoodsShipmentConsignmentGovernmentProcedureDescription { Value = item.Description };
					collection.Add(newItem);
				}
				bo.GovernmentProcedure = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentLoadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			if (obj != null)
			{
				bo.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation() { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.ID } };
			}
		}

		void PopulateGoodsShipmentConsignmentPackaging(DeclarationGoodsShipmentConsignment bo, IPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationGoodsShipmentConsignmentPackaging { TypeCode = new DeclarationGoodsShipmentConsignmentPackagingTypeCode { Value = obj.TypeCode } };
			}
		}

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IBCDConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				newItem.TotalPackageQuantity = new DeclarationGoodsShipmentConsignmentTotalPackageQuantity { Value = obj.TotalPackageQuantity };
				newItem.TwInvoiceAmount = new DeclarationGoodsShipmentConsignmentTwInvoiceAmount { Value = obj.InvoiceAmount };
				PopulateGoodsShipmentConsignmentItem(newItem, obj.ConsignmentItem);
				PopulateGoodsShipmentGovernmentProcedures(newItem, obj.GovernmentProcedures);
				PopulateGoodsShipmentConsignmentLoadingLocation(newItem, obj.LoadingLocation);
				PopulateGoodsShipmentConsignmentPackaging(newItem, obj.Packaging);
				PopulateValueIfNodeValueIsNotEmpty(obj.TransportContractDocumentId, () => newItem.TransportContractDocument = new DeclarationGoodsShipmentConsignmentTransportContractDocument() { Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId { Value = obj.TransportContractDocumentId } });
			}
		}

		void PopulateGoodsShipmentCurrencyExchange(DeclarationGoodsShipment bo, ICurrencyExchange obj)
		{
			if (obj != null)
			{
				bo.CurrencyExchange = new DeclarationGoodsShipmentCurrencyExchange { CurrencyTypeCode = new DeclarationGoodsShipmentCurrencyExchangeCurrencyTypeCode { Value = obj.CurrencyTypeCode } };
			}
		}

		void PopulateGoodsShipmentCustomsValuation(DeclarationGoodsShipment bo, ICustomsValuation obj)
		{
			if (obj != null)
			{
				bo.CustomsValuation = new DeclarationGoodsShipmentCustomsValuation();
				var newItem = bo.CustomsValuation;
				PopulateValueIfNodeValueIsNotEmpty(obj.ExitToEntryChargeAmount, () => newItem.ExitToEntryChargeAmount = new DeclarationGoodsShipmentCustomsValuationExitToEntryChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ExitToEntryChargeAmount)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.FreightChargeAmount, () => newItem.FreightChargeAmount = new DeclarationGoodsShipmentCustomsValuationFreightChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.FreightChargeAmount)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.OtherChargeDeductionAmount, () => newItem.OtherChargeDeductionAmount = new DeclarationGoodsShipmentCustomsValuationOtherChargeDeductionAmount { Value = obj.OtherChargeDeductionAmount });
				PopulateValueIfNodeValueIsNotEmpty(obj.OtherChargeAmount, () => newItem.TwOtherChargeAmount = new DeclarationGoodsShipmentCustomsValuationTwOtherChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherChargeAmount)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.OtherDeductionAmount, () => newItem.TwOtherDeductionAmount = new DeclarationGoodsShipmentCustomsValuationTwOtherDeductionAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.OtherDeductionAmount)) });
				PopulateValueIfNodeValueIsNotEmpty(obj.TotalDutyTaxFeeAmount, () => newItem.TwTotalDutyTaxFeeAmount = new DeclarationGoodsShipmentCustomsValuationTwTotalDutyTaxFeeAmount { Value = obj.TotalDutyTaxFeeAmount });
			}
		}

		void PopulateGoodsShipmentDutyTaxFee(DeclarationGoodsShipment bo, IEnumerable<IGoodsShipmentDutyTaxFee> obj)
		{
			if (obj != null)
			{
				var effectiveAmounts = obj.Where(x => !x.TypeCode.IsEmpty && !x.AdValoremTaxBaseAmount.IsEmpty);
				if (effectiveAmounts.Any())
				{
					var collection = new Collection<DeclarationGoodsShipmentDutyTaxFee>();
					foreach (var item in effectiveAmounts.Take(9))
					{
						var newItem = new DeclarationGoodsShipmentDutyTaxFee();
						newItem.AdValoremTaxBaseAmount = new DeclarationGoodsShipmentDutyTaxFeeAdValoremTaxBaseAmount { Value = item.AdValoremTaxBaseAmount };
						newItem.TypeCode = new DeclarationGoodsShipmentDutyTaxFeeTypeCode { Value = item.TypeCode };
						collection.Add(newItem);
					}
					bo.DutyTaxFee = collection;
				}
			}
		}

		void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IClassification obj)
		{
			if (obj != null)
			{
				bo.Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationId { Value = obj.ID } };
			}
		}

		void PopulateConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			if (obj != null && !obj.ElementDescription.IsEmpty)
			{
				bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
				var newItem = bo.Constituent;
				newItem.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription };
			}
		}

		void PopulateCommodityDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityDutyTaxFee obj)
		{
			if (obj != null)
			{
				bo.DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee();
				var newItem = bo.DutyTaxFee;
				newItem.AdValoremTaxBaseAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeAdValoremTaxBaseAmount { Value = obj.AdValoremTaxBaseAmount };
				newItem.SpecificTaxBaseQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeSpecificTaxBaseQuantity { Value = obj.SpecificTaxBaseQuantity };
			}
		}

		void PopulateCommodityGovernmentProcedure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IGovernmentProcedure obj)
		{
			if (obj != null)
			{
				bo.GovernmentProcedure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure() { CurrentCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureCurrentCode { Value = obj.CurrentCode } };
			}
		}

		void PopulateCommodityDutyOtherTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<IDutyOtherTaxFee> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFee>();
				foreach (var item in obj.Take(9))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFee();
					newItem.TwTaxRateNumeric = item.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.TaxRateNumeric));
					newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyOtherTaxFeeTwTypeCode { Value = item.TypeCode };
					PopulateValueIfNodeValueIsNotEmpty(item.PercentageNumeric, () =>
					{
						newItem.TwPercentageNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(item.PercentageNumeric));
					});
					collection.Add(newItem);
				}
				bo.TwDutyOtherTaxFee = collection;
			}
		}

		void PopulateCommodityDutyTaxFeeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IDutyTaxFeeAmount obj)
		{
			if (obj != null)
			{
				bo.TwDutyTaxFeeAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyTaxFeeAmount();
				var newItem = bo.TwDutyTaxFeeAmount;
				newItem.TwTaxRateNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TaxRateNumeric));
				PopulateValueIfNodeValueIsNotEmpty(obj.PercentageNumeric, () =>
				{
					newItem.TwPercentageNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.PercentageNumeric));
				});
			}
		}

		void PopulateCommodityDutyTaxFeeQuantity(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IDutyTaxFeeQuantity obj)
		{
			if (obj != null)
			{
				bo.TwDutyTaxFeeQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyTaxFeeQuantity();
				var newItem = bo.TwDutyTaxFeeQuantity;
				newItem.TwDutyUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwDutyTaxFeeQuantityTwDutyUnitCode { Value = obj.DutyUnitCode };
				newItem.TwTaxRateNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TaxRateNumeric));
				PopulateValueIfNodeValueIsNotEmpty(obj.PercentageNumeric, () =>
				{
					newItem.TwPercentageNumeric = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.PercentageNumeric));
				});
			}
		}

		void PopulateCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID.Left(80) });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description.Left(512) };
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name.Left(50) });
				PopulateClassification(newItem, obj.Classification);
				PopulateConstituent(newItem, obj.Constituent);
				PopulateCommodityDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateCommodityGovernmentProcedure(newItem, obj.GovernmentProcedure);
				PopulateCommodityDutyOtherTaxFee(newItem, obj.DutyOtherTaxFees);
				PopulateCommodityDutyTaxFeeAmount(newItem, obj.DutyTaxFeeAmount);
				PopulateCommodityDutyTaxFeeQuantity(newItem, obj.DutyTaxFeeQuantity);
			}
		}

		void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsMeasure obj)
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

		void PopulateOrigin(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IOrigin obj)
		{
			if (obj != null && !obj.CountryCode.IsEmpty)
			{
				bo.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin() { CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = obj.CountryCode } };
			}
		}

		void PopulateGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateOrigin(newItem, item.Origin);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateSupplier(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Supplier = new DeclarationGoodsShipmentSupplier();
				var newItem = bo.Supplier;
				newItem.Name = new DeclarationGoodsShipmentSupplierName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentSupplierTwChineseName { Value = obj.ChineseName.Left(70) });
			}
		}

		void PopulateTradeTerms(DeclarationGoodsShipment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.TradeTerms = new DeclarationGoodsShipmentTradeTerms() { ConditionCode = new DeclarationGoodsShipmentTradeTermsConditionCode { Value = obj } };
			}
		}

		void PopulateGoodsShipments(Declaration bo, IEnumerable<IN5135GoodsShipment> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipment>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipment();
					newItem.SequenceNumeric = item.SequenceNumeric;
					newItem.TwInvoiceAmount = new DeclarationGoodsShipmentTwInvoiceAmount { Value = item.InvoiceAmount };
					PopulateValueIfNodeValueIsNotEmpty(item.TaxFeeDeclared, () => newItem.TwTaxFeeDeclared = new DeclarationGoodsShipmentTwTaxFeeDeclared { Value = item.TaxFeeDeclared });
					newItem.TwTotalGrossMassMeasure = new DeclarationGoodsShipmentTwTotalGrossMassMeasure { Value = item.TotalGrossMassMeasure };
					PopulateAdditionalDocument(newItem, item.AdditionalDocuments);
					PopulateConsignee(newItem, item.Consignee);
					PopulateGoodsShipmentConsignment(newItem, item.Consignment);
					PopulateGoodsShipmentCurrencyExchange(newItem, item.CurrencyExchange);
					PopulateGoodsShipmentCustomsValuation(newItem, item.CustomsValuation);
					PopulateGoodsShipmentDutyTaxFee(newItem, item.DutyTaxFees);
					PopulateGovernmentAgencyGoodsItem(newItem, item.GovernmentAgencyGoodsItems);
					PopulateSupplier(newItem, item.Supplier);
					PopulateTradeTerms(newItem, item.TradeTermsConditionCode);
					collection.Add(newItem);
				}
				bo.GoodsShipment = collection;
			}
		}
	}
}
