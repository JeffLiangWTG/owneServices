using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(ContainerTranshipmentIndicatorControl))]
	internal class ContainerTranshipmentIndicatorControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			ContainerTranshipmentIndicatorCollection collection = new ContainerTranshipmentIndicatorCollection();
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}
		#endregion
	}
}
