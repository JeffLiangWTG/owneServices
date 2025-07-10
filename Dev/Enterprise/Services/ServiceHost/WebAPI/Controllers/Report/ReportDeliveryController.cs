using System;
using System.Collections.Generic;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	[GlowTicketAuthentication]
	[ReportServiceErrorHandler]
	public class ReportDeliveryController : ReportDataBaseController
	{
		[Route("api/report/delivery/onlineprinters")]
		[HttpGet]
		public IHttpActionResult GetOnlinePrinters()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetOnlinePrinters());
			}
		}

		[Route("api/report/delivery/methods")]
		[HttpGet]
		public IHttpActionResult GetDeliveryMethods()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetDeliveryMethods());
			}
		}

		[Route("api/report/delivery/contactNames/{organizationId?}")]
		[HttpGet]
		public IHttpActionResult GetContactNames(Guid? organizationId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> results = null;
				if (IsAuthorizedOrganization(organizationId))
				{
					results = Service.GetDeliveryContactNames(organizationId ?? Guid.Empty);
				}

				return Json(new
				{
					count = results?.Count ?? 0,
					results
				});
			}
		}

		[Route("api/report/delivery/organizations")]
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

		[Route("api/report/delivery/contactEmails/{copyRecipientType}/{organizationID?}")]
		[HttpGet]
		public IHttpActionResult GetContactEmails(string copyRecipientType, Guid? organizationId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> results = null;
				if (IsAuthorizedOrganization(organizationId))
				{
					results = Service.GetContactEmails(copyRecipientType, organizationId ?? Guid.Empty);
				}

				return Json(new
				{
					count = results?.Count ?? 0,
					results
				});
			}
		}

		[Route("api/report/delivery/contactEmail/{contactName}/{organizationID?}")]
		[HttpGet]
		public IHttpActionResult GetContactEmail(string contactName, Guid? organizationId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				string contactEmail = null;
				if (IsAuthorizedOrganization(organizationId))
				{
					contactEmail = Service.GetContactEmail(contactName, organizationId ?? Guid.Empty);
				}

				return Json(new
				{
					contactEmail
				});
			}
		}

		[Route("api/report/delivery/attachmenttypes/{reportId?}")]
		[HttpGet]
		public IHttpActionResult GetAttachmentTypes(Guid? reportId = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var results = Service.GetAttachmentTypes(reportId);
				return Json(new
				{
					count = results.Count,
					results
				});
			}
		}

		[Route("api/report/delivery/salutations")]
		[HttpGet]
		public IHttpActionResult GetSalutations()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetSalutations());
			}
		}

		[Route("api/report/delivery")]
		[HttpPost]
		public IHttpActionResult DeliverReport(DeliveryData reportData)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.DeliverReport(reportData);

				return Ok();
			}
		}
	}

	#endregion
}
