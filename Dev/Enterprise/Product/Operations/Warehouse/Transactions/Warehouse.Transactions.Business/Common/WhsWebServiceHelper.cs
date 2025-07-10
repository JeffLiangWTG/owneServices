using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsWebAPIServiceHelper
	{
		#region SaveFactoryWithExceptionHandling

		public static string SaveFactoryWithExceptionHandling(BusinessObjectFactory factory, Func<ZSaveConcurrencyException, string> getErrorMessageForConcurrencyException)
		{
			var result = string.Empty;
			SaveFactoryWithExceptionHandling(factory, ex =>
			{
				result = ex is ZSaveConcurrencyException concurrencyException
					? getErrorMessageForConcurrencyException(concurrencyException)
					: ex.Message;
			});

			return result;
		}

		static void SaveFactoryWithExceptionHandling(BusinessObjectFactory factory, Action<Exception> onExceptionThrown)
		{
			try
			{
				factory.Save();
			}
			catch (ZCannotSaveException ex)
			{
				onExceptionThrown(ex);
			}
			catch (ZSaveConcurrencyException ex)
			{
				onExceptionThrown(ex);
			}
		}

		#endregion
	}
}
