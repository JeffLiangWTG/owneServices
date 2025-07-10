namespace Enterprise.Freight.Business
{
	public enum ConfirmationOption
	{
		YesNo
	}

	public enum ConfirmationResult
	{
		None, Yes, No
	}

	public interface IConfirmationProvider
	{
		ConfirmationResult GetConfirmation(string message, string caption, ConfirmationOption option);
	}
}
