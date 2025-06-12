using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.DataModel.Business.Semantics;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Models.View.AirRouting;
using Common.Logging;

namespace CargoWise.eHub.Portal.Controllers
{
	public class MessageRoutingController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("AirlineMessagingLogger");
		protected List<string> tempLogs = new List<string>();
		
		//
		// GET: /MessageRouting/

		public ActionResult Index(Guid id)
		{
			var messageRoutingView = new MessageRoutingView(Context, id);
			return View("Index", messageRoutingView);
		}

		public ActionResult EditClientProvider(Guid id)
		{
			var clientView = new ClientEditView(Context, id);
			return View("EditClientProvider", clientView);
		}

		protected void AddAirDefaultServiceProviderLog(string oper, eHubAirDefaultServiceProvider routing)
		{
			tempLogs.Add($"[{oper}] eHubAirDefaultServiceProvider: AD_CC_Client={routing.AD_CC_Client}, AD_DT_MessageType={routing.AD_DT_MessageType}, AD_CC_AirServiceProvider={routing.AD_CC_AirServiceProvider}");
		}

		protected void AddAirServiceProviderMappingLog(string oper, eHubAirServiceProviderMapping airlineMapping)
		{
			tempLogs.Add($"[{oper}] eHubAirServiceProviderMapping: AM_CC_Client={airlineMapping.AM_CC_Client}, AM_CC_Airline={airlineMapping.AM_CC_Airline}, AM_DT_MessageType={airlineMapping.AM_DT_MessageType}, AM_CC_AirServiceProvider={airlineMapping.AM_CC_AirServiceProvider}, AM_RecipientAddress={airlineMapping.AM_RecipientAddress}, AM_ShipmentOrigin={airlineMapping.AM_ShipmentOrigin}");
		}

		protected void AddAirServiceProviderMappingBatchLog(string oper, List<eHubAirServiceProviderMapping> airlineMappings)
        {
			airlineMappings.ForEach(r => AddAirServiceProviderMappingLog(oper, r));
        }


		protected void AddClientLog(string oper, eHubClient client)
		{
			tempLogs.Add($"[{oper}] eHubClient: CC_PK={client.CC_PK}, CC_ID={client.CC_ID}, CC_FriendlyName={client.CC_FriendlyName}, CC_Odyssey_OH={client.CC_Odyssey_OH}, CC_DistributionZone={client.CC_DistributionZone}" +
			$", CC_EmailAddress={client.CC_EmailAddress}, CC_Password={client.CC_Password}, CC_IsAirServiceProvider={client.CC_IsAirServiceProvider}, CC_AirlineCode={client.CC_AirlineCode}, CC_AirServiceProvider={client.CC_AirServiceProvider}" +
			$", CC_AirlinePrefix={client.CC_AirlinePrefix}, CC_USCustomsRecipient={client.CC_USCustomsRecipient}, CC_AS2_Code={client.CC_AS2_Code}, CC_SCAC_Code={client.CC_SCAC_Code}, CC_OwnerCategory={client.CC_OwnerCategory}" +
			$", CC_SystemCategory={client.CC_SystemCategory}, CC_RR={client.CC_RR}, CC_RequireStatusResponse={client.CC_RequireStatusResponse}, CC_NotificationForInboxRecipient={client.CC_NotificationForInboxRecipient}");
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
		public ActionResult EditClientProvider(Guid id, FormCollection formValues)
		{
			var clientView = new ClientEditView(Context, id);

			if (formValues.Count == 1)
			{
				if (formValues[0] != "") clientView.Client.CC_AirServiceProvider = new Guid(formValues[0]);
				else clientView.Client.CC_AirServiceProvider = null;
			}

			if (ModelState.IsValid)
			{
				try
				{
					Context.SaveChanges();
					AddClientLog("edit", clientView.Client);
					SaveLogs();

					return RedirectToAction("Index", new { id });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.InnerException.Message);
					return View("EditClientProvider", clientView);
				}
			}
			else
			{
				return View("EditClientProvider", clientView);
			}
		}

		#region MessageTypeProviders
		public JsonResult MessageTypeProviders(Guid clientID)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rowNo = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchField = Request["searchField"];
			string searchString = Request["searchString"];
			string searchOper = Request["searchOper"];

			var messageTypeProviders = from dfp in Context.eHubAirDefaultServiceProviders
									   join sp in Context.eHubClients on dfp.AD_CC_AirServiceProvider equals sp.CC_PK
									   join messageType in Context.eHubMessageTypes on dfp.AD_DT_MessageType equals messageType.DT_PK
									   where dfp.AD_CC_Client == clientID
									   select new MessageTypeProvidersGridViewModel
									   {
										   rowId = dfp.AD_DT_MessageType + ":" + dfp.AD_CC_AirServiceProvider,
										   MessageType = messageType.DT_Code.Length > 3
													   ? messageType.DT_Code.Substring(messageType.DT_Code.Length - 3)
													   : messageType.DT_Code,
										   ServiceProvider = sp.CC_ID,
									   };

			messageTypeProviders = ApplyValuesFilter(searchField, searchString, searchOper, messageTypeProviders);

			int count = messageTypeProviders.Count();

			messageTypeProviders = ApplyValuesSort(sidx, sord, messageTypeProviders);

