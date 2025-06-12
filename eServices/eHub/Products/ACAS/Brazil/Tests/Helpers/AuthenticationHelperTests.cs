using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AuthenticationHelper = CargoWise.eHub.Products.ACAS.BR.Helpers.AuthenticationHelper;
using System.Xml;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Xml.Linq;


namespace CargoWise.eHub.Products.ACAS.BR.Tests.Helpers
{
    [TestClass]
    public class AuthenticationHelperTests : TestBase
    {
        
        [TestMethod]
        public void TestGetAndUpdateAuthenticationToken_Success()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var headers = SetupTestHttpHeaders();
            var authenticationHeaders = AuthenticationHelper.UpdateAuthenticationToken("AAABBB", "BOB", headers.FirstOrDefault(x=>x.Key == "Valid").Value);
            var expectedString = XDocument.Load(GetEmbeddedResource("Helpers.TestFiles.UpdateAuthenticationToken_Expected.xml")).ToString(SaveOptions.DisableFormatting);
            var variableElements = new String[3] {"CD_IssuedUTC", "CD_ExpiryUTC", "X-CSRF-Expiration"};
            Assert.AreEqual(RemoveVariableValueElements(expectedString, variableElements), RemoveVariableValueElements(authenticationHeaders.OuterXml, variableElements));
            var token = ExtractAuthenticationValuesFromHeadersXML(authenticationHeaders);
            Assert.IsTrue(token.Item1 == "Test1" && token.Item2 == "BDB7B67C-6B14-4C91-9062-9A22FE6393ED" && DateTime.UtcNow < AuthenticationHelper.UnixTimeStampToDateTime(token.Item3));
        }

        [TestMethod]
        public void TestGetAndUpdateAuthenticationToken_Failure_BadHeaders()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var headers = SetupTestHttpHeaders().FirstOrDefault(x => x.Key == "BadFormat").Value;
            try
            {
                var authenticationHeaders = AuthenticationHelper.UpdateAuthenticationToken("AAABBB", "BOB", headers);
                Assert.Fail();
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(e.Message, String.Format(@"Http Headers configuration invalid. Provided headers for
Client System: AAABBB; Staff Code: BOB, was: {0}", headers));
            }
        }

        [TestMethod]
        public void TestGetAndUpdateAuthenticationToken_Failure_BadRego()
        {
           var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var headers = SetupTestHttpHeaders().FirstOrDefault(x => x.Key == "Valid").Value;
            try
            {
                var authenticationHeaders = AuthenticationHelper.UpdateAuthenticationToken("EEEFFF", "BOB", headers);
                Assert.Fail();
            }
            catch (InvalidOperationException e)
            {
                Assert.IsTrue(e.Message.Contains("Client System Registration does not exists."));
            }
        }

        [TestMethod]
        public void TestGetAndUpdateAuthenticationToken_IgnoreCase()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var testCases = new[]
            {
                "x-csrf-expiration", "X-CSRF-EXPIRATION", "X-Csrf-Expiration", "x-cSRF-eXPIRATION"
            };

            foreach (var testCase in testCases)
            {
                var headers = $@"Accept: text/html
Accept-Charset: utf-8
Accept-Language: en-US
Access-Control-Request-Method: GET
Date: Tue, 1 Jan 2020 00:00:00 GMT
Authorization: Test1
X-CSRF-Token: BDB7B67C-6B14-4C91-9062-9A22FE6393ED
{testCase}: {DateTimeToUnix(DateTime.UtcNow.AddHours(1))}";

                try
                {
                    AuthenticationHelper.UpdateAuthenticationToken("AAABBB", "BOB", headers);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Header X-CSRF-Expiration should be case insensitive, but {testCase} caught exception: {ex.Message}");
                }
            }
        }

