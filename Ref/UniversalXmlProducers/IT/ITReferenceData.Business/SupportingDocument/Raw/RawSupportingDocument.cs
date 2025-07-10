using System;
using System.Web;
using CargoWise.RefDbRepo.Common.Argument;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public abstract class RawSupportingDocument : IRawSupportingDocument
	{
		protected RawSupportingDocument(string rawHtml)
		{
			Argument.NotNullOrEmpty(rawHtml, nameof(rawHtml));
			htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(SanitizeHtml(rawHtml));
		}

		readonly HtmlDocument htmlDocument;

		protected HtmlNode DocumentNode => htmlDocument.DocumentNode;

		public string Code => code ?? (code = GetCodeCore());
		string code;

		public DateTime? StartDate
		{
			get
			{
				if (!isStartDateInitialized)
				{
					startDate = GetStartDateCore();
					isStartDateInitialized = true;
				}
				return startDate;
			}
		}

		DateTime? startDate;
		bool isStartDateInitialized;

		public DateTime? EndDate
		{
			get
			{
				if (!isEndDateInitialized)
				{
					endDate = GetEndDateCore();
					isEndDateInitialized = true;
				}
				return endDate;
			}
		}

		DateTime? endDate;
		bool isEndDateInitialized;

		public string Description => description ?? (description = SanitizeDescription(GetDescriptionCore()));
		string description;

		public bool IsRetroActiveRequired => isRetroActiveRequired ?? (isRetroActiveRequired = GetIsRetroActiveRequiredCore()).Value;
		bool? isRetroActiveRequired;

		public bool IsYearRequired => isYearRequired ?? (isYearRequired = GetIsYearRequiredCore()).Value;
		bool? isYearRequired;

		public bool IsCountryRequired => isCountryRequired ?? (isCountryRequired = GetIsCountryRequiredCore()).Value;
		bool? isCountryRequired;

		public bool IsCertificateIdRequired => isCertificateIdRequired ?? (isCertificateIdRequired = GetIsCertificateIdRequiredCore()).Value;
		bool? isCertificateIdRequired;

		public bool IsQuantityRequired => isQuantityRequired ?? (isQuantityRequired = GetIsQuantityRequiredCore()).Value;
		bool? isQuantityRequired;

		public bool IsUnitOfQuantityRequired => isUnitOfQuantityRequired ?? (isUnitOfQuantityRequired = GetIsUnitOfQuantityRequiredCore()).Value;
		bool? isUnitOfQuantityRequired;

		public bool IsElectronicFolderRequired => isElectronicFolderRequired ?? (isElectronicFolderRequired = GetIsElectronicFolderRequiredCore()).Value;
		bool? isElectronicFolderRequired;

		public string ElectronicFolderNote => electronicFolderNote ?? (electronicFolderNote = GetElectronicFolderNoteCore());
		string electronicFolderNote;

		public bool IsPaperFolderRequired => isPaperFolderRequired ?? (isPaperFolderRequired = GetIsPaperFolderRequiredCore()).Value;
		bool? isPaperFolderRequired;

		public string PaperFolderNote => paperFolderNote ?? (paperFolderNote = GetPaperFolderNoteCore());
		string paperFolderNote;

		public SupportingDocumentType Type => supportingDocumentType ?? (supportingDocumentType = GetSupportingDocumentType()).Value;
		SupportingDocumentType? supportingDocumentType;

		protected abstract string GetCodeCore();

		protected abstract DateTime? GetStartDateCore();

		protected abstract DateTime? GetEndDateCore();

		protected abstract string GetDescriptionCore();

		protected abstract bool GetIsRetroActiveRequiredCore();

		protected abstract bool GetIsYearRequiredCore();

		protected abstract bool GetIsCountryRequiredCore();

		protected abstract bool GetIsCertificateIdRequiredCore();

		protected abstract bool GetIsQuantityRequiredCore();

		protected abstract bool GetIsUnitOfQuantityRequiredCore();

		protected abstract bool GetIsElectronicFolderRequiredCore();

		protected abstract string GetElectronicFolderNoteCore();

		protected abstract bool GetIsPaperFolderRequiredCore();

		protected abstract string GetPaperFolderNoteCore();

		protected abstract SupportingDocumentType GetSupportingDocumentType();

		static string SanitizeHtml(string rawHtml) => HttpUtility.HtmlDecode(rawHtml);

		static string SanitizeDescription(string description)
		{
			return description?
				.Replace(Environment.NewLine, " ")
				.Replace("Ã¨", "è")
				.Replace("", " ")
				.Trim();
		}

		protected bool HasCheckedAttribute(string nodePath)
		{
			return DocumentNode
				?.SelectSingleNode(nodePath)
				?.HasCheckedAttribute() ?? false;
		}

		protected bool HasYesValueAttribute(string nodePath)
		{
			return DocumentNode
				?.SelectSingleNode(nodePath)
				?.HasYesValueAttribute() ?? false;
		}
	}
}