			messageTypeProviders = messageTypeProviders.Skip((page - 1) * rowNo).Take(rowNo);

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rowNo),
				records = count,
				messageTypeProviders = messageTypeProviders.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult MessageTypeProviderEdit(Guid clientID)
		{
			try
			{
				var oper = Request["oper"];
				var rowId = Request["id"];
				var messageType = Request["MessageType"];
				var serviceProvider = Request["ServiceProvider"];

				string[] ids;
				Guid messageTypeGuid;
				Guid serviceProviderGuid;
				eHubAirDefaultServiceProvider routing;
				var messageTypeRoutingList = Context.GetMessageTypeRouting(clientID);

				switch (oper)
				{
					case "add":
						messageTypeGuid = new Guid(messageType);
						serviceProviderGuid = new Guid(serviceProvider);
						CheckRecordShouldNotExist(messageTypeRoutingList, messageTypeGuid, serviceProviderGuid);

						routing = new eHubAirDefaultServiceProvider
						{
							AD_CC_AirServiceProvider = serviceProviderGuid,
							AD_DT_MessageType = messageTypeGuid,
							AD_CC_Client = clientID
						};
						Context.eHubAirDefaultServiceProviders.AddObject(routing);
						AddAirDefaultServiceProviderLog("add", routing);
						break;
					case "edit":
						ids = rowId.Split(':');
						if (ids.Count() != 2) throw new ArgumentException($"rowId '{rowId}' is invalid");
						var oldMessageTypeGuid = new Guid(ids[0]);
						var oldServiceProviderGuid = new Guid(ids[1]);
						routing = GetExistRoutingRecord(messageTypeRoutingList, oldMessageTypeGuid, oldServiceProviderGuid);

						messageTypeGuid = new Guid(messageType);
						serviceProviderGuid = new Guid(serviceProvider);
						CheckRecordShouldNotExist(messageTypeRoutingList, messageTypeGuid, serviceProviderGuid);

						Context.eHubAirDefaultServiceProviders.DeleteObject(routing);
						AddAirDefaultServiceProviderLog("edit (Previous Values)", routing);
						routing = new eHubAirDefaultServiceProvider
						{
							AD_CC_AirServiceProvider = serviceProviderGuid,
							AD_DT_MessageType = messageTypeGuid,
							AD_CC_Client = clientID
						};
						Context.eHubAirDefaultServiceProviders.AddObject(routing);
						AddAirDefaultServiceProviderLog("edit (New Values)", routing);
						break;
					case "del":
						ids = rowId.Split(':');
						if (ids.Count() != 2) throw new ArgumentException($"rowId '{rowId}' is invalid");

						messageTypeGuid = new Guid(ids[0]);
						serviceProviderGuid = new Guid(ids[1]);
						routing = GetExistRoutingRecord(messageTypeRoutingList, messageTypeGuid, serviceProviderGuid);

						Context.eHubAirDefaultServiceProviders.DeleteObject(routing);
						AddAirDefaultServiceProviderLog("del", routing);
						break;
					default:
						break;
				}
				Context.SaveChanges();
				SaveLogs();

				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				var message = ex.InnerException != null ? ex.Message + " " + ex.InnerException.Message : ex.Message;
				return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
			}

			void CheckRecordShouldNotExist(IQueryable<eHubAirDefaultServiceProvider> messageTypeRoutingList, Guid messageTypeGuid, Guid serviceProviderGuid)
			{
				if (messageTypeRoutingList.Any(x => x.AD_DT_MessageType == messageTypeGuid && x.AD_CC_AirServiceProvider == serviceProviderGuid))
				{
					throw new ArgumentException($"Record already exists for AD_DT_MessageType '{messageTypeGuid}' and AD_CC_AirServiceProvider '{serviceProviderGuid}'. Please choose a different MessageType or ServiceProvider.");
				}
			}

			eHubAirDefaultServiceProvider GetExistRoutingRecord(IQueryable<eHubAirDefaultServiceProvider> messageTypeRoutingList, Guid oldMessageTypeGuid, Guid oldServiceProviderGuid)
			{
				var routing = messageTypeRoutingList.FirstOrDefault(x => x.AD_DT_MessageType == oldMessageTypeGuid && x.AD_CC_AirServiceProvider == oldServiceProviderGuid);
				if (routing == null)
				{
					throw new ArgumentException($"Record doesn't exist for AD_DT_MessageType '{oldMessageTypeGuid}' and AD_CC_AirServiceProvider '{oldServiceProviderGuid}'.");
				}

				return routing;
			}
		}

		[HttpGet]
		public string MessageTypes()
		{
			return string.Join(";", Context.eHubMessageTypes
										.Where(m => m.DT_Code == "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL"
												|| m.DT_Code == "http://www.cargowise.com/ehub/clients/edi/2010/06#FWB"
												|| m.DT_Code == "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD")
										.OrderBy(x => x.DT_Code.Substring(x.DT_Code.Length - 3))
										.Select(x => x.DT_PK + ":" + x.DT_Code));
		}

		[HttpGet]
		public string AirLineServiceProviderMessageTypes()
        {
			return string.Join(";", Context.eHubMessageTypes
										.Where(m => m.DT_Code == "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL"
												|| m.DT_Code == "http://www.cargowise.com/ehub/clients/edi/2010/06#FWB")
										.OrderBy(x => x.DT_Code.Substring(x.DT_Code.Length - 3))
										.Select(x => x.DT_PK + ":" + x.DT_Code));
		}

		[HttpGet]
		public string ServiceProviders()
		{
			return string.Join(";", Context.GetAllAirServiceProvider().OrderBy(x => x.CC_ID).Select(x => x.CC_PK + ":" + x.CC_ID));
		}

		[HttpGet]
		public string Airlines()
		{
			return string.Join(";", Context.GetAllAirline().OrderBy(x => x.CC_ID).Select(x =>
			x.CC_PK + ":" + x.CC_ID));
		}

		static IQueryable<MessageTypeProvidersGridViewModel> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<MessageTypeProvidersGridViewModel> messageTypeProviders)
		{
			switch (searchField)
			{
				case "MessageType":
					switch (searchOper)
					{
						case "eq":
							messageTypeProviders = messageTypeProviders.Where(x => x.MessageType == searchString);
							break;
						case "bw":
							messageTypeProviders = messageTypeProviders.Where(x => x.MessageType.StartsWith(searchString));
							break;
						case "ew":
							messageTypeProviders = messageTypeProviders.Where(x => x.MessageType.EndsWith(searchString));
							break;
						case "cn":
							messageTypeProviders = messageTypeProviders.Where(x => x.MessageType.Contains(searchString));
							break;
					}
					break;
				case "ServiceProvider":
					switch (searchOper)
					{
						case "eq":
							messageTypeProviders = messageTypeProviders.Where(x => x.ServiceProvider == searchString);
							break;
						case "bw":
							messageTypeProviders = messageTypeProviders.Where(x => x.ServiceProvider.StartsWith(searchString));
							break;
						case "ew":
							messageTypeProviders = messageTypeProviders.Where(x => x.ServiceProvider.EndsWith(searchString));
							break;
						case "cn":
							messageTypeProviders = messageTypeProviders.Where(x => x.ServiceProvider.Contains(searchString));
							break;
					}
					break;
			}
			return messageTypeProviders;
		}

		static IQueryable<MessageTypeProvidersGridViewModel> ApplyValuesSort(string sidx, string sord, IQueryable<MessageTypeProvidersGridViewModel> messageTypeProviders)
		{
			switch (sidx + " " + sord)
			{
				case "MessageType asc":
					messageTypeProviders = messageTypeProviders.OrderBy(x => x.MessageType).ThenBy(x => x.ServiceProvider);
					break;
				case "MessageType desc":
					messageTypeProviders = messageTypeProviders.OrderByDescending(x => x.MessageType).ThenBy(x => x.ServiceProvider);
					break;
				case "ServiceProvider asc":
					messageTypeProviders = messageTypeProviders.OrderBy(x => x.ServiceProvider).ThenBy(x => x.MessageType);
					break;
				case "ServiceProvider desc":
					messageTypeProviders = messageTypeProviders.OrderByDescending(x => x.ServiceProvider).ThenBy(x => x.MessageType);
					break;
				default:
					messageTypeProviders = messageTypeProviders.OrderBy(x => x.MessageType).ThenBy(x => x.ServiceProvider);
					break;
			}
			return messageTypeProviders;
		}
		#endregion

		#region AirlineServiceProviderMapping
		public JsonResult AirlineServiceProviderMapping(Guid clientID)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rowNo = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchField = Request["searchField"];
			string searchString = Request["searchString"];
			string searchOper = Request["searchOper"];

			var airlineServiceProvidersMapping = from mapping in Context.eHubAirServiceProviderMappings
												 join client in Context.eHubClients on mapping.AM_CC_Client equals client.CC_PK
												 join airline in Context.eHubClients on mapping.AM_CC_Airline equals
								   airline.CC_PK
												 join serviceProvider in Context.eHubClients on mapping.AM_CC_AirServiceProvider equals serviceProvider.CC_PK
												 join messageType in Context.eHubMessageTypes on mapping.AM_DT_MessageType equals messageType.DT_PK
												 where mapping.AM_CC_Client == clientID

												 select new AirlineProviderMappingGridView
												 {
													 RowId = client.CC_PK + ":" + airline.CC_PK + ":" + mapping.AM_DT_MessageType + ":" + serviceProvider.CC_PK + ":" + mapping.AM_RecipientAddress + ":" + mapping.AM_ShipmentOrigin,
													 Airline = airline.CC_ID,
													 MessageType = messageType.DT_Code,
													 RecipientAddress = mapping.AM_RecipientAddress,
													 ServiceProvider = serviceProvider.CC_ID,
													 ClientPIMA = mapping.AM_ClientPIMA,
													 MessagePriority = mapping.AM_MessagePriority,
													 DoubleSignatureCode = mapping.AM_DoubleSignatureCode,
													 ShipmentOrigin = mapping.AM_ShipmentOrigin
												 };


            airlineServiceProvidersMapping = ApplyAirplineServiceProviderValuesFilter(searchField, searchString, searchOper, airlineServiceProvidersMapping);

		int count = airlineServiceProvidersMapping.ToList().Count();

		airlineServiceProvidersMapping = ApplyAirplineServiceProviderValuesSort(sidx, sord, airlineServiceProvidersMapping);

		airlineServiceProvidersMapping = airlineServiceProvidersMapping.Skip((page - 1) * rowNo).Take(rowNo);

            return Json(new
            {
                page = page,
                total = Math.Ceiling((double) count / (double) rowNo),
				 records = count,
				 airlineServiceProvidersMapping = airlineServiceProvidersMapping.ToList()

			}, JsonRequestBehavior.AllowGet);
		}

        [HttpPost]
        public JsonResult AirlineServiceProviderMappingEdit(Guid ClientID)
        {
			var airlineMapping = GetAirServiceProviderMappingForAddEdit(ClientID);
			var selectedProviderId = airlineMapping != null ? Context.eHubClients.FirstOrDefault(v => v.CC_PK == airlineMapping.AM_CC_AirServiceProvider)?.CC_ID : null;
			
			eHubAirServiceProviderMapping existedMapping;
			try
			{
				var oper = Request["oper"];
				var errorMessage = "";
				switch (oper)
				{
					case "add":
						errorMessage = CheckFieldRequirement(airlineMapping, selectedProviderId);
						AppendModelStateFromErrorMessage(errorMessage);
						if (!ModelState.IsValid)
						{

							var errors = String.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(x => x.ErrorMessage));
							return Json(new { success = false, message = errors }, JsonRequestBehavior.AllowGet);

						}
						
						existedMapping = GetAirServiceProviderMappingByCurrentOne(Context, airlineMapping);
						if (existedMapping != null)
                        {
							return Json(
                                new {success = false,
									message = "The AirServiceProviderMapping has existed"},
								JsonRequestBehavior.AllowGet
								);
                        }

						Context.eHubAirServiceProviderMappings.AddObject(airlineMapping);
						Context.SaveChanges();
						AddAirServiceProviderMappingLog("add", airlineMapping);
						SaveLogs();
						break;
					case "edit":
						errorMessage = CheckFieldRequirement(airlineMapping, selectedProviderId);
						AppendModelStateFromErrorMessage(errorMessage);
						if (!ModelState.IsValid)
						{

							var errors = String.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(x => x.ErrorMessage));
							return Json(new { success = false, message = errors }, JsonRequestBehavior.AllowGet);

						}

						existedMapping = GetAirServiceProviderMappingByRowId(Context, Request["id"]);
						if (existedMapping == null)
                        {
							return Json(
								new
								{
									success = false,
									message = "The record is not existed"
								},
								JsonRequestBehavior.AllowGet
								);
						}
						Context.eHubAirServiceProviderMappings.DeleteObject(existedMapping);
						Context.eHubAirServiceProviderMappings.AddObject(airlineMapping);
						Context.SaveChanges();
						AddAirServiceProviderMappingLog("edit", existedMapping);
						SaveLogs();
						break;
					case "del":
						existedMapping = GetAirServiceProviderMappingByRowId(Context, Request["id"]);
						if (existedMapping == null)
						{
							return Json(
								new
								{
									success = false,
									message = "The record is not existed"
								},
								JsonRequestBehavior.AllowGet
								);
						}
						Context.eHubAirServiceProviderMappings.DeleteObject(existedMapping);
						Context.SaveChanges();
						AddAirServiceProviderMappingLog("del", existedMapping);
						SaveLogs();
						break;
					default:
						break;
				}

				return Json(new { success = true }, JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				if (!String.IsNullOrEmpty(ex.Message))
                {
					ModelState.AddModelError("", ex.Message);
                }
				if(ex.InnerException != null)
                {
					ModelState.AddModelError("",
						ex.InnerException.Message.Contains("Violation of PRIMARY KEY")
						? "There is an existing record with the information you are trying to add"
						: ex.InnerException.Message);
				}
				var errors = String.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(x => x.ErrorMessage));
				return Json(new { success = false, message= errors }, JsonRequestBehavior.AllowGet);
			}
		}

		private eHubAirServiceProviderMapping GetAirServiceProviderMappingForAddEdit(Guid clientId)
        {
			var oper = Request["oper"];
			var airlineMapping = new eHubAirServiceProviderMapping();

			if (oper == "add" || oper == "edit")
            {
				airlineMapping.AM_CC_Client = clientId;
				airlineMapping.AM_DT_MessageType = new Guid(Request["MessageType"]);
				airlineMapping.AM_CC_Airline = new Guid(Request["Airline"]);
				airlineMapping.AM_CC_AirServiceProvider = new Guid(Request["ServiceProvider"]);
				airlineMapping.AM_ShipmentOrigin = string.IsNullOrEmpty(Request["ShipmentOrigin"]) ? "" : Request["ShipmentOrigin"].ToUpper();
				airlineMapping.AM_MessagePriority = string.IsNullOrEmpty(Request["MessagePriority"]) ? "" : Request["MessagePriority"].Split(' ', '-')[0]; ;
				airlineMapping.AM_ClientPIMA = string.IsNullOrEmpty(Request["ClientPIMA"]) ? "" : Request["ClientPIMA"].ToUpper();
				airlineMapping.AM_RecipientAddress = string.IsNullOrEmpty(Request["recipientAddress"]) ? "" : Request["recipientAddress"].ToUpper();
				airlineMapping.AM_DoubleSignatureCode = string.IsNullOrEmpty(Request["DoubleSignatureCode"]) ? "" : Request["DoubleSignatureCode"].ToUpper();
			}
			else if (oper == "del")
            {
				airlineMapping = null;
            }
			return airlineMapping;
		}

		private string CheckFieldRequirement(eHubAirServiceProviderMapping airlineMapping, string selectedProviderId)
		{
			var errorMessage = "";
			if (airlineMapping == null)
			{
				return errorMessage;
			}
			if (airlineMapping.AM_CC_Airline == Guid.Empty)
			{
				errorMessage += "Airline: Airline not found. Please check if this client is a valid airline or some typo existed;";
			}
			if (airlineMapping.AM_DT_MessageType == Guid.Empty)
			{
				errorMessage += "MessageType: MessageType not found. Please check if typo existed;";
			}

			if (airlineMapping.AM_CC_AirServiceProvider == Guid.Empty)
			{
				errorMessage += "ServiceProvider: ServiceProvider not found. Please check if this client is a valid serviceProvider or some typo existed;";
			}
			else
			{
				var fieldSemantics = eHubPortalSemanticsFactory.GetSemantics<string, string>(selectedProviderId, "AirServiceProviderMappingFields");
                var isAddIllegalFields = false;

                if (!fieldSemantics.ContainsKey("RecipientAddress") && !String.IsNullOrEmpty(airlineMapping.AM_RecipientAddress))
                {
                    isAddIllegalFields = true;
                }
                if (!fieldSemantics.ContainsKey("ClientPIMA") && !String.IsNullOrEmpty(airlineMapping.AM_ClientPIMA))
                {
                    isAddIllegalFields = true;
                }

                if (!fieldSemantics.ContainsKey("DoubleSignatureCode") && !String.IsNullOrEmpty(airlineMapping.AM_DoubleSignatureCode))
                {
                    isAddIllegalFields = true;
                }

                if (isAddIllegalFields)
				{
					errorMessage += "Special ServiceProvider required: For ServiceProvider which not ARINC/Qatar/Nalian, fields RecipientAddress/ClientPIMA/DoubleSignatureCode should be blank;";
				}

				string enabled;
				if (string.IsNullOrEmpty(airlineMapping.AM_RecipientAddress) && (fieldSemantics.TryGetValue("RecipientAddress", out enabled) && enabled == "enabled"))
				{
					errorMessage += $"RecipientAddress: RecipientAddress is required for provider {selectedProviderId};";
				}
			}

			if (string.IsNullOrEmpty(airlineMapping.AM_MessagePriority) ||
				!GetMessagePriorityDict().Keys.ToList().Contains(airlineMapping.AM_MessagePriority))
            {
				errorMessage += "MessagePriority: MessagePriority Should not blank, and only choose from QK/QD/QU";
            }

			if (!string.IsNullOrEmpty(airlineMapping.AM_RecipientAddress)
				&& string.IsNullOrEmpty(airlineMapping.AM_ClientPIMA))
			{
				errorMessage += "ClientPIMA: ClientPIMA is required when recipientAddress is given;";
			}

			if (!string.IsNullOrEmpty(airlineMapping.AM_ShipmentOrigin) && airlineMapping.AM_ShipmentOrigin.Length != 3)
			{
				errorMessage += "ShipmentOrigin: The length of ShipmentOrigin must be 3;";
			}
			return errorMessage;
		}

		private static eHubAirServiceProviderMapping GetAirServiceProviderMappingByRowId(
			IeHubTransactionsContext context,
			string rowId)
		{
			var primaryKeys = rowId.Split(':');
			var expectedKeyLength = 6;
			if (primaryKeys.Length != expectedKeyLength)
            {
				throw new ArgumentException($"The length of pks for AirServiceProviderMapping should be {expectedKeyLength}");
            }
			var clientPK = Guid.Parse(primaryKeys[0]);
			var airlinePK = Guid.Parse(primaryKeys[1]);
			var messageTypePK = Guid.Parse(primaryKeys[2]);
			var serviceProviderPK = Guid.Parse(primaryKeys[3]);
			var recipientAddress = primaryKeys[4];
			var shipmentOrigin = primaryKeys[5];
			return context.eHubAirServiceProviderMappings.FirstOrDefault(
				x => x.AM_CC_Client == clientPK
				&& x.AM_CC_Airline == airlinePK
				&& x.AM_DT_MessageType == messageTypePK
				&& x.AM_CC_AirServiceProvider == serviceProviderPK
				&& x.AM_RecipientAddress == recipientAddress
				&& x.AM_ShipmentOrigin == shipmentOrigin);
		}

		private static eHubAirServiceProviderMapping GetAirServiceProviderMappingByCurrentOne(IeHubTransactionsContext context,
			eHubAirServiceProviderMapping airlineMapping)
        {
			return  context.eHubAirServiceProviderMappings.FirstOrDefault(
				x => x.AM_CC_Client == airlineMapping.AM_CC_Client
				&& x.AM_CC_Airline == airlineMapping.AM_CC_Airline
				&& x.AM_DT_MessageType == airlineMapping.AM_DT_MessageType
				&& x.AM_CC_AirServiceProvider == airlineMapping.AM_CC_AirServiceProvider
				&& x.AM_RecipientAddress == airlineMapping.AM_RecipientAddress
				&& x.AM_ShipmentOrigin == airlineMapping.AM_ShipmentOrigin);
		}

		static IQueryable<AirlineProviderMappingGridView> ApplyAirplineServiceProviderValuesFilter(string searchField, string searchString, string searchOper, IQueryable<AirlineProviderMappingGridView> providers)
		{
			switch (searchField)
			{
				case "MessageType":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.MessageType == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.MessageType.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.MessageType.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.MessageType.Contains(searchString));
							break;
					}
					break;
				case "Airline":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.Airline == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.Airline.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.Airline.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.Airline.Contains(searchString));
							break;
					}
					break;
				case "ServiceProvider":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.ServiceProvider == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.ServiceProvider.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.ServiceProvider.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.ServiceProvider.Contains(searchString));
							break;
					}
					break;
				case "Messagepriority":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.MessagePriority == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.MessagePriority.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.MessagePriority.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.MessagePriority.Contains(searchString));
							break;
					}
					break;
				case "RecipientAddress":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.RecipientAddress == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.RecipientAddress.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.RecipientAddress.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.RecipientAddress.Contains(searchString));
							break;
					}
					break;

				case "DoubleSignatureCode":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.DoubleSignatureCode == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.DoubleSignatureCode.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.DoubleSignatureCode.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.DoubleSignatureCode.Contains(searchString));
							break;
					}
					break;

				case "ShipmentOrigin":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.ShipmentOrigin == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.ShipmentOrigin.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.ShipmentOrigin.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.ShipmentOrigin.Contains(searchString));
							break;
					}
					break;

				case "ClientPIMA":
					switch (searchOper)
					{
						case "eq":
							providers = providers.Where(x => x.ClientPIMA == searchString);
							break;
						case "bw":
							providers = providers.Where(x => x.ClientPIMA.StartsWith(searchString));
							break;
						case "ew":
							providers = providers.Where(x => x.ClientPIMA.EndsWith(searchString));
							break;
						case "cn":
							providers = providers.Where(x => x.ClientPIMA.Contains(searchString));
							break;
					}
					break;

				default:
					break;

			}
			return providers;
		}

		static IQueryable<AirlineProviderMappingGridView> ApplyAirplineServiceProviderValuesSort(string sidx, string sord, IQueryable<AirlineProviderMappingGridView> mappings)
		{
			switch (sidx + " " + sord)
			{
				case "MessageType asc":
					mappings = mappings.OrderBy(x => x.MessageType);
					break;
				case "MessageType desc":
					mappings = mappings.OrderByDescending(x => x.MessageType);
					break;
				case "ServiceProvider asc":
					mappings = mappings.OrderBy(x => x.ServiceProvider);
					break;
				case "ServiceProvider desc":
					mappings = mappings.OrderByDescending(x => x.ServiceProvider);
					break;
				case "Airline asc":
					mappings = mappings.OrderBy(x => x.Airline);
					break;
				case "Airline desc":
					mappings = mappings.OrderByDescending(x => x.Airline);
					break;

				case "Messagepriority asc":
					mappings = mappings.OrderBy(x => x.MessagePriority);
					break;
				case "Messagepriority desc":
					mappings = mappings.OrderByDescending(x => x.MessagePriority);
					break;

				case "RecipientAddress asc":
					mappings = mappings.OrderBy(x => x.RecipientAddress);
					break;
				case "RecipientAddress desc":
					mappings = mappings.OrderByDescending(x => x.RecipientAddress);
					break;

				case "DoubleSignatureCode asc":
					mappings = mappings.OrderBy(x => x.DoubleSignatureCode);
					break;
				case "DoubleSignatureCode desc":
					mappings = mappings.OrderByDescending(x => x.DoubleSignatureCode);
					break;

				case "ShipmentOrigin asc":
					mappings = mappings.OrderBy(x => x.ShipmentOrigin);
					break;
				case "ShipmentOrigin desc":
					mappings = mappings.OrderByDescending(x => x.ShipmentOrigin);
					break;

				case "ClientPIMA asc":
					mappings = mappings.OrderBy(x => x.ClientPIMA);
					break;
				case "ClientPIMA desc":
					mappings = mappings.OrderByDescending(x => x.ClientPIMA);
					break;

				default:
					mappings = mappings.OrderBy(x => x.MessageType);
					break;
			}
			return mappings;
		}

        [HttpGet]
		public FileContentResult AirServiceProviderMappingExportCsv()
        {
			var selectedClientPK = new Guid(Request["selectedClientPK"]);
			var selectedClient = Context.eHubClients.FirstOrDefault(r => r.CC_PK == selectedClientPK);
            using (var wrt = new StringWriter())
            using (var csv = new CsvHelper.CsvWriter(wrt))
            {
                csv.WriteField("MessageType");
				csv.WriteField("Airline");
				csv.WriteField("Service Provider");
				csv.WriteField("Recipient Address");
				csv.WriteField("Client PIMA");
				csv.WriteField("Message priority");
				csv.WriteField("Double Signature Code");
				csv.WriteField("Shipment Origin");
				csv.NextRecord();
				var airlineMappings = from mapping in Context.eHubAirServiceProviderMappings
				join client in Context.eHubClients on mapping.AM_CC_Client equals client.CC_PK
				join airline in Context.eHubClients on mapping.AM_CC_Airline equals airline.CC_PK
				join serviceProvider in Context.eHubClients on mapping.AM_CC_AirServiceProvider equals serviceProvider.CC_PK
				join messageType in Context.eHubMessageTypes on mapping.AM_DT_MessageType equals messageType.DT_PK
				where mapping.AM_CC_Client == selectedClientPK
				select new AirlineProviderMappingGridView
				{
					Airline = airline.CC_ID,
					MessageType = messageType.DT_Code,
					RecipientAddress = mapping.AM_RecipientAddress,
					ServiceProvider = serviceProvider.CC_ID,
					ClientPIMA = mapping.AM_ClientPIMA,
					MessagePriority = mapping.AM_MessagePriority,
					DoubleSignatureCode = mapping.AM_DoubleSignatureCode,
					ShipmentOrigin = mapping.AM_ShipmentOrigin
				};
				airlineMappings.ToList().ForEach(r =>
				{
					csv.WriteField(r.MessageType);
					csv.WriteField(r.Airline);
					csv.WriteField(r.ServiceProvider);
					csv.WriteField(r.RecipientAddress);
					csv.WriteField(r.ClientPIMA);
					csv.WriteField(r.MessagePriority);
					csv.WriteField(r.DoubleSignatureCode);
					csv.WriteField(r.ShipmentOrigin);
					csv.NextRecord();
				});
                string filename = String.Format("{0}_{1}.csv", "AirlineServiceProviderMapping", selectedClient.CC_ID);
                return File(Encoding.Default.GetBytes(wrt.ToString()), "text/text", filename);
            }
        }

		[HttpPost]
		public JsonResult AirServiceProviderMappingImportCsv()
        {
			var selectedClientPK = new Guid(Request["selectedClientPKForImport"]);
			var selectedClient = Context.eHubClients.FirstOrDefault(r => r.CC_PK == selectedClientPK);
			var records = new List<eHubAirServiceProviderMapping>();

			var airMappingCacheHelper = new AirMappingCacheHelper();

			using (var rdr = new StreamReader(Request.Files["uploadFile"].InputStream))
			using (var csv = new CsvHelper.CsvParser(rdr))
            {
				var fields = csv.Read();
				while ((fields = csv.Read()) != null)
                {

					var record = AirlineMappingTransform(airMappingCacheHelper, selectedClientPK, fields);
					records.Add(record);
				}
				var serviceProviderDict = GetServiceProviderDict(records);
				var errorMessages = CheckRecordRequirements(records, serviceProviderDict);
				if (errorMessages != "")
                {
					return Json(new { success = false, message = errorMessages }, JsonRequestBehavior.AllowGet);
				}

				var oldRecords = Context.eHubAirServiceProviderMappings.Where(r => r.AM_CC_Client == selectedClientPK).ToList();
				oldRecords.ForEach(Context.eHubAirServiceProviderMappings.DeleteObject);
				records.ForEach(Context.eHubAirServiceProviderMappings.AddObject);
				Context.SaveChanges();
				AddAirServiceProviderMappingBatchLog("del", oldRecords);
				AddAirServiceProviderMappingBatchLog("add", records);
			}

			return Json(new { success = true, message="Import CSV Success. Please refresh page to see the results." }, JsonRequestBehavior.AllowGet);
		}

		private void AppendModelStateFromErrorMessage(string errorMessage)
        {
			var errorPairs = errorMessage.Split(new char[] { ';'}, StringSplitOptions.RemoveEmptyEntries);
			errorPairs.ToList().ForEach(errorPair =>
			{
				var errorKV = errorPair.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				var key = errorKV[0].Trim();
				var value = errorKV[1].Trim();
				ModelState.AddModelError(key, value);

			});
        }

		private string CheckRecordRequirements(List<eHubAirServiceProviderMapping> records, Dictionary<Guid, string> serviceProviderDict)
        {
			var errorMessages = "";
			var uniqueMaps = new Dictionary<string, int>();
			var uniqueValue = 0;
			for(var lineNum=0; lineNum<records.Count();lineNum++)
            {

				var record = records[lineNum];
				var lineError = CheckFieldRequirement(record, serviceProviderDict[record.AM_CC_AirServiceProvider]);
				var uniqueIdentifier = string.Join("_",
					record.AM_CC_Client.ToString(),
					record.AM_CC_Airline,
					record.AM_DT_MessageType,
					record.AM_CC_AirServiceProvider,
					record.AM_RecipientAddress,
					record.AM_ShipmentOrigin);
				var repeatedError = uniqueMaps.TryGetValue(uniqueIdentifier, out uniqueValue)  ? "Duplicate record found." : "";
				uniqueMaps[uniqueIdentifier] = 1;
				if(lineError != "" || repeatedError != "")
                {
					errorMessages += String.Format("line {0}: {1}<br/><br/>", lineNum + 2, lineError + repeatedError);
                }
            }
			return errorMessages;
        }

		private Dictionary<Guid, string> GetServiceProviderDict(List<eHubAirServiceProviderMapping> records)
        {
			var serviceProviderDict = new Dictionary<Guid, string>();
			records.ForEach(r =>
			{
				if (!serviceProviderDict.ContainsKey(r.AM_CC_AirServiceProvider))
                {
					var providerID = Context.eHubClients.FirstOrDefault(v => v.CC_PK == r.AM_CC_AirServiceProvider)?.CC_ID;
					serviceProviderDict.Add(r.AM_CC_AirServiceProvider, providerID);
				}
			});
			return serviceProviderDict;
        }




		private eHubAirServiceProviderMapping AirlineMappingTransform(AirMappingCacheHelper airMappingCacheHelper, Guid clientPK, string[] fields)
        {

			var mappingData = new eHubAirServiceProviderMapping();
			var messageType = fields[0];
			var airline = fields[1];
			var serviceProvider = fields[2];
			var recipientAddress = fields[3];
			var clientPIMA = fields[4];
			var messagePriority = fields[5];
			var doubleSignatureCode = fields[6];
			var shipmentOrigin = fields[7];

			mappingData.AM_CC_Client = clientPK;
			mappingData.AM_DT_MessageType = airMappingCacheHelper.GetMessageTypeByCache(Context, messageType);
			mappingData.AM_CC_Airline = airMappingCacheHelper.GetAirlineByCache(Context, airline);
			mappingData.AM_CC_AirServiceProvider = airMappingCacheHelper.GetSerivceProviderByCache(Context, serviceProvider);
			mappingData.AM_RecipientAddress = string.IsNullOrEmpty(recipientAddress) ? "" : recipientAddress.ToUpper();
			mappingData.AM_ClientPIMA = string.IsNullOrEmpty(clientPIMA) ? "" : clientPIMA.ToUpper();
			mappingData.AM_MessagePriority = string.IsNullOrEmpty(messagePriority) ? "" : messagePriority.Split(' ', '-')[0];
			mappingData.AM_DoubleSignatureCode = string.IsNullOrEmpty(doubleSignatureCode) ? "" : doubleSignatureCode.ToUpper();
			mappingData.AM_ShipmentOrigin = string.IsNullOrEmpty(shipmentOrigin) ? "" : shipmentOrigin.ToUpper();
			return mappingData;
		}

		[HttpGet]
		public string GetSpecialServiceProviderNames()
        {
			return string.Join(";", new string[] { "ARINC_SP", "ARINC_SPTest", "Qatar", "Qatar_Test", "Nallian", "Nallian_Test" });
        }

		[HttpGet]
		public string GetMessagePriorities()
        {
			var priorityDict = GetMessagePriorityDict();
			var priorityList = new List<string>();
			foreach(var entry in priorityDict)
            {
				priorityList.Add(string.Format("{0}:{1}", entry.Key, entry.Value));
            }
			return string.Join(";", priorityList);
        }

		public Dictionary<string, string> GetMessagePriorityDict()
        {
			return new Dictionary<string, string>()
			{
				{"QK", "QK - Normal Priority"},
				{"QD", "QD - Deferred Priority"},
				{"QU", "QU - Urgent Priority"}
			};
        }

		private class AirMappingCacheHelper
		{

			protected Dictionary<string, Dictionary<string, Guid>> AirMappingCache;
			public AirMappingCacheHelper()
			{
				AirMappingCache = new Dictionary<string, Dictionary<string, Guid>>()
			{
				{"ServiceProvider", new Dictionary<string, Guid>()},
				{"MessageType", new Dictionary<string, Guid>()},
				{"Airline", new Dictionary<string, Guid>() }
			};
			}


			public Guid GetMessageTypeByCache(IeHubTransactionsContext context, string messageType)
			{
				var value = Guid.Empty;
				if (AirMappingCache["MessageType"].TryGetValue(messageType, out value))
				{
					return value;
				}
				else
				{
					var messageTypeItem = context.eHubMessageTypes.FirstOrDefault(r => r.DT_Code == messageType);
					AirMappingCache["MessageType"][messageType] = messageTypeItem == null ? Guid.Empty : messageTypeItem.DT_PK;
					return AirMappingCache["MessageType"][messageType];
				}
			}

			public Guid GetAirlineByCache(IeHubTransactionsContext context, string airline)
			{
				var value = Guid.Empty;
				if (AirMappingCache["Airline"].TryGetValue(airline, out value))
				{
					return value;
				}
				else
				{
					var airlineItem = context.eHubClients.FirstOrDefault(r => r.CC_ID == airline && !String.IsNullOrEmpty(r.CC_AirlineCode));
					AirMappingCache["Airline"][airline] = airlineItem == null ? Guid.Empty : airlineItem.CC_PK;
					return AirMappingCache["Airline"][airline];
				}
			}

			public Guid GetSerivceProviderByCache(IeHubTransactionsContext context, string serviceProvider)
			{
				var value = Guid.Empty;
				if (AirMappingCache["ServiceProvider"].TryGetValue(serviceProvider, out value))
				{
					return value;
				}
				else
				{
					var serviceProviderItem = context.eHubClients.FirstOrDefault(r => r.CC_ID == serviceProvider && r.CC_IsAirServiceProvider == true);
					AirMappingCache["ServiceProvider"][serviceProvider] = serviceProviderItem == null ? Guid.Empty : serviceProviderItem.CC_PK;
					return AirMappingCache["ServiceProvider"][serviceProvider];
				}
			}
		}

		#endregion
	}
}
