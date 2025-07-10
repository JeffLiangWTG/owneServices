using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AISRetrospectiveQuotaMessageSenderTest : AbstractMessageSenderTest
{
	public void TestConstructor()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		_ = AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: entryLines",
			() => new AISRetrospectiveQuotaMessageSender(factory, sendingObject, sendingObjectParent, null, ZString.Empty, ZString.Empty));
	}
}
