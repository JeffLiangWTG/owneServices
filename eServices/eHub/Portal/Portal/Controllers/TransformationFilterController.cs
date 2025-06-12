using System;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Models;

namespace CargoWise.eHub.Portal.Controllers
{
	public class TransformationFilterController : ControllerBase
	{
		public ActionResult Clients(string type, string client)
		{
			if (client == "*")
			{
				return Json(new SelectValueView[] { new SelectValueView
					{
						Id = Guid.Empty,
						Code = "*",
						Name = "Multiple"
					}
				}, JsonRequestBehavior.AllowGet);
			}
			else if (type == "sender")
			{
				var clients = from t in Context.eHubTransformationSets
							  join s in Context.eHubClients on t.TS_CC_Sender equals s.CC_PK
							  where s.CC_FriendlyName.StartsWith(client) || s.CC_ID.StartsWith(client)
							  select new SelectValueView { Id = s.CC_PK, Code = s.CC_ID, Name = s.CC_FriendlyName };

				return Json(clients.Distinct().OrderBy(r => r.Code).ToArray(), JsonRequestBehavior.AllowGet);
			}
			else if (type == "recipient")
			{
				var clients = from t in Context.eHubTransformationSets
							  join r in Context.eHubClients on t.TS_CC_Recipient equals r.CC_PK
							  where r.CC_FriendlyName.StartsWith(client) || r.CC_ID.StartsWith(client)
							  select new SelectValueView { Id = r.CC_PK, Code = r.CC_ID, Name = r.CC_FriendlyName };

				return Json(clients.Distinct().OrderBy(r => r.Code).ToArray(), JsonRequestBehavior.AllowGet);
			}
			else if (type == "messageType")
			{
				var clients = from t in Context.eHubTransformationSets
							  join r in Context.eHubMessageTypes on t.TS_DT_Source equals r.DT_PK
							  where r.DT_Code.Contains(client)
							  select new SelectValueView { Id = r.DT_PK, Code = r.DT_Code, Name = r.DT_Code };

				return Json(clients.Distinct().OrderBy(r => r.Code).ToArray(), JsonRequestBehavior.AllowGet);
			}
			else
			{
				return new EmptyResult();
			}
		}

		public ActionResult OpositClients(string type, Guid client)
		{
			if (type == "sender")
			{
				var clients = from t in Context.eHubTransformationSets
							  where (t.TS_CC_Sender ?? Guid.Empty) == client
							  select new ClientSetView
							  {
								  SenderId = client,
								  RecipientId = t.TS_CC_Recipient ?? Guid.Empty,
								  Name = t.TS_CC_Recipient == null ? "Multiple" : t.eHubClient_Recipient.CC_FriendlyName,
								  Code = t.TS_CC_Recipient == null ? "*" : t.eHubClient_Recipient.CC_ID
							  };

				return Json(clients.Distinct().OrderBy(r => r.Code).ToArray(), JsonRequestBehavior.AllowGet);
			}

			else if (type == "recipient")
			{
				var clients = from t in Context.eHubTransformationSets
							  where (t.TS_CC_Recipient ?? Guid.Empty) == client
							  select new ClientSetView
							  {
								  SenderId = t.TS_CC_Sender ?? Guid.Empty,
								  RecipientId = client,
								  Name = t.TS_CC_Sender == null ? "Multiple" : t.eHubClient_Sender.CC_FriendlyName,
								  Code = t.TS_CC_Sender == null ? "*" : t.eHubClient_Sender.CC_ID
							  };

				return Json(clients.Distinct().OrderBy(r => r.Code).ToArray(), JsonRequestBehavior.AllowGet);
			}
			else
			{
				return new EmptyResult();
			}
		}
	}
}
