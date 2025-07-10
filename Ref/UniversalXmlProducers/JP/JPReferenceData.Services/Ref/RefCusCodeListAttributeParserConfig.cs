using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class RefCusCodeListAttributeParserConfig
	{
		public abstract string ZZE_ZXE_NKName { get; }
		public abstract string GetZZE_Value(string[] columns);
		public virtual Regex ZZE_ValueRegex => null;
		public virtual bool NeedAttributeInThisRow(string[] columns) => true;

		public virtual bool ValidateZZE_Value(RefCusCodeListAttribute refCusCodeListAttribute)
		{
			if (IsValueRequired && string.IsNullOrWhiteSpace(refCusCodeListAttribute.ZZE_Value))
			{
				return false;
			}

			if (IsValueRequired && ZZE_ValueRegex != null && !ZZE_ValueRegex.IsMatch(refCusCodeListAttribute.ZZE_Value))
			{
				return false;
			}

			return true;
		}

		public virtual bool ValidateUniqueness(RefCusCodeList refCusCodeList, RefCusCodeListAttribute refCusCodeListAttribute)
		{
			if (refCusCodeListAttribute != null && refCusCodeList.RefCusCodeListAttributes != null && refCusCodeList.RefCusCodeListAttributes.Length > 0
				&& refCusCodeList.RefCusCodeListAttributes.Where(x => x?.ZZE_ZXE_NKName == refCusCodeListAttribute?.ZZE_ZXE_NKName && x?.ZZE_Value == refCusCodeListAttribute?.ZZE_Value).Any())
			{
				return false;
			}

			return true;
		}

		public virtual Func<RefCusCodeList, RefCusCodeListAttribute, bool> Validate => (refCusCodeList, refCusCodeListAttribute)
			=> ValidateZZE_Value(refCusCodeListAttribute) && ValidateUniqueness(refCusCodeList, refCusCodeListAttribute);

		protected virtual bool IsValueRequired => true;
	}
}
