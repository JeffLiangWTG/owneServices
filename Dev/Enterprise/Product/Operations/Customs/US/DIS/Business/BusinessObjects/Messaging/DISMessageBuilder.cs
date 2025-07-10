using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.US.DIS.Messaging.DataFileSchema;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISMessageBuilder
	{
		internal const string DocumentImagePlaceHolder = "!dOcUmEnTiMaGePlAcEHoLdEr:{0}!";
		internal const string DocumentIDPlaceHolder = "!DOCUMENTIDENTIFICATIONNUMBER!";

		public DISMessageBuilder(IDISDocument documentData)
		{
			this.documentData = documentData;
		}

		readonly IDISDocument documentData;

		MessageHeader BuildMessageHeader()
		{
			var result = new MessageHeader();
			result.MessageID = EDIMessage.DISMessageNumberPlaceHolder;
			result.MessageType = Constants.enumMessageType.DocumentSubmission;
			result.SentDateTime = ZDateTime.Now.ToDateTime();
			result.TransmitterID = ZString.Empty;
			result.TransmitterSiteCode = ZString.Empty;
			result.PreparerID = documentData.PreparerID;
			result.PreparerSiteCode = documentData.PreparerSiteCode;

			return result;
		}

		public EDIMessage BuildDocumentSubmissionPackage()
		{
			var documentSubmissionPackage = new DocumentSubmissionPackage();
			documentSubmissionPackage.SubmittedToPortCode = documentData.PortCode.ToUpper().NullIfEmpty();

			switch (documentData.ActionCodeForSubmission)
			{
				case ActionCodeList.Codes.Add:
					documentSubmissionPackage.ActionCode = Constants.enumActionCode.ADD;
					break;
				case ActionCodeList.Codes.Replace:
					documentSubmissionPackage.ActionCode = Constants.enumActionCode.REPLACE;
					break;
			}

			documentSubmissionPackage.PackageIdentifier = GetPackageIdentifier(documentData.PackageIdentifierData);
			documentSubmissionPackage.TradeTransaction = GetTradeTransaction(documentData.TradeTransactions, documentData.TransactionCategory);
			documentSubmissionPackage.CBPRequest = GetCBPRequst(documentData.CBPRequest);
			documentSubmissionPackage.DocumentData = new DocumentData[] { GetDocumentData(isWithdrawal: false) };

			var messageEnvelope = new MessageEnvelope();
			messageEnvelope.MessageHeader = BuildMessageHeader();
			messageEnvelope.MessageBody = new MessageBody();
			messageEnvelope.MessageBody.Item = documentSubmissionPackage;

			var result = documentData.CreateMessage(MessageTypeList.Codes.Submission);
			result.EM_MessageText = SerializeToString(messageEnvelope);

			return result;
		}

		public EDIMessage BuildDocumentWithdrawalUsingDocSubmissionPackage()
		{
			var documentSubmissionPackage = new DocumentSubmissionPackage();

			documentSubmissionPackage.SubmittedToPortCode = documentData.PortCode.ToUpper().NullIfEmpty();

			documentSubmissionPackage.ActionCode = Constants.enumActionCode.DELETE;

			documentSubmissionPackage.TradeTransaction = GetTradeTransaction(documentData.TradeTransactions, documentData.TransactionCategory);

			documentSubmissionPackage.CBPRequest = GetCBPRequst(documentData.CBPRequest);

			documentSubmissionPackage.DocumentData = new DocumentData[] { GetDocumentData(isWithdrawal: true) };

			var messageEnvelope = new MessageEnvelope();
			messageEnvelope.MessageHeader = BuildMessageHeader();
			messageEnvelope.MessageBody = new MessageBody();
			messageEnvelope.MessageBody.Item = documentSubmissionPackage;

			var result = documentData.CreateMessage(MessageTypeList.Codes.Submission);
			result.EM_MessageText = SerializeToString(messageEnvelope);

			return result;
		}

		#region Implementation

		DocumentData GetDocumentData(bool isWithdrawal)
		{
			var result = new DocumentData();

			result.DocumentHeader = GetDocumentHeader(documentData);

			if (documentData.PGAs.Any())
			{
				result.GovtAgencyList = documentData.PGAs.Select(ogaCode => ogaCode.ToString()).ToArray();
			}

			if (!isWithdrawal)
			{
				result.Comment = ((ZString)Regex.Replace(documentData.Comment.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();

				result.OptionalData = GetOptionalData(documentData);

				if (documentData.eDocsDocumentPK.IsValid)
				{
					result.DocumentObject = DocumentImagePlaceHolder;
				}
			}
			return result;
		}
		const string ReplacementStr = @"*";
		const string ValidPatternForValues = @"[^!@\#\$%\^&\*\(\)-_=\+\[\{\]}\\\|;:'"",<\.>/\?`~¢\ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789]";
		const string ValidPatternForEDocFileName = @"[^!@\#\$\^\*\(\)-_=\+\[\{\]}\\\|;:,\.\?`~¢\ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789]";
		DocumentHeader GetDocumentHeader(IDISDocument document)
		{
			var docHeader = new DocumentHeader();
			docHeader.DocumentID = document.DocumentID.IsEmpty ? DocumentIDPlaceHolder.ToUpper() : document.DocumentID.ToString().ToUpper();
			docHeader.DocumentLabel = document.DocumentLabelUSDISDocCode.ToUpper();

			var fileName = (ZString)Regex.Replace(document.CompleteFileName.ToUpper(), ValidPatternForEDocFileName, ReplacementStr);
			docHeader.CompleteFileName = fileName.ToUpper().NullIfEmpty();

			ZString extension = System.IO.Path.GetExtension(fileName);
			docHeader.FileExtensionType = extension.SubstringSafe(1).ToUpper().NullIfEmpty();

			docHeader.DocumentDescription = ((ZString)Regex.Replace(document.DocumentDescription.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();

			if (document.DocumentSentDateEST.IsValid)
			{
				docHeader.DocumentSentDate = document.DocumentSentDateEST.ToDateTime();
			}

			docHeader.DocPreviouslySubmitted = document.PreviouslySubmitted ? "Y" : "N";
			return docHeader;
		}

		OptionalData GetOptionalData(IDISDocument document)
		{
			OptionalData optionalData = null;

			SetInvoiceData(ref optionalData, document);
			SetBondData(ref optionalData, document);
			SetPackingListData(ref optionalData, document);
			SetCertificateData(ref optionalData, document);
			SetPermitData(ref optionalData, document);
			SetToxicSubstancesData(ref optionalData, document);
			SetCommodityData(ref optionalData, document);
			SetAdditionalData(ref optionalData, document);

			return optionalData;
		}

		void SetInvoiceData(ref OptionalData optionalData, IDISDocument document)
		{
			var invoice = document.Invoice;

			if (!invoice.IsEmpty())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				optionalData.InvoiceData = new InvoiceData();
				optionalData.InvoiceData.InvoiceNbr = invoice.InvoiceNumber.ToUpper().NullIfEmpty();
				optionalData.InvoiceData.InvoiceType = invoice.InvoiceType == DISInvoiceType.CommercialInvoice ? Constants.enumInvoiceType.COMMERCIAL_INVOICE : Constants.enumInvoiceType.OTHER;

				if (invoice.InvoiceLines.Any())
				{
					var invoiceLineItems = new List<InvoiceLineItemData>();
					foreach (var invoiceLine in invoice.InvoiceLines)
					{
						if (!invoiceLine.IsEmpty())
						{
							var invoiceLineItemData = new InvoiceLineItemData();
							invoiceLineItemData.InvoiceLineNbr = Convert.ToString(invoiceLine.InvoiceLineNumber, CultureInfo.InvariantCulture).ToUpper();

							if (!invoiceLine.CommodityDetails.IsEmpty())
							{
								invoiceLineItemData.CommodityData = GetCommodityData(invoiceLine.CommodityDetails);
							}

							invoiceLineItems.Add(invoiceLineItemData);
						}
					}
					optionalData.InvoiceData.InvoiceLineItemData = invoiceLineItems.Count == 0 ? null : invoiceLineItems.ToArray();
				}
			}
		}

		void SetBondData(ref OptionalData optionalData, IDISDocument document)
		{
			var bondData = document.BondData;

			if (!bondData.IsEmpty())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				optionalData.BondData = new BondData();

				optionalData.BondData.BondName = GetMatchedCode(bondData.BondName);
				optionalData.BondData.BondNumber = bondData.BondNumber.ToUpper().NullIfEmpty();
				optionalData.BondData.BondType = bondData.BondType.ToUpper().NullIfEmpty();
				optionalData.BondData.SuretyCode = bondData.SuretyCode.ToUpper().NullIfEmpty();
				optionalData.BondData.AgentIDNbr = bondData.AgentIDNumber.ToUpper().NullIfEmpty();
				optionalData.BondData.Filer = bondData.Filer.ToUpper().NullIfEmpty();
				optionalData.BondData.BondAmount = bondData.BondAmount.IsEmpty ? null : bondData.BondAmount.ToString(2);
			}
		}

		void SetPackingListData(ref OptionalData optionalData, IDISDocument document)
		{
			var packingList = document.PackingList;

			if (!packingList.IsEmpty())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				optionalData.PackingListData = new PackingListData();
				optionalData.PackingListData.PackingListNbr = ((ZString)Regex.Replace(packingList.PackingListNumber.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();
				optionalData.PackingListData.InvoiceNumber = ((ZString)Regex.Replace(packingList.InvoiceNumber.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();
				optionalData.PackingListData.PurchaseOrderNbr = ((ZString)Regex.Replace(packingList.PurchaseOrderNumber.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();
			}
		}

		void SetCertificateData(ref OptionalData optionalData, IDISDocument document)
		{
			var certificateData = document.CertificateData;

			if (!certificateData.IsEmpty())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				optionalData.CertificateData = new CertificateData();
				optionalData.CertificateData.CertificateNumber = certificateData.Number.ToUpper().NullIfEmpty();
				optionalData.CertificateData.CertificateType = certificateData.Type.ToUpper().NullIfEmpty();
				optionalData.CertificateData.Statement = certificateData.Statement.ToUpper().NullIfEmpty();

				if (certificateData.ExpiryDate.IsValid)
				{
					optionalData.CertificateData.ExpirationDate = certificateData.ExpiryDate.ToDateTime();
					optionalData.CertificateData.ExpirationDateSpecified = true;
				}

				optionalData.CertificateData.ImporterOfRecord = certificateData.ImporterOfRecord.ToUpper().NullIfEmpty();
				optionalData.CertificateData.InspectionLocation = certificateData.InspectionLocation.ToUpper().NullIfEmpty();

				if (certificateData.IssueDate.IsValid || !certificateData.GrossTonnage.IsEmpty || !certificateData.NetTonnage.IsEmpty)
				{
					// Submitting in Additional Data until Schema is updated to support this field.
					var additionalDataList = optionalData.AdditionalData != null ? new List<NameValuePair>(optionalData.AdditionalData) : new List<NameValuePair>();

					if (certificateData.IssueDate.IsValid)
					{
						var nameValuePair = new NameValuePair();
						nameValuePair.Name = "ISSUE_DATE";
						nameValuePair.Value = certificateData.IssueDate.ToISO8601String();
						additionalDataList.Add(nameValuePair);
					}
					if (!certificateData.GrossTonnage.IsEmpty)
					{
						var nameValuePair = new NameValuePair();
						nameValuePair.Name = "GROSS_TONNAGE";
						nameValuePair.Value = certificateData.GrossTonnage.ToString();
						additionalDataList.Add(nameValuePair);
					}
					if (!certificateData.NetTonnage.IsEmpty)
					{
						var nameValuePair = new NameValuePair();
						nameValuePair.Name = "NET_TONNAGE";
						nameValuePair.Value = certificateData.NetTonnage.ToString();
						additionalDataList.Add(nameValuePair);
					}
					optionalData.AdditionalData = additionalDataList.ToArray();
				}
			}
		}

		void SetPermitData(ref OptionalData optionalData, IDISDocument document)
		{
			var permitData = document.PermitData;

			if (!permitData.IsEmpty())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				optionalData.PermitData = new PermitData();
				optionalData.PermitData.PermitNumber = permitData.Number.ToUpper().NullIfEmpty();
				optionalData.PermitData.PermitType = permitData.Type.ToUpper().NullIfEmpty();
				optionalData.PermitData.ApprovalNumber = permitData.ApprovalNumber.ToUpper().NullIfEmpty();
				optionalData.PermitData.Statement = permitData.Statement.ToUpper().NullIfEmpty();

				if (permitData.StartDate.IsValid)
				{
					optionalData.PermitData.StartDate = permitData.StartDate.ToDateTime();
				}

				if (permitData.EndDate.IsValid)
				{
					optionalData.PermitData.EndDate = permitData.EndDate.ToDateTime();
				}

				optionalData.PermitData.ImporterOfRecord = permitData.ImporterOfRecord.ToUpper().NullIfEmpty();
			}
		}

		void SetToxicSubstancesData(ref OptionalData optionalData, IDISDocument document)
		{
			var toxicSubstance = document.ToxicSubstanceData;
			if (!toxicSubstance.IsEmpty())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				optionalData.ToxicSubstancesData = new ToxicSubstancesData();
				optionalData.ToxicSubstancesData.CASNbr = toxicSubstance.CASNumber.ToUpper().NullIfEmpty();
				optionalData.ToxicSubstancesData.EPARegistrationNbr = toxicSubstance.EPARegoNumber.ToUpper().NullIfEmpty();
				optionalData.ToxicSubstancesData.EPAProducerEstNbr = toxicSubstance.EPAProducerEstNumber.ToUpper().NullIfEmpty();
			}
		}

		void SetCommodityData(ref OptionalData optionalData, IDISDocument document)
		{
			var commodityData = document.CommodityData;

			if (commodityData.Any())
			{
				var commodityLines = new List<CommodityData>();
				foreach (var commodityLine in commodityData)
				{
					if (!commodityLine.IsEmpty())
					{
						commodityLines.Add(GetCommodityData(commodityLine));
					}
				}

				if (commodityLines.Count > 0)
				{
					CreateOptionalDataIfNecessary(ref optionalData);
					optionalData.CommodityList = commodityLines.ToArray();
				}
			}
		}

		void SetAdditionalData(ref OptionalData optionalData, IDISDocument document)
		{
			var additionalData = document.AdditionalData;

			if (additionalData.Any())
			{
				CreateOptionalDataIfNecessary(ref optionalData);

				var additionalDataList = optionalData.AdditionalData != null ? new List<NameValuePair>(optionalData.AdditionalData) : new List<NameValuePair>();

				foreach (var addData in additionalData)
				{
					var nameValuePair = new NameValuePair();
					nameValuePair.Name = ((ZString)Regex.Replace(addData.FieldName.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();
					nameValuePair.Value = ((ZString)Regex.Replace(addData.Value.ToUpper(), ValidPatternForValues, ReplacementStr)).NullIfEmpty();
					additionalDataList.Add(nameValuePair);
				}

				optionalData.AdditionalData = additionalDataList.ToArray();
			}
		}

		CommodityData GetCommodityData(IDISCommodityLine commodityData)
		{
			var comm = new CommodityData();

			comm.EntryLineNumber = commodityData.EntryLineNumber;
			comm.EntryLineNumberSpecified = commodityData.EntryLineNumber > 0;
			comm.HTSNumber = commodityData.HTSNumber.ToUpper().NullIfEmpty();
			comm.CommodityDescription = commodityData.CommodityDescription.ToUpper().NullIfEmpty();
			comm.CountryOfOrigin = commodityData.CountryOfOrigin.ToUpper().NullIfEmpty();
			comm.ContainerNbr = commodityData.ContainerNumber.ToUpper().NullIfEmpty();

			if (commodityData.ArrivalDate.IsValid)
			{
				comm.ArrivalDate = commodityData.ArrivalDate.ToDateTime();
			}
			comm.PortOfLading = commodityData.PortOfLoading.ToUpper().NullIfEmpty();
			comm.PortOfUnlading = commodityData.PortOfUnlading.ToUpper().NullIfEmpty();
			comm.PortOfEntry = commodityData.PortOfEntry.ToUpper().NullIfEmpty();
			comm.SealNumbers = commodityData.SealNumber.ToUpper().NullIfEmpty();

			SetCommodityTradeParties(comm, commodityData);
			SetcommodityVehicleData(comm, commodityData);

			return comm;
		}

		void SetCommodityTradeParties(CommodityData comm, IDISCommodityLine commodityData)
		{
			var tradeParties = commodityData.TradeParties;
			if (tradeParties.Any())
			{
				var xmlTradeParties = new List<TradePartyInfo>();
				foreach (var tradeParty in tradeParties)
				{
					if (!tradeParty.IsEmpty())
					{
						var tParty = new TradePartyInfo();
						tParty.TradePartyID = tradeParty.ID.ToUpper().NullIfEmpty();
						tParty.TradePartyType = GetMatchedTradePartyType(tradeParty.Type);
						tParty.TradePartyName = tradeParty.Name.ToUpper().NullIfEmpty();
						tParty.TradePartyAddress = tradeParty.Address.ToUpper().NullIfEmpty();
						xmlTradeParties.Add(tParty);
					}
				}
				comm.TradeParties = xmlTradeParties.Count == 0 ? null : xmlTradeParties.ToArray();
			}
		}

		void CreateOptionalDataIfNecessary(ref OptionalData optionalData)
		{
			if (optionalData == null)
			{
				optionalData = new OptionalData();
			}
		}

		static string GetMatchedCode(BondNameType bondName)
		{
			switch (bondName)
			{
				case BondNameType.Single:
					return Constants.enumBondName.SINGLE_TXN_BOND;
				case BondNameType.ISFBond:
					return Constants.enumBondName.ISF_BOND;
				default:
					return Constants.enumBondName.OTHER;
			}
		}

		static string GetMatchedTradePartyType(DISTradePartyType tradeParty)
		{
			switch (tradeParty)
			{
				case DISTradePartyType.Agent:
					return Constants.enumTradePartyType.AGENT;
				case DISTradePartyType.Broker:
					return Constants.enumTradePartyType.BROKER;
				case DISTradePartyType.Buyer:
					return Constants.enumTradePartyType.BUYER;
				case DISTradePartyType.Carrier:
					return Constants.enumTradePartyType.CARRIER;
				case DISTradePartyType.Consignee:
					return Constants.enumTradePartyType.CONSIGNEE;
				case DISTradePartyType.Exporter:
					return Constants.enumTradePartyType.EXPORTER;
				case DISTradePartyType.Facilitator:
					return Constants.enumTradePartyType.FACILITATOR;
				case DISTradePartyType.Filer:
					return Constants.enumTradePartyType.FILER;
				case DISTradePartyType.Importer:
					return Constants.enumTradePartyType.IMPORTER;
				case DISTradePartyType.Manufacturer:
					return Constants.enumTradePartyType.MANUFACTURER;
				case DISTradePartyType.Seller:
					return Constants.enumTradePartyType.SELLER;
				case DISTradePartyType.Shippier:
					return Constants.enumTradePartyType.SHIPPER;
				case DISTradePartyType.Unknown:
					return Constants.enumTradePartyType.UNKNOWN;
				default:
					return Constants.enumTradePartyType.OTHER;
			}
		}

		void SetcommodityVehicleData(CommodityData comm, IDISCommodityLine commodityData)
		{
			var vehicleData = commodityData.VehicleData;
			if (vehicleData != null && !vehicleData.IsEmpty())
			{
				comm.VehicleAndEngineData = new VehicleAndEngineData();
				comm.VehicleAndEngineData.VIN = vehicleData.VIN.ToUpper().NullIfEmpty();
				comm.VehicleAndEngineData.VehicleManufacturer = vehicleData.Manufacturer.ToUpper().NullIfEmpty();
				comm.VehicleAndEngineData.VehicleModel = vehicleData.Model.ToUpper().NullIfEmpty();
				comm.VehicleAndEngineData.VehicleSerialNumber = vehicleData.SerialNumber.ToUpper().NullIfEmpty();

				var year = ZInt.ParseSafe(vehicleData.ManufactureYear, 0);
				var month = ZInt.ParseSafe(vehicleData.ManufactureMonth, 0);

				if (year > ZDateTime.MinSmallDateTimeValue.Year && month > 0 && month < 13)
				{
					comm.VehicleAndEngineData.VehicleManufactureDate = new DateTime(year, month, 1);
				}

				comm.VehicleAndEngineData.EngineManufacturer = vehicleData.EngineManufacturer.ToUpper().NullIfEmpty();
				comm.VehicleAndEngineData.EngineModel = vehicleData.EngineModel.ToUpper().NullIfEmpty();
				comm.VehicleAndEngineData.EngineSerialNumber = vehicleData.EngineSerialNumber.ToUpper().NullIfEmpty();

				if (vehicleData.EngineManufactureDate.IsValid)
				{
					comm.VehicleAndEngineData.EngineManufactureDate = vehicleData.EngineManufactureDate.ToDateTime();
				}
			}
		}

		string SerializeToString(object obj)
		{
			var serializer = ZXmlSerializer.New(obj.GetType());

			var result = "";
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(writer, obj);
				result = writer.ToString();
			}

			return result;
		}

		CBPRequest GetCBPRequst(IDISCBPRequest cbpRequest)
		{
			CBPRequest result = null;

			if (!cbpRequest.IsEmpty())
			{
				result = new CBPRequest();
				result.CBPRequestID = cbpRequest.ID.ToUpper().NullIfEmpty();

				result.CBPRequestType = GetMatchedCode(cbpRequest.Type);

				if (cbpRequest.RequestDate.IsValid)
				{
					result.CBPRequestDate = cbpRequest.RequestDate.ToDateTime();
				}
			}

			return result;
		}

		PackageIdentifierType GetPackageIdentifier(IDISPackageIdentifier packageIdentifierData)
		{
			PackageIdentifierType packageIdentifier = null;
			if (packageIdentifierData != null)
			{
				packageIdentifier = new PackageIdentifierType();
				packageIdentifier.PackageCategory = packageIdentifierData.PackageCategory.ToUpper().NullIfEmpty();
				packageIdentifier.ImporterOfRecordNbr = packageIdentifierData.ImporterOfRecordNumber.ToUpper().NullIfEmpty();
			}

			return packageIdentifier;
		}

		TradeTransaction GetTradeTransaction(IEnumerable<IDISTradeTransaction> tradeTransactions, TransactionCategory tranCategory)
		{
			var entries = new List<TradeTransactionEntry>();
			var entrySummaries = new List<TradeTransactionEntrySummary>();
			var bills = new List<TradeTransactionBill>();
			var isfNumbers = new List<TradeTransactionISFNumber>();
			var iTNs = new List<TradeTransactionITN>();
			var xTNs = new List<TradeTransactionXTN>();
			var ftzAdmissionNumber = ZString.Empty;

			foreach (var tradeTrans in tradeTransactions)
			{
				if (!tradeTrans.IsEmpty())
				{
					switch (tradeTrans.Type)
					{
						case TradeTransactionType.Entry:
							var tradeTransactionEntry = new TradeTransactionEntry();
							tradeTransactionEntry.EntryNumber = tradeTrans.Number.ToUpper().NullIfEmpty();
							tradeTransactionEntry.Filer = tradeTrans.FilerOrSCAC.ToUpper().NullIfEmpty();
							tradeTransactionEntry.ReferenceNumber = tradeTrans.ReferenceNumber.ToUpper().NullIfEmpty();
							entries.Add(tradeTransactionEntry);
							break;
						case TradeTransactionType.EntrySummary:
							var tradeTransactionEntrySummary = new TradeTransactionEntrySummary();
							tradeTransactionEntrySummary.EntryNumber = tradeTrans.Number.ToUpper().NullIfEmpty();
							tradeTransactionEntrySummary.Filer = tradeTrans.FilerOrSCAC.ToUpper().NullIfEmpty();
							tradeTransactionEntrySummary.ReferenceNumber = tradeTrans.ReferenceNumber.ToUpper().NullIfEmpty();
							entrySummaries.Add(tradeTransactionEntrySummary);
							break;
						case TradeTransactionType.Bill:
							var tradeTransactionBill = new TradeTransactionBill();
							tradeTransactionBill.SCAC = tradeTrans.FilerOrSCAC.ToUpper().NullIfEmpty();
							tradeTransactionBill.BillNumber = tradeTrans.Number.ToUpper().NullIfEmpty();
							tradeTransactionBill.HouseBillNumber = tradeTrans.AdditionalNumbers.FirstOrDefault().ToUpper().NullIfEmpty();
							tradeTransactionBill.ReferenceNumber = tradeTrans.ReferenceNumber.ToUpper().NullIfEmpty();
							bills.Add(tradeTransactionBill);
							break;
						case TradeTransactionType.ISFNumber:
							var tradeTransactionISFNumber = new TradeTransactionISFNumber();
							tradeTransactionISFNumber.Filer = tradeTrans.FilerOrSCAC.ToUpper().NullIfEmpty();
							tradeTransactionISFNumber.ISFNumber = tradeTrans.Number.ToUpper().NullIfEmpty();
							isfNumbers.Add(tradeTransactionISFNumber);
							break;
						case TradeTransactionType.Export:
							if (!tradeTrans.Number.IsEmpty)
							{
								var tradeTransactionITN = new TradeTransactionITN();
								tradeTransactionITN.ITN = tradeTrans.Number;
								tradeTransactionITN.ReferenceNumber = tradeTrans.ShipmentNo.ToUpper().NullIfEmpty();
								iTNs.Add(tradeTransactionITN);
							}
							else
							{
								var tradeTransactionXTN = new TradeTransactionXTN();
								tradeTransactionXTN.XTN = tradeTrans.XTN.ToUpper().NullIfEmpty();
								tradeTransactionXTN.ReferenceNumber = tradeTrans.ShipmentNo.ToUpper().NullIfEmpty();
								xTNs.Add(tradeTransactionXTN);
							}
							break;
						case TradeTransactionType.FTZAdmission:
							ftzAdmissionNumber = tradeTrans.Number.KeepAlphanumericCharacters();
							break;
					}
				}
			}

			TradeTransaction result = null;
			if (entries.Count > 0 || entrySummaries.Count > 0 || bills.Count > 0 || isfNumbers.Count > 0 || iTNs.Count > 0 || xTNs.Count > 0 || !ftzAdmissionNumber.IsEmpty)
			{
				result = new TradeTransaction();
				result.TransactionCategory = GetMappedTransactionCategory(tranCategory);
				result.Entry = entries.Count == 0 ? null : entries.ToArray();
				result.EntrySummary = entrySummaries.Count == 0 ? null : entrySummaries.ToArray();
				result.Bill = bills.Count == 0 ? null : bills.ToArray();
				result.ISFNumber = isfNumbers.Count == 0 ? null : isfNumbers.ToArray();
				result.ITN = iTNs.Count == 0 ? null : iTNs.ToArray();
				result.XTN = xTNs.Count == 0 ? null : xTNs.ToArray();
				result.FTZAdmissionNbr = ftzAdmissionNumber.ToUpper().NullIfEmpty();
			}

			return result;
		}

		static string GetMappedTransactionCategory(TransactionCategory tranCategory)
		{
			switch (tranCategory)
			{
				case TransactionCategory.Continuous:
					return Constants.enumTransactionCategory.CONTINUOUS;
				case TransactionCategory.SingleTransaction:
					return Constants.enumTransactionCategory.SINGLE_TXN;
				default:
					return Constants.enumTransactionCategory.OTHER;
			}
		}

		static string GetMatchedCode(string requestType)
		{
			switch (requestType)
			{
				case CBPRequestTypeList.Codes.ACEActionNumber:
					return Constants.enumCBPRequestType.ACEActionNumber;
				case CBPRequestTypeList.Codes.ATSDocRequest:
					return Constants.enumCBPRequestType.ATSDocRequest;
				default:
					return Constants.enumCBPRequestType.OtherCBPRequest;
			}
		}
		#endregion
	}

	public static class ZTypesExtensions
	{
		public static ZString? NullIfEmpty(this ZString value)
		{
			if (value.IsEmpty)
			{
				return null;
			}
			else
			{
				return value;
			}
		}
	}
}
