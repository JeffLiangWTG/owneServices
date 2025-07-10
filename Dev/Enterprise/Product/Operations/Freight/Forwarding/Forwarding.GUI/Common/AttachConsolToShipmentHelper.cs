using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class AttachConsolToShipmentHelper
	{
		public static bool CheckAttaching(List<BusinessObject> businessObjectsToAttach, CommonShipment shipment)
		{
			var result = true;

			if (shipment != null)
			{
				string caption = Res.GetString("abd626e9-ef19-4c34-87cf-9f221e6fff7e", "Attaching Consols...");
				var consolsToSkip = new List<CommonConsol>();
				var consolsWithNonMatchingAgents = new List<CommonConsol>();
				var skipMessageList = new List<string>();

				(var wasDialogDisplayed, var dialogResult) = AttachShipmentToConsolHelper.ValidateDates(caption, new[] { shipment }, businessObjectsToAttach.Cast<CommonConsol>());
				if (wasDialogDisplayed && dialogResult == ZDialogResult.Cancel)
				{
					return false;
				}

				if (!AttachShipmentToConsolHelper.AddWarningIfEstimatedDeliveryDateIsEarlierThanConsolETA(caption, new[] { shipment }, businessObjectsToAttach.Cast<CommonConsol>()))
				{
					return false;
				}

				if (!AttachShipmentToConsolHelper.ValidateComplianceRiskAndContinue(caption, new[] { shipment }, businessObjectsToAttach.Cast<CommonConsol>()))
				{
					return false;
				}

				foreach (CommonConsol consol in businessObjectsToAttach)
				{
					var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(shipment, consol);
					var relatedReceivingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(new[] { shipment }, new[] { consol });
					var relatedSendingAgentsWarning = FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(new[] { shipment }, new[] { consol });

					if (!attachRequest.Errors.IsEmpty)
					{
						skipMessageList.Add(attachRequest.Errors);
						consolsToSkip.Add(consol);
					}
					else if (!string.IsNullOrEmpty(relatedReceivingAgentsWarning))
					{
						consolsWithNonMatchingAgents.Add(consol);
					}
					else if (!string.IsNullOrEmpty(relatedSendingAgentsWarning))
					{
						consolsWithNonMatchingAgents.Add(consol);
					}
				}

				if (consolsToSkip.Count > 0)
				{
					skipMessageList.Sort();

					string message = Res.GetString("39b7b1c4-7acd-4b57-a193-86cfbe492c40",
						"Of the consols you are trying to attach to the shipment {1}, there are consols that cannot be attached for the following reasons:{2}{0}",
						String.Join(System.Environment.NewLine, skipMessageList.ToArray()),
						shipment.JS_UniqueConsignRef,
						System.Environment.NewLine);

					if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel) == DialogResult.Cancel)
					{
						return false;
					}
				}

				if (consolsWithNonMatchingAgents.Any())
				{
					if (!NonMatchingAgentsDialog.CheckAndConfirm(consolsWithNonMatchingAgents, new[] { shipment }))
					{
						consolsToSkip.AddRange(consolsWithNonMatchingAgents.Cast<ForwardingConsol>());
					}
				}

				Func<BusinessObject, bool> consolIsDirect = c => ((CommonConsol)c).IsDirect;
				Func<BusinessObject, bool> consolIsNotDirect = c => !((CommonConsol)c).IsDirect;

				if (businessObjectsToAttach.Any(consolIsDirect) && businessObjectsToAttach.Any(consolIsNotDirect))
				{
					string message = Res.GetString("3ff2eb0a-dc6e-4d03-be14-a059a910aa55",
						"You can not attach both Direct and Non-Direct consols to the same shipment. Would you like to ignore all Non-Direct consols?\r\n\r\nSelect Yes to attach only Direct Consols.\r\nSelect No to attach only Non-Direct Consols.\r\nSelect Cancel to abort the operation.");
					switch (Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, DialogResult.Cancel))
					{
						case DialogResult.Yes:
							consolsToSkip.AddRange(businessObjectsToAttach.Where(consolIsNotDirect).Cast<CommonConsol>().ToList());
							break;
						case DialogResult.No:
							consolsToSkip.AddRange(businessObjectsToAttach.Where(consolIsDirect).Cast<CommonConsol>().ToList());
							break;
						case DialogResult.Cancel:
							return false;
					}
				}

				foreach (var consol in consolsToSkip)
				{
					businessObjectsToAttach.Remove(consol);
				}

				result = businessObjectsToAttach.Count > 0;
			}

			return result;
		}
	}
}
