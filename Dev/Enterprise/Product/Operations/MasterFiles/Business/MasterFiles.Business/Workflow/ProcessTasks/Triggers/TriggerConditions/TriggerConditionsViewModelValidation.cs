using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerConditionsViewModelValidation : ZValidation
	{
		public static string TriggerFiredCountZeroMessage
		{
			get { return Res.GetString("abae69bf-c9d0-4332-88c0-400c3ac772bc", "This trigger has fired too many times and will not fire any more unless this value is increased. It may be part of an infinite loop."); }
		}

		public TriggerConditionsViewModelValidation(TriggerConditionsViewModel parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected TriggerConditionsViewModel Parent { get; }

		public override Type AutoValidationType => typeof(TriggerConditionsViewModel);

		public override void ValidateAll()
		{
			ValidateTriggerEventCode();
			ValidateTriggerFieldName();
			ValidateTriggerCondition();
			ValidateTriggerConditionValue();
			ValidateTriggerFiredCountdown();
		}

		#region TriggerEventCode

		public void ValidateTriggerEventCode()
		{
			ValidateCalculatedProperty(Parent.TriggerEventCodeInfo);
		}

		protected void CheckTriggerEventCode()
		{
			CheckArrivalEventIsUsedOnConsolDischargeLeg();
			CheckValidEventCodeAndChildTriggerActionTypes();
			CheckEventIsActive();
			CheckMilestoneEvent();
			CheckTriggerEvent();
			CheckTriggerFieldOrEventSpecified(Parent.TriggerEventCodeInfo);
		}

		#region Freight Logic

		void CheckArrivalEventIsUsedOnConsolDischargeLeg()
		{
			var eventCode = Parent.TriggerEventCode;

			if (!eventCode.IsEmpty && eventCode != Events.Arrival.Code)
			{
				var templateTrigger = Parent.Trigger as ITemplateTrigger;

				if (templateTrigger != null && templateTrigger.TemplateCondition1 == JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg)
				{
					Parent.TriggerEventCodeInfo.AddError(Res.GetString("af31ca90-7de9-4f33-9b24-3bd4b561af93", "Only the Arrival event is applicable on a Consol Discharge Transport Leg."));
				}
			}
		}

		#endregion

		#region Valid Event

		void CheckValidEventCodeAndChildTriggerActionTypes()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TriggerEventCodeInfo);
			ListValidation.ErrorIfCancelledAndEditable(Parent.TriggerEventCodeInfo);

			foreach (ProcessTaskNotification triggerAction in Parent.Trigger.TriggerActions)
			{
				triggerAction.Validation.ValidatePQ_TriggerType();
			}
		}

		#endregion

		#region Inactive Events

		void CheckEventIsActive()
		{
			var inactiveEvent = Events.InactiveEvents.FirstOrDefault(e => e.Code == Parent.TriggerEventCode);

			if (inactiveEvent != null)
			{
				var jobTrigger = Parent.Trigger as IWorkflowTrigger;

				if (Parent.TriggerEventCodeInfo.HasChanges || jobTrigger == null || !jobTrigger.LastFiredTime.IsValid)
				{
					var errorMessage = Res.GetString("eaae57a3-d747-4e6d-a841-35a05845995b", "This event is inactive and should not be used.");

					int replacedByPosition = inactiveEvent.Description.IndexOf("Replaced by", StringComparison.Ordinal);
					if (replacedByPosition > 0)
					{
						errorMessage = string.Format(CultureInfo.InvariantCulture, "{0} {1}", errorMessage, inactiveEvent.Description.Substring(replacedByPosition));
					}

					Parent.TriggerEventCodeInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region Milestone Event

		void CheckMilestoneEvent()
		{
			if (Parent.Trigger.IsMilestone())
			{
				MandatoryValidation.CheckEntered(Parent.TriggerEventCodeInfo);

				if (HasDuplicateMilestoneEvents())
				{
					Parent.TriggerEventCodeInfo.AddWarning(Res.GetString("2ada6d23-d6e4-42f5-951d-f31bb9c7c702", "There is more than one milestone for this event."));
				}
			}
		}

		bool HasDuplicateMilestoneEvents()
		{
			var milestone = (ProcessTask)Parent.Trigger; // This validation does not support Universal Milestones just yet.

			var query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_Type, Constants.Workflow.MilestoneType);
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, milestone.P9_ParentID);
			query.FetchOnlyFromLocalCache = true;

			foreach (var otherMilestone in milestone.Factory.Load<ProcessTask>(query))
			{
				if (otherMilestone.PK != milestone.PK &&
					otherMilestone.P9_ReferencedID == milestone.P9_ReferencedID &&
					otherMilestone.P9_SE_NKMilestoneEvent == milestone.P9_SE_NKMilestoneEvent &&
					!IsEventReferenceConditionsDifferent(otherMilestone, milestone) &&
					!IsArrivalOrDepartureWithDifferentConditions(otherMilestone, milestone))
				{
					return true;
				}
			}

			return false;
		}

		bool IsEventReferenceConditionsDifferent(ProcessTask milestone1, ProcessTask milestone2)
		{
			var doBothHaveEventReferenceConditions = milestone1.TriggerConditions.HasEventReferenceTriggerCondition && milestone2.TriggerConditions.HasEventReferenceTriggerCondition;
			return doBothHaveEventReferenceConditions && milestone1.P9_TriggerConditionValue != milestone2.P9_TriggerConditionValue;
		}

		bool IsArrivalOrDepartureWithDifferentConditions(ProcessTask milestone1, ProcessTask milestone2)
		{
			var areBothSameArrivalDepartureEvent =
				milestone1.P9_SE_NKMilestoneEvent == Events.Arrival.Code &&
				milestone1.P9_SE_NKMilestoneEvent == Events.Arrival.Code;
			areBothSameArrivalDepartureEvent |=
				milestone2.P9_SE_NKMilestoneEvent == Events.Departure.Code &&
				milestone2.P9_SE_NKMilestoneEvent == Events.Departure.Code;
			var hasDifferentConditions = (milestone1.P9_Condition1 != milestone2.P9_Condition1 || milestone1.P9_Condition2 != milestone2.P9_Condition2);
			return areBothSameArrivalDepartureEvent && hasDifferentConditions;
		}

		#endregion

		#region Trigger Event

		void CheckTriggerEvent()
		{
			if (Parent.Trigger.IsTrigger() && Parent.TriggerEventCode == Events.EditedARecord.Code)
			{
				Parent.TriggerEventCodeInfo.AddWarning(
					Res.GetString("226ab5a5-6380-4fbc-af23-dc548a28d3ce", "An 'Edit' trigger action will only occur when the edit happens on the module's form by a user. A data import or sailing schedule change will not cause the trigger action to occur."));
			}
		}

		#endregion

		#endregion

		#region TriggerFieldName

		public void ValidateTriggerFieldName()
		{
			ValidateCalculatedProperty(Parent.TriggerFieldNameInfo);
		}

		protected virtual void CheckTriggerFieldName()
		{
			var fieldName = Parent.TriggerFieldName;

			if (!fieldName.IsEmpty)
			{
				if (!fieldName.StartsWith(JobConsolTransportSchema.Constants.Prefix + "_", StringComparison.Ordinal))
				{
					var templateTrigger = Parent.Trigger as ITemplateTrigger;

					if (templateTrigger != null && templateTrigger.TemplateCondition1 == JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg)
					{
						Parent.TriggerFieldNameInfo.AddError(Res.GetString("7aad7c05-e7c1-4da6-a9a5-f61de1e07385", "Only Transport Leg Fields are applicable on a Consol Discharge Transport Leg."));
					}
				}

				ListValidation.WarnIfInvalidCode(Parent.TriggerFieldNameInfo);
			}

			PreventTriggerFieldBeingUsedWithEVTContext(Parent.TriggerFieldNameInfo);
			CheckTriggerFieldOrEventSpecified(Parent.TriggerFieldNameInfo);
		}

		#endregion

		#region TriggerCondition

		public void ValidateTriggerCondition()
		{
			ValidateCalculatedProperty(Parent.TriggerConditionInfo);
		}

		protected void CheckTriggerCondition()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TriggerConditionInfo);
		}

		#endregion

		#region TriggerConditionValue

		public void ValidateTriggerConditionValue()
		{
			ValidateCalculatedProperty(Parent.TriggerConditionValueInfo);
		}

		protected void CheckTriggerConditionValue()
		{
			var triggerCondition = Parent.TriggerCondition;
			CheckTriggerConditionValue(Parent.Factory, triggerCondition, Parent.TriggerConditionValueInfo, () => Parent.Trigger.ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix);

			if (!string.IsNullOrEmpty(triggerCondition) && Parent.Trigger.IsMilestone())
			{
				var task = (ProcessTask)Parent.Trigger;
				var reference = task.MilestoneActualDateSetter.GetEventReferenceForNewLog(TriggerConditionTypeConverter.Convert(task.P9_TriggerCondition), task.P9_TriggerConditionValue).Reference;
				var @event = Events.All[task.P9_SE_NKMilestoneEvent];

				foreach (var definition in GetRelatedMilestoneDefinitions(task, @event))
				{
					var duplicateParametersFromChildReference = GetDuplicateParametersFromChildReference(definition, reference);

					if (duplicateParametersFromChildReference.Any())
					{
						var warning = Res.GetString("TriggerConditionValidation|DuplicateKeysInReference", "This trigger condition contains parameters with identical keys ({0}). The Reference of the [{1}] Event created from this property will ignore the duplicate parameters.",
							string.Join(", ", duplicateParametersFromChildReference),
							definition.EventType);
						Parent.TriggerConditionValueInfo.AddWarning(warning);
						break; // Don't bother to continue evaluation.
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "expression.AST needs to be calculated to compile the expression. This'll get refactored in WI00119638.")]
		public static void CheckTriggerConditionValue(BusinessObjectFactory factory, string triggerCondition, ZPropertyInfo info, Func<bool> isTemplate)
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(info); // Because SL_Reference is a varchar, not nvarchar

			var conditionValue = (ZString)info.Value;
			if (triggerCondition == EventReferenceConditionList.Codes.ConditionWithMacros)
			{
				var context = new[] { StandardLibrary(factory), LocationsLibrary(factory) }.CreateContext();

				var expression = conditionValue.ToString().With(context).CreateExpression();
				var ast = expression.AST; // Forces actual compilation of the expression.

				foreach (var error in expression.Errors.Where(e => e.Category == ErrorCategory.Compiletime))
				{
					info.AddError(error.Message);
				}
			}
			else if (triggerCondition == EventReferenceConditionList.Codes.UserDefined)
			{
				MandatoryValidation.CheckEntered(info);
			}
			else if (triggerCondition == EventReferenceConditionList.Codes.EventReferenceParameters)
			{
				void AddIfTemplateError(string warningOrError)
				{
					if (isTemplate())
					{
						info.AddError(warningOrError);
					}
					else
					{
						info.AddWarning(warningOrError);
					}
				}

				var (valid, other) = MacroStringHelper.GetParameters(conditionValue);
				if (other.Any())
				{
					var message = Res.GetString("TriggerConditionValidation|ConditionIsNotValidRFP", "Condition [{0}] for Event Reference with Parameters contains invalid parameters. Parameters should always contain '=' and multiple parameters should be separated by ',' characters. (e.g. LOC=BIL,NAM=VIN).", conditionValue);

					AddIfTemplateError(message);
				}
				if (conditionValue.Contains('|'))
				{
					var message = Res.GetString("TriggerConditionValidation|ConditionShouldNotContainPipe", "Condition [{0}] for Event Reference with Parameters contains invalid parameters. This field should never contain the '|' character.", conditionValue);

					AddIfTemplateError(message);
				}
				var wrongKeySize = valid.Where(t => t.Key.Length < 1 || t.Key.Length > 3);

				if (wrongKeySize.Any())
				{
					var message = Res.GetString("TriggerConditionValidation|WrongKeyLength", "Condition [{0}] for Event Reference with Parameters contains invalid parameters. Parameter keys (e.g. KEY=VALUE) should be one to three characters long.", conditionValue);
					AddIfTemplateError(message);
				}
			}
			else if (triggerCondition == EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions)
			{
				try
				{
					Regex.Match("", conditionValue);
				}
				catch (ArgumentException ex)
				{
					var message = Res.GetString("TriggerConditionValidation|ConditionIsNotValidRFR", "\"{0}\" is not a valid regular expression, {1}", conditionValue, ex.Message);
					info.AddError(message);
				}
			}
		}

		IEnumerable<RelatedMilestoneEventDefinition> GetRelatedMilestoneDefinitions(ProcessTask task, Event @event)
		{
			var mp = new WorkflowMilestoneProxy(task);
			if (@event != null)
			{
				yield return new RelatedMilestoneActualEventDefinition(mp, @event.Code, mp.OriginalActualDate, mp.ActualDate, mp.ActualDate);
				yield return new RelatedMilestoneIsEstimateEvent(mp, @event.Code, mp.OriginalEstimateDate, mp.EstimateDate, mp.EstimateDate);
			}
			yield return new RelatedMilestoneESTEventDefinition(mp, mp.OriginalEstimateDate, mp.EstimateDate, mp.EstimateDate);
		}

		string[] GetDuplicateParametersFromChildReference(RelatedMilestoneEventDefinition definition, ZString reference)
		{
			return definition.GetBuilder(reference).GetDuplicateKeys();
		}

		#region Libraries

		static IMacroLibrary StandardLibrary(BusinessObjectFactory factory) => factory.GetCachedValue("TriggerValidation.StandardMacroLibrary", () => new StandardLibrary());
		static IMacroLibrary LocationsLibrary(BusinessObjectFactory factory) => factory.GetCachedValue("TriggerValidation.LocationsLibrary", () => new LocationsLibrary(factory));

		#endregion

		#endregion

		#region TriggerFiredCountdown

		public void ValidateTriggerFiredCountdown()
		{
			ValidateCalculatedProperty(Parent.TriggerFiredCountdownInfo);
		}

		protected void CheckTriggerFiredCountdown()
		{
			if (Parent.TriggerFiredCountdown <= 0 && !Parent.ReadOnly && !Parent.TriggerFiredCountdown_ReadOnly)
			{
				Parent.TriggerFiredCountdownInfo.AddWarning(TriggerFiredCountZeroMessage);
			}
		}

		#endregion

		#region TriggerContextCode

		public void ValidateTriggerContextCode()
		{
			ValidateCalculatedProperty(Parent.TriggerContextCodeInfo);
		}

		protected void CheckTriggerContextCode()
		{
			MandatoryValidation.CheckEntered(Parent.TriggerContextCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TriggerContextCodeInfo);

			PreventTriggerFieldBeingUsedWithEVTContext(Parent.TriggerContextCodeInfo);
		}

		void PreventTriggerFieldBeingUsedWithEVTContext(ZPropertyInfo info)
		{
			if (!Parent.Trigger.TriggerFieldName.IsEmpty && Parent.TriggerContextCode == TriggerUserContextList.Codes.Event)
			{
				info.AddError(Res.GetString("875a859c-2ffc-40dd-9168-0fc0ae491eb3", "Cannot use {0} when Trigger Field ({1}) is set.", TriggerUserContextList.Codes.Event, Parent.Trigger.TriggerFieldName));
			}
		}

		#endregion

		#region TriggerStaffCode

		public void ValidateTriggerStaffCode()
		{
			ValidateCalculatedProperty(Parent.TriggerStaffCodeInfo);
		}

		protected void CheckTriggerStaffCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TriggerStaffCodeInfo);
		}

		#endregion

		#region TriggerBranch

		public void ValidateTriggerBranch()
		{
			ValidateCalculatedProperty(Parent.TriggerBranchInfo);
		}

		protected void CheckTriggerBranch()
		{
			ListValidation.ErrorIfInvalidPK(Parent.TriggerBranchInfo);
		}

		#endregion

		#region TriggerCompany

		public void ValidateTriggerCompany()
		{
			ValidateCalculatedProperty(Parent.TriggerCompanyInfo);
		}

		protected void CheckTriggerCompany()
		{
			if (!Parent.IsTemplate && !Parent.TriggerContextCode.Equals(TriggerUserContextList.Codes.Event))
			{
				MandatoryValidation.CheckEntered(Parent.TriggerCompanyInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.TriggerCompanyInfo);
		}

		#endregion

		#region TriggerDepartment

		public void ValidateTriggerDepartment()
		{
			ValidateCalculatedProperty(Parent.TriggerDepartmentInfo);
		}

		protected void CheckTriggerDepartment()
		{
			ListValidation.ErrorIfInvalidPK(Parent.TriggerDepartmentInfo);
		}

		#endregion

		#region Implementation

		void CheckTriggerFieldOrEventSpecified(ZPropertyInfo property)
		{
			var trigger = Parent.Trigger;
			if (Parent.TriggerEventCode.IsEmpty && Parent.TriggerFieldName.IsEmpty && (trigger.IsMilestone() || trigger.IsTrigger()))
			{
				property.AddError(Res.GetString("5c1c924e-4c6c-4232-95f1-954c0a8449cd", "You must specify a Trigger Event Code or Trigger Field Name."));
			}
		}

		#endregion
	}
}
