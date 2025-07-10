namespace Enterprise.Customs.US.Business
{
	public static class FTZJobDeclarationValidationHelper
	{
		public static bool IsNotODZAdmissionType(JobDeclaration declaration)
		{
			return !declaration.IsODZ_AdmissionType;
		}

		public static bool IsFTZAdmissionValidationMode(JobDeclaration declaration)
		{
			return declaration.IsFTZAdmissionValidationMode;
		}

		public static bool IsFTZPTTValidationMode(JobDeclaration declaration)
		{
			return declaration.IsFTZPTTValidationMode;
		}

		public static string GetErrorMessage(JobDeclaration declaration)
		{
			return IsFTZPTTValidationMode(declaration) ? ValidationConstants.FTZ.DataRequired : ValidationConstants.FTZ.DataRequiredWhenPTTIncluded;
		}
	}
}
