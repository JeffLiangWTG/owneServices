using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class BondedAreaCodeParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.BondedAreaCode;
		protected override string GetZZD_Code(string[] columns) => columns[5];
		protected override string GetZZD_Description(string[] columns) => columns[7].Replace("　", " ");
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z0-9]{5}$");
		protected override bool CustomisedRowValidate(string[] columns) => !string.IsNullOrWhiteSpace(columns[7]);
		public override bool HasRefCusCodeOrAttributeTransportMode => true;
		public override bool HasAttribute => true;

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = GetZZD_Code(columns),
				ZZD_Description = GetZZD_Description(columns),
			};

			AddAttributesIfExist(refCusCodeList, columns);
			AddTransportModesIfExist(refCusCodeList, columns);

			if (ValidateRefCusCodeList(refCusCodeLists, refCusCodeList))
			{
				refCusCodeLists.Add(refCusCodeList);
			}
		}

		static void AddAttributesIfExist(RefCusCodeList refCusCodeList, string[] columns)
		{
			var refCusCodeListAttributeList = new List<RefCusCodeListAttribute>();

			AddStringAttributeIfExist(refCusCodeListAttributeList, "Type", columns[0]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "Jurisdiction", columns[1]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "Abbreviation", columns[6]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "Address", columns[8]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "UserCode", columns[9]);
			AddStringAttributeIfExist(refCusCodeListAttributeList, "UserCodeType", columns[10]);

			AddBooleanAttributeIfExist(refCusCodeListAttributeList, "AfterISCargoManagement", columns[4]);

			if (refCusCodeListAttributeList.Count > 0)
			{
				refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributeList.ToArray();
			}
		}

		static void AddBooleanAttributeIfExist(List<RefCusCodeListAttribute> refCusCodeListAttributeList, string attributeName, string attributeValue)
		{
			attributeValue = attributeValue.Trim();

			var booleanValue = attributeValue == "○" ? Constants.YesNoList.Yes : Constants.YesNoList.No;
			var refCusCodeListAttribute = new RefCusCodeListAttribute()
			{
				ZZE_ZXE_NKName = attributeName,
				ZZE_Value = booleanValue
			};

			refCusCodeListAttributeList.Add(refCusCodeListAttribute);
		}

		static void AddStringAttributeIfExist(List<RefCusCodeListAttribute> refCusCodeListAttributeList, string attributeName, string attributeValue)
		{
			attributeValue = attributeValue.Trim();

			if (attributeValue.Length > 255)
			{
				attributeValue = attributeValue.Substring(0, 255);
			}

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

		static void AddTransportModesIfExist(RefCusCodeList refCusCodeList, string[] columns)
		{
			var transportModeList = new List<RefCusCodeOrAttributeTransportMode>();

			var isSea = columns[2].Trim();
			if (isSea == "○")
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = "SEA",
				});
			}

			var isAir = columns[3].Trim();
			if (isAir == "○")
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = "AIR",
				});
			}

			if (transportModeList.Count > 0)
			{
				refCusCodeList.RefCusCodeOrAttributeTransportModes = transportModeList.ToArray();
			}
		}
	}
}
