using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExportApprovalCertificateTypeRefCusCodeListParser : RefCusCodeListParser
	{
		const string tokgTag = "TOKG";

		bool tokgContainsSea;
		bool tokgContainsAir;
		string tokgDescription;

		public override string CurrentCodeType => currentCodeType;
		string currentCodeType;

		protected override string GetZZD_Description(string[] columns)
		{
			var descriptionPart1 = columns[1].Trim();
			var descriptionPart2 = columns[2].Trim();

			return CurrentCodeType == Constants.CodeType.ExportConstantApprovalCertificateType
					? descriptionPart2
					: string.Join(" - ", new[] { descriptionPart1, descriptionPart2 }.Where(c => !string.IsNullOrWhiteSpace(c)));
		}

		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z0-9]{4}$");

		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new ExportApprovalCertificateTypeRefCusCodeListAttributeParser();

		public override bool HasRefCusCodeOrAttributeTransportMode => true;

		protected override string GetZZD_Code(string[] columns) => CurrentCodeType == Constants.CodeType.ExportConstantApprovalCertificateType ? columns[1] : columns[0];

		protected override void OnLineRead(string line)
		{
			base.OnLineRead(line);
			var columns = line.Split(',');

			currentCodeType = columns[0].Equals(tokgTag, System.StringComparison.OrdinalIgnoreCase)
				? Constants.CodeType.ExportConstantApprovalCertificateType
				: Constants.CodeType.ExportApprovalCertificateType;

			if (CurrentCodeType == Constants.CodeType.ExportConstantApprovalCertificateType)
			{
				if (string.IsNullOrEmpty(tokgDescription))
				{
					tokgDescription = columns[2];
				}
				else
				{
					tokgDescription = tokgDescription + ", " + columns[2];
				} 
			}
		}

		protected override void AddRefCusCodeOrAttributeTransportModeIfNeeded(RefCusCodeList refCusCodeList, string[] columns)
		{
			var transportMode = columns[3];
			var transportModeList = new List<RefCusCodeOrAttributeTransportMode>();

			if (transportMode.Contains("海"))
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Sea,
				});

				tokgContainsSea = tokgContainsSea || CurrentCodeType == Constants.CodeType.ExportConstantApprovalCertificateType;
			}

			if (transportMode.Contains("空"))
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Air,
				});

				tokgContainsAir = tokgContainsAir || CurrentCodeType == Constants.CodeType.ExportConstantApprovalCertificateType;
			}

			if (transportModeList.Count > 0)
			{
				refCusCodeList.RefCusCodeOrAttributeTransportModes = transportModeList.ToArray();
			}
		}

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			if (CurrentCodeType ==  Constants.CodeType.ExportConstantApprovalCertificateType)
			{
				var existingTOKGItem = refCusCodeLists.FirstOrDefault(x => x.ZZD_Code == columns[1]);
				if (existingTOKGItem != null)
				{
					existingTOKGItem.ZZD_Description = existingTOKGItem.ZZD_Description + ", " + columns[2];
					return;
				}
			}

			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = GetZZD_Code(columns),
				ZZD_Description = GetZZD_Description(columns),
				ZZD_StartDate = GetZZD_StartDate(columns),
				ZZD_EndDate = GetZZD_EndDate(columns),
				ZZD_ZZK_NKCodeType = CurrentCodeType,
				ZZD_ZZZ_NKDataGrouping = ZZD_ZZZ_NKDataGrouping
			};

			var attributesAreValid = true;
			var languagesAreValid = true;

			if (CurrentCodeType ==  Constants.CodeType.ExportConstantApprovalCertificateType)
			{
				var parser = GetRefCusCodeListAttributeParserCore();

				if (parser != null && parser.Configs.Length > 0)
				{
					refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[parser.Configs.Length];
					attributesAreValid = parser.TryAddRefCusCodeListAttribute(refCusCodeList, columns);
				}
			}

			AddRefCusCodeOrAttributeTransportModeIfNeeded(refCusCodeList, columns);

			if (ValidateRefCusCodeList(refCusCodeLists, refCusCodeList) && attributesAreValid && languagesAreValid)
			{
				refCusCodeLists.Add(refCusCodeList);
			}
		}

		public override bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			var baseResult = base.TryParse(rows, out refCusCodeLists);
			if (baseResult && (tokgContainsSea || tokgContainsAir))
			{
				AddTokgToList(refCusCodeLists);
			}

			return baseResult;
		}

		protected override bool ValidateZZD_Code(RefCusCodeList refCusCodeList)
		{
			if (string.IsNullOrWhiteSpace(refCusCodeList.ZZD_Code))
			{
				return false;
			}

			if (ZZD_CodeRegex != null && !ZZD_CodeRegex.IsMatch(refCusCodeList.ZZD_Code))
			{
				return CurrentCodeType == Constants.CodeType.ExportConstantApprovalCertificateType;
			}

			return true;
		}

		void AddTokgToList(List<RefCusCodeList> refCusCodeLists)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = tokgTag,
				ZZD_Description = tokgDescription,
				ZZD_StartDate = StaticResources.DefaultZZD_StartDate,
				ZZD_EndDate = StaticResources.DefaultZZD_EndDate,
				ZZD_ZZK_NKCodeType = Constants.CodeType.ExportApprovalCertificateType,
				ZZD_ZZZ_NKDataGrouping = ZZD_ZZZ_NKDataGrouping
			};

			var transportModeList = new List<RefCusCodeOrAttributeTransportMode>();
			if (tokgContainsSea)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Sea,
				});
			}

			if (tokgContainsAir)
			{
				transportModeList.Add(new RefCusCodeOrAttributeTransportMode()
				{
					ZZU_TransportMode = Air,
				});
			}

			refCusCodeList.RefCusCodeOrAttributeTransportModes = transportModeList.ToArray();
			refCusCodeLists.Add(refCusCodeList);
		}

		const string Air = "AIR";
		const string Sea = "SEA";
	}
}
