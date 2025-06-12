using System;
using System.Xml;
using System.Xml.XPath;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.Dakosy.BT.Transforms.Helper
{
  public class SubscriptionHelper
  {
    const string xmlLink = "/eHubSubscriptions/eHubSubscription/";
    string reference = string.Empty;
    string provider = string.Empty;
    string subscriber = string.Empty;
    string subscribedDateTime = string.Empty;

    DataModelAccessor DataModelAccessor
    {
      get { return dataModelAccessor ?? (dataModelAccessor = new DataModelAccessor()); }
    }
    DataModelAccessor dataModelAccessor;


    public virtual void SelectSubscriptions(string subscriptionType, string senderId, string value)
    {
      if (testNodes != null)
      {
        PopulateValues(testNodes);
        return;
      }

      var nodes = DataModelAccessor.SelectSubscriptions(subscriptionType, senderId, value);
      PopulateValues(nodes);
    }

    internal void PopulateValues(XPathNodeIterator inputXML)
    {
      subscriber = GetXMLNodeValueCore(inputXML, "Subscriber");
      provider = GetXMLNodeValueCore(inputXML, "Provider");
      reference = GetXMLNodeValueCore(inputXML, "Reference");
      subscribedDateTime = GetXMLNodeValueCore(inputXML, "Subscribed");
    }

    public string GetXMLNodeValueCore(XPathNodeIterator inputXML, string inputNode)
    {
      var result = string.Empty;

      if (inputXML != null)
      {
        var xmlNodeName = xmlLink + inputNode;

        var docx = new XmlDocument();
        docx.LoadXml(inputXML.Current.InnerXml);
        var nodeValue = docx.SelectSingleNode(xmlNodeName);

        if (nodeValue != null && !string.IsNullOrEmpty(nodeValue.InnerText))
        {
          result = nodeValue.InnerText;
        }
      }
      return result;
    }

    public virtual string GetSubscriber()
    {
      return subscriber;
    }

    public virtual string GetProvider()
    {
      return provider;
    }

    public virtual string GetReference()
    {
      return reference;
    }

    public virtual string GetSubscribedDateTime()
    {
      return string.IsNullOrEmpty(subscribedDateTime) 
        ? string.Empty
        : new DateMapper().ConvertToDateTimeString(subscribedDateTime, null, "yyyy-MM-dd HH:mm:ss");
    }

    public void SetXMLTestNodes(string inputXML)
    {
      var docXML = new XmlDocument();
      docXML.LoadXml(inputXML.Trim());
      testNodes = docXML.CreateNavigator().Select("/eHubSubscriptions");
    }
    XPathNodeIterator testNodes;

    #region ExceptionHandler

    public void ThrowMissingValueException(string senderId, string subscriptionTypeId, string elementName, string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        throw new Exception(String.Format("The source message does not contain a document identification value. Sender: {0}, ST_ID: {1} Element Name: {2}", senderId, subscriptionTypeId, elementName));
      }
    }

    #endregion
  }
}
