namespace CargoWise.RefDbRepo.Common.Customization.Fody
{
	class FodyFlagHelper
	{
		public static string AppException = "AppException";
		public static string UnhandledException = "UnhandledException";
		public static string GetFlag(string str)
		{
			return $"{Prefix}{str}{Postfix}";
		}

		const string Prefix = "$$";
		const string Postfix = "$$";
	}
}
