using System;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Receive.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests.SchemaTests
{
    [TestClass]
    public class SchemaTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestAIRPIN_LargeINDLoop()
        {
            var inputXmlFileName = Assembly.GetExecutingAssembly().GetName().Name + ".Schemas.AIRPIN_TestFiles.AIRPIN_LargeINDLoop_Input.xml";
            var inputXmlFile = XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(inputXmlFileName));
            var report = String.Empty;
            inputXmlFile.Validate(new EFACT_31_AIRPIN().SchemaSet, (_, validationEventArgs) =>
                {
                    report = validationEventArgs.Message;
                });
            Assert.AreEqual(string.Empty, report, typeof(EFACT_31_AIRPIN).Name + " is not valid: \r\n " + report);
        }
    }
}
