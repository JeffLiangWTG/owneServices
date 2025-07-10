using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business;

public class GlbReleaseNoteCombinedValidation : AutoGlbReleaseNoteCombinedValidation
{
	public GlbReleaseNoteCombinedValidation(AutoGlbReleaseNoteCombined parent) : base(parent)
	{
	}

	#region GF_Category

	protected override void CheckGF_Category()
	{
		base.CheckGF_Category();

		if (Parent.GF_Section == NewsSectionTypeList.Codes.ProductUpdates)
		{
			MandatoryValidation.CheckEntered(Parent.GF_CategoryInfo);
		}

		ListValidation.ErrorIfInvalidCode(Parent.GF_CategoryInfo);
	}

	#endregion

	#region GF_Summary

	protected override void CheckGF_Summary()
	{
		base.CheckGF_Summary();
		MandatoryValidation.CheckEntered(Parent.GF_SummaryInfo, Res.GetString("38E5F69E-98ED-4D60-A585-43453C17829A", "Summary"));
		TranslatableDataFieldAttribute.Validate(Parent.GF_SummaryInfo);
	}

	#endregion

	#region GF_URL

	protected override void CheckGF_URL()
	{
		base.CheckGF_URL();
		MandatoryValidation.CheckEntered(Parent.GF_URLInfo, Res.GetString("ABF61CB1-E731-42A3-B387-E054A3261F5F", "Link"));
	}

	#endregion

	#region GF_RN_NKCountryForReleaseNote

	protected override void CheckGF_RN_NKCountryForReleaseNote()
	{
		base.CheckGF_RN_NKCountryForReleaseNote();
		ListValidation.ErrorIfInvalidCode(Parent.GF_RN_NKCountryForReleaseNoteInfo);
	}

	#endregion

	#region GF_Section

	protected override void CheckGF_Section()
	{
		base.CheckGF_Section();
		MandatoryValidation.CheckEntered(Parent.GF_SectionInfo);
		ListValidation.ErrorIfInvalidCode(Parent.GF_SectionInfo);
	}

	#endregion
}
