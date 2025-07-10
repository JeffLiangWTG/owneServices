using System;
using CargoWise.RefDbRepo.NZReferenceData.CmdLine;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests.Programs
{
	[TestFixture]
	sealed class NZProgramTest
	{
		[Test]
		public void TestSendMessage()
		{
			var program = new NZProgramForTest();
			program.Run();
			Assert.That(program.EmailServiceForTest.SentEmailSubject, Is.EqualTo("Reference Data Failure: Test Program"));
			Assert.True(program.EmailServiceForTest.SentEmailIsHtml);
			var emailBodyLines = program.EmailServiceForTest.SentEmailBody.Split(Environment.NewLine);
			//Assert.That(emailBodyLines, Has.Length.EqualTo(19));
			Assert.That(emailBodyLines[2].StartsWith("<tr><td>ID</td><td>"));
			Assert.That(emailBodyLines[3], Is.EqualTo("<tr><td>Message</td><td>Additional&nbsp;Message</td></tr>"));
			Assert.That(emailBodyLines[4], Is.EqualTo("<tr><td colspan=\"2\">No Relevent Exception</td></tr>"));

			Assert.That(emailBodyLines[8], Is.EqualTo("<tr><td>Message</td><td>Exception&nbsp;throwed&nbsp;during&nbsp;running&nbsp;program&nbsp;Test&nbsp;Program</td></tr>"));
			Assert.That(emailBodyLines[9], Is.EqualTo("<tr><td>Exception Message</td><td>Test&nbsp;Exception</td></tr>"));
			Assert.That(emailBodyLines[10].StartsWith("<tr><td>Exception Stack</td><td>&nbsp;&nbsp;&nbsp;at&nbsp;CargoWise.RefDbRepo.NZReferenceData.Tests.Programs.NZProgramTest.NZProgramForTest.RunCore()"), "StackTrace");

			Assert.That(emailBodyLines[13].EndsWith("<td>Info</td><td>Log&nbsp;Info</td></tr>"), "Logs[0]");
			Assert.That(emailBodyLines[14].EndsWith("<td>Error</td><td>Log&nbsp;Error</td></tr>"), "Logs[1]");
			Assert.That(emailBodyLines[15].Contains("<td>Error</td><td>Exception&nbsp;throwed&nbsp;during&nbsp;running&nbsp;program,&nbsp;relevent&nbsp;message:"), "Logs[2]");
		}

		class NZProgramForTest : NZProgram
		{
			public EmailServiceForTest EmailServiceForTest { get; set; } = new EmailServiceForTest();

			public override string ProgramName => "Test Program";

			protected override void RunCore() {
				Logger.LogInfo("Log Info");
				Logger.LogError("Log Error");
				AddSendToEmailMessage("Additional Message", null);
				throw new Exception("Test Exception");
			}

			protected override IEmailService GetEmailService() => EmailServiceForTest;
		}

		class EmailServiceForTest : IEmailService
		{
			public string SentEmailSubject { get; set; }

			public string SentEmailBody { get; set; }

			public bool SentEmailIsHtml { get; set; }

			public void SendEmail(string subject, string body, bool isHtmlBody)
			{
				SentEmailSubject = subject;
				SentEmailBody = body;
				SentEmailIsHtml = isHtmlBody;
			}
		}
	}
}
