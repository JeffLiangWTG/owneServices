using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Freight.Agency.Business
{
	sealed public class EIDOMessageProcessor : BaseBillOfLadingMessageProcessor
	{
		public EIDOMessageProcessor(BillOfLading bizo) : base(bizo)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "workflow processor message")]
		public override bool ValidateSet(INotifications notifications)
		{
			EIDOBusinessObjectValidation.RegisterForFactory(bizo.Factory);
			var message = MessageSender.GetEIDOErrorMessage();

			if (!string.IsNullOrEmpty(message))
			{
				notifications.Add(NotificationType.Error, message);
			}
			else if (!bizo.IsBillOfLadingStage)
			{
				notifications.Add(NotificationType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} is a booking and not eligible for E-IDO messaging.", bizo.JS_UniqueConsignRef));
			}
			else if (MessageSender.HasErrorsOrMessageErrors(bizo))
			{
				notifications.Add(NotificationType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} has errors and/or message errors.\r\nYou will need to correct these before an E-IDO message can be sent for it.", bizo.JS_UniqueConsignRef));
			}
			else if (bizo.Principal == null)
			{
				notifications.Add(NotificationType.Error, string.Format(CultureInfo.InvariantCulture, "{0}  does not have a principal set.", bizo.JS_UniqueConsignRef));
			}
			else if (Detail.GetIdentity(bizo.JS_OH_DeliveryAgent) == null)
			{
				notifications.Add(NotificationType.Error, string.Format(CultureInfo.InvariantCulture, "E-IDO messaging has not been enabled for the principal {0}.\r\nYou can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option.", bizo.Principal.OH_Code));
			}
			else
			{
				if (!ImportExportHelper.IsBranchCountry(bizo.JS_RL_NKDestination))
				{
					notifications.Add(NotificationType.Warning, string.Format(CultureInfo.InvariantCulture, "{0} is released in another country/region and is not eligible for E-IDO messaging, skipping.", bizo.JS_UniqueConsignRef));
				}
				if (bizo.JS_PackingMode != Core.Constants.ContainerModes.FCL)
				{
					notifications.Add(NotificationType.Warning, string.Format(CultureInfo.InvariantCulture, "{0} is not a FCL shipment, skipping.", bizo.JS_UniqueConsignRef));
				}
				return true;
			}

			return false;
		}

		public override void SendMessage(INotifications notifications, List<BillOfLadingContainer> containers)
		{
			if (containers.Any())
			{
				var generator = new ContainerPinGenerator();
				generator.PopulateEmptyPins(containers);

				foreach (var container in containers)
				{
					var data = EIDOShipmentMessagingData.NewOriginal(container);
					var builder = EIDOMessageBuilderFactory.GetNewBuilder();
					var message = EIDOMessage.New(container, data.MessageFunction, builder.GenerateMessageText(data));
					message.SetEventToAddOnSaving(container.GetType(), Events.MessageSent, EIDOBaseApplicator.GetParamtersForEvent(Events.MessageSent));
				}
			}
		}

		EIDOMessagingHeader Detail
		{
			get { return detail ?? (detail = AgencyRegistry.Instance.EIDOMessagingDetails.Value); }
		}
		EIDOMessagingHeader detail;

		EIDOSendOriginalApplicator MessageSender => messageSender ?? (messageSender = new EIDOSendOriginalApplicator(new ReleaseImportOrderSettings()));
		EIDOSendOriginalApplicator messageSender;
	}
}
