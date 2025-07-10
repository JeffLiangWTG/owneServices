using System;
using System.ComponentModel;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowMacroSetFieldValidation : WorkflowMacroValidation
	{
		public WorkflowMacroSetFieldValidation(IProcessTaskNotification action)
		{
			DetailWarningMessage = Globals.IsUserInteractive
				? string.Empty
				: Res.GetString("90F839B0-786B-4A84-8AEC-E549CF0C8677", "Object: {0}", action.GetDiagnosticLogInfo());
		}

		public override bool Validate(Type componentType, PropertyInfo propertyInfo, string subPath, string nextPropertyName, INotifications notifications)
		{
			var valid = base.Validate(componentType, propertyInfo, subPath, nextPropertyName, notifications);

			if (!valid)
			{
				return valid;
			}

			if (propertyInfo == null)
			{
				return true;
			}

			if (MacroHelper.IsReferenceType(componentType))
			{
				MacroHelper.NotifyError(
					notifications,
					Res.GetString("28ee479e-ccda-4e60-967a-0a33ea03c428", "Cannot make changes on element of type {0} in 'Set Field' trigger action.", componentType.Name),
					DetailWarningMessage);

				return false;
			}
			else if (string.IsNullOrEmpty(subPath))
			{
				ReadOnlyAttribute readOnlyAttribute;
				ActionFieldAttribute actionAttribute;
				if (!typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType))
				{
					MacroHelper.NotifyError(
						notifications,
						Res.GetString("2971a6e4-6292-4a06-95d8-5f23ee955eea", "Property {0}.{1} of type {2} cannot be used in 'Set Field' trigger action - only simple fields are supported.",
							componentType.FullName,
							propertyInfo.Name,
							propertyInfo.PropertyType.Name),
						DetailWarningMessage);

					return false;
				}
				else if (!propertyInfo.CanWrite
					|| ((actionAttribute = ActionFieldAttribute.Get(propertyInfo)) != null && actionAttribute.ReadOnly)
					|| Attribute.GetCustomAttribute(propertyInfo, typeof(WorkflowSetFieldReadonly)) != null
					|| ((readOnlyAttribute = (ReadOnlyAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ReadOnlyAttribute))) != null && readOnlyAttribute.IsReadOnly && Attribute.GetCustomAttribute(propertyInfo, typeof(WorkflowSetFieldReadonlyCheckBypass)) == null)
					|| (actionAttribute?.FieldType == ActionFieldType.Hidden))
				{
					MacroHelper.NotifyError(
						notifications,
						Res.GetString("812ba2e5-2a7c-482b-9e0c-e751013afc9f", "Property {0}.{1} of type {2} is read-only and cannot be used in 'Set Field' trigger action.",
							componentType.FullName,
							propertyInfo.Name,
							propertyInfo.PropertyType.Name),
						DetailWarningMessage);

					return false;
				}
				else if (MacroHelper.IsSystemField(propertyInfo))
				{
					MacroHelper.NotifyError(
						notifications,
						Res.GetString("5067cac4-849d-4dfe-b6c3-0da4ee553697", "Property {0}.{1} of type {2} holds system data and cannot be used in 'Set Field' trigger action.",
							componentType.FullName,
							propertyInfo.Name,
							propertyInfo.PropertyType.Name),
						DetailWarningMessage);

					return false;
				}
			}
			else
			{
				if (MacroHelper.IsReferenceType(propertyInfo.PropertyType))
				{
					MacroHelper.NotifyError(
						notifications,
						Res.GetString("162aba10-45b3-4b61-95d0-9eb1537bbafe", "Property {0}.{1} of type {2} cannot be used in field path of 'Set Field' trigger action field path - reference data cannot be changed here.",
							componentType.FullName,
							propertyInfo.Name,
							propertyInfo.PropertyType.FullName),
						DetailWarningMessage);

					return false;
				}
				else if (!typeof(IBusiness).IsAssignableFrom(propertyInfo.PropertyType))
				{
					MacroHelper.NotifyError(
						notifications,
						Res.GetString("0a9b89c9-4f19-4ab7-9e3a-692444504cd9", "Property {0}.{1} of type {2} cannot be used in field path of 'Set Field' trigger action field path - only Business object are accepted.",
							componentType.FullName,
							propertyInfo.Name,
							propertyInfo.PropertyType.FullName),
						DetailWarningMessage);

					return false;
				}
			}

			return true;
		}
	}
}
