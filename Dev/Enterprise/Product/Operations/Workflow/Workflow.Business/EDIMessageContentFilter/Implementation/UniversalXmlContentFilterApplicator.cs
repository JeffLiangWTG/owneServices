using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	class UniversalXmlContentFilterApplicator : IUniversalXmlContentFilterApplicator
	{
		const string AttachedDocumentCollection = "AttachedDocumentCollection";

		const string TheOnlyTypeThatSupportDataContextFiltering = "SubShipmentCollection";

		public void RemoveNonExportedCollections(ITopLevelDataObject dataObject, IEDIMessageContentFilter contentFilter)
		{
			RemoveNonExportedCollections(dataObject, contentFilter as EDIMessageContentFilter);
		}

		public void RemoveNonExportedCollections(ITopLevelDataObject dataObject, IMessageProfile messageProfile)
		{
			var filterType = messageProfile.GetFilterType();
			if (filterType.HasValue)
			{
				RemoveNonExportedCollections(dataObject, messageProfile.GetFilterElements(), filterType.Value == SchemaFilterType.Exclude);
			}
		}

		public void RemoveNonExportedCollections(ITopLevelDataObject dataObject, IUniversalActionInfo info)
		{
			var filter = EDIMessageContentFilter.Load(info.FactoryForProcessing, info.PurposeCode);
			RemoveNonExportedCollections(dataObject, filter);
		}

		void RemoveNonExportedCollections(ITopLevelDataObject dataObject, EDIMessageContentFilter filter)
		{
			if (filter != null && TryGetFilterSchema(dataObject, filter, out var schema))
			{
				var elements = schema.Lines.Cast<EDIMessageContentFilterLine>().GroupBy(s => s.SchemaElement.ToString())
					.ToDictionary(group => group.Key, group => group.Select(s => s.DataContext.ToString()), StringComparer.OrdinalIgnoreCase);
				RemoveNonExportedCollections(dataObject, elements, schema.IsExclude);
			}
		}

		public void ExportAttachedDocuments(BusinessObject businessObject, ITopLevelDataObject dataObject, IUniversalActionInfo info)
		{
			if (businessObject is IDocManagerSupport docManagerSupport
				&& dataObject is IAttachedDocumentContainer documentContainer)
			{
				var documentCodesToAttach = GetDocumentCodesFromEdiMessageContentFilter(info.FactoryForProcessing, dataObject, info.PurposeCode);
				if (documentCodesToAttach.Count > 0)
				{
					var eDocsToAttach = docManagerSupport.DocManagerInfo?.AllEDocs?.Cast<IeDoc>().Where(doc => documentCodesToAttach.Contains(doc.DocType)).ToArray();

					if (eDocsToAttach != null && eDocsToAttach.Length > 0)
					{
						documentContainer.SetAttachedDocumentCollection(() => ObjectFactory.Get<IAttachedDocumentDataObjectWriter>().GenerateAttachedDocuments(false, eDocsToAttach));
					}
				}
			}
		}

		public void ImportAttachedDocuments(BusinessObject businessObject, ITopLevelDataObject dataObject, IXmlImportLogger logger)
		{
			if (businessObject is IDocManagerSupport docManagerSupport
				&& dataObject is IAttachedDocumentContainer documentContainer
				&& documentContainer.AttachedDocumentCollection != null)
			{
				var attachedDocumentDataObjectReader = ObjectFactory.New<IAttachedDocumentDataObjectReader>();

				foreach (var attachedDocument in documentContainer.AttachedDocumentCollection)
				{
					if (attachedDocumentDataObjectReader.TryAddAttachedDocument(attachedDocument, logger, docManagerSupport, out IeDoc eDoc))
					{
						if (businessObject is IStmALogParent logParent)
						{
							var ddiLog = logParent.Logs.AddNew(AutoEvents.DocumentImported, eDoc.CreateReference());

							if (logger is IXmlSessionTracker sessionTracker)
							{
								sessionTracker.SourceMessage?.AddUniversalDataLink(ddiLog);
							}
						}
					}
				}
			}
		}

		void RemoveNonExportedCollections(ITopLevelDataObject dataObject, Dictionary<string, IEnumerable<string>> filterElements, bool isExclude)
		{
			var stack = new Stack<ItemQueue>();
			stack.Push(new ItemQueue(null, dataObject, null));

			while (stack.Count > 0)
			{
				var queue = stack.Pop();

				while (queue.Queue.Count > 0)
				{
					var (parent, propertyInfo) = queue.Queue.Dequeue();
					if (ShouldRemoveProperty(propertyInfo.Name, isExclude, filterElements, parent, propertyInfo))
					{
						propertyInfo.SetValue(parent, null);
					}
					else
					{
						var value = propertyInfo.GetValue(parent);
						if (IsTypeAllowed(value))
						{
							stack.Push(new ItemQueue(parent, value, propertyInfo));
						}
					}
				}
			}
		}

		public bool GetShouldUniversalShipmentExcludeCollection(ITopLevelDataObject dataObject, IUniversalActionInfo info, string dataContext)
		{
			var filter = EDIMessageContentFilter.Load(info.FactoryForProcessing, info.PurposeCode);
			if (filter != null
				&& dataObject is Shipment
				&& TryGetFilterSchema(dataObject, filter, out var schema)
				&& schema.IsExclude)
			{
				var subShipmentCollectionLines = schema.Lines.Cast<EDIMessageContentFilterLine>()
					.Where(line => line.SchemaElement == TheOnlyTypeThatSupportDataContextFiltering);
				if (subShipmentCollectionLines.Any(line => line.DataContext.IsEmpty || line.DataContext == dataContext))
				{
					return true;
				}
			}

			return false;
		}

		bool ShouldRemoveProperty(string propertyName, bool isExclude, Dictionary<string, IEnumerable<string>> filterElements, object obj, PropertyInfo propertyInfo)
		{
			var result = false;

			if (isExclude)
			{
				if (propertyName == TheOnlyTypeThatSupportDataContextFiltering)
				{
					result = filterElements.TryGetValue(TheOnlyTypeThatSupportDataContextFiltering, out var dataContext) && !dataContext.Any();
					if (!result)
					{
						result = HasMatchingDataContextInAnySubShipment(filterElements, obj);
					}
				}
				else
				{
					result = IsPropertyConfigurable(propertyName, propertyInfo.PropertyType) && filterElements.Keys.Contains(propertyName);
				}
			}
			else
			{
				result = IsPropertyConfigurable(propertyName, propertyInfo.PropertyType) && !filterElements.Keys.Contains(propertyName);
			}

			return result;
		}

		bool HasMatchingDataContextInAnySubShipment(Dictionary<string, IEnumerable<string>> filterElements, object obj)
		{
			if (obj is Shipment subShipment
				&& filterElements.TryGetValue(TheOnlyTypeThatSupportDataContextFiltering, out var dataContext)
				&& dataContext.Any())
			{
				return subShipment.SubShipmentCollection.Any(sub => sub.DataContext.DataSourceCollection.Any(s => dataContext.Contains(s.Type.GetValueOrDefault().ToString())));
			}

			return false;
		}

		HashSet<string> GetDocumentCodesFromEdiMessageContentFilter(BusinessObjectFactory factory, ITopLevelDataObject dataObject, string purposeCode)
		{
			if (factory == null || string.IsNullOrEmpty(purposeCode))
			{
				return new HashSet<string>();
			}

			var filter = EDIMessageContentFilter.Load(factory, purposeCode);
			if (filter != null && TryGetFilterSchema(dataObject, filter, out var schema))
			{
				var isAttachedDocumentCollectionInLines = schema.Lines.Cast<EDIMessageContentFilterLine>()
					.Any(s => s.SchemaElement.EqualsIgnoringCase(AttachedDocumentCollection));

				if ((schema.FilterType.Equals(EDIMessageContentFilterTypes.Codes.Include) && isAttachedDocumentCollectionInLines) ||
					(schema.FilterType.Equals(EDIMessageContentFilterTypes.Codes.Exclude) && !isAttachedDocumentCollectionInLines))
				{
					return new HashSet<string>(schema.Documents.Cast<EDIMessageContentFilterDocument>().Select(d => d.DocumentType.ToString()), StringComparer.OrdinalIgnoreCase);
				}
			}

			return new HashSet<string>();
		}

		public bool ShouldExcludeEmptyElements(IUniversalActionInfo info)
		{
			if (info.FactoryForProcessing == null || string.IsNullOrEmpty(info.PurposeCode))
			{
				return false;
			}
			return EDIMessageContentFilter.Load(info.FactoryForProcessing, info.PurposeCode)?.Config?.ExcludeEmptyElements ?? false;
		}

		static bool IsTypeAllowed(object parent)
		{
			return !(parent is IZType) && !(parent is Stream) && Nullable.GetUnderlyingType(parent.GetType()) == null;
		}

		static bool TryGetFilterSchema(ITopLevelDataObject dataObject, EDIMessageContentFilter filter, out EDIMessageContentFilterSpec schema)
		{
			if (dataObject is Shipment)
			{
				schema = filter.UniversalShipment;
				return true;
			}
			else if (dataObject is UniversalDataBuss.DataObjects.Universal.Event)
			{
				schema = filter.UniversalEvent;
				return true;
			}
			else if (dataObject is UniversalDataBuss.DataObjects.Accounting.TransactionBatch
					|| dataObject is UniversalDataBuss.DataObjects.Accounting.TransactionInfo)
			{
				schema = filter.UniversalTransaction;
				return true;
			}
			else
			{
				ErrorReporter.ReportOnce(nameof(UniversalXmlContentFilterApplicator) + "UnknownLineType", FormattableString.Invariant($"Unknown type {dataObject.GetType().FullName}"));
				schema = null;
				return false;
			}
		}

		public static CodeDescriptionPairList GetPropertyList(Type type)
		{
			var result = new CodeDescriptionPairList();
			foreach (var (name, propertyType) in new[] { (string.Empty, type) }.SelectDistinctRecursive(r => r.type.GetProperties().Select(p => (p.Name, p.PropertyType))))
			{
				if (IsPropertyConfigurable(name, propertyType) && !result.ContainsCode(name) && name != nameof(ITopLevelDataObject.MessageNumberCollection))
				{
					result.AddPair(name, GetEnumerableTypeName(propertyType));
				}
			}
			return result;
		}

		static IEnumerable<Type> ParentTypes(Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}
		}

		static string GetEnumerableTypeName(Type type)
		{
			return type.GetInterfaces().Append(type)
					.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					.Select(t => t.GetGenericArguments()[0]).FirstOrDefault()?.Name
					??
					ParentTypes(type).Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(NonPersistentBusinessObjectCollection<>))
					.Select(t => t.GetGenericArguments()[0]).FirstOrDefault()?.Name
					?? string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Magic string constant that users don't see.")]
		public static bool IsPropertyConfigurable(string name, Type propertyType) => typeof(IEnumerable).IsAssignableFrom(propertyType) && name.EndsWith("collection", StringComparison.OrdinalIgnoreCase);

		class ItemQueue
		{
			public ItemQueue(object parent, object obj, PropertyInfo parentInfo)
			{
				Parent = parent;
				Object = obj;
				ParentInfo = parentInfo;

				if (obj is ICollection collection) // This line would cause issues if we had lists of lists in the schema.
				{
					Queue = new Queue<(object, PropertyInfo)>(collection.Cast<object>().SelectMany(o => GetProperties(o)));
				}
				else
				{
					Queue = new Queue<(object, PropertyInfo)>(GetProperties(obj));
				}
			}

			static IEnumerable<(object, PropertyInfo)> GetProperties(object o) => o.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0 && p.GetValue(o) != null).Select(p => (o, p));

			public object Object { get; }
			public object Parent { get; }
			public PropertyInfo ParentInfo { get; }
			public Queue<(object, PropertyInfo)> Queue { get; }
			public bool HasInclude { get; set; }
		}
	}
}
