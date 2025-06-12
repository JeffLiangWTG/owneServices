using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper
{
	public class SGCustomsSubscriptionHelper
	{
		public virtual XPathNavigator InitializeSubscription()
		{
			XDocument Subscription = new XDocument();
			Subscription.Add(new XElement("Shipment"));
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribeShipment(XPathNavigator SubscriptionNavigator, string ManifestNumber, string MAWB, string Purpose)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			Subscription.XPathSelectElement("/Shipment").Add(new XAttribute("ManifestNumber", ManifestNumber),
				new XAttribute("MAWB", MAWB),
				new XAttribute("Purpose", Purpose),
				new XElement("HistoryOrder"));
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribeHistoryOrder(XPathNavigator SubscriptionNavigator, string Purpose, string DateTime, string IDT1, string IDT2, string IDT3)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			Subscription.XPathSelectElement("/Shipment/HistoryOrder").Add(new XElement("Message",
				new XAttribute("Purpose", Purpose),
				new XAttribute("DateTime", DateTime),
				new XElement("IDT",
					new XElement("IDT1", IDT1),
					new XElement("IDT2", IDT2),
					new XElement("IDT3", IDT3)),
				new XElement("Info")));
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribeMessageInfo(XPathNavigator SubscriptionNavigator, string IDT, string Purpose, string InfoName, string InfoValue)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']/Info").Add(new XElement(InfoName, InfoValue));
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribeHAWB(XPathNavigator SubscriptionNavigator, string HAWB)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var shipment = Subscription.XPathSelectElement(@"/Shipment[not(House[@HAWB='" + HAWB + "'])]");
			if (shipment != null)
			{
				shipment.Add(new XElement("House", new XAttribute("HAWB", HAWB)));
			}
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribePackLine(XPathNavigator SubscriptionNavigator, string HAWB, string SGID, string ConsignmentRef, string Status, string IDT, string Purpose)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			if (Subscription.XPathSelectElement("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']") == null)
			{
				Subscription.XPathSelectElement("//House[@HAWB='" + HAWB + "']").Add(new XElement("Pack",
					new XAttribute("SGID", SGID),
					new XAttribute("Ref", ConsignmentRef),
					new XElement("History")));
			}
			SubscribeEvent(SubscriptionNavigator, HAWB, ConsignmentRef, Status, IDT, Purpose);
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribeEvent(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef, string Status, string IDT, string Purpose)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']").ElementsBeforeSelf().Count() + 1;
			Subscription.XPathSelectElement("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']/History").Add(new XElement("Event",
				new XAttribute("Status", Status),
				new XAttribute("IDTRef", IDTRef),
				new XElement("Info")));
			return Subscription.CreateNavigator();
		}

		public virtual XPathNavigator SubscribeInfo(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef, string IDT, string Purpose, string InfoName, string InfoValue)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']").ElementsBeforeSelf().Count() + 1;
			Subscription.XPathSelectElements("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef +"']//Event[@IDTRef='" + IDTRef + "']/Info").Last().Add(new XElement(InfoName, InfoValue));
			return Subscription.CreateNavigator();
		}

		public virtual string SelectIDTRef(XPathNavigator SubscriptionNavigator, string IDT, string Purpose)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']").ElementsBeforeSelf().Count() + 1;
			return IDTRef.ToString();
		}

		public virtual XPathNavigator SelectHousesByIDT(XPathNavigator SubscriptionNavigator, string IDT, string Purpose)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']").ElementsBeforeSelf().Count() + 1;
			return new XDocument(new XElement("HAWBs", Subscription.XPathSelectElements("//Event[@IDTRef='" + IDTRef + "']/ancestor::House"))).CreateNavigator();
		}

		public virtual XPathNavigator SelectHousesByIDT(XPathNavigator SubscriptionNavigator, string IDT)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "']").ElementsBeforeSelf().Count() + 1;
			return new XDocument(new XElement("HAWBs", Subscription.XPathSelectElements("//Event[@IDTRef='" + IDTRef + "']/ancestor::House"))).CreateNavigator();
		}

		public virtual void DeletePackLinesAndHouseByIDTAndHAWB(XPathNavigator SubscriptionNavigator, string IDT, string HAWB)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement($"//Message[IDT='{IDT}' and @Purpose='ERR']").ElementsBeforeSelf().Count() + 1;
			Subscription.XPathSelectElements($"//House[@HAWB='{HAWB}']//Event[@IDTRef='{IDTRef}']/ancestor::Pack").Remove();
			var house = Subscription.XPathSelectElement($"//House[@HAWB='{HAWB}']");
			if (house.Descendants().Count() == 0)
			{
				house.Remove();
			}
		}

		public virtual XPathNavigator SelectPackLinesByIDT(XPathNavigator SubscriptionNavigator, string HAWB, string IDT, string Purpose)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']").ElementsBeforeSelf().Count() + 1;
			return new XDocument(new XElement("Packs", Subscription.XPathSelectElements("//House[@HAWB='" + HAWB + "']//Event[@IDTRef='" + IDTRef + "']/ancestor::Pack"))).CreateNavigator();
		}

		public virtual XPathNavigator SelectPackLinesInTheSameSplitSegment(XPathNavigator SubscriptionNavigator, string HAWB, string IDT, string Purpose, string SGID)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']").ElementsBeforeSelf().Count() + 1;
			var SplitSegment =
				SelectPackLineBySGID(SubscriptionNavigator, HAWB, SGID)
				.SelectSingleNode(".//Info/HAWBSplitSegment").Value;
			return new XDocument(new XElement("Packs", Subscription.XPathSelectElements("//House[@HAWB='" + HAWB + "']//Event[@IDTRef='" + IDTRef + "' and Info/HAWBSplitSegment/text()='" + SplitSegment + "']/ancestor::Pack"))).CreateNavigator();
		}

		public virtual XPathNavigator SelectPackLinesByIDT(XPathNavigator SubscriptionNavigator, string HAWB, string IDT)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var IDTRef = Subscription.XPathSelectElement("//Message[IDT='" + IDT + "']").ElementsBeforeSelf().Count() + 1;
			return new XDocument(new XElement("Packs", Subscription.XPathSelectElements("//House[@HAWB='" + HAWB + "']//Event[@IDTRef='" + IDTRef + "']/ancestor::Pack"))).CreateNavigator();
		}

		public virtual XPathNavigator SelectPackLineByRef(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef)
		{
			return SubscriptionNavigator.SelectSingleNode("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']");
		}

		public virtual XPathNavigator SelectPackLineBySGID(XPathNavigator SubscriptionNavigator, string HAWB, string SGID)
		{
			return SubscriptionNavigator.SelectSingleNode("//House[@HAWB='" + HAWB + "']/Pack[@SGID='" + SGID + "']");
		}

		public virtual XPathNavigator SelectPackLinesByStatus(XPathNavigator SubscriptionNavigator, string IDT, string HAWB, string Purpose, string Status)
		{
			var packs = SelectPackLinesByIDT(SubscriptionNavigator, HAWB, IDT, Purpose).UnderlyingObject as XDocument;
			return new XDocument(new XElement("Packs", packs.XPathSelectElements(".//Pack")
				.Where(x => (SelectPackLineCurrentStatus(SubscriptionNavigator, HAWB, x.Attribute("Ref").Value)
								.UnderlyingObject as XElement).Attribute("Status").Value == Status))).CreateNavigator();
		}

		public virtual string SelectSGIDByRef(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef)
		{
			var pack = SubscriptionNavigator.SelectSingleNode("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']/@SGID");
			return pack == null ? string.Empty : pack.Value;
		}

		public virtual XPathNavigator SelectPackLineCurrentStatus(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var lastEvent = Subscription.XPathSelectElement("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']//Event[last()]");
			var IDTRef = lastEvent.Attribute("IDTRef").Value;
			return new XElement("CurrentStatus",
				new XAttribute("Status", lastEvent.Attribute("Status").Value),
				new XElement("SetBy", Subscription.XPathSelectElement("//Message[" + IDTRef + "]"))).CreateNavigator();
		}

		public virtual XPathNavigator SelectPackLineCurrentStatus(XPathNavigator SubscriptionNavigator, string IDT, string HAWB, string ConsignmentRef, string FilterOut)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var latestEvent = Subscription.XPathSelectElements("//Message[IDT='" + IDT + "']")
				.Select(x => x.ElementsBeforeSelf().Count() + 1)
				.Select(x => Subscription.XPathSelectElement("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']//Event[@IDTRef='" + x + "']"))
				.Where(x => x != null && x.Attribute("Status").Value != FilterOut)
				.OrderByDescending(x => x.Attribute("IDTRef").Value)
				.First();
			return new XElement("CurrentStatus",
				new XAttribute("Status", latestEvent.Attribute("Status").Value)).CreateNavigator();
		}

		public virtual XPathNavigator SelectPackLineCurrentStatus(XPathNavigator SubscriptionNavigator, string IDT, string HAWB, string ConsignmentRef)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			var latestEvent = Subscription.XPathSelectElements("//Message[IDT='" + IDT + "']")
				.Select(x => x.ElementsBeforeSelf().Count() + 1)
				.Select(x => Subscription.XPathSelectElement("//House[@HAWB='" + HAWB + "']/Pack[@Ref='" + ConsignmentRef + "']//Event[@IDTRef='" + x + "']"))
				.Where(x => x != null)
				.OrderByDescending(x => x.Attribute("IDTRef").Value)
				.First();
			return new XElement("CurrentStatus",
				new XAttribute("Status", latestEvent.Attribute("Status").Value)).CreateNavigator();
		}

		public string Join(XPathNodeIterator Values, string Delimeter)
		{
			return string.Join(Delimeter, Values.Cast<XPathNavigator>());
		}

		public virtual XPathNavigator SelectLatestIDTWhichDeclaredHAWB(XPathNavigator Subscription, string HAWB)
		{
			return Subscription.SelectSingleNode("//Message[position() = //House[@HAWB='" + HAWB + "']//Event[@Status='Declared'][last()]/@IDTRef]/IDT");
		}

		public virtual XPathNavigator SelectInfoByIDT(XPathNavigator Subscription, string IDT, string Purpose, string InfoKey)
		{
			return Subscription.SelectSingleNode("//Message[IDT='" + IDT + "' and @Purpose='" + Purpose + "']/Info/" + InfoKey);
		}

		public virtual XPathNavigator SelectInfoByIDT(XPathNavigator Subscription, string IDT, string InfoKey)
		{
			return Subscription.SelectSingleNode("//Message[IDT='" + IDT + "']/Info/" + InfoKey);
		}

		public virtual XPathNavigator LoadSubscription(XPathNavigator SubscriptionNavigator, string Content)
		{
			XDocument Subscription = SubscriptionNavigator.UnderlyingObject as XDocument;
			Subscription.RemoveNodes();
			Subscription.Add(XDocument.Parse(Content).FirstNode);
			return Subscription.CreateNavigator();
		}

		public virtual string ToString(XPathNavigator SubscriptionNavigator)
		{
			return SubscriptionNavigator.OuterXml;
		}
	}
}