        [TestMethod]
        public void TestGetValidClientSystemRegistrationConfigXml_Success()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var reg = AuthenticationHelper.GetValidClientSystemRegistrationConfigXml("AAABBB", "BOB");
            Assert.AreEqual(reg["XCSRFToken"].ToString(), "79E85CA37351BBADF02661F64FC21D3C");
            Assert.AreEqual(reg["Authorization"].ToString(), "Test1");          
        }

        [TestMethod]
        public void TestGetValidClientSystemRegistrationConfigXml_Failure_NoRego()
        {
            var Registraion = AuthenticationHelper.GetValidClientSystemRegistrationConfigXml("", "");
            Assert.IsNull(Registraion);
        }

        [TestMethod]
        public void TestGetValidClientSystemRegistrationConfigXml_Failure_Expired()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var Registraion = AuthenticationHelper.GetValidClientSystemRegistrationConfigXml("AAABBB", "EXPIRE");
            Assert.IsNull(Registraion);
        }

        [TestMethod]
        public void TestGetValidClientSystemRegistrationConfigXml_Failure_NullDate()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var Registraion = AuthenticationHelper.GetValidClientSystemRegistrationConfigXml("AAABBB", "NODATE");
            Assert.IsNull(Registraion);
        }
        
        [TestMethod]
        public void TestGetValidClientSystemRegistrationConfigXml_Failure_BadFlag()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var Registraion = AuthenticationHelper.GetValidClientSystemRegistrationConfigXml("AAABBB", "BADFLG");
            Assert.IsNull(Registraion);
        }

        [TestMethod]
        public void TestGetValidClientSystemRegistrationConfigXml_Failure_NullFlag()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var Registraion = AuthenticationHelper.GetValidClientSystemRegistrationConfigXml("AAABBB", "NOFLAG");
            Assert.IsNull(Registraion);
        }

        [TestMethod]
        public void TestTimeConversionFunctions()
        {
            var unixTime = "5837297596872";
            var expectedDateTime = new DateTime(2154, 12, 23, 7, 33, 16, 872);
            var dateTime = AuthenticationHelper.UnixTimeStampToDateTime(unixTime);
            Assert.AreEqual(dateTime, expectedDateTime);
            
            var gen = new Random();
            for (int i=0;i<50;i++)
            {
                var randomUnixTime = gen.Next(0, 1000000).ToString() + gen.Next(0, 10000000).ToString();
                var randomDateTime = AuthenticationHelper.UnixTimeStampToDateTime(randomUnixTime);
                Assert.AreEqual(randomUnixTime, DateTimeToUnix(randomDateTime));
            }
        }

        [TestMethod]
        public void TestGetAndUpdateAuthenticationTokenForDifferentCodeAndQualifier()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);
            var headers = SetupTestHttpHeaders();
            var registrationExists = true;
            try
            {
                var authenticationHeaders = AuthenticationHelper.UpdateAuthenticationToken("AAABBB", "JAMES", headers.FirstOrDefault(x => x.Key == "Valid").Value);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Client System Registration does not exists")) registrationExists = false;
            }
            Assert.IsFalse(registrationExists, "The CD_Code value should not be used to find the Client System Registration.");
            try
            {
                var authenticationHeaders = AuthenticationHelper.UpdateAuthenticationToken("AAABBB", "JOHN", headers.FirstOrDefault(x => x.Key == "Valid").Value);
                registrationExists = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Client System Registration does not exists")) registrationExists = false;
            }
            Assert.IsTrue(registrationExists, "The CD_Code value should be used to find the Client System Registration.");
        }

        private void SetupTestData(eHubTransactionsContext mockeHubTransactionsContext)
        {
            AuthenticationHelper.GetContext = () => mockeHubTransactionsContext;

            var clientID = Guid.NewGuid();
			var clientID1 = Guid.NewGuid();

			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>();
			var regType = eHubRegistrationTypes.Add(new eHubRegistrationType
			{
				RT_PK = Guid.NewGuid(),
				RT_ID = "ACAS_BR"
			});

			var regTypeToken = eHubRegistrationTypes.Add(new eHubRegistrationType
			{
				RT_PK = Guid.NewGuid(),
				RT_ID = "ACAS_BRToken"
			});

			var eHubClientSystems = new TestDbSet<eHubClientSystem>
			{
				new eHubClientSystem {EH_PK = Guid.NewGuid(), EH_ID = "AAABBB"},
				new eHubClientSystem {EH_PK = Guid.NewGuid(), EH_ID = "BBBCCC"}
			};

			var eHubClients = new TestDbSet<eHubClient> {new eHubClient {CC_PK = clientID, CC_ID = "AAADTWBBB"}};
			eHubClients.Add(new eHubClient {CC_PK = clientID1, CC_ID = "BBBFFFCCC"});

			var eHubClientRegistrations = new TestDbSet<eHubClientRegistration>
			{
				new eHubClientRegistration
				{
					CX_PK = Guid.NewGuid(),
					eHubRegistrationType = regType,
					eHubClient = eHubClients.FirstOrDefault(x => x.CC_ID == "BBBFFFCCC"),
					CX_Code = "BBBFFFCCC",
					CX_Flag1 = 1
				}
			};
            
			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
                    CD_PK = new Guid("8991E94B-BDB0-4894-B615-BFCADD615099"),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "AAABBB"),
					CD_Code = "BOB",
                    CD_ExpiryUTC = DateTime.UtcNow.AddHours(1),
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test1</Authorization>
    <X-CSRF-Token>79E85CA37351BBADF02661F64FC21D3C</X-CSRF-Token>
    <X-CSRF-Expiration>{0}</X-CSRF-Expiration>
