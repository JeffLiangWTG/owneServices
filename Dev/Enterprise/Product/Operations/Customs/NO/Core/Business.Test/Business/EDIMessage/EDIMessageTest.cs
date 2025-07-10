using CargoWise.Types;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.NO.Business;

abstract class EDIMessageTest<T> : Messaging.Testing.EDIMessageTest where T : NOEDIMessage
{
	protected virtual bool ExpectedIsMessageInterpretationSetterSupported => true;

	public void TestMessageInterpretation_Getter() => CombineAssertions(() =>
	{
		var thisType = typeof(T);
		AssertEquals($"[PRE-CONDITION] {thisType.FullName} IsPublic, so that type is visible to Moq", expected: true, thisType.IsPublic);
		AssertEquals($"[PRE-CONDITION] {thisType.FullName} IsSealed, so that Moq can inherit type", expected: false , thisType.IsSealed);
		var messageMock = Factory.NewMoq<T>();
		var prettier = Mock.Of<IEDIMessagePrettier>(x => x.MakeHumanReadable() == (ZString)"a very pretty string");
		messageMock.Protected().Setup<IEDIMessagePrettier>("GetNewPrettier").Returns(prettier);
		var message = messageMock.Object;
		message.EM_MessageInterpretation = "a pretty string";
		AssertEquals("When EM_MessageText is null, use base value", "a pretty string", message.EM_MessageInterpretation);
		message.EM_MessageText = "sample EDI message";
		AssertEquals("When EM_MessageText is not null, use pretty value", "a very pretty string", message.EM_MessageInterpretation);
	});

	public void TestIsMessageInterpretationSetterSupported()
	{
		var message = Factory.New<T>();
		AssertEquals(ExpectedIsMessageInterpretationSetterSupported, message.IsMessageInterpretationSetterSupported);
	}
}
