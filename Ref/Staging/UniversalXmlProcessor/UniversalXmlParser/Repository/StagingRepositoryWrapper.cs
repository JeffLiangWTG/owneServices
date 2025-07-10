using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;
using SequentialGuid;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Repository;

public class StagingRepositoryWrapper : IStagingRepositoryWrapper
{
	#region Member Variables

	readonly IStagingRepository repository;
	readonly IEntityMissingValuesProvider missingValuesProvider;
	readonly IEntityTypeHelper entityTypeHelper;
	readonly IParserConfig _parserConfig;
	readonly Dictionary<Type, MethodInfo> repositoryBulkInsertMethodInfos = new Dictionary<Type, MethodInfo>();
	readonly IDictionary<Type, ArrayList> refDataObjects = new Dictionary<Type, ArrayList>();
	IList<EntityTypeMetaData> _entityTypeMetaDataCollection;
	const int BulkInsertTimeoutInSeconds = 90;

	#endregion

	#region Constructors

	public StagingRepositoryWrapper(
		IStagingRepository stagingRepository,
		IEntityMissingValuesProvider entityMissingValuesProvider,
		IEntityTypeHelper entityTypeHelper,
		IParserConfig parserConfig)
	{
		Argument.NotNull(stagingRepository, nameof(stagingRepository));
		Argument.NotNull(entityMissingValuesProvider, nameof(entityMissingValuesProvider));
		Argument.NotNull(entityTypeHelper, nameof(entityTypeHelper));
		Argument.NotNull(parserConfig, nameof(parserConfig));

		repository = stagingRepository;
		missingValuesProvider = entityMissingValuesProvider;
		this.entityTypeHelper = entityTypeHelper;
		this._parserConfig = parserConfig;

		var logContext = GetType().Name;
	}

	#endregion

	#region Properties

	bool _checkBulkInsert;
	public bool CheckBulkInsert { get { return _checkBulkInsert; } private set { _checkBulkInsert = value; } }
	public IEnumerable<XmlSchemaEntityRelationshipMap> SchemaRelationshipMapList { get; set; }

	IList<string> RefDataTypes
	{
		get
		{
			if (lazyGetRefDataTypes == null)
			{
				lazyGetRefDataTypes = new Lazy<IList<string>>(() =>
				{
					if (StagingRepositoryAssembly != null)
					{
						return StagingRepositoryAssembly.GetTypes()
							.Where(x => !x.IsInterface && !x.IsAbstract && (x.Namespace == Constants.StagingSchema.NamespaceName))
							.Select(x => x.Name).ToList();
					}

					return new List<string>();
				});
			}

			return lazyGetRefDataTypes.Value;
		}
	}

	Lazy<IList<string>> lazyGetRefDataTypes;

	Assembly StagingRepositoryAssembly
	{
		get
		{
			if (lazyGetStagingAssembly == null)
			{
				lazyGetStagingAssembly = new Lazy<Assembly>(() => Assembly.Load(Constants.StagingSchema.AssemblyName));
			}

			return lazyGetStagingAssembly.Value;
		}
	}

	Lazy<Assembly> lazyGetStagingAssembly;

	#endregion

	#region Methods

	public async Task<bool> ExecuteBulkInsertAsync()
	{
		try
		{
			return await ExecuteBulkInsertCoreAsync();
		}
		catch
		{
			Console.Error.WriteLine($"BulkInsert failed in StagingDB.");
			throw;
		}
		finally
		{
			CheckBulkInsert = false;
		}
	}

