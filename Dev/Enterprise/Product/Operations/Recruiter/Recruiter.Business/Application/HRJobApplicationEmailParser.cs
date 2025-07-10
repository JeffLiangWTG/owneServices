using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using MsgReader.Outlook;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationEmailParser
	{
		[Flags]
		public enum ParseResult
		{
			None = 1 << 0,
			BadFormat = 1 << 1,
			InvalidFileFormat = 1 << 2,
			NoResumeParsed = 1 << 3,
			MultipleJobOpeningsFound = 1 << 4,
			Timeout = 1 << 5,
			IsEmail = 1 << 6
		}

		public HRJobApplicationEmailParser(MailItem mailItem, IApplicantResumeParser resumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail = false, bool allowMatchJobOpening = true)
		{
			Factory = factory;
			ResumeParser = resumeParser;
			MailItem = mailItem;
			CreateApplicantWithEmptyEmail = createApplicantWithEmptyEmail;
			AllowMatchJobOpening = allowMatchJobOpening;
		}

		public HRJobApplicationEmailParser(HRJobApplication jobApplication, IApplicantResumeParser resumeParser, bool createApplicantWithEmptyEmail = false, bool allowMatchJobOpening = true)
		{
			Parent = jobApplication;
			Factory = jobApplication.Factory;
			ResumeParser = resumeParser;
			CreateApplicantWithEmptyEmail = createApplicantWithEmptyEmail;
			AllowMatchJobOpening = allowMatchJobOpening;
		}

		public HRJobApplication Parent { get; private set; }
		readonly BusinessObjectFactory Factory;
		readonly IApplicantResumeParser ResumeParser;
		readonly MailItem MailItem;
		readonly bool CreateApplicantWithEmptyEmail;
		readonly bool AllowMatchJobOpening;

		public bool IsNewApplication { get; private set; }

		public const string MsgExt = ".MSG";
		public const string EmlExt = ".EML";

		public PopulateFromFileResult PopulateFromFile(ZString fileName, HRRecruitmentJobCampaign jobOpening = null)
		{
			CurrentResult = ParseResult.None;
			var errors = new ZStringBuilder();
			var date = DateTime.MinValue;

			try
			{
				if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
				{
					var fileExtension = Path.GetExtension(fileName).ToUpperInvariant();

					if (MsgExt.Equals(fileExtension) || EmlExt.Equals(fileExtension))
					{
						CurrentResult |= ParseResult.IsEmail;
						if (fileExtension == MsgExt)
						{
							using (var message = new Storage.Message(fileName, FileAccess.ReadWrite))
							{
								if (message.Headers != null)
								{
									date = message.Headers.DateSent;
								}
								var results = ParseIMail(message, jobOpening, fileName);
								foreach (var result in results)
								{
									if (!string.IsNullOrEmpty(result.Error))
									{
										errors.AppendLine(result.Error);
									}
								}
							}
						}
						else if (fileExtension == EmlExt)
						{
							var message = new MimeMessage();
							try
							{
								message = MimeMessage.Load(fileName);
							}
							catch (Exception ex) when (ex.Source.StartsWith(nameof(MimeKit)))
							{
								var key = $"{GetType().Name}.PopulateFromFile.{ex.GetType().Name}";
								ErrorReporter.ReportOnce(key, $"Exception parsing EML file: {Path.GetFileName(fileName)}", ex);
							}

							if (message.Date != DateTimeOffset.MinValue)
							{
								date = message.Date.UtcDateTime;
							}
							var results = ParseIMail(message, jobOpening);
							foreach (var result in results)
							{
								if (!string.IsNullOrEmpty(result.Error))
								{
									errors.AppendLine(result.Error);
								}
							}
						}
					}
					else if (IsValidAttachmentType(fileExtension))
					{
						var parseResult = Parse(fileName, ReadFile(fileName), true, false);
						if (!parseResult.IsParsed)
						{
							CurrentResult |= ParseResult.NoResumeParsed;
						}

						if (!string.IsNullOrEmpty(parseResult.Error))
						{
							errors.AppendLine(parseResult.Error);
						}
					}
					else
					{
						CurrentResult |= ParseResult.InvalidFileFormat;
					}
				}
				else
				{
					CurrentResult |= ParseResult.BadFormat;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CurrentResult |= ParseResult.InvalidFileFormat;
				errors.AppendLine(ex.Message);
			}

			return new PopulateFromFileResult(CurrentResult, errors.ToString(), date);
		}

		public PopulateFromFileResult PopulateFromMailItem(Storage.Message message, HRRecruitmentJobCampaign jobOpening = null)
		{
			CurrentResult = ParseResult.None;
			var errors = new ZStringBuilder();

			try
			{
				if (message != null)
				{
					CurrentResult |= ParseResult.IsEmail;
					var results = ParseIMail(message, jobOpening);
					foreach (var result in results)
					{
						if (!string.IsNullOrEmpty(result.Error))
						{
							errors.AppendLine(result.Error);
						}
					}
				}
				else
				{
					CurrentResult |= ParseResult.BadFormat;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CurrentResult |= ParseResult.InvalidFileFormat;
				errors.AppendLine(ex.Message);
			}

			var date = message != null && message.Headers != null
				? message.Headers.DateSent
				: DateTime.MinValue;
			return new PopulateFromFileResult(CurrentResult, errors.ToString(), date);
		}

		public PopulateFromFileResult PopulateFromMailItem(MimeMessage message, HRRecruitmentJobCampaign jobOpening = null)
		{
			CurrentResult = ParseResult.None;
			var errors = new ZStringBuilder();

			try
			{
				if (message != null)
				{
					CurrentResult |= ParseResult.IsEmail;
					var results = ParseIMail(message, jobOpening);
					foreach (var result in results)
					{
						if (!string.IsNullOrEmpty(result.Error))
						{
							errors.AppendLine(result.Error);
						}
					}
				}
				else
				{
					CurrentResult |= ParseResult.BadFormat;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CurrentResult |= ParseResult.InvalidFileFormat;
				errors.AppendLine(ex.Message);
			}

			var date = message?.Date != DateTimeOffset.MinValue
				? message.Date.UtcDateTime
				: DateTime.MinValue;
			return new PopulateFromFileResult(CurrentResult, errors.ToString(), date);
		}

		public PopulateFromFileResult ProcessQueueFile(ZString fileName, byte[] data, DateTime submissionDate)
		{
			var errors = new ZStringBuilder();
			try
			{
				var result = Parse(fileName, data, false, false);
				if (!string.IsNullOrEmpty(result.Error))
				{
					errors.AppendLine(result.Error);
				}
				if (!result.IsParsed)
				{
					CurrentResult |= ParseResult.NoResumeParsed;
				}
				else if (Parent.Applicant != null)
				{
					SetApplicantValues(result.ApplicantResume, Parent.Applicant);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CurrentResult |= ParseResult.InvalidFileFormat;
				errors.AppendLine(ex.Message);
			}

			return new PopulateFromFileResult(CurrentResult, errors.ToString(), submissionDate);
		}

		static byte[] ReadFile(string filePath)
		{
			using (var fs = File.OpenRead(filePath))
			{
				var result = new byte[fs.Length];
				var bytesRead = 0;
				while (bytesRead < result.Length)
				{
					bytesRead += fs.Read(result, bytesRead, result.Length - bytesRead);
				}
				return result;
			}
		}

		ParseDataToBeProcessed Parse(string filename, byte[] data, bool processParsedData, bool isEmailWithoutAttachment, string body = "")
		{
			IApplicantResumeParseResult parsedResult = null;
			try
			{
				var documentType = GetDocumentType(filename, isEmailWithoutAttachment);
				parsedResult = ParseCore(filename, data, documentType);
				var resume = parsedResult.ParsedResume;

				if (resume != null)
				{
					if (!IsLikelyCoverLetter(filename))
					{
						if (resume.EmailAddress.IsEmpty)
						{
							var suggestedEmail = TryExtractMissingData(body, (NoResString)"\"mailto:", "\"");
							if (!suggestedEmail.IsEmpty && EmailAddressValidation.IsEmailAddressValid(suggestedEmail))
							{
								resume.EmailAddress = suggestedEmail;
							}
						}

						if (resume.Mobile.IsEmpty && resume.HomePhone.IsEmpty)
						{
							var suggestedPhone = TryExtractMissingData(body, (NoResString)"Phone: ", (NoResString)"</td>");
							if (!suggestedPhone.IsEmpty && suggestedPhone.Trim('+', '(', ')', ' ', '-').IsNumbersOnlyOrEmpty)
							{
								resume.Mobile = suggestedPhone;
							}
						}
					}

					var result = new ParseDataToBeProcessed(true, filename, data, isEmailWithoutAttachment, resume);
					if (processParsedData)
					{
						ProcessParsedData(new List<ParseDataToBeProcessed>() { result });
					}
					return result;
				}
			}
			catch (TimeoutException)
			{
				CurrentResult |= ParseResult.Timeout;
				return new ParseDataToBeProcessed(false, parsedResult?.Message ?? ZString.Empty);
			}

			return new ParseDataToBeProcessed(false, parsedResult?.Message ?? ZString.Empty);
		}

		//TODO: remove once Daxtra fixes their eml processing
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String indexing")]
		ZString TryExtractMissingData(string body, string refTag, string endStr)
		{
			var indexContactInfo = body?.IndexOf("Contact Information", StringComparison.OrdinalIgnoreCase);
			if (indexContactInfo > 0)
			{
				var index = body.IndexOf(refTag, indexContactInfo.Value, StringComparison.OrdinalIgnoreCase);
				if (index > 0)
				{
					int start = index + refTag.Length;
					int end = body.IndexOf(endStr, start, StringComparison.OrdinalIgnoreCase);
					if (end > start)
					{
						return body.Substring(start, end - start).Trim();
					}
				}
			}

			return ZString.Empty;
		}

		protected virtual IApplicantResumeParseResult ParseCore(string filename, byte[] data, string documentType)
		{
			var parseResult = ResumeParser.Parse(filename, data);

			if (parseResult.Status == ApplicantResumeParseStatus.Success)
			{
				var parsedResume = parseResult.ParsedResume;
				CreateDocument(documentType, parsedResume.ResumeXml);
			}

			return parseResult;
		}

		protected void CreateDocument(string documentType, ZString resumeXml)
		{
			if (Parent != null)
			{
				var newDoc = Parent.Documents.AddNew();
				newDoc.HPD_Type = documentType;
				newDoc.HPD_Content = resumeXml;
			}
		}

		protected virtual void ProcessParsedData(List<ParseDataToBeProcessed> results)
		{
			if (Parent == null)
			{
				ParseDataToBeProcessed resume = null;

				var resumeFiles = results.Where(r => r.IsParsed && (r.IsResume || r.IsEmailWithoutAttachment)).ToList();
				if (resumeFiles.Count > 0)
				{
					if (resumeFiles.Count > 1)
					{
						resume = resumeFiles.FirstOrDefault(r => !string.IsNullOrEmpty(r.ApplicantEmailAddress));
					}

					if (resume == null)
					{
						resume = resumeFiles[0];
					}
				}

				if (resume != null)
				{
					MatchApplication(resume.Filename, resume.ApplicantResume);
					resume.IsProcessed = true;
				}

				if (Parent == null)
				{
					return;
				}
			}

			foreach (var item in results)
			{
				if (!item.IsProcessed)
				{
					if ((CreateApplicantWithEmptyEmail && item.ApplicantResume != null) || (!string.IsNullOrEmpty(item.ApplicantEmailAddress) && !string.IsNullOrEmpty(item.ApplicantName)))
					{
						ProcessApplicantResume(item.ApplicantResume);
					}

					if (item.Data != null)
					{
						AttachEdoc(Parent, item.Filename, item.Data, item.IsEmailWithoutAttachment);
					}
					item.IsProcessed = true;
				}
			}
		}

		public static bool IsLikelyCoverLetter(string filename)
		{
			filename = Path.GetFileName(filename).ToUpperInvariant();
			string[] words = filename.Split(new[] { ' ', ',', '.', '-', '!', '?' });
			return words.Any(w => w.Equals(Res.GetString("7C771EEA-6C44-486E-B021-FCD98E2D1230", "COVER")))
				|| filename.Contains(Res.GetString("5366032D-5C25-4981-8095-38910B979234", "COVERLETTER"));
		}

		public static bool IsValidAttachmentType(string fileExtention)
		{
			fileExtention = fileExtention.ToUpperInvariant();
			if (fileExtention.StartsWith(".", StringComparison.OrdinalIgnoreCase))
			{
				fileExtention = fileExtention.Substring(1);
			}

			return RecruiterDataRegistry.Instance.DaxtraResumeAcceptedFormats.Value.ContainsCode(fileExtention);
		}

		public static void AttachEdoc(HRJobApplication application, string filename, byte[] data, bool isEmailWithoutAttachment = false)
		{
			var docType = GetDocType(application.Factory, filename, isEmailWithoutAttachment);
			filename = Path.GetFileName(filename);
			if (isEmailWithoutAttachment && string.IsNullOrEmpty(filename))
			{
				filename = application.Applicant?.Name + (NoResString)" Referring Source.eml"; // File name
			}

			application.DocManagerInfo.AddFileOrDocument(data, filename, docType, false);
		}

		static string GetDocType(BusinessObjectFactory factory, string filename, bool isEmailWithoutAttachment)
		{
			string docType;
			if (isEmailWithoutAttachment)
			{
				docType = RecruiterDataRegistry.Instance.GetDocTypeReferringSource(factory)?.RT_DocType;
			}
			else if (IsLikelyCoverLetter(filename))
			{
				docType = RecruiterDataRegistry.Instance.GetDocTypeCoverLetter(factory)?.RT_DocType;
			}
			else
			{
				docType = RecruiterDataRegistry.Instance.GetDocTypeCV(factory)?.RT_DocType;
			}

			if (string.IsNullOrEmpty(docType))
			{
				docType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			}
			return docType;
		}

		public void ParseMailItem(MailItem mailItem)
		{
			ParseMailItemCore(new EmailData(mailItem), null);
		}

		List<ParseDataToBeProcessed> ParseIMail(MimeMessage message, HRRecruitmentJobCampaign jobOpening, string filename = null)
		{
			return ParseMailItemCore(new EmailData(message), jobOpening, filename);
		}

		List<ParseDataToBeProcessed> ParseIMail(Storage.Message message, HRRecruitmentJobCampaign jobOpening, string filename = null)
		{
			return ParseMailItemCore(new EmailData(message), jobOpening, filename);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual List<ParseDataToBeProcessed> ParseMailItemCore(EmailData emailData, HRRecruitmentJobCampaign jobOpening, string filename = null)
		{
			var resumeParsed = false;
			var parseResults = new List<ParseDataToBeProcessed>();
			var rule = GetEmailParsingRule(emailData.SenderEmailAddress);

			if (rule.AllowParseAttachments)
			{
				foreach (var attachment in emailData.MailAttachments.Where(x => x.Size > 0 && x.HasFileName
					&& IsValidAttachmentType(Path.GetExtension(x.FileName))).OrderByDescending(x => x.Size).ThenBy(x => x.FileName))
				{
					var parsedData = Parse(attachment.FileName, attachment.Data, false, false, (rule.AllowFallbackToEmailBody ? emailData.Body : string.Empty));
					if (parsedData.IsParsed && parsedData.IsResume)
					{
						resumeParsed = true;
					}
					parseResults.Add(parsedData);
				}

				if (rule.AllowFallbackToEmailBody && (!resumeParsed || !parseResults.Any(x => x.HasMinimumDataRequirements)))
				{
					var parsedData = Parse(filename ?? string.Empty, emailData.EmlNoAttachments, false, true, emailData.Body);
					if (parsedData.IsParsed)
					{
						resumeParsed = true;
					}
					parseResults.Add(parsedData);
				}

				ProcessParsedData(parseResults);
			}

			if (!resumeParsed && parseResults.Any())
			{
				CurrentResult |= ParseResult.NoResumeParsed;
			}

			if (Parent != null)
			{
				if (jobOpening != null)
				{
					Parent.HP_HV = jobOpening.PK;
				}
				else if (AllowMatchJobOpening && Parent.HP_HV.IsEmpty && !string.IsNullOrWhiteSpace(emailData.Subject))
				{
					MatchJobOpening(emailData.Subject, emailData.Date ?? ZDateTime.Today);
				}

				if (Parent.HP_OH_ReferringOrganisation.IsEmpty && Parent.HP_PER_ReferringPerson.IsEmpty)
				{
					Parent.SetupReferringSource(emailData.SenderEmailAddress);
				}
			}

			return parseResults;
		}

		void ProcessApplicantResume(IApplicantResume resume)
		{
			var applicant = Parent.Applicant;
			if (applicant == null)
			{
				applicant = MatchApplicant(Factory, resume);
				if (applicant == null)
				{
					applicant = Factory.New<HRJobApplicant>();
				}

				Parent.HP_HA = applicant.PK;
			}

			SetApplicantValues(resume, applicant);
		}

		public static void SetApplicantValues(IApplicantResume resume, HRJobApplicant applicant)
		{
			SetValue(resume.Name.IsEmpty ? (ZString)GlbPerson.EmptyFullName : resume.Name.Trim(), applicant.HA_FullName.IsEmpty, (x) => applicant.HA_FullName = x.SubstringSafe(0, GlbPersonSchema.PER_FullName.MaxLength));
			SetValue(resume.Gender.Trim(), (applicant.HA_Gender.IsEmpty || applicant.HA_Gender == "N"), (x) => applicant.HA_Gender = x.SubstringSafe(0, GlbPersonSchema.PER_Gender.MaxLength));
			SetValue(resume.Nationality.Trim(), applicant.HA_RN_NKNationalityCodeISO.IsEmpty, (x) => applicant.HA_RN_NKNationalityCodeISO = x.SubstringSafe(0, GlbPersonSchema.PER_RN_NKNationalityCodeISO.MaxLength));
			SetValue(resume.Mobile.Trim(), applicant.HA_MobilePhone.IsEmpty, (x) => applicant.HA_MobilePhone = x.SubstringSafe(0, GlbPersonSchema.PER_MobilePhone.MaxLength));
			if (applicant.HA_MobilePhone.IsEmpty)
			{
				SetValue(resume.HomePhone.Trim(), applicant.HA_MobilePhone.IsEmpty, (x) => applicant.HA_MobilePhone = x.SubstringSafe(0, GlbPersonSchema.PER_MobilePhone.MaxLength));
			}
			else
			{
				SetValue(resume.HomePhone.Trim(), applicant.HA_HomePhone.IsEmpty, (x) => applicant.HA_HomePhone = x.SubstringSafe(0, GlbPersonSchema.PER_HomePhone.MaxLength));
			}
			SetValue(resume.Address1.Trim(), applicant.HA_UserAddress1.IsEmpty, (x) => applicant.HA_UserAddress1 = x.SubstringSafe(0, GlbPersonSchema.PER_HomeAddress1.MaxLength));
			SetValue(resume.Address2.Trim(), applicant.HA_UserAddress2.IsEmpty, (x) => applicant.HA_UserAddress2 = x.SubstringSafe(0, GlbPersonSchema.PER_HomeAddress2.MaxLength));
			SetValue(resume.City.Trim(), applicant.HA_City.IsEmpty, (x) => applicant.HA_City = x.SubstringSafe(0, GlbPersonSchema.PER_City.MaxLength));
			SetValue(resume.Postcode.Trim(), applicant.HA_Postcode.IsEmpty, (x) => applicant.HA_Postcode = x.SubstringSafe(0, GlbPersonSchema.PER_Postcode.MaxLength));
			SetValue(resume.State.Trim(), applicant.HA_State.IsEmpty, (x) => applicant.HA_State = x.SubstringSafe(0, GlbPersonSchema.PER_State.MaxLength));
			SetValue(resume.Country.Trim(), applicant.HA_RN_NKCountry.IsEmpty, (x) => applicant.HA_RN_NKCountry = x.SubstringSafe(0, GlbPersonSchema.PER_RN_NKCountry.MaxLength));

			if (!resume.EmailAddress.IsEmpty && applicant.HA_EmailAddress.IsEmpty)
			{
				applicant.HA_EmailAddress = resume.EmailAddress; //Since we are Validating length for HA_EmailAdress While Setting the Property, trimming is also done while setting it.
			}
		}

		static void SetValue(ZString source, bool checker, Action<ZString> setter)
		{
			if (!source.IsEmpty && checker)
			{
				setter(source);
			}
		}

		void MatchJobOpening(ZString emailSubject, ZDateTime receivedDate)
		{
			/*
			subject samples:
			Application for Senior Commercial Analyst from Kiki JI CPA
			Respond Now: HUY NGUYEN applied to Software Developer on Glassdoor
			Application received for Website Designer
			*/

			var jobOpenings = FindMatchingJobOpenings(emailSubject, receivedDate, Factory);
			if (jobOpenings.Length == 0)
			{
				return;
			}

			if (jobOpenings.Length == 1)
			{
				Parent.HP_HV = jobOpenings.Single().PK;
			}
			else if (jobOpenings.Length > 1)
			{
				CurrentResult |= ParseResult.MultipleJobOpeningsFound;
			}
		}

		public static HRJobApplicant MatchApplicant(BusinessObjectFactory factory, IApplicantResume resume)
		{
			HRJobApplicant applicant = null;
			if (!string.IsNullOrEmpty(resume.EmailAddress))
			{
				applicant = factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, resume.EmailAddress));
			}

			if (applicant == null && !string.IsNullOrEmpty(resume.Mobile))
			{
				var resumeMobileTrimmed = resume.Mobile.Trim().Replace(" ", "");
				var filter = new ZDBOnlyQuery(typeof(GlbPerson));
				var sql = "REPLACE(PER_MobilePhone, ' ', '') = '" + resumeMobileTrimmed + "'";
				filter.AddFilterAndZSQLParameterCollection(sql, null);
				var matchingPersons = factory.Load<GlbPerson>(filter);
				applicant = MatchApplicantFromPersons(matchingPersons);
			}

			if (applicant == null && !string.IsNullOrEmpty(resume.HomePhone))
			{
				var matchingPersons = factory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_HomePhone, resume.HomePhone));
				applicant = MatchApplicantFromPersons(matchingPersons);
			}

			return applicant;

			HRJobApplicant MatchApplicantFromPersons(GlbPerson[] matchingPersons)
				=> (HRJobApplicant)matchingPersons
					.SelectMany(person => person.ApplicantCollection)
					.FirstOrDefault();
		}

		void MatchApplication(string filename, IApplicantResume resume)
		{
			if (Parent == null && MailItem != null)
			{
				var zipFileName = ZGuid.Empty + ApplicationDocumentsUpdater.Separator + filename;
				var dataFromZip = new ApplicationDocumentsUpdater.DataFromZip(zipFileName, ZGuid.Empty, Encoding.UTF8.GetBytes(resume.ResumeXml));
				Parent = new ApplicationDocumentsUpdater(ResumeParser).ProcessParsedDataCore(MailItem, dataFromZip, resume, false, CreateApplicantWithEmptyEmail, AllowMatchJobOpening);

				if (Parent != null)
				{
					IsNewApplication = !Parent.IsInDatabase;
				}
			}
		}

		public static HRRecruitmentJobCampaign[] FindMatchingJobOpenings(ZString emailSubject, ZDateTime receivedDate, BusinessObjectFactory factory)
		{
			string adTitle = GetAdTitleFromSubject(emailSubject);
			if (!string.IsNullOrWhiteSpace(adTitle))
			{
				var query = new ZQuery(HRRecruitmentJobCampaignSchema.HV_AdTitle, adTitle);
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, receivedDate);
				query.AddToFilter(HRRecruitmentJobCampaignSchema.HV_CampaignEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, receivedDate.AddDays(-14));

				var openings = factory.Load<HRRecruitmentJobCampaign>(query);

				var openingsNoGrace = openings.Where(o => o.HV_CampaignEndDate >= receivedDate);
				return openingsNoGrace.Any() ? openingsNoGrace.ToArray() : openings;
			}

			return Array.Empty<HRRecruitmentJobCampaign>();
		}

		public static string GetAdTitleFromSubject(ZString emailSubject)
		{
			return Regex.Matches(emailSubject, RecruiterDataRegistry.Instance.DaxtraSubjectRegex.Value, RegexOptions.IgnoreCase)
								.OfType<Match>().FirstOrDefault()?.Groups.OfType<Group>()
								.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value) && x.Name.StartsWith("adTitle", StringComparison.OrdinalIgnoreCase))?.Value;
		}

