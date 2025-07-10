using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Business.EDIMessage
{
	public static class GenralMessageParser
	{
		public static ZString GetMessageSubType(this Edifact.D16A.Messages.GENRAL.GENRALMessage message)
		{
			return ((ZString)message.BGM[0].DocumentMessageIdentification.VersionIdentifier).SubstringSafe(0, 3);
		}

		public static ZString GetMessageInterpretation(this Edifact.D16A.Messages.GENRAL.GENRALMessage message)
		{
			switch (message.GetMessageSubType())
			{
				case GenralMessageSubTypeList.Codes.UCR:
					return GetTableMessageInterpretation(message, $"GENRAL message - {GenralMessageSubTypeList.Descriptions.UCR}");
				case GenralMessageSubTypeList.Codes.PEN:
					return GetTableMessageInterpretation(message, $"GENRAL message - {GenralMessageSubTypeList.Descriptions.PEN}");
				default:
					return GetGenericMessageInterpretation(message);
			}
		}

		static ZString GetGenericMessageInterpretation(Edifact.D16A.Messages.GENRAL.GENRALMessage message)
		{
			var sb = new ZStringBuilder();
			foreach (Edifact.D16A.Messages.GENRAL.SegmentGroup5 sg5 in message.Group5)
			{
				foreach (Edifact.D16A.Segments.FTXSegment ftx in sg5.FTX)
				{
					sb.AppendIfNotEmpty(ftx.TextLiteral.FreeText1);
					sb.AppendIfNotEmpty(ftx.TextLiteral.FreeText2);
					sb.AppendIfNotEmpty(ftx.TextLiteral.FreeText3);
					sb.AppendIfNotEmpty(ftx.TextLiteral.FreeText4);
					sb.AppendIfNotEmpty(ftx.TextLiteral.FreeText5);
				}
			}
			var payload = sb.ToStringWithNewLineBetweenAppends().Trim();
			return GetHtmlPageLayout("GENRAL message", $"<p><b><pre><i>{payload}</i></pre></b></p>");
		}

		static ZString GetTableMessageInterpretation(Edifact.D16A.Messages.GENRAL.GENRALMessage message, string header)
		{
			var lines = message.Group5.Cast<Edifact.D16A.Messages.GENRAL.SegmentGroup5>()
				.SelectMany(group5 => group5.FTX.Cast<Edifact.D16A.Segments.FTXSegment>()
					.SelectMany(ftx => new[]
					{
						ftx.TextLiteral.FreeText1,
						ftx.TextLiteral.FreeText2,
						ftx.TextLiteral.FreeText3,
						ftx.TextLiteral.FreeText4,
						ftx.TextLiteral.FreeText5
					})
				)
				.Where(line => !string.IsNullOrEmpty(line) && line.Contains(",") && line.Contains("="));

			var sb = new ZStringBuilder();
			sb.AppendLine("<table>");
			foreach (var row in ConvertToHtmlTableRows(lines))
			{
				sb.AppendLine(row);
			}
			sb.Append("</table>");
			return GetHtmlPageLayout(header, sb.ToString());
		}

		static string GetHtmlPageLayout(string header, string body) => $@"<html>
<style>
table {{
   border-collapse: collapse;
}}
th, td {{
   border: 1px solid black;
   font-size: 0.8em;
   padding: 3px;
}}
</style>
<body style='font-family: arial;'>
<h3>{header}</h3>
{body}
</body>
</html>";

		static readonly ImmutableDictionary<string, string> columnDisplayNames = ImmutableDictionary.CreateRange(new Dictionary<string, string>
		{
			{ "Dir", "Direction" },
			{ "EvalDate", "Evaluation Date" }
		});

		static IEnumerable<string> ConvertToHtmlTableRows(IEnumerable<string> lines)
		{
			var result = new List<string>();
			var count = 0;
			foreach (var line in lines)
			{
				var lineItems = line.Split(',')
					.Select(item => item.TrimLineDescription().Split('='))
					.ToArray();
				if (count == 0)
				{
					result.Add("<tr>");
					result.Add("<th style=\"border: none;\" />");
					result.AddRange(lineItems.Select(x => $"<th>{GetColumnDisplayName(x[0].Trim())}</th>"));
					result.Add("</tr>");
				}
				result.Add("<tr>");
				result.Add($"<td>{++count}</td>");
				result.AddRange(lineItems.Select(x => $"<td>{x[1].Trim()}</td>"));
				result.Add("</tr>");
			}
			return result;
		}

		static string TrimLineDescription(this string line)
		{
			var separatorIndex = line.IndexOf(": ");
			if (separatorIndex >= 0 && separatorIndex < line.IndexOf("="))
			{
				return line.Remove(0, separatorIndex + 2).Trim();
			}
			else
			{
				return line.Trim();
			}
		}

		static string GetColumnDisplayName(string columnName) => columnDisplayNames.ContainsKey(columnName) ? columnDisplayNames[columnName] : columnName;
	}
}
