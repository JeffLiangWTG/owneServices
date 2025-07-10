using System;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SupportingDocument : NonPersistentBusinessObject, ICusAttachment, IObsoleteValidation
	{
		public SupportingDocument(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region DocumentType

		[MaxLength(256)]
		public ZString DocumentType
		{
			get { return documentType; }
			set
			{
				CheckMaximumLength(DocumentTypeInfo, value);
				SetNonPersistentPropertyValue(DocumentTypeInfo, ref documentType, value);
				if (!IsValidationSuspended)
				{
					ValidateDocumentType();
				}
			}
		}
		ZString documentType;

		public ZPropertyInfo DocumentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentType)); }
		}

		void ValidateDocumentType()
		{
			DocumentTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCodeOrEmpty(DocumentTypeInfo, DocumentTypes);
		}

		#region DocumentTypes

		public SupportingDocumentTypeCodeList DocumentTypes
		{
			get { return documentTypes ?? (documentTypes = new SupportingDocumentTypeCodeList()); }
		}
		SupportingDocumentTypeCodeList documentTypes;

		#endregion

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

		void ValidateeDoc()
		{
			eDocInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(eDocInfo);
			ListValidation.ErrorIfInvalidPK(eDocInfo, StorageDocs);
			if (eDoc.IsValid)
			{
				foreach (ICodeDescription codeDescription in StorageDocs)
				{
					if ((ZGuid)codeDescription.PK == eDoc)
					{
						if (codeDescription.Code.Length > 70)
						{
							eDocInfo.AddError("The document file name is too long for TradeNet.\r\nRename the attachment to 64 characters or less."); // storage docs prefixes with (6 chars) the document type, (as in "BOE - " for example)
						}

						break;
					}
				}
			}

			if (ParentCollections.Count == 1)
			{
				int occuranceOfEdoc = 0;
				foreach (ICusAttachment attachment in ParentCollections.First())
				{
					if (attachment.UniqueIdentifier == eDoc)
					{
						occuranceOfEdoc++;
						if (occuranceOfEdoc > 1)
						{
							eDocInfo.AddError("Only one of each supporting document can be sent.");
							return;
						}
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

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDocumentType();
			ValidateeDoc();
		}

		#endregion

		#region ICusAttachment Members

		ZGuid ICusAttachment.UniqueIdentifier
		{
			get { return eDoc; }
		}

		ZString ICusAttachment.FileName
		{
			get
			{
				if (eDoc.IsValid)
				{
					foreach (ICodeDescription codeDescription in StorageDocs)
					{
						if ((ZGuid)codeDescription.PK == eDoc)
						{
							string fileExtension = Path.GetExtension(codeDescription.Code);
							if (fileExtension.Equals(".TIF", StringComparison.OrdinalIgnoreCase))
							{
								return Path.GetFileNameWithoutExtension(codeDescription.Code) + ".pdf";
							}
							else
							{
								return codeDescription.Code;
							}
						}
					}
				}

				return "";
			}
		}

		ZString ICusAttachment.DocType
		{
			get { return DocumentType; }
		}

		#endregion
	}
}