	async Task<bool> ExecuteBulkInsertCoreAsync()
	{
		var bulkInsertResult = new Dictionary<Type, int>();
		BulkInsertExecuting?.Invoke();

		var idx = 0;
		while (refDataObjects.Count > 0)
		{
			var elementA = refDataObjects.ElementAt(idx);
			var hasDependency = refDataObjects.Any(x => elementA.Key.IsRelatedTo(x.Key, false));
			if (hasDependency)
			{
				if (idx < refDataObjects.Count - 1)
				{
					idx++;
				}
				else
				{
					throw new RefDataProcessingException($@"All the entities have dependency.
Relevant EntityTypes: {refDataObjects.Select(x => x.Key.Name)}", ErrorCodes.CircularDependency, refDataObjects.Keys);
				}
			}
			else
			{
				if ((elementA.Value == null) || (elementA.Value.Count <= 0))
				{
					continue;
				}

				var bulkInsert = GetGenericBulkInsertMethodInfo(elementA.Key);

				var array = elementA.Value.ToArray(elementA.Key);
				await (Task)bulkInsert.Invoke(repository, [array, BulkInsertTimeoutInSeconds]);

				bulkInsertResult[elementA.Key] = array.Length;
				refDataObjects.Remove(elementA.Key);
				idx = 0;
			}
		}
		var message = "BulkInsert succeeded in StagingDB. " + string.Join(' ', bulkInsertResult.Select(x => $"{x.Key.Name}: {x.Value} records inserted."));
		Console.WriteLine(message);
		return true;
	}

	public string CreateRecord(string entityType, string xmlContent, int xmlReaderLineNumber, Guid sourceId)
	{
		Argument.NotNullOrEmpty(entityType, nameof(entityType));
		Argument.NotNullOrEmpty(xmlContent, nameof(xmlContent));
		var result = string.Empty;
		if (!IsDataElement(entityType))
		{
			result = xmlContent;
		}
		else
		{
			var typeOfEntity = GetType(entityType);
			var errorMessage = string.Empty;
			var record = CreateRecordCore(typeOfEntity, xmlContent, xmlReaderLineNumber, out errorMessage);
			var dpi = new DataProcessingInformation();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				dpi = CreateDataProcessingInformation(sourceId, entityTypeHelper.GetTableCode(typeOfEntity), null, StatusProvider.GetERRStatus(), errorMessage);
			}
			else if (record != null)
			{
				dpi = CreateDataProcessingInformation(sourceId, entityTypeHelper.GetTableCode(typeOfEntity), GetPrimaryKeyValue(record), StatusProvider.GetQUEStatus());
			}
			var dpiType = dpi.GetType();
			AddRecordToRefDataObjects(dpiType, dpi);
			CheckBulkInsert = refDataObjects[dpiType].Count >= _parserConfig.BulkInsertSize;
		}

		return result;
	}

	public Action BulkInsertExecuting { get; set; }

	public bool IsDataElement(string entityTypeName)
	{
		return RefDataTypes.Contains(entityTypeName) || GetType(entityTypeName) != null;
	}

	public void SetEntityTypeMetaData(IList<EntityTypeMetaData> entityTypeMetaDataCollection)
	{
		_entityTypeMetaDataCollection = entityTypeMetaDataCollection;
	}

	void AddRecordToRefDataObjects(Type type, object instance)
	{
		Argument.NotNull(type, nameof(type));

		if (!refDataObjects.ContainsKey(type))
		{
			refDataObjects.Add(type, new ArrayList());
		}

		refDataObjects[type].Add(instance);
	}

	bool IsDefinedInSchema(string child, string parent)
	{
		if (!string.IsNullOrEmpty(child) && string.IsNullOrEmpty(parent))
		{
			return SchemaRelationshipMapList.Any(x => x.HasSchemaCombination(child));
		}
		else if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
		{
			return SchemaRelationshipMapList.Any(x => x.HasSchemaCombination(parent, child));
		}
		return false;
	}

	object CreateRecordCore(Type entityType, string xmlContent, int xmlReaderLineNumber, out string message, object parentInstance = null)
	{
		message = string.Empty;
		var instance = Activator.CreateInstance(entityType);

		if (!IsDefinedInSchema(entityType.Name, parentInstance?.GetType()?.Name))
		{
			message = $"Schema and content structure does not match: relationship between {entityType.Name} and {parentInstance?.GetType()?.Name ?? string.Empty} is either not specified or entity is not defined.";
			var ex = new RefDataProcessingException(message, ErrorCodes.IncorrectXmlDataContent,
				[entityType, parentInstance?.GetType()]);
			Console.Error.WriteLine(ex);
			return instance;
		}

