using System;
using System.Reflection;
using CargoWise.ComponentModel;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowMacroApplyTagValidation : WorkflowMacroValidation
	{
		public WorkflowMacroApplyTagValidation(ProcessTaskNotification action)
		{
			DetailWarningMessage = Globals.IsUserInteractive
				? string.Empty
				: Res.GetString("022b8516-a678-4b0f-bc27-b6558a0e2a3a", "Object: {0}", action.GetDiagnosticLogInfo());
		}

		public override bool Validate(Type componentType, PropertyInfo propertyInfo, string subPath, string nextPropertyName, INotifications notifications)
		{
			if (propertyInfo == null)
			{
				return true;
			}

			if (!typeof(IProcessHeaderCollection).IsAssignableFrom(propertyInfo.PropertyType))
			{
				MacroHelper.NotifyError(
					notifications,
					Res.GetString("03e6703d-28d4-4752-94d4-44fcc600b921", "Property {0}.{1} of type {2} cannot be used in 'Apply Tag' trigger action - only {3} type is supported.",
						componentType.FullName,
						propertyInfo.Name,
						propertyInfo.PropertyType.Name,
						nameof(IProcessHeaderCollection)),
					DetailWarningMessage);

				return false;
			}

			return base.Validate(componentType, propertyInfo, subPath, nextPropertyName, notifications);
		}
	}
}
