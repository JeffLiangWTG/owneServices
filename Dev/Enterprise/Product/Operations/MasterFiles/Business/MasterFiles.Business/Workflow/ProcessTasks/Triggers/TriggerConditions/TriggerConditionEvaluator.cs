using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public delegate (BusinessObject, IWorkflowDescriptor) GetTriggerContext();

	public static class TriggerConditionEvaluator
	{
		const bool FindExistingLogForRename_Default = false;

		public static bool AreTriggerConditionsMet(this IWorkflowProvider parent, IUniversalTemplateTrigger trigger, ZString triggerConditionValue, IStmALog log)
		{
			var job = (BusinessObject)parent;
			return AreTriggerConditionsMetCore(job, trigger, new TriggerEventLog(log, trigger, parent.WorkflowItems.Parent), () => (job, WorkflowDescriptors.Instance.TryGetValueSafe(parent.WorkflowType)), log, triggerConditionValue, FindExistingLogForRename_Default);
		}

		public static bool AreTriggerConditionsMet(IMilestoneDateDefaultable trigger, BusinessObject job, IStmALog log, ZString? triggerConditionValue = null)
		{
			if (log == null)
			{
				return trigger.TriggerCondition.IsEmpty;
			}
			return AreTriggerConditionsMetCore(job, trigger, new TriggerEventLog(log, trigger), () => (job, trigger.Descriptor), log, triggerConditionValue ?? trigger.TriggerConditionValue, FindExistingLogForRename_Default);
		}

		public static bool AreTriggerConditionsMet(IBaseTrigger trigger, IStmALog log, BusinessObject job, ZString? triggerConditionValue = null, bool findExistingLogForRename = FindExistingLogForRename_Default)
		{
			if (log == null)
			{
				return trigger.TriggerCondition.IsEmpty;
			}
			return AreTriggerConditionsMetCore(job, trigger, new TriggerEventLog(log, trigger, job), GetContextFromTrigger(trigger, job), log, triggerConditionValue ?? trigger.TriggerConditions_ForBinding.TriggerConditionValue, findExistingLogForRename);
		}

		public static bool AreTriggerConditionsMet(IBaseTrigger trigger, IQueuedLog log, BusinessObject job, bool findExistingLogForRename = FindExistingLogForRename_Default)
		{
			if (log == null)
			{
				return trigger.TriggerCondition.IsEmpty;
			}

			return AreTriggerConditionsMetCore(job, trigger, new TriggerEventLog(trigger.Factory, log, trigger, job), GetContextFromTrigger(trigger, job), null, trigger.TriggerConditions_ForBinding.TriggerConditionValue, findExistingLogForRename);
		}

		public static bool AreTriggerConditionsMet(BusinessObject job, IBaseTrigger trigger, IStmChangeLog log)
		{
			if (log == null)
			{
				return trigger.TriggerCondition.IsEmpty;
			}

			return AreTriggerConditionsMetCore(job, trigger, new TriggerEventLog(log), GetContextFromTrigger(trigger, job), null, trigger.TriggerConditions_ForBinding.TriggerConditionValue, findExistingLogForRename: false);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison")]
		static bool AreTriggerConditionsMetCore(BusinessObject job, ITriggerConditions trigger, TriggerEventLog eventLog, GetTriggerContext getContext, IStmALog log, ZString triggerConditionValue, bool findExistingLogForRename)
		{
			var triggerCondition = trigger.TriggerCondition;
			var result = triggerCondition.IsEmpty;

			if (!result)
			{
				if (TriggerConditionsViewModel.IsEventReferenceTriggerCondition(triggerCondition))
				{
					result = DoesEventReferenceConditionMatch(job, trigger, eventLog, getContext, log, triggerConditionValue, findExistingLogForRename);
				}
				else if (triggerCondition == ExceptionActionConditionList.Codes.ExceptionType)
				{
					var reference = WorkflowDataRegistry.Instance.ExceptionEXRReference.Value ?
						string.Format(CultureInfo.InvariantCulture, "|TYP={0}", triggerConditionValue) :
						string.Format(CultureInfo.InvariantCulture, (NoResString)"Type:[{0}]", triggerConditionValue);

					result = eventLog.Reference.Contains(reference, StringComparison.InvariantCultureIgnoreCase);
				}
				else if (triggerCondition == ExceptionActionConditionList.Codes.EventType)
				{
					var reference = WorkflowDataRegistry.Instance.ExceptionEXRReference.Value ?
						string.Format(CultureInfo.InvariantCulture, "|EVT={0}", triggerConditionValue) :
						string.Format(CultureInfo.InvariantCulture, (NoResString)"Event:[{0}]", triggerConditionValue);

					result = eventLog.Reference.Contains(reference, StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (result && trigger.Cascading && !trigger.CascadingContext.IsEmpty)
			{
				var (parent, descriptor) = getContext();

				result = parent != null && eventLog.LogParent != null && descriptor != null &&
					((WorkflowDescriptor)descriptor).IsRelatedEntityInContext(parent, eventLog.LogParent, trigger.CascadingContext);
			}

			return result;
		}

		static GetTriggerContext GetContextFromTrigger(IBaseTrigger trigger, BusinessObject job) => () => (job, trigger.GetWorkflowDescriptor());

#if DEBUG
		public
#endif
		static bool DoesEventReferenceConditionMatch(BusinessObject parent, ITriggerConditions triggerConditions, TriggerEventLog eventLog, GetTriggerContext getContext, IStmALog log, ZString? triggerConditionValueOverride = null, bool findExistingLogForRename = FindExistingLogForRename_Default)
		{
			var reference = eventLog.ReferenceForBinding;
			if (eventLog.IsEstimate)
			{
				reference = TriggerConditionRegexProvider.OldDateRegex.Replace(eventLog.ReferenceForBinding, "").Trim();
			}
			else if (eventLog.EventCode == Events.EstimatedDateChangedCode && findExistingLogForRename)
			{
				reference = TriggerConditionRegexProvider.DateRegex.Replace(eventLog.ReferenceForBinding, "").Trim();
			}

			var triggerCondition = triggerConditions.TriggerCondition;
			var triggerConditionValue = triggerConditionValueOverride ?? triggerConditions.TriggerConditionValue;
			if (triggerCondition == EventReferenceConditionList.Codes.ConditionWithMacros)
			{
				return IsConditionWithMacroMet(parent.Factory, triggerConditions, triggerConditionValue, eventLog, getContext, log);
			}
			else if (triggerCondition == EventReferenceConditionList.Codes.UserDefined)
			{
				var (job, descriptor) = getContext();
				var dataContext = descriptor.GetUDFMacroDataContext(triggerConditions, parent).Append(eventLog.EventDataModel.Log as BusinessObject).ToArray();
				return ObjectFactory.Get<IUserDefinedConditionEvaluator>().IsTextMacroConditionMet(triggerConditionValue, parent, triggerConditions.Job, false, dataContext);
			}
			else
			{
				return DoesReferenceConditionMatch((WorkflowDescriptor)triggerConditions.Descriptor, parent, reference, triggerCondition, triggerConditionValue);
			}
		}

		public static bool DoesReferenceConditionMatch(WorkflowDescriptor descriptor, BusinessObject parent, ZString reference, ZString triggerCondition, ZString triggerConditionValue)
		{
			switch (triggerCondition)
			{
				case "":
					return true;

				case EventReferenceConditionList.Codes.EventReference:
					return triggerConditionValue.EqualsIgnoringCase(reference);

				case EventReferenceConditionList.Codes.EventReferenceWithWildcards:
				case EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions:
					return triggerConditionValue.EqualsIgnoringCase(reference) ||
					(!triggerConditionValue.IsEmpty && GetTemplateRegex(parent.Factory, triggerCondition, triggerConditionValue).IsMatch(reference));

				case EventReferenceConditionList.Codes.EventReferenceParameters:
					return DoEventReferenceParametersMatch(descriptor, parent, triggerConditionValue, reference);

				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unknown condition code {triggerCondition}"));
			}
		}

		public static Regex GetTemplateRegex(BusinessObjectFactory factory, string condition, string expr)
		{
			var cacheKey = FormattableString.Invariant($"RGX{condition}{expr}");
			return factory.GetCachedValue(cacheKey,
			() =>
			{
				if (condition == EventReferenceConditionList.Codes.EventReferenceWithWildcards)
				{
					return TriggerConditionRegexProvider.GetEventReferenceWithWildcardsRegex(expr);
				}
				else
				{
					return new Regex(string.Concat('^', expr, '$'), RegexOptions.IgnoreCase);
				}
			});
		}

		internal static bool DoEventReferenceParametersMatch(WorkflowDescriptor descriptor, BusinessObject parent, ZString triggerConditionValue, ZString reference)
		{
			bool result;
			var conditionsDataContext = GetConditionsDataContext(null, null, null, () => (parent, descriptor));

			if (conditionsDataContext != null)
			{
				var evaluatedCondition = ObjectFactory.Get<IWorkflowMacroValueEvaluator>().GetMacroValue<string>(parent.Factory, conditionsDataContext, MacroStringHelper.WrapWithQuotes(triggerConditionValue));
				result = IsConditionValueMatchReference(evaluatedCondition, reference);
			}
			else
			{
				result = false;
			}

			return result;
		}

		public static bool IsConditionValueMatchReference(string conditionValue, string reference)
		{
			if (conditionValue == null)
			{
				return false;
			}

			var (conditions, other) = MacroStringHelper.GetParameters(conditionValue);
			if (other.Any())
			{
				return false; // Parameters are malformed.
			}

			var eventParametrs = StmALog.GetParametersFromReference(reference);

			Func<KeyValuePair<string, string>, bool> areMet = c =>
			{
				if (c.Key == Constants.EventReferenceReservedParameters.Codes.Reference)
				{
					var eventReferenceFreeText = StmALog.GetFreeTextFromReference(reference);
					return StringComparer.InvariantCultureIgnoreCase.Compare(c.Value, eventReferenceFreeText) == 0;
				}
				else
				{
					string eventParameterValue;
					if (eventParametrs.TryGetValue(c.Key, out eventParameterValue))
					{
						return StringComparer.InvariantCultureIgnoreCase.Compare(c.Value, eventParameterValue) == 0;
					}

					return string.IsNullOrEmpty(c.Value);
				}
			};

			return conditions.All(areMet);
		}

		internal static bool IsConditionWithMacroMet(BusinessObjectFactory factory, ITriggerConditions triggerConditions, ZString triggerConditionValue, TriggerEventLog eventLog, GetTriggerContext getContext, IStmALog log)
		{
			var conditionsDataContext = GetConditionsDataContext(eventLog.TriggerDataModel, eventLog.EventDataModel, eventLog.LogParent, getContext);
			if (conditionsDataContext == null)
			{
				return false;
			}

			bool? result = null;
			var contextDecider = ObjectFactory.Get<IWorkflowMacroContextDecider>();
			var macroEvaluator = ObjectFactory.Get<IWorkflowMacroValueEvaluator>();
			if (WorkflowDataRegistry.Instance.McrDataFieldMapEnhancements.Value)
			{
				using (var macroContext = contextDecider.GetContextForTriggerConditions(conditionsDataContext.TriggerSource, triggerConditions as IBaseTrigger, log))
				{
					result = macroEvaluator.EvaluateBooleanExpression(factory, macroContext, triggerConditionValue);
				}
			}

			if (result == null)
			{
				using (var macroContext = contextDecider.GetDefaultWorkflowMacroContext(factory, conditionsDataContext))
				{
					result = macroEvaluator.EvaluateBooleanExpression(factory, macroContext, triggerConditionValue);
				}
			}

			return result ?? false;
		}

		public static BusinessObjectEventDataModel GetConditionsDataContext(TriggerDataModel triggerData, LogEventDataModel eventData, BusinessObject logParent, GetTriggerContext getContext)
		{
			var (parent, descriptor) = getContext.Invoke();

			if (parent == null)
			{
				ErrorReporter.ReportOnce($"Parent should not be null. LogParent is of type {logParent.GetType().FullName}. EventData Reference = {eventData.Reference}. EventData Soruce = {eventData.Source}.");
				return null;
			}

			var conditionsDataContext = descriptor != null && !(parent is ProcessTaskTemplate)
				? ((WorkflowDescriptor)descriptor).GetEventDataModel(parent)
				: new BusinessObjectEventDataModel(parent);

			if (conditionsDataContext != null)
			{
				conditionsDataContext.Event = eventData;
				conditionsDataContext.Trigger = triggerData;
				conditionsDataContext.Source = logParent;
				conditionsDataContext.TriggerSource = parent;
			}
			return conditionsDataContext;
		}
	}
}
