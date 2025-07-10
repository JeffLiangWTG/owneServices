using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class ForwardingShipmentValidationTest : CommonShipmentValidationTest
	{
		#region House Bill Validation

		public void TestHBLCheckDigitRuleMatching()
		{
			var rule1 = CreateHBLCheckDigitRule("SEA", "", "", "ABC", false, "", "", 0, "NON");
			var rule2 = CreateHBLCheckDigitRule("ALL", "", "", "ABC", false, "", "", 0, "R07");
			var rule3 = CreateHBLCheckDigitRule("SEA", "", "", "ABC", false, "", "\"<Consol.JK_UniqueConsignRef>\" != \"\"", 0, "R31");
			var rule4 = CreateHBLCheckDigitRule("AIR", "AU", "DE", "EFG", true, "", "", 0, "NON");
			var rule5 = CreateHBLCheckDigitRule("AIR", "AUBNE", "DEHAM", "EFG", true, "", "", 0, "R07");
			var rule6 = CreateHBLCheckDigitRule("SEA", "", "", "DEF", true, "/*", "", 7, "R31");
			var rule7 = CreateHBLCheckDigitRule("SEA", "", "", "DEF", true, "", "", 7, "NON");
			var rule8 = CreateHBLCheckDigitRule("SEA", "", "", "", false, "", "", 7, "R07");
			var rule9 = CreateHBLCheckDigitRule("AIR", "", "", "GHI", false, "", "\"<JobNumber>\"==\"A67890\"", 0, "R31");
			var rule10 = CreateHBLCheckDigitRule("SEA", "NZ", "", "JKL", false, "", "", 0, "NON");
			var rule11 = CreateHBLCheckDigitRule("SEA", "", "FR", "JKL", false, "", "", 0, "R07");
			var rule12 = CreateHBLCheckDigitRule("SEA", "", "", "MNO", false, "/*A", "", 0, "R31");
			var rule13 = CreateHBLCheckDigitRule("AIR", "", "", "MNO", false, "/?B", "", 0, "NON");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_HouseBill = "ABC123";
			AssertHBLRuleHasClearWinner(shipment, rule1); // Exact transport mode trumps All

			shipment.JS_TransportMode = "AIR";
			AssertHBLRuleHasClearWinner(shipment, rule2);

			shipment.JS_TransportMode = "SEA";
			shipment.JS_UniqueConsignRef = "A12345";
			shipment.Consols.AddNew();
			Factory.Save();
			AssertEquals("Rule with Macro does not automatically trump rule without", shipment.Validation.HBLRuleMatchEvaluation(rule1), shipment.Validation.HBLRuleMatchEvaluation(rule3));

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_HouseBill = "EFG123";
			AssertHBLRuleHasClearWinner(shipment, rule5); // Port trumps country

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEBER";
			AssertHBLRuleHasClearWinner(shipment, rule4); // Fallback to country if no port match

			shipment.JS_HouseBill = "DEF123/001";
			shipment.JS_TransportMode = "SEA";
			AssertHBLRuleHasClearWinner(shipment, rule6); // Suffix match trumps blank suffix

			shipment.JS_HouseBill = "DEF123";
			AssertHBLRuleHasClearWinner(shipment, rule7); // Prefix match trumps blank prefix

			shipment.JS_HouseBill = "XYZ123";
			AssertHBLRuleHasClearWinner(shipment, rule8); // Fall back to blank prefix

			shipment.JS_HouseBill = "GHI123";
			shipment.JS_TransportMode = "AIR";
			AssertEquals("Macro doesn't match", 0, GetApplicableRulesInOrder(shipment).Count);

			shipment.JS_UniqueConsignRef = "A67890";
			AssertHBLRuleHasClearWinner(shipment, rule9); // Macro matches

			shipment.JS_HouseBill = "JKL123";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "FRCAL";
			shipment.JS_TransportMode = "SEA";
			AssertEquals("Origin and Destination have same value for matching", shipment.Validation.HBLRuleMatchEvaluation(rule10), shipment.Validation.HBLRuleMatchEvaluation(rule11));

			shipment.JS_HouseBill = "MNO123/XYZXYZXYZA";
			AssertHBLRuleHasClearWinner(shipment, rule12); // Wildcard * matches multiple characters

			shipment.JS_HouseBill = "MNO123/AB";
			shipment.JS_TransportMode = "AIR";
			AssertHBLRuleHasClearWinner(shipment, rule13); // Wildcard ? matches one character

			shipment.JS_HouseBill = "MNO123/12B";
			AssertEquals("Wildcard ? doesn't match multiple characters", 0, GetApplicableRulesInOrder(shipment).Count);
		}

		HouseBillsNumberValidation CreateHBLCheckDigitRule(ZString transportMode, ZString origin, ZString destination, ZString prefix, ZBool includePrefix, ZString suffix, ZString userDefinedCondition, ZInt length, ZString algorithm)
		{
			var rules = ForwardingConfigurationRegistry.Instance.HouseBillsNumberValidation.Value;
			var rule = rules.AddNew();
			rule.TransportMode = transportMode;
			rule.Origin = origin;
			rule.Destination = destination;
			rule.HBLPrefix = prefix;
			rule.IncludeHBLPrefix = includePrefix;
			rule.HBLSuffix = suffix;
			rule.UserDefinedCondition = userDefinedCondition;
			rule.HBLLengthFormatted = length.ToString();
			rule.CheckDigitAlgorithm = algorithm;

			ForwardingConfigurationRegistry.Instance.HouseBillsNumberValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			return rule;
		}

		void AssertHBLRuleHasClearWinner(ForwardingShipment shipment, HouseBillsNumberValidation expectedWinner)
		{
			var rules = GetApplicableRulesInOrder(shipment);
			HouseBillsNumberValidation winner = null;

			if (rules.Count > 1)
			{
				if (shipment.Validation.HBLRuleMatchEvaluation(rules[0]) > shipment.Validation.HBLRuleMatchEvaluation(rules[1]))
				{
					winner = rules[0];
				}
				else
				{
					Fail("More than one rule fulfils the condition");
				}
			}
			else if (rules.Count == 1)
			{
				winner = rules[0];
			}
			else
			{
				Fail("No rules fulfil the condition");
			}

			if (winner.TransportMode == expectedWinner.TransportMode
					&& winner.Origin == expectedWinner.Origin
					&& winner.Destination == expectedWinner.Destination
					&& winner.HBLPrefix == expectedWinner.HBLPrefix
					&& winner.IncludeHBLPrefix == expectedWinner.IncludeHBLPrefix
					&& winner.HBLSuffix == expectedWinner.HBLSuffix
					&& winner.UserDefinedCondition == expectedWinner.UserDefinedCondition
					&& winner.HBLLength == expectedWinner.HBLLength
					&& winner.CheckDigitAlgorithm == expectedWinner.CheckDigitAlgorithm)
			{
				Assert(true);
			}
			else
			{
				Fail("An incorrect rule was matched");
			}
		}

		List<HouseBillsNumberValidation> GetApplicableRulesInOrder(ForwardingShipment shipment)
		{
			var result = new List<HouseBillsNumberValidation>();

			result = ForwardingConfigurationRegistry.Instance.HouseBillsNumberValidation.Value
						.Cast<HouseBillsNumberValidation>()
						.OrderByDescending(x => shipment.Validation.HBLRuleMatchEvaluation(x))
						.Where(x => shipment.Validation.HBLRuleMatchEvaluation(x) > 0)
						.ToList();

			return result;
		}

		public void TestHouseBillCheckDigitValidation()
		{
			var rule1 = CreateHBLCheckDigitRule("SEA", "", "", "ABC", true, "", "", 0, "NON");
			var rule2 = CreateHBLCheckDigitRule("AIR", "", "", "ABC", true, "", "", 0, "R07");
			var rule3 = CreateHBLCheckDigitRule("AIR", "", "", "DEF", false, "", "", 0, "R31");
			var rule4 = CreateHBLCheckDigitRule("AIR", "", "", "DEF", false, "/*", "", 0, "R31");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			shipment.JS_TransportMode = "SEA";
			shipment.JS_HouseBill = "ABC123";
			AssertNoErrors("No rule is run 1", shipment.JS_HouseBillInfo);

			shipment.JS_HouseBill = "ABC124";
			AssertNoErrors("No rule is run 2", shipment.JS_HouseBillInfo);

			shipment.JS_TransportMode = "AIR";
			shipment.Validation.ValidateJS_HouseBill();
			AssertHasError(shipment.JS_HouseBillInfo, "Incorrect Check Digit. The check digit should be '2'."); // Prefix included in calculation

			shipment.JS_HouseBill = "ABC122";
			AssertNoErrors("Correct check digit is '2'", shipment.JS_HouseBillInfo);

			shipment.JS_HouseBill = "DEF123";
			AssertHasError(shipment.JS_HouseBillInfo, "Incorrect Check Digit. The check digit should be '5'."); // Prefix removed from calculation

			shipment.JS_HouseBill = "DEF125";
			AssertNoErrors("Correct check digit is '5'", shipment.JS_HouseBillInfo);

			shipment.JS_HouseBill = "DEF123/456";
			AssertHasError(shipment.JS_HouseBillInfo, "Incorrect Check Digit. The check digit should be '5'."); // Suffix removed from calculation)

			shipment.JS_HouseBill = "DEF125/456";
			AssertNoErrors("Correct check digit with suffix removed is '5'", shipment.JS_HouseBillInfo);
		}

		public void TestHouseBillCheckDigitValidation_RCC()
		{
			var rule1 = CreateHBLCheckDigitRule(Constants.TransportModes.Sea, "", "", "ABC", true, "", "", 0, CheckDigitAlgorithmList.Codes.None);
			var rule2 = CreateHBLCheckDigitRule(Constants.TransportModes.Air, "", "", "ABC", true, "", "", 0, CheckDigitAlgorithmList.Codes.CanadaCustoms);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "ABC001932";
			AssertNoErrors("No rule is run", shipment.JS_HouseBillInfo);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.Validation.ValidateJS_HouseBill();
			AssertHasError(shipment.JS_HouseBillInfo, "Incorrect Check Digit. The check digit should be '3'.");
		}

		[TestDate(2021, 08, 01)]
		public void TestHouseBillCheckDigitValidation_ShouldNotAddError_WhenHouseBillIsAutoGenerated()
		{
			var rule = CreateHBLCheckDigitRule(Constants.TransportModes.Air, "AUSYD", "NZAKL", "", true, "", "", 8, CheckDigitAlgorithmList.Codes.None);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("precondition: should be export", true, shipment.IsExport());

			shipment.JS_HouseBill = "123456789123";
			AssertHasError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");

			var newCustomizations = new BillOfLadingNumberCustomisationsByServiceLevel();
			var newCustomization = newCustomizations.BillOfLadingNumberCustomisations["ALL"];
			SetElement(newCustomization, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newCustomization, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomization, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");

			using (FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomizations))
			{
				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				AssertEquals("precondition: should be export", true, shipment.IsExport());

				Factory.Save();
				AssertEquals("Populate on first saving: JS_HouseBill", "S1H001", shipment.JS_HouseBill);
				AssertNoError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");
			}
		}

		public void TestHouseBillCheckDigitValidation_WhenHouseBillIsPendingAllocation()
		{
			CreateHBLCheckDigitRule(Constants.TransportModes.Air, "AUSYD", "NZAKL", "", true, "", "", 8, CheckDigitAlgorithmList.Codes.None);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("precondition: should be export", true, shipment.IsExport());

			shipment.JS_HouseBill = "Pending Allocation..";
			AssertHasError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");

			shipment.JS_HouseBill = "123456789";
			AssertHasError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");

			shipment.RegenerateHouseBillInSaving(true);

			AssertEquals(shipment.JS_HouseBill.ToUpper(), "PENDING ALLOCATION..");
			AssertNoError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Bypass HouseBillsNumberValidation", 9, shipment.JS_HouseBill.Length);
			AssertNoError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");

			shipment.JS_HouseBill = "Pending Allocation..";
			AssertHasError(shipment.JS_HouseBillInfo, "The length (excluding prefix and suffix) should be 8 characters.");
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			return element;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			return element;
		}

		#endregion

		#region TestJS_UniqueConsignRefValidation

		public void TestJS_UniqueConsignRefValidation()
		{
			TestJS_UniqueConsignRefValidationCore();
		}

		protected virtual void TestJS_UniqueConsignRefValidationCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.IsRoot = true;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			bool oldValue = Env.Registry.AllowManualShipmentEntry;
			try
			{
				Env.Registry.AllowManualShipmentEntry = false;

				shipment1.JS_UniqueConsignRef = "";
				AssertNoErrors(shipment1.JS_UniqueConsignRefInfo);

				shipment1.JS_UniqueConsignRef = "S00001000";
				AssertNoErrors(shipment1.JS_UniqueConsignRefInfo);

				Env.Registry.AllowManualShipmentEntry = true;

				shipment1.Validation.ValidateJS_UniqueConsignRef();
				AssertHasErrors("Auto-generated like numbers are not allowed when manually entered", shipment1.JS_UniqueConsignRefInfo);

				shipment1.JS_UniqueConsignRef = "";
				AssertHasErrors("", shipment1.JS_UniqueConsignRefInfo);

				shipment1.JS_UniqueConsignRef = "saonh";
				AssertNoNotifications(shipment1.JS_UniqueConsignRefInfo);

				shipment2.JS_UniqueConsignRef = shipment1.JS_UniqueConsignRef;
				AssertHasErrors(shipment2.JS_UniqueConsignRefInfo);

				shipment1.JS_UniqueConsignRef = "aosenuh";
				shipment2.Validation.ValidateJS_UniqueConsignRef();
				AssertNoNotifications(shipment2.JS_UniqueConsignRefInfo);
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = oldValue;
			}
		}

		#endregion

		#region TestJS_RS_NKServiceLevelValidation

		public void TestJS_RS_NKServiceLevelValidation()
		{
			const string errorMessage = "Enter a valid Service Level.";
			const string warningMessage = "You have not entered a valid code.";

			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "SL1";

			var serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "SL2";

			var serviceLevel3 = Factory.New<RefServiceLevel>();
			serviceLevel3.RS_Code = "SL3";

			var serviceLevel4 = Factory.New<RefServiceLevel>();
			serviceLevel4.RS_Code = "SL4";
			serviceLevel4.RS_DefaultTransitHours = 48;

			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = serviceLevel2.RS_Code;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CFS;
			shipment1.JS_RS_NKServiceLevel = serviceLevel1.RS_Code;
			AssertNoError(shipment1.JS_RS_NKServiceLevelInfo, errorMessage);

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CFS;
			shipment2.JS_RS_NKServiceLevel = "123";
			AssertHasError(shipment2.JS_RS_NKServiceLevelInfo, errorMessage);

			var shipment_nonServiceLevel = Factory.New<ForwardingShipment>();
			shipment_nonServiceLevel.JS_TransportMode = Constants.TransportModes.Air;
			shipment_nonServiceLevel.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CFS;
			shipment_nonServiceLevel.JS_ShipmentType = "HVL";
			shipment_nonServiceLevel.JS_RS_NKServiceLevel = string.Empty;
			AssertHasError(shipment_nonServiceLevel.JS_RS_NKServiceLevelInfo, "Please enter a Service Level for HVLV data.");

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				shipment1.JS_RS_NKServiceLevel = "SL5";
				shipment1.Validation.ValidateJS_RS_NKServiceLevel();
				AssertHasWarning("Warning - Service Level is not valid.", shipment1.JS_RS_NKServiceLevelInfo, warningMessage);
				AssertHasError(shipment1.JS_RS_NKServiceLevelInfo, errorMessage);

				shipment2.Validation.ValidateJS_RS_NKServiceLevel();
				AssertHasWarning("Warning - Service Level is not linked to a transit time record.", shipment2.JS_RS_NKServiceLevelInfo, warningMessage);
				AssertHasError(shipment2.JS_RS_NKServiceLevelInfo, errorMessage);

				shipment1.JS_RS_NKServiceLevel = serviceLevel4.RS_Code;
				AssertNoWarning("No Warning - When the selected Service Level has Transit Hours.", shipment1.JS_RS_NKServiceLevelInfo, warningMessage);

				shipment2.JS_RS_NKServiceLevel = serviceLevel2.RS_Code;
				AssertNoError(shipment2.JS_RS_NKServiceLevelInfo, errorMessage);
				AssertNoWarning(shipment2.JS_RS_NKServiceLevelInfo, warningMessage);

				var shipment3 = Factory.New<ForwardingShipment>();
				shipment3.JS_TransportMode = Constants.TransportModes.Air;
				shipment3.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CFS;
				shipment3.JS_RS_NKServiceLevel = serviceLevel2.RS_Code;
				AssertNoError(shipment3.JS_RS_NKServiceLevelInfo, errorMessage);
				AssertNoWarning(shipment3.JS_RS_NKServiceLevelInfo, warningMessage);

				shipment3.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CY_CFS;
				shipment3.JS_RS_NKServiceLevel = serviceLevel1.RS_Code;
				AssertNoError(shipment3.JS_RS_NKServiceLevelInfo, errorMessage);
				AssertNoWarning(shipment3.JS_RS_NKServiceLevelInfo, warningMessage);

				var shipment4 = Factory.New<ForwardingShipment>();
				shipment4.JS_TransportMode = Constants.TransportModes.Air;
				shipment4.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CFS;
				shipment4.JS_RS_NKServiceLevel = serviceLevel1.RS_Code;
				AssertHasWarning("Warning - Service Level is not linked to a transit time record.", shipment4.JS_RS_NKServiceLevelInfo, warningMessage);

				Factory.Save();
				shipment4.Validation.ValidateJS_RS_NKServiceLevel();
				AssertHasWarning("Warning - Service Level is not linked to a transit time record.", shipment4.JS_RS_NKServiceLevelInfo, warningMessage);
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				shipment1.Validation.ValidateJS_RS_NKServiceLevel();
				AssertNoWarning("No warning when delivery due date registry is disabled.", shipment1.JS_RS_NKServiceLevelInfo, warningMessage);
			}
		}

		#endregion

		public void TestShouldValidateFKToCancelledRecord()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingShipmentValidationForTest = new ForwardingShipmentValidationForTest(shipment);

			AssertEquals(true, forwardingShipmentValidationForTest.ShouldValidateFKToCancelledRecordForTest(shipment.JS_CFSReferenceInfo));
			AssertEquals(false, forwardingShipmentValidationForTest.ShouldValidateFKToCancelledRecordForTest(shipment.JS_OH_HandledOnBehalfOfForwarderInfo));
		}

		#region JS_InspectionTypeCode

		public void TestValidateJS_InspectionTypeCodeForWeb()
		{
			bool originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUAAB";
				shipment.JS_RL_NKDestination = "AUABH";
				shipment.JS_InspectionTypeCode = ZString.Empty;
				shipment.RunPreSaveValidation();
				AssertHasErrors(shipment.JS_InspectionTypeCodeInfo);
				AssertEquals("No recalculation message as Inspection Type has been manually set", ZString.Empty, shipment.MostRecentInspectionTypeChangeReason);

				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Web;
				shipment.RunPreSaveValidation();
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
				AssertEquals("No recalculation message as Inspection Type has been manually set", ZString.Empty, shipment.MostRecentInspectionTypeChangeReason);
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		public void TestValidateJS_InspectionTypeCode_ApprovedFlag()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			JobHeader job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "LOC");

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			shipment.JS_RL_NKOrigin = "AUAAB";
			shipment.JS_RL_NKDestination = "AUABH";
			shipment.JS_InspectionTypeCode = "APP";
			AssertHasErrors(shipment.JS_InspectionTypeCodeInfo);

			shipment.JS_RL_NKOrigin = "AUAAB";
			shipment.JS_RL_NKDestination = "AUABH";
			shipment.JS_InspectionTypeCode = "APP";
			AssertHasErrors(shipment.JS_InspectionTypeCodeInfo);

			shipment.JS_RL_NKOrigin = "AGBBQ";
			shipment.JS_RL_NKDestination = "AUAAB";
			shipment.JS_InspectionTypeCode = "APP";
			AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			shipment.Validation.ValidateJS_InspectionTypeCode();
			AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
			AssertEquals("No recalculation message as Inspection Type has been manually set", ZString.Empty, shipment.MostRecentInspectionTypeChangeReason);
		}

		public void TestValidateJS_InspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				ForwardingShipment shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKDestination = OverseasPort;
				shipment.JS_RL_NKOrigin = HomePort;

				shipment.JS_InspectionTypeCode = shipment.Lookups.InspectionTypes[2].Code;
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "SCR";
				AssertHasErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "XYZ";
				AssertHasErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "";
				AssertHasErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_RL_NKDestination = HomePort;
				shipment.JS_RL_NKOrigin = OverseasPort;

				shipment.JS_InspectionTypeCode = "";
				AssertNoErrors("Only mandatory for export/domestic", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_RL_NKDestination = HomePort;
				shipment.JS_RL_NKOrigin = AlternateHomePort;

				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertEquals("UNK", shipment.JS_InspectionTypeCode);
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
				shipment.JS_InspectionTypeCode = "";
				AssertHasErrors("Only mandatory for export/domestic", shipment.JS_InspectionTypeCodeInfo);

				Factory.Save();

				shipment.JS_SystemCreateTimeUtc = new DateTime(2015, 1, 1);
				FreightDataRegistry.Instance.EXMExemptionCodeRemovalDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2015, 2, 1));
				shipment.JS_InspectionTypeCode = "EXM";
				AssertNoErrors("Created before EXM Removal date", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_SystemCreateTimeUtc = new DateTime(2015, 3, 1);
				shipment.JS_InspectionTypeCode = "";
				shipment.JS_InspectionTypeCode = "EXM";
				AssertHasErrors("Created after EXM Removal date", shipment.JS_InspectionTypeCodeInfo);
				ErrorReporter.Clear();
			}
		}

		public void TestValidateJS_InspectionTypeCode_WithNoErrorForExistingShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKDestination = OverseasPort;
				shipment.JS_RL_NKOrigin = HomePort;

				shipment.JS_InspectionTypeCode = "RES";
				AssertHasErrors("Precondition: Should have error for new shipment", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "XRY";
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				using (shipment.GetValidationSuspender())
				{
					shipment.JS_InspectionTypeCode = "RES";
					AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

					Factory.Save();

					var newFactory = new BusinessObjectFactory();
					var reloadShipment = newFactory.Load<ForwardingShipment>(shipment.PK);

					AssertEquals("RES", reloadShipment.JS_InspectionTypeCode);
					AssertNoErrors("Should have no error for existing shipment", reloadShipment.JS_InspectionTypeCodeInfo);

					reloadShipment.RunPreSaveValidation();
					AssertNoErrors("No changes, should have no error", reloadShipment.JS_InspectionTypeCodeInfo);

					reloadShipment.JS_InspectionTypeCode = "FRD";
					AssertHasError("Invalid code, should have error", reloadShipment.JS_InspectionTypeCodeInfo, "Enter a valid Aviation Security Inspection.");

					reloadShipment.JS_InspectionTypeCode = "EDD";
					AssertNoErrors("Valid code, should have no error", reloadShipment.JS_InspectionTypeCodeInfo);
				}
			}
		}

		public void TestValidateJS_InspectionTypeCode_OrganisationsNotApprovedForPassengerFlights()
		{
			var orgsToUse = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.Value;
			((SupplyChainSecurityOrganisationToUse)orgsToUse.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)orgsToUse.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, orgsToUse))
			{
				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var knownConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "PROKWN"));
				knownShipperDetails = knownConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var localClient = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CARCOM"));
				knownShipperDetails = localClient.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "HKKWN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "HKKWN";
				consol.JK_RL_NKDischargePort = "USLAX";

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("HKKWN", "USLAX").PK;
				transport.JW_IsCargoOnly = false;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_RL_NKOrigin = "HKKWN";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.
