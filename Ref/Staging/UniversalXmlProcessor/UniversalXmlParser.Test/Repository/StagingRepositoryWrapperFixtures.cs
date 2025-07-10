using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using CargoWise.RefDbRepo.UniversalXmlParser.Providers;
using CargoWise.RefDbRepo.UniversalXmlParser.Repository;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using Common.Logging;
using Moq;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Repository
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public class StagingRepositoryWrapperFixtures : BaseUnitTestFixture
	{
		const string RefCusTariffTestFile = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.RefCusTariff.xml";
		const string RefCusTariffSingleLine = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.RefCusTariffSingleLine.xml";
		const string RefStlScript = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.RefStlScript.xml";

		readonly Mock<IStagingRepository> stagingRepositoryMock = new Mock<IStagingRepository>();
		readonly Mock<IEntityValuesProvider> valuesProviderMock = new Mock<IEntityValuesProvider>();
		readonly Mock<ILog> loggerMock = new Mock<ILog>();

		protected override void OnSetup()
		{
			base.OnSetup();

			Container.RegisterInstance(stagingRepositoryMock.Object);
			Container.RegisterInstance(valuesProviderMock.Object);
			Container.RegisterInstance(loggerMock.Object);
		}

		[Test]
		public async Task BulkInsertRecordsInCorrectOrder()
		{
			var entityTypeHelper = Container.Resolve<IEntityTypeHelper>();

			var dataProcessingInformationExpectedPosition = 5;
			var refCusTariffExpectedPosition = 1;
			var refCusExcludedTradeGroupExpectedPosition = 4;
			var refCusRateExpectedPosition = 2;
			var refCusApplicabilityExpectedPosition = 3;

			var currentPosition = 1;
			var stagingRepositoryMockLocal = new Mock<IStagingRepository>();
			Container.RegisterInstance(stagingRepositoryMockLocal.Object);

			stagingRepositoryMockLocal.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<DataProcessingInformation>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<DataProcessingInformation> dpiRecords, int? timeout) =>
				{
					Assert.That(currentPosition, Is.EqualTo(dataProcessingInformationExpectedPosition));
					currentPosition++;
				});

			stagingRepositoryMockLocal.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusTariff>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusTariff> tariffs, int? timeout) =>
				{
					Assert.That(currentPosition, Is.EqualTo(refCusTariffExpectedPosition));
					currentPosition++;
				});

			stagingRepositoryMockLocal.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusRate>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusRate> records, int? timeout) =>
				{
					Assert.That(currentPosition, Is.EqualTo(refCusRateExpectedPosition));
					currentPosition++;
				});

			stagingRepositoryMockLocal.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusApplicability>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusApplicability> records, int? timeout) =>
				{
					Assert.That(currentPosition, Is.EqualTo(refCusApplicabilityExpectedPosition));
					currentPosition++;
				});

			stagingRepositoryMockLocal.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusExcludedTradeGroup>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusExcludedTradeGroup> records, int? timeout) =>
				{
					Assert.That(currentPosition, Is.EqualTo(refCusExcludedTradeGroupExpectedPosition));
					currentPosition++;
				});

			var stagingRepoWrapper = Container.Resolve<IStagingRepositoryWrapper>();
			var tariffSchemaMap = new XmlSchemaEntityRelationshipMap("RefCusTariff");
			tariffSchemaMap.ChildEntityList = new List<string>() { "RefCusRate" };
			var rateSchemaMap = new XmlSchemaEntityRelationshipMap("RefCusRate");
			rateSchemaMap.ChildEntityList = new List<string>() { "RefCusApplicability" };
			var applicabilitySchemaMap = new XmlSchemaEntityRelationshipMap("RefCusApplicability");
			applicabilitySchemaMap.ChildEntityList = new List<string> { "RefCusExcludedTradeGroup" };
			var excludedTradeGroupSchemaMap = new XmlSchemaEntityRelationshipMap("RefCusExcludedTradeGroup");
			stagingRepoWrapper.SchemaRelationshipMapList = new List<XmlSchemaEntityRelationshipMap> { tariffSchemaMap, rateSchemaMap, applicabilitySchemaMap };
			var entityTypeMetaDataCollection = new List<EntityTypeMetaData>
			{
				new EntityTypeMetaData
				{
					Name = nameof(RefCusTariff),
					ConstantValues = new Dictionary<string, string>
					{
						{ nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping), "EUN" }
					}
				},
				new EntityTypeMetaData
				{
					Name = nameof(RefCusRate),
					ConstantValues = new Dictionary<string, string>
					{
						{ nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping), "EUN" }
					}
				},
				new EntityTypeMetaData
				{
					Name = nameof(RefCusApplicability),
					DefaultValues = new Dictionary<string, string>
					{
						{ nameof(RefCusApplicability.ZZT_EndDate), new DateTime(2079, 06, 06, 23, 59, 00).ToString("s") }
					}
				}
			};

			stagingRepoWrapper.SetEntityTypeMetaData(entityTypeMetaDataCollection);

			foreach (var filename in new[] { RefCusTariffTestFile, RefCusTariffSingleLine })
			{
				var sourceId = Guid.NewGuid();
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename))
				{
					if (stream != null)
					{
						using (var streamReader = new StreamReader(stream, Encoding.UTF8))
						using (var xmlReader = XmlReader.Create(streamReader))
						{
							var xmlInfo = (IXmlLineInfo)xmlReader;
							xmlReader.MoveToContent();

							var entityTypeName = xmlReader.Name;
							var testData = xmlReader.ReadOuterXml();

							var result = stagingRepoWrapper.CreateRecord(entityTypeName, testData, xmlInfo.LineNumber, sourceId);

							Assert.IsTrue(string.IsNullOrEmpty(result));
							Assert.IsNotNull(testData);
						}
					}
				}
			}

			await stagingRepoWrapper.ExecuteBulkInsertAsync();
		}

		[Test]
		public async Task BulkInsert_HasLogs()
		{
			var entityTypeHelper = Container.Resolve<IEntityTypeHelper>();
			var stagingRepositoryMockLocal = new Mock<IStagingRepository>();
			Container.RegisterInstance(stagingRepositoryMockLocal.Object);

			var stagingRepoWrapper = Container.Resolve<IStagingRepositoryWrapper>();
			var refStlScriptSchemaMap = new XmlSchemaEntityRelationshipMap("RefStlScript");
			stagingRepoWrapper.SchemaRelationshipMapList = new List<XmlSchemaEntityRelationshipMap> { refStlScriptSchemaMap };

			var sourceId = Guid.NewGuid();
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RefStlScript))
			{
				if (stream != null)
				{
					using (var streamReader = new StreamReader(stream, Encoding.UTF8))
					using (var xmlReader = XmlReader.Create(streamReader))
					{
						var xmlInfo = (IXmlLineInfo)xmlReader;
						xmlReader.MoveToContent();

						var entityTypeName = xmlReader.Name;
						var testData = xmlReader.ReadOuterXml();

						var result = stagingRepoWrapper.CreateRecord(entityTypeName, testData, xmlInfo.LineNumber, sourceId);

						Assert.IsTrue(string.IsNullOrEmpty(result));
						Assert.IsNotNull(testData);
					}
				}
			}

			var originalOut = Console.Out;
			try
			{
				using var sw = new StringWriter();
				Console.SetOut(sw);
				await stagingRepoWrapper.ExecuteBulkInsertAsync();
				Assert.That(sw.ToString(), Is.EqualTo("BulkInsert succeeded in StagingDB. RefStlScript: 1 records inserted. DataProcessingInformation: 1 records inserted.\r\n"));
			}
			finally
			{
				Console.SetOut(originalOut);
			}
		}

		[Test]
		public async Task BulkInsert_DoNotBeginTransactionBeforeRetry()
		{
			var stagingRepositoryMockLocal = new Mock<IStagingRepository>();
			Container.RegisterInstance(stagingRepositoryMockLocal.Object);
			var stagingRepoWrapper = Container.Resolve<IStagingRepositoryWrapper>();

			await stagingRepoWrapper.ExecuteBulkInsertAsync();

			stagingRepositoryMockLocal.Verify(x => x.BeginTransaction(), Times.Never, "Do not BeginTransaction and use the same transaction to retry BulkInsert.");
		}

		[Test]
		public async Task TestCreateRecordAsync()
		{
			var entityTypeHelper = Container.Resolve<IEntityTypeHelper>();

			DataProcessingInformation[] dataProcessingInformations = null;
			RefCusTariff[] tariffRecordList = null;
			RefCusRate[] cusRates = null;
			RefCusApplicability[] applicabilities = null;

			stagingRepositoryMock.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<DataProcessingInformation>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<DataProcessingInformation> dpiRecords, int? timeout) =>
				{
					var dpiList = dpiRecords as IList<DataProcessingInformation> ?? dpiRecords.ToList();
					if (dpiList.Any())
					{
						dataProcessingInformations = new DataProcessingInformation[dpiList.Count];
						dpiList.CopyTo(dataProcessingInformations, 0);
					}
				});

			stagingRepositoryMock.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusTariff>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusTariff> tariffs, int? timeout) =>
				{
					var list = tariffs as IList<RefCusTariff> ?? tariffs.ToList();
					if (list.Any())
					{
						tariffRecordList = new RefCusTariff[list.Count];
						list.CopyTo(tariffRecordList, 0);
					}
				});

			stagingRepositoryMock.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusRate>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusRate> records, int? timeout) =>
				{
					var list = records as IList<RefCusRate> ?? records.ToList();
					if (list.Any())
					{
						cusRates = new RefCusRate[list.Count];
						list.CopyTo(cusRates, 0);
					}
				});

			stagingRepositoryMock.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<RefCusApplicability>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<RefCusApplicability> records, int? timeout) =>
				{
					var list = records as IList<RefCusApplicability> ?? records.ToList();
					if (list.Any())
					{
						applicabilities = new RefCusApplicability[list.Count];
						list.CopyTo(applicabilities, 0);
					}
				});

			var stagingRepoWrapper = Container.Resolve<IStagingRepositoryWrapper>();
			var tariffSchemaMap = new XmlSchemaEntityRelationshipMap("RefCusTariff");
			tariffSchemaMap.ChildEntityList = new List<string>() { "RefCusRate" };
			var rateSchemaMap = new XmlSchemaEntityRelationshipMap("RefCusRate");
			rateSchemaMap.ChildEntityList = new List<string>() { "RefCusApplicability" };
			var applicabilitySchemaMap = new XmlSchemaEntityRelationshipMap("RefCusApplicability");
			stagingRepoWrapper.SchemaRelationshipMapList = new List<XmlSchemaEntityRelationshipMap> { tariffSchemaMap, rateSchemaMap, applicabilitySchemaMap };
			var entityTypeMetaDataCollection = new List<EntityTypeMetaData>
			{
				new EntityTypeMetaData
				{
					Name = nameof(RefCusTariff),
					ConstantValues = new Dictionary<string, string>
					{
						{ nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping), "EUN" }
					}
				},
				new EntityTypeMetaData
				{
					Name = nameof(RefCusRate),
					ConstantValues = new Dictionary<string, string>
					{
						{ nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping), "EUN" }
					}
				},
				new EntityTypeMetaData
				{
					Name = nameof(RefCusApplicability),
					DefaultValues = new Dictionary<string, string>
					{
						{ nameof(RefCusApplicability.ZZT_EndDate), new DateTime(2079, 06, 06, 23, 59, 00).ToString("s") }
					}
				}
			};

			stagingRepoWrapper.SetEntityTypeMetaData(entityTypeMetaDataCollection);

			foreach (var filename in new[] { RefCusTariffTestFile, RefCusTariffSingleLine })
			{
				var sourceId = Guid.NewGuid();
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename))
				{
					if (stream != null)
					{
						using (var streamReader = new StreamReader(stream, Encoding.UTF8))
						using (var xmlReader = XmlReader.Create(streamReader))
						{
							var xmlInfo = (IXmlLineInfo)xmlReader;
							xmlReader.MoveToContent();

							var entityTypeName = xmlReader.Name;
							var testData = xmlReader.ReadOuterXml();

							var result = stagingRepoWrapper.CreateRecord(entityTypeName, testData, xmlInfo.LineNumber, sourceId);

							Assert.IsTrue(string.IsNullOrEmpty(result));
							Assert.IsNotNull(testData);
						}
					}
				}
			}

			await stagingRepoWrapper.ExecuteBulkInsertAsync();

			Assert.IsNotNull(dataProcessingInformations);
			Assert.IsTrue(dataProcessingInformations.Any());
			Assert.AreEqual(2, dataProcessingInformations.Length);

			var dataProcessingInfo = dataProcessingInformations[0];
			Assert.IsNotNull(dataProcessingInfo);

			Assert.AreEqual("QUE", dataProcessingInfo.DPI_Status);
			Assert.AreEqual(entityTypeHelper.GetTableCode(typeof(RefCusTariff)), dataProcessingInfo.DPI_ParentTableCode);

			Assert.IsNotNull(tariffRecordList);
			Assert.IsTrue(tariffRecordList.Any());
			Assert.AreEqual(2, tariffRecordList.Length);

			var refCusTariff = tariffRecordList[0];
			Assert.IsNotNull(refCusTariff);
			Assert.AreNotEqual(Guid.Empty, refCusTariff.ZZ1_PK);
			Assert.AreEqual(refCusTariff.ZZ1_PK, dataProcessingInfo.DPI_ParentPk);
			Assert.AreEqual("EUN", refCusTariff.ZZ1_ZZZ_NKDataGrouping);

			Assert.IsNotNull(cusRates);
			Assert.AreEqual(48, cusRates.Length);
			Assert.IsTrue(cusRates.All(x => x.ZZ2_ZZZ_NKDataGrouping == "EUN"));

			foreach (var refCusRate in cusRates)
			{
				Assert.IsNotNull(refCusRate);
				Assert.AreNotEqual(Guid.Empty, refCusRate.ZZ2_PK);
				Assert.IsTrue(tariffRecordList.Any(x => x.ZZ1_PK == refCusRate.ZZ2_ZZ1_Tariff));
			}

			Assert.IsTrue(cusRates.FirstOrDefault(x => x.ZZ2_RateFormula == @"Escaped Characters AAA < BBB > CCC & DDD "" EEE ' FFF") != null);
			Assert.IsTrue(cusRates.FirstOrDefault(x => x.ZZ2_RateFormula == "Non Escaped Characters AAA * 100") != null);

			Assert.IsNotNull(applicabilities);
			Assert.AreEqual(48, applicabilities.Length);
			foreach (var applicability in applicabilities)
			{
				Assert.IsNotNull(applicability);
				Assert.AreNotEqual(Guid.Empty, applicability.ZZT_PK);
				Assert.IsTrue(cusRates.Any(x => x.ZZ2_PK == applicability.ZZT_ZZ2_Rate));
			}
			Assert.IsTrue(applicabilities.All(x => x.ZZT_EndDate == new DateTime(2079, 06, 06, 23, 59, 00)));

			stagingRepositoryMock.VerifyAll();
		}

		[Test]
		public async Task TestErrorXMLReportingToDPI()
		{
			DataProcessingInformation[] dataProcessingInformations = null;

			stagingRepositoryMock.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<DataProcessingInformation>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<DataProcessingInformation> dpiRecords, int? timeout) =>
				{
					var dpiList = dpiRecords as IList<DataProcessingInformation> ?? dpiRecords.ToList();
					if (dpiList.Any())
					{
						dataProcessingInformations = new DataProcessingInformation[dpiList.Count];
						dpiList.CopyTo(dataProcessingInformations, 0);
					}
				});

			var stagingRepoWrapper = Container.Resolve<IStagingRepositoryWrapper>();
			stagingRepoWrapper.SchemaRelationshipMapList = new List<XmlSchemaEntityRelationshipMap> { new XmlSchemaEntityRelationshipMap("RefCusTariff") };
			var sourceId = Guid.NewGuid();
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RefCusTariffTestFile))
			{
				if (stream != null)
				{
					using (var streamReader = new StreamReader(stream, Encoding.UTF8))
					using (var xmlReader = XmlReader.Create(streamReader))
					{
						var xmlInfo = (IXmlLineInfo)xmlReader;
						xmlReader.MoveToContent();

						var entityTypeName = xmlReader.Name;
						var testData = xmlReader.ReadOuterXml();

						var result = stagingRepoWrapper.CreateRecord(entityTypeName, testData, xmlInfo.LineNumber, sourceId);

						Assert.IsTrue(string.IsNullOrEmpty(result));
						Assert.IsNotNull(testData);
					}
				}
			}

			await stagingRepoWrapper.ExecuteBulkInsertAsync();

			var dataProcessingInfo = dataProcessingInformations[0];
			Assert.IsNotNull(dataProcessingInfo);
			Assert.AreEqual("ERR", dataProcessingInfo.DPI_Status);
			Assert.AreEqual("Schema and content structure does not match: relationship between RefCusRate and RefCusTariff is either not specified or entity is not defined.", dataProcessingInfo.DPI_Message);

			stagingRepositoryMock.VerifyAll();
		}

		[Test]
		public async Task CreateEmptyInstanceAsErrorOnCreateRecordCore()
		{
			DataProcessingInformation[] dataProcessingInformations = null;

			stagingRepositoryMock.Setup(x => x.BulkInsertWithRetryAsync(It.IsAny<IEnumerable<DataProcessingInformation>>(), It.IsAny<int?>()))
				.Callback((IEnumerable<DataProcessingInformation> dpiRecords, int? timeout) =>
				{
					var dpiList = dpiRecords as IList<DataProcessingInformation> ?? dpiRecords.ToList();
					if (dpiList.Any())
					{
						dataProcessingInformations = new DataProcessingInformation[dpiList.Count];
						dpiList.CopyTo(dataProcessingInformations, 0);
					}
				});

			var stagingRepoWrapper = Container.Resolve<IStagingRepositoryWrapper>();
			var sourceId = Guid.NewGuid();
			stagingRepoWrapper.SchemaRelationshipMapList = new List<XmlSchemaEntityRelationshipMap> { new XmlSchemaEntityRelationshipMap("RefCusTariff"), new XmlSchemaEntityRelationshipMap("RefCusTariffRelationship") };
			using (var fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.ScheamAndContentDoesNotMatchTestFile.xml"))
			{
				if (fs != null)
				{
					using (var streamReader = new StreamReader(fs, Encoding.UTF8))
					using (var xmlReader = XmlReader.Create(streamReader))
					{
						var xmlInfo = (IXmlLineInfo)xmlReader;

						xmlReader.ReadToDescendant("RefCusTariff");

						var entityTypeName = xmlReader.Name;
						var testData = xmlReader.ReadOuterXml();

						var result = stagingRepoWrapper.CreateRecord(entityTypeName, testData, xmlInfo.LineNumber, sourceId);
						Assert.IsTrue(string.IsNullOrEmpty(result));
						Assert.IsNotNull(testData);
					}
				}
			}

			await stagingRepoWrapper.ExecuteBulkInsertAsync();
			var dpiResult = dataProcessingInformations.FirstOrDefault();
			Assert.That(dpiResult != null);
			Assert.That(dpiResult.DPI_ParentPk, Is.Null);
			Assert.That(dpiResult.IsDataProcessingInformationErrorStatus());
		}

		[Test]
		public void TestGetPrimaryKey_Object_IsNull()
		{
			var entityMissingValueProvider = new Mock<IEntityMissingValuesProvider>();
			var parserConfig = new Mock<IParserConfig>();
			var entityTypeHelper = new EntityTypeHelper(new DummyObsoletePropertiesLookup());
			var stagingRepositoryWrapper = new StagingRepositoryWrapper(stagingRepositoryMock.Object, entityMissingValueProvider.Object, entityTypeHelper, parserConfig.Object);
			Assert.Throws<ArgumentNullException>(() => stagingRepositoryWrapper.GetPrimaryKeyValue(null));
		}

		[Test]
		public void TestGetPrimaryKey_Object_DoesNotHavePrimaryKey()
		{
			var entityMissingValueProvider = new Mock<IEntityMissingValuesProvider>();
			var parserConfig = new Mock<IParserConfig>();
			var entityTypeHelper = new EntityTypeHelper(new DummyObsoletePropertiesLookup());
			var stagingRepositoryWrapper = new StagingRepositoryWrapper(stagingRepositoryMock.Object, entityMissingValueProvider.Object, entityTypeHelper, parserConfig.Object);
			Assert.That(stagingRepositoryWrapper.GetPrimaryKeyValue(new DummyEntity()), Is.Null);
		}

		[Test]
		public void TestGetPrimaryKey_Object_PrimaryKeyValue_IsNull()
		{
			var entityMissingValueProvider = new Mock<IEntityMissingValuesProvider>();
			var parserConfig = new Mock<IParserConfig>();
			var entityTypeHelper = new EntityTypeHelper(new DummyObsoletePropertiesLookup());
			var stagingRepositoryWrapper = new StagingRepositoryWrapper(stagingRepositoryMock.Object, entityMissingValueProvider.Object, entityTypeHelper, parserConfig.Object);
			Assert.That(stagingRepositoryWrapper.GetPrimaryKeyValue(new DummyEntity { Entity_PK = null }), Is.Null);
		}

		[Test]
		public void TestGetPrimaryKey_Object_PrimaryKeyValue_IsInvalidGuid()
		{
			var entityMissingValueProvider = new Mock<IEntityMissingValuesProvider>();
			var parserConfig = new Mock<IParserConfig>();
			var entityTypeHelper = new EntityTypeHelper(new DummyObsoletePropertiesLookup());
			var stagingRepositoryWrapper = new StagingRepositoryWrapper(stagingRepositoryMock.Object, entityMissingValueProvider.Object, entityTypeHelper, parserConfig.Object);
			Assert.That(stagingRepositoryWrapper.GetPrimaryKeyValue(new DummyEntity { Entity_PK = "1234" }), Is.Null);
		}

		[Test]
		public void TestGetPrimaryKey_Object_PrimaryKeyValue_IsValidGuid()
		{
			var entityMissingValueProvider = new Mock<IEntityMissingValuesProvider>();
			var parserConfig = new Mock<IParserConfig>();
			var entityTypeHelper = new EntityTypeHelper(new DummyObsoletePropertiesLookup());
			var stagingRepositoryWrapper = new StagingRepositoryWrapper(stagingRepositoryMock.Object, entityMissingValueProvider.Object, entityTypeHelper, parserConfig.Object);
			var primaryKeyValue = Guid.NewGuid();
			Assert.That(stagingRepositoryWrapper.GetPrimaryKeyValue(new DummyEntity { Entity_PK = primaryKeyValue.ToString() }), Is.EqualTo(primaryKeyValue));
		}

		class DummyEntity
		{
			public string Entity_PK { get; set; }
		}
	}
}