		missingValuesProvider.SetPrimaryKey(instance);

		using (var xmlReader = XmlReader.Create(new StringReader(xmlContent.Replace("\r", "&#xD;"))))
		{
			var xmlInfo = (IXmlLineInfo)xmlReader;
			var lineNumber = xmlInfo.LineNumber + xmlReaderLineNumber;

			var entityTypeMetaData = _entityTypeMetaDataCollection?.FirstOrDefault(x => x.Name == entityType.Name);

			if (entityTypeMetaData?.DefaultValues != null && entityTypeMetaData.DefaultValues.Any())
			{
				SetPresetValues(instance, entityTypeMetaData.DefaultValues);
			}

			var lastField = string.Empty;
			while (!xmlReader.EOF)
			{
				if ((xmlReader.Depth != 1) || (xmlReader.NodeType != XmlNodeType.Element))
				{
					xmlReader.Read();
					lastField = xmlReader.Name;
					continue;
				}

				SetValue(instance, xmlReader.Name, xmlReader, lineNumber, out message);
				if (lastField == xmlReader.Name)
				{
					xmlReader.Read();
				}
				else if (xmlReader.IsEmptyElement)
				{
					xmlReader.Read();
					lastField = xmlReader.Name;
				}
			}

			if (entityTypeMetaData?.ConstantValues != null && entityTypeMetaData.ConstantValues.Any())
			{
				SetPresetValues(instance, entityTypeMetaData.ConstantValues);
			}

			if (parentInstance != null)
			{
				SetForeignKeyValue(instance, parentInstance);
			}

			missingValuesProvider.FillOutMissingValues(instance);

			AddRecordToRefDataObjects(entityType, instance);
		}

		return instance;
	}

	void SetPresetValues(object instance, IDictionary<string, string> presetValues)
	{
		Argument.NotNull(instance, nameof(instance));
		Argument.NotNull(presetValues, nameof(presetValues));

		foreach (var presetValuePair in presetValues)
		{
			var propertyName = presetValuePair.Key;
			var propertyInfo = entityTypeHelper.GetProperty(instance.GetType(), propertyName);
			if (propertyInfo != null)
			{
				propertyInfo.SetValue(instance, TypeExtension.ChangeType(presetValuePair.Value, propertyInfo.PropertyType), null);
			}
		}
	}

	void SetValue(object instance, string propertyName, XmlReader xmlReader, int xmlReaderLineNumber, out string message)
	{
		message = string.Empty;
		var collectionProperty = GetCollectionProperty(instance, propertyName);
		if (collectionProperty != null)
		{
			var entityType = GetType(propertyName);
			if (entityType != null)
			{
				if (!xmlReader.IsEmptyElement)
				{
					CreateRecordCore(entityType, xmlReader.ReadOuterXml(), xmlReaderLineNumber, out message, instance);
				}
				else
				{
					var emptyTypeMetaData = _entityTypeMetaDataCollection?.FirstOrDefault(x => x.Name == xmlReader.Name);

					if ((emptyTypeMetaData?.DefaultValues != null && emptyTypeMetaData.DefaultValues.Any())
						|| (emptyTypeMetaData?.ConstantValues != null && emptyTypeMetaData.ConstantValues.Any()))
					{
						CreateRecordCore(entityType, xmlReader.ReadOuterXml(), xmlReaderLineNumber, out message, instance);
					}
				}
			}
		}
		else
		{
			var propertyInfo = entityTypeHelper.GetProperty(instance.GetType(), propertyName);
			if (propertyInfo != null)
			{
				var unescapedXmlData = new System.Security.SecurityElement(xmlReader.Name, xmlReader.ReadInnerXml());
				propertyInfo.SetValue(instance, TypeExtension.ChangeType(unescapedXmlData.Text?.Trim(), propertyInfo.PropertyType), null);
			}
			else
			{
				var errorMessage = $"Property {propertyName} cannot be found from {instance.GetType().Name}";
				Console.Error.WriteLine(new RefDataProcessingException(errorMessage, ErrorCodes.IncorrectXmlDataContent, [instance.GetType()], [propertyName]).Message);
			}
		}
	}

