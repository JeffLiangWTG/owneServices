using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DeniedPartyScreeningHelper
	{
		public static string GetUnmatchedOrganizationMessage => Res.GetString("01B3D1D3-0CBA-4842-BE87-34C5621E851C", "Unmatched Organizations are not included in Denied Party Screening and will not be screened. Please create a new Organization or replace with an existing Organization. Unmatched Organizations will remain Not Screened");

		public static bool IsSystemDefinedUnmatchedOrganizationWithShowMessage(OrgHeader header)
		{
			var result = header.OH_Code == OrgHeader.UnmatchedOrganisationCode;
			if (result)
			{
				Globals.Message.ShowWarning(GetUnmatchedOrganizationMessage);
			}
			return result;
		}

		public static bool IsSystemDefinedUnmatchedOrganizationWithShowMessage(IDpsEntityProvider entityProvider)
		{
			var result = (entityProvider is OrgHeader header && header.OH_Code == OrgHeader.UnmatchedOrganisationCode);
			if (result)
			{
				Globals.Message.ShowWarning(Res.GetString("67ACB276-614E-4A8F-AF8A-F00A55C61DA1", "The selected organization is a system defined organization and its screening status cannot be modified."));
			}
			return result;
		}

		public static bool IsInactiveEntityWithShowMessage(IDpsEntityProvider entityProvider, bool isActive)
		{
			if (!isActive)
			{
				Globals.Message.ShowWarning(Res.GetString("072A9945-4F78-4BE0-9A2E-B2338D04FFFF", "The selected {0} is inactive and its screening status cannot be modified.", (entityProvider as BusinessObject).HumanReadableName));
			}
			return !isActive;
		}
	}
}
