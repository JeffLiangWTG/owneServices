using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using MsgReader.Outlook;

namespace Enterprise.Recruiter.Business
{
	public class ApplicationDocumentsUpdater
	{
		readonly IApplicantResumeParser parser;
		protected List<string> errors;
		public const string Separator = "~~~";
		public const string HREmailsServiceTaskCode = "HRE";

		public ApplicationDocumentsUpdater(IApplicantResumeParser parser)
		{
			this.parser = parser;
		}

		IRefDocType DocTypeCV { get; set; }
		IRefDocType DocTypeCover { get; set; }

		public event ApplicationDocumentsProgressEventHandler Progress;
		public event UpdateFinishedEventHandler Finished;
		public ApplicationDocumentsProgressStats Stats { get; protected set; }

		#region Process Old Emails

		public virtual void ProcessEmailsBacklog(CancellationToken cancellationToken)
		{
			processedFiles = new List<string>();
			errors = new List<string>();
			int batchIndex = 0;
			var tokens = new List<string>();
			cancel = false;

			var factory = new BusinessObjectFactory();

			var query = new ZQuery(MailDBItemsSchema.MI_Application, HREmailsServiceTaskCode);
			query.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.MarkedForReprocessing);
			query.OrderBy = MailDBItemsSchema.Constants.MI_SendDateTime;

			var reader = new FilteredBusinessObjectReader(query, typeof(MailItem));
			reader.BatchSize = RecruiterDataRegistry.Instance.DaxtraBatchSize.Value;
			reader.SaveBeforeLoadNextEnabled = true;

			Stats = new ApplicationDocumentsProgressStats(reader.ApproximateCount, reader.ApproximateCount / RecruiterDataRegistry.Instance.DaxtraBatchSize.Value + 1);
			List<ParseData> batch = new List<ParseData>();

			if (DocTypeCV == null)
			{
				DocTypeCV = RecruiterDataRegistry.Instance.GetDocTypeCV(factory);
			}

			if (DocTypeCover == null)
			{
				DocTypeCover = RecruiterDataRegistry.Instance.GetDocTypeCoverLetter(factory);
			}

			if (DocTypeCV == null || DocTypeCover == null)
			{
				errors.Add(Res.GetString("7C6487BD-A9D6-43DB-B5C5-C93B0E390353", "Resume and Cover Letter document types are not setup"));
				InvokeFinish(cancel || cancellationToken.IsCancellationRequested);
				return;
			}

			var queue = new List<ParsingQueueItem>();
			try
			{
				foreach (MailItem mailItem in reader)
				{
					var rule = HRJobApplicationEmailParser.GetEmailParsingRule(mailItem.GetFromEmailAddress());
					mailItem.MI_Status = MailStatus.Failed;
					if (cancel || cancellationToken.IsCancellationRequested)
					{
						break;
					}

					if (!rule.AllowParseAttachments)
					{
						Stats.RuleBlocked++;
						errors.Add(Res.GetString("8d8a47b2-3062-4b66-9dc9-16fafc8895a4", "Rule blocked : {0} emails", Stats.RuleBlocked));
						Stats.ItemsProcessed++;
						continue;
					}

					if (mailItem.MailAttachments.Count == 0 && !rule.AllowFallbackToEmailBody)
					{
						Stats.NoAttachments++;
						errors.Add(Res.GetString("3DAB53AE-9F9B-4C49-AB88-E289A85CDB2B", "Attachments not found : {0} emails", Stats.NoAttachments));
						Stats.ItemsProcessed++;
						continue;
					}

					var parseData = ExtractParseData(mailItem, rule.AllowFallbackToEmailBody);

					Stats.ItemsProcessed++;

					if (!parseData.HasResume && !parseData.HasCover && !parseData.HasEmailBody)
					{
						Stats.CouldntParse++;
						errors.Add(Res.GetString("BED28839-92C8-40E5-8BDB-1EC124D6F523", "Could not parse resume, cover letter or email body : {0} emails", Stats.CouldntParse));
						continue;
					}

					batch.Add(parseData);
					batchIndex += parseData.DataCount;
					queue.Add(new ParsingQueueItem(mailItem, parseData));

					if (batchIndex++ >= RecruiterDataRegistry.Instance.DaxtraBatchSize.Value)
					{
						batchIndex = 0;
						SendBatch(batch, tokens);

						batch.Clear();

						InvokeProgress();
					}

					var parsedData = ProcessTokens(tokens, queue.Select(x => x.Mail));
					ProcessParsedData(queue, tokens, parsedData);
				}
			}
			finally
			{
				if (batch.Count > 0)
				{
					SendBatch(batch, tokens);
					batch.Clear();
				}

				while (tokens.Count > 0)
				{
					var parsedData = ProcessTokens(tokens, queue.Select(x => x.Mail));
					ProcessParsedData(queue, tokens, parsedData);
				}
			}

