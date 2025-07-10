using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public static class TestExtensions
	{
		public static void AddCustomLabel(this ICustomLabelsConfigOrgProvider customLabelsConfigOrgProvider, ZString fieldName, ZString caption)
		{
			customLabelsConfigOrgProvider.ConfigOrg.AddCustomLabel(fieldName, caption);
		}

		public static void AddCustomLabel(this OrgHeader orgHeader, ZString fieldName, ZString caption)
		{
			var labelString = orgHeader.CustomLabels.AddNew();
			labelString.OT_FieldName = fieldName;
			labelString.OT_Caption = caption;
		}
	}
}
