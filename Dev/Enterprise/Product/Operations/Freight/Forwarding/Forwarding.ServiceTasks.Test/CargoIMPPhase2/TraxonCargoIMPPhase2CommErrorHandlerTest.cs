using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2.Test
{
	class TestMessageNumberStrategy : IMessageNumberStrategy
	{
		#region IMessageNumberStrategy Members

		public string GetMessageReferenceNumber()
		{
			return "1";
		}

		#endregion
	}

	public class TraxonCargoIMPPhase2CommErrorHandlerTest : TestCaseWithFactory
	{
		[TestDate(2008, 10, 10)]
		public void TestCanSend()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			TraxonCargoIMPPhase2CommErrorHandler handler = new TraxonCargoIMPPhase2CommErrorHandler(new TestServiceLogger());
			AssertEquals(true, handler.CanSend());
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(true, handler.CanSend());
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
			AssertEquals(false, handler.CanSend());
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime());
			AssertEquals(true, handler.CanSend());
		}

		public void TestErrorOccured()
		{
			GlbStaff currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "hello2@example.com";
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_To = "TO";
			interchange.EI_From = "FROM";
			EDIMessage message = interchange.ContainedMessages.AddNew();
			message.EM_MessageText = "MESSAGE1" + EDIMessage.MessageNumberPlaceHolder;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			message.EM_MessageType = "RMI";
			message.MessageNumberStrategy = new TestMessageNumberStrategy();
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);
			GlbGroup group = Factory.Load<GlbGroup>(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.Value);
			group.Staff[0].GS_EmailAddress = "hello@example.com";
			Factory.Save();
			TestServiceLogger logger = new TestServiceLogger();
			TraxonCargoIMPPhase2CommErrorHandler handler = new TraxonCargoIMPPhase2CommErrorHandler(logger);
			for (int i = 0; i < ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPMaxFailureCount.Value - 1; i++)
			{
				handler.ErrorOccured(new FtpException(FtpException.FtpExceptionType.Connect, "asdf"), interchange);
				AssertEquals(i + 1, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
				AssertContains("asdf", logger[i]);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			handler.ErrorOccured(new FtpException(FtpException.FtpExceptionType.Connect, "asdf"), interchange);
			AssertEquals(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPMaxFailureCount.Value, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			handler.ErrorOccured(new FtpException(FtpException.FtpExceptionType.Connect, "asdf"), interchange);
			AssertEquals(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPMaxFailureCount.Value + 1, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestExchangeSuccessful()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			TraxonCargoIMPPhase2CommErrorHandler handler = new TraxonCargoIMPPhase2CommErrorHandler(new TestServiceLogger());
			handler.ExchangeSuccessful();
			AssertEquals(0, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
		}
	}
}
