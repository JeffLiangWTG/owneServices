using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business
{
	public class CusStorageDocPivotValidation : Customs.Business.CusStorageDocPivotValidation
	{
		public CusStorageDocPivotValidation(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		protected override void CheckCSD_DocType()
		{
			base.CheckCSD_DocType();

			ListValidation.ErrorIfInvalidCode(Parent.CSD_DocTypeInfo);
		}

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();

			ListValidation.ErrorIfInvalidPK(Parent.CSD_StorageDocReferenceInfo, Parent.Lookups.AvailableEDocs);

			var attachment = (ITSWAttachment)Parent;
			var attachmentParent = (ICusStorageDocPivotParent)Parent.Parent;

			if (attachment != null && attachmentParent != null)
			{
				var attachmentCollection = attachmentParent.EDocPivotCollection.Cast<ITSWAttachment>();
				if (attachmentCollection.Count(x => x.UniqueIdentifier == attachment.UniqueIdentifier) > 1)
				{
					Parent.CSD_StorageDocReferenceInfo.AddError(Res.GetString("E85F84A1-0456-48E2-B230-F22B5D981887", "Only one of each supporting document can be sent."));
				}

				if (attachmentCollection.Any(x => x.UniqueIdentifier != attachment.UniqueIdentifier
				&& TSWMessageFormatter.FormatAcceptableFileNameForNZC(x.FileName).Equals(TSWMessageFormatter.FormatAcceptableFileNameForNZC(attachment.FileName), StringComparison.OrdinalIgnoreCase)))
				{
					Parent.CSD_StorageDocReferenceInfo.AddError(Res.GetString("800DA956-697D-4755-9502-36A53ADDD32A", "Filename contains Invalid character(s)."));
					return;
				}

				if (attachment.FileTooBig)
				{
					Parent.CSD_StorageDocReferenceInfo.AddError(Res.GetString("95EF2A4E-D0E5-4C64-91DC-74EBD1E2E108", @"The supporting document selected exceeds the NZ Customs system limitation file size maximum, [{0} bytes],
defined in the Registry (Customs > New Zealand > TSW > Attached documents maximum size).", NZCustomsDataRegistry.Instance.MaxMessageAttachmentSize.Value));
				}
			}
		}

		protected string DuplicateDocumentErrorMessage => Res.GetString("A2322427-1C24-4EF1-A444-C26517BD7B16", "Cannot add this document. The same document is found in the Header or in another House Bill.");
	}
}
