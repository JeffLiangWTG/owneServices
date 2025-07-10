using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class FailedMessageHtmlBuilder
	{
		public FailedMessageHtmlBuilder()
		{
			this.builder = new StringBuilder();
			this.textMode = true;

			this.columns = new KeyValuePair<string, Action<StringBuilder, FailedMessage>>[]
			{
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("bee69579-6224-4fa5-a70d-08287095c748", "Message No."), (b, f) => { AppendHtml(b, f.MessageNo); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("8d66b022-648a-448b-bf0a-e2e20c0105da", "Sent Date"), (b, f) => { AppendHtml(b, new ZDateTime(f.MessageDateTime).ToLongTimeString() + " UTC"); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("055064a1-ac5e-4487-89aa-d4987d1016c6", "Container No."), (b, f) => { AppendHtml(builder, f.ContainerNo); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("ddbc89fb-3406-4679-82af-a45c82b4d314", "Container Type"), (b, f) => { AppendHtml(b, f.ContainerType); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("6f0716a8-a327-4e7b-9135-3701ba9b2a08", "Bill of Lading"), (b, f) => { AppendHtml(b, f.BillOfLading); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("4d88fc25-537f-46ac-ae33-177484ed5e63", "Shipment No."), (b, f) => { AppendHtml(b, f.ShipmentNo); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("e68bc1b8-b9a6-4bba-84cc-23de89ff5d49", "Principal"), (b, f) => { AppendHtml(b, f.Principal); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("3deccb0b-26ba-4283-8180-cac02e005743", "Origin"), (b, f) => { AppendHtml(b, f.Origin); }),
				new KeyValuePair<string, Action<StringBuilder, FailedMessage>>(Res.GetString("7d519aa4-c46f-4010-bb7d-e726f05fad51", "Destination"), (b, f) => { AppendHtml(b, f.Destination); }),
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "raw html not localisable")]
		public void WriteEmailHeader(string title)
		{
			const string EmailHeaderPart1 =
				"<html>\r\n" +
				"	<head>\r\n" +
				"		<style type=\"text/css\">\r\n" +
				"			th\r\n" +
				"			{\r\n" +
				"				background: #005596;\r\n" +
				"				color: white;\r\n" +
				"				text-align: left;\r\n" +
				"			}\r\n" +
				"		</style>\r\n" +
				"		<title>" +
				"";

			const string EmailHeaderPart2 =
				"</title>\r\n" +
				"	</head>\r\n" +
				"	<body>\r\n" +
				"";

			builder.Append(EmailHeaderPart1);
			AppendHtml(builder, title);
			builder.Append(EmailHeaderPart2);
		}

		public void WriteText(string text)
		{
			if (!textMode)
			{
				builder.Append("		<br />\r\n"); // raw html not localisable
				textMode = true;
			}

			builder.Append("		"); // raw html not localisable
			AppendHtml(builder, text);
			builder.Append("<br />\r\n"); // raw html not localisable
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "raw html not localisable")]
		public void WriteTable(string errorText, List<FailedMessage> messages)
		{
			textMode = false;

			const string TableHeader =
				"		<table>\r\n" +
				"";

			const string StartRow =
				"			<tr>\r\n" +
				"";

			const string EndRow =
				"			</tr>\r\n" +
				"";

			const string TableFooter =
				"		</table>\r\n" +
				"";

			builder.Append("		<br />\r\n");
			builder.Append((NoResString)"		<i>");
			AppendHtml(builder, errorText);
			builder.Append((NoResString)"</i><br />\r\n");

			builder.Append(TableHeader);
			builder.Append(StartRow);

			for (int i = 0; i < columns.Length; i++)
			{
				builder.Append("				<th>");
				AppendHtml(builder, columns[i].Key);
				builder.Append("</th>\r\n");
			}

			builder.Append(EndRow);

			foreach (FailedMessage message in messages)
			{
				builder.Append(StartRow);

				for (int i = 0; i < columns.Length; i++)
				{
					builder.Append("				<td>");
					columns[i].Value(builder, message);
					builder.Append("</td>\r\n");
				}

				builder.Append(EndRow);
			}

			builder.Append(TableFooter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "raw html not localisable")]
		public void WriteEmailFooter()
		{
			const string EmailFooter =
				"	</body>\r\n" +
				"</html>\r\n" +
				"";

			builder.Append(EmailFooter);
		}

		public override string ToString()
		{
			return builder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGotoDefaultOrCase", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "raw html not localisable")]
		void AppendHtml(StringBuilder stringBuilder, string plainText)
		{
			int startOfSequence = 0;

			for (int i = 0; i < plainText.Length; i++)
			{
				char c = plainText[i];
				string magic;

				switch (c)
				{
					case '\r':
						if (i + 1 < plainText.Length && plainText[i + 1] == '\n')
						{
							i++;
						}
						goto case '\n';

					case '\n':
						magic = "<br />";
						break;
					case '<':
						magic = "&lt;";
						break;
					case '>':
						magic = "&gt;";
						break;
					case '&':
						magic = "&amp;";
						break;
					default:
						magic = null;
						break;
				}

				if (magic != null)
				{
					stringBuilder.Append(plainText, startOfSequence, i - startOfSequence);
					stringBuilder.Append(magic);
					startOfSequence = i + 1;
				}
			}

			if (startOfSequence < plainText.Length)
			{
				stringBuilder.Append(plainText, startOfSequence, plainText.Length - startOfSequence);
			}
		}

		bool textMode;
		readonly KeyValuePair<string, Action<StringBuilder, FailedMessage>>[] columns;
		readonly StringBuilder builder;
	}
}
