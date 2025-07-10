using System;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class BusinessObjectPropertyChangeTracker : IDisposable
	{
		public BusinessObjectPropertyChangeTracker(IWorkflowTrigger trigger, BusinessObjectFactory targetFactory)
		{
			Trigger = trigger;
			TargetFactory = targetFactory;
			PropertyChangeSubscription.PropertyChanged += OnPropertyChanged;
		}

		void OnPropertyChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			var sourceBizO = e?.Property?.BizObj;
			if (sourceBizO == null)
			{
				return;
			}

			var sourceFactory = sourceBizO.Factory;
			if (sourceFactory == null ||
				sourceFactory._Instance != TargetFactory._Instance ||
				sourceFactory.IsConstructingNullBusinessObject)
			{
				return;
			}

			if (sourceBizO.GetType().GetCustomAttribute<DisableWorkflowSettingPropertiesAfterOnSavingAttribute>() != null)
			{
				var message = (NoResString)@$"The {e.Property.BizObj.HumanReadableName} is protected from making changes when saving and its properties are not allowed to be changed by the Immediate Field Change ({WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange}) trigger action. Consider using a Set Field ({WorkflowTriggerActionTypeConstants.Codes.SetField}) trigger action instead.
Trigger Event Code: {Trigger.TriggerEventCode}.
Trigger Description: {Trigger.Description}.
Factory ID: {sourceFactory._Instance}.
Trigger Actions:
{CollectTriggerActionsInfo()}";

				ReportError(message);
			}
		}

		void ReportError(string message)
		{
			/*
			 *	Note for developer:
			 *	https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/10493/Problems-Caused-by-IFC-Trigger-Usage-on-Accounting-Bizos
			 */

			ErrorReporter.ReportDeveloperExceptionOnce("WorkflowSettingPropertiesAfterOnSaving", message, new DeveloperNotificationException(message));
		}

		string CollectTriggerActionsInfo()
		{
			var messageBuilder = new StringBuilder();
			foreach (var action in Trigger.TriggerActions)
			{
				if (action is ProcessTaskNotification triggerAction)
				{
					messageBuilder.AppendLine($"{triggerAction.PQ_TriggerType} - {triggerAction.PQ_FieldName} - {triggerAction.PQ_FieldValue}");
				}
			}

			return messageBuilder.ToString();
		}

		void IDisposable.Dispose()
		{
			PropertyChangeSubscription.PropertyChanged -= OnPropertyChanged;
		}

		IWorkflowTrigger Trigger { get; }
		BusinessObjectFactory TargetFactory { get; }
	}
}
