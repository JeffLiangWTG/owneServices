using System;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateBookingProcessTaskCollection))]
	sealed class GateBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<GateBookingProcessTaskCollection>
	{
		protected override GateBookingProcessTaskCollection GetCollectionToTestCore()
		{
			var parent = Factory.New<GateBooking>();
			return new GateBookingProcessTaskCollection(parent);
		}

		public void TestIsCondition1Met_Cargo()
		{
			var collection = GetCollectionToTestWithParentSetup(gateBookingDetail =>
			{
				gateBookingDetail.GTD_IsContainer = true;
			});

			Assert("Booking is for Container - not Cargo", !collection.IsCondition1Met(GateBookingWorkflowCondition1CodeList.Codes.Cargo));

			collection = GetCollectionToTestWithParentSetup(gateBookingDetail =>
			{
				gateBookingDetail.GTD_IsContainer = false;
			});

			Assert("Booking is for Cargo - not Container", collection.IsCondition1Met(GateBookingWorkflowCondition1CodeList.Codes.Cargo));
		}

		public void TestIsCondition1Met_FullContainer()
		{
			var collection = GetCollectionToTestWithParentSetup(gateBookingDetail =>
			{
				var yardUnit = Factory.NewWithValidTestData<YardUnit>();
				yardUnit.GTY_IsEmpty = true;

				gateBookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
				gateBookingDetail.GTD_IsContainer = true;
			});

			Assert("Booking is for Container and it is empty", !collection.IsCondition1Met(GateBookingWorkflowCondition1CodeList.Codes.FullContainer));

			collection = GetCollectionToTestWithParentSetup(gateBookingDetail =>
			{
				var yardUnit = Factory.NewWithValidTestData<YardUnit>();
				yardUnit.GTY_IsEmpty = false;

				gateBookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
				gateBookingDetail.GTD_IsContainer = true;
			});

			Assert("Booking is for Container and it is full", collection.IsCondition1Met(GateBookingWorkflowCondition1CodeList.Codes.FullContainer));
		}

		public void TestIsCondition1Met_EmptyContainer()
		{
			var collection = GetCollectionToTestWithParentSetup(gateBookingDetail =>
			{
				var yardUnit = Factory.NewWithValidTestData<YardUnit>();
				yardUnit.GTY_IsEmpty = true;

				gateBookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
				gateBookingDetail.GTD_IsContainer = true;
			});

			Assert("Booking is for Container and it is empty", collection.IsCondition1Met(GateBookingWorkflowCondition1CodeList.Codes.EmptyContainer));

			collection = GetCollectionToTestWithParentSetup(gateBookingDetail =>
			{
				var yardUnit = Factory.NewWithValidTestData<YardUnit>();
				yardUnit.GTY_IsEmpty = false;

				gateBookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
				gateBookingDetail.GTD_IsContainer = true;
			});

			Assert("Booking is for Container and it is full", !collection.IsCondition1Met(GateBookingWorkflowCondition1CodeList.Codes.EmptyContainer));
		}

		GateBookingProcessTaskCollection GetCollectionToTestWithParentSetup(Action<GateBookingDetail> gateBookingDetailSetupAction)
		{
			var collection = GetCollectionToTestCore();
			var parent = collection.Parent;
			var detail = parent.GateBookingDetails.AddNew();
			gateBookingDetailSetupAction(detail);

			return collection;
		}
	}
}
