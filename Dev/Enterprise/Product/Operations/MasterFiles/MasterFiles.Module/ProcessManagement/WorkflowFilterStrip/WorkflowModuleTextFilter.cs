using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowModuleTextFilter : ModuleTextFilter, IWorkflowModuleTextFilter
	{
		#region Construction

		public WorkflowModuleTextFilter(ZString description, GetTextQuery queryDelegate, IList list, Type bizObjType)
			: this(description, queryDelegate, list, bizObjType, ZString.Empty)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public WorkflowModuleTextFilter(ZString description, GetTextQuery queryDelegate, IList list, Type bizObjType, ZString jobType)
			: base(description, queryDelegate, list)
		{
			if (bizObjType == null)
			{
				throw new ArgumentNullException(nameof(bizObjType));
			}

			businessObjectType = bizObjType;
			this.jobType = jobType;

			if (queryDelegate.DynamicInvoke(Property) is ZDBOnlySubQuery)
			{
				getMilestoneCompletedFilter = queryDelegate;
			}
			else
			{
				throw new NotSupportedException("This filter type requires query delegate that returns ZDBOnlySubQuery");
			}
		}

		protected readonly Type businessObjectType;
		protected readonly ZString jobType;
		readonly GetTextQuery getMilestoneCompletedFilter;

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			MilestoneEvent = ZString.Empty;
			EventReference = ZString.Empty;
			EventReferenceComparisonOption = GetComparisonOperatorDefault();
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && MilestoneEvent.IsEmpty && EventReference.IsEmpty && !IsBlankComparisonOperation;

		#endregion

		#region Properties

		#region MilestoneEvent

		[List("MilestoneEventTypes")]
		[MaxLength(3)]
		public ZString MilestoneEvent
		{
			get { return fMilestoneEvent; }
			set
			{
				if (fMilestoneEvent != value)
				{
					CheckMaximumLength(MilestoneEventInfo, value);
					fMilestoneEvent = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty();
					}
					MilestoneEventInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo MilestoneEventInfo
		{
			get { return GetZPropertyInfo(nameof(MilestoneEvent)); }
		}

		ZString fMilestoneEvent;

		#endregion

		#region Event Reference

		[MaxLength(256)]
		public ZString EventReference
		{
			get { return IsBlankComparisonOperation ? ZString.Empty : eventReference; }
			set
			{
				if (EventReference != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(EventReferenceInfo, ref eventReference, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEventReference();
				}
			}
		}
		ZString eventReference;

		public ZPropertyInfo EventReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(EventReference)); }
		}

		protected bool EventReference_ReadOnly
		{
			get { return IsBlankComparisonOperation; }
		}

		[BusinessObjectTestExclude] // we store "?" when setting an invalid value
		[List("ComparisonOperator_List")]
		public ZString EventReferenceComparisonOption
		{
			get
			{
				if (eventReferenceComparisonOption.IsEmpty)
				{
					eventReferenceComparisonOption = GetComparisonOperatorDefault();
				}
				return eventReferenceComparisonOption;
			}
			set
			{
				if (eventReferenceComparisonOption != value)
				{
					if (!ComparisonOperator_List.ContainsCode(value))
					{
						eventReferenceComparisonOption = "?";
					}
					else
					{
						eventReferenceComparisonOption = value;
					}
					EventReferenceComparisonOptionInfo.RefreshBinding();

					InvalidateCachedQuery();
				}
			}
		}
		ZString eventReferenceComparisonOption;

		public ZPropertyInfo EventReferenceComparisonOptionInfo
		{
			get { return GetZPropertyInfo(nameof(EventReferenceComparisonOption)); }
		}

		bool IsBlankComparisonOperation
		{
			get
			{
				var eventReferenceSqlComparisonOperator = GetSqlComparisonOperator(EventReferenceComparisonOption, SQLComparisonOperator.StartsWith);
				return eventReferenceSqlComparisonOperator == SpecialComparisonOperator.IsBlank || eventReferenceSqlComparisonOperator == SpecialComparisonOperator.IsNotBlank;
			}
		}

		#endregion

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			return !IsEmpty
				? WorkflowModuleFilterQueryBuilder.BuildQuery(businessObjectType, GetMilestoneQuery(), RelatedParentSubQueries)
				: new ZQuery();
		}

		internal protected ZDBOnlySubQuery[] RelatedParentSubQueries { get; set; }

		protected virtual ZDBOnlySubQuery GetMilestoneQuery()
		{
			ZDBOnlySubQuery resultSubQuery;
			if (Property.IsValid)
			{
				resultSubQuery = (ZDBOnlySubQuery)(getMilestoneCompletedFilter.DynamicInvoke(Property));
			}
			else
			{
				resultSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
			}

			var eventReferenceSqlComparisonOperator = GetSqlComparisonOperator(EventReferenceComparisonOption, SQLComparisonOperator.StartsWith);
			if (IsBlankComparisonOperation)
			{
				resultSubQuery.AddToFilter(
					ProcessTasksSchema.P9_Notes,
					eventReferenceSqlComparisonOperator == SpecialComparisonOperator.IsBlank
						? SQLComparisonOperator.Equal
						: SQLComparisonOperator.NotEqual,
					null);
			}
			else if (!EventReference.IsEmpty)
			{
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Notes, eventReferenceSqlComparisonOperator, EventReference);
			}

			if (!MilestoneEvent.IsEmpty)
			{
				resultSubQuery = GetMilestoneTypeFilter(resultSubQuery);
			}

			AddParentTableCodeQuery(resultSubQuery);

			return resultSubQuery;
		}

		protected virtual void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
			if (RelatedParentSubQueries == null || RelatedParentSubQueries.Length == 0)
			{
				query.AddToFilter(WorkflowFilterStripsHelper.GetParentTableCodeQuery(businessObjectType), JoinCondition.And);
			}
		}

		ZDBOnlySubQuery GetMilestoneTypeFilter(ZDBOnlySubQuery processTaskSubQuery)
		{
			processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_SE_NKMilestoneEvent, SQLComparisonOperator.Equal, MilestoneEvent);
			return processTaskSubQuery;
		}

		#endregion

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "MilestoneEvent")
			{
				MilestoneEvent = reader.ReadElementString("MilestoneEvent");
			}

			if (reader.Name == "EventReference")
			{
				EventReference = reader.ReadElementString("EventReference");
			}
			if (reader.Name == "EventReferenceComparisonOption")
			{
				EventReferenceComparisonOption = reader.ReadElementString("EventReferenceComparisonOption");
			}
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("MilestoneEvent", MilestoneEvent);

			writer.WriteElementString("EventReference", EventReference);
			writer.WriteElementString("EventReferenceComparisonOption", EventReferenceComparisonOption);
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);

			if (filterToCopyFrom is WorkflowModuleTextFilter filter)
			{
				MilestoneEvent = filter.MilestoneEvent;
				EventReference = filter.EventReference;
				EventReferenceComparisonOption = filter.EventReferenceComparisonOption;
			}
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList MilestoneEventTypes
		{
			get { return new MilestoneEventTypeList(jobType); }
		}

		#endregion

		#region Validation

		public new WorkflowModuleTextFilterValidation Validation => (WorkflowModuleTextFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new WorkflowModuleTextFilterValidation(this);
		}

		public class WorkflowModuleTextFilterValidation : ModuleTextFilterValidation
		{
			public WorkflowModuleTextFilterValidation(WorkflowModuleTextFilter parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly WorkflowModuleTextFilter parent;

			public override void ValidateAll()
			{
				ValidateEventReference();
			}

			public void ValidateEventReference()
			{
				ValidateCalculatedProperty(parent.EventReferenceInfo);
			}

			protected void CheckEventReference()
			{
				if (parent?.FilterBusinessObject != null && parent.FilterBusinessObject.IsInFilterRuleMode && !parent.EventReference.IsEmpty)
				{
					parent.EventReferenceInfo.AddError(Res.GetString("98a3996e-4ba2-4a91-896a-b9ab5ffc7a6d", "This option cannot be used on filter rules for performance reasons."));
				}
			}

			public override Type AutoValidationType => typeof(WorkflowModuleTextFilterValidation);
		}

		#endregion
	}
}
