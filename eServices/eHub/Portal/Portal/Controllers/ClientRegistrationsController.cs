using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.DataModel.Business.Semantics;
using CargoWise.eHub.DataModel.Business.Validation;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eServices.Encryption.Client.Encryptor;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ClientRegistrationsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("ClientRegistrationsLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult RegistrationTypes(bool sortByDescription)
		{
			var regTypes = Context.eHubRegistrationTypes
				.Where(r => r.RT_RegistrantType == "Client")
				.Select(r => new
				{
					RT_PK = r.RT_PK,
					RT_ID = r.RT_ID,
					RT_Description = r.RT_Description
				});
			if (sortByDescription)
				return Json(new { eHubRegistrationTypes = regTypes.ToList().OrderBy(r => SortRegistrationTypes(r.RT_ID, r.RT_Description)).ToList() }, JsonRequestBehavior.AllowGet);
			else return Json(new { eHubRegistrationTypes = regTypes.OrderBy(r => r.RT_ID).ToList() }, JsonRequestBehavior.AllowGet);
		}

		internal string SortRegistrationTypes(string ID, string Description)
		{
			if (Description.Contains('-'))
			{
				return Description.Split('-')[0].Trim() + " " + ID;
			}
			return "" + ID;
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
		public String GetCustomisedHeader(Guid regType, string columnName)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			return GetCustomisedHeaderForID(registrationType.RT_ID, columnName);
		}

		[HttpGet]
		public String GetCustomisedHeaderForID(string regTypeID, string columnName)
		{
			if (regTypeID == "Default" && columnName == "RegType")
			{
				return "Registration Type";
			}
			var dict = eHubPortalSemanticsFactory.GetSemantics<string, string>(regTypeID, "ClientRegistrationHeaders");

			return dict.ContainsKey(columnName) ? dict[columnName] : string.Empty;
		}

		[HttpGet]
		public String GetAttr1List(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			var list = eHubPortalSemanticsFactory.GetSemantics<string, string>(registrationType.RT_ID, "Attr1Options");

			return string.Join(";", list.Select(x => x.Key + ":" + x.Value));
		}

		[HttpGet]
		public String GetFlag1DescriptionList(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			var list = eHubPortalSemanticsFactory.GetSemantics<int, string>(registrationType.RT_ID, "Flag1Options");

			return string.Join(";", list.Select(x => x.Key + ":" + x.Value));
		}

		[HttpGet]
		public String GetFlag2DescriptionList(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			var list = eHubPortalSemanticsFactory.GetSemantics<int, string>(registrationType.RT_ID, "Flag2Options");

			return string.Join(";", list.Select(x => x.Key + ":" + x.Value));
		}

		protected void AddRegistrationTypeLog(string oper, eHubRegistrationType regType)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubRegistrationType: RT_PK={regType.RT_PK}, RT_RegistrantType={regType.RT_RegistrantType}, RT_ID={regType.RT_ID}, RT_Description={regType.RT_Description}");
		}

		protected void AddRegistrationLog(string oper, eHubClientRegistration rego)
		{
			tempLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubClientRegistration: CX_PK={rego.CX_PK}, CX_Qualifier={rego.CX_Qualifier}, CX_Code={rego.CX_Code}, CX_Attr1={rego.CX_Attr1}, CX_Password1={rego.CX_Password1}, CX_Flag1={rego.CX_Flag1}, CX_Flag2={rego.CX_Flag2}, CX_IssuedUTC={rego.CX_IssuedUTC}, CX_ExpiryUTC={rego.CX_ExpiryUTC}" +
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
					regType.RT_RegistrantType = "Client";
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

		public JsonResult Registrations(Guid? regType)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchQuery = Request["filters"];

			(IEnumerable<object> list, int count) = regType == null ?
				RegistrationResults(page, rows, sidx, sord, searchQuery)
				: RegistrationResults(regType.Value, page, rows, sidx, sord, searchQuery);

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClientRegistrations = list.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		private (IEnumerable<object>, int) RegistrationResults(Guid regType, int page, int rows, string sidx, string sord, string searchQuery)
		{
			MultipleFilter filterQuery = new MultipleFilter();

			if (!string.IsNullOrEmpty(searchQuery))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			}

			var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);
			var partialRegistrations = Context.eHubClientRegistrations
					.Where(r => r.CX_RT == regTypeInstance.RT_PK);
			partialRegistrations = PartialColumnsSelect(partialRegistrations, filterQuery);

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				partialRegistrations = ApplyMultipleValuesFilter(filterQuery, partialRegistrations);

			var registrations = Context.eHubClientRegistrations
				.Join(partialRegistrations,
						r => r.CX_PK,
						pr => pr.CX_PK,
						(r, pr) => r)
				.Include(r => r.eHubClient);
			registrations = ApplyValuesSort(sidx, sord, registrations);

			int count = registrations.Count();
			registrations = registrations.Skip((page - 1) * rows).Take(rows);

			var regosPasswordDecoded = GetDecodedPasswordList(regTypeInstance, registrations);
			var customsValueList = ConfigXmlHelper.GetClientRegistrationCustomsValueList(regTypeInstance, registrations);

			var list = (from ro in registrations.ToList()
						join p in regosPasswordDecoded on ro.CX_PK equals p.CX_PK
						join k in customsValueList on ro.CX_PK equals k.PK
						select new
						{
							CX_PK = ro.CX_PK,
							CX_CC_ID = ro.eHubClient.CC_ID,
							CX_Qualifier = ro.CX_Qualifier,
							CX_Code = ro.CX_Code,
							CX_Attr1 = ro.CX_Attr1,
							CX_Password1 = ro.CX_Password1,
							CX_Flag1 = ro.CX_Flag1,
							CX_Flag2 = ro.CX_Flag2,
							CX_ConfigXml = ro.CX_PK,
							CX_IssuedUTC = ro.CX_IssuedUTC.HasValue ? ro.CX_IssuedUTC.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
							CX_ExpiryUTC = ro.CX_ExpiryUTC.HasValue ? ro.CX_ExpiryUTC.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
							DecodedPassword = p.DecodedPassword,
							CustomValue1 = k.CustomValue1,
							CustomValue2 = k.CustomValue2,
						}) as IEnumerable<object>;
			return (list, count);
		}

		private (IEnumerable<object>, int) RegistrationResults(int page, int rows, string sidx, string sord, string searchQuery)
		{
			if (string.IsNullOrEmpty(searchQuery))
			{
				searchQuery = "{}";
			}

			MultipleFilter filterQuery = new MultipleFilter();
			filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);

			var partialRegistrations = Context.eHubClientRegistrations
					.Where(registration => registration.eHubRegistrationType.RT_RegistrantType == "Client");
			partialRegistrations = PartialColumnsSelect(partialRegistrations, filterQuery);

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				partialRegistrations = ApplyMultipleValuesFilter(filterQuery, partialRegistrations);

			var registrations = Context.eHubClientRegistrations
				.Join(partialRegistrations,
						r => r.CX_PK,
						pr => pr.CX_PK,
						(r, pr) => r)
				.Include(r => r.eHubClient)
				.Include(r => r.eHubRegistrationType);

			int count = partialRegistrations.Count();

			registrations = ApplyValuesSort(sidx, sord, registrations);
			registrations = registrations.Skip((page - 1) * rows).Take(rows);

			var list = (from ro in registrations.ToList()
						select new
						{
							RegType = ro.eHubRegistrationType.RT_Description,
							CX_PK = ro.CX_PK,
							CX_CC_ID = ro.eHubClient.CC_ID,
							CX_Qualifier = ro.CX_Qualifier,
							CX_Code = ro.CX_Code,
							CX_Attr1 = ro.CX_Attr1,
							CX_Password1 = ro.CX_Password1,
							CX_Flag1 = ro.CX_Flag1,
							CX_Flag2 = ro.CX_Flag2,
							CX_ConfigXml = ro.CX_ConfigXml,
							CX_IssuedUTC = ro.CX_IssuedUTC,
							CX_ExpiryUTC = ro.CX_ExpiryUTC,
							DecodedPassword = "",
							CustomValue1 = "",
							CustomValue2 = "",
						}) as IEnumerable<object>;
			return (list, count);
		}

		private IQueryable<eHubClientRegistration> PartialColumnsSelect(IQueryable<eHubClientRegistration> registrations, MultipleFilter filterQuery)
		{
			registrations = registrations
						.Select(registration => new eHubClientRegistrationDTO
						{
							CX_PK = registration.CX_PK,
							CX_RT = registration.CX_RT,
							eHubClient = registration.eHubClient,
							CX_Code = registration.CX_Code,
							CX_Attr1 = registration.CX_Attr1,
							CX_Qualifier = registration.CX_Qualifier,
							eHubRegistrationType = registration.eHubRegistrationType
						});

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				registrations = ApplyMultipleValuesFilter(filterQuery, registrations);

			return registrations;
		}

		private List<ClientRegistrationEditView> GetDecodedPasswordList(eHubRegistrationType regTypeInstance, IQueryable<eHubClientRegistration> regos)
		{
			var semantics =
				eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(regTypeInstance.RT_ID, "EncodeSettings");
			var shouldDecodePassword = semantics.ContainsKey(EncodeOptions.DecodePasswordForEditBase64)
									   || semantics.ContainsKey(EncodeOptions.DecodePasswordRSA);
			var regosPasswordDecoded = (from ro in regos
										select new ClientRegistrationEditView()
										{
											CX_PK = ro.CX_PK,
											DecodedPassword = ro.CX_Password1
										}
				).ToList();
			var encodeOptions = GetDecodePasswordAlgorithm(semantics);

			if (shouldDecodePassword)
			{
				foreach (var clientRegistration in regosPasswordDecoded)
				{
					clientRegistration.DecodedPassword = DecodePassword(clientRegistration.DecodedPassword, encodeOptions);
				}
			}

			return regosPasswordDecoded;
		}

		public IEnumerable<object> GetDataWithConfig(List<Tuple<Guid, string, string>> cusValue, IQueryable<eHubClientRegistration> regos, List<ClientRegistrationEditView> regosPasswordDecoded)
		{
			var list = (from r in cusValue
						join o in regos.ToList() on new { c = r.Item1 } equals new { c = o.CX_PK } into jro
						join p in regosPasswordDecoded on r.Item1 equals p.CX_PK
						from ro in jro.DefaultIfEmpty()
						select new
						{
							CX_PK = ro.CX_PK,
							CX_CC_ID = ro.eHubClient.CC_ID,
							CX_Qualifier = ro.CX_Qualifier,
							CX_Code = ro.CX_Code,
							CX_Attr1 = ro.CX_Attr1,
							CX_Password1 = ro.CX_Password1,
							CX_Flag1 = ro.CX_Flag1,
							CX_Flag2 = ro.CX_Flag2,
							CustomsValue1 = r.Item2 ?? "",
							CustomsValue2 = r.Item3 ?? "",
							DecodedPassword = p.DecodedPassword
						}) as IEnumerable<object>;

			return list;
		}

		private IQueryable<eHubClientRegistration> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubClientRegistration> registrations)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						registrations = ApplyValuesFilter(rule.field, rule.data, rule.op, registrations);
					break;

				case "OR":
					var origin = registrations;
					registrations = ApplyValuesFilter(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, registrations);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<eHubClientRegistration> filterData = origin;
						filterData = ApplyValuesFilter(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
						registrations = registrations.AsQueryable().Union(filterData);
					}
					break;

				default:
					break;
			}

			return registrations;
		}

		static IQueryable<eHubClientRegistration> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<eHubClientRegistration> regos)
		{
			switch (searchField)
			{
				case "CX_CC_ID":
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
				case "CX_Qualifier":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CX_Qualifier == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CX_Qualifier.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CX_Qualifier.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CX_Qualifier.Contains(searchString));
							break;
					}
					break;
				case "CX_Code":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CX_Code == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CX_Code.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CX_Code.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CX_Code.Contains(searchString));
							break;
					}
					break;
				case "CX_Attr1":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CX_Attr1 == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CX_Attr1.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CX_Attr1.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CX_Attr1.Contains(searchString));
							break;
					}
					break;
				case "RegType":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.eHubRegistrationType.RT_Description == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.eHubRegistrationType.RT_Description.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.eHubRegistrationType.RT_Description.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.eHubRegistrationType.RT_Description.Contains(searchString));
							break;
					}
					break;
			}
			return regos;
		}

		static IQueryable<eHubClientRegistration> ApplyValuesSort(string sidx, string sord, IQueryable<eHubClientRegistration> regos)
		{
			switch (sidx + " " + sord)
			{
				case "CX_CC_ID asc":
					regos = regos.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code).ThenBy(r => r.CX_Attr1);
					break;
				case "CX_CC_ID desc":
					regos = regos.OrderByDescending(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code).ThenBy(r => r.CX_Attr1);
					break;
				case "CX_Code asc":
					regos = regos.OrderBy(r => r.CX_Code).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Attr1);
					break;
				case "CX_Code desc":
					regos = regos.OrderByDescending(r => r.CX_Code).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Attr1);
					break;
				case "CX_Attr1 asc":
					regos = regos.OrderBy(r => r.CX_Attr1).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code);
					break;
				case "CX_Attr1 desc":
					regos = regos.OrderByDescending(r => r.CX_Attr1).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code);
					break;
				case "RegType asc":
					regos = regos.OrderBy(r => r.eHubRegistrationType.RT_Description).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code).ThenBy(r => r.CX_Attr1);
					break;
				case "RegType desc":
					regos = regos.OrderByDescending(r => r.eHubRegistrationType.RT_Description).ThenBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code).ThenBy(r => r.CX_Attr1);
					break;
				default:
					regos = regos.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Qualifier).ThenBy(r => r.CX_Code).ThenBy(r => r.CX_Attr1);
					break;
			}
			return regos;
		}

		public bool ApplyCustomisedValidation(Guid regType)
		{
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			return eHubPortalValidationFactory.ApplyValidation(registrationType.RT_ID, typeof(eHubClientRegistration));
		}

		[HttpPost]
		public JsonResult RegistrationEdit(Guid regType)
		{
			var hasValidator = ApplyCustomisedValidation(regType);
			try
			{
				var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);
				var oper = Request["oper"];
				var cxpk = Request["CX_PK"] ?? Request["id"];
				var clientID = Request["CX_CC_ID"];
				var qualifier = String.IsNullOrWhiteSpace(Request["CX_Qualifier"]) ? null : Request["CX_Qualifier"];
				var code = Request["CX_Code"];
				var ftpURI = String.IsNullOrWhiteSpace(Request["CX_Attr1"]) ? null : Request["CX_Attr1"];
				var ftpPassword = String.IsNullOrWhiteSpace(Request["CX_Password1"]) ? null : Request["CX_Password1"];
				byte? flag1 = string.IsNullOrEmpty(Request["CX_Flag1"]) ? null : (byte?)byte.Parse(Request["CX_Flag1"]);
				byte? flag2 = string.IsNullOrEmpty(Request["CX_Flag2"]) ? null : (byte?)byte.Parse(Request["CX_Flag2"]);
				var decodedPassword = string.IsNullOrEmpty(Request["DecodedPassword"]) ? ftpPassword : Request["DecodedPassword"];
				var customValue1 = string.IsNullOrEmpty(Request["CustomValue1"]) ? null : Request["CustomValue1"];
				var customValue2 = string.IsNullOrEmpty(Request["CustomValue2"]) ? null : Request["CustomValue2"];
				var configXml = Request.Files.Count > 0 ? Request.Files["cx_inputFile"].InputStream : null;
				var isAddOrEdit = (oper == "add" || oper == "edit");
				var issuedUTC = string.IsNullOrEmpty(Request["CX_IssuedUTC"]) ? (DateTime?)null : DateTime.Parse(Request["CX_IssuedUTC"]);
				var expiryUTC = string.IsNullOrEmpty(Request["CX_ExpiryUTC"]) ? (DateTime?)null : DateTime.Parse(Request["CX_ExpiryUTC"]);

				eHubClientRegistration rego;
				if (String.IsNullOrWhiteSpace(cxpk) || cxpk == "_empty")
					rego = new eHubClientRegistration();
				else
				{
					var cxpkguid = new Guid(cxpk);
					rego = regTypeInstance.eHubClientRegistrations.First(r => r.CX_PK == cxpkguid);
				}
				var semantics = eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(regTypeInstance.RT_ID, "EncodeSettings");
				var decodeOptions = GetDecodePasswordAlgorithm(semantics);
				var password = decodedPassword;
				var isPasswordChanged = CheckPasswordChanged(rego, regTypeInstance, password);
				switch (oper)
				{
					case "add":
						rego.CX_PK = Guid.NewGuid();
						rego.eHubRegistrationType = regTypeInstance;
						rego.eHubClient = Context.eHubClients.First(c => c.CC_ID == clientID);
						rego.CX_Qualifier = qualifier;
						rego.CX_Code = code;
						rego.CX_Attr1 = ftpURI;
						rego.CX_Flag1 = flag1;
						rego.CX_Flag2 = flag2;
						rego.CX_ConfigXml = ConfigXmlHelper.GetConfigFromFile(configXml);
						rego.CX_IssuedUTC = issuedUTC;
						rego.CX_ExpiryUTC = expiryUTC;
						Context.eHubClientRegistrations.AddObject(rego);
						break;
					case "edit":
						rego.eHubClient = Context.eHubClients.First(c => c.CC_ID == clientID);
						rego.CX_Qualifier = qualifier;
						rego.CX_Code = code;
						rego.CX_Attr1 = ftpURI;
						rego.CX_Flag1 = flag1;
						rego.CX_Flag2 = flag2;

						if (configXml != null)
						{
							rego.CX_ConfigXml = ConfigXmlHelper.UpdateRegistrationConfigXml(rego.CX_ConfigXml, regTypeInstance, customValue1, customValue2); // preserve existing xml if new one isn't provided

						}
						rego.CX_IssuedUTC = issuedUTC;
						rego.CX_ExpiryUTC = expiryUTC;
						break;
					case "del":
						Context.eHubClientRegistrations.DeleteObject(rego);
						break;
					default:
						break;
				}
				if (hasValidator)
				{
					TryUpdateModel<eHubClientRegistration>(rego);
					if (ModelState.IsValid)
					{
						if (isAddOrEdit)
						{
							rego.CX_Code = string.IsNullOrEmpty(rego.CX_Code) ? string.Empty : code;
							if (isPasswordChanged)
							{
								rego.CX_Password1 = EncodePasswordForRegistration(rego, regType, decodedPassword);
							}

						}
						Context.SaveChanges();
						AddRegistrationLog(oper, rego);
						SaveLogs();
						return Json(new { success = true, id = rego.CX_PK }, JsonRequestBehavior.AllowGet);
					}
					else
					{
						var message = string.Join("<br/>", ModelState.Where(state => state.Value.Errors.Count > 0).Select(state => string.Join("<br/>", state.Value.Errors.Select(error => error.ErrorMessage))));
						return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
					}
				}
				else
				{
					if (isAddOrEdit && isPasswordChanged)
					{
						rego.CX_Password1 = EncodePasswordForRegistration(rego, regType, decodedPassword);
					}
					Context.SaveChanges();
					AddRegistrationLog(oper, rego);
					SaveLogs();
					return Json(new { success = true, id = rego.CX_PK }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ExceptionHelper.GetExceptionMessages(ex) }, JsonRequestBehavior.AllowGet);
			}
		}



		private (IEnumerable<object>, int) RegistrationResultsForFilteredExport(Guid regType, MultipleFilter filterQuery, string sidx, string sord)
		{
			var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_PK == regType);
			var partialRegistrations = Context.eHubClientRegistrations
					.Where(r => r.CX_RT == regTypeInstance.RT_PK);

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				partialRegistrations = ApplyMultipleValuesFilter(filterQuery, partialRegistrations);

			var registrations = Context.eHubClientRegistrations
				.Join(partialRegistrations,
						r => r.CX_PK,
						pr => pr.CX_PK,
						(r, pr) => r)
				.Include(r => r.eHubClient);

			registrations = ApplyValuesSort(sidx, sord, registrations);
			int count = registrations.Count();
			var regosPasswordDecoded = GetDecodedPasswordList(regTypeInstance, registrations);
			var customsValueList = ConfigXmlHelper.GetClientRegistrationCustomsValueList(regTypeInstance, registrations);

			var list = (from ro in registrations.ToList()
						join p in regosPasswordDecoded on ro.CX_PK equals p.CX_PK
						join k in customsValueList on ro.CX_PK equals k.PK
						select new
						{
							CX_PK = ro.CX_PK,
							CX_RT = regTypeInstance.RT_ID.ToString(),
							CX_CC_ID = ro.eHubClient.CC_ID,
							CX_Qualifier = ro.CX_Qualifier,
							CX_Code = ro.CX_Code,
							CX_Attr1 = ro.CX_Attr1,
							CX_Password1 = ro.CX_Password1,
							CX_Flag1 = ro.CX_Flag1,
							CX_Flag2 = ro.CX_Flag2,
							CX_ConfigXml = ro.CX_PK,
							CX_IssuedUTC = ro.CX_IssuedUTC.HasValue ? ro.CX_IssuedUTC.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
							CX_ExpiryUTC = ro.CX_ExpiryUTC.HasValue ? ro.CX_ExpiryUTC.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
							DecodedPassword = p.DecodedPassword,
							CustomValue1 = k.CustomValue1,
							CustomValue2 = k.CustomValue2,
						}) as IEnumerable<object>;
			return (list, count);
		}

		private (IEnumerable<object>, int) RegistrationResultsForFilteredExportWithoutRegistrationType(MultipleFilter filterQuery, string sidx, string sord)
		{
			var partialRegistrations = Context.eHubClientRegistrations
			.Where(registration => registration.eHubRegistrationType.RT_RegistrantType == "Client");

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				partialRegistrations = ApplyMultipleValuesFilter(filterQuery, partialRegistrations);

			var registrations = Context.eHubClientRegistrations
			.Join(partialRegistrations,
					r => r.CX_PK,
					pr => pr.CX_PK,
					(r, pr) => r)
			.Include(r => r.eHubClient)
			.Include(r => r.eHubRegistrationType);

			registrations = ApplyValuesSort(sidx, sord, registrations);
			int count = registrations.Count();

			var list = (from ro in registrations.ToList()
						select new
						{
							RegType = ro.eHubRegistrationType.RT_Description,
							CX_PK = ro.CX_PK,
							CX_CC_ID = ro.eHubClient.CC_ID,
							CX_Qualifier = ro.CX_Qualifier,
							CX_Code = ro.CX_Code,
							CX_Attr1 = ro.CX_Attr1,
							CX_Password1 = ro.CX_Password1,
							CX_Flag1 = ro.CX_Flag1,
							CX_Flag2 = ro.CX_Flag2,
							CX_ConfigXml = ro.CX_ConfigXml,
							CX_IssuedUTC = ro.CX_IssuedUTC,
							CX_ExpiryUTC = ro.CX_ExpiryUTC,
							DecodedPassword = "",
							CustomValue1 = "",
							CustomValue2 = "",
						}) as IEnumerable<object>;
			return (list, count);
		}

		[HttpGet]
		public FileContentResult RegistrationsExportCsvForFilteredDataWithoutRegistrationType()
		{
			var filter = Request["filterForExportCsv"];
			string sortData = Request["sortDataExportCsv"];

			string sidx = "";
			string sord = "";
			if (!string.IsNullOrEmpty(sortData))
			{
				dynamic sortParams = JsonConvert.DeserializeObject(sortData);
				sidx = sortParams.sidx;
				sord = sortParams.sord;
			}
			MultipleFilter filterQuery = new MultipleFilter();

			if (!string.IsNullOrEmpty(filter))
			{
				filterQuery = DynamicQueryable.ConvertSQLQueryToMultipleFilterObject(filter);
			}

			(IEnumerable<dynamic> reg_list, int count) = RegistrationResultsForFilteredExportWithoutRegistrationType(filterQuery, sidx, sord);

			using (var wrt = new StringWriter())
			using (var csv = new CsvHelper.CsvWriter(wrt))
			{
				csv.WriteField("Registration Type");
				csv.WriteField("Client");
				csv.WriteField("Qualifier");
				csv.WriteField("Code");
				csv.WriteField("Attribute 1");
				csv.WriteField("Password 1");
				csv.WriteField("Flag 1");
				csv.WriteField("Flag 2");
				csv.NextRecord();

				foreach (var reg in reg_list)
				{
					var semantics = eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(reg.RegType, "EncodeSettings");
					var decodeOptions = GetDecodePasswordAlgorithm(semantics);

					csv.WriteField(reg.RegType);
					csv.WriteField(reg.CX_CC_ID);
					csv.WriteField(reg.CX_Qualifier);
					csv.WriteField(reg.CX_Code);
					csv.WriteField(reg.CX_Attr1);
					csv.WriteField(DecodePassword(reg.CX_Password1, decodeOptions));
					csv.WriteField(reg.CX_Flag1);
					csv.WriteField(reg.CX_Flag2);
					csv.NextRecord();
				}

				string filename = String.Format("{0}_{1}.csv", "Client_Registrations", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
				return File(Encoding.Default.GetBytes(wrt.ToString()), "text/text", filename);
			}
		}

		[HttpGet]
		public FileContentResult RegistrationsExportCsv()
		{
			try
			{
				var regType = new Guid(Request["registrationTypePK"]);
				var filter = Request["filterForExportCsv"];
				var gridCols = Request["gridColumns"];
				string sortData = Request["sortDataExportCsv"];

				string sidx = "";
				string sord = "";
				if (!string.IsNullOrEmpty(sortData))
				{
					dynamic sortParams = JsonConvert.DeserializeObject(sortData);
					sidx = sortParams.sidx;
					sord = sortParams.sord;
				}
				MultipleFilter filterQuery = new MultipleFilter();

				if (!string.IsNullOrEmpty(filter))
				{
					filterQuery = DynamicQueryable.ConvertSQLQueryToMultipleFilterObject(filter);
				}

				(IEnumerable<dynamic> reg_list, int count) = RegistrationResultsForFilteredExport(regType, filterQuery, sidx, sord);

				var regTypeInstance = Context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_PK == regType);
				if (regTypeInstance == null)
				{
					throw new InvalidOperationException("Registration type not found.");
				}

				var semantics = eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(regTypeInstance.RT_ID, "EncodeSettings");
				var decodeOptions = GetDecodePasswordAlgorithm(semantics);

				var dict = eHubPortalSemanticsFactory.GetSemantics<Dictionary<string, string>>(regTypeInstance.RT_ID, "ClientRegistrationHeaders") ?? new Dictionary<string, string>();

				dict = SortColumns(dict, gridCols);

				bool includeDecodedPassword = reg_list.Any(r => !string.IsNullOrEmpty(r.DecodedPassword));

				if (includeDecodedPassword && !dict.ContainsKey("CX_Password1"))
				{
					dict["CX_Password1"] = "DecodedPassword";
				}

				using (var wrt = new StringWriter())
				using (var csv = new CsvHelper.CsvWriter(wrt))
				{
					foreach (var columnName in dict.Values)
					{
						csv.WriteField(columnName);
					}
					csv.NextRecord();

					foreach (var r in reg_list)
					{
						var propertyValues = new Dictionary<string, string>();

						foreach (var property in r.GetType().GetProperties())
						{
							var propValue = property.GetValue(r);
							var stringValue = propValue?.ToString();
							if (property.Name == "CX_Password1")
							{
								stringValue = DecodePassword(stringValue, decodeOptions);
							}
							propertyValues[property.Name] = stringValue;
						}

						foreach (var mapping in dict)
						{
							var value = propertyValues.TryGetValue(mapping.Key, out var mappedValue) ? mappedValue : null;
							csv.WriteField(value);
						}

						csv.NextRecord();
					}

					var sanitizedFileName = Regex.Replace($"{regTypeInstance.RT_ID}_{regTypeInstance.RT_Description}", @"[^\w\d_-]", "") + ".csv";
					return File(Encoding.UTF8.GetBytes(wrt.ToString()), "text/csv", sanitizedFileName);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Error generating CSV: {ex.Message}", ex);
			}
		}

		private Dictionary<string, string> SortColumns(Dictionary<string, string> dict, string gridCols)
		{
			if (string.IsNullOrEmpty(gridCols)) return dict;

			var cols = gridCols.Split(',');
			if (cols.Length == 0) return dict;

			var sortedList = new Dictionary<string, string>();
			foreach (var col in cols)
			{
				if (dict.TryGetValue(col, out var value))
				{
					sortedList.Add(col, value);
				}
			}

			return sortedList;
		}

		[HttpGet]
		public FileContentResult DownloadConfiguration(Guid CX_PK)
		{
			var isTest = Setting.ShowHidden();
			if (!isTest) return null;

			if (CX_PK == Guid.Empty)
				return null;
			var rego = Context.eHubClientRegistrations.First(r => r.CX_PK == CX_PK);
			if (rego.CX_ConfigXml == null) return null;
			var fileName = rego.CX_PK.ToString();
			return File(Encoding.Default.GetBytes(rego.CX_ConfigXml), "text/text", fileName + ".xml");
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

		public ActionResult UniqueAttr1(string CX_Attr1, Guid CX_RT, Guid CX_PK)
		{
			return Json(!Context.eHubClientRegistrations.Any(cx => cx.CX_RT == CX_RT && cx.CX_Attr1 == CX_Attr1 && cx.CX_PK != CX_PK), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult RegistrationsImportCsv()
		{
			try
			{
				var regType = new Guid(Request["registrationTypePK"]);
				var regTypeInstance = Context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_PK == regType);
				var semantics = eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(regTypeInstance.RT_ID, "EncodeSettings");
				var encodeOptions = GetEncodePasswordAlgorithm(semantics);
				var dict = eHubPortalSemanticsFactory.GetSemantics<Dictionary<string, string>>(regTypeInstance.RT_ID, "ClientRegistrationHeaders");
				var expectedColumns = dict.Values.ToList();
				if (!dict.ContainsKey("CX_Password1"))
				{
					expectedColumns.Add("DecodedPassword");
				}

				var regos = new List<Dictionary<string, string>>();
				using (var rdr = new StreamReader(Request.Files["uploadFile"].InputStream))
				using (var csv = new CsvHelper.CsvParser(rdr))
				{
					var headers = csv.Read();
					var columnIndices = headers
						.Select((name, idx) => new { name, idx })
						.ToDictionary(x => x.name, x => x.idx);

					var expectedSet = new HashSet<string>(expectedColumns);
					if (!expectedSet.SetEquals(columnIndices.Keys))
						throw new InvalidOperationException("CSV headers do not match the expected format.");

					string[] fields;
					while ((fields = csv.Read()) != null)
					{
						var row = new Dictionary<string, string>();
						foreach (var column in expectedColumns)
						{
							var idx = columnIndices[column];
							row[column] = string.IsNullOrWhiteSpace(fields[idx]) ? null : fields[idx];
						}
						regos.Add(row);
					}
				}

				var oldRegos = Context.eHubClientRegistrations
					.Where(r => r.CX_RT == regTypeInstance.RT_PK)
					.Select(r => new
					{
						CC_ID = r.eHubClient.CC_ID,
						CX_Qualifier = r.CX_Qualifier,
						Registration = r
					})
					.ToList();

				var option = Request["option"]?.ToLower();
				var merged = from r in regos
							 join o in oldRegos on new { c = r[dict["CX_CC_ID"]], q = r[dict["CX_Qualifier"]] }
								  equals new { c = o.CC_ID, q = o.CX_Qualifier } into jro
							 from ro in jro.DefaultIfEmpty()
							 select new { New = r, Old = ro };

				foreach (var x in merged)
				{
					string password = x.New.ContainsKey("DecodedPassword")
						? x.New["DecodedPassword"]
						: (dict.ContainsKey("CX_Password1") && x.New.ContainsKey(dict["CX_Password1"]))
							? x.New[dict["CX_Password1"]]
							: null;

					if (x.Old == null)
					{
						var cxCcId = x.New[dict["CX_CC_ID"]].ToString();
						var eHubClient = Context.eHubClients.FirstOrDefault(c => c.CC_ID == cxCcId);
						string encodedPassword = !string.IsNullOrEmpty(password) ? EncodePassword(password, encodeOptions) : null;
						var newRegistration = new eHubClientRegistration
						{
							CX_PK = Guid.NewGuid(),
							eHubRegistrationType = regTypeInstance,
							eHubClient = eHubClient,
							CX_Qualifier = dict.ContainsKey("CX_Qualifier") ? x.New[dict["CX_Qualifier"]] : null,
							CX_Code = dict.ContainsKey("CX_Code") ? x.New[dict["CX_Code"]] : null,
							CX_Attr1 = dict.ContainsKey("CX_Attr1") ? x.New[dict["CX_Attr1"]] : null,
							CX_Password1 = encodedPassword,
							CX_Flag1 = dict.ContainsKey("CX_Flag1") && !string.IsNullOrEmpty(x.New[dict["CX_Flag1"]])
								? (byte?)byte.Parse(x.New[dict["CX_Flag1"]])
								: null,
							CX_Flag2 = dict.ContainsKey("CX_Flag2") && !string.IsNullOrEmpty(x.New[dict["CX_Flag2"]])
								? (byte?)byte.Parse(x.New[dict["CX_Flag2"]])
								: null
						};
						Context.eHubClientRegistrations.AddObject(newRegistration);
					}
					else
					{
						x.Old.Registration.CX_Code = dict.ContainsKey("CX_Code") ? x.New[dict["CX_Code"]] : null;
						x.Old.Registration.CX_Attr1 = dict.ContainsKey("CX_Attr1") ? x.New[dict["CX_Attr1"]] : null;
						x.Old.Registration.CX_Flag1 = dict.ContainsKey("CX_Flag1") && !string.IsNullOrEmpty(x.New[dict["CX_Flag1"]])
							? (byte?)byte.Parse(x.New[dict["CX_Flag1"]])
							: null;
						x.Old.Registration.CX_Flag2 = dict.ContainsKey("CX_Flag2") && !string.IsNullOrEmpty(x.New[dict["CX_Flag2"]])
							? (byte?)byte.Parse(x.New[dict["CX_Flag2"]])
							: null;
						var isPasswordChanged = CheckPasswordChanged(x.Old.Registration, regTypeInstance, password);
						if (isPasswordChanged)
						{
							x.Old.Registration.CX_Password1 = string.IsNullOrEmpty(password)
								? null
								: EncodePassword(password, encodeOptions);
						}

					}
				}

				if (option == "replace")
				{
					var deletes = from o in oldRegos
								  join r in regos on new { c = o.CC_ID, q = o.CX_Qualifier }
									   equals new { c = r[dict["CX_CC_ID"]], q = r[dict["CX_Qualifier"]] } into jro
								  from or in jro.DefaultIfEmpty()
								  where or == null
								  select o.Registration;
					deletes.ToList().ForEach(x => Context.eHubClientRegistrations.DeleteObject(x));
				}

				Context.SaveChanges();
				return Json(new { success = true, message = "CSV import completed." }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = $"Error processing CSV: {ex.Message}" }, JsonRequestBehavior.AllowGet);
			}
		}


		public JsonResult Clients(bool includeNonProd)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

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
				string ccid = Request["CC_ID"];
				string ccname = Request["CC_FriendlyName"];
				if (!String.IsNullOrWhiteSpace(ccid))
					clients = clients.Where(c => c.CC_ID.StartsWith(ccid, StringComparison.OrdinalIgnoreCase));
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

		private string EncodePasswordForRegistration(eHubClientRegistration rego, Guid regType, String decodedPassword)
		{
			if (decodedPassword == null)
			{
				return null;
			}
			rego.CX_Password1 = decodedPassword;
			var registrationType = Context.eHubRegistrationTypes.First(x => x.RT_PK == regType);
			var semantics = eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(registrationType.RT_ID, "EncodeSettings");
			var encodeOptions = GetEncodePasswordAlgorithm(semantics);
			var encodedPassword = EncodePassword(rego.CX_Password1, encodeOptions);

			if (encodeOptions == EncodeOptions.OriginalPassword) return rego.CX_Password1;

			var contextItem = Context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == rego.CX_PK);
			if (contextItem != null)
			{
				contextItem.CX_Password1 = encodedPassword;
			}
			return encodedPassword;

		}

		private EncodeOptions GetEncodePasswordAlgorithm(Dictionary<EncodeOptions, String> semantics)
		{
			if (semantics.ContainsKey(EncodeOptions.EncodePasswordBase64))
			{
				return EncodeOptions.EncodePasswordBase64;
			}
			else if (semantics.ContainsKey(EncodeOptions.EncodePasswordRSA))
			{
				return EncodeOptions.EncodePasswordRSA;
			}
			else
			{
				return EncodeOptions.OriginalPassword;
			}
		}
		private EncodeOptions GetDecodePasswordAlgorithm(Dictionary<EncodeOptions, String> semantics)
		{
			if (semantics.ContainsKey(EncodeOptions.DecodePasswordForEditBase64))
			{
				return EncodeOptions.DecodePasswordForEditBase64;
			}
			else if (semantics.ContainsKey(EncodeOptions.DecodePasswordRSA))
			{
				return EncodeOptions.DecodePasswordRSA;
			}
			else
			{
				return EncodeOptions.OriginalPassword;
			}
		}

		private string EncodePassword(string password1, EncodeOptions encodeOptions)
		{
			switch (encodeOptions)
			{
				case EncodeOptions.EncodePasswordBase64:
					return EncodeDecodeHelper.EncodeBase64(password1);
				case EncodeOptions.EncodePasswordRSA:
					return EhubClientEncryptor.Encrypt(password1);
				case EncodeOptions.OriginalPassword:
					return password1;
				default:
					return password1;
			}
		}

		private string DecodePassword(string password1, EncodeOptions decodeOptions)
		{
			try
			{
				switch (decodeOptions)
				{
					case EncodeOptions.DecodePasswordForEditBase64:
						return EncodeDecodeHelper.DecodeBase64(password1);
					case EncodeOptions.DecodePasswordRSA:
						return EhubServerDecryptor.Decrypt(password1);
					case EncodeOptions.OriginalPassword:
						return password1;
					default:
						return password1;
				}
			}
			catch (Exception)
			{
				return password1;
			}

		}

		private bool CheckPasswordChanged(eHubClientRegistration rego, eHubRegistrationType regTypeInstance, string password)
		{
			var semantics = eHubPortalSemanticsFactory.GetSemantics<EncodeOptions, string>(regTypeInstance.RT_ID, "EncodeSettings");
			var encodedPassword = "";
			var decodedPassword = "";
			var encodeOptions = GetEncodePasswordAlgorithm(semantics);
			var decodeOptions = GetDecodePasswordAlgorithm(semantics);

			switch (encodeOptions)
			{
				case EncodeOptions.EncodePasswordSHA256:
					encodedPassword = password;
					return rego.CX_Password1 == encodedPassword ? false : true;
				case EncodeOptions.EncodePasswordBase64:
				case EncodeOptions.EncodePasswordRSA:
					if (rego.CX_Password1 == null) return true;

					decodedPassword = password;
					return DecodePassword(rego.CX_Password1, decodeOptions) == decodedPassword ? false : true;
				case EncodeOptions.OriginalPassword:
					return rego.CX_Password1 == password ? false : true;
				default:
					return false;
			}
		}

		private class eHubClientRegistrationDTO: eHubClientRegistration {
			public override eHubClient eHubClient { get; set; }
		}
	}
}