Account Consignor - Consignor (GLOKWN)");

				var jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
				jobHeader.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.
Account Consignor - Consignor (GLOKWN)
Account Consignor - Local Client (CARCOM)");

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoError("Can send on passenger flights when inspected", shipment.JS_InspectionTypeCodeInfo, @"The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.
Account Consignor - Consignor (GLOKWN)
Account Consignor - Local Client (CARCOM)");

				shipment.JS_InspectionTypeCode = "UNK";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = knownConsignor.PK;
				jobHeader.JH_OA_LocalChargesAddr = knownConsignor.PK;
				shipment.JS_RL_NKOrigin = "HKKWN";

				shipment.SetApprovedShipperStatus("", true);

				AssertEquals("APP", shipment.JS_InspectionTypeCode);
				AssertNoErrors("Known Consignor", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_InspectionTypeCode_HKAccountConsignorForPassengerFlightWithValidInspectionType()
		{
			var expectedError = @"The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.
Account Consignor - Consignor (GLOKWN)";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "HKKWN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "HKKWN";
				consol.JK_RL_NKDischargePort = "USLAX";

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("HKKWN", "USLAX").PK;
				transport.JW_IsCargoOnly = false;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_RL_NKOrigin = "HKKWN";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, expectedError);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);
			}
		}

		public void TestValidateJS_InspectionTypeCode_MultipleOrganisations()
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.HongKong))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				var consol = shipment.Consols.AddNew();

				var consignor = CreateOrg("CON");
				var localClient = CreateOrg("LOC");
				var transport = CreateOrg("TRN");
				var cfs = CreateOrg("CFS");
				var agent = CreateOrg("AGT");
				var airline = CreateOrg("AIR");
				var coloader = CreateOrg("CLD");

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transport.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
				consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
				consol.JK_OA_ShippingLineAddress = airline.MainAddress.PK;
				consol.JK_OA_CreditorAddress = coloader.MainAddress.PK;

				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_InspectionTypeCode = "APP";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Shipment Pickup Transport Company (TRN)
Consol Airline (AIR)");

				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations are not Approved:
Consignor (CON)
Local Client (LOC)
Consol Sending Agent (AGT)");

				SetOrgApproval(consignor);
				SetOrgApproval(localClient);
				SetOrgApproval(transport);
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Consol Airline (AIR)");

				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations are not Approved:
Consol Sending Agent (AGT)");

				SetOrgApproval(airline);
				SetOrgApproval(agent);
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
				AssertNoWarnings(shipment.JS_InspectionTypeCodeInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				var consignor = CreateOrg("CON");

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_InspectionTypeCode = "APP";

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations are not Approved:
Consignor (CON)");

				SetOrgApproval(consignor);
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertNoWarnings(shipment.JS_InspectionTypeCodeInfo);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertHasError("User can't change from one inspection type to another", shipment.JS_InspectionTypeCodeInfo, "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.");
			}
		}

		public void TestApprovedJS_InspectionTypeCodeWillNotBeAllowedToChangeWhenCurrentUserIsNotCertificatedForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				shipment.JS_InspectionTypeCode = "APP";
				shipment.JS_InspectionTypeCodeOriginalValue = "APP";
				AssertEquals("APP", shipment.JS_InspectionTypeCode);
				Factory.Save();

				var loadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);
				AssertEquals("APP", loadedShipment.JS_InspectionTypeCode);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "BKG";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);
				loadedShipment.JS_InspectionTypeCode = "UNK";
				AssertHasError("User can't change from one inspection type to another", loadedShipment.JS_InspectionTypeCodeInfo, "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.");
			}
		}

		public void TestValidateJS_InspectionTypeCode_NoOrganizations()
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.Value;
			settings.SuspendValidation();
			foreach (var org in settings.Cast<SupplyChainSecurityOrganisationToUse>())
			{
				org.ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			}
			settings.ResumeValidation();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.DataType.SuspendValidation())
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_InspectionTypeCode = "UNK";

				string expectedWarningText = @"Inspection Status - This Shipment has not been checked for Known/Unknown Organizations. Review and ensure this Shipment should be changed to Approved.

No Organizations have been configured to be checked in this Registry setting:

