using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX101;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX101MessageBuilder : BaseTWMessageBuilder<INX101, Declaration>
	{
		public override Declaration PopulateDeclaration(INX101 input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId() { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode() { Value = functionCode ?? string.Empty };
				PopulateConsignment(newItem, input.Consignment);
				PopulateDeclarationGoodsShipment(newItem, input.GoodsShipment);
				PopulateGovernmentProcedure(newItem, input.GovernmentProcedure);
				PopulatePackaging(newItem, input.Packaging);
				PopulatePreviousDocument(newItem, input.PreviousDocument);
				PopulateTW_Application(newItem, input.Application);
				PopulateImporter(newItem, input.COImporter);
			}
			return newItem;
		}

		#region Declaration/Consignment
		void PopulateConsignment(Declaration bo, INX101Consignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationConsignment();
				var newItem = bo.Consignment;
				PopulateConsignmentAdditionalDocument(newItem, obj.AdditionalDocument);
				PopulateConsignmentGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItem);
				PopulateConsignmentUnloadingLocation(newItem, obj.UnloadingLocation);
			}
		}

		void PopulateConsignmentAdditionalDocument(DeclarationConsignment bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentAdditionalDocument();
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationConsignmentAdditionalDocumentId { Value = item.ID });
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItem(DeclarationConsignment bo, INX101GovernmentAgencyGoodsItem obj)
		{
			if (obj != null)
			{
				bo.GovernmentAgencyGoodsItem = new DeclarationConsignmentGovernmentAgencyGoodsItem();
				var newItem = bo.GovernmentAgencyGoodsItem;
				PopulateConsignmentGovernmentAgencyGoodsItemManufacturers(newItem, obj.Manufacturers);
				PopulateConsignmentGovernmentAgencyGoodsItemOrigins(newItem, obj.Origins);
				PopulateConsignmentGovernmentAgencyGoodsItemPreviousDocuments(newItem, obj.PreviousDocuments);
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItemManufacturers(DeclarationConsignmentGovernmentAgencyGoodsItem bo, IEnumerable<IPartyDetails> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer();
					newItem.Id = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerId { Value = item.ID };
					PopulateValueIfNodeValueIsNotEmpty(item.Name, () => newItem.Name = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerName { Value = item.Name.Left(80) });
					PopulateValueIfNodeValueIsNotEmpty(item.ChineseName, () => newItem.TwChineseName = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerTwChineseName { Value = item.ChineseName.Left(70) });
					newItem.TwMainManufacturer = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerTwMainManufacturer { Value = item.MainManufacturer };
					newItem.TwTypeCode = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerTwTypeCode { Value = item.TypeCode };
					PopulateConsignmentGovernmentAgencyGoodsItemManufacturerAddress(newItem, item.Address);
					PopulateConsignmentGovernmentAgencyGoodsItemManufacturerCommunication(newItem, item.Communications);
					collection.Add(newItem);
				}
				bo.Manufacturer = collection;
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItemManufacturerAddress(DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer bo, IAddress obj)
		{
			if (obj != null && (!obj.Line.IsEmpty || !obj.ChineseLine.IsEmpty))
			{
				bo.Address = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddressLine { Value = obj.Line.Left(120) });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItemManufacturerCommunication(DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunication();
					newItem.Id = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItemOrigins(DeclarationConsignmentGovernmentAgencyGoodsItem bo, IEnumerable<IOrigin> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentGovernmentAgencyGoodsItemOrigin>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentGovernmentAgencyGoodsItemOrigin();
					newItem.CountryCode = new DeclarationConsignmentGovernmentAgencyGoodsItemOriginCountryCode { Value = item.CountryCode };
					collection.Add(newItem);
				}
				bo.Origin = collection;
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItemPreviousDocuments(DeclarationConsignmentGovernmentAgencyGoodsItem bo, IEnumerable<IPreviousDocument> obj)
		{
			if (obj?.Where(c => !c.ID.IsEmpty) is IEnumerable<IPreviousDocument> ids && ids.Any())
			{
				var collection = new Collection<DeclarationConsignmentGovernmentAgencyGoodsItemPreviousDocument>();
				foreach (var item in ids)
				{
					var newItem = new DeclarationConsignmentGovernmentAgencyGoodsItemPreviousDocument();
					newItem.Id = new DeclarationConsignmentGovernmentAgencyGoodsItemPreviousDocumentId { Value = item.ID };
					collection.Add(newItem);
				}
				bo.PreviousDocument = collection;
			}
		}

		void PopulateConsignmentUnloadingLocation(DeclarationConsignment bo, ILocation obj)
		{
			if (obj != null)
			{
				bo.UnloadingLocation = new DeclarationConsignmentUnloadingLocation();
				var newItem = bo.UnloadingLocation;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationConsignmentUnloadingLocationId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationConsignmentUnloadingLocationName { Value = obj.Name });
			}
		}
		#endregion

		#region Declaration/GoodsShipment
		void PopulateDeclarationGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				PopulateDeclarationGoodsShipmentAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateDeclarationGoodsShipmentAdditionalInformation(newItem, obj.AdditionalInformations);
				PopulateDeclarationGoodsShipmentConsignment(newItem, obj.Consignment);
				PopulateDeclarationGoodsShipmentExporter(newItem, obj.Exporter);
				PopulateDeclarationGoodsShipmentGoodsMeasure(newItem, obj.GoodsMeasures);
				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
				PopulateDeclarationGoodsShipmenttw_AdditionalDeclaration(newItem, obj.AdditionalDeclarations);
			}
		}

		#region Declaration/GoodsShipment/AdditionalDocument
		void PopulateDeclarationGoodsShipmentAdditionalDocument(DeclarationGoodsShipment bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentAdditionalDocument();
					newItem.Id = new DeclarationGoodsShipmentAdditionalDocumentId { Value = item.ID };
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}
		#endregion

		#region Declaration/GoodsShipment/AdditionalInformation
		void PopulateDeclarationGoodsShipmentAdditionalInformation(DeclarationGoodsShipment bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentAdditionalInformation>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentAdditionalInformation();
					newItem.TwApprovalId = new DeclarationGoodsShipmentAdditionalInformationTwApprovalId { Value = item.ApprovalID };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}
		#endregion

		#region Declaration/GoodsShipment/Consignment
		void PopulateDeclarationGoodsShipmentConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				PopulateDelcarationGoodsShipemntConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateDelcarationGoodsShipemntConsignmentDepartureTransportMeans(newItem, obj.DepartureTransportMeans);
				PopulateDelcarationGoodsShipemntConsignmentLoadingLocation(newItem, obj.LoadingLocation);
				PopulateDeclarationGoodsShipmentConsignmentTransportEquipment(newItem, obj.TransportEquipments);
				PopulateDeclarationGoodsShipmentConsignmentUnloadingLocation(newItem, obj.UnloadingLocation);
				PopulateDeclarationGoodsShipmentConsignmentAdditionalInformations(newItem, obj.AdditionalInformations);
			}
		}

		void PopulateDeclarationGoodsShipmentConsignmentAdditionalInformations(DeclarationGoodsShipmentConsignment bo, IEnumerable<IAdditionalInformation> objs)
		{
			if (objs != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation>();
				foreach (var obj in objs)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentAdditionalInformation();
					newItem.StatementDescription = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementDescription { Value = obj.StatementDescription };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateDelcarationGoodsShipemntConsignmentBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID };
			}
		}

		void PopulateDelcarationGoodsShipemntConsignmentDepartureTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.DepartureTransportMeans = new DeclarationGoodsShipmentConsignmentDepartureTransportMeans();
				var newItem = bo.DepartureTransportMeans;
				newItem.Name = new DeclarationGoodsShipmentConsignmentDepartureTransportMeansName { Value = obj.Name };
			}
		}

		void PopulateDelcarationGoodsShipemntConsignmentLoadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			if (obj != null)
			{
				bo.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation();
				var newItem = bo.LoadingLocation;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentConsignmentLoadingLocationName { Value = obj.Name });
				PopulateValueIfNodeValueIsNotEmpty(obj.LoadingDateTime, () => newItem.LoadingDateTime = obj.LoadingDateTime.ToISO8601ShortDateString());
				PopulateValueIfNodeValueIsNotEmpty(obj.EstimatedLoadingCode, () => newItem.TwEstimatedLoadingCode = new DeclarationGoodsShipmentConsignmentLoadingLocationTwEstimatedLoadingCode { Value = obj.EstimatedLoadingCode });
			}
		}

		void PopulateDeclarationGoodsShipmentConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportEquipment> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipment>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportEquipment();
					newItem.Id = new DeclarationGoodsShipmentConsignmentTransportEquipmentId { Value = item.ID };
					collection.Add(newItem);
				}
				bo.TransportEquipment = collection;
			}
		}

		void PopulateDeclarationGoodsShipmentConsignmentUnloadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			if (obj != null)
			{
				bo.UnloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation();
				var newItem = bo.UnloadingLocation;
				newItem.Id = new DeclarationGoodsShipmentConsignmentUnloadingLocationId { Value = obj.ID };
			}
		}
		#endregion

		#region Declaraton/GoodsShipment/Exporter
		void PopulateDeclarationGoodsShipmentExporter(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Exporter = new DeclarationGoodsShipmentExporter();
				var newItem = bo.Exporter;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentExporterId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentExporterName { Value = obj.Name });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentExporterTwChineseName { Value = obj.ChineseName });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentExporterTwTypeCode { Value = obj.TypeCode });
				PopulateDeclarationGoodsShipmentExporterAddress(newItem, obj.Address);
				PopulateDeclarationGoodsShipmentExporterCommunication(newItem, obj.Communications);
			}
		}

		void PopulateDeclarationGoodsShipmentExporterAddress(DeclarationGoodsShipmentExporter bo, IAddress obj)
		{
			if (obj != null && (!obj.Line.IsEmpty || !obj.ChineseLine.IsEmpty))
			{
				bo.Address = new DeclarationGoodsShipmentExporterAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationGoodsShipmentExporterAddressLine { Value = obj.Line });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentExporterAddressTwChineseLine { Value = obj.ChineseLine });
			}
		}

		void PopulateDeclarationGoodsShipmentExporterCommunication(DeclarationGoodsShipmentExporter bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentExporterCommunication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentExporterCommunication();
					newItem.Id = new DeclarationGoodsShipmentExporterCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationGoodsShipmentExporterCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}
		#endregion

		#region Declaratoin/GoodsShipment/GoodsMeasure
		void PopulateDeclarationGoodsShipmentGoodsMeasure(DeclarationGoodsShipment bo, IEnumerable<IGoodsMeasure> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGoodsMeasure>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGoodsMeasure();
					PopulateValueIfNodeValueIsNotEmpty(item.TariffQuantity, () => newItem.TariffQuantity = new DeclarationGoodsShipmentGoodsMeasureTariffQuantity { Value = item.TariffQuantity });
					PopulateValueIfNodeValueIsNotEmpty(item.UnitCode, () => newItem.TwCustomUnitCode = new DeclarationGoodsShipmentGoodsMeasureTwCustomUnitCode { Value = item.UnitCode });
					collection.Add(newItem);
				}
				bo.GoodsMeasure = collection;
			}
		}
		#endregion

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateValueIfNodeValueIsNotEmpty(item.CriteriaCode, () => newItem.TwCriteriaCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCriteriaCode { Value = item.CriteriaCode });
					PopulateValueIfNodeValueIsNotEmpty(item.PreferentialCriteria, () => newItem.TwPreferentialCriteria = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwPreferentialCriteria { Value = item.PreferentialCriteria });
					PopulateValueIfNodeValueIsNotEmpty(item.ProducerCode, () => newItem.TwProducerCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwProducerCode { Value = item.ProducerCode });
					PopulateValueIfNodeValueIsNotEmpty(item.OtherCriteria, () => newItem.TwOtherCriteria = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwOtherCriteria { Value = item.OtherCriteria });
					PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity(newItem, item.Commodity);
					PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasures(newItem, item.GoodsMeasure);
					PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer(newItem, item.Manufacturer);
					PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging(newItem, item.Packaging);
					PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemtw_AdditionalDeclaration(newItem, item.AdditionalDeclaration);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPackaging obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj?.MarksNumbers, () =>
			{
				bo.Packaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging();
				var newItem = bo.Packaging;
				newItem.MarksNumbers = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingMarksNumbers { Value = obj.MarksNumbers };
			});
		}

		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemtw_AdditionalDeclaration(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IAdditionalDeclaration obj)
		{
			if (obj != null)
			{
				bo.TwAdditionalDeclaration = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwAdditionalDeclaration();
				var newItem = bo.TwAdditionalDeclaration;
				newItem.TwId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwAdditionalDeclarationTwId { Value = obj.ID };
				newItem.TwSequenceNumeric = obj.SequenceNumeric;
			}
		}

		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasures(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsMeasure obj)
		{
			if (obj != null)
			{
				bo.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure();
				var newItem = bo.GoodsMeasure;
				newItem.TariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTariffQuantity { Value = obj.TariffQuantity };
				newItem.TwUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwUnitCode { Value = obj.UnitCode };
				newItem.TwCustomUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwCustomUnitCode { Value = obj.CustomUnitCode };
			}
		}

		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Manufacturer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer();
				var newItem = bo.Manufacturer;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerId { Value = obj.ID };
			}
		}

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description };
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name });
				newItem.TwPrintingTariffCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwPrintingTariffCode { Value = obj.PrintingTariffCode };
				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification(newItem, obj.Classifications);
				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent(newItem, obj.Constituent);
				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoice(newItem, obj.Invoice);
				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine(newItem, obj.InvoiceLine);
			}
		}

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Classification
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<IClassification> obj)
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
		#endregion

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/CommodityRelatedPackaging
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityRelatedPackaging obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj?.Specification, () =>
			{
				bo.CommodityRelatedPackaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackaging();
				var newItem = bo.CommodityRelatedPackaging;
				newItem.TwSpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwSpecification { Value = obj.Specification };
			});
		}
		#endregion

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Constituent
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj?.ElementDescription, () =>
			{
				bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
				var newItem = bo.Constituent;
				newItem.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription };
			});
		}
		#endregion

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Invoice
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoice(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IInvoice obj)
		{
			if (obj != null)
			{
				bo.Invoice = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoice();
				var newItem = bo.Invoice;
				newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceId { Value = obj.ID };
				newItem.IssueDateTime = obj.IssueDateTime.ToISO8601ShortDateString();
			}
		}
		#endregion

		#region Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/InvoiceLine
		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IInvoiceLine obj)
		{
			if (obj != null)
			{
				bo.InvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine();
				var newItem = bo.InvoiceLine;
				PopulateValueIfNodeValueIsNotEmpty(obj.ItemChargeAmount, () => newItem.ItemChargeAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineItemChargeAmount { Value = obj.ItemChargeAmount });
				PopulateValueIfNodeValueIsNotEmpty(obj.CurrencyTypeCode, () => newItem.TwCurrencyTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwCurrencyTypeCode { Value = obj.CurrencyTypeCode });
			}
		}
		#endregion

		#endregion

		#endregion

		#region Declaration/GoodsShipment/tw_AdditionalDeclaration
		void PopulateDeclarationGoodsShipmenttw_AdditionalDeclaration(DeclarationGoodsShipment bo, IEnumerable<IAdditionalDeclaration> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentTwAdditionalDeclaration>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentTwAdditionalDeclaration();
					newItem.TwId = new DeclarationGoodsShipmentTwAdditionalDeclarationTwId { Value = item.ID };
					newItem.TwSequenceNumeric = item.SequenceNumeric;
					collection.Add(newItem);
				}
				bo.TwAdditionalDeclaration = collection;
			}
		}
		#endregion
		#endregion

		void PopulateGovernmentProcedure(Declaration bo, IEnumerable<IGovernmentProcedure> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGovernmentProcedure>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGovernmentProcedure();
					newItem.Description = new DeclarationGovernmentProcedureDescription { Value = item.Description };
					collection.Add(newItem);
				}
				bo.GovernmentProcedure = collection;
			}
		}

		void PopulatePackaging(Declaration bo, IPackaging obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj?.MarksNumbers, () =>
			{
				bo.Packaging = new DeclarationPackaging();
				var newItem = bo.Packaging;
				newItem.MarksNumbers = new DeclarationPackagingMarksNumbers { Value = obj.MarksNumbers };
			});
		}

		void PopulatePreviousDocument(Declaration bo, IPreviousDocument obj)
		{
			PopulateValueIfNodeValueIsNotEmpty(obj?.ID, () =>
			{
				bo.PreviousDocument = new DeclarationPreviousDocument();
				var newItem = bo.PreviousDocument;
				newItem.Id = new DeclarationPreviousDocumentId { Value = obj.ID };
			});
		}

		void PopulateTW_Application(Declaration bo, INX101Application obj)
		{
			if (obj != null)
			{
				bo.TwApplication = new DeclarationTwApplication();
				var newItem = bo.TwApplication;
				newItem.TwAdhocCode = new DeclarationTwApplicationTwAdhocCode { Value = obj.AdhocCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.AdhocProcessNumber, () => newItem.TwAdhocProcessNumber = new DeclarationTwApplicationTwAdhocProcessNumber { Value = obj.AdhocProcessNumber });
				PopulateValueIfNodeValueIsNotEmpty(obj.CopyQuantity, () => newItem.TwCopyQuantity = new DeclarationTwApplicationTwCopyQuantity { Value = obj.CopyQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.DescriptionTooLong, () => newItem.TwDescriptionTooLong = new DeclarationTwApplicationTwDescriptionTooLong { Value = obj.DescriptionTooLong });
				PopulateValueIfNodeValueIsNotEmpty(obj.ECFAPrintingDescription, () => newItem.TwEcfaPrintingDescription = new DeclarationTwApplicationTwEcfaPrintingDescription { Value = obj.ECFAPrintingDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.EUSteelDeclarationCode, () => newItem.TwEuSteelDeclarationCode = new DeclarationTwApplicationTwEuSteelDeclarationCode { Value = obj.EUSteelDeclarationCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.EUSteelPhaseCode, () => newItem.TwEuSteelPhaseCode = new DeclarationTwApplicationTwEuSteelPhaseCode { Value = obj.EUSteelPhaseCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.FishingBoatName, () => newItem.TwFishingBoatName = new DeclarationTwApplicationTwFishingBoatName { Value = obj.FishingBoatName });
				PopulateValueIfNodeValueIsNotEmpty(obj.FishingCONoExport, () => newItem.TwFishingCoNoExport = new DeclarationTwApplicationTwFishingCoNoExport { Value = obj.FishingCONoExport });
				newItem.TwGoodsReleaseCode = new DeclarationTwApplicationTwGoodsReleaseCode { Value = obj.GoodsReleaseCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.GoodsReleaseReasonCode, () => newItem.TwGoodsReleaseReasonCode = new DeclarationTwApplicationTwGoodsReleaseReasonCode { Value = obj.GoodsReleaseReasonCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.ManufacturerPrintingCode, () => newItem.TwManufacturerPrintingCode = new DeclarationTwApplicationTwManufacturerPrintingCode { Value = obj.ManufacturerPrintingCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.Observations, () => newItem.TwObservations = new DeclarationTwApplicationTwObservations { Value = obj.Observations });
				newItem.TwOriginalCopyQuantity = new DeclarationTwApplicationTwOriginalCopyQuantity { Value = obj.OriginalCopyQuantity };
				PopulateValueIfNodeValueIsNotEmpty(obj.PreviousCORenderCode, () => newItem.TwPreviousCoRenderCode = new DeclarationTwApplicationTwPreviousCoRenderCode { Value = obj.PreviousCORenderCode });
				newItem.TwPrintingCode = new DeclarationTwApplicationTwPrintingCode { Value = obj.PrintingCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.TriangularTradeCode, () => newItem.TwTriangularTradeCode = new DeclarationTwApplicationTwTriangularTradeCode { Value = obj.TriangularTradeCode });
				newItem.TwTypeCode = new DeclarationTwApplicationTwTypeCode { Value = obj.TypeCode };
				PopulateTW_ApplicationAgent(newItem, obj.Agent);
				PopulateTW_ApplicationContactOffice(newItem, obj.ContactOffice);
				PopulateTW_ApplicationApplicant(newItem, obj.Applicant);
			}
		}

		void PopulateTW_ApplicationAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationTwApplicationAgentId { Value = obj.ID };
				newItem.Name = new DeclarationTwApplicationAgentName { Value = obj.Name };
				newItem.TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode };
				PopulateTW_ApplicationAgentAddress(newItem, obj.Address);
				PopulateTW_ApplicationAgentCommunication(newItem, obj.Communications);
			}
		}

		void PopulateTW_ApplicationAgentAddress(DeclarationTwApplicationAgent bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationAgentAddress();
				var newItem = bo.Address;
				newItem.TwChineseLine = new DeclarationTwApplicationAgentAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		void PopulateTW_ApplicationAgentCommunication(DeclarationTwApplicationAgent bo, IEnumerable<ICommunication> obj)
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
				bo.Communication = collection.FirstOrDefault();
			}
		}

		void PopulateTW_ApplicationContactOffice(DeclarationTwApplication bo, ZString obj)
		{
			bo.ContactOffice = new DeclarationTwApplicationContactOffice();
			var newItem = bo.ContactOffice;
			newItem.Id = new DeclarationTwApplicationContactOfficeId { Value = obj };
		}

		void PopulateTW_ApplicationApplicant(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwApplicant = new DeclarationTwApplicationTwApplicant();
				var newItem = bo.TwApplicant;
				newItem.TwChineseName = new DeclarationTwApplicationTwApplicantTwChineseName { Value = obj.ChineseName };
				newItem.TwId = new DeclarationTwApplicationTwApplicantTwId { Value = obj.ID };
				newItem.TwTypeCode = new DeclarationTwApplicationTwApplicantTwTypeCode { Value = obj.TypeCode };
				newItem.TwUndertakeCode = new DeclarationTwApplicationTwApplicantTwUndertakeCode { Value = obj.UndertakeCode };
				PopulateTW_ApplicationApplicantAddress(newItem, obj.Address);
				PopulateTW_ApplicationApplicantCommunication(newItem, obj.Communications);
			}
		}

		void PopulateTW_ApplicationApplicantAddress(DeclarationTwApplicationTwApplicant bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationTwApplicantAddress();
				var newItem = bo.Address;
				newItem.TwChineseLine = new DeclarationTwApplicationTwApplicantAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		void PopulateTW_ApplicationApplicantCommunication(DeclarationTwApplicationTwApplicant bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationTwApplicationTwApplicantCommunication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationTwApplicationTwApplicantCommunication();
					newItem.Id = new DeclarationTwApplicationTwApplicantCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationTwApplicationTwApplicantCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection.FirstOrDefault();
			}
		}

		void PopulateImporter(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwCoImporter = new DeclarationTwCoImporter();
				var newItem = bo.TwCoImporter;
				newItem.TwId = new DeclarationTwCoImporterTwId { Value = obj.ID };
				newItem.TwName = new DeclarationTwCoImporterTwName { Value = obj.Name };
				newItem.TwChineseName = new DeclarationTwCoImporterTwChineseName { Value = obj.ChineseName };
				PopulateImporterAddress(newItem, obj.Address);
				PopulateImporterCommunication(newItem, obj.Communications);
			}
		}

		void PopulateImporterAddress(DeclarationTwCoImporter bo, IAddress obj)
		{
			if (obj != null && (!obj.Line.IsEmpty || !obj.ChineseLine.IsEmpty))
			{
				bo.Address = new DeclarationTwCoImporterAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationTwCoImporterAddressLine { Value = obj.Line.Left(120) });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationTwCoImporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateImporterCommunication(DeclarationTwCoImporter bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationTwCoImporterCommunication>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationTwCoImporterCommunication();
					newItem.Id = new DeclarationTwCoImporterCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationTwCoImporterCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}
	}
}
