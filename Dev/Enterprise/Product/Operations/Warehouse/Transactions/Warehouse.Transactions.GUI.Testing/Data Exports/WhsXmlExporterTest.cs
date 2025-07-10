using System.IO;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public abstract class WhsXmlExporterTest<TDocket, TValueObject> : WhsTestCaseWithFactory
	where TDocket : WhsDocket
	where TValueObject : IValueObject
{
	[TestDate(2006, 10, 5, 12, 0, 0, 0)]
	public void TestExport()
	{
		var buffer = new NotificationBuffer();
		var logsFilter = new ZQuery(StmALogSchema.SL_Parent, Docket.PK);
		var logTypeFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
		logTypeFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
		logsFilter.AddToFilter(logTypeFilter);
		AssertEquals(0, Factory.GetDatabaseCount(typeof(StmALog), logsFilter));

		var exporter = GetNewExporter();
		var writer = new StringWriter();
		exporter.Export(Docket, writer, buffer);

		AssertEquals("Notify should not have errors", true, !buffer.HasErrors);
		AssertEquals("1 logs for this docket", 1, Factory.GetDatabaseCount(typeof(StmALog), logsFilter));
		AssertEquals("File should exist", false, string.IsNullOrEmpty(writer.GetStringBuilder().ToString()));
	}

	#region Implementation

	protected abstract WhsXmlExporter<TDocket, TValueObject> GetNewExporter();

	protected OrgHeader OrgProxy
	{
		get
		{
			if (orgProxy == null)
			{
				orgProxy = Factory.New<OrgHeader>();
				orgProxy.OH_RL_NKClosestPort = "AUBNE";
				orgProxy.OH_Code = "TESORG";
				orgProxy.OH_FullName = "TEST ORGPROXY";
				orgProxy.MainAddress.OA_Address1 = "10 HUTCHESON STREET";
			}
			return orgProxy;
		}
	}

	protected OrgHeader Client => client ?? (client = GetNewClient());

	protected WhsWarehouse Warehouse => warehouse ?? (warehouse = GetNewWarehouse());

	protected TDocket Docket => docket ?? (docket = GetNewDocket());

	protected virtual OrgHeader GetNewClient() => Helper.CreateClient("CLT", "Test Client");

	protected virtual WhsWarehouse GetNewWarehouse() => Helper.CreateWarehouse("TST WHS", "ROW", 2, 2);

	protected abstract TDocket GetNewDocket();

	OrgHeader client;
	WhsWarehouse warehouse;
	TDocket docket;

	protected override void SetUp()
	{
		base.SetUp();
		currentOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
		GlbCompany.CurrentCompany.GC_OH_OrgProxy = OrgProxy.PK;
	}

	OrgHeader currentOrgProxy;
	OrgHeader orgProxy;

	protected override void TearDown()
	{
		base.TearDown();
		GlbCompany.CurrentCompany.GC_OH_OrgProxy = currentOrgProxy.PK;
	}

	#endregion
}
