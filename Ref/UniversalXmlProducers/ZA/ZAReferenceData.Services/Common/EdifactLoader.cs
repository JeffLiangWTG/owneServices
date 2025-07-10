using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D96B.Messages.GESMES;
using Enterprise.Edifact.D96B.Messages.PRODAT;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public static class EdifactLoader
	{
		public static PRODATMessage LoadProdatMessage(string messageText) => LoadMessage<PRODATMessage>(messageText);
		public static GESMESMessage LoadGesmesMessage(string messageText) => LoadMessage<GESMESMessage>(messageText);

		static T LoadMessage<T>(string messageText) where T : SegmentGroup
		{
			var messageFactory = new MessageFactory();
			messageFactory.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(PRODATMessage), "UN", "D", "96B", "PRODAT"));
			messageFactory.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(GESMESMessage), "UN", "D", "96B", "GESMES"));

			var content = PrepareMessage(messageText);

			return (T)messageFactory.GetMessage(ZACharacterSet.Instance, content);
		}

		static string PrepareMessage(string messageText)
		{
			var cleanMessage = messageText;

			cleanMessage = cleanMessage.Replace("\r", "").Replace("\n", "");
			cleanMessage = Regex.Replace(cleanMessage, "'[ ]*", "'");
			cleanMessage = Regex.Replace(cleanMessage, "[^']FTX\\+", "'FTX+");
			cleanMessage = Regex.Replace(cleanMessage, "\\?FTX\\+", "'FTX+");

			var unhIndex = cleanMessage.IndexOf("UNH", StringComparison.Ordinal);
			if (unhIndex > 0)
			{
				cleanMessage = cleanMessage.Substring(unhIndex);
			}

			return cleanMessage;
		}

		#region Common Validation Helpers
		internal static bool CheckMessageSectionExists(string name, MessageSectionBase section, StringBuilder errorCollector, string lineNumber = "")
			=> Check(errorCollector, section == null, $"Expected '{name}' message section missing", lineNumber);

		internal static bool CheckMessageSectionOccurrences<T>(string name, T section, int count, StringBuilder errorCollector, string lineNumber = "") where T : MessageSectionBase, IEnumerable
			=> Check(errorCollector, section.Count != count && AllSegmentsNotNull(section), $"Expected {count} occurrence(s) of '{name}' message section", lineNumber);

		internal static bool CheckMessageSectionMinOccurrences<T>(string name, T section, int count, StringBuilder errorCollector, string lineNumber = "") where T : MessageSectionBase, IEnumerable
			=> Check(errorCollector, section.Count < count && AllSegmentsNotNull(section), $"Expected at least {count} occurrence(s) of '{name}' message section", lineNumber);

		internal static bool Check(StringBuilder errorCollector, bool isInvalid, string errorMessage, string lineNumber = "")
		{
			var result = true;

			if (isInvalid)
			{
				errorCollector.AppendLine(errorMessage + (string.IsNullOrEmpty(lineNumber) ? string.Empty : $". LineNumber: {lineNumber}"));
				result = false;
			}

			return result;
		}

		internal static bool AllSegmentsNotNull<T>(T section) where T : MessageSectionBase, IEnumerable
		{
			var notNull = true;

			var enumerator = section.GetEnumerator();
			enumerator.Reset();
			while (notNull && enumerator.MoveNext())
			{
				notNull &= enumerator.Current != null;
			}

			return notNull;
		}
		#endregion

		internal const string SectionUNH = "UNH";
		internal const string SectionBGM = "BGM";
		internal const string SectionDTM = "DTM";
		internal const string SectionLIN = "LIN";
		internal const string SectionPIA = "PIA";
		internal const string SectionMEA = "MEA";
		internal const string SectionFTX = "FTX";
		internal const string SectionPGI = "PGI";
		internal const string SectionDSI = "DSI";
		internal const string SectionARR = "ARR";
		internal const string SectionUNT = "UNT";

		internal const string ZAR = "ZAR";
	}
}
