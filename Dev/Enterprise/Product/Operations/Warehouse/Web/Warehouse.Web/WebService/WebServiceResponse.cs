namespace Enterprise.Warehouse.Web.WebService
{
	public class WebServiceResponse
	{
		public WebServiceResponse()
		{
			Error = ErrorTypes.None;
			SecurityKey = "";
		}

		#region Properties

		public string SecurityKey { get; set; }

		public ErrorTypes Error { get; set; }

		public string ErrorMessage { get; set; }

		#endregion

		public void LogError(ErrorTypes error, string errorMessage)
		{
			Error = error;
			ErrorMessage = errorMessage;
		}
	}

	public enum ErrorTypes
	{
		None,
		LoginFailed,
		UserLoggedInFromAnotherDevice,
		UserWasRemotelyLoggedOut,
		WarningOnly,
		BusinessValidationError,
		ConnectionFailed,
		YesNoEnquiry,
		UpgradeRequired,
		PalletAsnLineMissingMandatoryAttributes,
		Information,
		PalletAlreadyUnloaded
	}
}
