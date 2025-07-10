using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public static class WorkflowCustomFieldsGridEditableInitializer
	{
		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		public static void AddWorkflowCustomFieldsColumns(ZGrid grid, IBusinessObjectCollection collection, string workflowType, bool isVisible = false, bool isReadonly = false, ZGuid company = default, ZGuid branch = default, ZGuid department = default)
		{
			var newFactory = collection.Factory.CreateNewFactory();
			newFactory.NameForDebugging = nameof(AddWorkflowCustomFieldsColumns);
			var customColumnsProvider = ObjectFactory.New<ICustomColumnsProvider>();
			var columnDefinitions = customColumnsProvider
										.GetCustomColumnDefinitions(newFactory, workflowType, company, branch, department)
										.Cast<GenCustomColumnDefinition>()
										.OrderBy(x => x.XC_Name);

			MarkDuplicateColumnNames(columnDefinitions);
			AddWorkflowCustomFieldsColumns(grid, collection, columnDefinitions, isReadonly, isVisible);
		}

		public static void DisplayActiveCustomFieldsAndHideInactiveColumnFields(ZGrid grid, IBusinessObjectCollection collection, Dictionary<string, ICustomColumnDefinition> previousCustomFields, Dictionary<string, ICustomColumnDefinition> customFields)
		{
			var currentCell = grid.CurrentCell;
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (previousCustomFields != null)
				{
					foreach (var previousCustomField in previousCustomFields)
					{
						var gridColumn = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName.Equals(previousCustomField.Key));

						if (gridColumn != null)
						{
							grid.ColumnStyles.Remove(gridColumn);
						}

						grid.RemoveAndDisposeColumn(previousCustomField.Key);
					}
				}
			}

			AddWorkflowCustomFieldsColumns(grid, collection, customFields.Values, false, true);
			if (grid.Visible && !grid.CurrentCell.Equals(currentCell))
			{
				grid.CurrentCell = currentCell;
			}
		}

		static void AddWorkflowCustomFieldsColumns(ZGrid grid, IBusinessObjectCollection collection, IEnumerable<ICustomColumnDefinition> columnDefinitions, bool isReadonly, bool isVisible)
		{
			var propertyCollection = new UserDefinedPropertyCollectionView();
			var fieldsContainer = new CustomPropertyContainer(new CustomPropertyComparer());
			var columnToPropertyInfo = new Dictionary<string, (string PropertyName, string PropertyType)>();
			foreach (ICustomColumnDefinition columnDefinition in columnDefinitions)
			{
				if (isReadonly)
				{
					try
					{
						var columnDefinitionName = columnDefinition.Name;
						var customAddOnRules = columnDefinition.GetRules();

						if (AddOnColumnDataType.IsMultiPartCode(columnDefinition.Type))
						{
							var identifierPart1 = CustomPropertyHelper.GeneratePropertyIdentifier(columnDefinitionName + AddOnColumnDataType.PartIdentifier + "1", typeof(ZString));
							var identifierPart2 = CustomPropertyHelper.GeneratePropertyIdentifier(columnDefinitionName + AddOnColumnDataType.PartIdentifier + "2", typeof(ZString));

							AddCustomProperty(
								fieldsContainer,
								columnToPropertyInfo,
								customAddOnRules,
								identifierPart1,
								columnDefinitionName,
								columnDefinition.NameLocalized + " " + Res.GetString("396b9369-dd2f-44c7-9d53-02351c5af18a", "Code"),
								AddOnColumnDataType.Codes.String,
								1);

							AddCustomProperty(
								fieldsContainer,
								columnToPropertyInfo,
								customAddOnRules,
								identifierPart2,
								columnDefinitionName,
								columnDefinition.NameLocalized + " " + Res.GetString("f0e64f58-1e2c-473e-b18e-42b6160e8f14", "Description"),
								AddOnColumnDataType.Codes.String,
								2);
						}
						else
						{
							AddCustomProperty(
								fieldsContainer,
								columnToPropertyInfo,
								customAddOnRules,
								CustomPropertyHelper.GeneratePropertyIdentifier(columnDefinitionName, AddOnColumnDataType.GetTypeFromCode(columnDefinition.Type)),
								columnDefinitionName,
								columnDefinition.NameLocalized,
								columnDefinition.Type);
						}
					}
					catch (ArgumentException ex)
					{
						ErrorReporter.ReportOnce("Unable to load Custom Field due to invalid values.", ex);
					}
				}
				else
				{
					propertyCollection.AddProperty(columnDefinition);
				}
			}

			if (isReadonly)
			{
				SetFetchForView(collection, columnToPropertyInfo);
			}

			new WorkflowGridCustomColumnsInitializer(grid, collection, columnToPropertyInfo, null, isVisible, isReadonly).AddCustomColumns(isReadonly ? fieldsContainer.CustomProperties : propertyCollection);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		internal static void MarkDuplicateColumnNames(IEnumerable<GenCustomColumnDefinition> columnDefinitions)
		{
			var alreadySeen = new Dictionary<string, GenCustomColumnDefinition>();
			foreach (var columnDefinition in columnDefinitions)
			{
				if (alreadySeen.TryGetValue(columnDefinition.XC_Name, out var otherColumnDefinition))
				{
					otherColumnDefinition.AppendTypeToCaption = true;
					columnDefinition.AppendTypeToCaption = true;
				}
				else
				{
					alreadySeen.Add(columnDefinition.XC_Name, columnDefinition);
				}
			}
		}

		internal static ZQuery GetCustomValueQuery(BusinessObject bizo, string name, string type)
		{
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, bizo.PK)
				.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, bizo.TablePrefix)
				.AddToFilter(GenCustomAddOnValueSchema.XV_Name, name)
				.AddToFilter(GenCustomAddOnValueSchema.XV_Type, type);
			return query;
		}

		internal static void AddCustomProperty(
			CustomPropertyContainer fieldsContainer,
			Dictionary<string, (string PropertyName, string PropertyType)> columnToPropertyInfo,
			IEnumerable<ICustomAddOnRule> customAddOnRules,
			string identifier,
			string columnName,
			string columnNameLocalized,
			string dataType,
			int? part = null)
		{
			fieldsContainer.AddCustomProperty(
				identifier,
				columnNameLocalized,
				AddOnColumnDataType.GetTypeFromCode(dataType),
				null,
				null,
				customAddOnRules.ToArray());

			if (!columnToPropertyInfo.ContainsKey(identifier))
			{
				if (part != null)
				{
					columnName += AddOnColumnDataType.PartIdentifier + part;
				}

				columnToPropertyInfo.Add(
					identifier,
					(columnName, dataType));
			}
		}

		static void SetFetchForView(IBusinessObjectCollection collection, Dictionary<string, (string PropertyName, string PropertyType)> customFieldNames)
		{
			collection.FetchStrategy.AdditionalFetchForView +=
				(sender, eventArgs) =>
				{
					foreach (var tableColumn in eventArgs.TableColumns)
					{
						if (customFieldNames.TryGetValue(tableColumn.ColumnName, out var customPropertyInfo))
						{
							foreach (var bizo in eventArgs.BusinessObjects)
							{
								collection.Factory.AddFetchHint(typeof(GenCustomAddOnValue), GetCustomValueQuery(bizo, customPropertyInfo.PropertyName, customPropertyInfo.PropertyType));
							}
						}
					}
				};
		}

		#region ZGridCustomisedColumnsInitializer

		sealed class WorkflowGridCustomColumnsInitializer : ZGridCustomColumnsInitializer
		{
			readonly Dictionary<string, (string PropertyName, string PropertyType)> customFieldIdentifierToProperties;

			public WorkflowGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, Dictionary<string, (string PropertyName, string PropertyType)> customFieldIdentifierToProperties, ResourceStringData groupName, bool isVisible, bool isReadonly)
				: base(grid, collection, groupName, isVisible, isReadonly)
			{
				this.customFieldIdentifierToProperties = customFieldIdentifierToProperties;
			}

			protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
			{
				if (IsReadonly)
				{
					return new WorkflowReadonlyCustomPropertyDescriptor(property, customFieldIdentifierToProperties[property.Identifier].PropertyName);
				}
				else
				{
					return new WorkflowCustomPropertyDescriptor(property, true);
				}
			}
		}

		#endregion
	}

	public static class WorkflowCustomFieldsGridReadonlyInitializer
	{
		public static void AddWorkflowCustomFieldsColumns(ZGrid grid, IBusinessObjectCollection collection, string workflowType, bool isVisible = false,
			ZGuid company = default, ZGuid branch = default, ZGuid department = default)
		{
			WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, collection, workflowType, isVisible, isReadonly: true, company, branch, department);
		}
	}
}
