using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	sealed class UserExpressionEvaluatorTest : TestCaseWithFactory
	{
		public void TestEvaluateDocEngineCondition()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			shipment.JS_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDischargePort = "AUSYD";

			Factory.Save();

			const string macro = "\"<Destination.Location.Country.Code>\"==\"AU\"";

			var evaluator = new UserExpressionEvaluator(shipment);
			var res = evaluator.IsUserDefinedConditionMet(macro);

			AssertEquals($"Macro condition {macro} hase been evaluated", false, res);
		}

		public void TestEvaluateDocEngineCondition_NullBizObj()
		{
			AssertEquals("prerequisite; current department code", "BRN", GlbDepartment.CurrentDepartment.GE_Code);

			const string macro = "\"<CurrentDepartmentCode>\"==\"BRN\"";

			var evaluator = new UserExpressionEvaluator(null);
			var res = evaluator.IsUserDefinedConditionMet(macro);

			AssertEquals($"Macro condition {macro} hase been evaluated", false, res);
		}

		public void TestEvaluateFormBuilderMacroCondition_Bool()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			shipment.JS_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDischargePort = "AUSYD";

			Factory.Save();

			const string macro = "JS_RL_NKDischargePort.StartsWith(\"AU\")";

			var evaluator = new UserExpressionEvaluator(shipment);
			var res = evaluator.IsUserDefinedConditionMet(macro);

			AssertEquals($"Macro condition {macro} hase been evaluated", true, res);
		}

		public void TestEvaluateFormBuilderMacroCondition_ZBool()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			shipment.JS_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDischargePort = "AUSYD";

			Factory.Save();

			const string macro = "JS_IsForwardRegistered";

			var evaluator = new UserExpressionEvaluator(shipment);
			var res = evaluator.IsUserDefinedConditionMet(macro);

			AssertEquals($"Macro condition {macro} hase been evaluated", true, res);
		}

		public void TestEvaluateFormBuilderMacroCondition_GetCustomField()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			shipment.JS_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDischargePort = "AUSYD";

			const string customFieldName = "my custom field";
			const string customFieldValue = "value";

			var customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = shipment.PK;
			customAddOnValue.XV_ParentTableCode = shipment.TablePrefix;
			customAddOnValue.XV_Name = customFieldName;
			customAddOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			customAddOnValue.XV_Data = customFieldValue;

			Factory.Save();

			var evaluator = new UserExpressionEvaluator(shipment);

			AssertEquals($"GetCustomField macro condition hase been evaluated",
				true, evaluator.IsUserDefinedConditionMet("GetCustomField(\"my custom field\") == \"value\""));

			AssertEquals($"GetCustomField macro condition hase been evaluated",
				false, evaluator.IsUserDefinedConditionMet("GetCustomField(\"my custom field\") == \"other value\""));

			AssertEquals($"GetCustomField is case insensitive",
				true, evaluator.IsUserDefinedConditionMet("GetCustomField(\"mY cUstom FIelD\") == \"value\""));

			evaluator = new UserExpressionEvaluator(null);

			AssertEquals($"GetCustomField does not blow up on null data source",
				false, evaluator.IsUserDefinedConditionMet("GetCustomField(\"my custom field\") == \"value\""));
		}

		/// <summary>
		/// This tests is a developer only test in order to avoid false positives and have this test constantly coming in and out of amnesty
		/// </summary>
		[DeveloperOnlyTest]
		public void TestMacroEvaluationTimes()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			shipment.JS_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDischargePort = "AUSYD";

			const string customFieldName = "my custom field";
			const string customFieldValue = "value";

			var customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = shipment.PK;
			customAddOnValue.XV_ParentTableCode = shipment.TablePrefix;
			customAddOnValue.XV_Name = customFieldName;
			customAddOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			customAddOnValue.XV_Data = customFieldValue;

			Factory.Save();

			const int numberOfEvaluationTimes = 10000;

			var docEngineMacros = new[]
			{
				"\"<Destination.Location.Country.Code>\"!=\"AU\"&&\"<Origin.Location.Country.Code>\"!=\"AU\"&&\"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\"==\"\"",
				"\"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\" == \"\"",
				"\"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\"==\"\"&&\"<CurrentDepartmentCode>\"!=\"FEA\"",
				"\"<BaseShipment.GetCustomField(PRIORITY)>\"==\"SRVB\" && \"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\"==\"\""
			};

			var docEngineMacrosEvaluationTimeWithJSEngine = EvaluateDocEngineMacros(shipment, docEngineMacros, numberOfEvaluationTimes, true);
			var docEngineMacrosEvaluationTimeWithoutJSEngine = EvaluateDocEngineMacros(shipment, docEngineMacros, numberOfEvaluationTimes, false);

			var formBuilderEngineMacros = new[]
			{
				"!JS_RL_NKDestination.StartsWith(\"AU\") && !JS_RL_NKOrigin.StartsWith(\"AU\") && JS_JS_ColoadMasterShipment.IsEmpty",
				"JS_JS_ColoadMasterShipment.IsEmpty",
				"JS_JS_ColoadMasterShipment.IsEmpty && @env.Department.Code != \"FEA\"",
				"GetCustomField(\"PRIORITY\")==\"SRVB\" && JS_JS_ColoadMasterShipment.IsEmpty"
			};

			var formBuilderMacrosEvaluationTime = EvaluateDocEngineMacros(shipment, docEngineMacros, numberOfEvaluationTimes, false);

			var expectedEvaluationTimesFromFastestToSlowest = new[]
			{
				new
				{
					Label = "FormBuilder macros",
					EvaluationTime = formBuilderMacrosEvaluationTime
				},
				new
				{
					Label = "DocEngine macros JSEngine on",
					EvaluationTime = docEngineMacrosEvaluationTimeWithJSEngine
				},
				new
				{
					Label = "DocEngine macros JSEngine off",
					EvaluationTime = docEngineMacrosEvaluationTimeWithoutJSEngine
				}
			};

			var actualEvaluationTimesOrdered = expectedEvaluationTimesFromFastestToSlowest
				.OrderBy(t => t.EvaluationTime)
				.ToArray();

			AssertArrayEqualsByElements($"Expected order of evaluation times for {docEngineMacros.Length * numberOfEvaluationTimes} macros",
				expectedEvaluationTimesFromFastestToSlowest, actualEvaluationTimesOrdered);
		}

		TimeSpan EvaluateDocEngineMacros(ForwardingShipment shipment, string[] macros, int numberOfEvaluationTimes, bool useJSEngine)
		{
			void EvaluateMacros(int numberOfTimes)
			{
				using (RawDataRegistry.Instance.UseJSEngineForAutoRatingConditionsEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useJSEngine))
				using (var evaluator = new UserExpressionEvaluator(shipment))
				{
					for (int i = 0; i < numberOfTimes; i++)
					{
						foreach (var macro in macros)
						{
							evaluator.IsUserDefinedConditionMet(macro);
						}
					}
				}
			}

			// warming up to let the JSEngine initialise and populate caches
			EvaluateMacros(1);

			// record time of the evaluations
			var stopwatch = Stopwatch.StartNew();

			EvaluateMacros(numberOfEvaluationTimes);

			stopwatch.Stop();
			UserExpressionEvaluator.ClearStaticCaches();

			return stopwatch.Elapsed;
		}
	}
}
