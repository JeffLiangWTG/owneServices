using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using Moq;
using NUnit.Framework;

namespace Enterprise.BatchProcessor.Customs.Testing
{
	public class NewBaseInterchangeRetrieverTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestProcessExceptionWithPotentialRetryWithCryptoUtilitiesException()
		{
			var newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ThrowCryptoUtilitiesException = true;
			retriever.SetSendCryptoExceptionToPostMaster(true);

			AssertNoExceptionThrown(() => retriever.ExecuteBatch());

			var hasFailedSendlog = retriever.Logger.Logs.Any(log => log.Message.Contains(@"Email (Subject: 'Decrypt/decode error while processing eMail') This email could not be delivered to the requested group Notification -> Company Notification Group. Please add a member with an email address to this group or specify an email address on an existing member."));
			Assert("logger should has failed send log.", hasFailedSendlog);

			AssertEquals("Should retry three times.", 3, retriever.ExposedPKsOfMailItemsThatCausedExceptions[newMail.PK]);
			AssertCollectionContains("Should mark as processed.", newMail.PK, retriever.ExposedPKsOfMailItemsThatProcessed);
			newMail.Reload();
			AssertEquals("MailStatus", MailStatus.Failed, newMail.MI_Status);
		}

		public void TestProcessMultipleEmailsWhichCountIsGreaterThanNumberToRetrieveAtATime()
		{
			var retriever = new TestHelperNewBaseInterchangeRetriever() { NumberToRetrieveAtATimeOverride = 3 };
			var emailPks = new List<ZGuid>();

			for (int i = 0; i < 10; i++)
			{
				emailPks.Add(CreateIncomingMailItem().PK);
				Factory.Save();
			}

			retriever.ExecuteBatch();

			var emails = NewFactory().Load<MailItem>(new ZQuery(MailDBItemsSchema.PK, emailPks));
			Assert("All emails should processed without any exceptions.", emails.All(c => c.MI_Status == MailStatus.Processed));
		}

		public void TestEmailWhoseRetrieverReturnedNoInterchangeTextDoesNotRemainsQueuedButIsUpdatedToFailed()
		{
			TestHelperNewBaseInterchangeRetrieverThatReturnsNoInterchangeText retrieveWithEmptyInterchangeText = new TestHelperNewBaseInterchangeRetrieverThatReturnsNoInterchangeText();
			MailItem newMail = CreateIncomingMailItem();
			newMail.MI_Body = "irrelevant";
			Factory.Save();
			retrieveWithEmptyInterchangeText.ExecuteBatch();
			newMail.Reload();
			AssertEquals(MailStatus.Failed, newMail.MI_Status);
			AssertContains("Derived interchange text was empty, ignoring", retrieveWithEmptyInterchangeText.Logger.DebugLogStrings[0]);
		}

		public void TestEmailWhoseRetrieverReturnedNoInterchangeTextButIsAloowedRemainsQueued()
		{
			TestHelperNewBaseInterchangeRetrieverThatReturnsNoInterchangeText retrieveWithEmptyInterchangeText = new TestHelperNewBaseInterchangeRetrieverThatReturnsNoInterchangeText();
			retrieveWithEmptyInterchangeText.ShouldIgnoreEmptyInterchangeTextExposed = true;
			MailItem newMail = CreateIncomingMailItem();
			newMail.MI_Body = "irrelevant";
			Factory.Save();
			retrieveWithEmptyInterchangeText.ExecuteBatch();
			newMail.Reload();
			AssertEquals(MailStatus.Queued, newMail.MI_Status);
			AssertNotContains("Derived interchange text was empty, ignoring", retrieveWithEmptyInterchangeText.Logger.DebugLogStrings[0]);
		}

		public void TestClearTextEmailBodyDoesntCauseAnException()
		{
			MailItem newMail = CreateIncomingMailItem();
			newMail.MI_Body = "INTERCHANGESTRING";
			Factory.Save();
			retriever.ExecuteBatch();
			newMail.Reload();
			AssertEquals(MailStatus.Processed, newMail.MI_Status);
		}

