using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class SupportingDocSendingObjectValidation : AutoSupportingDocSendingObjectValidation
	{
		public SupportingDocSendingObjectValidation(AutoSupportingDocSendingObject parent) : base(parent)
		{
		}

		public new SupportingDocSendingObject Parent => base.Parent as SupportingDocSendingObject;

		public virtual char[] InvalidFileNameCharsForSending => Array.Empty<char>();

		protected override void CheckEDoc()
		{
			MandatoryValidation.CheckEntered(Parent.EDocInfo);
			ListValidation.ErrorIfInvalidPK(Parent.EDocInfo);
			CheckEDocFileName();
		}

		protected virtual void CheckEDocFileName()
		{
			if (Parent.EDoc.IsValid && Parent.ShouldCheckFileNameInEdocField)
			{
				var fileName = Parent.Document?.FileName;
				if (fileName.HasValue && fileName.Value.IndexOfAny(InvalidFileNameCharsForSending) > -1)
				{
					Parent.EDocInfo.AddError(EDocContainInvalidCharError);
				}
			}
		}

		public virtual string EDocContainInvalidCharError => Res.GetString(
			"09B0B7CB-58E4-49AE-916A-A1E11E741190",
			"The filename contains characters that are not acceptable. On the eDocs tab, please rename the file so that its name does not contain any of the following characters, the retry the operation. Forbidden characters: {0}",
			string.Join(" ", InvalidFileNameCharsForSending));

		protected override void CheckDocumentType()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DocumentTypeInfo);
		}

		protected override void CheckLocalReferenceNumber()
		{
			MandatoryValidation.CheckEntered(Parent.LocalReferenceNumberInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LocalReferenceNumberInfo);
		}

		protected override void CheckCaseNumber()
		{
		}

		public virtual ZInt MaxEDocFileSizeInBytes => 2_000_000;

		public virtual ZString EDocTooLargeError
		{
			get
			{
				var sizeInMB = MaxEDocFileSizeInBytes / 1_000_000;
				return Res.GetString("C0118175-15E7-4E77-BBD7-43ECADF7DF81", "The eDoc selected is larger than the maximum allowed size of {0}MB.", sizeInMB);
			}
		}

		protected override void CheckEDocFileSizeInMB()
		{
			if (Parent.EDoc.IsValid && Parent.ShouldCheckSizeInEdocField)
			{
				var eDoc = Parent.Document;
				if (eDoc?.ImageData.Length > MaxEDocFileSizeInBytes)
				{
					Parent.EDocFileSizeInMBInfo.AddError(EDocTooLargeError);
				}
			}
		}
	}
}
