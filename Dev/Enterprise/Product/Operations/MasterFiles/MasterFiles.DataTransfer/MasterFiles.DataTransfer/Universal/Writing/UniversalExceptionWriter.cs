using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalExceptionWriter : IUniversalExceptionWriter
	{
		public void PopulateExceptions(BusinessObject source, IDataObject destination)
		{
			if (source is IWorkflowProvider workflowProvider && destination is IExceptionCollectionParent exceptionCollectionParent)
			{
				var exceptions =
					workflowProvider.WorkflowItems.Exceptions.Cast<ProcessTask>()
						.Select(TransformToUniversalException).ToList();

				exceptionCollectionParent.SetExceptionCollection(() => exceptions.Count > 0 ? exceptions : null);
			}
		}

		static WorkflowException TransformToUniversalException(ProcessTask exception)
		{
			var ex = new WorkflowException
			{
				ExceptionID = exception.P9_TaskID,
				Description = exception.P9_Description,
				Actioned = exception.IsExceptionActioned,
				Date = !exception.P9_ActualDateOffset.IsEmpty ? exception.P9_ActualDateOffset : null,
				EndDate = !exception.P9_ExceptionEndDate.IsEmpty ? exception.P9_ExceptionEndDate : null,
				DurationHours = exception.P9_ExceptionDurationHours,
				Location = ListHelper.GetWithName(exception.P9_RL_NKExceptionLocation, exception.Lookups.ExceptionLocations),
				Notes = exception.P9_NotesAsPlainText,
			};

			var exceptionType = exception.ExceptionType;

			if (exceptionType != null)
			{
				ex.Type = exceptionType.WET_Code;
				ex.Category = !exceptionType.WET_Category.IsEmpty ? exceptionType.WET_Category : null;
			}

			var processWorkflowException = exception.ProcessWorkflowException;

			if (processWorkflowException != null)
			{
				ex.Cause = processWorkflowException.Cause?.WEC_Code;
				ex.Resolution = processWorkflowException.Resolution?.WER_Code;
			}

			ex.Group = exception.AssignedGroup != null ? Group.New(exception.AssignedGroup) : null;
			ex.Staff = exception.AssignedStaffMember != null ? Staff.New(exception.AssignedStaffMember) : null;

			if (exception.IsExceptionActioned)
			{
				var actionedDate = exception.P9_CompletedTimeUtc;
				if (exception.P9_ActualDateOffset.IsValid && actionedDate.IsValid)
				{
					var offset = exception.P9_ActualDateOffset.Offset;
					ex.ActionedDate = new ZDateTimeOffset(actionedDate.Add(offset), DateTimeKind.Local, offset);
				}
				else if (actionedDate.IsValid)
				{
					ex.ActionedDate = actionedDate.ToLocalBranchTimeOffset();
				}
				else
				{
					ex.ActionedDate = ZDateTimeOffset.Empty;
				}
			}

			return ex;
		}
	}
}
