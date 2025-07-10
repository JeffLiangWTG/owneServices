using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using NUnit.Framework;
using Constants = CargoWise.RefDbRepo.ILReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Services
{
	[TestFixture]
	sealed class XtMessageInfoTest
	{
		[Test]
		public void TestConstructor()
		{
			Assert.Catch<ArgumentNullException>(() => new XtMessageInfo(null, null), "ArgumentNullException raise when requestXml is null ");
			Assert.Catch<ArgumentException>(() => new XtMessageInfo(string.Empty, null), "ArgumentNullException raise when requestXml is Empty ");

			Assert.Catch<ArgumentNullException>(() => new XtMessageInfo("<Root>1</Root>", null), "ArgumentNullException raise when messageSubType is null ");
			Assert.Catch<ArgumentException>(() => new XtMessageInfo("<Root>1</Root>", ""), "ArgumentNullException raise when messageSubType is Empty ");
		}

		[Test]
		public void TestApplicationCode_ShouldReturnILC()
		{
			Assert.AreEqual("ILC", messageInfo.ApplicationCode);
		}

		[Test]
		public void TestMessageType_ShouldReturnREF()
		{
			Assert.AreEqual("GEN", messageInfo.MessageType);
		}

		[Test]
		public void TestGetMessageData_ShouldReturnBinaryReader()
		{
			const string request = "<Root>1</Root>";
			var messageInfo = new XtMessageInfo(request, Constants.MessageSubType.CustomCodes) as IXtMessageInfo;
			var expectedBytes = Encoding.UTF8.GetBytes(request);

			var binaryReader = messageInfo.GetMessageData();

			using (var memoryStream = new MemoryStream(expectedBytes))
			{
				using (var expectedBinaryReader = new BinaryReader(memoryStream))
				{
					var actualBytes = binaryReader.ReadBytes(expectedBytes.Length);
					var expectedBytesFromReader = expectedBinaryReader.ReadBytes(expectedBytes.Length);
					Assert.AreEqual(expectedBytesFromReader, actualBytes);
				}
			}
		}

		[Test]
		public void TestDestinationParty()
		{
			Assert.AreEqual("ILCUSTOMS", messageInfo.DestinationParty);
		}

		[Test]
		public void TestSourceParty()
		{
			Assert.AreEqual("ILREFDATA", messageInfo.SourceParty);
		}

		[Test]
		public void TestMessageTrackingID()
		{
			Assert.IsNotEmpty(null, messageInfo.MessageTrackingID);
		}

		[Test]
		public void TestXTMessageAttributes()
		{
			var expected = new Dictionary<string, string>()
			{
				{"custom.MessageType","GEN"},
				{"custom.MessageSubType","901"},
			};
			Assert.AreEqual(expected, messageInfo.XTMessageAttributes);
		}

		[SetUp]
		public void SetUp()
		{
			messageInfo = new XtMessageInfo("<Root>1</Root>",  Constants.MessageSubType.CustomCodes);
		}

		IXtMessageInfo messageInfo;
	}
}
