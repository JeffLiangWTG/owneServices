using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMenuItemMessageSendingObjectCollection))]
	sealed class InBondMenuItemMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InBondMenuItemMessageSendingObjectCollection>
	{
		public void TestPopulate()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var moveHeader1 = header1.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "1";
			var moveHeader2 = header1.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "2";
			var header2 = Factory.New<CusInBondHeader>();
			var moveHeader3 = header2.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "3";
			var collection = new InBondMenuItemMessageSendingObjectCollection(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
			collection.Populate(new List<CusInBondMoveHeader>()
			{ moveHeader1, moveHeader2, moveHeader3 });
			var inBondMenuItemMessageSendingObjects = collection.Cast<InBondMenuItemMessageSendingObject>().Select(x => x.InBondNumber);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1", "2", "3" }, inBondMenuItemMessageSendingObjects);
		}

		public void TestSetDefaultsForNewChild()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "1";
			movementHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var movementHeader2 = inBondHeader.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "2";
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber == "1");
			AssertEquals(InbondCommonTypeList.Codes._2TransportandExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber == "2");
			AssertEquals(InbondCommonTypeList.Codes._3ImmediateExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			AssertEquals(InbondCommonTypeList.Codes._3ImmediateExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "1";
			AssertEquals(InbondCommonTypeList.Codes._2TransportandExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "2";
			AssertEquals(InbondCommonTypeList.Codes._3ImmediateExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber == "1");
			AssertEquals("Should not set default for Arrival", InbondCommonTypeList.Codes._2TransportandExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber == "2");
			AssertEquals("Should not set default for Arrival", ZString.Empty, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			AssertEquals("Should not set default for Arrival", ZString.Empty, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "1";
			AssertEquals("Should not set default for Arrival", InbondCommonTypeList.Codes._2TransportandExport, inBondMenuItemMessageSendingObject.EntryType);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "2";
			AssertEquals("Should not set default for Arrival", ZString.Empty, inBondMenuItemMessageSendingObject.EntryType);
		}

		protected override InBondMenuItemMessageSendingObjectCollection GetCollectionToTest()
		{
			return new InBondMenuItemMessageSendingObjectCollection(new InBondMenuItemMessageData(new BusinessObjectFactory(), new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InBondMenuItemMessageSendingObject(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
		}
	}
}
