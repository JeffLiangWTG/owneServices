using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using WTG.StaticAnalysis.Annotation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISDocumentValidation : AutoDISDocumentValidation
	{
		public DISDocumentValidation(AutoDISDocument bizObj)
			: base(bizObj)
		{
		}

		new DISDocument Parent
		{
			get { return (DISDocument)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRequiredDocumentPK();
		}

		public void ValidateRequiredDocumentPK()
		{
			ValidateCalculatedProperty(Parent.RequiredDocumentPKInfo);
		}

		protected void CheckRequiredDocumentPK()
		{
			MandatoryValidation.CheckEntered(Parent.RequiredDocumentPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.RequiredDocumentPKInfo);
		}

		protected override void CheckDocumentDescription()
		{
			base.CheckDocumentDescription();

			var docDescription = Parent.DocumentDescription;
			if (HasInvalidCharacters(docDescription))
			{
				Parent.DocumentDescriptionInfo.AddWarning(string.Format(DataHasInvalidCharacters, DISDocument.Schema.DocumentDescription));
			}
		}

		protected override void CheckComment()
		{
			base.CheckComment();
			var comment = Parent.Comment;
			if (HasInvalidCharacters(comment))
			{
				Parent.CommentInfo.AddWarning(string.Format(DataHasInvalidCharacters, DISDocument.Schema.Comment));
			}
		}

		protected override void CheckEDocsDocumentPK()
		{
			base.CheckEDocsDocumentPK();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.EDocsDocumentPKInfo);
			var docFileName = Parent.EDoc?.FileName ?? string.Empty;
			if (!Parent.EDocsList.Cast<CodeElement>().Any(x => x.PK.Equals(Parent.EDocsDocumentPK)))
			{
				Parent.EDocsDocumentPKInfo.AddMessageError(SelectValidEDocs);
			}
			else
			{
				if (HasDocFileNameInvalidCharacters(docFileName))
				{
					Parent.EDocsDocumentPKInfo.AddWarning(FileNameHasInvalidCharacters);
				}

				ZString extension = System.IO.Path.GetExtension(docFileName);
				string fileExtension = extension.SubstringSafe(1).ToLower();

				if (!preferredFileFormats.Contains(fileExtension) && !otherAcceptedFileFormats.Contains(fileExtension))
				{
					Parent.EDocsDocumentPKInfo.AddError(FileIsAnInvalidFormat);
				}
				else
				{
					var restrictedFileTypes = Parent.GetRestrictedFileTypes();
					if (restrictedFileTypes.Any() && !restrictedFileTypes.Contains(fileExtension))
					{
						Parent.EDocsDocumentPKInfo.AddMessageError(FileIsAnExcludedFormat(string.Join(", ", restrictedFileTypes.OrderBy(o => o))));
					}
				}
			}
		}

		internal static string FileIsAnExcludedFormat(string acceptedFormats)
		{
			return Res.GetString("0aa7448c-a3d3-4eb4-999e-9d3168225296", "This eDoc file is an excluded type. File Types for this Form are restricted to {0}", acceptedFormats);
		}

		internal static string FileIsAnInvalidFormat
		{
			get
			{
				return Res.GetString("173145ab-c0e0-4481-8ab5-86156c004f56", "This eDoc file is not an accepted type. Preferred Types are {0} although {1} will be accepted.", preferredFileFormatsJoined, otherAcceptedFileFormatsJoined);
			}
		}

		[ThreadSafe]
		static readonly string[] preferredFileFormats = { "pdf", "gif", "png", "jpg", "jpeg", "xls", "xlsx" };
		[ThreadSafe]
		static readonly string preferredFileFormatsJoined = string.Join(", ", preferredFileFormats);

		[ThreadSafe]
		static readonly string[] otherAcceptedFileFormats = { "doc", "docx", "ppt", "bmp" };
		[ThreadSafe]
		static readonly string otherAcceptedFileFormatsJoined = string.Join(", ", otherAcceptedFileFormats);

		internal static bool HasDocFileNameInvalidCharacters(string fileName)
		{
			return !Regex.IsMatch(fileName.ToUpper(CultureInfo.CurrentCulture), ValidPatternForEDocFileName);
		}

		internal static bool HasInvalidCharacters(string strData)
		{
			if (!string.IsNullOrEmpty(strData))
			{
				return !Regex.IsMatch(strData.ToUpper(CultureInfo.CurrentCulture), NonValidCharactersMatchPattern);
			}
			return false;
		}

		const string ValidPatternForEDocFileName = @"^[!@\#\$\^\*\(\)-_=\+\[\{\]}\\\|;:,\.\?`~¢\ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789]*$";
		const string NonValidCharactersMatchPattern = @"^[!@\#\$%\^&\*\(\)-_=\+\[\{\]}\\\|;:'"",<\.>/\?`~¢\ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789]*$";

		internal const string SelectValidEDocs = "Please enter a valid eDocs.";
		internal const string NoPGAsSelected = "At least one Agency Code is required.";
		internal const string FileNameHasInvalidCharacters = "This eDoc file name has invalid characters. These invalid characters will be removed when sending the message to Customs";
		internal const string DataHasInvalidCharacters = "This {0} has invalid characters. These invalid characters will be removed when sending the message to Customs.";
		internal const string DocTypeIsDeleted = "Please enter a valid type. The requested CBP Form is no longer available";
		internal const string DocTypeIsUnreleased = "Please enter a valid type. The requested CBP Form has not been released";

		protected override void CheckDocumentLabel()
		{
			base.CheckDocumentLabel();

			var errorMessage = GetEffectiveDateErrorMessage(Parent.DISFormCusCode);
			if (!errorMessage.IsEmpty)
			{
				Parent.DocumentLabelInfo.AddMessageError(errorMessage);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DocumentLabelInfo);

				if (Parent.PGAs.Count == 0)
				{
					Parent.DocumentLabelInfo.AddMessageError(NoPGAsSelected);
				}
			}
		}

		protected override void CheckShipmentNo()
		{
			base.CheckShipmentNo();
			if (Parent.HostWrapper.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ShipmentNoInfo);
			}
		}

		ZString GetEffectiveDateErrorMessage(ZZRefCusCodeListCombined refCusCodeList)
		{
			var errorMessage = ZString.Empty;

			if (refCusCodeList != null)
			{
				if (refCusCodeList.ZZD_EndDate <= ZDateTime.Today)
				{
					errorMessage = string.Format(CultureInfo.InvariantCulture, "{0} (effective {1}).", DocTypeIsDeleted, refCusCodeList.ZZD_EndDate.ToBestReadableDateString());
				}
				else if (refCusCodeList.ZZD_StartDate > ZDateTime.Today)
				{
					errorMessage = string.Format(CultureInfo.InvariantCulture, "{0} (effective {1}).", DocTypeIsUnreleased, refCusCodeList.ZZD_StartDate.ToBestReadableDateString());
				}
			}

			return errorMessage;
		}
	}
}
