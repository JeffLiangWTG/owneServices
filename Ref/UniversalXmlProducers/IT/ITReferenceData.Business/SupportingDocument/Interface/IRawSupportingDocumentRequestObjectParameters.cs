namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public interface IRawSupportingDocumentRequestObjectParameters
	{
		string UC { get; }
		string SC { get; }
		string ST { get; }
		string Label { get; }
		string Suffix { get; }
		string ProgressiveNumber { get; }
		string DescriptionValidityStartDate { get; }
	}
}
