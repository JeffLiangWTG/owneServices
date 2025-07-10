using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ForwarderOrBrokerOrCarrierOrServicesCollection))]
	sealed class ForwarderOrBrokerOrCarrierOrServicesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ForwarderOrBrokerOrCarrierOrServicesCollection(Factory, orgDefaults);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = OrgProxies.AddNew();
			organisation.OH_IsForwarder = false;
			organisation.OH_IsBroker = false;
			organisation.OH_IsMiscFreightServices = false;
			organisation.OH_IsShippingProvider = false;
			OrgProxies.ValidateEntityOnSaving(organisation);
			Assert("Error - organisation is not a valid orgproxy type", organisation.OH_IsForwarderInfo.HasErrors());
			Assert("Error - organisation is not a valid orgproxy type", organisation.OH_IsBrokerInfo.HasErrors());
			Assert("Error - organisation is not a valid orgproxy type", organisation.OH_IsMiscFreightServicesInfo.HasErrors());
			Assert("Error - organisation is not a valid orgproxy type", organisation.OH_IsShippingProviderInfo.HasErrors());

			organisation.OH_IsBroker = true;
			OrgProxies.ValidateEntityOnSaving(organisation);
			Assert("No error - organisation is a valid orgproxy type", !organisation.OH_IsForwarderInfo.HasErrors());
		}

		public void TestMessageNotificationWhenAdditionalFilterNotMet()
		{
			var org = OrgProxies.AddNew();
			org.OH_IsActive = true;

			var message = OrgProxies.GetAllNotificationsWhenAdditionalFilterNotMet(org);

			AssertEquals("An Organization selected from here must have an Organization type of Forwarder, Broker, Carrier or Services selected.", message);
		}

		#region Implementation

		ForwarderOrBrokerOrCarrierOrServicesCollection OrgProxies;

		protected override void SetUp()
		{
			base.SetUp();
			OrgProxies = new ForwarderOrBrokerOrCarrierOrServicesCollection(Factory);
		}

		#endregion
	}
}
