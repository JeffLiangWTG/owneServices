using System;
using System.Linq;
using System.Xml.XPath;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class XpathNavFactResolver : IFactResolver
	{
		readonly IXPathNavigable XmlNav;

		public XpathNavFactResolver(IXPathNavigable xmlNav)
		{
			XmlNav = xmlNav ?? throw new ArgumentNullException(nameof(xmlNav));
		}

		public void Resolve(Fact[] facts)
		{
			var xpNav = XmlNav.CreateNavigator();

			foreach (var fact in facts.Where(f => f.Type == "XPATH" || f.Type == "XPATHNAV"))
			{
				var node = xpNav?.SelectSingleNode(fact.Query);
				fact.Value = node != null ? node.TypedValue as string : String.Empty;
			}

			foreach (var fact in facts.Where(f => f.Type == "XPATHNAVFUNC"))
			{
				var resultValue = xpNav?.Evaluate(fact.Query);
				fact.Value = resultValue?.ToString() ?? string.Empty;
			}
		}
	}
}