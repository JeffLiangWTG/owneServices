using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusCodeListCustomColumnsInitializer : ZGridCustomColumnsInitializer
	{
		ICustomPropertyContainer PropertyContainer { get; }

		public ZZRefCusCodeListCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, ICustomPropertyContainer propertyContainer)
			: base(grid, collection, groupName, isVisible: true)
		{
			PropertyContainer = propertyContainer;
		}

		public void AddCustomColumns()
		{
			AddCustomColumns(PropertyContainer.CustomProperties);
		}

		protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
		{
			return new ZZRefCusCodeListCustomColumnsPropertyDescriptor(PropertyContainer, property);
		}

		class ZZRefCusCodeListCustomColumnsPropertyDescriptor : ZCustomPropertyDescriptor
		{
			ICustomPropertyContainer PropertyContainer { get; }

			public ZZRefCusCodeListCustomColumnsPropertyDescriptor(ICustomPropertyContainer propertyContainer, ICustomProperty property) : base(property.Identifier, property.Info.Type)
			{
				PropertyContainer = propertyContainer;
			}

			protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
			{
				return PropertyContainer;
			}

			public override bool IsReadOnly => false;
		}
	}
}
