using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(InBondNumberRangeControl))]
	sealed class InBondNumberRangeControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new InBondNumberRange();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			InBondNumberRange numberRange = control.CurrentDataItem as InBondNumberRange;
			if (numberRange != null)
			{
				result = numberRange.ReadOnly;
			}
			return result;
		}
	}
}
