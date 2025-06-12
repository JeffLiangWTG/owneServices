using System;
using System.Linq;
using System.Xml;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using eServices.eHubRoutingRuleEngine;
using System.Xml.XPath;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public class RoutingRuleMessageFactResolver : IFactResolver
	{
		public RoutingRuleMessageFactResolver(IBaseMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");

			this.message = message;
		}

		IBaseMessage message;

		public void Resolve(Fact[] facts)
		{
			var propertyFactResolver = new RoutingRulePropertyFactResolver(message);
			propertyFactResolver.Resolve(facts);

			if (facts.Any(f => f.Type == "XPATHNAV" || f.Type == "XPATHNAVFUNC"))
			{
				if (!this.message.BodyPart.Data.CanSeek)
					this.message.BodyPart.Data = new ReadOnlySeekableStream(this.message.BodyPart.Data, new VirtualStream());

				this.message.BodyPart.Data.Position = 0;
				var xRdr = XmlReader.Create(this.message.BodyPart.Data);
				var xpDoc = new XPathDocument(xRdr);
				var xpathNavFactResolver = new RoutingRuleXpathNavFactResolver(xpDoc);
				xpathNavFactResolver.Resolve(facts);
				this.message.BodyPart.Data.Position = 0;
			}
			else if (facts.Any(f => f.Type == "XPATH"))
			{
				if (!this.message.BodyPart.Data.CanSeek)
					this.message.BodyPart.Data = new ReadOnlySeekableStream(this.message.BodyPart.Data, new VirtualStream());

				this.message.BodyPart.Data.Position = 0;
				var xpaths = facts.Where(f => f.Type == "XPATH").Select(f => f.Query).Distinct().ToList();
				var xRdr = XmlReader.Create(this.message.BodyPart.Data);
				var xpathCollection = new XPathCollection();
				foreach (var xpath in xpaths)
					xpathCollection.Add(xpath);
				XPathReader xpathReader = new XPathReader(xRdr, xpathCollection);
				while (xpaths.Count > 0 && xpathReader.ReadUntilMatch())
				{
					for (int i = 0; i < xpaths.Count; i++)
					{
						if (xpathReader.Match(xpaths[i]))
						{
							string value = xpathReader.ReadString();
							foreach (var fact in facts.Where(f => f.Type == "XPATH" && f.Query == xpaths[i]))
								fact.Value = value;
							xpaths.RemoveAt(i);
							break;
						}
					}
				}
				foreach (var xpath in xpaths)
				foreach (var fact in facts.Where(f => f.Type == "XPATH" && f.Query == xpath))
					fact.Value = String.Empty;
				this.message.BodyPart.Data.Position = 0;
			}
		}
	}
}
