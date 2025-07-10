using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(AllocationMethodDefaultRegistryItemControl))]
	sealed class AllocationMethodDefaultRegistryItemControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new AllocationMethodDefaultHeader();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((BusinessObject)businessEntity).ReadOnly;
		}

		#endregion
	}
}
