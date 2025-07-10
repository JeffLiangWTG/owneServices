using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class IWhsLogEventsExtentionsTest : WhsTestCaseWithFactory
	{
		#region TestAddEvents

		public void TestAddEvents()
		{
			var dummyBizO = Factory.New<DummyWhsLogBusinessObject>();
			dummyBizO.AddEvents(Events.ServiceCommenced);

			var logServiceCommenced = dummyBizO.Logs.Find(GetLogFilter(dummyBizO.PK, Events.ServiceCommenced.Code))[0];
			AssertEquals("Free Text Reference.", "Dummy", logServiceCommenced.ReferenceFreeText);
			AssertEquals("Event Reference Parameter Type", "DummyType", logServiceCommenced.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter Location", "DummyCity", logServiceCommenced.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location]);
		}

		public void TestAddEvents_WithReferenceAndParameterType()
		{
			var dummyBizO = Factory.New<DummyWhsLogBusinessObject>();

			dummyBizO.AddEvents(Events.ServiceCommenced, "MyReference", "MyType");
			var logServiceCommenced = dummyBizO.Logs.Find(GetLogFilter(dummyBizO.PK, Events.ServiceCommenced.Code))[0];
			AssertEquals("Free Text Reference.", "MyReference", logServiceCommenced.ReferenceFreeText);
			AssertEquals("Event Reference Parameter Type", "MyType", logServiceCommenced.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter Location", "DummyCity", logServiceCommenced.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location]);
		}

		ZQuery GetLogFilter(ZGuid pK, ZString code)
		{
			var logFilter = new ZQuery(StmALogSchema.SL_Parent, pK);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, code);
			return logFilter;
		}

		#endregion

		#region DummyWhsLogBusinessObject

		class DummyWhsLogBusinessObject : DummyBusinessObject, IWhsLogEventParent
		{
			public DummyWhsLogBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZString IWhsLogEventParent.EventFreeTextReference
			{
				get { return "Dummy"; }
			}

			string IWhsLogEventParent.EventReferenceParameterType
			{
				get { return "DummyType"; }
			}

			public WhsWarehouse Warehouse
			{
				get
				{
					if (warehouse == null)
					{
						var orgAddress = Factory.New<OrgAddress>();
						orgAddress.OA_City = "DummyCity";
						warehouse = Factory.New<WhsWarehouse>();
						warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
					}
					return warehouse;
				}
			}
			WhsWarehouse warehouse;

			public BusinessObject[] BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

			public BusinessObjectFactory LogsFactory
			{
				get { return base.Factory; }
			}

			public ZGuid LogsParentPK
			{
				get { return PK; }
			}

			string IStmALogParent.LogsParentTableName
			{
				get { return "DummyTable"; }
			}

			void IStmALogParent.ProcessLog(IStmALog log)
			{
			}

			bool IStmALogParent.DeferFiringWorkflow
			{
				get { return false; }
			}

			public Logs Logs
			{
				get { return logs ?? (logs = new Logs(this)); }
			}
			Logs logs;
		}

		#endregion
	}
}
