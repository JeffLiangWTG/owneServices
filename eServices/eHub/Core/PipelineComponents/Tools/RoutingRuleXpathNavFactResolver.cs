using System;
using System.Linq;
using System.Xml.XPath;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public class RoutingRuleXpathNavFactResolver : IFactResolver
	{
		readonly IXPathNavigable xmlNav;

		public RoutingRuleXpathNavFactResolver(IXPathNavigable xmlNav)
		{
			if (xmlNav == null) throw new ArgumentNullException("xmlNav");
			this.xmlNav = xmlNav;
		}

		public void Resolve(Fact[] facts)
		{
			var xpNav = xmlNav.CreateNavigator();
			foreach (var fact in facts.Where(f => f.Type == "XPATH" || f.Type == "XPATHNAV"))
			{
				var node = xpNav.SelectSingleNode(fact.Query);
				fact.Value = node != null ? node.TypedValue as string : String.Empty;
			}
			foreach (var fact in facts.Where(f => f.Type == "XPATHNAVFUNC"))
			{
				var resultValue = xpNav.Evaluate(fact.Query);
				fact.Value = resultValue != null ? resultValue.ToString() : String.Empty;
			}
		}
	}
}
