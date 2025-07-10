using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Recruiter.ServiceTasks.HREmailsServiceTask.Code,
	"Human Resources Email Service Task",
	"HRM",
	typeof(Enterprise.Recruiter.ServiceTasks.HREmailsServiceTask),
	MinimumPeriod = "30seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true
	)]

namespace Enterprise.Recruiter.ServiceTasks
{
	public class HREmailsServiceTask : ServiceProviderImpl
	{
		public const string Code = "HRE";

		[HostedServiceRequirement]
		public static string CheckDaxtraEnabled()
		{
			if (RecruiterDataRegistry.Instance.DaxtraEnable.Value)
			{
				return string.Empty;
			}

			return (NoResString)"This service requires Daxtra to be enabled"; // information for logging only.
		}

		[HostedServiceRequirement]
		public static string CheckMailServerConfigured()
		{
			if (string.IsNullOrWhiteSpace(RecruiterDataRegistry.Instance.HREmailMailServer.Value.ToString()))
			{
				return (NoResString)"This service requires mail server to be configured"; // information for logging only.
			}

			return string.Empty;
		}

		[HostedServiceRequirement]
		public static string CheckMailAccountUsernameConfigured()
		{
			if (string.IsNullOrWhiteSpace(RecruiterDataRegistry.Instance.HREmailMailboxUserName.Value.ToString()))
			{
				return (NoResString)"This service requires mail account username to be configured"; // information for logging only.
			}

			return string.Empty;
		}

		HREmailsMailboxSettings MailboxSettings => mailboxSettings ?? (mailboxSettings = new HREmailsMailboxSettings());
		HREmailsMailboxSettings mailboxSettings;

