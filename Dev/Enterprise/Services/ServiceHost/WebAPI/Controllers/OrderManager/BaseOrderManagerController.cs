using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Forwarding;
using IJobSupplierBooking = Enterprise.Integration.Forwarding.IJobSupplierBooking;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public abstract class OrderManagerController : ApiController
	{
		protected readonly IGlowContactSecurityService securityService;

		public OrderManagerController(IGlowContactSecurityService securityService)
		{
			this.securityService = securityService;
		}

		public OrderManagerController() : this(new GlowContactSecurityService())
		{
		}

		protected bool CanEdit<T>(T entity, IPrincipal user, IGlowContactSecurityService securityService) => GetEntityFromTVF(entity, SecurityQueryType.Edit, user, securityService, out _);
		protected bool CanView<T>(T entity, IPrincipal user, IGlowContactSecurityService securityService, out bool isContact) => GetEntityFromTVF(entity, SecurityQueryType.View, user, securityService, out isContact);

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Will refactor in future WI")]
		bool GetEntityFromTVF<T>(T entity, SecurityQueryType securityQueryType, IPrincipal user, IGlowContactSecurityService securityService, out bool isContact)
		{
			isContact = false;
			if (user?.Identity is IGlowAuthenticationTicketIdentity identity)
			{
				if (identity.ProviderType == OrgContactSchema.Constants.Prefix)
				{
					var contactOrganisation = securityService.GetContactOrganisation(identity);
					var businessObject = entity as BusinessObject;
					isContact = true;

					if (contactOrganisation != null && businessObject.PK.IsValid)
					{
						var (tvfName, pkColumnName) = GetTVFAndPKColumn(entity);
						var selector = securityQueryType == SecurityQueryType.Edit ? "CanEdit" : "COUNT(*)"; // SQL query.
						var query = $"SELECT {selector} FROM {tvfName}(@loggedInContactOrganisation) WHERE {pkColumnName} = @entityPK"; // SQL query.

						using (var command = Db.Connection.Command(query)) // CanEdit not accessible from BizO.
						{
							command.AddParameter("@loggedInContactOrganisation", SqlDbType.UniqueIdentifier, contactOrganisation.PK.ToGuid());
							command.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, businessObject.PK.ToGuid());

							return Convert.ToBoolean(command.ExecuteScalar());
						}
					}
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		static (string tvfName, string pkColumnName) GetTVFAndPKColumn<T>(T entity)
		{
			if (entity is IOrder)
			{
				return ("dbo.GetJobOrderHeadersWithSecurityContext", "JD_PK");
			}
			else if (entity is IJobSupplierBooking)
			{
				return ("dbo.GetJobSupplierBookingsWithSecurityContext", "JSB_PK");
			}
			else if (entity is ICYContainerLoadList)
			{
				return ("dbo.GetCYContainerLoadListsWithSecurityContext", "CLH_PK");
			}
			else if (entity is ICFSContainerLoadList)
			{
				return ("dbo.GetCFSContainerLoadListsWithSecurityContext", "CLH_PK");
			}
			else if (entity is ICommonContainerLoadList containerLoadList)
			{
				string containerLoadListTVF;

				switch (containerLoadList.CLH_LoadMode)
				{
					case ContainerLoadListHeaderLoadMode.ContainerYard:
						containerLoadListTVF = "dbo.GetCYContainerLoadListsWithSecurityContext";
						break;
					case ContainerLoadListHeaderLoadMode.ContainerFreightStation:
						containerLoadListTVF = "dbo.GetCFSContainerLoadListsWithSecurityContext";
						break;
					default:
						throw new NotSupportedException(nameof(CommonContainerLoadList.CLH_LoadMode));
				}

				return (containerLoadListTVF, "CLH_PK");
			}

			throw new NotSupportedException(nameof(entity));
		}

		protected bool HasValidationErrors(BusinessObject businessObject, out List<string> errorsList)
		{
			errorsList = new List<string>();

			if (businessObject.HasErrors)
			{
				var errorNotifications = businessObject.Notifications.Where(x => x.Type == NotificationType.Error).Select(x => x.Message);
				errorsList = errorNotifications.ToList();

				return true;
			}

			return false;
		}

		enum SecurityQueryType {
			View,
			Edit
		}

		protected static string SomethingWentWrong => Res.GetString("e48254ee-68de-4c7f-9db5-ea4d14115257", "Something went wrong with creating this exception. Please contact your system administrator.");
		protected static string SecurityError => Res.GetString("7dcc1681-4851-4f07-b5fe-a5ea4112d9fa", "You do not have permission to perform this action. Please contact your system administrator.");
	}
}
