using System;
using System.IO;
using CargoWise.eHub.Common;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class WrapMessageComponentTest : BaseComponentTest
	{
		private delegate void InsertToInboxDelegate(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message);

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWrapMessageComponent()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write("<bla>Test Message</bla>");
				writer.Flush();
				stream.Position = 0;
				message.BodyPart.Data = stream;

				var component = MockRepository.PartialMock<WrapMessageComponent>();
				MockRepository.ReplayAll();

				Assert.AreEqual(message, component.Execute(pipelineContext, message));
				component.Enabled = true;

				using (var outputStream = component.Execute(pipelineContext, message).BodyPart.GetOriginalDataStream())
				{
					string outputMessage = new StreamReader(outputStream).ReadToEnd();
					Assert.AreEqual("<Send xmlns=\"http://cargowise.com/ehub/product/2013/04\"><senderId>SENDERID</senderId><recepientId>RECIPIENTID</recepientId><message><![CDATA[<bla>Test Message</bla>]]></message></Send>", outputMessage);
				}
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_WrapMessageComponent()
		{
			var component = new WrapMessageComponent();
			Assert.AreEqual("WrapMessageComponent", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("97CF340A-0872-47A9-995D-36C56DCF85FB"), guid);
		}
	}
}
