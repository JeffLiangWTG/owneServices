using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(BillOfLadingWorkflowDescriptor))]
	internal class BillOfLadingWorkflowDescriptorTest : AgencyShipmentWorkflowDescriptorTestBase<BillOfLading, BillOfLadingWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "BOL", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Bill Of Lading", WorkflowDescriptor.Description);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.AgencyDocumentation, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public new void TestWorkflowTriggerActionTypes()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var testItem = new BillOfLadingWorkflowDescriptor();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				CombineAssertions(() =>
				{
					template.GlobalTemplate = false;
					var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should contain EID for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEIDO));
					Assert("Should not contain IRO for non global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder));

					template.GlobalTemplate = true;
					testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should not contain EDI for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEIDO));
					Assert("Should not contain IRO for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder));
				});
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				CombineAssertions(() =>
				{
					template.GlobalTemplate = false;
					var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should not contain EID for non global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEIDO));
					Assert("Should contain IRO for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder));

					template.GlobalTemplate = true;
					testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should not contain EDI for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEIDO));
					Assert("Should not contain IRO for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder));
				});
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return base.ExpectedSupportedMessageRecipientParties | MessageRecipientPartyType.DeliveryCartage | MessageRecipientPartyType.PickupCartage | MessageRecipientPartyType.ArrivalCTO;
			}
		}

		#region EstimateDefaultedFromList
		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfArrival));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfDeparture));
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(BillOfLadingDefaultedFromList.Codes.ShippedOnBoard));
		}

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			switch (dateTimeSourceType)
			{
				case BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfArrival:
					return TimeSpan.FromHours(10);
				case BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfDeparture:
					return TimeSpan.FromHours(11);
				case BillOfLadingDefaultedFromList.Codes.ShippedOnBoard:
					return TimeSpan.FromHours(8); // AUPER
				default:
					return base.GetUtcOffsetForDateTimeSourceType(workflowProvider, dateTimeSourceType);
			}
		}

		protected override IWorkflowProvider GetParentForGettingDateTimeOffset()
		{
			var job = (BillOfLading)base.GetParentForGettingDateTimeOffset();
			job.JS_RL_NKLoadPort = "AUPER";
			return job;
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			switch (dateTimeSourceType)
			{
				case BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfArrival:
					var bill = ((BillOfLading)workflowProvider);
					bill.Transports[bill.Transports.Count - 1].JW_ATA = localTime;
					break;
				case BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfDeparture:
					((BillOfLading)workflowProvider).Transports[0].JW_ATD = localTime;
					break;
				case BillOfLadingDefaultedFromList.Codes.ShippedOnBoard:
					((BillOfLading)workflowProvider).JS_ShippedOnBoardDate = localTime;
					break;
				default:
					base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
					break;
			}
		}
		#endregion
	}
}
