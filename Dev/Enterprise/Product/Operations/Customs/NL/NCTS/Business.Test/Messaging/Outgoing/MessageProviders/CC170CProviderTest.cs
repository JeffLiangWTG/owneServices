using System;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC170CProvider))]
sealed class CC170CProviderTest : DepartureHeaderProviderAbstractTest<CC170CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC170CProvider(null));

	protected override bool HasSendingActionParameter => true;

	protected override string MessageType => NLConstants.WCoTypeCodes.PresentationNotification;
}
