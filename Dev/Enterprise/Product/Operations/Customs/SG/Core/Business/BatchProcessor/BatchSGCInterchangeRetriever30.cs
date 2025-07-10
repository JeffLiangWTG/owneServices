using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public abstract class BatchSGCInterchangeRetriever30 : BatchProcess
	{
		protected BatchSGCInterchangeRetriever30()
		{
			helper = new BatchSG4InterchangeHelper(Logger);
		}
		protected BatchSGInterchangeHelper helper;

		protected override bool IsEnvironmentDataValid()
		{
			return base.IsEnvironmentDataValid() && helper.IsEnvironmentDataValid();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void Execute(CancellationToken token)
		{
			Logger.Log("Retrieving...");
			try
			{
				BatchSGBrokerChecker staffChecker = new BatchSGBrokerChecker(Logger, helper);
				staffChecker.CheckStaff();
			}
			catch (Exception e) when (!e.IsCriticalException())//if fails, not a critical problem, inform us and Batch Processor continues working
			{
				ErrorReporter.ReportOnce(e.Message);
				helper.VerboseLog(Logger, "Check Staff failed (not critical) = " + e.Message);
			}

			Logger.Log("Testing Connection...");

			if (TestConnection())
			{
				helper.VerboseLog(Logger, "Check Accounts");
				foreach (GlbStaff broker in helper.GetValidBrokerMailboxes())
				{
					token.ThrowIfCancellationRequested();
					var login = CheckBrokerMailbox(broker);
					if (login.LoginState == LoginCommand.LoginStateType.LoggedIn)
					{
						Logout(login);
					}
				}

				helper.VerboseLog(Logger, "Retrieve for valid mailboxes");
				foreach (GlbStaff broker in helper.GetValidBrokerMailboxes())
				{
					token.ThrowIfCancellationRequested();
					LoginAndRetrieveAndLogout(broker, token);
				}
			}
			else
			{
				Logger.Log("Test Connection Failed");
			}

			Logger.Log("Retrieving Complete");
			Logger.AddBlankLine();
		}

		protected abstract void LoginAndRetrieveAndLogout(GlbStaff broker, CancellationToken token);

		protected virtual bool TestConnection()
		{
			return helper.UseTestConnection;
		}

		protected virtual LoginCommand CheckBrokerMailbox(GlbStaff broker)
		{
			return helper.MailboxChecker.Execute(broker);
		}

		protected virtual void Logout(LoginCommand loginCommand)
		{
			new LogoutCommand(loginCommand, new MHUBSettingsProvider(), Logger, helper.ShowVerboseLogging).Execute();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static bool TryAddInterchange(string localFile, LoggingInformation logger)
		{
			var success = true;
			string interchangeData;

			EDIInterchange interchange = null;
			try
			{
				using (TextReader reader = File.OpenText(localFile))
				{
					interchangeData = reader.ReadToEnd();
				}

				interchange = CreateAndSaveInterchange(logger, ref success, interchangeData);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (interchange != null)
				{
					interchange.ContainedMessages.RemoveAndDeleteAll();
					if (!interchange.IsDeleted)
					{
						interchange.Delete();
					}
				}

				ErrorReporter.ReportOnce(e.Message);
				success = false;
			}

			return success;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected static EDIInterchange CreateAndSaveInterchange(LoggingInformation logger, ref bool success, ZString interchangeData)
		{
			EDIInterchange interchange;
			var factory = new BusinessObjectFactory();
			try
			{
				interchange = SGEDIInterchange.CreateNewInterchangeFromXmlOrEdifactString(factory, interchangeData, EDIInterchange.ApplicationCodes.SingaporeTradenet4);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				logger.LogWarning("Could not create interchange from interchange string. Error: " + e.Message);
				success = false;
				return null;
			}

			if (interchange.EI_HeaderText.Contains("CLASET", StringComparison.OrdinalIgnoreCase))
			{
				interchange.EI_InterchangeNum = ZDateTime.UtcNow.ToString(DateTimeFormatWithMilliSeconds, CultureInfo.InvariantCulture);
			}
			else
			{
				interchange.EI_InterchangeNum = interchange.EI_InterchangeNum + "-" + ZDateTime.UtcNow.ToString(DateTimeFormatWithMilliSeconds, CultureInfo.InvariantCulture);
			}

			var existingInterchange = interchange.ExistingInterchangeMatchingToFromAndInterchangeNum;
			if (existingInterchange != null)
			{
				logger.Log("Duplicate of interchange number " + interchange.EI_InterchangeNum + " received");
				interchange.ContainedMessages.RemoveAndDeleteAll();
				interchange.Delete();
				success = false;
			}

			factory.Save();
			return interchange;
		}

		const string DateTimeFormatWithMilliSeconds = "yyyyMMddHHmmssfff";
	}
}
