using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed partial class DtbConsignmentRunSheetCMRConsignmentNoteBuilderTest : TestCaseWithFactory
	{
		public void TestGetRunSheetCMRConsignmentNoteBuilder()
		{
			var runSheet = TransportBookingHelper.CreateRunSheet();

			var consignment1 = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var action1 = address1.Actions.AddNew();
			AssertNull(action1.RunSheetInstruction);
			var runSheetInstruction1 = runSheet.RunSheetInstructions.AddNew();
			action1.LTA_K1_RunSheetInstruction = runSheetInstruction1.PK;
			AssertEquals(runSheetInstruction1, action1.RunSheetInstruction);

			var consignment2 = Helper.CreateConsignment();
			var address2 = Helper.CreateConsignmentAddress(consignment2, ConsignmentAddressTypes.Codes.Delivery);
			var action2 = address2.Actions.AddNew();
			AssertNull(action2.RunSheetInstruction);
			var runSheetInstruction2 = runSheet.RunSheetInstructions.AddNew();
			action2.LTA_K1_RunSheetInstruction = runSheetInstruction2.PK;
			AssertEquals(runSheetInstruction2, action2.RunSheetInstruction);

			var consignment3 = Helper.CreateConsignment();
			var address3 = Helper.CreateConsignmentAddress(consignment3, ConsignmentAddressTypes.Codes.PickUp);
			var action3 = address3.Actions.AddNew();
			AssertNull(action3.RunSheetInstruction);
			var runSheetInstruction3 = runSheet.RunSheetInstructions.AddNew();
			action3.LTA_K1_RunSheetInstruction = runSheetInstruction3.PK;
			AssertEquals(runSheetInstruction3, action3.RunSheetInstruction);

			var address4 = Helper.CreateConsignmentAddress(consignment3, ConsignmentAddressTypes.Codes.Delivery); // Reuse consignment3
			var action4 = address4.Actions.AddNew();
			AssertNull(action4.RunSheetInstruction);
			var runSheetInstruction4 = runSheet.RunSheetInstructions.AddNew();
			action4.LTA_K1_RunSheetInstruction = runSheetInstruction4.PK;
			AssertEquals(runSheetInstruction4, action4.RunSheetInstruction);

			var builder = new DtbConsignmentRunSheetCMRConsignmentNoteBuilder(runSheet);
			AssertEquals("should return a CMRConsignmentNote builder", typeof(DtbConsignmentRunSheetCMRConsignmentNoteBuilder), builder.GetType());

			var cmrConsignmentNoteCollection = builder.Build();
			AssertEquals("should return a CMRConsignmentNoteDocDataObjectCollection", typeof(CMRConsignmentNoteDocDataObjectCollection), cmrConsignmentNoteCollection.GetType());
			AssertEquals("should return 3 CMRConsignmentNoteDocDataObject", 3, cmrConsignmentNoteCollection.CMRConsignmentNoteDocuments.Count);

			AssertEquals("should return CMRConsignmentNoteDocDataObject for consignment1", cmrConsignmentNoteCollection.CMRConsignmentNoteDocuments[0].JobNumber, consignment1.LTC_JobID);
			AssertEquals("should return CMRConsignmentNoteDocDataObject for consignment2", cmrConsignmentNoteCollection.CMRConsignmentNoteDocuments[1].JobNumber, consignment2.LTC_JobID);
			AssertEquals("should return CMRConsignmentNoteDocDataObject for consignment3", cmrConsignmentNoteCollection.CMRConsignmentNoteDocuments[2].JobNumber, consignment3.LTC_JobID);
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		TransportBookingConsignmentTestHelper TransportBookingHelper
		{
			get { return transportBookingHelper ?? (transportBookingHelper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper transportBookingHelper;
	}
}
