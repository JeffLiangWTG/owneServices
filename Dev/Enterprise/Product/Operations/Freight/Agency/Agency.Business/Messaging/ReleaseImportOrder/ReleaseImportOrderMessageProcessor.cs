using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseImportOrderMessageProcessor : BaseBillOfLadingMessageProcessor
	{
		public ReleaseImportOrderMessageProcessor(BillOfLading bizo) : base(bizo)
		{
		}

		public override bool ValidateSet(INotifications notifications)
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			var eHubID = EHubID;

			if (string.IsNullOrEmpty(eHubID))
			{
				notifications.Add(NotificationType.Error, FormattableString.Invariant($"The Import Release Order message cannot be sent as eHub ID is not set.")); // workflow processor message
			}
			else if (orgProxy == null)
			{
				notifications.Add(NotificationType.Error, FormattableString.Invariant($"The current branch does not have an org. proxy."));  // workflow processor message
			}
			else
			{
				bizo.MarkAsNeedingValidation();
				bizo.RunPreSaveValidation();

				if (bizo.HasErrors || bizo.HasMessageErrors)
				{
					notifications.Add(NotificationType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} has errors and/or message errors.\r\nYou will need to correct these before an Import Release Order message can be sent for it.", bizo.JS_UniqueConsignRef));  // workflow processor message
				}
				else
				{
					if ((orgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, bizo.JS_NKDischargePort.SubstringSafe(0, 2)) ?? string.Empty).IsEmpty)
					{
						notifications.Add(NotificationType.Warning, FormattableString.Invariant($"The current branch's organization proxy does not have a CAR code for the country/region of container's port of discharge entered. Containers is skipped."));  // workflow processor message
					}

					var portConfig = MessageSender.RetrievePortConfiguration(bizo.JS_NKDischargePort, bizo.Principal);
					if (portConfig == null || !portConfig.Enabled)
					{
						notifications.Add(NotificationType.Warning, string.Format(CultureInfo.InvariantCulture, (NoResString)"Principal is not configured for sending Import Release Order to {0}.", bizo.JS_NKDischargePort));  // workflow processor message
					}

					return true;
				}
			}

			return false;
		}

		public string EHubID
		{
			get
			{
				return ShippingPortsMessagingEHubIDHelper.GetEHubID(bizo.JS_NKDischargePort);
			}
		}

		public override void SendMessage(INotifications notifications, List<BillOfLadingContainer> containers)
		{
			if (containers.Any())
			{
				foreach (var container in containers)
				{
					MessageSender.SendMessage(notifications, container);
				}
			}
		}

		SendNZReleaseOrderActionMethodApplicator MessageSender => messageSender ?? (messageSender = new SendNZReleaseOrderActionMethodApplicator(new ReleaseImportOrderSettings()));
		SendNZReleaseOrderActionMethodApplicator messageSender;
	}
}
