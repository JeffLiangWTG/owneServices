using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailsCollection))]
	sealed class CusInBondMoveDetailsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var collection = new CusInBondMoveDetailFilteredCollection(header);
			IBindingList list = collection;
			AssertEquals(true, list.AllowNew);
			header.BH_ParentID = shipment.PK;
			AssertEquals(false, list.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals(true, list.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals(false, list.AllowNew);
			var message = Factory.New<US.Business.MQEDIMessage>();
			header.MovementHeader.Messages.Add(message);
			AssertEquals(true, list.AllowNew);
		}

		public void TestRelationshipFilter()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveDetail2 = moveHeader2.MovementDetails.AddNew();
			var header2 = Factory.New<CusInBondHeader>();
			var moveHeader12 = header2.MovementHeaders.AddNew();
			var moveDetail12 = moveHeader12.MovementDetails.AddNew();
			var moveHeader22 = header2.MovementHeaders.AddNew();
			var moveHeader32 = header2.MovementHeaders.AddNew();
			var moveDetail32 = moveHeader32.MovementDetails.AddNew();
			AssertEquals(2, header.MovementDetails.Count);
			Assert(header.MovementDetails.Contains(moveDetail1));
			Assert(header.MovementDetails.Contains(moveDetail2));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			return header.MovementDetails;
		}
	}
}
