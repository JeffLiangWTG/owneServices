using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Tools.Common;
using CargoWise.RefDbRepo.Tools.Common.Test;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataSetServiceGenerator.Test
{
	[TestFixture]
	public class ServiceGeneratorHelperFixture
	{
		[Test]
		public void CreateServiceForDateSets()
		{
			var dataSets = new[] { "RefDataGrouping" };
			var serviceScripts = serviceGeneratorHelper.CreateServiceForDateSets(dataSets);
			Assert.AreEqual(dataGroupingServiceScripts, serviceScripts);

			dataSets = new[] { "RefCusTariffType", "RefCusTariffTypeLanguage" };
			serviceScripts = serviceGeneratorHelper.CreateServiceForDateSets(dataSets);
			Assert.AreEqual(tariffTypeServiceScripts, serviceScripts);
		}

		[Test]
		public void TestGetStaticConstructor()
		{
			var dataSets = new[] { "RefAirline", "StmNote" };
			var relatedTypes = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			var constructor = serviceGeneratorHelper.GetStaticConstructor(dataSets, relatedTypes);
			Assert.AreEqual(@"
		static RefAirlineService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.AllowNullCollections = true;
				cfg.CreateMap<RefAirline, Models.RefAirline>();
				cfg.CreateMap<StmNote, Models.StmNote>().ForMember(dest => dest.ST_Table, opt => opt.MapFrom(src => nameof(RefAirline)));
			});
		}", constructor);

			dataSets = new[] { "RefCusTariffType", "RefCusTariffTypeLanguage" };
			relatedTypes = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			constructor = serviceGeneratorHelper.GetStaticConstructor(dataSets, relatedTypes);
			Assert.AreEqual(@"
		static RefCusTariffTypeService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusRateType, Models.RefCusRateType>();
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusTariffTypeLanguage, Models.RefCusTariffTypeLanguage>();
			});
		}", constructor);

			dataSets = new[] { "RefSysConfigType", "RefSysConfig" };
			relatedTypes = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			constructor = serviceGeneratorHelper.GetStaticConstructor(dataSets, relatedTypes);
			Assert.AreEqual(@"
		static RefSysConfigTypeService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.AllowNullCollections = true;
				cfg.CreateMap<RefSysConfig, Models.RefSysConfig>();
				cfg.CreateMap<RefSysConfigType, Models.RefSysConfigType>();
			});
		}", constructor);

			dataSets = new[] { "RefFacility", "RefFacilityLocalCode" };
			relatedTypes = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			constructor = serviceGeneratorHelper.GetStaticConstructor(dataSets, relatedTypes);
			Assert.AreEqual(@"
		static RefFacilityService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefFacility, Models.RefFacility>().ForMember(x => x.RFT_GeoLocation, o => o.MapFrom(s => s.RFT_GeoLocation.AsText()));
				cfg.CreateMap<RefFacilityLocalCode, Models.RefFacilityLocalCode>();
			});
		}", constructor);
		}

		[Test]
		public void TestGetJoinStatements()
		{
			var dataSets = new[] { "RefCountry", "RefLanguageText" };
			var dataSetName = dataSets[0];
			var contents = new StringBuilder();
			var tableAndRelatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			var variableNameDic = new Dictionary<string, string>
			{
				{ dataSetName, "country" },
				{ "RefDbVersionControl", "version.RVC_Deleted" }
			};
			var success = serviceGeneratorHelper.GetJoinStatements(dataSetName, "RN_PK", contents, tableAndRelatedTypesDic, variableNameDic);
			Assert.True(success);
			Assert.True(variableNameDic.ContainsKey("RefLanguageText"));
			Assert.AreEqual(@"
						   join languageText in RefDbRepo.Get<RefLanguageText>()
						   on new { c = country.RN_PK, l = TableCode } equals new { c = languageText.RLT_ParentId, l = languageText.RLT_ParentTableCode } into languageTextG
						   from languageText in languageTextG.DefaultIfEmpty()
", contents.ToString());

			dataSets = new[] { "RefCusTariffType", "RefCusTariffTypeLanguage" };
			dataSetName = dataSets[0];
			contents = new StringBuilder();
			tableAndRelatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			variableNameDic = new Dictionary<string, string>
			{
				{ dataSetName, "tariffType" },
				{ "RefDbVersionControl", "version.RVC_Deleted" }
			};
			success = serviceGeneratorHelper.GetJoinStatements(dataSetName, "ZZI_PK", contents, tableAndRelatedTypesDic, variableNameDic);
			Assert.True(success);
			Assert.True(variableNameDic.ContainsKey("RefCusTariffTypeLanguage"));
			Assert.True(variableNameDic.ContainsKey("RefCusRateType"));
			Assert.AreEqual(@"
						   join rateType in RefDbRepo.Get<RefCusRateType>().WhereNotDeleted(RefDbRepo) on tariffType.ZZI_ZZR_RateType equals rateType.ZZR_PK into rateTypeG
						   from rateType in rateTypeG.DefaultIfEmpty()

						   join tariffTypeLanguage in RefDbRepo.Get<RefCusTariffTypeLanguage>() on tariffType.ZZI_PK equals tariffTypeLanguage.ZXK_ZZI_TariffType into tariffTypeLanguageG
						   from tariffTypeLanguage in tariffTypeLanguageG.DefaultIfEmpty()
", contents.ToString());

			dataSets = new[] { "RefCusTaxOrFeeType", "RefCusTaxOrFee", "RefCusTaxOrFeeLanguage" };
			dataSetName = dataSets[0];
			contents = new StringBuilder();
			tableAndRelatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			variableNameDic = new Dictionary<string, string>
			{
				{ dataSetName, "taxOrFeeType" },
				{ "RefDbVersionControl", "version.RVC_Deleted" }
			};
			success = serviceGeneratorHelper.GetJoinStatements(dataSetName, "ZX0_PK", contents, tableAndRelatedTypesDic, variableNameDic);
			Assert.True(success);
			Assert.True(variableNameDic.ContainsKey("RefCusTaxOrFee"));
			Assert.True(variableNameDic.ContainsKey("RefCusTaxOrFeeLanguage"));
			Assert.AreEqual(@"
						   join taxOrFee in RefDbRepo.Get<RefCusTaxOrFee>() on taxOrFeeType.ZX0_TaxOrFeeType equals taxOrFee.ZZF_ZX0_NKTaxOrFeeType into taxOrFeeG
						   from taxOrFee in taxOrFeeG.DefaultIfEmpty()

						   join taxOrFeeLanguage in RefDbRepo.Get<RefCusTaxOrFeeLanguage>() on (taxOrFee != null ? taxOrFee.ZZF_PK : Guid.Empty) equals taxOrFeeLanguage.ZXU_ZZF_TaxOrFee into taxOrFeeLanguageG
						   from taxOrFeeLanguage in taxOrFeeLanguageG.DefaultIfEmpty()
", contents.ToString());

			dataSets = new[] { "RefCusCodeType", "RefCusCodeTypeLanguage" };
			dataSetName = dataSets[0];
			contents = new StringBuilder();
			tableAndRelatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			variableNameDic = new Dictionary<string, string>
			{
				{ dataSetName, "codeType" },
				{ "RefDbVersionControl", "version.RVC_Deleted" }
			};
			success = serviceGeneratorHelper.GetJoinStatements(dataSetName, "ZZK_PK", contents, tableAndRelatedTypesDic, variableNameDic);
			Assert.False(success);
		}

		[Test]
		public void TestSetupProperties()
		{
			var dataSets = new[] { "RefCusTariffType", "RefCusTariffTypeLanguage" };
			var tableAndRelatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			var variableNameDic = new Dictionary<string, string>()
			{
				{ dataSets[0], "tariffType" },
				{ "RefDbVersionControl", "version.RVC_Deleted" },
				{ "RefCusTariffTypeLanguage", "tariffTypeLanguage" },
				{ "RefCusRateType", "rateType" }
			};
			var contents = serviceGeneratorHelper.SetupProperties(dataSets[0], variableNameDic, tableAndRelatedTypesDic);
			Assert.AreEqual(@"						result.RefCusRateType = mapper.Map<Models.RefCusRateType>(dataSet[0].rateType);
						result.RefCusTariffTypeLanguages = dataSet.Select(x => x.tariffTypeLanguage).NotNull().DistinctByKey(x => x.ZXK_PK).Select(x => mapper.Map<Models.RefCusTariffTypeLanguage>(x)).ToArray();
", contents);

			dataSets = new[] { "RefCusTaxOrFeeType", "RefCusTaxOrFee", "RefCusTaxOrFeeLanguage" };
			tableAndRelatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			variableNameDic = new Dictionary<string, string>()
			{
				{ dataSets[0], "taxOrFeeType" },
				{ "RefDbVersionControl", "version.RVC_Deleted" },
				{ "RefCusTaxOrFee", "taxOrFee" },
				{ "RefCusTaxOrFeeLanguage", "taxOrFeeLanguage" }
			};
			contents = serviceGeneratorHelper.SetupProperties(dataSets[0], variableNameDic, tableAndRelatedTypesDic);
			Assert.AreEqual(@"					result.RefCusTaxOrFees = dataSet.Select(x => x.taxOrFee).NotNull().DistinctByKey(x => x.ZZF_PK).Select(d =>
					{
						var a = mapper.Map<Models.RefCusTaxOrFee>(d);
						a.RefCusTaxOrFeeLanguages = dataSet.Select(y => y.taxOrFeeLanguage).Where(x => x != null && x.ZXU_ZZF_TaxOrFee == d.ZZF_PK).DistinctByKey(x => x.ZXU_PK).Select(z => mapper.Map<Models.RefCusTaxOrFeeLanguage>(z)).ToArray();
						return a;
					}).ToArray();
", contents);
		}

		[Test]
		public void TestGetRelatedTypes()
		{
			var dataSets = new[] { "RefDataGrouping" };
			var relatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			Assert.AreEqual(0, relatedTypesDic.Keys.Count);

			dataSets = new[] { "RefCountry", "RefLanguageText" };
			relatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			Assert.AreEqual(1, relatedTypesDic.Keys.Count);
			Assert.AreEqual("RefLanguageText[]", relatedTypesDic["RefCountry"][0].PropertyType.Name);

			dataSets = new[] { "RefAirlineProductCode", "RefAirlineProductCodeCommodityCodePivot" };
			relatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			Assert.AreEqual(2, relatedTypesDic.Keys.Count);
			Assert.AreEqual(1, relatedTypesDic["RefAirlineProductCode"].Count);
			Assert.AreEqual("RefAirlineProductCodeCommodityCodePivot[]", relatedTypesDic["RefAirlineProductCode"][0].PropertyType.Name);
			Assert.AreEqual("RefAirlineCommodityCode", relatedTypesDic["RefAirlineProductCodeCommodityCodePivot"][0].PropertyType.Name);

			dataSets = new[] { "RefCusTradeGroup", "RefCusTradeGroupCountry", "RefCusTradeGroupLanguage" };
			relatedTypesDic = serviceGeneratorHelper.GetRelatedTypes(dataSets);
			Assert.AreEqual(1, relatedTypesDic.Keys.Count);
			Assert.AreEqual(2, relatedTypesDic["RefCusTradeGroup"].Count);
			CollectionAssert.AreEquivalent(new[] { "RefCusTradeGroupCountry[]", "RefCusTradeGroupLanguage[]" }, relatedTypesDic["RefCusTradeGroup"].Select(x => x.PropertyType.Name));
		}

		IDataSetHelper GetDataSetHelperMock()
		{
			var tableAndColumnsDic = DataSetHelperFixture.CreateTableAndColumns();
			var tableAndFKsDic = DataSetHelperFixture.CreateTableAndFKs();
			tableAndColumnsDic["RefCusTariffType"] = new List<string> { "ZZI_PK", "ZZI_TariffType", "ZZI_Description", "ZZI_ZZ9_NKNomenclatureGroupType", "ZZI_ZZZ_NKDataGrouping", "ZZI_ZZR_RateType" };
			tableAndColumnsDic["RefCusTariffTypeLanguage"] = new List<string> { "ZXK_PK", "ZXK_ZZI_TariffType", "ZXK_ZX6_NKLanguage", "ZXK_Description" };
			tableAndColumnsDic["RefCusRateType"] = new List<string> { "ZZR_PK", "ZZR_RateType", "ZZR_Description", "ZZR_IsPayable", "ZZR_ZZZ_NKDataGrouping", "ZZR_RX_NKFormulaCurrency" };
			tableAndFKsDic["RefCusTariffType"] = new List<FKRelationship> { new FKRelationship("RefCusTariffType", "RefCusRateType", "ZZI_ZZR_RateType", "ZZR_PK", "uniqueidentifier") };
			tableAndFKsDic["RefCusTariffTypeLanguage"] = new List<FKRelationship> { new FKRelationship("RefCusTariffTypeLanguage", "RefCusTariffType", "ZXK_ZZI_TariffType", "ZZI_PK", "uniqueidentifier") };
			if (tableAndFKsDic.ContainsKey("RefCusCodeTypeLanguage"))
			{
				tableAndFKsDic.Remove("RefCusCodeTypeLanguage");
			}

			var dataSetHelper = new Mock<IDataSetHelper>();
			dataSetHelper.Setup(x => x.GetAllTableAndColumns()).Returns(tableAndColumnsDic);
			dataSetHelper.Setup(x => x.GetAllTableAndFKs()).Returns(tableAndFKsDic);
			return dataSetHelper.Object;
		}

		IDataSetHelper dataSetHelper;
		ServiceGeneratorHelperTest serviceGeneratorHelper;

		[SetUp]
		public void SetUp()
		{
			dataSetHelper = GetDataSetHelperMock();
			serviceGeneratorHelper = new ServiceGeneratorHelperTest(dataSetHelper);
		}

		readonly string dataGroupingServiceScripts = @"
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefDataGroupingService : OneTableUpdateService<RefDataGrouping, Models.RefDataGrouping>
	{
		public RefDataGroupingService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}
	}
}
";

		readonly string tariffTypeServiceScripts = @"
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefCusTariffTypeService : ReferenceDataServiceBase<Models.RefCusTariffType>
	{
		protected override string TableCode => ""ZZI"";

		static RefCusTariffTypeService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusRateType, Models.RefCusRateType>();
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusTariffTypeLanguage, Models.RefCusTariffTypeLanguage>();
			});
		}

		public RefCusTariffTypeService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null) throw new InvalidProgramException(""Mapper configuration should not be set second time"");
				else config = value;
			}
		}

		IEnumerable<Models.RefCusTariffType> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();

			var dataSets = from tariffType in RefDbRepo.Get<RefCusTariffType>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint) on tariffType.ZZI_PK equals version.RVC_ParentPK

						   join rateType in RefDbRepo.Get<RefCusRateType>().WhereNotDeleted(RefDbRepo) on tariffType.ZZI_ZZR_RateType equals rateType.ZZR_PK into rateTypeG
						   from rateType in rateTypeG.DefaultIfEmpty()

						   join tariffTypeLanguage in RefDbRepo.Get<RefCusTariffTypeLanguage>() on tariffType.ZZI_PK equals tariffTypeLanguage.ZXK_ZZI_TariffType into tariffTypeLanguageG
						   from tariffTypeLanguage in tariffTypeLanguageG.DefaultIfEmpty()

						   select new { tariffType, version.RVC_Deleted, rateType, tariffTypeLanguage };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.OrderBy(x => x.tariffType.ZZI_PK).GroupBy(x => x.tariffType.ZZI_PK);

			int idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{
				var dataSet = dataSetG.FirstOrDefault().tariffType;
				var result = mapper.Map<Models.RefCusTariffType>(dataSet);
				result.Deleted = dataSetG.FirstOrDefault().RVC_Deleted;
				if (!result.Deleted)
				{
						result.RefCusRateType = mapper.Map<Models.RefCusRateType>(dataSetG.FirstOrDefault().rateType);
						result.RefCusTariffTypeLanguages = dataSetG.Select(x => x.tariffTypeLanguage).NotNull().DistinctByKey(x => x.ZXK_PK).Select(x => mapper.Map<Models.RefCusTariffTypeLanguage>(x)).ToArray();
				}
				result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
				yield return result;
			}
		}

		public override IEnumerable<Models.RefCusTariffType> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}
