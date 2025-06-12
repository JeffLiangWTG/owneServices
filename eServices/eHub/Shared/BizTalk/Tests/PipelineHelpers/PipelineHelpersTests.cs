using System.IO;
using System.Text;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Shared.BizTalk.Tests.PipelineHelpers
{
    [TestClass]
    public class PipelineHelpersTests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void PipelineHelpers_CloneMessage()
        {
            var messageFactory = new MessageFactory();
            var pipelineContext = new PipelineContext();
            string messageContent = "<Test>TEST</Test>";

            var sourceMessage = messageFactory.CreateMessage();
            sourceMessage.Context = messageFactory.CreateMessageContext();
            sourceMessage.AddPart("xml", messageFactory.CreateMessagePart(), true);
            sourceMessage.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageContent));
            sourceMessage.Context.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Sender");
            sourceMessage.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Recipient");

            var target = new BizTalk.PipelineHelpers.PipelineHelpers();
            var outputMessage = target.CloneMessage(pipelineContext, sourceMessage);
            
            Assert.AreNotSame(sourceMessage, outputMessage);
            Assert.AreNotSame(sourceMessage.BodyPart.GetOriginalDataStream(), outputMessage.BodyPart.GetOriginalDataStream());
            Assert.AreEqual("Sender", outputMessage.Context.Read("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
            Assert.AreEqual("Recipient", outputMessage.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
            Assert.AreEqual(0, sourceMessage.BodyPart.GetOriginalDataStream().Position);
            Assert.AreEqual(0, outputMessage.BodyPart.GetOriginalDataStream().Position);
            using (var sr = new StreamReader(outputMessage.BodyPart.GetOriginalDataStream()))
                Assert.AreEqual(messageContent, sr.ReadToEnd());
        }
    }
}
