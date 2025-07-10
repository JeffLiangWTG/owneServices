using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class WorkflowExceptionReader : DataObjectReader<WorkflowException, ProcessTask>
	{
		public WorkflowExceptionReader(WorkflowException dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ProcessTask exception, ExceptionCollectionView parentExceptions) : base(dataObject, logger, factory)
		{
			Exception = exception;
			ParentExceptions = parentExceptions;
			TypeName = ResString.GetMultilingualString("WorkflowExceptionReaderTypeName-6fe28a44-43e7-42a8-aa0a-7d8fc7dd1e59", "Exception");
		}

		ProcessTask Exception { get; }
		ExceptionCollectionView ParentExceptions { get; }
		ResourceString TypeName { get; }

		protected override ProcessTask GetNewBusinessObject() => ParentExceptions.AddNew();
		protected override void LogSuccessfullyLoadedMessage(string typeName) => base.LogSuccessfullyLoadedMessage(TypeName);
		protected override string GetBusinessObjectHumanReadableName(ProcessTask businessObject) => $"{TypeName}: {dataObject?.Description}";
		protected override LogType LogTypeForReasonNotAbleToUpdate => LogType.Information;

		protected override ProcessTask GetExistingBusinessObject()
		{
			if (Exception == null || !dataObject.ExceptionID.HasValue || Exception.P9_TaskID != dataObject.ExceptionID.Value)
			{
				return null;
			}

			return Exception;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(ProcessTask targetBO)
		{
			if (string.IsNullOrEmpty(dataObject.Description))
			{
				return ResString.GetMultilingualString("WorkflowExceptionReaderExceptionWithoutDescription-6fe28a44-43e7-42a8-aa0a-7d8fc7dd1e59", "Exception without description");
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		protected override void PopulateBusinessObject(ProcessTask targetBO)
		{
			SetValue(targetBO, ProcessTasksSchema.P9_Description, dataObject.Description);
			SetType(targetBO);

			var fallbackOffsetIfNoneIsPresent = ZDateTimeOffset.Now.Offset;
			if (dataObject.Actioned.HasValue)
			{
				if (dataObject.Actioned.Value)
				{
					SetValue(targetBO, ProcessTask.Schema.IsExceptionActioned, true);

					if (dataObject.ActionedDate.HasValue && dataObject.ActionedDate.Value.IsValid)
					{
						var actionedDate = dataObject.ActionedDate.Value.ToUTCTime(fallbackOffsetIfNoneIsPresent);
						SetValue(targetBO, ProcessTasksSchema.P9_CompletedTimeUtc, actionedDate);
					}
					else
					{
						SetValue(targetBO, ProcessTasksSchema.P9_CompletedTimeUtc, ZDateTime.UtcNow);
					}
				}
				else
				{
					SetValue(targetBO, ProcessTask.Schema.IsExceptionActioned, false);
				}
			}

			if (dataObject.Date.HasValue)
			{
				var date = dataObject.Date.Value.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(fallbackOffsetIfNoneIsPresent);
				SetValue(targetBO, ProcessTask.Schema.P9_ActualDateOffset, date);
			}

			if (dataObject.EndDate.HasValue)
			{
				var endDate = dataObject.EndDate.Value.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(fallbackOffsetIfNoneIsPresent);
				SetValue(targetBO, ProcessTasksSchema.P9_ExceptionEndDate, endDate);
			}

			if (dataObject.DurationHours.HasValue)
			{
				SetValue(targetBO, ProcessTasksSchema.P9_ExceptionDurationHours, dataObject.DurationHours.Value);
			}

			SetValue(targetBO, ProcessTasksSchema.P9_RL_NKExceptionLocation, dataObject.Location);
			if (dataObject.Notes.HasValue)
			{
				SetValue(targetBO, ProcessTasksSchema.P9_Notes, ZBlob.FromUTF8(dataObject.Notes.Value));
			}

			SetCauseAndResolution(targetBO.ProcessWorkflowException, targetBO.ExceptionType, targetBO);
			SetGroupAndStaff(targetBO);
		}

		void SetType(ProcessTask targetBO)
		{
			var currentType = targetBO.P9_SE_NKExceptionEvent;

			if (dataObject.Type.HasValue)
			{
				SetValue(targetBO, ProcessTasksSchema.P9_SE_NKExceptionEvent, dataObject.Type.Value);
			}

			if (targetBO.ProcessWorkflowException == null || targetBO.ExceptionType == null)
			{
				if (dataObject.Type.HasValue)
				{
					logger.Log(LogType.Warning, Res.GetString("00cd19d2-affc-4f5e-905c-2e651f300cdc", "Could not find exception type:{0} of the exception:{1}", dataObject.Type, targetBO.P9_Description));
				}

				SetValue(targetBO, ProcessTasksSchema.P9_SE_NKExceptionEvent, currentType);

				return;
			}

			if (dataObject.Category.HasValue)
			{
				if (!StringComparer.OrdinalIgnoreCase.Equals(targetBO.ExceptionTypeCategory, dataObject.Category.Value))
				{
					logger.Log(LogType.Warning, Res.GetString("fa986554-0614-4a3b-859f-5da4ff11c50e", "Could not find category:{0} on exception type:{1} of the exception:{2}", dataObject.Category, dataObject.Type, targetBO.P9_Description));
				}
			}
		}

		void SetCauseAndResolution(ProcessWorkflowException processWorkflowException, ProcessWorkflowExceptionType exceptionType, ProcessTask targetBO)
		{
			if (dataObject.Cause.HasValue)
			{
				var query = new ZQuery(ProcessWorkflowExceptionCauseSchema.WEC_WET_Type, exceptionType.PK);
				query.AddToFilter(ProcessWorkflowExceptionCauseSchema.WEC_Code, dataObject.Cause.Value);

				var cause = factory.LoadTop1<ProcessWorkflowExceptionCause>(query);

				if (cause != null)
				{
					SetValue(processWorkflowException, ProcessWorkflowExceptionSchema.WEX_WEC_Cause, cause.PK);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("c61a408d-2c82-49c6-8906-e41f444c3112", "Could not find exception cause:{0} of the exception:{1}", dataObject.Cause, targetBO.P9_Description));
				}
			}

			if (dataObject.Resolution.HasValue)
			{
				var query = new ZQuery(ProcessWorkflowExceptionResolutionSchema.WER_WET_Type, exceptionType.PK);
				query.AddToFilter(ProcessWorkflowExceptionResolutionSchema.WER_Code, dataObject.Resolution.Value);

				var resolution = factory.LoadTop1<ProcessWorkflowExceptionResolution>(query);

				if (resolution != null)
				{
					SetValue(processWorkflowException, ProcessWorkflowExceptionSchema.WEX_WER_Resolution, resolution.PK);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("8087fe80-6344-40b0-8256-446d5cb37449", "Could not find exception resolution:{0} of the exception:{1}", dataObject.Resolution, targetBO.P9_Description));
				}
			}
		}

		void SetGroupAndStaff(ProcessTask targetBO)
		{
			if (dataObject.Group != null && dataObject.Group.Code.HasValue)
			{
				var glbGrpup = factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, dataObject.Group.Code));

				if (glbGrpup != null)
				{
					SetValue(targetBO, ProcessTasksSchema.P9_GG_AssignedGroup, glbGrpup.PK);
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("8fc53c33-8f41-4dd2-8d98-8c4a9e796fa2", "Could not find exception group:{0} of the exception:{1}", dataObject.Group.Code, targetBO.P9_Description));
				}
			}

			if (dataObject.Staff != null && dataObject.Staff.Code.HasValue)
			{
				SetValue(targetBO, ProcessTasksSchema.P9_GS_NKAssignedStaffMember, dataObject.Staff.Code);
			}
		}
	}
}
