using System;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class PortAuthorityMessageSenderBOTest : BaseInterchangeSenderTest
	{
		#region Implementation
		protected override EDIInterchange GetNewEDIInterchangeReadyToSend()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(GetNewEDIMessageReadyToSend());
			return new PortAuthorityInterchangeProvider(messages).Interchanges[0];
		}

		protected override EDIMessage GetNewEDIMessageReadyToSend()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			EDIMessage message = voyage.Origins[0].Messages.AddNew(typeof(PortAuthorityMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.PortAuthority;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return message;
		}

		protected override BaseInterchangeSender GetNewSender()
		{
			return new PortAuthorityInterchangeSender();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PortAuthorityPortCollection ports = new PortAuthorityPortCollection();
			PortAuthorityPort portA = ports.AddNew();
			portA.Port = port1;
			portA.ProductionEmail = "bob@freadnet.org";
			portA.ProductionID = "recipient1";
			PortAuthorityPort portB = ports.AddNew();
			portB.Port = port2;
			portB.ProductionEmail = "bob@freadnet.org";
			portB.ProductionID = "recipient2";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ports);
			PortAuthoritySettings settings = new PortAuthoritySettings();
			SetupPort(settings, port1, "sender1");
			SetupPort(settings, port2, "sender2");
			AgencyRegistry.Instance.PortAuthoritySettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		void SetupPort(PortAuthoritySettings settings, ZString port, ZString senderid)
		{
			foreach (PortAuthoritySetting setting in settings.Settings)
			{
				if (setting.Port == port)
				{
					setting.Status = PortAuthoritySettingStatus.Codes.Production;
					setting.SenderID = senderid;
				}
			}
		}

		const string port1 = "AUBNE";
		const string port2 = "AUMEL";
		#endregion
	}
}
