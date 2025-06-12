using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ITCustomsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("ITCustomsLogger");
		protected List<string> listLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult JobStatus()
		{
			bool isSearching = Convert.ToBoolean(Request["_search"]);
			int page = Convert.ToInt32(Request["page"]);
			int rowNo = Convert.ToInt32(Request["rows"]);
			string sortIndex = Request["sidx"];
			string sortDirection = Request["sord"];
			string searchQuery = Request["filters"];

			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);

			IQueryable<eHubITCustomsJobStatu> vals = Context.eHubITCustomsJobStatus;

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				vals = ApplyMultipleValuesFilter(filterQuery, vals);

			int count = vals.Count();

			vals = ApplySorting(sortIndex, sortDirection, vals).Skip((page - 1) * rowNo).Take(rowNo);


			var jobStatusRows = vals.ToList().Select(x => new
			{
				IT_PK = x.IT_PK,
				SenderID = x.eHubClient.CC_ID,
				ClientSystemID = x.eHubClientSystem.EH_ID,
				ProdInd = x.IT_ProdInd,
				FileName = x.IT_FileName,
				JobID = x.IT_JobID,
				PollingStart = x.IT_PollingStartUTC.ToLocalTime(),
				FileLastModified = x.IT_FileLastModifiedUTC != null ? x.IT_FileLastModifiedUTC.Value.ToLocalTime() : (DateTime?)null,
				LastStatus = x.IT_LastStatus,
				ReferenceID = x.IT_ReferenceID,
				MessageType = x.IT_MessageType,
				MessageTrackingID = x.IT_MessageTrackingID,
				NotifiedInvalidProfile = x.IT_NotifiedInvalidProfile,
				DeclarationContent = x.IT_DeclarationContent,
			}).ToList();

			return Json(new
			{
				page = page,
				total = Math.Ceiling(count / (double)rowNo),
				records = count,
				eHubITCustomsJobStatus = jobStatusRows
			}, JsonRequestBehavior.AllowGet);

		}

		private IQueryable<eHubITCustomsJobStatu> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubITCustomsJobStatu> vals)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						vals = ApplySearching(rule.field, rule.data, rule.op, vals);
					break;
				case "OR":
					vals = ApplySearching(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, vals);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<eHubITCustomsJobStatu> filterData = ReadOnlyContext.eHubITCustomsJobStatus;
						filterData = ApplySearching(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
						vals = vals.AsQueryable().Union(filterData);
					}
					break;
				default:
					break;
			}
			return vals;
		}


		private static IQueryable<eHubITCustomsJobStatu> ApplySorting(string sortIndex, string sortDirection, IQueryable<eHubITCustomsJobStatu> jobStatusRows)
		{
			switch (sortIndex + " " + sortDirection)
			{
				case "SenderID asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.eHubClient.CC_ID);
					break;
				case "SenderID desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.eHubClient.CC_ID);
					break;
				case "PollingStart asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_PollingStartUTC);
					break;
				case "PollingStart desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_PollingStartUTC);
					break;
				case "ClientSystemID asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.eHubClientSystem.EH_ID);
					break;
				case "ClientSystemID desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.eHubClientSystem.EH_ID);
					break;
				case "FileName asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_FileName);
					break;
				case "FileName desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_FileName);
					break;
				case "JobID asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_JobID);
					break;
				case "JobID desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_JobID);
					break;
				case "LastStatus asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_LastStatus);
					break;
				case "LastStatus desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_LastStatus);
					break;
				case "MessageType asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_MessageType);
					break;
				case "MessageType desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_MessageType);
					break;
				case "ReferenceID asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_ReferenceID);
					break;
				case "ReferenceID desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_ReferenceID);
					break;
				case "MessageTrackingID asc":
					jobStatusRows = jobStatusRows.OrderBy(r => r.IT_MessageTrackingID.ToString());
					break;
				case "MessageTrackingID desc":
					jobStatusRows = jobStatusRows.OrderByDescending(r => r.IT_MessageTrackingID.ToString());
					break;
			}

			return jobStatusRows;
		}

		static IQueryable<eHubITCustomsJobStatu> ApplySearching(string searchField, string searchString, string searchOper, IQueryable<eHubITCustomsJobStatu> jobStatusRows)
		{
			switch (searchField)
			{
				case "SenderID":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClient.CC_ID == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClient.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClient.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClient.CC_ID.Contains(searchString));
							break;
					}
					break;
				case "ClientSystemID":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClientSystem.EH_ID == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClientSystem.EH_ID.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClientSystem.EH_ID.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.eHubClientSystem.EH_ID.Contains(searchString));
							break;
					}
					break;
				case "FileName":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.IT_FileName == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.IT_FileName.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.IT_FileName.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.IT_FileName.Contains(searchString));
							break;
					}
					break;
				case "JobID":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.IT_JobID == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.IT_JobID.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.IT_JobID.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.IT_JobID.Contains(searchString));
							break;
					}
					break;
				case "LastStatus":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.IT_LastStatus == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.IT_LastStatus.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.IT_LastStatus.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.IT_LastStatus.Contains(searchString));
							break;
					}
					break;
				case "MessageType":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.IT_MessageType == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.IT_MessageType.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.IT_MessageType.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.IT_MessageType.Contains(searchString));
							break;
					}
					break;
				case "ReferenceID":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.IT_ReferenceID == searchString);
							break;
						case "bw":
							jobStatusRows = jobStatusRows.Where(r => r.IT_ReferenceID.StartsWith(searchString));
							break;
						case "ew":
							jobStatusRows = jobStatusRows.Where(r => r.IT_ReferenceID.EndsWith(searchString));
							break;
						case "cn":
							jobStatusRows = jobStatusRows.Where(r => r.IT_ReferenceID.Contains(searchString));
							break;
					}
					break;
				case "MessageTrackingID":
					switch (searchOper)
					{
						case "eq":
							jobStatusRows = jobStatusRows.Where(r => r.IT_MessageTrackingID == new Guid(searchString));
							break;
					}
					break;
			}

			return jobStatusRows;
		}

		protected void SaveLogs()
		{
			foreach (var listLog in listLogs)
			{
				logger.Info(listLog);
			}
			listLogs.Clear();
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
					CC_ID = c.CC_ID,
					CC_FriendlyName = c.CC_FriendlyName
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

		public JsonResult ClientSystem(bool includeNonProd)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clientSystems;
			if (includeNonProd)
			{
				clientSystems = Context.eHubClientSystems.Select(c => new
				{
					c.EH_ID,
					c.EH_LastUpdateUTC
				});
			}
			else
			{
				clientSystems = from sys in Context.eHubClientSystems
								join prodClient in Context.ediProdClients
								on sys.EH_ID equals prodClient.EnterpriseServerCode
								where prodClient == null || prodClient.LD_LicenceType == "PRD"
								select new { sys.EH_ID, sys.EH_LastUpdateUTC };
			}

			if (filtered)
			{
				string ehid = Request["EH_ID"];
				if (!String.IsNullOrWhiteSpace(ehid))
					clientSystems = clientSystems.Where(c => c.EH_ID.StartsWith(ehid, StringComparison.OrdinalIgnoreCase));
			}

			int count = clientSystems.Count();

			switch (sidx + " " + sord)
			{
				case "EH_ID asc":
					clientSystems = clientSystems.OrderBy(c => c.EH_ID).ThenBy(c => c.EH_LastUpdateUTC);
					break;
				case "EH_ID desc":
					clientSystems = clientSystems.OrderByDescending(c => c.EH_ID).ThenBy(c => c.EH_LastUpdateUTC);
					break;
				case "EH_LastUpdateUTC asc":
					clientSystems = clientSystems.OrderBy(c => c.EH_LastUpdateUTC).ThenBy(c => c.EH_ID);
					break;
				case "EH_LastUpdateUTC desc":
					clientSystems = clientSystems.OrderByDescending(c => c.EH_LastUpdateUTC).ThenBy(c => c.EH_ID);
					break;
				default:
					clientSystems = clientSystems.OrderBy(c => c.EH_ID).ThenBy(c => c.EH_LastUpdateUTC);
					break;
			}

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClientSystem = clientSystems.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult AddOrUpdateOrDeleteOnJobStatus()
		{
			try
			{
				var operValue = Request["oper"];
				listLogs.Add($"[{operValue}] eHubITCustomsJobStatus");

				var iT_PK = Request["id"];
				var eHubITCustomsJobStatus = new eHubITCustomsJobStatu();
				if (!string.IsNullOrEmpty(Request["id"]) && Request["id"] != "_empty")
				{
					Guid id;
					if (!Guid.TryParse(iT_PK, out id))
						return Json(new { success = false, message = "Id must be GUID" }, JsonRequestBehavior.AllowGet);

					eHubITCustomsJobStatus = Context.eHubITCustomsJobStatus.FirstOrDefault(c => c.IT_PK == id);
					if (eHubITCustomsJobStatus == null)
						return Json(new { success = false, message = "Record does not exist because Id could not be found" }, JsonRequestBehavior.AllowGet);
				}
				AddITCustomsLog(operValue, "Before", eHubITCustomsJobStatus);

				if (operValue != "del")
				{
					DateTime? fileLastModifiedUTC = null;
					if (!string.IsNullOrEmpty(Request["FileLastModified"]))
						fileLastModifiedUTC = DateTime.Parse(Request["FileLastModified"]).ToUniversalTime();

					var LastStatus = Request["LastStatus"];
					var referenceID = Request["ReferenceID"];
					var NotifiedInvalidProfile = bool.Parse(Request["NotifiedInvalidProfile"]);
					var fileName = Request["FileName"];
					var jobID = Request["JobID"];
					var senderID = Request["SenderID"];
					var clientSystemID = Request["ClientSystemID"];
					var prodInd = bool.Parse(Request["ProdInd"]);
					var pollingStartUTC = DateTime.Parse(Request["PollingStart"]).ToUniversalTime();
					var messageType = Request["MessageType"];
					var declarationContent = Request["DeclarationContent"];
					var messageTrackingID = Request["MessageTrackingID"];

					var client = Context.eHubClients.FirstOrDefault(c => c.CC_ID == senderID);
					var clientSystem = Context.eHubClientSystems.FirstOrDefault(c => c.EH_ID == clientSystemID);

					if (client == null)
					{
						return Json(new { success = false, message = "Client must be selected" }, JsonRequestBehavior.AllowGet);
					}
					if (clientSystem == null)
					{
						return Json(new { success = false, message = "Client system must be selected" }, JsonRequestBehavior.AllowGet);
					}
					if (referenceID == null)
					{
						return Json(new { success = false, message = "referenceID must be filled" }, JsonRequestBehavior.AllowGet);
					}
					if (fileName == null)
					{
						return Json(new { success = false, message = "fileName must be filled" }, JsonRequestBehavior.AllowGet);
					}
					if (jobID == null)
					{
						return Json(new { success = false, message = "jobID must be filled" }, JsonRequestBehavior.AllowGet);
					}
					if (messageType == null)
					{
						return Json(new { success = false, message = "messageType must be filled" }, JsonRequestBehavior.AllowGet);
					}
					if (messageTrackingID == null)
					{
						return Json(new { success = false, message = "messageTrackingID must be filled" }, JsonRequestBehavior.AllowGet);
					}
					if (string.IsNullOrEmpty(LastStatus))
					{
						LastStatus = null;
					}

					Guid guidMessageTrackingID;
					if (!Guid.TryParse(messageTrackingID, out guidMessageTrackingID))
						return Json(new { success = false, message = "messageTrackingID must be GUID" }, JsonRequestBehavior.AllowGet);

					if (operValue == "add")
					{
						eHubITCustomsJobStatus.eHubClient = client;
						eHubITCustomsJobStatus.eHubClientSystem = clientSystem;
						eHubITCustomsJobStatus.IT_FileName = fileName;
						eHubITCustomsJobStatus.IT_JobID = jobID;
						eHubITCustomsJobStatus.IT_LastStatus = LastStatus;
						eHubITCustomsJobStatus.IT_MessageTrackingID = guidMessageTrackingID;
						eHubITCustomsJobStatus.IT_MessageType = messageType;
						eHubITCustomsJobStatus.IT_NotifiedInvalidProfile = NotifiedInvalidProfile;
						eHubITCustomsJobStatus.IT_PollingStartUTC = pollingStartUTC;
						eHubITCustomsJobStatus.IT_PK = Guid.NewGuid();
						eHubITCustomsJobStatus.IT_ProdInd = prodInd;
						eHubITCustomsJobStatus.IT_ReferenceID = referenceID;
						eHubITCustomsJobStatus.IT_CC_Sender = client.CC_PK;
						eHubITCustomsJobStatus.IT_EH_ClientSystem = clientSystem.EH_PK;
						eHubITCustomsJobStatus.IT_FileLastModifiedUTC = fileLastModifiedUTC;
						eHubITCustomsJobStatus.IT_DeclarationContent = declarationContent;

						Context.eHubITCustomsJobStatus.AddObject(eHubITCustomsJobStatus);
					}
					else
					{
						eHubITCustomsJobStatus.eHubClient = client;
						eHubITCustomsJobStatus.eHubClientSystem = clientSystem;
						eHubITCustomsJobStatus.IT_FileName = fileName;
						eHubITCustomsJobStatus.IT_JobID = jobID;
						eHubITCustomsJobStatus.IT_LastStatus = LastStatus;
						eHubITCustomsJobStatus.IT_MessageType = messageType;
						eHubITCustomsJobStatus.IT_MessageTrackingID = guidMessageTrackingID;
						eHubITCustomsJobStatus.IT_NotifiedInvalidProfile = NotifiedInvalidProfile;
						eHubITCustomsJobStatus.IT_PollingStartUTC = pollingStartUTC;
						eHubITCustomsJobStatus.IT_ProdInd = prodInd;
						eHubITCustomsJobStatus.IT_ReferenceID = referenceID;
						eHubITCustomsJobStatus.IT_CC_Sender = client.CC_PK;
						eHubITCustomsJobStatus.IT_EH_ClientSystem = clientSystem.EH_PK;
						eHubITCustomsJobStatus.IT_DeclarationContent = declarationContent;
						eHubITCustomsJobStatus.IT_FileLastModifiedUTC = fileLastModifiedUTC;
					}
				}
				else
				{
					Context.eHubITCustomsJobStatus.DeleteObject(eHubITCustomsJobStatus);
				}

				Context.SaveChanges();
				AddITCustomsLog(operValue, "After", eHubITCustomsJobStatus);
				SaveLogs();
				return Json(new { success = true, newid = eHubITCustomsJobStatus.IT_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (ApplicationException ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
			}
		}

		protected void AddITCustomsLog(string oper, string status, eHubITCustomsJobStatu eHubIT)
		{
			listLogs.Add($"[{HttpContext.User.Identity.Name}] [{status} Changes] [{oper}] eHubITCustomsJobStatus: IT_CC_Sender={eHubIT.IT_CC_Sender}, IT_EH_ClientSystem={eHubIT.IT_EH_ClientSystem}, IT_FileName={eHubIT.IT_FileName ?? ""}," +
				$" IT_LastStatus={eHubIT.IT_LastStatus ?? ""}, IT_MessageTrackingID={eHubIT.IT_MessageTrackingID}, IT_NotifiedInvalidProfile = {eHubIT.IT_NotifiedInvalidProfile?.ToString()}, IT_ProdInd = {eHubIT.IT_ProdInd.ToString()}");
		}
	}
}