		public void TestInvalidDataDoesntCauseAnUnhandledException()
		{
			MailItem newMail = CreateIncomingMailItem();
			Factory.Save();
			retriever.TestingInterchangeText = true;
			retriever.TestInterchangeText = "BADINTERCHANGE";
			retriever.ExecuteBatch();
			newMail.Reload();
			AssertEquals(MailStatus.Failed, newMail.MI_Status);
			AssertContains("The interchange does not start with a UNA or UNB segment", retriever.Logger.DebugLogStrings[1]);
			AssertContains("Mail Item set to Failed.", retriever.Logger.DebugLogStrings[2]);
		}

		public void TestConcurrencyErrorDoesNotCauseException()
		{
			var newMail = CreateIncomingMailItem();
			Factory.Save();
			retriever.ForceConcurrencyError = true;
			retriever.ExecuteBatch();
			newMail.Reload();
			AssertEquals(MailStatus.Queued, newMail.MI_Status);

			retriever.ForceConcurrencyError = false;
			retriever.ExecuteBatch();
			newMail.Reload();
			AssertEquals(MailStatus.Processed, newMail.MI_Status);
		}

		public void TestMailDecodeFailureMarksMessageFailedAfterThreeAttempts()
		{
			var newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ThrowMailItemDecodeFailException = true;
			newMail.Reload();

			CombineAssertions(() =>
			{
				var expectedSource = $@"Enterprise.Customs.Business.Test, Mail Item PK = {newMail.PK}. Mail Subject = 12345. Mail Item Header = Subject: 12345
From: MI_From
Start of Mail Body:
EMAIL BODY TEXT";

				var exception = AssertExceptionThrown<MailItemDecodeFail>("Should throw the MailItemDecodeFail exceptin after three attempts.", "BLAH", () => retriever.ExecuteBatch());
				AssertEquals(expectedSource, exception.Source);

				AssertEquals("Should retry three times.", 3, retriever.ExposedPKsOfMailItemsThatCausedExceptions[newMail.PK]);
				AssertCollectionContains("Should mark as processed.", newMail.PK, retriever.ExposedPKsOfMailItemsThatProcessed);

				newMail.Reload();
				AssertEquals("MailStatus", MailStatus.Failed, newMail.MI_Status);
			});
		}

		public void TestCryptoUtilitiesExceptionFailsAfterThreeAttempts()
		{
			var newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ThrowCryptoUtilitiesException = true;

			CombineAssertions(() =>
			{
				AssertExceptionThrown<CryptoUtilitiesException>(() => retriever.ExecuteBatch());
				AssertEquals("Should retry three times.", 3, retriever.ExposedPKsOfMailItemsThatCausedExceptions[newMail.PK]);
				AssertCollectionContains("Should mark as processed.", newMail.PK, retriever.ExposedPKsOfMailItemsThatProcessed);

				newMail.Reload();
				AssertEquals("MailStatus", MailStatus.Failed, newMail.MI_Status);
			});
		}

		[TestDate(2007, 09, 09, 09, 09, 09)]
		public void TestIgnoreMessageIfItHasNoContent()
		{
			MailItem blankMail = CreateIncomingMailItem();
			blankMail.MI_Body = ZString.Empty;
			AssertEquals("precondition", false, blankMail.HasContent);
			Factory.Save();
			bool found = false;
			retriever.Logger.OnLogInfoAdded += (string logEntry, LogType logType) =>
			{
				if (logEntry.Contains("Mail message has no content, ignoring. (from: MI_From, date: 09-Sep-07 09:09:09, subject: 12345)"))
				{
					found = true;
				}
			};
			retriever.ExecuteBatch();
			Assert(found);
			blankMail.Reload();
			AssertEquals(MailStatus.Failed, blankMail.MI_Status);
		}

