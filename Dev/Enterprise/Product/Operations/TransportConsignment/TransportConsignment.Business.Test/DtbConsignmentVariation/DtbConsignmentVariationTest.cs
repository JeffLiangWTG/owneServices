using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentVariation))]
	class DtbConsignmentVariationTest : EnterpriseBusinessObjectTestCase
	{
		#region Related Objects

		public void TestConsignment_WhenJobTableCodeIsLTC()
		{
			var consignment = helper.CreateConsignment("CN001");
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = helper.CreatePackage(consignment, 1, 2, 3);
			var divot = helper.CreatePackageDivot(action, package);

			var variation = helper.CreateVariation(consignment, divot);
			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals(consignment, variation.Consignment);

				var allQueries = string.Join(System.Environment.NewLine, TestConnection.ExecutedCommands);
				AssertNotContains(
					"When LTV_JobTableCode = 'LTC' we want to use LTV_JobId to query the consignment because it's much cheaper than going through the ParentId relationship chain.",
					DtbConsignmentActionSchema.Constants.TableName,
					allQueries);
			}
		}

		public void TestConsignment_WhenJobTableCodeIsNotLTC()
		{
			var consignment = helper.CreateConsignment("CN001");
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = helper.CreatePackage(consignment, 1, 2, 3);
			var divot = helper.CreatePackageDivot(action, package);

			var runSheet = helper.CreateRunSheet();
			helper.CreateRunSheetInstruction(action, runSheet.PK);

			var variation = helper.CreateVariation(runSheet, divot);
			Factory.Save();

			AssertEquals(consignment, variation.Consignment);
		}

		public void TestConsignment_WhenParentIsNotDivot_AndJobIsNotConsignment_ShouldThrowException()
		{
			var variation = Factory.New<DtbConsignmentVariation>();
			variation.LTV_ParentId = new ZGuid();
			variation.LTV_ParentTableCode = DtbEquipmentItemSchema.Constants.Prefix;
			variation.LTV_JobId = new ZGuid();
			variation.LTV_JobTableCode = DtbConsignmentLegSchema.Constants.Prefix;

			// Can't actually save this because of DB constrains on the table code columns.

			AssertExceptionThrown<NotImplementedException>(() => _ = variation.Consignment);
		}

		public void TestRunSheet_WhenJobTableCodeIsKG()
		{
			var consignment = helper.CreateConsignment("CN001");
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = helper.CreatePackage(consignment, 1, 2, 3);
			var divot = helper.CreatePackageDivot(action, package);

			var runSheet = helper.CreateRunSheet();
			helper.CreateRunSheetInstruction(action, runSheet.PK);

			var variation = helper.CreateVariation(runSheet, divot);
			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals(runSheet, variation.RunSheet);

				var allQueries = string.Join(System.Environment.NewLine, TestConnection.ExecutedCommands);
				AssertNotContains(
					"When LTV_JobTableCode = 'KG' we want to use LTV_JobId to query the run sheet because it's much cheaper than going through the ParentId relationship chain.",
					DtbConsignmentRunSheetInstructionSchema.Constants.TableName,
					allQueries);
			}
		}

		public void TestRunSheet_WhenJobTableCodeIsNotKG()
		{
			var consignment = helper.CreateConsignment("CN001");
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = helper.CreatePackage(consignment, 1, 2, 3);
			var divot = helper.CreatePackageDivot(action, package);

			var runSheet = helper.CreateRunSheet();
			helper.CreateRunSheetInstruction(action, runSheet.PK);

			var variation = helper.CreateVariation(consignment, divot);
			Factory.Save();

			AssertEquals(runSheet, variation.RunSheet);
		}

		public void TestRunSheet_WhenParentIsNotDivot_AndJobIsNotRunSheet_ShouldThrowException()
		{
			var variation = Factory.New<DtbConsignmentVariation>();
			variation.LTV_ParentId = new ZGuid();
			variation.LTV_ParentTableCode = DtbEquipmentItemSchema.Constants.Prefix;
			variation.LTV_JobId = new ZGuid();
			variation.LTV_JobTableCode = DtbConsignmentLegSchema.Constants.Prefix;

			// Can't actually save this because of DB constrains on the table code columns.

			AssertExceptionThrown<NotImplementedException>(() => _ = variation.RunSheet);
		}

		public void TestVariationAddress_WhenParentIsDivot_DtbConsignmentAddressReturned()
		{
			var testAddress1 = "My test Address1";
			var testCompany = "My test Company";
			var testCity = "My test City";
			var testState = "My test State";

			var org = helper.CreateOrganisation("ORG1", false, testAddress1);
			org.MainAddress.OA_CompanyNameOverride = testCompany;
			org.MainAddress.OA_City = testCity;
			org.MainAddress.OA_State = testState;
			var consignment = helper.CreateConsignment("CN001");
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, org.MainAddress);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = helper.CreatePackage(consignment, 1, 2, 3);
			var divot = helper.CreatePackageDivot(action, package);
			var variation = helper.CreateVariation(consignment, divot);

			Factory.Save();

			var variationAddress = variation.VariationAddress;
			AssertNotNull(variationAddress);
			AssertEquals(testAddress1, variationAddress.DocAddresses[0].Address1);
			AssertEquals(testCompany, variationAddress.DocAddresses[0].CompanyName);
			AssertEquals(testCity, variationAddress.DocAddresses[0].City);
			AssertEquals(testState, variationAddress.DocAddresses[0].State);
		}

		public void TestVariationAddress_WhenParentIsNotDivot_ShouldThrowException()
		{
			var variation = Factory.New<DtbConsignmentVariation>();
			variation.LTV_ParentId = new ZGuid();
			variation.LTV_ParentTableCode = DtbEquipmentItemSchema.Constants.Prefix;

			AssertExceptionThrown<NotImplementedException>(() => _ = variation.VariationAddress);
		}

		public void TestAction_WhenParentIsDivot_DtbConsignmentActionReturned()
		{
			var consignment = helper.CreateConsignment("CN001");
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);
			var package = helper.CreatePackage(consignment, 1, 2, 3);
			var divot = helper.CreatePackageDivot(action, package);
			var variation = helper.CreateVariation(consignment, divot);

			Factory.Save();

			var variationAction = variation.Action;
			AssertNotNull(variationAction);
			AssertEquals(variationAction.ActionType, ActionTypes.Codes.PickUp);
		}

		public void TestAction_WhenParentIsNotDivot_NullReturned()
		{
			var variation = Factory.New<DtbConsignmentVariation>();
			variation.LTV_ParentId = new ZGuid();
			variation.LTV_ParentTableCode = DtbEquipmentItemSchema.Constants.Prefix;

			AssertNull(variation.Action);
		}

		#endregion

		#region TestDifferenceInQty

		public void TestDifferenceInQty()
		{
			var variation = helper.CreateVariation();
			variation.LTV_PlannedQty = 1;
			variation.LTV_ActualQty = 6;

			AssertEquals($"5 additional items", variation.DifferenceInQty.ToString());

			variation.LTV_PlannedQty = 5;
			variation.LTV_ActualQty = 6;

			AssertEquals($"1 additional item", variation.DifferenceInQty.ToString());

			variation.LTV_PlannedQty = 6;
			variation.LTV_ActualQty = 1;

			AssertEquals($"5 fewer items", variation.DifferenceInQty.ToString());

			variation.LTV_PlannedQty = 6;
			variation.LTV_ActualQty = 5;

			AssertEquals($"1 fewer item", variation.DifferenceInQty.ToString());

			variation.LTV_PlannedQty = 1;
			variation.LTV_ActualQty = 1;

			AssertEquals("0 items", variation.DifferenceInQty.ToString());
		}

		#endregion

		#region TestDifferenceInWeight

		public void TestDifferenceInWeight()
		{
			var variation = helper.CreateVariation();
			variation.LTV_PlannedWeight = 1.88;
			variation.LTV_ActualWeight = 6.55;
			variation.LTV_WeightUQ = "KG";

			AssertEquals($"4.67 additional KG", variation.DifferenceInWeight.ToString());

			variation.LTV_PlannedWeight = 6.55;
			variation.LTV_ActualWeight = 1.88;

			AssertEquals($"4.67 fewer KG", variation.DifferenceInWeight.ToString());

			variation.LTV_PlannedWeight = 1.88;
			variation.LTV_ActualWeight = 1.88;

			AssertEquals($"0 KG", variation.DifferenceInWeight.ToString());
		}

		#endregion

		#region TestDifferenceInVolume

		public void TestDifferenceInVolume()
		{
			var variation = helper.CreateVariation();
			variation.LTV_PlannedVolume = 5.87;
			variation.LTV_ActualVolume = 46.5;
			variation.LTV_VolumeUQ = "M3";

			AssertEquals($"40.63 additional M3", variation.DifferenceInVolume.ToString());

			variation.LTV_PlannedVolume = 46.5;
			variation.LTV_ActualVolume = 5.87;

			AssertEquals($"40.63 fewer M3", variation.DifferenceInVolume.ToString());

			variation.LTV_PlannedVolume = 5.87;
			variation.LTV_ActualVolume = 5.87;

			AssertEquals($"0 M3", variation.DifferenceInVolume.ToString());
		}

		#endregion

		#region Business Object Overrides

		public void TestHumanReadableName()
		{
			var variation = Factory.New<DtbConsignmentVariation>();
			AssertEquals("Consignment Variation", variation.HumanReadableName);
		}

		#endregion

		#region Workflow

		public void TestTriggersOnConsignments_ShouldRespondToRelatedVariationEvents()
		{
			var variation = helper.CreateVariation();
			Factory.Save();

			var unconditionalTrigger = MasterFilesTestHelper.CreateTrigger(variation.Consignment, Events.JobOpenCode,
				description: "Unconditional trigger");

			var conditionalTrigger = MasterFilesTestHelper.CreateTrigger(variation.Consignment, Events.JobOpenCode,
				triggerCondition: EventReferenceConditionList.Codes.EventReferenceParameters,
				triggerConditionValue: "PTP=DtbConsignmentVariation",
				description: "Conditional trigger");

			Factory.Save();

			AssertEquals(ZDateTime.Empty, unconditionalTrigger.P9_ActualDate);
			AssertEquals(ZDateTime.Empty, conditionalTrigger.P9_ActualDate);

			var referenceParameters = new[] { new KeyValuePair<string, string>("PTP", "DtbConsignmentVariation") };
			variation.Logs.AddNew(Events.JobOpen, ZDateTimeOffset.Now, referenceParameters);
			Factory.Save();

			AssertNotEquals("Variation events should be available to fire triggers on related consignments.", ZDateTime.Empty, unconditionalTrigger.P9_ActualDate);
			AssertNotEquals("Triggers with a conditional macro that requires the event source to be a related variation should also be fired.", ZDateTime.Empty, conditionalTrigger.P9_ActualDate);
		}

		public void TestTriggersOnRunSheets_ShouldRespondToRelatedVariationEvents()
		{
			var variation = helper.CreateVariation();
			Factory.Save();

			var unconditionalTrigger = MasterFilesTestHelper.CreateTrigger(variation.Consignment, Events.JobOpenCode,
				description: "Unconditional trigger");

			var conditionalTrigger = MasterFilesTestHelper.CreateTrigger(variation.Consignment, Events.JobOpenCode,
				triggerCondition: EventReferenceConditionList.Codes.EventReferenceParameters,
				triggerConditionValue: "PTP=DtbConsignmentVariation",
				description: "Conditional trigger");

			Factory.Save();

			AssertEquals(ZDateTime.Empty, unconditionalTrigger.P9_ActualDate);
			AssertEquals(ZDateTime.Empty, conditionalTrigger.P9_ActualDate);

			var referenceParameters = new[] { new KeyValuePair<string, string>("PTP", "DtbConsignmentVariation") };
			variation.Logs.AddNew(Events.JobOpen, ZDateTimeOffset.Now, referenceParameters);
			Factory.Save();

			AssertNotEquals("Variation events should be available to fire triggers on related run sheets.", ZDateTime.Empty, unconditionalTrigger.P9_ActualDate);
			AssertNotEquals("Triggers with a conditional macro that requires the event source to be a related variation should also be fired.", ZDateTime.Empty, conditionalTrigger.P9_ActualDate);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return helper.CreateVariation();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new TransportConsignmentTestHelper(Factory);
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
