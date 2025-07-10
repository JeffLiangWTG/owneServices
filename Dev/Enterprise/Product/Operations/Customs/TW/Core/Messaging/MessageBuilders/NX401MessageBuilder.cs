using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX401;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX401MessageBuilder : BaseTWMessageBuilder<INX401Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX401Declaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				PopulateValueIfNodeValueIsValid(input.AcceptanceDateTime, () => newItem.AcceptanceDateTime = input.AcceptanceDateTime.ToISO8601ShortDateString());
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId() { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode() { Value = MessageFunctionCode.Add };
				newItem.Id = new DeclarationId { Value = input.ID };
				PopulateValueIfNodeValueIsNotEmpty(input.TypeCode, () => newItem.TypeCode = new DeclarationTypeCode() { Value = input.TypeCode });
				PopulateAdditionalDocument(newItem, input.AdditionalDocument);
				PopulateAdditionalInformation(newItem, input.AdditionalInformation);
				PopulateAgent(newItem, input.Agent);
				PopulateBorderTransportMeans(newItem, input.BorderTransportMeans);
				PopulateCurrencyExchange(newItem, input.CurrencyExchange);
				PopulateGoodsShipment(newItem, input.GoodsShipment);
				PopulateImporter(newItem, input.Importer);
				PopulatePackaging(newItem, input.Packaging);
				PopulateApplication(newItem, input.Application);
			}
			return newItem;
		}

		void PopulateApplication(Declaration bo, IApplication obj)
		{
			if (obj != null)
			{
				bo.TwApplication = new DeclarationTwApplication();
				var newItem = bo.TwApplication;
				newItem.TwTypeCode = new DeclarationTwApplicationTwTypeCode() { Value = obj.TypeCode };
				PopulateApplicationAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateApplicationAdditionalInformation(newItem, obj.AdditionalInformation);
				PopulateApplicationAgent(newItem, obj.Agent);
				PopulateContactOffice(newItem, obj.ContactOffice);
				PopulateApplicationPayment(newItem, obj.Payment);
				PopulateAppointment(newItem, obj.Appointment);
				PopulateAuthorizedInformation(newItem, obj.AuthorizedInformation);
			}
		}

		public void PopulateAuthorizedInformation(DeclarationTwApplication bo, IAuthorizedInformation obj)
		{
			if (obj != null)
			{
				bo.TwAuthorizedInformation = new DeclarationTwApplicationTwAuthorizedInformation();
				PopulateAuthorizedInformationAdditionalDocument(bo.TwAuthorizedInformation, obj.AdditionalDocument);
			}
		}

		public void PopulateAuthorizedInformationAdditionalDocument(DeclarationTwApplicationTwAuthorizedInformation bo, IAdditionalDocument obj)
		{
			if (obj != null && !obj.ID.IsEmpty)
			{
				bo.AdditionalDocument = new DeclarationTwApplicationTwAuthorizedInformationAdditionalDocument() { Id = new DeclarationTwApplicationTwAuthorizedInformationAdditionalDocumentId { Value = obj.ID } };
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
					newItem.TwReservationDate = obj.ReservationDate.ToDateTime();
				});
				PopulateValueIfNodeValueIsNotEmpty(obj.ReservationPeriodCode, () => newItem.TwReservationPeriodCode = new DeclarationTwApplicationTwAppointmentTwReservationPeriodCode { Value = obj.ReservationPeriodCode });
			}
		}

		void PopulateApplicationPayment(DeclarationTwApplication bo, IPayment obj)
		{
			if (obj != null)
			{
				bo.Payment = new DeclarationTwApplicationPayment() { MethodCode = new DeclarationTwApplicationPaymentMethodCode { Value = obj.MethodCode } };
			}
		}

		void PopulateContactOffice(DeclarationTwApplication bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ContactOffice = new DeclarationTwApplicationContactOffice() { Id = new DeclarationTwApplicationContactOfficeId { Value = obj } };
			}
		}

		void PopulateApplicationAgent(DeclarationTwApplication bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationTwApplicationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationTwApplicationAgentId { Value = obj.ID };
				newItem.Name = new DeclarationTwApplicationAgentName { Value = obj.Name };
				newItem.TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = obj.TypeCode };
				PopulateApplicationAgentAddress(newItem, obj.Address);
				PopulateApplicationAgentCommunication(newItem, obj.Communications);
			}
		}

		void PopulateApplicationAgentAddress(DeclarationTwApplicationAgent bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationTwApplicationAgentAddress() { TwChineseLine = new DeclarationTwApplicationAgentAddressTwChineseLine { Value = obj.ChineseLine.Left(100) } };
			}
		}

		void PopulateApplicationAgentCommunication(DeclarationTwApplicationAgent bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
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

		void PopulateApplicationAdditionalInformation(DeclarationTwApplication bo, IApplicationAdditionalInformation obj)
		{
			if (obj != null)
			{
				bo.AdditionalInformation = new DeclarationTwApplicationAdditionalInformation();
				var newItem = bo.AdditionalInformation;
				PopulateValueIfNodeValueIsNotEmpty(obj.StatementDescription, () => newItem.StatementDescription = new DeclarationTwApplicationAdditionalInformationStatementDescription() { Value = obj.StatementDescription });
				PopulateValueIfNodeValueIsNotEmpty(obj.ProvedPaper, () => newItem.TwProvedPaper = new DeclarationTwApplicationAdditionalInformationTwProvedPaper { Value = obj.ProvedPaper });
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
					PopulateValueIfNodeValueIsNotEmpty(item.Content, () => newItem.TwContent = new DeclarationTwApplicationAdditionalDocumentTwContent { Value = item.Content });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileFormat, () => newItem.TwImageFileFormat = new DeclarationTwApplicationAdditionalDocumentTwImageFileFormat() { Value = item.ImageFileFormat });
					PopulateValueIfNodeValueIsNotEmpty(item.ImageFileName, () => newItem.TwImageFileName = new DeclarationTwApplicationAdditionalDocumentTwImageFileName { Value = item.ImageFileName });
					PopulateValueIfNodeValueIsNotEmpty(item.TypeCode, () => new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = item.TypeCode });
					PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateApplicationAdditionalDocumentResponsibleGovernmentAgency(DeclarationTwApplicationAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationTwApplicationAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		void PopulatePackaging(Declaration bo, IPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationPackaging();
				bo.Packaging.MarksNumbers = new DeclarationPackagingMarksNumbers() { Value = obj.MarksNumbers };
			}
		}

		void PopulateImporter(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Importer = new DeclarationImporter();
				var newItem = bo.Importer;
				newItem.Id = new DeclarationImporterId() { Value = obj.ID };
				newItem.Name = new DeclarationImporterName() { Value = obj.Name };
				newItem.TwChineseName = new DeclarationImporterTwChineseName() { Value = obj.ChineseName };
				newItem.TwTypeCode = new DeclarationImporterTwTypeCode() { Value = obj.TypeCode };
				PopulateImporterAddress(newItem, obj.Address);
				PopulateImporterCommunication(newItem, obj.Communications);
			}
		}

		void PopulateImporterCommunication(DeclarationImporter bo, IEnumerable<ICommunication> obj)
		{
			if (obj != null)
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
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationImporterAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				PopulateValueIfNodeValueIsValid(obj.ExitDateTime, () => newItem.ExitDateTime = obj.ExitDateTime.ToISO8601ShortDateString());
				PopulateValueIfNodeValueIsNotEmpty(obj.ItemChargeAmount, () => newItem.TwItemChargeAmount = new DeclarationGoodsShipmentTwItemChargeAmount { Value = obj.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(obj.ItemChargeAmount)) });
				PopulateConsignee(newItem, obj.Consignee);
				PopulateGoodsShipmentConsignment(newItem, obj.Consignment);
				PopulateExporter(newItem, obj.Exporter);
				PopulateGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
				PopulateSeller(newItem, obj.Seller);
			}
		}

		void PopulateSeller(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Seller = new DeclarationGoodsShipmentSeller();
				var newItem = bo.Seller;
				newItem.Name = new DeclarationGoodsShipmentSellerName { Value = obj.Name };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentSellerTwChineseName { Value = obj.ChineseName });
				PopulateSellerAddress(newItem, obj.Address);
			}
		}

		void PopulateSellerAddress(DeclarationGoodsShipmentSeller bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentSellerAddress();
				var newItem = bo.Address;
				PopulateValueIfNodeValueIsNotEmpty(obj.CountryCode, () => newItem.CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = obj.CountryCode });
				newItem.Line = new DeclarationGoodsShipmentSellerAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentSellerAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
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
					PopulateGovernmentAgencyGoodsItemAdditionalDocument(newItem, item.AdditionalDocuments);
					PopulateGovernmentAgencyGoodsItemAdditionalInformation(newItem, item.AdditionalInformations);
					PopulateCommodity(newItem, item.Commodity);
					PopulateGoodsMeasure(newItem, item.GoodsMeasure);
					PopulateOrigin(newItem, item.Origin);
					PopulatePackaging(newItem, item.Packaging);
					PopulateGoodsStatisticalMeasure(newItem, item.GoodsStatisticalMeasure);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		void PopulateGoodsStatisticalMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IGoodsStatisticalMeasure obj)
		{
			if (obj != null && !obj.StatisticalUnitCode.IsEmpty && !obj.TariffQuantity.IsEmpty)
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

		void PopulateOrigin(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IOrigin obj)
		{
			if (obj != null)
			{
				bo.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin();
				var newItem = bo.Origin;
				newItem.CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = obj.CountryCode };
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

		void PopulateCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				bo.Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				var newItem = bo.Commodity;
				newItem.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = obj.Description };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseDescription, () => newItem.TwChineseDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwChineseDescription() { Value = obj.ChineseDescription });
				PopulateClassification(newItem, obj.Classification);
				PopulateDutyTaxFee(newItem, obj.DutyTaxFee);
				PopulateGovernmentProcedure(newItem, obj.GovernmentProcedure);
				PopulateInvoiceLine(newItem, obj.InvoiceLine);
				PopulateQuarantine(newItem, obj.Quarantine);
			}
		}

		void PopulateQuarantine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IQuarantine obj)
		{
			if (obj != null)
			{
				bo.TwQuarantine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine();
				var newItem = bo.TwQuarantine;
				PopulateValueIfNodeValueIsNotEmpty(obj.ObjectFeature, () => newItem.TwObjectFeature = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwObjectFeature() { Value = obj.ObjectFeature });
				PopulateValueIfNodeValueIsNotEmpty(obj.Treatment, () => newItem.TwTreatment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwTreatment() { Value = obj.Treatment });
				PopulateQuarantineAdditionalDocument(newItem, obj.AdditionalDocument);
				PopulateQuarantineAdditionalInformation(newItem, obj.AdditionalInformation);
				PopulateQuarantinePackaging(newItem, obj.Packing);
				PopulateQuarantineAnimal(newItem, obj.Animal);
			}
		}

		void PopulateQuarantineAnimal(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IAnimal obj)
		{
			if (obj != null)
			{
				bo.TwAnimal = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimal();
				var newItem = bo.TwAnimal;
				PopulateValueIfNodeValueIsNotEmpty(obj.AgeMonthNumeric, () => newItem.TwAgeMonthNumeric = obj.AgeMonthNumeric);
				PopulateValueIfNodeValueIsNotEmpty(obj.AgeYearNumeric, () => newItem.TwAgeYearNumeric = obj.AgeYearNumeric);
				PopulateValueIfNodeValueIsNotEmpty(obj.FemaleQuantity, () => newItem.TwFemaleQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwFemaleQuantity { Value = obj.FemaleQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.MaleQuantity, () => newItem.TwMaleQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMaleQuantity { Value = obj.MaleQuantity });
				PopulateValueIfNodeValueIsNotEmpty(obj.MicrochipID, () => newItem.TwMicrochipId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMicrochipId { Value = obj.MicrochipID });
				PopulateValueIfNodeValueIsNotEmpty(obj.Vaccination, () => newItem.TwVaccination = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwVaccination { Value = obj.Vaccination });
			}
		}

		void PopulateQuarantinePackaging(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IEnumerable<IPackaging> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantinePackaging>();
				foreach (var item in obj)
				{
					var packingDateTime = item.PackingDateTime;
					if (packingDateTime.IsValid)
					{
						var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantinePackaging();
						newItem.TwPackingDateTime = packingDateTime.ToISO8601ShortDateString();
						collection.Add(newItem);
					}
				}
				bo.Packaging = collection;
			}
		}

		void PopulateQuarantineAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalInformation>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalInformation();
					newItem.TwPackingHouse = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalInformationTwPackingHouse() { Value = item.PackingHouse };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		void PopulateQuarantineAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalDocument>();
				foreach (var item in obj.Take(5))
				{
					var slaughterDateTime = item.SlaughterDateTime;
					if (slaughterDateTime.IsValid)
					{
						var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineAdditionalDocument();
						newItem.TwSlaughterDateTime = slaughterDateTime.ToISO8601ShortDateString();
						collection.Add(newItem);
					}
				}
				bo.AdditionalDocument = collection;
			}
		}

		void PopulateInvoiceLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IInvoiceLine obj)
		{
			if (obj != null && !obj.ItemChargeAmount.IsEmpty)
			{
				bo.InvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine();
				var newItem = bo.InvoiceLine;
				newItem.ItemChargeAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineItemChargeAmount { Value = obj.ItemChargeAmount.Normalize() };
			}
		}

		void PopulateGovernmentProcedure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IGovernmentProcedure obj)
		{
			if (obj != null && !obj.CurrentCode.IsEmpty)
			{
				bo.GovernmentProcedure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure() { CurrentCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureCurrentCode() { Value = obj.CurrentCode } };
			}
		}

		void PopulateDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, ICommodityDutyTaxFee obj)
		{
			if (obj != null && !obj.AdValoremTaxBaseAmount.IsEmpty)
			{
				bo.DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee() { AdValoremTaxBaseAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeAdValoremTaxBaseAmount() { Value = obj.AdValoremTaxBaseAmount } };
			}
		}

		void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity bo, IClassification obj)
		{
			if (obj != null)
			{
				bo.Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification() { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationId() { Value = obj.ID } };
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

		void PopulateGovernmentAgencyGoodsItemAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null && obj.Any())
			{
				bo.AdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument() { TwSequenceNumeric = obj.FirstOrDefault().SequenceNumeric };
			}
		}

		void PopulateExporter(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Exporter = new DeclarationGoodsShipmentExporter();
				var newItem = bo.Exporter;
				newItem.Id = new DeclarationGoodsShipmentExporterId { Value = obj.ID };
				newItem.Name = new DeclarationGoodsShipmentExporterName { Value = obj.Name };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentExporterTwChineseName() { Value = obj.ChineseName });
				newItem.TwTypeCode = new DeclarationGoodsShipmentExporterTwTypeCode { Value = obj.TypeCode };
				PopulateExporterAddress(newItem, obj.Address);
			}
		}

		void PopulateExporterAddress(DeclarationGoodsShipmentExporter bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentExporterAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationGoodsShipmentExporterAddressLine() { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentExporterAddressTwChineseLine() { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.ArrivalTransportMeansTypeCode, () => newItem.ArrivalTransportMeans = new DeclarationGoodsShipmentConsignmentArrivalTransportMeans() { TypeCode = new DeclarationGoodsShipmentConsignmentArrivalTransportMeansTypeCode { Value = obj.ArrivalTransportMeansTypeCode } });
				PopuluateBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopuluateDepartureTransportMeans(newItem, obj.DepartureTransportMeans);
				PopulateValueIfNodeValueIsNotEmpty(obj.GoodsLocation, () => newItem.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId() { Value = obj.GoodsLocation } });
				PopulateLoadingLocation(newItem, obj.LoadingLocation);
				PopulateTransportContractDocument(newItem, obj.TransportContractDocuments);
				PopulateTransportEquipment(newItem, obj.TransportEquipments);
				PopulateUnloadingLocation(newItem, obj.UnloadingLocation);
			}
		}

		void PopuluateBorderTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationGoodsShipmentConsignmentBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentBorderTransportMeansId() { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.JourneyID, () => newItem.JourneyId = new DeclarationGoodsShipmentConsignmentBorderTransportMeansJourneyId() { Value = obj.JourneyID });
			}
		}

		void PopulateUnloadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			var id = obj?.ID ?? ZString.Empty;
			if (!id.IsEmpty)
			{
				bo.UnloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation() { Id = new DeclarationGoodsShipmentConsignmentUnloadingLocationId { Value = id } };
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
					PopulateValueIfNodeValueIsNotEmpty(item.CharacteristicCode, () => newItem.CharacteristicCode = new DeclarationGoodsShipmentConsignmentTransportEquipmentCharacteristicCode { Value = item.CharacteristicCode });
					PopulateValueIfNodeValueIsNotEmpty(item.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentTransportEquipmentId { Value = item.ID });
					PopulateValueIfNodeValueIsNotEmpty(item.UsedCapacityCode, () => newItem.TwUsedCapacityCode = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwUsedCapacityCode { Value = item.UsedCapacityCode });
					PopulateSeal(newItem, item.Seals);
					collection.Add(newItem);
				}
				bo.TransportEquipment = collection;
			}
		}

		void PopulateSeal(DeclarationGoodsShipmentConsignmentTransportEquipment bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentTwSeal>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwSeal() { TwSealId = new DeclarationGoodsShipmentConsignmentTransportEquipmentTwSealTwSealId { Value = item } };
					collection.Add(newItem);
				}
				bo.TwSeal = collection;
			}
		}

		void PopulateTransportContractDocument(DeclarationGoodsShipmentConsignment bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentConsignmentTransportContractDocument();
					newItem.Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId { Value = item.ID };
					newItem.TypeCode = new DeclarationGoodsShipmentConsignmentTransportContractDocumentTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TransportContractDocument = collection;
			}
		}

		void PopulateLoadingLocation(DeclarationGoodsShipmentConsignment bo, ILocation obj)
		{
			if (obj != null)
			{
				bo.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation();
				var newItem = bo.LoadingLocation;
				newItem.Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId() { Value = obj.ID };
			}
		}

		void PopuluateDepartureTransportMeans(DeclarationGoodsShipmentConsignment bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.DepartureTransportMeans = new DeclarationGoodsShipmentConsignmentDepartureTransportMeans();
				var newItem = bo.DepartureTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsignmentDepartureTransportMeansId() { Value = obj.ID });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TypeCode = new DeclarationGoodsShipmentConsignmentDepartureTransportMeansTypeCode() { Value = obj.TypeCode });
			}
		}

		void PopulateConsignee(DeclarationGoodsShipment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Consignee = new DeclarationGoodsShipmentConsignee();
				var newItem = bo.Consignee;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => newItem.Id = new DeclarationGoodsShipmentConsigneeId { Value = obj.ID });
				newItem.Name = new DeclarationGoodsShipmentConsigneeName { Value = obj.Name };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => newItem.TwChineseName = new DeclarationGoodsShipmentConsigneeTwChineseName { Value = obj.ChineseName });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => newItem.TwTypeCode = new DeclarationGoodsShipmentConsigneeTwTypeCode { Value = obj.TypeCode });
				PopulateAddress(newItem, obj.Address);
			}
		}

		void PopulateAddress(DeclarationGoodsShipmentConsignee bo, IAddress obj)
		{
			if (obj != null)
			{
				bo.Address = new DeclarationGoodsShipmentConsigneeAddress();
				var newItem = bo.Address;
				newItem.Line = new DeclarationGoodsShipmentConsigneeAddressLine { Value = obj.Line.Left(120) };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => newItem.TwChineseLine = new DeclarationGoodsShipmentConsigneeAddressTwChineseLine { Value = obj.ChineseLine.Left(100) });
			}
		}

		void PopulateCurrencyExchange(Declaration bo, ICurrencyExchange obj)
		{
			if (obj != null)
			{
				bo.CurrencyExchange = new DeclarationCurrencyExchange()
				{
					CurrencyTypeCode = new DeclarationCurrencyExchangeCurrencyTypeCode() { Value = obj.CurrencyTypeCode },
					RateNumeric = obj.RateNumeric
				};
			}
		}

		void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			var result = new DeclarationBorderTransportMeans();
			if (obj != null)
			{
				PopulateValueIfNodeValueIsValid(obj.ArrivalDateTime, () => { result.ArrivalDateTime = obj.ArrivalDateTime.ToISO8601ShortDateString(); });
				result.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
				PopulateBorderTransportMeansItinerary(result, obj.ItineraryRoutingCountryCodes);
			}
			bo.BorderTransportMeans = result;
		}

		void PopulateBorderTransportMeansItinerary(DeclarationBorderTransportMeans bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationBorderTransportMeansItinerary>();
				foreach (var item in obj.Take(99))
				{
					var newItem = new DeclarationBorderTransportMeansItinerary() { RoutingCountryCode = new DeclarationBorderTransportMeansItineraryRoutingCountryCode { Value = item } };
					collection.Add(newItem);
				}
				bo.Itinerary = collection;
			}
		}

		void PopulateAdditionalDocument(Declaration bo, IDeclarationAdditionalDocument obj)
		{
			if (obj != null)
			{
				bo.AdditionalDocument = new DeclarationAdditionalDocument() { Id = new DeclarationAdditionalDocumentId { Value = obj.ID } };
			}
		}

		void PopulateAdditionalInformation(Declaration bo, IDeclarationAdditionalInformation obj)
		{
			if (obj != null)
			{
				var additionalInformation = new DeclarationAdditionalInformation() { StatementDescription = new DeclarationAdditionalInformationStatementDescription { Value = obj.StatementDescription } };
				bo.AdditionalInformation = additionalInformation;
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
