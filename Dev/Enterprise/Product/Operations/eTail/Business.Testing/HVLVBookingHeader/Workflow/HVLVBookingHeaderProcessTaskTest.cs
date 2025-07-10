using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeaderProcessTask))]
	public class HVLVBookingHeaderProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			return bookingHeader.WorkflowItems.AddNew();
		}

		#endregion
	}
}
