using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	public class SuppressHookHelperTest : TestCaseWithFactory
	{
		[TestDate(2023, 08, 22)]
		public void TestDummyUpdate()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Date";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

			var startDate = ZDateTime.UtcNow;
			dummy.Z0_Date = startDate;
			AssertMatch(new Regex($".*Z0_Date.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{startDate.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			factory.Save();

			TestDateAttribute.AddMinutes(5);
			var updateDate = ZDateTime.UtcNow;
			dummy.Z0_Date = updateDate;
			AssertMatch(new Regex($".*Z0_Date.*{startDate.ToString(null, CultureInfo.InvariantCulture)}.*{updateDate.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		[TestDate(2023, 08, 22)]
		public void TestDummyUpdateWithoutSuppress()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Date";
			Factory.Save();

			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook(false))
			{
				var factory = new BusinessObjectFactory();
				var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

				var startDate = ZDateTime.UtcNow;
				dummy.Z0_Date = startDate;
				AssertMatch(new Regex($".*Z0_Date.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{startDate.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
				factory.Save();

				TestDateAttribute.AddMinutes(5);
				var updateDate = ZDateTime.UtcNow;
				dummy.Z0_Date = updateDate;
				AssertMatch(new Regex($".*Z0_Date.*{startDate.ToString(null, CultureInfo.InvariantCulture)}.*{updateDate.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
			}
		}

		[TestDate(2023, 08, 22)]
		public void TestDummyUpdateWithSuppress()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Date";
			Factory.Save();

			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
			{
				var factory = new BusinessObjectFactory();
				var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

				var startDate = ZDateTime.UtcNow;
				dummy.Z0_Date = startDate;
				AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			}
		}

		[TestDate(2023, 08, 22)]
		public void TestShipmentUpdate()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z97";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "JS_RL_NKDestination";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertMatch(new Regex($".*AUSYD.*"), shipment.Logs.GetAllLogs()[0].Parameters["CHG"]);
			factory.Save();

			shipment.JS_RL_NKDestination = "USLAX";
			AssertMatch(new Regex($".*USLAX.*"), shipment.Logs.GetAllLogs()[3].Parameters["CHG"]);
		}

		[TestDate(2023, 08, 22)]
		public void TestShipmentUpdateWithoutSupress()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z97";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "JS_RL_NKDestination";
			Factory.Save();

			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook(false))
			{
				var factory = new BusinessObjectFactory();
				var shipment = factory.NewWithValidTestData<ForwardingShipment>();

				shipment.JS_RL_NKDestination = "AUSYD";
				AssertMatch(new Regex($".*AUSYD.*"), shipment.Logs.GetAllLogs()[0].Parameters["CHG"]);
				factory.Save();

				shipment.JS_RL_NKDestination = "USLAX";
				AssertMatch(new Regex($".*USLAX.*"), shipment.Logs.GetAllLogs()[3].Parameters["CHG"]);
			}
		}

		[TestDate(2023, 08, 22)]
		public void TestShipmentUpdateWithSuppress()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z97";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "JS_RL_NKDestination";
			Factory.Save();

			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
			{
				var factory = new BusinessObjectFactory();
				var shipment = factory.NewWithValidTestData<ForwardingShipment>();

				shipment.JS_RL_NKDestination = "AUSYD";
				AssertEquals(0, shipment.Logs.GetAllLogs().Count);
			}
		}
		[TestDate(2023, 08, 22)]
		public void TestNestUsing()
		{
			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
			{
				AssertEquals(true, ObjectFactory.Get<ISuppressHookHelper>().IsSuppressFieldOnChangeHook());
				using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
				{
					AssertEquals(true, ObjectFactory.Get<ISuppressHookHelper>().IsSuppressFieldOnChangeHook());
				}
				AssertEquals(true, ObjectFactory.Get<ISuppressHookHelper>().IsSuppressFieldOnChangeHook());
			}
			AssertEquals(false, ObjectFactory.Get<ISuppressHookHelper>().IsSuppressFieldOnChangeHook());
		}
	}
}
