using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(AMSBillEDIMessageCollection))]
	class AMSBillEDIMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestGetMessagesByBillPK()
		{
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			var moveHeader = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;

			var moveHeader1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader1.BM_BH = header.PK;
			var bill = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			var message1 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			message1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message1.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message1.EM_LinkUniqueID = moveHeader.PK;
			message1.EM_ApplicationReference = bill.PK.ToString();

			var message2 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			message2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message2.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message2.EM_LinkUniqueID = moveHeader.PK;

			var message3 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message3.EM_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			message3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message3.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message3.EM_LinkUniqueID = moveHeader1.PK;
			message3.EM_ApplicationReference = bill.PK.ToString();
			Factory.Save();
			var collection = new AMSBillEDIMessageCollection(Factory, bill.PK.ToString(), new ZGuid[] { moveHeader.PK, moveHeader1.PK });
			collection.Load();
			var messages = collection.ToArray<Enterprise.Messaging.Business.EDIMessage>();
			AssertEquals("collection should have 2 message", 2, messages.Length);
			var linkUniqueIDs = messages.Select(x => x.EM_LinkUniqueID);
			AssertCollectionContains(message1.EM_LinkUniqueID, linkUniqueIDs);
			AssertCollectionContains(message3.EM_LinkUniqueID, linkUniqueIDs);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			var moveHeader = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var bill = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			bill.B0_BH = header.PK;
			return new AMSBillEDIMessageCollection(Factory, bill.PK.ToString(), moveHeader.PK);
		}
	}
}
