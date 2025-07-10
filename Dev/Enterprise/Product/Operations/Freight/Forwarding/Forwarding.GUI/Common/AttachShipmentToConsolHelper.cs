using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class AttachShipmentToConsolHelper
	{
		public static bool CheckAttaching(List<BusinessObject> businessObjectsToAttach, ForwardingConsol consol)
		{
			var result = true;

			if (consol != null)
			{
				string caption = Res.GetString("9fc3c5af-9d37-4194-96d7-7576e5b61a51", "Attaching Shipments...");
				var shipmentsToRemove = new List<ForwardingShipment>();
				var shipmentsToCheck = new List<ForwardingShipment>();
				var shipmentsToSkip = new List<ForwardingShipment>();
				var checkMessageList = new List<string>();
				var skipMessageList = new List<string>();
				var shipmentsWithNonMatchingAgents = new List<CommonShipment>();

				(var wasDialogDisplayed, var dialogResult) = ValidateDates(caption, businessObjectsToAttach.Cast<CommonShipment>(), new[] { consol });
				if (wasDialogDisplayed && dialogResult == ZDialogResult.Cancel)
				{
					return false;
				}

				if (!AddWarningIfEstimatedDeliveryDateIsEarlierThanConsolETA(caption, businessObjectsToAttach.Cast<CommonShipment>(), new[] { consol }))
				{
					return false;
				}

				if (!ValidateComplianceRiskAndContinue(caption, businessObjectsToAttach.Cast<CommonShipment>(), new[] { consol }))
				{
					return false;
				}

				foreach (ForwardingShipment shipment in businessObjectsToAttach)
				{
					var master = (ForwardingShipment)shipment.CoLoadMasterShipment;
					var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(consol, shipment);
					if (master != null)
					{
						if (businessObjectsToAttach.Contains(master))
						{
							shipmentsToRemove.Add(shipment);
						}
						else if (!attachRequest.Errors.IsEmpty)
						{
							skipMessageList.Add(attachRequest.Errors);
							shipmentsToSkip.Add(shipment);
						}
						else if (!master.Consols.Contains(consol))
						{
							checkMessageList.Add(Res.GetString("310b9f33-a651-4ec8-85df-32aab43070b7",
															   "{0} is a sub-shipment of master/lead {1}",
															   shipment.JS_UniqueConsignRef,
															   master.JS_UniqueConsignRef));

							shipmentsToCheck.Add(shipment);
						}
					}
					else if (!attachRequest.Errors.IsEmpty)
					{
						skipMessageList.Add(attachRequest.Errors);
						shipmentsToSkip.Add(shipment);
					}
					else if (!string.IsNullOrEmpty(FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedReceivingAgents(new[] { shipment }, new[] { consol })))
					{
						shipmentsWithNonMatchingAgents.Add(shipment);
					}
					else if (!string.IsNullOrEmpty(FreightShipmentVsConsolMessageHelper.Instance.CheckRelatedSendingAgents(new[] { shipment }, new[] { consol })))
					{
						shipmentsWithNonMatchingAgents.Add(shipment);
					}
				}

				if (shipmentsToSkip.Count > 0)
				{
					skipMessageList.Sort();

					string message = Res.GetString("cd289920-865f-4847-9554-dcfb9f967c01",
												   "Of the shipments you are trying to attach to the consol {1}, there are shipments that cannot be attached for the following reasons:{2}{0}",
												   string.Join(System.Environment.NewLine, skipMessageList.ToArray()),
												   consol.JK_UniqueConsignRef,
												   System.Environment.NewLine);
					if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel) == DialogResult.Cancel)
					{
						return false;
					}

					shipmentsToRemove.AddRange(shipmentsToSkip);
				}

				if (shipmentsWithNonMatchingAgents.Any())
				{
					if (!NonMatchingAgentsDialog.CheckAndConfirm(new[] { consol }, shipmentsWithNonMatchingAgents))
					{
						shipmentsToRemove.AddRange(shipmentsWithNonMatchingAgents.Cast<ForwardingShipment>());
					}
				}

				if (shipmentsToCheck.Count > 0)
				{
					checkMessageList.Sort();

					string message = Res.GetString("cd746331-71de-4160-909b-4ce9c9f28987",
													"The shipments that you are trying to attach are sub-shipments:\r\n{0}\r\n\r\nOnly these shipments, without their masters, will be attached to {1} consol.\r\n\r\nIf you would like to attach these shipments, their masters/leads and all sub-shipments of their masters/leads to this consol, you need to attach the master/lead shipments to this consol instead.\r\n\r\nPress [Yes] if you would like to continue.\r\nPress [No] if you would like to skip this shipments and apply all other selected shipments.\r\nPress [Cancel] to cancel operation.",
													string.Join(System.Environment.NewLine, checkMessageList.ToArray()),
													consol.JK_UniqueConsignRef);

					var dilogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
					switch (dilogResult)
					{
						case DialogResult.Yes:
							break;
						case DialogResult.No:
							shipmentsToRemove.AddRange(shipmentsToCheck);
							break;
						default:
							return false;
					}
				}

				foreach (var shipment in shipmentsToRemove)
				{
					businessObjectsToAttach.Remove(shipment);
				}

				result = businessObjectsToAttach.Count > 0;
			}

			return result;
		}

		internal static (bool wasDialogDisplayed, ZDialogResult dislogResult) ValidateDates(string caption, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var messages = FreightShipmentVsConsolMessageHelper.Instance.CheckDatesWithinRange(shipments, consols);
			if (messages.IsNullOrEmpty())
			{
				return (false, ZDialogResult.None);
			}

			var dialogResult = ShowDatesWarningMessage(caption, messages);
			if (dialogResult == ZDialogResult.No)
			{
				shipments.ForEach(shipment => shipment.IsSuppressedETAETDOnAttachToConsol = true);
			}

			return (true, dialogResult);
		}

		internal static bool ValidateComplianceRiskAndContinue(string caption, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var message = FreightShipmentVsConsolMessageHelper.Instance.CheckShipmentAndConsolComplianceRisk(shipments, consols);
			if (message.IsNullOrEmpty())
			{
				return true;
			}

			var dialogResult = ShowComplianceWarningMessage(caption, message);

			return dialogResult == ZDialogResult.Yes;
		}

		internal static ZDialogResult ShowDatesWarningMessage(string caption, IEnumerable<string> messages)
		{
			string message = Res.GetString("14554bff-ff08-49e4-83e1-b12883ca1e66",
				"There's inconsistency between ETD/ETA of Shipments and Consols you are trying to link.\r\nSee details below:\r\n{0}\r\n\r\nHow would you like to proceed?\r\n\r\nPress [Yes] to attach and update the Shipments dates to match the Consols dates.\r\nPress [No] to attach Shipments but do not update Shipments dates.\r\nPress [Cancel] to cancel operation.",
				string.Join(System.Environment.NewLine, messages.ToArray()));

			var dialogContext = new DialogDefaultContext(new ZGuid("7dc50ec6-7a1d-4cc9-a4e2-9df225539e0d"), caption, ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Question, null, showCheckboxOnly: true);
			return Globals.Message.ShowOrDefault(dialogContext, message);
		}

		internal static ZDialogResult ShowComplianceWarningMessage(string caption, string message)
		{
			var dialogContext = new DialogDefaultContext(
						new ZGuid("7063df15-6d34-460e-89f8-6c6a1cf0d120"), caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning, null, showCheckboxOnly: true, defaultResult: ZDialogResult.No);
			return Globals.Message.ShowOrDefault(dialogContext, message);
		}

		internal static bool AddWarningIfEstimatedDeliveryDateIsEarlierThanConsolETA(string caption, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var messages = FreightShipmentVsConsolMessageHelper.Instance.CheckShipmentEstimatedDeliveryIsAfterConsolArrival(shipments, consols);
			if (messages.IsNullOrEmpty())
			{
				return true;
			}

			return ShowEstimatedDeliveryDateWarningMessage_ShowWarning(caption, messages) == ZDialogResult.Yes;
		}

		static ZDialogResult ShowEstimatedDeliveryDateWarningMessage_ShowWarning(string caption, IEnumerable<string> messages)
		{
			string message = Res.GetString("a3711606-848c-4290-a855-37f865938073",
				"The Estimated Delivery Date of below shipment(s) you are trying to link is before Consol's ETA.\r\n{0}\r\n\r\nIf [No] is selected, shipment will not be attached as the Estimated Delivery Date will not be met.\r\nIf [Yes] is selected, shipment will be attached with a warning on the Estimated Delivery Date field.",
				string.Join(System.Environment.NewLine, messages.ToArray()));

			var dialogContext = new DialogDefaultContext(new ZGuid("d8ad597d-b59d-4a17-849b-8ad3baa5b2f8"), caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, null, showCheckboxOnly: true);
			return Globals.Message.ShowOrDefault(dialogContext, message);
		}
	}
}
