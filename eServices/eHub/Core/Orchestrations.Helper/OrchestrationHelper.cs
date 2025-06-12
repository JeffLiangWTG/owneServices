using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	public static class OrchestrationHelper
	{
		public static string RemoveXmlDeclaration(string xmlString)
		{
			var declaration = Regex.Match(xmlString, @"^(<\?*?xml.*?>[\w\W]*?)<.*?").Groups[1].Value;
			return xmlString.Remove(0, declaration.Length);
		}

		public static string RemoveInvalidCharactersFromXmlContent(string inString)
		{
			inString = inString.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

			var newString = new StringBuilder();
			foreach (var ch in inString)
			{
				if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
				{
					newString.Append(ch);
				}
			}
			return newString.ToString();
		}
	}
}