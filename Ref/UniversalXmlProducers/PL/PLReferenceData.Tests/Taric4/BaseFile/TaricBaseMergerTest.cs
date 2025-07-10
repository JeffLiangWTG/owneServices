using System;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class TaricBaseMergerTest
{
	[Test]
	public void TestMergeUpdateIntoBaseXml()
	{
		var baseData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28))
		]);

		var updateData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(2, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		var expected = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28)),
			CreateMeasure(2, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			TestDataContent<findMeasureByDatesResponseHistory, measure>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.Measure);
		});
	}

	[Test]
	public void TestMerge_OriginTypeN()
	{
		var baseData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28))
		]);

		var updateData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(2, opType: OpType.C, origin: originType.T, transactionDate: new DateTime(2016, 07, 28)),
			CreateMeasure(3, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 08, 28))
		]);

		var expected = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28)),
			CreateMeasure(3, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 08, 28))
		]);

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			TestDataContent<findMeasureByDatesResponseHistory, measure>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.Measure);
		});
	}

	[Test]
	public void TestMerge_OriginTypeT()
	{
		var expected = CreateIsztarHistoryResponse(quotas:
		[
			CreateQuotaDefinition(1, origin: originType.T, transactionDate: new DateTime(2016, 06, 28)),
			CreateQuotaDefinition(3, origin: originType.T, transactionDate: new DateTime(2016, 08, 28))
		]);

		var baseData = CreateIsztarHistoryResponse(quotas:
		[
			CreateQuotaDefinition(1, origin: originType.T, transactionDate: new DateTime(2016, 06, 28))
		]);

		var updateData = CreateIsztarHistoryResponse(quotas:
		[
			CreateQuotaDefinition(2, origin: originType.N, transactionDate: new DateTime(2016, 07, 28)),
			CreateQuotaDefinition(3, origin: originType.T, transactionDate: new DateTime(2016, 08, 28))
		]);

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			TestDataContent<findQuotaDefinitionByDatesResponseHistory, quotaDefinition>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.QuotaDefinition);
		});
	}

	[Test]
	public void TestUpdateBaseXml()
	{
		var baseData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28))
		]);

		var updateData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		var expected = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			TestDataContent<findMeasureByDatesResponseHistory, measure>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.Measure);
		});
	}

	[Test]
	public void TestMergeAndUpdateIntoBaseXml()
	{
		var expected = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2020, 01, 01)),
			CreateMeasure(3, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28)),
			CreateMeasure(2, opType: OpType.U, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		var baseData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28)),
			CreateMeasure(3, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28))
		]);

		var updateData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2020, 01, 01)),
			CreateMeasure(2, opType: OpType.U, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			TestDataContent<findMeasureByDatesResponseHistory, measure>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.Measure);
		});
	}

	[Test]
	public void TestDeleteDataFromBaseXml()
	{
		var expected = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(3, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28)),
			CreateMeasure(2, opType: OpType.U, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		var baseData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28)),
			CreateMeasure(3, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2016, 06, 28))
		]);

		var updateData = CreateIsztarHistoryResponse(measures:
		[
			CreateMeasure(1, opType: OpType.D, origin: originType.N, transactionDate: new DateTime(2020, 01, 01)),
			CreateMeasure(2, opType: OpType.U, origin: originType.N, transactionDate: new DateTime(2016, 07, 28))
		]);

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			TestDataContent<findMeasureByDatesResponseHistory, measure>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.Measure);
		});
	}

	[Test]
	public void TestAllDataTypesForMergeAndUpdateIntoBaseXml()
	{
		var baseData = new IsztarHistoryResponse()
		{
			IsztarHistoryItem =
			[
				CreateItem(measures: []),
				CreateItem(publicationSigles: []),
				CreateItem(baseRegulations: []),
				CreateItem(modificationRegulations: []),
				CreateItem(prorogationRegulations: []),
				CreateItem(measureConditionCodes: []),
				CreateItem(fullTemporaryStopRegulations: []),
				CreateItem(explicitAbrogationRegulations: []),
				CreateItem(completeAbrogationRegulations: []),
				CreateItem(quotaDefinitions: []),
				CreateItem(footnoteTypes: []),
			]
		};

		var updateData = new IsztarHistoryResponse()
		{
			IsztarHistoryItem =
			[
				CreateItem(measures:
				[
					CreateMeasure(111, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2020, 01, 01)),
					CreateMeasure(110, opType: OpType.C, origin: originType.T, transactionDate: new DateTime(2020, 01, 01)),
				]),
				CreateItem(publicationSigles:
				[
					CreatePublicationSigle(222, origin: originType.N, transactionDate: new DateTime(2020, 01, 02)),
					CreatePublicationSigle(220, origin: originType.T, transactionDate: new DateTime(2020, 01, 02)),
				]),
				CreateItem(baseRegulations:
				[
					CreateBaseRegulation(333, origin: originType.N, transactionDate: new DateTime(2020, 01, 03)),
					CreateBaseRegulation(330, origin: originType.T, transactionDate: new DateTime(2020, 01, 03)),
				]),
				CreateItem(modificationRegulations:
				[
					CreateModificationRegulation(444, origin: originType.N, transactionDate: new DateTime(2020, 01, 04)),
					CreateModificationRegulation(440, origin: originType.T, transactionDate: new DateTime(2020, 01, 04)),
				]),
				CreateItem(prorogationRegulations:
				[
					CreateProrogationRegulation(555, origin: originType.N, transactionDate: new DateTime(2020, 01, 05)),
					CreateProrogationRegulation(550, origin: originType.T, transactionDate: new DateTime(2020, 01, 05)),
				]),
				CreateItem(measureConditionCodes:
				[
					CreateMeasureConditionCode(666, origin: originType.N, transactionDate: new DateTime(2020, 01, 06)),
					CreateMeasureConditionCode(660, origin: originType.T, transactionDate: new DateTime(2020, 01, 06)),
				]),
				CreateItem(fullTemporaryStopRegulations:
				[
					CreateFullTemporaryStopRegulation(777, origin: originType.N, transactionDate: new DateTime(2020, 01, 07)),
					CreateFullTemporaryStopRegulation(770, origin: originType.T, transactionDate: new DateTime(2020, 01, 07)),
				]),
				CreateItem(explicitAbrogationRegulations:
				[
					CreateExplicitAbrogationRegulation(888, origin: originType.N, transactionDate: new DateTime(2020, 01, 08)),
					CreateExplicitAbrogationRegulation(880, origin: originType.T, transactionDate: new DateTime(2020, 01, 08)),
				]),
				CreateItem(completeAbrogationRegulations:
				[
					CreateCompleteAbrogationRegulation(999, origin: originType.N, transactionDate: new DateTime(2020, 01, 09)),
					CreateCompleteAbrogationRegulation(990, origin: originType.T, transactionDate: new DateTime(2020, 01, 09)),
				]),
				CreateItem(quotaDefinitions:
				[
					CreateQuotaDefinition(1999, origin: originType.T, transactionDate: new DateTime(2020, 01, 09)),
					CreateQuotaDefinition(1990, origin: originType.N, transactionDate: new DateTime(2020, 01, 09)),
				]),
				CreateItem(footnoteTypes:
				[
					CreateFootnoteType(1999, origin: originType.T, transactionDate: new DateTime(2020, 01, 09)),
					CreateFootnoteType(1990, origin: originType.N, transactionDate: new DateTime(2020, 01, 09)),
				]),
				CreateItem()
			]
		};

		var expected = new IsztarHistoryResponse()
		{
			IsztarHistoryItem =
			[
				CreateItem(measures:
				[
					CreateMeasure(111, opType: OpType.C, origin: originType.N, transactionDate: new DateTime(2020, 01, 01)),
				]),
				CreateItem(publicationSigles:
				[
					CreatePublicationSigle(222, origin: originType.N, transactionDate: new DateTime(2020, 01, 02)),
				]),
				CreateItem(baseRegulations:
				[
					CreateBaseRegulation(333, origin: originType.N, transactionDate: new DateTime(2020, 01, 03)),
				]),
				CreateItem(modificationRegulations:
				[
					CreateModificationRegulation(444, origin: originType.N, transactionDate: new DateTime(2020, 01, 04)),
				]),
				CreateItem(prorogationRegulations:
				[
					CreateProrogationRegulation(555, origin: originType.N, transactionDate: new DateTime(2020, 01, 05)),
				]),
				CreateItem(measureConditionCodes:
				[
					CreateMeasureConditionCode(666, origin: originType.N, transactionDate: new DateTime(2020, 01, 06)),
				]),
				CreateItem(fullTemporaryStopRegulations:
				[
					CreateFullTemporaryStopRegulation(777, origin: originType.N, transactionDate: new DateTime(2020, 01, 07)),
				]),
				CreateItem(explicitAbrogationRegulations:
				[
					CreateExplicitAbrogationRegulation(888, origin: originType.N, transactionDate: new DateTime(2020, 01, 08)),
				]),
				CreateItem(completeAbrogationRegulations:
				[
					CreateCompleteAbrogationRegulation(999, origin: originType.N, transactionDate: new DateTime(2020, 01, 09)),
				]),
				CreateItem(quotaDefinitions:
				[
					CreateQuotaDefinition(1999, origin: originType.T, transactionDate: new DateTime(2020, 01, 09)),
				]),
			]
		};

		try
		{
			new TaricBaseMerger(new MergeStrategyFactory()).MergeUpdateToBase(baseData, updateData);
		}
		catch (Exception ex)
		{
			Assert.Fail($"{ex.Message}");
		}

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expected.IsztarHistoryItem.Length, baseData.IsztarHistoryItem.Length);

			for (var i = 0; i < expected.IsztarHistoryItem.Length; i++)
			{
				var expectedPropertyType = expected.IsztarHistoryItem[i].Item.GetType();
				var basePropertyType = baseData.IsztarHistoryItem[i].Item.GetType();

				Assert.AreEqual(expectedPropertyType, basePropertyType, $"items[{i}] are of different type");
			}

			TestDataContent<findMeasureByDatesResponseHistory, measure>(expected.IsztarHistoryItem[0].Item, baseData.IsztarHistoryItem[0].Item, Taric4Constants.ItemNodes.Measure);
			TestDataContent<findPublicationSigleByDatesResponseHistory, publicationSigle>(expected.IsztarHistoryItem[1].Item, baseData.IsztarHistoryItem[1].Item, Taric4Constants.ItemNodes.PublicationSigle);
			TestDataContent<findBaseRegulationByDatesResponseHistory, baseRegulation>(expected.IsztarHistoryItem[2].Item, baseData.IsztarHistoryItem[2].Item, Taric4Constants.ItemNodes.BaseRegulation);
			TestDataContent<findModificationRegulationByDatesResponseHistory, modificationRegulation>(expected.IsztarHistoryItem[3].Item, baseData.IsztarHistoryItem[3].Item, Taric4Constants.ItemNodes.ModificationRegulation);
			TestDataContent<findProrogationRegulationByDatesResponseHistory, prorogationRegulation>(expected.IsztarHistoryItem[4].Item, baseData.IsztarHistoryItem[4].Item, Taric4Constants.ItemNodes.ProrogationRegulation);
			TestDataContent<findMeasureConditionCodeByDatesResponse, measureConditionCode>(expected.IsztarHistoryItem[5].Item, baseData.IsztarHistoryItem[5].Item, Taric4Constants.ItemNodes.MeasureConditionCode);
			TestDataContent<findFullTemporaryStopRegulationByDatesResponseHistory, fullTemporaryStopRegulation>(expected.IsztarHistoryItem[6].Item, baseData.IsztarHistoryItem[6].Item, Taric4Constants.ItemNodes.FullTemporaryStopRegulation);
			TestDataContent<findExplicitAbrogationRegulationByDatesResponseHistory, explicitAbrogationRegulation>(expected.IsztarHistoryItem[7].Item, baseData.IsztarHistoryItem[7].Item, Taric4Constants.ItemNodes.ExplicitAbrogationRegulation);
			TestDataContent<findCompleteAbrogationRegulationByDatesResponseHistory, completeAbrogationRegulation>(expected.IsztarHistoryItem[8].Item, baseData.IsztarHistoryItem[8].Item, Taric4Constants.ItemNodes.CompleteAbrogationRegulation);
			TestDataContent<findQuotaDefinitionByDatesResponseHistory, quotaDefinition>(expected.IsztarHistoryItem[9].Item, baseData.IsztarHistoryItem[9].Item, Taric4Constants.ItemNodes.QuotaDefinition);
		});

		IsztarHistoryResponseIsztarHistoryItem CreateItem(
			measure[] measures = null,
			publicationSigle[] publicationSigles = null,
			baseRegulation[] baseRegulations = null,
			modificationRegulation[] modificationRegulations = null,
			prorogationRegulation[] prorogationRegulations = null,
			measureConditionCode[] measureConditionCodes = null,
			fullTemporaryStopRegulation[] fullTemporaryStopRegulations = null,
			explicitAbrogationRegulation[] explicitAbrogationRegulations = null,
			completeAbrogationRegulation[] completeAbrogationRegulations = null,
			quotaDefinition[] quotaDefinitions = null,
			footnoteType[] footnoteTypes = null) => new IsztarHistoryResponseIsztarHistoryItem()
			{
				Item =
				measures is not null ? new findMeasureByDatesResponseHistory() { Measure = measures }
				: publicationSigles is not null ? new findPublicationSigleByDatesResponseHistory() { PublicationSigle = publicationSigles }
				: baseRegulations is not null ? new findBaseRegulationByDatesResponseHistory() { BaseRegulation = baseRegulations }
				: modificationRegulations is not null ? new findModificationRegulationByDatesResponseHistory() { ModificationRegulation = modificationRegulations }
				: prorogationRegulations is not null ? new findProrogationRegulationByDatesResponseHistory() { ProrogationRegulation = prorogationRegulations }
				: measureConditionCodes is not null ? new findMeasureConditionCodeByDatesResponse() { MeasureConditionCode = measureConditionCodes }
				: fullTemporaryStopRegulations is not null ? new findFullTemporaryStopRegulationByDatesResponseHistory() { FullTemporaryStopRegulation = fullTemporaryStopRegulations }
				: explicitAbrogationRegulations is not null ? new findExplicitAbrogationRegulationByDatesResponseHistory() { ExplicitAbrogationRegulation = explicitAbrogationRegulations }
				: completeAbrogationRegulations is not null ? new findCompleteAbrogationRegulationByDatesResponseHistory() { CompleteAbrogationRegulation = completeAbrogationRegulations }
				: quotaDefinitions is not null ? new findQuotaDefinitionByDatesResponseHistory() { QuotaDefinition = quotaDefinitions }
				: footnoteTypes is not null ? new findFootnoteTypeByDatesResponse() { FootnoteType = footnoteTypes }
				: (object)null
			};

		publicationSigle CreatePublicationSigle(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		baseRegulation CreateBaseRegulation(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		modificationRegulation CreateModificationRegulation(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		prorogationRegulation CreateProrogationRegulation(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		measureConditionCode CreateMeasureConditionCode(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		fullTemporaryStopRegulation CreateFullTemporaryStopRegulation(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		explicitAbrogationRegulation CreateExplicitAbrogationRegulation(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};

		completeAbrogationRegulation CreateCompleteAbrogationRegulation(long hjid, originType origin, DateTime transactionDate) => new()
		{
			hjid = hjid,
			metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
		};
	}

	IsztarHistoryResponse CreateIsztarHistoryResponse(measure[] measures = null, quotaDefinition[] quotas = null) => new()
	{
		IsztarHistoryItem = [
			measures is not null
			? new IsztarHistoryResponseIsztarHistoryItem() { Item = new findMeasureByDatesResponseHistory() { Measure = measures } }
			: new IsztarHistoryResponseIsztarHistoryItem() { Item = new findQuotaDefinitionByDatesResponseHistory() { QuotaDefinition = quotas } },
		]
	};

	measure CreateMeasure(long hjid, OpType opType, originType origin, DateTime transactionDate) => new()
	{
		hjid = hjid,
		metainfo = new metainfo() { opType = opType, origin = origin, status = statusType.L, transactionDate = transactionDate }
	};

	quotaDefinition CreateQuotaDefinition(long hjid, originType origin, DateTime transactionDate) => new()
	{
		hjid = hjid,
		metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
	};

	footnoteType CreateFootnoteType(long hjid, originType origin, DateTime transactionDate) => new()
	{
		hjid = hjid,
		metainfo = new metainfo() { opType = OpType.C, origin = origin, status = statusType.L, transactionDate = transactionDate }
	};

	void TestDataContent<T1, T2>(object expectedItem, object actualItem, string memberName)
	{
		var expectedData = (T1)expectedItem;
		var actualData = (T1)actualItem;
		Assert.IsNotNull(expectedData, $"expectedData is Null for {memberName}");
		Assert.IsNotNull(actualData, $"actualData is Null for {memberName}");

		var expectedDataArray = (T2[])GetPropertyValue(expectedData, memberName);
		var actualDataArray = (T2[])GetPropertyValue(actualData, memberName);
		Assert.IsNotNull(expectedDataArray, $"expectedDataArray is Null for {memberName}");
		Assert.IsNotNull(actualDataArray, $"actualDataArray is Null for {memberName}");
		Assert.AreEqual(expectedDataArray.Length, actualDataArray.Length, $"expectedDataArray and actualDataArray different in item amount for {memberName}");

		for (var i = 0; i < expectedDataArray.Length; i++)
		{
			var expectedElement = expectedDataArray[i];
			var actualElement = actualDataArray[i];
			Assert.IsNotNull(expectedElement, $"expected item[{i}] is Null for {memberName}");
			Assert.IsNotNull(actualElement, $"actual item[{i}] is Null for {memberName}");

			var expectedHjid = GetPropertyValue(expectedElement, "hjid");
			var actualHjid = GetPropertyValue(actualElement, "hjid");
			Assert.IsNotNull(expectedHjid, $"expected item[{i}].hjid is Null for {memberName}");
			Assert.IsNotNull(actualHjid, $"actual item[{i}].hjid is Null for {memberName}");
			Assert.AreEqual(expectedHjid, actualHjid, $"item[{i}].hjid are not the same for {memberName}");

			var expectedMetainfo = GetPropertyValue(expectedElement, "metainfo");
			var actualMetainfo = GetPropertyValue(actualElement, "metainfo");
			Assert.IsNotNull(expectedMetainfo, $"expectedDataArrayElement[{i}].metainfo is Null for {memberName}");
			Assert.IsNotNull(actualMetainfo, $"actualDataArrayElement[{i}].metainfo is Null for {memberName}");

			AssertPropertiesAreTheSame("opType", expectedMetainfo, actualMetainfo);
			AssertPropertiesAreTheSame("status", expectedMetainfo, actualMetainfo);
			AssertPropertiesAreTheSame("origin", expectedMetainfo, actualMetainfo);
			AssertPropertiesAreTheSame("transactionDate", expectedMetainfo, actualMetainfo);

			void AssertPropertiesAreTheSame(string propName, object expected, object actual) => Assert.AreEqual(
				GetPropertyValue(expected, propName),
				GetPropertyValue(actual, propName),
				$"item[{i}].metainfo.{propName} are not the same for {memberName}");
		}

		object GetPropertyValue(object val, string propName) => val.GetType().GetProperty(propName).GetValue(val);
	}
}
