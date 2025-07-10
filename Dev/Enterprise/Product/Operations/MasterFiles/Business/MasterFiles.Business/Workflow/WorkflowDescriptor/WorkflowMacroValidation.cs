using System;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Types;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowMacroValidation
	{
		public virtual bool Validate(Type componentType, PropertyInfo propertyInfo, string subPath, string nextPropertyName, INotifications notifications)
		{
			if (propertyInfo == null)
			{
				if (notifications == null || !notifications.ToString().Contains(WhereMacroClause.WhereClauseErrorMessage, StringComparison.Ordinal))
				{
					MacroHelper.NotifyWarning(
						notifications,
						Res.GetString("1d19dac7-1990-465a-acf1-7e0d8c0dabf9", "Cannot find property {0} on {1}.", nextPropertyName, componentType.FullName),
						DetailWarningMessage);
				}

				return false;
			}

			return true;
		}

		#region Implementation

		public ZString DetailWarningMessage { get; set; }

		#endregion
	}
}
