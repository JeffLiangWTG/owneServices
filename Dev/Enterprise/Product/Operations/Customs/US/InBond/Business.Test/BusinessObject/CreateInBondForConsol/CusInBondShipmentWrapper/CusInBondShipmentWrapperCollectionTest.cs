using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondShipmentWrapperCollection))]
	sealed class CusInBondShipmentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusInBondShipmentWrapperCollection>
	{
		public void TestMoveBOToTargetCollection()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var collection1 = new CusInBondShipmentWrapperCollection(consol);
			AssertEquals(collection1.Count, 2);
			var collection2 = new CusInBondShipmentWrapperCollection(null);
			AssertEquals(collection2.Count, 0);
			var bo1 = (CusInBondShipmentWrapper)collection1.FirstOrDefault();
			collection1.MoveBOToTargetCollection(collection2, bo1);
			AssertEquals(collection1.Count, 1);
			AssertEquals(collection2.Count, 1);
			Assert(!collection1.Contains(bo1));
			Assert(collection2.Contains(bo1));

			collection2.MoveBOToTargetCollection(collection1, bo1);
			AssertEquals(collection1.Count, 2);
			AssertEquals(collection2.Count, 0);
			Assert(collection1.Contains(bo1));
			Assert(!collection2.Contains(bo1));
		}

		public void TestCopyConsigneeFromShipmentWhenHasRealAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TSTCONSIGNEE";

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.ConsigneePK = org.PK;
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			shipment3.ConsigneePK = org.PK;
			shipment3.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			var collection = new CusInBondShipmentWrapperCollection(consol);

			AssertEquals(collection.Count, 3);
			Assert(shipment1.ConsigneeDocumentaryAddress.HasRealAddress);
			AssertEquals(collection[0].ConsigneeOrgPK, org.PK);
			AssertEquals(collection[0].ConsigneeOrganizationAddress, shipment1.ConsigneeDocumentaryAddress.E2_OA_Address);

			Assert(!shipment2.ConsigneeDocumentaryAddress.HasRealAddress);
			AssertEquals(collection[1].ConsigneeOrgPK, ZGuid.Empty);
			AssertEquals(collection[1].ConsigneeOrganizationAddress, ZGuid.Empty);

			Assert(!shipment3.ConsigneeDocumentaryAddress.HasRealAddress);
			AssertEquals(collection[2].ConsigneeOrgPK, ZGuid.Empty);
			AssertEquals(collection[2].ConsigneeOrganizationAddress, ZGuid.Empty);
		}

		protected override CusInBondShipmentWrapperCollection GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new CusInBondShipmentWrapperCollection(consol);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new CusInBondShipmentWrapper(shipment);
		}
	}
}
