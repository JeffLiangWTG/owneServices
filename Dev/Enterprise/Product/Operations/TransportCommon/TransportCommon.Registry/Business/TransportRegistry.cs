using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportCommon.Registry
{
	public sealed class TransportRegistry : RegistryItemSet, ITransportRegistry
	{
		#region Constants

		public const int AutoCreateTransportConsignmentsCreationPeriodDefaultValue = 24;
		public const int AutoCreateTransportConsignmentsCreationPeriodMinValue = 1;
		public const int AutoCreateTransportConsignmentsCreationPeriodMaxValue = 10000;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Instance

		public static TransportRegistry Instance
		{
			get { return instance ?? (instance = new TransportRegistry()); }
		}

		TransportRegistry()
		{
		}

		[ThreadStatic]
		static TransportRegistry instance;

		#endregion

		#region TransportCommon
		#region Authorised to Leave

		public BooleanRegistryItem AuthorityToLeave
		{
			get
			{
				return GetItem("AuthorityToLeave", () => new BooleanRegistryItem(
					"AuthorityToLeave",
					RawDataRegistry.Categories.Transport,
					ResString.GetMultilingualString("BEFF7C09-79BE-48FC-B46C-48988570E1E8", "Authority To Leave Default"),
					ResString.GetMultilingualString("E58755BF-0373-41F6-A311-4FBCA1206C47", "The default value for Authority To Leave."),
					RegistryStorageFlags.System,
					false));
			}
		}

		#endregion
		#endregion

		#region CarrierMessagingBuss

		public BooleanRegistryItem EnableBookingWithCarrierMessagingBuss
		{
			get
			{
				return GetItem("EnableBookingWithCarrierMessagingBuss", () => new BooleanRegistryItem(
					"EnableBookingWithCarrierMessagingBuss",
					RawDataRegistry.Categories.Transport_CarrierMessagingBuss,
					(NoResString)"Enable Booking with Carrier Messaging Buss",
					(NoResString)"Enable Booking with Carrier Messaging Buss",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public PackageVersionOverrideRegistryItem PackageVersionOverride
		{
			get
			{
				return GetItem("PackageVersionOverride", () => new PackageVersionOverrideRegistryItem(
					"PackageVersionOverride",
					RawDataRegistry.Categories.Transport_CarrierMessagingBuss,
					(NoResString)"Package Version Override",
					(NoResString)"Override Package Version based on Carrier Code and Account Number",
					RegistryStorageFlags.Company,
					new PackageVersionOverrideCollection(),
					RegistryOptions.IsOnlyForSupport));
			}
		}

		public StringRegistryItem CarrierMessagingBussServerAddress
		{
			get
			{
				return GetItem("CarrierMessagingBussServerAddress", delegate
				{
					return new StringRegistryItem(
						"CarrierMessagingBussServerAddress",
						RawDataRegistry.Categories.Transport_CarrierMessagingBuss,
						(NoResString)"Carrier Messaging Buss Server Address",
						(NoResString)"This is the internal address used to access the CarrierMessagingBuss Server.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://cmb.wisegrid.net/");
				});
			}
		}

		#endregion

		#region Transport Bookings

		#region Additional Reference Numbers

		public TransportReferenceNumberTypesRegistryItem AdditionalReferenceNumbers
		{
			get
			{
				return GetItem("TransportBookingsAdditionalReferenceNumbers", () =>
				{
					var defaultValue = new TransportReferenceNumberTypeCollection();
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.TransportReference, TransportCommonAdditionalReferenceTypes.Descriptions.TransportReference);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.CommercialInvoiceNumber, TransportCommonAdditionalReferenceTypes.Descriptions.CommercialInvoiceNumber);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, TransportCommonAdditionalReferenceTypes.Descriptions.ExternalTransportBookingNumber);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.ExternalUniqueConsignmentReference, TransportCommonAdditionalReferenceTypes.Descriptions.ExternalUniqueConsignmentReference);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.Client, TransportCommonAdditionalReferenceTypes.Descriptions.Client);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, TransportCommonAdditionalReferenceTypes.Descriptions.ClientReferenceNumber);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, TransportCommonAdditionalReferenceTypes.Descriptions.OrderNumber, isUnique: false);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, TransportCommonAdditionalReferenceTypes.Descriptions.HouseBill);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, TransportCommonAdditionalReferenceTypes.Descriptions.MasterBill, isUnique: false);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, TransportCommonAdditionalReferenceTypes.Descriptions.BookingPartyReference);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.CarrierBookingReference, TransportCommonAdditionalReferenceTypes.Descriptions.CarrierBookingReference);

					return new TransportReferenceNumberTypesRegistryItem(
						"TransportBookingsAdditionalReferenceNumbers",
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("10864854-42ba-4ba9-a67d-880248c9226e", "Additional Reference Types"),
						ResString.GetMultilingualString("4f792730-38ed-4b7d-a937-91c82fd7ca86", "This list defines the additional reference number types available under the Additional References tab."),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		#endregion

		#region AutoCreateTransportConsignmentsCreationPeriod

		public IntRegistryItem AutoCreateTransportConsignmentsCreationPeriod
		{
			get
			{
				var registryItem = GetItem("AutoCreateTransportConsignmentsCreationPeriod", delegate
				{
					return new IntRegistryItem("AutoCreateTransportConsignmentsCreationPeriod",
						RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
						ResString.GetMultilingualString("6ff6c528-9ed7-4d2a-9406-b808df3d4207", "Period before which to Auto-Create Transport Jobs"),
						ResString.GetMultilingualString("ddea84ba-3ed9-4199-80bf-54a29d8482b7",
							"The Transport Jobs Service Task automatically creates Transport Consignments or Port Transport jobs from received Transport Bookings. Specify in hours the period before the earliest Pick Up Date of the booking that the automatic creation of the job will occur." +
							"\r\n\r\nFor example, if this value is set to 24, Transport Jobs will be auto-created for Transport Bookings where the Pickup Date is within 24 hrs."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						AutoCreateTransportConsignmentsCreationPeriodDefaultValue,
						AutoCreateTransportConsignmentsCreationPeriodMinValue,
						AutoCreateTransportConsignmentsCreationPeriodMaxValue);
				});
				return registryItem;
			}
		}

		#endregion

		#region AutoCreateTransportConsignmentsGracePeriod

		public IntRegistryItem AutoCreateTransportConsignmentsGracePeriod
		{
			get
			{
				var registryItem = GetItem("AutoCreateTransportConsignmentsGracePeriod", delegate
				{
					return new IntRegistryItem("AutoCreateTransportConsignmentsGracePeriod",
						RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
						ResString.GetMultilingualString("354fe67b-2ba1-4fdc-9840-bdecfbfc5ee6", "Minimum Booking Age after which to Auto-Create Transport Jobs"),
						ResString.GetMultilingualString("c2efd5c3-7890-40ac-9813-ca388d3a99b3",
							"Specify the minimum delay (in minutes) after the last Edit on a Transport Booking, before that Booking can create a Consignment or Port Transport Job via the Transport Booking to Port/Land Transport Jobs Service Task."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						30,
						1,
						int.MaxValue);
				});
				return registryItem;
			}
		}

		#endregion

		#region CreateTransportJobsFromTransportBookingEffectiveDate

		public DateTimeRegistryItem CreateTransportJobsFromTransportBookingEffectiveDate
		{
			get
			{
				var registryItem = GetItem("AutoCreateTransportConsignmentsFromDate", delegate
				{
					return new DateTimeRegistryItem("AutoCreateTransportConsignmentsFromDate",
						RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
						ResString.GetMultilingualString("72E92293-E596-472F-BB79-48DAC1854F4F", "Create Transport Jobs from Transport Booking Effective Date"),
						ResString.GetMultilingualString("E19F6496-66F7-4B2D-A56C-104F53B796FB", "This setting is used to define the start date used to auto create Port Transport Jobs, or Land Transport Consignments from a Transport Bookings via the TBC Service Task."),
						RegistryStorageFlags.System,
						DateTime.MinValue);
				});
				return registryItem;
			}
		}

		#endregion

		#region CreateTransportJobsFromTransportBookingWithinPeriod

		public IntRegistryItem CreateTransportJobsFromTransportBookingWithinPeriod
		{
			get
			{
				var registryItem = GetItem("CreateTransportJobsFromTransportBookingWithinPeriod", delegate
				{
					return new IntRegistryItem("CreateTransportJobsFromTransportBookingWithinPeriod",
						RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
						ResString.GetMultilingualString("F706DB1C-443C-459C-BBA8-99B1CD0D662E", "Create Transport Jobs from Transport Booking within period"),
						ResString.GetMultilingualString("35335C32-B0EF-4150-A90F-0B7DC1465E8A", "This setting is used to define the age of the Transport Booking. When the number of days is set to 30 the TBC Service Task will consider Transport Bookings that have been created within the last 30 days. Any Bookings older than this will not be used. (Maximum value is 90 days, Minimum value is 0 day)"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						30,
						0,
						90);
				});
				return registryItem;
			}
		}

		#endregion

		#region ServiceTaskCreatorOption

		public ServiceTaskCreatorOptionRegistryItem ServiceTaskCreatorOption
		{
			get
			{
				var registryItem = GetItem("ServiceTaskCreatorOption", delegate
					{
						return new ServiceTaskCreatorOptionRegistryItem(
							"ServiceTaskCreatorOption",
							RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
							ResString.GetMultilingualString("TransportRegistry|ServiceTaskCreatorOption|Caption", "Service Task Target Options"),
							ServiceTaskCreatorOptionHint,
							RegistryStorageFlags.System,
							EnableLandTransport.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							ServiceTaskCreatorOptionCollection.GetDefault());
					});

				return registryItem;
			}
		}

		static MultilingualString ServiceTaskCreatorOptionHint
		{
			get
			{
				return ResString.GetMultilingualString("TransportRegistry|ServiceTaskCreatorOption|Hint", "Valid Transport Bookings will be sent to the selected Module based on the Freight Mode.");
			}
		}
		#endregion

		#region OrganisationRTUSOptions

		IRegistryItem ITransportRegistry.OrganisationRTUSOptions => OrganisationRTUSOptions;

		IOrganisationRTUSOption ITransportRegistry.GetRTUSOption(Guid carrierBookingAgentPK)
		{
			return OrganisationRTUSOptions.Value
				.Cast<OrganisationRTUSOption>()
				.SingleOrDefault(o => o.OrganisationPK == carrierBookingAgentPK);
		}

		public OrganisationRTUSRegistryItem OrganisationRTUSOptions
		{
			get
			{
				return GetItem("OrganisationRTUSOptions", delegate
				{
					return new OrganisationRTUSRegistryItem(
						"OrganisationRTUSOptions",
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("486F5344-5601-403A-8F12-CDD59430E65F", "Organization RTUS Options"),
						ResString.GetMultilingualString("1F613432-DE09-4D82-A08B-683C3402963B", "Map organizations to a CBA."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region OrganisationRTUSWebProxy

		public StringRegistryItem OrganisationRTUSWebProxy
		{
			get
			{
				return GetItem("OrganisationRTUSWebProxy", delegate
				{
					return new StringRegistryItem(
						"OrganisationRTUSWebProxy",
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("4F1DF2A1-AED1-4C6C-9FB4-515CFC080E94", "RTUS Web Proxy"),
						ResString.GetMultilingualString("51BFA596-8942-4AD2-888B-BC40D44D69C5", "Assign web proxy address for RTUS web service."),
						new InsecureUrlRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, string.Empty);
				});
			}
		}

		#endregion

		#region OrganisationRTUSWebProxyCredentials

		ServerUsernamePassword ITransportRegistry.OrganisationRTUSWebProxyCredentials
		{
			get
			{
				var config = new ServerUsernamePasswordConfiguration();
				config.UserName = OrganisationRTUSWebProxyUsername.Value;
				return new ServerUsernamePassword(config.UserName, config.UserNameWithoutServer, OrganisationRTUSWebProxyPassword.Value, config.ServerName);
			}
		}

		public StringRegistryItem OrganisationRTUSWebProxyUsername
		{
			get
			{
				return GetItem(nameof(OrganisationRTUSWebProxyUsername), delegate
				{
					return new StringRegistryItem(
						"OrganisationRTUSWebProxyUsername",
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("72A11F84-DD05-4C4E-BA29-06D5B0BC5473", "RTUS Web Proxy Username"),
						ResString.GetMultilingualString("224205B5-7119-4650-AC6D-75B9CE5B6C15", "Enter the RTUS web service proxy username, if applicable."),
						new SecureStringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						string.Empty);
				});
			}
		}

		public StringRegistryItem OrganisationRTUSWebProxyPassword
		{
			get
			{
				return GetItem(nameof(OrganisationRTUSWebProxyPassword), delegate
				{
					return new StringRegistryItem(
						"OrganisationRTUSWebProxyPassword",
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("9CC09379-104D-4938-9DFA-3563955D9E96", "RTUS Web Proxy Password"),
						ResString.GetMultilingualString("3AF95707-A7F9-4803-B730-45703DDB41B9", "Enter the RTUS web service proxy password, if applicable."),
						new SecureStringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						string.Empty);
				});
			}
		}

		#endregion

		#region RemotePrintServerURL

		public StringRegistryItem RemotePrintServerURL
		{
			get
			{
				return GetItem(nameof(RemotePrintServerURL), delegate
				{
					return new StringRegistryItem(
						nameof(RemotePrintServerURL),
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("55704859-B553-4E91-A928-94DA3DAA4909", "Remote Print Server URL for RTUS"),
						ResString.GetMultilingualString("5EDBDCF3-5B67-4ABB-992F-44B931373F1C", "Input the print web service URL for remote printing."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						string.Empty);
				});
			}
		}

		#endregion

		#region UseStandardRemotePrintingForRTUS

		public BooleanRegistryItem UseStandardRemotePrintingForRTUS
		{
			get
			{
				return GetItem("UseStandardRemotePrintingForRTUS", delegate
				{
					return new BooleanRegistryItem(
						"UseStandardRemotePrintingForRTUS",
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("0DCC18C8-6543-41FB-BA40-D3E4E42ECD51", "Use Standard Remote Printing for RTUS"),
						ResString.GetMultilingualString("64A8F7F4-92F1-4D57-BA97-DAB2C03AFE1C", "Enable this to use standard remote printing for RTUS."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region EnableRTUSBatchProcessing

		public BooleanRegistryItem EnableRTUSBatchProcessing
		{
			get
			{
				return GetItem("EnableRTUSBatchProcessing", delegate
				{
					return new BooleanRegistryItem(
						"EnableRTUSBatchProcessing",
						RawDataRegistry.Categories.Transport_RTUS,
						ResString.GetMultilingualString("EE32E3DB-C68F-4042-B0B9-B3DED6628026", "Enable RTUS Batch Processing"),
						ResString.GetMultilingualString("50A1138F-29FB-48CC-9B7A-F97157F6193C", "Enable this to allow batch processing of multiple RTUS requests instead of single processing for each RTUS request."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region DateAndReference

		public DateAndReferenceRegistryItem DateAndReference
		{
			get
			{
				var registryItem =

					GetItem("DateAndReferences", delegate
					{
						return new DateAndReferenceRegistryItem(
							"DateAndReferences",
							RawDataRegistry.Categories.Transport_TransportBookings,
							ResString.GetMultilingualString("693dad11-870e-4734-b844-233caa56df6d", "Dates and References"),
							DateAndReferenceHint,
							RegistryStorageFlags.System,
							DateAndReferenceCollection.GetDefault());
					});

				return registryItem;
			}
		}

		static MultilingualString DateAndReferenceHint
		{
			get
			{
				return ResString.GetMultilingualString("830feeba-24dd-4836-a3e3-04cc0792e1bb",
					@"The following Dates & References apply to an instruction. They can be customized to appear, add automatically and display different fields/labels for specific instructions.

Date & Reference : Consists of a Code and Description
	The same code may be listed more than once to allow for it to be customized for more than one condition.

Conditions : Booking Direction / Instruction Type / Organization Type / Container Mode
	The conditions under which Auto Add / Allow / Labels are applied.

Auto : Add
	If checked and matching the conditions, the Date & Reference will be automatically added.

Allow : Estimated / Actual / Required From / Required To / Reference Number / Slot Date / Signed By
	If checked and matching the conditions, these fields will be editable.

Labels : Required From / Required To
	Matching the conditions, these labels will be displayed.");
			}
		}

		#endregion

		#region TransportBookingChargeableFactor

		public ChargeableFactorRegistryItem TransportBookingChargeableFactor
		{
			get
			{
				return GetItem("TransportBookingChargeableFactor", () =>
				{
					var defaultFactor = new ChargeableFactor(
						new ConversionFactor(3000, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
						new ConversionFactor(250m, Constants.Volume.CubicInches, Constants.Weight.Pounds));

					return new ChargeableFactorRegistryItem(
						"TransportBookingChargeableFactor",
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("TransportRegistry|TransportBookingChargeableFactor|Caption", "Chargeable Factor for Transport Booking"),
						GetChargeableFactorVolumeToWeightHint(defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultFactor);
				});
			}
		}

		MultilingualString GetChargeableFactorVolumeToWeightHint(ChargeableFactor defaultFactor)
		{
			return ResString.GetMultilingualString("TransportRegistry|TransportBookingChargeableFactor|Hint",
@"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is {0}.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is {1}."
, defaultFactor.MetricFactor, defaultFactor.ImperialFactor);
		}

		#endregion

		#region JobTemplateDefault

		public JobTemplateDefaultRegistryItem JobTemplateDefault
		{
			get
			{
				var registryItem =

					GetItem("JobTemplateDefaults", delegate
					{
						return new JobTemplateDefaultRegistryItem(
							"JobTemplateDefaults",
							RawDataRegistry.Categories.Transport_TransportBookings,
							ResString.GetMultilingualString("b79d4960-56c5-418e-b4e5-f75bca57fb16", "Job Template Defaults"),
							JobTemplateDefaultHint,
							RegistryStorageFlags.System,
							JobTemplateDefaultCollection.GetDefault());
					});

				return registryItem;
			}
		}

		static MultilingualString JobTemplateDefaultHint
		{
			get
			{
				return ResString.GetMultilingualString("dcb7ae3b-d5ce-46aa-a270-a405b36a92d2", "When a Transport Booking is created from a Job, the Transport Booking Template to default will be based on various values. These values define the template chosen and can be seen below.");
			}
		}

		#endregion

		#region TransportBookingNumberFormat

		public BillCustomisationByServiceLevelRegistryItem TransportBookingNumberFormat
		{
			get
			{
				return GetItem("TransportBookingNumberFormat", delegate
				{
					var dataType = new BillCustomisationByServiceLevelRegistryDataType();
					dataType.FountainPrefix = "TB";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("50fe52a1-asdf-46c9-8d43-0186bd03182e", "Transport Booking");
					dataType.SequenceNumberName = ResString.GetMultilingualString("50fe52a1-asdf-46c9-8d43-0186bd03182e", "Transport Booking");
					dataType.MaxLength = 20;
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Domestic;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationByServiceLevelRegistryItem(
						"TransportBookingNumberFormat",
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("195e0d66-ad5e-asdf-b09c-bd5a23882e74", "Transport Booking Number Format"),
						ResString.GetMultilingualString("cbc3cb2a-dfde-asdf-898b-d3eb4a26342f", "Override this value to customize how Transport Booking numbers are formatted"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						dataType
						);
				});
			}
		}

		#endregion

		#region TransportBookingView

		public CodePairRegistryItem DefaultTransportBookingTabView
		{
			get
			{
				return GetItem("DefaultTransportBookingTabView", delegate
				{
					return new CodePairRegistryItem(
						"DefaultTransportBookingTabView",
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("79d1e82a-0f5f-4fdb-bce7-d668775ebdad", "Default Transport Booking Tab View"),
						ResString.GetMultilingualString("e21d7cbc-a626-4af4-af16-14f1a19079c8", "Select a default view when opening a Transport Booking."),
						BookingViewsListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						BookingViews.Codes.Standard);
				});
			}
		}

		ICodeDescriptionPairListProvider BookingViewsListProvider
		{
			get { return bookingViewsListProvider ?? (bookingViewsListProvider = new CodeDescriptionPairListProvider(() => new BookingViews())); }
		}
		ICodeDescriptionPairListProvider bookingViewsListProvider;

		#endregion

		#region TransportBookingDefaultTransportCompany

		public GuidRegistryItem TransportBookingDefaultTransportCompany
		{
			get
			{
				return GetItem("TransportBookingDefaultTransportCompany", delegate
				{
					return new GuidRegistryItem(
						"TransportBookingDefaultTransportCompany",
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("1b7185a0-0471-46e2-afb4-d640a3231421", "Default Transport Company"),
						ResString.GetMultilingualString("6275cdda-2723-4e74-88a3-1240a5cbb10b", "The transport company that will be automatically set on new Transport Bookings."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch)
					{ EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ShippingProvider) };
				});
			}
		}

		#endregion

		public StringRegistryItem TestCbaId
		{
			get
			{
				return GetItem(nameof(TestCbaId), delegate
				{
					return new StringRegistryItem("TestCbaId",
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("42e3d60f-8e6a-416a-942c-8f454001c8db", "Test Carrier Booking Agent ID"),
						ResString.GetMultilingualString("3688e1fe-87c8-456e-82b2-034bf5cf00f1", "Set a test eHub Client ID to be treated as an Authorized Carrier Booking Agent to enable testing of update of Transport Booking by an XML message from an Authorized Carrier Booking Agent."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty);
				});
			}
		}

		public BooleanRegistryItem MasterBookingsEnabled
		{
			get
			{
				return GetItem(nameof(MasterBookingsEnabled), delegate
				{
					return new BooleanRegistryItem(nameof(MasterBookingsEnabled),
						RawDataRegistry.Categories.Transport_TransportBookings,
						ResString.GetMultilingualString("5ecbfef2-637e-4ccd-b304-ec1e5bbebc65", "Enable Master Bookings"),
						ResString.GetMultilingualString("207f835c-fd06-4474-89f4-9166728e52d8", "Turn this on to enable Master Bookings feature"),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Land & Port Transport

		#region EnableLandTransport

		public BooleanRegistryItem EnableLandTransport
		{
			get
			{
				return GetItem("EnableLandTransport", () => new BooleanRegistryItem(
					"EnableLandTransport",
					RawDataRegistry.Categories.Transport_LandAndPortTransport,
					ResString.GetMultilingualString("49160011-e6bf-4fee-b4b6-c6ee3b3b9f69", "Enable Land Transport"),
					ResString.GetMultilingualString("49160011-e6bf-4fee-b4b6-c6ee3b3b9f69", "Enable Land Transport"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion

		#region DriversGroup

		public IRegistryItem TransportDriversGroup
		{
			get
			{
				return GetItem("LocalCartageDriversGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"LocalCartageDriversGroup",
						RawDataRegistry.Categories.Transport_LandAndPortTransport,
						ResString.GetMultilingualString("2dbe9aa1-a0d0-4b4c-9ff6-1354f862a679", "Transport Drivers"), ResString.GetMultilingualString("1c8e58ff-9d7a-43f9-bca7-d182454b3ea7", "Group of Transport Drivers"),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);//RegistryEditorType.ZGuidFindBox;
					return result;
				});
			}
		}

		#endregion

		#region LandTransportJobServices

		public CodeDescriptionPairListWithDefaultCodeRegistryItem LandTransportJobServices
		{
			get
			{
				return GetItem("LandTransportJobServices", delegate
				{
					var defaultList = (CodeDescriptionPairList)Activator.CreateInstance(ObjectFactory.GetType<IFreightServiceTypes>());
					defaultList.DefaultCode = defaultList[0].Code; // we don't use the default, this is just to get rid of the error

					var item = new CodeDescriptionPairListWithDefaultCodeRegistryItem
					(
						"LandTransportJobServices",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
						ResString.GetMultilingualString("2c927609-23d4-4fc0-aad7-9c01cfde9449", "Job Services"),
						ResString.GetMultilingualString("9780b7f0-cbca-4bff-93e2-040d1992b532", "Services that can be performed on a Land Transport Job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultList,
						false
					);

					return item;
				});
			}
		}

		#endregion

		#region Port Transport

		#region Amount of Free Waiting Time

		public FreeWaitingTimeRegistryItem AmountOfFreeWaitingTime
		{
			get
			{
				return GetItem("LocalCartageAmountOfFreeWaitingTime", delegate
				{
					return new FreeWaitingTimeRegistryItem(
						"LocalCartageAmountOfFreeWaitingTime",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("01d42d78-acf2-4812-a479-d9eb035d2449", "Amount of Free Waiting Time"),
						ResString.GetMultilingualString("8773ac1b-c4cd-4e6f-ac63-4789ccd98774", "This is the amount of free waiting time for Port Transport."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, new FreeWaitingTimeCollection());
				});
			}
		}

		#endregion

		#region UseCumulativeFreeWaitingTime

		public BooleanRegistryItem UseCumulativeFreeWaitingTime
		{
			get
			{
				return GetItem("UseCumulativeFreeWaitingTime", delegate
				{
					return new BooleanRegistryItem(
					"UseCumulativeFreeWaitingTime",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
					ResString.GetMultilingualString("eb907571-80b9-abcd-b49e-957e2f779886", "Use Cumulative Free Waiting Time"),
					ResString.GetMultilingualString("bac1a06f-023f-abcd-b5c6-a79f38a1ab80", "When enabled, system will accumulate free waiting time for each address for use in the next.\r\nThis only applies to the Registry fall back and does not affect Free Waiting from addresses."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false);
				});
			}
		}

		#endregion

		#region Default RunSheet Opening Mode

		public CodePairRegistryItem DefaultRunSheetDay
		{
			get
			{
				return GetItem("DefaultRunSheetDay", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(Constants.RunSheetNewModes.RS0_Today, ResString.GetMultilingualString("a375420f-536a-4a5e-8410-b883d1c1b8fd", "Today"));
						list.AddPair(Constants.RunSheetNewModes.RS1_Tomorrow, ResString.GetMultilingualString("4cf4bb1a-44d9-46e0-b1c9-39fdf6ffda1e", "Tomorrow"));
						list.AddPair(Constants.RunSheetNewModes.RS2, ResString.GetMultilingualString("488b7c82-94e3-474a-a087-347b7b67bc05", "2 Days Ahead"));
						list.AddPair(Constants.RunSheetNewModes.RS3, ResString.GetMultilingualString("67a1f437-4023-42bc-b1b4-018c72c3fd13", "3 Days Ahead"));
						list.AddPair(Constants.RunSheetNewModes.RS4, ResString.GetMultilingualString("87e62de0-19da-4a5e-8c46-44a360ac262a", "4 Days Ahead"));
						list.AddPair(Constants.RunSheetNewModes.RS5, ResString.GetMultilingualString("183a806e-2dc4-4e16-a8ab-7f6db3adc3ed", "5 Days Ahead"));
						list.AddPair(Constants.RunSheetNewModes.RS6, ResString.GetMultilingualString("153ca754-5115-4d68-8d73-c6bb9cad8814", "6 Days Ahead"));
						return list;
					});

					return new CodePairRegistryItem(
						"DefaultRunSheetDay",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("a521c56d-fd55-4bde-9b5e-1372dc89efbf", "Default Run Sheet Day"),
						ResString.GetMultilingualString("ef508134-36f7-41e3-80cf-91dd98106335", "The default day for New Run Sheets."),
						listProvider,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.RunSheetNewModes.RS0_Today);
				});
			}
		}

		#endregion

		#region Job Types

		internal LinkRegistryItem JobTypes
		{
			get
			{
				return GetNonCachedItem(delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("e639c436-3f94-4dc7-8f5e-999def0c6a47", "{0} provides many system defined Job Types that can assist you with making Port Transport jobs. Note that deleting a system defined job type is not allowed. Also editing a system defined job type is partially possible, for example you can hide them or change something on the affiliated legs.\r\nMore importantly, you can define your own job type here and use it on the Port Transport form.", Core.Constants.ProductName);
					return new LinkRegistryItem(RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport, ResString.GetMultilingualString("e2318772-29da-4fe7-a2b6-cbc46c1efe0a", "Job Types"), hint, ModuleIDs.CartageType);
				});
			}
		}

		#endregion

		#region PortTransportJobServices

		public CodeDescriptionPairListWithDefaultCodeRegistryItem PortTransportJobServices
		{
			get
			{
				return GetItem("LocalCartageJobServices", delegate
				{
					var defaultList = (CodeDescriptionPairList)Activator.CreateInstance(ObjectFactory.GetType<IFreightServiceTypes>());
					defaultList.DefaultCode = defaultList[0].Code; // we don't use the default, this is just to get rid of the error

					var item = new CodeDescriptionPairListWithDefaultCodeRegistryItem
					(
						"LocalCartageJobServices",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("8F2ACDDB-7FA0-4427-A1AF-130F7A35D203", "Job Services"),
						ResString.GetMultilingualString("0640420F-8EE9-45EE-82F3-72130016D71C", "Services that can be performed on a Port Transport Job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultList,
						false
					);

					return item;
				});
			}
		}

		#endregion

		#region Equipment TruckSafe

		public StringRegistryItem EquipmentTruckSafe
		{
			get
			{
				return GetItem("LocalCartageEquipmentTruckSafe", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"LocalCartageEquipmentTruckSafe",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("E799117F-CF57-4aef-B8ED-AEF6BF0A33F2", "TruckSafe"),
						ResString.GetMultilingualString("741cf1cc-8352-4374-bc89-e43c591a90d6", "TruckSafe Reference No."),
						RegistryStorageFlags.Company,
						RegistryOptions.NotCached | RegistryOptions.PreserveTestValue
						);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return result;
				});
			}
		}

		#endregion

		#region Generate Slot Booking

		public static class GenerateSlotBookingCodes
		{
			public const string Off = "OFF";
			public const string Verify = "VRF";
			public const string Auto = "AUT";
		}

		public static class GenerateSlotBookingDescriptions
		{
			public static MultilingualString Off
			{
				get { return ResString.GetMultilingualString("9c6b8dd3-7fdc-4047-a0ab-59dded22388d", "Do not generate"); }
			}
			public static MultilingualString Verify
			{
				get { return ResString.GetMultilingualString("1a7d2804-f417-4e68-9607-bc60978739c2", "Verify with the user"); }
			}
			public static MultilingualString Auto
			{
				get { return ResString.GetMultilingualString("80602a5b-0e2f-4a13-939f-968eec17891d", "Fully Automated"); }
			}
		}

		public CodePairRegistryItem GenerateSlotBookingOption
		{
			get
			{
				return GetItem("TimeSlotConfirmationAutoSendOption", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookUpList = new CodeDescriptionPairList();
						lookUpList.AddPair(GenerateSlotBookingCodes.Off, GenerateSlotBookingDescriptions.Off);
						lookUpList.AddPair(GenerateSlotBookingCodes.Verify, GenerateSlotBookingDescriptions.Verify);
						lookUpList.AddPair(GenerateSlotBookingCodes.Auto, GenerateSlotBookingDescriptions.Auto);
						return lookUpList;
					});

					CodePairRegistryItem regItem = new CodePairRegistryItem(
						"TimeSlotConfirmationAutoSendOption",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("5a7dc988-c8bc-4422-b1eb-f38ce83ebce1", "Time Slot Confirmation Auto-Send Option"),
						ResString.GetMultilingualString("c88df7be-d352-4f2f-889d-f102b1cf848e", @"Select one of the options for generating the slot booking document after a slot date or reference is changed;

Off:     Do not auto-send.
Verify:  Verify with the user before sending the document.
Auto:    Fully automated - Send if Transport Contact found upon date reference entry or amendment."),
						lookUpListProvider,
						RegistryStorageFlags.System,
						GenerateSlotBookingCodes.Auto);
					return regItem;
				});
			}
		}

		#endregion

		#region Auto populate demurrage

		public BooleanRegistryItem AutoPopulateDemurrage
		{
			get
			{
				return GetItem("AutoPopulateDemurrage", delegate
				{
					return new BooleanRegistryItem(
					"AutoPopulateDemurrage",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
					ResString.GetMultilingualString("eb907571-80b9-4698-b49e-957e2f779886", "Auto Populate Waiting Time Charge"),
					ResString.GetMultilingualString("bac1a06f-023f-45f2-b5c6-a79f38a1ab80", "When enabled, system will auto populate waiting time charge when time in / time out values are entered."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region Prompt to create Local Transport Job

		public BooleanRegistryItem PromptToCreateLocalTransportJob
		{
			get
			{
				return GetItem("PromptToCreateLocalTransportJob", delegate
				{
					return new BooleanRegistryItem(
					"PromptToCreateLocalTransportJob",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
					ResString.GetMultilingualString("92a7556c-bbd6-4eae-849c-65271b4c8ad1", "Prompt To Create Port Transport Job"),
					ResString.GetMultilingualString("db034ad7-f32c-41c9-a0f6-64c437cc0b7d", "When enabled, the user will be prompted to create a Port Transport job from a related operational job like a Forwarding or CFS job if the Local Transport company specified is an Organization Proxy for any company in the system."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region Show Port Transport Job On Creation

		public BooleanRegistryItem ShowPortTransportJobOnCreation
		{
			get
			{
				return GetItem("ShowPortTransportJobOnCreation", delegate
				{
					return new BooleanRegistryItem(
					"ShowPortTransportJobOnCreation",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
					ResString.GetMultilingualString("CF295C85-65FA-4841-A6D6-6305D99432C1", "Show Port Transport job on creation"),
					ResString.GetMultilingualString("3156C553-8FDC-4C4C-8B4A-652C5D87D767", "This registry setting will determine if the Port Transport job information opens to display it to a user."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region LocalTransportMobileSettingsPassword

		public IRegistryItem LocalTransportMobileSettingsPassword
		{
			get
			{
				return GetItem("LocalTransportMobileSettingsPassword", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"LocalTransportMobileSettingsPassword",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("A7C10E06-6879-4655-B8D2-80E2081621AC", "Port Transport Mobile Settings Password"), ResString.GetMultilingualString("1AA03BDF-5059-46A4-BB4F-C3999093D5E1", "Port Transport Mobile Settings Password."),
						RegistryStorageFlags.All, RegistryOptions.IsPasswordVisibleForControllerUser, "");
					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return item;
				});
			}
		}

		#endregion

		#region ShowLegSignatures

		public BooleanRegistryItem ShowLegSignatures
		{
			get
			{
				return GetItem("ShowLegSignatures", delegate
				{
					return new BooleanRegistryItem(
						"ShowLegSignatures",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						ResString.GetMultilingualString("e96e87a3-67b7-4b67-b70e-768e8b924ba7", "Show Leg Signatures"),
						ResString.GetMultilingualString("35E805A4-78FE-4AFE-8D5B-D43DB7CB45E9", "Specify whether the signatures should be shown on leg form."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region ShowSequence

		public BooleanRegistryItem ShowSequence
		{
			get
			{
				return GetItem("ShowSequence", delegate
				{
					return new BooleanRegistryItem(
						"ShowSequence",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
						(NoResString)"Show Sequence",
						(NoResString)"Specify whether the sequencing functionality should be shown to the users.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Equipment

		public BooleanRegistryItem EnableEquipmentCombination
		{
			get
			{
				return GetItem("EnableEquipmentCombination", () => new BooleanRegistryItem(
					"EnableEquipmentCombination",
					RawDataRegistry.Categories.Transport_Equipment,
					(NoResString)"Enable Equipment Combination",
					(NoResString)"Enable Equipment Combination",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion
	}
}
