using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public interface IUserDefinedPropertyCollection : ICustomPropertyCollection
	{
		void AddProperty(ICustomColumnDefinition column, params DynamicMetaData[] additionalMetaData);
		void AddProperty(ICustomColumnDefinition column, int sequence, params DynamicMetaData[] additionalMetaData);
	}

	public class UserDefinedPropertyCollection : IUserDefinedPropertyCollection
	{
		readonly BusinessObject parent;
		readonly Dictionary<string, ICustomProperty> properties = new Dictionary<string, ICustomProperty>();
		readonly SortedSet<ICustomProperty> sortedProperties = new SortedSet<ICustomProperty>(new CustomPropertyComparer());

		UserDefinedPropertyManager propertyManager;

		public UserDefinedPropertyCollection(BusinessObject bizObj)
		{
			parent = bizObj ?? throw new ArgumentNullException(nameof(bizObj));
		}

		public UserDefinedPropertyCollection(BusinessObject bizo, IEnumerable<ICustomProperty> initialProperties)
			: this(bizo)
		{
			AddCustomProperties(initialProperties);
		}

		public UserDefinedPropertyCollection(BusinessObject bizo, Action valueChanged)
			: this(bizo)
		{
			PropertyManager.ValueChanged = valueChanged;
		}

		public void Add(string propertyName, string caption, int? position = null)
		{
			Add(ZGuid.NewZGuid(), propertyName, caption, position);
		}

		public void Add(ZGuid identifier, string propertyName, string caption, int? position = null)
		{
			ZPropertyInfo info = parent.FindPropertyInfo(propertyName);
			Add(identifier, propertyName, caption, info.PropertyType, info.SupportsMaxLength ? info.MaxLength : int.MaxValue, position);
		}

		public void Add(ZGuid identifier, string propertyName, string caption, Type propertyType, int maxLength, int? position = null)
		{
			var columnDefinition = CreateCustomColumnDefinition(identifier, propertyName, caption, propertyType, maxLength, position);
			var customProperty = columnDefinition.CreateWrapperCustomProperty(parent);
			Add(customProperty);
		}

		public void Add(ICustomProperty customProperty)
		{
			if (!properties.ContainsKey(customProperty.Identifier))
			{
				properties.Add(customProperty.Identifier, customProperty);
				sortedProperties.Add(customProperty);
			}
		}

		public UserDefinedPropertyCollection WithWorkflowTemplateCustomFields<T>(T bizo)
			where T : BusinessObject, IWorkflowProvider
		{
			var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), bizo.Factory);
			Add(loader.FindMatches(bizo));
			return this;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502")]
		public void Add(IProcessTaskTemplateMatches templates)
		{
			if (UserDefinedValuesAttribute.IsEnabled(parent))
			{
				var templateMatches = templates.Matches
					.TakeUntil(t => StringComparer.OrdinalIgnoreCase.Equals(t.P0_CustomFieldFallback, FallbackTypeList.Codes.NeverFallback)).ToArray();

#if NET
				var allColumnDefinitions = System.Linq.Enumerable.DistinctBy(templateMatches.SelectMany(t => t.CustomColumnDefinitions), d => (d.Name, d.Type)).ToArray();
#else
				var allColumnDefinitions = templateMatches.SelectMany(t => t.CustomColumnDefinitions).DistinctBy(d => (d.Name, d.Type)).ToArray();
#endif
				var positions = new CustomPositionMap(allColumnDefinitions);

				//first pass is to add fetch hints, second pass to do load
				var definitions = FilterDefinitions(allColumnDefinitions, positions);

				// TODO: Put in Uber rather than fetch hints
				foreach (var (definition, _, factory) in definitions)
				{
					factory.AddFetchHint(typeof(GenCustomAddOnRule), (ZGuid)definition.RuleDefinitionReference);
				}

				foreach (var template in templateMatches)
				{
					var columnDefinitions = template.CustomColumnDefinitions;
					foreach (ICustomColumnDefinition columnDefinition in columnDefinitions)
					{
						if (columnDefinition is BusinessObject defBizO)
						{
							defBizO.Factory.AddFetchHint(typeof(GenCustomAddOnRule), (ZGuid)columnDefinition.RuleDefinitionReference);
						}
					}
				}

				foreach (var (definition, position, _) in definitions)
				{
					var customProperties = definition.CreatePropertyManagedCustomProperties(PropertyManager, parent);
					foreach (var customProperty in customProperties)
					{
						Add(customProperty);
					}
				}

				foreach (var template in templateMatches)
				{
					var columnDefinitions = template.CustomColumnDefinitions;
					var templateBizo = template as BusinessObject;
					ZGuid templatePK = templateBizo != null ? templateBizo.PK : ZGuid.Empty;

					foreach (ICustomColumnDefinition columnDefinition in columnDefinitions)
					{
						var customProperties = columnDefinition.CreatePropertyManagedCustomProperties(PropertyManager, parent);
						foreach (var customProperty in customProperties)
						{
							Add(customProperty);
						}
					}
				}
			}
		}

		public void AddProperty(ICustomColumnDefinition column, params DynamicMetaData[] additionalMetaData)
		{
			ICustomProperty customProperty;
			customProperty = column.CreatePropertyManagedCustomProperty(PropertyManager, parent, additionalMetaData);
			Add(customProperty);
		}

		public void AddProperty(ICustomColumnDefinition column, int sequence, params DynamicMetaData[] additionalMetaData)
		{
			ICustomProperty customProperty;
			customProperty = new CustomColumnDefinitionSequenceOverride(column, sequence).CreatePropertyManagedCustomProperty(PropertyManager, parent, additionalMetaData);
			Add(customProperty);
		}

		public void LoadAllPresavedPropertiesOnParent()
		{
			if (UserDefinedValuesAttribute.IsEnabled(parent))
			{
				foreach (var addOnValue in PropertyManager.GetProperties())
				{
					var customProperties = addOnValue.GetCustomColumnDefinition().CreatePropertyManagedCustomProperties(PropertyManager, parent);
					foreach (var customProperty in customProperties)
					{
						Add(customProperty);
					}
				}
			}
		}

		ICollection<(ICustomColumnDefinition definition, int? position, BusinessObjectFactory factory)> FilterDefinitions(ICollection<ICustomColumnDefinition> allColumnDefinitions, CustomPositionMap positions)
		{
			var groupedDefinitions = allColumnDefinitions.ToLookup(d => d.Name);

			var results = new List<(ICustomColumnDefinition, int?, BusinessObjectFactory)>();
			foreach (var addOnValue in PropertyManager.GetProperties())
			{
				var property = addOnValue.GetCustomColumnDefinition();
				if (property.Name.IndexOf(AddOnColumnDataType.PartIdentifier + "1", StringComparison.Ordinal) != -1
					|| property.Name.IndexOf(AddOnColumnDataType.PartIdentifier + "2", StringComparison.Ordinal) != -1)
				{
					continue;
				}

				ICustomColumnDefinition definition;
				var definitions = groupedDefinitions[property.Name];
				if (definitions.Count() <= 1)
				{
					definition = definitions.FirstOrDefault();
				}
				else
				{
					definition = definitions.FirstOrDefault(x => string.Equals(x.Type, property.Type));
				}

				if (definition is BusinessObject defBizO)
				{
					results.Add((definition, positions[definition], defBizO.Factory));
				}
				else if (!WorkflowDataRegistry.Instance.ShowCustomFieldsFromCurrentTemplateOnly.Value)
				{
					results.Add((property, positions[definition], addOnValue.Factory));
				}
			}

			return results;
		}

		static ICustomColumnDefinition CreateCustomColumnDefinition(ZGuid identifier, string propertyName, string caption, Type propertyType, int maxLength, int? position = null)
		{
			return new CustomColumnDefinition(identifier, propertyName, caption, AddOnColumnDataType.GetCodeFromType(propertyType), maxLength, position);
		}

		public string[] GetIdentifiers()
		{
			return sortedProperties.Select(p => p.Identifier).ToArray();
		}

		public IEnumerable<ICustomProperty> GetProperties()
		{
			return sortedProperties;
		}

		public IEnumerator<ICustomProperty> GetEnumerator()
		{
			return sortedProperties.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return sortedProperties.GetEnumerator();
		}

		public ICustomProperty GetCustomProperty(string identifier)
		{
			if (properties.TryGetValue(identifier, out var property))
			{
				return property;
			}
			else
			{
				return null;
			}
		}

		public void AddCustomProperties(IEnumerable<ICustomProperty> newProperties)
		{
			foreach (var customProperty in newProperties)
			{
				if (!properties.ContainsKey(customProperty.Identifier))
				{
					properties.Add(customProperty.Identifier, customProperty);
					sortedProperties.Add(customProperty);
				}
			}
		}

		UserDefinedPropertyManager PropertyManager
		{
			get { return propertyManager ?? (propertyManager = new UserDefinedPropertyManager(parent)); }
		}

		sealed class CustomPositionMap
		{
			readonly Dictionary<Tuple<string, string>, int?> dictionary;

			public CustomPositionMap(ICustomColumnDefinition[] definitions)
			{
				dictionary = definitions.ToDictionary(CreateKey, d => d.Sequence);
			}

			Tuple<string, string> CreateKey(ICustomColumnDefinition condition) => Tuple.Create(condition.Name, condition.Type);

			public int? this[ICustomColumnDefinition definition]
			{
				get
				{
					if (definition != null)
					{
						return dictionary.TryGetValue(CreateKey(definition), out var result) ? result : null;
					}
					else
					{
						return null;
					}
				}
			}
		}
	}

	sealed class CustomColumnDefinition : ICustomColumnDefinition
	{
		public CustomColumnDefinition(
			ZGuid identifier,
			string name,
			string nameLocalized,
			string type,
			int maxLength,
			int? sequence = null,
			ZGuid? ruleDefinitionReference = null,
			BusinessObjectFactory ruleFactory = null)
		{
			this.Identifier = identifier;
			this.Name = name;
			this.NameLocalized = nameLocalized;
			this.Type = type;
			this.MaxLength = maxLength;
			this.Sequence = sequence;
			this.RuleDefinitionReference = ruleDefinitionReference;
			this.ruleFactory = ruleFactory;
		}

		public ZGuid Identifier { get; }

		public string Type { get; }

		public string Name { get; }

		public string NameLocalized { get; }

		public int MaxLength { get; }

		public int? Sequence { get; }

		public bool IsDeleted => false;

		public ZGuid? RuleDefinitionReference { get; }

		public bool IsRuleActive => GetAddOnRule()?.XR_IsActive ?? true;

		public ICustomAddOnRule[] GetRules()
		{
			var rule = GetAddOnRule();
			return rule?.GetRules() ?? Array.Empty<ICustomAddOnRule>();
		}

		GenCustomAddOnRule GetAddOnRule()
		{
			if (ruleFactory != null && RuleDefinitionReference.HasValue)
			{
				return ruleFactory.Load<GenCustomAddOnRule>(RuleDefinitionReference.Value);
			}
			return null;
		}

		readonly BusinessObjectFactory ruleFactory;
	}

	public class UserDefinedPropertyCollectionView : IUserDefinedPropertyCollection
	{
		readonly Dictionary<string, ICustomProperty> properties = new Dictionary<string, ICustomProperty>();
		public void AddProperty(ICustomColumnDefinition column, params DynamicMetaData[] additionalMetaData)
		{
			foreach (var property in column.CreateViewOnlyManagedCustomProperties(additionalMetaData))
			{
				if (!properties.ContainsKey(property.Identifier))
				{
					properties.Add(property.Identifier, property);
				}
			}
		}

		public void AddProperty(ICustomColumnDefinition column, int sequence, params DynamicMetaData[] additionalMetaData)
		{
			var sequenceOverride = new CustomColumnDefinitionSequenceOverride(column, sequence);
			foreach (var property in sequenceOverride.CreateViewOnlyManagedCustomProperties(additionalMetaData))
			{
				if (!properties.ContainsKey(property.Identifier))
				{
					properties.Add(property.Identifier, property);
				}
			}
		}

		public ICustomProperty GetCustomProperty(string identifier)
		{
			if (properties.TryGetValue(identifier, out var property))
			{
				return property;
			}
			else
			{
				return null;
			}
		}

		public string[] GetIdentifiers()
		{
			return properties.Keys.ToArray();
		}

		public IEnumerable<ICustomProperty> GetProperties()
		{
			return properties.Values;
		}

		public IEnumerator<ICustomProperty> GetEnumerator()
		{
			return properties.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return properties.Values.GetEnumerator();
		}

		public void AddCustomProperties(IEnumerable<ICustomProperty> newProperties)
		{
			foreach (var customProperty in newProperties)
			{
				if (!properties.ContainsKey(customProperty.Identifier))
				{
					properties.Add(customProperty.Identifier, customProperty);
				}
			}
		}
	}

	sealed class CustomColumnDefinitionSequenceOverride : ICustomColumnDefinition
	{
		readonly ICustomColumnDefinition columnDefinition;
		readonly int sequence;

		public CustomColumnDefinitionSequenceOverride(ICustomColumnDefinition customColumnDefinition, int sequence)
		{
			this.columnDefinition = customColumnDefinition;
			this.sequence = sequence;
		}

		public ZGuid Identifier => columnDefinition.Identifier;

		public string Type => columnDefinition.Type;

		public string Name => columnDefinition.Name;

		public string NameLocalized => columnDefinition.NameLocalized;

		public int MaxLength => columnDefinition.MaxLength;

		public int? Sequence => this.sequence;

		public bool IsDeleted => columnDefinition.IsDeleted;

		public ZGuid? RuleDefinitionReference => columnDefinition.RuleDefinitionReference;

		public bool IsRuleActive => columnDefinition.IsRuleActive;

		public ICustomAddOnRule[] GetRules()
		{
			return columnDefinition.GetRules();
		}
	}

	static class ICustomColumnDefinitionExtensions
	{
		internal static ICustomProperty CreateWrapperCustomProperty(
			this ICustomColumnDefinition columnDefinition,
			BusinessObject parent,
			params DynamicMetaData[] additionalMetaData)
		{
			void AddValidation(ICustomColumnDefinition customColumnDefinition, Type columnType, List<Action<ZPropertyInfo>> validatorList)
			{
				if (typeof(ZString).IsAssignableFrom(columnType))
				{
					bool englishOnly = true;
					string finalPropertyName = columnDefinition.Name.Split('+').Last();
					var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(Schema.GetPrefixFromColumnName(finalPropertyName));
					if (tableSchema != null)
					{
						var schemaColumn = tableSchema.GetSchemaColumn(finalPropertyName);
						if (schemaColumn != null)
						{
							englishOnly = !schemaColumn.IsUnicode;
						}
					}

					if (englishOnly)
					{
						validatorList.Add(EnglishCharactersValidation.ErrorIfNotWesternEuropean);
					}
				}
				else if (typeof(ZDateTime).IsAssignableFrom(columnType))
				{
					validatorList.Add(TypeValidation.CheckValidZDateTimeWithoutRange);
				}
			}

			var (propertyIdentifier, addOnRulesData) = BuildCommon(columnDefinition, AddValidation, additionalMetaData);
			return new WrappedUserDefinedCustomProperty(
				columnDefinition,
				parent,
				propertyIdentifier,
				(info) =>
				{
					foreach (var validator in addOnRulesData.Validators)
					{
						validator(info);
					}
				},
				OnSetAction(addOnRulesData.OnSetActions),
				addOnRulesData.MetaData);
		}

		internal static ICustomProperty CreatePropertyManagedCustomProperty(
			this ICustomColumnDefinition columnDefinition,
			UserDefinedPropertyManager propertyManager,
			BusinessObject parent,
			params DynamicMetaData[] additionalMetaData)
		{
			void AddValidation(ICustomColumnDefinition customColumnDefinition, Type columnType, List<Action<ZPropertyInfo>> validatorList)
			{
				if (typeof(ZDateTime).IsAssignableFrom(columnType))
				{
					validatorList.Add(TypeValidation.CheckValidZDateTimeWithoutRange);
				}
			}

			var identifier = GetInternedPropertyIdentifier(columnDefinition);

			FlyweightPropertyManagedUserDefinedCustomProperty GetFlyWeight()
			{
				var addOnRulesData = BuildCommonCore(columnDefinition, AddValidation, additionalMetaData);
				return new FlyweightPropertyManagedUserDefinedCustomProperty(columnDefinition,
					identifier,
					addOnRulesData.Validators,
					OnSetAction(addOnRulesData.OnSetActions),
					addOnRulesData.MetaData,
					addOnRulesData.IsUpperCase);
			}

			var flyweight = additionalMetaData.Length == 0 ?
				parent.Factory.GetCachedValue((columnDefinition.Identifier, "FlyweightCustomPropertyDefinition"), GetFlyWeight) :
				GetFlyWeight();

			return new PropertyManagedUserDefinedCustomProperty(parent, flyweight, propertyManager);
		}

		internal static IEnumerable<ICustomProperty> CreatePropertyManagedCustomProperties(
			this ICustomColumnDefinition columnDefinition,
			UserDefinedPropertyManager propertyManager,
			BusinessObject parent)
		{
			if (!AddOnColumnDataType.IsMultiPartCode(columnDefinition.Type))
			{
				yield return columnDefinition.CreatePropertyManagedCustomProperty(propertyManager, parent);
				yield break;
			}

			(FlyweightPropertyManagedUserDefinedCustomProperty, FlyweightPropertyManagedUserDefinedCustomProperty, List<OnSet>) GetComboBoxDefinitions()
			{
				var part1Definition = new ComboBoxCustomColumnDefinition(
					columnDefinition,
					false,
					ComboBoxCustomColumnDefinition.Part.Part1);

				var (propertyIdentifier, addOnRulesData) = BuildCommon(part1Definition, (a, b, c) => { });
				var fpart1 = new FlyweightPropertyManagedUserDefinedCustomProperty(
					part1Definition,
					propertyIdentifier,
					addOnRulesData.Validators,
					OnSetAction(addOnRulesData.OnSetActions),
					addOnRulesData.MetaData.Concat(new List<DynamicMetaData>() { DynamicMetaData.ParentCustomFieldType(columnDefinition.Type), DynamicMetaData.CustomFieldPosition(1) }).ToArray(),
					addOnRulesData.IsUpperCase);

				var part2Definition = new ComboBoxCustomColumnDefinition(
					columnDefinition,
					false,
					ComboBoxCustomColumnDefinition.Part.Part2);
				(propertyIdentifier, addOnRulesData) = BuildCommon(part2Definition, (a, b, c) => { });
				var fpart2 = new FlyweightPropertyManagedUserDefinedCustomProperty(
					part2Definition,
					propertyIdentifier,
					addOnRulesData.Validators,
					OnSetAction(addOnRulesData.OnSetActions),
					addOnRulesData.MetaData.Concat(new List<DynamicMetaData>() { DynamicMetaData.ParentCustomFieldType(columnDefinition.Type), DynamicMetaData.CustomFieldPosition(2) }).ToArray(),
					addOnRulesData.IsUpperCase);

				return (fpart1, fpart2, addOnRulesData.OnSetActions);
			}

			var (flyweightpart1, flyweightpart2, onSetActions) = parent.Factory.GetCachedValue((columnDefinition.Identifier, "ComboBoxDefinition"), GetComboBoxDefinitions);
			var part1 = new PropertyManagedUserDefinedCustomProperty(parent, flyweightpart1, propertyManager);
			var part2 = new PropertyManagedUserDefinedCustomProperty(parent, flyweightpart2, propertyManager);

			yield return new ComboBoxCustomProperty(parent, propertyManager, part1, part2, ComboBoxCustomColumnDefinition.Part.Part1, OnSetAction(onSetActions));
			yield return new ComboBoxCustomProperty(parent, propertyManager, part1, part2, ComboBoxCustomColumnDefinition.Part.Part2, OnSetAction(onSetActions));
		}

		internal static IEnumerable<ICustomProperty> CreateViewOnlyManagedCustomProperties(
			this ICustomColumnDefinition columnDefinition,
			params DynamicMetaData[] additionalMetaData)
		{
			if (!AddOnColumnDataType.IsMultiPartCode(columnDefinition.Type))
			{
				var (propertyIdentifier, addOnRulesData) = BuildCommon(columnDefinition, (a, b, c) => { }, additionalMetaData);
				yield return new ViewOnlyCustomProperty(columnDefinition, propertyIdentifier, addOnRulesData.MetaData);
				yield break;
			}

			var part1Definition = new ComboBoxCustomColumnDefinition(
				columnDefinition,
				true,
				ComboBoxCustomColumnDefinition.Part.Part1);
			var (part1Identifier, addOnRulesData1) = BuildCommon(part1Definition, (a, b, c) => { }, additionalMetaData);
			yield return new ViewOnlyCustomProperty(
				columnDefinition,
				part1Identifier,
				addOnRulesData1.MetaData.Concat(new List<DynamicMetaData>() { DynamicMetaData.ParentCustomFieldType(columnDefinition.Type), DynamicMetaData.CustomFieldPosition(1) }).ToArray());

			var part2Definition = new ComboBoxCustomColumnDefinition(
				columnDefinition,
				true,
				ComboBoxCustomColumnDefinition.Part.Part2);
			var (part2Identifier, addOnRulesData2) = BuildCommon(part2Definition, (a, b, c) => { }, additionalMetaData);
			yield return new ViewOnlyCustomProperty(
				columnDefinition,
				part2Identifier,
				addOnRulesData2.MetaData.Concat(new List<DynamicMetaData>() { DynamicMetaData.ParentCustomFieldType(columnDefinition.Type), DynamicMetaData.CustomFieldPosition(2) }).ToArray());
		}

		static (string propertyIdentifier,
				AddOnRulesData addOnRulesData)
				BuildCommon(
					ICustomColumnDefinition columnDefinition,
					Action<ICustomColumnDefinition, Type, List<Action<ZPropertyInfo>>> validationAdder,
					params DynamicMetaData[] additionalMetaData)
		{
			var addOnRulesData = BuildCommonCore(columnDefinition, validationAdder, additionalMetaData);
			string propertyIdentifier = GetInternedPropertyIdentifier(columnDefinition);
			return (propertyIdentifier, addOnRulesData);
		}

		static string GetInternedPropertyIdentifier(ICustomColumnDefinition columnDefinition)
		{
			string GetDefinition() => CustomPropertyHelper.GeneratePropertyIdentifier(columnDefinition.Name, AddOnColumnDataType.GetTypeFromCode(columnDefinition.Type));
			if (columnDefinition is BusinessObject bizo && bizo.Factory != null)
			{
				return bizo.Factory.GetCachedValue((columnDefinition.Name, columnDefinition.Type, "CustomPropertyIdentifier"), GetDefinition);
			}
			else
			{
				return GetDefinition();
			}
		}

		static AddOnRulesData BuildCommonCore(
					ICustomColumnDefinition columnDefinition,
					Action<ICustomColumnDefinition, Type, List<Action<ZPropertyInfo>>> validationAdder,
					params DynamicMetaData[] additionalMetaData)
		{
			var columnType = AddOnColumnDataType.GetTypeFromCode(columnDefinition.Type);
			var activeRules = columnDefinition.GetRules().Where(rule => rule.IsEnabled && rule.CanBeApplied(columnType)).ToArray();

			List<DynamicMetaData> metadata = new List<DynamicMetaData>();
			metadata.AddRange(additionalMetaData);
			metadata.Add(DynamicMetaData.Position(columnDefinition.Sequence));
			metadata.Add(DynamicMetaData.Name(columnDefinition.Name));
			metadata.Add(DynamicMetaData.Description(new Description(columnDefinition.NameLocalized)));

			foreach (ICustomAddOnRule rule in activeRules)
			{
				metadata.AddRange(rule.GetMetaData());
			}

			if (!metadata.Exists(md => md.Id == MetaDataTypes.MaxLength)
				&& typeof(ZString).IsAssignableFrom(columnType)
				&& columnDefinition.MaxLength < int.MaxValue)
			{
				metadata.Add(DynamicMetaData.MaxLength(columnDefinition.MaxLength));
			}

			var validatorList = new List<Action<ZPropertyInfo>>(activeRules.Length + 1);
			validationAdder(columnDefinition, columnType, validatorList);

			var isUpperCase = false;
			var onSetActions = new List<OnSet>(activeRules.Length + 1);
			foreach (ICustomAddOnRule rule in activeRules)
			{
				var validator = rule.GetValidator();
				if (validator != null)
				{
					validatorList.Add(validator);
				}

				isUpperCase = isUpperCase || rule.IsUpperCase;

				var onSetAction = rule.GetOnSetBehaviour();
				if (onSetAction != null)
				{
					onSetActions.Add(onSetAction);
				}
			}

			return new AddOnRulesData(metadata.ToArray(), validatorList.ToArray(), onSetActions, isUpperCase);
		}

		struct AddOnRulesData
		{
			public AddOnRulesData(DynamicMetaData[] metaData, Action<ZPropertyInfo>[] validators, List<OnSet> onSetActions, ZBool isUpperCase)
			{
				MetaData = metaData;
				Validators = validators;
				OnSetActions = onSetActions;
				IsUpperCase = isUpperCase;
			}

			public DynamicMetaData[] MetaData { get; }
			public Action<ZPropertyInfo>[] Validators { get; }
			public List<OnSet> OnSetActions { get; }
			public ZBool IsUpperCase { get; }
		}

		sealed class Description : IDescription
		{
			public Description(string description)
			{
				this.description = description;
			}

			public int Count { get { return 1; } }

			public string GetDescription(int index, CultureInfo culture)
			{
				return description;
			}

			public string GetDescription(int index)
			{
				return description;
			}

			readonly string description;
		}

		static Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> OnSetAction(List<OnSet> setActions)
		{
			if (setActions.Any())
			{
				return (parent, bizo, prop, instanceIdentifier, args) =>
				{
					if (bizo != null)
					{
						var info = bizo.FindPropertyInfo(prop);
						if (info != null && args.NewValue != null)
						{
							foreach (var action in setActions)
							{
								action.Invoke(parent, info, instanceIdentifier, args);
							}
						}
					}
				};
			}
			else
			{
				return null;
			}
		}

		sealed class WrappedUserDefinedCustomProperty : ICustomProperty
		{
			readonly BusinessObject parent;
			readonly Action<ZPropertyInfo> validator;
			readonly Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> onSet;
			readonly Func<IEnumerable<ICustomProperty>> relatedProperties;

			public WrappedUserDefinedCustomProperty(
				ICustomColumnDefinition customColumnDefinition,
				BusinessObject parent,
				string identifier,
				Action<ZPropertyInfo> validator,
				Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> onSet,
				DynamicMetaData[] metaData,
				Func<IEnumerable<ICustomProperty>> relatedProperties = null)
			{
				this.CustomColumnDefinition = customColumnDefinition;
				this.parent = parent;
				Identifier = identifier;
				Info = new DynamicBusinessObjectProperty(AddOnColumnDataType.GetTypeFromCode(customColumnDefinition.Type), false, metaData: metaData);
				this.validator = validator;
				this.onSet = onSet;
				Func<IEnumerable<ICustomProperty>> defaultRelatedProperties = () => Enumerable.Empty<ICustomProperty>();
				this.relatedProperties = relatedProperties ?? defaultRelatedProperties;
			}

			public string Identifier { get; private set; }
			public DynamicBusinessObjectProperty Info { get; private set; }
			public IEnumerable<ICustomProperty> RelatedProperties => relatedProperties.Invoke();
			public ICustomColumnDefinition CustomColumnDefinition { get; }
			public bool IsDeleted => CustomColumnDefinition != null && CustomColumnDefinition.IsDeleted;

			public object GetValue(BusinessObject cusObj)
			{
				return parent[CustomColumnDefinition.Name];
			}

			public bool TrySetValue(BusinessObject cusObj, object value)
			{
				var originalValue = parent.GetOrSetOriginalWrappedValue(CustomColumnDefinition);

				var args = new CustomAddOnRuleArgs
				{
					FieldName = CustomColumnDefinition.Name,
					OldValue = originalValue,
					NewValue = value as IZType,
					Rules = CustomColumnDefinition.GetRules(),
					Type = CustomColumnDefinition.Type
				};

				parent[CustomColumnDefinition.Name] = value;
				onSet?.Invoke(parent, cusObj, Identifier, parent?.PK ?? ZGuid.Empty, args);
				return true;
			}

			public void Validate(BusinessObject cusObj)
			{
				validator?.Invoke(((CustomBusinessObject)cusObj).FindPropertyInfo(Identifier));
			}
		}

		sealed class PropertyManagedUserDefinedCustomProperty : ICustomProperty
		{
			public PropertyManagedUserDefinedCustomProperty(BusinessObject parent, FlyweightPropertyManagedUserDefinedCustomProperty provider, UserDefinedPropertyManager propertyManager)
			{
				this.parent = parent;
				this.provider = provider;
				this.propertyManager = propertyManager;
			}
			readonly BusinessObject parent;
			readonly FlyweightPropertyManagedUserDefinedCustomProperty provider;
			readonly UserDefinedPropertyManager propertyManager;

			string ICustomProperty.Identifier => provider.Identifier;
			IEnumerable<ICustomProperty> ICustomProperty.RelatedProperties => provider.RelatedProperties;
			DynamicBusinessObjectProperty ICustomProperty.Info => provider.Info;
			ICustomColumnDefinition ICustomProperty.CustomColumnDefinition => provider.CustomColumnDefinition;
			bool ICustomProperty.IsDeleted => provider.IsDeleted;

			object ICustomProperty.GetValue(BusinessObject parent)
			{
				return propertyManager.GetValue(provider.CustomColumnDefinition.Name, provider.Type);
			}

			bool ICustomProperty.TrySetValue(BusinessObject cusobj, object value)
			{
				return provider.TrySetValue(parent, cusobj, value, propertyManager);
			}

			void ICustomProperty.Validate(BusinessObject parent)
			{
				provider.Validate(parent, propertyManager);
			}
		}

		public sealed class FlyweightPropertyManagedUserDefinedCustomProperty
		{
			readonly Action<ZPropertyInfo>[] validators;
			readonly Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> onSet;
			readonly Func<IEnumerable<ICustomProperty>> relatedProperties;
			readonly ZBool isUpperCase;
			bool storageBizoValidated;

			public FlyweightPropertyManagedUserDefinedCustomProperty(ICustomColumnDefinition customColumnDefinition,
				string identifier,
				Action<ZPropertyInfo>[] validators,
				Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> onSet,
				DynamicMetaData[] metaData,
				ZBool isUpperCase,
				Func<IEnumerable<ICustomProperty>> relatedProperties = null)
			{
				this.CustomColumnDefinition = customColumnDefinition;
				Type = AddOnColumnDataType.GetTypeFromCode(customColumnDefinition.Type);
				Identifier = identifier;
				Info = new DynamicBusinessObjectProperty(Type, false, metaData: metaData);
				this.validators = validators;
				this.onSet = onSet;
				Func<IEnumerable<ICustomProperty>> defaultRelatedProperties = () => Enumerable.Empty<ICustomProperty>();
				this.relatedProperties = relatedProperties ?? defaultRelatedProperties;
				this.isUpperCase = isUpperCase;
			}

			public string Identifier { get; private set; }
			public DynamicBusinessObjectProperty Info { get; private set; }
			public IEnumerable<ICustomProperty> RelatedProperties => relatedProperties.Invoke();
			public ICustomColumnDefinition CustomColumnDefinition { get; }
			public bool IsDeleted => CustomColumnDefinition != null && CustomColumnDefinition.IsDeleted;
			public Type Type { get; }

			public bool TrySetValue(BusinessObject parent, BusinessObject cusObj, object value, UserDefinedPropertyManager propertyManager)
			{
				var val = (IZType)value;

				if (isUpperCase && val is ZString str)
				{
					val = str.ToUpper();
				}

				var originalValue = cusObj.GetOrSetOriginalValue(propertyManager, CustomColumnDefinition);

				var args = new CustomAddOnRuleArgs
				{
					FieldName = CustomColumnDefinition.Name,
					OldValue = originalValue,
					NewValue = val,
					Rules = CustomColumnDefinition.GetRules(),
					Type = CustomColumnDefinition.Type
				};

				var addOnField = propertyManager.SetValue(CustomColumnDefinition.Name, CustomColumnDefinition.Type, val);
				if (addOnField != null && !addOnField.IsDeleted && CustomColumnDefinition.RuleDefinitionReference != null)
				{
					addOnField.XV_XR_Rule = (ZGuid)CustomColumnDefinition.RuleDefinitionReference;
				}

				onSet?.Invoke(parent, cusObj, Identifier, addOnField?.PK ?? ZGuid.Empty, args);
				return true;
			}

			public void Validate(BusinessObject cusObj, UserDefinedPropertyManager propertyManager)
			{
				var info = ((CustomBusinessObject)cusObj).FindPropertyInfo(Identifier);

				foreach (var validator in validators)
				{
					validator(info);
				}

				if (CustomColumnDefinition is BusinessObject bizO && bizO.IsDeleted)
				{
					return;
				}

				AddWarningForInactiveRule(info);

				var propertyStorageBizo = propertyManager.GetProperty(CustomColumnDefinition.Name, CustomColumnDefinition.Type);
				if (propertyStorageBizo != null)
				{
					if (!storageBizoValidated)
					{
						storageBizoValidated = true;
						propertyStorageBizo.Validation.ValidateAll();
					}

					if (propertyStorageBizo.HasErrors)
					{
						foreach (var notification in propertyStorageBizo.Notifications)
						{
							((INotifications)info).Add(notification);
						}
					}
				}
			}

			void AddWarningForInactiveRule(ZPropertyInfo info)
			{
				if (!CustomColumnDefinition.IsRuleActive)
				{
					info.AddWarning(Res.GetString("E105EC78-0216-4A94-8ACA-FC606F307AC3", "The Add On Rule is inactive - The Workflow Template configuration should be updated."));
				}
			}
		}

		sealed class ComboBoxCustomProperty : ICustomProperty
		{
			readonly UserDefinedPropertyManager propertyManager;
			readonly ICustomProperty customProperty;
			readonly ICustomProperty otherProperty;
			readonly ICustomProperty parentProperty;
			readonly BusinessObject parent;
			readonly Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> onSet;
			readonly ComboBoxCustomColumnDefinition.Part part;

			public ComboBoxCustomProperty(
				BusinessObject parent,
				UserDefinedPropertyManager propertyManager,
				ICustomProperty customPropertyPart1,
				ICustomProperty customPropertyPart2,
				ComboBoxCustomColumnDefinition.Part part,
				Action<BusinessObject, BusinessObject, string, ZGuid, CustomAddOnRuleArgs> onSet)
			{
				this.parent = parent;
				this.propertyManager = propertyManager;
				switch (part)
				{
					case ComboBoxCustomColumnDefinition.Part.Part1:
						this.customProperty = customPropertyPart1;
						this.otherProperty = customPropertyPart2;
						break;
					case ComboBoxCustomColumnDefinition.Part.Part2:
					default:
						this.customProperty = customPropertyPart2;
						this.otherProperty = customPropertyPart1;
						break;
				}

				this.part = part;
				this.parentProperty = customPropertyPart1;
				this.onSet = onSet;
			}

			public string Identifier => customProperty.Identifier;

			public IEnumerable<ICustomProperty> RelatedProperties
			{
				get
				{
					yield return otherProperty;
				}
			}

			public DynamicBusinessObjectProperty Info => customProperty.Info;

			public ICustomColumnDefinition CustomColumnDefinition => customProperty.CustomColumnDefinition;
			public bool IsDeleted => customProperty.IsDeleted;

			public object GetValue(BusinessObject parent)
			{
				return customProperty.GetValue(parent);
			}

			public bool TrySetValue(BusinessObject cusObj, object value)
			{
				var val = (IZType)value;

				var mainColumn = CustomColumnDefinition;
				var descColumn = otherProperty.CustomColumnDefinition;

				if (part == ComboBoxCustomColumnDefinition.Part.Part2)
				{
					mainColumn = otherProperty.CustomColumnDefinition;
					descColumn = CustomColumnDefinition;
				}

				var originalValue = cusObj.GetOrSetOriginalValue(propertyManager, mainColumn);
				var originalDesc = cusObj.GetOrSetOriginalValue(propertyManager, descColumn);

				var addOnField = propertyManager.SetValue(CustomColumnDefinition.Name, CustomColumnDefinition.Type, val);
				if (addOnField != null && !addOnField.IsDeleted && CustomColumnDefinition.RuleDefinitionReference != null)
				{
					addOnField.XV_XR_Rule = (ZGuid)CustomColumnDefinition.RuleDefinitionReference;
				}

				// Only raise the event if both properties have been set
				ZGuid? parentCustomPropertyPK = null;
				var otherAddOnField = propertyManager.GetProperty(otherProperty.CustomColumnDefinition.Name, otherProperty.CustomColumnDefinition.Type);
				if (otherAddOnField != null && addOnField != null)
				{
					parentCustomPropertyPK = customProperty.Identifier.Equals(parentProperty.Identifier) ? addOnField.PK : otherAddOnField.PK;
				}

				var args = new CustomAddOnRuleArgs
				{
					FieldName = ((ComboBoxCustomColumnDefinition)CustomColumnDefinition).OriginalName,
					OldValue = originalValue,
					OldDescription = originalDesc,
					Rules = CustomColumnDefinition.GetRules(),
					Type = AddOnColumnDataType.Codes.ComboBox
				};

				if (part == ComboBoxCustomColumnDefinition.Part.Part1)
				{
					args.NewValue = val;
					args.NewDescription = val.IsEmpty ? ZString.Empty : otherAddOnField?.XV_Data;
				}
				else
				{
					args.NewValue = val.IsEmpty ? ZString.Empty : otherAddOnField?.XV_Data;
					args.NewDescription = val;
				}

				onSet?.Invoke(parent, cusObj, Identifier, parentCustomPropertyPK ?? ZGuid.Empty, args);
				return true;
			}

			public void Validate(BusinessObject cusObj)
			{
				customProperty.Validate(cusObj);

				var customPropertyInfo = ((CustomBusinessObject)cusObj).FindPropertyInfo(customProperty.Identifier);
				if (cusObj is CustomBusinessObject bizo && !customProperty.CustomColumnDefinition.IsDeleted)
				{
					if (string.IsNullOrWhiteSpace(bizo[customProperty.Identifier].ToString()) && !string.IsNullOrWhiteSpace(bizo[otherProperty.Identifier].ToString()))
					{
						customPropertyInfo.AddError(Res.GetString("a1240d43-0f17-4360-ad4c-d88596665c12", "Enter a value for both or neither fields."));
					}
				}
			}
		}

		sealed class ComboBoxCustomColumnDefinition : ICustomColumnDefinition
		{
			readonly ICustomColumnDefinition columnDefinition;
			readonly bool codeAndDescriptionForComboBoxCaptions;
			readonly Part part;

			public enum Part
			{
				Part1 = 1,
				Part2 = 2
			}

			public ComboBoxCustomColumnDefinition(ICustomColumnDefinition columnDefinition, bool codeAndDescriptionForComboBoxCaptions, Part part)
			{
				this.columnDefinition = columnDefinition;
				this.codeAndDescriptionForComboBoxCaptions = codeAndDescriptionForComboBoxCaptions;
				this.part = part;
				MaxLength = columnDefinition.MaxLength;
				Sequence = columnDefinition.Sequence;
				RuleDefinitionReference = columnDefinition.RuleDefinitionReference;
				NameLocalized = GetNameLocalized();
				Name = columnDefinition.Name + AddOnColumnDataType.PartIdentifier + (int)part;
				Identifier = columnDefinition.Identifier;
				OriginalName = columnDefinition.Name;
			}

			public string Type => AddOnColumnDataType.Codes.String;

			public string Name { get; }

			public string GetNameLocalized()
			{
				switch (part)
				{
					case Part.Part1:
						return columnDefinition.NameLocalized + (codeAndDescriptionForComboBoxCaptions ? " " + Res.GetString("23dad99c-8d1a-4a75-b8d1-894de1893224", "Code") : "");
					case Part.Part2:
					default:
						return columnDefinition.NameLocalized + (codeAndDescriptionForComboBoxCaptions ? " " + Res.GetString("472e30fb-73d6-43dc-b751-a42dbbaeec63", "Description") : "");
				}
			}

			public string OriginalName { get; }
			public ZGuid Identifier { get; }
			public string NameLocalized { get; }
			public int MaxLength { get; }
			public int? Sequence { get; }
			public ZGuid? RuleDefinitionReference { get; }
			public bool IsDeleted => columnDefinition.IsDeleted;
			public bool IsRuleActive => !columnDefinition.IsDeleted && columnDefinition.IsRuleActive;

			public ICustomAddOnRule[] GetRules()
			{
				switch (part)
				{
					case Part.Part1:
						return columnDefinition.GetRules();
					case Part.Part2:
					default:
						return columnDefinition.GetRules().Where(x => !(x is InvalidCodeRule)).ToArray();
				}
			}
		}

		sealed class ViewOnlyCustomProperty : ICustomProperty
		{
			readonly Func<IEnumerable<ICustomProperty>> relatedProperties;

			public ViewOnlyCustomProperty(
				ICustomColumnDefinition customColumnDefinition,
				string identifier,
				DynamicMetaData[] metaData,
				Func<IEnumerable<ICustomProperty>> relatedProperties = null)
			{
				this.CustomColumnDefinition = customColumnDefinition;
				Identifier = identifier;
				var typeToUse = customColumnDefinition.Type == AddOnColumnDataType.Codes.ComboBox ? AddOnColumnDataType.Codes.String : customColumnDefinition.Type;
				Info = new DynamicBusinessObjectProperty(AddOnColumnDataType.GetTypeFromCode(typeToUse), false, metaData: metaData);
				Func<IEnumerable<ICustomProperty>> defaultRelatedProperties = () => Enumerable.Empty<ICustomProperty>();
				this.relatedProperties = relatedProperties ?? defaultRelatedProperties;
			}

			public string Identifier { get; private set; }
			public DynamicBusinessObjectProperty Info { get; private set; }
			public IEnumerable<ICustomProperty> RelatedProperties => relatedProperties.Invoke();
			public ICustomColumnDefinition CustomColumnDefinition { get; }
			public bool IsDeleted => CustomColumnDefinition != null && CustomColumnDefinition.IsDeleted;

			public object GetValue(BusinessObject cusObj)
			{
				return null;
			}

			public bool TrySetValue(BusinessObject cusObj, object value)
			{
				return true;
			}

			public void Validate(BusinessObject cusObj)
			{
			}
		}
	}
}
