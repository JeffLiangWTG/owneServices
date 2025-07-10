using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FsisEstNumbersCrawler.Properties;
using CsvHelper.Configuration.Attributes;

namespace FsisEstNumbersCrawler.Models
{
	public class Establishment
	{
		const char Separator = '+';

		[Name("establishment_number")]
		public string EstNumber { get; set; }

		[Name("establishment_name")]
		public string Company { get; set; }

		[Name("street")]
		public string Street { get; set; }

		[Name("city")]
		public string City { get; set; }

		[Name("state")]
		public string State { get; set; }

		[Name("zip")]
		public string Zip { get; set; }

		[Name("phone")]
		public string Phone { get; set; }

		[Name("grant_date"), Format("M/d/yyyy", "yyyy-MM-dd", "MM-dd-yyyy")]
		public DateTime GrantDate { get; set; }

		public IEnumerable<RefCusCodeList> ToCodeLists()
		{
			var codes = EstNumber.Split(Separator);
			foreach (var code in codes)
			{
				var codeList = new RefCusCodeList
				{
					ZZD_ZZK_NKCodeType = Settings.Default.ZZD_ZZK_NKCodeType,
					ZZD_Code = RemoveSpecialCharacters(code),
					ZZD_Description = RemoveSpecialCharacters(code),
					ZZD_StartDate = GrantDate,
					ZZD_EndDate = Settings.Default.ZZD_EndDate,
					ZZD_ZZZ_NKDataGrouping = Settings.Default.ZZD_ZZZ_NKDataGrouping
				};
				var attributelList = new List<RefCusCodeListAttribute>();

				AddAttribute(attributelList, Settings.Default.CompanyAttributeType, Company);
				AddAttribute(attributelList, Settings.Default.StreetAttributeType, Street);
				AddAttribute(attributelList, Settings.Default.CityAttributeType, City);
				AddAttribute(attributelList, Settings.Default.StateAttributeType, State);
				AddAttribute(attributelList, Settings.Default.ZipAttributeType, Zip);
				AddAttribute(attributelList, Settings.Default.PhoneAttributeType, Phone);
				codeList.RefCusCodeListAttributes = attributelList.ToArray();

				yield return codeList;
			}
		}

		static void AddAttribute(List<RefCusCodeListAttribute> list, string typeName, string value)
		{
			var attributeValue = value.Trim();
			if (!string.IsNullOrEmpty(attributeValue))
			{
				list.Add(CreateNewAttribute(typeName, RemoveSpecialCharacters(attributeValue)));
			}
		}

		static string RemoveSpecialCharacters(string input)
		{
			return Regex.Replace(input, "[^a-zA-Z0-9&,#.\\// -']", "").Trim();
		}

		static RefCusCodeListAttribute CreateNewAttribute(string attributeName, string attributeValue)
		{
			var value = Regex.Replace(attributeValue, @"\s+", " ").Trim();
			return new RefCusCodeListAttribute()
			{
				ZZE_ZXE_NKName = attributeName,
				ZZE_Value = value.Length > 255 ? value.Substring(0, 252) + "..." : value
			};
		}
	}
}
