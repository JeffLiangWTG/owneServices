
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess
{
	public static class Extensions
	{
		public static IEnumerable<XElement> ElementsAnyNS(this XElement source, string localName)
		{
			return source.Elements().Where(e => e.Name.LocalName == localName);
		}

		public static string ElementValue(this XElement source, string localName)
		{
			var element = source.Elements().Where(e => e.Name.LocalName == localName).FirstOrDefault();
			return element?.Value ?? string.Empty;
		}
	}
}
