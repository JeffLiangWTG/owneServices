using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class PackageViewGridCustomColumnsInitializer : ZGridCustomColumnsInitializer
	{
		public PackageViewGridCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, ICustomPropertyContainer propertyContainer)
			: base(grid, collection, groupName, isVisible: true)
		{
			PropertyContainer = propertyContainer;
		}

		readonly ICustomPropertyContainer PropertyContainer;

		public void AddCustomColumns()
		{
			AddCustomColumns(PropertyContainer.CustomProperties);
		}

		protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
		{
			return new PkgPackageGUICustomPropertyDescriptor(PropertyContainer, property);
		}
	}

	class PkgPackageGUICustomPropertyDescriptor : ZCustomPropertyDescriptor
	{
		public PkgPackageGUICustomPropertyDescriptor(ICustomPropertyContainer propertyContainer, ICustomProperty property)
				: base(property.Identifier, property.Info.Type)
		{
			PropertyContainer = propertyContainer;
		}

		readonly ICustomPropertyContainer PropertyContainer;

		protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
		{
			return PropertyContainer;
		}

		public override bool IsReadOnly => true;
	}
}
