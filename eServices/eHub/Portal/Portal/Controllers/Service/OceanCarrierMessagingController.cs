using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging;
using eServices.eHubDataModel.eHubTransactions;

namespace CargoWise.eHub.Portal.Controllers.Service
{
	public class OceanCarrierMessagingController : Controller
	{
		readonly List<IOceanCarrierMessagingType> OceanCarrierMessagingTypes;
		readonly IOceanCarrierMessagingType DefaultCarrierType;
		internal IOcmConfigBackupManager ConfigBackupManager = new OcmConfigBackupManager();

		public static class TypeNames
		{
			public const string SplitingBlackList = "SplitingBlackList";
			public const string SplitingWhiteList = "SplitingWhiteList";
			public const string MultipleRecipients = "MultipleRecipientsCopying";
			public const string CarrierHandlingAgent = "CarrierHandlingAgent";
			public const string CarrierBookingAgent = "CarrierBookingAgent";
			public const string DefaultCarrier = "DefaultCarrier";
		}
		
		public OceanCarrierMessagingController()
			: this(new eHubTransactionsContext())
		{ }

		public OceanCarrierMessagingController(eHubTransactionsContext context)
		{
			DefaultCarrierType = new DefaultCarrierType(this, context);
			OceanCarrierMessagingTypes = new List<IOceanCarrierMessagingType>()
			{
				new SplitingBlackListType(this, context),
				new SplitingWhiteListType(this, context),
				new MultipleRecipientType(this, context),
				new CarrierHandlingAgentType(this, context),
				new CarrierBookingAgentType(this, context),
				DefaultCarrierType
			};
		}

		internal IOceanCarrierMessagingType GetOceanCarrierMessagingType(string typeID = null)
		{
			typeID = string.IsNullOrEmpty(typeID) ? (string)Session["OCMTypeID"] : typeID;
			return OceanCarrierMessagingTypes.FirstOrDefault(x => x.ID == typeID) ?? DefaultCarrierType;
		}

		#region public method

		public ActionResult Index()
		{
			var type = DefaultCarrierType;
			Session["OCMTypeID"] = type.ID;

			return View("Index", new OCMIndexModel()
			{
				Selected = type.ID,
				Options = OceanCarrierMessagingTypes.Select(x => new Option()
				{
					OptionID = x.ID,
					Description = string.IsNullOrEmpty(x.PrefixName) ? x.Name : string.Format("{0} - {1}", x.PrefixName, x.Name)
				}).ToList()
			});
		}

		public JsonResult SelectType()
		{
			var type = GetOceanCarrierMessagingType(Request["id"]);
			Session["OCMTypeID"] = type.ID;
			return Json(new { rows = type.JqGridColumns() }, JsonRequestBehavior.AllowGet);
		}

		public string GetColumnName(string column)
		{
			return GetOceanCarrierMessagingType().GetColumnName(column);
		}

		[HttpPost]
		public JsonResult SaveAll()
		{
			return Json(GetOceanCarrierMessagingType().Save(), JsonRequestBehavior.AllowGet);
		}

		public JsonResult Clients(bool includeNonProd)
		{
			return Json(GetOceanCarrierMessagingType().GetClients(includeNonProd), JsonRequestBehavior.AllowGet);
		}

		public JsonResult ServiceProviders()
		{
			return Json(GetOceanCarrierMessagingType().GetServiceProviders(), JsonRequestBehavior.AllowGet);
		}

		public JsonResult DocumentNames()
		{
			return Json(GetOceanCarrierMessagingType().GetDocumentNames(), JsonRequestBehavior.AllowGet);
		}

		public JsonResult PartiesToCopy()
		{
			return Json(GetOceanCarrierMessagingType().GetPartiesToCopy(), JsonRequestBehavior.AllowGet);
		}

		public JsonResult SplitByOptions()
		{
			return Json(GetOceanCarrierMessagingType().GetSplitByOptions(), JsonRequestBehavior.AllowGet);
		}

		public JsonResult ShipmentTypes()
		{
			return Json(GetOceanCarrierMessagingType().GetShipmentTypes(), JsonRequestBehavior.AllowGet);
		}

		public JsonResult Values()
		{
			return Json(GetOceanCarrierMessagingType().Values(), JsonRequestBehavior.AllowGet);
		}

		public void ClearCache()
		{
			GetOceanCarrierMessagingType().ClearCache();
		}

		public JsonResult MoveRow()
		{
			return Json(GetOceanCarrierMessagingType().MoveRow(), JsonRequestBehavior.AllowGet);
		}

		public void ReformatSessionRowID()
		{
			GetOceanCarrierMessagingType().ReformatSessionRowID();
		}

		[HttpPost]
		public JsonResult ValuesEdit()
		{
			return Json(GetOceanCarrierMessagingType().ValuesEdit(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public FileContentResult ExportCSV()
		{
			return File(GetOceanCarrierMessagingType().GetValuesExportCsv(), "text/csv", GetOceanCarrierMessagingType().GetExportFileName());
		}

		[HttpPost]
		public JsonResult ImportCSV()
		{
			var ocmType = GetOceanCarrierMessagingType();
			ConfigBackupManager.Backup(Server, ocmType.ID, ocmType.GetValuesExportCsv());
			return Json(ocmType.ImportCsv(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult ExtendSession()
		{
			// Do nothing but extend the session
			return new EmptyResult();
		}

		#endregion
	}
}
