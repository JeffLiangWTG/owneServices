using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowProcessorHelper
	{
		public static (PropertyInfo FinalPropertyInfo, Type ParentType, ZString fieldName) GetFinalPropertyInfoAndParentType(IProcessTaskNotification action, INotifications notifications, WorkflowMacroValidation validation, IZType fieldValue = null)
		{
			Type parentType = null;
			PropertyInfo finalPropertyInfo = null;
			var fieldNameWithoutPrefix = ZString.Empty;

			var types = RootTypesHelper.GetDataFieldsOnlyRootTypes(((IRootTypeProvider)action).RootTypes);
			if (types.Length > 0)
			{
				var errorMessage = MacroDataSource.GetDataSourceError(action.PQ_FieldNameTrimmed, types, out parentType, out fieldNameWithoutPrefix);
				if (!errorMessage.IsEmpty)
				{
					MacroHelper.NotifyWarning(notifications, errorMessage, validation.DetailWarningMessage);
				}
				else
				{
					if (parentType != null)
					{
						var bizo = ((IRootTypeProvider)action).Roots.FirstOrDefault(x => x.GetType() == parentType);
						finalPropertyInfo = new WorkflowMacroEvaluator(notifications, validation).GetFinalPropertyInfo(parentType, fieldNameWithoutPrefix, bizo, fieldValue);
					}
					else
					{
						var typesNotifications = new ProcessTaskNotificationValidation.ValidationNotifications();
						foreach (var bizoType in types)
						{
							var bizoTypeNotifications = new ProcessTaskNotificationValidation.ValidationNotifications();
							var bizo = ((IRootTypeProvider)action).Roots.FirstOrDefault(x => x.GetType() == bizoType);
							finalPropertyInfo = new WorkflowMacroEvaluator(bizoTypeNotifications, validation).GetFinalPropertyInfo(bizoType, fieldNameWithoutPrefix, bizo, fieldValue);
							if (!bizoTypeNotifications.HasNotifications())
							{
								parentType = bizoType;
								break;
							}
							typesNotifications.AddRange(bizoTypeNotifications);
						}

						if (parentType == null)
						{
							notifications.AddRange(typesNotifications);
						}
					}
				}
			}

			return (finalPropertyInfo, parentType, fieldNameWithoutPrefix);
		}
	}
}
