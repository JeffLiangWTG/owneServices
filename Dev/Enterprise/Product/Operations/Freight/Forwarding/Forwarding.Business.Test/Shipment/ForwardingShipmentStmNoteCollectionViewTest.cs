using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentStmNoteCollectionView))]
	sealed class ForwardingShipmentStmNoteCollectionViewTest : BusinessObjectCollectionViewTestCase<ForwardingShipmentStmNoteCollectionView>
	{
		public void TestParent()
		{
			var view = new ForwardingShipmentStmNoteCollectionView(Shipment);
			AssertEquals(Shipment, view.Parent);
		}

		#region Implementation

		protected override ForwardingShipmentStmNoteCollectionView GetCollectionToTest()
		{
			return new ForwardingShipmentStmNoteCollectionView(Shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var note = Factory.New<ForwardingShipmentStmNote>();
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;

			return note;
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		#endregion
	}
}
