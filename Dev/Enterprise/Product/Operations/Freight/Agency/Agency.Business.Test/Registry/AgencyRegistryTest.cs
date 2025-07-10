using System;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RegReleaseType = Enterprise.Registry.Business.ReleaseType;
using RegReleaseTypes = Enterprise.Registry.Business.ReleaseTypes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyRegistry))]
	internal class AgencyRegistryTest : RegistryItemSetTestCaseWithFactory<AgencyRegistry>
	{
		public void TestReplaceExistingContractNumbersWithNewNumbers()
		{
			TestGenericRegistryItem(ItemSet.ReplaceExistingContractNumbersWithNewNumbers, "AgencyReplaceExistingContractNumbersWithNewNumbers", "Liner & Agency/Carrier Contract Numbers", "Replace existing contract numbers with new numbers.", "When this registry is set to Yes, the previously entered/auto-populated contract numbers will be deleted and new contract numbers will be populated.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestPopulateContractNumbersFromRates()
		{
			TestGenericRegistryItem(ItemSet.PopulateContractNumbersFromRevenueRates, "AgencyPopulateContractNumbersFromRevenueRates", "Liner & Agency/Carrier Contract Numbers", "Populate contract numbers from revenue rates.", "When enabled, the carrier contract numbers will be added to bills of lading and bookings during revenue rating.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestCMMeHubNotificationGroup()
		{
			TestGenericRegistryItem(ItemSet.CMMeHubNotificationGroup, "AgencyCMMeHubNotificationGroup", "Notification/Liner & Agency/Periodic Container Movement Exporter", "Export Error Notification Group", "Members of this notification group will receive emails if errors are encountered while trying to export container movements to eHub.", RegistryStorageFlags.System, RegistryOptions.IsValueOptional, Constants.Groups.AllPK);
		}

		public void TestCMMeHubCutOff()
		{
			TestGenericRegistryItem(ItemSet.CMMeHubCutOff, "AgencyCMMeHubCutOff", "Liner & Agency/Port Messaging/Container Management Messaging", "Auto Movement Export Cut Off", "If entered, then movements that were added before this date will not generate messages. This date should be entered in UTC.", RegistryStorageFlags.System, RegistryOptions.Default, DateTime.MinValue);
		}

		public void TestCMMeHubHighWaterMark()
		{
			TestGenericRegistryItem(ItemSet.CMMeHubHighWaterMark, "CMMeHubHighWaterMark", "Liner & Agency/Port Messaging/Container Management Messaging", "Auto Movement Export High-Water Mark", "Auto Movement Export High-Water Mark", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached, DateTime.MinValue);
		}

		public void TestConvertToDefaultUnitsOnConfirmation()
		{
			TestGenericRegistryItem(ItemSet.ConvertToDefaultUnitsOnConfirmation, "AgencyConvertToDefaultUnitsOnConfirmation", "Liner & Agency/Default Units", "Convert Weight and Volume Units on Booking Confirmation", "If enabled, the weight and volume units will be converted to the default units for a bill of lading when a booking is confirmed.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, false);
		}

		public void TestDefaultBillWeightUnit()
		{
			TestGenericRegistryItem(ItemSet.DefaultBillWeightUnit, "AgencyDefaultWeightUnit", "Liner & Agency/Default Units", "Default Weight Unit for Bills of Lading", "The default unit of weight to use for Liner & Agency bills of lading.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
			AssertEquals(Constants.Weight.Kilograms, ItemSet.DefaultBillWeightUnit.Value);
			ItemSet.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AssertEquals(Constants.Weight.Tonnes, ItemSet.DefaultBillWeightUnit.Value);
		}

		public void TestDefaultBillVolumeUnit()
		{
			TestGenericRegistryItem(ItemSet.DefaultBillVolumeUnit, "AgencyDefaultVolumeUnit", "Liner & Agency/Default Units", "Default Volume Unit for Bills of Lading", "The default unit of volume to use for Liner & Agency bills of lading.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
			AssertEquals(Constants.Volume.CubicMetres, ItemSet.DefaultBillVolumeUnit.Value);
			ItemSet.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.MegaLitre);
			AssertEquals(Constants.Volume.MegaLitre, ItemSet.DefaultBillVolumeUnit.Value);
		}

		public void TestDefaultBillDimensionUnit()
		{
			TestGenericRegistryItem(ItemSet.DefaultBillDimensionUnit, "AgencyDefaultDimensionUnit", "Liner & Agency/Default Units", "Default Dimension Unit for Bills of Lading", "The default unit of dimension to use for Liner & Agency bills of lading.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
			AssertEquals(Constants.Length.Metres, ItemSet.DefaultBillDimensionUnit.Value);
			ItemSet.DefaultBillDimensionUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Feet);
			AssertEquals(Constants.Length.Feet, ItemSet.DefaultBillDimensionUnit.Value);
		}

		public void TestDefaultBookingWeightUnit()
		{
			TestGenericRegistryItem(ItemSet.DefaultBookingWeightUnit, "AgencyDefaultBookingWeightUnit", "Liner & Agency/Default Units", "Default Weight Unit for Bookings", "The default unit of weight to use for Liner & Agency bookings.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
			AssertEquals(Constants.Weight.Kilograms, ItemSet.DefaultBookingWeightUnit.Value);
			ItemSet.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AssertEquals(Constants.Weight.Tonnes, ItemSet.DefaultBookingWeightUnit.Value);
		}

		public void TestDefaultBookingVolumeUnit()
		{
			TestGenericRegistryItem(ItemSet.DefaultBookingVolumeUnit, "AgencyDefaultBookingVolumeUnit", "Liner & Agency/Default Units", "Default Volume Unit for Bookings", "The default unit of volume to use for Liner & Agency bookings.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
			AssertEquals(Constants.Volume.CubicMetres, ItemSet.DefaultBookingVolumeUnit.Value);
			ItemSet.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.MegaLitre);
			AssertEquals(Constants.Volume.MegaLitre, ItemSet.DefaultBookingVolumeUnit.Value);
		}

		public void TestDefaultBookingDimensionUnit()
		{
			TestGenericRegistryItem(ItemSet.DefaultBookingDimensionUnit, "AgencyDefaultBookingDimensionUnit", "Liner & Agency/Default Units", "Default Dimension Unit for Bookings", "The default unit of dimension to use for Liner & Agency bookings.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default);
			AssertEquals(Constants.Length.Metres, ItemSet.DefaultBookingDimensionUnit.Value);
			ItemSet.DefaultBookingDimensionUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Feet);
			AssertEquals(Constants.Length.Feet, ItemSet.DefaultBookingDimensionUnit.Value);
		}

		public void TestDefaultNumberOfDecimalPlaces()
		{
			AssertEquals("DefaultNumberOfDecimalPlacesForShipping", ItemSet.DefaultNumberOfDecimalPlaces.Name);
			AssertEquals("Liner & Agency/Default Units", ItemSet.DefaultNumberOfDecimalPlaces.Category);
			AssertEquals("Default Number of Decimal Places", ItemSet.DefaultNumberOfDecimalPlaces.Caption);
			AssertEquals(@"This registry item allows you to configure default number of decimal places for different units of measure (and different rounding rule when data is calculated or imported electronically) per specific transport mode.

If not configured, 3 decimal places are used for weight, volume and length units of measure by default.", ItemSet.DefaultNumberOfDecimalPlaces.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DefaultNumberOfDecimalPlaces.Storage);
			var collection = ItemSet.DefaultNumberOfDecimalPlaces.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("RegistryItem contains no rows.", 0, collection.Count);
		}

		public void TestDuplicateRegistryEntry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Shipping);
			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = "KG";
			defaultNumberOfDecimals.NumberOfDecimals = 1;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;
			AssertNoExceptionThrown(() => AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection));
			collection.RunPreSaveValidation();
			Assert(!defaultNumberOfDecimals.HasErrors);
			var duplicateDefaultNumberOfDecimals = collection.AddNew();
			duplicateDefaultNumberOfDecimals.UnitOfMeasure = "KG";
			duplicateDefaultNumberOfDecimals.NumberOfDecimals = 2;
			duplicateDefaultNumberOfDecimals.RoundingMode = RoundingModes.Down;
			AssertExceptionThrown(typeof(RegistryValidationException), () => AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection));
			collection.RunPreSaveValidation();
			Assert(defaultNumberOfDecimals.HasErrors);
			Assert(duplicateDefaultNumberOfDecimals.HasErrors);
		}

		public void TestAllowNonStandardContainerNumbersInContainerManager()
		{
			TestGenericRegistryItem(ItemSet.AllowNonStandardContainerNumbersInContainerManager, "AgencyCMAllowNonStandardContainerNumbers", "Liner & Agency/Container Manager", "Allow Non-Standard Container Numbers", "When enabled, validation checking on container stock records will be relaxed to allow non-standard container numbers.", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestAllowInterCountryDetentionJobCreation()
		{
			TestGenericRegistryItem(ItemSet.AllowInterCountryDetentionJobCreation, "AgencyAllowInterCountryDetentionJobCreation", "Liner & Agency/Principal\\Agency Settings", "Allow creation of detention jobs for foreign countries/regions", "Allow users to create container detention jobs for containers in other countries/regions.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestCMMDefaultVoyageFromMovement()
		{
			TestGenericRegistryItem(ItemSet.CMMDefaultVoyageFromMovement, "CMMDefaultVoyageFromMovement", "Liner & Agency/Port Messaging/Container Management Messaging", "Default Voyage From Movement", "If enabled and the container event message processor is unable to identify the correct Vessel/Voyage, the processor will resort to using the vessel/voyage of an earlier movement considered to be related.", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestDefaultCreditor()
		{
			TestGenericRegistryItem(ItemSet.DefaultCreditorFromPrincipal, "AgencyDefaultCreditorToPrincipal", "Liner & Agency/Principal\\Agency Settings", "Default Creditor From Principal", "If this is enabled, the principal charges in the Booking, Bill of Lading and Container Detention modules will be defaulted from the principal. Whilst this is correct for agencies, if you are a principal you probably want to disable this option.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestPostAllShipmentRevenueCharges()
		{
			TestGenericRegistryItem(ItemSet.PostAllShipmentRevenueCharges, "AgencyPostAllShipmentRevenueCharges", "Liner & Agency/Principal\\Agency Settings", "Booking/Bill of Lading Revenue Charges Posting", "If you are a Principal (Shipping Line) organization, you may want to post all revenue charges on Bookings and Bills of Lading (prepaid & collect charges in sending and receiving locations). If you are an Agency organization, you would most likely only want to post charges that are to be paid locally.\r\n\r\nPost all revenue charges?", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestPostAllShipmentCostCharges()
		{
			TestGenericRegistryItem(ItemSet.PostAllShipmentCostCharges, "AgencyPostAllShipmentCostCharges", "Liner & Agency/Principal\\Agency Settings", "Booking/Bill of Lading Cost Charges Posting", "If you are a Principal (Shipping Line) organization, you may want to post all cost charges on Bookings and Bills of Lading (prepaid & collect charges in sending and receiving locations). If you are an Agency organization, you would most likely only want to post charges that are to be paid locally.\r\n\r\nPost all cost charges?", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestCMMUpdateBookings()
		{
			TestGenericRegistryItem(ItemSet.CMMUpdateBookings, "CMMUpdateBookings", "Liner & Agency/Port Messaging/Container Management Messaging", "Update Bookings", "Which container event message should update related bookings.", RegistryStorageFlags.System);
		}

		public void TestUpdateShipmentDatesFromSailing()
		{
			TestGenericRegistryItem(ItemSet.UpdateShipmentDatesFromSailing, "AgencyUpdateShipmentDatesFromSailing", "Liner & Agency", "Update Shipment Dates From Sailing", "If enabled, 'Shipped on Board' and 'Issued' dates on the shipments will be defaulted from the sailings ATD. If the date is updated on the sailing schedule then the user will be asked if the defaulting should take place.", RegistryStorageFlags.System, false);
		}

		public void TestAlwaysGenerateBillOfLadingNumbers()
		{
			TestGenericRegistryItem(ItemSet.AlwaysGenerateBillOfLadingNumbers, "AlwaysGenerateBillOfLadingNumbers", "Liner & Agency", "Generate Bill Of Lading Numbers", "If enabled, system will always generate Bill Of Lading Numbers regardless of the direction of the job. If disabled, the Bill Of Lading Numbers will only be generated for the export or domestic jobs.", RegistryStorageFlags.System, false);
		}

		public void TestAlwaysGenerateBookingNumbers()
		{
			TestGenericRegistryItem(ItemSet.AlwaysGenerateBookingNumbers, "AlwaysGenerateBookingNumbers", "Liner & Agency", "Generate Booking Numbers", "If enabled, system will always generate Booking Numbers regardless of the direction of the job. If disabled, the Booking Numbers will only be generated for the export or domestic jobs.", RegistryStorageFlags.System, false);
		}

		public void TestSundryChargeTypes()
		{
			TestGenericRegistryItem(ItemSet.SundryChargeTypes, "SundryChargeTypes", "Liner & Agency/Sundry Charges", "Sundry Charge Types", "", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			const string expectedList = "PSN - Principal Sundries" + "";
			ICodeDescriptionPairListWithDefaultCode value = ItemSet.SundryChargeTypes.Value;
			AssertMultilineASCIIEquals("", expectedList, value.GetCodeDescriptionPairList().ElementsAsString);
			AssertEquals("PSN", value.DefaultCode);
		}

		public void TestSundryChargeModes()
		{
			TestGenericRegistryItem(ItemSet.SundryChargeModes, "SundryChargeModes", "Liner & Agency/Sundry Charges", "Sundry Charge Modes", "", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			const string expectedList = "STD - Standard" + "";
			ICodeDescriptionPairListWithDefaultCode value = ItemSet.SundryChargeModes.Value;
			AssertMultilineASCIIEquals("", expectedList, value.GetCodeDescriptionPairList().ElementsAsString);
			AssertEquals("STD", value.DefaultCode);
		}

		public void TestSundryChargeActivities()
		{
			TestGenericRegistryItem(ItemSet.SundryChargeActivities, "SundryChargeActivities", "Liner & Agency/Sundry Charges", "Sundry Charge Activities", "", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			const string expectedList = "STD - Standard" + "";
			ICodeDescriptionPairListWithDefaultCode value = ItemSet.SundryChargeActivities.Value;
			AssertMultilineASCIIEquals("", expectedList, value.GetCodeDescriptionPairList().ElementsAsString);
			AssertEquals("STD", value.DefaultCode);
		}

		public void TestShipmentNumberCustomisation()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.OceanBillShipmentNumberCustomisation.Storage);
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			customisation.RemoveFountainPrefix = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";
			ItemSet.OceanBillShipmentNumberCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);
			BillOfLadingNumberCustomisation value = ItemSet.OceanBillShipmentNumberCustomisation.Value;
			AssertEquals("Value.RemoveSPrefix", true, value.RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.LinerAgency, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", false, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", false, value.EnableMacroInsertion);
		}

		public void TestOceanBillNumberCustomisation()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.OceanBillNumberCustomisation.Storage);
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			customisation.RemoveFountainPrefix = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";
			ItemSet.OceanBillNumberCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);
			BillOfLadingNumberCustomisation value = ItemSet.OceanBillNumberCustomisation.Value;
			AssertEquals("Value.RemoveSPrefix", true, value.RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.LinerAgency, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", false, value.EnableMacroInsertion);
		}

		public void TestBookingNumberCustomisation()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.BookingNumberCustomisation.Storage);
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			customisation.RemoveFountainPrefix = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";
			ItemSet.BookingNumberCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);
			BillOfLadingNumberCustomisation value = ItemSet.BookingNumberCustomisation.Value;
			AssertEquals("Value.RemoveSPrefix", true, value.RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.LinerAgency, value.Categories);
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", false, value.EnableMacroInsertion);
		}

		public void TestDetentionAdviceWarningDays()
		{
			TestGenericRegistryItem(ItemSet.DetentionAdviceWarningDays, "DetentionAdviceWarningDays", "Liner & Agency/Container Detention", "Detention Advice Warning Days", "", RegistryStorageFlags.System, RegistryOptions.Default, 7);
		}

		public void TestUpdateEmptyReturnByWhenAvailabilityDatesChange()
		{
			TestGenericRegistryItem(ItemSet.UpdateEmptyReturnByWhenAvailabilityDatesChange, "UpdateEmptyReturnByWhenAvailabilityDatesChange", "Liner & Agency/Container Detention", "Recalculated Empty Return By Date", "If enabled, ‘Empty Return By’ date on Bills of Lading Containers will be recalculated automatically when the corresponding Availability Date on Sailing Schedule changes. If there are detention days already calculated with no detention invoices attached, then the movements will also have their detention days recalculated. Any manually given free days to the bill of lading container will be overridden.", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestCMMCreateMissingContainers()
		{
			TestGenericRegistryItem(ItemSet.CMMCreateMissingContainers, "CMMCreateMissingContainers", "Liner & Agency/Port Messaging/Container Management Messaging", "Create Missing Containers", "By default, if a message is received referring to the movement of a container not already in the Container Management module, CargoWise will ignore the movement and report it as a discrepancy. Set this to true if you would rather CargoWise to add a new container to the Container Management module.", RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestCMMEmailGroups()
		{
			TestGenericRegistryItem(ItemSet.CMMAcknowledgementEmailGroup, "CODECOAcknowledgementEmailGroup", "Notification/Liner & Agency/Container Management Messaging", "Group To Send Acknowledgement Emails To", "Override the Registry if you would like to suppress Acknowledgement email notifications.", RegistryStorageFlags.System, RegistryOptions.IsValueOptional, Constants.Groups.AllPK);
			TestGenericRegistryItem(ItemSet.CMMDiscrepanciesEmailGroup, "CODECODiscrepanciesEmailGroup", "Notification/Liner & Agency/Container Management Messaging", "Group To Send Discrepancies Emails To", "Override the Registry if you would like to suppress Discrepancies email notifications.", RegistryStorageFlags.System, RegistryOptions.IsValueOptional, Constants.Groups.AllPK);
			TestGenericRegistryItem(ItemSet.CMMErrorEmailGroup, "CODECOErrorEmailGroup", "Notification/Liner & Agency/Container Management Messaging", "Group To Send Error Emails To", "Override the Registry if you would like to suppress Error email notifications.", RegistryStorageFlags.System, RegistryOptions.IsValueOptional, Constants.Groups.AllPK);
		}

		public void TestCMMAllowDirectCODECOCOARRI()
		{
			TestGenericRegistryItem(ItemSet.AllowDirectCODECOCOARRICMMMessaging, "AllowDirectCODECOCOARRICMMMessaging", "Liner & Agency/Port Messaging/Container Management Messaging", "Allow Direct CODECO/COARRI (developer only)", "Allow Direct CODECO/COARRI usage for Container Management Messaging.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestReleaseTypeDefault()
		{
			AssertEquals(String.Empty, ItemSet.ReleaseTypeDefault.Value);
			CodeDescriptionPair codeDescriptionPair = new CodeDescriptionPair("OBR", "Ocean bill required at destination");
			ItemSet.ReleaseTypeDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionPair.Code);
			AssertEquals("OBR", ItemSet.ReleaseTypeDefault.Value);
		}

		public void TestAllowInvoiceAmendmentsDays()
		{
			string hint = "Specify a number of days from last discharge port ATA for Imports, " + "and last load port ATD for Exports, when " + "unrestricted amendments to the Bill of Lading > Billing are allowed.";
			TestGenericRegistryItem(ItemSet.AllowInvoiceAmendmentsDays, "AllowInvoiceAmendmentsDays", "Liner & Agency", "Allow Invoice Amendments Days", hint, RegistryStorageFlags.System, 0m);
		}

		public void TestReleaseTypes()
		{
			AssertEquals("Name", "LinerAgencyReleaseTypes", ItemSet.ReleaseTypes.Name);
			AssertEquals("Caption", "Release Types", ItemSet.ReleaseTypes.Caption);
			AssertEquals("Hint", "This list defines the different Release Type options available in Liner & Agency. You can add your own items to this list but cannot change code and description of the system defined items.", ItemSet.ReleaseTypes.Hint);
			AssertEquals("Category", LinerAgencyDataRegistry.Categories.LinerAgency, ItemSet.ReleaseTypes.Category);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ReleaseTypes.Storage);
			AssertEquals("System List", 4, ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count);
			AssertEquals(3, ItemSet.ReleaseTypes.Value.OriginalsNumber);
			AssertEquals(3, ItemSet.ReleaseTypes.Value.CopiesNumber);
			RegReleaseTypes newValue = new RegReleaseTypes();
			RegReleaseType value1 = new RegReleaseType();
			value1.Code = "AAA";
			value1.Description = (NoResString)"blah blah blah";
			newValue.Types.Add(value1);
			ItemSet.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals(5, ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count);
			AssertEquals("AAA", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[0].Code);
			AssertEquals("blah blah blah", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[0].Description);
			AssertEquals(false, ItemSet.ReleaseTypes.Value.Types[0].SystemDefined);
			newValue = ItemSet.ReleaseTypes.DefaultValue;
			value1 = new RegReleaseType();
			value1.Code = "AAA";
			value1.Description = (NoResString)"blah blah blah";
			newValue.Types.Add(value1);
			ItemSet.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals(5, ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count);
			AssertEquals("AAA", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count - 1].Code);
			AssertEquals("blah blah blah", ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList()[ItemSet.ReleaseTypes.Value.GetCodeDescriptionPairList().Count - 1].Description);
			AssertEquals(false, ItemSet.ReleaseTypes.Value.Types[ItemSet.ReleaseTypes.Value.Types.Count - 1].SystemDefined);
			for (int i = 0; i < ItemSet.ReleaseTypes.Value.Types.Count - 1; i++)
			{
				AssertEquals(true, ItemSet.ReleaseTypes.Value.Types[i].SystemDefined);
			}
		}

		public void TestServiceLevelDefault()
		{
			RefServiceLevel standardService = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			TestGenericRegistryItem(ItemSet.ServiceLevelDefault, "AgencyServiceLevelDefault", "Liner & Agency", "Service Level Default", "Override the Registry if you would like to set the default Service Level.", RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.BranchDepartment, RegistryOptions.IsValueOptional, standardService.PK);
		}

		public void TestServiceLevelDefaultNoStandard()
		{
			RefServiceLevel standardService = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			standardService.Delete();
			Factory.Save();
			TestGenericRegistryItem(ItemSet.ServiceLevelDefault, "AgencyServiceLevelDefault", "Liner & Agency", "Service Level Default", "Override the Registry if you would like to set the default Service Level.", RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.BranchDepartment, RegistryOptions.IsValueOptional, Guid.Empty);
		}

		public void TestContainerTranshipmentIndicator()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.ContainerTranshipmentIndicator.Storage);
			ContainerTranshipmentIndicatorCollection collection = ContainerTranshipmentIndicatorCollection.NewAndPopulate();
			collection[0].Direct = "x";
			collection[0].Tranship = "y";
			collection[0].Domestic = "z";
			ItemSet.ContainerTranshipmentIndicator.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			ContainerTranshipmentIndicator indicator = ItemSet.ContainerTranshipmentIndicator.Value[0];
			AssertEquals("Value direct has new value", "x", indicator.Direct);
			AssertEquals("Value Tranship has new value", "y", indicator.Tranship);
			AssertEquals("Value Domestic has new value", "z", indicator.Domestic);
		}

		public void TestPortAuthoritySettings()
		{
			String testString = "This option allows you to enable port authority messaging for specific ports.\r\n" + "If the desired port is not in this list then you will need to add it to:\r\n" + "Liner & Agency->Port Authority->Ports";
			AssertEquals(testString, ItemSet.PortAuthoritySettings.Hint);
		}

		public void TestDangerousGoodsManifestPorts()
		{
			TestGenericRegistryItem(ItemSet.DangerousGoodsManifestPorts, "DangerousGoodsManifestPorts", "Liner & Agency/Port Messaging/Dangerous Goods Manifest", "Ports", "This registry allows you to enable Dangerous Goods Manifest messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident.", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestContainerExportPreAdvicePorts()
		{
			TestGenericRegistryItem(ItemSet.ContainerExportPreAdvicePorts, "ContainerExportPreAdvicePorts", "Liner & Agency/Port Messaging/Container Export Pre-Advice", "Ports", "This registry allows you to enable Container Export Pre-Advice messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident.", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestLoadAndDischargeManifestPorts()
		{
			TestGenericRegistryItem(ItemSet.LoadAndDischargeManifestPorts, "LoadAndDischargeManifestPorts", "Liner & Agency/Port Messaging/Load and Discharge Manifest", "Ports", "This registry allows you to enable Load and Discharge Manifest messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident.", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestImportReleaseOrderPorts()
		{
			TestGenericRegistryItem(ItemSet.ImportReleaseOrderPorts, "ImportReleaseOrderPorts", "Liner & Agency/Port Messaging/Import Release Order", "Ports", "This registry allows you to enable Import Release Order messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident.", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestShippingPortsMessagingEHubID()
		{
			TestGenericRegistryItem(ItemSet.ShippingPortsMessagingEHubID, "ShippingPortsMessagingEHubID", "Liner & Agency/Port Messaging", "Shipping Ports Messaging eHub ID", "This is the eHub ID used for sending interchange to Shipping Ports.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
			AssertEquals("RegistryItem contains four rows", 5, ItemSet.ShippingPortsMessagingEHubID.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Count);
		}

		public void TestEIDOMessaging()
		{
			OrgHeader principalOrg = BaseAgencyTest.NewPrincipal(Factory);
			Factory.Save();
			AssertEquals(RegistryStorageFlags.System, ItemSet.EIDOMessagingDetails.Storage);
			EIDOMessagingHeader messaging = new EIDOMessagingHeader();
			messaging.Email = "bob@freadnet.org";
			EIDOMessagingIdentity principal = messaging.Identities.AddNew();
			principal.PrincipalPK = principalOrg.PK;
			principal.Password = "password";
			principal.SenderID = "sender";
			principal.RecipientID = "recipient";
			ItemSet.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, messaging);
			EIDOMessagingHeader value = ItemSet.EIDOMessagingDetails.Value;
			AssertEquals("bob@freadnet.org", value.Email);
			AssertEquals(1, value.Identities.Count);
			AssertEquals("password", value.Identities[0].Password);
			AssertEquals("sender", value.Identities[0].SenderID);
			AssertEquals("recipient", value.Identities[0].RecipientID);
		}

		public void TestMovementArchiveDays()
		{
			TestGenericRegistryItem(ItemSet.MovementArchiveDays, "MovementArchiveDays", "Liner & Agency/Container Manager", "Movement Archive Days", "Movements on the movements tab more than this many days old will be hidden by default. A value of 0 indicates that all movements should be shown by default.", RegistryStorageFlags.All, RegistryOptions.Default, 60);
		}

		public void TestContainerDamageCodes()
		{
			TestGenericRegistryItem(ItemSet.ContainerDamageCodes, "ContainerDamageCodes", "Liner & Agency/Container Manager", "Container Damage Codes", "Container Damage Codes", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
		}

		public void TestContainerCleanCodes()
		{
			TestGenericRegistryItem(ItemSet.ContainerCleanCodes, "ContainerCleanCodes", "Liner & Agency/Container Manager", "Container Clean Codes", "Container Clean Codes", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
		}

		public void TestContainerServiceCodes()
		{
			TestGenericRegistryItem(ItemSet.ContainerServiceCodes, "ContainerServiceCodes", "Liner & Agency/Container Manager", "Container Service Codes", "Container Service Codes", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestAgencyBillOfLadingClause()
		{
			var principal1 = BaseAgencyTest.NewPrincipal(Factory);
			principal1.OH_Code = "P Code 1";
			principal1.OH_FullName = "P Full Name 1";
			var principal2 = BaseAgencyTest.NewPrincipal(Factory);
			principal2.OH_Code = "P Code 2";
			principal2.OH_FullName = "P Full Name 2";
			Factory.Save();

			TestGenericRegistryItem
			(
				ItemSet.BillOfLadingClause(principal1),
				"AgencyBillOfLadingClause-" + principal1.PK.ToString(),
				"Liner & Agency/Bills of Lading/Bill Clause",
				"P Full Name 1",
				"The Bill Clause will be printed on the body of the Bill Of Lading.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				""
			);

			TestGenericRegistryItem
			(
				ItemSet.BillOfLadingClause(principal2),
				"AgencyBillOfLadingClause-" + principal2.PK.ToString(),
				"Liner & Agency/Bills of Lading/Bill Clause",
				"P Full Name 2",
				"The Bill Clause will be printed on the body of the Bill Of Lading.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				""
			);

			AssertSame(ItemSet.BillOfLadingClause(principal1), ItemSet.BillOfLadingClause(principal1));

			var list = ItemSet.GetAllItems();

			AssertCollectionContains(ItemSet.BillOfLadingClause(principal1), list);
			AssertCollectionContains(ItemSet.BillOfLadingClause(principal2), list);
		}

		public void TestAgencyOBLChargesDefaultDisplay()
		{
			TestGenericRegistryItem
			(
				ItemSet.AgencyOBLChargesDefaultDisplay,
				"AgencyOBLChargesDisplay",
				"Liner & Agency/Bills of Lading",
				"OBL Charges Default",
				"Use this registry to default what is displayed in the charges section of the Bill Of Lading.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryOptions.Default
			);
			AssertEquals("SHW", ItemSet.AgencyOBLChargesDefaultDisplay.DefaultValue);
		}

		public void TestUseNewFormBuilderBillOfLading()
		{
			TestGenericRegistryItem
			(
				ItemSet.UseNewFormBuilderBillOfLading,
				"UseNewFormBuilderBillOfLading",
				"Liner & Agency/Bills of Lading",
				"Use new Form Builder Bill Of Lading",
				"If turned on CW1 will use new Form builder Bill of Lading.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false
			);
		}

		public void TestPrintSignature()
		{
			TestGenericRegistryItem
			(
				ItemSet.PrintSignature,
				"PrintSignature",
				"Liner & Agency/Bills of Lading",
				"Print Signature",
				"Use this registry to allow staff signature to be printed on Bill of Lading documents where a dedicated signature box is provided.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false
			);
		}

		public void TestShowPacklineDetailsOnBillsOfLading()
		{
			TestGenericRegistryItem
			(
				ItemSet.ShowPacklineDetailsOnBillsOfLading,
				"ShowPacklineDetailsOnBillsOfLading",
				"Liner & Agency/Bills of Lading",
				"Show Packline Details on Bills of Lading",
				"Enable this option to show the breakdown of Outer Packline details for each container on Bill of Ladings.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false
			);
		}

		public void TestBillOfLadingTermsAndConditionsImages()
		{
			TestGenericRegistryItem
			(
				ItemSet.BillOfLadingTermsAndConditionsImages,
				"BillOfLadingTermsAndConditionsImages",
				"Liner & Agency/Bills of Lading",
				"Bill Of Lading Terms & Conditions",
				"If enabled, the terms and conditions will be printed on the Bill Of Lading for the nominated principal.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default
			);
		}

		public void TestBillOfLadingLogosImages()
		{
			TestGenericRegistryItem
			(
				ItemSet.BillOfLadingLogosImages,
				"BillOfLadingLogosImages",
				"Liner & Agency/Bills of Lading",
				"Bill Of Lading Logos",
				"If enabled, the logo will be printed on the Bill Of Lading for the nominated principal.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default
			);
		}

		public void TestElectronicBookingAndShippingInstructions()
		{
			TestGenericRegistryItem(ItemSet.ElectronicBookingAndShippingInstructions, "ElectronicBookingAndShippingInstructions", "Liner & Agency", "Enable electronic Booking and Shipping Instructions", "If enabled, Electronic Booking Requests and Shipping Instructions from other CargoWise Systems can be received and processed in Liner & Agency Module (Bookings/Bill of Lading).", RegistryStorageFlags.System, RegistryOptions.Default, true);
		}

		public void TestAllowSendingBookingConfirmationEDIAfterATD()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestGenericRegistryItem
				(
					ItemSet.AllowSendingBookingConfirmationEDIAfterATD,
					"AllowSendingBookingConfirmationEDIAfterATD",
					"Liner & Agency/Electronic Booking Messaging",
					"Send Booking Confirmation after ATD",
					"If enabled, an updated Booking Confirmation will be automatically sent to the Booking Party after the ATD (Actual Time of Departure) is added to the load port.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsHidden
				);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				RegistryItemDictionary.Instance.PurgeAll();
				TestGenericRegistryItem
				(
					ItemSet.AllowSendingBookingConfirmationEDIAfterATD,
					"AllowSendingBookingConfirmationEDIAfterATD",
					"Liner & Agency/Electronic Booking Messaging",
					"Send Booking Confirmation after ATD",
					"If enabled, an updated Booking Confirmation will be automatically sent to the Booking Party after the ATD (Actual Time of Departure) is added to the load port.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default
				);
			}
		}
	}
}
