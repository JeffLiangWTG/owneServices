using System.Xml.XPath;

namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public  interface ISGCustomsSubscriptionHelper
    {
        XPathNavigator InitializeSubscription();
        XPathNavigator SubscribeShipment(XPathNavigator SubscriptionNavigator, string ManifestNumber, string MAWB, string Purpose);
        XPathNavigator SubscribeHistoryOrder(XPathNavigator SubscriptionNavigator, string Purpose, string DateTime, string IDT1, string IDT2, string IDT3);
        XPathNavigator SubscribeMessageInfo(XPathNavigator SubscriptionNavigator, string IDT, string Purpose, string InfoName, string InfoValue);
        XPathNavigator SubscribeHAWB(XPathNavigator SubscriptionNavigator, string HAWB);
        XPathNavigator SubscribePackLine(XPathNavigator SubscriptionNavigator, string HAWB, string SGID, string ConsignmentRef, string Status, string IDT, string Purpose);
        XPathNavigator SubscribeEvent(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef, string Status, string IDT, string Purpose);
        XPathNavigator SubscribeInfo(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef, string IDT, string Purpose, string InfoName, string InfoValue);
        XPathNavigator SelectHousesByIDT(XPathNavigator SubscriptionNavigator, string IDT, string Purpose);
        XPathNavigator SelectHousesByIDT(XPathNavigator SubscriptionNavigator, string IDT);
        XPathNavigator SelectPackLinesByIDT(XPathNavigator SubscriptionNavigator, string HAWB, string IDT, string Purpose);
        XPathNavigator SelectPackLinesByIDT(XPathNavigator SubscriptionNavigator, string HAWB, string IDT);
        XPathNavigator SelectPackLineByRef(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef);
        XPathNavigator SelectPackLineBySGID(XPathNavigator SubscriptionNavigator, string HAWB, string SGID);
        string SelectSGIDByRef(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef);
        XPathNavigator SelectPackLineCurrentStatus(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef);
        string Join(XPathNodeIterator Values, string Delimeter);
        XPathNavigator SelectLatestIDTWhichDeclaredHAWB(XPathNavigator Subscription, string HAWB);
        XPathNavigator SelectInfoByIDT(XPathNavigator Subscription, string IDT, string Purpose, string InfoKey);
        XPathNavigator SelectInfoByIDT(XPathNavigator Subscription, string IDT, string InfoKey);
        XPathNavigator LoadSubscription(XPathNavigator SubscriptionNavigator, string Content);
        string ToString(XPathNavigator SubscriptionNavigator);
    }
}