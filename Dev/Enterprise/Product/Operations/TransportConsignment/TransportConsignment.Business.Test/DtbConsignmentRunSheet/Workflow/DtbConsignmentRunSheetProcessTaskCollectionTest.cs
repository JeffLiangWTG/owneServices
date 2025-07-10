using System;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetProcessTaskCollection))]
	sealed class DtbConsignmentRunSheetProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbConsignmentRunSheetProcessTaskCollection>
	{
		#region Implementation

		protected override DtbConsignmentRunSheetProcessTaskCollection GetCollectionToTestCore()
		{
			return new DtbConsignmentRunSheetProcessTaskCollection(Helper.CreateRunSheet());
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(DtbConsignmentRunSheetProcessTaskCollection);
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