	void SetForeignKeyValue(object childInstance, object parentInstance)
	{
		Argument.NotNull(childInstance, nameof(childInstance));
		Argument.NotNull(parentInstance, nameof(parentInstance));

		var parentType = parentInstance.GetType();
		var childType = childInstance.GetType();

		var parentTableCode = entityTypeHelper.GetTableCode(parentType);
		var childTableCode = entityTypeHelper.GetTableCode(childType);
		if ((string.IsNullOrEmpty(parentTableCode)) || string.IsNullOrEmpty(childTableCode))
		{
			return;
		}

		var primaryKeyProperty = entityTypeHelper.GetPrimaryKeyProperty(parentType);

		var foreignKeyProperty = entityTypeHelper.GetForeignKeyProperty(childType, childTableCode, parentTableCode)
			?? entityTypeHelper.GetForeignKeyProperty(childType, childTableCode, "ParentPK");

		if ((foreignKeyProperty != null) && foreignKeyProperty.CanWrite)
		{
			foreignKeyProperty.SetValue(childInstance, primaryKeyProperty.GetValue(parentInstance, null));
		}
	}

	#endregion

	#region Helpers

	static DataProcessingInformation CreateDataProcessingInformation(Guid sourceId, string parentTableCode, Guid? parentPk, string dpiStatus, string message = "")
	{
		var sequentialGuidInstance = SequentialSqlGuidGenerator.Instance;

		var dataProcessingInformation = new DataProcessingInformation
		{
			DPI_ID = sequentialGuidInstance.NewGuid(),
			DPI_Status = dpiStatus,
			DPI_SourceId = sourceId,
			DPI_Message = message,
			DPI_ParentTableCode = parentTableCode,
			DPI_ParentPk = parentPk
		};

		return dataProcessingInformation;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
#if DEBUG
	public Guid? GetPrimaryKeyValue(object instance)
#else
		Guid? GetPrimaryKeyValue(object instance)
#endif
	{
		Argument.NotNull(instance, nameof(instance));

		var entityType = instance.GetType();
		var primaryKeyProperty = entityTypeHelper.GetPrimaryKeyProperty(entityType);
		if (primaryKeyProperty == null)
		{
			return null;
		}

		var value = primaryKeyProperty.GetValue(instance);
		if (value == null)
		{
			return null;
		}

		Guid? primaryKeyValue;
		try
		{
			primaryKeyValue = Guid.Parse(value.ToString());
		}
		catch
		{
			primaryKeyValue = null;
		}

		return primaryKeyValue;
	}

	Type GetType(string entityType)
	{
		var namespaceQualifiedTypeName = Constants.StagingSchema.NamespaceName + "." + entityType;

		if (StagingRepositoryAssembly != null)
		{
			return StagingRepositoryAssembly.GetType(namespaceQualifiedTypeName);
		}

		return Type.GetType(namespaceQualifiedTypeName);
	}

	PropertyInfo GetCollectionProperty(object instance, string propertyName)
	{
		var propertyType = GetType(propertyName);
		if (propertyType == null)
		{
			return null;
		}

		Type[] types = { propertyType };
		var collectionType = typeof(ICollection<>);
		var instanceType = instance.GetType();

		var genericType = collectionType.MakeGenericType(types);

		return instanceType.GetProperties().FirstOrDefault(x => x.PropertyType == genericType);
	}

	MethodInfo GetGenericBulkInsertMethodInfo(Type entityType)
	{
		Argument.NotNull(entityType, nameof(entityType));

		if (repositoryBulkInsertMethodInfos.ContainsKey(entityType))
		{
			return repositoryBulkInsertMethodInfos[entityType];
		}

		var addMethod = repository.GetType().GetMethod(nameof(IStagingRepository.BulkInsertWithRetryAsync));
		if (addMethod == null)
		{
			return null;
		}

		var genericMethod = addMethod.MakeGenericMethod(entityType);
		repositoryBulkInsertMethodInfos[entityType] = genericMethod;
		return genericMethod;
	}
	#endregion
}
