using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Common;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using MvcContrib.Pagination;
using MvcContrib.Sorting;
using MvcContrib.UI.Grid;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ClientController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("ClientLogger");

		public ActionResult Index(string id, string Name, string AS2Code, bool? IsAirline, string AirlineCode, string AirlinePrefix, GridSortOptions gridSortOptions, int? page)
		{
			var list = Context.GetClientQuery();

			// Set default sort column
			if (string.IsNullOrWhiteSpace(gridSortOptions.Column))
			{
				gridSortOptions.Column = "CC_ID";
			}

			// Filter on Cient ID 
			if (!String.IsNullOrEmpty(id))
			{
				list = list.Where(c => c.CC_ID.Contains(id));
			}

			// Filter on Cient Name
			if (!String.IsNullOrEmpty(Name))
			{
				list = list.Where(c => c.CC_FriendlyName.Contains(Name));
			}

			// Filter on AS2 Code
			if (!String.IsNullOrEmpty(AS2Code))
			{
				list = list.Where(c => c.CC_AS2_Code.Contains(AS2Code));
			}

			// Filter on IsAirline
			if (IsAirline.HasValue && IsAirline.Value)
			{
				list = list.Where(c => c.CC_AirlineCode != null);
			}

			// Filter on AirlineCode
			if (!String.IsNullOrEmpty(AirlineCode))
			{
				list = list.Where(c => c.CC_AirlineCode == AirlineCode);
			}

			// Filter on AirlinePrefix
			if (!String.IsNullOrEmpty(AirlinePrefix))
			{
				list = list.Where(c => c.CC_AirlinePrefix == AirlinePrefix);
			}

			var filterViewModel = new ClientFilterViewModel();
			filterViewModel.ID = id;
			filterViewModel.Name = Name;
			filterViewModel.AS2Code = AS2Code;
			if (IsAirline.HasValue) filterViewModel.IsAirline = IsAirline.Value;

			// Order and page the product list
			var pagedList = list.OrderBy(gridSortOptions.Column, gridSortOptions.Direction).AsPagination(page ?? 1, Constants.PageSize);


			var clientListContainer = new ClientListContainerViewModel
			{
				PagedList = pagedList,
				FilterViewModel = filterViewModel,
				GridSortOptions = gridSortOptions
			};

			return View(clientListContainer);
		}

		public ActionResult Find(string text)
		{
			if (String.IsNullOrEmpty(text) || text.Length < Constants.MinLettersToStartClientSearch)
			{
				return Json(new SelectValueView[0], JsonRequestBehavior.AllowGet);
			}

			return Json(Context.FindClientViewList(text), JsonRequestBehavior.AllowGet);
		}

		public ActionResult DetailsModal(Guid id)
		{
			return ModalRedirect(Details(id));
		}

		public ActionResult Details(Guid id)
		{
			var client = Context.GetClientView(id);
			if (client.Client == null) return View("NotFound");
			return View("Details", client);
		}

		public ActionResult CreateModal(string text)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return ModalRedirect(Create(text));
		}

		[HttpPost]
		public ActionResult CreateModal(FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return ModalRedirect(Create(formValues));
		}

		public ActionResult Create(string text)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var client = CreateClientView();
			if (text != null) client.Client.CC_FriendlyName = text;
			return View("Edit", client);
		}

		[HttpPost]
		public ActionResult Create(FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var client = CreateClientView();
			Context.eHubClients.AddObject(client.Client);
			return EditClient(formValues, client);
		}

		public ActionResult UnauthorisedAccess()
		{
			return View();
		}

		private bool AllowAccountAuthorisation()
		{
			try
			{
				if (Setting.ShowHidden())
				{
					return true;
				}

				var allowedGroup = System.Configuration.ConfigurationManager.AppSettings["AccountAuthorisationGroup"] ?? "g_ehubportaladmins";
				PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "wtg.zone");
				UserPrincipal user = UserPrincipal.FindByIdentity(ctx, HttpContext.User.Identity.Name);
				var group = GroupPrincipal.FindByIdentity(ctx, allowedGroup);
				return user.IsMemberOf(group);
			}
			catch (Exception)
			{
				AddAuthorisationLog(HttpContext.User.Identity.Name);
				return false;
			}
		}

		protected void AddAuthorisationLog(string user)
		{
			logger.Info(() => $"Access denied to IntegrationAccountAuthorisations for [{user}]");
		}

		protected void AddClientLog(string oper, eHubClient client)
		{
			logger.Info(() => $"[{oper}] eHubClient: CC_PK={client.CC_PK}, CC_ID={client.CC_ID}, CC_FriendlyName={client.CC_FriendlyName}, CC_Odyssey_OH={client.CC_Odyssey_OH}, CC_DistributionZone={client.CC_DistributionZone}" +
			$", CC_EmailAddress={client.CC_EmailAddress}, CC_Password={client.CC_Password}, CC_IsAirServiceProvider={client.CC_IsAirServiceProvider}, CC_AirlineCode={client.CC_AirlineCode}, CC_AirServiceProvider={client.CC_AirServiceProvider}" +
			$", CC_AirlinePrefix={client.CC_AirlinePrefix}, CC_USCustomsRecipient={client.CC_USCustomsRecipient}, CC_AS2_Code={client.CC_AS2_Code}, CC_SCAC_Code={client.CC_SCAC_Code}, CC_OwnerCategory={client.CC_OwnerCategory}" +
			$", CC_SystemCategory={client.CC_SystemCategory}, CC_RR={client.CC_RR}, CC_RequireStatusResponse={client.CC_RequireStatusResponse}, CC_NotificationForInboxRecipient={client.CC_NotificationForInboxRecipient}");
		}

		public ActionResult IntegrationAccountAuthorisations()
		{
			return AllowAccountAuthorisation() ? (ActionResult)View() : RedirectToAction("UnauthorisedAccess");
		}

		public ActionResult CreateThirdPartyPartner()
		{
			if (AllowAccountAuthorisation())
			{
				return View(new ClientThirdPartyPartner());
			}
			else
			{
				return RedirectToAction("UnauthorisedAccess");
			}
		}

		[HttpPost]
		public ActionResult CreateThirdPartyPartner(ClientThirdPartyPartner clientForm)
		{
			// Does the eHubClient ID already exist?
			if (Context.eHubClients.Where(x => x.CC_ID == clientForm.Id).FirstOrDefault() != null)
			{
				ModelState.AddModelError("Id", string.Format("eHubClient with code '{0}' already exists", clientForm.Id));
				return View(clientForm);
			}

			// Let's find the Org in ediProd
			var org = Context.ediProdAllOrgs.Where(x => x.OH_Code == clientForm.OrgCode).FirstOrDefault();
			if (org == null)
			{
				ModelState.AddModelError("OrgCode", string.Format("Organisation with code '{0}' could not be found in ediProd", clientForm.OrgCode));
				return View(clientForm);
			}

			// So far so good, let's generate the password and create the new eHubClient
			var password = SHA512Encryptor.Encrypt(clientForm.Id + clientForm.Password);
			var client = new eHubClient()
			{
				CC_PK = Guid.NewGuid(),
				CC_ID = clientForm.Id,
				CC_Password = password,
				CC_Odyssey_OH = org.OH_PK,
				CC_FriendlyName = org.OH_FullName,
				CC_EmailAddress = clientForm.Email,
				CC_OwnerCategory = "Partner",
				CC_SystemCategory = "Third Party"
			};

			Context.eHubClients.AddObject(client);

			try
			{
				Context.SaveChanges();
				AddClientLog("add", client);
				TempData["Success"] = "eHubClient successfully created";
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}

			return RedirectToAction("IntegrationAccountAuthorisations");
		}

		public ActionResult Edit(Guid id)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var clientView = Context.GetClientView(id);
			return View("Edit", clientView);
		}

		[HttpPost]
		public ActionResult Edit(Guid id, FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var client = Context.GetClientView(id);
			return EditClient(formValues, client);
		}

		public ActionResult Delete(Guid id)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var client = Context.GetClient(id);

			if (client == null) return View("NotFound");

			return View("Delete", client);
		}

		[HttpPost]
		public ActionResult Delete(Guid id, string confirmButton)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var client = Context.GetClient(id);
			if (client == null) return View("NotFound");
			Context.eHubClients.DeleteObject(client);

			try
			{
				Context.SaveChanges();
				AddClientLog("del", client);
				return View("Deleted");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", ex.InnerException.Message);
				return View("Delete", client);
			}
		}

		#region Implementation

		ActionResult EditClient(FormCollection formValues, ClientView client)
		{
			UpdateClient(client.Client, formValues);

			if (ModelState.IsValid)
			{
				try
				{
					Context.SaveChanges();
					AddClientLog("edit", client.Client);
					return RedirectToAction("Details", new { id = client.Client.CC_PK });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.InnerException.Message);
					return View("Edit", client);
				}

			}
			else
			{
				return View("Edit", client);
			}
		}

		void UpdateClient(Models.eHubTransactions.eHubClient client, FormCollection formValues)
		{
			client.CC_ID = formValues["Client.CC_ID"];
			client.CC_FriendlyName = formValues["Client.CC_FriendlyName"];
			client.CC_EmailAddress = formValues["Client.CC_EmailAddress"];
			client.CC_Password = formValues["Client.CC_Password"];

			string airlineCode = formValues["Client.CC_AirlineCode"];
			client.CC_AirlineCode = string.IsNullOrWhiteSpace(airlineCode) ? null : airlineCode;
			string airlinePrefix = formValues["Client.CC_AirlinePrefix"];
			client.CC_AirlinePrefix = string.IsNullOrWhiteSpace(airlinePrefix) ? null : airlinePrefix;
			string as2Code = formValues["Client.CC_AS2_Code"];
			client.CC_AS2_Code = string.IsNullOrWhiteSpace(as2Code) ? null : as2Code;

			Guid id;
			client.CC_Odyssey_OH = Guid.Empty;
			if (Guid.TryParse(formValues["Client.CC_Odyssey_OH"], out id))
			{
				client.CC_Odyssey_OH = id;
			}

			client.CC_DistributionZone = null;
			if (Guid.TryParse(formValues["Client.CC_DistributionZone"], out id))
			{
				client.CC_DistributionZone = id;
			}

			client.CC_IsAirServiceProvider = null;
			bool value;
			if (Boolean.TryParse(formValues["Client.CC_IsAirServiceProvider"], out value))
			{
				if (value) client.CC_IsAirServiceProvider = value;
			}

			client.CC_AirServiceProvider = null;
			if (Guid.TryParse(formValues["Client.CC_AirServiceProvider"], out id))
			{
				client.CC_AirServiceProvider = id;
			}

			TryUpdateModel<eHubClient>(client);
		}

		ClientView CreateClientView()
		{
			var client = ClientExtensions.CreateClient();
			var clientView = new ClientView(client);
			clientView.DistributionZone = Context.GetZoneQuery();
			clientView.AirServiceProvider = Context.GetAllAirServiceProvider();
			return clientView;
		}

		#endregion
	}
}