			InvokeProgress();
			InvokeFinish(cancel || cancellationToken.IsCancellationRequested);
		}

		protected void InvokeProgress()
		{
			Progress?.Invoke(null, new ApplicationDocumentsProgressEventArgs(Stats));
		}

		protected void InvokeFinish(bool canceled)
		{
			Finished?.Invoke(null, new UpdateFinishedEventArgs(Stats, errors.ToArray(), canceled));
		}

		ParseData ExtractParseData(MailItem mailItem, bool allowFallbackToEmailBody)
		{
			var parseData = new ParseData() { ItemPK = mailItem.PK };

			foreach (var attachment in mailItem.MailAttachments
									.Cast<MailAttachment>()
									.Where
										(x => x.MA_Data.Length > 0
										&& !string.IsNullOrEmpty(x.MA_FileName)
										&& HRJobApplicationEmailParser.IsValidAttachmentType(Path.GetExtension(x.MA_FileName)))
									.OrderByDescending(x => x.MA_Data.Length)
									.ThenBy(x => x.MA_FileName))
			{
				var filename = CVXtractorServiceClient.SanitizeFilename(attachment.MA_FileName);

				if (!HRJobApplicationEmailParser.IsLikelyCoverLetter(filename))
				{
					if (!parseData.HasResume)
					{
						parseData.ResumeData = attachment.MA_Data;
						parseData.ResumeFilename = filename;
					}
				}
				else
				{
					if (!parseData.HasCover)
					{
						parseData.CoverData = attachment.MA_Data;
						parseData.CoverFilename = filename;
					}
				}

				if (parseData.HasResume && parseData.HasCover)
				{
					break;
				}
			}

			if (allowFallbackToEmailBody)
			{
				parseData.EmailBodyData = Encoding.UTF8.GetBytes(mailItem.BodyTextDecoded);
				if (!parseData.HasResume && !parseData.HasCover)
				{
					parseData.Type = ParseData.ParsingType.EmailBody;
				}
			}

			return parseData;
		}

