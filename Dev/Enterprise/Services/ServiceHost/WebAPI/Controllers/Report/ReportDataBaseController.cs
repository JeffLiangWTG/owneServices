using System;
using System.Linq;
using System.Web.Http;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;

namespace Enterprise.Services.ServiceHost
{
	public abstract class ReportDataBaseController : ApiController
	{
		protected ZGuid? ContactPK
		{
			get
			{
				if (User?.Identity is IGlowAuthenticationTicketIdentity identity)
				{
					return identity.GetContactPK();
				}

				return null;
			}
		}

		protected bool IsStaff => GlowPrincipalHelper.IsStaff(User);

		protected bool IsAuthorizedOrganization(Guid? orgID)
		{
			return IsStaff || Service.GetRelatedOrganizationIDs(ContactPK ?? ZGuid.Empty).Any(o => o.ToGuid() == orgID);
		}

		IReportDataService service;
		public virtual IReportDataService Service => service ??= GetService();

		IReportDataService GetService()
		{
			var service = new ReportDataService();
			service.ContactPk = ContactPK;
			return service;
		}
	}
}