</Config>", DateTimeToUnix(DateTime.UtcNow.AddHours(1))),
					CD_Flag1 = 1
				},
                new eHubClientSystemRegistration
				{
                    CD_PK = Guid.NewGuid(),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "AAABBB"),
					CD_Code = "EXPIRE",
                    CD_ExpiryUTC = DateTime.UtcNow.AddHours(-1),
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test1</Authorization>
    <X-CSRF-Token>79E85CA37351BBADF02661F64FC21D3C</X-CSRF-Token>
    <X-CSRF-Expiration>{0}</X-CSRF-Expiration>
</Config>", DateTimeToUnix(DateTime.UtcNow.AddHours(-1))),
					CD_Flag1 = 1
				},
                new eHubClientSystemRegistration
				{
                    CD_PK = Guid.NewGuid(),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "AAABBB"),
					CD_Code = "NODATE",
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test1</Authorization>
    <X-CSRF-Token>79E85CA37351BBADF02661F64FC21D3C</X-CSRF-Token>
</Config>"),
					CD_Flag1 = 1
				},
                new eHubClientSystemRegistration
				{
                    CD_PK = Guid.NewGuid(),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "AAABBB"),
					CD_Code = "BADFLG",
                    CD_ExpiryUTC = DateTime.UtcNow.AddHours(1),
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test1</Authorization>
    <X-CSRF-Token>79E85CA37351BBADF02661F64FC21D3C</X-CSRF-Token>
    <X-CSRF-Expiration>{0}</X-CSRF-Expiration>
</Config>", DateTimeToUnix(DateTime.UtcNow.AddHours(1))),
					CD_Flag1 = 0
				},
                new eHubClientSystemRegistration
				{
                    CD_PK = Guid.NewGuid(),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "AAABBB"),
					CD_Code = "NOFLAG",
                    CD_ExpiryUTC = DateTime.UtcNow.AddHours(1),
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test1</Authorization>
    <X-CSRF-Token>79E85CA37351BBADF02661F64FC21D3C</X-CSRF-Token>
    <X-CSRF-Expiration>{0}</X-CSRF-Expiration>
</Config>", DateTimeToUnix(DateTime.UtcNow.AddHours(1)))
				},
                new eHubClientSystemRegistration
				{
                    CD_PK = new Guid("AA9C1970-0C10-43CB-86D0-4BF7169AE725"),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "BBBCCC"),
					CD_Code = "BOB",
                    CD_ExpiryUTC = DateTime.UtcNow,
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test2</Authorization>
    <X-CSRF-Token>56247986-B153-4CD7-8EC2-8C91279B0D9D</X-CSRF-Token>
    <X-CSRF-Expiration>{0}</X-CSRF-Expiration>
