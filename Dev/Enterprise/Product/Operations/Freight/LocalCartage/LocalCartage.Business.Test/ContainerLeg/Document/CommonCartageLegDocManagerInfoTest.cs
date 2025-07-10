using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageLegDocManagerInfo))]
	public class CommonCartageLegDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CommonCartageLeg>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var cartage = Factory.New<CartageForTest>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			return leg;
		}

		public void TestRelatedObjectsRetrieved()
		{
			var cfs = Factory.New<OrgHeader>();
			var cto = Factory.New<OrgHeader>();
			var cartage = Factory.New<CartageForTest>();
			cartage.FirstDocAddress.E2_OA_Address = cfs.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cto.MainAddress.PK;
			var move = cartage.ContainerBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			IList<BusinessObject> relatedObjects = ((IDocManagerSupport)leg).DocManagerInfo.RelatedObjects;
			AssertEquals("Should have the Cartage in the related business objects", true, relatedObjects.Contains(leg.Cartage));
		}
	}
}
