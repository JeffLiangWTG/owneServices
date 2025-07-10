using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.NCTS.Business.Messaging;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class TRNctsMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new TRNctsMessageSender(null));
		}
	}
}
