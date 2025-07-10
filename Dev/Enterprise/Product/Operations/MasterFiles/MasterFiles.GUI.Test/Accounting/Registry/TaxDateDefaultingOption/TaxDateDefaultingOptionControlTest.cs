using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TaxDateDefaultingOptionControl))]
	sealed class TaxDateDefaultingOptionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TaxDateDefaultingOptionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TaxDateDefaultingOptionControl)control).TaxDateDefaultingOptionGrid.ReadOnly;
		}
	}
}
