using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVConsignmentForMessagingCollection))]
	class CusUSLVConsignmentForMessagingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusUSLVConsignmentForMessagingCollection>
	{
		public void TestNewCollection_FromClearance()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var collection = new CusUSLVConsignmentForMessagingCollection(clearance);

			var pksInCollection = collection.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, pksInCollection);
		}

		public void TestInactiveConsignmentsAreExcluded()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			consignment2.ULB_IsActive = false;

			var collection = new CusUSLVConsignmentForMessagingCollection(clearance);

			var pksInCollection = collection.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertEquals(consignment1.PK, pksInCollection.Single());
		}

		public void TestNewCollection_FromSingleConsignment()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var collection = new CusUSLVConsignmentForMessagingCollection(consignment1);

			var pksInCollection = collection.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK }, pksInCollection);
		}

		public void TestNewCollection_FromSelectedConsignments()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var collection = new CusUSLVConsignmentForMessagingCollection(clearance.Factory, new[] { consignment1, consignment2 });

			var pksInCollection = collection.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, pksInCollection);
		}

		public void TestNewCollection_ExcludesConsignmentsWithInvalidMessageStatus()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment2.InitAction(UpdateActionCode.Add);

			CombineAssertions("Precondtion", () =>
			{
				AssertEquals("Consignment1 is Invalid", false, MessageSendingHelper.HasValidStatusOnConsignment(consignment1));
				AssertEquals("Consignment2 is Valid", true, MessageSendingHelper.HasValidStatusOnConsignment(consignment2));
			});
			var collection = new CusUSLVConsignmentForMessagingCollection(clearance);

			var pksInCollection = collection.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2.PK }, pksInCollection);
		}

		public void TestAddNew_ShouldNotBeSupported()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<InvalidOperationException>(() => collection.AddNew());
		}

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, GetCollectionToTest().AllowRemove);
		}

		#region Implementation

		protected override CusUSLVConsignmentForMessagingCollection GetCollectionToTest() => new CusUSLVConsignmentForMessagingCollection(Factory.NewWithValidTestData<CusUSLVConsignment>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());

		#endregion
	}
}
