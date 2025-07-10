using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Customs.SG.V4.MHUB.MHUBConstants;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Testing
{
	sealed class MHAccessClientTest : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSubmit()
		{
			WithMHAccessVersion(effectiveDays: 5, () =>
			{
				var client = new MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), new LoggingInformation());
				var tempDir = Temp.TempPath;
				var tempData = Path.Combine(tempDir, "data.txt");
				var tempAttachment1 = Path.Combine(tempDir, "att1.txt");
				var tempAttachment2 = Path.Combine(tempDir, "att2.txt");
				File.WriteAllBytes(tempData, System.Text.Encoding.ASCII.GetBytes("This is some fake same EDIFACT"));
				File.WriteAllBytes(tempAttachment1, new byte[] { 1, 234, 24, 242, 2, 4, 255, 65, 7, 53, 5, 0, 0, 24 });
				File.WriteAllBytes(tempAttachment2, new byte[] { 0, 1, 2, 3, 4, 242, 2, 4, 255, 65, 7, 53, 5, 0, 0, 24 });
				try
				{
					client.SubmitRequest.CannedDataToSendBack = SubmitRequestPostTest.cannedReplyOMG;
					client.SubmitInterchangeFile(tempData, "CUSDEC", "123", false, new string[] { tempAttachment1, tempAttachment2 });
					var whatWeSent = client.HttpClientForTest.RequestsMade;
					CombineAssertions(delegate
					{
						AssertEquals("Command=Login&Userid=R9z74QLpZcE%3D&Password=ZJJciPbEUHY%3D&AppId=CargoWise&DigestForLibs=1Uf9kgxzufLXaZiGyCw93g%3D%3D&Encrypted=true&ClientId=MHXWIN&CurrentVersion=FourZeroTwoOne&VndId=Daniel", whatWeSent[0]);
						AssertStartsWith("getParam request string", "Command=getParam&CurrentVersion=FourZeroTwoOne", whatWeSent[1]);
						AssertEquals("Command=Logout", whatWeSent[2]);
					});
					var soapPayloadRequest = Encoding.ASCII.GetString(client.SubmitRequest.Wrapper.MemoryStreamRequest.GetBuffer());
					CombineAssertions(() =>
					{
						using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.SOAP_MTOM.TestFiles.MHAccessClient.SubmitEdiFile.txt"))
						{
							var expectedRequest = new StreamReader(stream).ReadToEnd();
							expectedRequest = string.Format(expectedRequest, client.LastFileUUID);
							AssertEquals(expectedRequest, soapPayloadRequest);
						}

						AssertContainsExactElementsInAnyOrder("Attachment Files", new[] { tempAttachment1, tempAttachment2 }, client.LastAttachments);
					});
				}
				finally
				{
					File.Delete(tempData);
					File.Delete(tempAttachment1);
					File.Delete(tempAttachment2);
				}
			});
		}

		[TestDate(2020, 1, 21, 10, 0, 0)]
		public void TestSubmitXmlFile()
		{
			WithMHAccessVersion(effectiveDays: 5, digestPreviousValue: "Previous", action: () =>
			{
				var client = new MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), new LoggingInformation());
				var tempDir = Temp.TempPath;
				var tempData = Path.Combine(tempDir, "data.xml");
				var tempAttachment1 = Path.Combine(tempDir, "att1.txt");
				var tempAttachment2 = Path.Combine(tempDir, "att2.txt");
				File.WriteAllBytes(tempData, System.Text.Encoding.ASCII.GetBytes(@"<?xml version=""1.0"" encoding=""UTF - 8""?><TradenetDeclaration xmlns=""urn: crimsonlogic:tn: schema:xsd: TradenetDeclaration""><This is some fake xml data>"));
				File.WriteAllBytes(tempAttachment1, new byte[] { 1, 234, 24, 242, 2, 4, 255, 65, 7, 53, 5, 0, 0, 24 });
				File.WriteAllBytes(tempAttachment2, new byte[] { 0, 1, 2, 3, 4, 242, 2, 4, 255, 65, 7, 53, 5, 0, 0, 24 });
				try
				{
					client.SubmitRequest.CannedDataToSendBack = SubmitRequestPostTest.cannedReplyOMG;
					client.SubmitInterchangeFile(tempData, "CUSDEC", "123", true, new string[] { tempAttachment1, tempAttachment2 });
					var whatWeSent = client.HttpClientForTest.RequestsMade;
					CombineAssertions(delegate
					{
						AssertEquals("Command=Login&Userid=R9z74QLpZcE%3D&Password=ZJJciPbEUHY%3D&AppId=CargoWise&DigestForLibs=Jaxm%2BPLOBVPQycnxoOf%2B3g%3D%3D&Encrypted=true&ClientId=MHXWIN&CurrentVersion=FourZeroTwoOne&VndId=Daniel", whatWeSent[0]);
						AssertStartsWith("getParam request string", "Command=getParam&CurrentVersion=FourZeroTwoOne", whatWeSent[1]);
						AssertEquals("Command=Logout", whatWeSent[2]);
					});
					var soapPayloadRequest = Encoding.ASCII.GetString(client.SubmitRequest.Wrapper.MemoryStreamRequest.GetBuffer());
					CombineAssertions(() =>
					{
						using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.SOAP_MTOM.TestFiles.MHAccessClient.SubmitXmlFile.txt"))
						{
							var expectedRequest = new StreamReader(stream).ReadToEnd();
							expectedRequest = string.Format(expectedRequest, client.LastFileUUID);
							AssertEquals(expectedRequest, soapPayloadRequest);
						}

						AssertContainsExactElementsInAnyOrder("Attachment Files", new[] { tempAttachment1, tempAttachment2 }, client.LastAttachments);
					});
				}
				finally
				{
					File.Delete(tempData);
					File.Delete(tempAttachment1);
					File.Delete(tempAttachment2);
				}
			});
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestRetrieveEdiFactStubWhenOnlyNdnsExist()
		{
			WithMHAccessVersion(effectiveDays: 0, () =>
			{
				var client = new MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), new LoggingInformation(), runScenarioWithOnlyAnNdnToDownload: true);
				client.SubmitRequest.CannedDataToSendBack = SubmitRequestPostTest.cannedReplyOMG.Replace("OMG", "");
				client.RetrieveMessageStub((IEnumerable<string> interchangesToSave) =>
				{
					AssertEquals("Should see zero interchanges to save", 0, interchangesToSave.Count());
				});
				Assert("We got here without puking in the Decyrpt method, all is good", true);
			});
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestRetrieveEdiFactStub()
		{
			WithMHAccessVersion(effectiveDays: 10, () =>
			{
				var client = new MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), new LoggingInformation());
				client.SubmitRequest.CannedDataToSendBack = SubmitRequestPostTest.cannedReplyOMG;
				IEnumerable<string> downloadedData = null;
				client.RetrieveMessageStub((IEnumerable<string> x) =>
				{
					downloadedData = x;
				});
				AssertEquals("OMG", downloadedData.First());
				var whatWeSent = client.HttpClientForTest.RequestsMade;
				CombineAssertions(() =>
				{
					AssertEquals("Command=Login&Userid=R9z74QLpZcE%3D&Password=ZJJciPbEUHY%3D&AppId=CargoWise&DigestForLibs=1Uf9kgxzufLXaZiGyCw93g%3D%3D&Encrypted=true&ClientId=MHXWIN&CurrentVersion=FourZeroTwoOne&VndId=Daniel", whatWeSent[0]);
					AssertStartsWith("getParam request string", "Command=getParam&CurrentVersion=FourZeroTwoOne", whatWeSent[1]);
					AssertContains("Command=Retrieve&filename=file%3A%2F%2FWS%3A%2F%2F%2Fmilton%2Fkeynes%2FDaniel%2Fmhxdownload%2Fdjc_", whatWeSent[2]);
					AssertContains("&loc=I&split=Y&destroy=N&no_of_msg=50&zipfile=file%3A%2F%2F%2Fmilton%2Fkeynes%2FDaniel%2Fmhxdownload%2Fdjc_", whatWeSent[2]);
					AssertContains(".zip&retr_session=123456&CurrentVersion=FourZeroTwoOne&Encrypted=true&VndId=Daniel", whatWeSent[2]);
					AssertEquals("Command=Update&session_id=123456.1&destroy=Y", whatWeSent[3]);
					AssertEquals("Command=Logout", whatWeSent[4]);
				});
				var soapPayloadRequest = Encoding.ASCII.GetString(client.SubmitRequest.Wrapper.MemoryStreamRequest.GetBuffer());
				AssertContains(@"--uuid:DELIMITMEBABY
Content-Id: <rootpart*DELIMITMEBABY@example.jaxws.sun.com>
Content-Type: application/xop+xml; charset=utf-8;type=""text/xml""
Content-Transfer-Encoding: binary

<?xml version='1.0' encoding='UTF-8'?><S:Envelope xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/""><S:Body><ns2:downloadRequest xmlns:ns2=""http://service.wsvc.mhb.crimsonlogic.com/wsdl""><userID>djc</userID><fileName>djc_1440252000000.zip</fileName><version>FourZeroTwoOne</version><uuid>UNHDLRgVNzF/B6suNswlZErL/Ycpoyif56hlYrR7cqXDAcel74P4saZavIIpE7tW</uuid><vndId>Daniel</vndId></ns2:downloadRequest></S:Body></S:Envelope>
--uuid:DELIMITMEBABY--", soapPayloadRequest);
			});
		}

		public void TestShouldAlwaysDownloadEvenWhenOnlyNDNsArePresent()
		{
			WithMHAccessVersion(effectiveDays: 0, () =>
			{
				var client = new MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), new LoggingInformation());
				var emptyMailbox = @"<body>\r\n\r\nrequestStatus=1|\r\n</body>";
				var oneMessage = @"<body>\r\n\r\nrequestStatus=0|mail_type=1|sender_id=dcst201\r\n</body>";
				var oneAperak = @"<body>\r\n\r\nrequestStatus=0|mail_type=4|sender_id=dcst201\r\n</body>";
				var oneOfEach = @"<body>\r\n\r\nrequestStatus=0|mail_type=4|sender_id=dcst201\r\nrequestStatus=0|mail_type=1|sender_id=dcst201\r\n</body>";
				AssertEquals(DownloadAndDecryptOptions.DownloadAndDecrypt, client.ShouldDownloadExposed(new ServerResponses(oneMessage)));
				AssertEquals(DownloadAndDecryptOptions.DownloadOnly, client.ShouldDownloadExposed(new ServerResponses(oneAperak)));
				AssertEquals(DownloadAndDecryptOptions.DownloadAndDecrypt, client.ShouldDownloadExposed(new ServerResponses(oneOfEach)));
				AssertEquals(DownloadAndDecryptOptions.None, client.ShouldDownloadExposed(new ServerResponses(emptyMailbox)));
			});
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidResponse()
		{
			WithMHAccessVersion(effectiveDays: 0, () =>
			{
				var response = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\MHUB\NetworkOperations\TestFiles\MHUBInvalidResponse.html", Encoding.ASCII);
				var logger = new LoggingInformation();
				var client = new MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), logger, cannedResponse: response);
				var result = client.RetrieveMessageStub(null);
				AssertEquals(0, result.Count());
				AssertStartsWith("", @"Unable to retrieve: <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">", logger.UserLogStrings[0].Trim());
			});
		}

		void WithMHAccessVersion(int effectiveDays, Action action, string digestPreviousValue = "Mastication")
		{
			var digest = new StringEffectiveDate()
			{
				PreviousValue = digestPreviousValue,
				NewValue = "E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D",
				EffectiveDate = ZDateTime.Now.AddDays(effectiveDays)
			};
			var mhAccessVersion = new StringEffectiveDate()
			{
				PreviousValue = "FourZeroTwoOne",
				NewValue = "4.0.4",
				EffectiveDate = ZDateTime.Now.AddDays(effectiveDays)
			};

			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SGCustomsDataRegistry.Instance.DigestForLibs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, digest))
			using (SGCustomsDataRegistry.Instance.MhaccessVersionString.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mhAccessVersion))
			using (SGCustomsDataRegistry.Instance.VendorID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Daniel"))
			{
				action.Invoke();
			}
		}
	}
}
