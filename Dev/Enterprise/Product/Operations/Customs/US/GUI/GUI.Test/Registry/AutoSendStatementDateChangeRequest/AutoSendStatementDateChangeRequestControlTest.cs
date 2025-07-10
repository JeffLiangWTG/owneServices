using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutoSendStatementDateChangeRequestControl))]
	sealed class AutoSendStatementDateChangeRequestControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutoSendStatementDateChangeRequest();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			AutoSendStatementDateChangeRequest autoGenerateChangeRequestData = control.CurrentDataItem as AutoSendStatementDateChangeRequest;
			if (autoGenerateChangeRequestData != null)
			{
				result = autoGenerateChangeRequestData.ReadOnly;
			}
			return result;
		}
	}
}
