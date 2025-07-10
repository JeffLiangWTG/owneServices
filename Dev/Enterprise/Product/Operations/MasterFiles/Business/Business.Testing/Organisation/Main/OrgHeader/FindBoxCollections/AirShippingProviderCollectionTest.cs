using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AirShippingProviderCollection))]
	sealed class AirShippingProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AirShippingProviderCollection(Factory);
		}

		public void TestFilter()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader airLineNotLinked = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader inactiveAirLineLinked = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader activeAirLineLinked = Factory.NewWithValidTestData<OrgHeader>();

			RefAirline activeAirline = Factory.NewWithValidTestData<RefAirline>();
			activeAirline.RM_IsActive = true;
			RefAirline inactiveAirline = Factory.NewWithValidTestData<RefAirline>();
			inactiveAirline.RM_IsActive = false;

			carrier.OH_IsShippingProvider = true;

			airLineNotLinked.OH_IsShippingProvider = true;
			airLineNotLinked.OH_IsAirLine = true;
			inactiveAirLineLinked.OH_IsShippingProvider = true;
			inactiveAirLineLinked.OH_IsAirLine = true;
			inactiveAirLineLinked.MiscServ.OM_RM_Airline = inactiveAirline.PK;
			activeAirLineLinked.OH_IsShippingProvider = true;
			activeAirLineLinked.OH_IsAirLine = true;
			activeAirLineLinked.MiscServ.OM_RM_Airline = activeAirline.PK;

			Factory.Save();
			AirLines.Load();

			Assert("Header should not be in list", !AirLines.Contains(header));
			Assert("Carrier should not be in list", !AirLines.Contains(carrier));
			Assert("Inactive Airline linked should not be in list", !AirLines.Contains(inactiveAirLineLinked));
			Assert("Airline not linked should be in list", AirLines.Contains(airLineNotLinked));
			Assert("Active Airline linked should be in list", AirLines.Contains(airLineNotLinked));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = AirLines.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("AirLine is selected", organisation.OH_IsAirLine);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = AirLines.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsAirLine = false;
			AirLines.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider has error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("AirLine has error", organisation.OH_IsAirLineInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			AirLines.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("AirLine has error", organisation.OH_IsAirLineInfo.HasErrors());

			organisation.OH_IsAirLine = true;
			AirLines.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("AirLine does not have error", !organisation.OH_IsAirLineInfo.HasErrors());
		}

		public void TestOrgLinksToInactiveAirline_HasAdditionalFilterError()
		{
			RefAirline inactiveAirline = Factory.NewWithValidTestData<RefAirline>();
			inactiveAirline.RM_IsActive = false;

			var organisation = AirLines.AddNew();
			organisation.OH_IsShippingProvider = true;
			organisation.OH_IsAirLine = true;
			organisation.MiscServ.OM_RM_Airline = inactiveAirline.PK;

			string errorMsg = AirLines.GetAllNotificationsWhenAdditionalFilterNotMet(organisation);
			AssertEquals("This Organization is linked to an airline that is marked as inactive in the airline reference file.", errorMsg);
		}

		#region Implementation

		AirShippingProviderCollection AirLines;

		protected override void SetUp()
		{
			base.SetUp();
			AirLines = new AirShippingProviderCollection(Factory);
		}

		#endregion
	}
}
