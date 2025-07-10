using System;
using System.Collections;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IProcessTaskLoadStrategy
	{
		Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory);
		void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery);
	}

	public class ProcessTaskTypeDecider : TypeDecider
	{
		protected internal ProcessTaskTypeDecider()
		{
			var de = ObjectFactory.Get<Hashtable>("ProcessTaskTypes").Cast<DictionaryEntry>().ToDictionary(kvp => (string)kvp.Key, kvp => (ObjectHandle)kvp.Value);
			types = ImmutableDictionary.CreateRange(de);
		}

		#region GetTypeForLoad

		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;

			ZString parentTablePrefix = row[ProcessTasksSchema.P9_ParentTableCode.Name].ToString();
			ZGuid parentID = new ZGuid(row[ProcessTasksSchema.P9_ParentID.Name]);

			if (!parentTablePrefix.IsEmpty)
			{
				if (parentTablePrefix == JobHeaderSchema.Constants.Prefix)
				{
					JobHeader job = factory.Load<JobHeader>(new ZGuid(row[ProcessTasksSchema.P9_ParentID.Name]));
					if (job != null)
					{
						parentTablePrefix = job.JH_ParentTableCode;
						parentID = job.JH_ParentID;
					}
				}
				result = GetTypeForLoad(parentTablePrefix, parentID, factory);
			}

			if (result == null)
			{
				result = typeof(ProcessTask);
			}
			return result;
		}

		protected virtual Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			Type result = null;

			if (types.TryGetValue(parentTablePrefix, out var objectHandle))
			{
				result = objectHandle.GetObjectType();
				if (typeof(IProcessTaskLoadStrategy).IsAssignableFrom(result))
				{
					IProcessTaskLoadStrategy loadStrategy = (IProcessTaskLoadStrategy)objectHandle.GetObject();
					result = loadStrategy.GetTypeForLoad(parentTablePrefix, parentID, factory);
				}
			}

			return result;
		}

		#endregion

		public override Type GetTypeForNew()
		{
			return typeof(ProcessTask);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(ProcessTask);
		}

		public static ProcessTaskTypeDecider GetInstance()
		{
			ProcessTaskTypeDecider clientTypeDecider = (ProcessTaskTypeDecider)TypeDecider.GetClientTypeDeciderFromType(typeof(ProcessTask));
			return clientTypeDecider ?? ProcessTask.TypeDecider;
		}

		public ZQuery GetQueryForLoad(WorkflowDescriptor descriptor)
		{
			var result = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = GetParentJobSubQuery(descriptor, ProcessTasksSchema.P9_ParentID, isNotInQuery: false);

			result.AddSubQuery(subQuery, JoinCondition.Or);

			if (descriptor.AreTasksCompanySpecific)
			{
				result.AddToFilter(ProcessTask.Loader.GetFilterForCurrentCompany());
			}

			return result;
		}

		public ZDBOnlySubQuery GetParentJobSubQuery(WorkflowDescriptor descriptor, SchemaGuidColumn foreignKeyColumnToParentJob, bool isNotInQuery)
		{
			var subQuery = new ZDBOnlySubQuery(descriptor.WorkflowProviderType, foreignKeyColumnToParentJob, isNotInQuery);
			var parentTablePrefix = BusinessObjectFactory.GetTableCodeFromType(descriptor.WorkflowProviderType);

			if (types.TryGetValue(parentTablePrefix, out var objectHandle))
			{
				var loadStrategy = GetProcessTaskLoadStrategyIfApplicable(objectHandle);

				if (loadStrategy != null)
				{
					loadStrategy.AddAdditionalParentFilters(descriptor, subQuery);
				}
			}

			AddAdditionalParentFilters(descriptor, subQuery);

			return subQuery;
		}

		protected virtual void AddAdditionalParentFilters(WorkflowDescriptor descriptor, ZDBOnlySubQuery subQuery)
		{
		}

		readonly ImmutableDictionary<string, ObjectHandle> types;

		IProcessTaskLoadStrategy GetProcessTaskLoadStrategyIfApplicable(ObjectHandle objectHandle)
		{
			return (typeof(IProcessTaskLoadStrategy).IsAssignableFrom(objectHandle.GetObjectType()))
				? (IProcessTaskLoadStrategy)objectHandle.GetObject()
				: null;
		}
	}
}
