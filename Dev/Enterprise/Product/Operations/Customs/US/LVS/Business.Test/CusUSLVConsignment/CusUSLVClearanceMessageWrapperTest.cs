using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearanceMessageWrapper))]
	public class CusUSLVClearanceMessageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestResetCusUSLVConsignmentsActions_EntireClearance()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var wrapperCreatedWithClearance = new CusUSLVClearanceMessageWrapper(clearance);
			wrapperCreatedWithClearance.ResetCusUSLVConsignmentsActions();

			AssertNull(consignment1.Action);
			AssertNull(consignment2.Action);
		}

		public void TestResetCusUSLVConsignmentsActions_SingleConsignment()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var wrapperCreatedWithClearance = new CusUSLVClearanceMessageWrapper(clearance, consignment1);
			wrapperCreatedWithClearance.ResetCusUSLVConsignmentsActions();

			AssertNull(consignment1.Action);
			AssertEquals(UpdateActionCode.Add, consignment2.Action);
		}

		public void TestResetCusUSLVConsignmentsActions_SelectedConsignments()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var wrapperCreatedWithClearance = new CusUSLVClearanceMessageWrapper(clearance, new[] { consignment1, consignment2 });
			wrapperCreatedWithClearance.ResetCusUSLVConsignmentsActions();

			AssertNull(consignment1.Action);
			AssertNull(consignment2.Action);
		}

		public void TestCusUSLVConsignmentsToSend_EntireClearance()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var wrapperCreatedWithClearance = new CusUSLVClearanceMessageWrapper(clearance);

			var pksToSend = wrapperCreatedWithClearance.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, pksToSend);
		}

		public void TestCusUSLVConsignmentsToSend_SingleConsignment()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var wrapperCreatedWithSingleConsignment = new CusUSLVClearanceMessageWrapper(clearance, consignment1);

			var pksToSend = wrapperCreatedWithSingleConsignment.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK }, pksToSend);
		}

		public void TestCusUSLVConsignmentsToSend_SelectedConsignments()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			consignment1.InitAction(UpdateActionCode.Add);
			consignment2.InitAction(UpdateActionCode.Add);

			var wrapperCreatedWithClearance = new CusUSLVClearanceMessageWrapper(clearance, new[] { consignment1, consignment2 });

			var pksToSend = wrapperCreatedWithClearance.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, pksToSend);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => new CusUSLVClearanceMessageWrapper(Factory.NewWithValidTestData<CusUSLVClearance>());

		#endregion
	}
}
