using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX301_DN;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX301_DNMessageBuilder : BaseTWMessageBuilder<INX301_DNDeclaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX301_DNDeclaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode { Value = input.FunctionCode };
				newItem.Id = new DeclarationId { Value = input.ID };
				PopulateValueIfNodeValueIsNotEmpty(input.TypeCode, () => newItem.TypeCode = new DeclarationTypeCode { Value = input.TypeCode });
				PopulateAdditionalInformation(newItem, input.AdditionalDocument);
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

		void PopulateApplicationWinePreviousDocument(DeclarationTwApplicationTwWine bo, IPreviousDocument obj)
		{
			if (obj != null)
			{
				bo.PreviousDocument = new DeclarationTwApplicationTwWinePreviousDocument() { Id = new DeclarationTwApplicationTwWinePreviousDocumentId { Value = obj.ID.Left(14) } };
			}
		}

		void PopulateApplicationWineGovernmentProcedure(DeclarationTwApplicationTwWine bo, ZString obj)
		{
			bo.GovernmentProcedure = new DeclarationTwApplicationTwWineGovernmentProcedure() { PreviousCode = new DeclarationTwApplicationTwWineGovernmentProcedurePreviousCode { Value = obj.Left(1) } };
		}

		DeclarationTwApplicationTwWineAdditionalDocument NewWineAdditionalDocument(IAdditionalDocument obj) =>
			new DeclarationTwApplicationTwWineAdditionalDocument() { Id = new DeclarationTwApplicationTwWineAdditionalDocumentId { Value = obj.ID.Left(14) } };

		bool CanPopulateApplicationWineAdditionalDocument(IEnumerable<IAdditionalDocument> obj) => obj != null && obj.Any(x => !x.ID.IsEmpty);

		void PopulateApplicationWineAdditionalDocument(DeclarationTwApplicationTwWine bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (CanPopulateApplicationWineAdditionalDocument(obj))
			{
				var collection = new Collection<DeclarationTwApplicationTwWineAdditionalDocument>();
				obj.Where(x => !x.ID.IsEmpty).Take(10).ToList().ForEach(y => collection.Add(NewWineAdditionalDocument(y)));
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateApplicationWine(DeclarationTwApplication bo, IApplicationWine obj)
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

		void PopulateAuthorizedInformationAdditionalDocument(DeclarationTwApplicationTwAuthorizedInformation bo, IAdditionalDocument obj)
		{
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.AdditionalDocument = new DeclarationTwApplicationTwAuthorizedInformationAdditionalDocument() { Id = new DeclarationTwApplicationTwAuthorizedInformationAdditionalDocumentId { Value = obj.ID.Left(10) } };
			}
		}

		void PopulateAuthorizedInformation(DeclarationTwApplication bo, IAuthorizedInformation obj)
		{
			if (obj != null && !obj.AuthorizedTypeCode.IsEmpty)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.AuthorizedTypeCode, () => bo.TwAuthorizedInformation = new DeclarationTwApplicationTwAuthorizedInformation() { TwAuthorizedTypeCode = new DeclarationTwApplicationTwAuthorizedInformationTwAuthorizedTypeCode { Value = obj.AuthorizedTypeCode.Left(1) } });
				PopulateAuthorizedInformationAdditionalDocument(bo.TwAuthorizedInformation, obj.AdditionalDocument);
			}
		}

		void PopulateAppointment(DeclarationTwApplication bo, IAppointment obj)
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

		void PopulateApplicationPayment(DeclarationTwApplication bo, IPayment obj)
		{
			if (obj != null)
			{
				bo.Payment = new DeclarationTwApplicationPayment() { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.MethodCode } };
			}
		}

		void PopulateBankAccount(DeclarationTwApplication bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.BankAccount = new DeclarationTwApplicationBankAccount() { Id = new DeclarationTwApplicationBankAccountId { Value = obj } };
			}
		}

		void PopulateApplicationAgentCommunication(DeclarationTwApplicationAgent bo, IEnumerable<ICommunication> obj)
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

		void PopulateApplicationAgentAddress(DeclarationTwApplicationAgent bo, IAddress obj)
		{
			if (obj != null)
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
				newItem.Name = new DeclarationTwApplicationAgentName { Value = obj.Name.Left(70) };
				newItem.TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode };
				PopulateApplicationAgentAddress(newItem, obj.Address);
				PopulateApplicationAgentCommunication(newItem, obj.Communications);
			}
		}

		void PopulateApplicationAdditionalInformationAddress(DeclarationTwApplicationAdditionalInformation bo, IApplicationAdditionalInformation obj)
		{
			if (bo != null)
			{
				bo.Address = new DeclarationTwApplicationAdditionalInformationAddress { TwChineseLine = new DeclarationTwApplicationAdditionalInformationAddressTwChineseLine { Value = obj.AddressChineseLine } };
			}
		}

		void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription { Value = obj.StatementDescription };
				PopulateApplicationAdditionalInformationAddress(newItem, obj);
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
				foreach (var item in obj)
				{
					var newItem = new DeclarationTwApplicationAdditionalDocument();
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationTwApplicationAdditionalDocumentId { Value = item.ID });
					PopulateValueIfNodeValueIsNotEmpty(item.Content, () => newItem.TwContent = new DeclarationTwApplicationAdditionalDocumentTwContent { Value = item.Content });
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
				newItem.TwPurposeCode = new DeclarationTwApplicationTwPurposeCode { Value = obj.PurposeCode };
				newItem.TwTypeCode = new DeclarationTwApplicationTwTypeCode { Value = obj.TypeCode };
				PopulateApplicationAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateApplicationAdditionalInformation(newItem, obj.AdditionalInformation);
				PopulateApplicationAgent(newItem, obj.Agent);
				PopulateBankAccount(newItem, obj.BankAccount);
				PopulateApplicationPayment(newItem, obj.Payment);
				PopulateAppointment(newItem, obj.Appointment);
				PopulateAuthorizedInformation(newItem, obj.AuthorizedInformation);
				PopulateApplicationWine(newItem, obj.Wine);
			}
		}

		void PopulateImporterCommunication(DeclarationImporter bo, IEnumerable<ICommunication> obj)
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
				bo.Address.CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.CountryCode };
			}
		}

		void PopulateSeller(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Seller = new DeclarationGoodsShipmentSeller();
				PopulateSellerAddress(bo.Seller, obj.Address);
			}
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification NewShippingIdentification(IShippingIdentification obj)
		{
			var result = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification();
			PopulateValueIfNodeValueIsNotEmpty(obj.LotNumberID, () => result.TwLotNumberId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwLotNumberId { Value = obj.LotNumberID.Left(150) });
			PopulateValueIfNodeValueIsNotEmpty(obj.ProductLotNumberAmount, () => result.TwProductLotNumberAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwProductLotNumberAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ProductLotNumberAmount)) });
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

		void PopulateGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsStatisticalMeasure obj)
		{
			if (obj != null)
			{
				bo.TwGoodsStatisticalMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasure();
				var newItem = bo.TwGoodsStatisticalMeasure;
				newItem.TwStatisticalUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwStatisticalUnitCode { Value = obj.StatisticalUnitCode };
				newItem.TwTariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsStatisticalMeasureTwTariffQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TariffQuantity)) };
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
				newItem.TariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTariffQuantity { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.TariffQuantity)) };
				newItem.TwUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwUnitCode { Value = obj.UnitCode };
			}
		}

		void PopulateWine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IWine obj)
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
				bo.CommodityRelatedPackaging.TwSpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwSpecification { Value = obj.Specification };
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
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseDescription, () => newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription { Value = obj.ChineseDescription });
				newItem.TwEnglishDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwEnglishDescription { Value = obj.EnglishDescription };
				PopulateClassification(newItem, obj.Classification);
				PopulateCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
				PopulateCommodityDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateWine(newItem, obj.Wine);
			}
		}

		void PopulateGovernmentAgencyGoodsItemAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalInformation> obj)
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

		void PopulateGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateGovernmentAgencyGoodsItemAdditionalInformation(newItem, item.AdditionalInformations);
					PopulateGovernmentAgencyGoodsItemCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateManufacturer(newItem, item.Manufacturer);
					PopulatePackaging(newItem, item.Packaging);
					PopulateGoodsStatisticalMeasure(newItem, item.GoodsStatisticalMeasure);
					PopulatePreBondedDocument(newItem, item.PreBondedDocument);
					PopulateShippingIdentification(newItem, item.ShippingIdentifications);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
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

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				newItem.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				newItem.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation() { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.LoadingLocation?.ID ?? ZString.Empty } };
				PopulateTransportEquipment(newItem, obj.TransportEquipments);
			}
		}

		void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
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

		void PopulateCommodityRelatedPackaging(DeclarationConsignmentConsignmentItemCommodity bo, ICommodityRelatedPackaging obj)
		{
			if (obj != null)
			{
				bo.CommodityRelatedPackaging = new DeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging();
				var newItem = bo.CommodityRelatedPackaging;
				newItem.PackingMethodDescription = new DeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackagingPackingMethodDescription { Value = obj.PackingMethodDescription };
				newItem.TwMaterialCode = new DeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackagingTwMaterialCode { Value = obj.MaterialCode };
			}
		}

		void PopulateCommodity(DeclarationConsignmentConsignmentItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationConsignmentConsignmentItemCommodity();
				var newItem = bo.Commodity;
				PopulateValueIfNodeValueIsNotEmpty(obj.Description, () => newItem.Description = new DeclarationConsignmentConsignmentItemCommodityDescription { Value = obj.Description });
				newItem.GoodsGroupNameCode = new DeclarationConsignmentConsignmentItemCommodityGoodsGroupNameCode { Value = obj.GoodsGroupNameCode };
				PopulateCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
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

		void PopulateAdditionalInformation(Declaration bo, IDeclarationAdditionalDocument obj)
		{
			if (obj != null)
			{
				bo.AdditionalDocument = new DeclarationAdditionalDocument();
				bo.AdditionalDocument.Id = new DeclarationAdditionalDocumentId { Value = obj.ID };
			}
		}
	}
}
