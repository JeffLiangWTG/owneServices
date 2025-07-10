using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgParkContainerType))]
	sealed class OrgParkContainerTypeTest : EnterpriseBusinessObjectTestCase
	{
		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;

			try
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				OrgCarrierAppointedAgentPorts testCarrierPort = org.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
				OrgParkContainerType containerType = testCarrierPort.ContainerTypes.AddNew();

				Env.Security.OrgCarrierModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !containerType.PT_ContainerStorageClassInfo.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", containerType.PT_ContainerStorageClassInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
			}
		}

		#endregion
	}
}
