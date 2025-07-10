using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.MasterFiles;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowMacroEvaluator : IWorkflowMacroEvaluator
	{
		public WorkflowMacroEvaluator(INotifications notifications = null, WorkflowMacroValidation validation = null, IBusiness[] actionRoots = null)
		{
			this.notifications = notifications ?? new NotificationCollection();
			this.validation = validation ?? new WorkflowMacroValidation();

			this.clauseProcessor = new MacroClauseProcessor(notifications, validation);

			this.actionRoots = actionRoots;
		}

		// Constructor for ObjectFactory using IWorkflowMacroEvaluator
		public WorkflowMacroEvaluator() : this(null)
		{
		}

		#region Value

		public (IZType result, IEnumerable<IReportError> errors) EvaluateMacros(IBusiness[] businessObjects, ZString macrosValuePath)
		{
			var macrosValuePathList = GetMacroList(macrosValuePath);
			var macroErrors = new List<IReportError>();

			try
			{
				if (macrosValuePathList.Any())
				{
					var result = macrosValuePath;

					foreach (var macroValuePath in macrosValuePathList)
					{
						var (value, errors) = EvaluateMacro(businessObjects, macroValuePath);
						macroErrors.AddRange(errors);

						if (value == null)
						{
							return (value, macroErrors);
						}

						result = result.Replace(macroValuePath, value.ToString());
					}

					return (result, macroErrors);
				}
				else
				{
					// For non-string i.e. number we need to return non-string type
					var (value, errors) = EvaluateMacro(businessObjects, macrosValuePath);
					macroErrors.AddRange(errors);
					return (value, errors);
				}
			}
			finally
			{
				if (notifications != null)
				{
					foreach (var error in macroErrors)
					{
						this.notifications.AddWarning(error.Message);
					}
				}
			}
		}

		internal PropertyInfo GetFinalValueInfo(Type componentType, string valuePath)
		{
			if (componentType != null)
			{
				if (typeof(IBusinessObjectCollection).IsAssignableFrom(componentType))
				{
					clauseProcessor.ProcessValue(valuePath, out string nextPath, out string expression);

					if (!notifications.HasNotifications())
					{
						return GetFinalValueInfo(
							BusinessObjectCollection.GetElementTypeFromCollectionType(componentType),
							nextPath);
					}
				}
				else if (valuePath.StartsWith(MacroHelper.GetCustomFieldWithTypeMacroName, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
				else if (valuePath.StartsWith(MacroHelper.GetCustomFieldMacroName, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
				else
				{
					var propertyInfo = GetNextPropertyInfo(componentType, valuePath, out string nextPath);
					if (propertyInfo != null)
					{
						return string.IsNullOrEmpty(nextPath)
							? propertyInfo
							: GetFinalValueInfo(propertyInfo.PropertyType, nextPath);
					}
				}
			}
			return null;
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public FieldValueInfo GetValue(IBusiness businessObject, string valuePath, out bool returnDefaultValue)
		{
			returnDefaultValue = false;

			var collectionReader = MacroHelper.GetCollectionReader(businessObject);

			if (collectionReader != null)
			{
				return GetValueForBusinessObjects(collectionReader, valuePath, out returnDefaultValue);
			}
			else if (businessObject != null)
			{
				return GetValueForBusinessObject(businessObject, valuePath);
			}

			return null;
		}

		#endregion

		#region Property Info

		public static PropertyInfo GetPropertyInfo(Type componentType, string propertyName)
		{
			PropertyInfo result = null;

			try
			{
				result = componentType.GetProperty(propertyName);
				if (result == null
					&& componentType.IsInterface
					&& componentType.GetCustomAttribute<FlattenPropertiesFromInheritanceForMacroEvaluationAttribute>() is FlattenPropertiesFromInheritanceForMacroEvaluationAttribute flattenPropertiesFromInheritanceForMacroEvaluationAttribute
					&& componentType.GetInterface(flattenPropertiesFromInheritanceForMacroEvaluationAttribute.InterfaceName) is Type inheritedInterface)
				{
					result = GetPropertyInfo(inheritedInterface, propertyName);
				}
			}
			catch (AmbiguousMatchException)
			{
				foreach (PropertyInfo info in componentType.GetProperties())
				{
					if (info.Name == propertyName && (result == null || result.DeclaringType.IsAssignableFrom(info.DeclaringType)))
					{
						result = info;
					}
				}
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public PropertyInfo GetNextPropertyInfo(Type componentType, string propertyPath, out string subPath)
		{
			var delimiterIndex = propertyPath.IndexOfAny(new[] { '.', '+' });

			var nextPropertyName = delimiterIndex > 0
				? propertyPath.Substring(0, delimiterIndex)
				: propertyPath;

			subPath = delimiterIndex > 0
				? propertyPath.Substring(delimiterIndex + 1)
				: string.Empty;

			var propertyInfo = GetPropertyInfo(componentType, nextPropertyName);
			NotifyWarningIfMacroIgnore(propertyInfo);
			var valid = validation.Validate(componentType, propertyInfo, subPath, nextPropertyName, notifications);

			return valid
				? propertyInfo
				: null;
		}

		public PropertyInfo GetFinalPropertyInfo(Type componentType, string propertyPath, IBusiness bizo = null, IZType fieldValue = null)
		{
			if ((bizo as BusinessObject)?.IsDeleted ?? false)
			{
				MacroHelper.NotifyError(
							notifications,
							Res.GetString("eeae2966-c22a-4b44-af21-ed667dab64c7", "Property of type {0} cannot be accessed because the row is deleted", bizo.TableName),
							validation.DetailWarningMessage);
				return null;
			}
			propertyPath = Utilities.TrimExpression(propertyPath);

			if (componentType != null)
			{
				if (typeof(IBusinessObjectCollection).IsAssignableFrom(componentType))
				{
					var clause = clauseProcessor.ProcessPropertyAndValue(propertyPath, out string nextPath, out _);

					if (clauseProcessor.ValueClauses.Any(c => c == clause))
					{
						MacroHelper.NotifyWarning(
							notifications,
							Res.GetString("eeae2966-c22a-4b44-af21-ed667dab64c8", "<{0}> function is not supported.", clause.Keyword),
							validation.DetailWarningMessage);
					}

					return GetFinalPropertyInfo(BusinessObjectCollection.GetElementTypeFromCollectionType(componentType), nextPath, null, fieldValue);
				}

				var (customFieldName, customFieldType) = MacroHelper.GetCustomFieldNameAndType(propertyPath);
				var isCustomField = !customFieldName.IsEmpty;
				if (isCustomField)
				{
					return GetCustomPropertyFromBusinessObject(bizo, customFieldName, customFieldType, fieldValue);
				}

				var nextPropertyInfo = GetNextPropertyInfo(componentType, propertyPath, out string subPath);
				if (nextPropertyInfo != null)
				{
					var nextBizo = TryGetIBusinessObjectFromProperty(nextPropertyInfo, bizo, subPath);

					return string.IsNullOrEmpty(subPath)
						? nextPropertyInfo
						: GetFinalPropertyInfo(nextBizo?.GetType() ?? nextPropertyInfo.PropertyType, subPath, nextBizo ?? bizo, fieldValue);
				}
			}

			return null;
		}

		IBusiness TryGetIBusinessObjectFromProperty(PropertyInfo info, IBusiness parent, string valuePath)
		{
			try
			{
				if (parent != null)
				{
					return EvaluatePropertyInfo(info, parent, valuePath) as IBusiness;
				}
			}
			catch (WorkflowMacroEvaluationException) { }

			return null;
		}

		PropertyInfo GetCustomPropertyFromBusinessObject(IBusiness bizo, string customFieldName, string customFieldType, IZType value = null)
		{
			if (bizo != null)
			{
				var (customProperty, customBizo) = MacroHelper.GetCustomProperty(bizo, customFieldName, customFieldType, value);
				if (customBizo == null || customProperty == null || customProperty.IsDeleted)
				{
					MacroHelper.NotifyWarning(
						notifications,
						Res.GetString("559F5863-7F99-4483-B3A7-4E46987D9D07", "Cannot find custom field '{0}{1}' on {2}{3} by Set Field trigger action.", customFieldName, string.IsNullOrEmpty(customFieldType) ? "" : "(" + customFieldType + ")", bizo.GetType(), value == null ? "" : (NoResString)" to set value '" + value + (NoResString)"'"),
						validation.DetailWarningMessage);
				}
			}
			return null;
		}

		#endregion

		#region Implementation

		(IZType result, IEnumerable<IReportError> errors) EvaluateMacro(IBusiness[] businessObjects, string macroValuePath)
		{
			var trimmedMacroValuePath = Utilities.TrimExpression(macroValuePath);

			if (clauseProcessor.IsValueClause(trimmedMacroValuePath))
			{
				return (EvaluateClauseMacro(trimmedMacroValuePath, businessObjects), new List<IReportError>());
			}
			else
			{
				return EvaluateSimpleMacro(macroValuePath, businessObjects);
			}
		}

		IZType EvaluateClauseMacro(string trimmedMacroValuePath, IBusiness[] businessObjects)
		{
			var macroNotification = new MacroNotification();

			IZType result = null;

			foreach (var businessObject in businessObjects)
			{
				var businessObjectNotifications = new ProcessTaskNotificationValidation.ValidationNotifications();

				result = new WorkflowMacroEvaluator(businessObjectNotifications, validation, this.actionRoots)
					.GetValue(businessObject, trimmedMacroValuePath, out bool returnDefaultValue)?.FieldValue;

				if (!returnDefaultValue && result != null)
				{
					return result;
				}

				if (businessObjectNotifications.HasNotifications())
				{
					macroNotification.Add(businessObjectNotifications);
				}
				else
				{
					// if we don't have notification, then it is valid result, just return
					return result;
				}
			}

			var notificationInString = macroNotification.GetNotifications();
			if (!string.IsNullOrEmpty(notificationInString))
			{
				MacroHelper.NotifyWarning(
					this.notifications,
					notificationInString,
					validation.DetailWarningMessage);
			}

			return result;
		}

		(IZType result, IEnumerable<IReportError> errors) EvaluateSimpleMacro(string macroValuePath, IBusiness[] businessObjects)
		{
			try
			{
				(ZString result, var errors) = MacroHelper.ReplaceMacros(businessObjects, macroValuePath);
				return (result, errors);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new WorkflowMacroEvaluationException(ResString.GetMultilingualString("9D6D0881-FB99-4AB8-841E-6ED035242287", "Unable to evaluate expression [{0}]", macroValuePath), ex);
			}
		}

		FieldValueInfo GetValueForBusinessObjects(BusinessObjectReader collectionReader, string valuePath, out bool returnDefaultValue)
		{
			returnDefaultValue = false;

			var currentNotifications = new ProcessTaskNotificationValidation.ValidationNotifications();

			var clause = new MacroClauseProcessor(currentNotifications, validation).ProcessValue(valuePath, out string nextPath, out string expression);

			if (string.IsNullOrEmpty(nextPath))
			{
				MacroHelper.NotifyWarning(
					currentNotifications,
					Res.GetString("094E3A65-51F4-4CEE-B0A1-7D7BD0A4018A", "Invalid macro: {0}", valuePath),
					validation.DetailWarningMessage);
			}

			if (!currentNotifications.HasNotifications())
			{
				var elementMatched = false;

				foreach (var element in collectionReader)
				{
					var businessObjects = actionRoots != null
						? new[] { element }.Concat(actionRoots).ToArray()
						: new[] { element };

					if (MacroHelper.MatchesFilter(businessObjects, expression, notifications))
					{
						elementMatched = true;
						var result = GetValue(element, nextPath, out returnDefaultValue);

						if (result != null)
						{
							return result;
						}
						else if (notifications.HasNotifications())
						{
							break;
						}
					}
				}

				if (!notifications.HasNotifications() && clause != null && !elementMatched && clause.ShouldReturnDefaultValueIfNull())
				{
					return GetDefaultValue(collectionReader.BusinessObjectType, valuePath, out returnDefaultValue);
				}
			}
			else if (notifications != null)
			{
				notifications.AddError(currentNotifications.ToString().Trim());
			}

			return null;
		}

		public static object EvaluatePropertyInfo(PropertyInfo info, IBusiness bizo, string valuePath)
		{
			try
			{
				return info.GetValue(bizo, null);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new WorkflowMacroEvaluationException(ResString.GetMultilingualString("fd3098ac-6592-43dd-8910-20a29976b3df", "Unable to evaluate expression [{0}]", valuePath), ex);
			}
		}

		FieldValueInfo GetValueForBusinessObject(IBusiness businessObject, string valuePath)
		{
			var propertyInfo = GetNextPropertyInfo(
					businessObject.GetType(),
					valuePath,
					out string nonCollectionSubPath);

			if (propertyInfo != null)
			{
				if (string.IsNullOrEmpty(nonCollectionSubPath))
				{
					return new FieldValueInfo(businessObject, propertyInfo.Name, EvaluatePropertyInfo(propertyInfo, businessObject, valuePath) as IZType);
				}
				else
				{
					var propertyInfoValue = EvaluatePropertyInfo(propertyInfo, businessObject, valuePath) as IBusiness;

					return propertyInfoValue != null
						? GetValue(propertyInfoValue, nonCollectionSubPath, out bool returnDefaultValue)
						: null;
				}
			}

			return GetValueForCustomField(businessObject, valuePath);
		}

		void NotifyWarningIfMacroIgnore(PropertyInfo propertyInfo)
		{
			var obsoleteAttribute = propertyInfo?.GetCustomAttribute<MacroIgnoreAttribute>();
			if (obsoleteAttribute != null)
			{
				MacroHelper.NotifyWarning(
				notifications,
				Res.GetString("3c181385-2f6f-4fd3-a930-e21ee904b81d", "Macro Ignore Property: '{0}' on '{1}'.", propertyInfo.Name, propertyInfo.DeclaringType?.FullName),
				ZString.Empty);
			}
		}

		static FieldValueInfo GetValueForCustomField(IBusiness businessObject, string valuePath)
		{
			var (customFieldName, customFieldType) = MacroHelper.GetCustomFieldNameAndType(valuePath);
			if (!customFieldName.IsEmpty)
			{
				CustomBusinessObject customBizo = null;
				ICustomProperty customProperty = null;

				if (businessObject is BusinessObject bizo)
				{
					(customProperty, customBizo) = MacroHelper.GetCustomProperty(customFieldName, customFieldType, bizo);
				}

				if (customBizo != null && customProperty != null)
				{
					return new FieldValueInfo(customBizo, customProperty.Identifier, (IZType)customProperty.GetValue(customBizo), true);
				}
			}

			return null;
		}

		FieldValueInfo GetDefaultValue(Type componentType, string valuePath, out bool returnDefaultValue)
		{
			returnDefaultValue = false;

			clauseProcessor.ProcessValue(valuePath, out string nextPath, out string expression);

			var nextProperty = GetNextPropertyInfo(componentType, nextPath, out string nextNextValuePath);

			if (nextProperty != null)
			{
				if (string.IsNullOrEmpty(nextNextValuePath))
				{
					var attributes = nextProperty.GetCustomAttributes(typeof(DefaultValueAttribute), true);
					if (attributes.Length > 0)
					{
						var defaultAttr = (DefaultValueAttribute)attributes[0];
						return new FieldValueInfo(null, nextProperty.Name, defaultAttr.Value as IZType);
					}

					if (nextProperty.PropertyType.IsValueType)
					{
						returnDefaultValue = true;
						var result = Activator.CreateInstance(nextProperty.PropertyType);
						return new FieldValueInfo(null, nextProperty.Name, result as IZType);
					}
				}
				else
				{
					Type nextPropertyType = null;

					if (typeof(IBusinessObjectCollection).IsAssignableFrom(nextProperty.PropertyType))
					{
						nextPropertyType = BusinessObjectCollection.GetElementTypeFromCollectionType(nextProperty.PropertyType);
					}
					else
					{
						nextPropertyType = nextProperty.PropertyType;
					}

					return GetDefaultValue(
						nextPropertyType,
						nextNextValuePath,
						out returnDefaultValue);
				}
			}

			return new FieldValueInfo(null, ZString.Empty, null);
		}

		static IEnumerable<string> GetMacroList(string macrosAsString)
		{
			return ObjectFactory.Get<IRegexProviderWrapper>().GetOuterMostMacros(macrosAsString);
		}

		readonly INotifications notifications;
		readonly WorkflowMacroValidation validation;

		readonly MacroClauseProcessor clauseProcessor;

		readonly IBusiness[] actionRoots;

		#endregion
	}

	public record FieldValueInfo(IBusiness BusinessObject, ZString FieldName, IZType FieldValue, bool IsCustomField = false)
	{
		public IBusiness BusinessObject { get; } = BusinessObject;
		public ZString FieldName { get; } = FieldName;
		public IZType FieldValue { get; } = FieldValue;
		public bool IsCustomField { get; } = IsCustomField;
	}
}
