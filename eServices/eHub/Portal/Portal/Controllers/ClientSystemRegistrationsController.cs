using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.DataModel.Business;
using CargoWise.eHub.DataModel.Business.Semantics;
using CargoWise.eHub.DataModel.Business.Validation;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using Newtonsoft.Json;
namespace CargoWise.eHub.Portal.Controllers
{
	public class ClientSystemRegistrationsController : ControllerBase
	{

		public ILog logger = LogManager.GetLogger("ClientSystemRegistrationsLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult RegistrationTypes()
		{
			var regTypes = Context.eHubRegistrationTypes
				.Where(r => r.RT_RegistrantType == "ClientSystem")
				.Select(r => new
				{
					RT_PK = r.RT_PK,
					RT_ID = r.RT_ID,
					RT_Description = r.RT_Description
				}).OrderBy(r => r.RT_ID);
			return Json(new { eHubRegistrationTypes = regTypes.ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult RegistrationTypeInfo(Guid regType)
		{
			return Json(new
			{
				eHubRegistrationType = Context.eHubRegistrationTypes.Select(r => new
				{
					RT_PK = r.RT_PK,
					RT_ID = r.RT_ID,
					RT_Description = r.RT_Description
				}).First(r => r.RT_PK == regType)
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public String GetStatusDescriptionList(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			var list = ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus(registrationType.RT_ID);
			var defaultString = string.Format("{0}:Not Applicable", (byte?)null);

			return list.Any() ? string.Join(";", list.Select(x => x.Key + ":" + x.Value)) : defaultString;
		}

		[HttpGet]
		public String GetCustomisedHeader(Guid regType, string columnName)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			return GetCustomisedHeaderForID(registrationType.RT_ID, columnName);
		}

		[HttpGet]
		public String GetCustomisedHeaderForID(string regTypeID, string columnName)
		{
			var dict = eHubPortalSemanticsFactory.GetSemantics<string, string>(regTypeID, "ClientSystemRegistrationHeaders");

			return dict.ContainsKey(columnName) ? dict[columnName] : string.Empty;
		}

		public bool ApplyCustomisedValidation(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			return eHubPortalValidationFactory.ApplyValidation(registrationType.RT_ID, typeof(eHubClientSystemRegistration));
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

		[HttpPost]
		public void RegistrationTypeInfoEdit()
		{
			var rtPK = Request["RT_PK"];
			var rtID = Request["RT_ID"];
			var rtDesc = Request["RT_Description"];
			Guid rtPKGuid = Guid.Empty;

			eHubRegistrationType regType;
			if (String.IsNullOrWhiteSpace(rtPK))
				regType = new eHubRegistrationType { RT_PK = Guid.NewGuid() };
			else
			{
				rtPKGuid = Guid.Parse(rtPK);
				regType = Context.eHubRegistrationTypes.First(t => t.RT_PK == rtPKGuid);
			}

			switch (Request["oper"])
			{
				case "add":
					if (String.IsNullOrWhiteSpace(rtID) || String.IsNullOrWhiteSpace(rtDesc)) throw new ValidationException("ID and Description are required.");
					if (Context.eHubRegistrationTypes.Any(r => r.RT_ID == rtID)) throw new ValidationException("ID is not unique.");
					regType.RT_ID = rtID;
					regType.RT_Description = rtDesc;
					regType.RT_RegistrantType = "ClientSystem";
					Context.eHubRegistrationTypes.AddObject(regType);
					break;
				case "edit":
					if (String.IsNullOrWhiteSpace(rtID) || String.IsNullOrWhiteSpace(rtDesc)) throw new ValidationException("ID and Description are required.");
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
				default:
					break;
			}
			Context.SaveChanges();
			AddRegistrationTypeLog(Request["oper"], regType);
			SaveLogs();
		}

		public JsonResult ClientSystemRegistrations(Guid regType)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchQuery = Request["filters"];

			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			}
			var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);
			var regTypeName = regTypeInstance.RT_ID;
			var regos = Context.eHubClientSystemRegistrations.Where(r => r.CD_RT == regTypeInstance.RT_PK);
			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
			{
				regos = ApplyMultipleValuesFilter(filterQuery, regos);
			}

			int count = regos.Count();

			regos = ApplyValuesSort(sidx, sord, regos);

			regos = regos.Skip((page - 1) * rows).Take(rows);
			var customsValueList = ConfigXmlHelper.GetClientSystemRegistrationCustomsValueList(regTypeInstance, regos);
			var list = from r in regos.ToList()
					   join k in customsValueList on r.CD_PK equals k.PK
					   select new
					   {
						   CD_PK = r.CD_PK,
						   CD_RT = regTypeName,
						   CD_EH_ID = r.eHubClientSystem.EH_ID,
						   CD_Code = r.CD_Code,
						   CD_Qualifier = r.CD_Qualifier,
						   CD_Attr1 = r.CD_Attr1,
						   CD_Attr2 = r.CD_Attr2,
						   CD_Flag1 = r.CD_Flag1,
						   CD_ConfigXml = r.CD_PK,
						   CD_IssuedUTC = r.CD_IssuedUTC.HasValue ? r.CD_IssuedUTC.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
						   CD_ExpiryUTC = r.CD_ExpiryUTC.HasValue ? r.CD_ExpiryUTC.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
						   CustomValue1 = k.CustomValue1,
						   CustomValue2 = k.CustomValue2,
					   };

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClientSystemRegistrations = list.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public FileContentResult DownloadConfiguration(Guid CD_PK)
		{
			var isTest = Setting.ShowHidden();
			if (!isTest) return null;

			if (CD_PK == Guid.Empty)
				return null;
			var rego = Context.eHubClientSystemRegistrations.First(r => r.CD_PK == CD_PK);
			if (rego.CD_ConfigXml == null) return null;
			var fileName = rego.CD_PK.ToString();
			return File(Encoding.Default.GetBytes(rego.CD_ConfigXml), "text/text", fileName + ".xml");
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

		private IQueryable<eHubClientSystemRegistration> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubClientSystemRegistration> regos)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						regos = ApplyValuesFilter(rule.field, rule.data, rule.op, regos);
					break;
				case "OR":
					regos = ApplyValuesFilter(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, regos);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<eHubClientSystemRegistration> filterData = Context.eHubClientSystemRegistrations;
						filterData = ApplyValuesFilter(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
						regos = regos.AsQueryable().Union(filterData);
					}
					break;
				default:
					break;
			}
			return regos;
		}

		static IQueryable<eHubClientSystemRegistration> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<eHubClientSystemRegistration> regos)
		{
			switch (searchField)
			{
				case "CD_EH_ID":
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
				case "CD_Qualifier":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CD_Qualifier == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CD_Qualifier.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CD_Qualifier.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CD_Qualifier.Contains(searchString));
							break;
					}
					break;
				case "CD_Code":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CD_Code == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CD_Code.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CD_Code.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CD_Code.Contains(searchString));
							break;
					}
					break;
				case "CD_Attr1":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CD_Attr1 == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CD_Attr1.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CD_Attr1.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CD_Attr1.Contains(searchString));
							break;
					}
					break;
			}
			return regos;
		}

		static IQueryable<eHubClientSystemRegistration> ApplyValuesSort(string sidx, string sord, IQueryable<eHubClientSystemRegistration> regos)
		{
			switch (sidx + " " + sord)
			{
				case "CD_EH_ID asc":
					regos = regos.OrderBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code).ThenBy(r => r.CD_Attr1);
					break;
				case "CD_EH_ID desc":
					regos = regos.OrderByDescending(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code).ThenBy(r => r.CD_Attr1);
					break;
				case "CD_Qualifier asc":
					regos = regos.OrderBy(r => r.CD_Qualifier).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_Qualifier desc":
					regos = regos.OrderByDescending(r => r.CD_Qualifier).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_Code asc":
					regos = regos.OrderBy(r => r.CD_Code).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Attr1);
					break;
				case "CD_Code desc":
					regos = regos.OrderByDescending(r => r.CD_Code).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Attr1);
					break;
				case "CD_Attr1 asc":
					regos = regos.OrderBy(r => r.CD_Attr1).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_Attr1 desc":
					regos = regos.OrderByDescending(r => r.CD_Attr1).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_Flag1 asc":
					regos = regos.OrderBy(r => r.CD_Flag1).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_Flag1 desc":
					regos = regos.OrderByDescending(r => r.CD_Flag1).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_IssuedUTC asc":
					regos = regos.OrderBy(r => r.CD_IssuedUTC).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_IssuedUTC desc":
					regos = regos.OrderByDescending(r => r.CD_IssuedUTC).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_ExpiryUTC asc":
					regos = regos.OrderBy(r => r.CD_ExpiryUTC).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				case "CD_ExpiryUTC desc":
					regos = regos.OrderByDescending(r => r.CD_ExpiryUTC).ThenBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code);
					break;
				default:
					regos = regos.OrderBy(r => r.eHubClientSystem.EH_ID).ThenBy(r => r.CD_Code).ThenBy(r => r.CD_Attr1);
					break;
			}
			return regos;
		}

		[HttpPost]
		public JsonResult ClientSystemRegistrationsEdit(Guid regType)
		{
			var hasValidator = ApplyCustomisedValidation(regType);
			try
			{
				var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);
				var oper = Request["oper"];
				var pk = Request["CD_PK"] ?? Request["id"];
				var clientID = Request["CD_EH_ID"];
				var qualifier = Request["CD_Qualifier"];
				var code = Request["CD_Code"];
				var username = Request["CD_Attr1"];
				var password = Request["CD_Attr2"];
				byte? flag = !string.IsNullOrEmpty(Request["CD_Flag1"]) ? (byte?)byte.Parse(Request["CD_Flag1"]) : null;
				DateTime? issueDate = !string.IsNullOrEmpty(Request["CD_IssuedUTC"]) ? (DateTime?)DateTime.Parse(Request["CD_IssuedUTC"]) : null;
				DateTime? expiryDate = !string.IsNullOrEmpty(Request["CD_ExpiryUTC"]) ? (DateTime?)DateTime.Parse(Request["CD_ExpiryUTC"]) : null;
				var customValue1 = string.IsNullOrEmpty(Request["CustomValue1"]) ? null : Request["CustomValue1"];
				var customValue2 = string.IsNullOrEmpty(Request["CustomValue2"]) ? null : Request["CustomValue2"];
				var configXml = Request.Files.Count > 0 ? Request.Files["cd_inputFile"].InputStream : null;

				eHubClientSystemRegistration rego;
				if (String.IsNullOrWhiteSpace(pk) || pk == "_empty")
					rego = new eHubClientSystemRegistration();
				else
				{
					var pkguid = new Guid(pk);
					rego = regTypeInstance.eHubClientSystemRegistrations.First(r => r.CD_PK == pkguid);
				}

				switch (oper)
				{
					case "add":
						rego.CD_PK = Guid.NewGuid();
						rego.eHubRegistrationType = regTypeInstance;
						rego.eHubClientSystem = Context.eHubClientSystems.First(c => c.EH_ID == clientID);
						rego.CD_Qualifier = qualifier;
						rego.CD_Code = code;
						rego.CD_Attr1 = username;
						rego.CD_Attr2 = password;
						rego.CD_Flag1 = flag;
						rego.CD_IssuedUTC = issueDate;
						rego.CD_ExpiryUTC = expiryDate;
						rego.CD_ConfigXml = ConfigXmlHelper.GetConfigFromFile(configXml);
						Context.eHubClientSystemRegistrations.AddObject(rego);
						break;
					case "edit":
						rego.eHubClientSystem = Context.eHubClientSystems.First(c => c.EH_ID == clientID);
						rego.CD_Qualifier = qualifier;
						rego.CD_Code = code;
						rego.CD_Attr1 = username;
						rego.CD_Attr2 = password;
						rego.CD_Flag1 = flag;
						rego.CD_IssuedUTC = issueDate;
						rego.CD_ExpiryUTC = expiryDate;
						if (configXml != null)
						{
							rego.CD_ConfigXml = ConfigXmlHelper.UpdateRegistrationConfigXml(rego.CD_ConfigXml, regTypeInstance, customValue1, customValue2);

						}
						break;
					case "del":
						Context.eHubClientSystemRegistrations.DeleteObject(rego);
						break;
					default:
						break;
				}
				if (hasValidator)
				{
					TryUpdateModel<eHubClientSystemRegistration>(rego);
					if (ModelState.IsValid)
					{
						Context.SaveChanges();
						AddClientSystemRegistrationsLog(oper, rego);
						SaveLogs();
						return Json(new { success = true, id = rego.CD_PK }, JsonRequestBehavior.AllowGet);
					}
					else
					{
						var message = string.Join("<br/>", ModelState.Where(state => state.Value.Errors.Count > 0).Select(state => string.Join("<br/>", state.Value.Errors.Select(error => error.ErrorMessage))));
						return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
					}
				}
				else
				{
					Context.SaveChanges();
					AddClientSystemRegistrationsLog(oper, rego);
					SaveLogs();
					return Json(new { success = true, id = rego.CD_PK }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "") }, JsonRequestBehavior.AllowGet);
			}
		}

		public JsonResult ClientSystems(bool includeNonProd)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if (includeNonProd)
			{
				clients = Context.eHubClientSystems.Select(c => new
				{
					EH_ID = c.EH_ID,
					EH_URL = c.EH_URL
				});
			}
			else
			{
				clients = Context.eHubClientSystems
							.Where(sys => Context.ediProdClients
								.Any(prodClient => prodClient.EnterpriseServerCode == sys.EH_ID && prodClient.LD_LicenceType == "PRD"))
							.Select(sys => new
							{
								EH_ID = sys.EH_ID,
								EH_URL = sys.EH_URL
							});
			}


			if (filtered)
			{
				string id = Request["EH_ID"];
				string url = Request["EH_URL"];
				if (!String.IsNullOrWhiteSpace(id))
					clients = clients.Where(c => c.EH_ID.StartsWith(id, StringComparison.OrdinalIgnoreCase));
				if (!String.IsNullOrWhiteSpace(url))
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
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClientSystems = clients.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}
	}
}