		public void TestSetStatusToProcessedIfIterchangeAdded()
		{
			MailItem newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ExecuteBatch();
			newMail.Reload();
			AssertEquals("MailStatus", MailStatus.Processed, newMail.MI_Status);
		}

		[ExpectNoExceptions]
		public void TestUsesTheMaxLoadValue()
		{
			var filter = new Mock<IMailFilter>();
			filter.Setup(m => m.Load(It.IsAny<BusinessObjectFactory>(), retriever.NumberToRetrieveAtATimeOverride)).Returns(Array.Empty<MailItem>());
			retriever.FilterOverride = filter.Object;

			retriever.ExecuteBatch();
			filter.VerifyAll();
		}

		public void TestSetStatusToProcessedIfIterchangeAddedOnSecondAttempt()
		{
			MailItem newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ThrowExceptionTimesWhilstCreatingInterchange = 1;

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => retriever.ExecuteBatch());
				AssertEquals("Should retry two times.", 1, retriever.ExposedPKsOfMailItemsThatCausedExceptions[newMail.PK]);
				AssertCollectionContains("Should mark as processed.", newMail.PK, retriever.ExposedPKsOfMailItemsThatProcessed);

				newMail.Reload();
				AssertEquals("MailStatus", MailStatus.Processed, newMail.MI_Status);
			});
		}

		public void TestSetStatusToFailedIfAddingFailsThreeTimes()
		{
			var newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ThrowExceptionTimesWhilstCreatingInterchange = 3;

			CombineAssertions(() =>
			{
				AssertExceptionThrown<Exception>(() => retriever.ExecuteBatch());
				AssertEquals("Should retry three times.", 3, retriever.ExposedPKsOfMailItemsThatCausedExceptions[newMail.PK]);
				AssertCollectionContains("Should mark as processed.", newMail.PK, retriever.ExposedPKsOfMailItemsThatProcessed);

				newMail.Reload();
				AssertEquals("MailStatus", MailStatus.Failed, newMail.MI_Status);
			});
		}

		public void TestDecodeMIMEText()
		{
			AssertMultilineEquals("CONTENT-TYPE: TEXT/PLAIN was not detected", CONTENTTYPE_TEXTPLAIN_MIMESample.Substring(3), BatchProcessorSupporter.DecodeMIMEText(CONTENTTYPE_TEXTPLAIN_MIMESample), '\n');
		}

		public void TestCorruptedDuplicateInterchange()
		{
			MailItem newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.TestingInterchangeText = true;
			retriever.TestInterchangeText =
"UNA:+.? '" +
"UNB+UNOC:3+AAA336C::AAA336C+AAA374M+070702:1624+00000000314454++++++1'" +
"UNH+000001+CONTRL:D:3:UN'" +
"UCI+1076+AAA374M::AAA374M+AAA336C+7'" +
"UCM+1+CUSDEC:D:99B:UN+7'" +
"UNT+4+000001'" +
"UNZ+1+00000000314455'";

			retriever.ExecuteBatch();
			AssertEquals(1, retriever.RetrievedInterchanges);
			newMail.Reload();
			AssertEquals(MailStatus.Processed, newMail.MI_Status);

			newMail = CreateIncomingMailItem();
			Factory.Save();

			retriever.ExecuteBatch();
			AssertEquals(0, retriever.RetrievedInterchanges);
			newMail.Reload();
			AssertEquals(MailStatus.Processed, newMail.MI_Status);
		}

		[TestDate(2020, 09, 09, 09, 09, 09)]
		public void TestIgnoreMessageIfNoCompanyFound()
		{
			var mail = CreateIncomingMailItem();
			Factory.Save();
			var retriever = new NewBaseInterchangeRetrieverReturnsNoCompany();

			var expectedLogAdded = false;
			retriever.Logger.OnLogInfoAdded += (string logEntry, LogType logType) =>
			{
				if (logEntry.Contains("No company was found to process this mail, ignoring. (from: MI_From, date: 09-Sep-20 09:09:09, subject: 12345)"))
				{
					expectedLogAdded = true;
				}
			};

			retriever.ExecuteBatch();
			Assert("No company found log should have been added.", expectedLogAdded);
			AssertEquals("Email should be marked as Failed.", MailStatus.Failed, mail.MI_Status);
		}

		protected virtual MailItem CreateIncomingMailItem()
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.Queued;
			result.MI_Direction = DirectionList.Codes.Receive;
			result.MI_SendDateTime = ZDateTime.Now;
			result.MI_ReceivedDateTime = ZDateTime.Now;
			result.MI_Subject = "12345";
			result.MI_Body = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("INTERCHANGESTRING"));
			result.MI_From = "MI_From";
			//might be a test filter, so can't use normal method
			if (retriever.IncomingInterchangeMailFilter.CanProcess(result))
			{
				result.MI_Application = retriever.IncomingInterchangeMailFilter.Code;
			}
			else
			{
				Fail("Result couldn't be processed");
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			retriever = new TestHelperNewBaseInterchangeRetriever();
		}
		protected TestHelperNewBaseInterchangeRetriever retriever;

		const string CONTENTTYPE_TEXTPLAIN_MIMESample = @"--#BOUNDARY#1\r\nContent-Type: text/plain\r\n\r\nUNB+UNOA:1+SARS+EDITEST+051031:2343+000002420+A1A1A1A1A1A1A1:A1+CONTRL-EXPORT+++00299774ROH-E+1'\r\nUNH+00000242000001+CONTRL:D:3:UN+CONTRL'\r\nUCI+1759+EDITEST+SARS+7'\r\nUCM+1192+CUSDEC:D:96B:UN:ZZZ01+7'\r\nUNT+4+00000242000001'\r\nUNZ+1+000002420'\r\n\r\n--#BOUNDARY#1\r\nContent-Type: application/pkcs7-signature; micalg=sha1\r\nContent-Transfer-Encoding: base64\r\n\r\nMIILRAYJKoZIhvcNAQcCoIILNTCCCzECAQExCzAJBgUrDgMCGgUAMAsGCSqGSIb3\r\nDQEHAaCCCZUwggK2MIICH6ADAgECAhBhyB+ZF8yrt9EoGXVT2TMyMA0GCSqGSIb3\r\nDQEBBAUAMIHBMQswCQYDVQQGEwJVUzEXMBUGA1UEChMOVmVyaVNpZ24sIEluYy4x\r\nPDA6BgNVBAsTM0NsYXNzIDIgUHVibGljIFByaW1hcnkgQ2VydGlmaWNhdGlvbiBB\r\ndXRob3JpdHkgLSBHMjE6MDgGA1UECxMxKGMpIDE5OTggVmVyaVNpZ24sIEluYy4g\r\nLSBGb3IgYXV0aG9yaXplZCB1c2Ugb25seTEfMB0GA1UECxMWVmVyaVNpZ24gVHJ1\r\nc3QgTmV0d29yazAeFw0wMjEyMTgwMDAwMDBaFw0xMjEyMTcyMzU5NTlaMFIxETAP\r\nBgNVBAoTCE5hbUlUZWNoMR8wHQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3Jr\r\nMRwwGgYDVQQDExNOYW1JVGVjaCBDbGFzcyAyIENBMIGfMA0GCSqGSIb3D