		void ProcessParsedData(List<ParsingQueueItem> queue, List<string> tokens, List<DataFromZip> parsedData)
		{
			var emailBodyBatch = new List<ParseData>();
			foreach (var entry in parsedData)
			{
				var item = queue.FirstOrDefault(i => i.Mail.PK == entry.ItemPk);

				if (item != null)
				{
					var xml = Encoding.UTF8.GetString(entry.Data);

					var conversionError = GetConversionReportError(xml);

					if (!string.IsNullOrEmpty(conversionError))
					{
						errors.Add(conversionError);
						return;
					}

					var resume = GetResumeFromParsed(xml);

					//try parse email body
					if ((resume.Name.IsEmpty || resume.EmailAddress.IsEmpty) && item.Data.Type == ParseData.ParsingType.ResumeAndCover && item.Data.HasEmailBody)
					{
						item.Data.Type = ParseData.ParsingType.EmailBody;
						emailBodyBatch.Add(item.Data);
						continue;
					}

					if (resume.EmailAddress.IsEmpty)
					{
						return;
					}

					if (ProcessParsedDataCore(item.Mail, entry, resume) != null)
					{
						item.Mail.MI_Status = MailStatus.Processed;
					}
				}
			}

			if (emailBodyBatch.Any())
			{
				Stats.BatchesTotal++;
				SendBatch(emailBodyBatch, tokens);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		string GetConversionReportError(string xml)
		{
			if (!xml.ToUpperInvariant().Contains("CVPROCESSORREPORT"))
			{
				return null;
			}

			xml = xml.Replace("encoding='utf-8'", "").Replace("encoding=\"utf-8\"", "");

			try
			{
				var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
				if (xml.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
				{
					xml = xml.Remove(0, byteOrderMarkUtf8.Length);
				}

				var doc = new XmlDocument();
				doc.PreserveWhitespace = true;
				doc.LoadXml(xml);

				return doc.SelectSingleNode($"CVProcessorReport/CVProcessorResult")?.InnerText ?? ZString.Empty;
			}
			catch { }

			return null;
		}

		public HRJobApplication ProcessParsedDataCore(MailItem processedMailItem, DataFromZip entry, IApplicantResume resume, bool matchByPeriod = true, bool createApplicantWithEmptyEmail = false, bool allowMatchJobOpening = true)
		{
			var processEmail = true;

			if (string.IsNullOrEmpty(resume.EmailAddress))
			{
				if (createApplicantWithEmptyEmail)
				{
					processEmail = false;
				}
				else
				{
					return null;
				}
			}

			if (Stats == null)
			{
				Stats = new EmptyStats();
			}

			HRJobApplication application = null;
			HRJobApplicant applicant = null;
			var noAttachments = processedMailItem.MailAttachments.Count < 1;

			if (processEmail)
			{
				applicant = HRJobApplicationEmailParser.MatchApplicant(processedMailItem.Factory, resume);

				if (applicant != null)
				{
					var adTitle = HRJobApplicationEmailParser.GetAdTitleFromSubject(processedMailItem.MI_Subject);

					if (!string.IsNullOrWhiteSpace(adTitle))
					{
						application = ProcessMatchByAdTitle(processedMailItem, entry, adTitle, applicant);
					}

					if (application == null)
					{
						application = GetApplicationByExactSubmissionTime(applicant, processedMailItem.MI_SendDateTime, processedMailItem.Factory);

						if (application == null)
						{
							if (matchByPeriod)
							{
								var applications = GetApplicationBySubmissionPeriod(processedMailItem, applicant);

								if (applications.Length > 0)
								{
									foreach (var app in applications)
									{
										CreateApplicationDocument(entry, app, noAttachments);
										AttachOriginalAttachment(entry, processedMailItem, app);
										AttachOriginalEmail(processedMailItem, app);
									}
									application = applications[0];
								}
							}
						}
						else
						{
							CreateApplicationDocument(entry, application, noAttachments);
							AttachOriginalAttachment(entry, processedMailItem, application);
							AttachOriginalEmail(processedMailItem, application);
						}

						if (application == null)
						{
							application = processedMailItem.Factory.New<HRJobApplication>();
							application.HP_HA = applicant.PK;
							application.HP_SubmissionTimeUtc = processedMailItem.MI_SendDateTime;
							CreateApplicationDocument(entry, application, noAttachments);
							AttachOriginalAttachment(entry, processedMailItem, application);
							AttachOriginalEmail(processedMailItem, application);

							if (allowMatchJobOpening)
							{
								MatchJobOpening(processedMailItem, application);
							}
						}
					}
				}
			}

			if (applicant == null)
			{
				application = CreateNewJobApplicantAndApplication(processedMailItem, resume, allowMatchJobOpening);
				CreateApplicationDocument(entry, application, noAttachments);
				AttachOriginalAttachment(entry, processedMailItem, application);
			}

			return application;
		}

		HRJobApplication GetApplicationByExactSubmissionTime(HRJobApplicant applicant, ZDateTime submissionTime, BusinessObjectFactory factory)
		{
			var applicationsQuery = new ZQuery(HRJobApplicationSchema.HP_HA, applicant.PK);
			applicationsQuery.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, submissionTime);

			applicationsQuery.OrderBy = HRJobApplicationSchema.Constants.HP_SubmissionTimeUtc;

			return factory.LoadTop1<HRJobApplication>(applicationsQuery);
		}

		public static HRJobApplication CreateNewJobApplicantAndApplication(MailItem processedMailItem, IApplicantResume resume, bool allowMatchJobOpening = true)
		{
			var applicant = processedMailItem.Factory.New<HRJobApplicant>();
			if (resume != null)
			{
				if (!string.IsNullOrEmpty(resume.EmailAddress))
				{
					var existingPerson = processedMailItem.Factory.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PER_EmailAddress, resume.EmailAddress));
					if (existingPerson != null)
					{
						applicant.HA_PER = existingPerson.PK;
					}
				}

				HRJobApplicationEmailParser.SetApplicantValues(resume, applicant);
			}

			var application = processedMailItem.Factory.New<HRJobApplication>();
			application.HP_HA = applicant.PK;
			application.HP_SubmissionTimeUtc = processedMailItem.MI_SendDateTime;

			AttachOriginalEmail(processedMailItem, application);

			if (allowMatchJobOpening)
			{
				MatchJobOpening(processedMailItem, application);
			}

			return application;
		}

