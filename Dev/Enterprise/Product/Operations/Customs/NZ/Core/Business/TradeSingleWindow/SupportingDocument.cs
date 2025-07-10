using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SupportingDocument : NonPersistentBusinessObject, ITSWAttachment, IObsoleteValidation
	{
		public SupportingDocument(BusinessObjectFactory factory, string msgType)
			: base(factory)
		{
			this.msgType = msgType;
		}
		readonly string msgType;

		#region DocumentType

		[MaxLength(3)]
		public ZString AttachmentType
		{
			get { return attachmentType; }
			set
			{
				var trimmedValue = value.TrimEnd();
				CheckMaximumLength(AttachmentTypeInfo, trimmedValue);
				SetNonPersistentPropertyValue(AttachmentTypeInfo, ref attachmentType, trimmedValue);
				if (!IsValidationSuspended)
				{
					ValidateDocumentType();
				}
			}
		}
		ZString attachmentType;

		public ZPropertyInfo AttachmentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AttachmentType)); }
		}

		void ValidateDocumentType()
		{
			AttachmentTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCodeOrEmpty(AttachmentTypeInfo, AttachmentTypes);
		}

		#endregion

		#region AttachmentTypes

		public CodeDescriptionPairList AttachmentTypes
		{
			get
			{
				switch (msgType)
				{
					case MessageTypeList.Codes.CRE:
						return Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForCRE>();
					case MessageTypeList.Codes.I10:
					case MessageTypeList.Codes.I11:
					case MessageTypeList.Codes.I51:
					case MessageTypeList.Codes.I52:
					case MessageTypeList.Codes.I53:
					case MessageTypeList.Codes.IPI:
						return Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForIM1>();
					case MessageTypeList.Codes.E40:
					case MessageTypeList.Codes.E41:
						return Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForEX1>();
					case MessageTypeList.Codes.OCR:
						return Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForOCR>();
					default:
						return Factory.GetCachedValue<AttachmentTypeList>();
				}
			}
		}

		#endregion

		#region eDoc

		public ZGuid eDoc
		{
			get { return edoc; }
			set
			{
				SetNonPersistentPropertyValue(eDocInfo, ref edoc, value);
				if (!IsValidationSuspended)
				{
					ValidateeDoc();
				}
			}
		}
		ZGuid edoc;

		public ZPropertyInfo eDocInfo
		{
			get { return GetZPropertyInfo(nameof(eDoc)); }
		}

		#endregion

		void ValidateeDoc()
		{
			eDocInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(eDocInfo);
			ListValidation.ErrorIfInvalidPK(eDocInfo, StorageDocs);

			if (ParentCollections.Count == 1)
			{
				int occurrenceOfEdoc = 0;
				foreach (ITSWAttachment attachment in ParentCollections.First())
				{
					if (attachment.UniqueIdentifier == eDoc)
					{
						occurrenceOfEdoc++;
						if (occurrenceOfEdoc > 1)
						{
							eDocInfo.AddError("Only one of each supporting document can be sent.");
							return;
						}
					}

					if (ParentCollections.First().Cast<ITSWAttachment>().Any(x => x.UniqueIdentifier != attachment.UniqueIdentifier
					&& TSWMessageFormatter.FormatAcceptableFileNameForNZC(x.FileName).Equals(TSWMessageFormatter.FormatAcceptableFileNameForNZC(attachment.FileName), StringComparison.OrdinalIgnoreCase)))
					{
						eDocInfo.AddError("Filename contains Invalid character(s).");
						return;
					}

					if (attachment.FileTooBig)
					{
						eDocInfo.AddError(string.Format(System.Globalization.CultureInfo.CurrentCulture, @"The supporting document selected exceeds the NZ Customs system limitation file size maximum, [{0} bytes],
defined in the Registry (Customs > New Zealand > TSW > Attached documents maximum size).", NZCustomsDataRegistry.Instance.MaxMessageAttachmentSize.Value));
						return;
					}
				}
			}
		}

		#region StorageDocs

		public CodeDescriptionPairList StorageDocs
		{
			get { return ((SupportingDocumentCollection)ParentCollections.First()).StorageDocs; }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDocumentType();
			ValidateeDoc();
		}

		#endregion

		#region ITSWAttachment Members

		ZGuid ITSWAttachment.UniqueIdentifier
		{
			get { return eDoc; }
		}

		ZString ITSWAttachment.FileName
		{
			get
			{
				if (eDoc.IsValid)
				{
					foreach (ICodeDescription codeDescription in StorageDocs)
					{
						if ((ZGuid)codeDescription.PK == eDoc)
						{
							return codeDescription.Code;
						}
					}
				}

				return "";
			}
		}

		ZString ITSWAttachment.DocType
		{
			get { return AttachmentType; }
		}

		ZBool ITSWAttachment.FileTooBig
		{
			get
			{
				if (eDoc.IsValid)
				{
					foreach (ICodeDescription codeDescription in StorageDocs)
					{
						if ((ZGuid)codeDescription.PK == eDoc)
						{
							return codeDescription.Description.EndsWith(StorageDocList.FileTooBigIndicator, StringComparison.OrdinalIgnoreCase);
						}
					}
				}

				return false;
			}
		}

		#endregion
	}
}
