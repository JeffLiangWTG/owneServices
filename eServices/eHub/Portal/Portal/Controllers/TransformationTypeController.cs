using System;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Models;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;

namespace CargoWise.eHub.Portal.Controllers
{
	public class TransformationTypeController : ControllerBase
	{
		public ActionResult Find(Guid id)
		{
			var typeList = Context.FindTransformationTypeViewList(id);
			UpdateAsseblyName(typeList);
			return Json(typeList, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Add(int orderId)
		{
			var mapping = new eHubTransformationMapping();
			mapping.TM_Order = (byte)orderId;
			var mappingView = new TransformationMappingView();
			mappingView.Mapping = mapping;

			return View("TransformationTypeAutoCompleteTextBox", mappingView);
		}

		void UpdateAsseblyName(TransformationTypeView[] transformationTypeView)
		{
			for (int i = 0; i < transformationTypeView.Length; i++)
			{
				transformationTypeView[i].Name = UtilExtensions.FormatAsseblyName(transformationTypeView[i].Name);
			}
		}
	}
}
