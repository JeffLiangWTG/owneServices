using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusRateTypeLanguage : AutoRefCusRateTypeLanguage
	{
		public RefCusRateTypeLanguage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ZXT_ZX6_NKLanguage

		[RelatedBusinessObject("LanguageType")]
		public override ZString ZXT_ZX6_NKLanguage
		{
			get => base.ZXT_ZX6_NKLanguage;
			set => base.ZXT_ZX6_NKLanguage = value;
		}

		public RefLanguageType LanguageType => Factory.LoadFromNaturalKey<RefLanguageType>(RefLanguageTypeSchema.ZX6_Language, ZXT_ZX6_NKLanguage);

		#endregion

		#region ZXT_ZZR_RateType

		[RelatedBusinessObject("CusRateType")]
		public override ZGuid ZXT_ZZR_RateType
		{
			get => base.ZXT_ZZR_RateType;
			set => base.ZXT_ZZR_RateType = value;
		}

		public RefCusRateType CusRateType => Factory.Load<RefCusRateType>(ZXT_ZZR_RateType);

		#endregion
	}
}
