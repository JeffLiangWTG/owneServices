using System.IO;
using System.Text;
using CargoWise.eHub.Core.Orchestrations.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests
{
	[TestClass()]
	public class MessagesTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetMessageBodyString()
		{
			var mockMessage = MockRepository.GenerateMock<XLANGMessage>();
			var mockPart = MockRepository.GenerateMock<XLANGPart>();
			string messageText = "<message>text</message>";
			var bodyStream = new MemoryStream(Encoding.Default.GetBytes(messageText));
			mockMessage.Stub(x => x[0]).Return(mockPart);
			mockPart.Stub(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);

			string result = Messages.GetMessageBodyString(mockMessage);

			Assert.AreEqual(messageText, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetMessagePartBase64String()
		{
			var mockMessage = MockRepository.GenerateMock<XLANGMessage>();
			var mockPart = MockRepository.GenerateMock<XLANGPart>();
			string messageText = "<message>text</message>";
			int index = 1;
			var bodyStream = new MemoryStream(Encoding.Default.GetBytes(messageText));
			mockMessage.Stub(x => x[index]).Return(mockPart);
			mockPart.Stub(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);

			string result = Messages.GetMessagePartBase64String(mockMessage, index);

			Assert.AreEqual("PG1lc3NhZ2U+dGV4dDwvbWVzc2FnZT4=", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSetMessageContext()
		{
			Messages.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "TestSender");
			var result = Messages.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			Assert.AreEqual("TestSender", result);

			Messages.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "TestSender2");
			result = Messages.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			Assert.AreEqual("TestSender2", result);
		}
	}
}
