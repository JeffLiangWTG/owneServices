using System;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2
{
	class TraxonCargoIMPPhase2CommErrorHandler
	{
		public TraxonCargoIMPPhase2CommErrorHandler(ILogger logger)
		{
			Argument.NotNull(logger, "Logger");
			fLogger = logger;
		}
		readonly ILogger fLogger;

		public bool CanSend()
		{
			bool result = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value == 0;
			if (!result)
			{
				ZDateTime lastFTPFailureTime = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.Value;
				result = lastFTPFailureTime.IsEmpty || !lastFTPFailureTime.IsValid ||
					(ZDateTime.Now - lastFTPFailureTime).TotalMinutes > ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPDelayAfterFailure.Value;
			}

			return result;
		}

		public void ErrorOccured(FtpException ex, EDIInterchange interchange)
		{
			int fTPFailureCount = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value;
			fTPFailureCount++;
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fTPFailureCount);
			if (fTPFailureCount == ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPMaxFailureCount.Value)
			{
				fLogger.Log(LogType.Error, ex.FullMessage);
				ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
				SendFailureNotify(Res.GetString("874ecdde-6d2b-4f3a-9857-72457cf86a3a", "Traxon communications have failed {0} consecutive times with the following error details. Transmission has been suspended.\r\n\r\n{1}",
						fTPFailureCount, ex.FullMessage), interchange);
			}
			else
			{
				fLogger.Log(LogType.Warning, ex.FullMessage);
			}
		}

		public void ExchangeSuccessful()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}

		public static void SendFailureNotify(string information, EDIInterchange interchange)
		{
			if (interchange != null)
			{
				EmailDef notificationEmail = new EmailDef();
				notificationEmail.AddRecipientForUserCommunication(NotificationUtils.GetRecipients(interchange.Factory,
					GetRecipientFromInterchange(interchange),
					ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.Value,
					ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.Value).ToArray());

				if (notificationEmail.Recipients.Count > 0)
				{
					string body = information;
					notificationEmail.Subject = Res.GetString("5d9780bf-bfc5-42c9-b2ff-c62f519aedf5", "Traxon Failure Notification");
					notificationEmail.Body = body;
					Env.OutgoingMailManager.CreateAndSave(notificationEmail);
				}
			}
		}

		static string GetRecipientFromInterchange(EDIInterchange interchange)
		{
			GlbStaff staff = null;
			if (interchange.ContainedMessages.Count > 0)
			{
				var user = interchange.ContainedMessages[0]?.EM_SystemCreateUser ?? ZString.Empty;
				if (!user.IsEmpty)
				{
					staff = interchange.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, user);
				}
			}

			return staff != null ? staff.GS_EmailAddress : ZString.Empty;
		}
	}
}
