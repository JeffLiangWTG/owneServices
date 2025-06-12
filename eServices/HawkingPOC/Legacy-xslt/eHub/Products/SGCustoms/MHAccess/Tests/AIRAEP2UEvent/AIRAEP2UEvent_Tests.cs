using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Hawking.UnitTest.Tools.Xslt;

using Hawking.Xslt.ExtensionObjects.Interfaces;
using Moq;
using System.IO;
using System;
using System.Text;
using System.Xml.Linq;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
    public class AIRAEP2UEvent_Tests
    {
        const string filePath = "AIRAEP2UEvent.TestFiles.";
        const string xsl = "AIRAEP2UEvent.MapFiles.AIRAEP2UEvent.xsl";

        [Fact]
        public void AssertMapping1_Message1()
        {
            var input = filePath + "Test01_Message1_input.xml";
            var expectedOutput = filePath + "Test01_Message1_output.xml";

            var subscriptionData = GetEmbeddedResource(filePath + "Test01_SubscriptionData.xml");
            var subscriptionDataString =
                XDocument.Load(subscriptionData)
                    .ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIDateMapper= new Mock<IDateMapper>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGTVWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "198801949D201707078106E", "@referenceType", "ManifestNumber")).Returns("MAN0000092");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "MAWB0707003HAWB0707001MAN000009200005E", "@referenceType", "CNRF")).Returns("000022");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                    "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "MAWB0707003HAWB0707001MAN000009200006E", "@referenceType", "CNRF"))
                .Returns("000011|00002413|000031337");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGTVWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "MAWB0707003HAWB0707002MAN000009200007E", "@referenceType", "CNRF")).Returns("");
            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGTVWGT001", "PRET1.PRET001",
                "198801949D201707078106E", subscriptionDataString, "AIRAEP"));
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }


        [Fact]
        public void AssertMapping1_Message2()
        {
            var input = filePath + "Test01_Message2_input.xml";
            var expectedOutput = filePath + "Test01_Message2_output.xml";

            var subscriptionData = GetEmbeddedResource(filePath + "Test01_SubscriptionData.xml");
            var subscriptionDataString =
                XDocument.Load(subscriptionData)
                    .ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIDateMapper= new Mock<IDateMapper>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGTVWGT001");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "PRET1.PRET001", "@recipientId", "VWGTVWGT001", "@ST_ID", "SGCMSG", "@value",
                "198801949D201707078106E", "@referenceType", "AIRAEP")).Returns(subscriptionDataString);
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-23T11:30:00");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        [Fact]
        public void AssertMapping2_Message1()
        {
            var input = filePath + "Test02_Message1_input.xml";
            var expectedOutput = filePath + "Test02_Message1_output.xml";

            var subscriptionData = GetEmbeddedResource(filePath + "Test02_SubscriptionData.xml");
            var subscriptionDataString =
                XDocument.Load(subscriptionData)
                    .ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIDateMapper= new Mock<IDateMapper>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x =>
                    x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"))
                .Returns("Source");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("Destination");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value",
                "0000000004000000050006E", "@referenceType", "ManifestNumber")).Returns("MAN0000092");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value",
                "MAWB0000014HAWB0000013MAN000009200012E", "@referenceType", "CNRF")).Returns("0000120|0000221");
            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "Source", "Destination",
                "0000000004000000050006E", subscriptionDataString, "AIRAEP"));
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2020-04-13T01:00:00");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        [Fact]
        public void AssertMapping2_Message2()
        {
            var input = filePath + "Test02_Message2_input.xml";
            var expectedOutput = filePath + "Test02_Message2_output.xml";

            var subscriptionData = GetEmbeddedResource(filePath + "Test02_SubscriptionData.xml");
            var subscriptionDataString =
                XDocument.Load(subscriptionData)
                    .ToString(SaveOptions.DisableFormatting); // XDocument is used to strip formatting

            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIDateMapper= new Mock<IDateMapper>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x =>
                    x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"))
                .Returns("Source");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("Destination");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "Source", "@recipientId", "Destination", "@ST_ID", "SGCMSG", "@value",
                "0000000004000000050006E", "@referenceType", "AIRAEP")).Returns(subscriptionDataString);
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2020-04-13T04:13:00");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        public Stream GetEmbeddedResource(string resourceName)
        {
            string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
            if (resource == null)
            {
                throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
            }
            return resource;
        }
    }
}