using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutoBillingGroupNotificationUserControl))]
	sealed class AutoBillingGroupNotificationUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new AutoBillingGroupNotification();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((ZArchitecture.GUI.ZGuidFindBox)((AutoBillingGroupNotificationUserControl)control).Controls.Find("SendGroupGuidFindBox", true)[0]).ReadOnly;
	}
}