		public override void RunTask(CancellationToken token)
		{
			var processedList = new List<long>();

			using (Env.Instance.SuspendBranchAccessError())
			{
				try
				{
					var isValidMailboxSettings = ValidateMailboxSettings();
					if (isValidMailboxSettings)
					{
						using (var emailReader = GetNewEmailReader(MailboxSettings))
						{
							Log(LogType.Debug, (NoResString)"Attempting to connect to Server:{0} | Mailbox:{1}", emailReader.MailHost, emailReader.MailUserName); // Log message
							var messageCount = emailReader.GetCount();
							Log(LogType.Debug, (NoResString)"{0} emails found in the mailbox", messageCount); // Log message

							ReadAndProcessEmail(emailReader, messageCount, processedList, token);

							emailReader.DeleteProcessedMail(processedList);
							if (processedList.Count > 0)
							{
								Log(LogType.Debug, (NoResString)"{0} emails read, processed and deleted", processedList.Count); // Log message
							}
							processedList.Clear();
						}
					}
				}
				catch (Exception ex) when (ThrownFromMailkit(ex))
				{
					var socketEx = ex.GetBaseException() as SocketException;
					const int WSAETIMEDOUT = 10060; // connection timed out - see Windows Sockets Error Codes
					const int WSAECONNABORTED = 10053; // connection lost
					if (socketEx != null && socketEx.ErrorCode == WSAETIMEDOUT)
					{
						// Treat timeouts as just a warning. We don't want the service to decide
						// it is faulty just because the mail server is not responding.
						Log(LogType.Warning, (NoResString)"Connection timed out"); // Log message
					}
					else if (socketEx != null && socketEx.ErrorCode == WSAECONNABORTED)
					{
						// Mail server is either blocking the defined SMTP port, or is not configured properly
						// Or a firewall is stopping the connection.
						Log(LogType.Warning, (NoResString)"Connection lost"); // Log message
					}
					else
					{
						Log(LogType.Error, ex.Message, ex);
					}
				}
				catch (MailInterfaceException ex)
				{
					Log(LogType.Error, ex.Message, ex);
				}
				finally
				{
					if (processedList.Count > 0)
					{
						try
						{
							using (var emailReader = GetNewEmailReader(MailboxSettings))
							{
								emailReader.DeleteProcessedMail(processedList);
								Log(LogType.Information, (NoResString)"{0} emails read, processed and deleted", processedList.Count); // Log message
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
						}
					}
				}
			}
		}

		bool ValidateMailboxSettings()
		{
			var result = true;

			if (string.IsNullOrWhiteSpace(MailboxSettings.MailServer))
			{
				result = false;
			}

			if (string.IsNullOrWhiteSpace(MailboxSettings.UserName))
			{
				result = false;
			}

			return result;
		}

#if DEBUG
		public Func<string, string, string, HRServiceEmailReader> getEmailReaderForTest;
#endif

		HRServiceEmailReader GetNewEmailReader(HREmailsMailboxSettings settings)
		{
#if DEBUG
			if (getEmailReaderForTest != null)
			{
				return getEmailReaderForTest(settings.MailRetrievalProtocol, settings.MailServer, settings.UserName);
			}
#endif

			var protocol = settings.MailRetrievalProtocol;
			string securityType;
			if (protocol == MailRetrievalProtocols.POP3)
			{
				securityType = settings.POP3SecureConnectionType;
			}
			else
			{
				securityType = settings.IMAPSecureConnectionType;
			}
			return new HRServiceEmailReader(protocol, settings.MailServer, settings.ServerPort, settings.UserName, settings.Password, securityType);
		}

		void ReadAndProcessEmail(HRServiceEmailReader emailReader, long messageCount, List<long> processedList, CancellationToken token)
		{
			using var daxtraResumeParser = new DaxtraResumeParser();
			var factory = new BusinessObjectFactory();

			for (var index = 1; index <= messageCount; index++)
			{
				token.ThrowIfCancellationRequested();

				var email = ReadEmail(emailReader, index);
				if (email != null)
				{
					if (ProcessEmail(factory, email, daxtraResumeParser, index))
					{
						processedList.Add(index);
					}
				}
			}
		}

		bool ProcessEmail(BusinessObjectFactory factory, HRServiceEmail email, DaxtraResumeParser daxtraResumeParser, int index)
		{
			_ = Argument.NotNull(factory, nameof(factory));
			_ = Argument.NotNull(email, nameof(email));
			_ = Argument.NotNull(daxtraResumeParser, nameof(daxtraResumeParser));

			Log(LogType.Debug,
				(NoResString)"Email:{0} Attachments:{1} From:{2} Subject:{3}", // Log message
				index,
				email.NonVisualCount + email.VisualCount,
				email.SenderNameAddress,
				email.Subject);

			var emailCreated = CreateMailItem(factory, email);
			if (emailCreated != null)
			{
				CreateJobApplication(factory, email, emailCreated, daxtraResumeParser);

				Log(LogType.Debug, (NoResString)"Marking email no. {0} as processed", index); // Log message
				factory.Save();
				return true;
			}
			else
			{
				Log(LogType.Error, (NoResString)"Failed to process email. Index:{0}", index); // Log message
				return false;
			}
		}

		HRServiceEmail ReadEmail(HRServiceEmailReader emailReader, int index)
		{
			Log(LogType.Debug, (NoResString)"Reading email no. {0}", index); // Log message
			var emailText = string.Empty;
			HRServiceEmail email = null;

			try
			{
				emailText = emailReader.GetStringEmailFromPosition(index);
				if (!string.IsNullOrEmpty(emailText))
				{
					email = emailReader.GetRawEmailFromString(emailText);
				}
				else
				{
					Log(LogType.Error, (NoResString)"Failed to read email from server. Index:{0} EmailText:{1}", index, emailText); // Log message
				}
				return email;
			}
			catch (Exception ex) when (ThrownFromMailkit(ex))
			{
				throw;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var key = GetType().Name + ".ProcessEmail." + ex.GetType().Name;
				ErrorReporter.ReportOnce(key, ex.Message, ex);

				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Exception processing email from {0}: {1} EmailText='{2}'", // Log message
					email != null ? email.From : "NULL", // Log message
					ex.Message,
					emailText);

				Log(message, ex);
				return null;
			}
		}

		internal MailItem CreateMailItem(BusinessObjectFactory factory, HRServiceEmail serviceEmail)
		{
			var mailItem = factory.New<MailItem>();
			try
			{
				mailItem.RawMIMEString = serviceEmail.GetEml();
				mailItem.MI_Application = Code;
				mailItem.MI_Status = MailStatus.Unprocessed;
				mailItem.MI_Direction = MailDirection.Receive;
				mailItem.MI_Subject = ((ZString)serviceEmail.Subject).Left(AutoMailDBItems.Schema.MI_SubjectMaxLength);
				mailItem.MI_From = serviceEmail.SenderAddress;
				mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;

				if (!string.IsNullOrEmpty(serviceEmail.HtmlBody))
				{
					mailItem.MI_Body = serviceEmail.HtmlBody;
					mailItem.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
				}
				else
				{
					mailItem.MI_Body = serviceEmail.Body;
				}

				var attachments = serviceEmail.Mail.GetFullAttachments();
				foreach (var attachment in attachments)
				{
					var newAttachment = mailItem.MailAttachments.AddNew();
					newAttachment.MA_FileName = attachment.GetName();
					newAttachment.MA_Data = attachment.GetData();
				}

				Log(LogType.Debug, (NoResString)"HR email '{0}' created", serviceEmail.Subject); // Log message
				return mailItem;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Exception processing email from {0}: {1}", serviceEmail != null ? serviceEmail.From : "NULL", ex.Message); // Log message
				Log(message, ex);
				mailItem.Delete();
				return null;
			}
		}

