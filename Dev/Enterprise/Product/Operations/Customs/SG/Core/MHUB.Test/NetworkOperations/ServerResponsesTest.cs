using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.MHUB.Testing
{
	sealed class ServerResponsesTest : TestCaseWithFactory
	{
		public void TestServerResponses()
		{
			ServerResponses testServerResponses = new ServerResponses(@"
				asdfasdf
				<body>
					requestStatus=0|sessionId=34523523|state=|someerrormessage|
					requestStatus=0|sessionId=98798987|nfn=B|recip_id=recp001|
				</body>
				sdfsd");
			AssertEquals("Server Responses Count", 2, testServerResponses.Count);
			bool firstPass = true;
			foreach (ServerResponse serverResponse in testServerResponses)
			{
				if (firstPass)
				{
					Assert("Response contains key requestStatus", serverResponse.ContainsKey("requestStatus"));
					AssertEquals("The key requestStatus value", "0", serverResponse.GetValue("requestStatus"));
					Assert("Response contains key sessionId", serverResponse.ContainsKey("sessionId"));
					AssertEquals("The key sessionId value", "34523523", serverResponse.GetValue("sessionId"));
					Assert("Response contains key state", serverResponse.ContainsKey("state"));
					AssertEquals("The key state value", string.Empty, serverResponse.GetValue("state"));
					firstPass = false;
				}
				else
				{
					Assert("Response contains key requestStatus", serverResponse.ContainsKey("requestStatus"));
					AssertEquals("The key requestStatus value", "0", serverResponse.GetValue("requestStatus"));
					Assert("Response contains key sessionId", serverResponse.ContainsKey("sessionId"));
					AssertEquals("The key sessionId value", "98798987", serverResponse.GetValue("sessionId"));
					Assert("Response contains key nfn", serverResponse.ContainsKey("nfn"));
					AssertEquals("The key nfn value", string.Empty, serverResponse.GetValue("B"));
					Assert("Response containe recip_id key", serverResponse.ContainsKey("recip_id"));
					AssertEquals("The key recip_id value", "recp001", serverResponse.GetValue("recip_id"));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestServerResponses_InvalidResponse()
		{
			string responseMessage = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\MHUB\NetworkOperations\TestFiles\MHUBInvalidResponse.html", Encoding.ASCII);
			var serverResponses = new ServerResponses(responseMessage);
			AssertEquals(1, serverResponses.Count);
			var responseStatusCode = serverResponses.First().GetValue(MHUBConstants.Parameters.requestStatus);
			AssertEquals(string.Empty, responseStatusCode);
			var successOrFailure = SG.MHUB.MhxScriptSuccessChecker.ObtainSuccessOrFailureFromResponses(serverResponses, MHUBConstants.CommandType.Retrieve);
			AssertEquals(MHUBConstants.ResponseStatusCodes.Failed, successOrFailure);
		}

		public void TestHasDuplicatedKeyInServerResponses()
		{
			try
			{
				ErrorReporter.Clear();
				var testServerResponses = new ServerResponses(@"
				<html>
				<body>
					requestStatus=0|sessionId=34523523|state=01|someerrormessage|state=02
				</body>
				</html>");
				CombineAssertions(() =>
				{
					AssertEquals("LastKeyReported", "MHUBWebCommand - An item with the same key has already been added.", ErrorReporter.LastKeyReported);
					AssertEquals("LastMessageReported", "'state' has been added in this response. The full response body is: |requestStatus=0|sessionId=34523523|state=01|someerrormessage|state=02", ErrorReporter.LastMessageReported);
				});
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestCount()
		{
			ServerResponses testServerResponses = new ServerResponses(@"
				asdfasdf
				<body>
					requestStatus=0|sessionId=34523523|state=|someerrormessage|
					requestStatus=0|sessionId=98798987|nfn=B|recip_id=recp001|
				</body>
				sdfsd");
			AssertEquals("Server Responses Count", 2, testServerResponses.Count);
		}

		public void TestGetFirst()
		{
			ServerResponses testServerResponses = new ServerResponses(@"
				asdfasdf
				<body>
					requestStatus=0|sessionId=34523523|state=|someerrormessage|
					requestStatus=0|sessionId=98798987|nfn=B|recip_id=recp001|
				</body>
				sdfsd");
			AssertEquals("Server Responses Count", 2, testServerResponses.Count);
			ServerResponse firstServerResponse = testServerResponses.GetFirst();
			Assert("Response contains key requestStatus", firstServerResponse.ContainsKey("requestStatus"));
			AssertEquals("The key requestStatus value", "0", firstServerResponse.GetValue("requestStatus"));
			Assert("Response contains key sessionId", firstServerResponse.ContainsKey("sessionId"));
			AssertEquals("The key sessionId value", "34523523", firstServerResponse.GetValue("sessionId"));
			Assert("Response contains key state", firstServerResponse.ContainsKey("state"));
			AssertEquals("The key state value", string.Empty, firstServerResponse.GetValue("state"));
		}

		public void TestErrorResponse()
		{
			ServerResponses testServerResponses = new ServerResponses(@"<html>
				<head><title>EDI Servlet</title></head>
				<body>
				requestStatus=-1|Server Exception caught: 1326::User account currently frozen
				</body>
				</html>");
			ServerResponse firstServerResponse = testServerResponses.GetFirst();
			Assert("Response contains key requestStatus", firstServerResponse.ContainsKey("requestStatus"));
			AssertEquals("The key requestStatus value", "-1", firstServerResponse.GetValue("requestStatus"));
			Assert("Response contains ErrorCode", firstServerResponse.ContainsKey("ErrorCode"));
			AssertEquals("The key ErrorCode value", "1326", firstServerResponse.GetValue("ErrorCode"));
			Assert("Response contains key ErrorMsg", firstServerResponse.ContainsKey("ErrorMsg"));
			AssertEquals("The key ErrorMsg value", "User account currently frozen", firstServerResponse.GetValue("ErrorMsg"));
		}
	}
}
