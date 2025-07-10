using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX301_AX;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX301_AXMessageBuilder : BaseTWMessageBuilder<INX301_AXDeclaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX301_AXDeclaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode { Value = input.FunctionCode };
				newItem.Id = new DeclarationId { Value = input.ID };
				PopulateAdditionalDocument(newItem, input.AdditionalDocument);
				PopulateAgent(newItem, input.Agent);
				PopulateBorderTransportMeans(newItem, input.BorderTransportMeans);
				PopulateConsignment(newItem, input.Consignment);
				PopulateCurrencyExchange(newItem, input.CurrencyExchange);
				PopulateGoodsShipment(newItem, input.GoodsShipment);
				PopulateImporter(newItem, input.Importer);
				PopulateApplication(newItem, input.Application);
			}
			return newItem;
		}

		void PopulateAppointment(DeclarationTwApplication bo, IAppointment obj)
		{
			if (obj != null && (obj.ReservationDate.IsValid || !obj.ReservationPeriodCode.IsEmpty))
			{
				bo.TwAppointment = new DeclarationTwApplicationTwAppointment();
				var newItem = bo.TwAppointment;
				newItem.TwReservationDate = obj.ReservationDate.ToDateTime();
				newItem.TwReservationPeriodCode = new DeclarationTwApplicationTwAppointmentTwReservationPeriodCode { Value = obj.ReservationPeriodCode.Left(1) };
			}
		}

		void PopulateApplicationPayment(DeclarationTwApplication bo, IPayment obj)
		{
			if (obj != null)
			{
				bo.Payment = new DeclarationTwApplicationPayment() { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.MethodCode } };
				PopulateValueIfNodeValueIsNotEmpty(obj.ReferenceID, () => bo.Payment.ReferenceId = new DeclarationTwApplicationPaymentReferenceId { Value = obj.ReferenceID });
			}
		}

		void PopulateApplicationAgentCommunication(DeclarationTwApplicationAgent bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationTwApplicationAgentCommunication>();
				foreach (var item in obj.Take(2))
				{
					var newItem = new DeclarationTwApplicationAgentCommunication();
					newItem.Id = new DeclarationTwApplicationAgentCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationTwApplicationAgentCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}

		void PopulateApplicationAgentAddress(DeclarationTwApplicationAgent bo, IAddress obj)
		{
			if (obj != null && !obj.ChineseLine.IsEmpty)
			{
				bo.Address = new DeclarationTwApplicationAgentAddress() { TwChineseLine = new DeclarationTwApplicationAgentAddressTwChineseLine { Value = obj.ChineseLine.Left(100) } };
			}
		}

		void PopulateApplicationAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationTwApplicationAgentId { Value = obj.ID };
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationTwApplicationAgentName { Value = obj.Name });
				newItem.TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode };
				PopulateApplicationAgentAddress(newItem, obj.Address);
				PopulateApplicationAgentCommunication(newItem, obj.Communications);
			}
		}

		void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				PopulateValueIfNodeValueIsNotEmpty(obj.StatementDescription, () => newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription { Value = obj.StatementDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.BulkApplicationID, () => newItem.TwBulkApplicationId = new DeclarationTwApplicationAdditionalInformationTwBulkApplicationId { Value = obj.BulkApplicationID });
				PopulateValueIfNodeValueIsNotEmpty(obj.BulkPortCode, () => newItem.TwBulkPortCode = new DeclarationTwApplicationAdditionalInformationTwBulkPortCode() { Value = obj.BulkPortCode });
				newItem.TwElectronicReceipt = new DeclarationTwApplicationAdditionalInformationTwElectronicReceipt { Value = obj.ElectronicReceipt };
				PopulateValueIfNodeValueIsNotEmpty(obj.ProvedPaper, () => newItem.TwProvedPaper = new DeclarationTwApplicationAdditionalInformationTwProvedPaper { Value = obj.ProvedPaper });
			}
		}

		void PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(DeclarationTwApplicationAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		void PopulateApplicationAdditionalDocument(DeclarationTwApplication bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationTwApplicationAdditionalDocument>();
				foreach (var item in obj.Take(99))
				{
					var newItem = new DeclarationTwApplicationAdditionalDocument();
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationTwApplicationAdditionalDocumentId { Value = item.ID });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileFormat, () => newItem.TwImageFileFormat = new DeclarationTwApplicationAdditionalDocumentTwImageFileFormat { Value = item.ImageFileFormat });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileName, () => newItem.TwImageFileName = new DeclarationTwApplicationAdditionalDocumentTwImageFileName { Value = item.ImageFileName });
					newItem.TypeCode = new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = item.TypeCode };
					PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateApplication(Declaration bo, IApplication obj)
		{
			if (obj != null)
			{
				bo.TwApplication = new DeclarationTwApplication();
				var newItem = bo.TwApplication;
				PopulateApplicationAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateApplicationAdditionalInformation(newItem, obj.AdditionalInformation);
				PopulateApplicationAgent(newItem, obj.Agent);
				newItem.ContactOffice = new DeclarationTwApplicationContactOffice { Id = new DeclarationTwApplicationContactOfficeId { Value = obj.ContactOffice } };
				PopulateApplicationPayment(newItem, obj.Payment);
				PopulateAppointment(newItem, obj.Appointment);
			}
		}

		void PopulateImporterCommunication(DeclarationImporter bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationImporterCommunication>();
				foreach (var item in obj.Take(2))
				{
					var newItem = new DeclarationImporterCommunication();
					newItem.Id = new DeclarationImporterCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationImporterCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}

		void PopulateImporterAddress(DeclarationImporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationImporterAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationImporterAddressLine { Value = obj.Line };
				newItem.TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		void PopulateImporter(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Importer = new DeclarationImporter();
				var newItem = bo.Importer;
				newItem.Id = new DeclarationImporterId { Value = obj.ID };
				newItem.Name = new DeclarationImporterName { Value = obj.Name };
				newItem.TwChineseName = new DeclarationImporterTwChineseName { Value = obj.ChineseName };
				newItem.TwTypeCode = new DeclarationImporterTwTypeCode { Value = obj.TypeCode };
				PopulateImporterAddress(newItem, obj.Address);
				PopulateImporterCommunication(newItem, obj.Communications);
			}
		}

		void PopulateSellerAddress(DeclarationGoodsShipmentSeller bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentSellerAddress();
				var newItem = bo.Address;
				newItem.CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.CountryCode };
				newItem.Line = new DeclarationGoodsShipmentSellerAddressLine { Value = obj.Line };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentSellerAddressTwChineseLine { Value = obj.ChineseLine });
			}
		}

		void PopulateSeller(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Seller = new DeclarationGoodsShipmentSeller();
				var newItem = bo.Seller;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentSellerId { Value = obj.ID });
				newItem.Name = new DeclarationGoodsShipmentSellerName { Value = obj.Name };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentSellerTwChineseName { Value = obj.ChineseName });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentSellerTwTypeCode { Value = obj.TypeCode });
				PopulateSellerAddress(newItem, obj.Address);
			}
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification NewShippingIdentification(IShippingIdentification obj)
		{
			var result = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification();
			PopulateValueIfNodeValueIsNotEmpty(obj.LotNumberID, () => result.TwLotNumberId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwLotNumberId { Value = obj.LotNumberID.Left(150) });
			PopulateValueIfNodeValueIsValid(obj.ProductBestBeforeDateTime, () => result.TwProductBestBeforeDateTime = obj.ProductBestBeforeDateTime.ToString());
			PopulateValueIfNodeValueIsValid(obj.ProductManufacturedDate, () =>
			{
				result.TwProductManufacturedDateValueSpecified = true;
				result.TwProductManufacturedDate = obj.ProductManufacturedDate.ToDateTime();
			});
			return result;
		}

		bool CanPopulateShippingIdentification(IShippingIdentification obj) => obj != null && (!obj.LotNumberID.IsEmpty || obj.ProductBestBeforeDateTime.IsValid || !obj.ProductLotNumberAmount.IsEmpty || obj.ProductManufacturedDate.IsValid);

		void PopulateShippingIdentification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IShippingIdentification> obj)
		{
			if (obj?.Any() ?? false)
			{
				var identifications = obj.Where(x => CanPopulateShippingIdentification(x)).Take(99).Select(y => NewShippingIdentification(y));
				if (identifications.Any())
				{
					bo.TwShippingIdentification = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification>(identifications.ToList());
				}
			}
		}

		void PopulateGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsStatisticalMeasure obj)
		{
			if (obj != null)
			{
				bo.TwGoodsStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasure();
				var newItem = bo.TwGoodsStatisticalMeasure;
				PopulateValueIfNodeValueIsNotEmpty(obj.StatisticalUnitCode, () => newItem.TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.TariffQuantity, () => newItem.TwTariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwTariffQuantity { Value = obj.TariffQuantity });
			}
		}

		void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsMeasure obj)
		{
			if (obj != null)
			{
				bo.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure();
				var newItem = bo.GoodsMeasure;
				newItem.NetWeightMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureNetWeightMeasure { Value = obj.NetWeightMeasure };
				newItem.TariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTariffQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TariffQuantity)) };
				newItem.TwUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwUnitCode { Value = obj.UnitCode };
			}
		}

		void PopulateCommodityDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityDutyTaxFee obj)
		{
			if (obj != null)
			{
				bo.DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee();
				bo.DutyTaxFee.AdValoremTaxBaseAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeAdValoremTaxBaseAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.AdValoremTaxBaseAmount)) };
			}
		}

		void PopulateCommodityRelatedPackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityRelatedPackaging obj)
		{
			if (obj != null)
			{
				bo.CommodityRelatedPackaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackaging();
				bo.CommodityRelatedPackaging.PackingMethodDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingPackingMethodDescription { Value = obj.PackingMethodDescription };
			}
		}

		void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IClassification obj)
		{
			if (obj != null)
			{
				bo.Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification();
				bo.Classification.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationId { Value = obj.ID };
			}
		}

		void PopulateCommodityConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			if (obj != null && !obj.ElementDescription.IsEmpty)
			{
				bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
				bo.Constituent.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription };
			}
		}

		void PopulateGovernmentAgencyGoodsItemCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description };
				newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription { Value = obj.ChineseDescription };
				PopulateValueIfNodeValueIsNotEmpty(obj.TariffCodeExtensionCode, () => newItem.TwTariffCodeExtensionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwTariffCodeExtensionCode { Value = obj.TariffCodeExtensionCode });
				PopulateClassification(newItem, obj.Classification);
				PopulateCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
				PopulateCommodityConstituent(newItem, obj.Constituent);
				PopulateCommodityDutyTaxFee(newItem, obj.DutyTaxFee);
			}
		}

		void PopulateGovernmentAgencyGoodsItemAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
				foreach (var item in obj.Take(10))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
					newItem.StatementCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementCode { Value = item.StatementCode };
					newItem.StatementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementDescription { Value = item.StatementDescription };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj.Take(9999))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateGovernmentAgencyGoodsItemAdditionalInformation(newItem, item.AdditionalInformations);
					PopulateGovernmentAgencyGoodsItemCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateGoodsStatisticalMeasure(newItem, item.GoodsStatisticalMeasure);
					PopulateShippingIdentification(newItem, item.ShippingIdentifications);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentTransportEquipmentSeals(DeclarationGoodsShipmentConsignmentTransportEquipment bo, IEnumerable<ZString> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentTwSeal>();
				foreach (var item in objs.Take(10))
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwSeal();
					newItem.TwSealId = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwSealTwSealId { Value = item };
					collection.Add(newItem);
				}
				bo.TwSeal = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportEquipment> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipment>();
				foreach (var item in objs.Take(9999))
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportEquipment();
					PopulateValueIfNodeValueIsNotEmpty(item.CharacteristicCode, () => newItem.CharacteristicCode = new DeclarationGoodsShipmentConsignmentTransportEquipmentCharacteristicCode { Value = item.CharacteristicCode });
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentTransportEquipmentId { Value = item.ID });
					PopulateValueIfNodeValueIsNotEmpty(item.UsedCapacityCode, () => newItem.TwUsedCapacityCode = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwUsedCapacityCode { Value = item.UsedCapacityCode });
					PopulateGoodsShipmentConsignmentTransportEquipmentSeals(newItem, item.Seals);

					collection.Add(newItem);
				}
				bo.TransportEquipment = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentAdditionalInformation(DeclarationGoodsShipmentConsignment bo, IEnumerable<IAdditionalInformation> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation>();
				foreach (var item in objs.Take(10))
				{
					var newItem = new DeclarationGoodsShipmentConsignmentAdditionalInformation();
					newItem.StatementDescription = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementDescription { Value = item.StatementDescription };
					newItem.StatementCode = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementCode { Value = item.StatementCode };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				newItem.Id = new DeclarationGoodsShipmentConsignmentBorderTransportMeansId { Value = obj.ID };
				newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID };
			}
		}

		void PopulateGoodsShipmentConsignmentTransportContractDocuments(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument>();
				foreach (var item in obj.Take(2))
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportContractDocument();
					newItem.Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId { Value = item.ID };
					newItem.TypeCode = new DeclarationGoodsShipmentConsignmentTransportContractDocumentTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TransportContractDocument = collection;
			}
		}

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				PopulateGoodsShipmentConsignmentAdditionalInformation(newItem, obj.AdditionalInformations);
				PopulateGoodsShipmentConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				newItem.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				newItem.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation() { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.LoadingLocation?.ID ?? ZString.Empty } };
				PopulateGoodsShipmentConsignmentTransportContractDocuments(newItem, obj.TransportContractDocuments);
				PopulateGoodsShipmentConsignmentTransportEquipment(newItem, obj.TransportEquipments);
			}
		}

		void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				PopulateValueIfNodeValueIsValid(obj.ExitDateTime, () => newItem.ExitDateTime = obj.ExitDateTime.ToISO8601ShortDateString());
				PopulateGoodsShipmentConsignment(newItem, obj.Consignment);
				PopulateGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
				PopulateSeller(newItem, obj.Seller);
			}
		}

		void PopulateCurrencyExchange(Declaration bo, ICurrencyExchange obj)
		{
			if (obj != null)
			{
				bo.CurrencyExchange = new DeclarationCurrencyExchange();
				bo.CurrencyExchange.CurrencyTypeCode = new DeclarationCurrencyExchangeCurrencyTypeCode { Value = obj.CurrencyTypeCode };
			}
		}

		void PopulateCommodity(DeclarationConsignmentConsignmentItem bo, ICommodity obj)
		{
			if (obj != null && !obj.Name.IsEmpty)
			{
				bo.Commodity = new DeclarationConsignmentConsignmentItemCommodity();
				bo.Commodity.Name = new DeclarationConsignmentConsignmentItemCommodityName { Value = obj.Name };
			}
		}

		void PopulateGovernmentAgencyGoodsItemManufacturerCommunication(DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer bo, ICommunication obj)
		{
			if (obj != null)
			{
				bo.Communication = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunication();
				var newItem = bo.Communication;
				newItem.Id = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunicationId { Value = obj.ID };
				newItem.TypeId = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunicationTypeId { Value = obj.TypeID };
			}
		}

		void PopulateGovernmentAgencyGoodsItemManufacturerAddress(DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.CountrySubDivisionID, () => newItem.CountrySubDivisionId = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddressCountrySubDivisionId { Value = obj.CountrySubDivisionID });
				PopulateValueIfNodeValueIsNotEmpty(obj.CountrySubDivisionName, () => newItem.CountrySubDivisionName = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddressCountrySubDivisionName { Value = obj.CountrySubDivisionName });
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => newItem.Line = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerAddressLine { Value = obj.Line.Left(120) });
			}
		}

		void PopulateGovernmentAgencyGoodsItemManufacturer(DeclarationConsignmentGovernmentAgencyGoodsItem bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Manufacturer = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer();
				var newItem = bo.Manufacturer;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerId { Value = obj.ID });
				newItem.Name = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerName { Value = obj.Name };
				PopulateGovernmentAgencyGoodsItemManufacturerAddress(newItem, obj.Address);
				var communications = obj.Communications;
				if (communications != null && communications.Any())
				{
					PopulateGovernmentAgencyGoodsItemManufacturerCommunication(newItem, communications.First());
				}
				PopulateValueIfNodeValueIsNotEmpty(obj.ContactName, () => newItem.Contact = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerContact { Name = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerContactName { Value = obj.ContactName } });
			}
		}

		void PopulateConsignmentGovernmentAgencyGoodsItem(DeclarationConsignment bo, IGovernmentAgencyGoodsItem obj)
		{
			if (obj != null)
			{
				bo.GovernmentAgencyGoodsItem = new DeclarationConsignmentGovernmentAgencyGoodsItem();
				PopulateGovernmentAgencyGoodsItemManufacturer(bo.GovernmentAgencyGoodsItem, obj.Manufacturer);
			}
		}

		void PopulateOrigin(DeclarationConsignmentConsignmentItem bo, IOrigin obj)
		{
			if (obj != null)
			{
				bo.Origin = new DeclarationConsignmentConsignmentItemOrigin();
				bo.Origin.CountryCode = new DeclarationConsignmentConsignmentItemOriginCountryCode { Value = obj.CountryCode };
			}
		}

		void PopulateConsignmentItem(DeclarationConsignment bo, IConsignmentItem obj)
		{
			if (obj != null)
			{
				bo.ConsignmentItem = new DeclarationConsignmentConsignmentItem();
				var newItem = bo.ConsignmentItem;
				PopulateCommodity(newItem, obj.Commodity);
				PopulateOrigin(newItem, obj.Origin);
			}
		}

		void PopulateConsignment(Declaration bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationConsignment();
				PopulateConsignmentItem(bo.Consignment, obj.ConsignmentItem);
				PopulateConsignmentGovernmentAgencyGoodsItem(bo.Consignment, obj.GovernmentAgencyGoodsItem);
			}
		}

		void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				newItem.ArrivalDateTime = obj.ArrivalDateTime.ToISO8601ShortDateString();
				newItem.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateAgent(Declaration bo, IDeclarationAgent obj)
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

		void PopulateAdditionalDocument(Declaration bo, IDeclarationAdditionalDocument obj)
		{
			if (obj != null)
			{
				bo.AdditionalDocument = new DeclarationAdditionalDocument();
				bo.AdditionalDocument.Id = new DeclarationAdditionalDocumentId { Value = obj.ID };
			}
		}
	}
}
