using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public sealed class CMMEmailGenerator
	{
		enum Mode
		{
			Main,
			Voyage,
			Container,
		}

		public CMMEmailGenerator()
		{
			tmpHTMLBuilder = new StringBuilder();
			voyageBuilder = new StringBuilder();
			ackHTMLBuilder = new StringBuilder();
			ackFooterBuilder = new StringBuilder();
			warningHTMLBuilder = new StringBuilder();
			warningFooterBuilder = new StringBuilder();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void SetSubjectDetail(string messageTypeTitle, string senderName)
		{
			if (mode != Mode.Main)
			{
				throw new InvalidOperationException();
			}

			const string subjectFormat = "Processed {0} Message From {1}";
			Subject = string.Format(CultureInfo.InvariantCulture, subjectFormat, messageTypeTitle, senderName);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void WriteVoyageHeader(string vessel, string lloyds, string voyage)
		{
			if (mode != Mode.Main)
			{
				throw new InvalidOperationException();
			}

			const string leadIn =
				"		<h2>Voyage" +
				"";

			const string leadOut =
				"</h2>\r\n" +
				"";

			const string Open = " (";
			const string Close = ")";
			const string Separator = ", ";

			const string vesselFormat =
				"Vessel: {0}" +
				"";

			const string lloydsFormat =
				"Lloyds: {0}" +
				"";

			const string voyageFormat =
				"Voyage No: {0}" +
				"";

			tmpHTMLBuilder.Append(leadIn);

			bool arg = false;

			if (!string.IsNullOrEmpty(vessel))
			{
				tmpHTMLBuilder.Append(Open);
				tmpHTMLBuilder.AppendFormat(CultureInfo.InvariantCulture, vesselFormat, vessel);
				arg = true;
			}

			if (!string.IsNullOrEmpty(lloyds))
			{
				tmpHTMLBuilder.Append(arg ? Separator : Open);
				tmpHTMLBuilder.AppendFormat(CultureInfo.InvariantCulture, lloydsFormat, lloyds);
				arg = true;
			}

			if (!string.IsNullOrEmpty(voyage))
			{
				tmpHTMLBuilder.Append(arg ? Separator : Open);
				tmpHTMLBuilder.AppendFormat(CultureInfo.InvariantCulture, voyageFormat, voyage);
				arg = true;
			}

			if (arg)
			{
				tmpHTMLBuilder.Append(Close);
			}

			tmpHTMLBuilder.Append(leadOut);

			mode = Mode.Voyage;
			listOpened = false;
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "The parameters which represent URLs are allowed to be wrong/incomplete URLs therefore they are strings, not System.Uri")]
		public void WriteContainerHeader(string containerNumber, string url)
		{
			if (mode != Mode.Main)
			{
				throw new InvalidOperationException();
			}

			const string formatWithoutURL =
				"		<h2>{0}</h2>\r\n" +
				""; // Notification message sent by a service task

			const string formatWithURL =
				"		<h2><a href=\"{0}\">{1}</a></h2>\r\n" +
				""; // Notification message sent by a service task

			if (string.IsNullOrEmpty(url))
			{
				tmpHTMLBuilder.AppendFormat(CultureInfo.InvariantCulture, formatWithoutURL, containerNumber);
			}
			else
			{
				tmpHTMLBuilder.AppendFormat(CultureInfo.InvariantCulture, formatWithURL, url, containerNumber);
			}

			mode = Mode.Container;
			listOpened = false;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void WriteInfo(string message)
		{
			if (mode == Mode.Main)
			{
				throw new InvalidOperationException();
			}

			if (!listOpened)
			{
				tmpHTMLBuilder.Append((NoResString)"		<ul>\r\n");
				listOpened = true;
			}

			tmpHTMLBuilder.Append((NoResString)"			<li class=\"NotificationInfo\">");
			tmpHTMLBuilder.Append(message);
			tmpHTMLBuilder.Append("</li>\r\n");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void WriteWarning(string message)
		{
			if (mode == Mode.Main)
			{
				throw new InvalidOperationException();
			}

			if (!listOpened)
			{
				tmpHTMLBuilder.Append("		<ul>\r\n");
				listOpened = true;
			}

			tmpHTMLBuilder.Append("			<li class=\"NotificationWarning\">");
			tmpHTMLBuilder.Append(message);
			tmpHTMLBuilder.Append("</li>\r\n");
			hasPendingWarnings = true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void WriteVoyageFooter()
		{
			if (mode != Mode.Voyage)
			{
				throw new InvalidOperationException();
			}

			voyageBuilder.Append(tmpHTMLBuilder.ToString());

			if (listOpened)
			{
				voyageBuilder.Append("		</ul>\r\n");
				listOpened = false;
			}

			tmpHTMLBuilder.Length = 0;
			hasVesselWarnings |= hasPendingWarnings;
			hasPendingWarnings = false;

			mode = Mode.Main;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void WriteContainerFooter()
		{
			if (mode != Mode.Container)
			{
				throw new InvalidOperationException();
			}

			if (listOpened)
			{
				tmpHTMLBuilder.Append("		</ul>\r\n");
				listOpened = false;
			}

			if (hasPendingWarnings)
			{
				warningHTMLBuilder.Append(tmpHTMLBuilder.ToString());
				hasPendingWarnings = false;
				hasWarnings = true;
			}
			else
			{
				ackHTMLBuilder.Append(tmpHTMLBuilder.ToString());
				hasAck = true;
			}

			tmpHTMLBuilder.Length = 0;
			mode = Mode.Main;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public void WriteSubscriptionComment(bool isWarning, string subscriptionComment)
		{
			if (mode != Mode.Main)
			{
				throw new InvalidOperationException();
			}

			StringBuilder builder = isWarning ? warningFooterBuilder : ackFooterBuilder;

			builder.Append("		<span class=\"SubscriptionComment\">");
			builder.Append(subscriptionComment);
			builder.Append("</span>\r\n");
		}

		public string Subject { get; private set; }

		public override string ToString()
		{
			return GenerateHTML(true, true);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public EmailDef ToAckEmail()
		{
			if (hasAck && !hasVesselWarnings)
			{
				EmailDef result = new EmailDef();
				result.Subject = Subject + " - Containers Without Warnings";
				result.ContentType = EmailContentTypes.HTML;
				result.Body = GenerateHTML(true, false);
				return result;
			}
			else
			{
				return null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		public EmailDef ToWarningEmail()
		{
			if (hasWarnings || hasVesselWarnings)
			{
				EmailDef result = new EmailDef();
				result.Subject = hasVesselWarnings ? Subject : Subject + " - Containers With Warnings";
				result.ContentType = EmailContentTypes.HTML;
				result.Body = GenerateHTML(hasVesselWarnings, true);
				return result;
			}
			else
			{
				return null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Notification message sent by a service task")]
		string GenerateHTML(bool includeAck, bool includeWarning)
		{
			const string headerFormat =
				"<html>\r\n" +
				"	<head>\r\n" +
				"		<style type=\"text/css\">\r\n" +
				"			h1 {{ font-size: 140%; font-style: normal; font-weight: bold; }}\r\n" +
				"			h2 {{ font-size: 120%; font-style: normal; font-weight: bold; }}\r\n" +
				"			.NotificationInfo {{ color: Gray; }}\r\n" +
				"			.NotificationWarning {{ color: Maroon; }}\r\n" +
				"			.SubscriptionComment {{ font-size: 75%; }}\r\n" +
				"		</style>\r\n" +
				"		<title>{0}</title>\r\n" +
				"	</head>\r\n" +
				"	<body>\r\n" +
				"		<h1>{0}</h1>\r\n" +
				"";

			const string footerFormat =
				"	</body>\r\n" +
				"</html>\r\n" +
				"";

			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(CultureInfo.InvariantCulture, headerFormat, Subject);
			builder.Append(voyageBuilder.ToString());

			if (includeWarning)
			{
				builder.Append(warningHTMLBuilder);
			}

			if (includeAck)
			{
				builder.Append(ackHTMLBuilder);
			}

			if (includeWarning)
			{
				builder.Append(warningFooterBuilder.ToString());
			}
			else
			{
				builder.Append(ackFooterBuilder.ToString());
			}

			builder.Append(footerFormat);

			return builder.ToString();
		}

		bool hasPendingWarnings;
		bool hasVesselWarnings;
		bool hasWarnings;
		bool hasAck;
		bool listOpened;
		Mode mode;
		readonly StringBuilder tmpHTMLBuilder;
		readonly StringBuilder voyageBuilder;
		readonly StringBuilder ackHTMLBuilder;
		readonly StringBuilder ackFooterBuilder;
		readonly StringBuilder warningHTMLBuilder;
		readonly StringBuilder warningFooterBuilder;
	}
}
