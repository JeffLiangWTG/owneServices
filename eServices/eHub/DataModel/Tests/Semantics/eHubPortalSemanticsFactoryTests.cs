using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.DataModel.Business.Semantics;

namespace CargoWise.eHub.DataModel.Tests.Semantics
{
    [TestClass]
    public class eHubPortalSemanticsFactoryTests
    {
        [TestMethod]
        public void TestGetITCustomsAccount_ClientSystemRegistrationHeaders()
        {
            var semantics = eHubPortalSemanticsFactory.GetSemantics<string, string>("ITCustomsAccount", "ClientSystemRegistrationHeaders");
            var dictionaryPair = semantics.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

            Assert.AreEqual(10, semantics.Count, "The dictionary will have 10 pair of semantics for IT customs.");
            Assert.AreEqual("CD_Attr1: AccountNumber; CD_Attr2: Password; CD_Code: Node; CD_ConfigXml: ConfigXml; CD_EH_ID: Client System; CD_ExpiryUTC: ExpiryUTC; CD_Flag1: Status; CD_IssuedUTC: IssuedUTC; CD_Qualifier: Qualifier; CD_RT: Registration Type", string.Join("; ", dictionaryPair));
        }

        [TestMethod]
        public void TestGetJPCustomsAccount_ClientSystemRegistrationHeaders()
        {
            var statusList = eHubPortalSemanticsFactory.GetSemantics<string, string>("JPCustomsAccount_SystemLevel", "ClientSystemRegistrationHeaders");
            var dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

            Assert.AreEqual(10, statusList.Count, "The dictionary will have 10 pair of semantics for JP customs.");
            Assert.AreEqual("CD_Attr1: Password; CD_Attr2: Attribute 2; CD_Code: UserName; CD_ConfigXml: ConfigXml; CD_EH_ID: Client System; CD_ExpiryUTC: ExpiryUTC; CD_Flag1: Flag 1; CD_IssuedUTC: IssuedUTC; CD_Qualifier: Qualifier; CD_RT: Registration Type", string.Join("; ", dictionaryPair));
        }

        [TestMethod]
        public void TestGetJPCustomsAccount_ClientRegistrationHeaders()
        {
	        var statusList = eHubPortalSemanticsFactory.GetSemantics<string, string>("JPCustomsAccount_ClientLevel", "ClientRegistrationHeaders");
	        var dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

	        Assert.AreEqual(3, statusList.Count, "The dictionary will have 3 pair of semantics for JP customs.");
			Assert.AreEqual("CX_CC_ID: Client System; CX_Code: UserName; CX_Password1: Password", string.Join("; ", dictionaryPair));
		}

        [TestMethod]
        public void TestGetGBCustomsTLAccount_ClientRegistrationHeaders()
        {
	        var statusList = eHubPortalSemanticsFactory.GetSemantics<string, string>("GBCustoms-Transport", "ClientRegistrationHeaders");
	        var dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

	        Assert.AreEqual(6, statusList.Count, "The dictionary will have 6 pair of semantics for GB customs TL.");
	        Assert.AreEqual("CX_Attr1: Name; CX_CC_ID: Provider; CX_Code: Path; CX_Flag1: Source; CX_Flag2: Type; CX_Qualifier: Service", string.Join("; ", dictionaryPair));
        }

        [TestMethod]
        public void TestGetGBCustomsTLAccount_Flag1Options()
        {
            var statusList = eHubPortalSemanticsFactory.GetSemantics<int, string>("GBCustoms-Transport", "Flag1Options");
            var dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

            Assert.AreEqual(2, statusList.Count, "The dictionary will have 2 pair of semantics for GB customs TL.");
            Assert.AreEqual("0: Synchronous; 1: Notification", string.Join("; ", dictionaryPair));
        }

        [TestMethod]
        public void TestGetGBCustomsTLAccount_Flag2Option()
        {
            var statusList = eHubPortalSemanticsFactory.GetSemantics<int, string>("GBCustoms-Transport", "Flag2Options");
            var dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

            Assert.AreEqual(3, statusList.Count, "The dictionary will have 3 pair of semantics for GB customs TL.");
            Assert.AreEqual("0: Subscribed; 1: Additional; 2: Data", string.Join("; ", dictionaryPair));
        }


        [TestMethod]
        public void TestGetGBCustomsICSNI_ClientSystemRegistrationHeaders()
        {
            var semantics = eHubPortalSemanticsFactory.GetSemantics<string, string>("GBCustoms-ICSNI", "ClientSystemRegistrationHeaders");
            var dictionaryPair = semantics.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

            Assert.AreEqual(10, semantics.Count, "The dictionary will have 10 pairs of semantics for GB customs.");
            Assert.AreEqual("CD_Attr1: Active Subscription; CD_Attr2: Attribute 2; CD_Code: Declarant; CD_ConfigXml: Config Xml; CD_EH_ID: Client System; CD_ExpiryUTC: ExpiryUTC; CD_Flag1: Status; CD_IssuedUTC: Issued UTC; CD_Qualifier: Qualifier; CD_RT: Registration Type", string.Join("; ", dictionaryPair));
        }

        [TestMethod]
        public void TestGetGBCustomsPentant_ClientRegistrationHeaders()
        {
	        var semantics = eHubPortalSemanticsFactory.GetSemantics<string, string>("GBCustoms-Pentant", "ClientRegistrationHeaders");
	        var dictionaryPair = semantics.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

	        Assert.AreEqual(3, semantics.Count, "The dictionary will have 3 pair of semantics for GB customs - Pentant.");
	        Assert.AreEqual("CX_Attr1: URL; CX_CC_ID: Provider; CX_Code: Authorisation", string.Join("; ", dictionaryPair));
        }

		[TestMethod]
		public void TestGetGBCustomsEMCS_ClientRegistrationHeaders()
		{
			var semantics = eHubPortalSemanticsFactory.GetSemantics<string, string>("GBCustoms-EMCS", "ClientRegistrationHeaders");
			var dictionaryPair = semantics.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

			Assert.AreEqual(3, semantics.Count, "The dictionary will have 3 pair of semantics for GB customs - EMCS.");
			Assert.AreEqual("CX_CC_ID: Client; CX_ConfigXml: ConfigXML; CX_Flag1: Should poll", string.Join("; ", dictionaryPair));
		}

		[TestMethod]

        public void TestGetNonExistingProduct()
        {
            var statusList = eHubPortalSemanticsFactory.GetSemantics<string, string>("NonExistingProduct", "Whatever");
            Assert.AreEqual(0, statusList.Count, "The dictionary will have no key and value pair for non existing product.");
        }

        [TestMethod]
        public void TestGetNonExistingSemantics()
        {
            var statusList = eHubPortalSemanticsFactory.GetSemantics<string, string>("SampleProduct1", "NonExistingSemantics");
            Assert.AreEqual(0, statusList.Count, "The dictionary will have no key and value pair for non existing Semantics.");
        }

        [TestMethod]
        public void TestGetIncorrectSemanticsTypes()
        {
            try
            {
                var statusList = eHubPortalSemanticsFactory.GetSemantics<int, string>("ITCustomsAccount", "ClientSystemRegistrationHeaders");
                Assert.Fail("Should throw InvalidCastException");
            }
            catch (InvalidCastException ex)
            {
                Assert.AreEqual(ex.Message, "Unable to cast object of type 'System.Collections.Generic.Dictionary`2[System.String,System.String]' to type 'System.Collections.Generic.Dictionary`2[System.Int32,System.String]'.");
            }
        }
    }
}