Freight > Supply Chain Security > Hong Kong > Organization to Use for Supply Chain Security";

				AssertNoWarning("There should be no warnings", shipment.JS_InspectionTypeCodeInfo, expectedWarningText);

				shipment.JS_InspectionTypeCode = "APP";
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertHasWarning("Warning should be added", shipment.JS_InspectionTypeCodeInfo, expectedWarningText);
			}
		}

		public void TestValidateJS_InspectionTypeCode_RegistryValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var types = new ShipmentInspectionTypes();
				var collection = types.Types;
				collection.Add("ABC", (NoResString)"hello", true, true);
				collection.Add("DEF", (NoResString)"hello", false, true);
				FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, types);

				ForwardingShipment shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = OverseasPort;
				shipment.JS_RL_NKOrigin = HomePort;
				shipment.JS_InspectionTypeCode = "ABC";
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "DEF";
				AssertHasErrors("DEF is not valid", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestWarningOnJS_InspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				const string expectedWarning = "This is a custom inspection type. Custom inspection types, if not authorized by LGA, will cause rejection by airlines and may result in cargo being re-screened, delayed or not uplifted.";

				var types = new ShipmentInspectionTypes();
				var collection = types.Types;
				collection.Add("AOM", (NoResString)"hello", true, true);
				collection.Add("ZXZ", (NoResString)"hello", false, true);

				using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, types))
				{
					var shipment = (ForwardingShipment)GetShipment();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKDestination = OverseasPort;
					shipment.JS_RL_NKOrigin = HomePort;

					shipment.JS_InspectionTypeCode = "AOM";
					AssertNoWarning(shipment.JS_InspectionTypeCodeInfo, expectedWarning);

					shipment.JS_InspectionTypeCode = "ZXZ";
					AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, expectedWarning);
				}
			}
		}

		public void TestJS_InspectionTypeCode_UNKValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				ForwardingShipment shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JMALP";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);

				shipment.JS_InspectionTypeCode = "XRY";
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestJS_InspectionTypeCode_WithAviationSecurity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				ForwardingShipment shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors("Pre-condition", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors("UNK can be chosen for future departure date", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_E_DEP = ZDateTime.Now.AddHours(1);
				AssertNoErrors("Can change to UNK for a departure an hour from now", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_E_DEP = ZDateTime.Now.AddHours(-1);
				AssertNoErrors("Can change to UNK for a departure an hour ago", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);
				var consol = shipment.Consols.AddNew();
				consol.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(1);
				shipment.RunPreSaveValidation();
				AssertNoErrors("UNK can be chosen for future MAWB issue date", shipment.JS_InspectionTypeCodeInfo);

				consol.JK_MasterBillIssueDate = ZDateTime.Now;
				shipment.RunPreSaveValidation();
				AssertNoErrors("UNK can be chosen for MAWB issue date of today", shipment.JS_InspectionTypeCodeInfo);

				consol.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(-1);
				shipment.RunPreSaveValidation();
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "'Unknown' cannot be manually chosen as the departure Consol's MAWB issue date has passed.");
			}
		}

		public void TestJS_InspectionTypeCode_WithAviationSecurity_OnlyChangeToUNKIfConsolInFuture()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var expectedError = "'Unknown' cannot be manually chosen as the first routing air leg’s ATD is in the past.";
				ForwardingShipment shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors("Pre-condition", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors("UNK can be chosen for future departure date", shipment.JS_InspectionTypeCodeInfo);

				var consolEarlierDeparture = shipment.Consols.AddNew();
				var earlierTransport = consolEarlierDeparture.Transports.FirstOrDefault() as Transport;
				earlierTransport.JW_TransportMode = "AIR";
				earlierTransport.JW_ETD = ZDateTime.Now.AddDays(+1);
				shipment.Validation.ValidateAll();
				AssertNoErrors("Because the ATD of the first Air Leg is empty, there is no error message", shipment.JS_InspectionTypeCodeInfo);

				earlierTransport.JW_ATD = ZDateTime.Now.AddDays(-1);
				shipment.Validation.ValidateAll();
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, expectedError);

				earlierTransport.JW_TransportMode = "ROA";
				earlierTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
				shipment.Validation.ValidateAll();
				AssertNoErrors("If there is no Air Leg, this error will not be displayed", shipment.JS_InspectionTypeCodeInfo);

				earlierTransport.JW_TransportMode = "AIR";
				earlierTransport.JW_ATD = ZDateTime.Now.AddDays(-1);
				earlierTransport.JW_RL_NKLoadPort = "JPOSA";
				earlierTransport.JW_RL_NKDiscPort = "USCHI";

				var secondTansport = consolEarlierDeparture.Transports.AddNew();
				secondTansport.JW_TransportMode = "AIR";
				secondTansport.JW_ATD = ZDateTime.Now.AddDays(2);
				secondTansport.JW_RL_NKLoadPort = "USCHI";
				secondTansport.JW_RL_NKDiscPort = "DEHAM";
				shipment.Validation.ValidateAll();
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, expectedError);

				earlierTransport.JW_TransportMode = "ROA";
				shipment.Validation.ValidateAll();
				AssertNoErrors("The ATD of the first Air leg is in the future, this error will not be displayed", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestJS_InspectionTypeCode_WithAviationSecurity_CheckConsolTimingGetsSorted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				ForwardingShipment shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors("Pre-condition", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors("UNK can be chosen for future departure date", shipment.JS_InspectionTypeCodeInfo);

				var consoleLaterDeparture = shipment.Consols.AddNew();
				var transportLaterDeparture = consoleLaterDeparture.Transports.FirstOrDefault() as Transport;
				transportLaterDeparture.JW_TransportMode = "AIR";
				transportLaterDeparture.JW_ATD = ZDateTime.Now.AddDays(2);
				shipment.Validation.ValidateAll();
				AssertNoErrors("First consol is in the future", shipment.JS_InspectionTypeCodeInfo);

				var consolEarlierDepartureUnsorted = shipment.Consols.AddNew();
				var transportEarlierDeparture = consolEarlierDepartureUnsorted.Transports.FirstOrDefault() as Transport;
				transportEarlierDeparture.JW_ATD = ZDateTime.Now.AddDays(-1);
				shipment.Validation.ValidateAll();
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "'Unknown' cannot be manually chosen as the first routing air leg’s ATD is in the past.");

				transportEarlierDeparture.JW_ATD = ZDateTime.Now.AddDays(1);
				shipment.Validation.ValidateAll();
				AssertNoErrors("First consol has been changed to be at least the current time so changing back to UNK is now valid", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestJS_InspectionTypeCode_WithAviationSecurity_DoesNotRaiseError_WhenFirstAirLegHasDeparted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = (ForwardingShipment)GetShipment();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors("Pre-condition", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors("UNK can be chosen for future departure date", shipment.JS_InspectionTypeCodeInfo);

				var consoleLaterDeparture = shipment.Consols.AddNew();
				var transportLaterDeparture = consoleLaterDeparture.Transports.FirstOrDefault() as Transport;
				transportLaterDeparture.JW_TransportMode = "AIR";
				transportLaterDeparture.JW_ATD = ZDateTime.Now.AddDays(2);
				shipment.RunPreSaveValidation();
				AssertNoErrors("First consol is in the future", shipment.JS_InspectionTypeCodeInfo);

				var consolEarlierDepartureUnsorted = shipment.Consols.AddNew();
				var transportEarlierDeparture = consolEarlierDepartureUnsorted.Transports.FirstOrDefault() as Transport;
				transportEarlierDeparture.JW_TransportMode = "AIR";
				transportEarlierDeparture.JW_ATD = ZDateTime.Now.AddDays(-1);
				shipment.Validation.ValidateAll();
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "'Unknown' cannot be manually chosen as the first routing air leg’s ATD is in the past.");

				transportEarlierDeparture.JW_ATD = ZDateTime.Now.AddDays(1);
				shipment.Validation.ValidateAll();
				AssertNoErrors("First consol has been changed to be at least the current time so changing back to UNK is now valid", shipment.JS_InspectionTypeCodeInfo);

				transportEarlierDeparture.JW_ATD = ZDateTime.Empty;
				transportEarlierDeparture.JW_ETD = ZDateTime.Now.AddDays(-1);
				shipment.Validation.ValidateAll();
				AssertNoErrors("First consol ATD is blank so changing back to UNK is now valid", shipment.JS_InspectionTypeCodeInfo);

				transportEarlierDeparture.JW_ETD = ZDateTime.Now.AddDays(1);
				shipment.Validation.ValidateAll();
				AssertNoErrors("First consol ATD is blank so changing back to UNK is now valid", shipment.JS_InspectionTypeCodeInfo);

				transportEarlierDeparture.JW_ATD = ZDateTime.Now.AddDays(-1);

				Factory.Save();

				var anotherFactory = new BusinessObjectFactory();

				var loadedShipment = anotherFactory.Load<ForwardingShipment>(shipment.PK);
				loadedShipment.Validation.ValidateAll();
				AssertNoErrors("Should not raise any error when first air leg is departed.", loadedShipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestJS_InspectionTypeCode_WithAviationSecurity_WhenPackLevelScreeningIsUnavailable()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var packline1 = shipment.OuterPackLines.AddNew();
			Assert(!shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			Assert(packline1.Shipment.AviationSecurity.SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(packline1.Shipment));
			Assert(packline1.JL_InspectionTypeCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, packline1.JL_InspectionTypeCode);

			shipment.JS_InspectionTypeCode = "SCR";
			AssertHasError(shipment.JS_InspectionTypeCodeInfo, "SCR - Screened cannot be chosen here as it applies when all Packing Inspections are entered. Packing Inspections are not available on this Shipment.");
		}

		public void TestJS_InspectionTypeCode_WithAviationSecurity_HandlesNullTransportsOnConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);

				shipment.JS_InspectionTypeCode = "UNK";
				shipment.JS_InspectionTypeCodeHasChanges = true;

				var consol = shipment.Consols.AddNew();
				consol.Transports.RemoveAndDeleteAll();

				AssertNoExceptionThrown(() => shipment.Validation.ValidateJS_InspectionTypeCode());
				AssertNoErrors("Consol's departures were null, therefore no error was created on the property", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_InspectionTypeCode_Japan_WithManualApproval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JPTYO";
				shipment.JS_RL_NKDestination = "AUSYD";

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "This Consignor is not Approved so an Inspection Type of Approved/Known Shipper is not allowed.");

				shipment.DocManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Consignment Security Declaration File"), "CSD.txt", "CSD");
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_InspectionTypeCode_NoErrorForDepartedShipments()
		{
			var expectedError = "This Consignor is not Approved so an Inspection Type of Approved/Known Shipper is not allowed.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var agentApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				agentApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				agentApproval.OV_EXApprovedOrMajorExporter = "RA";
				agentApproval.OV_EXApprovalNumber = "5678";
				agentApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "SGSIN";
				shipment1.JS_RL_NKDestination = "JPOSA";

				var transport1 = shipment1.Transports.AddNew();
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "SGSIN";
				transport1.JW_RL_NKDiscPort = "JPOSA";

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "SGSIN";
				shipment2.JS_RL_NKDestination = "JPOSA";

				var transport2 = shipment2.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "SGSIN";
				transport2.JW_RL_NKDiscPort = "JPOSA";
				transport2.JW_ATD = ZDateTime.Today.AddDays(-1);

				var shipment3 = Factory.New<ForwardingShipment>();
				shipment3.JS_TransportMode = "AIR";
				shipment3.JS_RL_NKOrigin = "SGSIN";
				shipment3.JS_RL_NKDestination = "JPOSA";

				var transport3 = shipment3.Transports.AddNew();
				transport3.JW_TransportMode = "AIR";
				transport3.JW_RL_NKLoadPort = "SGSIN";
				transport3.JW_RL_NKDiscPort = "JPOSA";
				transport3.JW_ATD = ZDateTime.Today.AddDays(-1);

				Factory.Save();

				shipment1.JS_InspectionTypeCode = "APP";
				shipment2.JS_InspectionTypeCode = "APP";
				shipment3.JS_InspectionTypeCode = "UNK";

				Factory.Save();

				var newFactory = new BusinessObjectFactory();

				var reloadedShipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
				var reloadedShipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);
				var reloadedShipment3 = newFactory.Load<ForwardingShipment>(shipment3.PK);

				reloadedShipment1.Validation.ValidateJS_InspectionTypeCode();
				reloadedShipment2.Validation.ValidateJS_InspectionTypeCode();
				reloadedShipment3.Validation.ValidateJS_InspectionTypeCode();

				AssertHasError("Shipment1 has an error, as the Approval is invalid", reloadedShipment1.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError("Shipment2 doesn't have an error as it has already departed", reloadedShipment2.JS_InspectionTypeCodeInfo, expectedError);
				AssertHasWarning("Instead, the error has been changed to a warning", reloadedShipment2.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError("Shipment3 is not approved, so no error", reloadedShipment3.JS_InspectionTypeCodeInfo, expectedError);

				reloadedShipment3.JS_InspectionTypeCode = "APP";
				AssertHasError("It is not possible to change the approval to 'APP', even after departure", reloadedShipment1.JS_InspectionTypeCodeInfo, expectedError);
			}
		}

		public void TestValidateJS_InspectionTypeCode_Singapore_NoErrorForDepartedShipments_AdditionalValidation()
		{
			var expectedError = "Only a Regulated Cargo Agent can handle known consignor cargo, otherwise it is considered unknown. Configure your regulated cargo agent details against the login company's Organization Proxy for Singapore.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "SGSIN";
				shipment1.JS_RL_NKDestination = "JPOSA";

				var transport1 = shipment1.Transports.AddNew();
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "SGSIN";
				transport1.JW_RL_NKDiscPort = "JPOSA";

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "SGSIN";
				shipment2.JS_RL_NKDestination = "JPOSA";

				var transport2 = shipment2.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "SGSIN";
				transport2.JW_RL_NKDiscPort = "JPOSA";
				transport2.JW_ATD = ZDateTime.Today.AddDays(-1);

				var shipment3 = Factory.New<ForwardingShipment>();
				shipment3.JS_TransportMode = "AIR";
				shipment3.JS_RL_NKOrigin = "SGSIN";
				shipment3.JS_RL_NKDestination = "JPOSA";

				var transport3 = shipment3.Transports.AddNew();
				transport3.JW_TransportMode = "AIR";
				transport3.JW_RL_NKLoadPort = "SGSIN";
				transport3.JW_RL_NKDiscPort = "JPOSA";
				transport3.JW_ATD = ZDateTime.Today.AddDays(-1);

				Factory.Save();

				shipment1.JS_InspectionTypeCode = "APP";
				shipment2.JS_InspectionTypeCode = "APP";
				shipment3.JS_InspectionTypeCode = "UNK";

				Factory.Save();

				var newFactory = new BusinessObjectFactory();

				var reloadedShipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
				var reloadedShipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);
				var reloadedShipment3 = newFactory.Load<ForwardingShipment>(shipment3.PK);

				reloadedShipment1.Validation.ValidateJS_InspectionTypeCode();
				reloadedShipment2.Validation.ValidateJS_InspectionTypeCode();
				reloadedShipment3.Validation.ValidateJS_InspectionTypeCode();

				AssertHasError("Shipment1 has an error, as the Approval is invalid", reloadedShipment1.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError("Shipment2 doesn't have an error as it has already departed", reloadedShipment2.JS_InspectionTypeCodeInfo, expectedError);
				AssertHasWarning("Instead, the error has been changed to a warning", reloadedShipment2.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError("Shipment3 is not approved, so no error", reloadedShipment3.JS_InspectionTypeCodeInfo, expectedError);

				reloadedShipment3.JS_InspectionTypeCode = "APP";
				AssertHasError("It is not possible to change the approval to 'APP', even after departure", reloadedShipment1.JS_InspectionTypeCodeInfo, expectedError);
			}
		}

		public void TestValidationJS_InspectionTypeCode_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "SCR - Screened status cannot be used as there are no packlines.");

				var packline1 = shipment.OuterPackLines.AddNew();
				var packline2 = shipment.OuterPackLines.AddNew();

				AssertEquals("Precondition: packline1", "", packline1.JL_InspectionTypeCode);
				AssertEquals("Precondition: packline2", "", packline2.JL_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Inspection method SCR – Screened means each packline of the shipment is screened. You have packlines with UNK status. Either ensure each packline has an additional inspection entered that is not UNK - Unknown, or change shipment Inspection to UNK until all packs are screened.");

				packline1.JL_InspectionTypeCode = "UNK";
				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Inspection method SCR – Screened means each packline of the shipment is screened. You have packlines with UNK status. Either ensure each packline has an additional inspection entered that is not UNK - Unknown, or change shipment Inspection to UNK until all packs are screened.");

				packline1.JL_InspectionTypeCode = "PHS";
				packline2.JL_InspectionTypeCode = "XRY";
				shipment.JS_InspectionTypeCode = "PHS";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Shipment inspection states PHS, however packlines are screened using different methods. Change Shipment Inspection to SCR – Screened instead.");

				packline2.JL_InspectionTypeCode = "UNK";
				shipment.JS_InspectionTypeCode = "PHS";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Shipment inspection states PHS, however some packlines are UNK. Either add inspection value to each packline, or change Shipment Inspection to UNK until all packs are screened.");

				packline2.JL_InspectionTypeCode = "PHS";
				shipment.JS_InspectionTypeCode = "XRY";

				var partialNotification = "Change Shipment Inspection to match packline inspection type.";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Shipment inspection states XRY, however all packlines are screened by PHS. Change Shipment Inspection to match packline inspection type.");
				AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, partialNotification);

				shipment.JS_InspectionTypeCode = "APP";

				AssertNoErrorContaining("Shipment is APP", shipment.JS_InspectionTypeCodeInfo, partialNotification);

				shipment.JS_InspectionTypeCode = "PHS";
				packline1.JL_InspectionTypeCode = "PHS";
				packline2.JL_InspectionTypeCode = "PHS";

				AssertNoErrorContaining("Shipment and packlines match", shipment.JS_InspectionTypeCodeInfo, partialNotification);
			}
		}

		public void TestValidationJS_InspectionTypeCode_EU_Booking_SkipValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_IsForwardRegistered = false;
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "SCR - Screened status cannot be used as there are no packlines.");

				var packline1 = shipment.OuterPackLines.AddNew();
				var packline2 = shipment.OuterPackLines.AddNew();

				AssertEquals("Precondition: packline1", "", packline1.JL_InspectionTypeCode);
				AssertEquals("Precondition: packline2", "", packline2.JL_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "SCR";

				AssertEquals(false, shipment.JS_InspectionTypeCodeInfo.HasErrors());

				packline1.JL_InspectionTypeCode = "UNK";
				shipment.JS_InspectionTypeCode = "SCR";

				AssertEquals(false, shipment.JS_InspectionTypeCodeInfo.HasErrors());

				packline1.JL_InspectionTypeCode = "PHS";
				packline2.JL_InspectionTypeCode = "XRY";
				shipment.JS_InspectionTypeCode = "PHS";

				AssertEquals(false, shipment.JS_InspectionTypeCodeInfo.HasErrors());

				packline2.JL_InspectionTypeCode = "UNK";
				shipment.JS_InspectionTypeCode = "PHS";

				AssertEquals(false, shipment.JS_InspectionTypeCodeInfo.HasErrors());

				packline2.JL_InspectionTypeCode = "PHS";
				shipment.JS_InspectionTypeCode = "XRY";

				AssertEquals(false, shipment.JS_InspectionTypeCodeInfo.HasErrors());
			}
		}

		public void TestValidateJS_InspectionTypeCode_Template()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var messageError = @"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Consol (CON)";
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				var consignor = CreateOrg("CON");
				SetOrgApproval(consignor);
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "SCR - Screened status cannot be used as there are no packlines.");
				var packline1 = shipment.OuterPackLines.AddNew();
				var packline2 = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "SCR";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Inspection method SCR – Screened means each packline of the shipment is screened. You have packlines with UNK status. Either ensure each packline has an additional inspection entered that is not UNK - Unknown, or change shipment Inspection to UNK until all packs are screened.");

				packline1.JL_InspectionTypeCode = "PHS";
				packline2.JL_InspectionTypeCode = "XRY";
				shipment.JS_InspectionTypeCode = "PHS";

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Shipment inspection states PHS, however packlines are screened using different methods. Change Shipment Inspection to SCR – Screened instead.");
				shipment.JS_InspectionTypeCode = "APP";

				var templateRecord = Factory.New<StmTemplateRecord>();
				var templateRecordProvider = (ITemplateRecordProvider)shipment;
				templateRecordProvider.TemplateRecord = templateRecord;

				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertNoErrors(messageError, shipment.JS_InspectionTypeCodeInfo);

				consignor.MainAddress.KnownShipperDetails.FirstOrDefault().OV_EXApprovedOrMajorExporter = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertHasErrors(messageError, shipment.JS_InspectionTypeCodeInfo);

				templateRecordProvider.IsTemplateRecord = true;
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertNoErrors(messageError, shipment.JS_InspectionTypeCodeInfo);
			}
		}

		[TestDate(2023, 11, 18)]
		public void TestValidateJS_InspectionTypeCode_WhenPackLineUNK()
		{
			var whsCode = "WHS01";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_Code = whsCode;
			warehouse.OH_FullName = $"{whsCode} PLD.";
			warehouse.MainAddress.OA_Address1 = $"{whsCode} Address";

			const string errorMessage = "At least one pack line’s Inspection status is 'UNK - Unknown' so either it is pending receipt at the warehouse or its secured status has not yet been advised by the Warehouse.";
			TestCase("DEHAM", isForwardRegistered: true, expectedRequiresSecuredCargoFromWarehouse: true, expectedError: true);
			TestCase("DEHAM", isForwardRegistered: false, expectedRequiresSecuredCargoFromWarehouse: true, expectedError: false);
			TestCase("AUSYD", isForwardRegistered: true, expectedRequiresSecuredCargoFromWarehouse: false, expectedError: false);

			void TestCase(ZString origin, bool isForwardRegistered, bool expectedRequiresSecuredCargoFromWarehouse, bool expectedError)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(origin.Left(2)))
				using (WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = "HKHKG";
					shipment.JS_TransportMode = "AIR";
					shipment.JS_IsForwardRegistered = isForwardRegistered;

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
					packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouse.MainAddress.PK;
					shipment.JS_InspectionTypeCode = "UNK";
					packLine.JL_InspectionTypeCode = "UNK";
					shipment.JS_InspectionTypeCodeHasChanges = false;

					AssertEquals(expectedRequiresSecuredCargoFromWarehouse, shipment.RequiresSecuredCargoFromWarehouse);

					Factory.Save();
					var entryNum = Factory.New<CusEntryNumber>();
					entryNum.CE_ParentID = shipment.PK;
					entryNum.CE_ParentTable = "JobShipment";
					entryNum.CE_RN_NKCountryCode = "EU";
					entryNum.CE_EntryType = "INS";
					entryNum.CE_Category = "INS";
					entryNum.CE_EntryNum = "APP";
					Factory.Save();

					var newShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
					newShipment.JS_InspectionTypeCodeHasChanges = true;
					newShipment.Validation.ValidateJS_InspectionTypeCode();

					if (expectedError)
					{
						AssertHasError(newShipment.JS_InspectionTypeCodeInfo, errorMessage);
					}
					else
					{
						AssertNoError(newShipment.JS_InspectionTypeCodeInfo, errorMessage);
					}
				}
			}
		}

		public void TestCheckJS_A_RCV_ShouldNotBeInTheFuture_BasedOnCFSPickupAddressTimeZone_WhenCFSPickupAddressHasValue()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddSeconds(-2);
			AssertNoErrors("doesn't show error because it is not in the future", shipment.JS_A_RCVInfo);

			var pickupCFSAddress = Factory.New<OrgHeader>();
			pickupCFSAddress.OH_Code = "CNRORG";
			pickupCFSAddress.MainAddress.OA_Address1 = "Main st";
			pickupCFSAddress.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			var pickupCFSPort = ((ILocation)pickupCFSAddress.MainAddress).UNLOCO;
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.MainAddress.PK;
			shipment.JS_A_RCV = pickupCFSPort.LocationDateTime.AddMinutes(1);
			AssertHasError(shipment.JS_A_RCVInfo, "Interim Receipt Date cannot be in the future.");
		}

		public void TestCheckJS_A_RCV_ShouldNotBeInTheFuture_BasedOnCurrentBranchTimeZone_WhenCFSPickupAddressIsBlank_ConsignorPickupAddressIsBlank()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddSeconds(2);
			AssertHasWarnings("The Interim Receipt Date is in the future.", shipment.JS_A_RCVInfo);

			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddSeconds(-2);
			AssertNoWarnings(shipment.JS_A_RCVInfo);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_GB_HomeBranch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, (ZString)"SYD").PK;
			Factory.Save();
			shipment.JS_A_RCV = GlbStaff.CurrentUser.HomeBranch.HomePort.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime().AddSeconds(2));
			AssertHasWarnings("The Interim Receipt Date is in the future.", shipment.JS_A_RCVInfo);

			shipment.JS_A_RCV = GlbStaff.CurrentUser.HomeBranch.HomePort.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime().AddSeconds(-2));
			AssertNoWarnings(shipment.JS_A_RCVInfo);
		}

		public void TestCheckJS_A_RCV_ShouldNotBeInTheFuture_BasedOnCFSPickupAddress_WhenRelatedPortCodeIsBlank()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var pickupCFSAddress = Factory.New<OrgHeader>();
			pickupCFSAddress.OH_Code = "CNRORG";
			pickupCFSAddress.MainAddress.OA_Address1 = "Main st";
			pickupCFSAddress.MainAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.MainAddress.PK;
			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddSeconds(20);
			AssertHasWarnings("Interim Receipt Date cannot be in the future.", shipment.JS_A_RCVInfo);
		}

		public void TestCheckJS_A_RCV_ShouldNotBeInTheFuture_BasedOnCFSPickupAddress_WhenCFSPickupAddressHasValueAndUNLOCOTimeZoneDoesNotHaveValue()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AAAAA";
			unloco.RL_R3 = ZGuid.Empty;
			var pickupCFSAddress = Factory.New<OrgHeader>();
			pickupCFSAddress.OH_Code = "CNRORG";
			pickupCFSAddress.MainAddress.OA_Address1 = "Main st";
			pickupCFSAddress.MainAddress.OA_RL_NKRelatedPortCode = unloco.RL_Code;
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.MainAddress.PK;
			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddMinutes(1);
			AssertHasError(shipment.JS_A_RCVInfo, "Interim Receipt Date cannot be in the future.");
		}

		public void TestCheckJS_A_RCV_ShouldNotBeInTheFuture_BasedOnCurrentBranchTimeZone_WhenCFSPickupAddressIsBlank_FallBackToConsignorPickupAddressOrOrigin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddSeconds(2);
			AssertHasWarnings("The Interim Receipt Date is in the future.", shipment.JS_A_RCVInfo);

			shipment.JS_A_RCV = Env.Time.CurrentLocalDateTime.AddSeconds(-2);
			AssertNoWarnings(shipment.JS_A_RCVInfo);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.ConsignorPickupAddress.E2_OA_Address = Guid.Empty;
			shipment.JS_A_RCV = shipment.Origin.LocationDateTime.AddMinutes(3);
			AssertHasError(shipment.JS_A_RCVInfo, "Interim Receipt Date cannot be in the future.");

			shipment.JS_A_RCV = shipment.Origin.LocationDateTime.AddMinutes(-3);
			AssertNoErrors("doesn't show error because it is not in the future", shipment.JS_A_RCVInfo);

			var consignorOrg = Factory.New<OrgHeader>();
			consignorOrg.OH_Code = "CNRORG";
			consignorOrg.MainAddress.OA_Address1 = "Main st";
			consignorOrg.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			var pickupPort = ((ILocation)consignorOrg.MainAddress).UNLOCO;
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorOrg.MainAddress.PK;
			shipment.JS_A_RCV = pickupPort.LocationDateTime.AddMinutes(1);
			AssertHasError(shipment.JS_A_RCVInfo, "Interim Receipt Date cannot be in the future.");

			shipment.JS_A_RCV = pickupPort.LocationDateTime.AddMinutes(-2);
			AssertNoErrors("doesn't show error because it is not in the future", shipment.JS_A_RCVInfo);
		}

		#region Implementation

		OrgHeader CreateOrg(string code)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;
			return org;
		}

		void SetOrgApproval(OrgHeader org)
		{
			var approval = org.MainAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
		}

		#endregion

		#endregion

		#region Origin / Destination

		public void TestValidateJS_RL_NKOrigin_ProhibitedRouting()
		{
			var expectedWarning = "The Australian Government has imposed prohibitions on air cargo that has originated from, or transited through, Turkey. However, this prohibition applies only to electromechanical devices that weigh over 1 kilogram. You are required to meet with government requirements and/or consider a change of transport mode.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "TRAYT";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_TransportMode = "AIR";
			AssertHasWarning("Prohibited routing", shipment.JS_RL_NKOriginInfo, expectedWarning);

			shipment.JS_TransportMode = "SEA";
			AssertNoWarning("No warning for sea", shipment.JS_RL_NKOriginInfo, expectedWarning);

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = "NZAKL";
			AssertNoWarning("No warning for for non-AU discharge", shipment.JS_RL_NKOriginInfo, expectedWarning);

			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_RL_NKOrigin = "FRPAR";
			AssertNoWarning("No warning for non-prohibited load port", shipment.JS_RL_NKOriginInfo, expectedWarning);
		}

		#endregion

		public void TestValidateJS_InspectionTypeCodeShouldSkipDuringUniversalCopy()
		{
			var entityJobShipment = new EntityCopyTemplateNode
			{
				Name = "JobShipment"
			};
			entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = "JS_RL_NKDestination", CopyMethod = CopyMethod.Copy });
			entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = "JS_ShipmentType", CopyMethod = CopyMethod.Copy });
			entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = "JS_RL_NKOrigin", CopyMethod = CopyMethod.Copy });

			var relatedEntityJobShipment = new RelatedEntityCopyTemplateNode
			{
				Name = "JobShipment",
				RelatedPropertyName = "JN_JS",
				RelatedEntityTableName = "JobShipment",
				CopyMethod = RelatedEntityCopyMethod.Copy,
				InnerNode = entityJobShipment
			};

			var jobConShipLinkNode = new EntityCopyTemplateNode
			{
				Name = "JobConShipLink"
			};
			jobConShipLinkNode.Nodes.Add(relatedEntityJobShipment);

			var jobConShipLinksNode = new CollectionCopyTemplateNode
			{
				Name = "JobConShipLinks",
				ItemPropertyName = "JN_JK",
				ItemsTableName = "JobConShipLink",
				InnerNode = jobConShipLinkNode,
				CopyMethod = CollectionCopyMethod.All
			};

			var entityNode = new EntityCopyTemplateNode
			{
				Name = "JobConsol"
			};
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "JK_AgentType", CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(jobConShipLinksNode);

			var copyTree = new CopyTemplateTree
			{
				Name = "JobConsol",
				InnerNode = entityNode,
				TableName = "JobConsol"
			};

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "DRT";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals(1, consol.Shipments.Count);
			AssertEquals(1, consol.Shipments[0].Consols.Count);

			var copyManager = new BusinessObjectCopyManager();
			var copiedConsol = copyManager.Copy(consol, copyTree).Object as ForwardingConsol;

			AssertEquals(1, copiedConsol.Shipments.Count);
			AssertEquals(1, copiedConsol.Shipments[0].Consols.Count);
			AssertNoError(copiedConsol.JK_AgentTypeInfo, "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master.");

			copiedConsol.Validation.ValidateJK_AgentType();
			AssertHasError(copiedConsol.JK_AgentTypeInfo, "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master.");
		}

		public void TestUniversalCopyForDirectConsol()
		{
			var fesDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES"));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), fesDepartment.PK.ToGuid()))
			{
				var copyTree = SetUpCopyTree();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Constants.AgentType.Direct;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "HKHKG";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "HKHKG";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_ActualWeight = 3m;
				packLine.JL_PackageCount = 5;
				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals(1, consol.Shipments.Count);
				AssertEquals(1, consol.Shipments[0].Consols.Count);

				var copyManager = new BusinessObjectCopyManager();
				var copiedConsol = copyManager.Copy(consol, copyTree).Object as ForwardingConsol;

				AssertEquals(1, copiedConsol.Shipments.Count);
				AssertEquals(1, copiedConsol.Shipments[0].Consols.Count);

				copiedConsol.Validation.ValidateJK_AgentType();
				var errorMessage = "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master.";
				AssertNoError(copiedConsol.JK_AgentTypeInfo, errorMessage);

				copiedConsol.Shipments[0].JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				copiedConsol.Validation.ValidateJK_AgentType();
				AssertHasError(copiedConsol.JK_AgentTypeInfo, errorMessage);
			}

			#region SetUpCopyTree

			CopyTemplateTree SetUpCopyTree()
			{
				var entityJobShipment = new EntityCopyTemplateNode
				{
					Name = "JobShipment"
				};
				entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_ShipmentType, CopyMethod = CopyMethod.Copy });
				entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKOrigin, CopyMethod = CopyMethod.Copy });
				entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKDestination, CopyMethod = CopyMethod.Copy });
				entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_OuterPacks, CopyMethod = CopyMethod.Copy });
				entityJobShipment.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_ActualWeight, CopyMethod = CopyMethod.Copy });

				var packlineNode = new EntityCopyTemplateNode
				{
					Name = "JobPackLine"
				};
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_FreightMode, CopyMethod = CopyMethod.Copy });
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_ItemNo, CopyMethod = CopyMethod.Copy });
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_PackageCount, CopyMethod = CopyMethod.Copy });
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_F3_NKPackType, CopyMethod = CopyMethod.Copy });
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_ActualWeight, CopyMethod = CopyMethod.Copy });
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_ActualVolume, CopyMethod = CopyMethod.Copy });
				packlineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobPackLinesSchema.Constants.JL_PackageCount, CopyMethod = CopyMethod.Copy });

				entityJobShipment.Nodes.Add(new CollectionCopyTemplateNode
				{
					Name = "JobPackLines",
					Description = "Pack Lines",
					ItemPropertyName = JobPackLinesSchema.Constants.JL_JS,
					ItemsTableName = "JobPackLines",
					InnerNode = packlineNode,
					CopyMethod = CollectionCopyMethod.All
				});

				var relatedEntityJobShipment = new RelatedEntityCopyTemplateNode
				{
					Name = "JobShipment",
					RelatedPropertyName = JobConShipLinkSchema.Constants.JN_JS,
					RelatedEntityTableName = "JobShipment",
					CopyMethod = RelatedEntityCopyMethod.Copy,
					InnerNode = entityJobShipment
				};

				var jobConShipLinkNode = new EntityCopyTemplateNode
				{
					Name = "JobConShipLink"
				};
				jobConShipLinkNode.Nodes.Add(relatedEntityJobShipment);

				var jobConShipLinksNode = new CollectionCopyTemplateNode
				{
					Name = "JobConShipLinks",
					ItemPropertyName = JobConShipLinkSchema.Constants.JN_JK,
					ItemsTableName = "JobConShipLink",
					InnerNode = jobConShipLinkNode,
					CopyMethod = CollectionCopyMethod.All
				};

				var entityNode = new EntityCopyTemplateNode
				{
					Name = "JobConsol"
				};
				entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_AgentType, CopyMethod = CopyMethod.Copy });
				entityNode.Nodes.Add(jobConShipLinksNode);

				return new CopyTemplateTree
				{
					Name = "JobConsol",
					InnerNode = entityNode,
					TableName = "JobConsol"
				};
			}

			#endregion
		}

		public void TestValidateJS_CompanyTariffLevelOverride()
		{
			Factory.New<GlobalTariff>();
			Factory.New<GlobalTariff>();
			Factory.Save();
			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_CompanyTariffLevelOverride = 2;
				AssertNoErrors("If Company Tariff Level Override is disabled, we will not check Company Tariff Level Override.", shipment.JS_CompanyTariffLevelOverrideInfo);
				shipment.JS_CompanyTariffLevelOverride = 3;
				AssertNoErrors("If Company Tariff Level Override is disabled, we will not check Company Tariff Level Override.", shipment.JS_CompanyTariffLevelOverrideInfo);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_CompanyTariffLevelOverride = 2;
				AssertNoErrors("Company Tariff Level Override should not exceed the max level of Company Tariff", shipment.JS_CompanyTariffLevelOverrideInfo);
				shipment.JS_CompanyTariffLevelOverride = 3;
				AssertHasErrors("Company Tariff Level Override should not exceed the max level of Company Tariff", shipment.JS_CompanyTariffLevelOverrideInfo);
			}
		}

		public void TestValidateJS_ReleaseType()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			shipment.JS_TransportMode = "";
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasErrors(shipment.JS_ReleaseTypeInfo);

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.Cheque;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			Factory.Save();
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			Env.Security.MaintainShipmentEditReleaseType.IsAllowed = false;
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			Factory.Save();
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			AssertHasError(shipment.JS_ReleaseTypeInfo, "Security right '" +
					Env.Security.MaintainShipmentEditReleaseType.DisplayTextPathToSecurityRight +
					"' doesn't allow you to change Release Type from EBL.");
		}

		#region Weight and Volume

		public void TestValidateWeightAndVolumeAfterConsolCutOffDate()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			Factory.Save();

			consol.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddMinutes(-1);
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "LB";
			shipment.JS_ActualVolume = 2;
			shipment.JS_UnitOfVolume = "CF";

			var expectedWarning = "You have modified shipment's weight or volume after the Consol Cut Off Date.";

			AssertHasWarning(shipment.JS_ActualWeightInfo, expectedWarning);
			AssertHasWarning(shipment.JS_UnitOfWeightInfo, expectedWarning);
			AssertHasWarning(shipment.JS_ActualVolumeInfo, expectedWarning);
			AssertHasWarning(shipment.JS_UnitOfVolumeInfo, expectedWarning);

			consol.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddMinutes(1);

			shipment.JS_ActualWeight = 101;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 3;
			shipment.JS_UnitOfVolume = "M3";

			AssertNoWarning(shipment.JS_ActualWeightInfo, expectedWarning);
			AssertNoWarning(shipment.JS_UnitOfWeightInfo, expectedWarning);
			AssertNoWarning(shipment.JS_ActualVolumeInfo, expectedWarning);
			AssertNoWarning(shipment.JS_UnitOfVolumeInfo, expectedWarning);

			Env.Security.ConsolChangeWeightOrVolumeAfterCutOffDate.IsAllowed = false;

			consol.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddMinutes(-1);

			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "LB";
			shipment.JS_ActualVolume = 2;
			shipment.JS_UnitOfVolume = "CF";

			var expectedError = @"You cannot modify Shipment's weight or volume after the Consol Cut Off Date. Supervisor access is required to save the changes at this time.

