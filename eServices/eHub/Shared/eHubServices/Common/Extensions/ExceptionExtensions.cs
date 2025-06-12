using System;
using System.Text;
using System.Threading;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common.Extensions
{
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Critical errors SHOULD be passed to the top level exception handler.
		/// </summary>
		public static bool IsCriticalException(this Exception ex)
		{
			return ex.Find<ThreadAbortException>() != null || ex.Find<OutOfMemoryException>() != null || ex.Find<AppDomainUnloadedException>() != null;
		}

		public static T Find<T>(this Exception ex) where T : Exception
		{
			do
			{
				T result = ex as T;
				if (result != null)
				{
					return result;
				}

				ex = ex.InnerException;
			} while (ex != null);

			return null;
		}

	}
}