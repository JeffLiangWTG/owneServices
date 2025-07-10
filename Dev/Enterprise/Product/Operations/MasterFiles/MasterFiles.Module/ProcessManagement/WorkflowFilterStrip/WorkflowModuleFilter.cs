using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	#region Enums

	public enum WorkflowModuleFilterTypes
	{
		MilestoneDate,
		MilestoneNext,
		MilestoneLastCompleted
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Consumed here, wrapped in nameof()")]
	public enum DatesToFilterTypes
	{
		LST,
		ALL,
		ACT,
		EST,
		OES
	}

	#endregion

	public class WorkflowModuleFilter : ModuleDateFilter
	{
		#region Construction

		public WorkflowModuleFilter(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType)
			: this(description, bizObjType, filterType, ZString.Empty)
		{
		}

		public WorkflowModuleFilter(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType, ZString jobType)
			: base(description, delegate
			{ return new ZQuery(); })
		{
			if (bizObjType == null)
			{
				throw new ArgumentNullException(nameof(bizObjType));
			}

			businessObjectType = bizObjType;
			this.jobType = jobType;
			this.filterType = filterType;
			this.useDatesToFilter = filterType == WorkflowModuleFilterTypes.MilestoneDate;
		}

		protected readonly Type businessObjectType;
		readonly ZString jobType;
		readonly WorkflowModuleFilterTypes filterType;

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
						Validation.ValidatePropertySearch();
					}
					MilestoneEventInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString fMilestoneEvent;

		public ZPropertyInfo MilestoneEventInfo
		{
			get { return GetZPropertyInfo(nameof(MilestoneEvent)); }
		}

		#endregion

		#region DatesToFilter

		public bool UseDatesToFilter
		{
			get
			{
				return useDatesToFilter;
			}
		}
		readonly bool useDatesToFilter;

		[List("DatesToFilterList")]
		[MaxLength(3)]
		public ZString DatesToFilter
		{
			get { return datesToFilter; }
			set
			{
				if (datesToFilter != value && UseDatesToFilter)
				{
					CheckMaximumLength(DatesToFilterInfo, value);
					datesToFilter = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidatePropertySearch();
					}
					DatesToFilterInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString datesToFilter;

		public ZPropertyInfo DatesToFilterInfo
		{
			get { return GetZPropertyInfo(nameof(DatesToFilter)); }
		}

		#endregion

		#region Event Reference

		[MaxLength(256)]
		public ZString EventReference
		{
			get { return eventReference; }
			set
			{
				if (EventReference != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(EventReferenceInfo, ref eventReference, value);
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
		[List("EventReferenceComparisonOptions")]
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
					if (!EventReferenceComparisonOptions.ContainsCode(value))
					{
						eventReferenceComparisonOption = "?";
					}
					else
					{
						eventReferenceComparisonOption = value;
					}
					EventReferenceComparisonOptionInfo.RefreshBinding();

					if (IsBlankComparisonOperation)
					{
						EventReference = ZString.Empty;
					}

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
				var eventReferenceSqlComparisonOperator = ModuleTextFilter.GetSqlComparisonOperator(EventReferenceComparisonOption, SQLComparisonOperator.StartsWith);
				return eventReferenceSqlComparisonOperator == SpecialComparisonOperator.IsBlank ||
					eventReferenceSqlComparisonOperator == SpecialComparisonOperator.IsNotBlank;
			}
		}

		protected ZString GetComparisonOperatorDefault()
		{
			const string defaultCode = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Default;

			ZString result;
			if (EventReferenceComparisonOptions.DefaultCode != null && EventReferenceComparisonOptions.ContainsCode(EventReferenceComparisonOptions.DefaultCode))
			{
				result = EventReferenceComparisonOptions.DefaultCode;
			}
			else if (EventReferenceComparisonOptions.ContainsCode(defaultCode) || EventReferenceComparisonOptions.Count == 0)
			{
				result = defaultCode;
			}
			else
			{
				result = EventReferenceComparisonOptions[0].GetMultilingualCode();
			}
			return result;
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList MilestoneEventTypes => milestoneEventTypes ?? (milestoneEventTypes = new MilestoneEventTypeList(jobType));
		CodeDescriptionPairList milestoneEventTypes;

		public CodeDescriptionPairList DatesToFilterList
		{
			get
			{
				return datesToFilterList ?? (datesToFilterList =
					new CodeDescriptionPairList
					{
						new CodeDescriptionPair(nameof(DatesToFilterTypes.LST), Res.GetString("914a5a5d-51b5-4147-a5ea-24346328aadb", "Actual date first, if empty then Estimated (default)")),
						new CodeDescriptionPair(nameof(DatesToFilterTypes.ALL), Res.GetString("d7640ed5-32c0-437a-a78e-937c46069819", "Actual and Estimated dates")),
						new CodeDescriptionPair(nameof(DatesToFilterTypes.ACT), Res.GetString("b1983762-82bc-4d58-a7fb-15661cd4b821", "Actual date")),
						new CodeDescriptionPair(nameof(DatesToFilterTypes.EST), Res.GetString("9c9239ee-9540-47a1-887b-d35a6de5337b", "Estimated date")),
						new CodeDescriptionPair(nameof(DatesToFilterTypes.OES), Res.GetString("bd8e27e7-f04f-421a-b7cb-699cbbfa28e8", "Original estimated date"))
					});
			}
		}

		CodeDescriptionPairList datesToFilterList;

		public CodeDescriptionPairList EventReferenceComparisonOptions
		{
			get
			{
				if (eventReferenceComparisonOptions == null)
				{
					eventReferenceComparisonOptions = new CodeDescriptionPairList();
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
					eventReferenceComparisonOptions.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
				}
				return eventReferenceComparisonOptions;
			}
		}
		CodeDescriptionPairList eventReferenceComparisonOptions;

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

		#region Query

		protected override ZQuery GetQuery()
		{
			return !IsEmpty
				? WorkflowModuleFilterQueryBuilder.BuildQuery(businessObjectType, GetMilestoneQuery(), RelatedParentSubQueries)
				: new ZQuery();
		}

		internal protected ZDBOnlySubQuery[] RelatedParentSubQueries
		{
			get { return relatedParentSubQueries; }
			set
			{
				if (RelatedParentSubQueries != value)
				{
					InvalidateCachedQuery();
				}

				relatedParentSubQueries = value;
			}
		}

		ZDBOnlySubQuery[] relatedParentSubQueries;

		protected virtual ZDBOnlySubQuery GetMilestoneQuery()
		{
			ZDBOnlySubQuery resultSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
			resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);

			var eventReferenceSqlComparisonOperator = ModuleTextFilter.GetSqlComparisonOperator(EventReferenceComparisonOption, SQLComparisonOperator.StartsWith);
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

			if (Globals.IsWeb)
			{
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_IsPublished, true);
			}

			if (filterType == WorkflowModuleFilterTypes.MilestoneDate)
			{
				resultSubQuery = GetMilestoneDateAndMilestoneTypeFilter(FromDate, ToDate, resultSubQuery);
			}
			else if (filterType == WorkflowModuleFilterTypes.MilestoneNext)
			{
				resultSubQuery = GetNextMilestoneAndMilestoneTypeFilter(FromDate, ToDate, resultSubQuery);
			}
			else if (filterType == WorkflowModuleFilterTypes.MilestoneLastCompleted)
			{
				resultSubQuery = GetLastCompletedMilestoneAndMilestoneTypeFilter(FromDate, ToDate, resultSubQuery);
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

		#region MilestoneDate query

		ZDBOnlySubQuery GetMilestoneDateAndMilestoneTypeFilter(ZDateTime fromDate, ZDateTime toDate, ZDBOnlySubQuery resultSubQuery)
		{
			resultSubQuery = GetMilestoneDateFilter(fromDate, toDate, resultSubQuery);

			if (!MilestoneEvent.IsEmpty)
			{
				resultSubQuery = GetMilestoneTypeFilter(resultSubQuery);
			}
			return resultSubQuery;
		}

		ZDBOnlySubQuery GetMilestoneDateFilter(ZDateTime fromDate, ZDateTime toDate, ZDBOnlySubQuery processTaskSubQuery)
		{
			ZDateTime from = fromDate;
			ZDateTime to = ZDateTime.Empty;

			if (toDate.IsValid)
			{
				to = toDate;
			}

			if (!fromDate.IsValid)
			{
				if (this.GetComparisonOperator() == DateComparisonOperator.HasNoDateEntered)
				{
					processTaskSubQuery.AddToFilter(GetMilestoneDateInternalQuery(SQLComparisonOperator.Equal, from), JoinCondition.And);
				}
				if (this.GetComparisonOperator() == DateComparisonOperator.HasDateEntered)
				{
					processTaskSubQuery.AddToFilter(GetMilestoneDateInternalQuery(SQLComparisonOperator.NotEqual, from), JoinCondition.And);
				}
			}

			if (string.Compare(DatesToFilter, nameof(DatesToFilterTypes.ALL), StringComparison.OrdinalIgnoreCase) == 0 && fromDate.IsValid && toDate.IsValid)
			{
				ZQuery subQuery = new ZQuery();
				var fromUtc = ToUtc(ref from);
				var toUtc = ToUtc(ref to);

				subQuery.AddToFilter(GetMilestoneDateInternalQuery(ProcessTasksSchema.P9_ActualDate, from, to), JoinCondition.Or);
				subQuery.AddToFilter(GetMilestoneDateInternalQuery(ProcessTasksSchema.P9_ScheduledDateUtc, fromUtc, toUtc), JoinCondition.Or);
				processTaskSubQuery.AddToFilter(subQuery, JoinCondition.And);
			}
			else
			{
				if (fromDate.IsValid)
				{
					processTaskSubQuery.AddToFilter(GetMilestoneDateInternalQuery(SQLComparisonOperator.GreaterThanOrEqualTo, from), JoinCondition.And);
				}
				if (toDate.IsValid)
				{
					SQLComparisonOperator comparisonOperator = SQLComparisonOperator.LessThanOrEqualTo;

					processTaskSubQuery.AddToFilter(GetMilestoneDateInternalQuery(comparisonOperator, to), JoinCondition.And);
				}
			}

			return processTaskSubQuery;
		}

		ZQuery GetMilestoneDateInternalQuery(SchemaColumn column, ZDateTime from, ZDateTime to)
		{
			ZQuery query = new ZQuery();

			SQLComparisonOperator comparisonOperator = SQLComparisonOperator.LessThanOrEqualTo;

			query.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.GreaterThanOrEqualTo, from);
			query.AddToFilter(JoinCondition.And, column, comparisonOperator, to);

			return query;
		}

		ZQuery GetMilestoneDateInternalQuery(SQLComparisonOperator comparisonOperator, ZDateTime date)
		{
			var utcDate = ToUtc(ref date);
			if (string.Compare(DatesToFilter, nameof(DatesToFilterTypes.OES), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ZQuery(ProcessTasksSchema.P9_OriginalScheduledDateUtc, comparisonOperator, utcDate);
			}

			if (string.Compare(DatesToFilter, nameof(DatesToFilterTypes.EST), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ZQuery(ProcessTasksSchema.P9_ScheduledDateUtc, comparisonOperator, utcDate);
			}

			if (string.Compare(DatesToFilter, nameof(DatesToFilterTypes.ACT), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ZQuery(ProcessTasksSchema.P9_ActualDate, comparisonOperator, date);
			}

			ZQuery query = new ZQuery();

			if (string.Compare(DatesToFilter, nameof(DatesToFilterTypes.ALL), StringComparison.OrdinalIgnoreCase) == 0)
			{
				query.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_ActualDate, comparisonOperator, date);

				ZQuery subQuery = new ZQuery();
				subQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_ScheduledDateUtc, comparisonOperator, utcDate);
				query.AddToFilter(subQuery, JoinCondition.Or);
				return query;
			}

			if (DatesToFilter.IsEmpty || string.Compare(DatesToFilter, nameof(DatesToFilterTypes.LST), StringComparison.OrdinalIgnoreCase) == 0)
			{
				query.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_ActualDate, comparisonOperator, date);

				ZQuery subQuery = new ZQuery();
				subQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ScheduledDateUtc, comparisonOperator, utcDate);
				subQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, ZDateTime.Empty);

				query.AddToFilter(subQuery, JoinCondition.Or);
			}

			return query;
		}

		static ZDateTime ToUtc(ref ZDateTime date)
		{
			return date.IsValid ? new ZDateTime(EnvProxy.Instance.Time.GetUtcFromLocalTime(date.ToDateTime())) : date;
		}

		ZDBOnlySubQuery GetMilestoneTypeFilter(ZDBOnlySubQuery processTaskSubQuery)
		{
			if (!MilestoneEvent.IsEmpty)
			{
				processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_SE_NKMilestoneEvent, SQLComparisonOperator.Equal, MilestoneEvent);
			}
			return processTaskSubQuery;
		}

		#endregion

		#region GetNextMilestoneFilter Query

		ZDBOnlySubQuery GetNextMilestoneAndMilestoneTypeFilter(ZDateTime fromDate, ZDateTime toDate, ZDBOnlySubQuery resultSubQuery)
		{
			if ((fromDate.IsValid) || (toDate.IsValid))
			{
				resultSubQuery = GetNextMilestoneFilter(FromDate, ToDate, resultSubQuery);
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.NextToBeCompletedStatusCode);
				if (!MilestoneEvent.IsEmpty)
				{
					resultSubQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, MilestoneEvent);
				}
			}
			else if (!MilestoneEvent.IsEmpty)
			{
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.NextToBeCompletedStatusCode);
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, MilestoneEvent);
			}

			return resultSubQuery;
		}

		ZDBOnlySubQuery GetNextMilestoneFilter(ZDateTime fromDate, ZDateTime toDate, ZDBOnlySubQuery processTaskSubQuery)
		{
			ZDateTime from = fromDate;
			ZDateTime to = ZDateTime.Empty;
			if (toDate.IsValid)
			{
				to = toDate;
			}

			processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, null);

			if (fromDate.IsValid)
			{
				processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.GreaterThanOrEqualTo, from.ToUniversalBranchTime());
			}
			if (toDate.IsValid)
			{
				processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.LessThanOrEqualTo, to.ToUniversalBranchTime());
			}

			return processTaskSubQuery;
		}

		#endregion

		#region GetLastCompletedMilestoneDateFilter Query

		ZDBOnlySubQuery GetLastCompletedMilestoneAndMilestoneTypeFilter(ZDateTime fromDate, ZDateTime toDate, ZDBOnlySubQuery resultSubQuery)
		{
			if (PropertySearch == HasNoDateEntered)
			{
				resultSubQuery.AddToFilter(ZQuery.NoResultQuery);
			}
			else if ((fromDate.IsValid) || (toDate.IsValid))
			{
				resultSubQuery = GetLastCompletedMilestoneDateFilter(FromDate, ToDate, resultSubQuery);
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.LastCompletedStatusCode);
				if (!MilestoneEvent.IsEmpty)
				{
					resultSubQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, MilestoneEvent);
				}
			}
			else if (!MilestoneEvent.IsEmpty)
			{
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.LastCompletedStatusCode);
				resultSubQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, MilestoneEvent);
			}

			return resultSubQuery;
		}

		ZDBOnlySubQuery GetLastCompletedMilestoneDateFilter(ZDateTime fromDate, ZDateTime toDate, ZDBOnlySubQuery processTaskSubQuery)
		{
			ZDateTime from = fromDate;
			ZDateTime to = ZDateTime.Empty;
			if (toDate.IsValid)
			{
				to = toDate;
			}

			if (fromDate.IsValid)
			{
				processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.GreaterThanOrEqualTo, from);
			}
			if (toDate.IsValid)
			{
				processTaskSubQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.LessThanOrEqualTo, to);
			}

			return processTaskSubQuery;
		}

		#endregion

		#endregion

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "MilestoneEvent")
			{
				MilestoneEvent = reader.ReadElementString("MilestoneEvent");
			}
			if (reader.Name == "DatesToFilter")
			{
				DatesToFilter = reader.ReadElementString("DatesToFilter");
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
			writer.WriteElementString("DatesToFilter", DatesToFilter);

			writer.WriteElementString("EventReference", EventReference);
			writer.WriteElementString("EventReferenceComparisonOption", EventReferenceComparisonOption);
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);

			if (filterToCopyFrom is WorkflowModuleFilter filter)
			{
				MilestoneEvent = filter.MilestoneEvent;
				DatesToFilter = filter.DatesToFilter;
				EventReference = filter.EventReference;
				EventReferenceComparisonOption = filter.EventReferenceComparisonOption;
			}
		}

		#endregion
	}
}
