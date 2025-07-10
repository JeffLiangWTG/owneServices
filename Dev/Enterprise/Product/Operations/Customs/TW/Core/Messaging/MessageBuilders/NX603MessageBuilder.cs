using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX603;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX603MessageBuilder : BaseTWMessageBuilder<INX603Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX603Declaration obj, string functionCode = null)
		{
			var newItem = new Declaration();
			if (obj != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = obj.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode { Value = obj.FunctionCode };
				newItem.Id = new DeclarationId { Value = obj.ID };
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TypeCode = new DeclarationTypeCode { Value = obj.TypeCode });
				newItem.AdditionalDocument = new DeclarationAdditionalDocument { Id = new DeclarationAdditionalDocumentId { Value = obj.AdditionalDocument?.ID ?? string.Empty } };
				PopulateAgent(newItem, obj.Agent);
				PopulateBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateCurrencyExchange(newItem, obj.CurrencyExchange);
				PopulateGoodsShipment(newItem, obj.GoodsShipment);
				PopulateImporter(newItem, obj.Importer);
				PopulateApplication(newItem, obj.Application);
			}
			return newItem;
		}

		void PopulateApplicationDeclarerCommunication(DeclarationTwApplicationTwDeclarer bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationTwApplicationTwDeclarerCommunication>();
				foreach (var item in obj.Take(2))
				{
					var declarerCommunication = new DeclarationTwApplicationTwDeclarerCommunication
					{
						Id = new DeclarationTwApplicationTwDeclarerCommunicationId { Value = item.ID },
						TypeId = new DeclarationTwApplicationTwDeclarerCommunicationTypeId { Value = item.TypeID }
					};
					collection.Add(declarerCommunication);
				}
				bo.Communication = collection;
			}
		}

		void PopulateApplicationDeclarer(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwDeclarer = new DeclarationTwApplicationTwDeclarer
				{
					TwChineseName = new DeclarationTwApplicationTwDeclarerTwChineseName { Value = obj.ChineseName },
					TwId = new DeclarationTwApplicationTwDeclarerTwId { Value = obj.ID },
					TwName = new DeclarationTwApplicationTwDeclarerTwName { Value = obj.Name },
					TwTypeCode = new DeclarationTwApplicationTwDeclarerTwTypeCode { Value = obj.TypeCode },
					Address = new DeclarationTwApplicationTwDeclarerAddress { TwChineseLine = new DeclarationTwApplicationTwDeclarerAddressTwChineseLine { Value = obj.Address?.ChineseLine.Left(100) ?? ZString.Empty } }
				};
				PopulateApplicationDeclarerCommunication(bo.TwDeclarer, obj.Communications);
			}
		}

		void PopulateApplicationAppointment(DeclarationTwApplication bo, IAppointment obj)
		{
			if (obj != null && obj.ReservationDate.IsValid && !obj.ReservationPeriodCode.IsEmpty)
			{
				bo.TwAppointment = new DeclarationTwApplicationTwAppointment
				{
					TwReservationDate = obj.ReservationDate.ToDateTime(),
					TwReservationPeriodCode = new DeclarationTwApplicationTwAppointmentTwReservationPeriodCode { Value = obj.ReservationPeriodCode }
				};
			}
		}

		void PopulateApplicationAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent
				{
					Id = new DeclarationTwApplicationAgentId { Value = obj.ID },
					TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode }
				};
			}
		}

		void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				PopulateValueIfNodeValueIsNotEmpty(obj.StatementDescription, () => newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription { Value = obj.StatementDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.ElectronicReceipt, () => newItem.TwElectronicReceipt = new DeclarationTwApplicationAdditionalInformationTwElectronicReceipt { Value = obj.ElectronicReceipt });
				PopulateValueIfNodeValueIsNotEmpty(obj.ProvedPaper, () => newItem.TwProvedPaper = new DeclarationTwApplicationAdditionalInformationTwProvedPaper { Value = obj.ProvedPaper });
			}
		}

		void PopulateApplicationAdditionalDocument(DeclarationTwApplication bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationTwApplicationAdditionalDocument>();
				foreach (var item in obj.Take(99))
				{
					var additionalDocument = new DeclarationTwApplicationAdditionalDocument();
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => additionalDocument.Id = new DeclarationTwApplicationAdditionalDocumentId { Value = item.ID });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileFormat, () => additionalDocument.TwImageFileFormat = new DeclarationTwApplicationAdditionalDocumentTwImageFileFormat { Value = item.ImageFileFormat });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileName, () => additionalDocument.TwImageFileName = new DeclarationTwApplicationAdditionalDocumentTwImageFileName { Value = item.ImageFileName });
					additionalDocument.TypeCode = new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = item.TypeCode };
					PopulateValueIfNodeValueIsNotEmpty(item.ResponsibleGovernmentAgency, () => additionalDocument.ResponsibleGovernmentAgency = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgency
					{
						Id = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgencyId { Value = item.ResponsibleGovernmentAgency }
					});
					collection.Add(additionalDocument);
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateApplication(Declaration bo, IApplication obj)
		{
			if (obj != null)
			{
				bo.TwApplication = new DeclarationTwApplication
				{
					TwTypeCode = new DeclarationTwApplicationTwTypeCode { Value = obj.TypeCode },
					ContactOffice = new DeclarationTwApplicationContactOffice { Id = new DeclarationTwApplicationContactOfficeId { Value = obj.ContactOffice } },
					Payment = new DeclarationTwApplicationPayment { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.Payment?.MethodCode ?? ZString.Empty } }
				};
				var newItem = bo.TwApplication;
				PopulateApplicationAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateApplicationAdditionalInformation(newItem, obj.AdditionalInformation);
				PopulateApplicationAgent(newItem, obj.Agent);
				PopulateApplicationAppointment(newItem, obj.Appointment);
				PopulateApplicationDeclarer(newItem, obj.Declarer);
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
				newItem.Address = new DeclarationImporterAddress { TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.Address?.ChineseLine.Left(100) ?? ZString.Empty } };
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IConstituent obj)
		{
			if (obj != null)
			{
				var elementDescription = obj.ElementDescription;
				var levelID = obj.LevelID;
				var thickness = obj.Thickness;
				if (!elementDescription.IsEmpty || !levelID.IsEmpty || !thickness.IsEmpty)
				{
					bo.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
					var newItem = bo.Constituent;
					PopulateValueIfNodeValueIsNotEmpty(elementDescription, () => newItem.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = elementDescription });
					PopulateValueIfNodeValueIsNotEmpty(levelID, () => newItem.TwLevelId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwLevelId { Value = levelID });
					PopulateValueIfNodeValueIsNotEmpty(thickness, () => newItem.TwThickness = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwThickness { Value = thickness });
				}
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			if (obj != null)
			{
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.CommercialCategorizationID, () => newItem.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId { Value = obj.CommercialCategorizationID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Description, () => newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description });
				newItem.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = obj.Name };
				newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription { Value = obj.ChineseDescription };
				newItem.TwEnglishDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwEnglishDescription { Value = obj.EnglishDescription };
				PopulateValueIfNodeValueIsNotEmpty(obj.TariffCodeExtensionCode, () => newItem.TwTariffCodeExtensionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwTariffCodeExtensionCode { Value = obj.TariffCodeExtensionCode });
				newItem.Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationId { Value = obj.Classification?.ID ?? string.Empty } };
				PopulateGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent(newItem, obj.Constituent);
				newItem.DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee { AdValoremTaxBaseAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeAdValoremTaxBaseAmount { Value = obj.DutyTaxFee?.AdValoremTaxBaseAmount ?? decimal.Zero } };
				PopulateValueIfNodeValueIsNotEmpty(obj.PreviousDocument?.ID ?? ZString.Empty, () => newItem.PreviousDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocumentId { Value = obj.PreviousDocument.ID } });
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
				foreach (var item in obj.Take(10))
				{
					var additionalInformation = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
					{
						StatementCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementCode { Value = item.StatementCode },
						StatementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementDescription { Value = item.StatementDescription }
					};
					collection.Add(additionalInformation);
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsMeasure obj)
		{
			if (obj != null)
			{
				bo.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure
				{
					NetWeightMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureNetWeightMeasure { Value = obj.NetWeightMeasure },
					TariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTariffQuantity { Value = obj.TariffQuantity },
					TwUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwUnitCode { Value = obj.UnitCode }
				};
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemManufacturer(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Manufacturer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer
				{
					Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerName { Value = obj.Name }
				};
				var newItem = bo.Manufacturer;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Address?.Line ?? ZString.Empty, () => newItem.Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddress
				{
					Line = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerAddressLine { Value = obj.Address.Line.Left(100) }
				});
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemOrigin(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IOrigin obj)
		{
			if (obj != null)
			{
				bo.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin { CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = obj.CountryCode } };
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemPackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging
				{
					QuantityQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingQuantityQuantity { Value = obj.QuantityQuantity },
					TypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackagingTypeCode { Value = obj.TypeCode }
				};
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemCommoditySpecification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommoditySpecification obj)
		{
			if (obj != null && (!obj.CharacteristicQualifierCode.IsEmpty || !obj.ElementDescription.IsEmpty))
			{
				bo.TwCommoditySpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecification();
				var newItem = bo.TwCommoditySpecification;
				PopulateValueIfNodeValueIsNotEmpty(obj.CharacteristicQualifierCode, () => newItem.TwCharacteristicQualifierCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecificationTwCharacteristicQualifierCode { Value = obj.CharacteristicQualifierCode });
				PopulateValueIfNodeValueIsNotEmpty(obj.ElementDescription, () => newItem.TwElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwCommoditySpecificationTwElementDescription { Value = obj.ElementDescription });
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsStatisticalMeasure obj)
		{
			if (obj != null)
			{
				bo.TwGoodsStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasure
				{
					TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode },
					TwTariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwTariffQuantity { Value = obj.TariffQuantity }
				};
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemMedicalInstrument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ILPCODetail obj)
		{
			if (obj != null)
			{
				bo.TwMedicalInstrument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument { TwLpcoid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentTwLpcoid { Value = obj.LPCOID } };
				var party = obj.LPCOAuthorizedParty;
				if (party != null && !party.ID.IsEmpty && !party.TypeCode.IsEmpty)
				{
					bo.TwMedicalInstrument.LpcoAuthorizedParty = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedParty
					{
						Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedPartyId { Value = party.ID },
						TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedPartyTwTypeCode { Value = party.TypeCode }
					};
				}
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemShippingIdentifications(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IShippingIdentification> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification>();
				foreach (var item in obj.Take(99))
				{
					var identification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification();
					PopulateValueIfNodeValueIsNotEmpty(item.LotNumberID, () => identification.TwLotNumberId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwLotNumberId { Value = item.LotNumberID });
					PopulateValueIfNodeValueIsValid(item.ProductBestBeforeDateTime, () => identification.TwProductBestBeforeDateTime = item.ProductBestBeforeDateTime.ToISO8601ShortDateString());
					PopulateValueIfNodeValueIsValid(item.ProductManufacturedDate, () => identification.TwProductManufacturedDate = item.ProductManufacturedDate.ToDateTime());
					collection.Add(identification);
				}
				bo.TwShippingIdentification = collection;
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj.Take(9999))
				{
					var governmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem { SequenceNumeric = item.SequenceNumeric };
					PopulateGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation(governmentAgencyGoodsItem, item.AdditionalInformations);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemCommodity(governmentAgencyGoodsItem, item.Commodity);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure(governmentAgencyGoodsItem, item.GoodsMeasure);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemManufacturer(governmentAgencyGoodsItem, item.Manufacturer);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemOrigin(governmentAgencyGoodsItem, item.Origin);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemPackaging(governmentAgencyGoodsItem, item.Packaging);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemCommoditySpecification(governmentAgencyGoodsItem, item.CommoditySpecification);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemGoodsStatisticalMeasure(governmentAgencyGoodsItem, item.GoodsStatisticalMeasure);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemMedicalInstrument(governmentAgencyGoodsItem, item.MedicalInstrument);
					PopulateGoodsShipmentGovernmentAgencyGoodsItemShippingIdentifications(governmentAgencyGoodsItem, item.ShippingIdentifications);
					collection.Add(governmentAgencyGoodsItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportEquipment> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipment>();
				foreach (var item in obj.Take(9999))
				{
					var transportContractDocument = new DeclarationGoodsShipmentConsignmentTransportEquipment
					{
						Id = new DeclarationGoodsShipmentConsignmentTransportEquipmentId { Value = item.ID }
					};
					collection.Add(transportContractDocument);
				}
				bo.TransportEquipment = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentTransportContractDocument(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument>();
				foreach (var item in obj.Take(2))
				{
					var transportContractDocument = new DeclarationGoodsShipmentConsignmentTransportContractDocument
					{
						Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId { Value = item.ID },
						TypeCode = new DeclarationGoodsShipmentConsignmentTransportContractDocumentTypeCode { Value = item.TypeCode }
					};
					collection.Add(transportContractDocument);
				}
				bo.TransportContractDocument = collection;
			}
		}

		void PopulateGoodsShipmentConsignmentBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.JourneyID.IsEmpty))
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentBorderTransportMeansId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.JourneyID, () => newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId { Value = obj.JourneyID });
			}
		}

		void PopulateGoodsShipmentConsignmentAdditionalInformation(DeclarationGoodsShipmentConsignment bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null && obj.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation>();
				foreach (var item in obj.Take(10))
				{
					var additionalInformation = new DeclarationGoodsShipmentConsignmentAdditionalInformation
					{
						StatementCode = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementCode { Value = item.StatementCode },
						StatementDescription = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementDescription { Value = item.StatementDescription }
					};
					collection.Add(additionalInformation);
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.ManifestSerialNumber, () => newItem.TwManifestSerialNumber = new DeclarationGoodsShipmentConsignmentTwManifestSerialNumber { Value = obj.ManifestSerialNumber });
				PopulateGoodsShipmentConsignmentAdditionalInformation(newItem, obj.AdditionalInformations);
				PopulateGoodsShipmentConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				newItem.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				newItem.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.LoadingLocation?.ID ?? string.Empty } };
				PopulateGoodsShipmentConsignmentTransportContractDocument(newItem, obj.TransportContractDocuments);
				PopulateGoodsShipmentConsignmentTransportEquipment(newItem, obj.TransportEquipments);
			}
		}

		void PopulateGoodsShipmentSeller(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Seller = new DeclarationGoodsShipmentSeller();
				var newItem = bo.Seller;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentSellerId { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentSellerTwChineseName { Value = obj.ChineseName });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentSellerTwTypeCode { Value = obj.TypeCode });
				newItem.Address = new DeclarationGoodsShipmentSellerAddress { CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.Address?.CountryCode ?? string.Empty } };
			}
		}

		void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				newItem.ExitDateTime = obj.ExitDateTime.ToISO8601ShortDateString();
				PopulateGoodsShipmentConsignment(newItem, obj.Consignment);
				PopulateGoodsShipmentGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
				PopulateGoodsShipmentSeller(newItem, obj.Seller);
			}
		}

		void PopulateCurrencyExchange(Declaration bo, ICurrencyExchange obj)
		{
			if (obj != null)
			{
				bo.CurrencyExchange = new DeclarationCurrencyExchange();
				var newItem = bo.CurrencyExchange;
				newItem.CurrencyTypeCode = new DeclarationCurrencyExchangeCurrencyTypeCode { Value = obj.CurrencyTypeCode };
				newItem.RateNumeric = obj.RateNumeric;
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
	}
}
