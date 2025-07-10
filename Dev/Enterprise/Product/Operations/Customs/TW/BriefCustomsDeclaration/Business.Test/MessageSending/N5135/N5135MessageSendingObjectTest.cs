using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(N5135MessageSendingObject))]
	sealed class N5135MessageSendingObjectTest : MessageSendingObjectTest<N5135MessageSendingObject>
	{
		public void TestMessageSendingObject()
		{
			(var sendingObj, var header, _) = SetupData();
			CombineAssertions(() =>
			{
				AssertEquals("MessageType", MessageTypeCodeList.Descriptions.IBC, sendingObj.MessageType);
				AssertEquals("Description", "進口快遞貨物簡易申報單", sendingObj.Description);
			});
		}

		public void TestDutyTaxFee()
		{
			(var sendingObj, var header, _) = SetupData();
			header.AMA_PaymentMethod = "C";
			header.AMA_PaymentAccountNumber = "YY";
			CombineAssertions(() =>
			{
				var dutyTaxFee = ((IN5135Declaration)sendingObj).DutyTaxFee;
				AssertEquals("DutyTaxFee.DutyMethodCode", "C", dutyTaxFee.DutyMethodCode);
				AssertEquals("DutyTaxFee.PaymentObligationGuaranteeReferenceID", "YY", dutyTaxFee.PaymentObligationGuaranteeReferenceID);
			});
		}

		public void TestAgent()
		{
			(var sendingObj, var header, _) = SetupData();
			header.AMA_RecipientReference = "F";
			header.AMA_CustomsProfile = "AF3";
			var agent = ((IN5135Declaration)sendingObj).Agent;
			CombineAssertions(() =>
			{
				AssertEquals("ID", "F", agent.ID);
				AssertEquals("RoleCode", "CB", agent.RoleCode);
				AssertEquals("SubBoxID", "3", agent.SubBoxID);
			});
		}

		public void TestGoodsShipments()
		{
			(var sendingObj, var header, _) = SetupData();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_GoodsValue = 1m;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_GoodsValue = 2m;
			var bill3 = header.Bills.AddNew();
			bill3.ABL_GoodsValue = 3m;
			bill3.ABL_BolType = "BOL";
			bill1.ABL_SequenceNumber = 3;
			var goodsShipments = ((IN5135Declaration)sendingObj).GoodsShipments.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("count", 2, goodsShipments.Count);
				AssertEquals("should create from bill2", 2m, goodsShipments[0].InvoiceAmount);
				AssertEquals("should create from bill1", 1m, goodsShipments[1].InvoiceAmount);
			});
		}

		public override void TestSerializeToMessageString()
		{
			(var sendingObj, _, _) = SetupData();
			var expected = new N5135MessageBuilder().PopulateXml(sendingObj, ZString.Empty);
			AssertEquals(expected, sendingObj.SerializeToMessageString());
		}

		protected override N5135MessageSendingObject GetMessageSendingObject(AsycudaManifestHeader header)
		{
			return new N5135MessageSendingObject(header);
		}
	}
}
