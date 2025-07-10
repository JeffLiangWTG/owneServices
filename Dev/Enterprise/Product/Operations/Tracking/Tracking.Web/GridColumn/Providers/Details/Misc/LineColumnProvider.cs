using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public abstract class LineColumnProvider : GridColumnProvider
	{
		public LineColumnProvider()
		{
		}

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
					AddToDictionaryAsDefault(column);
					columnKey++;
				}
			}
		}

		protected TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}
	}
}
