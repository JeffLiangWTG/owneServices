using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class SeaCarrierParser
	{
		readonly static Regex ZZ4_CodeRegex = new Regex("^[A-Z0-9]{4}$");

		static string Type1 => "Type1";

		static string Type2 => "Type2";

		const string column2Name = "Location";

		const string column3Name = "NACCSUserCode";

		const string column4Name = "NACCSUserName";

		public static bool TryParse(IEnumerable<string> rows, IEnumerable<string> vesselRows, out List<RefCarrierCode> refCarrierCodeList)
		{
			var currentDescriptionType = Type1;
			refCarrierCodeList = new List<RefCarrierCode>();

			var vesselDictionary = vesselRows.Select(ParserHelper.SplitComma).GroupBy(vesselColumns => vesselColumns[3])
			.ToDictionary(group => group.Key, group => group.ToArray());

			foreach (var row in rows)
			{
				var columns = ParserHelper.SplitComma(row);
				if (ZZ4_CodeRegex.IsMatch(columns[0]))
				{
					AddRefCarrierCodeList(refCarrierCodeList, columns, vesselDictionary, currentDescriptionType);
				}
				else if (columns[0] == "2.システム参加NVOCC")
				{
					currentDescriptionType = Type2;
				}
				else if (columns[0] == "3.混載貨物等用コード")
				{
					currentDescriptionType = Type1;
				}
			}

			if (refCarrierCodeList.Any())
			{
				return true;
			}

			ErrorWriter.WriteError("No RefCarrierCodeList is parsed");
			return false;
		}

		static void AddRefCarrierCodeList(List<RefCarrierCode> refCarrierCodeList, string[] columns, Dictionary<string, string[][]> vesselDictionary, string currentDescriptionType)
		{
			var refCarrierCode = new RefCarrierCode()
			{
				ZZ4_Code = columns[0],
				ZZ4_Description = currentDescriptionType == Type1 ? columns[1] : columns[2],
			};

			var refCarrierCodeAttributes = new List<RefCarrierCodeAttribute>();

			AddRefCarrierCodeAttributeIfNeeded(column2Name, columns[2]);
			AddRefCarrierCodeAttributeIfNeeded(column3Name, columns[3]);
			AddRefCarrierCodeAttributeIfNeeded(column4Name, columns[4]);

			refCarrierCode.RefCarrierCodeAttributes = refCarrierCodeAttributes.Count > 0 ? refCarrierCodeAttributes.ToArray() : null;

			if (!refCarrierCodeList.Any(x => x.ZZ4_Code == refCarrierCode.ZZ4_Code))
			{
				RefCarrierVesselPivot[] refCarrierVesselPivots = null;
				if (vesselDictionary.TryGetValue(columns[0], out var matchingVesselColumns))
				{
					refCarrierVesselPivots = matchingVesselColumns.Select(x => new RefCarrierVesselPivot() { ZZQ_ZZO_NKCode = x[1], ZZQ_ZZO_NKRadioCallSign = x[2] }).ToArray();
				}
				
				if (refCarrierVesselPivots != null)
				{
					refCarrierCode.RefCarrierVesselPivots = refCarrierVesselPivots;
				}
				refCarrierCodeList.Add(refCarrierCode);
			}

			void AddRefCarrierCodeAttributeIfNeeded(string name, string value)
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					var refCarrierCodeAttribute = new RefCarrierCodeAttribute();
					refCarrierCodeAttribute.ZZG_Name = name;
					refCarrierCodeAttribute.ZZG_Value = value.Length <= 100 ? value : value.Substring(0, 100);
					refCarrierCodeAttributes.Add(refCarrierCodeAttribute);
				}
			}
		}
	}
}
