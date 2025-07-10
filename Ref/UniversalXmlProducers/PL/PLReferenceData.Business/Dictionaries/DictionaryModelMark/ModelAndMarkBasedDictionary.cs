using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryModelMark
{
	public class ModelAndMarkBasedElement
	{
		public string Code { get; set; }
		public DateTime? ValidFrom { get; set; }
		public DateTime? ValidTo { get; set; }

		public string Model { get; set; }
		public string Mark {  get; set; }

		public string ValidToString
		{
			get
			{
				if (ValidTo == null)
				{
					return DictionariesConstants.DefaultEndDate;
				}
				else
				{
					return Convert.ToDateTime(ValidTo, CultureInfo.InvariantCulture).ToString(DictionariesConstants.DateTimeConverterFormat, CultureInfo.InvariantCulture);
				}
			}
		}
		public string ValidFromString
		{
			get
			{
				if (ValidFrom == null)
				{
					return DictionariesConstants.DefaultEndDate;
				}
				else
				{
					return Convert.ToDateTime(ValidFrom, CultureInfo.InvariantCulture).ToString(DictionariesConstants.DateTimeConverterFormat, CultureInfo.InvariantCulture);
				}
			}
		}
		public ModelAndMarkBasedElement()
		{
			Code = string.Empty;
			Model = string.Empty;
			Mark = string.Empty;
			ValidTo = null;
			ValidFrom = null;
		}

		public ModelAndMarkBasedElement(string code, string model = null, string mark = null, DateTime? validTo = null, DateTime? validFrom = null)
		{
			Code = code;
			Model = model;
			Mark = mark;
			ValidTo = validTo;
			ValidFrom = validFrom;
		}
	}
}
