using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using Common.Logging;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ClientAuthorisationController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("ClientAuthorisationLogger");

		public JsonResult GetClientAuthorisation()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rowNo = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchField = Request["searchField"];
			string searchString = Request["searchString"];
			string searchOper = Request["searchOper"];

			IQueryable<eHubClientAuthorisation> clientAuths = Context.eHubClientAuthorisations;

			clientAuths = ApplyValuesFilter(searchField, searchString, searchOper, clientAuths);

			int count = clientAuths.Count();

			clientAuths = ApplyValuesSort(sidx, sord, clientAuths);

			clientAuths = clientAuths.Skip((page - 1) * rowNo).Take(rowNo);
			var rows = clientAuths.Select(r => new
			{
				CA_CC_Sender = r.eHubClientSender.CC_ID,
				CA_CC_Recipient = r.eHubClientRecipient.CC_ID,
				CA_CreatedUTC = r.CA_CreatedUTC,
				id = r.eHubClientSender.CC_ID + "|" + r.eHubClientRecipient.CC_ID
			});

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rowNo),
				records = count,
				eHubClientAuthorisations = rows.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		static IQueryable<eHubClientAuthorisation> ApplyValuesFilter(string searchField, string searchString,
			string searchOper, IQueryable<eHubClientAuthorisation> clientAuthorisations)
		{
			switch (searchField)
			{
				case "CA_CC_Sender":
					switch (searchOper)
					{
						case "eq":
							clientAuthorisations = clientAuthorisations.Where(r => r.eHubClientSender.CC_ID == searchString);
							break;
						case "bw":
							clientAuthorisations =
								clientAuthorisations.Where(r => r.eHubClientSender.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							clientAuthorisations =
								clientAuthorisations.Where(r => r.eHubClientSender.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							clientAuthorisations =
								clientAuthorisations.Where(r => r.eHubClientSender.CC_ID.Contains(searchString));
							break;
					}

					break;
				case "CA_CC_Recipient":
					switch (searchOper)
					{
						case "eq":
							clientAuthorisations = clientAuthorisations.Where(r => r.eHubClientRecipient.CC_ID == searchString);
							break;
						case "bw":
							clientAuthorisations =
								clientAuthorisations.Where(r => r.eHubClientRecipient.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							clientAuthorisations =
								clientAuthorisations.Where(r => r.eHubClientRecipient.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							clientAuthorisations =
								clientAuthorisations.Where(r => r.eHubClientRecipient.CC_ID.Contains(searchString));
							break;
					}

					break;
			}

			return clientAuthorisations;
		}

		static IQueryable<eHubClientAuthorisation> ApplyValuesSort(string sidx, string sord,
			IQueryable<eHubClientAuthorisation> regos)
		{
			switch (sidx + " " + sord)
			{
				case "CA_CC_Sender asc":
					regos = regos.OrderBy(r => r.eHubClientSender.CC_ID).ThenBy(r => r.eHubClientRecipient.CC_ID);
					break;
				case "CA_CC_Sender desc":
					regos = regos.OrderByDescending(r => r.eHubClientSender.CC_ID).ThenBy(r => r.eHubClientRecipient.CC_ID);
					break;
				default:
					regos = regos.OrderBy(r => r.eHubClientSender.CC_ID).ThenBy(r => r.eHubClientRecipient.CC_ID);
					break;
			}

			return regos;
		}

		[HttpPost]
		public JsonResult ClientAuthorisationEdit()
		{
			try
			{
				var oper = Request["oper"];
				var id = Request["id"];
				var senderID = Request["CA_CC_Sender"];
				var recepientID = Request["CA_CC_Recipient"];

				eHubClientAuthorisation clientAuth;
				clientAuth = new eHubClientAuthorisation();

				if (String.IsNullOrWhiteSpace(id) || id == "_empty")
				{
					clientAuth = new eHubClientAuthorisation();
				}
				else
				{
					var sender = id.Substring(0, 9);
					var recipient = id.Substring(10, 9);
					clientAuth = Context.eHubClientAuthorisations.FirstOrDefault(x => x.eHubClientSender.CC_ID == sender && x.eHubClientRecipient.CC_ID == recipient);
				}


				switch (oper)
				{
					case "add":
						clientAuth.eHubClientSender = Context.eHubClients.First(x => x.CC_ID == senderID);
						clientAuth.eHubClientRecipient = Context.eHubClients.First(x => x.CC_ID == recepientID);
						clientAuth.CA_CreatedUTC = DateTime.UtcNow;
						Context.eHubClientAuthorisations.AddObject(clientAuth);
						break;
					case "del":
						Context.eHubClientAuthorisations.DeleteObject(clientAuth);
						break;
					default:
						break;
				}

				Context.SaveChanges();
				AddClientAuthorisationLog(oper, clientAuth, id);

				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				var message = ex.InnerException != null ? ex.Message + " " + ex.InnerException.Message : ex.Message;
				return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
			}
		}

		protected void AddClientAuthorisationLog(string oper, eHubClientAuthorisation clientAuthorisation, string id)
		{
			var sender = clientAuthorisation.eHubClientSender == null ? id.Substring(0, 9) : clientAuthorisation.eHubClientSender.CC_ID;
			var recipient = clientAuthorisation.eHubClientRecipient == null ? id.Substring(10, 9) : clientAuthorisation.eHubClientRecipient.CC_ID;
			try
			{
				PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "wtg.zone");
				UserPrincipal user = UserPrincipal.FindByIdentity(ctx, HttpContext.User.Identity.Name);
				logger.Info($"[{user}] - [{oper}]");
			}
			catch (Exception exception)
			{
				logger.Error($"Get current user information failed with exception {exception}");
			}
			logger.Info($"[{oper}] ClientAuthorisation: CA_CC_Sender={sender}, CA_CC_Recipient={recipient}");
		}

		public JsonResult Clients()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			var clients = Context.eHubClients.Select(c => new
			{
				CC_ID = c.CC_ID,
				CC_FriendlyName = c.CC_FriendlyName
			});
			if (filtered)
			{
				string ccid = Request["CC_ID"];
				string ccname = Request["CC_FriendlyName"];
				if (!String.IsNullOrWhiteSpace(ccid))
					clients = clients.Where(c => c.CC_ID.StartsWith(ccid));
				if (!String.IsNullOrWhiteSpace(ccname))
					clients = clients.Where(c => c.CC_FriendlyName.Contains(ccname));
			}

			int count = clients.Count();

			switch (sidx + " " + sord)
			{
				case "CC_ID asc":
					clients = clients.OrderBy(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
					break;
				case "CC_ID desc":
					clients = clients.OrderByDescending(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
					break;
				case "CC_FriendlyName asc":
					clients = clients.OrderBy(c => c.CC_FriendlyName).ThenBy(c => c.CC_ID);
					break;
				case "CC_FriendlyName desc":
					clients = clients.OrderByDescending(c => c.CC_FriendlyName).ThenBy(c => c.CC_ID);
					break;
				default:
					clients = clients.OrderBy(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
					break;
			}

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClients = clients.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}
	}
}

