using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests
{
	public static class TestHelperClasses
	{
		public class VirtualBuilder : IRefXmlBuilder
		{
			public DateTime PublicationDate { get; private set; }
			public int BuildCount { get; private set; }
			public int CallCount { get; private set; }
			public void BuildXml(DateTime publicationDate, List<ITariffModel> data, string outputPath, string chapterFilter)
			{
				PublicationDate = publicationDate;
				BuildCount = data.Count;
				CallCount++;

				if (BuiltContent == null)
				{
					BuiltContent = new List<string>();
				}

				var models = data.OfType<ProcessDataCommon>();
				if (models?.Any() ?? false)
				{
					BuiltContent.Add(string.Join(", ", models.Select(x => x.Data)));
				}
				else
				{
					BuiltContent.Add("Not ProcessData content");
				}
			}
			public List<string> BuiltContent { get; private set; }

			public Common.Tests.CommonHelpers.DateTimeProvider TestDateTimeProvider { get; set; } = new Common.Tests.CommonHelpers.DateTimeProvider();
		}

		public class ProcessorTester<T> : ProcessorBase<T> where T : ProcessDataCommon, new()
		{
			public ProcessorTester(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders, new ProcessorTesterLoader<T>())
			{
				TestBuilders = builders.OfType<VirtualBuilder>().ToArray();
			}

			public VirtualBuilder[] TestBuilders { get; private set; }

			public new List<ITariffModel> Models { get { return base.Models; } set { base.Models = value; } }

			protected override void UpdateModelsCore(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector)
			{
				if (typeof(T) == typeof(ProcessDataOne))
				{
					Models.Cast<ProcessDataOne>().ToList().ForEach((m) =>
					{
						var l = referenceData.OfType<LoadDataOne>().FirstOrDefault(x => x.Key == m.Key.Replace("P1", "L1"));
						var p = referenceData.OfType<ProcessDataTwo>().FirstOrDefault(x => x.Key == m.Key.Replace("P1", "P2"));

						m.Data = $"{m.Key}.{l?.Key ?? "x"}.{p?.Key ?? "y"}.{p?.DataTwo ?? "z"}";
					});
				}
				else if (typeof(T) == typeof(ProcessDataTwo))
				{
					Models.Cast<ProcessDataTwo>().ToList().ForEach((m) =>
					{
						var l = referenceData.OfType<LoadDataOne>().FirstOrDefault(x => x.Key == m.Key.Replace("P2", "L2"));
						var p = referenceData.OfType<ProcessDataOne>().FirstOrDefault(x => x.Key == m.Key.Replace("P2", "P1"));

						m.Data = $"{m.Key}.{l?.Key ?? "x"}.{p?.Key ?? "y"}.{p?.DataOne ?? "z"}";
					});
				}
			}

			public void SimulateProcessing(string chapterFilter, List<FileDetails> files, StringBuilder errorCollector, List<ITariffModel> referenceData, string outputFolder)
			{
				LoadData(chapterFilter, files, errorCollector);
				UpdateModels(chapterFilter, referenceData, errorCollector);
				ProcessChapter(chapterFilter, outputFolder);
			}

			public bool? TestIsChapterSpecific { get; set; }
			public override bool IsChapterSpecific => TestIsChapterSpecific.HasValue ? TestIsChapterSpecific.Value : base.IsChapterSpecific;
		}

		class ProcessorTesterLoader<T> : XmlElementReader<T>, IProcessorLoader where T : ProcessDataCommon, new()
		{
			protected override string ParentElement => typeof(T).Name;

			protected override string ElementName => "ProcessItem";

			protected override T CreateMinimumModelFromXElement(XElement element)
			{
				var x = new T();

				x.Key = element.Element("Key").Value;
				x.Chapter = element.Element("Chapter").Value;
				UpdateIntFromElementIfPresent(element, "Priority", y => x.Priority = y);
				x.HJID = x.Key;

				if (x is ProcessDataOne p1)
				{
					p1.DataOne = $"T1-{x.Key}";
				}
				else if (x is ProcessDataTwo p2)
				{
					p2.DataTwo = $"T2-{x.Key}";
				}

				return x;
			}
		}

		public class TestTariffModel : ITariffModel, IRegulationData
		{
			public string HJID { get; set; }
			public string OpType { get; set; }
			public DateTime? OpDate { get; set; }

			public bool IsValid(StringBuilder errorCollector, string source) => Valid;
			public bool IsValidForCreate(StringBuilder errorCollector, string source) => true;
			public bool IsInChapter(string chapterFilter) => true;
			public bool IsReferenceData => false;
			public bool IsChapterSpecific => true;

			public string Id { get; set; }
			public bool Valid { get; set; }
			public DateTime EndDate { get; set; }
			public int Priority { get; set; }

			public string RegulationId { get; set; }

			public string RegulationRoleTypeId { get; set; }

			public ITariffModel GetLatest(ITariffModel firstModel)
			{
				if (firstModel is TestTariffModel fm)
				{
					if (fm.OpDate.HasValue && OpDate.HasValue && fm.OpDate > OpDate)
					{
						return fm;
					}
				}

				return this;
			}

			public bool ProcessUpdate(XElement element)
			{
				throw new NotImplementedException();
			}
		}

		public class TestRefModel : RefDataRepoModelEntityType
		{
			public string UniqueId { get; set; } = "UTUniqueId";
			public bool IsValid { get; set; }
			public DateTime EndDate { get; set; }
		}

		public class BuilderBaseTester : BuilderBase<TestRefModel>
		{
			public BuilderBaseTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
			{
			}

			protected override string FilePrefix => "UnitTest";
			protected override string XMLWriterDataSource => "UnitTest";

			protected override IEnumerable<TestRefModel> ConvertToRefModels(List<ITariffModel> data) => data.Cast<TestTariffModel>().Select(model => new TestRefModel() { UniqueId = model.Id, IsValid = model.Valid, EndDate = model.EndDate });
			protected override void DuplicateError(TestRefModel refModel, string uniqueId, string chapterFilter) => ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"Duplicate[{uniqueId}]");
			protected override bool IsValid(TestRefModel refModel, string chapterFilter)
			{
				if (!refModel.IsValid)
				{
					ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"IsValid[{refModel.UniqueId}]");
				}

				return refModel.IsValid;
			}
			protected override bool IsExpired(TestRefModel refModel) => refModel.EndDate.Date < dateTimeProvider.UTCDateTime.Date.AddYears(-1);

			protected override string UniqueId(TestRefModel refModel) => refModel.UniqueId;
			protected override XmlWriterConfiguration XmlWriterConfiguration()
			{
				var writerConfig = new XmlWriterConfiguration();
				var entityConfig = new EntityTypeConfiguration<TestRefModel>(true);

				entityConfig.IncludeColumn(x => x.UniqueId, true);

				writerConfig.IncludeEntityTypeConfiguration(entityConfig);

				return writerConfig;
			}

			public new string OutputFileName => base.OutputFileName;
			public void SimulateFileWrite() => FileNumber++;
		}

		public class GoodsNomenclatureProcessorTester : GoodsNomenclatureProcessor
		{
			public GoodsNomenclatureProcessorTester() : this(new Common.Tests.CommonHelpers.DateTimeProvider(), new IRefXmlBuilder[] { new VirtualBuilder() }) { }
			public GoodsNomenclatureProcessorTester(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders)
			{
				TestBuilders = builders.OfType<VirtualBuilder>().ToArray();
			}

			public VirtualBuilder[] TestBuilders { get; private set; }
			public new List<ITariffModel> Models { get { return base.Models; } set { base.Models = value; } }

			public void SimulateProcessing(string chapterFilter, List<FileDetails> files, StringBuilder errorCollector, string outputFolder)
			{
				LoadData(chapterFilter, files, errorCollector);
				UpdateModels(chapterFilter, null, errorCollector);
				ProcessChapter(chapterFilter, outputFolder);
			}

			public Action<string, List<GoodsNomenclature>> TrackingAction { get; set; }
			protected override void ModelTracking(string action, List<GoodsNomenclature> models)
			{
				TrackingAction?.Invoke(action, models);
			}
		}

		public class MeasureProcessorTester : MeasureProcessor
		{
			public MeasureProcessorTester() : this(new Common.Tests.CommonHelpers.DateTimeProvider(), new MeasureMappingTestDataProvider(), new IRefXmlBuilder[] { new VirtualBuilder() }) {}
			public MeasureProcessorTester(MeasureMappingTestDataProvider measureMappingTestDataProvider) : this(new Common.Tests.CommonHelpers.DateTimeProvider(), measureMappingTestDataProvider, new IRefXmlBuilder[] { new VirtualBuilder() }) { }
			public MeasureProcessorTester(MeasureMappingTestDataProvider measureMappingTestDataProvider, bool processConfiguredMeasureTypesOnly) : this(new Common.Tests.CommonHelpers.DateTimeProvider(), measureMappingTestDataProvider, new IRefXmlBuilder[] { new VirtualBuilder() })
			{
				this.processConfiguredMeasureTypesOnly = processConfiguredMeasureTypesOnly;
			}
			public MeasureProcessorTester(IDateTimeProvider dateTimeProvider, IMeasureMappingProvider measureMappingProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, measureMappingProvider, builders)
			{
				TestBuilders = builders.OfType<VirtualBuilder>().ToArray();
			}

			protected override bool ProcessConfiguredMeasureTypesOnly => processConfiguredMeasureTypesOnly;
			bool processConfiguredMeasureTypesOnly;

			public VirtualBuilder[] TestBuilders { get; private set; }
			public new List<ITariffModel> Models { get { return base.Models; } set { base.Models = value; } }

			public void SimulateProcessing(string chapterFilter, List<FileDetails> files, StringBuilder errorCollector, string outputFolder)
			{
				var refData = TestReferenceData ?? TestData.CreateReferenceData();
				LoadData(chapterFilter, files, errorCollector);
				UpdateModels(chapterFilter, refData, errorCollector);
				ProcessChapter(chapterFilter, outputFolder);
			}

			public List<ITariffModel> TestReferenceData { get; set; }

			public Action<string, List<Measure>> TrackingAction { get; set; }
			protected override void ModelTracking(string action, List<Measure> models)
			{
				TrackingAction?.Invoke(action, models);
			}
		}

		public class RegulationHelperTester : RegulationHelper
		{
			public RegulationHelperTester(List<ITariffModel> referenceData, IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(referenceData, dateTimeProvider, errorCollector)
			{
			}

			public new Dictionary<string, BaseRegulation> BaseRegulations => base.BaseRegulations;
			public new Dictionary<string, ModificationRegulation> ModificationRegulations => base.ModificationRegulations;
		}

		public static class TestData
		{
			public static List<ITariffModel> CreateReferenceData()
			{
				var data = new List<ITariffModel>
				{
					new GoodsNomenclature { ItemId = "0811109010", ProductLineSuffix = "80", Level = 1, Description = "Measure 001", Key = "03.01.02.03", IsForMeasure = true },
					new GoodsNomenclature { ItemId = "2105001000", ProductLineSuffix = "80", Level = 1, Description = "Measure 002", IsForMeasure = true },
					new GoodsNomenclature { ItemId = "17029099XX", ProductLineSuffix = "80", Level = 1, Description = "Measure 003", IsForMeasure = true },
					new GoodsNomenclature { ItemId = "0711510000", ProductLineSuffix = "80", Level = 1, Description = "Measure 004", IsForMeasure = true },
					new GoodsNomenclature { ItemId = "0205000000", ProductLineSuffix = "80", Level = 1, Description = "Measure 005 - Non-CommodityCode", IsForMeasure = true },
					new GoodsNomenclature { ItemId = "3921905590", ProductLineSuffix = "80", Level = 1, Description = "Test", IsForMeasure = true },

					new BaseRegulation { RegulationId = "R9307300", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },
					new BaseRegulation { RegulationId = "R9523120", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },
					new ModificationRegulation { RegulationId = "C2002001", RegulationRoleTypeId = "4", StartDate = new DateTime(2021, 01, 01) },
					new ModificationRegulation { RegulationId = "D1002240", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },
					new ModificationRegulation { RegulationId = "ENDED", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01), EffectiveEndDate = new DateTime(2020, 01, 02) },

					new MeasureType { Id = "690", Description = "UT Measure Type 690", TradeMovementCode = "2", MeasureTypeSeries = "J" },
					new MeasureType { Id = "570", Description = "UT Measure Type 570", TradeMovementCode = "1", MeasureTypeSeries = "D" },
					new MeasureType { Id = "110", Description = "UT Measure Type 110", TradeMovementCode = "0", MeasureTypeSeries = "O" },

					new MeasureConditionCode { Id = "B", Description = "UT Measure Condition Code B" },

					new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
				};

				return data;
			}
		}

		public sealed class MeasureMappingTestDataProvider : IMeasureMappingProvider
		{
			public Dictionary<string, MeasureTypeMapping> MeasureTypeMappingsForTest { get; set; } = new Dictionary<string, MeasureTypeMapping>();
			Dictionary<string, MeasureTypeMapping> IMeasureMappingProvider.GetMeasureTypeMappings() => MeasureTypeMappingsForTest;

			public List<string> PreferencesForTest { get; set; }
			IEnumerable<string> IMeasureMappingProvider.ConvertPreferences(IEnumerable<string> defaultPreferences, string geographicalArea, IEnumerable<string> footnotes) => PreferencesForTest == null ? defaultPreferences : PreferencesForTest.Concat(defaultPreferences);

			public string RateCodeForTest { get; set; }
			string IMeasureMappingProvider.ConvertRateCode(string defaultRateCode, Measure measure) => RateCodeForTest == null ? defaultRateCode : $"{RateCodeForTest}-{defaultRateCode}";

			public string VatCodeForTest { get; set; } = string.Empty;
			string IMeasureMappingProvider.GetVatCode(Measure measure) => VatCodeForTest;
		}

		public static class ObjectComparison
		{
			// Code below sourced from https://www.cyotek.com/blog/comparing-the-properties-of-two-objects-via-reflection
			public static bool AreObjectsEqual(object objectA, object objectB, params string[] ignoreList)
			{
				bool result;

				if (objectA != null && objectB != null)
				{
					Type objectType;

					objectType = objectA.GetType();

					result = true; // assume by default they are equal

					foreach (PropertyInfo propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && !ignoreList.Contains(p.Name)))
					{
						object valueA;
						object valueB;

						valueA = propertyInfo.GetValue(objectA, null);
						valueB = propertyInfo.GetValue(objectB, null);

						// if it is a primative type, value type or implements IComparable, just directly try and compare the value
						if (CanDirectlyCompare(propertyInfo.PropertyType))
						{
							if (valueA == null)
							{
								Console.WriteLine("Source property is null '{0}.{1}'.", objectType.FullName, propertyInfo.Name);
								result = false;
							}
							else if (!AreValuesEqual(valueA, valueB))
							{
								Console.WriteLine("Mismatch with property '{0}.{1}' found.", objectType.FullName, propertyInfo.Name);
								result = false;
							}
						}
						// if it implements IEnumerable, then scan any items
						else if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
						{
							IEnumerable<object> collectionItems1;
							IEnumerable<object> collectionItems2;
							int collectionItemsCount1;
							int collectionItemsCount2;

							// null check
							if (valueA == null && valueB != null || valueA != null && valueB == null)
							{
								Console.WriteLine("Mismatch with property '{0}.{1}' found.", objectType.FullName, propertyInfo.Name);
								result = false;
							}
							else if (valueA != null && valueB != null)
							{
								collectionItems1 = ((IEnumerable)valueA).Cast<object>();
								collectionItems2 = ((IEnumerable)valueB).Cast<object>();
								collectionItemsCount1 = collectionItems1.Count();
								collectionItemsCount2 = collectionItems2.Count();

								// check the counts to ensure they match
								if (collectionItemsCount1 != collectionItemsCount2)
								{
									Console.WriteLine("Collection counts for property '{0}.{1}' do not match.", objectType.FullName, propertyInfo.Name);
									result = false;
								}
								// and if they do, compare each item... this assumes both collections have the same order
								else
								{
									for (int i = 0; i < collectionItemsCount1; i++)
									{
										object collectionItem1;
										object collectionItem2;
										Type collectionItemType;

										collectionItem1 = collectionItems1.ElementAt(i);
										collectionItem2 = collectionItems2.ElementAt(i);
										collectionItemType = collectionItem1.GetType();

										if (CanDirectlyCompare(collectionItemType))
										{
											if (collectionItem1 == null)
											{
												Console.WriteLine("Source property is null '{0}.{1}'.", objectType.FullName, propertyInfo.Name);
												result = false;
											}
											else if (!AreValuesEqual(collectionItem1, collectionItem2))
											{
												Console.WriteLine("Item {0} in property collection '{1}.{2}' does not match.", i, objectType.FullName, propertyInfo.Name);
												result = false;
											}
										}
										else if (!AreObjectsEqual(collectionItem1, collectionItem2, ignoreList))
										{
											Console.WriteLine("Item {0} in property collection '{1}.{2}' does not match.", i, objectType.FullName, propertyInfo.Name);
											result = false;
										}
									}
								}
							}
						}
						else if (propertyInfo.PropertyType.IsClass)
						{
							if (!AreObjectsEqual(propertyInfo.GetValue(objectA, null), propertyInfo.GetValue(objectB, null), ignoreList))
							{
								Console.WriteLine("Mismatch with property '{0}.{1}' found.", objectType.FullName, propertyInfo.Name);
								result = false;
							}
						}
						else
						{
							Console.WriteLine("Cannot compare property '{0}.{1}'.", objectType.FullName, propertyInfo.Name);
							result = false;
						}
					}
				}
				else
				{
					result = object.Equals(objectA, objectB);
				}

				return result;
			}

			/// <summary>
			/// Determines whether value instances of the specified type can be directly compared.
			/// </summary>
			/// <param name="type">The type.</param>
			/// <returns>
			/// 	<c>true</c> if this value instances of the specified type can be directly compared; otherwise, <c>false</c>.
			/// </returns>
			static bool CanDirectlyCompare(Type type)
			{
				return typeof(IComparable).IsAssignableFrom(type) || type.IsPrimitive || type.IsValueType;
			}

			/// <summary>
			/// Compares two values and returns if they are the same.
			/// </summary>
			/// <param name="valueA">The first value to compare.</param>
			/// <param name="valueB">The second value to compare.</param>
			/// <returns><c>true</c> if both values match, otherwise <c>false</c>.</returns>
			static bool AreValuesEqual(object valueA, object valueB)
			{
				bool result;
				IComparable selfValueComparer;

				selfValueComparer = valueA as IComparable;

				if (valueA == null && valueB != null || valueA != null && valueB == null)
				{
					result = false; // one of the values is null
				}
				else if (selfValueComparer != null && selfValueComparer.CompareTo(valueB) != 0)
				{
					result = false; // the comparison using IComparable failed
				}
				else if (!object.Equals(valueA, valueB))
				{
					result = false; // the comparison using Equals failed
				}
				else
				{
					result = true; // match
				}

				return result;
			}
		}
	}
}
