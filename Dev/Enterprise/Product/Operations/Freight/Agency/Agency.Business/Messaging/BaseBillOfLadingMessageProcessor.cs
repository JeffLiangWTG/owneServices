using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class BaseBillOfLadingMessageProcessor : IProcessor
	{
		public BaseBillOfLadingMessageProcessor(BillOfLading bizo)
		{
			this.bizo = bizo;
		}

		public readonly BillOfLading bizo;

		public virtual void Process(INotifications notifications, CancellationToken token = default)
		{
			var containers = bizo?.RealContainers;
			var validateContainers = new List<BillOfLadingContainer>();

			if (containers == null || !containers.Any())
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, FormattableString.Invariant($"messaging cannot be send because no containers.")); // workflow processor message
				return;
			}

			if (!ValidateSet(notifications))
			{
				return;
			}

			foreach (BillOfLadingContainer container in containers)
			{
				if (ValidateBillOfLadingContainer(notifications, container))
				{
					validateContainers.Add(container);
				}
			}

			SendMessage(notifications, validateContainers);
		}

		public abstract bool ValidateSet(INotifications notifications);

		public virtual bool ValidateBillOfLadingContainer(INotifications notifications, BillOfLadingContainer container)
		{
			if (container.JC_ImportReleaseOrderStatus == ReleaseImportOrderMessageStatusList.Codes.OriginalSent || container.JC_ImportReleaseOrderStatus == ReleaseImportOrderMessageStatusList.Codes.WithdrawSent)
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} is still waiting on a response.", container.JC_ContainerNum));  // workflow processor message
			}
			else
			{
				container.MarkAsNeedingValidation();
				container.RunPreSaveValidation();

				if (container.HasErrors || container.HasMessageErrors)
				{
					notifications.Add(CargoWise.ComponentModel.NotificationType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} has errors and/or message errors.\r\nYou will need to correct these before an Import Release Order message can be sent for it.", container.JC_ContainerNum));  // workflow processor message
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		public abstract void SendMessage(INotifications notifications, List<BillOfLadingContainer> congtainers);
	}
}
