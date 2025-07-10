using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOEDIMessagePrettier))]
sealed class NOEDIMessagePrettierTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When message is null", () => new NOEDIMessagePrettier(null));
		AssertNoExceptionThrown("When message is not null", () => new NOEDIMessagePrettier(Factory.New<NOEDIMessage>()));
	});

	public void TestMakeHumanReadable()
	{
		var message = Factory.New<NOEDIMessage>();
		message.EM_MessageText = "MESSAGE_TEXT";
		var prettier = new NOEDIMessagePrettier(message);
		AssertEquals("MESSAGE_TEXT", prettier.MakeHumanReadable());
	}
}
