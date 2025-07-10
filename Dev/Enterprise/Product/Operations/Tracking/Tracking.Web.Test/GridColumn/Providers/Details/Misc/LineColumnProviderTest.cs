using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class LineColumnProviderTest : GridColumnProviderTest
	{
		protected virtual void AddLineAttributeColumns(AttributeManager.AttributeModules module, OrgHeader relatedOrg, ZString bindToPrefix)
		{
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				int columnKey = 12000;
				var attributes = new AttributeManager().GetLineAttributes(module, relatedOrg, SiteUser.LoggedInOrganisation);
				foreach (var attribute in attributes)
				{
					ZTemplateColumn column = ZTemplateColumn.GetNew(attribute.Caption, attribute.Column, bindToPrefix);
					column.ColumnKey = columnKey;
					AddDefaultsColumn(column);
					columnKey++;
				}
			}
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}
	}
}
