using System;
using System.Diagnostics;

namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule")]
	public class Notification
	{
		public enum Events : int
		{
			None = 0,
			ServiceStart = 1,
			ServiceStop = 2,
			ServicePause = 3,
			ServiceResume = 4,
			ServiceShutdown = 5,
			ServiceWorkingGetInfo = 6,
			MessageReception = 10,
			FileReception = 11,
			MessageSent = 21,
			FileSent = 22,
			NotificationSent = 23,
			FTPFileRecovery = 31,
			InterfaceMessageRecovery = 32,
			FileCopied = 41,
			FileMoved = 42,
			ProcessInProgress = 51,
			ProcessOver = 52,
			ReceptionError = 61,
			SendingError = 62,
			PathError = 63,
			PingError = 64,
			RecipientIdError = 65,
			MessageValidationError = 66,
			GlobalConfigurationError = 67,
			EHubConfigurationError = 68,
			FtpParamsError = 69,
			UnkownError = 99,
		}

		public string Message { get; private set; }

		public string ConsoleMessage { get; private set; }

		public MessageType NotificationType { get; private set; }

		public Events TransportEventType { get; private set; }

		public Notification(MessageType type, Events myEvent)
		{
			var st = new StackTrace();
			InitNotification(string.Empty, type, myEvent, st.GetFrame(2).GetMethod().Name);
		}

		public Notification(string message, MessageType type, Events myEvent)
		{
			var st = new StackTrace();
			InitNotification(message, type, myEvent, st.GetFrame(2).GetMethod().Name);
		}

		void InitNotification(string message, MessageType type, Events myEvent, string callingMethod)
		{
			var messageEvent = TransportEventMessage(myEvent);

			//Let's add a tab where necessary

			if (type.ToString() == "Error")
			{
				Message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + type + "\t" + "\t" + "\t" + callingMethod + "\t" + "\t" + "\t" + messageEvent + "\t" + "\t" + "\t" + message;
			}
			else
			{
				Message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "\t" + type + "\t" + "\t" + callingMethod + "\t" + "\t" + "\t" + messageEvent + "\t" + "\t" + "\t" + message;
			}

			ConsoleMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + message;

			NotificationType = type;

			TransportEventType = myEvent;
		}

		public void Add(bool consoleOut, Logger logger)
		{
			if (logger != null && Message != null)
			{
				if (consoleOut)
				{
					Console.WriteLine(ConsoleMessage); // SuppressCodeSmell Reason = put there by design
				}
				logger.WriteToFile(Message);
			}
		}

		public override string ToString()
		{
			return Message;
		}

		public enum MessageType
		{
			Information,
			Warning,
			Error,
			Confirmation,
		}

		static string TransportEventMessage(Events evt)
		{
			var message = string.Empty;

			switch (evt)
			{
				case Events.ServiceStart:
					message = "Service start" + "\t" + "\t";
					break;

				case Events.ServiceStop:
					message = "Service stopped" + "\t" + "\t";
					break;

				case Events.ServicePause:
					message = "Service paused" + "\t" + "\t";
					break;

				case Events.ServiceResume:
					message = "Service resumed" + "\t" + "\t";
					break;

				case Events.ServiceShutdown:
					message = "Service shutdown" + "\t";
					break;

				case Events.FileReception:
					message = "File reception" + "\t" + "\t";
					break;

				case Events.MessageReception:
					message = "Message reception" + "\t";
					break;

				case Events.PathError:
					message = "Invalid path detected";
					break;

				case Events.RecipientIdError:
					message = "Recipient Id invalid";
					break;

				case Events.FileSent:
					message = "File sent" + "\t" + "\t" + "\t";
					break;

				case Events.FileCopied:
					message = "File copied" + "\t" + "\t" + "\t";
					break;

				case Events.FileMoved:
					message = "File moved" + "\t" + "\t" + "\t";
					break;

				case Events.FtpParamsError:
					message = "invalid FTP configuration.";
					break;

				case Events.GlobalConfigurationError:
					message = "Invalid configuration";
					break;

				case Events.ServiceWorkingGetInfo:
					message = "Service is working" + "\t";
					break;

				case Events.ProcessInProgress:
					message = "Process in progress" + "\t";
					break;

				case Events.ProcessOver:
					message = "Process over" + "\t" + "\t";
					break;

				case Events.FTPFileRecovery:
					message = "FTP file recovery" + "\t";
					break;

				case Events.InterfaceMessageRecovery:
					message = "Session file recovery";
					break;

				case Events.MessageValidationError:
					message = "Message is invalid" + "\t";
					break;

				case Events.NotificationSent:
					message = "Notification sent" + "\t";
					break;

				case Events.MessageSent:
					message = "Message sent" + "\t" + "\t";
					break;

				case Events.EHubConfigurationError:
					message = "Invalid eHub Configuration";
					break;

				case Events.UnkownError:
					message = "Error" + "\t" + "\t";
					break;
			}
			return message;
		}
	}
}
