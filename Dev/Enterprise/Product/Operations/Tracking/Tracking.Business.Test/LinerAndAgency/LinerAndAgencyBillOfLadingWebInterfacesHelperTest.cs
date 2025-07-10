using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyBillOfLadingWebInterfacesHelper))]
	sealed class LinerAndAgencyBillOfLadingWebInterfacesHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new LinerAndAgencyBillOfLadingWebInterfacesHelper(Factory.New<BillOfLading>());
		}
	}
}
