using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	public static class ExtensionMethods
	{
		#region BusinessObject.ReplaceOverflowExceptionWithRowError

		public static void ReplaceOverflowExceptionWithRowError(this BusinessObject bizO, ZString propertyName, Action calculations)
		{
			try
			{
				calculations();
			}
			catch (OverflowException)
			{
				var errorMessage = GetAttemptToOverflowCapacityErrorMessage(propertyName);
				if (!bizO.RowErrors.Any(e => e.Message.Equals(errorMessage)))
				{
					bizO.AddRowError(errorMessage);
				}
			}
		}

		static string GetAttemptToOverflowCapacityErrorMessage(string propertyName)
		{
			return Res.GetString("72B16CA7-202E-4E6C-A8F2-E91E5A016CA9", "Attempt to overflow capacity of {0}. Please validate your setup and restart the process.", propertyName);
		}

		#endregion

	}
}