		internal void CreateJobApplication(BusinessObjectFactory factory, HRServiceEmail serviceEmail, MailItem mailItem, DaxtraResumeParser daxtraResumeParser)
		{
			if (serviceEmail.Mail.Date == DateTimeOffset.MinValue)
			{
				Log(LogType.Error, (NoResString)"Unable to determine date from email. This means the input email is probably corrupt or unreadable."); // Log message
				return;
			}

			var emailStartDaxtraValue = RecruiterDataRegistry.Instance.HREmailStartDaxtra.Value;
			if (emailStartDaxtraValue == RecruiterDataRegistry.Instance.HREmailStartDaxtra.DefaultValue)
			{
				Log(LogType.Error, (NoResString)"Email processing start date did not have a value."); // Log message
				return;
			}

			if (emailStartDaxtraValue.Date.ToUniversalTime() > serviceEmail.Mail.Date.UtcDateTime)
			{
				Log(LogType.Error,
					(NoResString)"Email was too old to be processed. ProcessingStartDate:{0} EmailDate{1}", // Log message
					emailStartDaxtraValue.Date.ToUniversalTime(),
					serviceEmail.Mail.Date.UtcDateTime);
				return;
			}

			HRJobApplication jobApplication = null;
			var isNewApplication = false;
			var parser = CreateHRJobApplicationEmailParser(mailItem, daxtraResumeParser, factory);

			try
			{
				var populateFromFileResult = parser.PopulateFromMailItem(serviceEmail.Mail);

				var warningMessage = HRJobApplicationEmailParser.ProcessFileResultMessage(populateFromFileResult);
				if (!string.IsNullOrEmpty(warningMessage))
				{
					Log(LogType.Warning, warningMessage);
				}

				isNewApplication = parser.IsNewApplication;
				jobApplication = parser.Parent;

				if (jobApplication == null)
				{
					Log(LogType.Error, (NoResString)"Failed to process, job application was null, Daxtra failed to parse."); // Log message
					return;
				}

				jobApplication.RunPreSaveValidation();
				if (jobApplication.HasErrors || jobApplication.Applicant == null)
				{
					Log(LogType.Error,
						(NoResString)"Failed to process new application. HasErrors:{0} ApplicantIsNull:{1} IsNewApplication:{2}", // Log message
						jobApplication.HasErrors,
						(jobApplication.Applicant == null).ToString(), // Log message
						isNewApplication);

					if (isNewApplication)
					{
						jobApplication.Delete();
					}

					return;
				}

				mailItem.MI_Status = MailStatus.Processed;

				var resumeConverter = ObjectFactory.Get<IResumeConverter>(nameof(IResumeConverter), ServiceLogger);
				resumeConverter.ConvertAvailableResumes(jobApplication);

				if (isNewApplication)
				{
					jobApplication.HP_SubmissionTimeUtc = serviceEmail.Mail.Date.UtcDateTime;
					Log(LogType.Debug,
						(NoResString)"Job application for '{0}' created", // Log message
						jobApplication.Applicant.HA_FullName);
				}
				else
				{
					Log(LogType.Debug,
						(NoResString)"Email attached to '{0}' job application", // Log message
						jobApplication.Applicant.HA_FullName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (isNewApplication)
				{
					jobApplication?.Delete();
				}

				var message = string.Format(CultureInfo.InvariantCulture,
					(NoResString)"Exception creating job application from {0}: {1}", // Log message
					serviceEmail != null ? serviceEmail.From : "NULL", ex.Message); // Log message

				Log(message, ex);
			}
		}

		protected virtual HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(MailItem mailItem, DaxtraResumeParser daxtraResumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail = false)
		{
			return new HRJobApplicationEmailParser(mailItem, daxtraResumeParser, factory, createApplicantWithEmptyEmail, allowMatchJobOpening: false);
		}

		const string MailKitSource = "MailKit";

		static bool ThrownFromMailkit(Exception ex)
			=> ex.Source == MailKitSource;

		#region Log

		protected void Log(string message, Exception ex)
		{
			ServiceLogger?.Log(LogType.Error, message, ex);
		}

		protected void Log(LogType logType, string format, params object[] args)
		{
			ServiceLogger?.Log(logType, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		#endregion
	}
}
