using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	class CusEntryHeaderMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CusEntryHeaderMessageSender(null));
		}
	}
}
