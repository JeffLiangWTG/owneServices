using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class MimePartyParserTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestResolveParties()
		{
			var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.02_PgpMimeHtml_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

			var partyAccessor = MockRepository.StrictMock<IPartyAccessor>();
            Expect.Call(partyAccessor.GetClientIDFromEmail("Jay Li", "Jay.Li@wisetechglobal.com")).Return("AAABBBCCC");
            Expect.Call(partyAccessor.GetClientIDFromEmail("WTLDAUJLI", "rian.mclellan@gmail.com")).Return("WTLDAUJLI");

            var component = MockRepository.PartialMock<MimePartyParser>();
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();

			MockRepository.ReplayAll();

            Assert.AreEqual(null, message.Context.ReadPropertyString<BTS.SourceParty>());
            Assert.AreEqual(null, message.Context.ReadPropertyString<BTS.DestinationParty>());

            component.Enable = false;
            component.Execute(pipelineContext, message);
            Assert.AreEqual(null, message.Context.ReadPropertyString<BTS.SourceParty>());
            Assert.AreEqual(null, message.Context.ReadPropertyString<BTS.DestinationParty>());

            component.Enable = true;
			component.Execute(pipelineContext, message);
            Assert.AreEqual("AAABBBCCC", message.Context.ReadPropertyString<BTS.SourceParty>());
            Assert.AreEqual("WTLDAUJLI", message.Context.ReadPropertyString<BTS.DestinationParty>());

			MockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestResolveParties_Exception()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.Context = MessageFactory.CreateMessageContext();

            var component = MockRepository.PartialMock<MimePartyParser>();

            try
            {
                component.Enable = true;
                component.Execute(pipelineContext, message);
                Assert.Fail("Should throw an exception");
            }
            catch (ApplicationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(@"Value cannot be null.
Parameter name: stream
   at MimeKit.MimeMessage.Load(ParserOptions options, Stream stream, Boolean persistent, CancellationToken cancellationToken)
   at MimeKit.MimeMessage.Load(Stream stream, CancellationToken cancellationToken)
   at CargoWise.eHub.Core.PipelineComponents.MimePartyParser.Execute(IPipelineContext context, IBaseMessage message)"));
            }
        }
	}
}
