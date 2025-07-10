using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class FSBRefCusCodeListParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.FSB;

		protected override string GetZZD_Code(string[] columns) => columns[0].Normalize(NormalizationForm.FormKC);

		protected override string GetZZD_Description(string[] columns) => columns[1];

		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z0-9]{12}$");

		public override bool HasAttribute => true;

		public override bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			refCusCodeLists = new List<RefCusCodeList>();

			foreach (var row in rows)
			{
				OnLineRead(row);
				string[] columns = null;

				using (var parser = new TextFieldParser(new StringReader(row)))
				{
					parser.HasFieldsEnclosedInQuotes = true;
					parser.SetDelimiters("\t");
					columns = parser.ReadFields();
				}

				if (CustomisedRowValidate(columns))
				{
					AddRefCusCodeList(refCusCodeLists, columns);
				}
			}

			if (refCusCodeLists.Any())
			{
				return true;
			}

			ErrorWriter.WriteError("No RefCusCodeList is parsed");
			return false;
		}

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = GetZZD_Code(columns),
				ZZD_Description = GetZZD_Description(columns),
			};

			AddAttributesIfExist(refCusCodeList, columns);

			if (ValidateRefCusCodeList(refCusCodeLists, refCusCodeList))
			{
				refCusCodeLists.Add(refCusCodeList);
			}
		}

		static void AddAttributesIfExist(RefCusCodeList refCusCodeList, string[] columns)
		{
			var refCusCodeListAttributeList = new List<RefCusCodeListAttribute>();

			AddStringAttributeIfExist(refCusCodeListAttributeList, "Address1", columns[2]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "Address2", columns[3]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "Address3", columns[4]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "Address4", columns[5]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "CountryCode", columns[6]);

			if (refCusCodeListAttributeList.Count > 0)
			{
				refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributeList.ToArray();
			}
		}

		static void AddStringAttributeIfExist(List<RefCusCodeListAttribute> refCusCodeListAttributeList, string attributeName, string attributeValue)
		{
			if (!string.IsNullOrEmpty(attributeValue))
			{
				var refCusCodeListAttribute = new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = attributeName,
					ZZE_Value = attributeValue
				};

				refCusCodeListAttributeList.Add(refCusCodeListAttribute);
			}
		}
	}
}
