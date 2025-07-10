using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentLodgement))]
	sealed class DtbConsignmentLodgementTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.CreateConsignmentLodgement();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Helper.CreateConsignmentLodgement();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateConsignmentLodgement();
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
