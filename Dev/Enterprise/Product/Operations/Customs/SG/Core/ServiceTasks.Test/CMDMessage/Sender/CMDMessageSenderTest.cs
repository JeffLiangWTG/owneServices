using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.Testing;

class CMDMessageSenderTest : TestCaseWithFactory
{
	class CMDMessageSenderForTest : CMDMessageSender
	{
		public CMDMessageSenderForTest(ILogger logger)
			: base(logger)
		{
		}

		public new string ApplicationCode
		{
			get { return base.ApplicationCode; }
		}

		public new string AttachmentFileName
		{
			get { return base.AttachmentFileName; }
		}

		public new bool IsGoingViaCCN => base.IsGoingViaCCN;
	}

	public void TestApplicationCode()
	{
		var sender = new CMDMessageSenderForTest(new TestServiceLogger());
		AssertEquals(EDIInterchange.ApplicationCodes.SingaporeCMD, sender.ApplicationCode);
	}

	public void TestAttachmentFileName()
	{
		var sender = new CMDMessageSenderForTest(new TestServiceLogger());
		AssertEquals("CMD Message.txt", sender.AttachmentFileName);
	}

	public void TestIsGoingViaCCN()
	{
		var sender = new CMDMessageSenderForTest(new TestServiceLogger());
		using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes))
		{
			Assert(sender.IsGoingViaCCN);
		}

		using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.CCN))
		{
			Assert(sender.IsGoingViaCCN);
		}
	}

	public void TestSend_Descartes()
	{
		using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes))
		{
			SendAndAssert("");
		}
	}

	void SendAndAssert(string serviceProvider)
	{
		var interchange = CreateInterchange();
		interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeCMD;
		var message = CreateMessage(interchange);
		Factory.Save();

		new CMDMessageSender(new TestServiceLogger()).Process();
		interchange.Reload();
		message.Reload();

		AssertEquals(EDIInterchange.Status.Sent, interchange.EI_Status);
		AssertEquals(EDIMessage.Status.Sent, message.EM_Status);

		var emailDef = Environment.Env.OutgoingMailManager.EmailsCreated[0];
		string subject = "EAGLE FWB EDI - TEST - FROM";
		if (serviceProvider.Length > 0)
		{
			subject += " - " + serviceProvider;
		}

		AssertEquals(subject, emailDef.Subject);
		AssertEquals(1, Environment.Env.OutgoingMailManager.EmailsCreated.Count);
	}

	CIMEDIInterchange CreateInterchange()
	{
		CIMEDIInterchange result = Factory.New<CIMEDIInterchange>();
		result.EI_HeaderText = "Header";
		result.EI_BodyText = "Body";
		result.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		result.EI_From = "FROM";
		result.EI_Status = EDIInterchange.Status.Queued;
		result.EI_To = "TO";
		return result;
	}

	CIMEDIMessage CreateMessage(CIMEDIInterchange interchange)
	{
		CIMEDIMessage result = Factory.New<CIMEDIMessage>();
		result.EM_MessageText = "MessageText";
		result.EM_MessageType = "FWB";
		result.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		result.EM_ApplicationReference = "AppRef";
		result.EM_EI = interchange.PK;
		return result;
	}
}
