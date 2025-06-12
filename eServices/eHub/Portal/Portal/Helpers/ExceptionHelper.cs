using System;

namespace CargoWise.eHub.Portal.Helpers
{
	public static class ExceptionHelper
	{
		public static string GetExceptionMessages(Exception ex)
		{
			if (ex == null)
			{
				return string.Empty;
			}
			if (ex.InnerException == null)
			{
				return ex.Message;
			}
			return $"{ex.Message}{Environment.NewLine}{GetExceptionMessages(ex.InnerException)}";
		}
	}
}