using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public static class TriggerProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static ICollection<(IBaseTrigger trigger, IBusiness job)> LoadAllMilestonesAndLineTriggersForEvent(IStmALogParent logParent, IStmALog log, ZString lineTriggerType)
		{
			var eventTypeCode = log.SL_SE_NKEvent;
			var eventType = Events.All[eventTypeCode];

			if (eventType != null && log.SL_Parent.IsValid)
			{
				return LoadMilestonesAndTriggers(new ProcessTaskTriggerQueryStrategy(lineTriggerType), log.Factory, logParent, eventTypeCode);
			}

			return Array.Empty<(IBaseTrigger trigger, IBusiness job)>();
		}

		internal static (IBaseTrigger trigger, IBusiness job)[] LoadAllMilestonesAndTriggersForEvent(IStmALogParent logParent, IStmALog log, ZDateTimeOffset eventTime)
		{
			var eventTypeCode = log.SL_SE_NKEvent;

			if (Events.All[eventTypeCode] == null || !log.SL_Parent.IsValid)
			{
				return Array.Empty<(IBaseTrigger trigger, IBusiness job)>();
			}

			var factory = log.Factory;
			var triggers = LoadMilestonesAndTriggers(new ProcessTaskTriggerQueryStrategy(), factory, logParent, eventTypeCode).Where(t =>
			{
				var conditionsMet = t.trigger.AreTriggerConditionsMet(log, t.job);

				if (!conditionsMet && eventTime.IsEmpty)
				{
					conditionsMet = WorkflowTriggerEventCache.WasTriggerFiredByLog(factory, t.trigger.Identifier, log.Identifier);
				}

				return conditionsMet;
			});

			var universalTriggers = GetUniversalTriggersForEvent(factory, logParent, eventTypeCode, log, eventTime);
			return triggers.Concat(universalTriggers).ToArray();
		}

		static void AddFetchHintsForTriggeringEvent(BusinessObjectFactory factory, IStmALogParent logParent, string eventCode)
		{
			if (logParent is BusinessObject bizo && bizo.IsInDatabase && new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects(logParent, eventCode, out ZQuery taskHint))
			{
				factory.AddFetchHint(ProcessTasksSchema.Instance, taskHint);
			}

			if (new ProcessTemplateTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects(logParent, eventCode, out ZQuery templateHint))
			{
				factory.AddFetchHint(ProcessTemplateTriggerSchema.Instance, templateHint);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		static ICollection<(IBaseTrigger trigger, IBusiness job)> LoadMilestonesAndTriggers(ProcessTaskTriggerQueryStrategy strategy, BusinessObjectFactory factory, IStmALogParent logParent, ZString eventTypeCode)
		{
			if (strategy.TryGetQueryForAllTriggersIncludingThoseOnParentObjects(logParent, eventTypeCode, out ZQuery processTasksQuery))
			{
				var milestonesAndTriggers = factory.Load<ProcessTask>(processTasksQuery).Select(trigger => ((IBaseTrigger)trigger, (IBusiness)trigger.GetJob())).Split(tuple => tuple.Item2 == null);

				if (milestonesAndTriggers.MatchingSet.Any())
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($@"The following process tasks were loaded for logParent, but with a null parent reference. logParent (PK: {logParent.LogsParentPK}, TableName: {logParent.LogsParentTableName}, IsDeleted: {logParent.IsDeleted}).
{string.Join("\n", milestonesAndTriggers.MatchingSet.Select(s => (ProcessTask)s.Item1).Select(
	item => FormattableString.Invariant($"ProcessTask(PK: {item.PK}, Type: {item.GetType().Name}, Description: {item.P9_Description}, Condition: {item.P9_TriggerCondition}, ActualDate: {item.P9_ActualDateUtc.ToBestReadableDateTimeString()}, Task Type: {item.P9_Type}, IsDeleted: {item.IsDeleted}, IsDeleting: {item.IsDeleting}, Parent Table Code: {item.P9_ParentTableCode}, Parent ID: {item.P9_ParentID}). ProcessTasksQuery String: {processTasksQuery.FilterString}. Factory RefreshEnable: {factory.RefreshEnabled}")))}"));
				}

				return milestonesAndTriggers.NonMatchingSet.ToList();
			}
			else
			{
				return Array.Empty<(IBaseTrigger, IBusiness)>();
			}
		}

		static IEnumerable<(IBaseTrigger, IBusiness)> GetUniversalTriggersForEvent(BusinessObjectFactory factory, IStmALogParent logParent, ZString eventTypeCode, IStmALog log, ZDateTimeOffset eventTime)
		{
			var templateTriggerStrategy = new ProcessTemplateTriggerQueryStrategy();
			if (templateTriggerStrategy.TryGetQueryForAllTriggersIncludingThoseOnParentObjects(logParent, eventTypeCode, out ZQuery processTemplateTriggersQuery))
			{
				var templateTriggers = factory.Load<IUniversalTemplateTrigger>(processTemplateTriggersQuery);
				var jobTriggerList = new List<IWorkflowTrigger>(templateTriggers.Length);

				foreach (var trigger in templateTriggers)
				{
					if (templateTriggerStrategy.TemplatePKToMatchingWorkflowProviderMap.TryGetValue(trigger.SourceTemplatePK, out IWorkflowProvider parent) && parent is IBusiness bizo)
					{
						if (eventTime.IsEmpty)
						{
							var triggerLink = trigger.GetOrCreateJobVersionOfTrigger(bizo, createIfNotFound: false);
							if (triggerLink == null)
							{
								continue;
							}
						}

						if (trigger.AreConditionsMet(bizo, log))
						{
							yield return (trigger.GetOrCreateJobVersionOfTrigger(bizo), bizo);
						}
					}
				}
			}
		}

		public static GlbBranch GetBranchForTemporaryUserContext(IBaseTrigger trigger, BusinessObject parent, Lazy<GlbStaff> getUser)
		{
			return GetJobHeaderBranch(trigger, parent)
				?? GetTriggerUserBranch(trigger, getUser)
				?? FindFirstActiveBranch((GlbCompany)trigger.GetCompany());
		}

		static GlbBranch FindFirstActiveBranch(GlbCompany company)
		{
			GlbBranch result = null;
			if (company != null)
			{
				ZQuery query = new ZQuery(GlbBranchSchema.GB_GC, company.PK);
				query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				query.OrderBy = GlbBranchSchema.Constants.GB_Code;
				result = company.Factory.LoadTop1<GlbBranch>(query);
			}
			return result;
		}

		static GlbBranch GetJobHeaderBranch(IBaseTrigger trigger, BusinessObject parent)
		{
			GlbBranch result = null;
			if (parent is IJobHeaderParent)
			{
				var query = new ZQuery(JobHeaderSchema.JH_ParentID, parent.PK);
				query.AddToFilter(JobHeaderSchema.JH_GC, trigger.CompanyPK);
				query.AddToFilter(JobHeaderSchema.JH_IsActive, true);
				result = trigger.Factory.LoadTop1<JobHeader>(query)?.Branch ?? (parent as IBranchProvider)?.Branch;
			}
			return result;
		}

		static GlbBranch GetTriggerUserBranch(IBaseTrigger trigger, Lazy<GlbStaff> getUser)
		{
			GlbBranch result = null;
			var user = getUser.Value;
			if (user != null && !user.GS_IsSystemAccount)
			{
				var company = (GlbCompany)trigger.GetCompany();
				if (company == null || company.Branches.Contains(user.HomeBranch))
				{
					result = user.HomeBranch;
				}
			}
			return result;
		}

		#region Proxy for ObjectFactory

		[CodeAlive("Used via ObjectFactory")]
		class ProviderProxy : ITriggerProvider
		{
			[DebuggerStepThrough]
			bool ITriggerProvider.TryGetQueryForAllTriggersIncludingThoseOnParentObjects(IBusiness logParent, string eventCode, out ZQuery query)
			{
				return new ProcessTaskTriggerQueryStrategy().TryGetQueryForAllTriggersIncludingThoseOnParentObjects((IStmALogParent)logParent, eventCode, out query);
			}

			[DebuggerStepThrough]
			void ITriggerProvider.AddFetchHintsForTriggeringEvent(BusinessObjectFactory factory, IBusiness logParent, string eventCode)
			{
				TriggerProvider.AddFetchHintsForTriggeringEvent(factory, (IStmALogParent)logParent, eventCode);
			}

			[DebuggerStepThrough]
			ICollection<(IBaseTrigger, IBusiness)> ITriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(IBusiness logParent, IStmALog log, string lineTriggerType)
			{
				return TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent((IStmALogParent)logParent, log, lineTriggerType);
			}

			[DebuggerStepThrough]
			ICollection<(IBaseTrigger, IBusiness)> ITriggerProvider.LoadAllMilestonesAndTriggersForEvent(IBusiness logParent, IStmALog log, ZDateTimeOffset eventTime)
			{
				return TriggerProvider.LoadAllMilestonesAndTriggersForEvent((IStmALogParent)logParent, log, eventTime);
			}
		}

		#endregion
	}
}
