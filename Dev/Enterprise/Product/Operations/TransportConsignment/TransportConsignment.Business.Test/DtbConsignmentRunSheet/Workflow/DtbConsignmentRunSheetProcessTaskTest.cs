using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetProcessTask))]
	sealed class DtbConsignmentRunSheetProcessTaskTest : ProcessTaskTest
	{
		public void TestParentControllerID()
		{
			var processTask = (DtbConsignmentRunSheetProcessTask)GetNewBusinessObject();
			AssertEquals(ControllerIDs.DtbConsignmentRunSheet, processTask.ParentControllerID);
		}

		public void TestParentType_RunSheet()
		{
			var runSheet = Helper.CreateRunSheet();
			var processTask = runSheet.WorkflowItems.AddNew();
			AssertEquals(typeof(DtbConsignmentRunSheet), processTask.Parent.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var runSheet = Helper.CreateRunSheet();
			return runSheet.WorkflowItems.AddNew();
		}

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
