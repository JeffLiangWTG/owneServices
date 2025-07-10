namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceSequenceValidationProvider
	{
		bool IsPrintingAuthorizationNumberLengthValid(string number);
		bool IsPrintingAuthorizationNumberFormatValid(string number);

		AccComplianceSequenceValidation GetAccComplianceSequenceValidation(AccComplianceSequence sequence);
	}
}
