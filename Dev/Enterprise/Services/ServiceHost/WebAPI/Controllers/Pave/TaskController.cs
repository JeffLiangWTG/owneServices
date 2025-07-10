using System;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle;
using Enterprise.BufferManagement.Service.Shared.Templates.Dtos;
using Enterprise.BufferManagement.Service.Templates;
using Enterprise.Services.ServiceHost.Pave.Dtos;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class TaskController : BasePaveController<ITaskService>
	{
		public TaskController() : base(() => new TaskService())
		{
		}

		[Route("Pave/Task/ChangeChannel")]
		public IHttpActionResult PostChangeTaskChannel(ChangeTaskChannelRequestDTO dto)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var response = Service.TryChangeChannel(dto, dto.GetActions);

				if (response == null)
				{
					return Content(HttpStatusCode.BadRequest, dto);
				}

				if (!response.Success)
				{
					return ToPaveResponseJson(response.ContainmentBarrierRequired, response.Error);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPut]
		[Route("api/pave/tasks/{taskId}/assignment/capability")]
		public IHttpActionResult PutAssignmentCapability([FromUri] Guid taskId, AssignToCapabilityRequest request)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryAssignToCapability(taskId, request, out var response))
				{
					return Content((HttpStatusCode)422, response, GetFormatter());
				}

				return Ok();
			}
		}

		[HttpDelete]
		[Route("Pave/Task/{taskId}/assignment/capability")]
		public IHttpActionResult DeleteAssignmentCapability([FromUri] Guid taskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryDeleteAssignmentCapability(taskId, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, taskId);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpDelete]
		[Route("api/pave/tasks/{taskId}")]
		public IHttpActionResult DeleteTask([FromUri] Guid taskId, [FromBody] DeleteTaskRequest request)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryDelete(taskId, request, out var response))
				{
					return Content((HttpStatusCode)422, response);
				}

				return Ok();
			}
		}

		[HttpDelete]
		[Route("Pave/Task/{taskId}/assignment/staff")]
		public IHttpActionResult DeleteAssignmentStaff([FromUri] Guid taskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryDeleteAssignmentStaff(taskId, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, taskId);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpDelete]
		[Route("Pave/Task/{taskId}/assignment")]
		public IHttpActionResult DeleteAssignment([FromUri] Guid taskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryDeleteAssignment(taskId, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					return Content(HttpStatusCode.BadRequest, taskId);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("Pave/tasks/{taskId}/estimates")]
		public IHttpActionResult PostChangeEstimates(Guid taskId, UpdateTaskEstimatesRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryUpdateEstimates(taskId, updateRequest, out PaveError error))
				{
					if (error != null)
					{
						return ToPaveResponseJson((object)null, error);
					}

					//TODO: BadRequest is probably not the right http status to use here
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				return ToPaveResponseJson();
			}
		}

		[Route("Pave/tasks/{taskId}/notes")]
		public IHttpActionResult GetNotes(Guid taskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var notes = Service.GetNotes(taskId);
				var result = new WorkItemDetailsResponseDto()
				{
					Hash = notes.Hash,
					Content = notes.Content,
				};

				return ToPaveResponseJson(result);
			}
		}

		[HttpPost]
		[Route("Pave/tasks/{taskId}/notes")]
		public IHttpActionResult PostChangeNotes(Guid taskId, UpdateTaskNotesRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (updateRequest.PreviousHash == null)
				{
					return Content(HttpStatusCode.BadRequest, updateRequest);
				}

				var response = Service.TryUpdateNotes(taskId, updateRequest);

				if (!response.Success)
				{
					return ToPaveResponseJson((object)null, response.Error);
				}

				return ToPaveResponseJson(response.TaskNotesResponse);
			}
		}

		[HttpPost]
		[Route("Pave/tasks/{taskId}/apply-note-template")]
		public IHttpActionResult PostApplyNoteTemplate([FromUri] Guid taskId, ApplyTemplateRequest request)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var service = new TemplateService();
				var response = service.ApplyTaskNoteTemplate(taskId, request);
				return ToPaveResponseJson(response);
			}
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/type")]
		public IHttpActionResult PostChangeType([FromUri] Guid taskId, UpdateTaskTypeRequest dto)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryUpdateType(taskId, dto, out var response))
				{
					return Content((HttpStatusCode)422, response, GetFormatter());
				}

				return Ok();
			}
		}

		[HttpPost]
		[Route("Pave/tasks")]
		public IHttpActionResult PostTask(NewTaskRequest dto)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var response = Service.TryAddNewTask(dto, out PaveError error);

				if (!response.Success)
				{
					return ToPaveResponseJson((object)null, response.Error);
				}

				return ToPaveResponseJson(response.Response);
			}
		}

		[HttpPost]
		[Route("api/pave/tasks/{taskId}/actual-duration")]
		public IHttpActionResult PostChangeActualDuration(Guid taskId, UpdateActualDurationRequest updateRequest)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryUpdateActualDuration(taskId, updateRequest, out var response))
				{
					return Content((HttpStatusCode)422, response, GetFormatter());
				}

				return Ok();
			}
		}

		[HttpPost]
		[Route("Pave/tasks/{taskId}/type-notes")]
		public IHttpActionResult PostChangeTypeNotes([FromUri] Guid taskId, UpdateTypeAndNotesRequest dto)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!Service.TryUpdateTypeAndNotes(taskId, dto, out var error))
				{
					return ToPaveResponseJson((object)null, error);
				}

				return ToPaveResponseJson();
			}
		}

		[HttpPost]
		[Route("Pave/tasks/{taskId}/description")]
		public IHttpActionResult PostUpdateTaskDescription([FromUri] Guid taskId, UpdateTaskDescriptionRequest dto)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = Service.TryUpdateTaskDescription(taskId, dto);
				return ToPaveResponseJson(result);
			}
		}

		//TODO: Remove when move all to BaseController
		static JsonMediaTypeFormatter GetFormatter()
		{
			var formatter = new JsonMediaTypeFormatter();
			formatter.SerializerSettings.Converters.Add(new StringEnumConverter());
			formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			return formatter;
		}
	}
}