#if DEBUG
		protected
#endif
		ParseResult CurrentResult = ParseResult.None;

		public static string ProcessFileResultMessage(PopulateFromFileResult result)
		{
			var sb = new ZStringBuilder();
			if (result.ParseResult.HasFlag(ParseResult.Timeout))
			{
				sb.AppendLine(Res.GetString("1BB6F6EC-BFE1-4CA8-B81F-7C61E3C2416F", "Timeout expired, please try again later."));
			}

			if (result.ParseResult.HasFlag(ParseResult.InvalidFileFormat))
			{
				sb.AppendLine(Res.GetString("5E746D06-04B1-55F2-B0C4-E40143205CEA", "The file cannot be parsed, please check it."));
			}

			if (result.ParseResult.HasFlag(ParseResult.MultipleJobOpeningsFound))
			{
				sb.AppendLine(Res.GetString("9017498B-F43B-4BB4-B05D-08ED91A75F90", "Multiple matching job openings found."));
			}

			if (result.ParseResult.HasFlag(ParseResult.BadFormat))
			{
				sb.AppendLine(Res.GetString("2DDF5B08-4161-448A-B897-A5FB24FD3B95", "Attachment is malformed."));
			}

			if (result.ParseResult.HasFlag(ParseResult.NoResumeParsed))
			{
				sb.AppendLine(Res.GetString("720DE8E9-0DCE-4AFE-B6A5-2039B101A968", "Could not parse the resume."));
			}

			if (!string.IsNullOrEmpty(result.Error))
			{
				sb.AppendLine(Res.GetString("88D2F71D-2F18-4177-91FC-FE1FA7714EB0", "Error: {0}", result.Error));
			}
			return sb.ToString().TrimEnd();
		}

		public static bool TryParseSenderEmailAddress(ZString fileName, out ZString senderEmailAddress)
		{
			senderEmailAddress = "";

			try
			{
				if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
				{
					var fileExtension = Path.GetExtension(fileName).ToUpperInvariant();
					if (MsgExt.Equals(fileExtension))
					{
						using (var message = new Storage.Message(fileName))
						{
							senderEmailAddress = message?.Sender.Email ?? string.Empty;
						}
					}
					else if (EmlExt.Equals(fileExtension))
					{
						var message = MimeMessage.Load(fileName);
						senderEmailAddress = message.GetSenderOrFrom()?.Address ?? string.Empty;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return !senderEmailAddress.IsEmpty;
		}

		public static EmailParsingRule GetEmailParsingRule(ZString senderEmailAddress)
		{
			EmailParsingRule rule = null;
			var configurations = RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.Value.Cast<ReferringPartyConfiguration>();
			var rules = RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.Value.Cast<EmailParsingRule>();

			var configuration = configurations.FirstOrDefault(x => x.Domain.EqualsIgnoringCase(senderEmailAddress)) ??
					configurations.FirstOrDefault(x => senderEmailAddress.EndsWith(x.Domain, StringComparison.OrdinalIgnoreCase));

			if (configuration != null)
			{
				rule = rules.FirstOrDefault(x => x.ReferringPartyCode.EqualsIgnoringCase(configuration.ReferringParty));
			}

			return rule ?? rules.First(x => x.ReferringPartyCode.EqualsIgnoringCase(EmailParsingRuleReferringParties.Codes.Unknown)) ?? new EmailParsingRule();
		}

		static string GetDocumentType(string filename, bool isEmailWithoutAttachment)
		{
			string documentType;
			if (isEmailWithoutAttachment)
			{
				documentType = ApplicationDocumentTypes.Codes.ReferringSource;
			}
			else
			{
				documentType = IsLikelyCoverLetter(filename) ? ApplicationDocumentTypes.Codes.CoverLetter : ApplicationDocumentTypes.Codes.Resume;
			}
			return documentType;
		}

#if DEBUG
		public void SetParentForTest(HRJobApplication jobApplication)
		{
			Parent = jobApplication;
			IsNewApplication = true;
		}
#endif

		#region Class

		public class PopulateFromFileResult
		{
			public PopulateFromFileResult(ParseResult result, string error, DateTime submissionDate)
			{
				ParseResult = result;
				Error = error;
				SubmissionDate = submissionDate;
			}

			public ParseResult ParseResult { get; }
			public string Error { get; }
			public DateTime SubmissionDate { get; }
		}

		public class ParseDataToBeProcessed
		{
			public ParseDataToBeProcessed(bool parsed, string error)
			{
				IsParsed = parsed;
				Error = error;
			}

			public ParseDataToBeProcessed(bool isParsed, string filename, byte[] data, bool isEmailWithoutAttachment, IApplicantResume applicantResume)
			{
				IsParsed = isParsed;
				Filename = filename;
				Data = data;
				IsEmailWithoutAttachment = isEmailWithoutAttachment;
				ApplicantResume = applicantResume;

				if (!string.IsNullOrEmpty(Filename))
				{
					IsResume = !IsLikelyCoverLetter(Filename);
				}

				if (ApplicantResume != null)
				{
					ApplicantEmailAddress = ApplicantResume.EmailAddress;
					ApplicantName = ApplicantResume.Name;
				}
			}

			public bool IsParsed { get; }

			public string Filename { get; set; }

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public byte[] Data { get; }

			public bool IsEmailWithoutAttachment { get; }

			public bool IsResume { get; }

			public IApplicantResume ApplicantResume { get; }

			public string ApplicantEmailAddress { get; }

			public string ApplicantName { get; }

			public bool IsProcessed { get; set; }

			public bool HasNameAndEmail => ApplicantResume != null && !ApplicantResume.Name.IsEmpty && !ApplicantResume.EmailAddress.IsEmpty;

			public bool HasMinimumDataRequirements => HasNameAndEmail && (!ApplicantResume.Mobile.IsEmpty || !ApplicantResume.HomePhone.IsEmpty);

			public string Error { get; set; }
		}

		public class EmailData
		{
			public EmailData(Storage.Message message)
			{
				Subject = message.Subject;
				Date = message.Headers?.DateSent.ToLocalTime();
				SenderEmailAddress = message.Sender?.Email;
				Body = (message.BodyHtml ?? message.BodyText ?? message.BodyRtf)?.Replace("=\r\n", "");

				MailAttachments = new List<Attachments>();
				if (message.Attachments.Count > 0)
				{
					foreach (var attachment in message.Attachments.Select(x => x as Storage).ToList())
					{
						var fileData = attachment.GetData();
						var fileName = attachment.GetName();
						MailAttachments.Add(new Attachments(fileName, fileData, fileData.Length, !string.IsNullOrEmpty(fileName)));
						message.DeleteAttachment(attachment);
					}
				}

				EmlNoAttachments = message.GetData();
			}

			public EmailData(MimeMessage message)
			{
				Subject = message.Subject;
				Date = message.Date.LocalDateTime;
				var senderMailbox = message.GetSenderOrFrom();
				SenderEmailAddress = senderMailbox?.Address;
				Body = (message.HtmlBody ?? message.TextBody)?.Replace("=\r\n", "");

				MailAttachments = new List<Attachments>();
				var attachments = new List<MimeEntity>();
				var multiParts = new List<Multipart>();
				var mimeIterator = new MimeIterator(message);

				while (mimeIterator.MoveNext())
				{
					var multiPart = mimeIterator.Parent as Multipart;
					var mimeEntity = mimeIterator.Current;
					if (multiPart != null && (mimeEntity is MessagePart || mimeEntity?.ContentDisposition?.FileName != null || mimeEntity?.ContentType?.Name != null))
					{
						multiParts.Add(multiPart);
						attachments.Add(mimeIterator.Current);
					}
				}

				for (var i = 0; i < attachments.Count; i++)
				{
					var attachment = attachments[i];
					var fileName = attachment.GetName();
					var fileData = attachment.GetData();
					MailAttachments.Add(new Attachments(fileName, fileData, fileData.Length, !string.IsNullOrEmpty(fileName)));
					multiParts[i].Remove(attachment);
				}

				EmlNoAttachments = message.GetData();
			}

			public EmailData(MailItem mailItem)
			{
				Subject = mailItem.MI_Subject;
				Date = mailItem.MI_ReceivedDateTime.ToDateTime();
				SenderEmailAddress = mailItem.GetFromEmailAddress();
				Body = mailItem.MI_Body;

				MailAttachments = new List<Attachments>();
				if (mailItem.MailAttachments.Count > 0)
				{
					foreach (MailAttachment item in mailItem.MailAttachments)
					{
						MailAttachments.Add(new Attachments(item.MA_FileName, item.MA_Data, item.MA_Data.Length, true));
					}
				}

				var mailItemInOtherFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<MailItem>(mailItem.PK);
				mailItemInOtherFactory.MailAttachments.RemoveAll();

				EmlNoAttachments = BusinessObjectEmailAttacher.BuildMessage(mailItemInOtherFactory);
			}

			public string Subject { get; }
			public string SenderEmailAddress { get; }
			public DateTime? Date { get; }
			public List<Attachments> MailAttachments { get; }
			public string Body { get; }

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public byte[] EmlNoAttachments { get; }

			public class Attachments
			{
				public Attachments(string fileName, byte[] data, int size, bool hasFileName)
				{
					FileName = fileName;
					Data = data;
					Size = size;
					HasFileName = hasFileName;
				}

				public string FileName { get; set; }

				[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
				public byte[] Data { get; set; }
				public int Size { get; set; }
				public bool HasFileName { get; set; }
			}
		}

		#endregion
	}
}
