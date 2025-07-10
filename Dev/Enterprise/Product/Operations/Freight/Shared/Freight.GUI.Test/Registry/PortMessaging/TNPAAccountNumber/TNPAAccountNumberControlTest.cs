using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(TNPAAccountNumberControl))]
	sealed class TNPAAccountNumberControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TNPAAccountNumberCollection();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new TNPAAccountNumberControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TNPAAccountNumberControl)control).TNPAAccountNumberGrid.ReadOnly;
		}
	}
}
