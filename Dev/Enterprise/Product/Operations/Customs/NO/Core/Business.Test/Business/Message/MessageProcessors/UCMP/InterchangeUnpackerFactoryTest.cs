using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(InterchangeUnpackerFactory))]
sealed class InterchangeUnpackerFactoryTest : TestCaseWithFactory
{
	public void TestGetInterchangeUnpacker() => CombineAssertions(() =>
	{
		AssertType<XLGInterchangeUnpacker>("When InterchangeType is XLG", InterchangeUnpackerFactory.GetInterchangeUnpacker(Constant.MessageTypes.XLG));
		AssertType("When InterchangeType is not XLG", null, InterchangeUnpackerFactory.GetInterchangeUnpacker(Constant.MessageTypes.XER));
	});
}
