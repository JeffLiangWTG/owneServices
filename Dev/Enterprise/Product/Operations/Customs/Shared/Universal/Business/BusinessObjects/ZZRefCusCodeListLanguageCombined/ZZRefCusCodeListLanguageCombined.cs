using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListLanguageCombined : AutoZZRefCusCodeListLanguageCombined, ICanDelete
	{
		public ZZRefCusCodeListLanguageCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusCodeListLanguageCombined|ZXA_ZX6_NKLanguage", Caption = "Language Code", ShortCaption = "Lang. Code")]
		[List(nameof(Lookups) + "." + nameof(ZZRefCusCodeListLanguageCombinedLookups.LanguageTypeList))]
		public override ZString ZXA_ZX6_NKLanguage { get => base.ZXA_ZX6_NKLanguage; set => base.ZXA_ZX6_NKLanguage = value; }

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusCodeListLanguageCombined|ZXA_Description", Caption = "Description", ShortCaption = "Desc.")]
		public override ZString ZXA_Description { get => base.ZXA_Description; set => base.ZXA_Description = value; }

		public override bool ReadOnly
		{
			get
			{
				var codeList = CodeList;
				return (codeList != null && codeList.ReadOnly) || base.ReadOnly;
			}
			set { base.ReadOnly = value; }
		}

		protected override bool SupportsCloneCore() => true;

		ZZRefCusCodeListCombined CodeList => Factory.Load<ZZRefCusCodeListCombined>(ZXA_ZZD_CodeList);

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get
			{
				var codeList = CodeList;
				return codeList == null || ((ICanDelete)codeList).CanDelete;
			}
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				var codeList = CodeList;
				return codeList == null ? (NoResString)string.Empty : ((ICanDelete)codeList).ReasonForNotAbleToDelete;
			}
		}

		#endregion
	}
}
