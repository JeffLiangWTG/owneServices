using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class ZGridCustomColumnsInitializerWithPropertyContainer : ZGridCustomColumnsInitializer
	{
		public ZGridCustomColumnsInitializerWithPropertyContainer(ZGrid grid, IBusinessObjectCollection collection, ICustomPropertyContainer propertyContainer, ResourceStringData groupName, bool isVisible = false)
			: base(grid, collection, groupName, isVisible, isReadonly: true, isCustomColumn: true)
		{
			PropertyContainer = Argument.NotNull(propertyContainer, nameof(propertyContainer));
		}

		ICustomPropertyContainer PropertyContainer { get; }

		protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property)
		{
			return new PropertyWithContainer(PropertyContainer, property.Identifier, property.Info.Type);
		}

		class PropertyWithContainer : ZCustomPropertyDescriptor
		{
			public PropertyWithContainer(ICustomPropertyContainer propertyContainer, string identifier, Type propertyType)
				: base(identifier, propertyType)
			{
				PropertyContainer = propertyContainer;
			}

			ICustomPropertyContainer PropertyContainer { get; }

			protected override ICustomPropertyContainer GetCustomPropertyContainer(object component) => PropertyContainer;
		}
	}
}
