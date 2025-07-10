using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class RefDataRepoRawEuropeanSupportingDocument : IRawSupportingDocument
	{
		public RefDataRepoRawEuropeanSupportingDocument(
			string code,
			DateTime? startDate,
			DateTime? endDate,
			string description)
		{
			_code = Argument.NotNullOrEmpty(code, nameof(code));
			_startDate = startDate;
			_endDate = endDate;
			_description = description;
		}

		string IRawSupportingDocument.Code => _code;

		DateTime? IRawSupportingDocument.StartDate => _startDate;

		DateTime? IRawSupportingDocument.EndDate => _endDate;

		string IRawSupportingDocument.Description => _description;

		bool IRawSupportingDocument.IsRetroActiveRequired => false;

		bool IRawSupportingDocument.IsYearRequired => false;

		bool IRawSupportingDocument.IsCountryRequired => false;

		bool IRawSupportingDocument.IsCertificateIdRequired => false;

		bool IRawSupportingDocument.IsQuantityRequired => false;

		bool IRawSupportingDocument.IsUnitOfQuantityRequired => false;

		bool IRawSupportingDocument.IsElectronicFolderRequired => false;

		string IRawSupportingDocument.ElectronicFolderNote => string.Empty;

		bool IRawSupportingDocument.IsPaperFolderRequired => false;

		string IRawSupportingDocument.PaperFolderNote => string.Empty;

		SupportingDocumentType IRawSupportingDocument.Type => SupportingDocumentType.European;

		readonly string _code;
		readonly DateTime? _startDate;
		readonly DateTime? _endDate;
		readonly string _description;
	}
}
