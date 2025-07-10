using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(UpdateHVLVItemStatusApplicator))]
	public class UpdateHVLVItemStatusApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestUpdateHVLVItemStatus()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "Consignment1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "Consignment2";
			var item1 = consignment2.Items.AddNew();
			item1.HVI_ItemId = "HVI000000000000001";
			item1.HVI_Status = HVLVItemStatus.Codes.ShipmentAllocated;

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_ConsignmentId = "Consignment3";
			var item2 = consignment3.Items.AddNew();
			item2.HVI_ItemId = "HVI000000000000002";
			item2.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			var item3 = consignment3.Items.AddNew();
			item3.HVI_ItemId = "HVI000000000000003";
			item3.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;

			Factory.Save();

			AssertEquals("PRECONDITION", HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);
			AssertEquals("PRECONDITION", HVLVItemStatus.Codes.ManifestedByETailer, item2.HVI_Status);
			AssertEquals("PRECONDITION", HVLVItemStatus.Codes.ManifestedByETailer, item3.HVI_Status);

			var expected = new[]
			{
				"INFO: \tProcessing Item with Item ID HVI000000000000001\n",
				"INFO: \t\tSkipped, because item status is the same\n",
				"INFO: \tProcessing Item with Item ID HVI000000000000002\n",
				"INFO: \tProcessed!\n",
				"INFO: \tProcessing Item with Item ID HVI000000000000003\n",
				"INFO: \tProcessed!\n",
				"INFO: Finished Processing\r\n"
			};

			Applicator.StatusCode = HVLVItemStatus.Codes.ShipmentAllocated;
			ApplyApplicatorWhereLogOrderIsUnimportant(new BusinessObject[] { consignment1, consignment2, consignment3 }, expected);

			AssertEquals("Item 1 status should stay the same", HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);
			AssertEquals("Item 2 status should be updated to Shipment Allocated", HVLVItemStatus.Codes.ShipmentAllocated, item2.HVI_Status);
			AssertEquals("Item 3 status should be updated to Shipment Allocated", HVLVItemStatus.Codes.ShipmentAllocated, item3.HVI_Status);
		}

		public void TestUpdateHVLVItemStatusValidation_HVLVItemStatus()
		{
			Applicator.StatusCode = ZString.Empty;
			AssertHasError(Applicator.StatusCodeInfo, "Please enter a value.");

			Applicator.StatusCode = "ZZZ";
			AssertHasError(Applicator.StatusCodeInfo, "Enter a valid selection.");

			Applicator.StatusCode = HVLVItemStatus.Codes.Delivered;
			AssertNoErrors(Applicator.StatusCodeInfo);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2022, 4, 22, 12, 0, 0, 0)]
		public void TestUpdateHVLVItemStatusValidation_STUEventTime()
		{
			Applicator.STUEventTime = ZDateTimeOffset.Empty;
			AssertHasError(Applicator.STUEventTimeInfo, "Please enter a value.");

			Applicator.STUEventTime = new ZDateTimeOffset(2022, 4, 23);
			AssertHasError(Applicator.STUEventTimeInfo, "Entered time must be in the past.");

			Applicator.STUEventTime = new ZDateTimeOffset(2022, 4, 21);
			AssertNoErrors(Applicator.StatusCodeInfo);
		}

		public void TestUpdateHVLVItemStatusApplicator_NoHVLVConsignments()
		{
			var expectedError = "ERROR: No HVLV Consignments selected.";
			ApplyApplicator(System.Array.Empty<BusinessObject>(), expectedError);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2022, 4, 22, 12, 0, 0, 0)]
		public void TestUpdateHVLVItemStatus_WhenStatusUpdatedAfterTime_ThenDontUpdateStatus()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "Consignment1";
			var item1 = consignment.Items.AddNew();
			item1.HVI_ItemId = "HVI000000000000001";
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;

			Factory.Save();

			AssertEquals("PRECONDITION", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);

			var expectedUpdate1 = new[]
			{
				"INFO: \tProcessing Item with Item ID HVI000000000000001\n",
				"INFO: \tProcessed!\n",
				"INFO: Finished Processing\r\n"
			};

			var update1Time = ZDateTimeOffset.Now;
			Applicator.StatusCode = HVLVItemStatus.Codes.ShipmentDeparted;
			Applicator.STUEventTime = update1Time;
			ApplyApplicatorWhereLogOrderIsUnimportant(new BusinessObject[] { consignment }, expectedUpdate1);

			AssertEquals("Item 1 status code should be updated to Shipment Departed", HVLVItemStatus.Codes.ShipmentDeparted, item1.HVI_Status);

			var logQuery1 = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode);
			logQuery1.AddToFilter(StmALogSchema.SL_Parent, item1.PK);
			logQuery1.AddToFilter(StmALogSchema.SL_Table, HVLVItemSchema.Constants.TableName);
			var logs1 = Factory.Load<StmALog>(logQuery1);

			AssertEquals(1, logs1.Length);
			var referenceNumber = item1.GetItemEventReferenceNumber();
			var referenceType = item1.GetItemEventReferenceType();
			var eventReference1 = new EventReference(AutoEvents.StatusUpdatedCode, referenceNumber);
			eventReference1.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.Old, HVLVItemStatus.Codes.ManifestedByETailer));
			eventReference1.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.New, HVLVItemStatus.Codes.ShipmentDeparted));
			eventReference1.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.ReferenceNumber, referenceNumber));
			eventReference1.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.Type, referenceType));

			AssertEquals(true, eventReference1.CompleteText.Contains(logs1[0].referenceFreeText ?? null));

			Factory.Save();

			var expectedUpdate2 = new[]
			{
				"INFO: \tProcessing Item with Item ID HVI000000000000001\n",
				"INFO: \tProcessed! Status updated since entered event time so current status is maintained.\n",
				"INFO: Finished Processing\r\n"
			};

			var update2Time = new ZDateTimeOffset(2022, 4, 20);
			Applicator.StatusCode = HVLVItemStatus.Codes.ShipmentAllocated;
			Applicator.STUEventTime = update2Time;
			ApplyApplicatorWhereLogOrderIsUnimportant(new BusinessObject[] { consignment }, expectedUpdate2);

			AssertEquals("Item 1 status code should remain Shipment Departed", HVLVItemStatus.Codes.ShipmentDeparted, item1.HVI_Status);

			var logQuery2 = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode);
			logQuery2.AddToFilter(StmALogSchema.SL_Parent, item1.PK);
			logQuery2.AddToFilter(StmALogSchema.SL_Table, HVLVItemSchema.Constants.TableName);
			var logs2 = Factory.Load<StmALog>(logQuery2);

			AssertEquals(2, logs2.Length);
			var eventReference2 = new EventReference(AutoEvents.StatusUpdatedCode, referenceNumber);
			eventReference2.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.New, HVLVItemStatus.Codes.ShipmentAllocated));
			eventReference2.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.ReferenceNumber, referenceNumber));
			eventReference2.ParameterCollection.Add(new Parameter(AutoEvents.StatusUpdatedCode, Codes.Type, referenceType));

			AssertEquals(true, eventReference2.CompleteText.Contains(logs2[0].referenceFreeText ?? null) || eventReference2.CompleteText.Contains(logs2[1].referenceFreeText ?? null));
		}

		public void TestUpdateHVLVItemStatusApplicator_LogsAddedWhenStatusIsDifferentToCurrent()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "Consignment1";
			var item1 = consignment.Items.AddNew();
			item1.HVI_ItemId = "HVI000000000000001";
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;

			Factory.Save();

			AssertEquals("PRECONDITION", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);

			var expected = new[]
			{
				"INFO: \tProcessing Item with Item ID HVI000000000000001\n",
				"INFO: \tProcessed!\n",
				"INFO: Finished Processing\r\n"
			};

			Applicator.StatusCode = HVLVItemStatus.Codes.ShipmentAllocated;
			ApplyApplicatorWhereLogOrderIsUnimportant(new BusinessObject[] { consignment }, expected);

			AssertEquals("Item 1 status code should be updated to Shipment Allocated", HVLVItemStatus.Codes.ShipmentAllocated, item1.HVI_Status);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, item1.PK);
			logQuery.AddToFilter(StmALogSchema.SL_Table, HVLVItemSchema.Constants.TableName);
			var logs = Factory.Load<StmALog>(logQuery);

			AssertEquals(1, logs.Length);
		}

		public void TestUpdateHVLVItemStatusApplicator_NoLogsAddedWhenStatusIsSameAsCurrent()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "Consignment1";
			var item1 = consignment.Items.AddNew();
			item1.HVI_ItemId = "HVI000000000000001";
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;

			Factory.Save();

			AssertEquals("PRECONDITION", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);

			var expected = new[]
			{
				"INFO: \tProcessing Item with Item ID HVI000000000000001\n",
				"INFO: \t\tSkipped, because item status is the same\n",
				"INFO: Finished Processing\r\n"
			};

			Applicator.StatusCode = HVLVItemStatus.Codes.ManifestedByETailer;
			ApplyApplicatorWhereLogOrderIsUnimportant(new BusinessObject[] { consignment }, expected);

			AssertEquals("Item 1 status code should remain Manifested By ETailer", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdatedCode);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, item1.PK);
			logQuery.AddToFilter(StmALogSchema.SL_Table, HVLVItemSchema.Constants.TableName);
			var logs = Factory.Load<StmALog>(logQuery);

			AssertEquals(0, logs.Length);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateHVLVItemStatusApplicator(new BusinessObjectFactory());
		}

		new UpdateHVLVItemStatusApplicator Applicator => (UpdateHVLVItemStatusApplicator)base.Applicator;
	}
}
