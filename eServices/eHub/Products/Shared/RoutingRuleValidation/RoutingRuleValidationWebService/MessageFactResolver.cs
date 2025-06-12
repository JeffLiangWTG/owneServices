using System;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class MessageFactResolver : IFactResolver
	{
		readonly byte[] Message;

		public MessageFactResolver(byte[] message)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public void Resolve(Fact[] facts)
		{
			if (facts.Any(f => f.Type == "XPATHNAV" || f.Type == "XPATHNAVFUNC" || f.Type == "XPATH" || (f.Type == "PROPERTY" && f.Name == "MessageType")))
			{
				using (var ms = new MemoryStream(Message))
				{
					var xpDoc = new XPathDocument(ms);
					var xpathNavFactResolver = new XpathNavFactResolver(xpDoc);
					xpathNavFactResolver.Resolve(facts);

					var messageType = facts.SingleOrDefault(f => f.Type == "PROPERTY" && f.Name == "MessageType");
					if (messageType != null)
					{
						messageType.Value = GetMessageType(xpDoc);
					}
				}
			}
		}

		public string GetMessageType(XPathDocument xpDoc)
		{
			var xpNav = xpDoc.CreateNavigator();
			xpNav.MoveToRoot();
			xpNav.MoveToFollowing(XPathNodeType.Element);

			return string.IsNullOrWhiteSpace(xpNav.NamespaceURI)
				? xpNav.LocalName
				: $"{xpNav.NamespaceURI}#{xpNav.LocalName}";
		}
	}
}