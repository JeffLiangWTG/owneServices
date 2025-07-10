using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressCapability))]
	sealed class OrgAddressCapabilityTest : EnterpriseBusinessObjectTestCase
	{
		#region IReadOnly

		public void TestMainAddressReadOnlySecurityMembers()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.MainAddress;
			address.AddAddressType(OrgAddressType.Pickup);

			OrgAddressCapabilityWrapper capabilityWrapper = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			OrgAddressCapability testCapability = capabilityWrapper.Capability;

			bool oldvalue = Env.Security.OrgAddressCapabilitiesModify.IsAllowed;
			try
			{
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = false;
				Assert("not in database - not read only", !testCapability.PZ_AddressTypeInfo.ReadOnly);
				Assert("not in database - not read only", !testCapability.PZ_IsMainAddressInfo.ReadOnly);

				Factory.Save();

				Assert("in database but not allowed - read only", testCapability.PZ_AddressTypeInfo.ReadOnly);
				Assert("in database but not allowed - read only", testCapability.PZ_IsMainAddressInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = oldvalue;
			}
		}

		public void TestMainAddressReadOnlySecurityMembersAllowed()
		{
			bool oldvalue = Env.Security.OrgAddressModify.IsAllowed;
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.MainAddress;
			address.AddAddressType(OrgAddressType.Pickup);

			Env.Security.OrgAddressModify.IsAllowed = true;
			Factory.Save();
			OrgAddressCapabilityWrapper capabilityWrapper = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			OrgAddressCapability testCapability = capabilityWrapper.Capability;
			try
			{
				Assert("in database and allowed - not read only", !testCapability.PZ_AddressTypeInfo.ReadOnly);
				Assert("in database and allowed - not read only", !testCapability.PZ_IsMainAddressInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressModify.IsAllowed = oldvalue;
			}
		}

		#endregion

		public void TestOverrides()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.MainAddress;
			address.AddAddressType(OrgAddressType.Pickup);
			OrgAddressCapabilityWrapper capabilityWrapper = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			OrgAddressCapability testCapability = capabilityWrapper.Capability;
			testCapability.PZ_AddressType = OrgAddressType.Delivery.ToString();
			AssertEquals(testCapability.PZ_AddressType, OrgAddressType.Delivery.ToString());
			testCapability.PZ_IsMainAddress = true;
			AssertEquals(testCapability.PZ_IsMainAddress, true);
		}
	}
}
