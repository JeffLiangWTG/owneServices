using System.Collections.Generic;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries
{
	public class ExcludedDictionaryCodes
	{
		public string DictionaryCode { get; set; }
		public List<string> ExcludedCodes { get; set; }

		public ExcludedDictionaryCodes()
		{
			DictionaryCode = string.Empty;
			ExcludedCodes = new List<string>();
		}
	}
}
