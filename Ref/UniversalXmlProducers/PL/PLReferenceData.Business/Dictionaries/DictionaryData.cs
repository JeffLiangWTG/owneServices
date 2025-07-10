using System.Collections.Generic;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries
{
	public class DictionaryData
	{
		public string Code { get; } // used mostly for url and identifying dictionary, example : https://puesc.gov.pl/seap_pdr_extimpl/slowniki/001
		public string CW1Code { get; }
		public RefDataType RefDataType { get; }
		public IReadOnlyList<ExcludedDictionaryCodes> ExcludedCodesList { get; }
		public IReadOnlyList<DictionaryData> AdditionalDictionaries { get; }
		public IReadOnlyList<string> SearchedCodes { get; }
		public bool PublishedByTestPuesc { get; } // determines from where files should be searched for -> puesc.gov.pl or test.puesc.gov.pl

		public DictionaryData(string code, string cw1Code, RefDataType type, List<ExcludedDictionaryCodes> excludedCodesList = null,
			List<DictionaryData> additionalDictionaries = null, List<string> searchedCodes = null, bool publishedByTestPuesc = false)
		{
			Code = code;
			CW1Code = cw1Code;
			RefDataType = type;
			ExcludedCodesList = excludedCodesList ?? new List<ExcludedDictionaryCodes>();
			AdditionalDictionaries = additionalDictionaries ?? new List<DictionaryData>();
			SearchedCodes = searchedCodes ?? new List<string>();
			PublishedByTestPuesc = publishedByTestPuesc;
		}
	}
}
