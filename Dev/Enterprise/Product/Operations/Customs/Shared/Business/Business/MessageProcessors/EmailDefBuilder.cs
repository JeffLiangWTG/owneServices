namespace Enterprise.Customs.Business.MessageProcessors
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using CargoWise.Application;
	using CargoWise.ResourceStrings.Grammar;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Modules;

	public class EmailDefBuilder
	{
		#region Constructors

		public EmailDefBuilder(string emailSubject, string bodyHtmlTemplateName)
		{
			this.emailSubject = emailSubject;
			this.bodyHtmlTemplateName = bodyHtmlTemplateName;
		}

		public EmailDefBuilder(string emailSubject, string textToAttach, string bodyHtmlTemplateName)
			: this(emailSubject, bodyHtmlTemplateName)
		{
			attachments.Add("Message.txt", textToAttach);
		}

		public EmailDefBuilder(string emailSubject, string textToAttach, string bodyHtmlTemplateName, params object[] replacementArgs)
			: this(emailSubject, textToAttach, bodyHtmlTemplateName)
		{
			argReplacements.AddRange(replacementArgs);
		}

		#endregion

		public EmailDef ToEmail(Guid? compPKForLogo = null, Guid? branchPKForLogo = null, Guid? deptPkForLogo = null)
		{
			var result = GetEmail(emailSubject, GetEmailHtml(), compPKForLogo, branchPKForLogo, deptPkForLogo);

			foreach (var attachment in attachments)
			{
				result.Attachments.Add(new AttachmentDef(attachment.Key, Encoding.ASCII.GetBytes(attachment.Value)));
			}

			return result;
		}

		public override string ToString()
		{
			return GetEmailHtml();
		}

		public static EmailDef GetEmail(string subject, string htmlBody)
		{
			var emailSender = new HtmlNotificationEmailSender();
			return emailSender.CreateEmail(subject, htmlBody);
		}

		public static EmailDef GetEmail(string subject, string htmlBody, Guid? compPKForLogo = null, Guid? branchPKForLogo = null, Guid? deptPkForLogo = null)
		{
			var emailSender = new HtmlNotificationEmailSender();
			return emailSender.CreateEmail(subject, htmlBody, compPKForLogo, branchPKForLogo, deptPkForLogo);
		}

		public static string GetJobLink(IControllerIDProvider controllerIDProvider, string linkText)
		{
			var result = string.Empty;
			if (controllerIDProvider != null)
			{
				result = GetJobLink(ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerIDProvider), linkText);
			}
			return result;
		}

		public static string GetJobLink(ControllerID controllerID, Guid pk, string linkText)
		{
			var result = string.Empty;
			if (controllerID != null && pk != Guid.Empty)
			{
				result = GetJobLink(ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, pk), linkText);
			}
			return result;
		}

		static string GetJobLink(string link, string linkText)
		{
			return $"<a href=\"{link}\">{linkText}</a>";
		}

		#region Add Replacement

		public void AddArgReplacement(object replacementArg)
		{
			argReplacements.Add(replacementArg);
		}

		public void AddArgReplacementRange(params object[] replacementArgs)
		{
			argReplacements.AddRange(replacementArgs);
		}

		public void AddDynamicHtmlReplacement(object replacement)
		{
			foreach (var textToReplace in new[] { HtmlTemplates.DynamicHtml1, HtmlTemplates.DynamicHtml2, HtmlTemplates.DynamicHtml3, HtmlTemplates.DynamicHtml4, HtmlTemplates.DynamicHtml5 })
			{
				if (!textReplacements.ContainsKey(textToReplace))
				{
					AddTextReplacement(textToReplace, replacement.ToString() + "<br />");
					break;
				}
			}
		}

		public void AddTextReplacement(string textToReplace, object replacement)
		{
			AddTextReplacement(textToReplace, replacement, false);
		}

		public void AddTextReplacement(string textToReplace, object replacement, bool convertNewLineToHtml)
		{
			textReplacements.Add(textToReplace, convertNewLineToHtml ? replacement.ToString().Replace("\r\n", "<br />").Replace("\n", "<br />") : replacement.ToString());
		}

		public void AddAttachment(string fileName, string textToAttach)
		{
			attachments.Add(fileName, textToAttach);
		}

		#endregion

		#region HtmlTemplates

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html Code")]
		public static class HtmlTemplates
		{
			//Common
			public const string AcceptedResponse = "Common.AcceptedResponse.htm";
			public const string ClearResponse = "Common.ClearResponse.htm";
			public const string ErrorResponse = "Common.ErrorResponse.htm";
			public const string MailProblemResponse = "Common.MailProblem.htm";
			public const string ValidatedResponse = "Common.ValidatedResponse.htm";
			public const string FreeFormResponse = "Common.FreeFormResponse.htm";
			public const string StatusUpdate = "Common.StatusUpdate.htm";
			public const string Empty = "Common.Empty.htm";
			public const string EmptyWithDynamicHtml5 = "Common.EmptyWithDynamicHtml5.htm";

			// Asycuda
			public const string AsycudaManifestUniversalEventResponse = "Asycuda.ManifestUniversalEventResponse.htm";

			//CA
			public const string MatchedResponse = "CA.MatchedResponse.htm";
			public const string NotMatchedResponse = "CA.NotMatchedResponse.htm";
			public const string RiskAssessmentACIResponse = "CA.RiskAssessmentACI.htm";
			public const string QueryResponse = "CA.QueryResponse.htm";
			public const string IIDResponse = "CA.IIDResponse.htm";
			public const string IIDResponseWithNotifyInfo = "CA.IIDResponseWithNotifyInfo.htm";
			public const string ConfirmedResponse = "CA.ConfirmedResponse.htm";
			public const string CADMessageResponse = "CA.CADMessageResponse.htm";

			//Constants
			public const string DynamicHtmlHeading = "<!--DynamicHtmlHeading-->";
			public const string DynamicHtml1 = "<!--DynamicHtml1-->";
			public const string DynamicHtml2 = "<!--DynamicHtml2-->";
			public const string DynamicHtml3 = "<!--DynamicHtml3-->";
			public const string DynamicHtml4 = "<!--DynamicHtml4-->";
			public const string DynamicHtml5 = "<!--DynamicHtml5-->";
			public const string EndSectionDetails = "<!--EndSection Details-->";
			public const string FromMessageSender = "<!--From Message Sender-->";
			public const string HeaderSectionDetails = "<!--HeaderSection Details-->";
			public const string AdditionalComments = "<!--AdditionalComments-->";
			internal const string Article = "<!--Article-->";
		}

		#endregion

		#region Implementation

		string GetEmailHtml()
		{
			var result = string.Empty;
			var key = "Enterprise.Customs.Business.MessageProcessors.HtmlTemplates." + bodyHtmlTemplateName;

			using (var stream = typeof(EmailDefBuilder).Assembly.GetManifestResourceStream(key))
			{
				if (stream != null)
				{
					result = new StreamReader(stream).ReadToEnd();
				}
			}

			if (!string.IsNullOrEmpty(result))
			{
				result = string.Format(result, argReplacements.ToArray());
				result = textReplacements.Aggregate(result, (current, pair) => current.Replace(pair.Key, pair.Value));
				result = Regex.Replace(result, @"(?<PreviousChar>.?)\s*<!--Article-->(\s*<.*?>)?\s*(?<NextChar>.)", GetReplacementString);
			}

			return result;
		}

		static string GetReplacementString(Match match)
		{
			ZString article = Grammar.Instance.IndefiniteArticlePrefix(match.Groups["NextChar"].Value).TrimEnd();
			var previousChar = match.Groups["PreviousChar"].Value;
			var makeTitle = string.IsNullOrEmpty(previousChar) || char.IsSymbol(previousChar, 0) || char.IsPunctuation(previousChar, 0);
			return match.Value.Replace(HtmlTemplates.Article, makeTitle ? article.ToTitleCase() : article);
		}

		readonly string emailSubject;
		readonly string bodyHtmlTemplateName;
		readonly Dictionary<string, string> attachments = new Dictionary<string, string>();
		readonly List<object> argReplacements = new List<object>();
		readonly Dictionary<string, string> textReplacements = new Dictionary<string, string>();

		#endregion
	}
}
