using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentLodgementPivot))]
	sealed class DtbConsignmentLodgementPivotTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.CreateConsignmentLodgementPivot();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Helper.CreateConsignmentLodgementPivot();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateConsignmentLodgementPivot();
		}

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper helper;

		#endregion
	}
}
