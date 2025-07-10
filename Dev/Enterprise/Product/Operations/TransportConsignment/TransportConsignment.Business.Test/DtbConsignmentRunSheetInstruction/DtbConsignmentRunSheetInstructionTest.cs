using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstruction))]
	sealed class DtbConsignmentRunSheetInstructionTest : DtbBookingConsignmentBusinessObjectTestCase
	{
		#region Related Entities

		#region IPalletTransactionParent

		public void TestIPalletTransactionParent()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ALE";
			var branch = Factory.New<GlbBranch>();
			staff.GS_GB_HomeBranch = branch.PK;

			var runsheet = Factory.New<DtbConsignmentRunSheet>();
			runsheet.KG_RunSheetNumber = "RUNLOLA";
			var runSheetInstruction = runsheet.RunSheetInstructions.AddNew();
			runSheetInstruction.K1_Sequence = 3;
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.PickupInstruction.PickupConfirmation;
			consignment.PickupInstruction.Address.E2_AddressOverride = true;
			consignment.PickupInstruction.Address.E2_Address1 = "18 Henricks Avenue";
			confirmation.KK_K1_RunSheetInstruction = runSheetInstruction.PK;

			var palletParent = (IPalletTransactionParent)runSheetInstruction;
			AssertEquals("Run Sheet RUNLOLA #3", palletParent.GetJobDescription());
			AssertEquals("Run Sheet", palletParent.GetJobDescription(typeof(DtbConsignmentRunSheetInstruction)));
			AssertEquals(null, palletParent.RelevantBranch);

			runsheet.KG_GS_NKTruckDriver = staff.GS_Code;
			AssertEquals(branch, palletParent.RelevantBranch);

			AssertEquals(1, palletParent.JobReferences.Count());
			AssertEquals("RUNLOLA #3", palletParent.JobReferences.Single());
		}

		#endregion

		#region TestActions

		public void TestActions()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals("Check collection is cached.", runSheetInstruction.Actions, runSheetInstruction.Actions);
			AssertEquals(typeof(DtbConsignmentActionCollection), runSheetInstruction.Actions.GetType());
			AssertEquals("Actions should be registered editable on RunSheet Instruciton.", true, runSheetInstruction.IsRegisteredEditableChildObject(runSheetInstruction.Actions));

			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var address = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();
			action.LTA_K1_RunSheetInstruction = runSheetInstruction.PK;

			AssertContainsExactElementsInAnyOrder(new[] { action }, runSheetInstruction.Actions);
		}

		#endregion

		#region TestConsignmentActions

		public void TestConsignmentActions()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals(typeof(DtbConsignmentActionCollection), runSheetInstruction.ConsignmentActions.GetType());

			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var address = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();
			action.LTA_K1_RunSheetInstruction = runSheetInstruction.PK;

			AssertContainsExactElementsInAnyOrder(new[] { action }, runSheetInstruction.ConsignmentActions);
			AssertContainsExactElementsInAnyOrder(runSheetInstruction.Actions.ToList(), runSheetInstruction.ConsignmentActions);
		}

		#endregion

		#region TestAddress

		public void TestAddress_Action()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertNull(runSheetInstruction.Address);

			var address = Factory.New<OrgAddress>();
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, address);
			var action = consignmentAddress.Actions.AddNew();
			action.LTA_K1_RunSheetInstruction = runSheetInstruction.PK;

			AssertEquals("Address should be dirrectly taken from Instruction.", consignmentAddress.Address, runSheetInstruction.Address);
		}

		#endregion

		#endregion

		#region Properties

		#region K1_Sequence

		public void TestK1_Sequence()
		{
			AssertEquals(true, Factory.New<DtbConsignmentRunSheetInstruction>().K1_SequenceInfo.ReadOnly);
		}

		#endregion

		#region TestK1_ReceivedBy

		public void TestK1_ReceivedBy()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var pickUpConfirmation = consignment.PickupInstruction.Confirmations[0];
			var runSheetInstruction = Helper.CreateRunSheetInstruction(pickUpConfirmation);
			AssertEquals(true, runSheetInstruction.K1_ReceivedByInfo.ReadOnly);
		}

		#endregion

		#region TestAddressAsSingleLine

		public void TestAddressAsSingleLine()
		{
			var runsheetInstructionWithNoAddress = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals("", runsheetInstructionWithNoAddress.AddressAsSingleLine);

			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var runsheetInstruction1 = Helper.CreateRunSheetInstruction(consignment.PickupInstruction.Confirmations[0]);
			AssertEquals("1/2 SOME OTHER STREET MELBOURNE VIC 3039", runsheetInstruction1.AddressAsSingleLine);

			var runsheetInstruction2 = Helper.CreateRunSheetInstruction(consignment.PickupDepotInstruction.Confirmations.First(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp));
			AssertEquals("DEPOT", runsheetInstruction2.AddressAsSingleLine);

			consignment.PickupInstruction.OrganisationType = "CFS";
			AssertEquals("1/2 SOME OTHER STREET MELBOURNE VIC 3039", runsheetInstruction1.AddressAsSingleLine);
		}

		#endregion

		#region TestHasSignature

		public void TestHasSignature()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals(false, runSheetInstruction.HasSignature);

			runSheetInstruction.K1_ReceivedBySignature = ZBlob.FromAscii("test");
			AssertEquals(true, runSheetInstruction.HasSignature);
		}

		#endregion

		#region TestFailureReasonDescription

		public void TestFailureReasonDescription()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals("", runSheetInstruction.FailureReasonDescription);

			foreach (SystemDefinableCodeDescriptionBool failureReason in runSheetInstruction.Lookups.FailureReasons)
			{
				runSheetInstruction.K1_FailureReason = failureReason.Code;
				AssertEquals(failureReason.Description, runSheetInstruction.FailureReasonDescription);
			}
		}

		#endregion

		// calculated -- totals

		#region TestTotalWeightUnit

		public void TestTotalWeightUnit()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var runsheetInstruction = Helper.CreateRunSheetInstruction(consignment.PickupInstruction.Confirmations[0]);

			var originalUnit = PackingRegistry.Instance.WeightUnit.Value;
			try
			{
				PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);
				AssertEquals(runsheetInstruction.TotalWeightUnit, Constants.Weight.Kilograms);

				PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Grams);
				AssertEquals(runsheetInstruction.TotalWeightUnit, Constants.Weight.Grams);
			}
			finally
			{
				PackingRegistry.Instance.SetWeightUnitForTest(originalUnit);
			}
		}

		#endregion

		#region TestTotalVolumeUnit

		public void TestTotalVolumeUnit()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var runsheetInstruction = Helper.CreateRunSheetInstruction(consignment.PickupInstruction.Confirmations[0]);

			var originalUnit = PackingRegistry.Instance.VolumeUnit.Value;
			try
			{
				PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicMetres);
				AssertEquals(runsheetInstruction.TotalVolumeUnit, Constants.Volume.CubicMetres);

				PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicInches);
				AssertEquals(runsheetInstruction.TotalVolumeUnit, Constants.Volume.CubicInches);
			}
			finally
			{
				PackingRegistry.Instance.SetVolumeUnitForTest(originalUnit);
			}
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsConsignmentAction

		public void TestIsConsignmentAction()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var address = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = address.Actions.AddNew();

			var runsheet = helper.CreateRunSheet();
			var runSheetInstruction1 = helper.CreateRunSheetInstruction(action, runsheet.PK);
			AssertEquals(true, runSheetInstruction1.IsConsignmentAction);

			var runSheetInstruction2 = Factory.New<DtbConsignmentRunSheetInstruction>();
			var bookingConsignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = bookingConsignment.PickupInstruction.PickupConfirmation;
			confirmation.KK_K1_RunSheetInstruction = runSheetInstruction2.PK;
			AssertEquals(false, runSheetInstruction2.IsConsignmentAction);
		}

		#endregion

		#region TestIsCompleted

		public void TestIsCompleted()
		{
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals("Pre-condition: K1_TimeOut.IsEmpty.", true, instruction.K1_TimeOut.IsEmpty);
			AssertEquals("Pre-condition: K1_ReceivedBy.IsEmpty.", true, instruction.K1_ReceivedBy.IsEmpty);
			AssertEquals("Pre-condition: K1_FailureReason.IsEmpty.", true, instruction.K1_FailureReason.IsEmpty);
			AssertEquals("IsCompleted", false, instruction.IsCompleted);

			instruction.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			instruction.K1_TimeOut = ZDateTimeOffset.Now.AddHours(1);
			instruction.K1_ReceivedBy = "TST";
			AssertEquals("IsCompleted", true, instruction.IsCompleted);

			instruction.K1_FailureReason = "OTH";
			AssertEquals("IsCompleted", false, instruction.IsCompleted);
		}

		#endregion

		#region TestIsOwnDepot

		public void TestIsOwnDepot_Actions()
		{
			var helper = new TransportConsignmentTestHelper(Factory);

			var consignment = helper.CreateConsignment();
			var nonCFSPickUpAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			var cfsPickUpAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			var nonCFSPickUpAction = helper.CreateConsignmentAction(nonCFSPickUpAddress, ActionTypes.Codes.PickUp);
			var cfsPickUpAction = helper.CreateConsignmentAction(cfsPickUpAddress, ActionTypes.Codes.PickUp);
			var nonCFSrunSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			var cfsrunSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			nonCFSrunSheetInstruction.Actions.Add(nonCFSPickUpAction);
			cfsrunSheetInstruction.Actions.Add(cfsPickUpAction);
			AssertEquals(false, nonCFSrunSheetInstruction.IsOwnDepot);
			AssertEquals(false, cfsrunSheetInstruction.IsOwnDepot);

			cfsrunSheetInstruction.Address.DocAddressType = DocAddressType.LocalCartageCFS;
			AssertEquals(true, cfsrunSheetInstruction.IsOwnDepot);

			nonCFSPickUpAddress.Address.DocAddressType = DocAddressType.LocalCartageCTO;
			AssertEquals(false, nonCFSrunSheetInstruction.IsOwnDepot);
		}

		#endregion

		#region TestIsPickingUpConsignments

		public void TestIsPickingUpConsignments()
		{
			var helper = new TransportConsignmentTestHelper(Factory);

			var consignment = helper.CreateConsignment();
			var pickUpAddress1 = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var pickUpAddress2 = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickUpAction1 = helper.CreateConsignmentAction(pickUpAddress1, ActionTypes.Codes.PickUp);
			var pickUpAction2 = helper.CreateConsignmentAction(pickUpAddress2, ActionTypes.Codes.PickUp);
			var deliveryAction = helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			var pickingUpOnlyRunSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			var mixedRunSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals(false, pickingUpOnlyRunSheetInstruction.IsPickingUpConsignments);
			AssertEquals(false, mixedRunSheetInstruction.IsPickingUpConsignments);

			pickingUpOnlyRunSheetInstruction.Actions.Add(pickUpAction1);
			mixedRunSheetInstruction.Actions.Add(pickUpAction2);
			mixedRunSheetInstruction.Actions.Add(deliveryAction);
			AssertEquals(true, pickingUpOnlyRunSheetInstruction.IsPickingUpConsignments);
			AssertEquals(true, mixedRunSheetInstruction.IsPickingUpConsignments);
			AssertEquals(false, pickingUpOnlyRunSheetInstruction.IsDeliveringConsignments);
			AssertEquals(true, mixedRunSheetInstruction.IsDeliveringConsignments);
		}

		#endregion

		#region TestIsDeliveringConsignments

		public void TestIsDeliveringConsignments()
		{
			var helper = new TransportConsignmentTestHelper(Factory);

			var consignment = helper.CreateConsignment();
			var pickUpAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress1 = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var deliveryAddress2 = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickUpAction = helper.CreateConsignmentAction(pickUpAddress, ActionTypes.Codes.PickUp);
			var deliveryAction1 = helper.CreateConsignmentAction(deliveryAddress1, ActionTypes.Codes.Delivery);
			var deliveryAction2 = helper.CreateConsignmentAction(deliveryAddress2, ActionTypes.Codes.Delivery);
			var deliveryOnlyRunSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			var mixedRunSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals(false, deliveryOnlyRunSheetInstruction.IsDeliveringConsignments);
			AssertEquals(false, mixedRunSheetInstruction.IsDeliveringConsignments);

			deliveryOnlyRunSheetInstruction.Actions.Add(deliveryAction1);
			deliveryOnlyRunSheetInstruction.Actions.Add(deliveryAction2);
			mixedRunSheetInstruction.Actions.Add(pickUpAction);
			mixedRunSheetInstruction.Actions.Add(deliveryAction1);
			AssertEquals(true, deliveryOnlyRunSheetInstruction.IsDeliveringConsignments);
			AssertEquals(true, mixedRunSheetInstruction.IsDeliveringConsignments);
			AssertEquals(false, deliveryOnlyRunSheetInstruction.IsPickingUpConsignments);
			AssertEquals(true, mixedRunSheetInstruction.IsPickingUpConsignments);
		}

		#endregion

		#endregion

		#region TestLookups

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbConsignmentRunSheetInstructionLookups); }
		}

		protected override bool IsLookupsOverridden
		{
			get { return false; }
		}

		#endregion

		#region TestValidation

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbConsignmentRunSheetInstructionValidation); }
		}

		protected override bool IsValidationOverridden
		{
			get { return false; }
		}

		#endregion

		#region TestDelete_NoExceptionThrown

		public void TestDelete_NoExceptionThrown()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertNoExceptionThrown(() => { runSheetInstruction.Delete(); });
		}

		#endregion

		#region TestDelete_SequencesInstructions

		public void TestDelete_SequencesInstructions()
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction1 = runSheet.RunSheetInstructions.AddNew();
			var instruction2 = runSheet.RunSheetInstructions.AddNew();
			var instruction3 = runSheet.RunSheetInstructions.AddNew();
			var instruction4 = runSheet.RunSheetInstructions.AddNew();
			var instruction5 = runSheet.RunSheetInstructions.AddNew();

			AssertEquals("Precondition", 1, instruction1.K1_Sequence);
			AssertEquals("Precondition", 2, instruction2.K1_Sequence);
			AssertEquals("Precondition", 3, instruction3.K1_Sequence);
			AssertEquals("Precondition", 4, instruction4.K1_Sequence);
			AssertEquals("Precondition", 5, instruction5.K1_Sequence);

			instruction2.Delete();
			AssertEquals(1, instruction1.K1_Sequence);
			AssertEquals(2, instruction3.K1_Sequence);
			AssertEquals(3, instruction4.K1_Sequence);
			AssertEquals(4, instruction5.K1_Sequence);

			using (((IBusinessObjectCollection)runSheet.RunSheetInstructions).SuspendListChanged())
			{
				instruction4.Delete();
				AssertEquals(1, instruction1.K1_Sequence);
				AssertEquals(2, instruction3.K1_Sequence);
				AssertEquals(3, instruction5.K1_Sequence);

				instruction3.Delete();
				AssertEquals(1, instruction1.K1_Sequence);
				AssertEquals(2, instruction5.K1_Sequence);
			}
		}

		#endregion

		#region TestDelete_DeletesPalletTransactions

		public void TestDelete_DeletesPalletTransactions()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var runSheetInstruction = runSheet.RunSheetInstructions.AddNew();
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.PickupInstruction.PickupConfirmation;
			confirmation.KK_K1_RunSheetInstruction = runSheetInstruction.PK;

			var palletTransactionOnConfirmation = Factory.New<PkgPalletTransaction>();
			palletTransactionOnConfirmation.RelatedJob = confirmation;

			var palletTransactionOnRunsheetInstruction = Factory.New<PkgPalletTransaction>();
			palletTransactionOnRunsheetInstruction.RelatedJob = runSheetInstruction;

			runSheetInstruction.Delete();
			AssertEquals(true, palletTransactionOnRunsheetInstruction.IsDeleted);
			AssertEquals(false, palletTransactionOnConfirmation.IsDeleted);
		}

		#endregion

		#region TestISignatureSupporterMembers

		public void TestISignatureSupporterMembers()
		{
			var runSheetInstruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			AssertEquals(false, ((ISignatureSupporter)runSheetInstruction).HasSignature);
			AssertEquals(ZBlob.Empty, ((ISignatureSupporter)runSheetInstruction).ReceivedBySignature);

			runSheetInstruction.K1_ReceivedBySignature = ZBlob.FromAscii("test");
			AssertEquals(true, ((ISignatureSupporter)runSheetInstruction).HasSignature);
			AssertEquals(ZBlob.FromAscii("test"), ((ISignatureSupporter)runSheetInstruction).ReceivedBySignature);
		}

		#endregion

		#region ICustomProcessTaskHandlerProviderMembers

		public void TestGetHandler()
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction1 = runSheet.RunSheetInstructions.AddNew();
			var instruction2 = runSheet.RunSheetInstructions.AddNew();

			var trigger = runSheet.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.RunSheetInstruction;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());

			instruction1.GetLogs().AddNew(Events.CustomisableEvent00);
			instruction2.GetLogs().AddNew(Events.CustomisableEvent00);
			AssertEquals(2, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
		}

		static void SetupEmailNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "bung@bung.bung";
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new TransportConsignmentTestHelper(factory);
			var consignmentAddress = helper.CreateConsignmentAddress(ConsignmentAddressTypes.Codes.PickUp);
			var action = helper.CreateConsignmentAction(consignmentAddress, ActionTypes.Codes.PickUp);

			return helper.CreateRunSheetInstruction(action, helper.CreateRunSheet().PK);
		}

		#endregion
	}
}