		static void MatchJobOpening(MailItem mailItem, HRJobApplication application)
		{
			var openings = HRJobApplicationEmailParser.FindMatchingJobOpenings(mailItem.MI_Subject, mailItem.MI_SendDateTime, mailItem.Factory);
			if (openings.Length == 0)
			{
				return;
			}

			var latestOpening = openings.OrderByDescending(o => o.HV_CampaignStartDate).First();
			application.HP_HV = latestOpening.PK;
		}

		static void AttachOriginalEmail(MailItem mailItem, HRJobApplication application)
		{
			var docType = RecruiterDataRegistry.Instance.GetDocTypeReferringSource(mailItem.Factory)?.RT_DocType;
			if (string.IsNullOrEmpty(docType))
			{
				docType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			}

			BusinessObjectEmailAttacher.AttachEmail(application, docType, mailItem);
		}

		HRJobApplication ProcessMatchByAdTitle(MailItem mailItem, DataFromZip entry, string adTitle, HRJobApplicant applicant)
		{
			var relevantApplication = GetApplicationByAdTitle(mailItem, adTitle, applicant);
			if (relevantApplication != null)
			{
				CreateApplicationDocument(entry, relevantApplication, mailItem.MailAttachments.Count < 1);

				AttachOriginalAttachment(entry, mailItem, relevantApplication);
				AttachOriginalEmail(mailItem, relevantApplication);
			}

			return relevantApplication;
		}

		static void AttachOriginalAttachment(DataFromZip entry, MailItem mailItem, HRJobApplication application)
		{
			var splits = entry.Filename.Split(new[] { Separator }, StringSplitOptions.None);

			if (splits.Length != 2)
			{
				return;
			}

			var filename = Path.GetFileNameWithoutExtension(splits[1]);
			var originalAttachment = mailItem.MailAttachments
					.Cast<MailAttachment>()
					.FirstOrDefault(a => CVXtractorServiceClient.SanitizeFilename(a.MA_FileName)
					.EqualsIgnoringCase(filename))
				?? mailItem.MailAttachments
					.Cast<MailAttachment>()
					.FirstOrDefault(a => CVXtractorServiceClient.SanitizeFilename(a.MA_FileName)
					.EqualsIgnoringCase(splits[1]));

			if (originalAttachment != null)
			{
				HRJobApplicationEmailParser.AttachEdoc(application, originalAttachment.MA_FileName, originalAttachment.MA_Data);
			}
		}

		IApplicantResume GetResumeFromParsed(string resumeXml)
		{
			var resume = parser.ReadFromXml(resumeXml);
			return resume;
		}

		HRJobApplication GetApplicationByAdTitle(MailItem mailItem, string adTitle, HRJobApplicant applicant)
		{
			var jobOpeningQuery = new ZDBOnlyQuery(typeof(HRRecruitmentJobCampaign));
			jobOpeningQuery.AddToFilter(HRRecruitmentJobCampaignSchema.HV_AdTitle, adTitle ?? string.Empty);

			var applicationQuery = new ZDBOnlySubQuery(typeof(HRJobApplication), HRJobApplicationSchema.HP_HV);
			applicationQuery.AddToFilter(HRJobApplicationSchema.HP_HA, applicant.PK);

			jobOpeningQuery.AddSubQuery(applicationQuery, JoinCondition.And);
			jobOpeningQuery.OrderBy = HRRecruitmentJobCampaignSchema.Constants.HV_CampaignEndDate + " desc";

			var openings = mailItem.Factory.Load<HRRecruitmentJobCampaign>(jobOpeningQuery);
			if (openings.Length > 0)
			{
				HRRecruitmentJobCampaign relevantOpening = null;
				if (openings.Length == 1)
				{
					if (IsMailWithinOpeningDates(mailItem, openings[0]))
					{
						relevantOpening = openings[0];
					}
				}
				else
				{
					foreach (var opening in openings)
					{
						bool mailWithinOpeningDates = IsMailWithinOpeningDates(mailItem, opening);
						if (mailWithinOpeningDates)
						{
							relevantOpening = opening;
							break;
						}
					}

					if (relevantOpening == null && IsMailWithinOpeningDates(mailItem, openings[0]))
					{
						relevantOpening = openings[0];
					}
				}

				if (relevantOpening != null)
				{
					var relevantApplicationQuery = new ZQuery(HRJobApplicationSchema.HP_HV, relevantOpening.PK);
					relevantApplicationQuery.AddToFilter(HRJobApplicationSchema.HP_HA, applicant.PK);
					relevantApplicationQuery.OrderBy = HRJobApplicationSchema.Constants.HP_SubmissionTimeUtc + " desc";

					return mailItem.Factory.LoadTop1<HRJobApplication>(relevantApplicationQuery);
				}
			}
			return null;
		}

