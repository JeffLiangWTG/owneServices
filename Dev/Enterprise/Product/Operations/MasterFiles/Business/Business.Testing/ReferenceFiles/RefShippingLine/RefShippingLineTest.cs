using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefShippingLine))]
	sealed class RefShippingLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetReadOnlySecurity()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			CombineAssertions(() =>
			{
				AssertEquals("CargoWiseOneCode Not ReadOnly:", false, shippingLine.RSL_CargoWiseOneCodeInfo.ReadOnly);
				AssertEquals("CarrierName Not ReadOnly:", false, shippingLine.RSL_CarrierNameInfo.ReadOnly);
				AssertEquals("IsNVO Not ReadOnly:", false, shippingLine.RSL_IsNVOInfo.ReadOnly);
				AssertEquals("StandardCarrierAlphaCode Not ReadOnly:", false, shippingLine.RSL_StandardCarrierAlphaCodeInfo.ReadOnly);
				AssertEquals("IsSystem ReadOnly:", true, shippingLine.RSL_IsSystemInfo.ReadOnly);
				AssertEquals("IsActive Not ReadOnly:", false, shippingLine.RSL_IsActiveInfo.ReadOnly);

				AssertEquals("CargoSphereRatesAvailable ReadOnly:", true, shippingLine.RSL_CargoSphereRatesAvailableInfo.ReadOnly);
				AssertEquals("ContainerAutomationAvailable ReadOnly:", true, shippingLine.RSL_ContainerAutomationAvailableInfo.ReadOnly);
				AssertEquals("GlobalSailingScheduleAvailable ReadOnly:", true, shippingLine.RSL_GlobalSailingScheduleAvailableInfo.ReadOnly);
				AssertEquals("InvoiceAvailable ReadOnly:", true, shippingLine.RSL_InvoiceAvailableInfo.ReadOnly);
				AssertEquals("OceanCarrierMessagingAvailable ReadOnly:", true, shippingLine.RSL_OceanCarrierMessagingAvailableInfo.ReadOnly);

				AssertEquals("BookingRequestAvailable ReadOnly:", true, shippingLine.RSL_BookingRequestAvailableInfo.ReadOnly);
				AssertEquals("ShippingInstructionAvailable ReadOnly:", true, shippingLine.RSL_ShippingInstructionAvailableInfo.ReadOnly);
				AssertEquals("VerifiedGrossContainerWeightAvailable ReadOnly:", true, shippingLine.RSL_VerifiedGrossContainerWeightAvailableInfo.ReadOnly);
				AssertEquals("ShippingOrderAvailable ReadOnly:", true, shippingLine.RSL_ShippingOrderAvailableInfo.ReadOnly);
				AssertEquals("EManifestAvailable ReadOnly:", true, shippingLine.RSL_EManifestAvailableInfo.ReadOnly);
			});

			shippingLine.RSL_IsSystem = true;

			CombineAssertions(() =>
			{
				AssertEquals("CargoWiseOneCode ReadOnly:", true, shippingLine.RSL_CargoWiseOneCodeInfo.ReadOnly);
				AssertEquals("CarrierName ReadOnly:", true, shippingLine.RSL_CarrierNameInfo.ReadOnly);
				AssertEquals("IsNVO ReadOnly:", true, shippingLine.RSL_IsNVOInfo.ReadOnly);
				AssertEquals("StandardCarrierAlphaCode ReadOnly:", true, shippingLine.RSL_StandardCarrierAlphaCodeInfo.ReadOnly);
				AssertEquals("IsSystem ReadOnly:", true, shippingLine.RSL_IsSystemInfo.ReadOnly);
				AssertEquals("IsActive ReadOnly:", true, shippingLine.RSL_IsActiveInfo.ReadOnly);

				AssertEquals("CargoSphereRatesAvailable ReadOnly:", true, shippingLine.RSL_CargoSphereRatesAvailableInfo.ReadOnly);
				AssertEquals("ContainerAutomationAvailable ReadOnly:", true, shippingLine.RSL_ContainerAutomationAvailableInfo.ReadOnly);
				AssertEquals("GlobalSailingScheduleAvailable ReadOnly:", true, shippingLine.RSL_GlobalSailingScheduleAvailableInfo.ReadOnly);
				AssertEquals("InvoiceAvailable ReadOnly:", true, shippingLine.RSL_InvoiceAvailableInfo.ReadOnly);
				AssertEquals("OceanCarrierMessagingAvailable ReadOnly:", true, shippingLine.RSL_OceanCarrierMessagingAvailableInfo.ReadOnly);

				AssertEquals("BookingRequestAvailable ReadOnly:", true, shippingLine.RSL_BookingRequestAvailableInfo.ReadOnly);
				AssertEquals("ShippingInstructionAvailable ReadOnly:", true, shippingLine.RSL_ShippingInstructionAvailableInfo.ReadOnly);
				AssertEquals("VerifiedGrossContainerWeightAvailable ReadOnly:", true, shippingLine.RSL_VerifiedGrossContainerWeightAvailableInfo.ReadOnly);
				AssertEquals("ShippingOrderAvailable ReadOnly:", true, shippingLine.RSL_ShippingOrderAvailableInfo.ReadOnly);
				AssertEquals("EManifestAvailable ReadOnly:", true, shippingLine.RSL_EManifestAvailableInfo.ReadOnly);
			});
		}

		public void TestGetShippingLineIntegration()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			AssertEquals("No Integrations Specified", shippingLine.ShippingLineIntegration);

			var shippinglineWithAllItems = Factory.New<RefShippingLine>();
			shippinglineWithAllItems.RSL_OceanCarrierMessagingAvailable = true;
			shippinglineWithAllItems.RSL_BookingRequestAvailable = true;
			shippinglineWithAllItems.RSL_ShippingInstructionAvailable = true;
			shippinglineWithAllItems.RSL_VerifiedGrossContainerWeightAvailable = true;
			shippinglineWithAllItems.RSL_ShippingOrderAvailable = true;
			shippinglineWithAllItems.RSL_EManifestAvailable = true;
			shippinglineWithAllItems.RSL_GlobalSailingScheduleAvailable = true;
			shippinglineWithAllItems.RSL_ContainerAutomationAvailable = true;
			shippinglineWithAllItems.RSL_InvoiceAvailable = true;
			AssertEquals(@"Ocean Carrier Messaging
    Booking Request
    Shipping Instruction
    Verified Gross Container Weight
    Shipping Order (China)
    eManifest (China)
Global Sailing Schedule
Container Automation
Invoice
", shippinglineWithAllItems.ShippingLineIntegration);
		}

		public void TestGetAvailableEBLProviders()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			AssertNullOrEmpty(shippingLine.AvailableEBLProviders);

			var eblProvider1 = shippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider1.RSE_IsAvailable = true;
			eblProvider1.RSE_Name = "TS1";

			var eblProvider2 = shippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider2.RSE_IsAvailable = false;
			eblProvider2.RSE_Name = "TS2";

			AssertEquals(@"TS1", shippingLine.AvailableEBLProviders);

			eblProvider2.RSE_IsAvailable = true;

			AssertEquals(@"TS1, TS2", shippingLine.AvailableEBLProviders);

			var eblProvider3 = shippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider3.RSE_IsAvailable = true;
			eblProvider3.RSE_Name = "TS3";

			AssertEquals(@"TS1, TS2, TS3", shippingLine.AvailableEBLProviders);
		}

		public void TestDefaultValues()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			CombineAssertions(() =>
			{
				AssertEquals("CargoSphereRates Available:", false, shippingLine.RSL_CargoSphereRatesAvailable);
				AssertEquals("ContainerAutomatation Available:", false, shippingLine.RSL_ContainerAutomationAvailable);
				AssertEquals("GlobalSailingSchedule Available:", false, shippingLine.RSL_GlobalSailingScheduleAvailable);
				AssertEquals("Invoice Available:", false, shippingLine.RSL_InvoiceAvailable);
				AssertEquals("OceanCarrierMessaging Available:", false, shippingLine.RSL_OceanCarrierMessagingAvailable);

				AssertEquals("BookingRequest Available:", false, shippingLine.RSL_BookingRequestAvailable);
				AssertEquals("ShippingInstruction Available:", false, shippingLine.RSL_ShippingInstructionAvailable);
				AssertEquals("VerifiedGrossContainerWeight Available:", false, shippingLine.RSL_VerifiedGrossContainerWeightAvailable);
				AssertEquals("ShippingOrder Available:", false, shippingLine.RSL_ShippingOrderAvailable);
				AssertEquals("EManifest Available:", false, shippingLine.RSL_EManifestAvailable);
			});
		}
	}
}