QEBAQUA\r\nA4GNADCBiQKBgQDYSaBiXS+MlG4Ks76pHI1kFLMJH+JmjaFeCKC8hQdk7lT3Ytig\r\n42WMoY9qFcZFgD8frwO0LRihGvg8yomHBqxEFTvbQEDhaNULuMWs2xo3+nM+BOFd\r\nPivb53VeDNYf6Wve/eVh+LX1fRrPo9/E5TGr3cPC7pSkX0FGF7uC+WN0pQIDAQAB\r\nox0wGzAMBgNVHRMEBTADAQH/MAsGA1UdDwQEAwIBBjANBgkqhkiG9w0BAQQFAAOB\r\ngQCHbhwWKNnaSLL2NR3V1+D77b2PsqX31M48dl4EdaDIx+O+t/MnTc4BGUOz7R0X\r\n1BW8Zja1jE9Wtx2LVcZKQjMBOOyHcgZw+DKpiK/gFjVK3W7e0FTYdxGA1P6utxlj\r\nx1TusG18AoLbuVhs4ug0BZlf7vZZpvdjMz+DgUv79MbczTCCAzAwggKZoAMCAQIC\r\nEFGJLG6J1rMB+k/8L/8KzT8wDQYJKoZIhvcNAQEEBQAwUjERMA8GA1UEChMITmFt\r\nSVRlY2gxHzAdBgNVBAsTFlZlcmlTaWduIFRydXN0IE5ldHdvcmsxHDAaBgNVBAMT\r\nE05hbUlUZWNoIENsYXNzIDIgQ0EwHhcNMDQwNDI4MDAwMDAwWhcNMDkwNDI3MjM1\r\nOTU5WjCBuzENMAsGA1UEChMEU0FSUzEfMB0GA1UECxMWVmVyaVNpZ24gVHJ1c3Qg\r\nTmV0d29yazE7MDkGA1UECxMyVGVybXMgb2YgdXNlIGF0IGh0dHBzOi8vd3d3Lm5h\r\nbWl0ZWNoLmNvbS9ycGEgKGMpMDQxMDAuBgNVBAsTJ0NsYXNzIDIgT25TaXRlIElu\r\nZGl2aWR1YWwgU3Vic2NyaWJlciBDQTEaMBgGA1UEAxMRU0FSUyBlQ29tbWVyY2Ug\r\nQ0EwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAM1IxDz37XwQwU/iCNBNZF
tR\r\nw4xIfdxiea1vfpkG/cBF9zxZobv/yRLx2CWtHZE4jKy1jRCG4rjsH8O2UP3IstUL\r\nO32i4OwE8H1WmmnkrPDAhFlUcug9/EL3iuqsxJ9Gkw0ScW65dNM8lrHX7djcax+q\r\n4cVqZvi0LT5twRkXzLXBAgMBAAGjgZwwgZkwDwYDVR0TBAgwBgEB/wIBADBEBgNV\r\nHSAEPTA7MDkGC2CGSAGG+EUBBxcCMCowKAYIKwYBBQUHAgEWHGh0dHBzOi8vd3d3\r\nLm5hbWl0ZWNoLmNvbS9ycGEwCwYDVR0PBAQDAgEGMBEGCWCGSAGG+EIBAQQEAwIB\r\nBjAgBgNVHREEGTAXpBUwEzERMA8GA1UEAxMIQzFDMi0xLTgwDQYJKoZIhvcNAQEE\r\nBQADgYEApJJwB6MZKXOTuHY+yL8itnaUtIeT/UcNmHnJHyvWOeFJ0nUq0rFCWJgj\r\nBVVUZ9gzXcUIS/4xjvgxQVEvHF1L+IC+gs6HUC1KIEk/zRyAnRT5HL4EwzZEp+tC\r\nx86eOOn2yi9AMCeFahraS9vrMvJiYNxPzjyf1Xp1zwZ0B+E5sr8wggOjMIIDDKAD\r\nAgECAhB2JM7jofMA4K98Iic6PbP5MA0GCSqGSIb3DQEBBAUAMIG7MQ0wCwYDVQQK\r\nEwRTQVJTMR8wHQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTswOQYDVQQL\r\nEzJUZXJtcyBvZiB1c2UgYXQgaHR0cHM6Ly93d3cubmFtaXRlY2guY29tL3JwYSAo\r\nYykwNDEwMC4GA1UECxMnQ2xhc3MgMiBPblNpdGUgSW5kaXZpZHVhbCBTdWJzY3Jp\r\nYmVyIENBMRowGAYDVQQDExFTQVJTIGVDb21tZXJjZSBDQTAeFw0wNTAyMjIwMDAw\r\nMDBaFw0wNjAyMjIyMzU5NTlaMIHZMQ0wCwYDVQQKFARTQVJTMRIwEAYDVQQLFAll\r\
nQ29tbWVyY2UxHDAaBgNVBAsUE0NvbXBhbnkgTmFtZSAtIFNBUlMxLjAsBgNVBAsU\r\nJVNBUlMgQnVzaW5lc3MgUmVmZXJlbmNlIE51bWJlciAtIENBVFMxLjAsBgNVBAsU\r\nJUluZGl2aWR1YWwgSUQgTnVtYmVyIC0gNzIwOTE0NTE0MTA4MTIxEzARBgNVBAMT\r\nCkRlb24gU21pdGgxITAfBgkqhkiG9w0BCQEWEmRzbWl0aEBzYXJzLmdvdi56YTCB\r\nnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAzvIgBE+jB+ubjaO0N9cZwL05ltsz\r\nK7wdAbdp7flqehOXp6w7A6YnHCLudZix09UUCiCzNiaO7Da9JWCF4F+jc7SbbKvr\r\nlsMu/8E1a5r0Hw5VhoSimel+BVj81AyTWeU2wF5KVHYIVwOJxuJ7PQsCv970mSxz\r\naXCAVzpVOHywaWcCAwEAAaOBhzCBhDAJBgNVHRMEAjAAMAsGA1UdDwQEAwIFoDAR\r\nBglghkgBhvhCAQEEBAMCB4AwRAYDVR0fBD0wOzA5oDegNYYzaHR0cDovL2NybC5u\r\nYW1pdGVjaC5jb20vU0FSU2VDb21tZXJjZS9MYXRlc3RDUkwuY3JsMBEGCmCGSAGG\r\n+EUBBgkEAwEB/zANBgkqhkiG9w0BAQQFAAOBgQB1hS6eW9+f70IhXKojwiAIEDf3\r\n8riTZkE8tBu2GqrkUrL79rO4XNWZgUcpMXB/INvafuIzGeu9npIs7ZpIIbp7wNFO\r\nxy4nas1Ktaqh1QxeLNLcMVjmkPB4SnIOTPNvat/4N7NpZyWkzWCL8alERRlnm5YR\r\nhe6bdjRJllJoJI8+XTGCAXcwggFzAgEBMIHQMIG7MQ0wCwYDVQQKEwRTQVJTMR8w\r\nHQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTswOQYDVQQLEzJUZXJtcyBv\r\nZiB1
c2UgYXQgaHR0cHM6Ly93d3cubmFtaXRlY2guY29tL3JwYSAoYykwNDEwMC4G\r\nA1UECxMnQ2xhc3MgMiBPblNpdGUgSW5kaXZpZHVhbCBTdWJzY3JpYmVyIENBMRow\r\nGAYDVQQDExFTQVJTIGVDb21tZXJjZSBDQQIQdiTO46HzAOCvfCInOj2z+TAJBgUr\r\nDgMCGgUAMA0GCSqGSIb3DQEBAQUABIGAtVQH+D2td6l4gV7TXmxyZT1kwhn9sCxh\r\nJq2Cc0dcld4VF/QeOF9vaqVrStuNLyJDN9Z0ERyPQKMO9AYEag4ll2WHg0K8Vf22\r\nYuICyRvgt2+5sOsMNJMq0c2cC3wIok+UPCMXEx2nW/SWFm2AniJ2wsk5N5fPcNoW\r\nkk0mm8/x2iY=\r\n\r\n\r\n--#BOUNDARY#1--\r\n\r\n\0";

		protected class TestHelperNewBaseInterchangeRetriever : NewBaseInterchangeRetriever
		{
			public bool ThrowMailItemDecodeFailException;
			public bool ThrowCryptoUtilitiesException;
			public bool TestingInterchangeText;
			public bool ForceConcurrencyError;

			public TestHelperNewBaseInterchangeRetriever()
			{
				NumberToRetrieveAtATimeOverride = base.NumberToRetrieveAtATime;
			}

			protected override bool SendCryptoExceptionToPostMaster => sendCryptoExceptionToPostMaster;
			bool sendCryptoExceptionToPostMaster;

			public void SetSendCryptoExceptionToPostMaster(bool send)
			{
				sendCryptoExceptionToPostMaster = send;
			}

			public ZString TestInterchangeText
			{
				get
				{
					return fTestInterchangeText;
				}
				set
				{
					fTestInterchangeText = value;
				}
			}
			ZString fTestInterchangeText;

			protected override ZString GetInterchangeText(MailItem item, bool decryptInNewThread)
			{
				if (ThrowMailItemDecodeFailException)
				{
					throw new MailItemDecodeFail("BLAH", "EMAIL BODY TEXT", null);
				}
				else if (ThrowCryptoUtilitiesException)
				{
					throw new CryptoUtilitiesException("BLAH", 2146881278);
				}
				else if (TestingInterchangeText)
				{
					return fTestInterchangeText;
				}
				else if (ForceConcurrencyError)
				{
					var anotherFactory = new BusinessObjectFactory();
					anotherFactory.RefreshEnabled = false;

					var itemInAnotherFactory = anotherFactory.Load<MailItem>(item.PK);
					itemInAnotherFactory.MI_ReplyTo = "AAA";

					anotherFactory.Save();

					item.MI_ReplyTo = "BBB";
					return base.GetInterchangeText(item, decryptInNewThread);
				}
				else
				{
					return base.GetInterchangeText(item, decryptInNewThread);
				}
			}

			public Dictionary<ZGuid, int> ExposedPKsOfMailItemsThatCausedExceptions => PKsOfMailItemsThatCausedExceptions;

			public HashSet<ZGuid> ExposedPKsOfMailItemsThatProcessed => PKsOfMailItemsThatProcessed;

			public int? ThrowExceptionTimesWhilstCreatingInterchange;

			public IMailFilter FilterOverride { get; set; } = new QueryMailFilter("TST", subject: "12345", from: "MI_From");

			protected override IMailFilter GetMailFilter() => FilterOverride;

			protected override int NumberToRetrieveAtATime => NumberToRetrieveAtATimeOverride;

			public int NumberToRetrieveAtATimeOverride { get; set; }

			protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
			{
				if (ThrowExceptionTimesWhilstCreatingInterchange.HasValue)
				{
					if (ThrowExceptionTimesWhilstCreatingInterchange > 0)
					{
						ThrowExceptionTimesWhilstCreatingInterchange--;

						throw new Exception("Temp Exception");
					}
				}

				if (TestingInterchangeText)
				{
					return base.CreateInterchangeAndMessages(factory, interchangeString, mailItem);
				}
				else
				{
					var result = factory.New<EDIInterchange>();
					result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
					result.EI_From = "FROM";
					result.EI_To = "TO";
					result.EI_GB = Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK;
					return result;
				}
			}
		}

		class TestHelperNewBaseInterchangeRetrieverThatReturnsNoInterchangeText : TestHelperNewBaseInterchangeRetriever
		{
			protected override ZString GetInterchangeText(MailItem item, bool decryptInNewThread)
			{
				return "";
			}

			protected override bool ShouldIgnoreEmptyInterchangeText(MailItem item)
			{
				return ShouldIgnoreEmptyInterchangeTextExposed;
			}
			public bool ShouldIgnoreEmptyInterchangeTextExposed;
		}

		class NewBaseInterchangeRetrieverReturnsNoCompany : NewBaseInterchangeRetriever
		{
			protected override IMailFilter GetMailFilter()
			{
				return new QueryMailFilter("TST", subject: "12345", from: "MI_From");
			}

			protected override IEnumerable<ZString> GetCompanyCodesFromMailItem(MailItem item)
			{
				return Enumerable.Empty<ZString>();
			}
		}
	}
}