		static bool IsMailWithinOpeningDates(MailItem mailItem, HRRecruitmentJobCampaign opening)
		{
			return opening.HV_CampaignStartDate < mailItem.MI_SendDateTime && opening.HV_CampaignEndDate > mailItem.MI_SendDateTime.AddDays(-30);
		}

		HRJobApplication[] GetApplicationBySubmissionPeriod(MailItem mailItem, HRJobApplicant applicant)
		{
			var applicationsQuery = new ZQuery(HRJobApplicationSchema.HP_HA, applicant.PK);
			applicationsQuery.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, mailItem.MI_SendDateTime.AddDays(30));
			applicationsQuery.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, mailItem.MI_SendDateTime.AddDays(-30));

			applicationsQuery.OrderBy = HRJobApplicationSchema.Constants.HP_SubmissionTimeUtc;

			return mailItem.Factory.Load<HRJobApplication>(applicationsQuery);
		}

		#endregion

		#region Process Application Batch

		public void ProcessBatch(HRJobApplication[] applications)
		{
			processedFiles = new List<string>();
			errors = new List<string>();
			int batchIndex = 0;
			var tokens = new List<string>();
			cancel = false;

			if (applications.Length == 0)
			{
				return;
			}

			Stats = new ApplicationDocumentsProgressStats(applications.Length, applications.Length / RecruiterDataRegistry.Instance.DaxtraBatchSize.Value + 1);
			List<ParseData> batch = new List<ParseData>();

			try
			{
				if (DocTypeCV == null)
				{
					DocTypeCV = RecruiterDataRegistry.Instance.GetDocTypeCV(applications[0].Factory);
				}

				if (DocTypeCover == null)
				{
					DocTypeCover = RecruiterDataRegistry.Instance.GetDocTypeCoverLetter(applications[0].Factory);
				}

				foreach (var application in applications)
				{
					if (cancel)
					{
						break;
					}

					if (application.Documents.Count > 0)
					{
						Stats.ItemsProcessed++;
						continue;
					}

					if (!application.SubmissionTimeLocal.Equals(application.Applicant.LastSubmittedJobApplicationSubmissionTime))
					{
						Stats.ItemsProcessed++;
						continue;
					}

					if (DocTypeCV != null && DocTypeCover != null)
					{
						var parseData = ExtractParseData(application);
						Stats.ItemsProcessed++;

						if (!parseData.HasResume && !parseData.HasCover)
						{
							continue;
						}

						batch.Add(parseData);
						batchIndex += parseData.HasResume ? parseData.HasCover ? 2 : 1 : 0;

						if (batchIndex++ >= RecruiterDataRegistry.Instance.DaxtraBatchSize.Value)
						{
							batchIndex = 0;
							SendBatch(batch, tokens);

							batch.Clear();

							InvokeProgress();
						}
					}

					var result = ProcessTokens(tokens, applications);
					CreateApplicationDocuments(result, applications);
				}
			}
			finally
			{
				if (batch.Count > 0)
				{
					SendBatch(batch, tokens);
				}

				while (tokens.Count > 0)
				{
					var result = ProcessTokens(tokens, applications);
					CreateApplicationDocuments(result, applications);
				}
			}

			InvokeProgress();
			InvokeFinish(cancel);
		}

		#endregion

		#region CreateApplicationDocuments

		void CreateApplicationDocuments(List<DataFromZip> parsedData, HRJobApplication[] applications)
		{
			foreach (var entry in parsedData)
			{
				var application = applications.FirstOrDefault(a => a.PK == entry.ItemPk);

				if (application != null)
				{
					CreateApplicationDocument(entry, application, false);
				}
			}
		}

		void CreateApplicationDocument(DataFromZip entry, HRJobApplication application, bool isEmailWithoutAttachment)
		{
			string docType;
			if (isEmailWithoutAttachment)
			{
				docType = ApplicationDocumentTypes.Codes.ReferringSource;
			}
			else
			{
				docType = !HRJobApplicationEmailParser.IsLikelyCoverLetter(entry.Filename)
								? ApplicationDocumentTypes.Codes.Resume
								: ApplicationDocumentTypes.Codes.CoverLetter;
			}

			var newDoc = application.Documents.AddNew();
			newDoc.HPD_Type = docType;

			newDoc.HPD_Content = Encoding.UTF8.GetString(entry.Data).Replace("encoding='utf-8'", "").Replace("encoding=\"utf-8\"", "");
			Stats.DocumentsSaved++;
		}

		#endregion

		#region Batching and Zip

		void SendBatch(List<ParseData> batch, List<string> tokens)
		{
			var result = parser.SendBatch(CreateZip(batch));
			if (result.Status == ApplicantResumeParseStatus.Success && !string.IsNullOrEmpty(result.Token))
			{
				if (!tokens.Contains(result.Token))
				{
					tokens.Add(result.Token);
					Stats.BatchesSent++;
				}
			}
			else
			{
				string message = Res.GetString("3F2A1248-1A39-4545-9185-E9D76AA9A895", "Error sending batch. Status: {0}, token exists: {1}, error message: {2}.",
					result.Status, !string.IsNullOrEmpty(result.Token), result.Message);

				errors.Add(message);
			}
		}

		readonly ZipExtractor extractor = new ZipExtractor();

		byte[] CreateZip(List<ParseData> batch)
		{
			byte[] result;
			ZipCreator zip = new ZipCreator();
			List<ZipStream> streams = new List<ZipStream>();
			List<Stream> toDispose = new List<Stream>();
			try
			{
				using (var ms = new MemoryStream())
				{
					foreach (var parseData in batch)
					{
						if (parseData.HasResume && parseData.Type == ParseData.ParsingType.ResumeAndCover)
						{
							var resumeStream = new MemoryStream(parseData.ResumeData);
							streams.Add(new ZipStream(parseData.ItemPK + Separator + parseData.ResumeFilename, resumeStream));

							toDispose.Add(resumeStream);
						}

						if (parseData.HasCover && parseData.Type == ParseData.ParsingType.ResumeAndCover)
						{
							var coverStream = new MemoryStream(parseData.CoverData);
							streams.Add(new ZipStream(parseData.ItemPK + Separator + parseData.CoverFilename, coverStream));
							toDispose.Add(coverStream);
						}

						if (parseData.HasEmailBody && parseData.Type == ParseData.ParsingType.EmailBody)
						{
							var emailBodyStream = new MemoryStream(parseData.EmailBodyData);
							streams.Add(new ZipStream(parseData.ItemPK + Separator + "EmailBody.html", emailBodyStream));
							toDispose.Add(emailBodyStream);
						}

						result = ms.ToArray();
					}

					zip.ZipStream(streams, ms);
					result = ms.ToArray();
				}
			}
			finally
			{
				foreach (var stream in toDispose)
				{
					stream.Dispose();
				}
			}

			return result;
		}

		List<string> processedFiles;
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		List<DataFromZip> ProcessZip(byte[] zipContent, ZGuid[] itemPKs)
		{
			var result = new List<DataFromZip>();
			try
			{
				string[] files;
				using (var memoryStream = new MemoryStream(zipContent))
				{
					files = extractor.GetFileNames(memoryStream);
				}

				if (files.Length == 0)
				{
					return result;
				}

				using (var memoryStream = new MemoryStream(zipContent))
				{
					foreach (var file in files)
					{
						if (processedFiles.Contains(file))
						{
							continue;
						}

						processedFiles.Add(file);
						using (var output = new MemoryStream())
						{
							extractor.ExtractZipStream(memoryStream, output, file);
							var nameSplits = file.Split(new[] { Separator }, StringSplitOptions.None);
							if (nameSplits.Length < 2)
							{
								errors.Add(Res.GetString("D4B32FF8-374E-4E80-8338-40964A3A8A5B", "Invalid filename : [{0}]", file));
								continue;
							}

							var parsedId = nameSplits[0].Substring(nameSplits[0].Length - 36, 36);
							ZGuid itemPK = ZGuid.Empty;
							if (!ZGuid.TryParse(parsedId, out itemPK))
							{
								errors.Add(Res.GetString("3912F07E-1041-43FD-AC5F-D53DE701EF73", "Invalid item ID in [{0}] : [{1}]", file, nameSplits[0]));
								continue;
							}

							var resultPK = itemPKs.FirstOrDefault(i => i == itemPK);
							result.Add(new DataFromZip(file, resultPK, output.ToArray()));
						}
					}
				}
			}
			catch (Exception ex)
			{
				errors.Add(Res.GetString("40AE3559-D882-40DD-A998-1458C0597EE5", "Error: ", ex.Message));
			}

			return result;
		}

		public class DataFromZip
		{
			public DataFromZip(string filename, ZGuid itemPk, byte[] data)
			{
				Filename = filename;
				ItemPk = itemPk;
				Data = data;
			}

			public string Filename { get; }
			public ZGuid ItemPk { get; }

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public byte[] Data { get; }
		}

		protected virtual List<DataFromZip> ProcessTokens(List<string> tokens, IEnumerable<BusinessObject> bizOs)
		{
			var result = new List<DataFromZip>();
			if (tokens.Count == 0)
			{
				return result;
			}

			if (cancel)
			{
				return result;
			}

			var token = tokens[0];

			var parsedData = parser.GetData(token);

			if (parsedData.Status == ApplicantBatchGetDataStatus.Processing)
			{
				return result;
			}

			if (parsedData.Status == ApplicantBatchGetDataStatus.Error)
			{
				if (!string.IsNullOrEmpty(parsedData.Error))
				{
					errors.Add(parsedData.Error + ": " + token);
				}

				tokens.Remove(token);
				Stats.BatchesProcessed++;
				InvokeProgress();
			}
			else if (parsedData.Status == ApplicantBatchGetDataStatus.Finished)
			{
				tokens.Remove(token);
				bizOs.First().Factory.Save();
				Stats.BatchesProcessed++;
				InvokeProgress();
			}
			else if (parsedData.Status == ApplicantBatchGetDataStatus.Zip)
			{
				result = ProcessZip(parsedData.ZipContent, bizOs.Select(b => b.PK).ToArray());
			}

			return result;
		}

		#endregion

		#region Cancel

		bool cancel;
		public void Cancel()
		{
			cancel = true;
		}

		#endregion

		#region Extract and Parse

		void ExtractFromMail(ParseData parseData, IeDoc[] edocs)
		{
			var latestCV = edocs.FirstOrDefault(e =>
											Path.GetExtension(e.FileName).ToUpperInvariant().Equals(HRJobApplicationEmailParser.MsgExt)
											|| Path.GetExtension(e.FileName).ToUpperInvariant().Equals(HRJobApplicationEmailParser.EmlExt));
			var extension = Path.GetExtension(latestCV.FileName).ToUpperInvariant();
			var conditionAttachments = new List<(byte[], string)>();
			var attachments = Enumerable.Empty<object>();
			if (extension.Equals(HRJobApplicationEmailParser.MsgExt))
			{
				using (var stream = latestCV.GetImageDataReader())
				using (var message = new Storage.Message(stream))
				{
					attachments = message.Attachments.Select(x => x as Storage);
				}
			}
			else
			{
				using (var stream = latestCV.GetImageDataReader())
				{
					attachments = MimeMessage.Load(stream).GetFullAttachments();
				}
			}

			foreach (var attachment in attachments)
			{
				var fileName = string.Empty;
				var fileData = Array.Empty<byte>();
				if (attachment is Storage msgAttachment)
				{
					fileName = msgAttachment.GetName();
					fileData = msgAttachment.GetData();
				}
				if (attachment is MimeEntity emlAttachment)
				{
					fileName = emlAttachment.GetName();
					fileData = emlAttachment.GetData();
				}
				if (fileData.Length > 0 && !string.IsNullOrEmpty(fileName) && HRJobApplicationEmailParser.IsValidAttachmentType(Path.GetExtension(fileName)))
				{
					conditionAttachments.Add((fileData, fileName));
				}
			}

			foreach (var attachment in conditionAttachments
				.OrderBy(x => x.Item1.Length)
				.ThenBy(x => x.Item2))
			{
				if (parseData.ResumeData == null)
				{
					parseData.ResumeData = attachment.Item1;
					parseData.ResumeFilename = attachment.Item2;
				}
				else if (parseData.CoverData == null)
				{
					parseData.CoverData = attachment.Item1;
					parseData.CoverFilename = attachment.Item2;
				}
				else
				{
					break;
				}
			}
		}

		IeDoc[] GetFilesFromObject(DocManagerInfo docManager, IRefDocType docTypeCV, IRefDocType docTypeCover)
		{
			if (docManager.Files.Count == 0)
			{
				return Array.Empty<IeDoc>();
			}

			List<IeDoc> result = new List<IeDoc>();

			var resume = docManager.Files.GetMostRecentEDoc(docTypeCV.RT_DocType);
			if (resume != null)
			{
				result.Add(resume);

				var cover = docManager.Files.GetMostRecentEDoc(docTypeCover.RT_DocType);
				if (cover != null && cover.DateAdded.Date == resume.DateAdded.Date)
				{
					result.Add(cover);
				}
			}

			return result.ToArray();
		}

		ParseData ExtractParseData(HRJobApplication application)
		{
			var parseData = new ParseData();
			parseData.ItemPK = application.PK;

			var edocsApplication = GetFilesFromObject(application.DocManagerInfo, DocTypeCV, DocTypeCover);
			var edocsApplicant = GetFilesFromObject(application.Applicant.DocManagerInfo, DocTypeCV, DocTypeCover);

			IeDoc latestCV = null;
			IeDoc latestCover = null;
			if (edocsApplication.Length > 0)
			{
				latestCV = edocsApplication.FirstOrDefault(e => e.DocType == DocTypeCV.RT_DocType);
				latestCover = edocsApplication.FirstOrDefault(e => e.DocType == DocTypeCover.RT_DocType);
			}

			if (edocsApplicant.Length > 0)
			{
				var cv = edocsApplicant.FirstOrDefault(e => e.DocType == DocTypeCV.RT_DocType);
				if (latestCV == null || cv.DateAdded > latestCV.DateAdded)
				{
					latestCV = cv;
					latestCover = edocsApplicant.FirstOrDefault(e => e.DocType == DocTypeCover.RT_DocType);
				}
			}

			if (latestCV != null)
			{
				parseData.ResumeData = latestCV.ImageData;
				parseData.ResumeFilename = latestCV.FileName;
			}

			if (latestCover != null)
			{
				parseData.CoverData = latestCover.ImageData;
				parseData.CoverFilename = latestCover.FileName;
			}

			if (parseData.ResumeData == null && edocsApplication.Length > 0)
			{
				ExtractFromMail(parseData, edocsApplication);
			}

			if (parseData.ResumeData == null && edocsApplicant.Length > 0)
			{
				ExtractFromMail(parseData, edocsApplicant);
			}

			return parseData;
		}

		class ParseData
		{
			public byte[] ResumeData { get; set; }
			public byte[] CoverData { get; set; }
			public byte[] EmailBodyData { get; set; }
			public string ResumeFilename { get; set; }
			public string CoverFilename { get; set; }
			public ZGuid ItemPK { get; set; }

			public bool HasResume => ResumeData != null;
			public bool HasCover => CoverData != null;
			public bool HasEmailBody => EmailBodyData != null;

			public ParsingType Type { get; set; } = ParsingType.ResumeAndCover;

			public enum ParsingType
			{
				ResumeAndCover,
				EmailBody
			}

			public int DataCount => (Type == ParsingType.ResumeAndCover ? new[] { HasResume, HasCover } : new[] { HasEmailBody }).Count(x => x);
		}

		class ParsingQueueItem
		{
			public ParsingQueueItem(MailItem mail, ParseData parseData)
			{
				Mail = mail;
				Data = parseData;
			}

			public readonly MailItem Mail;
			public readonly ParseData Data;
		}

		#endregion

		#region Events

		[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
		public delegate void ApplicationDocumentsProgressEventHandler(object sender, ApplicationDocumentsProgressEventArgs e);

		[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
		public delegate void UpdateFinishedEventHandler(object sender, UpdateFinishedEventArgs e);

		public class UpdateFinishedEventArgs : EventArgs
		{
			public UpdateFinishedEventArgs(ApplicationDocumentsProgressStats stats, string[] errors, bool cancelled)
				: base()
			{
				Stats = stats;
				Errors = errors;
				Cancelled = cancelled;
			}

			public ApplicationDocumentsProgressStats Stats { get; }
			public IEnumerable<string> Errors { get; }
			public bool Cancelled { get; }
		}

		public class ApplicationDocumentsProgressEventArgs : EventArgs
		{
			public ApplicationDocumentsProgressEventArgs(ApplicationDocumentsProgressStats stats)
				: base()
			{
				Stats = stats;
			}

			public ApplicationDocumentsProgressStats Stats { get; }
		}

		#endregion

		#region Statistics

		public class ApplicationDocumentsProgressStats
		{
			public ApplicationDocumentsProgressStats(int total, int batches)
			{
				ItemsTotal = total;
				BatchesTotal = batches;
			}

			public int ItemsProcessed { get; set; }
			public int ItemsTotal { get; }
			public int BatchesSent { get; set; }
			public int BatchesProcessed { get; set; }
			public int BatchesTotal { get; set; }
			public int DocumentsSaved { get; set; }
			public int NoAttachments { get; set; }
			public int CouldntParse { get; set; }
			public int RuleBlocked { get; set; }
		}

		class EmptyStats : ApplicationDocumentsProgressStats
		{
			public EmptyStats()
				: base(0, 0)
			{
			}
		}

		#endregion
	}
}
