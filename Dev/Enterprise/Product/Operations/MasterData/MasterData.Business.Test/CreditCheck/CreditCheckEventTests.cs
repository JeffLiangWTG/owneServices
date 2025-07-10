#if DEBUG

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class CreditCheckEventTests : TestCaseWithFactory
	{
		public void TestAddCreditCheckEvent()
		{
			var debo = Factory.New<DummyEnterpriseBusinessObject>();
			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.CreditCheckEvent.Code);

			int foundCCEEvents;
			foundCCEEvents = debo.GetLogs().Find(filter).Length;
			AssertEquals("Precondition", 0, foundCCEEvents);

			var logWithChanges = debo.GetLogs().AddNew(AutoEvents.CreditCheckEvent);
			foundCCEEvents = debo.GetLogs().Find(filter).Length;
			AssertEquals("CCE event added to log", 1, foundCCEEvents);
		}

		public void TestCreditCheckMonitorEventDetails()
		{
			var logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "SEC"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Score Change", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "CNC"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Collection Change", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "CTA"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Court Action", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "PCF"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Public Filing", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "DRA"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Director Alert", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "SRC"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Shareholder Change", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "FLC"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Financial Change", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "MON",
				["RES"] = "SSA"
			};
			GenericTestCreditCheckEventDetails("Event Type: Monitor | Reason: Status Alert", logParameters);
		}

		public void TestCreditCheckBuyEventDetails()
		{
			var logParameters = new Dictionary<string, string>
			{
				["TYP"] = "BCR",
				["RES"] = "LPR"
			};
			GenericTestCreditCheckEventDetails("Event Type: Bought Report | Reason: Late Payment Risk", logParameters);
			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "BCR",
				["RES"] = "LPR",
				["DOC"] = "Test Doc"
			};
			GenericTestCreditCheckEventDetails("Event Type: Bought Report | Reason: Late Payment Risk", logParameters);
		}

		public void TestCreditCheckRenewEventDetails()
		{
			var logParameters = new Dictionary<string, string>
			{
				["TYP"] = "RCR",
				["RES"] = "CRE"
			};
			GenericTestCreditCheckEventDetails("Event Type: Renewed Report | Reason: Comprehensive Report", logParameters);

			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "RCR",
				["RES"] = "LPR"
			};
			GenericTestCreditCheckEventDetails("Event Type: Renewed Report | Reason: Late Payment Risk", logParameters);

			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "RCR",
				["RES"] = "FRR"
			};
			GenericTestCreditCheckEventDetails("Event Type: Renewed Report | Reason: Failure Risk Report", logParameters);

			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "RCR",
				["RES"] = "CBE"
			};
			GenericTestCreditCheckEventDetails("Event Type: Renewed Report | Reason: Commercial Bureau Enquiry", logParameters);
		}

		void GenericTestCreditCheckEventDetails(string expectedEventDetails, Dictionary<string, string> logParameters)
		{
			var debo = Factory.New<DummyEnterpriseBusinessObject>();
			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.CreditCheckEvent.Code);
			var logWithChanges = debo.GetLogs().AddNew(AutoEvents.CreditCheckEvent, string.Empty, logParameters.ToArray());
			var log = debo.GetLogs().Find(filter).Single();
			AssertNotNull("Should have a StmALog", log);
			AssertEquals("Reference Display Event Details", expectedEventDetails, log.DisplayEventReference);
		}
	}
}
#endif