</Config>", DateTimeToUnix(DateTime.UtcNow.AddHours(-1))),
					CD_Flag1 = 0
				},
                 new eHubClientSystemRegistration
				{
                    CD_PK = Guid.NewGuid(),
					eHubRegistrationType = regTypeToken,
					eHubClientSystem = eHubClientSystems.First(x => x.EH_ID == "AAABBB"),
					CD_Code = "JOHN",
                    CD_ExpiryUTC = DateTime.UtcNow,
                    CD_ConfigXml = String.Format(@"<Config>
    <Authorization>Test2</Authorization>
    <X-CSRF-Token>56247986-B153-4CD7-8EC2-8C91279B0D9D</X-CSRF-Token>
    <X-CSRF-Expiration>{0}</X-CSRF-Expiration>
</Config>", DateTimeToUnix(DateTime.UtcNow.AddHours(-1))),
					CD_Flag1 = 0
				}
			};

            mockeHubTransactionsContext.Stub(x => x.eHubRegistrationTypes).Return(eHubRegistrationTypes);
            mockeHubTransactionsContext.Stub(x => x.eHubClientSystemRegistrations).Return(eHubClientSystemRegistrations);
            mockeHubTransactionsContext.Stub(x => x.eHubClientSystems).Return(eHubClientSystems);
            mockeHubTransactionsContext.Stub(x => x.SaveChanges()).Return(0);

        }

        private static Dictionary<string, string> SetupTestHttpHeaders()
        {
            var headers = new Dictionary<string, string>();

            headers.Add("Valid", String.Format(@"Accept: text/html
set-token: Test1
Accept-Charset: utf-8
Accept-Language: en-US
Access-Control-Request-Method: GET
Date: Tue, 1 Jan 2020 00:00:00 GMT
X-CSRF-Token: BDB7B67C-6B14-4C91-9062-9A22FE6393ED
X-CSRF-Expiration: {0}", DateTimeToUnix(DateTime.UtcNow.AddHours(1))));

            headers.Add("Expired", String.Format(@"Accept: text/html
set-token: Test1
Accept-Charset: utf-8
Accept-Language: en-US
Access-Control-Request-Method: GET
Date: Tue, 1 Jan 2020 00:00:00 GMT
X-CSRF-Token: DE929F7A-00BC-46CC-A031-8E446F671EFE
X-CSRF-Expiration: {0}", DateTimeToUnix(DateTime.UtcNow.AddHours(-1))));

            headers.Add("BadFormat", "Bad Format");

            return headers;
        }

        private static Tuple<string, string, string> ExtractAuthenticationValuesFromHeadersXML(XmlDocument xmlDoc)
        {
            if (xmlDoc.InnerXml == "") return null;
            var headersXml = new XmlDocument();
            headersXml.LoadXml(xmlDoc.SelectSingleNode("//CompositeOperation").FirstChild.FirstChild.FirstChild.FirstChild.ChildNodes[2].InnerText);
            return new Tuple<string, string, string>(headersXml.SelectSingleNode("//Config/Authorization/text()").Value,
                headersXml.SelectSingleNode("//Config/X-CSRF-Token/text()").Value,
                headersXml.SelectSingleNode("//Config/X-CSRF-Expiration/text()").Value);
        }

        private string RemoveVariableValueElements(string xmlString, string[] elementNames)
        {
            var result = xmlString;

            foreach (var elementName in elementNames)
            {
                var index1 = result.IndexOf("<" + elementName);
                var index2 = result.IndexOf("</" + elementName + ">") + elementName.Length + 3;

                if (index1 > 0 && index2 > index1)
                {
                    result = result.Remove(index1, index2 - index1);
                }

                index1 = result.IndexOf("&lt;" + elementName);
                index2 = result.IndexOf("&lt;/" + elementName + "&gt;") + elementName.Length + 5;

                if (index1 > 0 && index2 > index1)
                {
                    result = result.Remove(index1, index2 - index1);
                }
            }
            return result;
        }

        private static string DateTimeToUnix (DateTime time)
        {
            return ((Int64)time.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
        }

    }
}
