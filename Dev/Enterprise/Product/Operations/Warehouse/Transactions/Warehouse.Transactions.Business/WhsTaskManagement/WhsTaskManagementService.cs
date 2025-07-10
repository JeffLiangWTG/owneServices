using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTaskManagementService : IWhsTaskManagementService
	{
		#region BatchChangeTaskPlanningStatus

		static string ConcurrencyErrorMessage => Res.GetString("e5e6a40b-e816-4e86-9e9c-7ff5d7d0fba1", "Another user has changed the job. Please restart the operation and try again.");

		public string BatchChangeTaskPlanningStatus(IReadOnlyCollection<ZGuid> jobPKs, string jobType, bool changeStatusToReady)
		{
			var factory = new BusinessObjectFactory();
			var batchResult = new ZStringBuilder();
			var jobs = LoadJobs(factory, jobPKs, jobType).OrderBy(j => j.JobID);
			foreach (var job in jobs)
			{
				var message = ChangeTaskPlanningStatusCore(job, changeStatusToReady);
				if (message.IsNullOrEmpty())
				{
					message = Res.GetString("0ffa177a-60aa-449c-9fda-435cdefe62bb", "{0} has been updated", job.HumanReadableNameWithoutID);
				}

				batchResult.AppendFormat("{0} - {1}", job.JobID, message);
				batchResult.AppendLine();
			}

			var saveExceptionMessage = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => ConcurrencyErrorMessage);
			return saveExceptionMessage.IsNullOrEmpty() ? batchResult.ToString() : saveExceptionMessage;
		}

		#endregion

		#region ChangeTaskPlanningStatus

		public string ChangeTaskPlanningStatus(ZGuid jobPK, string jobType, bool changeStatusToReady)
		{
			var message = string.Empty;
			var factory = new BusinessObjectFactory();
			var job = LoadJob(factory, jobPK, jobType);
			if (job != null)
			{
				message = ChangeTaskPlanningStatusCore(job, changeStatusToReady);
				if (message.IsNullOrEmpty())
				{
					message = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => ConcurrencyErrorMessage);
				}
			}
			else
			{
				message = Res.GetString("0b67af6b-6483-4cdb-8cd3-1592eb50986a", "PK '{0}', Job Type '{1}' is not found", jobPK, jobType);
			}

			return message;
		}

		ITaskPlanningJob LoadJob(BusinessObjectFactory factory, ZGuid jobPK, string jobType) => jobType switch
		{
			TaskManagementJobType.WhsReceive => factory.Load<WhsReceive>(jobPK),
			TaskManagementJobType.WhsCycleCountLocation => factory.Load<WhsCycleCountLocation>(jobPK),
			TaskManagementJobType.WhsPick => factory.Load<WhsPick>(jobPK),
			TaskManagementJobType.WhsTransfer => factory.Load<WhsTransfer>(jobPK),
			TaskManagementJobType.WhsLoad => factory.Load<WhsLoad>(jobPK),
			_ => null,
		};

		IReadOnlyCollection<ITaskPlanningJob> LoadJobs(BusinessObjectFactory factory, IEnumerable<ZGuid> jobPKs, string jobType)
		{
			IReadOnlyCollection<ITaskPlanningJob> jobs = null;
			if (jobType == TaskManagementJobType.WhsCycleCountLocation)
			{
				var query = new ZDBOnlyQuery(typeof(WhsCycleCountLocation));
				query.AddToFilter(WhsCycleCountLocationSchema.PK, jobPKs);
				jobs = factory.Load<WhsCycleCountLocation>(query);
			}
			else if (jobType == TaskManagementJobType.WhsLoad)
			{
				var query = new ZDBOnlyQuery(typeof(WhsLoad));
				query.AddToFilter(WhsLoadSchema.PK, jobPKs);
				jobs = factory.Load<WhsLoad>(query);
			}

			return jobs;
		}

		public string ChangeTaskPlanningStatus(ITaskPlanningJob job) => ChangeTaskPlanningStatusCore(job, job.IsUnplanned());

		string ChangeTaskPlanningStatusCore(ITaskPlanningJob job, bool changeStatusToReady)
		{
			Argument.NotNull(job, nameof(job));

			var message = string.Empty;
			if (job.IsTaskManagementEnabled())
			{
				message = job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady);
				if (message.IsNullOrEmpty())
				{
					if (changeStatusToReady && !job.IsUnplanned())
					{
						message = Res.GetString("c525c267-cb0b-4c71-bf21-068199aaec39",
							"Cannot change Task Planning Status to Ready as the status is currently '{0}'",
							new TaskPlanningStatus().GetDescriptionFromCode(job.TaskPlanningStatus));
					}
					else if (!changeStatusToReady && job.IsUnplanned())
					{
						message = Res.GetString("a52f0ce9-1832-482d-9215-72bea1c69c05", "Cannot change Task Planning Status to Not Ready as it is already Unplanned");
					}
					else
					{
						if (changeStatusToReady)
						{
							job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
						}
						else
						{
							job.TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

							var processTasks = job is ITaskPlanningJobWithExternalTasks jobWithExternalTasks ? jobWithExternalTasks.GetRelatedProcessTasks() : job.GetRelatedProcessTasksOffJob();
							job.ClearFKForProcessTasks(processTasks.Select(task => task.PK).ToHashSet());
							ProcessTaskHelper.DeleteProcessTasksAndRelatedProcessHeader(job.Factory, processTasks);
						}
					}
				}
			}
			else
			{
				message = Res.GetString("45d89758-4785-4016-8d7d-8264c5eeb9e4", "Task management is not enabled for the {0}", job.HumanReadableNameWithoutID);
			}

			return message;
		}

		#endregion

		#region GetNextTask

		public GetNextTaskResult GetNextTask(BusinessObjectFactory factory, string taskReference, Guid staffPK, Guid warehousePK, string formFlowType, string lastFormFlowType, Guid[] tasksToIgnore)
		{
			Argument.NotNull(factory, nameof(factory));

			var staff = factory.Load<IGlbStaff>(staffPK);
			var warehouse = factory.Load<WhsWarehouse>(warehousePK);

			GetNextTaskResult result;
			var errorMessage = ValidateParameters(factory, staff, warehouse, formFlowType);
			if (string.IsNullOrEmpty(errorMessage))
			{
				var userRegistry = GetOrCreateRFRegistry(factory, staff, warehouse);
				var tasksToConsider = LoadTasksToConsider(taskReference, staff, userRegistry, warehouse, formFlowType, tasksToIgnore);
				result = GetNextTask(factory, staff, tasksToConsider);
			}
			else
			{
				result = new GetNextTaskResult(errorMessage);
			}
			return result;
		}
		
		string ValidateParameters(BusinessObjectFactory factory, IGlbStaff staff, WhsWarehouse warehouse, string formFlowType)
		{
			var errorMessage = string.Empty;
			if (staff == null)
			{
				errorMessage = Res.GetString("e998da2a-400d-46fb-87db-89e09037b6d6", "Staff could not be found. Please try again.");
			}
			else if (!string.IsNullOrEmpty(formFlowType) && !ValidWarehouseFormFlowTypes.Contains(formFlowType))
			{
				errorMessage = Res.GetString("63f34b38-8339-4c7f-9d56-adacdf65141e", "This job type is not supported for Task Management.");
			}
			else
			{
				if (warehouse == null)
				{
					errorMessage = Res.GetString("c7021086-d37c-4c58-a13b-4be77d8a6294", "Warehouse could not be found. Please check branch details and try again.");
				}
				else if (!warehouse.WW_GG_ReleaseGroup.IsValid)
				{
					errorMessage = Res.GetString("08b4e841-0add-4e10-942a-e0c8a5e8ec69", "This warehouse does not support Task Management. Please check you are logged in with the correct branch.");
				}
			}

			return errorMessage;
		}

		WhsRFRegistry GetOrCreateRFRegistry(BusinessObjectFactory factory, IGlbStaff staff, WhsWarehouse warehouse)
		{
			var userRegistryQuery = new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, staff.GS_Code);
			userRegistryQuery.AddToFilter(WhsRFRegistrySchema.WRR_WW_Whs, warehouse.PK);
			var userRegistry = factory.LoadTop1<WhsRFRegistry>(userRegistryQuery);

			if (userRegistry == null)
			{
				// A user registry should already exist to invoke this method
				// But create a stub registry with defaults if one is not found
				userRegistry = factory.New<WhsRFRegistry>();
				userRegistry.WRR_GS_NKAssignedTo = staff.GS_Code;
				userRegistry.WRR_WW_Whs = warehouse.PK;
			}

			return userRegistry;
		}

		GlowIndexQueryResultCollection LoadTasksToConsider(
			string reference,
			IGlbStaff staff,
			WhsRFRegistry registry,
			WhsWarehouse warehouse,
			string formFlowType,
			Guid[] tasksToIgnore)
		{
			var luceneTaskSearch = ObjectFactory.Get<IWhsLuceneTaskSearch>();

			var formFlowTypesToConsider = string.IsNullOrEmpty(formFlowType)
				? WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray()
				: new[] { formFlowType };

			return luceneTaskSearch.QueryLuceneForTasks(
				reference,
				staff,
				registry,
				warehouse,
				formFlowTypesToConsider,
				tasksToIgnore ?? Array.Empty<Guid>());
		}

		GetNextTaskResult GetNextTask(BusinessObjectFactory factory, IGlbStaff staff, GlowIndexQueryResultCollection tasksToConsider)
		{
			// Final solution will integrate with PRE
			GetNextTaskResult result;
			if (tasksToConsider.Results.Count == 0 || !tasksToConsider.ErrorMessage.IsNullOrEmpty())
			{
				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine(Res.GetString("c1f0b2d3-4a5e-4b8c-9f6d-7f8e9a0b1c2d", "No available task could be found. Please try again."));
				if (!tasksToConsider.ErrorMessage.IsNullOrEmpty())
				{
					errorMessageBuilder.AppendLine(tasksToConsider.ErrorMessage);
				}

				result = new GetNextTaskResult(errorMessageBuilder.ToString().Trim());
			}
			else
			{
				var task = factory.Load<IProcessTask>(new ZGuid(tasksToConsider.Results.First().PK));
				result = new GetNextTaskResult(task.PK.ToGuid(), task.P9_FormFlowType);
			}

			return result;
		}

		#endregion

		#region UpdateTaskStatus

		public UpdateTaskStatusResult SetTaskToPlayIfValid(IProcessTask task, string expectedRFType, string staffCode)
		{
			Argument.NotNull(task, nameof(task));
			Argument.NotNullOrEmpty(staffCode, nameof(staffCode));

			var result = UpdateTaskStatusResult.SuccessWithNoChanges;
			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Working)
			{
				result = IsValidWarehouseTask(task, expectedRFType, staffCode, ExpectedTaskStatusCodeToSetToWorking);
				if (result == UpdateTaskStatusResult.Success)
				{
					task.P9_GS_NKAssignedStaffMember = staffCode;
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				}
			}

			return result;
		}

		static readonly HashSet<string> ExpectedTaskStatusCodeToSetToWorking = new HashSet<string>
		{
			ProcessTaskStatusCodeList.Codes.Assigned,
			ProcessTaskStatusCodeList.Codes.Suspended,
			ProcessTaskStatusCodeList.Codes.Working,
			ProcessTaskStatusCodeList.Codes.Open,
		};

		public UpdateTaskStatusResult SetTaskToSuspendedIfValid(IProcessTask task, string staffCode)
		{
			Argument.NotNull(task, nameof(task));
			Argument.NotNullOrEmpty(staffCode, nameof(staffCode));

			var result = UpdateTaskStatusResult.SuccessWithNoChanges;
			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Suspended)
			{
				result = IsValidWarehouseTask(task, staffCode, ExpectedTaskStatusCodeToSetToClosed);
				if (result == UpdateTaskStatusResult.Success)
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				}
			}

			return result;
		}

		public UpdateTaskStatusResult SetTaskToCompletedIfValid(IProcessTask task, string staffCode)
		{
			Argument.NotNull(task, nameof(task));
			Argument.NotNullOrEmpty(staffCode, nameof(staffCode));

			var result = UpdateTaskStatusResult.SuccessWithNoChanges;
			if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
			{
				result = IsValidWarehouseTask(task, staffCode, ExpectedTaskStatusCodeToSetToClosed);
				if (result == UpdateTaskStatusResult.Success)
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
			}

			return result;
		}

		static readonly HashSet<string> ExpectedTaskStatusCodeToSetToClosed = new HashSet<string>
		{
			ProcessTaskStatusCodeList.Codes.Assigned,
			ProcessTaskStatusCodeList.Codes.Suspended,
			ProcessTaskStatusCodeList.Codes.Working,
		};

		UpdateTaskStatusResult IsValidWarehouseTask(IProcessTask task, string expectedRFType, string staffCode, HashSet<string> expectedCodes)
		{
			var result = UpdateTaskStatusResult.Success;

			if (expectedRFType != task.P9_FormFlowType)
			{
				result = UpdateTaskStatusResult.TaskIsWrongFormFlowType;
			}
			else
			{
				result = IsValidWarehouseTask(task, staffCode, expectedCodes);
			}
			return result;
		}

		UpdateTaskStatusResult IsValidWarehouseTask(IProcessTask task, string staffCode, HashSet<string> expectedCodes)
		{
			var result = UpdateTaskStatusResult.Success;

			if (!expectedCodes.Contains(task.P9_Status.ToString()))
			{
				if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					result = UpdateTaskStatusResult.TaskStatusIsCancelled;
				}
				else if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
				{
					result = UpdateTaskStatusResult.TaskStatusIsCompleted;
				}
				else if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Open)
				{
					result = UpdateTaskStatusResult.TaskStatusIsOpen;
				}
			}
			else if (!task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_GS_NKAssignedStaffMember != staffCode)
			{
				result = UpdateTaskStatusResult.AssignedUserIsDifferent;
			}
			else if (!ValidWarehouseFormFlowTypes.Contains(task.P9_FormFlowType.ToString()))
			{
				result = UpdateTaskStatusResult.TaskIsNotValidWarehouseJob;
			}

			return result;
		}

		static readonly HashSet<string> ValidWarehouseFormFlowTypes = new HashSet<string>
		{
			WarehouseTaskFormFlowTypes.UnloadJob,
			WarehouseTaskFormFlowTypes.PutawayJob,
			WarehouseTaskFormFlowTypes.PickJob,
			WarehouseTaskFormFlowTypes.PickByLabelJob,
			WarehouseTaskFormFlowTypes.DirectedPackingJob,
			WarehouseTaskFormFlowTypes.TransferJob,
			WarehouseTaskFormFlowTypes.ReplenishmentJob,
			WarehouseTaskFormFlowTypes.LoadJob,
			WarehouseTaskFormFlowTypes.CycleCountJob,
		};

		#endregion
	}
}
