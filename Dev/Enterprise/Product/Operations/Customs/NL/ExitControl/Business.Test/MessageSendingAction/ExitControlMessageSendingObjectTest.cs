using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.ExitControl.Business.Testing;

[TestedType(typeof(ExitControlMessageSendingObject))]
sealed class ExitControlMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestExitReport()
	{
		var sendingObject = (ExitControlMessageSendingObject)GetNewBusinessObject();
		AssertSame(exitReport, sendingObject.MessagingObject);
	}

	public void TestEntryType()
	{
		_ = AssertEntity<ExitControlMessageSendingObject>()
			.HasProperty(x => x.EntryType)
			.WithList("Lookups.EntryTypeList")
			.WithCaption("Entry Type");
	}

	protected override BusinessObject GetNewBusinessObject() => new ExitControlMessageSendingObject(exitReport);

	protected override void SetUp()
	{
		base.SetUp();
		exitReport = Factory.New<CusExitReport>();
	}
	CusExitReport exitReport;
}
