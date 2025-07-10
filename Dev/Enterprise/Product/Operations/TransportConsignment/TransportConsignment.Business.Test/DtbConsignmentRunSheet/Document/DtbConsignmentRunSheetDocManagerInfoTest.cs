using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetDocManagerInfo))]
	sealed class DtbConsignmentRunSheetDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region GetEmptyParentBusinessObject

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Helper.CreateRunSheet();
		}

		#endregion

		#region GetPopulatedParentBusinessObject

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Helper.CreateRunSheet();
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
