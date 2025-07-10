using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class ApplicantResumeParserForTest : IApplicantResumeParser
	{
		readonly bool noExceptions;
		bool failOnce;
		readonly string testFilesDir;
		readonly ZGuid itemPK;
		readonly bool failedCoversion;
		readonly bool emptyEmail;
		readonly bool emptyPhone;
		readonly bool getDataReturnsError;
		readonly bool invalidFilename;

		public ApplicantResumeParserForTest(string testFilesDir, ZGuid applicationPK, bool noExceptions = true, bool failOnce = false, bool failedCoversion = false, bool emptyEmail = false, bool emptyPhone = false, bool getDataReturnsError = false, bool invalidFilename = false)
		{
			this.testFilesDir = testFilesDir;
			this.itemPK = applicationPK;
			this.noExceptions = noExceptions;
			this.failOnce = failOnce;
			this.failedCoversion = failedCoversion;
			this.emptyEmail = emptyEmail;
			this.emptyPhone = emptyPhone;
			this.getDataReturnsError = getDataReturnsError;
			this.invalidFilename = invalidFilename;
		}

		public IApplicantResumeParseResult Parse(string resumeFileName, byte[] resumeFileContent)
		{
			if (!noExceptions)
			{
				throw new System.Exception("boom");
			}

			if (failOnce)
			{
				failOnce = false;
				return new ApplicantResumeParseResultForTest(ApplicantResumeParseStatus.Fail, null, "");
			}

			var resumeXml = Encoding.UTF8.GetString(resumeFileContent);

			return new ApplicantResumeParseResultForTest(ApplicantResumeParseStatus.Success,
				new ApplicantResume()
				{
					ResumeXml = resumeXml,
					ReceivedTime = ZDateTime.UtcNow,
					Name = "name",
					Gender = "M",
					Nationality = "AU",
					Mobile = emptyPhone ? "" : "0499702888",
					Address1 = "address1",
					Address2 = "address2",
					City = "Miranda",
					Postcode = "2228",
					State = "NSW",
					Country = "AU",
					EmailAddress = emptyEmail ? "" : "email@gmail.com",
					HomePhone = emptyPhone ? "" : "0212345678"
				}, string.Empty);
		}

		public IApplicantResume ReadFromXml(string resumeXml)
		{
			var resume = new ApplicantResume()
			{
				ResumeXml = resumeXml,
				EmailAddress = "donantonio@wisetechglobal.com",
				Mobile = "+61 444 444 4444",
				HomePhone = "02 2222 2222",
			};

			ReadFromXmlEvent?.Invoke(resume);
			return resume;
		}

		public byte[] LastBatch { get; private set; }
		public IApplicantResumeBatchSendResult SendBatch(byte[] contents)
		{
			LastBatch = contents;
			return new ApplicantResumeBatchSendResultForTest();
		}

		public IApplicantResumeBatchGetDataResult GetData(string token)
		{
			if (getDataReturnsError)
			{
				var result = new ApplicantResumeBatchGetDataResult();
				result.Error = "error";
				result.Status = ApplicantBatchGetDataStatus.Error;
				return result;
			}

			if (Tokens.Contains(token))
			{
				return new ApplicantResumeBatchGetDataResultForTest(ApplicantBatchGetDataStatus.Finished, new byte[] { 0 });
			}
			else
			{
				Tokens.Add(token);
			}

			if (GetDataEvent != null)
			{
				var cfg = GetDataEvent();
				var resume = cfg.Resume != null ? File.ReadAllBytes(testFilesDir + cfg.Resume) : null;
				var cover = cfg.Cover != null ? File.ReadAllBytes(testFilesDir + cfg.Cover) : null;
				var emailBody = cfg.EmailBody != null ? File.ReadAllBytes(testFilesDir + cfg.EmailBody) : null;

				byte[] archive = AddToZip(resume, cover, emailBody);
				return new ApplicantResumeBatchGetDataResultForTest(ApplicantBatchGetDataStatus.Zip, archive);
			}
			else
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					byte[] archive;
					if (failedCoversion)
					{
						var bytesError = resourceRetriever.GetBytes("Enterprise.Recruiter.Business.Testing.Application.TestFiles.errorreport.xml");
						archive = AddToZip(bytesError);
					}
					else
					{
						var bytesResume = resourceRetriever.GetBytes("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio resume.xml");
						var bytesCover = resourceRetriever.GetBytes("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio Cover letter.xml");
						archive = AddToZip(bytesResume, bytesCover, null);
					}
					return new ApplicantResumeBatchGetDataResultForTest(ApplicantBatchGetDataStatus.Zip, archive);
				}
			}
		}

		readonly ZipCreator zipCreator = new ZipCreator();

		bool invalidFilenameInserted;
		byte[] AddToZip(byte[] content)
		{
			byte[] result;
			using (var ms = new MemoryStream())
			using (var resumeStream = new MemoryStream(content))
			{
				string name1 = itemPK + "~~~file.xml";

				zipCreator.ZipStream(new ZipStream[] { new ZipStream(name1, resumeStream) }, ms);

				result = ms.ToArray();
			}

			return result;
		}

		byte[] AddToZip(byte[] resume, byte[] cover, byte[] emailBody)
		{
			byte[] result;
			using (var ms = new MemoryStream())
			using (var resumeStream = resume != null ? new MemoryStream(resume) : null)
			using (var coverStream = cover != null ? new MemoryStream(cover) : null)
			using (var emailBodyStream = emailBody != null ? new MemoryStream(emailBody) : null)
			{
				string name1 = itemPK + "~~~Don Antonio resume.xml";
				string name2 = itemPK + "~~~Don Antonio Cover letter.xml~~~";
				string name3 = itemPK + "~~~EmailBody.xml~~~";

				if (invalidFilename && !invalidFilenameInserted)
				{
					invalidFilenameInserted = true;
					name1 = "bibop";
				}

				zipCreator.ZipStream(new[] { (name1, resumeStream), (name2, coverStream), (name3, emailBodyStream) }
					.Where(x => x.Item2 != null).Select(x => new ZipStream(x.Item1, x.Item2)), ms);

				result = ms.ToArray();
			}

			return result;
		}

		public delegate void ReadFromXmlEventHandler(ApplicantResume applicantResume);
		public event ReadFromXmlEventHandler ReadFromXmlEvent;

		public delegate (string Resume, string Cover, string EmailBody) GetDataEventHandler();
		public event GetDataEventHandler GetDataEvent;
		readonly HashSet<string> Tokens = new HashSet<string>();
	}

	class ApplicantResumeBatchSendResultForTest : IApplicantResumeBatchSendResult
	{
		public ApplicantResumeParseStatus Status => ApplicantResumeParseStatus.Success;

		public string Message => "";

		public string Token { get; private set; } = ZGuid.NewZGuid().ToStringKey();
	}

	class ApplicantResumeBatchGetDataResultForTest : IApplicantResumeBatchGetDataResult
	{
		public ApplicantResumeBatchGetDataResultForTest(ApplicantBatchGetDataStatus status, byte[] zip)
		{
			Status = status;
			ZipContent = zip;
		}

		public ApplicantBatchGetDataStatus Status { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] ZipContent { get; }

		public string Error => "";
	}

	class ApplicantResumeParseResultForTest : IApplicantResumeParseResult
	{
		public ApplicantResumeParseResultForTest(ApplicantResumeParseStatus status, ApplicantResume resume, string message)
		{
			this.status = status;
			this.resume = resume;
			this.message = message;
		}

		readonly ApplicantResumeParseStatus status;
		readonly ApplicantResume resume;
		readonly string message;

		public ApplicantResumeParseStatus Status => status;
		public IApplicantResume ParsedResume => resume;
		public string Message => message;
	}
}
