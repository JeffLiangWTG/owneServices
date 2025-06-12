using System.IO;
using System.Runtime.Serialization;
using System.Text;
using eServices.eHubDataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eServices.eHubRoutingRuleEngine.Tests
{
	[TestClass]
	public class ResultTests
	{
		[TestMethod]
		public void TestResult_Serialize()
		{
			var result = new Result
			{
				Recipient = new eHubClient { CC_ID = "GEI_Taiwan", CC_FriendlyName = "Global Electronic Invoicing - Taiwan" }
			};

			string serializedResult;
			using (var memoryStream = new MemoryStream())
			{
				var serializer = new DataContractSerializer(typeof(Result));
				serializer.WriteObject(memoryStream, result);

				memoryStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(memoryStream))
				{
					serializedResult = streamReader.ReadToEnd();
				}
			}

			StringAssert.Contains("<Result xmlns=\"http://schemas.datacontract.org/2004/07/eServices.eHubRoutingRuleEngine\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><ErrorCode i:nil=\"true\"/><ErrorDescription i:nil=\"true\"/><RecipientId>GEI_Taiwan</RecipientId><Value i:nil=\"true\"/></Result>", serializedResult);
		}

		[TestMethod]
		public void TestResult_Deserialize()
        {
            var serialized = "<Result xmlns=\"http://schemas.datacontract.org/2004/07/eServices.eHubRoutingRuleEngine\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><ErrorCode i:nil=\"true\"/><ErrorDescription i:nil=\"true\"/><RecipientId>GEI_Taiwan</RecipientId><Value i:nil=\"true\"/></Result>";

            Result deserializedResult;

			using (var memoryStream = new MemoryStream(Encoding.Default.GetBytes(serialized)))
			{
				var serializer = new DataContractSerializer(typeof(Result));
                deserializedResult = (Result) serializer.ReadObject(memoryStream);
			}

            StringAssert.Contains("GEI_Taiwan", deserializedResult.Recipient.CC_ID);
            Assert.IsNull(deserializedResult.ErrorCode);
            Assert.IsNull(deserializedResult.ErrorDescription);
            Assert.IsNull(deserializedResult.Value);
        }
    }
}
