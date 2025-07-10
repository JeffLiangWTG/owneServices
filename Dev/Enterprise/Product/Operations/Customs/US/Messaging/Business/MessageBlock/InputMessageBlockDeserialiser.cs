using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class InputMessageBlockDeserialiser : MessageBlockDeserialiser
	{
		public InputMessageBlockDeserialiser()
		{
		}

		protected override IReadOnlyList<MessageBlockDictionary> Dictionaries
		{
			get { return dictionaries ?? (dictionaries = new MessageBlockDictionaryGenerator().GenerateBlockDictionaries(typeof(InputBlockAttribute))); }
		}

		[ThreadStatic]
		static IReadOnlyList<MessageBlockDictionary> dictionaries;

		protected override string GetVersion(string[] applicationCodes, string applicationID, string eightyCharacterBlock)
		{
			var result = string.Empty;
			if (applicationCodes.Contains(CBPEDIInterchange.ApplicationCodes.USCustomsImport))
			{
				switch (applicationID)
				{
					case ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData:
						result = GetVersionForFTZBlocks(applicationID, eightyCharacterBlock);
						break;
					case ACEApplicationIdentifierCodeList.Codes.EntrySummary:
						result = GetVersionForEntrySummary(eightyCharacterBlock);
						break;
				}
			}
			return result;
		}
	}
}
