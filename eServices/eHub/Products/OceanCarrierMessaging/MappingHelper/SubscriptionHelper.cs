using System;
using System.Xml;
using System.Xml.XPath;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class SubscriptionHelper
  {
    #region Constuctors

    public SubscriptionHelper() { }

    public SubscriptionHelper(CodeMapper codeMapper, DataModelAccessor dataModelAccessor)
    {
      this.codeMapper = codeMapper;
      this.dataModelAccessor = dataModelAccessor;
    }

    #endregion

    #region Helpers

    const string xmlLink = "/eHubSubscriptions/eHubSubscription/";
    string reference = string.Empty;
    string provider = string.Empty;
    string subscriber = string.Empty;

    DataModelAccessor DataModelAccessor
    {
      get { return dataModelAccessor ?? (dataModelAccessor = new DataModelAccessor()); }
    }
    DataModelAccessor dataModelAccessor;

    CodeMapper CodeMapper
    {
      get { return codeMapper ?? (codeMapper = new CodeMapper()); }
    }
    CodeMapper codeMapper;

    #endregion

    #region Implementation

    public virtual void SelectSubscriptionsByValue(string subscriptionType, string value)
    {
      SelectSubscriptionsByValue(subscriptionType, value, null);
    }

    public virtual void SelectSubscriptionsByValue(string subscriptionType, string value, string referenceType)
    {
      var nodes = DataModelAccessor.SelectSubscriptionsByValue(subscriptionType, value, referenceType);
      PopulateValues(nodes);
    }

    internal void PopulateValues(XPathNodeIterator inputXML)
    {
      subscriber = GetXMLNodeValueCore(inputXML, "Subscriber");
      provider = GetXMLNodeValueCore(inputXML, "Provider");
      reference = GetXMLNodeValueCore(inputXML, "Reference");
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

    #endregion

    #region Provider-specific logic

    public virtual void InsertBoleroSubscription(string sender, string registrationCode, string recipient, string messageReference, string ConsolNumber)
    {
      DataModelAccessor.InsertSubscriptionValue("OCMMSG", recipient, sender, messageReference, ConsolNumber, "JobNumber");
      DataModelAccessor.InsertSubscriptionValue("OCMMSG", recipient, sender, ConsolNumber + "_" + registrationCode, ConsolNumber, "Client_Job");
    }

    public virtual string GetOCMBEClientID(string senderID, string destinationParty)
    {
      try
      {
        const string OCM_BookingEngine = "OCM_BookingEngine";
        var OCMBEClientID = CodeMapper.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "OBEID", "@value", destinationParty);

        return string.Equals(OCMBEClientID, OCM_BookingEngine, StringComparison.OrdinalIgnoreCase) 
              ? OCM_BookingEngine
              : string.Empty;
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }

    #endregion

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