using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public abstract class WhsXmlExportDirectorTest<TDocket, TValueObject> : WhsTestCaseWithFactory
	where TDocket : WhsDocket
	where TValueObject : IValueObject
{
	#region TestMessageShownWhenSourceHasChanges()

	public void TestMessageShownWhenSourceHasChanges()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		Docket.WD_TransportReference = "Test";
		Assert("Precondition", Docket.HasChanges);

		ExportDirector.RunExport(Docket);
		Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please save your changes before you continue."));
	}
	#endregion

	#region TestExportWhenConditionsAreNotMet()

	public void TestExportWhenConditionsAreNotMet()
	{
		var mockDirector = new Mock<WhsXmlExportDirector<TDocket, TValueObject>>(GetNewAdapter());
		mockDirector.Protected().Setup<ZString>("GetCheckExportConditionsAreMet", ItExpr.IsAny<TDocket>()).Returns(new ZString("error"));

		mockDirector
			.Protected()
			.Setup<bool>("ExportToXmlCore", ItExpr.IsAny<TDocket>(), ItExpr.IsAny<INotifications>());
		exportDirector = mockDirector.Object;
		AssertNotNull(exportDirector);
		Docket.Factory.Save();
		exportDirector.RunExport(Docket);

		mockDirector.Protected().Verify("GetCheckExportConditionsAreMet", Times.Once(), ItExpr.IsAny<TDocket>());
		mockDirector.Protected().Verify("ExportToXmlCore", Times.Never(), ItExpr.IsAny<TDocket>(), ItExpr.IsAny<INotifications>());
	}

	#endregion

	#region TestExportSuccessful()

	[TestDate(2006, 10, 5, 12, 0, 0, 0)]
	public void TestExportSuccessful()
	{
		if (GlbCompany.CurrentCompany.OrgProxy == null && GlbBranch.CurrentBranch.OrgProxy == null)
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
		}

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		Docket.Factory.Save();
		ExportDirector.RunExport(Docket);
		AssertEquals(GetExpectedSuccessNotification(), new ZString(UnitTestUserNotification.Instance.LastMessage.Text));
	}

	#endregion

	#region Implementation

	protected virtual ZString GetExpectedSuccessNotification() => GetExpectedDocketTypeDescription() + " successfully exported to XML.";

	protected virtual OrgHeader GetNewClient() => Helper.CreateClient("TST CLT", "Test Client");

	protected virtual WhsWarehouse GetNewWarehouse() => Helper.CreateWarehouse("TST WHS", "ROW", 2, 2);

	protected abstract TDocket GetNewDocket();

	protected abstract WhsXmlExportDirector<TDocket, TValueObject> GetNewExportDirectorObject();
	protected abstract WhsValueObjectDataAdapter<TDocket, TValueObject> GetNewAdapter();
	protected abstract ZString GetExpectedDocketTypeDescription();

	protected virtual WhsXmlExportDirector<TDocket, TValueObject> GetNewExportDirector() => GetNewExportDirectorObject();

	protected TDocket Docket => docket ?? (docket = GetNewDocket());

	protected WhsWarehouse Warehouse => warehouse ?? (warehouse = GetNewWarehouse());

	protected OrgHeader Client => client ?? (client = GetNewClient());

	protected WhsXmlExportDirector<TDocket, TValueObject> ExportDirector => exportDirector ?? (exportDirector = GetNewExportDirector());

	protected override void SetUp()
	{
		base.SetUp();
		Factory.Save();
	}

	TDocket docket;
	OrgHeader client;
	WhsWarehouse warehouse;
	WhsXmlExportDirector<TDocket, TValueObject> exportDirector;

	#endregion
}
