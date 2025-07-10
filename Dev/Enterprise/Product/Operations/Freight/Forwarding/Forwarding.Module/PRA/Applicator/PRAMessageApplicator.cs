using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Events = Enterprise.ZArchitecture.Business.Events;
using IContainerMessagingData = Enterprise.Integration.Customs.AU.IContainerMessagingData;
using IFreightDataLayer = Enterprise.Integration.Customs.AU.IFreightDataLayer;
using IMessageBuilder = Enterprise.Integration.Customs.AU.IMessageBuilder;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Freight.Forwarding.Module
{
	public sealed class PRAMessageApplicator : OperationalActionMethodApplicator
	{
		public PRAMessageApplicator(MessageType messageType, PRASettings settings)
			: base(Res.GetString("FD9E3365-8F17-4C35-945E-AE698AA8236C", "Send PRA Message"), settings.Factory)
		{
			this.messageType = Argument.NotNull(messageType, "messageType");
			this.praSettings = Argument.NotNull(settings, "settings");
		}
		readonly MessageType messageType;
		readonly PRASettings praSettings;

		#region Override

		protected override void ApplyCore(IOperationalActionSectionLog logValue, BusinessObject[] targets)
		{
			this.log = logValue;
			var validContainers = new List<ForwardingContainer>();

			foreach (BusinessObject bizObj in targets)
			{
				var consol = (ForwardingConsol)bizObj;

				if (consol != null && !consol.Containers.Any())
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
						Res.GetString("234552c2-98d8-499e-9a18-8531b772916f", "{0:G} : No containers selected."),
						Hyperlink(consol));
				}
				else
				{
					foreach (ForwardingContainer container in consol.Containers)
					{
						validContainers.Add(container);
					}
				}
			}

			var containerMessageDatas = GetDatas(validContainers);
			if (containerMessageDatas.Any())
			{
				CreatePRAMessage(containerMessageDatas);
			}
		}
		IOperationalActionSectionLog log;

		#endregion

		#region CreatePRAMessage

		void CreatePRAMessage(IEnumerable<IContainerMessagingData> containerMessagingDatas)
		{
			log.SetSectionProgressMax(containerMessagingDatas.Count());

			foreach (IContainerMessagingData dataLayer in containerMessagingDatas)
			{
				log.BumpSectionProgress();

				CommonContainer container = (CommonContainer)dataLayer.GetCommonContainer;

				if (container != null)
				{
					if ((messageType == MessageType.Submit || messageType == MessageType.Cancel) && container.IsWaitingForPRAResponse)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
								Res.GetString("11018f27-b549-409b-a303-70548cca58d6", "{0:G} - Container is waiting for response. Ignored."),
								Hyperlink(container));
					}
					else if (messageType == MessageType.ReSubmit || !container.IsWaitingForPRAResponse)
					{
						var containerLastPRAMessage = container.GetLastPRAMessageSent();
						if (containerLastPRAMessage == null)
						{
							PRALicenseConsumptionCount += 1;
						}

						var praMessageBuilder = GetNewMessageBuilder(dataLayer, messageType);
						praMessageBuilder.PostMessage();

						CreatePRAEventLogs(container);

						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
							Res.GetString("c45885ff-0562-470f-80cf-d667c2a2d4f3", "{0:G} - PRA Message generated."),
							Hyperlink(container));
					}
				}
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
		}

		int PRALicenseConsumptionCount { get; set; }

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				CreateLicenseConsumptionLog();
			}
		}

		void CreatePRAEventLogs(CommonContainer container)
		{
			if (messageType == MessageType.Submit || messageType == MessageType.ReSubmit)
			{
				container.Logs.AddNew(Events.MessageSent, PRAMessageEvent.GetEventParameters(Events.MessageSent));
			}
			else if (messageType == MessageType.Cancel)
			{
				container.Logs.AddNew(Events.MessageWithdrawCancelRequest, PRAMessageEvent.GetEventParameters(Events.MessageWithdrawCancelRequest));
			}
		}

		void CreateLicenseConsumptionLog()
		{
			for (int i = 0; i < PRALicenseConsumptionCount; i++)
			{
				var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
				logger.CreateLog(Env.Licence.PRAMessagingPerTransaction, true);
			}
		}

		bool UserConfirmWarningsAreNotAProblem()
		{
			string warningHeader = Res.GetString("3dc33a39-99b4-4669-8b2d-5890253e4b44", "Warnings were found when checking the data used to Send a PRA.");
			string warningFooter = Res.GetString("2092d6ce-3eb3-4c11-aee1-db06204be09c", @"You can still send this message, but this message MAY be rejected by some
of the Terminals or cause problems getting the container accepted by the Wharf.

The recommended course of action is to amend your data accordingly.
Are you sure you want to send this PRA now?");

			return (Globals.Message.Show(warningHeader + "\r\n" + warningFooter,
				Res.GetString("FD9E3365-8F17-4C35-945E-AE698AA8236C", "Send PRA Message"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes);
		}

		LogHyperlink Hyperlink(ForwardingConsol consol)
		{
			if (consol != null)
			{
				return new LogControllerLink(ZString.Format("{0}", consol.JK_UniqueConsignRef), ControllerIDs.JobConsol, consol.PK);
			}

			return null;
		}

		LogHyperlink Hyperlink(CommonContainer container)
		{
			LogControllerLink controllerLink;

			if (container.Consol != null)
			{
				controllerLink = new LogControllerLink(ZString.Format("{0} : {1}", container.Consol.JK_UniqueConsignRef, container.ContainerCode), ControllerIDs.JobConsol, container.Consol.PK);
			}
			else
			{
				controllerLink = new LogControllerLink(container.ContainerCode, ControllerIDs.Containers, container.PK);
			}

			return controllerLink;
		}

		bool ShouldSkipShipment(ForwardingContainer container)
		{
			var result = false;

			if (praSettings.ErrorBehaviour == PRASettings.Codes.Skip)
			{
				var invalidContainerInSameShipment = InvalidContainer.FirstOrDefault(x => x.JC_JK == container.Consol.PK);
				result = (invalidContainerInSameShipment != null);
			}

			return result;
		}
		List<ForwardingContainer> InvalidContainer { get; set; }

		#endregion

		#region Validation

		IEnumerable<IContainerMessagingData> GetDatas(IEnumerable<ForwardingContainer> containers)
		{
			bool hasWarningText = false;
			bool hasErrorText = false;
			var validContainers = new List<IContainerMessagingData>();
			InvalidContainer = new List<ForwardingContainer>();

			foreach (var container in containers)
			{
				var dataLayer = GetNewFreightDataLayer(container);
				if (dataLayer != null)
				{
					var errorText = dataLayer.GetErrorText();

					if (errorText.IsEmpty)
					{
						if (ShouldSkipShipment(container))
						{
							errorText = (NoResString)"Skip shipment that have errors or message errors."; // Operation Action Log Message
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
							Res.GetString("b72aea31-5fc6-4fed-a83d-4e19d107e1d1", "{0:G}\r\n{1:G}"),
							Hyperlink(container), errorText);
						}
						else
						{
							validContainers.Add(dataLayer);
						}
					}
					else if (!errorText.IsEmpty)
					{
						InvalidContainer.Add(container);

						log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
							Res.GetString("b72aea31-5fc6-4fed-a83d-4e19d107e1d1", "{0:G}\r\n{1:G}"),
							Hyperlink(container), errorText);
						hasErrorText = true;
					}

					if (!hasErrorText)
					{
						var warningText = dataLayer.GetWarningText();
						if (!warningText.IsEmpty)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
								Res.GetString("53df877b-cf8b-4d95-afc0-292030fa1717", "{0:G}\r\n{1:G}"),
								Hyperlink(container), warningText);
							hasWarningText = true;
						}
					}
				}
			}

			return ((!hasErrorText || praSettings.ErrorBehaviour == PRASettings.Codes.Skip) && (!hasWarningText || UserConfirmWarningsAreNotAProblem()))
				? validContainers
				: Enumerable.Empty<IContainerMessagingData>();
		}

		#endregion

		#region Interface

		IMessageBuilder GetNewMessageBuilder(IContainerMessagingData messageData, MessageType messageTypeValue)
		{
			return ObjectFactory.New<IMessageBuilder>(messageData, messageTypeValue);
		}

		IContainerMessagingData GetNewFreightDataLayer(CommonContainer container)
		{
			return ObjectFactory.New<IFreightDataLayer>(container);
		}

		#endregion

		#region properties

		internal OperationalActionLogErrorLevel ValidationErrorLevel
		{
			get
			{
				return (praSettings.ErrorBehaviour == PRASettings.Codes.Skip)
					? OperationalActionLogErrorLevel.Warning
					: OperationalActionLogErrorLevel.Error;
			}
		}

		#endregion
	}
}
