using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.GUI.Testing
{
	[TestedType(typeof(CPCAcquitByDateUserControl))]
	sealed class CPCAcquitByDateUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CPCAcquitByDate();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CPCAcquitByDateUserControl)control).QuantityEdit.ReadOnly;
		}
	}
}
