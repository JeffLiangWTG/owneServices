using System;
using System.Collections;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class BaseInterchangeRetriever : BatchProcess
	{
		protected override void Execute(CancellationToken token)
		{
			RetrieveAndProcessInterchanges(token);
		}

		protected abstract void ClearProcessedInterchanges();
		protected abstract void RetrieveInterchanges(int numberToRetrieve, CancellationToken token);

		#region	Implementation

		#region RetrieveAndProcessInterchanges

		void RetrieveAndProcessInterchanges(CancellationToken token)
		{
			try
			{
				RetrieveInterchanges(numberToRetrieveAtATime, token);
				RetrieveAndProcessInterchange(token);
			}
			catch (MessageProcessingException e)
			{
				HandleMessageProcessingException(e);
			}
		}

		public static void HandleMessageProcessingException(MessageProcessingException e)
		{
			var message = new ZStringBuilder();
			message.Append(e.Message);
			if (e.MessageOrInterchangeText != "")
			{
				message.Append("The interchange/message text:\r\n");
				message.Append(e.MessageOrInterchangeText);
			}

			if (e.ShouldSendDeveloperInformation)
			{
				ErrorReporter.ReportOnce(e.Message, message.ToString(), e);
			}

			if (e.ShouldSendEmailToUsers)
			{
				Env.OutgoingCustomsMailManager.CreateAndSaveToCompanyNotificationGroup("Service Task Problems while processing interchanges and messages", message.ToString());
			}
		}

		protected virtual void RetrieveAndProcessInterchange(CancellationToken token)
		{
			if (retrievedInterchanges.Count > 0)
			{
				try
				{
					AddRetrievedInterchangesAndTheirMessagesToDatabase(token);
					GenerateNeededAcknowledgementMessages(token);
				}
				finally
				{
					if (processedInterchanges.Length > 0)
					{
						ClearProcessedInterchanges();
					}
				}
			}
		}

		#endregion

		#region AddRetrievedInterchangesAndTheirMessagesToDatabase

		protected void AddRetrievedInterchangesAndTheirMessagesToDatabase(CancellationToken token)
		{
			processedInterchanges = Array.Empty<BaseRetrievedInterchange>();
			InterchangesThatNeedAcknowledgement.Clear();
			InterchangesThatDontNeedAcknowledgement.Clear();
			int addedInterchanges = 0;
			int duplicateInterchanges = 0;
			try
			{
				foreach (BaseRetrievedInterchange newInterchange in retrievedInterchanges)
				{
					token.ThrowIfCancellationRequested();
					Logger.DebugLog("Trying to add interchange to database:\r\n" + newInterchange.Contents.Replace("'", "'\r\n"));

					string interchangeType = applicationCode;
					if (newInterchange.InterchangeType != null)
					{
						interchangeType = newInterchange.InterchangeType;
					}
					if (AddInterchangeToDatabase(newInterchange, interchangeType))
					{
						addedInterchanges++;
					}
					else
					{
						duplicateInterchanges++;
					}

					ArrayList newBaseRetrievedInterchangeList = new ArrayList(processedInterchanges);
					newBaseRetrievedInterchangeList.Add(newInterchange);
					processedInterchanges = (BaseRetrievedInterchange[])newBaseRetrievedInterchangeList.ToArray(typeof(BaseRetrievedInterchange));
				}
			}
			finally
			{
				if (addedInterchanges > 0)
				{
					Logger.Log(addedInterchanges + " retrieved interchange(s) added to database for processing.");
				}
				if (duplicateInterchanges > 0)
				{
					Logger.LogWarning(duplicateInterchanges + " retrieved interchange(s) were duplicates.");
				}
			}
		}

		#endregion

		#region GenerateNeededAcknowledgementMessages

		void GenerateNeededAcknowledgementMessages(CancellationToken token)
		{
			foreach (EDIInterchange myInterchange in InterchangesThatNeedAcknowledgement)
			{
				token.ThrowIfCancellationRequested();
				CreateAcknowledgementMessage(myInterchange.EI_From,
					myInterchange.EI_From,
					myInterchange.EI_To,
					myInterchange.EI_InterchangeNum,
					myInterchange.PK.ToGuid());
			}
			if (InterchangesThatNeedAcknowledgement.Count > 0)
			{
				Logger.Log(InterchangesThatNeedAcknowledgement.Count + " acknowledgement messages generated and placed on the send queue.");
			}
		}

		#endregion

		#region CreateInterchange

		protected virtual EDIInterchange CreateInterchange(BusinessObjectFactory factory, BaseRetrievedInterchange retrievedInterchange)
		{
			return EDIInterchange.CreateNewInterchangeFromString(factory, retrievedInterchange.Contents);
		}

		#endregion

		#region CreateAcknowledgementMessage

		protected virtual void CreateAcknowledgementMessage(string senderID, string ownerID, string recipientID, string referenceNumber, Guid interchangePK)
		{
			ErrorReporter.ReportOnce("Message " + referenceNumber + " expected an Acknowledgement, but class " + this.GetType().FullName + " did not override CreateAcknowledgementMessage");
		}

		#endregion

		#region AddInterchangeToDatabase

		protected internal bool AddInterchangeToDatabase(BaseRetrievedInterchange retrievedInterchange, string interchangeType)
		{
			ZDateTime deliveryTime = retrievedInterchange.RetrievedTime;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			EDIInterchange zInterchange = CreateInterchange(factory, retrievedInterchange);

			zInterchange.EI_Priority = "HGH";

			if (zInterchange.EI_NeedsAcknowledgement)
			{
				InterchangesThatNeedAcknowledgement.Add(zInterchange);
			}
			else
			{
				InterchangesThatDontNeedAcknowledgement.Add(zInterchange);
			}

			int messagesInInterchange = zInterchange.ContainedMessages.Count;
			if (zInterchange.EI_IsDuplicateInterchange)
			{
				if (zInterchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.ERouter)
				{
					//we need to handle eRouter interchanges differently because they do not follow edifact
					//standards and don't have an interchange number.
					zInterchange.ContainedMessages.RemoveAll();
					zInterchange.Delete();
				}
				else
				{
					Logger.DebugLog("Interchange #" + zInterchange.EI_InterchangeNum + " from " + zInterchange.EI_From + " has been received previously.  Interchange not being reprocessed.");
					ReportTraxonDuplicationIfNeeded(zInterchange);
					return false;
				}
			}

			Logger.DebugLog(messagesInInterchange + " messages inside interchange.");

			//log interchange delivery time
			if (deliveryTime != ZDateTime.Empty)
			{
				zInterchange.Logs.AddNew(Events.Delivered, deliveryTime.ToOffset(), true);
			}
			try
			{
				factory.Save();
				return true;
			}
			catch (SqlException e)
			{
				Logger.LogWarning("Unable to save: " + e.ToString());
				return false;
			}
		}

		void ReportTraxonDuplicationIfNeeded(EDIInterchange zInterchange)
		{
			if (zInterchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.Traxon)
			{
				var headerText = zInterchange.EI_HeaderText;
				var bodyText = zInterchange.EI_BodyText;
				var footerText = zInterchange.EI_FooterText;
				var filter = new ZQuery();
				filter.AddToFilter(EDIInterchangeSchema.EI_From, zInterchange.EI_From);
				filter.AddToFilter(EDIInterchangeSchema.EI_To, zInterchange.EI_To);
				filter.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, zInterchange.EI_InterchangeNum);
				filter.AddToFilter(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, zInterchange.PK);
				var existingInterchanges = zInterchange.Factory.Load<EDIInterchange>(filter).Where(x => x.EI_HeaderText != headerText || x.EI_BodyText != bodyText || x.EI_FooterText != footerText).ToArray();
				if (existingInterchanges.Length > 0)
				{
					Logger.LogWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Interchange # {0} from {1} has been received previously. There are at least {2} found.", zInterchange.EI_InterchangeNum, zInterchange.EI_From, existingInterchanges.Length));
				}
			}
		}

		#endregion

		protected int numberToRetrieveAtATime = 100;

		protected String applicationCode;
		protected BaseRetrievedInterchange[] processedInterchanges;
		protected ArrayList retrievedInterchanges;

		protected readonly ArrayList InterchangesThatNeedAcknowledgement = new ArrayList();
		protected readonly ArrayList InterchangesThatDontNeedAcknowledgement = new ArrayList();

		#endregion
	}
}
