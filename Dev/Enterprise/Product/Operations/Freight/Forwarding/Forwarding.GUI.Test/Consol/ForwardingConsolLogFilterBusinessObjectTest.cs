using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.GUI.Consol;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ForwardingConsolLogFilterBusinessObject))]
	public class ForwardingConsolLogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ForwardingConsolLogFilterBusinessObject(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>());
		}

		public void TestEventCodesListDoesNotContainsOCB()
		{
			var filterBusinessObject = new TestForwardingConsolLogFilterBusinessObject(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>());
			var codeList = new List<string>();
			for (int i = 0; i < filterBusinessObject.EventCodesList.Count; i++)
			{
				codeList.Add(filterBusinessObject.EventCodesList[i].Code);
			}
			AssertCollectionNotContains("The codeList should not contains [OCB]", codeList, delegate(string code)
			{
				return code.Equals(AutoEvents.OceanCarrierBookingByTEU.Code);
			});
		}

		public void TestOCBHidden()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var ocbLog = dummy.Logs.AddNew();

			using (ocbLog.LockForUpdatingKeyFieldsForTesting())
			{
				ocbLog.SL_SE_NKEvent = AutoEvents.OceanCarrierBookingByTEU.Code;
			}

			var aasLog = dummy.Logs.AddNew();

			using (aasLog.LockForUpdatingKeyFieldsForTesting())
			{
				aasLog.SL_SE_NKEvent = AutoEvents.AccreditationAttemptCommenced.Code;
			}

			var filterBusinessObject = new TestForwardingConsolLogFilterBusinessObject(dummy);
			var query = filterBusinessObject.GetRelatedEventsQuery();
			var stmALogs = Factory.Load<StmALog>(query);

			AssertEquals("should have 1 record", stmALogs.Length, 1);
			AssertEquals("event code should be AAS", stmALogs[0].SL_SE_NKEvent, AutoEvents.AccreditationAttemptCommenced.Code);
		}

		#region Implementation

		class TestForwardingConsolLogFilterBusinessObject : ForwardingConsolLogFilterBusinessObject
		{
			public TestForwardingConsolLogFilterBusinessObject(IStmALogParent master) : base(master)
			{
			}
			public new CodeDescriptionPairList EventCodesList => base.EventCodesList;

			public ZQuery GetRelatedEventsQuery() => GetBusinessObjectsWithRelatedEventsQuery("All");
		}

		#endregion
	}
}
