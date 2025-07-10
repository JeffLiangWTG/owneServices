using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EUTaxIDDefaultingControl))]
	sealed class EUTaxIDDefaultingControl_Test : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EUTaxIDDefaultingRuleCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EUTaxIDDefaultingControl)control).CostGrid.ReadOnly && ((EUTaxIDDefaultingControl)control).SellGrid.ReadOnly;
		}
	}
}
