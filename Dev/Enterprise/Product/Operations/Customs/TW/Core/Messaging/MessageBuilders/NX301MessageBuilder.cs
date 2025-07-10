using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX301;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX301MessageBuilder : BaseTWMessageBuilder<INX301Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX301Declaration input, string functionCode = null)
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
				PopulateCurrencyExchange(newItem, input.CurrencyExchange);
				PopulateGoodsShipment(newItem, input.GoodsShipment);
				PopulateImporter(newItem, input.Importer);
				PopulateApplication(newItem, input.Application);
			}
			return newItem;
		}

		DeclarationTwApplicationTwLabelTwLabelDetail NewLabelDetail(ILabelDetail labelDetail)
		{
			var newItem = new DeclarationTwApplicationTwLabelTwLabelDetail();
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.EndNumber, () => newItem.TwEndNumber = new DeclarationTwApplicationTwLabelTwLabelDetailTwEndNumber { Value = labelDetail.EndNumber.Left(8) });
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.StartNumber, () => newItem.TwStartNumber = new DeclarationTwApplicationTwLabelTwLabelDetailTwStartNumber { Value = labelDetail.StartNumber.Left(8) });
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.Track, () => newItem.TwTrack = new DeclarationTwApplicationTwLabelTwLabelDetailTwTrack { Value = labelDetail.Track.Left(3) });
			PopulateValueIfNodeValueIsNotEmpty(labelDetail.Year, () => newItem.TwYear = new DeclarationTwApplicationTwLabelTwLabelDetailTwYear { Value = labelDetail.Year.Left(3) });
			return newItem;
		}

		bool CanPopulateLabelDetail(ILabelDetail obj) => obj != null && (!obj.EndNumber.IsEmpty || !obj.StartNumber.IsEmpty || !obj.Track.IsEmpty || !obj.Year.IsEmpty);

		void PopulateLabelDetail(DeclarationTwApplicationTwLabel bo, IEnumerable<ILabelDetail> obj)
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

		void PopulateLabels(DeclarationTwApplication bo, IEnumerable<ILabel> objs)
		{
			if (objs != null)
			{
				var collection = new Collection<DeclarationTwApplicationTwLabel>();
				foreach (var obj in objs.Take(99))
				{
					var newItem = new DeclarationTwApplicationTwLabel();
					newItem.Status = new DeclarationTwApplicationTwLabelStatus() { NameCode = new DeclarationTwApplicationTwLabelStatusNameCode { Value = obj.StatusNameCode } };
					PopulateLabelDetail(newItem, obj.LabelDetails);
					collection.Add(newItem);
				}
				bo.TwLabel = collection;
			}
		}

		void PopulateApplicationDeclarerCommunication(DeclarationTwApplicationTwDeclarer bo, IEnumerable<ICommunication> objs)
		{
			if (objs != null)
			{
				var collection = new Collection<DeclarationTwApplicationTwDeclarerCommunication>();
				foreach (var item in objs.Take(2))
				{
					var newItem = new DeclarationTwApplicationTwDeclarerCommunication();
					newItem.Id = new DeclarationTwApplicationTwDeclarerCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationTwApplicationTwDeclarerCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
			}
		}

		void PopulateApplicationDeclarerAddress(DeclarationTwApplicationTwDeclarer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationTwDeclarerAddress { TwChineseLine = new DeclarationTwApplicationTwDeclarerAddressTwChineseLine { Value = obj.ChineseLine } };
			}
		}

		void PopulateApplicationDeclarer(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (bo != null)
			{
				bo.TwDeclarer = new DeclarationTwApplicationTwDeclarer();
				var newItem = bo.TwDeclarer;
				newItem.TwChineseName = new DeclarationTwApplicationTwDeclarerTwChineseName { Value = obj.ChineseName };
				newItem.TwId = new DeclarationTwApplicationTwDeclarerTwId { Value = obj.ID };
				newItem.TwName = new DeclarationTwApplicationTwDeclarerTwName { Value = obj.Name };
				newItem.TwTypeCode = new DeclarationTwApplicationTwDeclarerTwTypeCode { Value = obj.TypeCode };
				PopulateApplicationDeclarerAddress(newItem, obj.Address);
				PopulateApplicationDeclarerCommunication(newItem, obj.Communications);
			}
		}

		void PopulateAppointment(DeclarationTwApplication bo, IAppointment obj)
		{
			if (obj != null)
			{
				bo.TwAppointment = new DeclarationTwApplicationTwAppointment();
				var newItem = bo.TwAppointment;
				var reservationDate = obj.ReservationDate.IsValid ? obj.ReservationDate.ToDateTime() : new DateTime();
				newItem.TwReservationDate = reservationDate;
				newItem.TwReservationPeriodCode = new DeclarationTwApplicationTwAppointmentTwReservationPeriodCode { Value = obj.ReservationPeriodCode };
			}
		}

		void PopulateApplicationPayment(DeclarationTwApplication bo, IPayment obj)
		{
			if (obj != null)
			{
				bo.Payment = new DeclarationTwApplicationPayment() { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.MethodCode } };
			}
		}

		void PopulateApplicationAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationTwApplicationAgentId { Value = obj.ID };
				newItem.TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				PopulateValueIfNodeValueIsNotEmpty(obj.StatementDescription, () => newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription { Value = obj.StatementDescription });
				newItem.TwElectronicReceipt = new DeclarationTwApplicationAdditionalInformationTwElectronicReceipt { Value = obj.ElectronicReceipt };
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
			if (obj != null)
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
				PopulateValueIfNodeValueIsNotEmpty(obj.ApprovalAuthenticationInformation, () => newItem.TwApprovalAuthenticationInformation = new DeclarationTwApplicationTwApprovalAuthenticationInformation { TwId = new DeclarationTwApplicationTwApprovalAuthenticationInformationTwId { Value = obj.ApprovalAuthenticationInformation } });
				PopulateApplicationDeclarer(newItem, obj.Declarer);
				PopulateLabels(newItem, obj.Labels);
			}
		}

		void PopulateImporterAddress(DeclarationImporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationImporterAddress();
				bo.Address.TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.ChineseLine };
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
			}
		}

		void PopulateSellerAddress(DeclarationGoodsShipmentSeller bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentSellerAddress();
				bo.Address.CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.CountryCode };
			}
		}

		void PopulateSeller(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Seller = new DeclarationGoodsShipmentSeller();
				var newItem = bo.Seller;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentSellerId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentSellerTwChineseName { Value = obj.ChineseName });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentSellerTwTypeCode { Value = obj.TypeCode });
				PopulateSellerAddress(newItem, obj.Address);
			}
		}

		void PopulateShippingIdentification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IShippingIdentification> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification>();
				foreach (var shippingIdentification in obj.Take(99))
				{
					var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification();
					PopulateValueIfNodeValueIsNotEmpty(shippingIdentification.LotNumberID, () => item.TwLotNumberId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwLotNumberId { Value = shippingIdentification.LotNumberID });
					PopulateValueIfNodeValueIsValid(shippingIdentification.ProductManufacturedDate, () =>
					{
						item.TwProductManufacturedDateValueSpecified = true;
						item.TwProductManufacturedDate = shippingIdentification.ProductManufacturedDate.ToDateTime();
					});
					collection.Add(item);
				}
				bo.TwShippingIdentification = collection;
			}
		}

		void PopulateGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsLicensingStatisticalMeasure obj)
		{
			if (obj != null)
			{
				bo.TwGoodsLicensingStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasure();
				var newItem = bo.TwGoodsLicensingStatisticalMeasure;
				newItem.TwLicensingQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureTwLicensingQuantity { Value = obj.LicensingQuantity };
				newItem.TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode };
				newItem.ResponsibleGovernmentAgency = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureResponsibleGovernmentAgency { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasureResponsibleGovernmentAgencyId { Value = obj.ResponsibleGovernmentAgency } };
			}
		}

		void PopulateCommoditySpecification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommoditySpecification obj)
		{
			if (obj != null)
			{
				bo.TwCommoditySpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecification();
				var newItem = bo.TwCommoditySpecification;
				PopulateValueIfNodeValueIsNotEmpty(obj.CharacteristicQualifierCode, () => newItem.TwCharacteristicQualifierCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecificationTwCharacteristicQualifierCode { Value = obj.CharacteristicQualifierCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.ElementDescription, () => newItem.TwElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecificationTwElementDescription { Value = obj.ElementDescription });
			}
		}

		void PopulateApprovalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ILPCODetail obj)
		{
			if (obj != null)
			{
				bo.TwApprovalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument();
				var newItem = bo.TwApprovalDocument;
				PopulateValueIfNodeValueIsNotEmpty(obj.LPCOExemptionCode, () => newItem.TwLpcoExemptionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentTwLpcoExemptionCode { Value = obj.LPCOExemptionCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.LPCOID, () => newItem.TwLpcoid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentTwLpcoid { Value = obj.LPCOID });
				var authorizedParty = obj.LPCOAuthorizedParty;
				if (authorizedParty != null)
				{
					newItem.LpcoAuthorizedParty = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedParty
					{
						Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedPartyId { Value = authorizedParty.ID },
						TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedPartyTwTypeCode { Value = authorizedParty.TypeCode }
					};
				}
			}
		}

		void PopulatePackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging();
				var newItem = bo.Packaging;
				newItem.QuantityQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingQuantityQuantity { Value = obj.QuantityQuantity };
				newItem.TypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingTypeCode { Value = obj.TypeCode };
			}
		}

		void PopulateOrigin(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IOrigin obj)
		{
			if (obj != null)
			{
				bo.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin();
				bo.Origin.CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = obj.CountryCode };
			}
		}

		void PopulateManufacturerAddress(DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddress();
				bo.Address.Line = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddressLine { Value = obj.Line };
			}
		}

		void PopulateManufacturer(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () =>
				{
					bo.Manufacturer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer();
					var newItem = bo.Manufacturer;
					newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerName { Value = obj.Name };
					PopulateManufacturerAddress(newItem, obj.Address);
				});
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

		void PopulateCommodityPreviousDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IPreviousDocument obj)
		{
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.PreviousDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument();
				bo.PreviousDocument.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocumentId { Value = obj.ID };
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

		void PopulateCommodityConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			if (obj != null)
			{
				bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
				var newItem = bo.Constituent;
				PopulateValueIfNodeValueIsNotEmpty(obj.ElementDescription, () => newItem.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.LevelID, () => newItem.TwLevelId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwLevelId { Value = obj.LevelID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Thickness, () => newItem.TwThickness = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwThickness { Value = obj.Thickness });
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

		void PopulateGovernmentAgencyGoodsItemCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID });
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description };
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name });
				newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription { Value = obj.ChineseDescription };
				PopulateValueIfNodeValueIsNotEmpty(obj.TariffCodeExtensionCode, () => newItem.TwTariffCodeExtensionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwTariffCodeExtensionCode { Value = obj.TariffCodeExtensionCode });
				PopulateClassification(newItem, obj.Classification);
				PopulateCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
				PopulateCommodityConstituent(newItem, obj.Constituent);
				PopulateCommodityDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateCommodityPreviousDocument(newItem, obj.PreviousDocument);
			}
		}

		void PopulateGovernmentAgencyGoodsItemAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null)
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
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj.Take(9999))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateGovernmentAgencyGoodsItemAdditionalInformation(newItem, item.AdditionalInformations);
					PopulateGovernmentAgencyGoodsItemCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateManufacturer(newItem, item.Manufacturer);
					PopulateOrigin(newItem, item.Origin);
					PopulatePackaging(newItem, item.Packaging);
					PopulateApprovalDocument(newItem, item.ApprovalDocument);
					PopulateCommoditySpecification(newItem, item.CommoditySpecification);
					PopulateGoodsStatisticalMeasure(newItem, item.GoodsLicensingStatisticalMeasure);
					PopulateShippingIdentification(newItem, item.ShippingIdentifications);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateTransportContractDocuments(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null)
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
				PopulateValueIfNodeValueIsNotEmpty(obj.ManifestSerialNumber, () => newItem.TwManifestSerialNumber = new DeclarationGoodsShipmentConsignmentTwManifestSerialNumber { Value = obj.ManifestSerialNumber });
				newItem.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				PopulateValueIfNodeValueIsNotEmpty(obj.LoadingLocation?.ID ?? ZString.Empty, () => newItem.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation() { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.LoadingLocation?.ID ?? ZString.Empty } });
				PopulateTransportContractDocuments(newItem, obj.TransportContractDocuments);
				PopulateGoodsShipmentConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateTransportEquipment(newItem, obj.TransportEquipments);
			}
		}

		void PopulateTransportEquipment(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportEquipment> obj)
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

		void PopulateGoodsShipmentConsignmentBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.JourneyID.IsEmpty || !obj.Registration.IsEmpty))
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentBorderTransportMeansId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.JourneyID, () => newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => newItem.TwRegistration = new DeclarationGoodsShipmentConsignmentBorderTransportMeansTwRegistration { Value = obj.Registration });
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
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.AdditionalDocument = new DeclarationAdditionalDocument();
				bo.AdditionalDocument.Id = new DeclarationAdditionalDocumentId { Value = obj.ID };
			}
		}
	}
}
