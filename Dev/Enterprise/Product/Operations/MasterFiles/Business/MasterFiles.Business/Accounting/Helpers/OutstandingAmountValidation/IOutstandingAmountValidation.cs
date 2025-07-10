namespace Enterprise.MasterFiles.Business
{
	public interface IOutstandingAmountValidation
	{
		string Validate(IOutstandingAmountValidationWrapper wrapper);
	}
}
