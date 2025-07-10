using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusPreferenceLanguage : AutoRefCusPreferenceLanguage
	{
		public RefCusPreferenceLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusPreference")]
		public override ZGuid ZX9_ZZS_Preference
		{
			get => base.ZX9_ZZS_Preference;
			set => base.ZX9_ZZS_Preference = value;
		}

		public CusRefPreferenceView CusPreference => Factory.Load<CusRefPreferenceView>(ZX9_ZZS_Preference);

		[BusinessObjectTestExclude]
		public override ZString ZX9_ZX6_NKLanguage
		{
			get => base.ZX9_ZX6_NKLanguage;
			set => base.ZX9_ZX6_NKLanguage = value;
		}
	}
}
