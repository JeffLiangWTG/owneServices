using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Products.GBCustoms.Pentant.BT.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Microsoft.XmlDiffPatch;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.BT.Tests.Helpers
{
    [TestFixture]
    public class OrchestrationHelpersTests : TestBase
    {
        const string filePath = "Helpers.TestFiles.";

        [Test]
        public void Test_CreateSoapRequestMessage_success()
        {
            var input = filePath + "01 - InputSuccessTest.xml";
            var expectedOutput = filePath + "01 - OutputSuccessTest.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_invalidEORI()
        {
            var input = filePath + "03 - InputInvalidEORI.xml";
            var expectedOutput = filePath + "03 - OutputInvalidEORI.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_large()
        {
            var input = filePath + "04 - InputSuccessTestLarge.xml";
            var expectedOutput = filePath + "04 - OutputSuccessTestLarge.xml";

           
            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_otherActions()
        {
            var input = filePath + "05 - InputCUSDECA.xml";
            var expectedOutput = filePath + "05 - OutputCUSDECA.xml";
            AssertSoapMessage(input, expectedOutput);

            input = filePath + "06 - InputCUSDECX.xml";
            expectedOutput = filePath + "06 - OutputCUSDECX.xml";
            AssertSoapMessage(input, expectedOutput);

            input = filePath + "07 - InputINVREQ.xml";
            expectedOutput = filePath + "07 - OutputINVREQ.xml";
            AssertSoapMessage(input, expectedOutput);

            input = filePath + "08 - InputCUSDECAN.xml";
            expectedOutput = filePath + "08 - OutputCUSDECAN.xml";
            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_dynamicBadge()
        {
            var input = filePath + "09 - InputBadge2Chars.xml";
            var expectedOutput = filePath + "09 - OutputBadge2Chars.xml";
            AssertSoapMessage(input, expectedOutput);

            input = filePath + "10 - InputBadge6Chars.xml";
            expectedOutput = filePath + "10 - OutputBadge6Chars.xml";
            AssertSoapMessage(input, expectedOutput);

            input = filePath + "11 - InputBadge12Chars.xml";
            expectedOutput = filePath + "11 - OutputBadge12Chars.xml";
            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_GBCustomsTest()
        {
            var input = filePath + "12 - InputGBCustomsTest1.xml";
            var expectedOutput = filePath + "12 - OutputGBCustomsTest1.xml";

            AssertSoapMessage(input, expectedOutput);

            input = filePath + "12 - InputGBCustomsTest2.xml";
            expectedOutput = filePath + "12 - OutputGBCustomsTest2.xml";

            AssertSoapMessage(input, expectedOutput, "GBCustomsTest-Pentant");
        }

        [Test]
        public void Test_CreateSoapRequestMessage_missingPartyID()
        {
            var input = filePath + "13 - InputNoPartyID.xml";
            var expectedOutput = filePath + "13 - OutputNoPartyID.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_emptyPartyID()
        {
            var input = filePath + "14 - InputEmptyPartyID.xml";
            var expectedOutput = filePath + "14 - OutputEmptyPartyID.xml";

            AssertSoapMessage(input, expectedOutput);
        }


        [Test]
        public void Test_CreateSoapRequestMessage_missingMessageBody()
        {
            var input = filePath + "15 - InputNoBody.xml";
            var expectedOutput = filePath + "15 - OutputNoBody.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_emptyMessageBody()
        {
            var input = filePath + "16 - InputEmptyBody.xml";
            var expectedOutput = filePath + "16 - OutputEmptyBody.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_ExportInventoryConsolidation()
        {
            var input = filePath + "17 - InputINVECREQ.xml";
            var expectedOutput = filePath + "17 - OutputINVECREQ.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_ExportInventoryMovement()
        {
            var input = filePath + "18 - InputINVEMREQ.xml";
            var expectedOutput = filePath + "18 - OutputINVEMREQ.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        [Test]
        public void Test_CreateSoapRequestMessage_ExportInventoryQuery()
        {
            var input = filePath + "19 - InputINVEQREQ.xml";
            var expectedOutput = filePath + "19 - OutputINVEQREQ.xml";

            AssertSoapMessage(input, expectedOutput);
        }

        void AssertSoapMessage(string input, string expectedOutput, string recipientId = "GBCustoms-Pentant")
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(GetResourceAsString(input));

            var actualSoapRequest = OrchestrationHelpers.CreateSoapRequestMessage(xmlDocument, "XCU7CSCKvES/sVlr+p6uEw==", recipientId);

            AssertXmlEquals("", GetResourceAsString(expectedOutput), actualSoapRequest);
        }
    }
}
