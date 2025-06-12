using System.Linq;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.GBCustoms.TL.BT.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.GBCustoms.TL.BT.Tests.Helpers
{
    [TestClass]
    public class EndpointConfigurationTests
    {
        [TestMethod]
        public void TestGetConfig()
        {
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();
            ctx.eHubClients.Add(new eHubClient { CC_ID = "GBCustoms" });
            ctx.eHubClients.Add(new eHubClient { CC_ID = "GBCustoms" });
            ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "GBCustoms Transport Layer Configuration", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

            ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Endpoints", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Endpoint URL" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "GBCustomsTest-ICSGB", CK_Key2Value = "Create" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "https://test-api.service.hmrc.gov.uk/customs/imports/declarations" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Method" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "GBCustomsTest-ICSGB", CK_Key2Value = "Create" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "POST" });
            ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Content Type" });
            ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "GBCustomsTest-ICSGB", CK_Key2Value = "Create" });
            ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "XML" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Version" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "GBCustomsTest-ICSGB", CK_Key2Value = "Create" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "2.0" });

			var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
            
            var config = EndpointConfiguration.GetConfig("GBCustomsTest-ICSGB", "Create");

            Assert.IsNotNull(config);
            Assert.AreEqual(config.EndpointUrl, "https://test-api.service.hmrc.gov.uk/customs/imports/declarations");
            Assert.AreEqual(config.Method, "POST");
            Assert.AreEqual(config.ContentType, "XML");
			Assert.AreEqual(config.Version, "2.0");
		}
    }
}