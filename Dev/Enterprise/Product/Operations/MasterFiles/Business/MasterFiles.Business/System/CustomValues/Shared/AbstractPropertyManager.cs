using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	// Abstract properties are designed to be used with ***THE*** Abstract subsystem 
	// (this ain't your +dad's+ "Abstract subsystem")
	// They are automatically imported and exported from Enterprise based on two attributes
	// 1 CustomValuesAttribute - must be set on each business object
	// 2 XSD of the object must be updated
	// Once set import and export will be automatic
	abstract class AbstractPropertyManager<TAddOnColumn>
		where TAddOnColumn : BusinessObject, IAddOnColumn
	{
		internal AbstractPropertyManager(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");
			this.businessObject = businessObject;
		}
		protected BusinessObject businessObject;

		public T GetValue<T>(string propertyName, bool reLoadData = false)
		{
			return (T)GetValue(propertyName, typeof(T), reLoadData);
		}

		public IZType GetValue(string propertyName, Type expectedZType, bool reLoadData = false)
		{
			var addOnColumn = GetProperty(propertyName, AddOnColumnDataType.GetCodeFromType(expectedZType), reLoadData);
			try
			{
				if (addOnColumn != null)
				{
					return FromXmlString(addOnColumn[Data].ToString(), expectedZType);
				}
				else
				{
					return (IZType)Activator.CreateInstance(expectedZType);
				}
			}
			catch (FormatException ex)
			{
				addOnColumn.AddRowWarning(Res.GetString("714772E3-F64F-4D7B-9546-8E4E422E8A09", "Error Converting custom property value of ") + propertyName + ":" +
							" " + ex.Message + " " +
							Res.GetString("051ECD68-249E-42AF-9695-F81482F0C621", "If you save, original value will be overridden."));

				var emptyValue = typeof(IZType).IsAssignableFrom(expectedZType) ? ZDataType.ZTypeToEmptyValue(expectedZType) : null;
				return emptyValue;
			}
		}

		public IEnumerable<IPropertyValue> GetValues()
		{
			foreach (var addOnColumn in GetProperties())
			{
#if DEBUG
				if (string.IsNullOrEmpty(addOnColumn[DataType].ToString()))
				{
					Globals.Message.ShowDeveloperException(new ArgumentException("Empty GenOnAddColumn DataType. If you are creating a GenAddOnCoumn record, ensure XA_Type has been set correctly."));
				}
#endif

				var value = FromXmlString(addOnColumn[Data].ToString(), AddOnColumnDataType.GetTypeFromCode(addOnColumn[DataType].ToString()));
				yield return new PropertyValue(addOnColumn[Name].ToString(), value);
			}
		}

		public TAddOnColumn GetProperty(string propertyName, string propertyType, bool reLoadData = false)
		{
			var query = new ZQuery(ParentID, businessObject.PK);
			query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
			query.AddToFilter(Name, propertyName);

			if (WorkflowDataRegistry.Instance.FilterCustomFieldsByParentTable.Value)
			{
				query.AddToFilter(ParentTableCode, businessObject.TablePrefix);
			}
			if (propertyType != null)
			{
				query.AddToFilter(new ZQuery(DataType, propertyType).AddToFilter(JoinCondition.Or, DataType, ZString.Empty));
			}

			var addOnColumn = businessObject.Factory.LoadTop1<TAddOnColumn>(query);
			if (addOnColumn != null)
			{
				if (reLoadData)
				{
					addOnColumn.ReloadSafe();
				}
				addOnColumn.Parent = businessObject;
				businessObject.RegisterEditableChildObject(addOnColumn);
			}

			return addOnColumn;
		}

		public IEnumerable<TAddOnColumn> GetProperties()
		{
			var query = new ZQuery(ParentID, businessObject.PK);

			if (Name != null && NameIsMandatory)
			{
				query.AddToFilter(Name, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			if (DataType != null && DataTypeIsMandatory)
			{
				query.AddToFilter(DataType, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			if (WorkflowDataRegistry.Instance.FilterCustomFieldsByParentTable.Value)
			{
				query.AddToFilter(ParentTableCode, businessObject.TablePrefix);
			}

			query.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;
			query.OrderBy = Name.Name;

			return businessObject.Factory.Load<TAddOnColumn>(query);
		}

		public TAddOnColumn SetValue(string propertyName, string propertyType, IZType value, IXmlImportLogger logger)
		{
			if (logger == null)
			{
				return SetValueCore(propertyName, propertyType, value);
			}
			else
			{
				Action<string> handleWarningAction = warning => { logger.Log(LogType.Warning, warning); };
				return SetValueCore(propertyName, propertyType, value, handleWarningAction);
			}
		}

		public TAddOnColumn SetValue(string propertyName, string propertyType, IZType value, INotifications notifications = null)
		{
			if (notifications == null)
			{
				return SetValueCore(propertyName, propertyType, value);
			}
			else
			{
				Action<string> handleWarningAction = notifications.AddWarning;
				return SetValueCore(propertyName, propertyType, value, handleWarningAction);
			}
		}

		public TAddOnColumn SetValueCore(string propertyName, string propertyType, IZType value, Action<string> handleWarningAction = null)
		{
#if DEBUG
			if (businessObject.GetType().GetCustomAttributes(AttributeType, true).Length == 0)
			{
				throw new InvalidOperationException(businessObject.GetType().FullName + " does not apply " + AttributeType.Name + ".\r\nThis is required to allow proper cleanup and fetch hints when the object is loaded or deleted");
			}
#endif
			var isSettingHasChangesSuspended = businessObject.IsSettingHasChangesSuspended;
			var addOnColumn = GetProperty(propertyName, propertyType);
			if (addOnColumn == null && !value.IsDefault)
			{
				addOnColumn = businessObject.Factory.New<TAddOnColumn>();
				using (isSettingHasChangesSuspended ? addOnColumn.SuspendSettingHasChanges() : null)
				{
					addOnColumn.Parent = businessObject;
					addOnColumn[Name] = propertyName;
				}
			}
			if (addOnColumn != null)
			{
				using (isSettingHasChangesSuspended ? addOnColumn.SuspendSettingHasChanges() : null)
				{
					businessObject.RegisterEditableChildObject(addOnColumn);
					if (!value.IsDefault)
					{
						addOnColumn[DataType] = propertyType ?? AddOnColumnDataType.GetCodeFromObject(value);
						var dataValue = ToXmlString(value);
						if (dataValue.Length > Data.MaxLength)
						{
							var warning = Res.GetString("553faab4-1bb3-4033-b208-226bd993cc34",
								 "Attempted to insert {0} characters into Field [{1}] which has a maximum length of {2} characters. Field was truncated.",
								dataValue.Length, propertyName, Data.MaxLength);

							if (handleWarningAction != null)
							{
								handleWarningAction.Invoke(warning);
							}
							else
							{
								handleWarningAction = InitialiseEmptyStringAction;
								handleWarningAction.Invoke(warning);
							}

							addOnColumn[Data] = dataValue.Substring(0, Data.MaxLength);
						}
						else
						{
							addOnColumn[Data] = dataValue;
						}
					}
					else
					{
						addOnColumn.Delete();
					}

					ValueChanged?.Invoke();
				}
			}

			return addOnColumn;
		}

		public Action ValueChanged { get; set; }

		void InitialiseEmptyStringAction(string a)
		{
		}

		string ToXmlString(IZType value)
		{
			if (value is ZDate date)
			{
				if (value.IsValid)
				{
					return date.ToISO8601ShortDateString();
				}
				else
				{
					return Invalid;
				}
			}
			else if (value is ZDateTime dateTime)
			{
				if (value.IsValid)
				{
					return dateTime.SqlFormat;
				}
				else
				{
					return Invalid;
				}
			}
			else if (value is ZDateTimeOffset dateTimeOffset)
			{
				if (value.IsValid)
				{
					return dateTimeOffset.SqlFormat;
				}
				else
				{
					return Invalid;
				}
			}
			else
			{
				return value.ToString();
			}
		}

		IZType FromXmlString(string value, Type expectedType)
		{
			if (value.Equals(Invalid, StringComparison.OrdinalIgnoreCase))
			{
				if (typeof(ZDate).IsAssignableFrom(expectedType))
				{
					return ZDate.Invalid;
				}
				else if (typeof(ZDateTime).IsAssignableFrom(expectedType))
				{
					return ZDateTime.Invalid;
				}
				else if (typeof(ZDateTimeOffset).IsAssignableFrom(expectedType))
				{
					return ZDateTimeOffset.Invalid;
				}
				else if (typeof(ZTime).IsAssignableFrom(expectedType))
				{
					return ZTime.Invalid;
				}
				else if (typeof(ZGeography).IsAssignableFrom(expectedType))
				{
					return ZGeography.Invalid;
				}
			}

			using (Culture.SetTemporarily(Culture.Invariant))
			{
				try
				{
					return (IZType)Activator.CreateInstance(expectedType, value);
				}
				catch (TargetInvocationException ex)
				{
					throw new FormatException(Res.GetString("250c8ce7-96f9-4766-abab-5c6c8cb4c3f1", "Error converting value '{0}' to type {1}.", value, expectedType.Name), ex);
				}
			}
		}

		protected abstract SchemaColumn ParentID { get; }
		protected abstract SchemaColumn ParentTableCode { get; }
		protected abstract SchemaColumn Name { get; }
		protected abstract SchemaColumn Data { get; }
		protected abstract SchemaColumn DataType { get; }
		protected abstract Type AttributeType { get; }

		protected virtual bool NameIsMandatory { get { return false; } }
		protected virtual bool DataTypeIsMandatory { get { return false; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal constant")]
		const string Invalid = "Invalid";
	}
}
