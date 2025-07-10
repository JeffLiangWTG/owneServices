using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class AdditionalMessageInformation : NonPersistentBusinessObject, IAdditionalInformation, IObsoleteValidation
	{
		public AdditionalMessageInformation(TSWMessage lastMessage, IEnumerable<IStorageDocsBaseCollection> eDocs, TSWTransactionTypes transactionType, BusinessObjectFactory factory, string msgType)
			: this(transactionType, factory)
		{
			this.msgType = msgType;
			this.eDocs = eDocs;
			this.lastMessage = lastMessage;
		}
		readonly string msgType;
		readonly IEnumerable<IStorageDocsBaseCollection> eDocs;
		readonly TSWMessage lastMessage;

		public AdditionalMessageInformation(TSWTransactionTypes transactionType, BusinessObjectFactory factory)
			: base(factory)
		{
			this.transactionType = transactionType;
			ValidateAll();
		}

		public readonly TSWTransactionTypes transactionType;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		#region Properties

		#region AM_FreeText

		[MaxLength(512)]
		public ZString AM_FreeText
		{
			get { return fAM_FreeText; }
			set
			{
				CheckMaximumLength(AM_FreeTextInfo, value);
				SetNonPersistentPropertyValue(AM_FreeTextInfo, ref fAM_FreeText, value);
			}
		}
		ZString fAM_FreeText;

		public ZPropertyInfo AM_FreeTextInfo
		{
			get { return GetZPropertyInfo(nameof(AM_FreeText)); }
		}

		#endregion

		#region AM_OverrideText

		[MaxLength(512)]
		public ZString AM_OverrideText
		{
			get { return fAM_OverrideText; }
			set
			{
				CheckMaximumLength(AM_OverrideTextInfo, value);
				SetNonPersistentPropertyValue(AM_OverrideTextInfo, ref fAM_OverrideText, value);
			}
		}
		ZString fAM_OverrideText;

		public ZPropertyInfo AM_OverrideTextInfo
		{
			get { return GetZPropertyInfo(nameof(AM_OverrideText)); }
		}

		#endregion

		#region AM_AdditionalStatementText

		[MaxLength(512)]
		public ZString AM_AdditionalStatementText
		{
			get { return fAM_AdditionalStatementText; }
			set
			{
				CheckMaximumLength(AM_AdditionalStatementTextInfo, value);
				SetNonPersistentPropertyValue(AM_AdditionalStatementTextInfo, ref fAM_AdditionalStatementText, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_AdditionalStatementText();
				}
			}
		}
		ZString fAM_AdditionalStatementText;

		public ZPropertyInfo AM_AdditionalStatementTextInfo
		{
			get { return GetZPropertyInfo(nameof(AM_AdditionalStatementText)); }
		}

		#endregion

		#region AM_QueueForManifesting

		public ZBool AM_QueueForManifesting
		{
			get { return fAM_QueueForManifesting; }
			set
			{
				SetNonPersistentPropertyValue(AM_QueueForManifestingInfo, ref fAM_QueueForManifesting, value);
			}
		}
		ZBool fAM_QueueForManifesting;

		public ZPropertyInfo AM_QueueForManifestingInfo
		{
			get { return GetZPropertyInfo(nameof(AM_QueueForManifesting)); }
		}

		#endregion

		#region AM_SendManifest

		public ZBool AM_SendManifest
		{
			get { return fSendManifest; }
			set
			{
				SetNonPersistentPropertyValue(SendManifestInfo, ref fSendManifest, value);
			}
		}
		ZBool fSendManifest;

		public ZPropertyInfo SendManifestInfo
		{
			get { return GetZPropertyInfo(nameof(AM_SendManifest)); }
		}

		#endregion

		void ValidateAll()
		{
			ValidateAM_AdditionalStatementText();
		}

		void ValidateAM_AdditionalStatementText()
		{
			AM_AdditionalStatementTextInfo.ClearAllNotifications();
			if (transactionType == TSWTransactionTypes.Cancel || transactionType == TSWTransactionTypes.Replace)
			{
				if (AM_AdditionalStatementText.IsEmpty)
				{
					if (transactionType == TSWTransactionTypes.Cancel)
					{
						AM_AdditionalStatementTextInfo.AddError(Res.GetString("24B422D3-599E-42CE-8DCD-39E11D109292", "Must be entered to advise the reason for the cancellation."));
					}
					else if (transactionType == TSWTransactionTypes.Replace)
					{
						AM_AdditionalStatementTextInfo.AddError(Res.GetString("A4886FF8-3DCE-4FE4-9543-DABDF6901625", "Must be entered to advise the reason for the change."));
					}
				}
			}
		}

		#endregion

		#region SupportingDocuments

		public void GatherPotentialSupportingDocuments()
		{
			if (eDocs != null)
			{
				foreach (IStorageDocsBaseCollection storageDocsCollection in eDocs)
				{
					SupportingDocuments.SetStorageDocs(storageDocsCollection);
				}
			}

			if (lastMessage != null)
			{
				foreach (CodeDescriptionPair lastMessagePair in lastMessage.SupportingDocuments)
				{
					ICodeDescription storageDocPair = SupportingDocuments.StorageDocs[lastMessagePair.Description, StringComparison.OrdinalIgnoreCase];
					if (storageDocPair != null)
					{
						SupportingDocument supportingDocument = SupportingDocuments.AddNew();
						supportingDocument.AttachmentType = lastMessagePair.Code;
						supportingDocument.eDoc = new ZGuid(storageDocPair.PK);
						supportingDocument.AddRowWarning("Verify that these supporting documents should be resent.");
					}
				}
			}
		}

		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(Factory, msgType);
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		#endregion

		#region AdditionalInformation

		ZString IAdditionalInformation.FreeText
		{
			get { return AM_FreeText.Replace("\r\n", " ").Replace("\r", "").Replace("\n", ""); }
		}

		ZString IAdditionalInformation.ManualOverrideText
		{
			get { return AM_OverrideText.Replace("\r\n", " ").Replace("\r", "").Replace("\n", ""); }
		}

		ZString IAdditionalInformation.AdditionalStatementText
		{
			get { return AM_AdditionalStatementText.Replace("\r\n", " ").Replace("\r", "").Replace("\n", ""); }
		}

		IEnumerable<ITSWAttachment> IAdditionalInformation.SupportingDocuments => SupportingDocuments.Cast<ITSWAttachment>();

		ZBool IAdditionalInformation.QueueForManifesting
		{
			get { return AM_QueueForManifesting; }
		}

		ZBool IAdditionalInformation.SendManifest
		{
			get { return AM_SendManifest; }
		}

		#endregion
	}
}
