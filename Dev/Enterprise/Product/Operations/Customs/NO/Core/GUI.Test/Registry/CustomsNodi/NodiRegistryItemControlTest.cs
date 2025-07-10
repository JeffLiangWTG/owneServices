using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.GUI.Testing
{
	[TestedType(typeof(NodiRegistryItemControl))]
	sealed class NodiRegistryItemControlTest : RegistryZUserControlTestCase
	{
		public void TestSystemName_Width()
		{
			using (var control = new NodiRegistryItemControl())
			{
				AssertEquals(80, control.NodiDetailsGrid.GetColumnStyle(NodiRegistry.Schema.SystemName).Width);
			}
		}

		public void TestNodiNumber_Width()
		{
			using (var control = new NodiRegistryItemControl())
			{
				AssertEquals(80, control.NodiDetailsGrid.GetColumnStyle(NodiRegistry.Schema.NodiNumber).Width);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new NodiRegistryCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
	}
}
