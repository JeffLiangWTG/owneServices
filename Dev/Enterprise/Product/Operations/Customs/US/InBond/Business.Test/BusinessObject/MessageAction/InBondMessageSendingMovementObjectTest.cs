using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMessageSendingMovementObject))]
	class InBondMessageSendingMovementObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetMovementHeadersForSending()
		{
			var sendingObject = (InBondMessageSendingMovementObject)GetNewBusinessObject();
			AssertEquals(1, sendingObject.GetMovementHeadersForSending().Count());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			moveHeader.InBondNumber = "123456";
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var sendingObject = new InBondMessageSendingMovementObject(movement, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return sendingObject;
		}
	}
}
