using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class StaffManagerRegistryFieldsGridReadOnlyInitializer
	{
		public static void AddStaffCustomFieldsColumns(ZGrid grid, IBusinessObjectCollection collection, bool isVisible = false)
		{
			if (!(Env.Security.StaffViewOtherReportingManagerRoles.IsAllowed || Env.Security.StaffViewOtherDirectReports.IsAllowed))
			{
				return;
			}

			var fieldsContainer = new CustomPropertyContainer(new CustomPropertyComparer());

			foreach (var reportingRole in SystemDataRegistry.Instance.StaffReportingRoles.Value.Cast<StaffReportingRole>())
			{
				fieldsContainer.AddCustomProperty(reportingRole.Code, reportingRole.Description, typeof(ZString), null, null);
			}

			new StaffGridCustomColumnsInitializer(grid, collection, null, isVisible).AddCustomColumns(fieldsContainer.CustomProperties);
		}

		#region ZGridCustomisedColumnsInitializer

		class StaffGridCustomColumnsInitializer : ZGridCustomColumnsInitializer
		{
			public StaffGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, bool isVisible)
				: base(grid, collection, groupName, isVisible, true)
			{ }

			protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
			{
				return new StaffManagerCustomPropertyDescriptor(property);
			}
		}

		#endregion
	}
}
