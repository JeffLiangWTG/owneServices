using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class BaseLoaderTestHelper
	{
		internal class BaseLoaderForTest : BaseLoader<DummyBizoForTest>
		{
			public BaseLoaderForTest(BusinessObjectFactory factory, int? batchSizeOverride = null) : base(factory)
			{
				BatchSizeOverride = batchSizeOverride;
			}

			public int? BatchSizeOverride
			{
				get;
			}

			public int MockDbHitCount
			{
				get;
				private set;
			}

			public int LastDbLoadBatchSize
			{
				get;
				private set;
			}

			public Dictionary<string, DummyBizoForTest[]> GetDictionaryForTesting() => GetCachedDictionary();

			protected override IEnumerable<DummyBizoForTest> LoadDataForBatchOfCriteriaSets(Dictionary<string, DummyBizoForTest[]> dictionary, IEnumerable<TariffCriteriaSet<DummyBizoForTest>> criteriaSetBatch)
			{
				MockDbHitCount++;
				LastDbLoadBatchSize = criteriaSetBatch.Count();
				return base.LoadDataForBatchOfCriteriaSets(dictionary, criteriaSetBatch);
			}

			protected override IEnumerable<(Guid CriteriaId, Guid dataPk)> LoadDataPksFromDatabase(DataTable criteriaDataTable, DataTable additionalCodesDataTable, DataTable secondTradeGroupsDataTable)
			{
				var criteriaIdAndPks = new List<(Guid CriteriaId, Guid bizoPk)>();

				foreach (DataRow row in criteriaDataTable.Rows)
				{
					var criteriaId = (Guid)row[TvpSelectionCriteria.Columns.CriteriaId];
					var bizoPk = (Guid)row[BizoPkColumn];
					criteriaIdAndPks.Add((criteriaId, bizoPk));
				}

				return criteriaIdAndPks;
			}

			protected override int CriteriaSetBatchSize => BatchSizeOverride ?? base.CriteriaSetBatchSize;

			protected override SchemaColumn DataPkSchemaColumn => factory.New<DummyEnterpriseBizo>().PKSchemaColumn;

			public override DataTable GetEmptyCriteriaTable()
			{
				var criteriaTable = base.GetEmptyCriteriaTable();
				criteriaTable.Columns.Add(BizoPkColumn, typeof(Guid));
				return criteriaTable;
			}
		}

		internal class SelectionCriteriaForTest : IZZApplicabilitySelectionCriteria
		{
			public SelectionCriteriaForTest(
				ZGuid bizoPk,
				ZString tradeGroupCountry,
				ZString primaryPreference,
				ISet<ZString> additionalCodes,
				ZString concessionOrder,
				ZString dataGrouping,
				ISet<ZString> secondTradeGroups = null)
			{
				BizoPk = bizoPk;
				TradeGroupCountry = tradeGroupCountry;
				PrimaryPreference = primaryPreference;
				AdditionalCodes = additionalCodes;
				ConcessionOrder = concessionOrder;
				DataGrouping = dataGrouping;
				SecondTradeGroups = secondTradeGroups;
			}

			public ZGuid BizoPk { get; }
			public ZDateTime EffectiveDate => ZDateTime.Today;
			public ZString TradeGroupCountry { get; }
			public ZString PrimaryPreference { get; }
			public ISet<ZString> AdditionalCodes { get; }
			public ZString ConcessionOrder { get; }
			public ZString DataGrouping { get; }
			public ISet<ZString> SecondTradeGroups { get; }
		}

		internal class TariffCriteriaSetForTest : TariffCriteriaSet<DummyBizoForTest>
		{
			public TariffCriteriaSetForTest(TariffView tariff, IZZApplicabilitySelectionCriteria selectionCriteria)
				: base(tariff, selectionCriteria)
			{
			}

			protected override string QualifiedName => "TariffCriteriaSetForTest";

			public override DataRow AddNewCriteriaSetRow(DataTable criteriaParameterTable, DataTable additionalCodesParameterTable, DataTable secondTradeGroupsDataTable, Guid criteriaId, ZGuid tariffPk, IZZApplicabilitySelectionCriteria selectionCriteria)
			{
				var criteriaRow = base.AddNewCriteriaSetRow(criteriaParameterTable, additionalCodesParameterTable, secondTradeGroupsDataTable, criteriaId, tariffPk, selectionCriteria);

				var selectionConditionCriteria = (SelectionCriteriaForTest)selectionCriteria;
				criteriaRow[BizoPkColumn] = selectionConditionCriteria.BizoPk.ToGuid();

				return criteriaRow;
			}
		}

		internal class DummyBizoForTest : DummyEnterpriseBizo, ITariffEffectiveDatesRelatedBusinessObject
		{
			public DummyBizoForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime StartDate => ZDateTime.Today.AddDays(-10);

			public ZDateTime EndDate => ZDateTime.Today.AddDays(10);

			public ZString DataGrouping => GlbCompany.CurrentCompany.Country.Code;
		}

		public const string BizoPkColumn = "BizoPk";
	}
}
