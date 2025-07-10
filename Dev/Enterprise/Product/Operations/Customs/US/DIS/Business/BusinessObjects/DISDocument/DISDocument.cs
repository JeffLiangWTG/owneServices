using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISDocument : AutoDISDocument, Integration.Customs.US.DIS.IDISDocument, IDISDocumentBase
	{
		public DISDocument(DISHostWrapper hostWrapper)
			: base(hostWrapper.Factory)
		{
			this.HostWrapper = hostWrapper;
		}

		internal readonly DISHostWrapper HostWrapper;

		public override bool CanDelete
		{
			get { return !StatusList.IsDocumentIDSentToCustoms(Status) && !StatusList.IsWaitingForResponse(Status); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return StatusList.IsWaitingForResponse(Status) ? ResString.GetMultilingualString("A4E844EA-D21E-4E0D-84AC-95AE1AFDB899", "This document has been submitted to Customs. Delete is not allowed when a response from Customs is outstanding.") :
					StatusList.IsDocumentIDSentToCustoms(Status) ? ResString.GetMultilingualString("7817DAFB-DB74-475C-AE96-62ABC85346C2", "This document has been accepted by Customs. Delete is not allowed once a document is accepted.") : (NoResString)string.Empty;
			}
		}

		[ReadOnlyMember(nameof(Status_ReadOnly))]
		public ZString Status
		{
			get { return status; }
			set
			{
				SetNonPersistentPropertyValue(StatusInfo, ref status, value);
				documentRejectReasonCached = null;
				documentReviewCommentCached = null;
			}
		}
		ZString status;

		bool Status_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}

		public ZString StatusDescription
		{
			get { return Status.IsEmpty ? "Not Sent" : Factory.GetCachedValue<StatusList>().GetDescriptionFromCode(Status); }
		}

		public OptionalDataTypes OptionalDataTypes
		{
			get
			{
				return DocumentLabelList.GetOptionalDataTypes(DISFormCusCode);
			}
		}

		public ZBool BondDataVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.BondData); }
		}

		public ZBool ToxicSubstanceVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.ToxicSubstance); }
		}

		public ZBool PermitVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.Permit); }
		}

		public ZBool CertificateVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.Certificate); }
		}

		public ZBool PackingListVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.PackingList); }
		}

		public ZBool InvoiceVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.Invoice); }
		}

		public ZBool CommodityDataVisible
		{
			get { return DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes, OptionalDataTypes.Commodity); }
		}

		public ZBool NoOptionalDataVisible
		{
			get { return OptionalDataTypes == Business.OptionalDataTypes.None; }
		}

		public ZString NoOptionalDataVisibleReason
		{
			get
			{
				ZString reason;

				if (DocumentLabel.IsEmpty)
				{
					reason = EnterAFormTypeMessage;
				}
				else if (DISFormCusCode == null)
				{
					reason = EnterAValidFormTypeMessage;
				}
				else
				{
					reason = FormTypeNotRequireDataMessage;
				}

				return reason;
			}
		}

		public static ZString EnterAFormTypeMessage
		{
			get { return Res.GetString("707e55c1-8aae-469d-b21c-1aca13b738d3", "Please enter a Form Type first."); }
		}

		public static ZString EnterAValidFormTypeMessage
		{
			get { return Res.GetString("5a43cb2f-2992-49e9-8a25-68392ca18ba4", "Please enter a valid Form Type."); }
		}

		public static ZString FormTypeNotRequireDataMessage
		{
			get { return Res.GetString("bb93f343-4fe3-4496-b980-9f45ae1877c4", "The selected Form Type does not require Optional Data."); }
		}

		[List(nameof(DocumentLabels))]
		public override ZString DocumentLabel
		{
			get { return base.DocumentLabel; }
			set
			{
				var oldValue = DocumentLabel;
				base.DocumentLabel = value;
				var newValue = DocumentLabel;

				if (oldValue != newValue && !newValue.IsEmpty)
				{
					var agencyCode = GetDefaultAgencyCode(newValue);
					if (!agencyCode.IsEmpty && !PGAs.Cast<DISPGA>().Any(p => p.Code == agencyCode))
					{
						PGAs.AddNew().Code = agencyCode;
					}

					Validation.ValidateEDocsDocumentPK();
				}
			}
		}

		[List(nameof(ShipmentList))]
		public override ZString ShipmentNo
		{
			get
			{
				return base.ShipmentNo;
			}
			set
			{
				var hasChanged = ShipmentNo != value;
				if (hasChanged)
				{
					base.ShipmentNo = value;
					var tradeTranscation = HostWrapper.DefaultValues.DefaultTradeTransactions.FirstOrDefault(x => x.ShipmentNo == value);
					if (tradeTranscation != null)
					{
						ITN = tradeTranscation.Number;
						XTN = tradeTranscation.XTN;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ITN_ReadOnly))]
		public override ZString ITN
		{
			get
			{
				return base.ITN;
			}

			set
			{
				base.ITN = value;
			}
		}

		bool ITN_ReadOnly
		{
			get { return true; }
		}

		[ReadOnlyMember(nameof(XTN_ReadOnly))]
		public override ZString XTN
		{
			get
			{
				return base.XTN;
			}

			set
			{
				base.XTN = value;
			}
		}

		bool XTN_ReadOnly
		{
			get { return true; }
		}

		public void DefaultShipmentNo()
		{
			if (ShipmentList.Count == 1 && ShipmentNo.IsEmpty)
			{
				ShipmentNo = ShipmentList[0].Code;
			}
		}

		ZString GetDefaultAgencyCode(ZString documentLabel)
		{
			var result = ZString.Empty;
			var pgaCode = Factory.GetCachedValue<PGAList>().Cast<CodeDescriptionPair>().FirstOrDefault(p => string.Compare(p.Code, 0, documentLabel, 0, 3, true, CultureInfo.CurrentCulture) == 0);

			if (pgaCode != null)
			{
				result = pgaCode.Code;
			}

			return result;
		}

		[System.Xml.Serialization.XmlIgnore]
		public DISDocumentLabelCollection DocumentLabels
		{
			get
			{
				if (documentLabelCollection == null)
				{
					documentLabelCollection = new DISDocumentLabelCollection(Factory, FormGroups);
				}
				return documentLabelCollection;
			}
		}
		DISDocumentLabelCollection documentLabelCollection;

		public ZString DocumentLabelUSDISDocCode
		{
			get
			{
				var documentLabel = DocumentLabel;
				return Factory.GetCachedValue(DocumentLabel + "_USDISDocCode_" + ZDateTime.Today.ToShortDateString(), () =>
				{
					var result = ZString.Empty;
					var uSDISDocCode = new RefCusCodeListAttribute.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, documentLabel, RefCusCodeListAttributeTypes.Codes.USDISDocCode).FirstOrDefault();
					if (uSDISDocCode != null)
					{
						result = uSDISDocCode.ZZE_Value;
					}
					return result;
				});
			}
		}

		[System.Xml.Serialization.XmlIgnore]
		public ZZRefCusCodeListCombined DISFormCusCode
		{
			get
			{
				if (disFormCusCode == null || disFormCusCode.ZZD_Code != DocumentLabel)
				{
					disFormCusCode = LookupDISCusCode(DocumentLabel);
				}
				return disFormCusCode;
			}
		}
		ZZRefCusCodeListCombined disFormCusCode;

		bool IsValidDISCode(ZString code)
		{
			return LookupDISCusCode(code) != null;
		}

		ZZRefCusCodeListCombined LookupDISCusCode(ZString code)
		{
			return DocumentLabels.FindBestMatch(code);
		}

		IEnumerable<ZString> FormGroups
		{
			get
			{
				if (formGroups == null)
				{
					var list = new List<ZString>(HostWrapper.DISHost.FormGroups);
					list.Add(DISFormGroupCodes.NoGroup);
					formGroups = list;
				}
				return formGroups;
			}
		}
		IEnumerable<ZString> formGroups;

		public CodeDescriptionPairList ShipmentList
		{
			get { return HostWrapper.Lookups.ShipmentList; }
		}

		internal IEnumerable<ZString> GetRestrictedFileTypes()
		{
			return DISFormCusCode?.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.USDISSupportedFileTypes).Select(v => { return v.ToLower(); }).ToArray() ?? System.Array.Empty<ZString>();
		}

		[ReadOnlyMember(nameof(IDSuffix_ReadOnly))]
		public override ZInt IDSuffix
		{
			get { return base.IDSuffix; }
			set { base.IDSuffix = value; }
		}

		bool IDSuffix_ReadOnly
		{
			get { return true; }
		}

		public ZString DocumentRejectReason
		{
			get
			{
				if (!documentRejectReasonCached.HasValue)
				{
					documentRejectReasonCached = ZString.Empty;
					if (Status == StatusList.Codes.ERV)
					{
						var documentReviewResult = GetDocReviewResultFromLatestDocReviewResponse();
						if (documentReviewResult != null)
						{
							var rejectReason = documentReviewResult.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentRejectReason));
							documentRejectReasonCached = rejectReason != null ? rejectReason.Value : "";
						}
					}
				}
				return documentRejectReasonCached.Value;
			}
		}
		ZString? documentRejectReasonCached;

		public ZString DocumentReviewComment
		{
			get
			{
				if (!documentReviewCommentCached.HasValue)
				{
					documentReviewCommentCached = ZString.Empty;
					if (Status == StatusList.Codes.ERV)
					{
						var documentReviewResult = GetDocReviewResultFromLatestDocReviewResponse();
						if (documentReviewResult != null)
						{
							var reviewComment = documentReviewResult.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewComment));
							documentReviewCommentCached = reviewComment != null ? reviewComment.Value : "";
						}
					}
				}
				return documentReviewCommentCached.Value;
			}
		}
		ZString? documentReviewCommentCached;

		XElement GetDocReviewResultFromLatestDocReviewResponse()
		{
			XElement result = null;
			var latestDocReviewResponse = Messages.Cast<EDIMessage>().Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Receive &&
					m.EM_MessageType == MessageTypeList.Codes.DocumentReviewResponse).OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();
			if (latestDocReviewResponse != null)
			{
				result = latestDocReviewResponse.MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewResult));
			}

			return result;
		}

		public ZString DocumentID
		{
			get { return FormatDocumentID(HostWrapper.DISHost.JobNumber, IDSuffix.ToString()); }
		}

		internal static ZString FormatDocumentID(ZString jobNumber, ZString idSuffix)
		{
			return jobNumber + "_DIS" + idSuffix;
		}

		[List(nameof(RequiredDocuments))]
		[ReadOnlyMember(nameof(RequiredDocumentPK_ReadOnly))]
		public ZGuid RequiredDocumentPK
		{
			get { return requiredDocumentPK; }
			set
			{
				SetNonPersistentPropertyValue(RequiredDocumentPKInfo, ref requiredDocumentPK, value);
				Validation.ValidateRequiredDocumentPK();
				if (RequiredDocumentAddInfo != null)
				{
					RequiredDocumentAddInfo = null;
				}
			}
		}
		ZGuid requiredDocumentPK;

		bool RequiredDocumentPK_ReadOnly
		{
			get { return !CanDelete; }
		}

		public ZPropertyInfo RequiredDocumentPKInfo
		{
			get { return GetZPropertyInfo(nameof(RequiredDocumentPK)); }
		}

		public JobRequiredDocument RequiredDocument
		{
			get { return Factory.Load<JobRequiredDocument>(RequiredDocumentPK); }
		}

		public IBusinessObjectCollection RequiredDocuments
		{
			get { return HostWrapper.Lookups.RequiredDocuments; }
		}

		public JobRequiredDocumentAddInfo RequiredDocumentAddInfo { get; set; }

		ZString IDISDocumentBase.Serialize(string xmlNamespace)
		{
			return BusinessObjectXmlSerializer.Serialize(this, xmlNamespace);
		}

		void IDISDocumentBase.Deserialize(string xml, string xmlNamespace)
		{
			BusinessObjectXmlSerializer.Deserialize(this, xml, xmlNamespace);
		}

		[List(nameof(EDocsList))]
		[ReadOnlyMember(nameof(EDocsDocumentPK_ReadOnly))]
		public override ZGuid EDocsDocumentPK
		{
			get { return base.EDocsDocumentPK; }
			set
			{
				base.EDocsDocumentPK = value;
				var eDoc = EDoc;
				if (eDoc != null)
				{
					if (RequiredDocumentPK.IsEmpty)
					{
						var requiredDoc = RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(x => x.EQ_DocType == eDoc.DocType);
						if (requiredDoc != null)
						{
							RequiredDocumentPK = requiredDoc.PK;
						}
					}
					if (DocumentLabel.IsEmpty)
					{
						var label = DocumentLabelList.TryToGetDefaultFormType(eDoc.DocType);
						if (!label.IsEmpty)
						{
							DocumentLabel = label;
						}
						else if (IsValidDISCode(eDoc.DocType))
						{
							DocumentLabel = eDoc.DocType;
						}
					}
					if (DocumentDescription.IsEmpty)
					{
						DocumentDescription = eDoc.Description.Left(Schema.DocumentDescriptionMaxLength);
					}
				}
			}
		}

		bool EDocsDocumentPK_ReadOnly
		{
			get { return !CanDelete; }
		}

		public CodeDescriptionPairList EDocsList
		{
			get { return HostWrapper.Lookups.EDocsList; }
		}

		public IeDoc EDoc
		{
			get { return EDocsDocumentPK.IsValid ? HostWrapper.GetEDoc(EDocsDocumentPK) : null; }
		}

		public DISCBPRequest CBPRequest
		{
			get
			{
				if (cbpRequests == null)
				{
					cbpRequests = new DISCBPRequest(this);
					RegisterEditableChildObject(cbpRequests);
				}
				return cbpRequests;
			}
		}
		DISCBPRequest cbpRequests;

		public CodeDescriptionPairList DefaultCBPRequestList
		{
			get { return HostWrapper.Lookups.DefaultCBPRequestList; }
		}

		public DISInvoice Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = new DISInvoice(this);
					RegisterEditableChildObject(invoice);
				}
				return invoice;
			}
		}
		DISInvoice invoice;

		public CodeDescriptionPairList DefaultInvoiceList
		{
			get { return HostWrapper.Lookups.InvoiceList; }
		}

		public DISBondData BondData
		{
			get
			{
				if (bondData == null)
				{
					bondData = new DISBondData(this);
					RegisterEditableChildObject(bondData);
				}
				return bondData;
			}
		}
		DISBondData bondData;

		public CodeDescriptionPairList DefaultBondDataList
		{
			get { return HostWrapper.Lookups.BondDataList; }
		}

		public IDISBondDataDefault GetDefaultBondData(string code)
		{
			return HostWrapper.GetDefaultBondData(code);
		}

		public DISPackingListData PackingList
		{
			get
			{
				if (packingList == null)
				{
					packingList = new DISPackingListData(Factory);
					RegisterEditableChildObject(packingList);
				}
				return packingList;
			}
		}
		DISPackingListData packingList;

		public DISCertificateData CertificateData
		{
			get
			{
				if (certificateData == null)
				{
					certificateData = new DISCertificateData(Factory, this);
					RegisterEditableChildObject(certificateData);
				}
				return certificateData;
			}
		}
		DISCertificateData certificateData;

		public DISPermitData PermitData
		{
			get
			{
				if (permitData == null)
				{
					permitData = new DISPermitData(Factory);
					RegisterEditableChildObject(permitData);
				}
				return permitData;
			}
		}
		DISPermitData permitData;

		public DISToxicSubstanceData ToxicSubstanceData
		{
			get
			{
				if (toxicSubstanceData == null)
				{
					toxicSubstanceData = new DISToxicSubstanceData(Factory);
					RegisterEditableChildObject(toxicSubstanceData);
				}
				return toxicSubstanceData;
			}
		}
		DISToxicSubstanceData toxicSubstanceData;

		public DISCommodityLineCollection CommodityData
		{
			get
			{
				if (commodityData == null)
				{
					commodityData = new DISCommodityLineCollection(this);
					RegisterEditableChildObject(commodityData);
				}
				return commodityData;
			}
		}
		DISCommodityLineCollection commodityData;

		public DISAdditionalDataCollection AdditionalData
		{
			get
			{
				if (additionalData == null)
				{
					additionalData = new DISAdditionalDataCollection(Factory);
					RegisterEditableChildObject(additionalData);
				}
				return additionalData;
			}
		}
		DISAdditionalDataCollection additionalData;

		public DISPGACollection PGAs
		{
			get
			{
				if (pgas == null)
				{
					pgas = new DISPGACollection(Factory);
					pgas.CountChanged += delegate
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateDocumentLabel();
						}
					};
					RegisterEditableChildObject(pgas);
				}
				return pgas;
			}
		}
		DISPGACollection pgas;

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					RegisterEditableChildObject(messages);
					messages.SetReadOnlyIncludingChildren(true);
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public ZString MessageSendingWarning
		{
			get
			{
				var sb = new ZStringBuilder();
				var messageWarnings = HostWrapper.MessageSendingWarning;
				if (!messageWarnings.IsEmpty)
				{
					sb.Append(messageWarnings);
				}
				var eDoc = EDoc;
				if (eDoc != null && DISDocumentValidation.HasDocFileNameInvalidCharacters(eDoc.FileName))
				{
					sb.Append(DISDocumentValidation.FileNameHasInvalidCharacters);
				}
				return sb.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString MessageSendingError
		{
			get
			{
				var sb = new ZStringBuilder();
				var messageSendingError = HostWrapper.MessageSendingError;
				if (!messageSendingError.IsEmpty)
				{
					sb.Append(messageSendingError);
				}

				return sb.ToStringWithNewLineBetweenAppends();
			}
		}
	}
}
