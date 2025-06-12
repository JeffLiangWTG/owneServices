using System;
using CargoWise.eServices.USCustoms.MQConfiguration;
using IBM.WMQ;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.USCustoms.IntegrationTests.MQ
{
	[TestFixture]
	public class MQMessageExtensionsTest
	{
		[Test]
		public void TestPropertiesToString()
		{
			var mQMessage = new MQMessage();
			var expected = "MessageType:MQMT_DATAGRAM;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:0;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:0;DataOffset:0;Encoding:546;Expiry:-1;Feedback:0;Format:        ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:0;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:2;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;PutDateTime:0001-01-01 00:00:00,000;ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:1;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;";
			var actual = mQMessage.PropertiesToString();
			Assert.AreEqual(expected, actual);

			expected = "MessageType:MQMT_DATAGRAM;Encoding:546;Expiry:-1;MessageSequenceNumber:1;OriginalLength:-1;Persistence:2;Priority:-1;PutDateTime:0001-01-01 00:00:00,000;Version:1;ReasonName:MQRC_OK;";
			actual = mQMessage.PropertiesToString(true);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestPropertiesToString_GetPropertyValueFail_ReturnNAWthExceptionDetail()
		{
			var mQMessage = new MQMessage();
			var mockMQMessagePropertyReader = new Mock<MQMessagePropertyReader>();
			mockMQMessagePropertyReader.Setup(_ => _.GetPutDateTime(mQMessage)).Throws(new FormatException("String was not recognized as a valid DateTime."));

			var expectedStart = "MessageType:MQMT_DATAGRAM;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:0;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:0;DataOffset:0;Encoding:546;Expiry:-1;Feedback:0;Format:        ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:0;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:2;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;";
			var expectedNAWithExceptionDetail = "PutDateTime:N/A{System.FormatException: String was not recognized as a valid DateTime.";
			var expectedEnd = "ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:1;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;";
			var actual = mockMQMessagePropertyReader.Object.PropertiesToString(mQMessage);

			Assert.IsTrue(actual.StartsWith(expectedStart));
			Assert.IsTrue(actual.Contains(expectedNAWithExceptionDetail));
			Assert.IsTrue(actual.EndsWith(expectedEnd));
		}
	}
}
