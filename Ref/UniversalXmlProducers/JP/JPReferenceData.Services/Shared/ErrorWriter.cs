using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class ErrorWriter
	{
		public static void WriteError(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:ss}][ERROR]{message}";
				Console.Error.WriteLine(errorMessage);
			}
		}

		public static void WriteException(Exception exception)
		{
			if (exception != null)
			{
				var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:ss}][ERROR][EXCEPTION]";
				Console.Error.WriteLine(errorMessage);
				Console.Error.WriteLine(exception);
			}
		}
	}
}
