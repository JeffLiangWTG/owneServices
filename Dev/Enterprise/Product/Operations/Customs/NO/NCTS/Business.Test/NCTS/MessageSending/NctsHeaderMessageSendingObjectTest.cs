using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObject))]
sealed class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderMessageSendingObject(null));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new NctsHeaderMessageSendingObject(Factory.New<NctsHeader>());
	}
}
