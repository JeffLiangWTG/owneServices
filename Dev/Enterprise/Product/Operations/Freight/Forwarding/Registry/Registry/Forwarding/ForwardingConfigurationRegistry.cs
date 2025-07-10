using System;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public sealed class ForwardingConfigurationRegistry : RegistryItemSet
	{
		#region Construction

		public static ForwardingConfigurationRegistry Instance
		{
			get { return instance ?? (instance = new ForwardingConfigurationRegistry()); }
		}

		[ThreadStatic]
		static ForwardingConfigurationRegistry instance;

		ForwardingConfigurationRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : FreightDataRegistry.Categories
		{
			public static MultilingualString Freight_AWB_CargoIMP_Phase2Messaging { get { return CombineCategories(Freight_AWB_CargoIMP, ResString.GetMultilingualString("e4364265-e13b-4ae5-a70d-c8a793e431c9", "Phase 2 Messaging")); } }
			public static MultilingualString Freight_AWB_CargoIMP_Phase2Messaging_Traxon { get { return CombineCategories(Freight_AWB_CargoIMP_Phase2Messaging, ResString.GetMultilingualString("fbe295f3-3fc6-497e-b940-8ebc1c1e92b1", "Traxon")); } }
			public static MultilingualString Freight_ContainerExceptionGenerators { get { return CombineCategories(Freight, ResString.GetMultilingualString("1f9d115c-1ee9-4e18-916e-65853af7a84c", "Container Exception Generators")); } }
		}

		#endregion

		#region CargoIMP

		#region FWB Messaging

		public StringRegistryItem SenderPIMAAddress
		{
			get
			{
				return GetItem("SenderPIMAAddress", delegate
				{
					return new StringRegistryItem(
						"SenderPIMAAddress",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						ResString.GetMultilingualString("903b4f1b-17b8-47e8-ad99-8b78754ca47d", "Sender Identification (PIMA)"),
						ResString.GetMultilingualString("05a2c978-7ba7-4dbe-a922-951831ec7504", "Specify the Participant Identification and Messaging Address (PIMA) for your organization. This will be used in the header for CargoIMP messages under certain conditions."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#region CargoIMPServiceProvider

		public CodePairRegistryItem CargoIMPServiceProvider
		{
			get
			{
				return GetItem("CargoImpServiceProvider", delegate
				{
					return new CodePairRegistryItem("CargoImpServiceProvider",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						(NoResString)"FWB Service Provider", // Support Only Registry Item
						(NoResString)"FWB Messaging Service Provider", // Support Only Registry Item,
						new CodeDescriptionPairListProvider(() => CargoIMPServiceProviderList.New()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						Constants.AWB.CargoIMPServiceProviderConstants.HUB);
				});
			}
		}

		#endregion

		public CodePairRegistryItem CargoImpSentMessageVersions
		{
			get
			{
				return GetItem("CargoImpSentMessageVersions", delegate
				{
					return new CodePairRegistryItem("CargoImpSentMessageVersions",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						ResString.GetMultilingualString("4c25d038-b8a1-486d-98d6-fa525517f64f", "FWB/FHL Message Versions"),
						ResString.GetMultilingualString("74bf8626-e7f1-40a3-9e53-712c9884789b", "Versions of the FWB and FHL CargoIMP Messages to send when sending an FWB/FHL Message set."), //S
						new CodeDescriptionPairListProvider(() => new CargoIMPSentMessageVersionsList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						CargoIMPSentMessageVersionsList.Codes.AutoSwitch);
				});
			}
		}

		public BooleanRegistryItem AllowUsersToSendFWBsWithErrors
		{
			get
			{
				return GetItem("ALLOW_USERS_TO_SEND_FWBS_WITH_ERRORS", delegate
				{
					return new BooleanRegistryItem(
						"ALLOW_USERS_TO_SEND_FWBS_WITH_ERRORS",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						ResString.GetMultilingualString("de7000e5-1875-4382-8d16-90812af2f2ae", "Allow Users To Send FWBs With Errors"),
						ResString.GetMultilingualString("d8cc77d7-2d37-45ed-aea4-ed7f69c13e8d", "Users will be allowed to send Electronic AWB messages even though they have errors that will cause failure or rejection by the Airlines. Also, Message Errors will be shown as Warnings on the AWB Tab."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem ForceSendingOfFWBCVDSegment
		{
			get
			{
				return GetItem("ForceSendingOfFWBCVDSegment", delegate
				{
					return new BooleanRegistryItem(
						"ForceSendingOfFWBCVDSegment",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						(NoResString)"Force Sending Of FWB CVD Segment",
						(NoResString)"Force Sending Of FWB CVD Segment",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		public StringRegistryItem CargoIMPClientCodeSuffix
		{
			get
			{
				return GetItem("FWBClientCodeSuffix", () =>
					{
						return new StringRegistryItem("FWBClientCodeSuffix",
							Categories.Freight_AWB_CargoIMP_FWBMessaging,
							(NoResString)"FWB Client Code Suffix", // Support Only Registry Item
							(NoResString)"Client Code Suffix to uniquely identify your CargoIMP sender ID (Server ID is used by default). This ID is to be provided by CargoWise, and only if required.", // Support Only Registry Item
							new StringRegistryDataType(3, 3) { CharacterCase = CharacterCase.Upper },
							new TextRegistryEditorInfo(TextEditorType.TextBox),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							Env.Registry.PhysicalServerID);
					});
			}
		}

		public BooleanRegistryItem AllowFWBWithoutAGTSegment
		{
			get
			{
				return GetItem("SendFWBWithoutAgentDetailsSegment", delegate
				{
					return new BooleanRegistryItem(
						"SendFWBWithoutAgentDetailsSegment",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						ResString.GetMultilingualString("c48f32b2-01c3-43a6-bfb9-802befb72096", "Allow FWB Without AGT Segment"),
						ResString.GetMultilingualString("63475984-1dae-4836-a019-5f9f960e1ad8", "This will allow the FWB to be sent without the AGT (Agent Details) segment. This is used where your company does not have an Agent IATA Code, but is creating MAWBs regardless. Please note that without an Agent IATA Code, you are not entitled to receive any Agent Commission."),
						RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem SendFWBOrFHLToAirlineBasedOnMAWBPrefix
		{
			get
			{
				return GetItem("SendFWBOrFHLToAirlineBasedOnMAWBPrefix", delegate
				{
					return new BooleanRegistryItem(
						"SendFWBOrFHLToAirlineBasedOnMAWBPrefix",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						ResString.GetMultilingualString("a1f843af-f194-4d86-8754-1f9eedd5f2e9", "Send FWB/FHL to Airline based on MAWB Prefix"),
						ResString.GetMultilingualString("cec792ce-1124-47cb-8123-51b1512d7b77", "This will allow FWB and FHL messages to be sent to the Airline based on the MAWB prefix rather than the Airline of the first flight as the two may not be the same Airline due to interline. This is used to route FWB and FHL messages to Airline who issued the MAWB regardless of which Airline might be carrying the shipment first."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region CargoIMP Phase 2

		public BooleanRegistryItem CargoIMPPhase2AllowToSendMessagesWithErrors
		{
			get
			{
				return GetItem("CargoIMPPhase2AllowToSendMessagesWithErrors", delegate
				{
					return new BooleanRegistryItem(
						"CargoIMPPhase2AllowToSendMessagesWithErrors",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("8d4508cc-e0b2-4e2a-9f89-573f0f77580a", "Allow Users To Send Messages With Errors"),
						ResString.GetMultilingualString("74bca723-3462-40b0-b300-9c2e280e8b5a", "Users will be allowed to send Electronic CargoIMP Phase 2 messages even though they have errors that will cause failure or rejection."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings
		{
			get
			{
				return GetItem("CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings", delegate
				{
					return new BooleanRegistryItem(
						"CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("0e6b4c34-b1d7-42c8-a0c1-ee95c78806f0", "Allow Messages With Warnings to be sent from Milestones"),
						ResString.GetMultilingualString("a6875ae5-6699-4d82-88d4-4518e8083d28", "Electronic CargoIMP Phase 2 messages will be sent from Milestone Trigger Actions even though they have warnings."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public StringRegistryItem CargoIMPPhase2ForwarderId
		{
			get
			{
				return GetItem("CargoIMPPhase2ForwarderId", delegate
				{
					return new StringRegistryItem(
						"CargoIMPPhase2ForwarderId",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("ab693509-27dc-4c2c-9b99-572f1d0badfb", "Forwarder Identification"),
						ResString.GetMultilingualString("46c4a1b0-94b8-420a-8333-6d37379b00d7", "Specify the Forwarder Identification for your organization. This will be used for CargoIMP Phase 2 messages."),
						RegistryStorageFlags.Company, "");
				});
			}
		}

		public CargoIMPPhase2MSUEventsMappingRegistryItem CargoIMPPhase2MSUEventsMapping
		{
			get
			{
				return GetItem("CargoIMPPhase2MSUEventsMapping", delegate
				{
					return new CargoIMPPhase2MSUEventsMappingRegistryItem(
						"CargoIMPPhase2MSUEventsMapping",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("1972912a-a833-440b-a7e2-0230e6c0e10d", "MSU Events Mapping"),
						ResString.GetMultilingualString("86a87b72-72e8-446e-a399-f1931217fb1c", "Specifies the MSU (Milestone Status Update) Event Code to use when a {0} Event with the corresponding CW1 Event Reference parameter is fired.", Core.Constants.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						CargoIMPPhase2MSUEventsMappingCollection.GetDefault());
				});
			}
		}

		public CargoIMPPhase2RouteMapRegistryItem CargoIMPPhase2RouteMap
		{
			get
			{
				return GetItem("CargoIMPPhase2RouteMap", delegate
				{
					return new CargoIMPPhase2RouteMapRegistryItem(
						"CargoIMPPhase2RouteMap",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("14b0d741-ef45-4861-8199-a11d43fb65e0", "Supported Routes"),
						ResString.GetMultilingualString("5f7ec561-9505-4b03-b827-f0fa072cac38", "Specifies the list of supported origin and destination pairs for each airline two-character code."),
						RegistryStorageFlags.Company,
						new CargoIMPPhase2RouteMapCollection());
				});
			}
		}

		public CodePairRegistryItem CargoIMPPhase2ServiceProvider
		{
			get
			{
				return GetItem("CargoIMPPhase2ServiceProvider", delegate
				{
					return new CodePairRegistryItem("CargoIMPPhase2ServiceProvider",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("49244ED3-9B24-46A1-83D1-D49FBA298575", "Service Provider"),
						ResString.GetMultilingualString("A92933A9-3511-4E3B-93D5-F4FCAC2B6882", "CargoIMP Phase 2 Service Provider"),
						new CodeDescriptionPairListProvider(() => new CargoIMPPhase2ServiceProviderList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						CargoIMPPhase2ServiceProviderList.Codes.Traxon);
				});
			}
		}

		public CodePairRegistryItem CargoIMPPhase2SendMessageErrors
		{
			get
			{
				return GetItem("CargoIMPPhase2SendMessageErrors", delegate
				{
					return new CodePairRegistryItem("CargoIMPPhase2SendMessageErrors",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("8ebbdda8-b58f-47ee-9d77-0a6c0f08207a", "Send Errors To"),
						ResString.GetMultilingualString("94c79a11-82a5-4426-873a-406f8bdd175f", "Send message errors to staff member, nominated group or combination of both"),
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem CargoIMPPhase2GroupToSendMessageErrors
		{
			get
			{
				return GetItem("CargoIMPPhase2GroupToSendMessageErrors", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CargoIMPPhase2GroupToSendMessageErrors",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("68681bbb-90ef-4f6f-b1d4-520b3c9194b2", "Group to Send Errors"),
						ResString.GetMultilingualString("b2ba039b-0133-449d-9b6e-5b9037b88941", "The group of users the informing emails can be sent to. Must be set if 'Send Errors To' is set to {0} or {1}.", Constants.EmailTo.NominatedGroup, Constants.EmailTo.StaffMemberAndNominatedGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public BooleanRegistryItem CargoIMPPhase2UseETDForHouseBillDate
		{
			get
			{
				return GetItem("CargoIMPPhase2UseETDForHouseBillDate", delegate
				{
					return new BooleanRegistryItem(
						"CargoIMPPhase2UseETDForHouseBillDate",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging,
						ResString.GetMultilingualString("ed13f821-0fe9-4529-bb9c-4397f03a507c", "Use ETD as message's 'House Bill Date'"),
						ResString.GetMultilingualString("586e3345-33ee-406d-b6a4-7a45a50c35b5", "System Creation Time is used by default to fill in 'House Bill Date' message field.\r\nIf this registry item is set then ETD is used instead.\r\nYou may need to use ETD instead of System Creation Time if there exists another system which sends RMI and/or MSU for the same shipment but uses ETD as 'House Bill Date'.\r\nYou should not enable this registry item otherwise."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#region Traxon FTP

		public StringRegistryItem CargoIMPPhase2TraxonSenderPIMAAddress
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonSenderPIMAAddress", delegate
				{
					return new StringRegistryItem(
						"CargoIMPPhase2TraxonSenderPIMAAddress",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("903b4f1b-17b8-47e8-ad99-8b78754ca47d", "Sender Identification (PIMA)"),
						ResString.GetMultilingualString("2b028eeb-3f7d-4dc3-a687-fba20438de7f", "Specify the Participant Identification and Messaging Address (PIMA) for your organization. This will be used in the Traxon header for CargoIMP Phase 2 messages."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, "");
				});
			}
		}

		public StringRegistryItem CargoIMPPhase2TraxonFTPServer
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPServer", delegate
				{
					return new StringRegistryItem(
						"CargoIMPPhase2TraxonFTPServer",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("7a5d2082-acd3-431f-bba2-6f9775f46290", "FTP Server Address"),
						ResString.GetMultilingualString("1c136b22-bbdd-44c6-8efe-a7c42cbc855a", "The FTP Server address for Traxon messages."),
						RegistryStorageFlags.System)
					{ DataType = new FtpUriRegistryDataType() };
				});
			}
		}

		public StringRegistryItem CargoIMPPhase2TraxonFTPUserName
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPUserName", delegate
				{
					return new StringRegistryItem(
						"CargoIMPPhase2TraxonFTPUserName",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("6f1c7ebd-1b54-4165-a8c5-617bb166d7d1", "FTP User Name"),
						ResString.GetMultilingualString("594abcc7-98c4-49bc-b1b0-b9da1da952a9", "The FTP user name for Traxon messages."),
						RegistryStorageFlags.System);
				});
			}
		}

		public StringRegistryItem CargoIMPPhase2TraxonFTPPasswordEncrypted
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPPasswordEncrypted", delegate
				{
					return new StringRegistryItem(
						"CargoIMPPhase2TraxonFTPPasswordEncrypted",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("37f54047-0b28-4646-98e1-81296b398525", "FTP Password"),
						ResString.GetMultilingualString("7c75ece9-356b-4ea1-9fb3-29513215bffe", "The FTP password for Traxon messages."),
						new SecureStringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem CargoIMPPhase2TraxonLastFTPFailureTime
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonLastFTPFailureTime", delegate
				{
					return new DateTimeRegistryItem(
						"CargoIMPPhase2TraxonLastFTPFailureTime",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						(NoResString)"Time of Last FTP Failure",
						(NoResString)"Time of Last Traxon FTP Failure.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						DateTime.MinValue);
				});
			}
		}

		public IntRegistryItem CargoIMPPhase2TraxonFTPFailureCount
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPFailureCount", delegate
				{
					return new IntRegistryItem(
						"CargoIMPPhase2TraxonFTPFailureCount",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						(NoResString)"Number of Consecutive FTP Failures",
						(NoResString)"Number of Consecutive Traxon FTP Failures.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						0);
				});
			}
		}

		#endregion

		public IntRegistryItem CargoIMPPhase2TraxonFTPMaxFailureCount
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPMaxFailureCount", delegate
				{
					return new IntRegistryItem(
						"CargoIMPPhase2TraxonFTPMaxFailureCount",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("8bda647b-ec7c-499e-b610-1d63f2af8285", "Allowed Number of Consecutive FTP Failures"),
						ResString.GetMultilingualString("9e86200e-082e-409a-ae59-50c78bec73d1", "Number of consecutive Traxon FTP failures before sending error email and suspending of communication."),
						RegistryStorageFlags.System,
						10);
				});
			}
		}

		public IntRegistryItem CargoIMPPhase2TraxonFTPDelayAfterFailure
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPDelayAfterFailure", delegate
				{
					return new IntRegistryItem(
						"CargoIMPPhase2TraxonFTPDelayAfterFailure",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("e733e3f3-063e-4dd7-98c0-9876e293b764", "Time From Last FTP Failure To Next Send Try"),
						ResString.GetMultilingualString("287658f0-9e5e-4414-8fd4-f310df55f251", "Number of minutes that should pass from last Traxon FTP failure to next try to send data."),
						RegistryStorageFlags.System,
						30);
				});
			}
		}

		public IntRegistryItem CargoIMPPhase2TraxonInterchangeMaxSends
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonInterchangeMaxSends", delegate
				{
					return new IntRegistryItem(
						"CargoIMPPhase2TraxonInterchangeMaxSends",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("402e9be2-08ce-4a2f-b490-549b114ef627", "Interchange Maximum Sends"),
						ResString.GetMultilingualString("25fd0577-4cb1-489b-beb9-cf97c364024d", "This is the maximum number of times a failed to send interchange will be sent to Traxon before the process controller gives up and flags an error on the interchange."),
						RegistryStorageFlags.System,
						10);
				});
			}
		}

		public StringRegistryItem CargoIMPPhase2TraxonFTPInboundDirectory
		{
			get
			{
				return GetItem("CargoIMPPhase2TraxonFTPInboundDirectory", delegate
				{
					return new StringRegistryItem(
						"CargoIMPPhase2TraxonFTPInboundDirectory",
						Categories.Freight_AWB_CargoIMP_Phase2Messaging_Traxon,
						ResString.GetMultilingualString("c2131895-7ede-46bf-832d-4158e9508b39", "FTP Server Inbound Directory"),
						ResString.GetMultilingualString("637814b1-e7c8-4f7b-b7e7-aa141021355d", "The directory name of Traxon FTP Server where messages will be uploaded by the system."),
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region HAWB

		public BooleanRegistryItem PrintForwarderBranchDetailsHAWBIssuedBySection
		{
			get
			{
				return GetItem("PrintForwarderBranchDetailsHAWBIssuedBySection", delegate
				{
					return new BooleanRegistryItem("PrintForwarderBranchDetailsHAWBIssuedBySection",
									Categories.Freight_AWB_HAWB,
									ResString.GetMultilingualString("aea59630-ca31-48eb-a926-b726c2adb03f", @"Print Forwarder Branch Details in the 'Issued By' section"),
									ResString.GetMultilingualString("03de7ecf-27da-45fe-a8ec-07d0ddc1c4a4", "This determines whether the 'issued by' section in the House AWB document is filled in or not."),
									RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									true);
				});
			}
		}

		#endregion

		#region House Bills Number Validation

		public HouseBillsNumberValidationRegistryItem HouseBillsNumberValidation
		{
			get
			{
				return GetItem("HouseBillsNumberValidation", delegate
				{
					return new HouseBillsNumberValidationRegistryItem(
						"HouseBillsNumberValidation",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("f9a552c9-9728-4b4b-817f-e06088d44135", "House Bills Number Validation"),
						ResString.GetMultilingualString("88ac5375-3904-4799-be76-de1050b97c96", "Use this registry to specify check digit algorithm to be applied to house bill numbers (not auto-generated) satisfying specified conditions."),
						RegistryStorageFlags.System,
						new HouseBillsNumberValidationCollection()
						);
				});
			}
		}

		#endregion

		#region Phase Security

		public PhaseSecurityRegistryItem ConsolPhaseSecurity
		{
			get
			{
				return GetItem("ConsolPhaseSecurity", delegate
				{
					return new PhaseSecurityRegistryItem(
						"ConsolPhaseSecurity",
						Categories.Freight_Consolidations_Phases,
						ResString.GetMultilingualString("e72cb2fc-578f-48d5-a620-a3be9df3fdb7", "Phases Configuration"),
						GetPhaseSecurityHint(ResString.GetMultilingualString("52a085e7-0bae-420b-b530-40ff90142cc6", "Consol")),
						PhaseConstants.GetConsolLocationsList(),
						new ConsolPhaseDependantsProvider());
				});
			}
		}

		public PhaseSecurityRegistryItem ShipmentPhaseSecurity
		{
			get
			{
				return GetItem("ShipmentPhaseSecurity", delegate
				{
					return new PhaseSecurityRegistryItem(
						"ShipmentPhaseSecurity",
						Categories.Freight_Shipment_Phases,
						ResString.GetMultilingualString("e72cb2fc-578f-48d5-a620-a3be9df3fdb7", "Phases Configuration"),
						GetPhaseSecurityHint(ResString.GetMultilingualString("0e6da834-bb92-4ab2-a901-74bc3f2238a3", "Shipment")),
						PhaseConstants.GetShipmentLocationsList(),
						new ShipmentPhaseDependantsProvider());
				});
			}
		}

		public CodePairRegistryItem ConsolDefaultPhase
		{
			get
			{
				return GetItem("ConsolDefaultPhase", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"ConsolDefaultPhase",
						FreightDataRegistry.Categories.Freight_Consolidations_Phases,
						ResString.GetMultilingualString("c9f03d05-4c36-49e9-a5a7-cf1e6dc6ff63", "Default Phase Override"),
						GetDefaultPhaseHint(ResString.GetMultilingualString("96bf2c00-cfd5-4eed-8f79-ed0770abefca", "Consolidation")),
						new CodeDescriptionPairListProvider(() => LoadPhases(ConsolPhaseSecurity.Value.Phases)),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						PhaseConstants.Phase.ALL);
					return result;
				});
			}
		}

		public CodePairRegistryItem ShipmentDefaultPhase
		{
			get
			{
				return GetItem("ShipmentDefaultPhase", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"ShipmentDefaultPhase",
						FreightDataRegistry.Categories.Freight_Shipment_Phases,
						ResString.GetMultilingualString("c9f03d05-4c36-49e9-a5a7-cf1e6dc6ff63", "Default Phase Override"),
						GetDefaultPhaseHint(ResString.GetMultilingualString("0e6da834-bb92-4ab2-a901-74bc3f2238a3", "Shipment")),
						new CodeDescriptionPairListProvider(() => LoadPhases(ShipmentPhaseSecurity.Value.Phases)),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						PhaseConstants.Phase.ALL);
					return result;
				});
			}
		}

		MultilingualString GetPhaseSecurityHint(MultilingualString bizoName)
		{
			return ResString.GetMultilingualString("403caad6-d036-429a-b8da-102389ff47c8",
@"Use this Registry item to define Phases controlling access to {1}.

Select “Override Default” to define or amend your own phases.

Default pre-defined phase “ALL” on the {0} indicates open security.

Select “Enable Phase Based Security” to enable the system to check Security Rights for {1} based on the current {0}’s Phase, logged in user’s Department & Location (where Locations cover ports and countries related to the logged in user’s Branch).",
				bizoName, bizoName.Pluralize());
		}

		MultilingualString GetDefaultPhaseHint(MultilingualString bizoName)
		{
			return ResString.GetMultilingualString("71725617-db7d-422b-a05c-faca9ce9ce8f",
				@"Use this Registry setting to override the default Phase on new {0} records.

Hint: The default Phase will load when creating a new shipment record automatically and may lock required fields for completion before save.",
				bizoName);
		}

		CodeDescriptionPairList LoadPhases(PhaseCollection customPhases)
		{
			var list = PhaseConstants.GetCommonPhaseList();
			foreach (IPhase phase in customPhases)
			{
				list.AddPairIfNotExist(phase.Code, phase.Description);
			}
			return list;
		}

		#endregion

		#region Consol PreAllocation Check

		public PreAllocationCheckRegistryItem ConsolPreAllocationCheck
		{
			get
			{
				return GetItem("ConsolPreAllocationCheck", delegate
				{
					return new PreAllocationCheckRegistryItem(
						"ConsolPreAllocationCheck",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("eb9250d0-cf8c-415d-afdd-4ca8882a6609", "Consol Pre-Allocations"),
						ResString.GetMultilingualString("7f490027-5ab1-4fa1-8877-1558bc508dca", "Nominate the action when consol actual values exceed the specified percentage of the consol pre-allocated values.\r\n\r\nWhen the registry is set to Override, you can nominate to show a warning when consol total weight, volume, chargeable or number of shipments exceeds the specified percentage of the consol pre-allocated values; or set restrictions for users without security rights to modify consol and attached shipments in any way that exceeds the specified percentage of the consol pre-allocated values.  Default value for all parameters is “None”, meaning no warning or restrictions will apply on over-allocations."),
						RegistryStorageFlags.Company | RegistryStorageFlags.BranchDepartment,
						PreAllocationCheckCollection.GetDefault());
				});
			}
		}

		#endregion

		#region Show Country/State on Booking Request/Shipping Instruction

		public BooleanRegistryItem ShowCountryStateBookingRequestShippingInstruction
		{
			get
			{
				return GetItem("ShowCountryStateBookingRequestShippingInstruction", delegate
				{
					return new BooleanRegistryItem(
						"ShowCountryStateBookingRequestShippingInstruction",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("4539C999-43B1-4CDC-B18F-196EAACC3CE9", "Show Country/Region on Booking Request/Shipping Instruction"),
						ResString.GetMultilingualString("706D75B1-DBA2-4E2D-88B7-6BD6697A4300", "Specify whether you would like by default to display Port Name, State (only for US ports), Country/Region on Port of Load, Port of Discharge, Origin, Destination, Place of Receipt and Place of Delivery fields on Booking Request and Shipping Instruction Forms. If set to 'No' only Port Name will display."),
						RegistryStorageFlags.Company,
						true
						);
				});
			}
		}

		#endregion

		#region ContainerWorkflowExceptionGeneratorsHighWaterMark

		public DateTimeRegistryItem ContainerDetentionExceptionGeneratorHighWaterMark
		{
			get
			{
				return GetItem("ContainerDetentionExceptionGeneratorHighWaterMark", delegate
				{
					return new DateTimeRegistryItem(
						"ContainerDetentionExceptionGeneratorHighWaterMark",
						Categories.Freight_ContainerExceptionGenerators,
						(NoResString)"Container Detention Exception Generator Last Run",      // Internal hidden item
						(NoResString)"Time of Container Detention Exception Generator Last Run.",   // Internal hidden item
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						DateTime.MinValue);
				});
			}
		}

		public DateTimeRegistryItem ContainerStorageExceptionGeneratorHighWaterMark
		{
			get
			{
				return GetItem("ContainerStorageExceptionGeneratorHighWaterMark", delegate
				{
					return new DateTimeRegistryItem(
						"ContainerStorageExceptionGeneratorHighWaterMark",
						Categories.Freight_ContainerExceptionGenerators,
						(NoResString)"Container Storage Exception Generator Last Run",  // Internal hidden item
						(NoResString)"Time of Container Storage Exception Generator Last Run.", // Internal hidden item
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						DateTime.MinValue);
				});
			}
		}

		#endregion

		#region Consol
		public CodePairRegistryItem ConsolTypeDefaultForConsol
		{
			get
			{
				return GetItem("Freight.Consol.DefaultForJKAgentType", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"Freight.Consol.DefaultForJKAgentType",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("B4AB8F30-F76E-4CB8-B060-AC307C9D9C77", "Consol type"),
						ResString.GetMultilingualString("97E1DF8F-2A0B-4DE7-8A85-5CEAADC4532C", "Default value for new consol's 'type' field."),
						OLookUpEditType.AgentType,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						Core.Constants.AgentType.Agent);
					return result;
				});
			}
		}
		#endregion

		#region Denied Party Screening Status Update

		public DpsStatusUpdateSettingRegistryItem ShipmentDpsStatusUpdateSetting
		{
			get
			{
				return GetItem("ShipmentDpsStatusUpdateSetting", delegate
				{
					return new DpsStatusUpdateSettingRegistryItem(
						"ShipmentDpsStatusUpdateSetting",
						OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("cbf69893-626b-4860-bf32-7c951d920757", "Shipment DPS Update"),
						GetDpsStatusUpdateSettingHint(ResString.GetMultilingualString("2235eaaf-647b-4c18-9ad0-54de3f6589ed", "Shipment")),
						RegistryStorageFlags.System, ShipmentPhaseSecurity);
				});
			}
		}

		public DpsStatusUpdateSettingRegistryItem ConsolDpsStatusUpdateSetting
		{
			get
			{
				return GetItem("ConsolDpsStatusUpdateSetting", delegate
				{
					return new DpsStatusUpdateSettingRegistryItem(
						"ConsolDpsStatusUpdateSetting",
						OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("bf0306f6-799f-47d6-9c83-7794f5d0c7ea", "Consol DPS Update"),
						GetDpsStatusUpdateSettingHint(ResString.GetMultilingualString("a7d98124-7708-464a-ae12-65bd3f87925f", "Consolidation")),
						RegistryStorageFlags.System, ConsolPhaseSecurity);
				});
			}
		}

		MultilingualString GetDpsStatusUpdateSettingHint(MultilingualString bizoName)
		{
			return ResString.GetMultilingualString("e034e8dd-47c2-4fb3-ae44-307a5c1f2be0",
@"Select the method to determine which {1} to update when the DPS status of an organization referenced on the {0} changes.",
				bizoName, bizoName.Pluralize());
		}

		#endregion
	}
}
