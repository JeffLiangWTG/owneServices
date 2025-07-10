using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using WTG.WiseTechAcademy;

namespace Enterprise.MasterFiles.Business.Workflow.TriggerActionRunners
{
	class EnrolInWiseTechAcademyCourseTriggerActionRunner : IProcessor
	{
		internal EnrolInWiseTechAcademyCourseTriggerActionRunner(ProcessTaskNotification action, BusinessObject parent)
		{
			this.action = action;
			this.parent = parent;
		}

		readonly ProcessTaskNotification action;
		readonly BusinessObject parent;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (action.PQ_ActionReference is not { IsValid: true } unitId)
			{
				notifications.AddError($"Could not get WTA course ID from PQ_ActionReference (value was: [{action.PQ_ActionReference}])");
				return;
			}

			var staffToEnrol = action.WorkflowDescriptor.GetStaffForWtaEnrolment(parent);
			if (staffToEnrol is null)
			{
				notifications.AddError((NoResString)"No staff record was found to process this trigger");
				return;
			}

			var apiClient = ObjectFactory.Get<IWiseTechAcademyApiClient>();
			var serviceResult = apiClient.EnsureEnrolled(unitId, staffToEnrol.GS_PER.ToGuid(), staffToEnrol.GS_FullName, staffToEnrol.GS_EmailAddress, sendEmailNotification: true);

			if (!serviceResult.IsSuccess)
			{
				notifications.AddError((NoResString)"Unsuccessful response from WTA API: " + serviceResult.ResponseMessage);
			}
		}
	}
}