";
	}

	class ServiceGeneratorHelperTest : ServiceGeneratorHelper
	{
		public ServiceGeneratorHelperTest(IDataSetHelper dataSetHelper) : base(dataSetHelper)
		{
		}

		public new string GetStaticConstructor(string[] dataSets, Dictionary<string, List<PropertyInfo>> tableAndRelatedTypes)
		{
			return base.GetStaticConstructor(dataSets, tableAndRelatedTypes);
		}

		public new bool GetJoinStatements(string dataSetName, string dataSetPK, StringBuilder contents, Dictionary<string, List<PropertyInfo>> tableAndRelatedTypes, Dictionary<string, string> variableNameDic)
		{
			return base.GetJoinStatements(dataSetName, dataSetPK, contents, tableAndRelatedTypes, variableNameDic);
		}

		public new string SetupProperties(string dataSetName, Dictionary<string, string> variableNameDic, Dictionary<string, List<PropertyInfo>> tableAndRelatedTypes, bool isEF7 = false)
		{
			return base.SetupProperties(dataSetName, variableNameDic, tableAndRelatedTypes, false);
		}

		public new Dictionary<string, List<PropertyInfo>> GetRelatedTypes(string[] dataSets)
		{
			return base.GetRelatedTypes(dataSets);
		}
	}
}
