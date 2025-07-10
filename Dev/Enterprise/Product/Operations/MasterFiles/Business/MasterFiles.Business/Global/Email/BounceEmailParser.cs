using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BounceEmailParser
	{
		public BounceEmailParser(string message)
		{
			var seperators =
				new[] { BounceEmailConstants.ContentTypeDeliveryStatus } // after human readable explanation, the machine parsable part should be proceeded by 'message/delivery-status' (as defined in RFC 6522)
				.Concat(BounceEmailConstants.HeaderKeys.Select(x => x + ":")) // however, not all DSNs follow this format, so we will split at any header values that we are looking for
				.ToArray();

			var approximateMessageSplit = message.Split(seperators, 2, StringSplitOptions.None);
			humanReadableExplanationPart = approximateMessageSplit[0];
			originalHeadersMessagePart = approximateMessageSplit.Length > 1 ? approximateMessageSplit[1] : string.Empty;
		}

		readonly string humanReadableExplanationPart;
		readonly string originalHeadersMessagePart;

		#region Reason

		public static string GetStatusCodeFromMatchedPhrase(ZString reason)
		{
			BouncebackErrorCodes errorCodes = new BouncebackErrorCodes();

			foreach (var code in IdentifyKeyWordsFromBounceBackReason(reason))
			{
				if (errorCodes.ContainsCode(code.ToString(CultureInfo.InvariantCulture)))
				{
					return code.ToString(CultureInfo.InvariantCulture);
				}
			}

			return BouncebackErrorCodes.Codes.EUNK;
		}

		static List<int> IdentifyKeyWordsFromBounceBackReason(ZString bounceBackReason)
		{
			List<int> numList = new List<int>();
			MatchCollection collection = Regex.Matches(bounceBackReason, @"[0-9]+");

			foreach (var match in collection)
			{
				ZInt temp;
				if (ZInt.TryParse(match.ToString(), out temp))
				{
					numList.Add(temp);
				}
			}

			return numList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "is a regex pattern")]
		public string Reason
		{
			get
			{
				if (reason == null)
				{
					const string reasonPattern = @"^[ \t]*Reason: (?<Value>.*)$";

					var regex = new Regex(reasonPattern, RegexOptions.Multiline);
					var match = regex.Match(humanReadableExplanationPart);
					if (match.Success)
					{
						reason = match.Groups["Value"].Value;
					}
					else
					{
						reason = string.Empty;
					}
				}

				return reason;
			}
		}
		string reason;

		public string ReasonCode
		{
			get { return GetStatusCodeFromMatchedPhrase(Reason); }
		}

		#endregion

		#region Original Headers

		Dictionary<string, string> OriginalHeaders
		{
			get
			{
				if (originalHeaders == null)
				{
					originalHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

					if (!string.IsNullOrEmpty(originalHeadersMessagePart))
					{
						var keyValuePairsPattern = string.Format(CultureInfo.InvariantCulture, (NoResString)@"^(?<Key>{0}):[ \t]*(?<Value>([^\r\n]|(\r\n|\n)[ \t]+)*)(\r\n|\n)", GetOrRegex(BounceEmailConstants.HeaderKeys));
						var regex = new Regex(keyValuePairsPattern, RegexOptions.Multiline);
						var matches = regex.Matches(originalHeadersMessagePart);
						foreach (Match match in matches)
						{
							var key = match.Groups["Key"].Value;
							var value = match.Groups["Value"].Value.Trim();
							originalHeaders[key] = value;
						}
					}
				}

				return originalHeaders;
			}
		}
		Dictionary<string, string> originalHeaders;

		public string BusinessEntityID => OriginalHeaders.TryGetValue(BounceEmailConstants.BusinessEntityIDKey, out string result) ? result : string.Empty;

		public string BusinessEntityTableCode => OriginalHeaders.TryGetValue(BounceEmailConstants.BusinessEntityTableCodeKey, out string result) ? result : string.Empty;
		public string BusinessEntityJobNumber => OriginalHeaders.TryGetValue(BounceEmailConstants.BusinessEntityJobNumberKey, out string result) ? result : string.Empty;

		public string DocumentName => OriginalHeaders.TryGetValue(BounceEmailConstants.DocumentNameKey, out string result) ? DecodeDocumentName(result) : string.Empty;

		public static string EncodeDocumentName(string name)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
		}

		public static string DecodeDocumentName(string encodedName)
		{
			try
			{
				var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encodedName));
				for (int i = 0; i < decoded.Length; ++i)
				{
					int ch = decoded[i];
					if (ch >= 256 || ch < 32)
					{
						// not a valid 8 bit character
						return encodedName;
					}
				}

				return decoded;
			}
			catch (FormatException)
			{
				return encodedName;
			}
		}

		public string SenderStaffID => OriginalHeaders.TryGetValue(BounceEmailConstants.SenderStaffIDKey, out string result) ? result : string.Empty;

		public string SenderTimeText => OriginalHeaders.TryGetValue(BounceEmailConstants.SentTimeKey, out string result) ? result : string.Empty;

		public MailAddressCollection BouncedRecipients
		{
			get
			{
				if (bouncedRecipients == null)
				{
					bouncedRecipients = new MailAddressCollection();

					var toEmails = GetOriginalHeaderEmails(BounceEmailConstants.ToKey);
					var ccEmails = GetOriginalHeaderEmails(BounceEmailConstants.CcKey);
					var bccEmails = GetOriginalHeaderEmails(BounceEmailConstants.BccKey);

					var originalRecipientAddresses =
						((toEmails != null) ? toEmails.Select(x => x.Address) : Enumerable.Empty<string>())
						.Concat((ccEmails != null) ? ccEmails.Select(x => x.Address) : Enumerable.Empty<string>())
						.Concat((bccEmails != null) ? bccEmails.Select(x => x.Address) : Enumerable.Empty<string>())
						.Where(x => !string.IsNullOrEmpty(x)).ToArray();

					if (originalRecipientAddresses.Length > 0)
					{
						foreach (var email in GetWholeWordsFoundInExplanationPart(originalRecipientAddresses))
						{
							bouncedRecipients.Add(email);
						}
					}
				}

				return bouncedRecipients;
			}
		}
		MailAddressCollection bouncedRecipients;

		IEnumerable<string> GetWholeWordsFoundInExplanationPart(IEnumerable<string> wordsToFind)
		{
			var found = new HashSet<string>();
			var emailsPattern = string.Format(CultureInfo.InvariantCulture, (NoResString)@"(?<!\w)({0})(?!\w)", GetOrRegex(wordsToFind, false));
			var regex = new Regex(emailsPattern);
			var matches = regex.Matches(humanReadableExplanationPart);
			foreach (Match match in matches)
			{
				var value = match.Groups[0].Value;
				if (!found.Contains(value))
				{
					found.Add(value);
					yield return value;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "is a regex pattern")]
		static string GetOrRegex(IEnumerable<string> orOptions, bool caseInsensitive = true)
		{
			orOptions = orOptions.Select(word => Regex.Escape(word));
			if (caseInsensitive)
			{
				return string.Format(CultureInfo.InvariantCulture, "(?i)({0})(?-i)", string.Join("|", orOptions));
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, "({0})", string.Join("|", orOptions));
			}
		}

		MailAddressCollection GetOriginalHeaderEmails(string type)
		{
			string value;
			if (!OriginalHeaders.TryGetValue(type, out value))
			{
				return null;
			}

			try
			{
				var mailAddressCollection = new MailAddressCollection();
				if (!string.IsNullOrEmpty(value))
				{
					mailAddressCollection.Add(value);
				}
				return mailAddressCollection;
			}
			catch (FormatException)
			{
				return null;
			}
		}

		#endregion

		#region Diagnositic Info

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "logging info doesn't require translation")]
		public ZString GetDiagnositicInfo()
		{
			var builder = new ZStringBuilder();
			builder.AppendLine("Human Readable Part:");
			builder.AppendLine(humanReadableExplanationPart);
			builder.AppendLine();
			builder.AppendLine("Original Message Header Part:");
			builder.AppendLine(originalHeadersMessagePart);
			return builder.ToString();
		}

		#endregion

		#region Constants

		#region SuppressResourceStringsCheckRegion

		public static class BounceEmailConstants
		{
			public const string ContentTypeDeliveryStatus = "Content-Type: message/delivery-status";
			public const string ReasonKey = "Reason";

			public const string ReceivedKey = "Received";
			public const string BusinessEntityIDKey = "X-BusinessEntityID";
			public const string BusinessEntityTableCodeKey = "X-BusinessEntityTableCode";
			public const string BusinessEntityJobNumberKey = "X-BusinessEntityJobNumber";
			public const string DocumentNameKey = "X-DocumentName";
			public const string SenderStaffIDKey = "X-SenderStaffID";
			public const string SentTimeKey = "Date";
			public const string ToKey = "To";
			public const string CcKey = "Cc";
			public const string BccKey = "Bcc";
			public const string FromKey = "From";
			public const string ReturnPathKey = "Return-Path";

			public static IEnumerable<string> HeaderKeys
			{
				get
				{
					yield return ReceivedKey;
					yield return BusinessEntityIDKey;
					yield return BusinessEntityTableCodeKey;
					yield return BusinessEntityJobNumberKey;
					yield return DocumentNameKey;
					yield return SenderStaffIDKey;
					yield return SentTimeKey;
					yield return ToKey;
					yield return CcKey;
					yield return BccKey;
					yield return FromKey;
					yield return ReturnPathKey;
				}
			}
		}

		#endregion

		#endregion
	}
}
