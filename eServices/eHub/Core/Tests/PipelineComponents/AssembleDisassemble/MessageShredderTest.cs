using System;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class MessageShredderTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageShredder()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.ConsolsInternalWithTwoConsolsAndTwoContainersEach.xml");

			var pipelineContext = new PipelineContext();
			var component = new MessageShredder();
			Assert.AreEqual(null, component.Execute(pipelineContext, message));
		}

	}
}
