using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.CryptoUtilities;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class NewBaseInterchangeRetriever : BatchProcess
	{
		protected NewBaseInterchangeRetriever() : base() { }

		protected NewBaseInterchangeRetriever(LoggingInformation logger) : base(logger) { }

		#region	Implementation

		public IMailFilter IncomingInterchangeMailFilter => LazyInitializer.EnsureInitialized(ref mailFilter, GetMailFilter);
		IMailFilter mailFilter;

		protected abstract IMailFilter GetMailFilter();

		protected static ZQuery BuildSubjectsQuery(params string[] subjects)
			=> subjects.Aggregate(new ZQuery(), (q, s) => q.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, s));

		public int RetrievedInterchanges;

		protected override void Execute(CancellationToken token)
		{
			RetrievedInterchanges = 0;

			PKsOfMailItemsThatProcessed.Clear();
			PKsOfMailItemsThatCausedExceptions.Clear();

			if (!ShouldProcessMailItems)
			{
				return;
			}

			IEnumerable<MailItem> mailItems;

			void GetQueuedEmailsToProcess()
			{
				token.ThrowIfCancellationRequested();

				var factory = new BusinessObjectFactory() { RefreshEnabled = false };

				mailItems = IncomingInterchangeMailFilter
					.Load(factory, NumberToRetrieveAtATime)
					.Cast<MailItem>()
					.Where(c => !PKsOfMailItemsThatProcessed.Contains(c.PK));
			}

			GetQueuedEmailsToProcess();

			do
			{
				if (mailItems.Any())
				{
					var numberOfInterchangesRetrievedInBatch = 0;

					var secondFactory = new BusinessObjectFactory();
					secondFactory.RefreshEnabled = false;

					foreach (var firstFactoryMailItem in mailItems)
					{
						token.ThrowIfCancellationRequested();
						ProcessMail(firstFactoryMailItem, secondFactory);

						numberOfInterchangesRetrievedInBatch++;

						if (numberOfInterchangesRetrievedInBatch == NumberToSaveAtATime)
						{
							SaveAndCreateNewFactory(ref secondFactory);
							numberOfInterchangesRetrievedInBatch = 0;
						}
					}

					if (numberOfInterchangesRetrievedInBatch > 0)
					{
						SaveAndCreateNewFactory(ref secondFactory);
					}

					GC.Collect();
				}
				else
				{
					break;
				}

				GetQueuedEmailsToProcess();
			}
			while (mailItems.Any());
		}

		protected virtual bool ShouldProcessMailItems => true;

		protected virtual IEnumerable<ZString> GetCompanyCodesFromMailItem(MailItem item)
		{
			return new[] { GlbCompany.CurrentCompany.GC_Code };
		}

		protected EDIInterchange ProcessMail(MailItem firstFactoryMailItem, BusinessObjectFactory secondFactory)
		{
			EDIInterchange newInterchange = null;
			var item = secondFactory.Load<MailItem>(firstFactoryMailItem.PK);

			if (item.HasContent)
			{
				var companiesForMail = GetCompanyCodesFromMailItem(item).ToArray();
				if (companiesForMail.Any())
				{
					for (var idx = 0; idx < companiesForMail.Length; idx++)
					{
						var companyCode = companiesForMail[idx];
						try
						{
							var processedResult = ProcessMailForCompany(item, secondFactory, companyCode);
							if (processedResult.IsProcessed)
							{
								newInterchange = processedResult.Interchange;
								PKsOfMailItemsThatProcessed.Add(item.PK);
							}
							else
							{
								if (idx == companiesForMail.Length - 1)
								{
									SaveImmediateFailure(item);
								}
								else
								{
									continue;
								}
							}
						}
						catch (CryptoUtilitiesException ex) when (ex.Message.Contains("Invalid cryptographic message type"))
						{
							if (idx == companiesForMail.Length - 1)
							{
								throw;
							}
							else
							{
								continue;
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ProcessExceptionWithPotentialRetry(item, ex, out var rethrow);

							if (rethrow)
							{
								throw;
							}
						}

						break;
					}
				}
				else
				{
					Logger.LogWarning(string.Format("No company was found to process this mail, ignoring. (from: {0}, date: {1}, subject: {2})", item.MI_From, item.MI_ReceivedDateTime, item.MI_Subject));
					SaveImmediateFailure(item);
				}
			}
			else
			{
				Logger.LogWarning(string.Format("Mail message has no content, ignoring. (from: {0}, date: {1}, subject: {2})", item.MI_From, item.MI_ReceivedDateTime, item.MI_Subject));
				SaveImmediateFailure(item);
			}

			return newInterchange;
		}

		(bool IsProcessed, EDIInterchange Interchange) ProcessMailForCompany(MailItem mailItem, BusinessObjectFactory secondFactory, ZString forCompany)
		{
			var isProcessed = true;
			EDIInterchange result = null;
			var enableCustomsDiagnostics = Env.Registry.EnableCustomsDiagnostics;

			using (forCompany == GlbCompany.CurrentCompany.GC_Code ? null : DisposableEnvironment.ForCompany(forCompany))
			{
				if (enableCustomsDiagnostics)
				{
					Logger.Log(string.Format(CultureInfo.InvariantCulture, "Processing company {0}...", forCompany));
				}

				var certificateConfig = GetCertificateConfig(mailItem.Factory);
				if (certificateConfig != null && NeedCompanyCertificate(mailItem) && certificateConfig.CompanyCertificate == null)
				{
					LogWarningMessageForMissingCompanyCertificate(forCompany);
					isProcessed = false;
				}
				else
				{
					var interchangeText = GetInterchangeText(mailItem, true);

					if (!interchangeText.IsEmpty)
					{
						Logger.DebugLog("Trying to add interchange to database:\r\n" + interchangeText.Replace("'", "'\r\n"));
						result = CreateInterchangeAndMessages(secondFactory, interchangeText, mailItem);

						if (result != null && !result.IsDeleted)
						{
							if (result.IsInDatabase)
							{
								Logger.LogWarning("Interchange was a duplicate.  Interchange #" + result.EI_InterchangeNum);
							}
							else
							{
								RetrievedInterchanges++;
								Logger.Log("New Interchange retrieved");
							}

							if (result.EI_NeedsAcknowledgement)
							{
								CreateAcknowledgementMessage(result);
								Logger.Log("Acknowledgement message generated for interchange #" + result.EI_InterchangeNum);
							}
						}
						else
						{
							Logger.LogWarning("Interchange not correctly created, text: ");
							Logger.LogWarning(interchangeText.Replace("'", "'\r\n"));
						}

						mailItem.MI_Status = MailStatus.Processed;
					}
					else if (!ShouldIgnoreEmptyInterchangeText(mailItem))
					{
						Logger.LogWarning(string.Format("Derived interchange text was empty, ignoring. (from: {0}, date: {1}, subject: {2})", mailItem.MI_From, mailItem.MI_ReceivedDateTime, mailItem.MI_Subject));
						SaveImmediateFailure(mailItem);
					}
				}
			}

			return (isProcessed, result);
		}
		protected virtual void LogWarningMessageForMissingCompanyCertificate(ZString companyCode)
		{
		}

		protected virtual bool NeedCompanyCertificate(MailItem mailItem)
		{
			return false;
		}

		protected virtual bool OnlyCreateInterchanges
		{
			get { return false; }
		}

		protected virtual bool ShouldIgnoreEmptyInterchangeText(MailItem mailItem)
		{
			return false;
		}

		protected void SaveImmediateFailure(MailItem item)
		{
			var factoryForPost = new BusinessObjectFactory();

			var itemInSecondFactory = factoryForPost.Load<MailItem>(item.PK);
			if (itemInSecondFactory != null)
			{
				itemInSecondFactory.MI_Status = MailStatus.Failed;
				factoryForPost.Save();
			}

			PKsOfMailItemsThatProcessed.Add(item.PK);
		}

		protected bool SaveAndCreateNewFactory(ref BusinessObjectFactory factory)
		{
			var result = true;

			try
			{
				factory.Save();
				Logger.Log("Interchanges saved to database");
			}
			catch (ZSaveConcurrencyException ex)
			{
				result = false;
				Logger.LogWarning("Interchanges NOT saved to database due to Concurrency Exception: " + ex.Message);
			}

			factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			return result;
		}

		protected virtual CertificateManager GetCertificateConfig(BusinessObjectFactory factory)
		{
			return null;
		}

		protected virtual ZString GetInterchangeText(MailItem item, bool decryptInNewThread)
		{
			var result = ZString.Empty;
			if (item != null)
			{
				if (item.MI_Header.IndexOf("smime.p7m") != -1)
				{
					var certificateConfig = GetCertificateConfig(item.Factory);
					if (certificateConfig != null)
					{
						var mIMEText = item.GetDecodedEmailText(certificateConfig.CompanyCertificate, certificateConfig.TrustPointCertificate, certificateConfig.EncryptionCertificateName, decryptInNewThread);
						result = DecodeMIMEText(mIMEText);
					}
					else
					{
						Logger.LogWarning("There is a secure interchange in incoming mail but there are no certificates configured to decode it");
					}
				}
				else
				{
					result = ExtractInterchangeTextFromAttachment(item);

					if (result.IsEmpty)
					{
						try
						{
							result = Encoding.ASCII.GetString(Convert.FromBase64String(item.MI_Body));
						}
						catch (FormatException)
						{
							result = item.MI_Body;
						}
					}
				}
			}

			return result;
		}

		ZString ExtractInterchangeTextFromAttachment(MailItem item)
		{
			List<ZString> extensions = GetAttachmentExtensionsToLookFor();

			foreach (MailAttachment attachment in item.MailAttachments)
			{
				foreach (ZString extension in extensions)
				{
					if (attachment.MA_FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
					{
						return Encoding.ASCII.GetString(attachment.MA_Data);
					}
				}
			}

			return ZString.Empty;
		}

		protected virtual List<ZString> GetAttachmentExtensionsToLookFor()
		{
			List<ZString> list = new List<ZString>();

			list.Add(".erf");
			list.Add(".edi");

			return list;
		}

		protected virtual ZString DecodeMIMEText(string mIMEText)
		{
			return BatchProcessorSupporter.DecodeMIMEText(mIMEText);
		}

		protected virtual bool SendCryptoExceptionToPostMaster
		{
			get { return false; }
		}

		protected virtual string CryptoExceptionToPostMasterText
		{
			get { return ZString.Empty; }
		}

		protected void ProcessExceptionWithPotentialRetry(MailItem item, Exception e, out bool rethrow)
		{
			rethrow = false;
			Logger.LogWarning(e.Message);

			if (item != null)
			{
				e.Source = e.Source + ", Mail Item PK = " + item.PK.ToString() + ". Mail Subject = " + item.MI_Subject + ". Mail Item Header = " + item.MI_Header;

				if (e is MessageProcessingException)
				{
					Logger.LogWarning("Mail Item set to Failed. " + e.Source);
					SaveImmediateFailure(item);
				}
				else
				{
					PKsOfMailItemsThatCausedExceptions.TryGetValue(item.PK, out var lastCount);

					lastCount++;
					PKsOfMailItemsThatCausedExceptions[item.PK] = lastCount;

					if (lastCount >= maximumAttemptsAtDecodingInterchange)
					{
						SaveImmediateFailure(item);

						if (e is MailItemDecodeFail)
						{
							ZString body = ((MailItemDecodeFail)e).EmailBody;
							e.Source = e.Source + "Start of Mail Body:\r\n" + body.Left(200);
							rethrow = true;
						}
						else if (SendCryptoExceptionToPostMaster && (e is System.Security.Cryptography.CryptographicException || e is CryptoUtilitiesException))
						{
							EmailDef postmasterEmailDef = new EmailDef();
							ZString messageBody = CryptoExceptionToPostMasterText + e.Message + "\r\n\r\n";
							Exception inner = e.InnerException;
							while (inner != null)
							{
								messageBody += inner.Message + "\r\n\r\n";
								inner = inner.InnerException;
							}
							messageBody += @"Some known possible causes:

2146885620 = invalid, or expired, digital certificate, 

2146885622 = certificate password invalid,

2146881278 = spam or virus filter,

2146893821 = 'BAD KEY', this is a Verisgn problem which should no longer occur. If you do experience this problem then please contact the Cargowise Help Desk.

'The enveloped-data message does not contain the specified recipient' = spam or virus filter,

'ASN1 unexpected end of data' = spam or virus filter,

'The documents signer was not the expected signer' = you loaded your (or another) certificate into the customs certificate registry location,

'Timeout decrypting message' = most likely caused when you have manually imported the Type 3 digital certificate into windows with the 'Strong Encryption'
check box checked. Remove the Type 3 certificate from windows. 

";
							postmasterEmailDef.Body = messageBody + e.Source;
							postmasterEmailDef.Subject = "Decrypt/decode error while processing eMail";

							try
							{
								Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(postmasterEmailDef);
							}
							catch (EmailHasNoRecipientsException ex)
							{
								Logger.LogWarning(ex.Message);
							}
						}
						else
						{
							rethrow = true;
						}
					}
				}
			}
		}

		protected int maximumAttemptsAtDecodingInterchange = 3;

		protected Dictionary<ZGuid, int> PKsOfMailItemsThatCausedExceptions
		{
			get { return pksOfMailItemsThatCausedExceptions ?? (pksOfMailItemsThatCausedExceptions = new Dictionary<ZGuid, int>()); }
		}
		Dictionary<ZGuid, int> pksOfMailItemsThatCausedExceptions;

		protected HashSet<ZGuid> PKsOfMailItemsThatProcessed
		{
			get { return pKsOfMailItemsThatProcessed ?? (pKsOfMailItemsThatProcessed = new HashSet<ZGuid>()); }
		}
		HashSet<ZGuid> pKsOfMailItemsThatProcessed;

		protected virtual void CreateAcknowledgementMessage(EDIInterchange interchangeToAcknowledge)
		{
			ErrorReporter.ReportOnce("Interchange " + interchangeToAcknowledge.EI_InterchangeNum + " expected an Acknowledgement, but class " + this.GetType().FullName + " did not override CreateAcknowledgementMessage");
		}

		protected virtual EDIInterchange GetNewInterchange(BusinessObjectFactory factory)
		{
			return factory.New<EDIInterchange>();
		}

		protected virtual EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange result = EDIInterchange.CreateNewInterchangeFromString(factory, interchangeString, "", false, OnlyCreateInterchanges);
			try
			{
				EDIInterchange existingInterchange = result.ExistingInterchangeMatchingToFromAndInterchangeNum;
				if (existingInterchange != null)
				{
					result = ProcessDuplicatedInterchange(result, existingInterchange);
				}

				LogDeliveryTime(result, mailItem.MI_ReceivedDateTime);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!result.IsDeleted)
				{
					result.Delete();
				}

				throw;
			}

			return result;
		}

		protected virtual EDIInterchange ProcessDuplicatedInterchange(EDIInterchange newInterchange, EDIInterchange existingInterchange)
		{
			if (newInterchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.ERouter)
			{
				newInterchange.ContainedMessages.RemoveAll();
			}
			else
			{
				newInterchange.ContainedMessages.RemoveAndDeleteAll();
			}
			newInterchange.Delete();

			return existingInterchange;
		}

		protected void LogDeliveryTime(EDIInterchange interchange, ZDateTime deliveryTime)
		{
			if (deliveryTime != ZDateTime.Empty)
			{
				interchange.Logs.AddNew(Events.Delivered, deliveryTime.ToOffset(), true);
			}
		}

		protected virtual int NumberToRetrieveAtATime
		{
			get { return 50; }
		}

		protected virtual int NumberToSaveAtATime
		{
			get { return 10; }
		}

		#endregion
	}
}
