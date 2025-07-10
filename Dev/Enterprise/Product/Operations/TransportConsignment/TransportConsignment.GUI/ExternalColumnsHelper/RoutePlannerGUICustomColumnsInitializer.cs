using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	#region RoutePlannerGUICustomColumnsInitializer

	public class RoutePlannerGUICustomColumnsInitializer : ZGridCustomColumnsInitializer
	{
		public RoutePlannerGUICustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, ICustomPropertyContainer propertyContainer)
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
			return new RoutePlannerGUICustomPropertyDescriptor(PropertyContainer, property);
		}

		#region RoutePlannerGUICustomPropertyDescriptor

		class RoutePlannerGUICustomPropertyDescriptor : ZCustomPropertyDescriptor
		{
			public RoutePlannerGUICustomPropertyDescriptor(ICustomPropertyContainer propertyContainer, ICustomProperty property)
				: base(property.Identifier, property.Info.Type)
			{
				PropertyContainer = propertyContainer;
			}

			readonly ICustomPropertyContainer PropertyContainer;

			protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
			{
				return PropertyContainer;
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}
		}

		#endregion
	}

	#endregion
}
