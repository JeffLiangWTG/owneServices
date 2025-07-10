using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using Microsoft.OData.Client;
using Moq;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses
{
	internal class TariffHelperForTest : TariffHelper
	{
		public TestLogger Logger { get; }
		public TariffHelperForTest() : this(SetupMockRepo(false)) { }

		public TariffHelperForTest(bool useDuplicateAttributes) : this(SetupMockRepo(useDuplicateAttributes)) { }

		public TariffHelperForTest(IRefDataLoader dataLoader) : this(dataLoader, CreateLogger()) { }
		public TariffHelperForTest(IRefDataLoader dataLoader, TestLogger logger) : base(dataLoader, logger)
		{
			Logger = logger;
		}

		static TestLogger CreateLogger() => new TestLogger();

		public string GetTariffQuery_Exposed(string tariffCode) => GetTariffQuery(tariffCode);
		public string GetTariffAttributeQuery_Exposed() => GetTariffAttributeQuery();
		public string GetTariffRelationshipQuery_Exposed() => GetTariffRelationshipQuery();
		public string GetTariffRuleQuery_Exposed() => GetTariffRuleQuery();
		public string GetTariffAttributeRuleQuery_Exposed(Guid tariffPK) => GetTariffAttributeRuleQuery(tariffPK);
		public string GetTariffTypeQuery_Exposed(Guid tariffTypePK) => GetTariffTypeQuery(tariffTypePK);
		public string GetTariffUOMRuleQuery_Exposed(Guid tariffPK) => GetTariffUOMRuleQuery(tariffPK);
		public string GetRateRuleQuery_Exposed(Guid tariffPK) => GetRateRuleQuery(tariffPK);
		public string GetExpandedTariffQuery_Exposed(string tariffCode, DateTime startDate, DateTime endDate) => GetExpandedTariffQuery(tariffCode, startDate, endDate);
		public IEnumerable<T> Get_Exposed<T>(string query) => base.Get<T>(query);

		static IRefDataLoader SetupMockRepo(bool useDuplicateAttributes)
		{
			var mockRepo = new Mock<IRefDataLoader>();

			mockRepo.Setup(x => x.LoadData<RefCusTariff>(It.Is<string>(query => !query.Contains("$expand=")))).Returns((string query) => FilterData_RefCusTariff(query, MockRefCusTariffData));
			mockRepo.Setup(x => x.LoadData<RefCusTariff>(It.Is<string>(query => query.Contains("$expand=")))).Returns((string query) => FilterExpandedData_RefCusTariff(query, MockRefCusTariffExpandedData));
			mockRepo.Setup(x => x.LoadData<RefCusTariff>("RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'ZA' and ZZ1_TariffCode eq 'CRASH'")).Throws(new Exception("Simulated Crash"));

			if (!useDuplicateAttributes)
			{
				mockRepo.Setup(x => x.LoadData<RefCusTariffAttribute>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffAttribute(query, MockRefCusTariffAttributeData));
			}
			else
			{
				mockRepo.Setup(x => x.LoadData<RefCusTariffAttribute>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffAttribute(query, MockRefCusTariffAttributeDuplicatedData));
			}
			mockRepo.Setup(x => x.LoadData<RefCusTariffRelationship>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffRelationship(query, MockRefCusTariffRelationshipData));

			mockRepo.Setup(x => x.LoadData<RefCusTariffType>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffType(query, MockRefCusTariffTypeData));
			mockRepo.Setup(x => x.LoadData<RefCusTariffRule>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffRule(query, MockRefCusTariffRuleData));
			mockRepo.Setup(x => x.LoadData<RefCusTariffAttributeRule>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffAttributeRule(query, MockRefCusTariffAttributeRuleData));
			mockRepo.Setup(x => x.LoadData<RefCusTariffUOMRule>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusTariffUOMRule(query, MockRefCusTariffUOMRuleData));
			mockRepo.Setup(x => x.LoadData<RefCusRateRule>(It.IsAny<string>())).Returns((string query) => FilterData_RefCusRateRule(query, MockRefCusRateRuleData));

			return mockRepo.Object;
		}

		static Task<IEnumerable<RefCusTariff>> FilterData_RefCusTariff(string query, RefCusTariff[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariff>>(() =>
			{
				// queryFormat: RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq '{Constants.ZADataGrouping}' and ZZ1_TariffCode eq '{tariffCode}'
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 7)
				{
					return data.Where(x => x.ZZ1_ZZZ_NKDataGrouping == parts[2] && x.ZZ1_TariffCode == parts[6]).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static Task<IEnumerable<RefCusTariff>> FilterExpandedData_RefCusTariff(string query, RefCusTariff[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariff>>(() =>
			{
				// queryFormat: RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq '{Constants.ZADataGrouping}' and ZZ1_TariffCode eq '{tariffCode}' and ZZ1_StartDate le {startDate} and ZZ1_EndDate ge {endDate}
				//    &$expand=RefCusRates($expand=RefCusApplicabilities($expand=RefCusTradeGroup),RefCusPreference,RefCusRateCode)
				var parts = query.Split(new char[] { ' ', '\'', '&' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 16)
				{
					var startDate = DateTime.ParseExact(parts[10], "yyyy-MM-dd", CultureInfo.InvariantCulture);
					var endDate = DateTime.ParseExact(parts[14], "yyyy-MM-dd", CultureInfo.InvariantCulture);
					return data.Where(x => x.ZZ1_ZZZ_NKDataGrouping == parts[2] && x.ZZ1_TariffCode == parts[6] && x.ZZ1_StartDate.Date <= startDate && x.ZZ1_EndDate.Date >= endDate).ToArray();
				}
				else
				{
					return Array.Empty<RefCusTariff>();
				}
			});
		}

		static Task<IEnumerable<RefCusTariffAttribute>> FilterData_RefCusTariffAttribute(string query, RefCusTariffAttribute[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariffAttribute>>(() =>
			{
				// queryFormat: RefCusTariffAttributeUpdate?$filter=ZZ3_Name eq '{Constants.CheckDigit}'
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 3)
				{
					return data.Where(x => x.ZZ3_Name == parts[2]).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static Task<IEnumerable<RefCusTariffRelationship>> FilterData_RefCusTariffRelationship(string query, RefCusTariffRelationship[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariffRelationship>>(() =>
			{
				// queryFormat: RefCusTariffRelationshipUpdate
				return data;
			});
		}

		static Task<IEnumerable<RefCusTariffType>> FilterData_RefCusTariffType(string query, RefCusTariffType[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariffType>>(() =>
			{
				// queryFormat: RefCusTariffTypeUpdate?$filter=ZZI_PK eq {tariffTypePk}
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 3)
				{
					return data.Where(x => x.ZZI_PK == new Guid(parts[2])).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static Task<IEnumerable<RefCusTariffRule>> FilterData_RefCusTariffRule(string query, RefCusTariffRule[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariffRule>>(() =>
			{
				// queryFormat: RefCusTariffRuleUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq '{Constants.ZADataGrouping}'
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 3)
				{
					return data.Where(x => x.ZZ1_ZZZ_NKDataGrouping == parts[2]).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static Task<IEnumerable<RefCusTariffAttributeRule>> FilterData_RefCusTariffAttributeRule(string query, RefCusTariffAttributeRule[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariffAttributeRule>>(() =>
			{
				// queryFormat: RefCusTariffAttributeRuleUpdate?$filter=ZZ3_ZZ1_Tariff eq {tariffPk}
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 3)
				{
					return data.Where(x => x.ZZ3_ZZ1_Tariff == new Guid(parts[2])).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static Task<IEnumerable<RefCusTariffUOMRule>> FilterData_RefCusTariffUOMRule(string query, RefCusTariffUOMRule[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusTariffUOMRule>>(() =>
			{
				// queryFormat: RefCusTariffUOMRuleUpdate?$filter=ZZ8_ZZ1_Tariff eq {tariffPk}
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 3)
				{
					return data.Where(x => x.ZZ8_ZZ1_Tariff == new Guid(parts[2])).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static Task<IEnumerable<RefCusRateRule>> FilterData_RefCusRateRule(string query, RefCusRateRule[] data)
		{
			return Task.Factory.StartNew<IEnumerable<RefCusRateRule>>(() =>
			{
				// queryFormat: RefCusRateRuleUpdate?$filter=ZZ2_ZZ1_Tariff eq {tariffPk}
				var parts = query.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 3)
				{
					return data.Where(x => x.ZZ2_ZZ1_Tariff == new Guid(parts[2])).ToArray();
				}
				else
				{
					return data;
				}
			});
		}

		static RefCusTariff[] MockRefCusTariffData => new RefCusTariff[]
		{
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000001"), ZZ1_ZZZ_NKDataGrouping = "GB", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 1, ZZ1_Description = "GB Tariff 1 should not be used" },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000002"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 2, ZZ1_Description = "ZA Tariff 2 - No Attrib/Relation" },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000003"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 3, ZZ1_Description = "ZA Tariff 3 - Chk Digit Only" },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000004"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 4, ZZ1_Description = "ZA Tariff 4 - Relation Only" },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000005"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 5, ZZ1_Description = "ZA Tariff 5 - Chk & Relations",
				ZZ1_StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ZZ1_EndDate = new DateTime(2024, 1, 31, 23, 59, 00, DateTimeKind.Utc), ZZ1_ZZI_TariffType = new Guid("10000000000000000000000000000001") },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000006"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "30010101", ZZ1_IAMUnique = 6, ZZ1_Description = "ZA Tariff 6 - Different Code" },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000007"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 0, ZZ1_Description = "ZA Tariff 7 - Unique 0" },
			new RefCusTariff { ZZ1_PK = new Guid("00000000000000000000000000000008"), ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_TariffCode = "20010101", ZZ1_IAMUnique = 1, ZZ1_Description = "ZA Tariff 8 - Unique 1" },
		};

		static RefCusTariff[] MockRefCusTariffExpandedData => new RefCusTariff[]
		{
			new RefCusTariff
			{
				ZZ1_PK = new Guid("00000000000000000000000000000001"),
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_TariffCode = "20010101",
				ZZ1_IAMUnique = 1,
				ZZ1_Description = "ZA Tariff - Expanded with rates and applicabilities",
				ZZ1_StartDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
				ZZ1_EndDate = CommonHelper.MaximumDateTime,
				RefCusRates = new DataServiceCollection<RefCusRate>(new[]
				{
					new RefCusRate
					{
						ZZ2_StartDate = new DateTimeOffset(CommonHelper.MinimumDateTime, TimeSpan.Zero),
						ZZ2_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
						ZZ2_RateFormula = "0.10 * VFD",
						ZZ2_RateFormulaDerivedFrom = "10%",
						RefCusApplicabilities = new DataServiceCollection<RefCusApplicability>(new[]
						{
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
								ZZT_EndDate = new DateTimeOffset(2024, 12, 31, 23, 59, 0, TimeSpan.Zero),
								RefCusTradeGroup = new RefCusTradeGroup
								{
									ZZA_TradeGroup = TradeGroups.Standard,
								},
							},
						}, TrackingMode.None),
						RefCusPreference = new RefCusPreference
						{
							ZZS_Preference = Preferences.None
						},
						RefCusRateCode = new RefCusRateCode
						{
							ZY1_RateCode = Schedules.S1P1
						},
					},
					new RefCusRate
					{
						ZZ2_StartDate = new DateTimeOffset(CommonHelper.MinimumDateTime, TimeSpan.Zero),
						ZZ2_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
						ZZ2_RateFormula = "0.15 * VFD",
						ZZ2_RateFormulaDerivedFrom = "15%",
						RefCusApplicabilities = new DataServiceCollection<RefCusApplicability>(new[]
						{
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
								ZZT_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
								RefCusTradeGroup = new RefCusTradeGroup
								{
									ZZA_TradeGroup = TradeGroups.Standard,
								},
							},
						}, TrackingMode.None),
						RefCusPreference = new RefCusPreference
						{
							ZZS_Preference = Preferences.None
						},
						RefCusRateCode = new RefCusRateCode
						{
							ZY1_RateCode = Schedules.S1P1
						},
					},
					new RefCusRate
					{
						ZZ2_StartDate = new DateTimeOffset(CommonHelper.MinimumDateTime, TimeSpan.Zero),
						ZZ2_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
						ZZ2_RateFormula = "0",
						ZZ2_RateFormulaDerivedFrom = "FREE",
						RefCusApplicabilities = new DataServiceCollection<RefCusApplicability>(new[]
						{
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
								ZZT_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
								RefCusTradeGroup = new RefCusTradeGroup
								{
									ZZA_TradeGroup = TradeGroups.SADC,
								},
							},
						}, TrackingMode.None),
						RefCusPreference = new RefCusPreference
						{
							ZZS_Preference = Preferences.PreferentialRate
						},
						RefCusRateCode = new RefCusRateCode
						{
							ZY1_RateCode = Schedules.S1P1
						},
					},
					new RefCusRate
					{
						ZZ2_StartDate = new DateTimeOffset(CommonHelper.MinimumDateTime, TimeSpan.Zero),
						ZZ2_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
						ZZ2_RateFormula = "0.2 * VFD",
						ZZ2_RateFormulaDerivedFrom = "20%",
						RefCusApplicabilities = new DataServiceCollection<RefCusApplicability>(new[]
						{
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
								ZZT_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
								RefCusTradeGroup = new RefCusTradeGroup
								{
									ZZA_TradeGroup = "PY",
								},
							},
							new RefCusApplicability
							{
								ZZT_StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
								ZZT_EndDate = new DateTimeOffset(CommonHelper.MaximumDateTime, TimeSpan.Zero),
								RefCusTradeGroup = new RefCusTradeGroup
								{
									ZZA_TradeGroup = "UY",
								},
							},
						}, TrackingMode.None),
						RefCusPreference = new RefCusPreference
						{
							ZZS_Preference = Preferences.PreferentialRate
						},
						RefCusRateCode = new RefCusRateCode
						{
							ZY1_RateCode = Schedules.S1P1
						},
					},
				}, TrackingMode.None)
			},
		};

		static RefCusTariffAttribute[] MockRefCusTariffAttributeData => new RefCusTariffAttribute[]
		{
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000001"), ZZ3_Name = "FAKE", ZZ3_Value = "FilterTesting" },
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000001"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "GB-CHK" },
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000003"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "03" },
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000005"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "05" }
		};

		static RefCusTariffAttribute[] MockRefCusTariffAttributeDuplicatedData => new RefCusTariffAttribute[]
		{
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000001"), ZZ3_Name = "FAKE", ZZ3_Value = "FilterTesting" },
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000001"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "GB-CHK" },
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000003"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "03" },
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000003"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "03" }, //Duplicate
			new RefCusTariffAttribute { ZZ3_ZZ1_Tariff = new Guid("00000000000000000000000000000005"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "05" }
		};

		static RefCusTariffRelationship[] MockRefCusTariffRelationshipData => new RefCusTariffRelationship[]
		{
			new RefCusTariffRelationship { ZZH_ZZ1_Tariff = new Guid("00000000000000000000000000000001"), ZZH_TariffCode = "1001" },
			new RefCusTariffRelationship { ZZH_ZZ1_Tariff = new Guid("00000000000000000000000000000004"), ZZH_TariffCode = "1004" },
			new RefCusTariffRelationship { ZZH_ZZ1_Tariff = new Guid("00000000000000000000000000000005"), ZZH_TariffCode = "100501" },
			new RefCusTariffRelationship { ZZH_ZZ1_Tariff = new Guid("00000000000000000000000000000005"), ZZH_TariffCode = "100502" }
		};

		static RefCusTariffType[] MockRefCusTariffTypeData => new RefCusTariffType[]
		{
			new RefCusTariffType { ZZI_PK = new Guid("10000000000000000000000000000001"), ZZI_TariffType = "1P1", ZZI_ZZZ_NKDataGrouping = "ZA" },
			new RefCusTariffType { ZZI_PK = new Guid("10000000000000000000000000000002"), ZZI_TariffType = "17A", ZZI_ZZZ_NKDataGrouping = "ZA" }
		};

		static RefCusTariffRule[] MockRefCusTariffRuleData => new RefCusTariffRule[]
		{
			new RefCusTariffRule { ZZ1_PK = new Guid("20000000000000000000000000000001"), ZZ1_TariffCode = "010101", ZZ1_ZZZ_NKDataGrouping = "ZA" },
			new RefCusTariffRule { ZZ1_PK = new Guid("20000000000000000000000000000002"), ZZ1_TariffCode = "020202", ZZ1_ZZZ_NKDataGrouping = "ZA" },
			new RefCusTariffRule { ZZ1_PK = new Guid("20000000000000000000000000000003"), ZZ1_TariffCode = "030303", ZZ1_ZZZ_NKDataGrouping = "ZA", ZZ1_ZZI_TariffType = new Guid("10000000000000000000000000000002") },  // Tariff Type
			new RefCusTariffRule { ZZ1_PK = new Guid("20000000000000000000000000000004"), ZZ1_TariffCode = "02020202", ZZ1_ZZZ_NKDataGrouping = "ZA" }
		};

		static RefCusTariffAttributeRule[] MockRefCusTariffAttributeRuleData => new RefCusTariffAttributeRule[]
		{
			new RefCusTariffAttributeRule { ZZ3_PK = new Guid("30000000000000000000000000000001"), ZZ3_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "1" },
			new RefCusTariffAttributeRule { ZZ3_PK = new Guid("30000000000000000000000000000002"), ZZ3_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ3_Name = "ATNAME1", ZZ3_Value = "ATVALUE1" },
			new RefCusTariffAttributeRule { ZZ3_PK = new Guid("30000000000000000000000000000003"), ZZ3_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ3_Name = "ATNAME2", ZZ3_Value = "ATVALUE2" },
			new RefCusTariffAttributeRule { ZZ3_PK = new Guid("30000000000000000000000000000004"), ZZ3_ZZ1_Tariff = new Guid("20000000000000000000000000000003"), ZZ3_Name = Constants.CheckDigit, ZZ3_Value = "3" },
			new RefCusTariffAttributeRule { ZZ3_PK = new Guid("30000000000000000000000000000005"), ZZ3_ZZ1_Tariff = new Guid("20000000000000000000000000000003"), ZZ3_Name = "ATNAME3", ZZ3_Value = "ATVALUE3" },
		};

		static RefCusTariffUOMRule[] MockRefCusTariffUOMRuleData => new RefCusTariffUOMRule[]
		{
			new RefCusTariffUOMRule { ZZ8_PK = new Guid("40000000000000000000000000000001"), ZZ8_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ8_Type = "UO1", ZZ8_UOM = "ONE" },
			new RefCusTariffUOMRule { ZZ8_PK = new Guid("40000000000000000000000000000002"), ZZ8_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ8_Type = "UO2", ZZ8_UOM = "TWO" },
			new RefCusTariffUOMRule { ZZ8_PK = new Guid("40000000000000000000000000000003"), ZZ8_ZZ1_Tariff = new Guid("20000000000000000000000000000002"), ZZ8_Type = "UO3", ZZ8_UOM = "THREE" },
			new RefCusTariffUOMRule { ZZ8_PK = new Guid("40000000000000000000000000000004"), ZZ8_ZZ1_Tariff = new Guid("20000000000000000000000000000004"), ZZ8_Type = "UO4", ZZ8_UOM = "FOUR" }
		};

		static RefCusRateRule[] MockRefCusRateRuleData => new RefCusRateRule[]
		{
			new RefCusRateRule { ZZ2_PK = new Guid("50000000000000000000000000000001"), ZZ2_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ2_RateFormula = "NEW-RATE-FORMULA1", ZZ2_SelectorFormula = "pp='EUQUOTA'", ZZ2_ZZZ_NKDataGrouping = "ZA", ZZ2_RateFormulaDerivedFrom = "Rate Rule 1" },
			new RefCusRateRule { ZZ2_PK = new Guid("50000000000000000000000000000002"), ZZ2_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ2_RateFormula = "REPLACE-RATE-FORMULA", ZZ2_SelectorFormula = "", ZZ2_ZZZ_NKDataGrouping = "ZA", ZZ2_RateFormulaDerivedFrom = "Rate Rule 2" },
			new RefCusRateRule { ZZ2_PK = new Guid("50000000000000000000000000000003"), ZZ2_ZZ1_Tariff = new Guid("20000000000000000000000000000001"), ZZ2_RateFormula = "NEW-RATE-FORMULA2", ZZ2_SelectorFormula = "pp='EFTAQUOTA'", ZZ2_ZZZ_NKDataGrouping = "ZA", ZZ2_RateFormulaDerivedFrom = "Rate Rule 3" },
		};
	}
}
