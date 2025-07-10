using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class MessageBuilderTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor()
		{
			var mock = new Mock<MessageBuilder<ABIInputBlockControlGenerator>>((IMessageAttachee)null);
			_ = mock.Object;
		}
	}
}
