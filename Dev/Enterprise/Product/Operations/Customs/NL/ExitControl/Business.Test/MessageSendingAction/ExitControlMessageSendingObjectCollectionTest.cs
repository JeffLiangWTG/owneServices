using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.ExitControl.Business.Testing;

[TestedType(typeof(ExitControlMessageSendingObjectCollection))]
sealed class ExitControlMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitControlMessageSendingObjectCollection>
{
	protected override Type GetExpectedCollectionType() => typeof(ExitControlMessageSendingObjectCollection);

	protected override ExitControlMessageSendingObjectCollection GetCollectionToTest()
	{
		return new ExitControlMessageSendingObjectCollection(exitHeader.CusExitReports, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var report = exitHeader.CusExitReports.AddNew();
		return new ExitControlMessageSendingObject(report);
	}

	protected override void SetUp()
	{
		base.SetUp();
		exitHeader = Factory.New<CusExitHeader>();
		var report = exitHeader.CusExitReports.AddNew();
		report.CER_TransportID = "Transport";
	}
	CusExitHeader exitHeader;
}
