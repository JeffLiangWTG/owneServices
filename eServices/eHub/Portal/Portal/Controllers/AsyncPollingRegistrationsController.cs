using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.DataModel.Business.Semantics;
using CargoWise.eHub.DataModel.Business.Validation;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using Common.Logging;

namespace CargoWise.eHub.Portal.Controllers
{
	public class AsyncPollingRegistrationsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("AsyncPollingRegistrationsLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult RegistrationTypes()
		{
			var regTypes = Context.eHubRegistrationTypes
				.Where(r => r.RT_RegistrantType == "AsyncPolling")
				.Select(r => new
				{
					r.RT_PK,
					r.RT_ID,
					r.RT_Description
				}).OrderBy(r => r.RT_ID);
			return Json(new { eHubRegistrationTypes = regTypes.ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult RegistrationTypeInfo(Guid regType)
		{
			return Json(new
			{
				eHubRegistrationType = Context.eHubRegistrationTypes.Select(r => new
				{
					r.RT_PK,
					r.RT_ID,
					r.RT_Description
				}).First(r => r.RT_PK == regType)
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public string GetCustomisedHeader(Guid regType, string columnName)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			return GetCustomisedHeaderForID(registrationType.RT_ID, columnName);
		}

		[HttpGet]
		public string GetCustomisedHeaderForID(string regTypeID, string columnName)
		{
			var dict = eHubPortalSemanticsFactory.GetSemantics<string, string>(regTypeID, "AsyncPollingRegistrationHeaders");

			return dict.ContainsKey(columnName) ? dict[columnName] : string.Empty;
		}

		public bool ApplyCustomisedValidation(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);

			var validation = eHubPortalValidationFactory.ApplyValidation(registrationType.RT_ID, typeof(eHubAsyncPollingRegistration));
			if (!validation)
			{
				validation = eHubPortalValidationFactory.ApplyValidation("Default", typeof(eHubAsyncPollingRegistration));
			}

			if (!validation)
			{
				return false;
			}
			return true;
		}

		protected void AddRegistrationTypeLog(string oper, eHubRegistrationType regType)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubRegistrationType: RT_PK={regType.RT_PK}, RT_RegistrantType={regType.RT_RegistrantType}, RT_ID={regType.RT_ID}, RT_Description={regType.RT_Description}");
		}

		protected void AddRegistrationLog(string oper, eHubClientRegistration rego)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubClientRegistration: CX_PK={rego.CX_PK}, CX_Qualifier={rego.CX_Qualifier}, CX_Code={rego.CX_Code}, CX_Attr1={rego.CX_Attr1}, CX_Password1={rego.CX_Password1}, CX_Flag1={rego.CX_Flag1}, CX_Flag2={rego.CX_Flag2}" +
								$", CX_CC={rego.CX_CC}, CX_RT={rego.CX_RT}");
		}

		protected void AddClientSystemRegistrationsLog(string oper, eHubClientSystemRegistration rego)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubClientSystemRegistration: CD_PK={rego.CD_PK}, CD_Qualifier={rego.CD_Qualifier}, CD_Code={rego.CD_Code}, CD_Attr1={rego.CD_Attr1}, CD_Attr2={rego.CD_Attr2}, CD_Flag1={rego.CD_Flag1}, CD_ExpiryUTC={rego.CD_ExpiryUTC?.ToString("yyyy-MM-dd hh:mm:ss.fff")}, CD_IssuedUTC={rego.CD_IssuedUTC?.ToString("yyyy-MM-dd hh:mm:ss.fff")}, CD_RT={rego.CD_RT}, CD_EH={rego.CD_EH}");

		}

		protected void AddAsyncPollingRegistrationsLog(string oper, eHubAsyncPollingRegistration rego)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubAsyncPollingRegistration: PR_PK={rego.PR_PK}, PR_CC={rego.PR_CC}, PR_EH={rego.PR_EH}, PR_RT={rego.PR_RT}, PR_Text={rego.PR_Text}" +
							   $",PR_CreatedUTC={rego.PR_CreatedUTC.ToString("yyyy-MM-dd hh:mm:ss.fff")}");

		}

		protected void AddServiceProviderRequiredRegistrationLog(string oper, eHubServiceProviderRequiredRegistration serviceProviderRequiredReg)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubServiceProviderRequiredRegistration: SX_RT={serviceProviderRequiredReg.SX_RT}, SX_SP={serviceProviderRequiredReg.SX_SP}, SX_LookupFactName={serviceProviderRequiredReg.SX_LookupFactName}, SX_QualifierFactName={serviceProviderRequiredReg.SX_QualifierFactName}");
		}

		protected void SaveLogs()
		{
			foreach (var tempLog in tempLogs)
			{
				logger.Info(() => tempLog);
			}
			tempLogs.Clear();
		}

		[HttpPost, ValidateInput(false)]
		public void RegistrationTypeInfoEdit()
		{
			var rtPK = Request["RT_PK"];
			var rtID = Request["RT_ID"];
			var rtDesc = Request["RT_Description"];
			Guid rtPKGuid = Guid.Empty;

			eHubRegistrationType regType;
			if (string.IsNullOrWhiteSpace(rtPK))
				regType = new eHubRegistrationType { RT_PK = Guid.NewGuid() };
			else
			{
				rtPKGuid = Guid.Parse(rtPK);
				regType = Context.eHubRegistrationTypes.First(t => t.RT_PK == rtPKGuid);
			}

			switch (Request["oper"])
			{
				case "add":
					if (string.IsNullOrWhiteSpace(rtID) || string.IsNullOrWhiteSpace(rtDesc)) throw new ValidationException("ID and Description are required.");
					if (Context.eHubRegistrationTypes.Any(r => r.RT_ID == rtID)) throw new ValidationException("ID is not unique.");
					regType.RT_ID = rtID;
					regType.RT_Description = rtDesc;
					regType.RT_RegistrantType = "AsyncPolling";
					Context.eHubRegistrationTypes.AddObject(regType);
					break;
				case "edit":
					if (string.IsNullOrWhiteSpace(rtID) || string.IsNullOrWhiteSpace(rtDesc)) throw new ValidationException("ID and Description are required.");
					if (Context.eHubRegistrationTypes.Any(r => r.RT_ID == rtID && r.RT_PK != rtPKGuid)) throw new ValidationException("ID is not unique.");
					regType.RT_ID = rtID;
					regType.RT_Description = rtDesc;
					break;
				case "del":
					regType.eHubClientSystemRegistrations.ToList().ForEach(r =>
					{
						Context.eHubClientSystemRegistrations.DeleteObject(r);
						AddClientSystemRegistrationsLog(Request["oper"], r);
					});

					regType.eHubClientRegistrations.ToList().ForEach(r =>
					{
						Context.eHubClientRegistrations.DeleteObject(r);
						AddRegistrationLog(Request["oper"], r);
					});
					regType.eHubAsyncPollingRegistrations.ToList().ForEach(r =>
					{
						Context.eHubAsyncPollingRegistrations.DeleteObject(r);
						AddAsyncPollingRegistrationsLog(Request["oper"], r);
					});
					regType.eHubServiceProviderRequiredRegistrations.ToList().ForEach(r =>
					{
						Context.eHubServiceProviderRequiredRegistrations.DeleteObject(r);
						AddServiceProviderRequiredRegistrationLog(Request["oper"], r);
					});
					Context.eHubRegistrationTypes.DeleteObject(regType);
					break;
			}
			Context.SaveChanges();
			AddRegistrationTypeLog(Request["oper"], regType);
			SaveLogs();
		}

		public JsonResult AsyncPollingRegistrations(Guid regType)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchField = Request["searchField"];
			string searchString = Request["searchString"];
			string searchOper = Request["searchOper"];

			var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);
			var regos = Context.eHubAsyncPollingRegistrations.Where(r => r.PR_RT == regTypeInstance.RT_PK);

			regos = ApplyValuesFilter(searchField, searchString, searchOper, regos);

			int count = regos.Count();

			regos = ApplyValuesSort(sidx, sord, regos);

			regos = regos.Skip((page - 1) * rows).Take(rows);
			var customsValueList = ConfigXmlHelper.GetAsyncPollingRegistrationsCustomsValueList(regTypeInstance, regos);

			var list = from r in regos.ToList()
					   join k in customsValueList on r.PR_PK equals k.PK
					   select new
					   {
						   PR_PK = r.PR_PK,
						   PR_EH_ID = r.eHubClientSystem?.EH_ID,
						   PR_CC_ID = r.eHubClient?.CC_ID,
						   PR_Text = r.PR_Text,
						   PR_XML = r.PR_PK,
						   PR_CreatedUTC = r.PR_CreatedUTC.ToString("yyyy-MM-dd hh:mm:ss"),
						   CustomValue1 = k.CustomValue1,
						   CustomValue2 = k.CustomValue2
					   };

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				AsyncPollingRegistrations = list.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public FileContentResult DownloadConfiguration(Guid PR_PK)
		{
			var isTest = Setting.ShowHidden();
			if (!isTest) return null;

			if (PR_PK == Guid.Empty)
				return null;

			var rego = Context.eHubAsyncPollingRegistrations.First(r => r.PR_PK == PR_PK);
			if (rego.PR_XML == null) return null;
			var fileName = rego.PR_PK.ToString();
			return File(Encoding.Default.GetBytes(rego.PR_XML), "text/text", fileName + ".xml");
		}

		[HttpPost]
		public JsonResult UploadConfiguration()
		{
			try
			{
				ConfigXmlHelper.UploadConfiguration(Context, Request["registrationPK"], Request["registrationTypePK"], Request.Files["uploadconfigxmlFile"].InputStream);
				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ExceptionHelper.GetExceptionMessages(ex) },
					JsonRequestBehavior.AllowGet);
			}
		}



		static IQueryable<eHubAsyncPollingRegistration> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<eHubAsyncPollingRegistration> regos)
		{
			switch (searchField)
			{
				case "PR_EH_ID":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.eHubClientSystem.EH_ID == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.eHubClientSystem.EH_ID.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.eHubClientSystem.EH_ID.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.eHubClientSystem.EH_ID.Contains(searchString));
							break;
					}
					break;
				case "PR_CC_ID":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.eHubClient.CC_ID == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.eHubClient.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.eHubClient.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.eHubClient.CC_ID.Contains(searchString));
							break;
					}
					break;
				case "PR_Text":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.PR_Text == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.PR_Text.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.PR_Text.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.PR_Text.Contains(searchString));
							break;
					}
					break;
			}
			return regos;
		}

		static IQueryable<eHubAsyncPollingRegistration> ApplyValuesSort(string sidx, string sord, IQueryable<eHubAsyncPollingRegistration> regos)
		{
			switch (sidx + " " + sord)
			{
				case "PR_EH_ID asc":
					regos = regos.OrderBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.PR_Text);
					break;
				case "PR_EH_ID desc":
					regos = regos.OrderByDescending(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.PR_Text);
					break;
				case "PR_CC_ID asc":
					regos = regos.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.PR_Text);
					break;
				case "PR_CC_ID desc":
					regos = regos.OrderByDescending(r => r.eHubClient.CC_ID).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.PR_Text);
					break;
				case "PR_Text asc":
					regos = regos.OrderBy(r => r.PR_Text).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID);
					break;
				case "PR_Text desc":
					regos = regos.OrderByDescending(r => r.PR_Text).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID);
					break;
				default:
					regos = regos.OrderBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.PR_Text);
					break;
			}
			return regos;
		}

		[HttpPost]
		public JsonResult AsyncPollingRegistrationsEdit(Guid regType)
		{
			var hasValidator = ApplyCustomisedValidation(regType);
			try
			{
				var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);

				var oper = Request["oper"];
				var pk = Request["PR_PK"] ?? Request["id"];
				var clientSystemId = Request["PR_EH_ID"];
				var clientId = Request["PR_CC_ID"];
				var staff = Request["PR_Text"];
				var customValue1 = string.IsNullOrEmpty(Request["CustomValue1"]) ? null : Request["CustomValue1"];
				var customValue2 = string.IsNullOrEmpty(Request["CustomValue2"]) ? null : Request["CustomValue2"];
				var configXml = Request.Files.Count > 0 ? Request.Files["pr_inputFile"].InputStream : null;

				eHubAsyncPollingRegistration rego;
				if (string.IsNullOrWhiteSpace(pk) || pk == "_empty")
					rego = new eHubAsyncPollingRegistration();
				else
				{
					var pkguid = new Guid(pk);
					rego = regTypeInstance.eHubAsyncPollingRegistrations.First(r => r.PR_PK == pkguid);
				}

				switch (oper)
				{
					case "add":
						if (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSystemId)) throw new ValidationException("Client or Client System is required.");
						rego.PR_PK = Guid.NewGuid();
						rego.eHubRegistrationType = regTypeInstance;
						rego.eHubClientSystem = Context.eHubClientSystems?.FirstOrDefault(c => c.EH_ID == clientSystemId);
						rego.eHubClient = Context.eHubClients?.FirstOrDefault(c => c.CC_ID == clientId);
						rego.PR_Text = staff;
						rego.PR_CreatedUTC = DateTime.UtcNow;
						rego.PR_XML = ConfigXmlHelper.GetConfigFromFile(configXml);
						Context.eHubAsyncPollingRegistrations.AddObject(rego);
						break;
					case "edit":
						if (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSystemId)) throw new ValidationException("Client or Client System is required.");
						rego.eHubClientSystem = Context.eHubClientSystems?.FirstOrDefault(c => c.EH_ID == clientSystemId);
						rego.eHubClient = Context.eHubClients?.First(c => c.CC_ID == clientId);
						rego.PR_Text = staff;
						rego.PR_XML = ConfigXmlHelper.UpdateRegistrationConfigXml(rego.PR_XML, regTypeInstance, customValue1, customValue2);
						break;
					case "del":
						Context.eHubAsyncPollingRegistrations.DeleteObject(rego);
						break;
				}
				if (hasValidator)
				{
					TryUpdateModel<eHubAsyncPollingRegistration>(rego);
					if (ModelState.IsValid)
					{
						Context.SaveChanges();
						AddAsyncPollingRegistrationsLog(oper, rego);
						SaveLogs();
						return Json(new { success = true, id = rego.PR_PK }, JsonRequestBehavior.AllowGet);
					}

					var message = string.Join("<br/>", ModelState.Where(state => state.Value.Errors.Count > 0).Select(state => string.Join("<br/>", state.Value.Errors.Select(error => error.ErrorMessage))));
					return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
				}

				Context.SaveChanges();
				AddAsyncPollingRegistrationsLog(oper, rego);
				SaveLogs();
				return Json(new { success = true, id = rego.PR_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "") }, JsonRequestBehavior.AllowGet);
			}
		}

		public JsonResult Clients(bool includeNonProd)
		{
			var page = Convert.ToInt32(Request["page"]);
			var rows = Convert.ToInt32(Request["rows"]);
			var sidx = Request["sidx"];
			var sord = Request["sord"];
			var filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if (includeNonProd)
			{
				clients = Context.eHubClients.Select(c => new
				{
					c.CC_ID,
					c.CC_FriendlyName
				});
			}
			else
			{
				clients = from c in Context.eHubClients
				join p in Context.ediProdClients on c.CC_PK equals p.CC_PK into joinedClients
				from p in joinedClients.DefaultIfEmpty()
				where p == null || p.LD_LicenceType == "PRD"
				select new { c.CC_ID, c.CC_FriendlyName };
			}

			if (filtered)
			{
				var ccId = Request["CC_ID"];
				var ccName = Request["CC_FriendlyName"];
				if (!string.IsNullOrWhiteSpace(ccId))
					clients = clients.Where(c => c.CC_ID.StartsWith(ccId, StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrWhiteSpace(ccName))
					clients = clients.Where(c => c.CC_FriendlyName.Contains(ccName));
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

		public JsonResult ClientSystems(bool includeNonProd)
		{
			var page = Convert.ToInt32(Request["page"]);
			var rows = Convert.ToInt32(Request["rows"]);
			var sidx = Request["sidx"];
			var sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if(includeNonProd)
			{
				clients = Context.eHubClientSystems.Select(c => new
				{
					c.EH_ID,
					c.EH_URL
				});
			}
			else
			{
				clients = from sys in Context.eHubClientSystems
				join prodClient in Context.ediProdClients
				on sys.EH_ID equals prodClient.EnterpriseServerCode
				where prodClient == null || prodClient.LD_LicenceType == "PRD"
				select new { sys.EH_ID, sys.EH_URL };
			}

			if (filtered)
			{
				var id = Request["EH_ID"];
				var url = Request["EH_URL"];
				if (!string.IsNullOrWhiteSpace(id))
					clients = clients.Where(c => c.EH_ID.StartsWith(id, StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrWhiteSpace(url))
					clients = clients.Where(c => c.EH_URL.Contains(url));
			}

			int count = clients.Count();

			switch (sidx + " " + sord)
			{
				case "EH_ID asc":
					clients = clients.OrderBy(c => c.EH_ID).ThenBy(c => c.EH_URL);
					break;
				case "EH_ID desc":
					clients = clients.OrderByDescending(c => c.EH_ID).ThenBy(c => c.EH_URL);
					break;
				case "EH_URL asc":
					clients = clients.OrderBy(c => c.EH_URL).ThenBy(c => c.EH_ID);
					break;
				case "EH_URL desc":
					clients = clients.OrderByDescending(c => c.EH_URL).ThenBy(c => c.EH_ID);
					break;
				default:
					clients = clients.OrderBy(c => c.EH_ID).ThenBy(c => c.EH_URL);
					break;
			}

			return Json(new
			{
				page,
				total = Math.Ceiling(count / (double)rows),
				records = count,
				eHubClientSystems = clients.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}
	}
}
