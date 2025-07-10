using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class DataContextDataObjectWriter
	{
		public void PopulateDataObject(IUniversalActionInfo action, IDataContextDataObject dataContext, IEventInfo eventInfo = null, bool includeWorkflowInfo = true)
		{
			PopulateDataObject(action, action.ParentBO, dataContext, eventInfo, includeWorkflowInfo);
		}

		public void PopulateDataObject(IUniversalActionInfo actionInfo, BusinessObject parentBO, IDataContextDataObject dataContext, IEventInfo eventInfo = null, bool includeWorkflowInfo = true)
		{
			var manager = GetManager(parentBO);

			if (dataContext == null)
			{
				// To fix code analysis error
				throw new InvalidOperationException("Data Context for the " + manager.DataContextType.ToString() + " data object was empty. The Data Context should always be created by the top level data object.");
			}

			dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			if (includeWorkflowInfo)
			{
				var eventType = ListHelper.GetWithDescription<CodeDescriptionPair>(actionInfo.TriggerEventCode, WorkflowListHelper.GetCachedEventCodeList(actionInfo.FactoryForProcessing));

				var workflowInfo = new WorkflowInfo()
				{
					EventType = eventType,
					ActionPurpose = actionInfo.PurposeCode.IsEmpty ? null : ListHelper.GetWithDescription<CodeDescriptionPair>(actionInfo.PurposeCode, WorkflowListHelper.GetCachedPurposeCodeList(actionInfo.FactoryForProcessing)),
					TriggerDescription = actionInfo.TriggerDescription,
					TriggerCount = Math.Max(1, actionInfo.TriggerCount),
					TriggerDate = actionInfo.TriggeringEvent?.EventTimeOffset ?? (actionInfo.TriggerActualDate.IsEmpty ? actionInfo.TriggerScheduledDate : actionInfo.TriggerActualDate),
					TriggerReference = actionInfo.TriggerReference,
					TriggerType = actionInfo.TriggerType,
					RecipientRoles = actionInfo.RecipientRoleDetails
				};

				UpdateEventInfo(workflowInfo, eventInfo);

				dataContext.SetWorkflowInfo(workflowInfo);
			}
		}

		void UpdateEventInfo(WorkflowInfo workflowInfo, IEventInfo eventInfo)
		{
			if (eventInfo != null)
			{
				workflowInfo.EventReference = eventInfo.EventReference;
				workflowInfo.EventUser = Staff.New(eventInfo.EventUser);
				workflowInfo.EventBranch = Branch.New(eventInfo.EventBranch);
				workflowInfo.EventDepartment = Department.New(eventInfo.EventDepartment);
			}
			else
			{
				workflowInfo.EventUser = Staff.New(GlbStaff.CurrentUser);
				workflowInfo.EventBranch = Branch.New(GlbBranch.CurrentBranch);
				workflowInfo.EventDepartment = Department.New(GlbDepartment.CurrentDepartment);
			}
		}

		IDataContextManager GetManager(BusinessObject parentBO)
		{
			Argument.NotNull(parentBO, "BusinessObject parentBO");

			return parentBO.GetUniversalDataContextManager()
				?? throw new InvalidOperationException(string.Format("The type of the parentBO passed in must have the UniversalDataContextAttribute applied. Type: [{0}] PK: [{1}]", parentBO.GetType().FullName, parentBO.PK.ToString()));
		}
	}
}
