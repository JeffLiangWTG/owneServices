using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;
using Common.Logging;

namespace CargoWise.eHub.Portal.Controllers
{
	public class TransformationSetController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("TransformationSetLogger");

		public ActionResult Index(Guid? sender, Guid? recipient, Guid? messageType)
		{
			if ((sender ?? recipient ?? messageType) == null)
			{
				return View();
			}

			if (!sender.HasValue && !recipient.HasValue && HttpContext.Request["opositClient"] != null)
			{
				string[] parts = HttpContext.Request["opositClient"].Trim().Split('/');
				if (parts.Length == 2)
				{
					sender = new Guid(parts[0]);
					recipient = new Guid(parts[1]);
				}
			}

			var transformationSetList = messageType.HasValue ? Context.GetTransformationSetList(messageType.Value) : Context.GetTransformationSetList(sender ?? Guid.Empty, recipient ?? Guid.Empty);

			return Request.IsAjaxRequest() ? View("TransformationSetGrid", transformationSetList) : View(transformationSetList);
		}

		public ActionResult Details(Guid id)
		{
			var set = Context.GetTransformationSetView(id);
			if (set == null) return View("NotFound");
			return View(set);
		}

		public ActionResult Create()
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return View("Edit", TransformationSetExtensions.CreateTransformationSetView());
		}

		[HttpPost]
		public ActionResult Create(FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var set = TransformationSetExtensions.CreateTransformationSetView();
			Context.eHubTransformationSets.AddObject(set.TransformationSet);
			return EditTransformationSetView(set, formValues);
		}

		public ActionResult Edit(Guid id)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			return View(Context.GetTransformationSetView(id));
		}

		[HttpPost]
		public ActionResult Edit(Guid id, FormCollection formValues)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var set = Context.GetTransformationSetView(id);
			return EditTransformationSetView(set, formValues);
		}

		public ActionResult Delete(Guid id)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var set = Context.GetTransformationSet(id);
			if (set == null) return View("NotFound");
			return View(set);
		}

		protected void AddTransformationSetLog(string oper, eHubTransformationSet set)
		{
			logger.Info(() => $"[{oper}] eHubTransformationSet: TS_PK={set.TS_PK}, TS_Name={set.TS_Name}, TS_CC_Sender={set.TS_CC_Sender}, TS_CC_Recipient={set.TS_CC_Recipient}, TS_DT_Source={set.TS_DT_Source}, TS_XPathPredicate={set.TS_XPathPredicate}" +
			$", TS_BillingInterfaceName={set.TS_BillingInterfaceName}, TS_BillingElement={set.TS_BillingElement}, TS_BillingXPathSource={set.TS_BillingXPathSource}, TS_BillingXPathTarget={set.TS_BillingXPathTarget}" +
			$", TS_BillSender={set.TS_BillSender}, TS_BillRecipient={set.TS_BillRecipient}, TS_CC_BillOther={set.TS_CC_BillOther}, TS_BillingNumMessagesIncluded={set.TS_BillingNumMessagesIncluded}, TS_BillingFee={set.TS_BillingFee}");
		}

		[HttpPost]
		public ActionResult Delete(Guid id, string confirmButton)
		{
			if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
				return RedirectToAction("Index");

			var set = Context.GetTransformationSet(id);
			if (set == null) return View("NotFound");

			Context.DeleteTransformationMappingList(id);
			Context.eHubTransformationSets.DeleteObject(set);

			try
			{
				Context.SaveChanges();
				AddTransformationSetLog("del", set);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", ex.InnerException.Message);
				return View("Delete", set);
			}

			return View("Deleted");
		}

		ActionResult EditTransformationSetView(TransformationSetView set, FormCollection formValues)
		{
			UpdateTransformationSet(set.TransformationSet, formValues);
			UpdateClientView(set.Sender, "Sender", formValues);
			UpdateClientView(set.Recipient, "Recipient", formValues);
			UpdateSource(set.Source, formValues);
			UpdateTransformationMapping(set, formValues);
			UpdateClientView(set.BillOther, "BillOther", formValues);

			if (ModelState.IsValid)
			{
				try
				{
					Context.SaveChanges();
					AddTransformationSetLog("edit", set.TransformationSet);
					return RedirectToAction("Details", new { id = set.TransformationSet.TS_PK });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.InnerException.Message);
					return View("Edit", set);
				}
			}
			else
			{
				return View("Edit", set);
			}
		}

		void UpdateTransformationSet(eHubTransformationSet transformationSet, FormCollection formValues)
		{
			transformationSet.TS_Name = formValues["TransformationSet.TS_Name"];
			transformationSet.TS_CC_Sender = formValues["CNameSender"] == "" ? null : formValues["CIdSender"].ToNullableGuid();
			transformationSet.TS_CC_Recipient = formValues["CNameRecipient"] == "" ? null : formValues["CIdRecipient"].ToNullableGuid();
			transformationSet.TS_DT_Source = formValues["TName"] == "" ? null : formValues["TId"].ToNullableGuid();
			transformationSet.TS_XPathPredicate = formValues["TransformationSet.TS_XPathPredicate"];
			transformationSet.TS_BillSender = formValues["TransformationSet.TS_BillSender"].ToUpper() == "TRUE";
			transformationSet.TS_BillRecipient = formValues["TransformationSet.TS_BillRecipient"].ToUpper() == "TRUE";
			transformationSet.TS_CC_BillOther = formValues["CNameBillOther"] == "" ? null : formValues["CIdBillOther"].ToNullableGuid();
			transformationSet.TS_BillingNumMessagesIncluded = formValues["TransformationSet.TS_BillingNumMessagesIncluded"] == "" ? null : (int?)int.Parse(formValues["TransformationSet.TS_BillingNumMessagesIncluded"]);
			transformationSet.TS_BillingFee = formValues["TransformationSet.TS_BillingFee"] == "" ? null : (decimal?)decimal.Parse(formValues["TransformationSet.TS_BillingFee"]);

			TryUpdateModel<eHubTransformationSet>(transformationSet);
		}

		void UpdateTransformationMapping(TransformationSetView set, FormCollection formValues)
		{
			set.MappingList.Clear();
			Context.DeleteTransformationMappingList(set.TransformationSet.TS_PK);
			if (!formValues.AllKeys.Any(k => k.StartsWith("MId"))) return;

			int orderId = 0;

			var transformationTypeList = new List<eHubTransformationType>();

			foreach (string key in formValues.Keys)
			{
				if (!key.StartsWith("MId")) continue;

				var transformationId = new Guid(formValues[key]);
				if (transformationId == Guid.Empty) continue;

				var transformationType = Context.GetTransformationType(transformationId);
				if (transformationType == null) continue;

				var transformation = new eHubTransformationMapping() { TM_Order = (byte)orderId, TM_TT_PK = transformationId, TM_TS_PK = set.TransformationSet.TS_PK };

				transformationTypeList.Add(transformationType);
				set.MappingList.Add(new TransformationMappingView() { Mapping = transformation, TransformationTypeName = transformationType.TT_TransformationType });
				Context.eHubTransformationMappings.AddObject(transformation);
				TryUpdateModel<eHubTransformationMapping>(transformation);

				orderId++;
			}
		}

		void UpdateSource(MessageTypeView messageTypeView, FormCollection formValues)
		{
			if (formValues["TName"] == "")
			{
				messageTypeView.Id = null;
				messageTypeView.Name = "";
			}
			else
			{
				messageTypeView.Id = new Guid(formValues["TId"]);
				messageTypeView.Name = formValues["TName"];
			}
		}

		void UpdateClientView(SelectValueView clientView, string type, FormCollection formValues)
		{
			if (formValues["CName" + type] == "")
			{
				clientView.Id = null;
				clientView.Name = "";
			}
			else
			{
				clientView.Id = formValues["CId" + type].ToNullableGuid();
				clientView.Name = formValues["CName" + type];
			}
		}
	}
}
