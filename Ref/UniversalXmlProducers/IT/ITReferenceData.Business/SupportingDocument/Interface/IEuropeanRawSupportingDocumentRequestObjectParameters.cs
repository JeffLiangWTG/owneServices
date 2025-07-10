namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public interface IEuropeanRawSupportingDocumentRequestObjectParameters : IRawSupportingDocumentRequestObjectParameters
	{
		string RegGrpCountryCode { get; }
	}
}
