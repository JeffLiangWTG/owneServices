using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(N5205MessageSendingObject))]
	sealed class N5205MessageSendingObjectTest : MessageSendingObjectTest<N5205MessageSendingObject>
	{
		public void TestMessageSendingObject()
		{
			(var sendingObj, _, _) = SetupData();
			CombineAssertions(() =>
			{
				AssertEquals("MessageType", MessageTypeCodeList.Descriptions.EBC, sendingObj.MessageType);
				AssertEquals("Description", "出口快遞貨物簡易申報單", sendingObj.Description);
			});
		}

		public void TestExporter()
		{
			(var sendingObj, _, _) = SetupData();
			AssertType<N5205DeclarationExporter>(((IN5205Declaration)sendingObj).Exporter);
		}

		public void TestAgent()
		{
			(var sendingObj, var header, _) = SetupData();
			header.AMA_RecipientReference = "F";
			header.AMA_CustomsProfile = "AF3";
			var agent = ((IN5205Declaration)sendingObj).Agent;
			CombineAssertions(() =>
			{
				AssertEquals("ID", "F", agent.ID);
				AssertEquals("RoleCode", "CB", agent.RoleCode);
				AssertEquals("SubBoxID", "3", agent.SubBoxID);
			});
		}

		public override void TestSerializeToMessageString()
		{
			(var sendingObj, _, _) = SetupData();
			var expected = new N5205MessageBuilder().PopulateXml(sendingObj, ZString.Empty);
			AssertEquals(expected, sendingObj.SerializeToMessageString());
		}

		protected override N5205MessageSendingObject GetMessageSendingObject(AsycudaManifestHeader header)
		{
			return new N5205MessageSendingObject(header);
		}
	}
}
