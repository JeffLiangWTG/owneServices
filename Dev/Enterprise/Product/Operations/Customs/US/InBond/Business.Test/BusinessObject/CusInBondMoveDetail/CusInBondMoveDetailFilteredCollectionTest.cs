using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailFilteredCollection))]
	sealed class CusInBondMoveDetailFilteredCollectionTest : BusinessObjectCollectionViewTestCase<CusInBondMoveDetailFilteredCollection>
	{
		public void TestSetDefaultsForNewElement()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			AssertEquals(0, moveDetail1.Containers.Count);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			AssertEquals(1, moveDetail2.Containers.Count);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveDetail3 = moveHeader.MovementDetails.AddNew();
			AssertEquals(0, moveDetail3.Containers.Count);
		}

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
			var message = Factory.New<MQEDIMessage>();
			header.MovementHeader.Messages.Add(message);
			AssertEquals(true, list.AllowNew);
		}

		public void TestIsThisPartOfTheCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveDetail2 = moveHeader2.MovementDetails.AddNew();
			var header2 = Factory.New<CusInBondHeader>();
			var moveHeader1_2 = header2.MovementHeaders.AddNew();
			var moveDetail1_2 = moveHeader1_2.MovementDetails.AddNew();
			var moveDetail2_2 = moveHeader1_2.MovementDetails.AddNew();
			AssertEquals(2, header.FilteredMovementDetails.Count);
			AssertEquals(0, header.SelectedMovementDetails.Count);
			header.SelectedMovementHeader = moveHeader1.PK;
			AssertEquals("should have been filtered", 1, header.FilteredMovementDetails.Count);
			AssertEquals("only 1 should be in the collection", moveDetail1, header.FilteredMovementDetails[0]);
			header.SelectedMovementHeader = moveHeader2.PK;
			AssertEquals("should have been filtered", 1, header.FilteredMovementDetails.Count);
			AssertEquals("only 1 should be in the collection", moveDetail2, header.FilteredMovementDetails[0]);
			header.SelectedMovementDetail = moveDetail1.PK;
			AssertEquals(1, header.SelectedMovementDetails.Count);
			AssertEquals(moveDetail1, header.SelectedMovementDetails[0]);
		}

		public override void TestAddNew()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = new CusInBondMoveDetailFilteredCollection(header);
			var moveDetail = collection.AddNew();
			AssertEquals(header.PK, moveDetail.InBondHeaderPK);
			AssertEquals(ZGuid.Empty, moveDetail.B9_BM);
			var moveHeader = header.MovementHeaders.AddNew();
			moveDetail = collection.AddNew();
			AssertEquals(header.PK, moveDetail.InBondHeaderPK);
			AssertEquals(moveHeader.PK, moveDetail.B9_BM);
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveDetail = collection.AddNew();
			AssertEquals(header.PK, moveDetail.InBondHeaderPK);
			AssertEquals(ZGuid.Empty, moveDetail.B9_BM);
			header.SelectedMovementHeader = moveHeader.PK;
			moveDetail = collection.AddNew();
			AssertEquals(header.PK, moveDetail.InBondHeaderPK);
			AssertEquals(moveHeader.PK, moveDetail.B9_BM);
		}

		protected override CusInBondMoveDetailFilteredCollection GetCollectionToTest() => new CusInBondMoveDetailFilteredCollection(Factory.New<CusInBondHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			return moveHeader.MovementDetails.AddNew();
		}
	}
}
