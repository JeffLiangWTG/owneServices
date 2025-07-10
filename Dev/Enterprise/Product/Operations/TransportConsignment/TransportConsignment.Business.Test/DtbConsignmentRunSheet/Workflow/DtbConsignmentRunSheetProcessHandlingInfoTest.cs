using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetProcessHandlingInfoTest : TestCaseWithFactory
	{
		#region TestDtbConsignmentRunSheetPropogateEventsToInstructions

		public void TestDtbConsignmentRunSheetPropogateEventsToInstructions()
		{
			var runSheet = Helper.CreateRunSheet();
			var instruction1 = runSheet.RunSheetInstructions.AddNew();
			var instruction2 = runSheet.RunSheetInstructions.AddNew();

			Factory.Save();
			var log = runSheet.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(1, instruction1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Count());
			AssertEquals(1, instruction2.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Count());
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

	}
}
