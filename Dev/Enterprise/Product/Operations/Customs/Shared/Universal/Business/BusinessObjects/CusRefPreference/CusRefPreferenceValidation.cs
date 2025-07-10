using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefPreferenceValidation : AutoCusRefPreferenceValidation
	{
		public CusRefPreferenceValidation(AutoCusRefPreference parent) : base(parent) { }

		new CusRefPreference Parent => (CusRefPreference)base.Parent;

		protected override void CheckCR8_RN_NKCountryCode()
		{
			base.CheckCR8_RN_NKCountryCode();
			var info = Parent.CR8_RN_NKCountryCodeInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}

		protected override void CheckCR8_Description()
		{
			base.CheckCR8_Description();
			MandatoryValidation.CheckEntered(Parent.CR8_DescriptionInfo);
		}

		protected override void CheckCR8_Preference()
		{
			base.CheckCR8_Preference();
			var info = Parent.CR8_PreferenceInfo;
			MandatoryValidation.CheckEntered(info);
			CheckPreferenceAndCountryNotDuplicated(info);
		}

		void CheckPreferenceAndCountryNotDuplicated(ZPropertyInfo info)
		{
			var parentBo = Parent;
			if (!parentBo.CR8_RN_NKCountryCode.IsEmpty && !parentBo.CR8_Preference.IsEmpty)
			{
				var query = new ZQuery();
				query.AddToFilter(CusRefPreferenceSchema.CR8_RN_NKCountryCode, parentBo.CR8_RN_NKCountryCode);
				query.AddToFilter(CusRefPreferenceSchema.CR8_Preference, parentBo.CR8_Preference);
				query.AddToFilter(CusRefPreferenceSchema.PK, SQLComparisonOperator.NotEqual, parentBo.PK);
				if (parentBo.Factory.Exists(typeof(CusRefPreference), query))
				{
					info.AddError(PreferenceAndCountryDuplicatingMessage);
				}
			}
		}

		string PreferenceAndCountryDuplicatingMessage => Res.GetString("57CA842D-D0F1-4F36-B9AF-F34E285E1870",
			@"There is already a Preference with the same Country/Region and Code.
The combination of Country and Preference must be unique.");
	}
}
