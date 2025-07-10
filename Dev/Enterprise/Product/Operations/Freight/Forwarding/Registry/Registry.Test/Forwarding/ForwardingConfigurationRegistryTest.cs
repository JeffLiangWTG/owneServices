using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(ForwardingConfigurationRegistry))]
	class ForwardingConfigurationRegistryTest : RegistryItemSetTestCaseWithFactory<ForwardingConfigurationRegistry>
	{
		#region DpsStatusUpdateSetting

		public void TestShipmentDpsStatusUpdateSetting()
		{
			CombineAssertions(delegate
			{
				AssertEquals("Name", "ShipmentDpsStatusUpdateSetting", ItemSet.ShipmentDpsStatusUpdateSetting.Name);
				AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening, ItemSet.ShipmentDpsStatusUpdateSetting.Category);
				AssertEquals("Caption", "Shipment DPS Update", ItemSet.ShipmentDpsStatusUpdateSetting.Caption);
				AssertEquals("Hint", "Select the method to determine which Shipments to update when the DPS status of an organization referenced on the Shipment changes.", ItemSet.ShipmentDpsStatusUpdateSetting.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ShipmentDpsStatusUpdateSetting.Storage);
				AssertEquals("DefaultValue.Option", "DAB", ItemSet.ShipmentDpsStatusUpdateSetting.DefaultValue.Option);
				AssertEquals("DefaultValue.JobUpdateSettings.Count", 0, ItemSet.ShipmentDpsStatusUpdateSetting.DefaultValue.JobUpdateSettings.Count);
				AssertEquals("Reference ShipmentPhaseSecurity.Name", "ShipmentPhaseSecurity", (ItemSet.ShipmentDpsStatusUpdateSetting.DefaultValue.PhaseRegistryItem.Name));
			});
		}

		public void TestConsolDpsStatusUpdateSetting()
		{
			CombineAssertions(delegate
			{
				AssertEquals("Name", "ConsolDpsStatusUpdateSetting", ItemSet.ConsolDpsStatusUpdateSetting.Name);
				AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening, ItemSet.ConsolDpsStatusUpdateSetting.Category);
				AssertEquals("Caption", "Consol DPS Update", ItemSet.ConsolDpsStatusUpdateSetting.Caption);
				AssertEquals("Hint", "Select the method to determine which Consolidations to update when the DPS status of an organization referenced on the Consolidation changes.", ItemSet.ConsolDpsStatusUpdateSetting.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ConsolDpsStatusUpdateSetting.Storage);
				AssertEquals("DefaultValue.Option", "DAB", ItemSet.ConsolDpsStatusUpdateSetting.DefaultValue.Option);
				AssertEquals("DefaultValue.JobUpdateSettings.Count", 0, ItemSet.ConsolDpsStatusUpdateSetting.DefaultValue.JobUpdateSettings.Count);
				AssertEquals("Reference ConsolPhaseSecurity.Name", "ConsolPhaseSecurity", (ItemSet.ConsolDpsStatusUpdateSetting.DefaultValue.PhaseRegistryItem.Name));
			});
		}

		#endregion

		public void TestCargoImpSentMessageVersions()
		{
			CombineAssertions(delegate
			{
				AssertEquals("Name", "CargoImpSentMessageVersions", ItemSet.CargoImpSentMessageVersions.Name);
				AssertEquals("Category", "Freight/AWB/CargoIMP/FWB Messaging", ItemSet.CargoImpSentMessageVersions.Category);
				AssertEquals("Caption", "FWB/FHL Message Versions", ItemSet.CargoImpSentMessageVersions.Caption);
				AssertEquals("Hint", "Versions of the FWB and FHL CargoIMP Messages to send when sending an FWB/FHL Message set.", ItemSet.CargoImpSentMessageVersions.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CargoImpSentMessageVersions.Storage);
				AssertEquals("DefaultValue", CargoIMPSentMessageVersionsList.Codes.AutoSwitch, ItemSet.CargoImpSentMessageVersions.DefaultValue);
				AssertEquals("LookUpList.Count", 3, ((CodePairRegistryDataType)ItemSet.CargoImpSentMessageVersions.DataType).LookUpList.Count);
			});
		}

		public void TestSenderPIMAAddress()
		{
			TestRegistryItem(ItemSet.SenderPIMAAddress,
					"SenderPIMAAddress",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_FWBMessaging,
					"Sender Identification (PIMA)",
					"Specify the Participant Identification and Messaging Address (PIMA) for your organization. This will be used in the header for CargoIMP messages under certain conditions.",
					RegistryStorageFlags.Company,
					TextEditorType.TextBox,
					RegistryOptions.IsOnlyForSupport,
					"");
		}

		public void TestCargoIMPServiceProvider()
		{
			AssertEquals("CargoImpServiceProvider", ItemSet.CargoIMPServiceProvider.Name);
			AssertEquals("FWB Service Provider", ItemSet.CargoIMPServiceProvider.Caption);
			AssertEquals("FWB Messaging Service Provider", ItemSet.CargoIMPServiceProvider.Hint);
			AssertEquals(ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_FWBMessaging, ItemSet.CargoIMPServiceProvider.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CargoIMPServiceProvider.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.CargoIMPServiceProvider.Options);
			AssertEquals(Constants.AWB.CargoIMPServiceProviderConstants.HUB, ItemSet.CargoIMPServiceProvider.DefaultValue);
			AssertEquals(3, ((CodePairRegistryDataType)ItemSet.CargoIMPServiceProvider.DataType).LookUpList.Count);
		}

		public void TestAllowUsersToSendFWBsWithErrors()
		{
			TestRegistryItem(ItemSet.AllowUsersToSendFWBsWithErrors, "ALLOW_USERS_TO_SEND_FWBS_WITH_ERRORS", "Freight/AWB/CargoIMP/FWB Messaging", "Allow Users To Send FWBs With Errors", "Users will be allowed to send Electronic AWB messages even though they have errors that will cause failure or rejection by the Airlines. Also, Message Errors will be shown as Warnings on the AWB Tab.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, false);
		}

		public void TestSendFWBOrFHLToAirlineBasedOnMAWBPrefix()
		{
			TestRegistryItem(ItemSet.SendFWBOrFHLToAirlineBasedOnMAWBPrefix, "SendFWBOrFHLToAirlineBasedOnMAWBPrefix", "Freight/AWB/CargoIMP/FWB Messaging", "Send FWB/FHL to Airline based on MAWB Prefix", "This will allow FWB and FHL messages to be sent to the Airline based on the MAWB prefix rather than the Airline of the first flight as the two may not be the same Airline due to interline. This is used to route FWB and FHL messages to Airline who issued the MAWB regardless of which Airline might be carrying the shipment first.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestForceSendingOfFWBCVDSegment()
		{
			TestRegistryItem(ItemSet.ForceSendingOfFWBCVDSegment, "ForceSendingOfFWBCVDSegment", "Freight/AWB/CargoIMP/FWB Messaging", "Force Sending Of FWB CVD Segment", "Force Sending Of FWB CVD Segment", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, false);
		}

		public void TestCargoIMPClientCodeSuffix()
		{
			var item = ItemSet.CargoIMPClientCodeSuffix;

			AssertEquals("Name", "FWBClientCodeSuffix", item.Name);
			AssertEquals("Category", "Freight/AWB/CargoIMP/FWB Messaging", item.Category);
			AssertEquals("Caption", "FWB Client Code Suffix", item.Caption);
			AssertEquals("Hint", "Client Code Suffix to uniquely identify your CargoIMP sender ID (Server ID is used by default). This ID is to be provided by CargoWise, and only if required.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("DefaultValue", Env.Registry.PhysicalServerID, item.DefaultValue);
			AssertEquals("Data Type", typeof(StringRegistryDataType), item.DataType.GetType());
			AssertEquals("Casing", CharacterCase.Upper, (item.DataType as StringRegistryDataType).CharacterCase);
			AssertExceptionThrown("Length Validation", typeof(RegistryValidationException), () => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AAAA"));
			AssertExceptionThrown("Length Validation", typeof(RegistryValidationException), () => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AA"));
			AssertNoExceptionThrown("Setting correct value", () => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AAA"));
			AssertEquals("Value", "AAA", item.Value);
		}

		public void TestSendFWBWithoutAgentDetailsSegment()
		{
			var item = ItemSet.AllowFWBWithoutAGTSegment;

			AssertEquals("Caption", "Allow FWB Without AGT Segment", item.Caption);
			AssertEquals("Storage", RegistryStorageFlags.Branch, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("DefaultValue", false, item.DefaultValue);

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals("Value after setting to 'true'", true, item.Value);

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, false);
			AssertEquals("Value after setting to 'false'", false, item.Value);
		}

		public void TestCargoIMPPhase2MSUEventsMapping()
		{
			AssertEquals("CargoIMPPhase2MSUEventsMapping", ItemSet.CargoIMPPhase2MSUEventsMapping.Name);
			AssertEquals(RawDataRegistry.Categories.Freight + "/AWB/CargoIMP/Phase 2 Messaging", ItemSet.CargoIMPPhase2MSUEventsMapping.Category);
			AssertEquals("MSU Events Mapping", ItemSet.CargoIMPPhase2MSUEventsMapping.Caption);
			AssertEquals($"Specifies the MSU (Milestone Status Update) Event Code to use when a {Core.Constants.ProductName} Event with the corresponding CW1 Event Reference parameter is fired.",
					ItemSet.CargoIMPPhase2MSUEventsMapping.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CargoIMPPhase2MSUEventsMapping.Storage);
			AssertEquals(CargoIMPPhase2MSUEventsMappingCollection.GetDefault().Count, ItemSet.CargoIMPPhase2MSUEventsMapping.DefaultValue.Count);

			var collection = CargoIMPPhase2MSUEventsMappingCollection.GetDefault();
			collection[0].EnterpriseEvent = AutoEvents.CargoReportRejected.Code;

			var mapping = collection.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.DOC;
			mapping.EnterpriseEvent = AutoEvents.CargoReportSent.Code;

			ItemSet.CargoIMPPhase2MSUEventsMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var value = ItemSet.CargoIMPPhase2MSUEventsMapping.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("value.Count", 9, value.Count);
		}

		public void TestCargoIMPPhase2RouteMap()
		{
			AssertEquals("CargoIMPPhase2RouteMap", ItemSet.CargoIMPPhase2RouteMap.Name);
			AssertEquals(ForwardingConfigurationRegistry.Categories.Freight + "/AWB/CargoIMP/Phase 2 Messaging", ItemSet.CargoIMPPhase2RouteMap.Category);
			AssertEquals("Supported Routes", ItemSet.CargoIMPPhase2RouteMap.Caption);
			AssertEquals("Specifies the list of supported origin and destination pairs for each airline two-character code.",
					ItemSet.CargoIMPPhase2RouteMap.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.CargoIMPPhase2RouteMap.Storage);
			AssertEquals(0, ItemSet.CargoIMPPhase2RouteMap.DefaultValue.Count);

			CargoIMPPhase2RouteMapCollection collection = new CargoIMPPhase2RouteMapCollection();
			CargoIMPPhase2RouteMap mapping = collection.AddNew();
			mapping.Origin = "UAIEV";
			mapping.Destination = "AUSYD";
			mapping.AirlineTwoCharacterCode = "FO";
			ItemSet.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			CargoIMPPhase2RouteMapCollection value = ItemSet.CargoIMPPhase2RouteMap.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("value.Count", 1, value.Count);
			AssertEquals("UAIEV", value[0].Origin);
			AssertEquals("AUSYD", value[0].Destination);
			AssertEquals("FO", value[0].AirlineTwoCharacterCode);
		}

		public void TestCargoIMPPhase2()
		{
			TestRegistryItem(ItemSet.CargoIMPPhase2AllowToSendMessagesWithErrors,
					"CargoIMPPhase2AllowToSendMessagesWithErrors",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging,
					"Allow Users To Send Messages With Errors",
					"Users will be allowed to send Electronic CargoIMP Phase 2 messages even though they have errors that will cause failure or rejection.",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false);

			TestRegistryItem(ItemSet.CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings,
					"CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging,
					"Allow Messages With Warnings to be sent from Milestones",
					"Electronic CargoIMP Phase 2 messages will be sent from Milestone Trigger Actions even though they have warnings.",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false);

			TestRegistryItem(ItemSet.CargoIMPPhase2ForwarderId,
					"CargoIMPPhase2ForwarderId",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging,
					"Forwarder Identification",
					"Specify the Forwarder Identification for your organization. This will be used for CargoIMP Phase 2 messages.",
					RegistryStorageFlags.Company,
					TextEditorType.TextBox,
					"");

			TestRegistryItem(ItemSet.CargoIMPPhase2SendMessageErrors,
					"CargoIMPPhase2SendMessageErrors",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging,
					"Send Errors To",
					"Send message errors to staff member, nominated group or combination of both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					new CodeDescriptionPairList(OLookUpEditType.EmailTo),
					Constants.EmailTo.StaffMemberAndNominatedGroup);

			TestRegistryItem(ItemSet.CargoIMPPhase2GroupToSendMessageErrors,
					"CargoIMPPhase2GroupToSendMessageErrors",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging,
					"Group to Send Errors",
					string.Format("The group of users the informing emails can be sent to. Must be set if 'Send Errors To' is set to {0} or {1}.", Constants.EmailTo.NominatedGroup, Constants.EmailTo.StaffMemberAndNominatedGroup),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryFindBoxCollection.GlbGroup,
					Guid.Empty);

			TestRegistryItem(ItemSet.CargoIMPPhase2UseETDForHouseBillDate,
					"CargoIMPPhase2UseETDForHouseBillDate",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging,
					"Use ETD as message's 'House Bill Date'",
					"System Creation Time is used by default to fill in 'House Bill Date' message field.\r\nIf this registry item is set then ETD is used instead.\r\nYou may need to use ETD instead of System Creation Time if there exists another system which sends RMI and/or MSU for the same shipment but uses ETD as 'House Bill Date'.\r\nYou should not enable this registry item otherwise.",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonSenderPIMAAddress,
					"CargoIMPPhase2TraxonSenderPIMAAddress",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"Sender Identification (PIMA)",
					"Specify the Participant Identification and Messaging Address (PIMA) for your organization. This will be used in the Traxon header for CargoIMP Phase 2 messages.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					TextEditorType.TextBox,
					"");

			AssertEquals("CargoIMPPhase2TraxonFTPServer", ItemSet.CargoIMPPhase2TraxonFTPServer.Name);
			AssertEquals(ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon, ItemSet.CargoIMPPhase2TraxonFTPServer.Category);
			AssertEquals("FTP Server Address", ItemSet.CargoIMPPhase2TraxonFTPServer.Caption);
			AssertEquals("The FTP Server address for Traxon messages.", ItemSet.CargoIMPPhase2TraxonFTPServer.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CargoIMPPhase2TraxonFTPServer.Storage);
			AssertEquals(string.Empty, ItemSet.CargoIMPPhase2TraxonFTPServer.DefaultValue);
			AssertType(typeof(FtpUriRegistryDataType), ItemSet.CargoIMPPhase2TraxonFTPServer.DataType);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonFTPUserName,
					"CargoIMPPhase2TraxonFTPUserName",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"FTP User Name",
					"The FTP user name for Traxon messages.",
					RegistryStorageFlags.System,
					TextEditorType.TextBox,
					"");

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonFTPPasswordEncrypted,
					"CargoIMPPhase2TraxonFTPPasswordEncrypted",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"FTP Password",
					"The FTP password for Traxon messages.",
					RegistryStorageFlags.System,
					RegistryOptions.IsPasswordVisibleForControllerUser,
					TextEditorType.Password,
					"");
			AssertType(typeof(SecureStringRegistryDataType), ItemSet.CargoIMPPhase2TraxonFTPPasswordEncrypted.DataType);

			TestGenericRegistryItem(ItemSet.CargoIMPPhase2TraxonLastFTPFailureTime,
					"CargoIMPPhase2TraxonLastFTPFailureTime",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"Time of Last FTP Failure",
					"Time of Last Traxon FTP Failure.",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					DateTime.MinValue);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonFTPFailureCount,
					"CargoIMPPhase2TraxonFTPFailureCount",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"Number of Consecutive FTP Failures",
					"Number of Consecutive Traxon FTP Failures.",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					0);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonFTPMaxFailureCount,
					"CargoIMPPhase2TraxonFTPMaxFailureCount",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"Allowed Number of Consecutive FTP Failures",
					"Number of consecutive Traxon FTP failures before sending error email and suspending of communication.",
					RegistryStorageFlags.System,
					10);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonFTPDelayAfterFailure,
					"CargoIMPPhase2TraxonFTPDelayAfterFailure",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"Time From Last FTP Failure To Next Send Try",
					"Number of minutes that should pass from last Traxon FTP failure to next try to send data.",
					RegistryStorageFlags.System,
					30);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonInterchangeMaxSends,
					"CargoIMPPhase2TraxonInterchangeMaxSends",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"Interchange Maximum Sends",
					"This is the maximum number of times a failed to send interchange will be sent to Traxon before the process controller gives up and flags an error on the interchange.",
					RegistryStorageFlags.System,
					10);

			TestRegistryItem(ItemSet.CargoIMPPhase2TraxonFTPInboundDirectory,
					"CargoIMPPhase2TraxonFTPInboundDirectory",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
					"FTP Server Inbound Directory",
					"The directory name of Traxon FTP Server where messages will be uploaded by the system.",
					RegistryStorageFlags.System,
					TextEditorType.TextBox,
					string.Empty);
		}

		public void TestPrintForwarderBranchDetailsHAWBIssuedBySection()
		{
			TestRegistryItem(ItemSet.PrintForwarderBranchDetailsHAWBIssuedBySection,
					"PrintForwarderBranchDetailsHAWBIssuedBySection",
					ForwardingConfigurationRegistry.Categories.Freight_AWB_HAWB,
					@"Print Forwarder Branch Details in the 'Issued By' section",
					"This determines whether the 'issued by' section in the House AWB document is filled in or not.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					true);

			ItemSet.PrintForwarderBranchDetailsHAWBIssuedBySection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Freight/AWB/HAWB", ItemSet.PrintForwarderBranchDetailsHAWBIssuedBySection.Category);
			AssertEquals(false, ItemSet.PrintForwarderBranchDetailsHAWBIssuedBySection.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region Phase Security

		public void TestConsolPhaseSecurity()
		{
			AssertPhaseSecurityItem(ItemSet.ConsolPhaseSecurity, "ConsolPhaseSecurity", FreightDataRegistry.Categories.Freight_Consolidations_Phases, PhaseConstants.GetConsolLocationsList(), typeof(ConsolPhaseDependantsProvider));
		}

		public void TestShipmentPhaseSecurity()
		{
			AssertPhaseSecurityItem(ItemSet.ShipmentPhaseSecurity, "ShipmentPhaseSecurity", FreightDataRegistry.Categories.Freight_Shipment_Phases, PhaseConstants.GetShipmentLocationsList(), typeof(ShipmentPhaseDependantsProvider));
		}

		void AssertPhaseSecurityItem(PhaseSecurityRegistryItem item, string name, string category, CodeDescriptionPairList locations, Type dependantsProviderType)
		{
			CombineAssertions(delegate
			{
				AssertEquals("Name", name, item.Name);
				AssertEquals("Category", category, item.Category);
				AssertEquals("Caption", "Phases Configuration", item.Caption);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertContainsExactElementsInAnyOrder("Location lookups", locations, item.DefaultValue.RuleLocations);
				AssertEquals("Dependants provider type", dependantsProviderType, item.DefaultValue.DependantsProvider.GetType());
			});
		}

		public void TestConsolDefaultPhaseRegistry()
		{
			TestGenericRegistryItem(ItemSet.ConsolDefaultPhase,
				"ConsolDefaultPhase",
				FreightDataRegistry.Categories.Freight_Consolidations_Phases,
				"Default Phase Override",
				@"Use this Registry setting to override the default Phase on new Consolidation records.

Hint: The default Phase will load when creating a new shipment record automatically and may lock required fields for completion before save.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				PhaseConstants.Phase.ALL);
		}

		public void TestShipmentDefaultPhaseRegistry()
		{
			TestGenericRegistryItem(ItemSet.ShipmentDefaultPhase,
				"ShipmentDefaultPhase",
				FreightDataRegistry.Categories.Freight_Shipment_Phases,
				"Default Phase Override",
				@"Use this Registry setting to override the default Phase on new Shipment records.

Hint: The default Phase will load when creating a new shipment record automatically and may lock required fields for completion before save.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				PhaseConstants.Phase.ALL);
		}

		#endregion

		#region PreAllocation Check

		public void TestConsolPreAllocationCheck()
		{
			PreAllocationCheckRegistryItem item = ItemSet.ConsolPreAllocationCheck;
			CombineAssertions(delegate
			{
				AssertEquals("Name", "ConsolPreAllocationCheck", item.Name);
				AssertEquals("Category", FreightDataRegistry.Categories.Freight_Consolidations, item.Category);
				AssertEquals("Caption", "Consol Pre-Allocations", item.Caption);
				AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.BranchDepartment, item.Storage);

				foreach (PreAllocationCheck check in PreAllocationCheckCollection.GetDefault())
				{
					AssertEquals(true, item.DefaultValue.Cast<PreAllocationCheck>().Any(x => x.Action == check.Action && x.Measure == check.Measure && x.Percentage == check.Percentage));
				}
			});
		}

		#endregion

		#region ShowCountryStateBookingRequestShippingInstruction

		public void TestShowCountryStateBookingRequestShippingInstruction()
		{
			TestRegistryItem(
				ItemSet.ShowCountryStateBookingRequestShippingInstruction,
				"ShowCountryStateBookingRequestShippingInstruction",
				ForwardingConfigurationRegistry.Categories.Freight_Consolidations_OceanCarrierMessaging,
				"Show Country/Region on Booking Request/Shipping Instruction",
				"Specify whether you would like by default to display Port Name, State (only for US ports), Country/Region on Port of Load, Port of Discharge, Origin, Destination, Place of Receipt and Place of Delivery fields on Booking Request and Shipping Instruction Forms. If set to 'No' only Port Name will display.",
				RegistryStorageFlags.Company,
				true
				);
		}

		#endregion

		#region ContainerWorkflowExceptionGeneratorsHighWaterMark

		public void TestContainerDetentionExceptionGeneratorHighWaterMark()
		{
			TestGenericRegistryItem(ItemSet.ContainerDetentionExceptionGeneratorHighWaterMark,
					"ContainerDetentionExceptionGeneratorHighWaterMark",
					ForwardingConfigurationRegistry.Categories.Freight_ContainerExceptionGenerators,
					"Container Detention Exception Generator Last Run",
					"Time of Container Detention Exception Generator Last Run.",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					DateTime.MinValue);
		}

		public void TestContainerStorageExceptionGeneratorHighWaterMark()
		{
			TestGenericRegistryItem(ItemSet.ContainerStorageExceptionGeneratorHighWaterMark,
					"ContainerStorageExceptionGeneratorHighWaterMark",
					ForwardingConfigurationRegistry.Categories.Freight_ContainerExceptionGenerators,
					"Container Storage Exception Generator Last Run",
					"Time of Container Storage Exception Generator Last Run.",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					DateTime.MinValue);
		}

		#endregion

		#region House Bills Number Validation

		public void TestHouseBillsNumberValidation()
		{
			AssertEquals(RegistryStorageFlags.System, ItemSet.HouseBillsNumberValidation.Storage);
			AssertEquals("Freight/House Bills", ItemSet.HouseBillsNumberValidation.Category);
			AssertEquals("HouseBillsNumberValidation", ItemSet.HouseBillsNumberValidation.Name);
			AssertEquals("House Bills Number Validation", ItemSet.HouseBillsNumberValidation.Caption);
			AssertEquals("Use this registry to specify check digit algorithm to be applied to house bill numbers (not auto-generated) satisfying specified conditions.", ItemSet.HouseBillsNumberValidation.Hint);
			AssertEquals(0, ItemSet.HouseBillsNumberValidation.DefaultValue.Count);
		}

		#endregion

		public void TestConsolTypeRegistry()
		{
			TestGenericRegistryItem(ItemSet.ConsolTypeDefaultForConsol,
									"Freight.Consol.DefaultForJKAgentType",
									FreightDataRegistry.Categories.Freight_Consolidations,
									"Consol type",
									"Default value for new consol's 'type' field.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
									Core.Constants.AgentType.Agent);
		}

		#region Implementation

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "SenderPIMAAddress";
			}
		}

		#endregion
	}
}
