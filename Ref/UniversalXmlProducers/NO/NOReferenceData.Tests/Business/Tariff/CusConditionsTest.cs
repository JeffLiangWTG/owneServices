using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff.Tests
{
	sealed class CusConditionsTest
	{
		[Test]
		public void TestConvertRefCusCondition()
		{
			var cusConditionValues = new RefCusConditionValue[]
			{
				new RefCusConditionValue() { ZX3_Value = "TestValue" }
			};

			var conditionValue = CusConditions.ConvertRefCusCondition(true, "2024-01-01T11:11:11", "2024-01-15T22:22:22", "MA", cusConditionValues);
			Assert.Multiple(() =>
			{
				Assert.AreEqual(true, conditionValue.ZX1_IsImport);
				Assert.AreEqual(DateTime.Parse("2024-01-01 11:11:11"), conditionValue.ZX1_StartDate);
				Assert.AreEqual(DateTime.Parse("2024-01-15 22:22:22"), conditionValue.ZX1_EndDate);
				Assert.AreEqual("MA", conditionValue.ZX1_ZX2_NKConditionType);
				Assert.AreEqual("TestValue", conditionValue.RefCusConditionValues[0].ZX3_Value);
				Assert.AreEqual(conditionValue.ZX1_StartDate, conditionValue.RefCusApplicabilities[0].ZZT_StartDate);
				Assert.AreEqual(conditionValue.ZX1_EndDate, conditionValue.RefCusApplicabilities[0].ZZT_EndDate);
				Assert.AreEqual(conditionValue.ZX1_ZX2_NKConditionType, conditionValue.RefCusApplicabilities[0].ZZT_AdditionalCode);
			});
		}

		[Test]
		public void TestConvertRefCusConditionNullCondition()
		{
			TariffParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to parse RefCusCondition code due to empty code or invalid Dates.{Environment.NewLine}";
			var refCusConditionValueLocal = new List<RefCusConditionValue>();
			var conditionValue = CusConditions.ConvertRefCusCondition(true, "2024-01-01T11:11:11", "2024-01-15T22:22:22", "MA", refCusConditionValueLocal.ToArray());
			Assert.Multiple(() =>
			{
				Assert.AreEqual(null, conditionValue);
				Assert.AreEqual(errMsg, TariffParser.ErrorBuilder.ToString());
				
			});
		}

		[Test]
		public void TestConvertRefCusConditionInvalidDate()
		{
			TariffParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to parse RefCusCondition code due to empty code or invalid Dates.{Environment.NewLine}";
			var cusConditionValues = new RefCusConditionValue[]
			{
				new RefCusConditionValue() { ZX3_Value = "XXX" }
			};

			var conditionValue = CusConditions.ConvertRefCusCondition(true, "2024-01-01T11:11:11", "2024-02-30T22:22:22", "MA", cusConditionValues);
			Assert.Multiple(() =>
			{
				Assert.AreEqual(null, conditionValue);
				Assert.That(errMsg, Is.EqualTo(TariffParser.ErrorBuilder.ToString()).NoClip);
			});
		}

		[Test]
		public void TestConvertRefCusConditionInvalidCode()
		{
			TariffParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to parse RefCusCondition code due to empty code or invalid Dates.{Environment.NewLine}";
			var cusConditionValues = new RefCusConditionValue[]
			{
				new RefCusConditionValue() { ZX3_Value = string.Empty }
			};

			var conditionValue = CusConditions.ConvertRefCusCondition(true, "2024-01-01T11:11:11", "2024-02-28T22:22:22", string.Empty, cusConditionValues);
			Assert.Multiple(() =>
			{
				Assert.AreEqual(null, conditionValue);
				Assert.That(TariffParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
			});
		}

		[Test]
		public void TestGetCusConditions()
		{
			var refCusConditions = CusConditionCodes.GetCusConditionData();
			var importFees = XmlHelper.ReadDeserializedManifestResourceContent<AvgiftListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.innfoerselsavgift.xml");

			AssertConditions("22030040", "MA100", "([RET] < 75)");
			AssertConditions("22071019", "BV512", "([ASV] > 0.7 & [ASV] <= 2.7)");

			void AssertConditions(string tariffId, string code, string zX3Value)
			{
				Assert.Multiple(() =>
				{
					var conditions = CusConditions.GetCusConditions(refCusConditions, importFees, tariffId);
					Assert.AreEqual(code, conditions[0].ZX1_ZX2_NKConditionType, $"ZX1_ZX2_NKConditionType - tariff:{tariffId} - code:{code}");
					Assert.AreEqual(code, conditions[0].RefCusApplicabilities[0].ZZT_AdditionalCode, $"ZZT_AdditionalCode - tariff:{tariffId} - code:{code}");
					Assert.AreEqual(zX3Value, conditions[0].RefCusConditionValues[0].ZX3_Value, $"ZX3_Value - tariff:{tariffId} - code:{code}");
				});
			}
		}
	}
}
