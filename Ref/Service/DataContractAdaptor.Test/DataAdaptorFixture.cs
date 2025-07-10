using System.Security.Cryptography;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.Test
{
	[TestFixture]
	public class DataAdaptorFixture
	{
		[Test]
		public void RequireTransform()
		{
			var adaptor = new DataAdaptorTest();
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCarrierCode)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusCodeList)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusCodeType)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusMap)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusNomenclatureGroup)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusProcedure)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusRateType)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusTariff)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusTariffType)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusTaxOrFee)));
			Assert.IsTrue(adaptor.RequireTransform("0_9_9", typeof(RefCusTradeAgreement)));

			Assert.IsTrue(adaptor.RequireTransform("0_12_9", typeof(RefCusNomenclatureGroup)));

			Assert.IsTrue(adaptor.RequireTransform("0_13_9", typeof(RefCusNomenclatureGroup)));

			Assert.IsTrue(adaptor.RequireTransform("0_14_9", typeof(RefCusRateType)));

			Assert.IsTrue(adaptor.RequireTransform("0_16_9", typeof(RefCusCodeList)));

			Assert.IsTrue(adaptor.RequireTransform("0_20_9", typeof(RefTimeZoneSet)));
			Assert.IsTrue(adaptor.RequireTransform("0_20_9", typeof(RefCusTradeGroup)));

			Assert.IsTrue(adaptor.RequireTransform("0_23_9", typeof(RefCarrierCode)));

			Assert.IsTrue(adaptor.RequireTransform("0_25_9", typeof(RefDocOrgCusCode)));

			Assert.IsTrue(adaptor.RequireTransform("0_26_9", typeof(RefCusCodeType)));

			Assert.IsTrue(adaptor.RequireTransform("0_27_9", typeof(RefCusConditionType)));
			Assert.IsTrue(adaptor.RequireTransform("0_27_9", typeof(RefCusNomenclatureGroup)));
			Assert.IsTrue(adaptor.RequireTransform("0_27_9", typeof(RefCusPreference)));
			Assert.IsTrue(adaptor.RequireTransform("0_27_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_28_9", typeof(RefExchangeRateZZ)));
			Assert.IsTrue(adaptor.RequireTransform("0_28_9", typeof(RefCusTaxOrFeeType)));
			Assert.IsTrue(adaptor.RequireTransform("0_28_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_29_9", typeof(RefCusProcedure)));

			Assert.IsTrue(adaptor.RequireTransform("0_30_9", typeof(RefHarbourRate)));

			Assert.IsTrue(adaptor.RequireTransform("0_31_9", typeof(UNDGSubstanceADR)));

			Assert.IsTrue(adaptor.RequireTransform("0_32_9", typeof(UNDGSubstanceADR)));

			Assert.IsTrue(adaptor.RequireTransform("0_36_9", typeof(RefSysConfigType)));

			Assert.IsTrue(adaptor.RequireTransform("0_37_9", typeof(RefDocOrgCusCode)));

			Assert.IsTrue(adaptor.RequireTransform("0_39_9", typeof(RefExchangeRateZZ)));

			Assert.IsTrue(adaptor.RequireTransform("0_40_9", typeof(RefUNLOCO)));

			Assert.IsTrue(adaptor.RequireTransform("0_41_9", typeof(RefStlScript)));

			Assert.IsTrue(adaptor.RequireTransform("0_44_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_53_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_58_9", typeof(RefCusConditionType)));
			Assert.IsTrue(adaptor.RequireTransform("0_58_9", typeof(RefCusTariff)));
			Assert.IsTrue(adaptor.RequireTransform("0_58_9", typeof(RefCusPreference)));
			Assert.IsTrue(adaptor.RequireTransform("0_58_9", typeof(RefCusNomenclatureGroup)));

			Assert.IsTrue(adaptor.RequireTransform("0_81_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_86_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_96_9", typeof(RefCusTariff)));

			Assert.IsTrue(adaptor.RequireTransform("0_107_9", typeof(RefShippingLine)));
			Assert.IsTrue(adaptor.RequireTransform("0_111_9", typeof(UNDGSubstanceCFR)));

			Assert.IsTrue(adaptor.RequireTransform("0_112_9", typeof(RefCusConditionType)));
			Assert.IsTrue(adaptor.RequireTransform("0_112_9", typeof(RefCusTariff)));
			Assert.IsTrue(adaptor.RequireTransform("0_112_9", typeof(RefCusPreference)));
			Assert.IsTrue(adaptor.RequireTransform("0_112_9", typeof(RefCusNomenclatureGroup)));

			Assert.IsTrue(adaptor.RequireTransform("0_117_9", typeof(RefExchangeRateZZ)));

			Assert.IsTrue(adaptor.RequireTransform("0_124_9", typeof(UNDGSubstanceJTT)));

			Assert.IsTrue(adaptor.RequireTransform("0_138_9", typeof(RefUNLOCO)));
			Assert.IsTrue(adaptor.RequireTransform("0_139_9", typeof(RefExchangeRateZZ)));
			Assert.IsTrue(adaptor.RequireTransform("0_163_9", typeof(RefCusTariffType)));

			Assert.IsFalse(adaptor.RequireTransform(adaptor.VersionInfo, typeof(RefStlScript)));

			Assert.That(adaptor.RequireTransform("SRDb_563", typeof(RefCusCodeList)));
			Assert.That(adaptor.RequireTransform("SRDb_564", typeof(RefCusCodeList)), Is.False);
		}

		[Test]
		public void ToVersion()
		{
			var obj = new object();
			var adaptor = new DataAdaptorTest();
			var latestVersion = adaptor.LatestVersion.Item2;
			for (var i = 9; i <= latestVersion; i++)
			{
				var version = $"0_{i}_9";
				Assert.IsNotNull(adaptor.ToVersion(obj, version), $"{version} should not be null in DataAdaptor");
			}

			var nextVersion = $"0_{latestVersion + 1}_9";
			Assert.Throws<NotSupportedException>(() => { adaptor.ToVersion(obj, nextVersion); }, $"{nextVersion} not supported");
			Assert.That(adaptor.ToVersion(obj, null), Is.EqualTo(obj));
			var dataAdapterVersion = adaptor.VersionInfo;
			Assert.That(DataContract.Version == dataAdapterVersion, $"Version strings in DataContractAdapter.DataAdapter and Contract_0_9.DataContract.Version should match. CurrentVersion in DataAdapter = {dataAdapterVersion} and version for Contract_0_9.DataContract.Version = {DataContract.Version}");
		}

		[Test]
		public void TestV164Transformation()
		{
			var tariffType1 = new RefCusTariffType { ZZI_Description = new string('A', 40) };
			var tariffType2 = new RefCusTariffType { ZZI_Description = new string('B', 100) };
			var tariffType3 = new RefCusTariffType { ZZI_Description = new string('C', 100), Deleted = true };
			var versionToTest = "0_164_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tariffType1, versionToTest) as RefCusTariffType;
				Assert.False(result.Deleted);
				Assert.AreEqual(tariffType1.ZZI_Description, result.ZZI_Description);
				result = adaptor.ToVersion(tariffType2, versionToTest) as RefCusTariffType;
				Assert.False(result.Deleted);
				Assert.AreEqual(tariffType2.ZZI_Description, result.ZZI_Description);
				result = adaptor.ToVersion(tariffType3, versionToTest) as RefCusTariffType;
				Assert.True(result.Deleted);
				Assert.AreEqual(tariffType3.ZZI_Description, result.ZZI_Description);
			}
			versionToTest = "0_163_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tariffType1, versionToTest) as RefCusTariffType;
				Assert.False(result.Deleted);
				Assert.AreEqual(tariffType1.ZZI_Description, result.ZZI_Description);
				result = adaptor.ToVersion(tariffType2, versionToTest) as RefCusTariffType;
				Assert.False(result.Deleted);
				Assert.AreEqual(tariffType2.ZZI_Description.Substring(0, 50), result.ZZI_Description);
				result = adaptor.ToVersion(tariffType3, versionToTest) as RefCusTariffType;
				Assert.True(result.Deleted);
				Assert.AreEqual(tariffType3.ZZI_Description, result.ZZI_Description);
			}
		}

		[Test]
		public void TestV147Transformation()
		{
			var refCarrierCode = new RefCarrierCode
			{
				Deleted = false,
				RefCarrierCodeAttributes = new[]
				{
					new RefCarrierCodeAttribute { ZZG_Value = "TestUnicode" },
					new RefCarrierCodeAttribute { ZZG_Value = "测试UniCode" }
				}
			};

			var deletedRefCarrierCode = new RefCarrierCode
			{
				Deleted = true,
				RefCarrierCodeAttributes = new[]
				{
					new RefCarrierCodeAttribute { ZZG_Value = "TestUnicode" },
					new RefCarrierCodeAttribute { ZZG_Value = "测试Unicode" }
				}
			};
			var unsupportedData = "Some string";
			var versionToTest = "0_146_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(refCarrierCode, versionToTest) as RefCarrierCode;
				Assert.AreEqual(1, result.RefCarrierCodeAttributes.Length);
				result = adaptor.ToVersion(deletedRefCarrierCode, versionToTest) as RefCarrierCode;
				Assert.AreEqual(2, result.RefCarrierCodeAttributes.Length);
				var result2 = adaptor.ToVersion(unsupportedData, versionToTest);
				Assert.AreEqual("Some string", result2);
			}
			versionToTest = "0_147_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(refCarrierCode, versionToTest) as RefCarrierCode;
				Assert.AreEqual(2, result.RefCarrierCodeAttributes.Length);
				result = adaptor.ToVersion(deletedRefCarrierCode, versionToTest) as RefCarrierCode;
				Assert.AreEqual(2, result.RefCarrierCodeAttributes.Length);
				var result2 = adaptor.ToVersion(unsupportedData, versionToTest);
				Assert.AreEqual("Some string", result2);
			}
		}

		[Test]
		public void TestV141Transformation()
		{
			var refExchangeRate1 = new RefExchangeRateZZ { ZZN_AsPublished = new string('A', 10) };
			var refExchangeRate2 = new RefExchangeRateZZ { ZZN_AsPublished = new string('B', 35) };
			var refExchangeRate3 = new RefExchangeRateZZ { ZZN_ExRateType = "BUY" };
			var refExchangeRate4 = new RefExchangeRateZZ { ZZN_ExRateType = "SEL" };
			var versionToTest = "0_141_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(refExchangeRate1, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(refExchangeRate1.ZZN_AsPublished, result.ZZN_AsPublished);
				result = adaptor.ToVersion(refExchangeRate2, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(refExchangeRate2.ZZN_AsPublished, result.ZZN_AsPublished);

				result = adaptor.ToVersion(refExchangeRate3, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(refExchangeRate4, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);
			}
			versionToTest = "0_140_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(refExchangeRate1, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(refExchangeRate1.ZZN_AsPublished, result.ZZN_AsPublished);
				result = adaptor.ToVersion(refExchangeRate2, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(new string('B', 10), result.ZZN_AsPublished);

				result = adaptor.ToVersion(refExchangeRate3, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(true, result.Deleted);
				result = adaptor.ToVersion(refExchangeRate4, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(true, result.Deleted);
			}
		}

		[Test]
		public void TestV139Transformation()
		{
			var unloco = new RefUNLOCO
			{
				RL_Code = "AA",
				RefUNLOCORelatedPorts = new[]
				{
					new RefUNLOCORelatedPort { RLR_RL_NKRelatedPort = "AA", RLR_GroupNumber = 2},
					new RefUNLOCORelatedPort { RLR_RL_NKRelatedPort = "AA", RLR_GroupNumber = 4}
				}
			};
			var versionToTest = "0_139_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(unloco, versionToTest) as RefUNLOCO;
				Assert.That(result.RefUNLOCORelatedPorts, Has.Length.EqualTo(2));
			}
			versionToTest = "0_138_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(unloco, versionToTest) as RefUNLOCO;
				Assert.That(result.RefUNLOCORelatedPorts, Has.Length.EqualTo(1));
				Assert.That(result.RefUNLOCORelatedPorts[0].RLR_GroupNumber, Is.EqualTo(4));
			}
		}

		[Test]
		public void TestV125Transformation()
		{
			var undgSubstanceJtt1 = new UNDGSubstanceJTT { JTT_PSN = new string('A', 250) };
			var undgSubstanceJtt2 = new UNDGSubstanceJTT { JTT_PSN = new string('B', 280) };
			var versionToTest = "0_125_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(undgSubstanceJtt1, versionToTest) as UNDGSubstanceJTT;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(undgSubstanceJtt2, versionToTest) as UNDGSubstanceJTT;
				Assert.AreEqual(false, result.Deleted);
			}
			versionToTest = "0_124_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(undgSubstanceJtt1, versionToTest) as UNDGSubstanceJTT;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(undgSubstanceJtt2, versionToTest) as UNDGSubstanceJTT;
				Assert.AreEqual(true, result.Deleted);
			}
		}

		[Test]
		public void TestV118Transformation()
		{
			var refExchangeRateZZ1 = new RefExchangeRateZZ() { ZZN_ExRateType = "CUE" };
			var refExchangeRateZZ2 = new RefExchangeRateZZ() { ZZN_ExRateType = "BNS" };
			var refExchangeRateZZ3 = new RefExchangeRateZZ() { ZZN_ExRateType = "BNB" };
			var versionToTest = "0_118_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(refExchangeRateZZ1, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(refExchangeRateZZ2, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(refExchangeRateZZ3, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);
			}
			versionToTest = "0_117_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(refExchangeRateZZ1, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(refExchangeRateZZ2, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(true, result.Deleted);
				result = adaptor.ToVersion(refExchangeRateZZ3, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(true, result.Deleted);
			}
		}

		[Test]
		public void TestV113Transformation()
		{
			var conditionType1 = new RefCusConditionType { ZX2_ConditionType = "AANBC" };
			var conditionType2 = new RefCusConditionType { ZX2_ConditionType = "DENBCD" };
			var condition1 = new RefCusCondition { RefCusConditionType = conditionType1 };
			var condition2 = new RefCusCondition { RefCusConditionType = conditionType2 };
			var tariff = new RefCusTariff { RefCusConditions = new[] { condition1, condition2 } };
			var preference = new RefCusPreference { RefCusConditions = new[] { condition1, condition2 } };
			var nomenclatureGroup = new RefCusNomenclatureGroup { RefCusConditions = new[] { condition1, condition2 } };

			var versionToTest = "0_113_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(conditionType1, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(conditionType2, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);

				var transformedTariff = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(2, transformedTariff.RefCusConditions.Length);
				var transformedPreference = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(2, transformedPreference.RefCusConditions.Length);
				var transformedNomenclatureGroup = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(2, transformedNomenclatureGroup.RefCusConditions.Length);
			}

			versionToTest = "0_112_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(conditionType1, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(conditionType2, versionToTest) as RefCusConditionType;
				Assert.AreEqual(true, result.Deleted);

				var transformedTariff = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(1, transformedTariff.RefCusConditions.Length);
				var transformedPreference = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(1, transformedPreference.RefCusConditions.Length);
				var transformedNomenclatureGroup = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(1, transformedNomenclatureGroup.RefCusConditions.Length);
			}
		}

		[Test]
		public void TestV112Transformation()
		{
			var versionToTest = "0_112_9";
			var undgCFR = new UNDGSubstanceCFR { CFR_Variation = new string('a', 81) };
			var adaptor = new DataAdaptor();
			var data = adaptor.ToVersion(undgCFR, versionToTest) as UNDGSubstanceCFR;
			Assert.False(data.Deleted);
			Assert.AreEqual(new string('a', 81), data.CFR_Variation);

			versionToTest = "0_111_9";
			undgCFR = new UNDGSubstanceCFR { CFR_Variation = new string('a', 81) };
			adaptor = new DataAdaptor();
			data = adaptor.ToVersion(undgCFR, versionToTest) as UNDGSubstanceCFR;
			Assert.True(data.Deleted);
		}

		[Test]
		public void TestV108Transformation()
		{
			var versionToTest = "0_106_9";
			{
				var shippingLine = new RefShippingLine
				{
					Deleted = false
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(shippingLine, versionToTest) as RefShippingLine;
				Assert.Null(data.RefShippingLineEBLProviders);
			}

			versionToTest = "0_107_9";
			{
				var shippingLine = new RefShippingLine
				{
					Deleted = false,
					RefShippingLineEBLProviders = new[] {
						new RefShippingLineEBLProvider {
							RSE_Name = "TEST",
							RSE_IsAvailable = true
							}
						}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(shippingLine, versionToTest) as RefShippingLine;
				Assert.Null(data.RefShippingLineEBLProviders);
			}

			versionToTest = "0_108_9";
			{
				var shippingLine = new RefShippingLine
				{
					Deleted = false,
					RefShippingLineEBLProviders = new[] {
						new RefShippingLineEBLProvider {
							RSE_Name = "TEST",
							RSE_IsAvailable = true
							}
						}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(shippingLine, versionToTest) as RefShippingLine;
				Assert.AreEqual(1, data.RefShippingLineEBLProviders.Length);
			}
		}

		[TestCase("0_107_9", false)]
		[TestCase("0_106_9", true)]
		[TestCase("0_105_9", true)]
		[TestCase("0_104_9", true)]
		public void TestV107Transformation(string versionToTest, bool dataShouldBeDeleted)
		{
			var adaptor = new DataAdaptor();
			var newValues = new string[] { "NLT", "GLM" };
			var uNDGSubstanceCFR = new UNDGSubstanceCFR
			{
				Deleted = false,
				CFR_PAXAirRailLimitType = newValues[RandomNumberGenerator.GetInt32(0, newValues.Length)],
				CFR_CargoAirRailLimitType = newValues[RandomNumberGenerator.GetInt32(0, newValues.Length)]
			};

			var data = adaptor.ToVersion(uNDGSubstanceCFR, versionToTest) as UNDGSubstanceCFR;

			Assert.AreEqual(dataShouldBeDeleted, data.Deleted);
		}

		[Test]
		public void TestV97Transformation()
		{
			var versionToTest = "0_96_9";
			{
				var tariff = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffAdditionalCodes = new[] {
						new RefCusTariffAdditionalCode {
							RefCusApplicabilities = new[] {
								new RefCusApplicability(),
								new RefCusApplicability()
							}
						}
					}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(0, data.RefCusTariffAdditionalCodes[0].RefCusApplicabilities.Length);
			}

			versionToTest = "0_97_9";
			{
				var tariff = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffAdditionalCodes = new[] {
						new RefCusTariffAdditionalCode {
							RefCusApplicabilities = new[] {
								new RefCusApplicability(),
								new RefCusApplicability()
							}
						}
					}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(2, data.RefCusTariffAdditionalCodes[0].RefCusApplicabilities.Length);
			}
		}

		[Test]
		public void TestV89Transformation()
		{
			var versionToTest = "0_89_9";
			{
				var script = new RefStlScript
				{
					Deleted = false,
					STL_DataGranularity = "SPS"
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(script, versionToTest) as RefStlScript;
				Assert.False(data.Deleted);
			}

			versionToTest = "0_88_9";
			{
				var script = new RefStlScript
				{
					Deleted = false,
					STL_DataGranularity = "SPS"
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(script, versionToTest) as RefStlScript;
				Assert.True(data.Deleted);
			}
		}

		[Test]
		public void TestV87Transformation()
		{
			var versionToTest = "0_87_9";
			{
				var tariff = new RefCusTariff
				{
					Deleted = false,
					RefCusVATApplicabilities = new RefCusVATApplicability[]
					{
						new RefCusVATApplicability
						{
							ZX5_ZZZ_NKDataGrouping = "AAA",
							ZX5_AdditionalCode = "XX",
							ZX5_StartDate = new DateTime(2023, 1, 1),
							ZX5_ZZF_NKTaxOrFeeCode = "YY",
							ZX5_EndDate = new DateTime(2033, 1, 1)
						},
						new RefCusVATApplicability
						{
							ZX5_ZZZ_NKDataGrouping = "BBB",
							ZX5_AdditionalCode = "XX",
							ZX5_StartDate = new DateTime(2023, 1, 1),
							ZX5_ZZF_NKTaxOrFeeCode = "YY",
							ZX5_EndDate = new DateTime(2033, 1, 1)
						}
					},
					RefCusTariffNationalCodes = new RefCusTariffNationalCode[]
					{
						new RefCusTariffNationalCode
						{
							ZZW_NationalCode = "A",
							RefCusVATApplicabilities = new RefCusVATApplicability[]
							{
								new RefCusVATApplicability
								{
									ZX5_ZZZ_NKDataGrouping = "AAA",
									ZX5_AdditionalCode = "XX",
									ZX5_StartDate = new DateTime(2023, 2, 1),
									ZX5_ZZF_NKTaxOrFeeCode = "YY",
									ZX5_EndDate = new DateTime(2033, 2, 1)
								},
								new RefCusVATApplicability
								{
									ZX5_ZZZ_NKDataGrouping = "BBB",
									ZX5_AdditionalCode = "XX",
									ZX5_StartDate = new DateTime(2023, 2, 1),
									ZX5_ZZF_NKTaxOrFeeCode = "YY",
									ZX5_EndDate = new DateTime(2033, 2, 1)
								}
							}
						}
					}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(2, data.RefCusVATApplicabilities.Length);
				Assert.AreEqual(2, data.RefCusTariffNationalCodes.First().RefCusVATApplicabilities.Length);
			}

			versionToTest = "0_86_9";
			{
				var tariff = new RefCusTariff
				{
					Deleted = false,
					RefCusVATApplicabilities = new RefCusVATApplicability[]
					{
						new RefCusVATApplicability
						{
							ZX5_ZZZ_NKDataGrouping = "AAA",
							ZX5_AdditionalCode = "XX",
							ZX5_StartDate = new DateTime(2023, 1, 1),
							ZX5_ZZF_NKTaxOrFeeCode = "YY",
							ZX5_EndDate = new DateTime(2033, 1, 1)
						},
						new RefCusVATApplicability
						{
							ZX5_ZZZ_NKDataGrouping = "BBB",
							ZX5_AdditionalCode = "XX",
							ZX5_StartDate = new DateTime(2023, 1, 1),
							ZX5_ZZF_NKTaxOrFeeCode = "YY",
							ZX5_EndDate = new DateTime(2033, 1, 1)
						}
					},
					RefCusTariffNationalCodes = new RefCusTariffNationalCode[]
					{
						new RefCusTariffNationalCode
						{
							ZZW_NationalCode = "A",
							RefCusVATApplicabilities = new RefCusVATApplicability[]
							{
								new RefCusVATApplicability
								{
									ZX5_ZZZ_NKDataGrouping = "AAA",
									ZX5_AdditionalCode = "XX",
									ZX5_StartDate = new DateTime(2023, 2, 1),
									ZX5_ZZF_NKTaxOrFeeCode = "YY",
									ZX5_EndDate = new DateTime(2033, 2, 1)
								},
								new RefCusVATApplicability
								{
									ZX5_ZZZ_NKDataGrouping = "BBB",
									ZX5_AdditionalCode = "XX",
									ZX5_StartDate = new DateTime(2023, 2, 1),
									ZX5_ZZF_NKTaxOrFeeCode = "YY",
									ZX5_EndDate = new DateTime(2033, 2, 1)
								}
							}
						}
					}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(1, data.RefCusVATApplicabilities.Length);
				Assert.AreEqual(1, data.RefCusTariffNationalCodes.First().RefCusVATApplicabilities.Length);
			}
		}

		[Test]
		public void TestV82Transformation()
		{
			var versionToTest = "0_82_9";
			{
				var tariffShouldNotDeleted = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[]
					{
						new RefCusTariffBRCharacteristic
						{
							ZB1_CharacteristicType = "NVE",
							ZB1_Style = "BOOLEAN",
						}
					}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(tariffShouldNotDeleted, versionToTest) as RefCusTariff;
				Assert.IsNotEmpty(data.RefCusTariffBRCharacteristics);

				tariffShouldNotDeleted = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[]
					{
						new RefCusTariffBRCharacteristic
						{
							ZB1_CharacteristicType = "LPC",
							ZB1_Style = "DATE",
						}
					}
				};

				adaptor = new DataAdaptor();
				data = adaptor.ToVersion(tariffShouldNotDeleted, versionToTest) as RefCusTariff;
				Assert.IsNotEmpty(data.RefCusTariffBRCharacteristics);
			}

			versionToTest = "0_81_9";
			{
				var tariffShouldNotDeleted = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[]
					{
						new RefCusTariffBRCharacteristic
						{
							ZB1_CharacteristicType = "NVE",
							ZB1_Style = "BOOLEAN",
						}
					}
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(tariffShouldNotDeleted, versionToTest) as RefCusTariff;
				Assert.IsNotEmpty(data.RefCusTariffBRCharacteristics);

				var tariffShouldDeleted = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[]
					{
						new RefCusTariffBRCharacteristic
						{
							ZB1_CharacteristicType = "LPC",
							ZB1_Style = "DATE",
						}
					}
				};

				adaptor = new DataAdaptor();
				data = adaptor.ToVersion(tariffShouldDeleted, versionToTest) as RefCusTariff;
				Assert.IsEmpty(data.RefCusTariffBRCharacteristics);

				tariffShouldDeleted = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[]
					{
						new RefCusTariffBRCharacteristic
						{
							ZB1_CharacteristicType = "NVE",
							ZB1_Style = "DATE",
						}
					}
				};

				adaptor = new DataAdaptor();
				data = adaptor.ToVersion(tariffShouldDeleted, versionToTest) as RefCusTariff;
				Assert.IsEmpty(data.RefCusTariffBRCharacteristics);

				tariffShouldDeleted = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[]
					{
						new RefCusTariffBRCharacteristic
						{
							ZB1_CharacteristicType = "NCMTE",
							ZB1_Style = "STRING",
						}
					}
				};

				adaptor = new DataAdaptor();
				data = adaptor.ToVersion(tariffShouldDeleted, versionToTest) as RefCusTariff;
				Assert.IsEmpty(data.RefCusTariffBRCharacteristics);
			}
		}

		[Test]
		public void TestV59Transformation()
		{
			var conditionType1 = new RefCusConditionType { ZX2_ConditionClass = "CLASS", ZX2_ConditionType = "EEEEE" };
			var conditionType2 = new RefCusConditionType { ZX2_ConditionClass = "RISK", ZX2_ConditionType = "BBBBB" };
			var condition1 = new RefCusCondition { RefCusConditionType = conditionType1 };
			var condition2 = new RefCusCondition { RefCusConditionType = conditionType2 };
			var tariff = new RefCusTariff { RefCusConditions = new[] { condition1, condition2 } };
			var preference = new RefCusPreference { RefCusConditions = new[] { condition1, condition2 } };
			var nomenclatureGroup = new RefCusNomenclatureGroup { RefCusConditions = new[] { condition1, condition2 } };

			var versionToTest = "0_59_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(conditionType1, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(conditionType2, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);

				var transformedTariff = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(2, transformedTariff.RefCusConditions.Length);
				var transformedPreference = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(2, transformedPreference.RefCusConditions.Length);
				var transformedNomenclatureGroup = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(2, transformedNomenclatureGroup.RefCusConditions.Length);
			}

			versionToTest = "0_58_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(conditionType1, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(conditionType2, versionToTest) as RefCusConditionType;
				Assert.AreEqual(true, result.Deleted);

				var transformedTariff = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(1, transformedTariff.RefCusConditions.Length);
				var transformedPreference = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(1, transformedPreference.RefCusConditions.Length);
				var transformedNomenclatureGroup = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(1, transformedNomenclatureGroup.RefCusConditions.Length);
			}
		}

		[Test]
		public void TestV54Transformation()
		{
			var versionToTest = "0_54_9";
			{
				var refCusTariff = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffNationalCodes = new RefCusTariffNationalCode[]
					{
						new RefCusTariffNationalCode
						{
							ZZW_NationalCode = "AAAA"
						},
						new RefCusTariffNationalCode
						{
							ZZW_NationalCode = "AAA"
						}
					}
				};
				var adaptor = new DataAdaptor();
				var dataTariff = adaptor.ToVersion(refCusTariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(2, dataTariff.RefCusTariffNationalCodes.Length);
			}
			versionToTest = "0_53_9";
			{
				var refCusTariff = new RefCusTariff
				{
					Deleted = false,
					RefCusTariffNationalCodes = new RefCusTariffNationalCode[]
	{
						new RefCusTariffNationalCode
						{
							ZZW_NationalCode = "AAAA"
						},
						new RefCusTariffNationalCode
						{
							ZZW_NationalCode = "AAA"
						}
	}
				};
				var adaptor = new DataAdaptor();
				var dataTariff = adaptor.ToVersion(refCusTariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(1, dataTariff.RefCusTariffNationalCodes.Length);
			}
		}

		[Test]
		public void TestV46Transformation()
		{
			var versionToTest = "0_46_9";
			{
				var refCusTariff = new RefCusTariff
				{
					Deleted = false,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZF_NKTaxOrFeeCode = "1234",
				};
				var adaptor = new DataAdaptor();
				var dataTariff = adaptor.ToVersion(refCusTariff, versionToTest) as RefCusTariff;
				Assert.AreEqual("1234", dataTariff.ZZ1_ZZF_NKTaxOrFeeCode);
				Assert.AreEqual(false, dataTariff.Deleted);
			}
			versionToTest = "0_45_9";
			{
				var refCusTariff = new RefCusTariff
				{
					Deleted = false,
					ZZ1_TariffCode = "0101",
					ZZ1_ZZF_NKTaxOrFeeCode = "1234"
				};
				var adaptor = new DataAdaptor();
				var dataTariff = adaptor.ToVersion(refCusTariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(true, dataTariff.Deleted);

				var refCusTariff2 = new RefCusTariff
				{
					Deleted = false,
					ZZ1_TariffCode = "0202",
					ZZ1_ZZF_NKTaxOrFeeCode = "123",
					RefCusTariffNationalCodes = new RefCusTariffNationalCode[]
					{
						new RefCusTariffNationalCode
						{
							ZZW_ZZF_NKTaxOrFeeCode = "1234",
							ZZW_Description = "ABCD",
							ZZW_NationalCode = "AAA"
						},
						new RefCusTariffNationalCode
						{
							ZZW_ZZF_NKTaxOrFeeCode = "123",
							ZZW_Description = "ABCDF",
							ZZW_NationalCode = "AAA"
						}
					},
					RefCusVATApplicabilities = new RefCusVATApplicability[]
					{
						new RefCusVATApplicability
						{
							ZX5_ZZF_NKTaxOrFeeCode = "1234",
							ZX5_Description = "ABCD"
						},
						new RefCusVATApplicability
						{
							ZX5_ZZF_NKTaxOrFeeCode = "123",
							ZX5_Description = "ABCDF"
						}
					}
				};

				dataTariff = adaptor.ToVersion(refCusTariff2, versionToTest) as RefCusTariff;
				Assert.AreEqual(false, dataTariff.Deleted);
				Assert.That(dataTariff.RefCusTariffNationalCodes, Has.Length.EqualTo(1));
				Assert.That(dataTariff.RefCusVATApplicabilities, Has.Length.EqualTo(1));
				Assert.That(dataTariff.RefCusTariffNationalCodes.First().ZZW_ZZF_NKTaxOrFeeCode, Is.EqualTo("123"));
				Assert.That(dataTariff.RefCusVATApplicabilities.First().ZX5_ZZF_NKTaxOrFeeCode, Is.EqualTo("123"));
			}
		}

		[Test]
		public void TestV44Transformation()
		{
			var versionToTest = "0_44_9";
			{
				var stlScript = new RefStlScript
				{
					Deleted = false,
					STL_FeatureCode = "AAA",
					STL_DataGranularity = "DAY"
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(stlScript, versionToTest) as RefStlScript;
				Assert.AreEqual(false, data.Deleted);
			}
			versionToTest = "0_43_9";
			{
				var stlScript = new RefStlScript
				{
					Deleted = false,
					STL_FeatureCode = "AAA",
					STL_DataGranularity = "DAY"
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(stlScript, versionToTest) as RefStlScript;
				Assert.AreEqual(true, data.Deleted);
			}
		}

		[Test]
		public void TestV40Transformation()
		{
			var versionToTest = "0_40_9";
			{
				var refExchange = new RefExchangeRateZZ
				{
					Deleted = false,
					ZZN_ExRateType = "IAT",
					ZZN_Rate = 1
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(refExchange, versionToTest) as RefExchangeRateZZ;

				Assert.AreEqual(false, data.Deleted);
			}
			versionToTest = "0_39_9";
			{
				var refExchange = new RefExchangeRateZZ
				{
					Deleted = false,
					ZZN_ExRateType = "IAT",
					ZZN_Rate = 1
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(refExchange, versionToTest) as RefExchangeRateZZ;

				Assert.AreEqual(true, data.Deleted);
			}
		}

		[Test]
		public void TestV40Transformation_2()
		{
			var tradeGroup = new RefCusTradeGroup { ZZA_TradeGroup = "ZA" };
			var tradeGroup1 = new RefCusTradeGroup { ZZA_TradeGroup = "FR" };
			var applicability1 = new RefCusApplicability
			{
				ZZT_AdditionalCode = "0001",
				RefCusTradeGroup = tradeGroup,
				RefCusTradeGroup1 = tradeGroup1
			};
			var applicability2 = new RefCusApplicability
			{
				ZZT_AdditionalCode = "0002",
				RefCusTradeGroup = tradeGroup,
				RefCusTradeGroup1 = null
			};

			var rate = new RefCusRate
			{
				ZZ2_ZZZ_NKDataGrouping = "ZA",
				ZZ2_StartDate = new DateTime(2022, 1, 1),
				RefCusApplicabilities = new[] { applicability1, applicability2 }
			};
			var condition = new RefCusCondition
			{
				ZX1_Source = "Source",
				ZX1_StartDate = new DateTime(2022, 1, 1),
				RefCusConditionType = new RefCusConditionType { ZX2_ConditionClass = "CLASS", ZX2_ConditionType = "BBBBB" },
				RefCusApplicabilities = new[] { applicability1, applicability2 }
			};
			var additionalCode = new RefCusTariffAdditionalCode
			{
				ZY2_AdditionalCode = "0003",
				RefCusApplicabilities = new[] { applicability1, applicability2 }
			};

			var versionToTest = "0_40_9";
			{
				var tariff = new RefCusTariff
				{
					ZZ1_TariffCode = "001",
					RefCusTariffAdditionalCodes = new RefCusTariffAdditionalCode[] { additionalCode }
				};
				var preference = new RefCusPreference
				{
					ZZS_Preference = "100",
					RefCusRates = new RefCusRate[] { rate }
				};
				var nomenclatureGroup = new RefCusNomenclatureGroup
				{
					ZZ5_CompositeKey = "01.01..01.1",
					RefCusConditions = new RefCusCondition[] { condition }
				};

				var adaptor = new DataAdaptor();
				var refCusTariff = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(0, refCusTariff.RefCusTariffAdditionalCodes[0].RefCusApplicabilities.Length);

				var refCusPreference = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(2, refCusPreference.RefCusRates[0].RefCusApplicabilities.Length);

				var refCusNomenclatureGroup = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(2, refCusNomenclatureGroup.RefCusConditions[0].RefCusApplicabilities.Length);
			}
			versionToTest = "0_39_9";
			{
				var tariff = new RefCusTariff
				{
					ZZ1_TariffCode = "001",
					RefCusTariffAdditionalCodes = new RefCusTariffAdditionalCode[] { additionalCode }
				};
				var preference = new RefCusPreference
				{
					ZZS_Preference = "100",
					RefCusRates = new RefCusRate[] { rate }
				};
				var nomenclatureGroup = new RefCusNomenclatureGroup
				{
					ZZ5_CompositeKey = "01.01..01.1",
					RefCusConditions = new RefCusCondition[] { condition }
				};

				var adaptor = new DataAdaptor();
				var refCusTariff = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(0, refCusTariff.RefCusTariffAdditionalCodes[0].RefCusApplicabilities.Length);

				var refCusPreference = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(1, refCusPreference.RefCusRates[0].RefCusApplicabilities.Length);
				Assert.AreEqual("0002", refCusPreference.RefCusRates[0].RefCusApplicabilities[0].ZZT_AdditionalCode);

				var refCusNomenclatureGroup = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(1, refCusNomenclatureGroup.RefCusConditions[0].RefCusApplicabilities.Length);
				Assert.AreEqual("0002", refCusNomenclatureGroup.RefCusConditions[0].RefCusApplicabilities[0].ZZT_AdditionalCode);
			}
		}

		[Test]
		public void TestV38Transformation()
		{
			var versionToTest = "0_38_9";
			{
				var docOrg = new RefDocOrgCusCode
				{
					Deleted = false,
					DOC_DocumentType = "HBL",
					DOC_CodeType = "XXX"
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(docOrg, versionToTest) as RefDocOrgCusCode;

				Assert.AreEqual(false, data.Deleted);
			}
			versionToTest = "0_37_9";
			{
				var docOrg = new RefDocOrgCusCode
				{
					Deleted = false,
					DOC_DocumentType = "HBL",
					DOC_CodeType = "XXX"
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(docOrg, versionToTest) as RefDocOrgCusCode;

				Assert.AreEqual(true, data.Deleted);
			}
		}

		[Test]
		public void TestV37Transformation()
		{
			var versionToTest = "0_37_9";
			{
				var configType = new RefSysConfigType
				{
					ZRT_ConfigCode = "TEST",
					RefSysConfigs = new[] { new RefSysConfig() { ZRC_ZRT_NKConfigCode = "TEST", ZRC_BinaryValue = System.Text.Encoding.UTF8.GetBytes("Binary") } }
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(configType, versionToTest) as RefSysConfigType;

				Assert.AreEqual(1, data.RefSysConfigs.Length);
			}
			versionToTest = "0_36_9";
			{
				var configType = new RefSysConfigType
				{
					ZRT_ConfigCode = "TEST",
					RefSysConfigs = new[] { new RefSysConfig() { ZRC_ZRT_NKConfigCode = "TEST", ZRC_BinaryValue = System.Text.Encoding.UTF8.GetBytes("Binary") } }
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(configType, versionToTest) as RefSysConfigType;

				Assert.AreEqual(0, data.RefSysConfigs.Length);
			}
		}

		[Test]
		public void TestV36Transformation()
		{
			var versionToTest = "0_36_9";
			{
				var configType = new RefSysConfigType
				{
					ZRT_ConfigCode = "ABC",
					RefSysConfigs = new[] { new RefSysConfig() { ZRC_ZRT_NKConfigCode = "ABC" } },
				};
				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(configType, versionToTest) as RefSysConfigType;

				Assert.AreEqual("ABC", data.RefSysConfigs[0].ZRC_ZRT_NKConfigCode);
			}
			versionToTest = "0_35_9";
			{
				var configType = new RefSysConfigType
				{
					ZRT_ConfigCode = "ABC",
					RefSysConfigs = new[] { new RefSysConfig() { ZRC_ZRT_NKConfigCode = "ABC" } },
				};

				var adaptor = new DataAdaptor();
				var data = adaptor.ToVersion(configType, versionToTest) as RefSysConfigType;

				Assert.AreEqual("ABC", data.RefSysConfigs[0].ZRC_ZRT_NKConfigCode);
			}
		}

		[Test]
		public void TestOffsetFromUTCTransformation()
		{
			var versionToTest = "0_36_9";
			{
				var unloco = new RefUNLOCO();
				var unlocoUtcOffset = new RefUNLOCOUtcOffset
				{
					RLO_OffsetMinutesFromUtc = 90,
				};
				unloco.RefUNLOCOUtcOffsets = new[] { unlocoUtcOffset };

				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(new RefUNLOCO { RefUNLOCOUtcOffsets = new[] { unlocoUtcOffset } }, versionToTest) as RefUNLOCO;
				Assert.AreEqual(1.5m, result.RefUNLOCOUtcOffsets[0].RLO_OffsetFromUtc);

				var timeZone = new RefTimeZone
				{
					R2_OffsetMinutesFromUTC = 180
				};
				var resultTimeZone = adaptor.ToVersion(new RefTimeZoneSet { RefTimeZoneStandardZone = timeZone }, versionToTest) as RefTimeZoneSet;

				Assert.AreEqual(3, resultTimeZone.RefTimeZoneStandardZone.R2_OffsetFromUTC);

				timeZone = new RefTimeZone
				{
					R2_OffsetMinutesFromUTC = 0
				};
				resultTimeZone = adaptor.ToVersion(new RefTimeZoneSet { RefTimeZoneStandardZone = timeZone }, versionToTest) as RefTimeZoneSet;

				Assert.AreEqual(0, resultTimeZone.RefTimeZoneStandardZone.R2_OffsetFromUTC);
			}
		}

		[Test]
		public void TestV33Transformation()
		{
			var versionToTest = "0_33_9";
			{
				var substanceADR = new UNDGSubstanceADR
				{
					ADR_ADRTankSpecProv = "80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length",
				};

				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(substanceADR, versionToTest) as UNDGSubstanceADR;

				Assert.AreEqual(substanceADR.ADR_ADRTankSpecProv, result.ADR_ADRTankSpecProv);
			}
			versionToTest = "0_32_9";
			{
				var substanceADR = new UNDGSubstanceADR
				{
					ADR_ADRTankSpecProv = "80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length",
				};
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(substanceADR, versionToTest) as UNDGSubstanceADR;

				Assert.AreEqual(substanceADR.ADR_ADRTankSpecProv.Substring(0, 62), result.ADR_ADRTankSpecProv);
			}
		}

		[Test]
		public void TestV32Transformation()
		{
			var versionToTest = "0_32_9";
			{
				var substanceADR = new UNDGSubstanceADR
				{
					ADR_PSN = "260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_",
					ADR_SpecialProvisions = "40_Length_40_Length_40_Length_40_Length_",
					ADR_BulkTankIns = "80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length",
					ADR_TransportCategory = "15Length_15Leng",
					ADR_BulkSpecialProv = "20Length_20Length_20"
				};

				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(substanceADR, versionToTest) as UNDGSubstanceADR;

				Assert.AreEqual(substanceADR.ADR_PSN, result.ADR_PSN);
				Assert.AreEqual(substanceADR.ADR_SpecialProvisions, result.ADR_SpecialProvisions);
				Assert.AreEqual(substanceADR.ADR_BulkTankIns, result.ADR_BulkTankIns);
				Assert.AreEqual(substanceADR.ADR_TransportCategory, result.ADR_TransportCategory);
				Assert.AreEqual(substanceADR.ADR_BulkSpecialProv, result.ADR_BulkSpecialProv);
			}
			versionToTest = "0_31_9";
			{
				var substanceADR = new UNDGSubstanceADR
				{
					ADR_PSN = "260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_260Length_",
					ADR_SpecialProvisions = "40_Length_40_Length_40_Length_40_Length_",
					ADR_BulkTankIns = "80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length_80Length",
					ADR_TransportCategory = "15Length_15Leng",
					ADR_BulkSpecialProv = "20Length_20Length_20"
				};
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(substanceADR, versionToTest) as UNDGSubstanceADR;

				Assert.AreEqual(substanceADR.ADR_PSN.Substring(0, 200), result.ADR_PSN);
				Assert.AreEqual(substanceADR.ADR_SpecialProvisions.Substring(0, 30), result.ADR_SpecialProvisions);
				Assert.AreEqual(substanceADR.ADR_BulkTankIns.Substring(0, 50), result.ADR_BulkTankIns);
				Assert.AreEqual(substanceADR.ADR_TransportCategory.Substring(0, 12), result.ADR_TransportCategory);
				Assert.AreEqual(substanceADR.ADR_BulkSpecialProv.Substring(0, 14), result.ADR_BulkSpecialProv);
			}
		}

		[Test]
		public void TestV31Transformation()
		{
			var versionToTest = "0_31_9";
			{
				var harbourRate = new RefHarbourRate();
				harbourRate.ZXF_Mode = "ALL";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(harbourRate, versionToTest) as RefHarbourRate;
				Assert.AreEqual(false, result.Deleted);
			}
			versionToTest = "0_30_9";
			{
				var harbourRate = new RefHarbourRate();
				harbourRate.ZXF_Mode = "ALL";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(harbourRate, versionToTest) as RefHarbourRate;
				Assert.AreEqual(true, result.Deleted);
			}
		}

		[Test]
		public void TestV30Transformation()
		{
			var description = new string('a', 600);
			var procedureCode = "12345";
			var versionToTest = "0_30_9";
			{
				var tester = new RefCusProcedure();
				tester.ZZ6_Description = description;
				tester.ZZ6_ProcedureCode = procedureCode;
				tester.ZZ6_PreviousProcedureCode = procedureCode;
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusProcedure;
				Assert.AreEqual(false, result.ZZ6_TemporaryProcedure);
				Assert.AreEqual(600, result.ZZ6_Description.Length);
				Assert.AreEqual(false, result.Deleted);
			}

			versionToTest = "0_29_9";
			{
				var tester = new RefCusProcedure();
				tester.ZZ6_Description = description;
				tester.ZZ6_ProcedureCode = procedureCode;
				tester.ZZ6_PreviousProcedureCode = procedureCode;
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusProcedure;
				Assert.AreEqual(false, result.ZZ6_TemporaryProcedure);
				Assert.AreEqual(500, result.ZZ6_Description.Length);
				Assert.AreEqual(true, result.Deleted);
			}
		}

		[Test]
		public void TestV29Transformation_NotThrowException()
		{
			var versionToTest = "0_28_9";
			var adaptor = new DataAdaptor();
			var taxOrFeeType = new RefCusTaxOrFeeType();
			Assert.DoesNotThrow(() => adaptor.ToVersion(taxOrFeeType, versionToTest));
		}

		[Test]
		public void TestV29Transformation()
		{
			var versionToTest = "0_29_9";
			{
				var tester1 = new RefExchangeRateZZ();
				tester1.ZZN_ExRateType = "CUD";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester1, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(false, result.Deleted);

				var tester2 = new RefCusTariff();
				tester2.RefCusTariffAttributes = new RefCusTariffAttribute[] {
					new RefCusTariffAttribute
					{
						ZZ3_Name = "test",
						ZZ3_Value = "101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length1"
					}
				};
				var result2 = adaptor.ToVersion(tester2, versionToTest) as RefCusTariff;
				Assert.AreEqual(101, result2.RefCusTariffAttributes.First().ZZ3_Value.Length);

				var tester3 = new RefCusTaxOrFeeType();
				tester3.RefCusTaxOrFees = new RefCusTaxOrFee[] {
					new RefCusTaxOrFee
					{
						ZZF_Code = "AAAA"
					}
				};
				var result3 = adaptor.ToVersion(tester3, versionToTest) as RefCusTaxOrFeeType;
				Assert.AreEqual(1, result3.RefCusTaxOrFees.Length);
			}
			versionToTest = "0_28_9";
			{
				var tester1 = new RefExchangeRateZZ();
				tester1.ZZN_ExRateType = "CUD";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester1, versionToTest) as RefExchangeRateZZ;
				Assert.AreEqual(true, result.Deleted);

				var tester2 = new RefCusTariff();
				tester2.RefCusTariffAttributes = new RefCusTariffAttribute[] {
					new RefCusTariffAttribute
					{
						ZZ3_Name = "test",
						ZZ3_Value = "101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length1"
					}
				};
				var result2 = adaptor.ToVersion(tester2, versionToTest) as RefCusTariff;
				Assert.AreEqual(100, result2.RefCusTariffAttributes.First().ZZ3_Value.Length);

				var tester3 = new RefCusTaxOrFeeType();
				tester3.RefCusTaxOrFees = new RefCusTaxOrFee[] {
					new RefCusTaxOrFee
					{
						ZZF_Code = "AAAA"
					}
				};
				var result3 = adaptor.ToVersion(tester3, versionToTest) as RefCusTaxOrFeeType;
				Assert.AreEqual(0, result3.RefCusTaxOrFees.Length);
			}
		}

		[Test]
		public void TestV28Transformation()
		{
			var conditionType1 = new RefCusConditionType();
			conditionType1.ZX2_ConditionClass = "CLASS";
			conditionType1.ZX2_ConditionType = "DDDD";
			var conditionType2 = new RefCusConditionType();
			conditionType2.ZX2_ConditionClass = "VAT";
			conditionType2.ZX2_ConditionType = "ADBC";

			var condition1 = new RefCusCondition();
			condition1.ZX1_Comment = "AA";
			condition1.RefCusConditionType = conditionType1;
			var condition2 = new RefCusCondition();
			condition1.ZX1_Comment = "BB";
			condition2.RefCusConditionType = conditionType2;
			var conditions = new RefCusCondition[] { condition1, condition2 };

			var versionToTest = "0_28_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(conditionType1, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(conditionType2, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);

				var nomenclatureGroup = new RefCusNomenclatureGroup();
				nomenclatureGroup.RefCusConditions = conditions;
				var groupResult = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(2, groupResult.RefCusConditions.Length);
				Assert.AreEqual(condition1.ZX1_Comment, groupResult.RefCusConditions[0].ZX1_Comment);
				Assert.AreEqual(condition2.ZX1_Comment, groupResult.RefCusConditions[1].ZX1_Comment);

				var preference = new RefCusPreference();
				preference.RefCusConditions = conditions;
				var preferenceResult = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(2, preferenceResult.RefCusConditions.Length);
				Assert.AreEqual(condition1.ZX1_Comment, preferenceResult.RefCusConditions[0].ZX1_Comment);
				Assert.AreEqual(condition2.ZX1_Comment, preferenceResult.RefCusConditions[1].ZX1_Comment);

				var tariff = new RefCusTariff();
				tariff.RefCusConditions = conditions;
				var tariffResult = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(2, tariffResult.RefCusConditions.Length);
				Assert.AreEqual(condition1.ZX1_Comment, tariffResult.RefCusConditions[0].ZX1_Comment);
				Assert.AreEqual(condition2.ZX1_Comment, tariffResult.RefCusConditions[1].ZX1_Comment);
			}
			versionToTest = "0_27_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(conditionType1, versionToTest) as RefCusConditionType;
				Assert.AreEqual(false, result.Deleted);
				result = adaptor.ToVersion(conditionType2, versionToTest) as RefCusConditionType;
				Assert.AreEqual(true, result.Deleted);

				var nomenclatureGroup = new RefCusNomenclatureGroup();
				nomenclatureGroup.RefCusConditions = conditions;
				var groupResult = adaptor.ToVersion(nomenclatureGroup, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(1, groupResult.RefCusConditions.Length);
				Assert.AreEqual(condition1.ZX1_Comment, groupResult.RefCusConditions[0].ZX1_Comment);

				var preference = new RefCusPreference();
				preference.RefCusConditions = conditions;
				var preferenceResult = adaptor.ToVersion(preference, versionToTest) as RefCusPreference;
				Assert.AreEqual(1, preferenceResult.RefCusConditions.Length);
				Assert.AreEqual(condition1.ZX1_Comment, preferenceResult.RefCusConditions[0].ZX1_Comment);

				var tariff = new RefCusTariff();
				tariff.RefCusConditions = conditions;
				var tariffResult = adaptor.ToVersion(tariff, versionToTest) as RefCusTariff;
				Assert.AreEqual(1, tariffResult.RefCusConditions.Length);
				Assert.AreEqual(condition1.ZX1_Comment, tariffResult.RefCusConditions[0].ZX1_Comment);
			}
		}

		[Test]
		public void TestV27Transformation()
		{
			var versionToTest = "0_26_9";
			{
				var tester = new RefCusCodeType();
				tester.ZZK_ZZZ_NKDataGrouping = "AU";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusCodeType;
				Assert.AreEqual("", result.ZZK_ZZZ_NKDataGrouping);
			}
			versionToTest = "0_27_9";
			{
				var tester = new RefCusCodeType();
				tester.ZZK_ZZZ_NKDataGrouping = "AU";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusCodeType;
				Assert.AreEqual("AU", result.ZZK_ZZZ_NKDataGrouping);
			}
		}

		[Test]
		public void TestV26Transformation()
		{
			var versionToTest = "0_26_9";
			{
				var tester = new RefDocOrgCusCode();
				tester.DOC_ShortLabel = "ANY";
				tester.DOC_LongLabel = "Long";
				tester.DOC_DocumentType = "HAW";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefDocOrgCusCode;
				Assert.AreEqual(null, result.DOC_Label);
				Assert.AreEqual(false, result.Deleted);
			}
			versionToTest = "0_25_9";
			{
				var tester = new RefDocOrgCusCode();
				tester.DOC_ShortLabel = "ANY";
				tester.DOC_LongLabel = "Long";
				tester.DOC_DocumentType = "HAW";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefDocOrgCusCode;
				Assert.AreEqual("ANY|Long", result.DOC_Label);
				Assert.AreEqual(true, result.Deleted);
			}
		}

		[Test]
		public void TestV24Transformation()
		{
			var versionToTest = "0_24_9";
			{
				var adaptor = new DataAdaptor();
				var tester = new RefCarrierCode();
				tester.ZZ4_IsAir = true;
				tester.RefCarrierCodeAttributes = new RefCarrierCodeAttribute[] { new RefCarrierCodeAttribute { ZZG_Name = "ANY", ZZG_Value = "ANY" } };
				var result = adaptor.ToVersion(tester, versionToTest) as RefCarrierCode;
				Assert.AreEqual(1, result.RefCarrierCodeAttributes.Length);
				Assert.AreEqual(1, result.RefCarrierCodeAttributes.Count(x => x.ZZG_Name == "ANY"));

				var tester2 = new RefCusCodeList
				{
					RefCusCodeListAttributes = new[] {
						new RefCusCodeListAttribute() {
							ZZE_Value = "101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length1"
						}
					}
				};
				var adaptor2 = new DataAdaptor();
				var result2 = adaptor.ToVersion(tester2, versionToTest) as RefCusCodeList;
				Assert.AreEqual(101, result2.RefCusCodeListAttributes[0].ZZE_Value.Length);
			}
			versionToTest = "0_23_9";
			{
				var adaptor = new DataAdaptor();
				var tester = new RefCarrierCode();
				tester.ZZ4_IsAir = true;
				tester.RefCarrierCodeAttributes = new RefCarrierCodeAttribute[] { new RefCarrierCodeAttribute { ZZG_Name = "ANY", ZZG_Value = "ANY" } };
				var result = adaptor.ToVersion(tester, versionToTest) as RefCarrierCode;
				Assert.AreEqual(2, result.RefCarrierCodeAttributes.Length);
				Assert.AreEqual(1, result.RefCarrierCodeAttributes.Count(x => x.ZZG_Name == "ANY"));
				Assert.AreEqual(1, result.RefCarrierCodeAttributes.Count(x => x.ZZG_Name == "AIR"));

				var tester2 = new RefCusCodeList
				{
					RefCusCodeListAttributes = new[] {
						new RefCusCodeListAttribute() {
							ZZE_Value = "101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length101_Length1"
						}
					}
				};
				var adaptor2 = new DataAdaptor();
				var result2 = adaptor.ToVersion(tester2, versionToTest) as RefCusCodeList;
				Assert.AreEqual(100, result2.RefCusCodeListAttributes[0].ZZE_Value.Length);
			}
		}

		[Test]
		public void TestV21Transformation()
		{
			var tester = new RefTimeZoneSet
			{
				RefTimeZoneStandardZone = new RefTimeZone
				{
					RefTimeZoneRules = new RefTimeZoneRule[] {
						new RefTimeZoneRule { R4_TypeOfTime = "ANY" }
					}
				}
			};
			var versionToTest = "0_21_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefTimeZoneSet;
				Assert.AreEqual(null, result.RefTimeZoneStandardZone.RefTimeZoneRule);

				var testerTradeGroup = new RefCusTradeGroup();
				testerTradeGroup.RefCusTradeGroupLanguages = new RefCusTradeGroupLanguage[] { new RefCusTradeGroupLanguage { ZXD_Description = "ANY" } };
				var resultTradeGroup = adaptor.ToVersion(testerTradeGroup, versionToTest) as RefCusTradeGroup;
				Assert.AreEqual(null, resultTradeGroup.RefCusTradeGroupLanguage);
			}
			versionToTest = "0_20_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefTimeZoneSet;
				Assert.NotNull(result.RefTimeZoneStandardZone.RefTimeZoneRule);
				Assert.AreEqual(tester.RefTimeZoneStandardZone.RefTimeZoneRules[0].R4_TypeOfTime, result.RefTimeZoneStandardZone.RefTimeZoneRule[0].R4_TypeOfTime);

				var testerTradeGroup = new RefCusTradeGroup();
				testerTradeGroup.RefCusTradeGroupLanguages = new RefCusTradeGroupLanguage[] { new RefCusTradeGroupLanguage { ZXD_Description = "ANY" } };
				var resultTradeGroup = adaptor.ToVersion(testerTradeGroup, versionToTest) as RefCusTradeGroup;
				Assert.NotNull(resultTradeGroup.RefCusTradeGroupLanguage);
				Assert.AreEqual(testerTradeGroup.RefCusTradeGroupLanguages[0].ZXD_Description, resultTradeGroup.RefCusTradeGroupLanguage[0].ZXD_Description);
			}
		}

		[Test]
		public void TestPriorV20Transformation()
		{
			var versionToTest = "0_20_9";
			{
				var tester = new RefCountry();
				tester.RN_Code = "GB";
				tester.RN_EconomicGrouping = string.Empty;
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCountry;
				Assert.AreEqual(string.Empty, result.RN_EconomicGrouping);
			}
			versionToTest = "0_19_9";
			{
				var tester = new RefCountry();
				tester.RN_Code = "GB";
				tester.RN_EconomicGrouping = string.Empty;
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCountry;
				Assert.AreEqual("EUN", result.RN_EconomicGrouping);
			}
		}

		[Test]
		public void TestV19Transformation()
		{
			var versionToTest = "0_19_9";
			{
				var tester = new RefUNLOCO();
				tester.RL_GeoLocation = "POINT (-148.083 -19.9)";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefUNLOCO;
				Assert.AreEqual(null, result.RL_CoOrdinates);
			}
			versionToTest = "0_18_9";
			{
				var tester = new RefUNLOCO();
				tester.RL_GeoLocation = "POINT(-148.083 -19.9)";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefUNLOCO;
				Assert.AreEqual("1954S 14805W", result.RL_CoOrdinates);

				tester.RL_GeoLocation = "POINT(-148.083 19.9)";
				result = adaptor.ToVersion(tester, versionToTest) as RefUNLOCO;
				Assert.AreEqual("1954N 14805W", result.RL_CoOrdinates);

				tester.RL_GeoLocation = "POINT(148.083 19.9)";
				result = adaptor.ToVersion(tester, versionToTest) as RefUNLOCO;
				Assert.AreEqual("1954N 14805E", result.RL_CoOrdinates);

				tester.RL_GeoLocation = "POINT(148.083 -19.9)";
				result = adaptor.ToVersion(tester, versionToTest) as RefUNLOCO;
				Assert.AreEqual("1954S 14805E", result.RL_CoOrdinates);
			}
		}

		[Test]
		public void TestV17Transformation()
		{
			var tester = new RefCusCodeList
			{
				RefCusCodeListAttributes = new[] {
					new RefCusCodeListAttribute() {
						ZZE_ZXE_NKName = "CHS"
					}
				}
			};
			var versionToTest = "0_17_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusCodeList;
				Assert.AreEqual(null, result.RefCusCodeListAttributes[0].ZZE_Name);
			}
			versionToTest = "0_16_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusCodeList;
				Assert.AreEqual("CHS", result.RefCusCodeListAttributes[0].ZZE_Name);
			}
		}

		[Test]
		public void TestV15Transformation()
		{
			var tester = new RefCusRateType
			{
				RefCusRateCodes = new[] {
						new RefCusRateCode {
							RefCusRateCodeLanguages = new[] {
								new RefCusRateCodeLanguage() {
									ZXC_ZX6_NKLanguage = "CHS",
									ZXC_Description = "TEST"
								}
							}
						}
					}
			};
			var versionToTest = "0_15_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusRateType;
				Assert.AreEqual(null, result.RefCusRateCodes[0].RefCusRateCodeLanguages[0].ZXB_ZX6_NKLanguage);
				Assert.AreEqual(null, result.RefCusRateCodes[0].RefCusRateCodeLanguages[0].ZXB_Description);
			}
			versionToTest = "0_14_9";
			{
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusRateType;
				Assert.AreEqual("CHS", result.RefCusRateCodes[0].RefCusRateCodeLanguages[0].ZXB_ZX6_NKLanguage);
				Assert.AreEqual("TEST", result.RefCusRateCodes[0].RefCusRateCodeLanguages[0].ZXB_Description);
			}
		}

		[Test]
		public void TestV14Transformation()
		{
			var versionToTest = "0_14_9";
			{
				var tester = new RefCusNomenclatureGroup();
				tester.ZZ5_ZZ9_NKNomenclatureGroupType = "123";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual(null, result.ZZ5_Type);
			}
			versionToTest = "0_13_9";
			{
				var tester = new RefCusNomenclatureGroup();
				tester.ZZ5_ZZ9_NKNomenclatureGroupType = "123";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual("123", result.ZZ5_Type);
			}
		}

		[Test]
		public void TestTransformationOfClonedData()
		{
			var versionToTest = "0_13_9";
			{
				var transportMode = new RefCusCodeOrAttributeTransportMode() { ZZU_TransportMode = "test1" };
				var codeList_original = new RefCusCodeList();
				var codeListAttribute = new[] { new RefCusCodeListAttribute { ZZE_ZXE_NKName = "TMP", ZZE_Value = "1" }, new RefCusCodeListAttribute { ZZE_ZXE_NKName = "TMP2", ZZE_Value = "2" } };

				codeList_original.RefCusCodeListAttributes = codeListAttribute;
				codeListAttribute[0].RefCusCodeOrAttributeTransportModes = new[] { transportMode };

				var adaptor = new DataAdaptor();
				var clonedData = adaptor.ToVersion(codeList_original, versionToTest) as RefCusCodeList;

				Assert.AreSame(codeList_original.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0], transportMode);
				Assert.AreNotSame(clonedData.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0], codeList_original.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0]);
				Assert.AreEqual(clonedData.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0].ZZU_TransportMode, codeList_original.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0].ZZU_TransportMode);

				var newTransportMode = new RefCusCodeOrAttributeTransportMode { ZZU_TransportMode = "test2" };
				clonedData.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0] = newTransportMode;
				Assert.AreNotEqual(clonedData.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0].ZZU_TransportMode, codeList_original.RefCusCodeListAttributes[0].RefCusCodeOrAttributeTransportModes[0].ZZU_TransportMode);
			}
		}

		[Test]
		public void TestV13Transformation()
		{
			var versionToTest = "0_13_9";
			{
				var tester = new RefCusNomenclatureGroupNote();
				tester.ZZL_ZX6_NKLanguage = "CHT";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusNomenclatureGroupNote;
				Assert.AreEqual(null, result.ZZL_Language);
			}
			{
				var tester = new RefCusNomenclatureGroup();
				var note1 = new RefCusNomenclatureGroupNote();
				note1.ZZL_ZX6_NKLanguage = "ENG";
				note1.ZZL_Note = "TEST1";
				var note2 = new RefCusNomenclatureGroupNote();
				note2.ZZL_ZX6_NKLanguage = "CHS";
				note2.ZZL_Note = "TEST2";
				tester.RefCusNomenclatureGroupNotes = new RefCusNomenclatureGroupNote[] { note1, note2 };

				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusNomenclatureGroup;

				var resultRate1 = tester.RefCusNomenclatureGroupNotes.FirstOrDefault(x => x.ZZL_Note == "TEST1");
				Assert.AreEqual(null, resultRate1.ZZL_Language);
				var resultRate2 = tester.RefCusNomenclatureGroupNotes.FirstOrDefault(x => x.ZZL_Note == "TEST2");
				Assert.AreEqual(null, resultRate2.ZZL_Language);
			}
		}

		[Test]
		public void TestV10Transformation()
		{
			var versionToTest = "0_9_9";
			{
				var tester = new RefCusCodeList();
				tester.ZZD_ZZZ_NKDataGrouping = "ZZ";
				tester.ZZD_ZZK_NKCodeType = "TST";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusCodeList;
				Assert.AreEqual("ZZ", result.ZZD_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZZ", result.ZZD_RN_CountryOrGrouping);
				Assert.AreEqual("TST", result.ZZD_ZZK_NKCodeType, "ZZD_ZZK_NKCodeType");
				Assert.AreEqual("TST", result.ZZD_ZZN_NKCodeType, "ZZD_ZZN_NKCodeType");
			}
			{
				var tester = new RefCusCodeType();
				tester.ZZK_CodeType = "TST";
				tester.ZZK_Description = "DESC";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusCodeType;
				Assert.AreEqual("TST", result.ZZK_CodeType, "ZZK_CodeType");
				Assert.AreEqual("TST", result.ZZN_CodeType, "ZZN_CodeType");
				Assert.AreEqual("DESC", result.ZZK_Description, "ZZK_Description");
				Assert.AreEqual("DESC", result.ZZN_Description, "ZZN_Description");
			}
			{
				var tester = new RefCusMap();
				tester.ZZM_ZZZ_NKDataGrouping = "ZA";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusMap;
				Assert.AreEqual("ZA", result.ZZM_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZM_RN_CountryOrGrouping);
			}
			{
				var tester = new RefCusNomenclatureGroup();
				var groupNote1 = new RefCusNomenclatureGroupNote();
				var groupNote2 = new RefCusNomenclatureGroupNote();
				groupNote1.ZZL_ZX6_NKLanguage = "ENG";
				groupNote1.ZZL_ZZZ_NKDataGrouping = "CA";
				groupNote2.ZZL_ZX6_NKLanguage = "CHN";
				groupNote2.ZZL_ZZZ_NKDataGrouping = "AU";
				tester.ZZ5_ZZZ_NKDataGrouping = "ZA";
				tester.RefCusNomenclatureGroupNotes = new RefCusNomenclatureGroupNote[] { groupNote1, groupNote2 };
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual("ZA", result.ZZ5_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZ5_RN_CountryOrGrouping);
				var note1 = result.RefCusNomenclatureGroupNotes.FirstOrDefault(x => x.ZZL_Language == "ENG");
				Assert.AreEqual("CA", note1.ZZL_ZZZ_NKDataGrouping);
				Assert.AreEqual("CA", note1.ZZL_RN_CountryOrGrouping);
				var note2 = result.RefCusNomenclatureGroupNotes.FirstOrDefault(x => x.ZZL_Language == "CHN");
				Assert.AreEqual("AU", note2.ZZL_ZZZ_NKDataGrouping);
				Assert.AreEqual("AU", note2.ZZL_RN_CountryOrGrouping);
			}
			{
				var tester = new RefCusNomenclatureGroup
				{
					RefCusNomenclatureGroupNotes = new[] {
						new RefCusNomenclatureGroupNote() {
							ZZL_ZZZ_NKDataGrouping = "ZA"
						}
					}
				};
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusNomenclatureGroup;
				Assert.AreEqual("ZA", result.RefCusNomenclatureGroupNotes[0].ZZL_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.RefCusNomenclatureGroupNotes[0].ZZL_RN_CountryOrGrouping);
			}
			{
				var tester = new RefCusProcedure();
				tester.ZZ6_ZZZ_NKDataGrouping = "ZA";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusProcedure;
				Assert.AreEqual("ZA", result.ZZ6_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZ6_RN_CountryOrGrouping);
			}
			{
				var tester = new RefCusRateType();
				tester.ZZR_ZZZ_NKDataGrouping = "ZA";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusRateType;
				Assert.AreEqual("ZA", result.ZZR_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZR_RN_CountryOrGrouping);
			}
			{
				var rateType1 = new RefCusRateType();
				rateType1.ZZR_ZZZ_NKDataGrouping = "R1";
				var rateType2 = new RefCusRateType();
				rateType2.ZZR_ZZZ_NKDataGrouping = "R2";
				var tariffType1 = new RefCusTariffType();
				tariffType1.RefCusRateType = rateType1;
				tariffType1.ZZI_ZZZ_NKDataGrouping = "C1";
				var tariffType2 = new RefCusTariffType();
				tariffType2.ZZI_ZZZ_NKDataGrouping = "C2";
				tariffType2.RefCusRateType = rateType2;
				var rateType3 = new RefCusRateType();
				rateType3.ZZR_ZZZ_NKDataGrouping = "R3";
				var rateType4 = new RefCusRateType();
				rateType4.ZZR_ZZZ_NKDataGrouping = "R4";
				var rate1 = new RefCusRate();
				rate1.ZZ2_SelectorFormula = "SEL1";
				var rate2 = new RefCusRate();
				rate2.ZZ2_SelectorFormula = "SEL2";
				var relationship1 = new RefCusTariffRelationship();
				relationship1.ZZH_TariffCode = "REL1";
				relationship1.RefCusTariffType = tariffType1;
				var relationship2 = new RefCusTariffRelationship();
				relationship2.ZZH_TariffCode = "REL2";
				relationship2.RefCusTariffType = tariffType2;
				var tester = new RefCusTariff();
				tester.RefCusTariffRelationships = new RefCusTariffRelationship[] { relationship1, relationship2 };
				tester.RefCusRates = new RefCusRate[] { rate1, rate2 };
				tester.ZZ1_ZZZ_NKDataGrouping = "ZA";

				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusTariff;

				Assert.AreEqual("ZA", result.ZZ1_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZ1_RN_CountryOrGrouping);
				var resultRate1 = result.RefCusRates.FirstOrDefault(x => x.ZZ2_SelectorFormula == "SEL1");
				var resultRate2 = result.RefCusRates.FirstOrDefault(x => x.ZZ2_SelectorFormula == "SEL2");
				var resultRelationship1 = result.RefCusTariffRelationships.FirstOrDefault(x => x.ZZH_TariffCode == "REL1");
				var resultRelationship2 = result.RefCusTariffRelationships.FirstOrDefault(x => x.ZZH_TariffCode == "REL2");
				Assert.AreEqual("C1", resultRelationship1.RefCusTariffType.ZZI_ZZZ_NKDataGrouping);
				Assert.AreEqual("C1", resultRelationship1.RefCusTariffType.ZZI_RN_CountryOrGrouping);
				Assert.AreEqual("R1", resultRelationship1.RefCusTariffType.RefCusRateType.ZZR_ZZZ_NKDataGrouping);
				Assert.AreEqual("R1", resultRelationship1.RefCusTariffType.RefCusRateType.ZZR_RN_CountryOrGrouping);
				Assert.AreEqual("C2", resultRelationship2.RefCusTariffType.ZZI_ZZZ_NKDataGrouping);
				Assert.AreEqual("C2", resultRelationship2.RefCusTariffType.ZZI_RN_CountryOrGrouping);
				Assert.AreEqual("R2", resultRelationship2.RefCusTariffType.RefCusRateType.ZZR_ZZZ_NKDataGrouping);
				Assert.AreEqual("R2", resultRelationship2.RefCusTariffType.RefCusRateType.ZZR_RN_CountryOrGrouping);
				Assert.AreEqual(null, resultRate1.ZZ2_ZZZ_NKDataGrouping);
				Assert.AreEqual(null, resultRate2.ZZ2_ZZZ_NKDataGrouping);
			}
			{
				var rateType = new RefCusRateType();
				rateType.ZZR_ZZZ_NKDataGrouping = "CA";
				var tariffType = new RefCusTariffType();
				tariffType.ZZI_ZZZ_NKDataGrouping = "ZA";
				tariffType.RefCusRateType = rateType;
				var tester = new RefCusTariff
				{
					RefCusTariffRelationships = new[] {
						new RefCusTariffRelationship() {
							RefCusTariffType = tariffType
						}
					}
				};
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusTariff;
				Assert.AreEqual("ZA", result.RefCusTariffRelationships[0].RefCusTariffType.ZZI_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.RefCusTariffRelationships[0].RefCusTariffType.ZZI_RN_CountryOrGrouping);
				Assert.AreEqual("CA", result.RefCusTariffRelationships[0].RefCusTariffType.RefCusRateType.ZZR_ZZZ_NKDataGrouping);
				Assert.AreEqual("CA", result.RefCusTariffRelationships[0].RefCusTariffType.RefCusRateType.ZZR_RN_CountryOrGrouping);
			}
			{
				var rateType = new RefCusRateType();
				rateType.ZZR_ZZZ_NKDataGrouping = "CA";
				var tester = new RefCusTariffType();
				tester.ZZI_ZZZ_NKDataGrouping = "ZA";
				tester.RefCusRateType = rateType;
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusTariffType;
				Assert.AreEqual("ZA", result.ZZI_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZI_RN_CountryOrGrouping);
				Assert.AreEqual("CA", result.RefCusRateType.ZZR_ZZZ_NKDataGrouping);
				Assert.AreEqual("CA", result.RefCusRateType.ZZR_RN_CountryOrGrouping);
			}
			{
				var tester = new RefCusTaxOrFee();
				tester.ZZF_ZZZ_NKDataGrouping = "ZA";
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusTaxOrFee;
				Assert.AreEqual("ZA", result.ZZF_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZF_RN_CountryOrGrouping);
			}
			{
				var tester = new RefCusTradeAgreement();
				tester.ZZA_ZZZ_NKDataGrouping = "ZA";
				tester.ZZA_TradeGroup = "ZAT";
				var testerCountry = new RefCusTradeAgreementCountry();
				testerCountry.ZZB_RN_NKTradeGroupCountryCode = "CA";
				tester.RefCusTradeAgreementCountries = new RefCusTradeAgreementCountry[] { testerCountry };
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusTradeAgreement;
				Assert.AreEqual("ZA", result.ZZA_ZZZ_NKDataGrouping);
				Assert.AreEqual("ZA", result.ZZA_RN_CountryOrGrouping);
				Assert.AreEqual("ZAT", result.ZZA_TradeAgreement);
				Assert.AreEqual("ZAT", result.ZZA_TradeGroup);
				Assert.AreEqual("CA", result.RefCusTradeAgreementCountries[0].ZZB_RN_NKTradeAgreementCountryCode);
				Assert.AreEqual("CA", result.RefCusTradeAgreementCountries[0].ZZB_RN_NKTradeGroupCountryCode);
			}
			{
				var tester = new RefCusTradeAgreement
				{
					RefCusTradeAgreementCountries = new[] {
						new RefCusTradeAgreementCountry() {
							ZZB_RN_NKTradeGroupCountryCode = "ZA"
						}
					}
				};
				var adaptor = new DataAdaptor();
				var result = adaptor.ToVersion(tester, versionToTest) as RefCusTradeAgreement;
				Assert.AreEqual("ZA", result.RefCusTradeAgreementCountries[0].ZZB_RN_NKTradeGroupCountryCode);
				Assert.AreEqual("ZA", result.RefCusTradeAgreementCountries[0].ZZB_RN_NKTradeAgreementCountryCode);
			}
		}

		[Test]
		public void TestParseVersion()
		{
			var adaptor = new DataAdaptor();
			Assert.Throws<System.NotSupportedException>(() => { adaptor.ParseVersion("dummy version"); },
				"Invalid Version dummy version provided. Version should be of format #_##_##.");

			var parsedVersion = adaptor.ParseVersion("0_16_9");
			Assert.IsNotNull(parsedVersion);
			Assert.AreEqual(0, parsedVersion.Item1);
			Assert.AreEqual(16, parsedVersion.Item2);
			Assert.AreEqual(9, parsedVersion.Item3);
		}
	}

	public class DataAdaptorTest : DataAdaptor
	{
		public string VersionInfo => CurrentVersion.Item1 + "_" + CurrentVersion.Item2 + "_" + CurrentVersion.Item3;
		public Tuple<int, int, int> LatestVersion => CurrentVersion;
	}
}