You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Allow changing Shipments Weight / Volume after Consol Cut Off Date.";

			AssertHasError(shipment.JS_ActualWeightInfo, expectedError);
			AssertHasError(shipment.JS_UnitOfWeightInfo, expectedError);
			AssertHasError(shipment.JS_ActualVolumeInfo, expectedError);
			AssertHasError(shipment.JS_UnitOfVolumeInfo, expectedError);

			consol.JK_ConsolCutOffDate = ZDateTime.Now.AddMinutes(1);

			shipment.JS_ActualWeight = 101;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 3;
			shipment.JS_UnitOfVolume = "M3";

			AssertNoError(shipment.JS_ActualWeightInfo, expectedError);
			AssertNoError(shipment.JS_UnitOfWeightInfo, expectedError);
			AssertNoError(shipment.JS_ActualVolumeInfo, expectedError);
			AssertNoError(shipment.JS_UnitOfVolumeInfo, expectedError);
		}

		#endregion

		public void TestValidateJS_TransportMode()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasErrors(shipment.JS_ReleaseTypeInfo);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(shipment.JS_ReleaseTypeInfo);
		}

		public void TestPackingMode_BuyersConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			AssertNoErrors(shipment.JS_PackingModeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			AssertNoErrors(shipment.JS_PackingModeInfo);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertHasError(shipment.JS_PackingModeInfo, "Container mode must be set to BCN to mark this Shipment as Buyer's Consol Lead.");

			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var consol = shipment.Consols.AddNew();

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertNoWarnings(shipment.JS_PackingModeInfo);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			shipment.Validation.ValidateJS_PackingMode();
			AssertHasWarning(shipment.JS_PackingModeInfo, "A Shipment that is a Buyers Consol must belong to a consol of type BCN or OTH");
		}

		public void TestPackingMode_ShippersConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();

			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			shipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			AssertNoErrors(shipment.JS_PackingModeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			AssertNoErrors(shipment.JS_PackingModeInfo);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			AssertHasError(shipment.JS_PackingModeInfo, "Container mode must be set to SCN to mark this Shipment as Shipper's Consol Lead.");

			shipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;

			var consol = shipment.Consols.AddNew();

			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			AssertNoWarnings(shipment.JS_PackingModeInfo);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			shipment.Validation.ValidateJS_PackingMode();
			AssertHasWarning(shipment.JS_PackingModeInfo, "A Shipment that is a Shippers Consol must belong to a consol type of SCN");
		}

		#region Shipment Type / Container Mode validation

		public void TestSettingShipmentTypeValidatesContainerMode()
		{
			CommonShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			Factory.Save();

			AssertNoErrors(shipment.JS_PackingModeInfo);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			AssertHasError(shipment.JS_PackingModeInfo, "Container mode must be set to BCN to mark this Shipment as Buyer's Consol Lead.");
		}

		#endregion

		public void TestValidateJS_TransportMode_CargoOnlyValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.Value;
				var shipmentInspectionType = (ShipmentInspectionType)inspectionTypes.Types.FindByCode("XRY");
				shipmentInspectionType.AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_RL_NKOrigin = "JPOSA";
					shipment.JS_RL_NKDestination = "FRPAR";
					shipment.JS_TransportMode = "AIR";

					var consol = shipment.Consols.AddNew();
					var transport = consol.Transports[0];
					transport.JW_RL_NKLoadPort = "JPOSA";
					transport.JW_RL_NKDiscPort = "FRPAR";
					transport.JW_TransportMode = "AIR";
					transport.JW_IsCargoOnly = true;

					var voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = transport.JW_RL_NKLoadPort;
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = transport.JW_RL_NKDiscPort;
					voyage.GenerateSailings();

					transport.JW_JX = voyage.Sailings[0].PK;

					shipment.JS_InspectionTypeCode = "XRY";
					AssertNoErrors("XRY is valid for cargo only flights", shipment.JS_InspectionTypeCodeInfo);

					shipment.JS_InspectionTypeCode = "PHS";
					AssertNoErrors("PHS is valid for cargo only flights", shipment.JS_InspectionTypeCodeInfo);

					transport.JW_IsCargoOnly = false;

					shipment.JS_InspectionTypeCode = "XRY";
					AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Inspection Type 'XRY' is not allowed for passenger flights.");

					shipment.JS_InspectionTypeCode = "PHS";
					AssertNoErrors("PHS is allowed on passenger flights", shipment.JS_InspectionTypeCodeInfo);

					shipment.JS_RL_NKOrigin = "NZAKL";
					transport.JW_RL_NKLoadPort = "NZAKL";
					transport.JW_RL_NKDiscPort = "USCHI";

					var transport2 = consol.Transports.AddNew();
					transport2.JW_RL_NKLoadPort = "USCHI";
					transport2.JW_RL_NKDiscPort = "FRPAR";

					shipment.JS_InspectionTypeCode = "XRY";
					AssertNoErrors("We're not concerned about transhipments", shipment.JS_InspectionTypeCodeInfo);
				}
			}
		}

		public void TestValidateJS_InspectionType_RA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();

				try
				{
					orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
					orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = "AIR";
					shipment.JS_RL_NKOrigin = "AUBNE";
					shipment.JS_RL_NKDestination = "NZAKL";

					shipment.JS_InspectionTypeCode = "XRY";
					AssertNoErrors("XRY", shipment.JS_InspectionTypeCodeInfo);

					shipment.JS_InspectionTypeCode = "BIO";
					AssertNoErrors("BIO", shipment.JS_InspectionTypeCodeInfo);

					orgProxyApproval.OV_EXApprovedOrMajorExporter = "RE";

					shipment.JS_InspectionTypeCode = "PHS";
					AssertNoErrors("PHS", shipment.JS_InspectionTypeCodeInfo);

					shipment.JS_InspectionTypeCode = "DIP";
					AssertNoErrors("DIP", shipment.JS_InspectionTypeCodeInfo);
				}
				finally
				{
					orgProxy.MainAddress.KnownShipperDetails.Delete(orgProxyApproval);
					Factory.Save();
				}
			}
		}

		public void TestValidateJS_InspectionType_AACA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();

				try
				{
					orgProxyApproval.OV_EXApprovedOrMajorExporter = "AA";
					orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = "AIR";
					shipment.JS_RL_NKOrigin = "AUBNE";
					shipment.JS_RL_NKDestination = "NZAKL";

					shipment.JS_InspectionTypeCode = "XRY";
					AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, "cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

					shipment.JS_InspectionTypeCode = "CMD";
					AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, "cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

					shipment.JS_InspectionTypeCode = "APP";
					AssertNoErrorContaining(shipment.JS_InspectionTypeCodeInfo, "cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

					shipment.JS_InspectionTypeCode = "UNK";
					AssertNoErrors("UNK", shipment.JS_InspectionTypeCodeInfo);

					shipment.JS_InspectionTypeCode = "BIO";
					AssertNoErrors("BIO", shipment.JS_InspectionTypeCodeInfo);

					shipment.JS_InspectionTypeCode = "DIP";
					AssertNoErrors("DIP", shipment.JS_InspectionTypeCodeInfo);
				}
				finally
				{
					orgProxy.MainAddress.KnownShipperDetails.Delete(orgProxyApproval);
					Factory.Save();
				}
			}
		}

		public void TestValidateJS_InspectionType_NotApproved()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var knownShipper = consignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipper.OV_OH_OrgHeader = consignor.PK;
				knownShipper.OV_RN_NKClientCountryRelation = "AU";
				knownShipper.OV_EXApprovedOrMajorExporter = "YES";
				knownShipper.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "NZAKL";

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasError("XRY", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of XRY cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				shipment.JS_InspectionTypeCode = "CMD";
				AssertHasError("CMD", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of CMD cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				shipment.JS_InspectionTypeCode = "BIO";
				AssertHasError("BIO", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of BIO cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				shipment.JS_InspectionTypeCode = "DIP";
				AssertHasError("DIP", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of DIP cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError("APP", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of APP cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors("UNK", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		#region JS_AdditionalInspectionTypeCode

		public void TestValidateJS_AdditionalInspectionTypeCode_ScreenedMethod()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_IsHighRisk = true;

				AssertEquals("Precondition: non-high risk shipment", FreightDataRegistry.AviationSecurity_Unknown_Code, shipment.JS_AdditionalInspectionTypeCode);
				shipment.JS_AdditionalInspectionTypeCode = BaseJobShipmentLookups.InspectionType_Screened;
				AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, "SCR - Screened status cannot be used as there are no high-risk packlines.");

				var packline1 = shipment.OuterPackLines.AddNew();
				AssertEquals(FreightDataRegistry.AviationSecurity_Unknown_Code, packline1.JL_AdditionalInspectionTypeCode);

				shipment.JS_AdditionalInspectionTypeCode = BaseJobShipmentLookups.InspectionType_Screened;
				AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, "SCR - Screened status cannot be used as there are no high-risk packlines.");

				packline1.JL_IsHighRisk = true;
				AssertEquals(FreightDataRegistry.AviationSecurity_Unknown_Code, packline1.JL_AdditionalInspectionTypeCode);
				AssertEquals("Shipment Additional Inspection Type is updated by packline1", FreightDataRegistry.AviationSecurity_Unknown_Code, shipment.JS_AdditionalInspectionTypeCode);
				AssertNoError(shipment.JS_AdditionalInspectionTypeCodeInfo, "SCR - Screened status cannot be used as there are no high-risk packlines.");
				shipment.JS_AdditionalInspectionTypeCode = BaseJobShipmentLookups.InspectionType_Screened;
				AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, "Additional Inspection method SCR – Screened means each high-risk packline of the shipment is screened. You have high-risk packlines with UNK status. Either ensure each high-risk packline has an additional inspection entered that is not UNK - Unknown, or change shipment Additional Inspection to UNK until all packs are screened.");

				shipment.JS_IsForwardRegistered = true;
				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				AssertHasWarning(shipment.JS_AdditionalInspectionTypeCodeInfo, "Shipment additional inspection states PHS, however some high-risk packlines are UNK. Either add additional inspection value to each high-risk packline, or change Shipment Inspection to UNK until all packs are screened.");

				packline1.JL_AdditionalInspectionTypeCode = "VCK";
				AssertEquals("Shipment Additional Inspection Type is updated by packline1", "VCK", shipment.JS_AdditionalInspectionTypeCode);
				AssertNoErrors(shipment.JS_AdditionalInspectionTypeCodeInfo);
				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, "Shipment additional inspection states PHS, however all high-risk packlines are screened by VCK. Change Shipment Additional Inspection to match high-risk packline additional inspection type.");

				shipment.JS_AdditionalInspectionTypeCode = "VCK";
				AssertNoErrors(shipment.JS_AdditionalInspectionTypeCodeInfo);

				var packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_IsHighRisk = true;
				AssertEquals("Shipment Additional Inspection Type is updated to UNK", FreightDataRegistry.AviationSecurity_Unknown_Code, shipment.JS_AdditionalInspectionTypeCode);
				shipment.JS_AdditionalInspectionTypeCode = BaseJobShipmentLookups.InspectionType_Screened;
				AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, "Additional Inspection method SCR – Screened means each high-risk packline of the shipment is screened. You have high-risk packlines with UNK status. Either ensure each high-risk packline has an additional inspection entered that is not UNK - Unknown, or change shipment Additional Inspection to UNK until all packs are screened.");

				packline2.JL_AdditionalInspectionTypeCode = "PHS";
				AssertEquals(BaseJobShipmentLookups.InspectionType_Screened, shipment.JS_AdditionalInspectionTypeCode);
				AssertNoErrors(shipment.JS_AdditionalInspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_AdditionalInspectionTypeCode_MandatoryValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var expectedError = "Please enter an Additional Inspection.";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_IsHighRisk = false;

				AssertEquals("Precondition: non-high risk shipment", "UNK", shipment.JS_AdditionalInspectionTypeCode);
				AssertNoError("Non-high risk shipment", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "";
				AssertNoError("Non-high risk shipment", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_IsHighRisk = true;
				shipment.JS_AdditionalInspectionTypeCode = "UNK";
				AssertNoError("High risk shipment", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "";
				AssertHasError("High risk shipment", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				AssertNoError("High risk shipment", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);
			}
		}

		public void TestValidateJS_AdditionalInspectionTypeCode_WithNoErrorForExistingShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var invalidCodeError = "Enter a valid Additional Inspection.";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_IsHighRisk = true;

				shipment.JS_AdditionalInspectionTypeCode = "XRY";
				AssertNoError("XRY is valid", shipment.JS_AdditionalInspectionTypeCodeInfo, invalidCodeError);

				shipment.JS_AdditionalInspectionTypeCode = "RES";
				AssertHasError("RES is invalid for existing shipment", shipment.JS_AdditionalInspectionTypeCodeInfo, invalidCodeError);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);

				AssertEquals("RES", reloadedShipment.JS_AdditionalInspectionTypeCode);
				reloadedShipment.Validation.ValidateJS_AdditionalInspectionTypeCode();
				AssertNoError("Existing shipment can have invalid code. (For instance, code has been disabled since shipment creation)", reloadedShipment.JS_AdditionalInspectionTypeCodeInfo, invalidCodeError);

				reloadedShipment.JS_AdditionalInspectionTypeCode = "FRD";
				AssertHasError("Invalid code, should have error", reloadedShipment.JS_AdditionalInspectionTypeCodeInfo, invalidCodeError);
			}
		}

		public void TestValidateAdditionalInspectionTypeAllowedForPassengerFlights()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_EU.Value;
				var shipmentInspectionType = (ShipmentInspectionType)inspectionTypes.Types.FindByCode("XRY");
				shipmentInspectionType.AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypes_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_RL_NKOrigin = "FRPAR";
					shipment.JS_RL_NKDestination = "JPOSA";
					shipment.JS_TransportMode = "AIR";
					shipment.JS_IsHighRisk = true;
					shipment.JS_InspectionTypeCode = "ETD";

					var consol = shipment.Consols.AddNew();
					var transport = consol.Transports[0];
					transport.JW_RL_NKLoadPort = "FRPAR";
					transport.JW_RL_NKDiscPort = "JPOSA";
					transport.JW_TransportMode = "AIR";
					transport.JW_IsCargoOnly = true;

					var voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = transport.JW_RL_NKLoadPort;
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = transport.JW_RL_NKDiscPort;
					voyage.GenerateSailings();

					transport.JW_JX = voyage.Sailings[0].PK;

					shipment.JS_AdditionalInspectionTypeCode = "XRY";
					AssertNoErrors("XRY is valid for cargo only flights", shipment.JS_AdditionalInspectionTypeCodeInfo);

					shipment.JS_AdditionalInspectionTypeCode = "PHS";
					AssertNoErrors("PHS is valid for cargo only flights", shipment.JS_AdditionalInspectionTypeCodeInfo);

					transport.JW_IsCargoOnly = false;

					shipment.JS_AdditionalInspectionTypeCode = "XRY";
					AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, "Inspection Type 'XRY' is not allowed for passenger flights.");

					shipment.JS_AdditionalInspectionTypeCode = "PHS";
					AssertNoErrors("PHS is allowed on passenger flights", shipment.JS_AdditionalInspectionTypeCodeInfo);

					shipment.JS_AdditionalInspectionTypeCode = "UNK";
					AssertNoErrors("UNK is allowed on passenger flights. (It means 'not inspected yet')", shipment.JS_AdditionalInspectionTypeCodeInfo);
				}
			}
		}

		public void TestValidateJS_AdditionalInspectionTypeCode_NotTheSameAsInspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				const string expectedError = "Aviation Security Inspection and Additional Inspection cannot be the same.";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "JPOSA";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_IsHighRisk = true;
				shipment.JS_InspectionTypeCode = "PHS";
				shipment.JS_AdditionalInspectionTypeCode = "XRY";

				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertHasError(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "UNK";
				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_InspectionTypeCode = "SCR";
				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "SCR";
				AssertNoError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertNoError(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);
			}
		}

		#endregion

		#region TestValidate_JS_Inco

		public void TestJS_INCO_Validation_Forced_On_Setter()
		{
			var shipment = (ForwardingShipment)GetShipment();
			shipment.JS_RL_NKDestination = "USLAX";

			Assert(!shipment.JS_INCOInfo.HasErrors());

			shipment.JS_INCO = "-";

			Assert(shipment.JS_INCOInfo.HasErrors());
		}

		public void TestJS_INCO_Validation_Mandatory()
		{
			var shipment = (ForwardingShipment)GetShipment();
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "";

			AssertEquals("Precondition", true, shipment.IsExport());

			shipment.Validation.ValidateJS_INCO();
			AssertNoErrors(shipment.JS_INCOInfo);

			FreightConfigurationRegistry.Instance.MandatoryIncoTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			string error = "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Shipment -> Incoterm Mandatory.";
			shipment.Validation.ValidateJS_INCO();
			AssertEquals(true, shipment.JS_INCOInfo.HasError(error));

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("Precondition", false, shipment.IsExport());

			shipment.Validation.ValidateJS_INCO();
			AssertNoErrors(shipment.JS_INCOInfo);
		}

		#endregion

		#region Validate Brokers

		public void TestValidateJS_OH_ImportBroker_Forced_On_Setter()
		{
			var broker = Factory.New<OrgHeader>();
			broker.OH_IsBroker = true;

			var notBroker = Factory.New<OrgHeader>();
			notBroker.OH_IsBroker = false;

			var shipment = (ForwardingShipment)GetShipment();

			Assert(!shipment.JS_OH_ImportBrokerInfo.HasErrors());

			shipment.JS_OH_ImportBroker = ZGuid.NewZGuid();

			Assert(shipment.JS_OH_ImportBrokerInfo.HasErrors());
		}

		public void TestValidateJS_OH_ExportBroker()
		{
			var broker = Factory.New<OrgHeader>();
			broker.OH_IsBroker = true;

			var notBroker = Factory.New<OrgHeader>();
			notBroker.OH_IsBroker = false;

			var shipment = (ForwardingShipment)GetShipment();
			shipment.JS_OH_ExportBroker = ZGuid.NewZGuid();
			AssertEquals("Must be a broker organization", true, shipment.JS_OH_ExportBrokerInfo.HasErrors());

			shipment.JS_OH_ExportBroker = notBroker.PK;
			AssertEquals("Must be a broker organization", true, shipment.JS_OH_ExportBrokerInfo.HasErrors());

			shipment.JS_OH_ExportBroker = broker.PK;
			AssertEquals(false, shipment.JS_OH_ExportBrokerInfo.HasErrors());
		}

		public void TestValidateJS_OH_ImportBroker()
		{
			var broker = Factory.New<OrgHeader>();
			broker.OH_IsBroker = true;

			var notBroker = Factory.New<OrgHeader>();
			notBroker.OH_IsBroker = false;

			var shipment = (ForwardingShipment)GetShipment();
			shipment.JS_OH_ImportBroker = ZGuid.NewZGuid();
			AssertEquals("Must be a broker organization", true, shipment.JS_OH_ImportBrokerInfo.HasErrors());

			shipment.JS_OH_ImportBroker = notBroker.PK;
			AssertEquals("Must be a broker organization", true, shipment.JS_OH_ImportBrokerInfo.HasErrors());

			shipment.JS_OH_ImportBroker = broker.PK;
			AssertEquals(false, shipment.JS_OH_ImportBrokerInfo.HasErrors());
		}

		public void TestValidateJS_OH_ImportBroker_MandatoryImportBroker()
		{
			string error = "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Shipment -> Import Broker Mandatory.";

			var shipment = (ForwardingShipment)GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_OH_ImportBroker = ZGuid.Empty;

			using (FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", false, shipment.IsImport());
				shipment.Validation.ValidateJS_OH_ImportBroker();
				AssertEquals(false, shipment.JS_OH_ImportBrokerInfo.HasError(error));

				shipment.JS_RL_NKOrigin = "USLAX";
				AssertEquals("Precondition", true, shipment.IsImport());

				shipment.Validation.ValidateJS_OH_ImportBroker();
				AssertEquals(true, shipment.JS_OH_ImportBrokerInfo.HasError(error));
			}

			using (FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				shipment.Validation.ValidateJS_OH_ImportBroker();
				AssertEquals(false, shipment.JS_OH_ImportBrokerInfo.HasError(error));
			}
		}

		public void TestValidateJS_OH_ImportBroker__ShouldNotRaiseError_ForQuickBooking_WhenImportBrokerIsMandatory()
		{
			string error = "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Shipment -> Import Broker Mandatory.";
			var booking = (ForwardingShipment)GetShipment();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_RL_NKDestination = "AUMEL";
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_OH_ImportBroker = ZGuid.Empty;

			AssertEquals("Precondition", false, booking.IsImport());

			using (FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				booking.JS_RL_NKOrigin = "USLAX";
				AssertEquals("Precondition", true, booking.IsImport());

				booking.Validation.ValidateJS_OH_ImportBroker();
				AssertEquals(false, booking.JS_OH_ImportBrokerInfo.HasError(error));
			}
		}

		public void TestValidateJS_OH_ImportBroker__ShouldRaiseError_ForQuickBookingConvertedToShipment_WhenImportBrokerIsMandatory()
		{
			string error = "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Shipment -> Import Broker Mandatory.";
			var booking = (ForwardingShipment)GetShipment();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_RL_NKDestination = "AUSYD";
			booking.JS_RL_NKOrigin = "USLAX";
			booking.JS_OH_ImportBroker = ZGuid.Empty;

			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.Bulk;
			booking.JS_ActualWeight = 10000m;
			booking.JS_IsDirectBooking = false;
			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(booking, null);

			using (FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", true, booking.IsImport());
				booking.Validation.ValidateJS_OH_ImportBroker();
				AssertEquals(true, booking.JS_OH_ImportBrokerInfo.HasError(error));
			}

			using (FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Precondition", true, booking.IsImport());
				booking.Validation.ValidateJS_OH_ImportBroker();
				AssertEquals(false, booking.JS_OH_ImportBrokerInfo.HasError(error));
			}
		}

		public void TestValidateImportAndExportBrokersCouldNotBeTheSame()
		{
			var broker1 = Factory.New<OrgHeader>();
			broker1.OH_IsBroker = true;

			var broker2 = Factory.New<OrgHeader>();
			broker2.OH_IsBroker = true;

			var shipment = (ForwardingShipment)GetShipment();

			Action<string, bool> assertBrokers = (message, isErrorExpected) =>
				{
					AssertEquals(message, isErrorExpected, shipment.JS_OH_ImportBrokerInfo.HasErrors());
					AssertEquals(message, isErrorExpected, shipment.JS_OH_ExportBrokerInfo.HasErrors());
				};

			shipment.JS_OH_ImportBroker = ZGuid.Empty;
			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			assertBrokers("Brokers are not defined, should be no errors", false);

			shipment.JS_OH_ImportBroker = broker1.PK;
			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			assertBrokers("Brokers are not the same, should be no errors", false);

			shipment.JS_OH_ImportBroker = ZGuid.Empty;
			shipment.JS_OH_ExportBroker = broker1.PK;
			assertBrokers("Brokers are not the same, should be no errors", false);

			shipment.JS_OH_ImportBroker = broker1.PK;
			shipment.JS_OH_ExportBroker = broker1.PK;
			assertBrokers("Brokers are the same, error expected", true);

			shipment.JS_OH_ImportBroker = broker1.PK;
			shipment.JS_OH_ExportBroker = broker2.PK;
			assertBrokers("Brokers are not the same, should be no errors", false);
		}

		#endregion

		#region TestValidateShipmentNotTravellingInReverseOfAnyConsols

		public void TestValidateShipmentNotTravellingInReverseOfAnyConsols()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetShipment();
			shipment.Consols.RemoveAll();
			shipment.RunPreSaveValidation();
			AssertEquals("should not have any errors", false, shipment.JS_RL_NKOriginInfo.HasErrors());

			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.RunPreSaveValidation();
			AssertEquals("should not have any errors", false, shipment.JS_RL_NKOriginInfo.HasErrors());

			CommonConsol consolSGGB = shipment.Consols.AddNew();
			consolSGGB.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transportSGGB = consolSGGB.Transports[0];
			transportSGGB.JW_RL_NKLoadPort = "SGSIN";
			transportSGGB.JW_RL_NKDiscPort = "GBLON";

			shipment.RunPreSaveValidation();
			AssertEquals("should not have any errors", false, shipment.JS_RL_NKOriginInfo.HasErrors());

			CommonConsol consolNZSG = shipment.Consols.AddNew();
			consolNZSG.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transportNZSG = consolNZSG.Transports[0];
			transportNZSG.JW_RL_NKLoadPort = "NZAKL";
			transportNZSG.JW_RL_NKDiscPort = "SGSIN";

			shipment.RunPreSaveValidation();
			AssertEquals("should have an error - shipment is the reverse of one of its consols", true, shipment.JS_RL_NKOriginInfo.HasErrors());

			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.RunPreSaveValidation();
			AssertEquals("should not have any errors", false, shipment.JS_RL_NKOriginInfo.HasErrors());
		}

		#endregion

		#region Test Forwarding Dates

		public void TestValidateJS_E_ARV()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetShipment();
			ZDateTime currentTime = ZDateTime.Now;

			ForwardingConsol consol = GetConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_ETA = currentTime.AddDays(2);

			consol.Shipments.Add(shipment);

			shipment.JS_E_ARV = currentTime;
			AssertEquals("Shipment ETA before Consol ETA. Should be error.", true, shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddDays(2);
			AssertEquals("Shipment ETA the same as Consol ETA. Shouldn't be error.", false, shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddDays(4);
			AssertEquals("Shipment ETA the after a Consol ETA. Shouldn't be error.", false, shipment.JS_E_ARVInfo.HasWarnings());

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_ETA = currentTime.AddDays(4);

			shipment.JS_E_ARV = currentTime;
			AssertEquals("Shipment ETA before multiple Consols ETA's. Should be error.", true, shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddDays(3);
			AssertEquals("Shipment ETA before inbetween 2 Consol ETA's. Should be error.", true, shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddDays(5);
			AssertEquals("Shipment ETA after mulitiple Consol ETA's. Shouldn't be error.", false, shipment.JS_E_ARVInfo.HasWarnings());

			transport.JW_ETA = currentTime.AddMonths(4);
			transport2.JW_ETA = currentTime.AddMonths(4).AddDays(15);
			shipment.JS_E_ARV = currentTime.AddMonths(7).AddDays(16);
			Assert("Shipment ETA cannot be more than 3 months after a Consol's ETA. Should be an error", shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddMonths(7).AddDays(1);
			Assert("Shipment ETA cannot be more than 3 months after a Consol's ETA. Should be an error", shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddMonths(7).AddDays(14);
			Assert("Shipment ETA cannot be more than 3 months after a Consol's ETA. Should be an error", shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = consol.JK_JX_JB_E_ARV.AddMonths(3);
			Assert("Shipment ETA exactly 3 months of a Consol's ETA. Shouldn't be an error", !shipment.JS_E_ARVInfo.HasWarnings());
			shipment.JS_E_ARV = currentTime.AddDays((7 * 30) - 1);
			Assert("Shipment ETA within 3 months of a Consol's ETA. Shouldn't be an error", !shipment.JS_E_ARVInfo.HasWarnings());
		}

		public void TestValidateJS_E_DEP()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetShipment();
			ZDateTime currentTime = ZDateTime.Now;

			ForwardingConsol consol = GetConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_ETD = currentTime.AddDays(4);
			consol.Shipments.Add(shipment);

			shipment.JS_E_DEP = currentTime;
			AssertEquals("Shipment ETD before Consol ETD. Shouldn't be an error.", false, shipment.JS_E_DEPInfo.HasWarnings());
			shipment.JS_E_DEP = currentTime.AddDays(4);
			AssertEquals("Shipment ETD the same as Consol ETD. Shouldn't be an error.", false, shipment.JS_E_DEPInfo.HasWarnings());
			shipment.JS_E_DEP = currentTime.AddDays(5);
			AssertEquals("Shipment ETD the after a Consol ETD. Should be error.", true, shipment.JS_E_DEPInfo.HasWarnings());

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_ETD = currentTime.AddDays(8);

			shipment.JS_E_DEP = currentTime;
			AssertEquals("Shipment ETD before multiple Consols ETD's. Shouldn't be an error.", false, shipment.JS_E_DEPInfo.HasWarnings());
			shipment.JS_E_DEP = currentTime.AddDays(5);
			AssertEquals("Shipment ETD before inbetween 2 Consol ETD's. Should be an error.", true, shipment.JS_E_DEPInfo.HasWarnings());
			shipment.JS_E_DEP = currentTime.AddDays(9);
			AssertEquals("Shipment ETD after mulitiple Consol ETD's. Should be an error.", true, shipment.JS_E_DEPInfo.HasWarnings());

			transport.JW_ETD = currentTime.AddMonths(4);
			transport2.JW_ETD = currentTime.AddMonths(4).AddDays(15);
			shipment.JS_E_DEP = currentTime;
			Assert("Shipment ETD cannot be more than 3 months before a Consol's ETD. Should be an error", shipment.JS_E_DEPInfo.HasWarnings());
			shipment.JS_E_DEP = currentTime.AddMonths(1).AddDays(3);
			Assert("Shipment ETD cannot be more than 3 months before a Consol's ETD. Should be an error", shipment.JS_E_DEPInfo.HasWarnings());
			shipment.JS_E_DEP = currentTime.AddMonths(1).AddDays(20);
			Assert("Shipment ETD less than 3 months before a Consol's ETD. Shouldn't be an error", !shipment.JS_E_DEPInfo.HasWarnings());
		}

		#endregion

		#region Chargeables

		public void TestJS_ActualChargeableMinimumWeightAtSea()
			=> TestChargeableMinimumWeightAtSea(nameof(ForwardingShipment.JS_ActualChargeable));

		public void TestJS_DocumentedChargeableMinimumWeightAtSea()
			=> TestChargeableMinimumWeightAtSea(nameof(ForwardingShipment.JS_DocumentedChargeable));

		public void TestJS_ManifestedChargeableMinimumWeightAtSea()
			=> TestChargeableMinimumWeightAtSea(nameof(ForwardingShipment.JS_ManifestedChargeable));

		void TestChargeableMinimumWeightAtSea(string chargeablePropertyName)
		{
			AssertEquals("Precondition: Default to zero (no minimum)", 0.0M, FreightDataRegistry.Instance.SeaMinimumChargeableWeight.Value);

			ForwardingShipment shipment = (ForwardingShipment)GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment[chargeablePropertyName] = 200M;
			AssertEquals("Min ChargeWt should NOT have kicked in", 200M, shipment[chargeablePropertyName]);

			FreightDataRegistry.Instance.SeaMinimumChargeableWeight.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 500);
			shipment[chargeablePropertyName] = 200M;
			AssertEquals("Min ChargeWt should have kicked in", 500M, shipment[chargeablePropertyName]);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment[chargeablePropertyName] = 125M;
			AssertEquals("Min ChargeWt should NOT have kicked in", 125M, shipment[chargeablePropertyName]);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Min ChargeWt should have kicked in", 500M, shipment[chargeablePropertyName]);
		}

		public void TestJS_ActualChargeable_RoundingAir()
		{
			ChargeableWeightRoundingCollection collection = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;
			ForwardingShipment shipment = (ForwardingShipment)GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualChargeable = 12.43M;
			AssertEquals("Default behavious - no rounding", 12.43M, shipment.JS_ActualChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			shipment.JS_ActualChargeable = 12.23M;
			AssertEquals(12M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.99M;
			AssertEquals(12M, shipment.JS_ActualChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			shipment.JS_ActualChargeable = 12.49M;
			AssertEquals(12M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.51M;
			AssertEquals(12.5M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.99M;
			AssertEquals(12.5M, shipment.JS_ActualChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			shipment.JS_ActualChargeable = 12.01M;
			AssertEquals(13M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.5M;
			AssertEquals(13M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.99M;
			AssertEquals(13M, shipment.JS_ActualChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			shipment.JS_ActualChargeable = 12.01M;
			AssertEquals(12.5M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.51M;
			AssertEquals(13M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.99M;
			AssertEquals(13M, shipment.JS_ActualChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.None);
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			shipment.JS_ActualChargeable = 12.53M;
			AssertEquals(12.53M, shipment.JS_ActualChargeable);
			shipment.JS_ActualChargeable = 12.97M;
			AssertEquals(12.97M, shipment.JS_ActualChargeable);
		}

		public void TestDocumentedManifestedChargeable_RoundingAir()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 40m;
			AssertEquals("Default behavious - no rounding", 18.144M, shipment.JS_ActualChargeable);
			AssertEquals("Default behavious - no rounding", 18.144M, shipment.JS_DocumentedChargeable);
			AssertEquals("Default behavious - no rounding", 18.144M, shipment.JS_ManifestedChargeable);

			ChargeableWeightRoundingCollection collection = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;
			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			shipment.JS_ActualWeight = 0m;
			shipment.JS_ActualWeight = 40m;
			AssertEquals("Rounding up to 0.5", 18.5M, shipment.JS_ActualChargeable);
			AssertEquals("Rounding up to 0.5", 18.5M, shipment.JS_DocumentedChargeable);
			AssertEquals("Rounding up to 0.5", 18.5M, shipment.JS_ManifestedChargeable);

			shipment.JS_ActualChargeable = 12.43m;
			AssertEquals("Rounding up to 0.5", 12.5M, shipment.JS_ActualChargeable);
			AssertEquals("Rounding up to 0.5", 12.5M, shipment.JS_DocumentedChargeable);
			AssertEquals("Rounding up to 0.5", 12.5M, shipment.JS_ManifestedChargeable);
		}

		#endregion

		#region ValidateCusentryNumber

		public void TestValidateCusEntryNumber()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.CustomsEntryNumberType = "CAN";
			shipment.CustomsEntryNumber = "12345";
			Assert("Expecting Message Errors", shipment.CustomsEntryNumberInfo.HasMessageErrors());
			shipment.CustomsEntryNumber = "AAAAJH4EF";
			Assert("Not Expecting Message Errors", !shipment.CustomsEntryNumberInfo.HasMessageErrors());
		}

		public void TestValidateCusEntryNumberMustBeNumericForNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.CustomsEntryNumberType = "FRM";
			shipment.CustomsEntryNumber = "NZF 23456789";
			AssertHasWarning("NZ Entry Number cannot contain alpha characters", shipment.CustomsEntryNumberInfo, "NZ Customs Entry Number can contain numbers only. Non-numeric characters will be stripped out of any NZ message sent.");
			shipment.CustomsEntryNumber = "23456789";
			AssertNoWarning("NZ Entry Number is now valid", shipment.CustomsEntryNumberInfo, "NZ Customs Entry Number can contain numbers only. Non-numeric characters will be stripped out of any NZ message sent.");
		}

		public void TestCANWhenCoLoadMasterHasCAN()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment coLoad1 = Factory.New<ForwardingShipment>();
			ForwardingShipment coLoad2 = Factory.New<ForwardingShipment>();
			coLoad1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			coLoad2.JS_JS_ColoadMasterShipment = coLoad1.PK;
			coLoad1.CustomsEntryNumberType = "CAN";
			coLoad1.CustomsEntryNumber = "AAAAJH4EF";
			coLoad2.CustomsEntryNumberType = "CAN";
			coLoad2.CustomsEntryNumber = "";
			Assert(!coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
			coLoad2.CustomsEntryNumber = "1";
			Assert(coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
		}

		public void TestCANWhenCoLoadMasterHasNoCAN()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment coLoad1 = Factory.New<ForwardingShipment>();
			ForwardingShipment coLoad2 = Factory.New<ForwardingShipment>();
			coLoad2.JS_JS_ColoadMasterShipment = coLoad1.PK;
			coLoad1.CustomsEntryNumberType = "CAN";
			coLoad1.CustomsEntryNumber = "";
			coLoad2.CustomsEntryNumberType = "CAN";
			coLoad2.CustomsEntryNumber = "";
			Assert(coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
		}

		public void TestCANWhenCoLoadsHaveCAN()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment coLoad1 = Factory.New<ForwardingShipment>();
			ForwardingShipment coLoad2 = Factory.New<ForwardingShipment>();
			coLoad1.JS_JS_ColoadMasterShipment = coLoad2.PK;
			coLoad1.CustomsEntryNumberType = "CAN";
			coLoad1.CustomsEntryNumber = "AAAAJH4EF";
			coLoad2.CustomsEntryNumberType = "CAN";
			coLoad2.CustomsEntryNumber = "";
			Assert(!coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
			coLoad2.CustomsEntryNumber = "1";
			Assert(coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
		}

		public void TestCANWhenOneCoLoadHasNoCAN()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment coLoad1 = Factory.New<ForwardingShipment>();
			ForwardingShipment coLoad2 = Factory.New<ForwardingShipment>();
			ForwardingShipment coLoad3 = Factory.New<ForwardingShipment>();
			coLoad1.JS_JS_ColoadMasterShipment = coLoad2.PK;
			coLoad3.JS_JS_ColoadMasterShipment = coLoad2.PK;
			coLoad1.CustomsEntryNumberType = "CAN";
			coLoad1.CustomsEntryNumber = "AAAAJH4EF";
			coLoad3.CustomsEntryNumberType = "CAN";
			coLoad3.CustomsEntryNumber = "";
			coLoad2.CustomsEntryNumberType = "CAN";
			coLoad2.CustomsEntryNumber = "";
			Assert(coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
			coLoad3.CustomsEntryNumber = "AAAAJH4EF";
			coLoad2.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
			Assert(!coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
			coLoad2.CustomsEntryNumber = "1";
			Assert(coLoad2.CustomsEntryNumberInfo.HasMessageErrors());
		}

		#endregion

		#region TestValidateHouseBillOfLadingType

		public void TestValidateHouseBillOfLadingType()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("precondition:", true, shipment.Lookups.JS_HouseBillOfLadingType_List.Count > 0);

			shipment.JS_IsBooking = false;
			shipment.JS_HouseBillOfLadingType = "XXX";
			AssertHasErrors(shipment.JS_HouseBillOfLadingTypeInfo);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.Validation.ValidateJS_HouseBillOfLadingType();
			AssertNoErrors(shipment.JS_HouseBillOfLadingTypeInfo);

			shipment.JS_IsBooking = true;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.Validation.ValidateJS_HouseBillOfLadingType();
			AssertNoErrors(shipment.JS_HouseBillOfLadingTypeInfo);

			shipment.JS_HouseBillOfLadingType = shipment.Lookups.JS_HouseBillOfLadingType_List[0].Code;
			AssertNoErrors(shipment.JS_HouseBillOfLadingTypeInfo);

			shipment.JS_IsBooking = false;
			shipment.Validation.ValidateJS_HouseBillOfLadingType();
			AssertNoErrors(shipment.JS_HouseBillOfLadingTypeInfo);
		}

		public void TestValidateHouseBillLengthForBrazilAir()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0002001";
			shipment.JS_HouseBill = "BILL2001";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRGIG";

			Factory.Save();
			AssertNoWarnings(shipment.JS_HouseBillInfo);

			shipment.JS_HouseBill = "BILL20011111111";
			AssertHasWarnings("HAWB number should be 11 characters or less when destined to Brazil to comply with Cargo Control and Transit (CCT) system requirements.", shipment.JS_HouseBillInfo);
		}

		#endregion

		#region ConsigneeNameOrPK Validation

		public void TestConsigneeNameOrPKValidationWhenAir()
		{
			var error = "This Organization has no address entered.";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			System.ComponentModel.IBindingList shipments = consol.Shipments;
			ForwardingShipment shipment = (ForwardingShipment)shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationNameOrPK();
			AssertHasError(shipment.ConsigneeNameOrPKInfo, error);

			shipment.ConsigneeNameOrPK = "Crapppo";
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationNameOrPK();
			AssertHasError(shipment.ConsigneeNameOrPKInfo, error);

			shipment.ConsigneeNameOrPK = consignee.PK.ToString();
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationNameOrPK();
			AssertNoErrors(shipment.ConsigneeNameOrPKInfo);
			AssertEquals("Documentary Address has no errors", ZBool.False, shipment.ConsigneeDocumentaryAddress.HasErrors);
			AssertEquals("Delivery Address has no errors", ZBool.False, shipment.ConsigneeDeliveryAddress.HasErrors);
		}

		public void TestConsignee_CheckDeliveryCartageCoPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsLocalTransport = false;
			Factory.Save();

			var relation = Factory.New<OrgRelatedParty>();
			relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relation.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relation.PR_OH_Parent = org1.PK;
			relation.PR_OH_RelatedParty = org2.PK;
			relation.PR_FreightTransportMode = "ALL";
			AssertHasError(relation.PR_OH_RelatedPartyInfo, $"Only an organization set up as a Carrier and flagged as Road Transport can be used as a {RelatedPartyTypeList.Descriptions.LocalTransport}.");
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(true, shipment.JS_IsForwardRegistered);

			shipment.DocsAndCartage.Validation.ValidateAll();
			AssertHasError(shipment.DocsAndCartage.DeliveryCartageCoPKInfo, "Enter a valid selection.");
			AssertHasError(shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo, "Enter a valid selection.");

			Factory.Save();
			shipment.DocsAndCartage.Validation.ValidateAll();
			AssertHasWarning(shipment.DocsAndCartage.DeliveryCartageCoPKInfo, "Enter a valid selection.");
			AssertHasWarning(shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo, "Enter a valid selection.");
		}

		#endregion

		#region ConsignorNameOrPK Validation

		public void TestConsignorNameOrPKValidationWhenAir()
		{
			var error = "This Organization has no address entered.";

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			System.ComponentModel.IBindingList shipments = consol.Shipments;
			ForwardingShipment shipment = (ForwardingShipment)shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationNameOrPK();
			AssertHasError(shipment.ConsignorNameOrPKInfo, error);

			shipment.ConsignorNameOrPK = "Crapppo";
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationNameOrPK();
			AssertHasError(shipment.ConsignorNameOrPKInfo, error);

			shipment.ConsignorNameOrPK = consignor.PK.ToString();
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationNameOrPK();
			AssertNoErrors(shipment.ConsignorNameOrPKInfo);
			AssertEquals("Documentary Address has no errors", ZBool.False, shipment.ConsignorDocumentaryAddress.HasErrors);
			AssertEquals("Delivery Address has no errors", ZBool.False, shipment.ConsignorPickupAddress.HasErrors);
		}

		#endregion

		#region Test ImportBroker / IncoTerm Validation

		public void TestImportBrokerIncoTermValidationChangingRegistryAfterSave()
		{
			FreightConfigurationRegistry.Instance.MandatoryIncoTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ForwardingShipment importShipment = Factory.New<ForwardingShipment>();
			importShipment.JS_RL_NKOrigin = "NZABY";
			importShipment.JS_RL_NKDestination = "AUAAB";

			ForwardingShipment exportShipment = Factory.New<ForwardingShipment>();
			exportShipment.JS_RL_NKOrigin = "AUAAB";
			exportShipment.JS_RL_NKDestination = "NZABY";

			importShipment.RunPreSaveValidation();
			exportShipment.RunPreSaveValidation();
			Factory.Save();
			AssertNoErrors("should have no error when registry is false", exportShipment.JS_INCOInfo);
			AssertNoErrors(importShipment.JS_OH_ImportBrokerInfo);

			FreightConfigurationRegistry.Instance.MandatoryIncoTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.MandatoryImportBroker.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			importShipment = newFactory.Load<ForwardingShipment>(importShipment.PK);
			exportShipment = newFactory.Load<ForwardingShipment>(exportShipment.PK);

			importShipment.RunPreSaveValidation();
			exportShipment.RunPreSaveValidation();

			newFactory.Save();
			AssertHasErrors("should have error since registry was changed on a saved shipment", importShipment.JS_OH_ImportBrokerInfo);
			AssertHasErrors(exportShipment.JS_INCOInfo);
		}

		#endregion

		#region JS_Phase

		public void TestValidateJS_Phase()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_Phase = "XXX";
			AssertHasErrors(shipment.JS_PhaseInfo);

			shipment.JS_Phase = "FOO";
			AssertHasErrors(shipment.JS_PhaseInfo);

			shipment.JS_Phase = PhaseConstants.Phase.ALL;
			AssertNoErrors(shipment.JS_PhaseInfo);
		}

		#endregion

		public void TestValidateJS_ShipmentType_HVLNotAllowedUnlessAppropriate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			var line1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVItem))) as IHVLVItem;
			var line2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVItem))) as IHVLVItem;

			line1.HVI_JS_LoadedOnShipment = shipment.PK;
			line2.HVI_JS_LoadedOnShipment = shipment.PK;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertHasError(shipment.JS_ShipmentTypeInfo, "HVLV Items can only be attached to 'HVL' shipment type.");

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);
		}

		public void TestMasterShipmentsWithMasterSubShipmentsShowAnError()
		{
			var masterOfNonSubShipmentErrorMessage = "Only Assembly Master shipments can have sub-shipments that are neither Standard House nor HVLV Shipper Consolidation (Legacy) shipments. Either update the sub-shipments or change this shipment type to Assembly Master.";
			var master = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var superMaster = FreightTestHelper.GetShipment<ForwardingShipment>("SUP", Constants.ShipmentTypes.AssemblyMaster, Factory);
			master.JS_JS_ColoadMasterShipment = superMaster.PK;

			AssertNoError("An assembly master can be a mastler of a co-load shipment", superMaster.JS_ShipmentTypeInfo, masterOfNonSubShipmentErrorMessage);

			superMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals("Pre-condition: normal master is still co-load", Constants.ShipmentTypes.CoLoadMaster, master.JS_ShipmentType);
			AssertHasError(superMaster.JS_ShipmentTypeInfo, masterOfNonSubShipmentErrorMessage);

			master.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			superMaster.Validation.ValidateJS_ShipmentType();

			AssertNoError(superMaster.JS_ShipmentTypeInfo, masterOfNonSubShipmentErrorMessage);

			master.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			superMaster.Validation.ValidateJS_ShipmentType();

			AssertNoError(superMaster.JS_ShipmentTypeInfo, masterOfNonSubShipmentErrorMessage);

			master.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			superMaster.Validation.ValidateJS_ShipmentType();

			AssertHasError(superMaster.JS_ShipmentTypeInfo, masterOfNonSubShipmentErrorMessage);

			superMaster.CoLoadShipments.RemoveAndDeleteAll();
			superMaster.Validation.ValidateJS_ShipmentType();

			AssertNoError("No sub shipment, no error", superMaster.JS_ShipmentTypeInfo, masterOfNonSubShipmentErrorMessage);
		}

		public void TestMasterShipmentAsmWithSubShipments()
		{
			var messageError = "Only Assembly Master shipments can have sub-shipments that are neither Standard House nor HVLV Shipper Consolidation (Legacy) shipments.";

			var master = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var subShipment = FreightTestHelper.GetShipment("SUB", master, Constants.ShipmentTypes.StandardHouse, Factory);

			subShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			subShipment.Validation.ValidateJS_ShipmentType();

			AssertNoNotifications(messageError, subShipment.JS_ShipmentTypeInfo);
		}

		public void TestMasterShipmentNotAsmWithSubShipmentsShowAnError()
		{
			var messageError = "Only Assembly Master shipments can have sub-shipments that are neither Standard House nor HVLV Shipper Consolidation (Legacy) shipments.";

			var master = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var subShipment = FreightTestHelper.GetShipment("SUB", master, Constants.ShipmentTypes.StandardHouse, Factory);

			subShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			subShipment.Validation.ValidateJS_ShipmentType();

			AssertHasNotifications(messageError, subShipment.JS_ShipmentTypeInfo);

			subShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			subShipment.Validation.ValidateJS_ShipmentType();

			AssertHasNotifications(messageError, subShipment.JS_ShipmentTypeInfo);
		}

		public void TestColoadMasterShipment_MAWBOfConsolFromMasterIsPrinted_GenerateAnError()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;

			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("Button", Factory);

			var master = FreightTestHelper.GetShipment<ForwardingShipment>("McLaren", Constants.ShipmentTypes.AssemblyMaster, Factory);
			master.Consols.Add(consol);

			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("Perez", Constants.ShipmentTypes.AssemblyMaster, Factory);
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			shipment.Consols.RemoveAll();

			consol.UpdateAWBPrinted();

			var printFinalMessage = string.Format(
				"Consol Button cannot be attached to the Shipment Perez from its proposed Master/Lead McLaren as the Master Bill for Button has already been printed on {0}.\r\n{1}",
				consol.FinalMAWBPrintedDate,
				Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster));

			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasNotifications(CargoWise.ComponentModel.NotificationType.Error, shipment.JS_JS_ColoadMasterShipmentInfo, printFinalMessage);
		}

		public void TestJS_JS_ColoadMasterShipment_PassengerFlightValidation()
		{
			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "MYKUL";
				consol.JK_ConsolMode = "BCN";

				var flight1 = consol.Transports[0];
				flight1.JW_IsCargoOnly = false;

				var master = consol.Shipments.AddNew();
				master.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

				var sub = Factory.New<ForwardingShipment>();
				sub.JS_InspectionTypeCode = "NUC";
				sub.JS_JS_ColoadMasterShipment = master.PK;

				AssertHasError(sub.JS_JS_ColoadMasterShipmentInfo, "For a voyage that is not Cargo Only, all Shipments must be Aviation Security Approved or Exempt.");
			}
		}

		public void TestJS_JS_ColoadMasterShipment_ACAS_CLD()
		{
			const string expectedError = "Advanced Air Cargo Reporting has been sent for this Shipment. In co-load scenario, Advance Air Cargo Reporting should be done from the Co-Load Master (CLD) shipment. Please withdraw the message sent from this shipment prior to attaching it to CLD master.";

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_TransportMode = "AIR";
			masterShipment.JS_ShipmentType = "CLD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "BRSAO";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "BRSAO";

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageSent, "|CMP=Not the right message", ZDateTimeOffset.Today.AddDays(-2));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-1));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_RL_NKDestination = "ARBUE";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("Shipment is not destined for Brazil", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_RL_NKDestination = "BRSAO";
			transport.JW_RL_NKDiscPort = "ARBUE";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("Shipment has no transport discharging in Brazil", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			transport.JW_RL_NKDiscPort = "BRSAO";
			shipment.JS_TransportMode = "SEA";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("Shipment is not by Air", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_TransportMode = "AIR";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasError("Shipment is by Air, destined for Brazil and has a transport discharging in Brazil", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertHasError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-2));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasError("The MWA does not correspond to the MSN as it was before", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("The message has been withdrawn", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);
		}

		public void TestJS_JS_ColoadMasterShipment_ACAS_ASM()
		{
			const string expectedError = "Advanced Air Cargo Reporting has been sent from this shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from this shipment prior to attaching it to Assembly master.";

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_TransportMode = "AIR";
			masterShipment.JS_ShipmentType = "ASM";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "BRSAO";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "BRSAO";

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageSent, "|CMP=Not the right message", ZDateTimeOffset.Today.AddDays(-2));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-1));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			masterShipment.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-1));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_RL_NKDestination = "ARBUE";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("Shipment is not destined for Brazil", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_RL_NKDestination = "BRSAO";
			transport.JW_RL_NKDiscPort = "ARBUE";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("Shipment has no transport discharging in Brazil", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			transport.JW_RL_NKDiscPort = "BRSAO";
			shipment.JS_TransportMode = "SEA";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("Shipment is not by Air", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_TransportMode = "AIR";
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasError("Shipment is by Air, destined for Brazil and has a transport discharging in Brazil", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNoError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertHasError(shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-2));
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasError("The MWA does not correspond to the MSN as it was before", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);

			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoError("The message has been withdrawn", shipment.JS_JS_ColoadMasterShipmentInfo, expectedError);
		}

		public void TestJS_Calc_EstimatedExportClearanceDateValidation_RegistyErrors()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			AssertNoErrors("By default there should be no error message on this field requiring it to be mandatory", shipment.JS_Calc_EstimatedExportClearanceDateInfo);

			FreightDataRegistry.Instance.EstimatedExportCustomsClearanceDateMandatory.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			shipment.RunPreSaveValidation();

			AssertHasError("Should have an error as the field is made mandatory in the registry", shipment.JS_Calc_EstimatedExportClearanceDateInfo, "Please enter a date. This field has been defined as mandatory. This setting is found in the system registry at Freight/Shipment/Estimated Export Customs Clearance Date Mandatory.");

			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Today;
			shipment.RunPreSaveValidation();

			AssertNoErrors("Should have no error on this date", shipment.JS_Calc_EstimatedExportClearanceDateInfo);
		}

		public void TestJS_Calc_EstimatedExportClearanceDateValidation_IsNonForwardingBooking()
		{
			FreightDataRegistry.Instance.EstimatedExportCustomsClearanceDateMandatory.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);

			var expectedErrorMessage = "Please enter a date. This field has been defined as mandatory. This setting is found in the system registry at Freight/Shipment/Estimated Export Customs Clearance Date Mandatory.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = true;
			shipment.RunPreSaveValidation();
			AssertHasError("Should have error on this date", shipment.JS_Calc_EstimatedExportClearanceDateInfo, expectedErrorMessage);

			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;
			shipment.RunPreSaveValidation();
			AssertHasError("Should have error on this date", shipment.JS_Calc_EstimatedExportClearanceDateInfo, expectedErrorMessage);

			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = false;
			shipment.RunPreSaveValidation();
			AssertHasError("Should have error on this date", shipment.JS_Calc_EstimatedExportClearanceDateInfo, expectedErrorMessage);

			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.RunPreSaveValidation();
			AssertNoErrors("Should have no error on this date", shipment.JS_Calc_EstimatedExportClearanceDateInfo);
		}

		public void TestJS_JS_ColoadMasterShipment_STDShipmentIsSentConsolidationAdviceAttachToCLDShipment()
		{
			var message = "A Consolidation Advice was already sent from the Shipment(s) S0001. Please arrange for the cancellation of Consolidation with the Forwarder.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S0001";
			var coLoadShipment = Factory.New<ForwardingShipment>();
			coLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			shipment.JS_JS_ColoadMasterShipment = coLoadShipment.PK;

			AssertNoWarning(shipment.JS_JS_ColoadMasterShipmentInfo, message);
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			var parameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentConstants.DocumentNames.ConsolidationAdvice)
			};

			shipment.Logs.AddNew(Events.MessageSent, new ZDateTimeOffset(2022, 04, 18), parameters);
			AssertEquals(true, shipment.IsSentConsolidationAdvice);

			shipment.JS_JS_ColoadMasterShipment = coLoadShipment.PK;

			AssertHasWarning(shipment.JS_JS_ColoadMasterShipmentInfo, message);
		}

		public void TestValidateJS_ShipmentType_SubShipmentIsSentConsolidationAdvice()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_UniqueConsignRef = "SS0001";

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment2.JS_UniqueConsignRef = "SS0002";

			var subShipment3 = Factory.New<ForwardingShipment>();
			subShipment3.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment3.JS_UniqueConsignRef = "SS0003";

			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment3.JS_JS_ColoadMasterShipment = masterShipment.PK;

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertNoWarningContaining(masterShipment.JS_ShipmentTypeInfo, "A Consolidation Advice was already sent from the Shipment(s)");
			AssertNoWarningContaining(masterShipment.JS_ShipmentTypeInfo, ". Please arrange for the cancellation of Consolidation with the Forwarder.");

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			var consolidationAdviceParameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentConstants.DocumentNames.ConsolidationAdvice)
			};
			subShipment.Logs.AddNew(Events.MessageSent, new ZDateTimeOffset(2022, 04, 19), consolidationAdviceParameters);
			subShipment2.Logs.AddNew(Events.MessageSent, new ZDateTimeOffset(2022, 04, 21), consolidationAdviceParameters);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertHasWarningContaining(masterShipment.JS_ShipmentTypeInfo, "A Consolidation Advice was already sent from the Shipment(s)");
			AssertHasWarningContaining(masterShipment.JS_ShipmentTypeInfo, "SS0001");
			AssertHasWarningContaining(masterShipment.JS_ShipmentTypeInfo, "SS0002");
			AssertHasWarningContaining(masterShipment.JS_ShipmentTypeInfo, ". Please arrange for the cancellation of Consolidation with the Forwarder.");

			AssertNoWarningContaining(masterShipment.JS_ShipmentTypeInfo, "SS0003");

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			subShipment3.Logs.AddNew(Events.MessageSent, new ZDateTimeOffset(2022, 04, 21), consolidationAdviceParameters);
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertHasWarningContaining(masterShipment.JS_ShipmentTypeInfo, "SS0003");
		}

		public void TestJS_Calc_EstimatedExportClearanceDateValidation_DateRangeErrors()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Empty;
			shipment.RunPreSaveValidation();

			AssertNoErrors("Pre-condition: there should be no warning or errors on this date", shipment.JS_Calc_EstimatedExportClearanceDateInfo);

			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Today.AddYears(5).AddDays(1);
			shipment.RunPreSaveValidation();

			AssertHasErrors("Should have an error as the date from today is more than 5 years into the future", shipment.JS_Calc_EstimatedExportClearanceDateInfo);

			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Today.AddYears(5).AddDays(-1);
			shipment.RunPreSaveValidation();

			AssertNoErrors("Should no errors a warning as the date from today is less than 5 years into the future from today", shipment.JS_Calc_EstimatedExportClearanceDateInfo);
			AssertHasWarnings("Should have a warning as the date from today is more than a year in the future", shipment.JS_Calc_EstimatedExportClearanceDateInfo);

			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Today;
			shipment.RunPreSaveValidation();

			AssertNoErrors("Should have no errors as a valid date is entered", shipment.JS_Calc_EstimatedExportClearanceDateInfo);
			AssertNoWarnings("Should have no warnings as a valid date is entered", shipment.JS_Calc_EstimatedExportClearanceDateInfo);

			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Today.AddYears(-11);
			shipment.RunPreSaveValidation();

			AssertHasErrors("Should have an error as the date from today is more than 10 years into the past", shipment.JS_Calc_EstimatedExportClearanceDateInfo);

			shipment.JS_Calc_EstimatedExportClearanceDate = ZDateTime.Invalid;
			shipment.RunPreSaveValidation();

			AssertEquals("Should show an invalid date, ZDateTime.Invalid", ZDateTime.Invalid, shipment.JS_Calc_EstimatedExportClearanceDate);
			AssertHasErrors("Should have an error as the date is invalid", shipment.JS_Calc_EstimatedExportClearanceDateInfo);
		}

		public void TestJS_OH_BookedShippingLine_Booking_HasWarningWhenCarrierIsDifferentWithOneFromAttachedVoyage()
		{
			Func<string, OrgHeader> createCarrier = (carrierCode) =>
				{
					var carrier = Factory.New<OrgHeader>();
					carrier.OH_Code = carrierCode;
					carrier.OH_IsShippingLine = true;
					carrier.OH_IsShippingProvider = true;

					return carrier;
				};

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_OH_Line = createCarrier("ONE").PK;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			voyage.GenerateSailings();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_IsBooking = true;
			shipment.JS_OA_BookedShippingLineAddress = createCarrier("TWO").MainAddress.PK;
			AssertNoWarning(shipment.JS_OA_BookedShippingLineAddressInfo, "Attached Sailing has different Carrier.");

			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.JS_OA_BookedShippingLineAddress = createCarrier("THREE").MainAddress.PK;
			AssertHasWarning(shipment.JS_OA_BookedShippingLineAddressInfo, "Attached Sailing has different Carrier.");

			shipment.JS_OA_BookedShippingLineAddress = voyage.Line.MainAddress.PK;
			AssertNoWarning(shipment.JS_OA_BookedShippingLineAddressInfo, "Attached Sailing has different Carrier.");

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OA_BookedShippingLineAddress = createCarrier("FOUR").MainAddress.PK;
			AssertNoWarning(shipment.JS_OA_BookedShippingLineAddressInfo, "Attached Sailing has different Carrier.");

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_IsBooking = false;
			shipment.JS_OA_BookedShippingLineAddress = createCarrier("FIVE").MainAddress.PK;
			AssertNoWarning(shipment.JS_OA_BookedShippingLineAddressInfo, "Attached Sailing has different Carrier.");
		}

		#region ValidateControllingAgentPK

		public void TestValidateControllingAgentPK_ControllingAgentFunctionalityAndValidations()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				shipment.ControllingAgentDocumentaryAddress.OrganisationPK = controllingAgent.PK;
				AssertNoError(shipment.ControllingAgentDocumentaryAddress.OrganisationPKInfo, "This organization is not a valid Controlling Agent.");

				shipment.ControllingAgentDocumentaryAddress.E2_AddressOverride = true;
				AssertNoError(shipment.ControllingAgentDocumentaryAddress.OrganisationPKInfo, "It is not possible to override a Controlling Agent. Amend the details on the linked organization.");
			}
			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				shipment.ControllingAgentDocumentaryAddress.OrganisationPK = controllingAgent.PK;
				AssertHasError(shipment.ControllingAgentDocumentaryAddress.OrganisationPKInfo, "This organization is not a valid Controlling Agent.");

				shipment.ControllingAgentDocumentaryAddress.E2_AddressOverride = true;
				AssertHasError(shipment.ControllingAgentDocumentaryAddress.OrganisationPKInfo, "It is not possible to override a Controlling Agent. Amend the details on the linked organization.");
			}
		}

		#endregion

		#region JS_ShipmentStatus

		public void TestValidateJS_ShipmentStatus_WhenShipmentStatusVisibilityIsEnabled()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.JS_ShipmentStatus = "XYZ";
			AssertNoErrors("Skip validation for non-sea shipments", shipment.JS_ShipmentStatusInfo);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentStatus = "XYZ";
			shipment.Validation.ValidateJS_ShipmentStatus();
			AssertHasError(shipment.JS_ShipmentStatusInfo, "Enter a valid Status.");

			shipment.JS_ShipmentStatus = ZString.Empty;
			AssertHasError(shipment.JS_ShipmentStatusInfo, "Please enter a Status.");

			shipment.JS_ShipmentStatus = "ESI";
			AssertNoErrors("ESI", shipment.JS_ShipmentStatusInfo);

			shipment.JS_ShipmentStatus = "WEB";
			AssertHasError("WEB - can't manually enter status", shipment.JS_ShipmentStatusInfo, "Enter a valid Status.");

			Factory.Save();

			shipment.Validation.ValidateJS_ShipmentStatus();
			AssertNoErrors("WEB - no errors once saved", shipment.JS_ShipmentStatusInfo);
		}

		#endregion

		#region JS_HBLAWBChargesDisplay

		public void TestJS_HBLAWBChargesValidation()
		{
			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUSYD";
			const string errorMessage = "Enter a valid Charges Apply.";

			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "BRRBA";
			shipment.JS_RL_NKDestination = "BRBAU";

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.APP;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NPP;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = "ABC";
			AssertHasError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);
		}

		#endregion

		#region JS_PaymentTermAutoratingOverride
		public void TestValidateJS_PaymentTermAutoratingOverride()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_PaymentTermAutoratingOverride = "CLT";
			AssertHasErrors(shipment.JS_PaymentTermAutoratingOverrideInfo);

			shipment.JS_PaymentTermAutoratingOverride = "CCX";
			AssertNoErrors(shipment.JS_PaymentTermAutoratingOverrideInfo);

			shipment.JS_PaymentTermAutoratingOverride = "PPD";
			AssertNoErrors(shipment.JS_PaymentTermAutoratingOverrideInfo);

			shipment.JS_PaymentTermAutoratingOverride = string.Empty;
			AssertNoErrors(shipment.JS_PaymentTermAutoratingOverrideInfo);
		}
		#endregion

		#region Duplicate House Bill number

		public void TestHasDuplicateHouseBill_ShouldRaiseError_WhenHouseBillIsDuplicateAndShipmentHasNotSaved()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consignRef = "S00000001";
				var duplicateHouseBill = "1";

				var ship1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship1.JS_UniqueConsignRef = consignRef;
				ship1.JS_HouseBill = duplicateHouseBill;
				ship1.JS_TransportMode = Constants.TransportModes.Air;

				var ship2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship2.JS_HouseBill = duplicateHouseBill;
				ship2.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertHasError("Ship2 should have error on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000001\r\n");

				ship2.JS_HouseBill = "2";
				ship2.JS_UniqueConsignRef = "S00000002";
				ship2.JS_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
				ship1.JS_UniqueConsignRef = "S00000004";

				Factory.Save();

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET JS_HouseBill = '1', JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_UniqueConsignRef = '{ship2.JS_UniqueConsignRef}'");

				var anotherFactory = new BusinessObjectFactory();
				var loadedShipment2 = anotherFactory.Load<CommonShipment>(ship2.PK);

				AssertEquals("1", ship1.JS_HouseBill);
				AssertEquals("1", loadedShipment2.JS_HouseBill);
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should have warnings on JS_HouseBill, duplicate entry", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");

				var ship3 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship3.JS_TransportMode = Constants.TransportModes.Air;
				ship3.JS_UniqueConsignRef = consignRef;
				ship3.JS_HouseBill = "3";
				ship3.JS_UniqueConsignRef = "S00000003";
				Factory.Save();

				loadedShipment2.JS_HouseBill = "3";
				loadedShipment2.Validation.ValidateAll();
				AssertHasError("Ship2 should have error on JS_HouseBill, duplicate entry", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000003\r\n");

				loadedShipment2.JS_TransportMode = Constants.TransportModes.Sea;
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should have warnings on JS_HouseBill, duplicate entry, when TransportMode is Sea", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000003\r\n");

				loadedShipment2.JS_TransportMode = Constants.TransportModes.AirSea;
				loadedShipment2.Validation.ValidateAll();
				AssertHasError("Ship2 should have error on JS_HouseBill, duplicate entry, when TransportMode is FAS", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000003\r\n");

				loadedShipment2.JS_TransportMode = Constants.TransportModes.Rail;
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should have warnings on JS_HouseBill, duplicate entry, when TransportMode is Rail", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000003\r\n");

				loadedShipment2.JS_TransportMode = Constants.TransportModes.SeaAir;
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should have warnings on JS_HouseBill, duplicate entry, when TransportMode is FSA", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000003\r\n");
			}
		}

		public void TestHasDuplicateHouseBill_ShouldRaiseError_ForConvertedBooking()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ZString consignRef = "S00000001";
				ZString duplicateHouseBill = "1";
				var ship1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship1.JS_UniqueConsignRef = consignRef;
				ship1.JS_HouseBill = duplicateHouseBill;
				ship1.JS_TransportMode = Constants.TransportModes.Air;

				var ship2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship2.JS_HouseBill = duplicateHouseBill;
				ship2.JS_TransportMode = Constants.TransportModes.Air;
				AssertHasError("Ship2 should have error on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000001\r\n");

				ship2.JS_HouseBill = "2";
				ship2.JS_UniqueConsignRef = "S00000002";
				ship2.JS_IsForwardRegistered = false;
				ship2.JS_IsBooking = true;

				ship1.JS_UniqueConsignRef = "S00000004";

				Factory.Save();

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET JS_HouseBill = '1', JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_UniqueConsignRef = '{ship2.JS_UniqueConsignRef}'");

				var anotherFactory = new BusinessObjectFactory();

				var loadedShipment2 = anotherFactory.Load<CommonShipment>(ship2.PK);

				AssertEquals("1", ship1.JS_HouseBill);
				AssertEquals("1", loadedShipment2.JS_HouseBill);

				loadedShipment2.Validation.ValidateAll();
				AssertNoError(loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");

				loadedShipment2.JS_IsForwardRegistered = true;
				loadedShipment2.Validation.ValidateAll();
				AssertHasError("Ship2 should have errors on JS_HouseBill, duplicate entry", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");

				loadedShipment2.JS_TransportMode = Constants.TransportModes.Sea;
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should have warning on JS_HouseBill, duplicate entry, when Transport Mode is SEA", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");
			}
		}

		public void TestHasDuplicateHouseBill_ShouldRaiseError_WhenCreationTimeIsAfterRegistryChange()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consignRef = "S00000001";
				var duplicateHouseBill = "1";
				var ship1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship1.JS_UniqueConsignRef = consignRef;
				ship1.JS_HouseBill = duplicateHouseBill;
				ship1.JS_TransportMode = Constants.TransportModes.Air;

				var ship2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship2.JS_HouseBill = duplicateHouseBill;
				ship2.JS_TransportMode = Constants.TransportModes.Air;
				AssertHasError("Ship2 should have error on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000001\r\n");

				ship2.JS_HouseBill = "2";
				ship2.JS_UniqueConsignRef = "S00000002";
				ship1.JS_UniqueConsignRef = "S00000004";

				Factory.Save();

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.JobShipment SET JS_HouseBill = '1', JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_UniqueConsignRef = '{ship2.JS_UniqueConsignRef}'");

				var anotherFactory = new BusinessObjectFactory();
				var loadedShipment2 = anotherFactory.Load<CommonShipment>(ship2.PK);

				AssertEquals("1", ship1.JS_HouseBill);
				AssertEquals("1", loadedShipment2.JS_HouseBill);

				loadedShipment2.JS_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
				loadedShipment2.Validation.ValidateAll();
				AssertHasError("Ship2 should have errors on JS_HouseBill, duplicate entry", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");

				loadedShipment2.JS_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should have warnings on JS_HouseBill, duplicate entry", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");

				loadedShipment2.JS_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
				loadedShipment2.JS_TransportMode = Constants.TransportModes.Sea;
				loadedShipment2.Validation.ValidateAll();
				AssertHasWarning("Ship2 should not have errors on JS_HouseBill, duplicate entry when transport mode is SEA", loadedShipment2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000004\r\n");
			}
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_ShouldNotRaiseWarningWhenShipmentIsNotForwardingRegistered()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();
				var shipment1 = creationFactory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.JS_HouseBill = "HELLO";
				creationFactory.Save();

				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.JS_HouseBill = "HELLO";
				shipment2.JS_IsForwardRegistered = false;
				shipment2.JS_IsCFSRegistered = true;

				var shipmentReloaded = Factory.Load<CommonShipment>(shipment1.PK);

				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertNoWarning("Shipment should not have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");

				shipment2.JS_IsForwardRegistered = true;

				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");
			}
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_ShoutNotRaiseError()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ship1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship1.JS_UniqueConsignRef = "ONE";
				ship1.JS_HouseBill = "1";
				ship1.JS_TransportMode = Constants.TransportModes.Sea;

				var ship2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship2.JS_HouseBill = "1";
				ship2.JS_TransportMode = Constants.TransportModes.Air;
				AssertNoError("ship2 should not have error on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nONE\r\n");

				ship1.JS_TransportMode = Constants.TransportModes.Air;
				ship2.Validation.ValidateAll();
				AssertHasError("ship2 should have error on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nONE\r\n");
			}
		}

		#endregion

		#region  JS_DeliveryDueDate

		public void TestCheckJS_DeliveryDueDate_Errors()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var deliveryDueDateCalculatorManagerMock = DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(ZDateTime.Today);
				deliveryDueDateCalculatorManagerMock.Setup(m => m.Calculate(It.IsAny<ForwardingShipment>())).Returns(DeliveryDueDateCalculationResult.Failure("Something went wrong", ZString.Empty));

				var shipment = GetShipmentForDDDCalculation();
				shipment.CalculateDeliveryDueDate();
				shipment.Validation.ValidateJS_DeliveryDueDate();
				AssertHasWarning(shipment.JS_DeliveryDueDateInfo, "Something went wrong");
			}
		}

		public void TestCheckJS_DeliveryDueDate_WhenRegistryIsEnabledAndDDDIsEmpty()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment = GetShipmentForDDDCalculation();
				shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CY_CY;
				shipment.Validation.ValidateJS_DeliveryDueDate();
				AssertNoWarnings("Calculate DDD registry is not enabled", shipment.JS_DeliveryDueDateInfo);
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment = GetShipmentForDDDCalculation();
				shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CY_CY;
				shipment.JS_DeliveryDueDate = ZDateTime.Now;
				AssertNoWarnings("JS_DeliveryDueDate is not empty", shipment.JS_DeliveryDueDateInfo);
			}
		}

		public void TestCheckJS_DeliveryDueDate_Warning_HBLDeliveryMode() => CheckJS_DeliveryDueDateWarning(
			shipment => shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CY_CFS,
			$"Only HBL Delivery Modes {JoinWithOr(DeliveryDueDateCalculator.SupportedDeliveryModes)} are used in DDD (Delivery Due Date) calculation.");

		public void TestCheckJS_DeliveryDueDate_Warning_ServiceLevel() => CheckJS_DeliveryDueDateWarning(
			shipment => shipment.JS_RS_NKServiceLevel = ZString.Empty,
			"Service Level is mandatory for DDD (Delivery Due Date) calculation.");

		public void TestCheckJS_DeliveryDueDate_Warning_PickupFrom() => CheckJS_DeliveryDueDateWarningWithCondition(
			shipment => shipment.ConsignorPickupAddress.OrganisationPK = ZGuid.Empty,
			"Pickup From address is mandatory for DDD (Delivery Due Date) calculation.",
			shipment => shipment.IsHBLContainerPackModeDOOR_X);

		public void TestCheckJS_DeliveryDueDate_Warning_OriginCFS()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var expectedWarning = "Origin CFS is mandatory for DDD (Delivery Due Date) calculation.";
				var shipment = GetShipmentForDDDCalculation();
				shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
				shipment.Validation.ValidateJS_DeliveryDueDate();
				AssertHasWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);

				var serviceLevel = Factory.New<RefServiceLevel>();
				serviceLevel.RS_Code = "D2D";
				serviceLevel.RS_DefaultTransitHours = 10;
				shipment.JS_RS_NKServiceLevel = "D2D";
				AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);
			}
		}

		public void TestCheckJS_DeliveryDueDate_Warning_PickupRequiredBy()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment = GetShipmentForDDDCalculation();
				AssertCheckHasValidDate(shipment, Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
					shipment.DocsAndCartage.JP_PickupCartageCompletedInfo,
					shipment.DocsAndCartage.JP_PickupRequiredByInfo,
					"Actual Pickup or Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var shipment2 = GetShipmentForDDDCalculation();
				AssertCheckHasValidDate(shipment2, Constants.HBLDeliveryModes.Codes.DOOR_CFS,
					shipment2.DocsAndCartage.JP_PickupCartageCompletedInfo,
					shipment2.DocsAndCartage.JP_PickupRequiredByInfo,
					"Actual Pickup or Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var shipment3 = GetShipmentForDDDCalculation();
				AssertCheckHasValidDate(shipment3, Constants.HBLDeliveryModes.Codes.CFS_DOOR,
					shipment3.JS_A_RCVInfo,
					shipment3.DocsAndCartage.JP_PickupRequiredByInfo,
					"Interim Receipt Date or Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var shipment4 = GetShipmentForDDDCalculation();
				AssertCheckHasValidDate(shipment4, Constants.HBLDeliveryModes.Codes.CFS_CFS,
					shipment4.JS_A_RCVInfo,
					shipment4.DocsAndCartage.JP_PickupRequiredByInfo,
					"Interim Receipt Date or Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var shipment5 = GetShipmentForDDDCalculation();
				shipment5.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.ARPT_ARPT;
				shipment5.JS_A_RCV = ZDateTime.Empty;
				shipment5.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
				shipment5.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
				shipment5.Validation.ValidateJS_DeliveryDueDate();
				AssertNoWarning(shipment5.JS_DeliveryDueDateInfo, "Actual Pickup or Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");
				AssertNoWarning(shipment5.JS_DeliveryDueDateInfo, "Interim Receipt Date or Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");
			}
		}

		void AssertCheckHasValidDate(ForwardingShipment shipment, ZString deliveryMode, ZPropertyInfo initialField, ZPropertyInfo fallbackField, ZString expectedMessage)
		{
			shipment.JS_HBLContainerPackModeOverride = deliveryMode;
			initialField.Value = ZDateTime.Empty;
			fallbackField.Value = ZDateTime.Empty;
			shipment.Validation.ValidateJS_DeliveryDueDate();
			AssertHasWarning(shipment.JS_DeliveryDueDateInfo, expectedMessage);

			initialField.Value = ZDateTime.Empty;
			fallbackField.Value = ZDateTime.Today;
			shipment.Validation.ValidateJS_DeliveryDueDate();
			AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedMessage);

			initialField.Value = ZDateTime.Today;
			fallbackField.Value = ZDateTime.Empty;
			shipment.Validation.ValidateJS_DeliveryDueDate();
			AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedMessage);
		}

		public void TestCheckJS_DeliveryDueDate_Warning_DeliveryTo() => CheckJS_DeliveryDueDateWarningWithCondition(
			shipment => shipment.ConsigneeDeliveryAddress.OrganisationPK = ZGuid.Empty,
			"Deliver To address is mandatory for DDD (Delivery Due Date) calculation.",
			shipment => shipment.IsHBLContainerPackModeX_DOOR);

		public void TestCheckJS_DeliveryDueDate_Warning_DestinationCFS()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var expectedWarning = "Destination CFS is mandatory for DDD (Delivery Due Date) calculation.";
				var shipment = GetShipmentForDDDCalculation();
				shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
				shipment.Validation.ValidateJS_DeliveryDueDate();
				AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);

				shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
				shipment.Validation.ValidateJS_DeliveryDueDate();
				AssertHasWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);

				var serviceLevel = Factory.New<RefServiceLevel>();
				serviceLevel.RS_Code = "D2D";
				serviceLevel.RS_DefaultTransitHours = 10;
				shipment.JS_RS_NKServiceLevel = "D2D";
				AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);
			}
		}

		void CheckJS_DeliveryDueDateWarning(Action<ForwardingShipment> setMissingProperty, string expectedWarning)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment = GetShipmentForDDDCalculation();
				setMissingProperty(shipment);
				shipment.Validation.ValidateJS_DeliveryDueDate();
				AssertHasWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);
			}
		}

		void CheckJS_DeliveryDueDateWarningWithCondition(Action<ForwardingShipment> setMissingProperty, string expectedWarning, Func<ForwardingShipment, bool> warningCondition)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment = GetShipmentForDDDCalculation();
				setMissingProperty(shipment);

				DDDCanCalculateModeList.ForEach(mode =>
				{
					shipment.JS_HBLContainerPackModeOverride = mode;
					shipment.Validation.ValidateJS_DeliveryDueDate();
					if (warningCondition(shipment))
					{
						AssertHasWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);
					}
					else
					{
						AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedWarning);
					}
				});
			}
		}

		ForwardingShipment GetShipmentForDDDCalculation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_RS_NKServiceLevel = "STD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var pickupCFS = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCFS = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_OA_ExportReceivingDepot = pickupCFS.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFS.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Today.AddDays(-1);
			shipment.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "Calculate test";

			return shipment;
		}

		static string JoinWithOr(IEnumerable<string> elements)
		{
			if (elements == null)
			{
				throw new ArgumentNullException(nameof(elements));
			}

			var list = elements.ToList();

			if (list.Count == 0)
			{
				return string.Empty;
			}

			if (list.Count == 1)
			{
				return list[0];
			}

			return string.Join(", ", list.Take(list.Count - 1)) + $" or {list.Last()}";
		}

		readonly static string[] DDDCanCalculateModeList = new[]
		{
			Constants.HBLDeliveryModes.Codes.ARPT_DOOR,
			Constants.HBLDeliveryModes.Codes.ARPT_CFS,
			Constants.HBLDeliveryModes.Codes.CFS_ARPT,
			Constants.HBLDeliveryModes.Codes.CFS_CFS,
			Constants.HBLDeliveryModes.Codes.CFS_DOOR,
			Constants.HBLDeliveryModes.Codes.DOOR_ARPT,
			Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
			Constants.HBLDeliveryModes.Codes.DOOR_CFS,
			Constants.HBLDeliveryModes.Codes.ARPT_ARPT,
		};

		#endregion

		#region CO2e

		public void TestCheckTotalCO2eForBinding()
		{
			// Arrange
			var shipment = Factory.New<ForwardingShipment>();
			var supporter = shipment as ICO2eCalculationSupporter;

			// Act & Assert
			shipment.JS_ActualWeight = 2;
			shipment.JS_UnitOfWeight = "T";
			shipment.SetCO2ePerTonneInKg(100m);
			Assert(!supporter.RequireTEU);

			shipment.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			shipment.Validation.ValidateTotalCO2eForBinding();
			Assert(shipment.TotalCO2eForBindingInfo.HasWarning("The greenhouse gas emissions value could not be calculated."));

			shipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			shipment.Validation.ValidateTotalCO2eForBinding();
			Assert(shipment.TotalCO2eForBindingInfo.HasWarning(CO2eTestHelper.CO2eStaleWarning));
		}

		#endregion

		#region TestSurrenderParty

		public void TestValidateSurrenderParty()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;

				shipment.SurrenderPartyDocAddress.Validation.ValidateAll();
				AssertHasMessageError(shipment.SurrenderPartyDocAddress.OrganisationPKInfo, "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.SurrenderPartyDocAddress.Validation.ValidateAll();
				AssertNoMessageError(shipment.SurrenderPartyDocAddress.OrganisationPKInfo, "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");
			});
		}

		#endregion

		#region TestHolder

		public void TestValidateHolderPK()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				shipment.HolderDocAddress.Validation.ValidateAll();
				AssertHasMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder cannot be blank. Please select a First Holder party from the organization lookup.");

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST002";
				org.OH_FullName = "Test Organization2";
				org.OH_IsConsignee = true;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
				shipment.HolderDocAddress.OrganisationPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK;

				shipment.HolderDocAddress.Validation.ValidateAll();
				AssertHasMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.HolderDocAddress.Validation.ValidateAll();
				AssertNoMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder cannot be blank. Please select a First Holder party from the organization lookup.");
				AssertNoMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");

				orgCusCode.OK_CustomsRegNo = ZString.Empty;
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = false;
				shipment.HolderDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");
				AssertHasMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > Consignor > Details > Exporter/Consignor Defaults.");

				orgCusCode.OK_CustomsRegNo = "123456";
				org.OH_IsConsignor = false;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = false;
				shipment.HolderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > Consignor > Details > Exporter/Consignor Defaults.");

				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
				shipment.HolderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > Consignor > Details > Exporter/Consignor Defaults.");

				org.OH_IsForwarder = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = false;
				shipment.HolderDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > FWD/Agent > Details > Forwarder Details.");

				org.OH_IsForwarder = false;
				org.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = false;
				shipment.HolderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > FWD/Agent > Details > Forwarder Details.");

				org.OH_IsForwarder = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = true;
				shipment.HolderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.HolderDocAddress.OrganisationPKInfo, "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > FWD/Agent > Details > Forwarder Details.");
			});
		}

		#endregion

		#region TestShipper

		public void TestValidateShipperPK()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;

				shipment.ShipperDocAddress.Validation.ValidateAll();
				AssertNoMessageError(shipment.ShipperDocAddress.OrganisationPKInfo, "Shipper cannot be blank. Please enter a Shipper in Basic Registration>Consignor");

				shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;

				shipment.ShipperDocAddress.Validation.ValidateAll();
				AssertHasMessageError(shipment.ShipperDocAddress.OrganisationPKInfo, "Shipper cannot be blank. Please enter a Shipper in Basic Registration>Consignor");
			});
		}

		public void TestValidateShipperPK_MissingTRI()
		{
			var errorMessage = $"Shipper must have a valid Bolero Entity Identifier (TRI){System.Environment.NewLine}to proceed with publishing an Electronic Bill of Lading.";

			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				shipment.ShipperDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.ShipperDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.ShipperDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.ShipperDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.BlankEndorse;
				shipment.ShipperDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.ShipperDocAddress.OrganisationPKInfo, errorMessage);

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				shipment.ShipperDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.ShipperDocAddress.OrganisationPKInfo, errorMessage);
			});
		}

		#endregion

		#region TestElectronicBillOfLadingConsignee

		public void TestElectronicBillOfLadingConsignee()
		{
			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();
				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, "Consignee cannot be blank. Please enter a Consignee in Basic Registration>Consignee");

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = ZGuid.Empty;

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();
				AssertHasMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, "Consignee cannot be blank. Please enter a Consignee in Basic Registration>Consignee");

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();
				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, "Consignee cannot be blank. Please enter a Consignee in Basic Registration>Consignee");
			});
		}

		public void TestElectronicBillOfLadingConsignee_MissingTRI()
		{
			var errorMessage = $"The consignee must have a valid Bolero Entity Identifier (TRI){System.Environment.NewLine}to proceed with publishing a 'Straight Bill'.";

			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.BlankEndorse;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, errorMessage);

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456"; 
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPK = ZGuid.Empty;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPKInfo, errorMessage);
				Assert(!shipment.JS_ElectronicBillOfLadingToOrderDocAddress.HasMessageErrors);
			});
		}

		#endregion

		#region TestJS_ElectronicBillOfLadingType

		public void TestJS_ElectronicBillOfLadingType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertWithEnabledElectronicBOL(delegate ()
			{
				shipment.JS_ElectronicBillOfLadingType = ZString.Empty;

				shipment.Validation.ValidateJS_ElectronicBillOfLadingType();
				AssertHasMessageError(shipment.JS_ElectronicBillOfLadingTypeInfo, "Please enter Bill Type.");

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.Validation.ValidateJS_ElectronicBillOfLadingType();
				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingTypeInfo, "Please enter Bill Type.");
			});

			shipment.JS_ElectronicBillOfLadingType = ZString.Empty;
			shipment.Validation.ValidateJS_ElectronicBillOfLadingType();
			AssertNoMessageError(shipment.JS_ElectronicBillOfLadingTypeInfo, "Please enter Bill Type.");
		}

		#endregion

		#region TestJS_ElectronicBillOfLadingTerms

		public void TestJS_ElectronicBillOfLadingTerms()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertWithEnabledElectronicBOL(delegate ()
			{
				shipment.JS_ElectronicBillOfLadingTerms = ZString.Empty;

				shipment.Validation.ValidateJS_ElectronicBillOfLadingTerms();
				AssertHasMessageError(shipment.JS_ElectronicBillOfLadingTermsInfo, "Please enter Bill Terms.");

				shipment.JS_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.Validation.ValidateJS_ElectronicBillOfLadingTerms();
				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingTermsInfo, "Please enter Bill Terms.");
			});

			shipment.JS_ElectronicBillOfLadingTerms = ZString.Empty;
			shipment.Validation.ValidateJS_ElectronicBillOfLadingTerms();
			AssertNoMessageError(shipment.JS_ElectronicBillOfLadingTermsInfo, "Please enter Bill Terms.");
		}

		#endregion

		#region TestElectronicBillOfLadingToOrder

		public void TestElectronicBillOfLadingToOrder_RequiresOrganisationPK()
		{
			var errorMessage = "An organization must be selected when the Bill Type is set to 'To Order.' If the 'To Order' party is unknown, you may opt for the Bill Type 'BLE - Blank Endorse'.";

			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPK = ZGuid.Empty;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();

				AssertHasMessageError(shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo, errorMessage);
			});
		}

		public void TestElectronicBillOfLadingToOrder_MissingTRI()
		{
			var errorMessage = $"The 'To Order' party must have a valid Bolero Entity Identifier (TRI){System.Environment.NewLine}to proceed with publishing a 'To Order Bill'.";

			AssertWithEnabledElectronicBOL(delegate ()
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "S00001001";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;

				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();

				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.Validation.ValidateAll();

				Assert(!shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.HasMessageErrors);
				AssertHasMessageError(shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo, errorMessage);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.BlankEndorse;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo, errorMessage);

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				shipment.JS_ElectronicBillOfLadingToOrderDocAddress.Validation.ValidateAll();

				AssertNoMessageError(shipment.JS_ElectronicBillOfLadingToOrderDocAddress.OrganisationPKInfo, errorMessage);
			});
		}

		#endregion

		#region AssertWithEnableBoleroEHBLIntegration

		void AssertWithEnabledElectronicBOL(Action assertAction)
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				assertAction();
			}
		}

		#endregion

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		protected override CommonShipment GetShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		protected virtual ForwardingConsol GetConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		#endregion
	}
}
