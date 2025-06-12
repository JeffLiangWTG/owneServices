using System.Xml;
using System.Xml.XPath;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.DataModelHelperTest
{
  [TestClass]
  public class SubscriptionHelperTest
  {
    CodeMapper MockCodeMapper => mockCodeMapper ?? (mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>());
    CodeMapper mockCodeMapper;

    DataModelAccessor MockDataModelAccessor => mockDataModelAccessor ?? (mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>());
    DataModelAccessor mockDataModelAccessor;

    SubscriptionHelper Helper => helper ?? (helper = new SubscriptionHelper(MockCodeMapper, MockDataModelAccessor));
    SubscriptionHelper helper;

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetXMLNodeValue()
    {
      var xmlValidXML = @"<eHubSubscriptions>
  <eHubSubscription>
    <Subscriber>VALIDXML001</Subscriber>
  </eHubSubscription>
<eHubSubscription>
    <Subscriber>VALIDXML002</Subscriber>
  </eHubSubscription>
</eHubSubscriptions>";
      var xmlInvalidXML1 = @"
<eHubSubscriptions>
  <eHubSubscription>
    <SubscriberXXX>HYEBNEUAT</SubscriberXXX>
  </eHubSubscription>
</eHubSubscriptions>";

      var nodes = CreateNodes(xmlValidXML, "/");
      Assert.AreEqual("VALIDXML001", Helper.GetXMLNodeValueCore(nodes, "Subscriber"));

      nodes = CreateNodes(xmlInvalidXML1, "/");
      Assert.AreEqual(string.Empty, Helper.GetXMLNodeValueCore(nodes, "Subscriber"));

      Assert.AreEqual(string.Empty, Helper.GetXMLNodeValueCore(nodes, "/eHub"));
      Assert.AreEqual(string.Empty, Helper.GetXMLNodeValueCore(null, "/eHub"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetSubscriberByValue()
    {
      var xmlData = @"<eHubSubscriptions>
  <eHubSubscription>
    <ID>1</ID>
    <Type>CW1MSG</Type>
    <Provider>Provider_WTG</Provider>
    <Subscriber>Subscriber_WTG01</Subscriber>
    <Value>WTG00000011</Value>
    <Reference>CONSOLNO_11111</Reference>
    <ReferenceType>JobNumber</ReferenceType>
    <Subscribed>2018-11-29T06:22:53</Subscribed>
    <Expiry />
  </eHubSubscription>
  <eHubSubscription>
    <ID>2</ID>
    <Type>CW1MSG</Type>
    <Provider>Provider_WTG</Provider>
    <Subscriber>Subscriber_WTG02</Subscriber>
    <Value>WTG00000011</Value>
    <Reference>CONSOLNO_22222</Reference>
    <ReferenceType>JobNumber</ReferenceType>
    <Subscribed>2018-11-29T06:22:53</Subscribed>
    <Expiry />
  </eHubSubscription>
</eHubSubscriptions>";

      var nodes = CreateNodes(xmlData, "/eHubSubscriptions");
      MockDataModelAccessor.Expect(x => x.SelectSubscriptionsByValue("CW1MSG", "WTG00000011")).Return(nodes).Repeat.Once();
      MockDataModelAccessor.Expect(x => x.SelectSubscriptionsByValue("CW1MSG", "WTG00000011", "JobNumber")).Return(nodes).Repeat.Once();

      Helper.SelectSubscriptionsByValue("CW1MSG", "WTG00000011");
      Assert.AreEqual("Subscriber_WTG01", Helper.GetSubscriber());
      Assert.AreEqual("Provider_WTG", Helper.GetProvider());
      Assert.AreEqual("CONSOLNO_11111", Helper.GetReference());

      Helper.SelectSubscriptionsByValue("CW1MSG", "WTG00000011", "JobNumber");
      Assert.AreEqual("Subscriber_WTG01", Helper.GetSubscriber());
      Assert.AreEqual("Provider_WTG", Helper.GetProvider());
      Assert.AreEqual("CONSOLNO_11111", Helper.GetReference());

      nodes = CreateNodes("<eHubSubscriptions/>", "/eHubSubscriptions");
      MockDataModelAccessor.Expect(x => x.SelectSubscriptionsByValue("CW1MSG", "WTG00000011", "JobNumber_1111")).Return(nodes).Repeat.Once();

      Helper.SelectSubscriptionsByValue("CW1MSG", "WTG00000011", "JobNumber_1111");
      Assert.AreEqual("", Helper.GetSubscriber());
      Assert.AreEqual("", Helper.GetProvider());
      Assert.AreEqual("", Helper.GetReference());

      MockDataModelAccessor.VerifyAllExpectations();
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestInsertBoleroSubscription()
    {
      MockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMMSG", "WizardOfOzz", "Dorothy", "YellowBrickRoad", "C00001001", "JobNumber")).Repeat.Once();
      MockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMMSG", "WizardOfOzz", "Dorothy", "C00001001_RC123", "C00001001", "Client_Job")).Repeat.Once();

      Helper.InsertBoleroSubscription("Dorothy", "RC123", "WizardOfOzz", "YellowBrickRoad", "C00001001");

      MockDataModelAccessor.VerifyAllExpectations();
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetOCMBEClientID()
    {
      MockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "Dorothy", "@ST_ID", "OBEID", "@value", "Home")).Return(string.Empty).Repeat.Once();
      MockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "Dorothy", "@ST_ID", "OBEID", "@value", "WizardOfOzz")).Return("Somwehere_Over_The_Rainbox").Repeat.Once();
      MockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "Dorothy", "@ST_ID", "OBEID", "@value", "OCMBE")).Return("OCM_BookingEngine").Repeat.Once();

      Assert.AreEqual(Helper.GetOCMBEClientID("Dorothy", "Home"), string.Empty);
      Assert.AreEqual(Helper.GetOCMBEClientID("Dorothy", "WizardOfOzz"), string.Empty);
      Assert.AreEqual(Helper.GetOCMBEClientID("Dorothy", "OCMBE"), "OCM_BookingEngine");

      MockCodeMapper.VerifyAllExpectations();
    }

    #region Implementation

    XPathNodeIterator CreateNodes(string xmlData, string xpath)
    {
      var docXML = new XmlDocument();
      docXML.LoadXml(xmlData.Trim());
      return docXML.CreateNavigator().Select(xpath);
    }

    #endregion
  }
}