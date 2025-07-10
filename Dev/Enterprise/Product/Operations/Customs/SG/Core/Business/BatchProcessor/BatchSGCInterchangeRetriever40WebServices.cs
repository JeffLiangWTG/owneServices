using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.SG.MHUB.Mhx4Soap;
using Enterprise.Customs.SG.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGCInterchangeRetriever40WebServices : BatchSGCInterchangeRetriever30
	{
		protected override bool TestConnection()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void LoginAndRetrieveAndLogout(GlbStaff broker, CancellationToken token)
		{
			var brokerWrapper = SGGlbStaffWrapper.Get(broker);
			var userId = brokerWrapper?.Tradenetv4Password?.GP_UserID ?? ZString.Empty;
			Logger.Log("Retrieving via SOAP for " + userId);
			MHAccessClient igorsAwesomeClient = GetIgorsClient(brokerWrapper, userId);

			var saveInterchangesDelegate = new Action<IEnumerable<string>>(delegate(IEnumerable<string> allInterchanges)
			{
				foreach (string interchangeString in allInterchanges)
				{
					token.ThrowIfCancellationRequested();
					try
					{
						bool savedOneInterchangeToDbSuccessfully = true;
						CreateAndSaveInterchange(Logger, ref savedOneInterchangeToDbSuccessfully, interchangeString);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						Logger.ContinueLog(e.Message);
						ErrorReporter.ReportOnce(e.Message);
					}
				}
			});

			try
			{
				igorsAwesomeClient.RetrieveMessageStub(saveInterchangesDelegate);
			}
			catch (AggregateException ex)
			{
				LogProblem(ex);
			}
			catch (System.Net.WebException ex)
			{
				LogProblem(ex);
			}
		}

		protected virtual MHAccessClient GetIgorsClient(SGGlbStaffWrapper brokerWrapper, ZString userId)
		{
			return new MHAccessClient(userId,
				brokerWrapper?.Tradenetv4Password?.CurrentDecryptedPassword ?? ZString.Empty,
				new MHUBSettingsProvider(),
				Logger);
		}

		void LogProblem(Exception e)
		{
			var messageBody = "Problem polling\r\n." + e.Message;
			var inner = e.InnerException;
			while (inner != null)
			{
				messageBody += inner.Message + "\r\n\r\n";
				inner = inner.InnerException;
			}
			Logger.LogWarning(messageBody);
		}
	}
}
