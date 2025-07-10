using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	internal class WorkflowCustomPropertyDescriptor : ZCustomPropertyDescriptor, IZPropertyInfoRetriever
	{
		public WorkflowCustomPropertyDescriptor(ICustomProperty property, bool initializeCustomBizo = false)
			: base(property.Identifier, property.Info.Type)
		{
			this.initializeCustomBizo = initializeCustomBizo;
		}

		readonly bool initializeCustomBizo;

		protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
		{
			return (component as BusinessObject)?.GetCustomBusinessObject(initializeCustomBizo) ?? base.GetCustomPropertyContainer(component);
		}

		public ZPropertyInfo GetZPropertyInfo(BusinessObject businessObject)
		{
			var customBusinessObject = businessObject.GetCustomBusinessObject(initializeCustomBizo);
			return customBusinessObject != null ? customBusinessObject.ZPropertyInfoHash.GetPropertySafe(this.Name) : null;
		}
	}
}
