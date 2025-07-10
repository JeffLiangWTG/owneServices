using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2
{
	class TraxonCargoIMPPhase2InterchangeSender : CargoIMPPhase2InterchangeSender
	{
		const int PODDelayTimeInMunutes = 6;

		public TraxonCargoIMPPhase2InterchangeSender(ILogger logger)
			: base(logger)
		{
			this.errorsHandler = new TraxonCargoIMPPhase2CommErrorHandler(logger);
		}

		readonly TraxonCargoIMPPhase2CommErrorHandler errorsHandler;

		#region Send

		protected override bool SendInterchange(EDIInterchange interchange)
		{
			bool interchangeSucceeded = true;
			if (IsEnvironmentDataValid())
			{
				if (this.errorsHandler.CanSend())
				{
					try
					{
						SendInterchangeCore(interchange);
						this.errorsHandler.ExchangeSuccessful();
					}
					catch (FtpException ex)
					{
						interchangeSucceeded = false;
						this.errorsHandler.ErrorOccured(ex, interchange);
					}

					LogMessageOutcome(interchange, interchangeSucceeded);
				}
			}
			else
			{
				Logger.Log(LogType.Error, "Traxon Server, Username, Password are not specified in registry.");
				TraxonCargoIMPPhase2CommErrorHandler.SendFailureNotify(Res.GetString("63162e5e-9566-4418-8ef3-c90ac40aea4a", "Traxon communications have failed 5 consecutive times with the following error details. Transmission has been suspended for 1 hour.\r\n\r\nTraxon Server, Username, Password not specified in registry."), interchange);
				interchangeSucceeded = false;
			}

			return interchangeSucceeded;
		}

		void SendInterchangeCore(EDIInterchange interchange)
		{
			interchange.LogInterchangeInProgressForAllMessages();
			Logger.Log(LogType.Information, "Interchange #" + interchange.EI_InterchangeNum + " being sent.");

			FtpProcessor processor = GetFtpProcessor();
			string remoteFilePath = GetRemoteFilePath(interchange);
			using (TempFile tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, interchange.EI_InterchangeText);
				processor.UploadFile(tempFile.Filename, remoteFilePath);
				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					message.EM_Status = EDIInterchange.Status.Sent;
				}
			}
		}

		FtpProcessor GetFtpProcessor()
		{
			return new FtpProcessor(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.Value,
					ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName.Value,
					ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted.Value,
					new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0),
					new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
		}

		string GetRemoteFilePath(EDIInterchange interchange)
		{
			var remoteDirectoryName = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPInboundDirectory.Value;

			var messageNum = interchange.ContainedMessages[0].EM_MessageNum;
			var remoteFileName = messageNum.PadLeft(8, '0') + ".msg";

			return string.IsNullOrWhiteSpace(remoteDirectoryName)
				? remoteFileName
				: string.Join(@"/", remoteDirectoryName, remoteFileName);
		}

		#endregion

		#region Implementation

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Prevents website data transfer during testing allowing tests to pass locally")]
		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			RemoveRecentPODMessages(messages);
			messages.Sort<EDIMessage>(PODMessageLastComparer);
			TraxonCargoIMPPhase2InterchangeProvider provider = new TraxonCargoIMPPhase2InterchangeProvider(messages);
			EDIInterchange[] interchanges = provider.Interchanges;
		}

		protected override bool IsEnvironmentDataValid()
		{
			return base.IsEnvironmentDataValid() &&
				!string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.Value) &&
				!string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName.Value) &&
				!string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted.Value);
		}

		static void RemoveRecentPODMessages(NonDependentEDIMessageCollection messages)
		{
			for (int i = messages.Count - 1; i >= 0; i--)
			{
				EDIMessage message = messages[i];
				if (IsMessagePOD(message))
				{
					if ((ZDateTime.UtcNow - message.EM_SystemCreateTimeUtc).TotalMinutes < PODDelayTimeInMunutes)
					{
						messages.Remove(message);
					}
				}
			}
		}

		static int PODMessageLastComparer(EDIMessage x, EDIMessage y)
		{
			if (x == null)
			{
				return (y == null) ? 0 : -1;
			}
			else
			{
				if (y == null)
				{
					return 1;
				}
				else
				{
					return IsMessagePOD(x).CompareTo(IsMessagePOD(y));
				}
			}
		}

		static bool IsMessagePOD(EDIMessage message)
		{
			return message.EM_MessageText.Contains("\r\nSTS,POD,", StringComparison.Ordinal); // Message text
		}

		#endregion
	}
}
