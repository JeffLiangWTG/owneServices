using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Testing
{
	public abstract class DocAddressControlsTest : TestCaseWithFactory
	{
		protected void AssertAndGetAddressControl(ZDocAddressWebControl docAddress, string expectedBindTo, string expectedCaption, string expectedSaveCheckboxCaption, NewOrgRelationTypes expectedNewOrgRelationType, bool expectedGovermentRegNoVisibility)
		{
			AssertNotNull(docAddress);

			AssertEquals(expectedCaption + ": BindTo", expectedBindTo, docAddress.BindTo);
			AssertEquals(expectedCaption + ": Caption", expectedCaption, docAddress.Caption);
			AssertEquals(expectedCaption + ": OrgModuleID", WebModuleIDs.OrganisationTracking, docAddress.OrgModuleID);
			AssertEquals(expectedCaption + ": OrgAutoCompleteHelper.NewOrgRelationType", expectedNewOrgRelationType, docAddress.NewOrgRelationType);

			bool allowToSaveNewOrg = !string.IsNullOrEmpty(expectedSaveCheckboxCaption);
			if (allowToSaveNewOrg)
			{
				Assert(expectedCaption + ": SaveCheckboxVisible", docAddress.SaveCheckboxVisible);
				AssertEquals(expectedCaption + ": SaveCheckboxCaption", expectedSaveCheckboxCaption, docAddress.SaveCheckboxCaption);
			}
			else
			{
				Assert(expectedCaption + ": SaveCheckboxVisible", !docAddress.SaveCheckboxVisible);
			}

			AssertEquals(expectedCaption + ": GovermentRegNoVisible", expectedGovermentRegNoVisibility, docAddress.GovermentRegNoVisible);
		}
	}
}
