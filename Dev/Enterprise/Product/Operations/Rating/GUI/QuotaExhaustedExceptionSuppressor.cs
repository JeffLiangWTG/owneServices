using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	public class QuotaExhaustedExceptionSuppressor : IExtraExceptionHandler
	{
		public const int NativeErrorCode = 1816;
		static readonly object registerLock = new object();

		QuotaExhaustedExceptionSuppressor() { }

		public bool HandleException(Exception exceptionToHandle)
		{
			var win32exception = exceptionToHandle as Win32Exception;
			var quotaException = (win32exception?.NativeErrorCode ?? 0) == NativeErrorCode;

			return quotaException;
		}

		public static IDisposable SuppressException()
		{
			var suppressor = RegisterSuppressor();
			return new DisposableAction(() => UnregisterSuppressor(suppressor));
		}

		static QuotaExhaustedExceptionSuppressor RegisterSuppressor()
		{
			if (ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName) is ArrayList extraExceptionHandlers)
			{
				lock (registerLock)
				{
					var result = new QuotaExhaustedExceptionSuppressor();
					extraExceptionHandlers.Add(result);
					return result;
				}
			}
			return null;
		}

		static void UnregisterSuppressor(QuotaExhaustedExceptionSuppressor suppressor)
		{
			if (suppressor != null &&
				ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName) is ArrayList extraExceptionHandlers)
			{
				extraExceptionHandlers.Remove(suppressor);
			}
		}
	}
}
