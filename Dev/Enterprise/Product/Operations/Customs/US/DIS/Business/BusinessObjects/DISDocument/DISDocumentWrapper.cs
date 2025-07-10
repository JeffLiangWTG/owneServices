using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISDocumentWrapper : IDISDocument
	{
		public DISDocumentWrapper(DISDocument disDocument)
		{
			this.disDocument = disDocument;
			this.defaultValues = disDocument.HostWrapper.DISHost.ValueProvider;
		}

		readonly IUSDISDefaultValues defaultValues;
		readonly DISDocument disDocument;

		ZString IDISDocument.DocumentID
		{
			get { return disDocument.DocumentID; }
		}

		ZString IDISDocument.DocumentLabel
		{
			get { return disDocument.DocumentLabel; }
		}

		ZString IDISDocument.CompleteFileName
		{
			get
			{
				var result = GetLastSentAcceptedCompleteFileName();
				if (result.IsEmpty)
				{
					var eDoc = disDocument.EDoc;
					var fileNameOfEDocs = eDoc != null ? eDoc.FileName : ZString.Empty;
					var fileNameOnly = eDoc != null ? eDoc.FileNameOnly : ZString.Empty;
					var extension = Path.GetExtension(fileNameOfEDocs);

					if (!string.IsNullOrEmpty(fileNameOfEDocs))
					{
						var completeFileName = disDocument.DocumentID.ToString() + "-" + fileNameOnly;
						result = ((ZString)completeFileName).Left(100 - extension.Length) + extension;
					}
				}
				return result;
			}
		}

		ZString IDISDocument.DocumentDescription
		{
			get { return disDocument.DocumentDescription; }
		}

		ZString IDISDocument.DocumentLabelUSDISDocCode
		{
			get { return disDocument.DocumentLabelUSDISDocCode; }
		}

		ZDateTime IDISDocument.DocumentSentDateEST
		{
			get { return disDocument.SubmitDateUTC.IsValid ? Env.Time.GetUnlocoTimeFromUtc("USNYC", disDocument.SubmitDateUTC.ToDateTime()) : ZDateTime.Empty; }
		}

		ZBool IDISDocument.PreviouslySubmitted
		{
			get { return StatusList.HasBeenLodgedAtCustoms(disDocument.Status); }
		}

		ZString IDISDocument.PortCode
		{
			get { return defaultValues.PortOfEntry; }
		}

		ZString IDISDocument.PreparerID
		{
			get { return defaultValues.PreparerID; }
		}

		ZString IDISDocument.PreparerSiteCode
		{
			get { return defaultValues.PreparerSiteCode; }
		}

		TransactionCategory IDISDocument.TransactionCategory
		{
			get { return defaultValues.TransactionCategory; }
		}

		IEnumerable<IDISTradeTransaction> IDISDocument.TradeTransactions
		{
			get
			{
				if (defaultValues.IsExport)
				{
					var tradeTransactionList = new List<TradeTransaction>();
					var tradeTransaction = new TradeTransaction();
					tradeTransaction.Type = TradeTransactionType.Export;
					tradeTransaction.Number = disDocument.ITN;
					tradeTransaction.ShipmentNo = disDocument.ShipmentNo;
					tradeTransaction.XTN = disDocument.XTN;
					tradeTransactionList.Add(tradeTransaction);
					return tradeTransactionList;
				}
				else
				{
					return defaultValues.DefaultTradeTransactions;
				}
			}
		}

		IDISCBPRequest IDISDocument.CBPRequest
		{
			get { return new DISCBPRequestWrapper(disDocument.CBPRequest); }
		}

		ZGuid IDISDocument.eDocsDocumentPK
		{
			get { return disDocument.EDocsDocumentPK; }
		}

		IEnumerable<ZString> IDISDocument.PGAs
		{
			get { return from DISPGA one in disDocument.PGAs select one.Code; }
		}

		ZString IDISDocument.Comment
		{
			get { return disDocument.Comment; }
		}

		IDISInvoice IDISDocument.Invoice
		{
			get { return disDocument.InvoiceVisible ? new DISInvoiceWrapper(disDocument.Invoice, disDocument.HostWrapper) : null; }
		}

		IDISBondData IDISDocument.BondData
		{
			get { return disDocument.BondDataVisible ? new DISBondDataWrapper(disDocument.BondData, defaultValues.PreparerID) : null; }
		}

		IDISPackingList IDISDocument.PackingList
		{
			get { return disDocument.PackingListVisible ? new DISPackingListDataWrapper(disDocument.PackingList) : null; }
		}

		IDISCertificate IDISDocument.CertificateData
		{
			get { return disDocument.CertificateVisible ? new DISCertificateDataWrapper(disDocument.CertificateData, defaultValues.ImporterOfRecordID) : null; }
		}

		IDISPermit IDISDocument.PermitData
		{
			get { return disDocument.PermitVisible ? new DISPermitDataWrapper(disDocument.PermitData, defaultValues.ImporterOfRecordID) : null; }
		}

		IDISPackageIdentifier IDISDocument.PackageIdentifierData
		{
			get { return new DISPackageIdentifierWrapper(disDocument.Factory, disDocument.DocumentLabel, defaultValues.ImporterOfRecordID); }
		}

		IDISToxicSubstanceData IDISDocument.ToxicSubstanceData
		{
			get { return disDocument.ToxicSubstanceVisible ? new DISToxicSubstanceDataWrapper(disDocument.ToxicSubstanceData) : null; }
		}

		IEnumerable<IDISCommodityLine> IDISDocument.CommodityData
		{
			get
			{
				if (disDocument.CommodityDataVisible)
				{
					foreach (DISCommodityLine line in disDocument.CommodityData)
					{
						foreach (var commodityLine in line.CommodityLines)
						{
							yield return commodityLine;
						}
					}
				}
			}
		}

		IEnumerable<IDISAdditionalData> IDISDocument.AdditionalData
		{
			get { return from DISAdditionalData data in disDocument.AdditionalData select new DISAdditionalDataWrapper(data); }
		}

		public EDIMessage CreateMessage(string messageType)
		{
			var result = disDocument.Messages.AddNew();

			result.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.EM_MessageType = messageType;
			result.EM_MessageSubType = messageType;
			result.EM_LinkedObject = disDocument.RequiredDocumentAddInfo;
			result.EM_MessageOwner = disDocument.IDSuffix.ToString();

			if (disDocument.EDocsDocumentPK.IsValid)
			{
				var attachment = result.MessageAttachments.AddNew();
				attachment.EG_StorageDocsGuid = disDocument.EDocsDocumentPK;
			}

			return result;
		}

		ZString IDISDocument.ActionCodeForSubmission
		{
			get { return StatusList.HasBeenLodgedAtCustoms(disDocument.Status) ? ActionCodeList.Codes.Replace : ActionCodeList.Codes.Add; }
		}

		ZString GetLastSentAcceptedCompleteFileName()
		{
			var result = ZString.Empty;

			if (StatusList.HasBeenLodgedAtCustoms(disDocument.Status))
			{
				var lastMessage = disDocument.Messages.Cast<EDIMessage>().Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit).OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();
				if (lastMessage != null)
				{
					result = lastMessage.MessageContent.Descendants().Where(x => x.Matches(EDIMessage.Constants.CompleteFileName)).Select(x => x.Value).FirstOrDefault();
				}
			}

			return result;
		}

		class TradeTransaction : IDISTradeTransaction
		{
			public TradeTransactionType Type
			{
				get;
				internal set;
			}

			public IEnumerable<ZString> AdditionalNumbers
			{
				get { return additionalNumbers ?? Enumerable.Empty<ZString>(); }
				internal set { additionalNumbers = value; }
			}
			IEnumerable<ZString> additionalNumbers;

			public ZString FilerOrSCAC
			{
				get;
				internal set;
			}

			public ZString Number
			{
				get;
				internal set;
			}

			public ZString ReferenceNumber
			{
				get;
				internal set;
			}

			public ZString ShipmentNo
			{
				get;
				internal set;
			}

			public ZString XTN
			{
				get;
				internal set;
			}
		}
	}
}
