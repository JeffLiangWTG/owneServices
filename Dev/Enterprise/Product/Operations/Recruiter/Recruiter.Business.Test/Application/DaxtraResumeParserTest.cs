using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Recruiter.Integration;
using NLog;
using NLog.Targets;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class DaxtraResumeParserTest : TestCaseWithFactory
	{
		public void TestDaxtraServiceFailsConsideredError()
		{
			var parser = new DaxtraResumeParserForTest();
			parser.GetDataError = "kaboom";
			var result = parser.GetData("token");
			AssertEquals(ApplicantBatchGetDataStatus.Error, result.Status);
			AssertEquals("kaboom", result.Error);
		}

		public void TestGetDataServiceNotReady()
		{
			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var parser = new DaxtraResumeParser();
				var result = parser.GetData("1211");
				AssertEquals("The parsing service is not ready.", result.Error);
				AssertEquals(ApplicantBatchGetDataStatus.Error, result.Status);
			}
		}

		public void TestParseServiceNotReady()
		{
			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var parser = new DaxtraResumeParser();
				var result = parser.Parse("1211", new byte[] { 1 });
				AssertEquals("The parsing service is not ready.", result.Message);
				AssertEquals(ApplicantResumeParseStatus.Fail, result.Status);
			}
		}

		public void TestSendBatchServiceNotReady()
		{
			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var parser = new DaxtraResumeParser();
				var result = parser.SendBatch(new byte[] { 1 });
				AssertEquals("The parsing service is not ready.", result.Message);
				AssertEquals(ApplicantResumeParseStatus.Fail, result.Status);
			}
		}

		public void TestReadFromXml()
		{
			var parser = new DaxtraResumeParser();
			var xmlText = resourceRetriever.Value.GetString("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio resume.xml");
			var result = parser.ReadFromXml(xmlText);
			Assert(!result.ResumeXml.Contains("<?xml version='1.0' encoding='utf-8'?>"));
		}

		public void TestReadFromXml_EmailFromSpan()
		{
			var parser = new DaxtraResumeParser();
			var xmlText = resourceRetriever.Value.GetString("Enterprise.Recruiter.Business.Testing.Application.TestFiles.resume_nointernetemailaddress.xml");
			var result = parser.ReadFromXml(xmlText);

			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(result.EmailAddress);
				AssertEquals("testexample@example.com", result.EmailAddress);
			});
		}

		public void TestLogsContainHash()
		{
			var parser = new DaxtraResumeParser(loggerProvider);
			var toHash = new byte[] { 1, 2, 3 };
			parser.Log(toHash, new ApplicantResume(), hasError: false);

			var target = LogManager.Configuration.FindTargetByName<DebugTarget>(GetType().Name);
			AssertContains("resume parsed: \"039058C6F2C0CB492C533B0A4D14EF77CC0F78ABCCCED5287D84A1A2011CFB81\"", target.LastMessage);
		}

		public void TestLogsContainAllStringResumeFieldsExceptXml()
		{
			var parser = new DaxtraResumeParser(loggerProvider);
			var resume = new ApplicantResume()
			{
				ResumeXml = "xml",
				Name = "Shania Twain",
				Gender = "N",
				Nationality = "Canadian",
				EmailAddress = "shania@twain.com",
				Mobile = "04123666666",
				HomePhone = "98765432",
				Address1 = "41 Dont Impress Me Much Rd",
				Address2 = "Beverly Hills CA 90210",
				City = "Toronto",
				Postcode = "YYZ",
				State = "Ontario",
				Country = "Canada",
				ReceivedTime = DateTime.UtcNow,
			};
			parser.Log(Array.Empty<byte>(), resume, hasError: false);

			var target = LogManager.Configuration.FindTargetByName<DebugTarget>(GetType().Name);
			AssertContains("fields retrieved: \"Name, Gender, Nationality, EmailAddress, Mobile, HomePhone, Address1, Address2, City, Postcode, State, Country\"", target.LastMessage);
		}

		public void TestLogsContainNonEmptyResumeFieldsOnly()
		{
			var parser = new DaxtraResumeParser(loggerProvider);
			var resume = new ApplicantResume()
			{
				Name = "Shania Twain",
				Mobile = "04123666666",
				State = "Ontario",
			};
			parser.Log(Array.Empty<byte>(), resume, hasError: false);

			var target = LogManager.Configuration.FindTargetByName<DebugTarget>(GetType().Name);
			AssertContains("fields retrieved: \"Name, Mobile, State\"", target.LastMessage);
		}

		public void TestNoFieldsParsed()
		{
			var parser = new DaxtraResumeParser(loggerProvider);
			parser.Log(Array.Empty<byte>(), new ApplicantResume(), hasError: false);

			var target = LogManager.Configuration.FindTargetByName<DebugTarget>(GetType().Name);
			AssertContains("fields retrieved: \"\";", target.LastMessage);
		}

		public void TestLogsContainLoggedInUserCode()
		{
			var parser = new DaxtraResumeParser(loggerProvider);
			parser.Log(Array.Empty<byte>(), new ApplicantResume(), hasError: false);

			var target = LogManager.Configuration.FindTargetByName<DebugTarget>(GetType().Name);
			AssertContains("User: \"E\"", target.LastMessage);
		}

		public void TestErrorLogSent()
		{
			var parser = new DaxtraResumeParser(loggerProvider);
			parser.Log(Array.Empty<byte>(), new ApplicantResume(), hasError: true);

			var target = LogManager.Configuration.FindTargetByName<DebugTarget>(GetType().Name);
			AssertEquals("Error parsing resume \"E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855\"", target.LastMessage);
		}

		IHRLoggerProvider loggerProvider;

		protected override void SetUp()
		{
			base.SetUp();
			loggerProvider = new LoggerProviderForTest(GetType().Name);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			LogManager.Shutdown();
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		sealed class DaxtraResumeParserForTest : DaxtraResumeParser
		{
			public string GetDataError { get; set; }

			protected override CVXtractorServiceClient GetServiceClient(string serviceUrl, string account)
			{
				var client = new CVXtractorServiceClientForTest("service", "account");
				client.GetDataError = GetDataError;
				return client;
			}
		}

		sealed class CVXtractorServiceClientForTest : CVXtractorServiceClient
		{
			public CVXtractorServiceClientForTest(string serviceUrl, string account, bool useJsonOutput = false) : base(serviceUrl, account, useJsonOutput)
			{
			}

			public string GetDataError { get; set; }

			public override byte[] GetData(string token)
			{
				ErrorMessage = GetDataError;
				return null;
			}
		}

		sealed class LoggerProviderForTest : IHRLoggerProvider
		{
			public LoggerProviderForTest(string name)
			{
				logger = LogManager.Setup().LoadConfiguration(builder =>
				{
					builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteTo(new DebugTarget(name) { Layout = "${message}" });
				}).GetLogger(name);
			}

			readonly ILogger logger;

			public ILogger GetLogger()
			{
				return logger;
			}

			public void Dispose()
			{ }
		}
	}
}
