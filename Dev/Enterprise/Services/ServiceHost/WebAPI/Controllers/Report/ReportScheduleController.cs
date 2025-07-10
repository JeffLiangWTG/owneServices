using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	[GlowTicketAuthentication]
	[RoutePrefix("api/report/schedule")]
	[ReportServiceErrorHandler]
	public class ReportScheduleController : ReportDataBaseController
	{
		[Route]
		[HttpPost]
		public IHttpActionResult ScheduleReport(ReportScheduleData scheduleData)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.ScheduleReport(scheduleData);
			}
			return Ok();
		}

		[Route("{reportScheduleTaskId}")]
		[HttpGet]
		public IHttpActionResult GetReportScheduleData(Guid reportScheduleTaskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetReportScheduleData(reportScheduleTaskId), new Newtonsoft.Json.JsonSerializerSettings() { DateFormatString = "yyyy-MM-ddTHH:mm:ss" });
			}
		}

		[Route("{reportScheduleTaskId}")]
		[HttpDelete]
		public IHttpActionResult DeleteReportScheduleTask(Guid reportScheduleTaskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.DeleteReportScheduleTask(reportScheduleTaskId);
			}

			return Ok();
		}

		[Route("checkPermission/{dataOperation}/{reportScheduleTaskId?}")]
		[HttpGet]
		public IHttpActionResult CheckPermissionForScheduleReport(string dataOperation, Guid reportScheduleTaskId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var hasPermission = false;
				if (Enum.TryParse(dataOperation.ToUpper(), out DataOperations operation))
				{
					hasPermission = Service.CheckPermissionForScheduleReport(operation, reportScheduleTaskId);
				}
				return Json(hasPermission);
			}
		}

		[Route("branch/{branchId}")]
		[HttpGet]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetBranch(Guid branchId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> results;
				if (branchId == Env.CurrentBranchPK)
				{
					results = [
						new()
						{
							Pk = Env.CurrentBranch.PK,
							Code = Env.CurrentBranch.Code,
							Description = Env.CurrentBranch.Name
						},
					];
				}
				else
				{
					var branch = Service.GetBranch(branchId);
					if (branch != null)
					{
						results = [
							new()
							{
								Pk = branch.PK.ToGuid(),
								Code = branch.GB_Code,
								Description = branch.GB_BranchName
							}
						];
					}
					else
					{
						results = new();
					}
				}

				return Json(new
				{
					count = results.Count,
					results,
				});
			}
		}

		[Route("printUsers")]
		[HttpPost]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetPrintUsers(ReportLookupSearchArgs args)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetPrintUsers(args, LookupFilterDataHelper.BuildCodeDescriptionQuery).Select(o => new { Pk = o.PK.ToGuid(), Code = o.GS_Code, Description = o.GS_FullName, AdditionalInfo = new { Email = o.GS_EmailAddress } });

				return Json(new
				{
					count = results.Count(),
					results
				});
			}
		}

		[Route("utcOffset")]
		[HttpGet]
		public IHttpActionResult GetUtcOffset()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(new { UtcOffset = Service.GetUtcOffset() });
			}
		}

		[Route("calculateStartDate")]
		[HttpPost]
		public IHttpActionResult CalculateStartDate(CalcStartDateOfAccountingPeriodData calData)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.CalcStartDateOfAccountingPeriod(calData);
				return Json(calData);
			}
		}

		[Route("deliveryRecipientTypes")]
		[HttpGet]
		public IHttpActionResult GetDeliverRecipientTypes()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetDeliverRecipientTypes();
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("deliveryMethods")]
		[HttpGet]
		public IHttpActionResult GetScheduleDeliveryMethods()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetScheduleDeliveryMethods();
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("attachmentTypes/{reportId}")]
		[HttpGet]
		public IHttpActionResult GetScheduleAttachmentTypes(Guid reportId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetScheduleAttachmentTypes(reportId);
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("printers/{currentPrinterId?}")]
		[HttpGet]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetScheduleRecipientPrinters(Guid? currentPrinterId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetScheduleRecipientPrinters(currentPrinterId ?? Guid.Empty);
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("blankReportActivities")]
		[HttpGet]
		public IHttpActionResult GetBlankReportActivities()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetBlankReportActivities();
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("emailFromAddressList/{printUserId}")]
		[HttpGet]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetEmailFromAddressList(Guid printUserId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetEmailFromAddressList(printUserId);
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("staffRecipients")]
		[HttpPost]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetStaffRecipients(ReportLookupSearchArgs args)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetStaffRecipients(args, LookupFilterDataHelper.BuildCodeDescriptionQuery).Select(o => new CodeDescription { Pk = o.PK.ToGuid(), Code = o.GS_Code, Description = o.GS_FullName });

				return Json(new
				{
					count = results.Count(),
					results
				});
			}
		}

		[Route("groups")]
		[HttpPost]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetGroups(ReportLookupSearchArgs args)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetGroups(args, LookupFilterDataHelper.BuildCodeDescriptionQuery).Select(o => new CodeDescription { Pk = o.PK.ToGuid(), Code = o.GG_Code, Description = o.GG_Desc });

				return Json(new
				{
					count = results.Count(),
					results
				});
			}
		}

		[Route("contactNames/{organizationId?}")]
		[HttpGet]
		public IHttpActionResult GetContactNames(Guid? organizationId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> results = null;
				if (IsAuthorizedOrganization(organizationId))
				{
					results = Service.GetScheduleContactNames(organizationId ?? Guid.Empty);
				}

				return Json(new
				{
					count = results?.Count ?? 0,
					results
				});
			}
		}

		[Route("organizations")]
		[HttpPost]
		public IHttpActionResult GetOrganizations(ReportLookupSearchArgs args)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> results;

				if (IsStaff)
				{
					results = LookupFilterDataHelper.RetrieveMatchesCodeDescriptions(LookupFilterDataHelper.LoadModuleDataForStaff(moduleId: ModuleIDs.Organisation, null, args) as IEnumerable<BusinessObject>);
				}
				else
				{
					results = LookupFilterDataHelper.RetrieveMatchesCodeDescriptions(Service.GetRelatedOrganizations((ZGuid)ContactPK, args, LookupFilterDataHelper.BuildCodeDescriptionQuery));
				}

				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("copyRecipientsEmails/{copyRecipientType}/{organizationId?}")]
		[HttpGet]
		public IHttpActionResult GetCopyRecipientsEmails(string copyRecipientType, Guid? organizationId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> results = null;
				if (IsAuthorizedOrganization(organizationId))
				{
					results = Service.GetCopyRecipientsEmails(copyRecipientType, organizationId ?? Guid.Empty);
				}

				return Json(new
				{
					count = results?.Count ?? 0,
					results
				});
			}
		}

		[Route("deliveryAddress")]
		[HttpPost]
		public IHttpActionResult GetDeliveryAddress(GetDeliveryAddressArgs args)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				string deliveryAddress = null;
				if (IsAuthorizedOrganization(args.OrganizationId))
				{
					deliveryAddress = Service.GetDeliveryAddress(args);
				}

				return Json(new
				{
					deliveryAddress
				});
			}
		}

		[Route("getAllowPrintUser")]
		[HttpGet]
		public IHttpActionResult GetAllowPrintUser()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = !Env.CurrentUser.IsWebUser && Env.Instance.Security.ScheduleOtherStaffAsPrintUser.IsAllowed;
				return Json(result);
			}
		}
	}

	#endregion
}
