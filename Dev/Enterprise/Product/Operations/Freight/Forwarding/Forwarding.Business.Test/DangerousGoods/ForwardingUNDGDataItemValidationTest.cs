using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGDataItemValidationTest : TestCaseWithFactory
	{
		#region Constructor

		public void TestValidationIATAConstructor()
		{
			// Arrange
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_Code = "1001";
			undgSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgDataItem.LinkDefault(undgSubstance);
			undgDataItem.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			// Act
			var validation = ForwardingUNDGDataItemValidation.New(undgDataItem);

			// Assert
			Assert(validation is ForwardingUNDGDataItemIATAValidation);
		}

		public void TestValidationJTTConstructor()
		{
			// Arrange
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_Code = "1001";
			undgSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT;
			undgDataItem.LinkDefault(undgSubstance);
			undgDataItem.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT;

			// Act
			var validation = ForwardingUNDGDataItemValidation.New(undgDataItem);

			// Assert
			Assert(validation is ForwardingUNDGDataItemJTTValidation);
		}

		public void TestValidationCFRConstructor()
		{
			// Arrange
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_Code = "1001";
			undgSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			undgDataItem.LinkDefault(undgSubstance);
			undgDataItem.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;

			// Act
			var validation = ForwardingUNDGDataItemValidation.New(undgDataItem);

			// Assert
			Assert(validation is ForwardingUNDGDataItemCFRValidation);
		}

		public void TestValidationIMOConstructor()
		{
			// Arrange
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_Code = "1001";
			undgSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgDataItem.LinkDefault(undgSubstance);
			undgDataItem.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			// Act
			var validation = ForwardingUNDGDataItemValidation.New(undgDataItem);

			// Assert
			Assert(validation is ForwardingUNDGDataItemIMOValidation);
		}

		#endregion

		#region Consol Acceptance

		public void TestConsolDoesNotAcceptDangerousGoods()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				const string errorMessage = "The following Consol(s) do not accept this dangerous cargo";

				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var packline = shipment.OuterPackLines.AddNew();
				var undgDataItem = packline.UNDGs.AddNew();

				Factory.Save();

				AssertNoErrorContaining("Shipment does not contain any dangerous goods", undgDataItem.DI_IMOClassInfo, errorMessage);

				undgDataItem.DI_IMOClass = "1.1D";

				AssertHasErrorContaining(undgDataItem.DI_IMOClassInfo, errorMessage);
			}
		}

		public void TestConsolDoesNotAcceptDangerousSubstances()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Transports[0].JW_IsCargoOnly = false;
			consol.Transports[0].JW_IsCargoOnly = false;
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var undgDataItem = packline.UNDGs.AddNew();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "3300";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.3";
			undgDataItem.SubstancePK = subs.PK;
			var errorSubstr = "contains lithium batteries which cannot be uplifted on a passenger flight. Either ensure that the following Consolidation";

			AssertNoErrorContaining("Shipment does not contain any dangerous substances (3300 is allowed)",
				undgDataItem.DI_DG_NKSubsInfo,
				errorSubstr);

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries;
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs2.DG_Class = "9";
			undgDataItem.SubstancePK = subs2.PK;

			AssertHasErrorContaining(undgDataItem.DI_DG_NKSubsInfo, errorSubstr);

			undgDataItem.LinkDefault(subs2);

			AssertHasErrorContaining(undgDataItem.DI_DG_NKSubsInfo, errorSubstr);
		}

		#endregion

		public void TestDGTechnicalNameValidationIfRequiredForThisSubstance()
		{
			const string errorMessage = "Technical Name is required for this substance.";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgDataItem.LinkDefault(subs);
			undgDataItem.Substance.DG_TechName = "";

			AssertNoWarning(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.Substance.DG_TechName = "*";
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertHasWarning(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.DI_TechnicalName = "Killer Vanila";

			AssertNoError(undgDataItem.DI_TechnicalNameInfo, errorMessage);
		}

		public void TestDI_SpecialPermitIssueDateValidationWhenPermitNumberEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var undgDataItem = packLine.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			substance.DG_UNNO = "1001";
			substance.DG_Mode = Constants.TransportModes.Air;
			undgDataItem.DI_DG = substance.PK;

			var specialPermitIssueErrorMessage = "Please enter a Special Issue Date.";

			undgDataItem.Validation.ValidateDI_SpecialPermitIssueDate();

			AssertNoError("When special permit no. is empty, issue date is not required.", undgDataItem.DI_SpecialPermitIssueDateInfo, specialPermitIssueErrorMessage);

			undgDataItem.DI_SpecialPermitNumber = "69420";
			undgDataItem.Validation.ValidateDI_SpecialPermitIssueDate();

			AssertHasError("When special permit no. is empty, issue date is required.", undgDataItem.DI_SpecialPermitIssueDateInfo, specialPermitIssueErrorMessage);
		}

		#region Flash Point

		public void TestFlashPoint_AboveMaximumTemperature()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_RequiredTemperatureUnit = "C";
			packline.JL_RequiredTemperatureMaximum = 30;
			packline.JL_RequiredTemperatureMinimum = 20;
			packline.JL_RequiresTemperatureControl = true;

			Factory.Save();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_ParentTableCode = "JL";
			undgDataItem.DI_DGFlashPoint = 32;

			AssertNoErrorContaining("Flash point is above max temp.", undgDataItem.DI_DGFlashPointInfo, "Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point");

			undgDataItem.DI_DGFlashPoint = 25;

			AssertHasErrorContaining(undgDataItem.DI_DGFlashPointInfo, "Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point");

			packline.JL_RequiredTemperatureMaximum = 25;
			undgDataItem.Validation.ValidateDI_DGFlashPoint();

			AssertNoErrorContaining("Max Temp is below Flash Point", undgDataItem.DI_DGFlashPointInfo, "Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point");

			packline.RequiredTemperatureUnit = "F";
			packline.JL_RequiredTemperatureMaximum = 50;
			undgDataItem.Validation.ValidateDI_DGFlashPoint();

			AssertNoErrorContaining("Max Temp is still below because its farenheight", undgDataItem.DI_DGFlashPointInfo, "Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point");

			packline.RequiredTemperatureUnit = "C";
			packline.JL_RequiredTemperatureMaximum = 50;
			packline.JL_RequiresTemperatureControl = false;
			undgDataItem.Validation.ValidateDI_DGFlashPoint();

			AssertNoErrorContaining("Does not require Temperature Control.", undgDataItem.DI_DGFlashPointInfo, "Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point");

			packline.JL_RequiresTemperatureControl = true;
			undgDataItem.Validation.ValidateDI_DGFlashPoint();

			AssertHasErrorContaining(undgDataItem.DI_DGFlashPointInfo, "Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point");
		}

		public void TestCheckDI_DGStandardWarning()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			var iataSubstance = Factory.New<UNDGSubstance>();
			iataSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			iataSubstance.DG_UNNO = "9999";

			undgDataItem.DI_DG = iataSubstance.PK;
			undgDataItem.LinkDefault(iataSubstance);

			undgDataItem.Validation.ValidateDI_DG();

			AssertHasWarning("Mismatched substance standard should be warning",
				undgDataItem.DI_DGInfo,
				"The Transport Mode of the Shipment does not match the dangerous goods Standard selected. Please reselect the dangerous goods substance from the IMO Standard.");
		}

		public void TestCheckInvalidDI_DG()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			undgDataItem.DI_DG = ZGuid.Invalid;
			undgDataItem.Validation.ValidateDI_DG();
			AssertHasError("Invalid DI_DG should have error", undgDataItem.DI_DGInfo, "Enter a valid DG Substance.");
	
			undgDataItem.DI_DG = ZGuid.NewZGuid();
			undgDataItem.Validation.ValidateDI_DG();
			AssertNoErrors("DI_DG should not validate Substance", undgDataItem.DI_DGInfo);
		}

		public void TestCheckInvalidSubstancePK()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();

			undgDataItem.SubstancePK = ZGuid.Invalid;
			AssertHasError("Invalid SubstancePK should have error",
				undgDataItem.SubstancePKInfo,
				"Enter a valid DG Substance.");

			undgDataItem.SubstancePK = ZGuid.NewZGuid();
			AssertHasError("Invalid SubstancePK should have error",
				undgDataItem.SubstancePKInfo,
				"Enter a valid DG Substance.");
		}

		public void TestFlashPoint_WhenEmpty()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			substance.DG_FlashPoint = "15";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			Factory.Save();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = subs.PK;
			undgDataItem.LinkDefault(subs);

			AssertEquals("Precondition: FlashPoint Defaulted", 15m, (decimal)undgDataItem.DI_DGFlashPoint);
			AssertNoWarningContaining(undgDataItem.DI_DGFlashPointInfo, "Ensure that the temperature range specified for the Dangerous Goods substance is less than its Flash Point");

			undgDataItem.DI_DGFlashPoint = 0;

			AssertHasWarningContaining(undgDataItem.DI_DGFlashPointInfo, "Ensure that the temperature range specified for the Dangerous Goods substance is less than its Flash Point");

			substance.DG_FlashPoint = "";
			undgDataItem.Validation.ValidateAll();

			AssertNoWarningContaining(undgDataItem.DI_DGFlashPointInfo, "Ensure that the temperature range specified for the Dangerous Goods substance is less than its Flash Point");
		}

		#endregion

		#region Class 7 Radioactive Materials

		public void TestClass7SecurityRight_ShowsError_IfNewIMOClassIs7()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				const string errorMessage = "You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment.";

				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var packline = shipment.OuterPackLines.AddNew();

				var substance = Factory.New<UNDGSubstance>();
				substance.DG_UNNO = "123";

				var dataItem = packline.UNDGs.AddNew();
				dataItem.DI_DG = substance.PK;
				dataItem.LinkDefault(substance);

				Factory.Save();

				AssertNoError("Pre-Condition: no error", dataItem.DI_IMOClassInfo, errorMessage);

				Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;
				dataItem.DI_IMOClass = "7";

				AssertHasError("No security to set Class to 7", dataItem.DI_IMOClassInfo, errorMessage);

				Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = true;
				dataItem.Validation.ValidateAll();

				AssertNoError("Security rights granted", dataItem.DI_IMOClassInfo, errorMessage);
			}
		}

		public void TestClass7SecurityRight_ShowsError_IfOriginalIMOClassIs7()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				const string errorMessage = "You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment.";

				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var packline = shipment.OuterPackLines.AddNew();

				var substance = Factory.New<UNDGSubstance>();
				substance.DG_UNNO = "123";

				var dataItem = packline.UNDGs.AddNew();
				dataItem.DI_DG = substance.PK;
				dataItem.DI_IMOClass = "7";
				dataItem.LinkDefault(substance);

				Factory.Save();

				CombineAssertions("Pre-Condition: original value is 7", () =>
				{
					AssertNoError(dataItem.DI_IMOClassInfo, errorMessage);
					AssertEquals("7", dataItem.DI_IMOClass);
				});

				Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;
				dataItem.DI_IMOClass = "5";

				AssertHasError("Can't change class because original value is 7 and no security right", dataItem.DI_IMOClassInfo, errorMessage);

				Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = true;
				dataItem.Validation.ValidateAll();

				AssertNoError("Security rights granted", dataItem.DI_IMOClassInfo, errorMessage);
			}
		}

		public void TestClass7SecurityRight_NoError_IfChangingBackToOriginalValue()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				const string errorMessage = "You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment.";

				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var packline = shipment.OuterPackLines.AddNew();

				var substance = Factory.New<UNDGSubstance>();
				substance.DG_UNNO = "123";

				var dataItem = packline.UNDGs.AddNew();
				dataItem.DI_DG = substance.PK;
				dataItem.DI_IMOClass = "7";
				dataItem.LinkDefault(substance);

				Factory.Save();

				CombineAssertions("Pre-Condition: original value is 7", () =>
				{
					AssertNoError(dataItem.DI_IMOClassInfo, errorMessage);
					AssertEquals("7", dataItem.DI_IMOClass);
				});

				Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;
				dataItem.DI_IMOClass = "5";

				AssertHasError("Can't change class because original value is 7 and no security right", dataItem.DI_IMOClassInfo, errorMessage);

				dataItem.DI_IMOClass = "7";

				AssertNoError("Original value was 7 - user shouldn't get an error if they set it back to that", dataItem.DI_IMOClassInfo, errorMessage);
			}
		}

		public void TestClass7CFRDI_MaterialFormDescription()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			subs.DG_Class = RadioactiveConstants.RadioactiveClass;
			subs.DG_PSN = "random psn";
			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var expectedErrorMessage = "Class 7 substances that are not special form require a description of the physical or chemical form to be entered.";

			undgDataItem.DI_DG = subs.PK;
			undgDataItem.DI_MaterialFormDescription = ZString.Empty;

			AssertEquals("Precondition: Material Form Description readonly expected to be false", undgDataItem.DI_MaterialFormDescriptionInfo.ReadOnly, false);
			AssertHasError("Expected material form description to have an error", undgDataItem.DI_MaterialFormDescriptionInfo, expectedErrorMessage);

			undgDataItem.DI_MaterialFormDescription = "This is a random description";

			AssertNoError("Expected material form description to have an error", undgDataItem.DI_MaterialFormDescriptionInfo, expectedErrorMessage);
		}

		#endregion
	}
}
