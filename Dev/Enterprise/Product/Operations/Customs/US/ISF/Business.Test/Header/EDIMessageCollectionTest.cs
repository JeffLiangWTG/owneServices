using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestOnCountChanged()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsStatus = MessageStatusList.Codes.ErrorISFAdd;
			var message1 = header.Messages.AddNew(typeof(MQEDIMessage));
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			var sf90Block1 = new ISFSF90();
			sf90Block1.MessageTypeCode = ISFMessageStatus.Codes.Accepted;
			message1.MessageBlock.AddMessageBlock(sf90Block1);
			AssertEquals(true, header.BF_CustomsReferenceInfo.ReadOnly);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new EDIMessageCollection(Factory.New<CusISFHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EDIMessage>();
	}
}
