
namespace Enterprise.Customs.Universal
{
	public class CusRefPreferenceViewValidation : AutoCusRefPreferenceViewValidation
	{
		public CusRefPreferenceViewValidation(AutoCusRefPreferenceView parent) : base(parent) { }

		protected override void CheckZZS_ZZZ_NKDataGrouping()
		{
			base.CheckZZS_ZZZ_NKDataGrouping();
			if (!Parent.ZZS_IsSystem && Parent.ZZS_ZZZ_NKDataGrouping.Length > 2)
			{
				Parent.ZZS_ZZZ_NKDataGroupingInfo.AddError(Res.GetString("A76BEF40-D8A5-4A5B-95AA-64E32BD0D47F", "Country/Region Code for non-system defined Preference cannot be longer than 2 characters."));
			}
		}
	}
}
