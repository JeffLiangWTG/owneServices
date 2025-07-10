using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	class MAFMessageSerializer
	{
		public MAFMessageSerializer(IMAFMessagingRequest messageData)
		{
			this.messageData = messageData;
		}

		public string Generate()
		{
			var message = new MessagingRequestType();
			message.Header = GetHeader(messageData);

			var body = message.Body = new MessagingRequestTypeBody();
			body.MetaData = GetMetaData(messageData.MetaData);
			body.Files = GetFiles(messageData.Files);

			return SerializeToXML(message);
		}

		static MessagingRequestTypeHeader GetHeader(IMAFMessagingRequest messageData)
		{
			var header = new MessagingRequestTypeHeader();
			header.ApplicationName = messageData.ApplicationName.ToString(true);
			header.ApplicationVersion = messageData.ApplicationVersion.ToString(true);
			header.DocumentType = messageData.DocumentType.ToString(true);

			var sender = header.Sender = new EndPointType();
			sender.Name = messageData.SenderName.ToString(true);
			sender.EndPointType1 = messageData.SenderEndPointType;
			sender.Address = messageData.SenderAddress.ToString(true);

			header.CallerRefID = messageData.CallerRefID.ToString(true);
			return header;
		}

		static BACCApplicationType[] GetMetaData(IMAFMessagingMetaData metaData)
		{
			var ebacca = new BACCApplicationType();

			ebacca.Broker = GetOrganisation(metaData.Source.Broker);
			ebacca.Importer = GetOrganisation(metaData.Importer);
			ebacca.Exporter = GetOrganisation(metaData.Exporter);

			ebacca.Details = GetDetails(metaData);

			ebacca.References = GetReferences(metaData);

			ebacca.TransitionalFacility = GetOrganisation(metaData.Source.TransitionalFacility);
			ebacca.TreatmentProvider = GetOrganisation(metaData.Source.TreatmentProvider);

			ebacca.PaymentDetails = GetPaymentDetails(metaData);

			ebacca.Comments = metaData.Comments.Left(255, true);

			return new[] { ebacca };
		}

		static BACCApplicationTypePaymentDetails GetPaymentDetails(IMAFMessagingMetaData metaData)
		{
			var paymentDetails = new BACCApplicationTypePaymentDetails();

			switch (metaData.AlternativePaymentMethod)
			{
				case MAFPaymentMethodList.Codes.Cash:
					paymentDetails.AlternativePaymentMethodSpecified = true;
					paymentDetails.AlternativePaymentMethod = BACCApplicationTypePaymentDetailsAlternativePaymentMethod.Cash;
					break;

				case MAFPaymentMethodList.Codes.Other:
					paymentDetails.AlternativePaymentMethodSpecified = true;
					paymentDetails.AlternativePaymentMethod = BACCApplicationTypePaymentDetailsAlternativePaymentMethod.Other;
					break;
			}

			var accountDetails = metaData.AccountDetails;
			if (accountDetails != null && !accountDetails.AccountHolderName.IsEmpty && !accountDetails.AccountNumber.IsEmpty)
			{
				var account = paymentDetails.Account = new BACCApplicationTypePaymentDetailsAccount();
				account.AccountHolderName = accountDetails.AccountHolderName.Left(50);
				account.AccountNumber = accountDetails.AccountNumber.Left(12);
			}
			return paymentDetails;
		}

		static BACCApplicationTypeReferences GetReferences(IMAFMessagingMetaData metaData)
		{
			var references = new BACCApplicationTypeReferences();

			if (!metaData.MAFConsignmentNumber.IsEmpty || !metaData.MAFReceiptNumber.IsEmpty)
			{
				var maf = references.MAF = new BACCApplicationTypeReferencesMAF();
				maf.ConsignmentNumber = metaData.MAFConsignmentNumber.Left(13, true);
				maf.ReceiptNumber = metaData.MAFReceiptNumber.Left(10, true);
			}

			if (metaData.Source.CustomsEntryNumber != 0 || metaData.Source.IsECIWriteoff)
			{
				var customs = references.Customs = new BACCApplicationTypeReferencesCustoms();
				customs.EntryNumber = metaData.Source.CustomsEntryNumber;
				customs.EntryType = metaData.Source.IsECIWriteoff ? BACCApplicationTypeReferencesCustomsEntryType.ECI : BACCApplicationTypeReferencesCustomsEntryType.IE;
			}

			references.ClientReferenceNumber = metaData.ClientReferenceNumber; // .Left(14); Removed this as we're putting our own placeholder in which is replaced later anyway.

			return references;
		}

		static BACCApplicationTypeDetails GetDetails(IMAFMessagingMetaData metaData)
		{
			var details = new BACCApplicationTypeDetails();
			details.MAFProcessingOffice = new MAFProcessingOfficeList().GetEnumValueOrDefault(metaData.ProcessingOffice, BACCApplicationTypeDetailsMAFProcessingOffice.Auckland);
			details.ConsignmentType = new ConsignmentTypeList().GetEnumValueOrDefault(metaData.ConsignmentType, BACCApplicationTypeDetailsConsignmentType.CommercialCargo);

			details.Shipment = GetShipment(metaData);

			details.ConsignmentDescription = metaData.Source.ConsignmentDescription.Left(100, true);
			var goodsMeasurement = details.ConsignmentMeasurement = new GoodsMeasurementType();
			goodsMeasurement.MeasurementUnitQualifer = new MeasurementUQList().GetEnumValueOrDefault(metaData.MeasurementUQ, GoodsMeasurementTypeMeasurementUnitQualifer.piece);
			goodsMeasurement.MeasurementValue = metaData.MeasurementValue;

			details.Commodities = GetCommodities(metaData.Source.Commodities);

			return details;
		}

		static BACCApplicationTypeDetailsShipment GetShipment(IMAFMessagingMetaData metaData)
		{
			var shipment = new BACCApplicationTypeDetailsShipment();
			shipment.OriginCountry = metaData.Source.OriginCountry.Left(2, true);
			shipment.DischargePorts = metaData.Source.DischargePorts.ToArrayLeft(5);
			shipment.Destinations = metaData.Source.Destinations.ToArrayLeft(50);

			shipment.IsMafAudit = metaData.IsMAFAuditRequiredByCustoms.GetValueOrDefault(false);
			shipment.IsMafAuditSpecified = metaData.IsMAFAuditRequiredByCustoms.HasValue;
			shipment.IsCustomsXrayRequired = metaData.IsCustomsXRayRequired.GetValueOrDefault(false);
			shipment.IsCustomsXrayRequiredSpecified = metaData.IsCustomsXRayRequired.HasValue;
			shipment.IsCustomsCashClient = metaData.IsCustomsCashClient.GetValueOrDefault(false);
			shipment.IsCustomsCashClientSpecified = metaData.IsCustomsCashClient.HasValue;

			if (!metaData.Source.FlightNumber.IsEmpty)
			{
				var flight = shipment.Flight = new BACCApplicationTypeDetailsShipmentFlight();
				flight.FlightNumber = metaData.Source.FlightNumber.Left(7);
				if (metaData.Source.FlightArrivalDate.IsValid)
				{
					flight.FlightArrivalDate = metaData.Source.FlightArrivalDate.ToDateTime();
				}
			}
			else
			{
				var voyage = shipment.Voyage = new BACCApplicationTypeDetailsShipmentVoyage();
				voyage.ShipName = metaData.Source.ShipName.Left(30, true);
				voyage.VoyageNumber = metaData.Source.VoyageNumber.Left(8, true);
				voyage.ShippingCompany = metaData.Source.ShippingCompany.Left(50, true);
				if (metaData.Source.VoyageArrivalDate.IsValid)
				{
					voyage.VoyageArrivalDate = metaData.Source.VoyageArrivalDate.ToDateTime();
					voyage.VoyageArrivalDateSpecified = true;
				}
				voyage.CargoType = new CargoTypeList().GetEnumValueOrDefault(metaData.CargoType, BACCApplicationTypeDetailsShipmentVoyageCargoType.FAK);
			}

			var identifiers = shipment.Identifiers = new BACCApplicationTypeDetailsShipmentIdentifiers();
			identifiers.BillOfLadings = metaData.Source.BillOfLadingNumbers.ToArrayLeft(17);
			identifiers.SubBillOfLadings = metaData.Source.SubBillOfLadingNumbers.ToArrayLeft(17);

			var containerTypeList = new ContainerTypeList();
			var containers = new List<BACCApplicationTypeDetailsShipmentIdentifiersContainer>();
			foreach (var containerData in metaData.Source.Containers)
			{
				var container = new BACCApplicationTypeDetailsShipmentIdentifiersContainer();
				containers.Add(container);
				container.ContainerNumber = containerData.ContainerNumber.Left(17, true);
				container.ContainerType = containerTypeList.GetEnumValueOrDefault(containerData.ContainerType, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.MixedContainerTypes);
			}
			identifiers.Containers = containers.Count == 0 ? null : containers.ToArray();

			return shipment;
		}

		static BACCApplicationTypeDetailsCommodity[] GetCommodities(IEnumerable<IMAFCommodity> commoditiesData)
		{
			var goodsTypeList = new GoodsTypeList();
			var measurementUQList = new MeasurementUQList();
			var result = new List<BACCApplicationTypeDetailsCommodity>();
			foreach (var commodityData in commoditiesData)
			{
				var commodity = new BACCApplicationTypeDetailsCommodity();
				result.Add(commodity);
				commodity.GoodsType = goodsTypeList.GetEnumValueOrDefault(commodityData.GoodsType, BACCApplicationTypeDetailsCommodityGoodsType.MSC);
				commodity.GoodsDescription = commodityData.GoodsDescription.ToString(true);
				commodity.TariffCodes = commodityData.TariffCodes.ToArray();
				var goodsMeasurements = new List<GoodsMeasurementType>();
				foreach (var goodsMeasurementData in commodityData.GoodsMeasurements)
				{
					var goodsMeasurement = new GoodsMeasurementType();
					goodsMeasurements.Add(goodsMeasurement);
					goodsMeasurement.MeasurementUnitQualifer = measurementUQList.GetEnumValueOrDefault(goodsMeasurementData.MeasurementUQ, GoodsMeasurementTypeMeasurementUnitQualifer.piece);
					goodsMeasurement.MeasurementValue = goodsMeasurementData.MeasurementValue.ToZInt();
				}
				commodity.GoodsMeasurements = goodsMeasurements.Count == 0 ? null : goodsMeasurements.ToArray();
				commodity.IsNew = commodityData.IsNew.GetValueOrDefault(false);
			}
			return result.Count == 0 ? null : result.ToArray();
		}

		static OrganisationType GetOrganisation(IMAFOrganisation organisationData)
		{
			if (organisationData != null)
			{
				const int length = 50;
				var organisation = new OrganisationType();
				organisation.OrganisationCode = organisationData.OrganisationCode.ToString(true);
				organisation.OrganisationName = organisationData.OrganisationName.Left(length, true);

				var address = organisation.Address = new AddressType();
				address.AddressLine1 = organisationData.AddressLine1.Left(length, true);
				address.AddressLine2 = organisationData.AddressLine2.Left(length, true);
				address.City = organisationData.City.Left(40, true);
				address.Country = organisationData.Country.Left(2, true);
				address.PostalCode = organisationData.PostalCode.Left(7, true);

				var contactName = organisationData.ContactName.Trim();
				if (!contactName.IsEmpty)
				{
					var contact = organisation.ContactPerson = new PersonType();
					var names = contactName.Split(' ');
					if (names.Length > 0)
					{
						contact.FirstName = names[0].Left(length);
						contact.LastName = names[names.Length - 1].Left(length);
					}
				}

				var contactMethods = new List<ContactMethodType>();
				AddContactMethodIfNotEmpty(contactMethods, ContactMethodEnum.Phone, organisationData.ContactPhone);
				AddContactMethodIfNotEmpty(contactMethods, ContactMethodEnum.Fax, organisationData.ContactFax);
				AddContactMethodIfNotEmpty(contactMethods, ContactMethodEnum.EMail, organisationData.ContactEmail);
				organisation.ContactMethods = contactMethods.Count == 0 ? null : contactMethods.ToArray();

				return organisation;
			}
			return null;
		}

		static void AddContactMethodIfNotEmpty(List<ContactMethodType> contactMethods, ContactMethodEnum type, ZString value)
		{
			if (!value.IsEmpty)
			{
				contactMethods.Add(new ContactMethodType { ContactMethodType1 = type, ContactMethodValue = value.Left(255) });
			}
		}

		static MessagingRequestTypeBodyFile[] GetFiles(IEnumerable<IMAFFile> fileDatas)
		{
			var documentTypes = new DocumentTypeList();
			var result = new List<MessagingRequestTypeBodyFile>();
			foreach (var fileData in fileDatas)
			{
				var file = new MessagingRequestTypeBodyFile();
				result.Add(file);
				file.FileName = fileData.FileName;
				file.DocumentType = documentTypes.GetEnumValue(fileData.DocumentType).ToString();
				file.ContentType = fileData.ContentType;
				var data = file.Data = new MessagingRequestTypeBodyFileData();
				data.DataEncoding = fileData.DataEncoding;
				data.DataContent = fileData.DataContent;
			}
			return result.Count == 0 ? null : result.ToArray();
		}

		static string SerializeToXML(MessagingRequestType message)
		{
			var settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.Indent = true;
			settings.IndentChars = "\t";

			using (var streamMessage = new MemoryStream())
			using (var writerMessage = XmlWriter.Create(streamMessage, settings))
			{
				var serializerMessage = ZXmlSerializer.New(typeof(MessagingRequestType));
				serializerMessage.Serialize(writerMessage, message);
				return Base64Encoder.Base64Decode(streamMessage.ToArray(), true);
			}
		}

		readonly IMAFMessagingRequest messageData;
	}

	static class Extensions
	{
		public static string ToString(this ZString input, bool nullIfEmpty)
		{
			string result = null;
			if (!input.IsEmpty || !nullIfEmpty)
			{
				result = input.ToString();
			}

			return result;
		}

		public static string Left(this ZString input, int length, bool nullIfEmpty)
		{
			string result = null;
			if (!input.IsEmpty || !nullIfEmpty)
			{
				result = input.Left(length);
			}

			return result;
		}

		public static string[] ToArray(this IEnumerable<ZString> input)
		{
			return !input.Any() ? null : input.Select(s => s.ToString()).ToArray();
		}

		public static string[] ToArrayLeft(this IEnumerable<ZString> input, int length)
		{
			return !input.Any() ? null : input.Select(s => s.Left(length)).ToArray();
		}
	}
}
