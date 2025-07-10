using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public class PuescBasedDictionaryElement
	{
		// int id; -  not needed by us, but present in xml
		public string Code { get; set; }
		public string Description { get; set; }
		public string DescriptionEng { get; set; }
		public DateTime? ValidTo { get; set; } // older date than current date does not mean that this rule is no longer needed !
		public DateTime? ValidFrom { get; set; }

		public string DictionaryIndex { get; set; }

		public PuescBasedDictionaryElement() : this(string.Empty)
		{
		}

		public PuescBasedDictionaryElement(string code, string description = null, string dictionaryIndex = null, string descriptionEng = null, DateTime? validTo = null, DateTime? validFrom = null)
		{
			Code = code;
			Description = description ?? string.Empty;
			DescriptionEng = descriptionEng ?? string.Empty;
			ValidTo = validTo;
			ValidFrom = validFrom;
			DictionaryIndex = dictionaryIndex ?? string.Empty;
		}
	}
}
