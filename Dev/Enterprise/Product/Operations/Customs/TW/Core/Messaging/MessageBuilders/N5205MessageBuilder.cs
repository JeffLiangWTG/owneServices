using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5205;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class N5205MessageBuilder : BaseTWMessageBuilder<IN5205Declaration, Declaration>
	{
		public ZString PopulateXml(IN5205Declaration declaration, string functionCode)
		{
			return declaration != null ? XmlHelper.Serializer(typeof(Declaration), PopulateDeclaration(declaration, functionCode), true) : string.Empty;
		}

		public override Declaration PopulateDeclaration(IN5205Declaration obj, string functionCode = null)
		{
			var newItem = new Declaration();
			if (obj != null)
			{
				newItem.AcceptanceDateTime = obj.AcceptanceDateTime.ToISO8601ShortDateString();
				newItem.FunctionCode = new DeclarationFunctionCode { Value = obj.FunctionCode };
				newItem.Id = new DeclarationId { Value = obj.ID };
				newItem.TypeCode = new DeclarationTypeCode { Value = obj.TypeCode };
				PopulateAgent(newItem, obj.Agent);
				PopulateBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateExporter(newItem, obj.Exporter);
				newItem.RepresentativePerson = new DeclarationRepresentativePerson { Name = new DeclarationRepresentativePersonName { Value = obj.RepresentativePersonName } };
				PopulateExpressCarrier(newItem, obj.ExpressCarrier);
				PopulateOnBoardCourier(newItem, obj.OnBoardCourier);
				PopulateConsignment(newItem, obj.Consignment);
				PopulateGoodsShipments(newItem, obj.GoodsShipments);
			}
			return newItem;
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
				foreach (var item in obj.Take(99))
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

		void PopulateBuyerAddress(DeclarationGoodsShipmentBuyer bo, IAddress obj)
		{
			bo.Address = new DeclarationGoodsShipmentBuyerAddress();
			if (obj != null)
			{
				var newItem = bo.Address;
				newItem.CountryCode = new DeclarationGoodsShipmentBuyerAddressCountryCode { Value = obj.CountryCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationGoodsShipmentBuyerAddressLine { Value = obj.Line.Left(120) });
			}
		}

		void PopulateBuyer(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Buyer = new DeclarationGoodsShipmentBuyer();
				var newItem = bo.Buyer;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentBuyerId { Value = obj.ID });
				newItem.Name = new DeclarationGoodsShipmentBuyerName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentBuyerTwTypeCode { Value = obj.TypeCode });
				PopulateBuyerAddress(newItem, obj.Address);
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
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentGovernmentProcedure>();
				foreach (var item in obj.Take(2))
				{
					var newItem = new DeclarationGoodsShipmentConsignmentGovernmentProcedure();
					newItem.Description = new DeclarationGoodsShipmentConsignmentGovernmentProcedureDescription { Value = item.Description.Left(256) };
					collection.Add(newItem);
				}
				bo.GovernmentProcedure = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentUnloadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			bo.UnloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation();
			if (obj != null)
			{
				bo.UnloadingLocation.Id = new DeclarationGoodsShipmentConsignmentUnloadingLocationId { Value = obj.ID };
			}
		}

		void PopulateGoodsShipmentConsignmentPackaging(DeclarationGoodsShipmentConsignment bo, IPackaging obj)
		{
			bo.Packaging = new DeclarationGoodsShipmentConsignmentPackaging();
			if (obj != null)
			{
				bo.Packaging.TypeCode = new DeclarationGoodsShipmentConsignmentPackagingTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IBCDConsignment obj)
		{
			bo.Consignment = new DeclarationGoodsShipmentConsignment();
			if (obj != null)
			{
				var newItem = bo.Consignment;
				newItem.TotalPackageQuantity = new DeclarationGoodsShipmentConsignmentTotalPackageQuantity { Value = obj.TotalPackageQuantity };
				PopulateGoodsShipmentConsignmentItem(newItem, obj.ConsignmentItem);
				PopulateGoodsShipmentGovernmentProcedures(newItem, obj.GovernmentProcedures);
				PopulateGoodsShipmentConsignmentPackaging(newItem, obj.Packaging);
				PopulateValueIfNodeValueIsNotEmpty(obj.TransportContractDocumentId, () => newItem.TransportContractDocument = new DeclarationGoodsShipmentConsignmentTransportContractDocument { Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId { Value = obj.TransportContractDocumentId } });
				PopulateGoodsShipmentConsignmentUnloadingLocation(newItem, obj.UnloadingLocation);
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
				newItem.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription.Left(256) };
			}
		}

		void PopulateCommodityInvoiceLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IInvoiceLine obj)
		{
			bo.InvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine();
			if (obj != null)
			{
				bo.InvoiceLine.ItemChargeAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineItemChargeAmount { Value = obj.ItemChargeAmount };
			}
		}

		void PopulateCommodityCommodityNumbers(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<ICommodityNumber> obj)
		{
			if (obj != null)
			{
				bo.TwCommodityNumber = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumber>();
				foreach (var item in obj.Take(2))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumber();
					newItem.TwId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumberTwId { Value = item.ID };
					newItem.TwIdentifierTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwCommodityNumberTwIdentifierTypeCode { Value = item.IdentifierTypeCode };
					bo.TwCommodityNumber.Add(newItem);
				}
			}
		}

		void PopulateCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			if (obj != null)
			{
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID.Left(80) });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description.Left(512) };
				newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name.Left(50) };
				PopulateClassification(newItem, obj.Classification);
				PopulateConstituent(newItem, obj.Constituent);
				PopulateCommodityInvoiceLine(newItem, obj.InvoiceLine);
				PopulateCommodityCommodityNumbers(newItem, obj.CommodityNumbers);
			}
		}

		void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsMeasure obj)
		{
			bo.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure();
			if (obj != null)
			{
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

		void PopulateGovernmentProcedure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGovernmentProcedure obj)
		{
			bo.GovernmentProcedure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGovernmentProcedure();
			if (obj != null)
			{
				bo.GovernmentProcedure.CurrentCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGovernmentProcedureCurrentCode { Value = obj.CurrentCode };
			}
		}

		void PopulatePreBondedDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPreviousDocument obj)
		{
			if (obj != null)
			{
				bo.TwPreBondedDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwPreBondedDocument();
				var newItem = bo.TwPreBondedDocument;
				newItem.TwId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwPreBondedDocumentTwId { Value = obj.ID };
				newItem.TwLineNumeric = obj.LineNumeric;
			}
		}

		void PopulateGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			if (obj != null)
			{
				foreach (var item in obj.Take(9999))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateGovernmentProcedure(newItem, item.GovernmentProcedure);
					PopulateOrigin(newItem, item.Origin);
					PopulatePreBondedDocument(newItem, item.PreBondedDocument);
					collection.Add(newItem);
				}
			}
			bo.GovernmentAgencyGoodsItem = collection;
		}

		void PopulateGoodsShipmentExporterAddress(DeclarationGoodsShipmentExporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentExporterAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationGoodsShipmentExporterAddressLine { Value = obj.Line.Left(120) });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentExporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}
		void PopulateGoodsShipmentExporter(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			bo.Exporter = new DeclarationGoodsShipmentExporter();
			if (obj != null)
			{
				var newItem = bo.Exporter;
				newItem.Id = new DeclarationGoodsShipmentExporterId { Value = obj.ID };
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentExporterName { Value = obj.Name.Left(80) });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentExporterTwChineseName { Value = obj.ChineseName.Left(70) });
				newItem.TwTypeCode = new DeclarationGoodsShipmentExporterTwTypeCode { Value = obj.TypeCode };
				PopulateGoodsShipmentExporterAddress(newItem, obj.Address);
			}
		}

		void PopulateGoodsShipments(Declaration bo, IEnumerable<IBCDGoodsShipment> obj)
		{
			var collection = new Collection<DeclarationGoodsShipment>();
			if (obj != null)
			{
				foreach (var item in obj.Take(9999))
				{
					var newItem = new DeclarationGoodsShipment();
					newItem.SequenceNumeric = item.SequenceNumeric;
					newItem.TwItemChargeAmount = new DeclarationGoodsShipmentTwItemChargeAmount { Value = item.ItemChargeAmount };
					newItem.TwTotalGrossMassMeasure = new DeclarationGoodsShipmentTwTotalGrossMassMeasure { Value = item.TotalGrossMassMeasure };
					PopulateAdditionalDocument(newItem, item.AdditionalDocuments);
					PopulateBuyer(newItem, item.Buyer);
					PopulateGoodsShipmentConsignment(newItem, item.Consignment);
					PopulateValueIfNodeValueIsNotEmpty(item.DeliveryDestinationName, () => newItem.DeliveryDestination = new DeclarationGoodsShipmentDeliveryDestination { Name = new DeclarationGoodsShipmentDeliveryDestinationName { Value = item.DeliveryDestinationName } });
					PopulateGoodsShipmentExporter(newItem, item.Exporter);
					PopulateGovernmentAgencyGoodsItem(newItem, item.GovernmentAgencyGoodsItems);
					collection.Add(newItem);
				}
			}
			bo.GoodsShipment = collection;
		}

		void PopulateConsignmentBorderTransportMeans(DeclarationConsignment bo, ITransportMeans obj)
		{
			bo.BorderTransportMeans = new DeclarationConsignmentBorderTransportMeans();
			if (obj != null)
			{
				var newItem = bo.BorderTransportMeans;
				newItem.JourneyId = new DeclarationConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID };
				PopulateValueIfNodeValueIsNotEmpty(obj.CallSignID, () => newItem.TwCallSignId = new DeclarationConsignmentBorderTransportMeansTwCallSignId { Value = obj.CallSignID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => newItem.TwRegistration = new DeclarationConsignmentBorderTransportMeansTwRegistration { Value = obj.Registration });
			}
		}

		void PopulateConsignmentDepartureTransportMeans(DeclarationConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.DepartureTransportMeans = new DeclarationConsignmentDepartureTransportMeans();
				var newItem = bo.DepartureTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationConsignmentDepartureTransportMeansId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationConsignmentDepartureTransportMeansName { Value = obj.Name });
			}
		}

		void PopulateConsignment(Declaration bo, IBCDConsignment obj)
		{
			bo.Consignment = new DeclarationConsignment();
			if (obj != null)
			{
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.BoardedQuantity, () => newItem.BoardedQuantity = new DeclarationConsignmentBoardedQuantity { Value = obj.BoardedQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.ShippingOrderNumber, () => newItem.TwShippingOrderNumber = new DeclarationConsignmentTwShippingOrderNumber { Value = obj.ShippingOrderNumber });
				PopulateValueIfNodeValueIsNotEmpty(obj.AssociatedTransportDocumentId, () => newItem.AssociatedTransportDocument = new DeclarationConsignmentAssociatedTransportDocument { Id = new DeclarationConsignmentAssociatedTransportDocumentId { Value = obj.AssociatedTransportDocumentId } });
				PopulateConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateConsignmentDepartureTransportMeans(newItem, obj.DepartureTransportMeans);
				newItem.GoodsLocation = new DeclarationConsignmentGoodsLocation { Id = new DeclarationConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				newItem.TransportContractDocument = new DeclarationConsignmentTransportContractDocument { Id = new DeclarationConsignmentTransportContractDocumentId { Value = obj.TransportContractDocumentId } };
			}
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
			bo.TwExpressCarrier = new DeclarationTwExpressCarrier();
			if (obj != null)
			{
				var newItem = bo.TwExpressCarrier;
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationTwExpressCarrierTwChineseName { Value = obj.ChineseName.Left(70) });
				newItem.TwId = new DeclarationTwExpressCarrierTwId { Value = obj.ID };
				newItem.TwName = new DeclarationTwExpressCarrierTwName { Value = obj.Name };
				newItem.TwTypeCode = new DeclarationTwExpressCarrierTwTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateExporter(Declaration bo, IPartyDetails obj)
		{
			bo.Exporter = new DeclarationExporter();
			if (obj != null)
			{
				var newItem = bo.Exporter;
				newItem.Id = new DeclarationExporterId { Value = obj.ID };
				newItem.Name = new DeclarationExporterName { Value = obj.Name.Left(80) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationExporterTwChineseName { Value = obj.ChineseName.Left(70) });
				PopulateValueIfNodeValueIsNotEmpty(obj.CustomsControlID, () => newItem.TwCustomsControlId = new DeclarationExporterTwCustomsControlId { Value = obj.CustomsControlID });
				newItem.TwTypeCode = new DeclarationExporterTwTypeCode { Value = obj.TypeCode };
				PopulateExporterAddress(newItem, obj.Address);
			}
		}

		void PopulateExporterAddress(DeclarationExporter bo, IAddress obj)
		{
			bo.Address = new DeclarationExporterAddress();
			if (obj != null)
			{
				var newItem = bo.Address;
				newItem.Line = new DeclarationExporterAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationExporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			var result = new DeclarationBorderTransportMeans();
			if (obj != null)
			{
				result.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
			}
			bo.BorderTransportMeans = result;
		}

		void PopulateAgent(Declaration bo, IPartyDetails obj)
		{
			bo.Agent = new DeclarationAgent();
			if (obj != null)
			{
				var newItem = bo.Agent;
				newItem.Id = new DeclarationAgentId { Value = obj.ID };
				newItem.RoleCode = new DeclarationAgentRoleCode { Value = obj.RoleCode };
				newItem.TwSubBoxId = new DeclarationAgentTwSubBoxId { Value = obj.SubBoxID };
			}
		}
	}
}
