using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public interface IRawSupportingDocument
	{
		string Code { get; }
		DateTime? StartDate { get; }
		DateTime? EndDate { get; }
		string Description { get; }
		bool IsRetroActiveRequired { get; }
		bool IsYearRequired { get; }
		bool IsCountryRequired { get; }
		bool IsCertificateIdRequired { get; }
		bool IsQuantityRequired { get; }
		bool IsUnitOfQuantityRequired { get; }
		bool IsElectronicFolderRequired { get; }
		string ElectronicFolderNote { get; }
		bool IsPaperFolderRequired { get; }
		string PaperFolderNote { get; }
		SupportingDocumentType Type { get; }
	}
}
