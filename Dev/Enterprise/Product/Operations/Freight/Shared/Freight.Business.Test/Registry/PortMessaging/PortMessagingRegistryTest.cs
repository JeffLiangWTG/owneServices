using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PortMessagingRegistry))]
	sealed class PortMessagingRegistryTest : RegistryItemSetTestCaseWithFactory<PortMessagingRegistry>
	{
		public void TestCommunitySystemCodesOfForwarderAndAgent()
		{
			var testRegistryValues = new CommunitySystemCodesOfForwarderAndAgentCollection();
			AssertEquals("Registry type:", testRegistryValues.GetType(), ItemSet.CommunitySystemCodesOfForwarderAndAgent.DefaultValue.GetType());
		}

		public void TestTNPAAccountNumber()
		{
			var testRegistryValues = new TNPAAccountNumberCollection();
			AssertEquals("Registry type:", testRegistryValues.GetType(), ItemSet.TNPAAccountNumber.DefaultValue.GetType());
		}

		public void TestEORIAndLRNEffectiveDate()
		{
			TestGenericRegistryItem(ItemSet.EORIAndLRNEffectiveDate,
				"EORIAndLRNEffectiveDate",
				PortMessagingRegistry.Categories.Freight_PortMessaging_Germany_Dakosy,
				"Enable ATLAS AES1 Release 3.0, one-stage AES1 procedure requirements",
				"ATLAS AES Release 3.0, one-stage AES procedure requirements to declare EORI and LRN Local Reference Number will be enabled from the date set below. Override to change the date.",
				RegistryStorageFlags.System,
				new DateTime(2023, 09, 06));
		}

		public void TestAllowToSendExportNotification()
		{
			AssertEquals("Default Value", false, ItemSet.AllowToSendExportNotification.DefaultValue);
			AssertEquals("Category", PortMessagingRegistry.Categories.Freight_PortMessaging_France, ItemSet.AllowToSendExportNotification.Category);
			AssertEquals("Allow to send Export Notification (755) to Cargo Information Network", ItemSet.AllowToSendExportNotification.Caption);
			AssertEquals("If Yes, CargoWise will allow to send the Export Notification (755) message to the Cargo Information Network (CIN) in France. Sending the Export Notification 755 is required to link ECS MRN numbers to your Air Waybill.\r\nYou should enable this Registry when you are NOT using the Customs Brokerage Module of CargoWise.", ItemSet.AllowToSendExportNotification.Hint);
			Assert("CountryFilterPK", ItemSet.AllowToSendExportNotification.CountryFilterPKs.Equals(PortMessagingCountryList.France));
		}

		public void TestAllowToSendExportNotificationToCargonaut()
		{
			AssertEquals("Default Value", false, ItemSet.AllowToSendExportNotificationToCargonaut.DefaultValue);
			AssertEquals("Category", PortMessagingRegistry.Categories.Freight_PortMessaging_Netherlands, ItemSet.AllowToSendExportNotificationToCargonaut.Category);
			AssertEquals("Allow to send Export Notification (755) to Cargonaut", ItemSet.AllowToSendExportNotificationToCargonaut.Caption);
			AssertEquals("If Yes, it will be able to send the Export Notification Cargonaut (NL) message to the Port Community System of Schiphol Airport in the Netherlands.", ItemSet.AllowToSendExportNotificationToCargonaut.Hint);
			Assert("CountryFilterPK", ItemSet.AllowToSendExportNotificationToCargonaut.CountryFilterPKs.Single().Equals(Core.Constants.CountryGuids.Netherlands));
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				var list = new List<string>(base.ConditionallyVisibleRegistryItems);
				list.Add("AllowToSendExportNotification");
				return list;
			}
		}

		public void TestEnablePortOrderFormBuilderForm()
		{
			TestGenericRegistryItem(ItemSet.EnablePortOrderFormBuilderForm,
				"EnablePortOrderFormBuilderForm",
				PortMessagingRegistry.Categories.Freight_PortMessaging_Germany_Dakosy,
				"Enable Port Order FormBuilder Form",
				"Enable this registry to use FormBuilder Form to send Dakosy Port Orders",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
				);
		}
	}
}
