using System;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Models;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;
using Common.Logging;
using MvcContrib.Pagination;
using MvcContrib.Sorting;
using MvcContrib.UI.Grid;

namespace CargoWise.eHub.Portal.Controllers
{
	public class MessageTypeController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("MessageTypeLogger");

		public ActionResult Index(string Code, int? formatID, GridSortOptions gridSortOptions, int? page)
		{
			var list = Context.GetMessageTypeQuery();

			// Set default sort column
			if (string.IsNullOrWhiteSpace(gridSortOptions.Column))
			{
				gridSortOptions.Column = "DT_Code";
			}

			// Filter on Cient ID 
			if (!String.IsNullOrEmpty(Code))
			{
				list = list.Where(c => c.DT_Code.Contains(Code));
			}

			// Filter on Cient Name
			if (formatID.HasValue)
			{
				if (formatID.Value == 0)
				{
					list = list.Where(c => c.DT_IsEDI == true);
				}
				if (formatID.Value == 1)
				{
					list = list.Where(c => c.DT_IsFlatFile == false);
				}

				if (formatID.Value == 2)
				{
					list = list.Where(c => c.DT_IsFlatFile == true);
				}
			}


			var filterViewModel = new MessageTypeFilterViewModel();
			filterViewModel.Code = Code;
			filterViewModel.SelectedFormatID = formatID ?? -1;
			filterViewModel.Fill();

			// Order and page the product list
			var pagedList = list.OrderBy(gridSortOptions.Column, gridSortOptions.Direction).AsPagination(page ?? 1, Constants.PageSize);


			var container = new MessageTypeListContainerViewModel
			{
				PagedList = pagedList,
				FilterViewModel = filterViewModel,
				GridSortOptions = gridSortOptions
			};

			return View(container);
		}

		public ActionResult Find(string text)
		{
			if (text.Length < Constants.MinLettersToStartMessageTypeSearch) return new EmptyResult();
			return Json(Context.FindMessageTypeViewList(text), JsonRequestBehavior.AllowGet);
		}

		public ActionResult DetailsModal(Guid id)
		{
			return ModalRedirect(Details(id));
		}

		public ActionResult Details(Guid id)
		{
			var type = Context.GetMessageType(id);
			if (type == null) return View("NotFound");
			return View("Details", type);
		}

		public ActionResult EditModal(Guid id)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return ModalRedirect(Edit(id));
		}

		[HttpPost]
		public ActionResult EditModal(Guid id, FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return ModalRedirect(Edit(id, formValues));
		}

		public ActionResult Edit(Guid id)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return View("Edit", Context.GetMessageType(id));
		}

		protected void AddMessageTypeLog(string oper, eHubMessageType type)
		{
			logger.Info(() => $"[{oper}] eHubMessageType: DT_PK={type.DT_PK}, DT_Code={type.DT_Code}, DT_IsFlatFile={type.DT_IsFlatFile}, DT_IsEDI={type.DT_IsEDI}, DT_Charset={type.DT_Charset}, DT_EnvelopeXpath={type.DT_EnvelopeXpath}" +
			$", DT_DT_InnerType={type.DT_DT_InnerType}, DT_ReprocessSubMessage={type.DT_ReprocessSubMessage}, DT_PostAssembleMapping={type.DT_PostAssembleMapping}, DT_DT_PostAssembleWrapper={type.DT_DT_PostAssembleWrapper}, DT_IsJson={type.DT_IsJson}");
		}

		[HttpPost]
		public ActionResult Edit(Guid id, FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var type = Context.GetMessageType(id);

			if (ModelState.IsValid)
			{
				try
				{
					Context.SaveChanges();
					AddMessageTypeLog("edit", type);
					return RedirectToAction("Details", new { id = type.DT_PK });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.InnerException.Message);
					return View("Edit", type);
				}
			}
			else
			{
				return View("Edit", type);
			}

		}
	}
}
