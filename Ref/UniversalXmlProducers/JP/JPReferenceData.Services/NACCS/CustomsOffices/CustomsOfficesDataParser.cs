using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class CustomsOfficesDataParser
	{
		int index;
		string errorStr = string.Empty;
		string codePrefix = string.Empty;
		string customsJurisdiction = string.Empty;

		const string CustomsJurisdiction = "CustomsJurisdiction";
		const string Reference = "Reference";

		public IEnumerable<RefCusCodeList> ParseToRefCusCodeLists(IEnumerable<string> records)
		{
			codePrefix = string.Empty;
			customsJurisdiction = string.Empty;

			var codeLists = new List<RefCusCodeList>();

			foreach (var record in records)
			{
				(var isValid, var array) = ValidationData(record);

				if (isValid)
				{
					var cusCode = new RefCusCodeList();
					SetRefCusCodeList(array, cusCode);
					SetRefCusCodeListAttribute(array, cusCode);
					codeLists.Add(cusCode);
				}
			}

			if (!codeLists.Any())
			{
				errorStr = "Can not find any Customs Office Codes!";
			}

			return codeLists;
		}

		(bool IsValid, string[] Array) ValidationData(string record)
		{
			index = index + 1;

			if (index <= 4 || string.IsNullOrWhiteSpace(record))
			{
				return (false, null);
			}

			var arrStr = record.Split(',');
			if (arrStr.Length != 7 || string.IsNullOrWhiteSpace(arrStr[2]))
			{
				return (false, null);
			}

			return (true, arrStr);
		}

		void SetRefCusCodeList(string[] array, RefCusCodeList cusCode)
		{
			codePrefix = string.IsNullOrWhiteSpace(array[1]) ? codePrefix : array[1];
			cusCode.ZZD_Code = codePrefix + array[2];
			cusCode.ZZD_Description = string.IsNullOrWhiteSpace(array[4]) ? cusCode.ZZD_Code : array[4];
		}

		void SetRefCusCodeListAttribute(string[] arrStr, RefCusCodeList cusCode)
		{
			var attrs = new RefCusCodeListAttribute[2];
			customsJurisdiction = string.IsNullOrWhiteSpace(arrStr[1]) ? customsJurisdiction : arrStr[1];

			var jurisdictionAttr = new RefCusCodeListAttribute();
			jurisdictionAttr.ZZE_ZXE_NKName = CustomsJurisdiction;
			jurisdictionAttr.ZZE_Value = customsJurisdiction;
			attrs[0] = jurisdictionAttr;

			var referenceAttr = new RefCusCodeListAttribute();
			referenceAttr.ZZE_ZXE_NKName = Reference;
			referenceAttr.ZZE_Value = arrStr[5];
			attrs[1] = referenceAttr;

			cusCode.RefCusCodeListAttributes = attrs;
		}

		public string ErrorStr { get { return errorStr; } }
	}
}
