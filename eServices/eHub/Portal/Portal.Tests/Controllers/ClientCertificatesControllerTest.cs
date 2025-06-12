using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View.Certificate;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
    [TestClass]
    public class ClientCertificatesControllerTest : BaseControllerTest<ClientCertificatesController>
    {
        [TestMethod]
        public void TestIndex()
        {
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
        }

        [TestMethod]
        public void TestCertificatesGet()
        {
            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["sidx"] = "CE_CC_ID";
            request.Container["sord"] = "asc";

            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
            var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
            var eh1 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), EH_ID = "WTLEHK" };
            var eh2 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), EH_ID = "HYEUAT" };
            var ce1 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", eHubClient = cc1, eHubClientSystem = eh1, CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_TextContainer = "pem text content", CE_BinaryContainer = null, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" };
            var ce2 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 2", CE_ID = "test2", eHubClient = cc2, eHubClientSystem = eh2, CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_TextContainer = "", CE_BinaryContainer = Encoding.UTF8.GetBytes("0x 58 az xa sd"), CE_Thumbprint = "thumbprint 2", CE_Password = "password 2" };

            context.eHubCertificates.AddObject(ce1);
            context.eHubCertificates.AddObject(ce2);

            var result = controller.Certificates();

            Assert.IsNotNull(result);
            AssertEx.JsonResultMatchesList(new List<object> { new {CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" },
                                                              new {CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 2", CE_ID = "test2", CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_Thumbprint = "thumbprint 2", CE_Password = "password 2"}},
                result, "eHubClientCertificates");
        }

        [TestMethod]
        public void TestCertificates_MultipleFilter_AND()
        {
            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{\"field\":\"CE_Category\",\"op\":\"bw\",\"data\":\"Category 1\"},{\"field\":\"CE_ID\",\"op\":\"bw\",\"data\":\"test1\"}]}";

            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
            var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
            var eh1 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), EH_ID = "WTLEHK" };
            var eh2 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), EH_ID = "HYEUAT" };
            var ce1 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", eHubClient = cc1, eHubClientSystem = eh1, CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_TextContainer = "pem text content", CE_BinaryContainer = null, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" };
            var ce2 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 2", CE_ID = "test3", eHubClient = cc2, eHubClientSystem = eh2, CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_TextContainer = "", CE_BinaryContainer = Encoding.UTF8.GetBytes("0x 58 az xa sd"), CE_Thumbprint = "thumbprint 2", CE_Password = "password 2" };
            var ce3 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 1", CE_ID = "test2", eHubClient = cc2, eHubClientSystem = eh2, CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_TextContainer = "", CE_BinaryContainer = Encoding.UTF8.GetBytes("0x 58 az xa sd"), CE_Thumbprint = "thumbprint 2", CE_Password = "password 2" };

            context.eHubCertificates.AddObject(ce1);
            context.eHubCertificates.AddObject(ce2);
            context.eHubCertificates.AddObject(ce3);

            var result = controller.Certificates();

            Assert.IsNotNull(result);
            AssertEx.JsonResultMatchesList(new List<object> { new { CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" } },
                result, "eHubClientCertificates");
        }

        [TestMethod]
        public void TestCertificates_MultipleFilter_OR()
        {
            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["filters"] = "{\"groupOp\":\"OR\",\"rules\":[{\"field\":\"CE_Category\",\"op\":\"bw\",\"data\":\"Category 1\"},{\"field\":\"CE_ID\",\"op\":\"bw\",\"data\":\"test1\"}]}";

            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
            var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
            var eh1 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), EH_ID = "WTLEHK" };
            var eh2 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), EH_ID = "HYEUAT" };
            var ce1 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", eHubClient = cc1, eHubClientSystem = eh1, CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_TextContainer = "pem text content", CE_BinaryContainer = null, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" };
            var ce2 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 2", CE_ID = "test3", eHubClient = cc2, eHubClientSystem = eh2, CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_TextContainer = "", CE_BinaryContainer = Encoding.UTF8.GetBytes("0x 58 az xa sd"), CE_Thumbprint = "thumbprint 2", CE_Password = "password 2" };
            var ce3 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 1", CE_ID = "test2", eHubClient = cc2, eHubClientSystem = eh2, CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_TextContainer = "", CE_BinaryContainer = Encoding.UTF8.GetBytes("0x 58 az xa sd"), CE_Thumbprint = "thumbprint 2", CE_Password = "password 2" };

            context.eHubCertificates.AddObject(ce1);
            context.eHubCertificates.AddObject(ce2);
            context.eHubCertificates.AddObject(ce3);

            var result = controller.Certificates();

            Assert.IsNotNull(result);
            AssertEx.JsonResultMatchesList(new List<object> { new {CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" },
                                                              new {CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 1", CE_ID = "test2", CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_Thumbprint = "thumbprint 2", CE_Password = "password 2"}},
                result, "eHubClientCertificates");
        }

        [TestMethod]
        public void TestRegistrationsEdit()
        {
            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            ClientCertificatesController.GetDateTimeUtcNow = () => new DateTime(2016, 11, 1);

            var cc = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
            context.eHubClients.AddObject(cc);

            var eh = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), EH_ID = "WTLEHK" };
            context.eHubClientSystems.AddObject(eh);

            request.Clear();
            request.Container["id"] = "_empty";
            request.Container["CE_CC_ID"] = "CLIENT001";
            request.Container["CE_EH_ID"] = "WTLEHK";
            request.Container["oper"] = "add";
            request.Container["CE_ID"] = "test id";
            request.Container["CE_Category"] = "Category 1";
            request.Container["CE_ContainerType"] = ContainerType.PEM_Text.ID;
            request.Container["CE_Thumbprint"] = "thumbprint 1";
            request.Container["CE_Password"] = "password 1";

            request.AddFile("certificate_inputFile", "pem text content.");

            JObject responseAdd = JObject.Parse(CertificateEditTest(logger));

            var resultState = (bool)responseAdd["success"];
            Assert.IsTrue(resultState);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCertificate:"));
            Assert.IsTrue(logger.Log.Contains("CE_Category=Category 1, CE_ID=test id, CE_CC_Owner=00000000-cccc-1111-1111-000000000000, CE_EH_Owner=00000000-eeee-1111-1111-000000000000, CE_ValidFromUTC=, CE_ValidToUTC=, CE_ActiveFromUTC=, CE_AddedUTC=2016-11-01 12:00:00.000, CE_ContainerType=pem-text, CE_Password=password 1, CE_Thumbprint=, CE_Issuer=, CE_SerialNumber=, CE_SubjectKeyIdentifier="));

            var newId = new Guid((string)responseAdd["id"]);

            var expected = new[] { new eHubCertificate
            {
                CE_PK = newId,
                CE_CC_Owner = cc.CC_PK,
                CE_EH_Owner = eh.EH_PK,
                CE_ContainerType = ContainerType.PEM_Text.ID,
                CE_Password = "password 1",
                CE_TextContainer = "pem text content.",
                CE_Category = "Category 1",
                CE_ID = "test id",
                CE_AddedUTC = new DateTime(2016, 11, 1),
                eHubClient = cc,
                eHubClientSystem = eh
            }};
            var compare = new CompareLogic();
            var compareResult = compare.Compare(expected, context.eHubCertificates.ToArray());
            Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);

            request.Clear();
            request.Container["CE_PK"] = newId.ToString();
            request.Container["CE_CC_ID"] = "CLIENT001";
            request.Container["CE_EH_ID"] = "WTLEHK";
            request.Container["CE_Category"] = "";
            request.Container["CE_ID"] = "test id 1";
            request.Container["CE_ActiveFromUtc"] = "2016/11/01 12:00 AM";
            request.Container["CE_ContainerType"] = ContainerType.PKCS12_Binary.ID;
            request.Container["CE_Password"] = "miragef7";
            request.Container["oper"] = "edit";

            var content = TestFiles.TestFileHelpers.GetResourceStream("00505655TST.pfx");
            var contentData = TestFiles.TestFileHelpers.GetResourceData("00505655TST.pfx");
            var contentBase64 = Convert.ToBase64String(TestFiles.TestFileHelpers.GetResourceData("00505655TST.pfx"));
            request.AddFile("certificate_inputFile", content);

            JObject responseEdit = JObject.Parse(CertificateEditTest(logger));

            Assert.IsTrue((bool)responseEdit["success"]);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCertificate:"));
            Assert.IsTrue(logger.Log.Contains("CE_Category=, CE_ID=test id 1, CE_CC_Owner=00000000-cccc-1111-1111-000000000000, CE_EH_Owner=00000000-eeee-1111-1111-000000000000, CE_ValidFromUTC=2015-10-09 10:00:34.000, CE_ValidToUTC=2018-10-09 10:30:34.000, CE_ActiveFromUTC=2016-11-01 12:00:00.000, CE_AddedUTC=2016-11-01 12:00:00.000, CE_ContainerType=pkcs12-binary, CE_Password=miragef7, CE_Thumbprint=66F05BCE19379888887591E62DE922B20CF3453F, CE_Issuer=CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA, CE_SerialNumber=551AA44B, CE_SubjectKeyIdentifier=B514875295CE13CBC0AF453C6C02572CF902BD7D"));

            expected[0].CE_Category = "";
            expected[0].CE_ID = "test id 1";
            expected[0].CE_Password = "miragef7";
            expected[0].CE_ContainerType = ContainerType.PKCS12_Binary.ID;
            expected[0].CE_TextContainer = null;
            expected[0].CE_BinaryContainer = contentData;
            expected[0].CE_Thumbprint = "66F05BCE19379888887591E62DE922B20CF3453F";
            expected[0].CE_Issuer = "CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA";
            expected[0].CE_SerialNumber = "551AA44B";
            expected[0].CE_SubjectKeyIdentifier = "B514875295CE13CBC0AF453C6C02572CF902BD7D";
            expected[0].CE_ValidFromUTC = new DateTime(2015, 10, 9, 10, 0, 34);
            expected[0].CE_ValidToUTC = new DateTime(2018, 10, 9, 10, 30, 34);
            expected[0].CE_ActiveFromUTC = new DateTime(2016, 11, 1);
            compare = new CompareLogic();
            compareResult = compare.Compare(expected, context.eHubCertificates.ToArray());
            Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);

            request.Clear();
            request.Container["CE_PK"] = newId.ToString();
            request.Container["CE_ContainerType"] = ContainerType.PEM_Text.ID;

            JObject responseEdit2 = JObject.Parse(CertificateEditTest(logger));

            Assert.IsFalse((bool)responseEdit2["success"]);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCertificate:"));
            Assert.IsTrue(logger.Log.Contains("CE_Category=, CE_ID=test id 1, CE_CC_Owner=00000000-cccc-1111-1111-000000000000, CE_EH_Owner=00000000-eeee-1111-1111-000000000000, CE_ValidFromUTC=2015-10-09 10:00:34.000, CE_ValidToUTC=2018-10-09 10:30:34.000, CE_ActiveFromUTC=2016-11-01 12:00:00.000, CE_AddedUTC=2016-11-01 12:00:00.000, CE_ContainerType=pkcs12-binary, CE_Password=miragef7, CE_Thumbprint=66F05BCE19379888887591E62DE922B20CF3453F, CE_Issuer=CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA, CE_SerialNumber=551AA44B, CE_SubjectKeyIdentifier=B514875295CE13CBC0AF453C6C02572CF902BD7D"));
            Assert.AreEqual("New File must be submitted with the ContainerType change.", (string)responseEdit2["message"]);
            compare = new CompareLogic();
            compareResult = compare.Compare(expected, context.eHubCertificates.ToArray());
            Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);

            request.Clear();
            request.Container["CE_PK"] = newId.ToString();
            request.Container["oper"] = "del";

            var responseDel = CertificateEditJQGridTest(logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubCertificate:"));
            Assert.IsTrue(logger.Log.Contains("CE_Category=, CE_ID=test id 1, CE_CC_Owner=00000000-cccc-1111-1111-000000000000, CE_EH_Owner=00000000-eeee-1111-1111-000000000000, CE_ValidFromUTC=2015-10-09 10:00:34.000, CE_ValidToUTC=2018-10-09 10:30:34.000, CE_ActiveFromUTC=2016-11-01 12:00:00.000, CE_AddedUTC=2016-11-01 12:00:00.000, CE_ContainerType=pkcs12-binary, CE_Password=miragef7, CE_Thumbprint=66F05BCE19379888887591E62DE922B20CF3453F, CE_Issuer=CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA, CE_SerialNumber=551AA44B, CE_SubjectKeyIdentifier=B514875295CE13CBC0AF453C6C02572CF902BD7D"));
            Assert.AreEqual(0, context.eHubClientRegistrations.Count());

            request.Clear();
            request.Container["CE_PK"] = newId.ToString();
            request.Container["oper"] = "edit";

            var responseEdit1 = CertificateEditJQGridTest(logger);

            Assert.IsFalse((bool)responseEdit1.Data.GetType().GetProperty("success").GetValue(responseEdit1.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubCertificate:"));
            Assert.IsTrue(logger.Log.Contains("CE_Category=, CE_ID=test id 1, CE_CC_Owner=00000000-cccc-1111-1111-000000000000, CE_EH_Owner=00000000-eeee-1111-1111-000000000000, CE_ValidFromUTC=2015-10-09 10:00:34.000, CE_ValidToUTC=2018-10-09 10:30:34.000, CE_ActiveFromUTC=2016-11-01 12:00:00.000, CE_AddedUTC=2016-11-01 12:00:00.000, CE_ContainerType=pkcs12-binary, CE_Password=miragef7, CE_Thumbprint=66F05BCE19379888887591E62DE922B20CF3453F, CE_Issuer=CN=LAWtrust2048 CA2, O=LAWtrust, C=ZA, CE_SerialNumber=551AA44B, CE_SubjectKeyIdentifier=B514875295CE13CBC0AF453C6C02572CF902BD7D"));
            Assert.AreEqual("invalid oper action: edit", (string)responseEdit1.Data.GetType().GetProperty("message").GetValue(responseEdit1.Data, null));
        }

        [TestMethod]
        public void TestDownloadCertificate()
        {
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
            var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
            var eh1 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), EH_ID = "WTLEHK" };
            var eh2 = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), EH_ID = "HYEUAT" };
            var ce1 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CE_Category = "Category 1", CE_ID = "test1", eHubClient = cc1, eHubClientSystem = eh1, CE_AddedUTC = Convert.ToDateTime("01/02/2016"), CE_ContainerType = ContainerType.PEM_Text.ID, CE_TextContainer = "pem text content", CE_BinaryContainer = null, CE_Thumbprint = "thumbprint 1", CE_Password = "password 1" };
            var ce2 = new eHubCertificate { CE_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CE_Category = "Category 2", CE_ID = "test2", eHubClient = cc2, eHubClientSystem = eh2, CE_AddedUTC = Convert.ToDateTime("02/03/2016"), CE_ContainerType = ContainerType.PKCS12_Binary.ID, CE_TextContainer = "", CE_BinaryContainer = Encoding.UTF8.GetBytes("0x 58 az xa sd"), CE_Thumbprint = "thumbprint 2", CE_Password = "password 2" };

            context.eHubCertificates.AddObject(ce1);
            context.eHubCertificates.AddObject(ce2);

            request.Clear();
            request.Container["CE_PK"] = "00000000-FFFF-1111-1111-000000000000";

            var result = controller.DownloadCertificate();

            Assert.AreEqual("pem text content", Encoding.Default.GetString(result.FileContents));
            Assert.AreEqual("test1.pem", result.FileDownloadName);

            request.Clear();
            request.Container["CE_PK"] = "00000000-FFFF-1111-2222-000000000000";

            var result2 = controller.DownloadCertificate();

            Assert.AreEqual("0x 58 az xa sd", Encoding.Default.GetString(result2.FileContents));
            Assert.AreEqual("test2.pfx", result2.FileDownloadName);
        }

        [TestMethod]
        public void TestContainerTypes()
        {
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;

            Assert.AreEqual("x509-text:X.509 Certificate - x509-text (*.cer,*.crt,*.der);x509-binary:X.509 Certificate - x509-binary (*.cer,*.crt,*.der);pem-text:S/MIME: Privacy Enhanced Mail - pem-text (*.pem);pkcs12-binary:Personal Information Exchange - pkcs12-binary (*.pfx,*.p12);pkcs7-text:Cryptographic Message Syntax Standard - PKCS #7 Certficates - pkcs7-text (*.p7b,*.spc);pkcs7-binary:Cryptographic Message Syntax Standard - PKCS #7 Certficates - pkcs7-binary (*.p7b,*.spc);openssh-text:OpenSSH certificates - openssh-text (*.*)",
                controller.ContainerTypes());
        }

        [TestMethod]
        public void TestClientsGetIncludeNonProdCWSystems()
        {
            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "8";
            request.Container["sidx"] = "CC_ID";
            request.Container["sord"] = "asc";
            request.Container["_search"] = "false";
            IList<object> expected1 = new List<object>
            {
                new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
                new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
                new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
                new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
                new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
                new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
                new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
            };

            var result1 = controller.Clients(true);

            Assert.IsNotNull(result1);
            AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["sidx"] = "CC_FriendlyName";
            request.Container["sord"] = "desc";
            request.Container["_search"] = "true";
            request.Container["CC_ID"] = "TEST";
            IList<object> expected2 = new List<object>
            {
                new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
                new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
                new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
            };

            var result2 = controller.Clients(true);

            Assert.IsNotNull(result2);
            AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");

            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["sidx"] = "CC_FriendlyName";
            request.Container["sord"] = "asc";
            request.Container["_search"] = "true";
            request.Container["CC_ID"] = "TEST";
            request.Container["CC_FriendlyName"] = "3";
            IList<object> expected3 = new List<object>
            {
                new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
            };

            var result3 = controller.Clients(true);

            Assert.IsNotNull(result2);
            AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients");
        }

        [TestMethod]
        public void TestClientSystemsGetncludeNonProdCWSystems()
        {
            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "8";
            request.Container["sidx"] = "EH_ID";
            request.Container["sord"] = "asc";
            request.Container["_search"] = "false";
            IList<object> expected1 = new List<object>
            {
                new { EH_ID = "Test WTLEHK1" },
                new { EH_ID = "Test WTLEHK2" },
                new { EH_ID = "WTLEHK3" },
                new { EH_ID = "WTLEHK4" },
				new { EH_ID = "WTLSV1" },
				new { EH_ID = "WTLSV2" },
			};

            var result1 = controller.ClientSystems(true);

            Assert.IsNotNull(result1);
            AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystems", true);

            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["sidx"] = "EH_ID";
            request.Container["sord"] = "desc";
            request.Container["_search"] = "true";
            request.Container["EH_ID"] = "1";
            IList<object> expected2 = new List<object>
            {
                new { EH_ID = "WTLEHK1" },
            };

            var result2 = controller.ClientSystems(true);

            Assert.IsNotNull(result2);
            AssertEx.JsonResultMatchesList(expected2, result2, "eHubClientSystems", true);
        }

		[TestMethod]
		public void TestClientsExcludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
				new { CC_ID = "CLIENT0007", CC_FriendlyName = "Client 7" },
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
				new { CC_ID = "CLIENT0007", CC_FriendlyName = "Client 7" },
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		[TestMethod]
		public void TestClientsFilterCaseInsensitive()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "test";

			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		[TestMethod]
		public void TestClientSystemsFilterCaseInsensitive()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "wtlsv";
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "WTLSV1" },
				new { EH_ID = "WTLSV2" },
			};

			var result1 = controller.ClientSystems(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystems");
		}


		[TestMethod]
		public void TestClientSystemsGetExcludeNonProdCWSystems()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001", EH_URL = "url01.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI002", EH_URL = "url02.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI003", EH_URL = "" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI004", EH_URL = "url04.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI005", EH_URL = "url05.com" });

			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD005", CC_PK = new Guid("{20000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI005", LE_EnterpriseCode = "CLI", LD_LicenceType = "TST", LD_ServerCode = "005" });

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "CLI004"}
			};

			var result1 = controller.ClientSystems(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystems");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "CLI00";
			IList<object> expected3 = new List<object>
			{
				new { EH_ID = "CLI004"}
			};

			var result3 = controller.ClientSystems(false);

			Assert.IsNotNull(result3);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClientSystems");
		}

		[TestMethod]
        public void TestAddCertificateWithouteHubClientAndeHubClientSystem()
        {
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            ClientCertificatesController.GetDateTimeUtcNow = () => new DateTime(2016, 11, 1);

            request.Clear();
            request.Container["id"] = "_empty";
            request.Container["oper"] = "add";
            request.Container["CE_ID"] = "test id";
            request.Container["CE_Category"] = "Category 1";
            request.Container["CE_ContainerType"] = ContainerType.PEM_Text.ID;
            request.Container["CE_Thumbprint"] = "thumbprint 1";
            request.Container["CE_Password"] = "password 1";

            request.AddFile("certificate_inputFile", "pem text content.");

            JObject responseAdd = JObject.Parse(controller.CertificateEdit());

            var resultState = (bool)responseAdd["success"];
            Assert.IsTrue(resultState);

            var newId = new Guid((string)responseAdd["id"]);

            var expected = new[] { new eHubCertificate
            {
                CE_PK = newId,
                CE_CC_Owner = null,
                CE_EH_Owner = null,
                CE_ContainerType = ContainerType.PEM_Text.ID,
                CE_Password = "password 1",
                CE_TextContainer = "pem text content.",
                CE_Category = "Category 1",
                CE_ID = "test id",
                CE_AddedUTC = new DateTime(2016, 11, 1),
                eHubClient = null,
                eHubClientSystem = null
            }};
            var compare = new CompareLogic();
            var compareResult = compare.Compare(expected, context.eHubCertificates.ToArray());
            Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
        }

        [TestMethod]
        public void TestEditCertificateRemoveHubClientAndeHubClientSystem()
        {
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
            controller.Context = context;
            ClientCertificatesController.GetDateTimeUtcNow = () => new DateTime(2016, 11, 1);

            var cc = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
            context.eHubClients.AddObject(cc);

            var eh = new eHubClientSystem { EH_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), EH_ID = "WTLEHK" };
            context.eHubClientSystems.AddObject(eh);

            request.Clear();
            request.Container["id"] = "_empty";
            request.Container["oper"] = "add";
            request.Container["CE_ID"] = "test id";
            request.Container["CE_Category"] = "Category 1";
            request.Container["CE_ContainerType"] = ContainerType.PEM_Text.ID;
            request.Container["CE_Thumbprint"] = "thumbprint 1";
            request.Container["CE_Password"] = "password 1";

            request.AddFile("certificate_inputFile", "pem text content.");

            JObject responseAdd = JObject.Parse(controller.CertificateEdit());

            var resultState = (bool)responseAdd["success"];
            Assert.IsTrue(resultState);

            var newId = new Guid((string)responseAdd["id"]);

            var expected = new[] { new eHubCertificate
            {
                CE_PK = newId,
                CE_CC_Owner = null,
                CE_EH_Owner = null,
                CE_ContainerType = ContainerType.PEM_Text.ID,
                CE_Password = "password 1",
                CE_TextContainer = "pem text content.",
                CE_Category = "Category 1",
                CE_ID = "test id",
                CE_AddedUTC = new DateTime(2016, 11, 1),
                eHubClient = null,
                eHubClientSystem = null
            }};
            var compare = new CompareLogic();
            var compareResult = compare.Compare(expected, context.eHubCertificates.ToArray());
            Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
        }

        protected String CertificateEditTest(ILog logger)
        {
            controller.logger = logger;
            return controller.CertificateEdit();
        }

        protected JsonResult CertificateEditJQGridTest(ILog logger)
        {
            controller.logger = logger;
            return controller.CertificateEditJQGrid();
        }
    }
}
