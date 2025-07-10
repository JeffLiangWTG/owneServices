using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Macros;
using Enterprise.DocumentEngineCore;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class MacroHelper
	{
		const uint MacroCollectionUpperLimit = 50000;
		internal static bool HasNotifications(this INotifications notifications)
		{
			var notificationsText = notifications.ToString().Trim();
			return !string.IsNullOrEmpty(notificationsText);
		}

		internal static (string result, IEnumerable<IReportError> errors) ReplaceMacros(IBusiness[] businessObjects, string path)
		{
			var processor = ObjectFactory.Get<ITextMacroProcessor>();
			string result = processor.Replace(path, businessObjects);

			return (result, processor.ReportErrors);
		}

		public static BusinessObjectReader GetCollectionReader(IBusiness bizo, string macroDefinition = "")
		{
			BusinessObjectReader reader = null;

			if (bizo is IBusinessObjectCollection collection)
			{
				var count = collection.Count;
				if (count > MacroCollectionUpperLimit)
				{
					var typeFullName = bizo.GetType().FullName;
					ErrorReporter.ReportOnce($"MacroHelper:{typeFullName}", $"MacroHelper is returning an enormous collection : {typeFullName}[{count}], Macro Definition: {macroDefinition}");
				}
				reader = new ArrayBusinessObjectReader(collection);
			}

			return reader;
		}

		public static bool MatchesFilter(IBusiness[] businessObjects, string filterExpression, INotifications notifications)
		{
			if (string.IsNullOrEmpty(filterExpression))
			{
				return true;
			}

			var result = false;

			try
			{
				string expression = ObjectFactory.Get<ITextMacroProcessor>().Replace(filterExpression, businessObjects);
				result = expression.EvaluateDocEngineExpression(RawDataRegistry.Instance.UseJSEngineForTriggerConditionsEvaluation.Value);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				NotifyWarning(notifications, Res.GetString("ABA22B04-A5EB-49C6-A0FD-1D4D905C22FC", "Cannot evaluate expression: {0}. \r\n{1}", filterExpression, ex.Message), ZString.Empty);
			}

			return result;
		}

		public static bool IsSystemRecord(object component)
		{
			var bizo = component as BusinessObject;
			if (bizo == null || component is NonPersistentBusinessObject)
			{
				return false;
			}

			var schema = EnterpriseSchema.GetTableSchema(bizo.TableName);
			if (schema == null)
			{
				return false;
			}

			var systemColumnName1 = bizo.TablePrefix + CargoWise.Schema.Schema.IsSystemColumnSuffix;
			var systemColumnName2 = bizo.TablePrefix + CargoWise.Schema.Schema.IsSystemDefinedColumnSuffix;
			var systemColumnName3 = bizo.TablePrefix + "_IsSystemAccount";

			return
				(schema.GetSchemaColumn(systemColumnName1) != null && (ZBool)bizo[systemColumnName1]) ||
				(schema.GetSchemaColumn(systemColumnName2) != null && (ZBool)bizo[systemColumnName2]) ||
				(schema.GetSchemaColumn(systemColumnName3) != null && (ZBool)bizo[systemColumnName3]);
		}

		#region Convert Value

		public static object ConvertValue(Type componentType, PropertyInfo propertyInfo, IZType value, INotifications notifications, ZString detailWarningMessage)
		{
			try
			{
				return ConvertValue(propertyInfo.PropertyType, value);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				NotifyWarning(notifications, Res.GetString("8b709593-6cf7-4972-a3a4-8df0f838fd12",
					"Property {0}.{1} of type {2} cannot be set with value '{3}' in 'Set Field' trigger action.",
					componentType.FullName, propertyInfo.Name, propertyInfo.PropertyType.Name, value) + "\r\n" + ex.Message,
					detailWarningMessage);
			}
			return null;
		}

		public static object ConvertValue(Type componentType, ICustomProperty customProperty, string customFieldName, IZType value, INotifications notifications, ZString detailWarningMessage)
		{
			if (customProperty.Info != null && customProperty.Info.Type != null)
			{
				try
				{
					return ConvertValue(customProperty.Info.Type, value);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					NotifyWarning(notifications, Res.GetString("6b067137-99a1-41b5-9849-6fb3ec388022",
						"Custom field '{0}' of type {1} on {2} cannot be set with value '{3}' in 'Set Field' trigger action.",
						customFieldName, customProperty.Info.Type.Name, componentType.FullName, value) + "\r\n" + ex.Message,
						detailWarningMessage);
				}
			}
			return null;
		}

		public static object ConvertValue(Type propertyType, IZType value)
		{
			var converter = TypeDescriptor.GetConverter(propertyType);
			if (converter != null && converter.CanConvertFrom(value.GetType()))
			{
				return converter.ConvertFrom(value);
			}
			return null;
		}

		public static void NotifyWarning(INotifications notifications, ZString message, ZString detailMessage)
		{
			if (notifications != null)
			{
				if ((GlbStaff.CurrentUser.GS_IsSystemAccount || !Globals.IsUserInteractive) && !detailMessage.IsEmpty)
				{
					message += string.Format(CultureInfo.CurrentCulture, "\r\n{0}", detailMessage);
				}
				notifications.AddWarning(message);
			}
		}

		public static void NotifyError(INotifications notifications, ZString message, ZString detailMessage)
		{
			if (notifications != null)
			{
				if (GlbStaff.CurrentUser.GS_IsSystemAccount && !detailMessage.IsEmpty)
				{
					message += string.Format(CultureInfo.CurrentCulture, "\r\n{0}", detailMessage);
				}
				notifications.AddError(message);
			}
		}

		#region Data type check

		internal static bool IsReferenceType(Type componentType)
		{
			return ReferenceTypeNameRegex.IsMatch(componentType.Name);
		}

		static Regex ReferenceTypeNameRegex
		{
			get { return referenceTypeNameRegex ?? (referenceTypeNameRegex = new Regex(@"^Ref[A-Z]", RegexOptions.Compiled)); }
		}

		[ThreadStatic]
		static Regex referenceTypeNameRegex;

		internal static bool IsSystemField(PropertyInfo propertyInfo) => CargoWise.Schema.Schema.IsSystemColumn(propertyInfo.Name);

		#endregion

		#region Get Custom Property

		public static (ZString customFieldName, ZString customFieldType) GetCustomFieldNameAndType(ZString fieldPath)
		{
			if (fieldPath.StartsWith(GetCustomFieldWithTypeMacroName, StringComparison.OrdinalIgnoreCase))
			{
				int customFieldNameLength = fieldPath.Length - GetCustomFieldWithTypeMacroName.Length - 1; // Minus macro name and parenthesis
				return GetCustomFieldParameterParser.ExtractParameters(fieldPath.SubstringSafe(GetCustomFieldWithTypeMacroName.Length, customFieldNameLength).Trim());
			}

			if (fieldPath.StartsWith(GetCustomFieldMacroName, StringComparison.CurrentCultureIgnoreCase))
			{
				int customFieldNameLength = fieldPath.Length - GetCustomFieldMacroName.Length - 1; // Minus macro name and parenthesis
				var fieldName = fieldPath.SubstringSafe(GetCustomFieldMacroName.Length, customFieldNameLength).Trim();
				return (fieldName, ZString.Empty);
			}

			return (ZString.Empty, ZString.Empty);
		}

		public static (ICustomProperty customProperty, CustomBusinessObject customBizo) GetCustomProperty(string customFieldName, string customFieldType, BusinessObject businessObject, IZType value = null)
		{
			ICustomProperty customProperty = null;
			CustomBusinessObject customBizo = null;
			var customBizoStrategy = new GetCustomFieldStrategy(businessObject);

			if (!string.IsNullOrEmpty(customFieldType))
			{
				customProperty = customBizoStrategy.GetCustomProperty(customFieldName, customFieldType, out customBizo);
			}
			else
			{
				if (value != null)
				{
					// If we can coerce the type to anything other than ZString, we just assume that is the correct type.
					// Oh god. Is this what the people that made PHP felt like?
					if (value is ZString s)
					{
						foreach (var code in GetCustomPropertyFallbackOrder())
						{
							if (CanConvertFromType(s, code))
							{
								customProperty = customBizoStrategy.GetCustomProperty(customFieldName, code, out customBizo);
								if (customProperty != null)
								{
									break;
								}
							}
						}
					}
					else
					{
						customProperty = customBizoStrategy.GetCustomProperty(customFieldName, AddOnColumnDataType.GetCodeFromObject(value), out customBizo);
					}
				}

				if (customProperty == null)
				{
					customProperty = customBizoStrategy.GetCustomProperty(customFieldName, null, out customBizo); // Fallback and just find anything with the same name
				}
			}

			return (customProperty, customBizo);
		}

		public static (ICustomProperty customProperty, CustomBusinessObject customBizo) GetCustomProperty(IBusiness bizo, string customFieldName, string customFieldType, IZType value = null)
		{
			customFieldName = PreprocessCustomFieldPart(customFieldName);

			return bizo is BusinessObject businessObject
				? GetCustomProperty(customFieldName, customFieldType, businessObject, value)
				: (null, null);
		}

		static string PreprocessCustomFieldPart(string input)
		{
			var result = input;
			var regex = new Regex(@",\s*(\d+)$");
			if (regex.IsMatch(input))
			{
				result = regex.Replace(input, (match => AddOnColumnDataType.PartIdentifier + match.Groups[1]));
			}
			return result;
		}

		static IEnumerable<string> GetCustomPropertyFallbackOrder()
		{
			yield return AddOnColumnDataType.Codes.Decimal;
			yield return AddOnColumnDataType.Codes.Integer;
			yield return AddOnColumnDataType.Codes.Boolean;
			yield return AddOnColumnDataType.Codes.Datetime;
			yield return AddOnColumnDataType.Codes.String;
		}

		static bool CanConvertFromType(IZType value, string code)
		{
			{
				try
				{
					if (MacroHelper.ConvertValue(AddOnColumnDataType.GetTypeFromCode(code), value) != null)
					{
						return true;
					}
				}
				catch (FormatException)
				{
					// Not this one, I guess.
				}
				catch (OverflowException)
				{
					// Or this one
				}
				catch (ZTypeValueException)
				{
					// Why does this throw so many different exception types?
				}
			}

			return false;
		}

		#endregion

		#endregion

		public const string GetCustomFieldMacroName = "GetCustomField(";
		internal const string GetCustomFieldWithTypeMacroName = "GetCustomFieldWithType(";
	}
}
