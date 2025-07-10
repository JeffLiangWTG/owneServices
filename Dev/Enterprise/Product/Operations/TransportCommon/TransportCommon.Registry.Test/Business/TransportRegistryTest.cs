using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(TransportRegistry))]
	class TransportRegistryTest : RegistryItemSetTestCaseWithFactory<TransportRegistry>
	{
		#region CommonTransport

		#region TestAuthorityToLeave

		public void TestAuthorityToLeave()
		{
			TestGenericRegistryItem
			(
				ItemSet.AuthorityToLeave,
				"AuthorityToLeave",
				RawDataRegistry.Categories.Transport,
				"Authority To Leave Default",
				"The default value for Authority To Leave.",
				RegistryStorageFlags.System, false
			);
		}

		#endregion
		#endregion

		#region CarrierMessagingBuss

		public void TestEnableBookingWithCarrierMessagingBuss()
		{
			TestGenericRegistryItem
			(
				ItemSet.EnableBookingWithCarrierMessagingBuss,
				"EnableBookingWithCarrierMessagingBuss",
				RawDataRegistry.Categories.Transport_CarrierMessagingBuss,
				"Enable Booking with Carrier Messaging Buss",
				"Enable Booking with Carrier Messaging Buss",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestPackageVersionOverride()
		{
			TestGenericRegistryItem
			(
				ItemSet.PackageVersionOverride,
				"PackageVersionOverride",
				RawDataRegistry.Categories.Transport_CarrierMessagingBuss,
				"Package Version Override",
				"Override Package Version based on Carrier Code and Account Number",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport
			);

			AssertContainsExactElementsInAnyOrder(new PackageVersionOverrideCollection(),
				ItemSet.PackageVersionOverride.DefaultValue);
		}

		public void TestCarrierMessagingBussServerAddress()
		{
			TestGenericRegistryItem(
				ItemSet.CarrierMessagingBussServerAddress,
				"CarrierMessagingBussServerAddress",
				RawDataRegistry.Categories.Transport_CarrierMessagingBuss,
				"Carrier Messaging Buss Server Address",
				"This is the internal address used to access the CarrierMessagingBuss Server.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"https://cmb.wisegrid.net/"
			);
		}

		#endregion

		#region Transport Bookings

		#region TestAdditionalReferenceNumbers

		public void TestAdditionalReferenceNumbers()
		{
			var internals = ItemSet.AdditionalReferenceNumbers;

			TestGenericRegistryItem(internals,
			"TransportBookingsAdditionalReferenceNumbers",
			"Transport/Transport Bookings",
			"Additional Reference Types",
			"This list defines the additional reference number types available under the Additional References tab.",
			RegistryStorageFlags.System);

			AssertContainsExactElementsInAnyOrder(
				"Should have correct additional reference number types",
				new[]
				{
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"ETB|External Transport Booking Number|Y",
					"UCR|External (3rd Party) Unique Consignment Reference|Y",
					"CLN|Client|Y",
					"CLR|Customer Reference Number|Y",
					"ORD|Order Number|N",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"BPR|Booking Party Reference|Y",
					"CBK|Carrier Booking Reference|Y",
				},
				internals.DefaultValue
				.Cast<TransportReferenceNumberType>()
				.Where((number) => number.CodeInfo.ReadOnly && !((ICanDelete)number).CanDelete)
				.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		#endregion

		public void TestTestCbaId()
		{
			TestStringRegistryItem(TransportRegistry.Instance.TestCbaId,
				nameof(TransportRegistry.Instance.TestCbaId),
				RawDataRegistry.Categories.Transport_TransportBookings,
				"Test Carrier Booking Agent ID",
				"Set a test eHub Client ID to be treated as an Authorized Carrier Booking Agent to enable testing of update of Transport Booking by an XML message from an Authorized Carrier Booking Agent.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				string.Empty,
				CharacterCase.Normal);
		}

		#region TestAutoCreateTransportConsignmentsCreationPeriod

		public void TestAutoCreateTransportConsignmentsCreationPeriod()
		{
			TestRegistryItem(TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod,
				"AutoCreateTransportConsignmentsCreationPeriod",
				RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
				"Period before which to Auto-Create Transport Jobs",
				"The Transport Jobs Service Task automatically creates Transport Consignments or Port Transport jobs from received Transport Bookings. Specify in hours the period before the earliest Pick Up Date of the booking that the automatic creation of the job will occur." +
				"\r\n\r\nFor example, if this value is set to 24, Transport Jobs will be auto-created for Transport Bookings where the Pickup Date is within 24 hrs.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TransportRegistry.AutoCreateTransportConsignmentsCreationPeriodDefaultValue, TransportRegistry.AutoCreateTransportConsignmentsCreationPeriodMinValue, TransportRegistry.AutoCreateTransportConsignmentsCreationPeriodMaxValue);
		}

		#endregion

		#region TestAutoCreateTransportConsignmentsGracePeriod

		public void TestAutoCreateTransportConsignmentsGracePeriod()
		{
			TestRegistryItem(TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod,
				"AutoCreateTransportConsignmentsGracePeriod",
				RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
				"Minimum Booking Age after which to Auto-Create Transport Jobs",
				"Specify the minimum delay (in minutes) after the last Edit on a Transport Booking, before that Booking can create a Consignment or Port Transport Job via the Transport Booking to Port/Land Transport Jobs Service Task.",
				RegistryStorageFlags.System,
				30);

			var dataType = (IntRegistryDataType)TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.DataType;
			AssertEquals(1d, dataType.LowerBound);
			AssertEquals((double)int.MaxValue, dataType.UpperBound);
		}

		#endregion

		#region TestCreateTransportJobsFromTransportBookingEffectiveDate

		public void TestCreateTransportJobsFromTransportBookingEffectiveDate()
		{
			TestGenericRegistryItem(TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate,
				"AutoCreateTransportConsignmentsFromDate",
				RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
				"Create Transport Jobs from Transport Booking Effective Date",
				"This setting is used to define the start date used to auto create Port Transport Jobs, or Land Transport Consignments from a Transport Bookings via the TBC Service Task.",
				RegistryStorageFlags.System,
				DateTime.MinValue);
		}

		#endregion

		#region TestCreateTransportJobsTransportBookingWithinPeriod

		public void TestCreateTransportJobsTransportBookingWithinPeriod()
		{
			TestRegistryItem(TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod,
				"CreateTransportJobsFromTransportBookingWithinPeriod",
				RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
				"Create Transport Jobs from Transport Booking within period",
				"This setting is used to define the age of the Transport Booking. When the number of days is set to 30 the TBC Service Task will consider Transport Bookings that have been created within the last 30 days. Any Bookings older than this will not be used. (Maximum value is 90 days, Minimum value is 0 day)",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				30,
				0,
				90);
		}

		#endregion

		#region TestServiceTaskCreatorOption

		public void TestServiceTaskCreatorOption()
		{
			// Consider removing ConditionallyVisibleRegistryItems if this is ever changed to always be visible
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(TransportRegistry.Instance.ServiceTaskCreatorOption,
					"ServiceTaskCreatorOption",
					RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
					"Service Task Target Options",
					"Valid Transport Bookings will be sent to the selected Module based on the Freight Mode.",
					RegistryStorageFlags.System, RegistryOptions.Default);
			}

			RegistryItemDictionary.Instance.PurgeAll(); // Clear Cache
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestGenericRegistryItem(TransportRegistry.Instance.ServiceTaskCreatorOption,
					"ServiceTaskCreatorOption",
					RawDataRegistry.Categories.Transport_TransportBookings_ServiceTask,
					"Service Task Target Options",
					"Valid Transport Bookings will be sent to the selected Module based on the Freight Mode.",
					RegistryStorageFlags.System, RegistryOptions.IsHidden);
			}
		}

		#endregion

		#region TestDefaultTransportBookingTabView

		public void TestDefaultTransportBookingTabView()
		{
			TestRegistryItem(
				TransportRegistry.Instance.DefaultTransportBookingTabView,
				"DefaultTransportBookingTabView",
				RawDataRegistry.Categories.Transport_TransportBookings,
				"Default Transport Booking Tab View",
				"Select a default view when opening a Transport Booking.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new BookingViews(),
				BookingViews.Codes.Standard);
		}

		#endregion

		#region TestOrganisationRTUSOptions

		public void TestOrganisationRTUSOptions()
		{
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(TransportRegistry.Instance.OrganisationRTUSOptions,
					"OrganisationRTUSOptions",
					RawDataRegistry.Categories.Transport_RTUS,
					"Organization RTUS Options",
					"Map organizations to a CBA.",
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region TestOrganisationRTUSWebProxy

		public void TestOrganisationRTUSWebProxy()
		{
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestStringRegistryItem(TransportRegistry.Instance.OrganisationRTUSWebProxy,
					"OrganisationRTUSWebProxy",
					RawDataRegistry.Categories.Transport_RTUS,
					"RTUS Web Proxy",
					"Assign web proxy address for RTUS web service.",
					RegistryStorageFlags.System,
					TextEditorType.TextBox,
					RegistryOptions.PreserveTestValue,
					string.Empty,
					CharacterCase.Normal,
					"http://test");
			}
		}

		public void TestOrganisationRTUSWebProxy_HTTPS()
		{
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertExceptionThrown(typeof(RegistryValidationException), () => RegistryTester.SetValue(TransportRegistry.Instance.OrganisationRTUSWebProxy, "https://test"));
			}
		}

		#endregion

		#region TestOrganisationRTUSWebProxyCredentials

		public void TestOrganisationRTUSWebProxyUsername()
		{
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(TransportRegistry.Instance.OrganisationRTUSWebProxyUsername,
					"OrganisationRTUSWebProxyUsername",
					RawDataRegistry.Categories.Transport_RTUS,
					"RTUS Web Proxy Username",
					"Enter the RTUS web service proxy username, if applicable.",
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue);
			}
		}

		public void TestOrganisationRTUSWebProxyPassword()
		{
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(TransportRegistry.Instance.OrganisationRTUSWebProxyPassword,
					"OrganisationRTUSWebProxyPassword",
					RawDataRegistry.Categories.Transport_RTUS,
					"RTUS Web Proxy Password",
					"Enter the RTUS web service proxy password, if applicable.",
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region TestRemotePrintServerURL

		public void TestRemotePrintServerURL()
		{
			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestStringRegistryItem(TransportRegistry.Instance.RemotePrintServerURL,
					"RemotePrintServerURL",
					RawDataRegistry.Categories.Transport_RTUS,
					"Remote Print Server URL for RTUS",
					"Input the print web service URL for remote printing.",
					RegistryStorageFlags.System,
					TextEditorType.TextBox,
					RegistryOptions.PreserveTestValue,
					string.Empty,
					CharacterCase.Normal);
			}
		}

		#endregion

		#region TestUseStandardRemotePrintingForRTUS

		public void TestUseStandardRemotePrintingForRTUS()
		{
			TestRegistryItem(TransportRegistry.Instance.UseStandardRemotePrintingForRTUS,
				"UseStandardRemotePrintingForRTUS",
				RawDataRegistry.Categories.Transport_RTUS,
				"Use Standard Remote Printing for RTUS",
				"Enable this to use standard remote printing for RTUS.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				expectedDefaultValue: false);
		}

		#endregion

		#region TestEnableRTUSBatchProcessing

		public void TestEnableRTUSBatchProcessing()
		{
			TestRegistryItem(TransportRegistry.Instance.EnableRTUSBatchProcessing,
				"EnableRTUSBatchProcessing",
				RawDataRegistry.Categories.Transport_RTUS,
				"Enable RTUS Batch Processing",
				"Enable this to allow batch processing of multiple RTUS requests instead of single processing for each RTUS request.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				expectedDefaultValue: false);
		}

		#endregion

		#region TestTransportBookingDefaultTransportCompany

		public void TestTransportBookingDefaultTransportCompany()
		{
			TestRegistryItem(
				TransportRegistry.Instance.TransportBookingDefaultTransportCompany,
				"TransportBookingDefaultTransportCompany",
				RawDataRegistry.Categories.Transport_TransportBookings,
				"Default Transport Company",
				"The transport company that will be automatically set on new Transport Bookings.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryFindBoxCollection.ShippingProvider,
				Guid.Empty);
		}

		#endregion

		#region TestTransportBookingChargeableFactor

		public void TestTransportBookingChargeableFactor()
		{
			var weightChargeableHint = @"The chargeable factor will be used to convert the actual volume into a chargeable weight.

The chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor.

Max of [ weight(KG) , volume(CM3) / factor ] = KG

Default chargeable factor is 3000 CC/KG.

Max of [ weight(LB) , volume(CI) / factor ] = LB

Default chargeable factor is 250 CI/LB.";

			var australianCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			var australianBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			australianCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
			australianBranch[GlbBranchSchema.Constants.GB_GC] = australianCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, australianBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var internals = (IRegistryItemInternals)ItemSet.TransportBookingChargeableFactor;

				TestGenericRegistryItem(internals,
				"TransportBookingChargeableFactor", "Transport/Transport Bookings",
				"Chargeable Factor for Transport Booking", weightChargeableHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new ChargeableFactor(
					new ConversionFactor(3000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
					new ConversionFactor(250m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));
			}
		}

		#endregion

		#region TestTransportBookingNumberFormat

		public void TestTransportBookingNumberFormat()
		{
			AssertEquals(RegistryStorageFlags.System, (ItemSet.TransportBookingNumberFormat.Storage & RegistryStorageFlags.System));
			AssertEquals(RegistryStorageFlags.Company, (ItemSet.TransportBookingNumberFormat.Storage & RegistryStorageFlags.Company));

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.TransportBookingNumberFormat.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			var value = ItemSet.TransportBookingNumberFormat.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Categories", NumberCustomisationElementCategories.Standard, (value.Categories & NumberCustomisationElementCategories.Standard));
			AssertEquals("Value.Categories", NumberCustomisationElementCategories.Domestic, (value.Categories & NumberCustomisationElementCategories.Domestic));
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", false, value.EnableMacroInsertion);
		}

		public void TestTransportBookingNumberFormatIsAddedToRegistry()
		{
			AssertVisible(ItemSet.TransportBookingNumberFormat);
		}

		#endregion

		public void TestMasterBookingsEnabled_AllSettings()
		{
			TestRegistryItem(TransportRegistry.Instance.MasterBookingsEnabled, nameof(TransportRegistry.Instance.MasterBookingsEnabled), RawDataRegistry.Categories.Transport_TransportBookings, "Enable Master Bookings", "Turn this on to enable Master Bookings feature", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		#region ConditionallyVisibleRegistryItems

		protected override System.Collections.Generic.IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return nameof(ItemSet.ServiceTaskCreatorOption);
				yield return nameof(ItemSet.EnableLandTransport);
				yield return nameof(ItemSet.TestCbaId);
			}
		}

		#endregion

		#endregion

		#region Land & Port Transport

		#region TestEnableLandTransport

		public void TestEnableLandTransport()
		{
			AssertEquals("TestEnableLandTransport", false, ItemSet.EnableLandTransport.Value);
		}

		#endregion

		#region TestTransportDriversGroup

		public void TestTransportDriversGroup()
		{
			Guid groupPK = Guid.NewGuid();
			ItemSet.TransportDriversGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupPK);
			AssertEquals("LocalCartageDriversGroup", groupPK, ItemSet.TransportDriversGroup.Value);
		}

		#endregion

		#region TestLandTransportJobServices

		public void TestLandTransportJobServices()
		{
			TestGenericRegistryItem
			(
				ItemSet.LandTransportJobServices,
				"LandTransportJobServices",
				"Transport/Land & Port Transport/Land Transport",
				"Job Services",
				"Services that can be performed on a Land Transport Job.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company
			);

			var defaultList = (CodeDescriptionPairList)ObjectFactory.Get<IFreightServiceTypes>();
			AssertContainsExactElementsInAnyOrder("Ensure we inherit all Domestic Transport Services.", defaultList, ItemSet.LandTransportJobServices.Value);
		}

		#endregion

		#region Port Transport

		#region TestPortTransportJobServices

		public void TestPortTransportJobServices()
		{
			TestGenericRegistryItem
			(
				ItemSet.PortTransportJobServices,
				"LocalCartageJobServices",
				"Transport/Land & Port Transport/Port Transport",
				"Job Services",
				"Services that can be performed on a Port Transport Job.", RegistryStorageFlags.System | RegistryStorageFlags.Company
			);

			var defaultList = (CodeDescriptionPairList)ObjectFactory.Get<IFreightServiceTypes>();
			AssertContainsExactElementsInAnyOrder("Ensure we inherit all freight JobServices.", defaultList, ItemSet.PortTransportJobServices.Value);
		}

		#endregion

		#region TestEquipmentTruckSafe

		public void TestEquipmentTruckSafe()
		{
			TestStringRegistryItem(ItemSet.EquipmentTruckSafe, "LocalCartageEquipmentTruckSafe", "Transport/Land & Port Transport/Port Transport", "TruckSafe", "TruckSafe Reference No.", RegistryStorageFlags.Company, TextEditorType.TextBox, RegistryOptions.NotCached | RegistryOptions.PreserveTestValue, "", CharacterCase.Normal);
		}

		#endregion

		#region TestAutoPopulateDemurrage

		public void TestAutoPopulateDemurrage()
		{
			TestRegistryItem(ItemSet.AutoPopulateDemurrage, "AutoPopulateDemurrage", "Transport/Land & Port Transport/Port Transport", "Auto Populate Waiting Time Charge", "When enabled, system will auto populate waiting time charge when time in / time out values are entered.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, true);
		}

		#endregion

		#region TestPromptToCreateLocalTransportJob

		public void TestPromptToCreateLocalTransportJob()
		{
			TestRegistryItem(ItemSet.PromptToCreateLocalTransportJob, "PromptToCreateLocalTransportJob", "Transport/Land & Port Transport/Port Transport", "Prompt To Create Port Transport Job", "When enabled, the user will be prompted to create a Port Transport job from a related operational job like a Forwarding or CFS job if the Local Transport company specified is an Organization Proxy for any company in the system.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, true);
		}

		#endregion

		#region TestShowPortTransportJobOnCreation

		public void TestShowPortTransportJobOnCreation()
		{
			TestRegistryItem(ItemSet.ShowPortTransportJobOnCreation, "ShowPortTransportJobOnCreation", "Transport/Land & Port Transport/Port Transport", "Show Port Transport job on creation", "This registry setting will determine if the Port Transport job information opens to display it to a user.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, true);
		}

		#endregion

		#region TestUseCumulativeFreeWaitingTime

		public void TestUseCumulativeFreeWaitingTime()
		{
			TestRegistryItem(ItemSet.UseCumulativeFreeWaitingTime, "UseCumulativeFreeWaitingTime", "Transport/Land & Port Transport/Port Transport", "Use Cumulative Free Waiting Time", "When enabled, system will accumulate free waiting time for each address for use in the next.\r\nThis only applies to the Registry fall back and does not affect Free Waiting from addresses.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
		}

		#endregion

		#region TestAmountOfFreeWaitingTime

		public void TestAmountOfFreeWaitingTime()
		{
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = new FreeWaitingTime();
			var cFS = new ZDateTime(2005, 1, 2, 12, 13, 14);
			var cTO = new ZDateTime(2005, 2, 3, 13, 14, 15);
			var containerYard = new ZDateTime(2005, 3, 4, 14, 15, 16);
			var consignee = new ZDateTime(2005, 5, 6, 15, 16, 17);
			var consignor = new ZDateTime(2005, 5, 6, 16, 17, 18);
			var other = new ZDateTime(2005, 7, 8, 17, 18, 19);
			freeWaitingTime.DropMode = Constants.EquipmentNeeded.Any;
			freeWaitingTime.CFS = cFS;
			freeWaitingTime.CTO = cTO;
			freeWaitingTime.CYD = containerYard;
			freeWaitingTime.CNE = consignee;
			freeWaitingTime.CNR = consignor;
			freeWaitingTime.Other = other;
			freeWaitingTimeCollection.Add(freeWaitingTime);
			ItemSet.AmountOfFreeWaitingTime.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, freeWaitingTimeCollection);

			AssertEquals("AmountOfFreeWaitingTime.Value.CFS", cFS, ItemSet.AmountOfFreeWaitingTime.Value[0].CFS);
			AssertEquals("AmountOfFreeWaitingTime.Value.CTO", cTO, ItemSet.AmountOfFreeWaitingTime.Value[0].CTO);
			AssertEquals("AmountOfFreeWaitingTime.Value.ContainerYard", containerYard, ItemSet.AmountOfFreeWaitingTime.Value[0].CYD);
			AssertEquals("AmountOfFreeWaitingTime.Value.Consignee", consignee, ItemSet.AmountOfFreeWaitingTime.Value[0].CNE);
			AssertEquals("AmountOfFreeWaitingTime.Value.Consignor", consignor, ItemSet.AmountOfFreeWaitingTime.Value[0].CNR);
			AssertEquals("AmountOfFreeWaitingTime.Value.Other", other, ItemSet.AmountOfFreeWaitingTime.Value[0].Other);
		}

		#endregion

		#region TestDefaultWorksheetOpeningMode

		public void TestDefaultWorksheetOpeningMode()
		{
			AssertEquals("DefaultValue", Constants.RunSheetNewModes.RS0_Today, ItemSet.DefaultRunSheetDay.DefaultValue);
			ItemSet.DefaultRunSheetDay.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, Constants.RunSheetNewModes.RS1_Tomorrow);
			AssertEquals("Value", Constants.RunSheetNewModes.RS1_Tomorrow, ItemSet.DefaultRunSheetDay.Value);
		}

		#endregion

		#region TestPortTransportJobTypes

		public void TestPortTransportJobTypes()
		{
			TestRegistryItem(ItemSet.JobTypes, "Transport/Land & Port Transport/Port Transport", "Job Types", $"{Core.Constants.ProductName} provides many system defined Job Types that can assist you with making Port Transport jobs. Note that deleting a system defined job type is not allowed. Also editing a system defined job type is partially possible, for example you can hide them or change something on the affiliated legs.\r\nMore importantly, you can define your own job type here and use it on the Port Transport form.", ModuleIDs.CartageType);
		}

		#endregion

		#region SlotBookingDocumentOption

		public void TestGenerateSlotBookingDocumentOption()
		{
			CodeDescriptionPairList expectedLookUpList = new CodeDescriptionPairList();
			expectedLookUpList.AddPair(TransportRegistry.GenerateSlotBookingCodes.Off, TransportRegistry.GenerateSlotBookingDescriptions.Off);
			expectedLookUpList.AddPair(TransportRegistry.GenerateSlotBookingCodes.Verify, TransportRegistry.GenerateSlotBookingDescriptions.Verify);
			expectedLookUpList.AddPair(TransportRegistry.GenerateSlotBookingCodes.Auto, TransportRegistry.GenerateSlotBookingDescriptions.Auto);

			#region Hint

			ZString hint = @"Select one of the options for generating the slot booking document after a slot date or reference is changed;

Off:     Do not auto-send.
Verify:  Verify with the user before sending the document.
Auto:    Fully automated - Send if Transport Contact found upon date reference entry or amendment.";

			#endregion

			AssertEquals("Name", "TimeSlotConfirmationAutoSendOption", ItemSet.GenerateSlotBookingOption.Name);
			AssertEquals("Category", "Transport/Land & Port Transport/Port Transport", ItemSet.GenerateSlotBookingOption.Category);
			AssertEquals("Caption", "Time Slot Confirmation Auto-Send Option", ItemSet.GenerateSlotBookingOption.Caption);
			AssertEquals("Hint", hint, new ZString(ItemSet.GenerateSlotBookingOption.Hint));
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.GenerateSlotBookingOption.Storage);
			AssertEquals("DefaultValue", TransportRegistry.GenerateSlotBookingCodes.Auto, ItemSet.GenerateSlotBookingOption.DefaultValue);

			CodePairRegistryDataType dataType = (CodePairRegistryDataType)ItemSet.GenerateSlotBookingOption.DataType;
			AssertEquals("DataType.LookUpList.Count", expectedLookUpList.Count, dataType.LookUpList.Count);
			foreach (ICodeDescription element in expectedLookUpList)
			{
				AssertEquals("GetDescriptionFromCode(" + element.Code + ")", element.Description, dataType.LookUpList.GetDescriptionFromCode(element.Code));
			}
			RegistryTester.SetValue(ItemSet.GenerateSlotBookingOption, expectedLookUpList[0].Code);
			AssertEquals("Value", expectedLookUpList[0].Code, ItemSet.GenerateSlotBookingOption.Value);
		}

		#endregion

		#region TestLocalTransportMobileSettingsPassword

		public void TestLocalTransportMobileSettingsPassword()
		{
			string pwd = "TestPassword";
			ItemSet.LocalTransportMobileSettingsPassword.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, pwd);
			AssertEquals("LocalTransportMobileSettingsPassword", "TestPassword", ItemSet.LocalTransportMobileSettingsPassword.Value);
			Assert(ItemSet.LocalTransportMobileSettingsPassword.HasOption(RegistryOptions.IsPasswordVisibleForControllerUser));
		}

		#endregion

		#region TestShowLegSignature

		public void TestShowLegSignature()
		{
			AssertEquals("ShowLegSignatures", ItemSet.ShowLegSignatures.Name);
			AssertEquals("Specify whether the signatures should be shown on leg form.", ItemSet.ShowLegSignatures.Hint);
			AssertEquals("Transport/Land & Port Transport/Port Transport", ItemSet.ShowLegSignatures.Category);
			AssertEquals("Show Leg Signatures", ItemSet.ShowLegSignatures.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ShowLegSignatures.Storage);
			AssertEquals(false, ItemSet.ShowLegSignatures.DefaultValue);
		}

		#endregion

		#region TestShowSequence

		public void TestShowSequence()
		{
			TestRegistryItem(
				TransportRegistry.Instance.ShowSequence,
				"ShowSequence",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_PortTransport,
				"Show Sequence",
				"Specify whether the sequencing functionality should be shown to the users.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#endregion

		#endregion

		#region TestITransportRegistry

		public void TestITransportRegistry_RemotePrintServerURL()
		{
			ITransportRegistry registry = TransportRegistry.Instance;
			AssertEquals(TransportRegistry.Instance.RemotePrintServerURL, registry.RemotePrintServerURL);
		}

		public void TestITransportRegistry_OrganisationRTUSOptions()
		{
			ITransportRegistry registry = TransportRegistry.Instance;
			AssertEquals(TransportRegistry.Instance.OrganisationRTUSOptions, registry.OrganisationRTUSOptions);
		}

		public void TestITransportRegistry_OrganisationRTUSWebProxy()
		{
			ITransportRegistry registry = TransportRegistry.Instance;
			AssertEquals(TransportRegistry.Instance.OrganisationRTUSWebProxy, registry.OrganisationRTUSWebProxy);
		}

		public void TestITransportRegistry_GetRTUSOption()
		{
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			ITransportRegistry registry = TransportRegistry.Instance;
			AssertNull(registry.GetRTUSOption(smartFreight.PK.ToGuid()));

			var rtusCollection = new OrganisationRTUSCollection();
			var rtusOption1 = rtusCollection.AddNew();
			var rtusOption2 = rtusCollection.AddNew();
			rtusOption1.CBACode = CBAList.Codes.Transtream;
			rtusOption1.Url = "http://Pierbridge";
			rtusOption1.OrganisationPK = ZGuid.NewZGuid();

			rtusOption2.OrganisationPK = smartFreight.PK;
			rtusOption2.Url = "http://SmartFreight";

			using (TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection))
			{
				var rtusOptionFromInterface = registry.GetRTUSOption(smartFreight.PK.ToGuid());
				AssertEquals(new Uri("http://SmartFreight"), rtusOptionFromInterface.WrappedUrl);
			}
		}

		public void TestITransportRegistry_OrganisationRTUSWebProxyCredentials()
		{
			ITransportRegistry registry = TransportRegistry.Instance;
			var credentials = registry.OrganisationRTUSWebProxyCredentials;
			AssertEquals("", credentials.Password);
			AssertEquals("", credentials.ServerName);
			AssertEquals("", credentials.UserName);
			AssertEquals("", credentials.UserNameWithoutServer);

			using (TransportRegistry.Instance.OrganisationRTUSWebProxyUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CORP\\John.Smith"))
			using (TransportRegistry.Instance.OrganisationRTUSWebProxyPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test123"))
			{
				var newCredentials = registry.OrganisationRTUSWebProxyCredentials;
				AssertEquals("test123", newCredentials.Password);
				AssertEquals("CORP", newCredentials.ServerName);
				AssertEquals("CORP\\John.Smith", newCredentials.UserName);
				AssertEquals("John.Smith", newCredentials.UserNameWithoutServer);
			}
		}

		public void TestITransportRegistry_TestCBAId()
		{
			ITransportRegistry registry = TransportRegistry.Instance;
			AssertEquals(TransportRegistry.Instance.TestCbaId, registry.TestCbaId);
		}

		#endregion

		#region Equipment

		public void TestEnableEquipmentCombination()
		{
			TestGenericRegistryItem
			(
				ItemSet.EnableEquipmentCombination,
				"EnableEquipmentCombination",
				RawDataRegistry.Categories.Transport_Equipment,
				"Enable Equipment Combination",
				"Enable Equipment Combination",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		#endregion
	}
}
