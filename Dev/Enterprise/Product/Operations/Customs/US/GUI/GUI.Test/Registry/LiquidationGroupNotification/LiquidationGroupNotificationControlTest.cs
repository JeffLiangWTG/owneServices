using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(LiquidationGroupNotificationControl))]
	sealed class LiquidationGroupNotificationControlTest : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new LiquidationGroupNotification();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((LiquidationGroupNotificationControl)control).SuppressNoChangeLiquidationCheckBox.ReadOnly;
		}
	}
}
