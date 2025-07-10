using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class RefCusCodeListParser
	{
		protected virtual string GetZZD_Code(string[] columns) => columns[0];
		protected virtual string GetZZD_Description(string[] columns) => columns[1];
		protected virtual DateTime GetZZD_StartDate(string[] columns) => StaticResources.DefaultZZD_StartDate;
		protected virtual DateTime GetZZD_EndDate(string[] columns) => StaticResources.DefaultZZD_EndDate;

		protected virtual Regex ZZD_CodeRegex => null;
		protected virtual Regex ZZD_DescriptionRegex => null;

		public virtual bool HasAttribute => RefCusCodeListAttributeParser != null && RefCusCodeListAttributeParser.Configs.Length > 0;
		public bool HasLanguage => RefCusCodeListLanguageParser?.Configs != null && RefCusCodeListLanguageParser.Configs.Length > 0;
		public virtual bool HasRefCusCodeOrAttributeTransportMode => false;

		public abstract string CurrentCodeType { get; }
		protected const string ZZD_ZZZ_NKDataGrouping = "JP";

		RefCusCodeListAttributeParser refCusCodeListAttributeParser;
		public RefCusCodeListAttributeParser RefCusCodeListAttributeParser
		{
			get
			{
				if (refCusCodeListAttributeParser == null)
				{
					refCusCodeListAttributeParser = GetRefCusCodeListAttributeParserCore();
				}
				return refCusCodeListAttributeParser;
			}
		}
		protected virtual RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => null;

		RefCusCodeListLanguageParser refCusCodeListLanguageParser;
		public RefCusCodeListLanguageParser RefCusCodeListLanguageParser
		{
			get
			{
				if (refCusCodeListLanguageParser == null)
				{
					refCusCodeListLanguageParser = GetRefCusCodeListLanguageParserCore();
				}
				return refCusCodeListLanguageParser;
			}
		}
		protected virtual RefCusCodeListLanguageParser GetRefCusCodeListLanguageParserCore() => null;

		protected virtual bool ValidateZZD_Code(RefCusCodeList refCusCodeList)
		{
			if (string.IsNullOrWhiteSpace(refCusCodeList.ZZD_Code))
			{
				return false;
			}

			if (ZZD_CodeRegex != null && !ZZD_CodeRegex.IsMatch(refCusCodeList.ZZD_Code))
			{
				return false;
			}

			return true;
		}

		protected virtual bool ValidateZZD_Description(RefCusCodeList refCusCodeList)
		{
			if (string.IsNullOrWhiteSpace(refCusCodeList.ZZD_Description))
			{
				return false;
			}

			if (ZZD_DescriptionRegex != null && !ZZD_DescriptionRegex.IsMatch(refCusCodeList.ZZD_Description))
			{
				return false;
			}

			return true;
		}

		protected virtual bool ValidateUniqueness(IEnumerable<RefCusCodeList> refCusCodeLists, RefCusCodeList refCusCodeList)
		{
			return refCusCodeLists.All(c => c.ZZD_Code != refCusCodeList.ZZD_Code);
		}

		protected virtual Func<IEnumerable<RefCusCodeList>, RefCusCodeList, bool> ValidateRefCusCodeList => (refCusCodeLists, refCusCodeList)
			=> ValidateZZD_Code(refCusCodeList) && ValidateZZD_Description(refCusCodeList) && ValidateUniqueness(refCusCodeLists, refCusCodeList);

		public virtual bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			refCusCodeLists = new List<RefCusCodeList>();

			foreach (var row in rows)
			{
				OnLineRead(row);
				var columns = ParserHelper.SplitComma(row);

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

		protected virtual void OnLineRead(string line)
		{ }

		protected virtual void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			AddRefCusCodeList(refCusCodeLists, columns, true);
		}

		protected virtual void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns, bool needAddAttribute)
		{
			AddRefCusCodeList(refCusCodeLists, columns, needAddAttribute, true);
		}

		protected virtual void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns, bool needAddAttribute, bool needAddLanguage)
		{
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

			if (needAddAttribute && RefCusCodeListAttributeParser != null)
			{
				var attributeConfigs = RefCusCodeListAttributeParser.Configs.Where(c => c.NeedAttributeInThisRow(columns)).ToArray();
				if (attributeConfigs.Length > 0)
				{
					refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[attributeConfigs.Length];
					attributesAreValid = RefCusCodeListAttributeParser.TryAddRefCusCodeListAttribute(refCusCodeList, columns);
				}
			}

			if (needAddLanguage && RefCusCodeListLanguageParser != null && RefCusCodeListLanguageParser.Configs.Length > 0)
			{
				refCusCodeList.RefCusCodeListLanguages = new RefCusCodeListLanguage[RefCusCodeListLanguageParser.Configs.Length];
				languagesAreValid = RefCusCodeListLanguageParser.TryAddRefCusCodeListLanguage(refCusCodeList, columns);
			}

			AddRefCusCodeOrAttributeTransportModeIfNeeded(refCusCodeList, columns);

			if (ValidateRefCusCodeList(refCusCodeLists, refCusCodeList) && attributesAreValid && languagesAreValid)
			{
				refCusCodeLists.Add(refCusCodeList);
			}
		}

		protected virtual void AddRefCusCodeOrAttributeTransportModeIfNeeded(RefCusCodeList refCusCodeList, string[] columns)
		{
		}

		protected virtual bool CustomisedRowValidate(string[] columns) => true;
	}
}
