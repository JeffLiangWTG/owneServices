using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.ExitControl.Business.Testing;

[TestedType(typeof(ExitControlMessageSendingObjectParent))]
sealed class ExitControlMessageSendingObjectParentTest : EU.ExitControl.Business.Testing.ExitControlMessageSendingObjectParentTest
{
	public new void TestSendingObjectsCollectionType()
	{
		AssertType<ExitControlMessageSendingObjectCollection>(exitControlMessageSendingObjectParent.SendingObjectsCollection);
	}

	protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetNewExitControlMessageSendingObjectParent(CusExitHeader exitHeader)
		=> new ExitControlMessageSendingObjectParent(exitHeader);

	protected override CusExitHeader GetNewCusExitHeader() => Factory.New<CusExitHeader>();

	protected override BusinessObject GetNewBusinessObject() => exitControlMessageSendingObjectParent;

	protected override void SetUp()
	{
		base.SetUp();
		var cusExitHeader = Factory.New<CusExitHeader>();
		exitControlMessageSendingObjectParent = new ExitControlMessageSendingObjectParent(cusExitHeader);
	}
	ExitControlMessageSendingObjectParent exitControlMessageSendingObjectParent;
}
