using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX601;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX601MessageBuilder : BaseTWMessageBuilder<INX601Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX601Declaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode { Value = input.FunctionCode };
				newItem.Id = new DeclarationId { Value = input.ID };
				newItem.TypeCode = new DeclarationTypeCode { Value = input.TypeCode };
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

		void PopulateOrigin(DeclarationConsignmentConsignmentItem bo, IOrigin obj)
		{
			if (obj != null)
			{
				bo.Origin = new DeclarationConsignmentConsignmentItemOrigin();
				bo.Origin.CountryCode = new DeclarationConsignmentConsignmentItemOriginCountryCode { Value = obj.CountryCode };
			}
		}

		void PopulateCommodity(DeclarationConsignmentConsignmentItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationConsignmentConsignmentItemCommodity();
				bo.Commodity.Name = new DeclarationConsignmentConsignmentItemCommodityName { Value = obj.Name };
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
		void PopulateGovernmentAgencyGoodsItemManufacturerCommunication(DeclarationConsignmentGovernmentAgencyGoodsItemManufacturer bo, IEnumerable<ICommunication> objs)
		{
			if (objs != null && objs.Any())
			{
				var obj = objs.First();
				bo.Communication = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunication();
				var newItem = bo.Communication;
				newItem.Id = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunicationId { Value = obj.ID };
				newItem.TypeId = new DeclarationConsignmentGovernmentAgencyGoodsItemManufacturerCommunicationTypeId { Value = obj.TypeID };
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
				PopulateGovernmentAgencyGoodsItemManufacturerCommunication(newItem, obj.Communications);
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

		void PopulateConsignment(Declaration bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationConsignment();
				PopulateConsignmentItem(bo.Consignment, obj.ConsignmentItem);
				PopulateConsignmentGovernmentAgencyGoodsItem(bo.Consignment, obj.GovernmentAgencyGoodsItem);
			}
		}

		void PopulateAppointment(DeclarationTwApplication bo, IAppointment obj)
		{
			if (obj != null && (!obj.ReservationPeriodCode.IsEmpty || obj.ReservationDate.IsValid))
			{
				bo.TwAppointment = new DeclarationTwApplicationTwAppointment();
				var newItem = bo.TwAppointment;
				PopulateValueIfNodeValueIsNotEmpty(obj.ReservationPeriodCode, () => newItem.TwReservationPeriodCode = new DeclarationTwApplicationTwAppointmentTwReservationPeriodCode { Value = obj.ReservationPeriodCode });
				PopulateValueIfNodeValueIsValid(obj.ReservationDate, () => newItem.TwReservationDate = obj.ReservationDate.ToDateTime());
			}
		}

		void PopulateApplicationPayment(DeclarationTwApplication bo, IPayment obj)
		{
			if (obj != null)
			{
				bo.Payment = new DeclarationTwApplicationPayment() { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.MethodCode } };
			}
		}

		void PopulateApplicationAgentCommunications(DeclarationTwApplicationAgent bo, IEnumerable<ICommunication> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationTwApplicationAgentCommunication>();
				foreach (var item in objs.Take(2))
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
				PopulateApplicationAgentCommunications(newItem, obj.Communications);
			}
		}

		void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				PopulateValueIfNodeValueIsNotEmpty(obj.StatementDescription, () => newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription { Value = obj.StatementDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.DeductionSample, () => newItem.TwDeductionSample = new DeclarationTwApplicationAdditionalInformationTwDeductionSample { Value = obj.DeductionSample });
				PopulateValueIfNodeValueIsNotEmpty(obj.ProvedPaper, () => newItem.TwProvedPaper = new DeclarationTwApplicationAdditionalInformationTwProvedPaper { Value = obj.ProvedPaper });
				PopulateValueIfNodeValueIsNotEmpty(obj.ReturnSample, () => newItem.TwReturnSample = new DeclarationTwApplicationAdditionalInformationTwReturnSample { Value = obj.ReturnSample });
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
					PopulateValueIfNodeValueIsNotEmpty(item.TypeCode, () => newItem.TypeCode = new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = item.TypeCode });
					PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateApplicationLocalManufacturerAddress(DeclarationTwApplicationTwLocalManufacturer bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationTwLocalManufacturerAddress() { TwChineseLine = new DeclarationTwApplicationTwLocalManufacturerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) } };
			}
		}

		void PopulateApplicationLocalManufacturerCommunication(DeclarationTwApplicationTwLocalManufacturer bo, ICommunication obj)
		{
			if (obj != null)
			{
				bo.Communication = new DeclarationTwApplicationTwLocalManufacturerCommunication();
				bo.Communication.Id = new DeclarationTwApplicationTwLocalManufacturerCommunicationId { Value = obj.ID };
			}
		}

		void PopulateApplicationLocalManufacturer(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwLocalManufacturer = new DeclarationTwApplicationTwLocalManufacturer();
				var newItem = bo.TwLocalManufacturer;
				newItem.TwChineseName = new DeclarationTwApplicationTwLocalManufacturerTwChineseName { Value = obj.ChineseName };
				PopulateApplicationLocalManufacturerAddress(newItem, obj.Address);
				var communications = obj.Communications;
				if (communications != null && communications.Any())
				{
					PopulateApplicationLocalManufacturerCommunication(newItem, communications.First());
				}
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
				PopulateApplicationLocalManufacturer(newItem, obj.LocalManufacturer);
			}
		}

		void PopulateImporterAddress(DeclarationImporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationImporterAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationImporterAddressLine { Value = obj.Line.Left(120) };
				newItem.TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) };
			}
		}

		void PopulateImporterCommunications(DeclarationImporter bo, IEnumerable<ICommunication> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationImporterCommunication>();
				foreach (var item in objs.Take(2))
				{
					var newItem = new DeclarationImporterCommunication();
					newItem.Id = new DeclarationImporterCommunicationId { Value = item.ID };
					newItem.TypeId = new DeclarationImporterCommunicationTypeId { Value = item.TypeID };
					collection.Add(newItem);
				}
				bo.Communication = collection;
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
				PopulateImporterCommunications(newItem, obj.Communications);
			}
		}

		void PopulateSellerAddress(DeclarationGoodsShipmentSeller bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentSellerAddress();
				var newItem = bo.Address;
				newItem.CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.CountryCode };
				newItem.Line = new DeclarationGoodsShipmentSellerAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentSellerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateSellerCommunication(DeclarationGoodsShipmentSeller bo, ICommunication obj)
		{
			if (obj != null)
			{
				bo.Communication = new DeclarationGoodsShipmentSellerCommunication();
				var newItem = bo.Communication;
				newItem.Id = new DeclarationGoodsShipmentSellerCommunicationId { Value = obj.ID };
				newItem.TypeId = new DeclarationGoodsShipmentSellerCommunicationTypeId { Value = obj.TypeID };
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
				PopulateSellerCommunication(newItem, obj.Communications?.FirstOrDefault());
				PopulateValueIfNodeValueIsNotEmpty(obj.ContactName, () => newItem.Contact = new DeclarationGoodsShipmentSellerContact { Name = new DeclarationGoodsShipmentSellerContactName { Value = obj.ContactName } });
			}
		}

		void PopulateShippingIdentification(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IShippingIdentification> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification>();
				foreach (var item in objs.Take(99))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentification();
					PopulateValueIfNodeValueIsNotEmpty(item.LotNumberID, () => newItem.TwLotNumberId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwShippingIdentificationTwLotNumberId { Value = item.LotNumberID });
					PopulateValueIfNodeValueIsNotEmpty(item.ProductBestBeforeDateTime, () => newItem.TwProductBestBeforeDateTime = item.ProductBestBeforeDateTime.ToString());
					PopulateValueIfNodeValueIsValid(item.ProductManufacturedDate, () =>
					{
						newItem.TwProductManufacturedDateValueSpecified = true;
						newItem.TwProductManufacturedDate = item.ProductManufacturedDate.ToDateTime();
					});
					collection.Add(newItem);
				}
				bo.TwShippingIdentification = collection;
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

		void PopulateFoodConstituents(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood bo, IEnumerable<IFoodConstituent> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFoodConstituent>();
				foreach (var item in objs.Take(99))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFoodConstituent();
					newItem.ElementName = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFoodConstituentElementName { Value = item.ElementName };
					PopulateValueIfNodeValueHasValue(item.ElementPercentNumeric, () => newItem.ElementPercentNumeric = item.ElementPercentNumeric);
					collection.Add(newItem);
				}
				bo.Constituent = collection;
			}
		}

		void PopulateCommodityFood(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IFood obj)
		{
			if (obj != null)
			{
				bo.TwFood = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood();
				var newItem = bo.TwFood;
				PopulateValueIfNodeValueHasValue(obj.PHValueNumeric, () => newItem.TwPhValueNumeric = obj.PHValueNumeric.Value);
				PopulateValueIfNodeValueHasValue(obj.SterilizationValueNumeric, () => newItem.TwSterilizationValueNumeric = obj.SterilizationValueNumeric.Value);
				PopulateFoodConstituents(newItem, obj.Constituents);
			}
		}

		void PopulateCommodityHandling(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IEnumerable<ZString> objs)
		{
			if (objs != null && objs.Any())
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandling>();
				foreach (var item in objs.Take(9))
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandling();
					newItem.InstructionsCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandlingInstructionsCode { Value = item };
					collection.Add(newItem);
				}
				bo.Handling = collection;
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
				bo.Constituent.ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription { Value = obj.ElementDescription };
			}
		}

		void PopulateCommodityRelatedPackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityRelatedPackaging obj)
		{
			if (obj != null)
			{
				bo.CommodityRelatedPackaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackaging();
				var newItem = bo.CommodityRelatedPackaging;
				newItem.PackingMethodDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingPackingMethodDescription { Value = obj.PackingMethodDescription };
				newItem.TwMaterialCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwMaterialCode { Value = obj.MaterialCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.Specification, () => newItem.TwSpecification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwSpecification { Value = obj.Specification });
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
				PopulateValueIfNodeValueIsNotEmpty(obj.Description, () => newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description });
				newItem.GoodsGroupNameCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsGroupNameCode { Value = obj.GoodsGroupNameCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.BarCode, () => newItem.TwBarCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwBarCode { Value = obj.BarCode });
				newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription { Value = obj.ChineseDescription };
				newItem.TwEnglishDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwEnglishDescription { Value = obj.EnglishDescription };
				PopulateValueIfNodeValueIsNotEmpty(obj.TariffCodeExtensionCode, () => newItem.TwTariffCodeExtensionCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwTariffCodeExtensionCode { Value = obj.TariffCodeExtensionCode });
				PopulateClassification(newItem, obj.Classification);
				PopulateCommodityRelatedPackaging(newItem, obj.CommodityRelatedPackaging);
				PopulateCommodityConstituent(newItem, obj.Constituent);
				PopulateCommodityDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateCommodityHandling(newItem, obj.HandlingInstructionsCodes);
				PopulateCommodityFood(newItem, obj.Food);
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
					PopulateShippingIdentification(newItem, item.ShippingIdentifications);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateTransportContractDocuments(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportContractDocument> obj)
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

		void PopulateGoodsShipmentConsignmentLoadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			if (obj != null)
			{
				bo.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation();
				bo.LoadingLocation.Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = obj.ID };
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
				PopulateValueIfNodeValueIsNotEmpty(obj.ManifestSerialNumber, () => newItem.TwManifestSerialNumber = new DeclarationGoodsShipmentConsignmentTwManifestSerialNumber { Value = obj.ManifestSerialNumber });
				PopulateGoodsShipmentConsignmentBorderTransportMeans(newItem, obj.BorderTransportMeans);
				newItem.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId { Value = obj.GoodsLocation } };
				PopulateGoodsShipmentConsignmentLoadingLocation(newItem, obj.LoadingLocation);
				PopulateTransportContractDocuments(newItem, obj.TransportContractDocuments);
				PopulateGoodsShipmentConsignmentTransportEquipment(newItem, obj.TransportEquipments);
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
