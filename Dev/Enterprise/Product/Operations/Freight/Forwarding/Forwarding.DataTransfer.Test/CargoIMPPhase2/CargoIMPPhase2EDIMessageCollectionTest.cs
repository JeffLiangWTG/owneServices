using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(CargoIMPPhase2EDIMessageCollection))]
	public class CargoIMPPhase2EDIMessageCollectionTest : ActiveBusinessObjectCollectionTestCase<CargoIMPPhase2EDIMessageCollection>
	{
		public void TestReadonly()
		{
			Messages.AddNew();
			Messages.AddNew();

			foreach (EDIMessage message in Messages)
			{
				AssertEquals(ZBool.True, message.ReadOnly);
			}
		}

		public void TestCargoIMPPhase2EDIMessageFKCorrectlySet()
		{
			Messages.AddNew();
			AssertEquals(Shipment.PK, Messages[0].EM_LinkUniqueID);
			AssertEquals(ForwardingShipment.Schema.TableName, Messages[0].EM_LinkTable);
			AssertEquals(ApplicationCodeList.Codes.CargoIMPPhase2, Messages[0].EM_ApplicationCode);
		}

		#region Implementation

		#region Shipment

		ForwardingShipment Shipment
		{
			get
			{
				if (this.shipment == null)
				{
					this.shipment = Factory.New<ForwardingShipment>();
				}
				return this.shipment;
			}
		}
		ForwardingShipment shipment;

		#endregion

		#region Messages

		CargoIMPPhase2EDIMessageCollection Messages
		{
			get
			{
				if (this.messages == null)
				{
					this.messages = new CargoIMPPhase2EDIMessageCollection(Shipment);
				}
				return this.messages;
			}
		}

		CargoIMPPhase2EDIMessageCollection messages;

		#endregion

		protected override CargoIMPPhase2EDIMessageCollection GetCollectionToTest()
		{
			return Messages;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CargoIMPPhase2EDIMessage>();
		}

		#endregion
	}
}
