using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;
using Moq;
using NUnit.Framework;
using Unity;
using Unity.Resolution;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Repository;

[TestFixture]
public class SourceDataWriterFixtures : BaseUnitTestFixture
{
	readonly Mock<IStagingRepository> stagingRepositoryMock = new Mock<IStagingRepository>();

	protected override void OnSetup()
	{
		base.OnSetup();

		Container.RegisterInstance(stagingRepositoryMock.Object);
	}

	static SourceData CreateSourceData(string fileName)
	{
		return new SourceData
		{
			SDA_Source = DataSourceConstants.Source.InternalWebsite,
			SDA_Filename = fileName,
			SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
			SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
			SDA_ContentText = string.Empty,
			SDA_Status = StatusProvider.GetQUEStatus(),
			SDA_SourceTime = DateTime.Now,
			SDA_SubSource = string.Empty
		};
	}

	[Test]
	public void TestCreateSourceData()
	{
		SourceData sourceData = null;
		stagingRepositoryMock.Setup(x => x.AddOrUpdate(It.IsAny<SourceData>())).Callback((SourceData data) =>
		{
			sourceData = data;
		});

		stagingRepositoryMock.Setup(x => x.SaveChanges()).Verifiable();

		var testFile = Guid.NewGuid().ToString();
		var sourceDataWriter = Container.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", CreateSourceData(testFile)));

		var fileHash = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";

		var id = sourceDataWriter.AddOrUpdateSourceDataAndGetPK("testSource", DateTime.Now, fileHash);
		Assert.That(sourceData, Is.Not.Null);
		Assert.That(sourceData.SDA_Filename, Is.EqualTo(testFile));
		Assert.That(id, Is.EqualTo(sourceData.SDA_PK));
		Assert.That(sourceData.SDA_FileHash, Is.EqualTo(fileHash));

		stagingRepositoryMock.Verify();
	}

	[Test]
	public void TestAddContent()
	{
		const string key = "key";
		const string value = "value";

		SourceData sourceData = null;
		stagingRepositoryMock.Setup(x => x.AddOrUpdate(It.IsAny<SourceData>())).Callback((SourceData data) =>
		{
			sourceData = data;
		});

		var testFile = Guid.NewGuid().ToString();
		var sourceDataWriter = Container.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", CreateSourceData(testFile)));

		var fileHash = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";
		var id = sourceDataWriter.AddOrUpdateSourceDataAndGetPK("testSource", DateTime.Now, fileHash);
		sourceDataWriter.AddContent(key, value);

		Assert.IsNotNull(sourceData);
		Assert.AreEqual(testFile, sourceData.SDA_Filename);
		Assert.AreEqual(id, sourceData.SDA_PK);
		Assert.AreEqual(fileHash, sourceData.SDA_FileHash);

		var contentText = sourceData.SDA_ContentText;
		Assert.IsTrue(contentText.Contains(Constants.UniversalXml.RootElement));

		var contextXml = XDocument.Parse(contentText);
		Assert.IsNotNull(contextXml);
		Assert.IsNotNull(contextXml.Root);
		Assert.AreEqual(Constants.UniversalXml.RootElement, contextXml.Root.Name.LocalName);
		Assert.IsNotNull(contextXml.Descendants(key));

		for (var index = 1; index <= 5; index++)
		{
			sourceDataWriter.AddContent(key + index, value + index);
		}

		contextXml = XDocument.Parse(sourceData.SDA_ContentText);
		for (var index = 1; index <= 5; index++)
		{
			var node = contextXml.Descendants(key + index).First();
			Assert.IsNotNull(node);
			Assert.AreEqual(value + index, node.Value);
		}
	}

	[Test]
	public void TestAddXmlContent()
	{
		const string xml = @"<Schema>
	<EntityType Name=""RefCusTariff"" data=""true"">
		<Key>
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_IAMUnique"" />
		</Key>
	</EntityType>
</Schema>";

		SourceData sourceData = null;
		stagingRepositoryMock.Setup(x => x.AddOrUpdate(It.IsAny<SourceData>())).Callback((SourceData data) =>
		{
			sourceData = data;
		});

		var testFile = Guid.NewGuid().ToString();
		var sourceDataWriter = Container.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", CreateSourceData(testFile)));
		var fileHash = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";
		var id = sourceDataWriter.AddOrUpdateSourceDataAndGetPK("testSource", DateTime.Now, fileHash);
		sourceDataWriter.AddXmlContent(xml);

		Assert.IsNotNull(sourceData);
		Assert.AreEqual(testFile, sourceData.SDA_Filename);
		Assert.AreEqual(id, sourceData.SDA_PK);
		Assert.AreEqual(fileHash, sourceData.SDA_FileHash);

		var contentText = sourceData.SDA_ContentText;
		Assert.IsTrue(contentText.Contains(Constants.UniversalXml.RootElement));

		var document = XDocument.Parse(sourceData.SDA_ContentText);
		var node = document.Descendants("Schema").First();
		var expectedXml = XDocument.Parse(xml).ToString(SaveOptions.DisableFormatting);

		Assert.IsNotNull(node);
		Assert.AreEqual(expectedXml, node.ToString(SaveOptions.DisableFormatting));
	}

	[Test]
	public void TestSetFlagErrorOnRootLevel()
	{
		SourceData sourceData = null;
		stagingRepositoryMock.Setup(x => x.AddOrUpdate(It.IsAny<SourceData>())).Callback((SourceData data) =>
		{
			sourceData = data;
		});

		var testFile = Guid.NewGuid().ToString();
		var sourceDataWriter = Container.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", CreateSourceData(testFile)));
		var fileHash = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";
		var id = sourceDataWriter.AddOrUpdateSourceDataAndGetPK("testSource", DateTime.Now, fileHash);
		Assert.AreEqual(id, sourceData.SDA_PK);

		sourceDataWriter.SetFlagErrorOnRootLevel();

		Assert.IsNotNull(sourceData);
		Assert.That(sourceData.IsSourceDataErrorStatus());
	}

	[Test]
	public void TestSetFlagAllRecordsAreProcessed()
	{
		SourceData sourceData = null;
		stagingRepositoryMock.Setup(x => x.AddOrUpdate(It.IsAny<SourceData>())).Callback((SourceData data) =>
		{
			sourceData = data;
		});

		var testFile = Guid.NewGuid().ToString();
		var sourceDataWriter = Container.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", CreateSourceData(testFile)));
		var fileHash = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";
		var id = sourceDataWriter.AddOrUpdateSourceDataAndGetPK("testSource", DateTime.Now, fileHash);
		Assert.AreEqual(id, sourceData.SDA_PK);

		sourceDataWriter.SetFlagAllRecordsAreProcessed();

		Assert.IsNotNull(sourceData);
		Assert.AreEqual("PRS", sourceData.SDA_Status);
	}

	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public void TestCheckSourceDataForDuplicate()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		const string sourceName1 = "ABC";
		const string sourceName2 = "DEF";
		var sourceTime = DateTime.Now;

		var fileHashOne = "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9";
		var fileHashTwo = "B3ABE5D8C69B38733AD57EA75E83BCAE42BBBBAC75E3A5445862ED2F8A2CD677";

		var sourceDataOne = new SourceData
		{
			SDA_PK = Guid.NewGuid(),
			SDA_Source = "UPL",
			SDA_SubSource = sourceName1,
			SDA_SourceTime = sourceTime,
			SDA_ContentText = string.Empty,
			SDA_FileHash = fileHashOne,
			SDA_Status = StatusProvider.GetMERStatus(),
			SDA_CreatedTime = DateTime.Now.AddMinutes(-10),
		};

		var sourceDataTwo = new SourceData
		{
			SDA_PK = Guid.NewGuid(),
			SDA_Source = "UPL",
			SDA_SubSource = sourceName1,
			SDA_SourceTime = sourceTime,
			SDA_ContentText = string.Empty,
			SDA_FileHash = fileHashOne,
			SDA_Status = StatusProvider.GetQUEStatus(),
			SDA_CreatedTime = DateTime.Now.AddMinutes(-5),
		};

		var sourceDataThree = new SourceData
		{
			SDA_PK = Guid.NewGuid(),
			SDA_Source = "UPL",
			SDA_SubSource = sourceName1,
			SDA_SourceTime = sourceTime,
			SDA_ContentText = string.Empty,
			SDA_FileHash = fileHashTwo,
			SDA_Status = StatusProvider.GetMERStatus(),
			SDA_CreatedTime = DateTime.Now,
		};

		var sourceDataFour = new SourceData
		{
			SDA_PK = Guid.NewGuid(),
			SDA_Source = "UPL",
			SDA_SubSource = sourceName2,
			SDA_SourceTime = sourceTime,
			SDA_ContentText = string.Empty,
			SDA_FileHash = fileHashTwo,
			SDA_Status = StatusProvider.GetPRSStatus(),
			SDA_CreatedTime = DateTime.Now.AddMinutes(-5),
		};

		using var repo = new StagingRepository(connectionString);
		repo.Add(sourceDataOne);
		repo.Add(sourceDataTwo);
		repo.Add(sourceDataThree);
		repo.SaveChanges();

		Container.RegisterInstance<IStagingRepository>(repo);

		var sourceDataWriter = Container.Resolve<ISourceDataWriter>();

		var isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashOne);
		Assert.That(isDuplicate, Is.False);
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashTwo);
		Assert.That(isDuplicate, Is.True);
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, "invalid-hash");
		Assert.That(isDuplicate, Is.False);

		sourceDataThree.SDA_Status = StatusProvider.GetERRStatus();
		repo.SaveChanges();
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashOne);
		Assert.That(isDuplicate, Is.False);
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashTwo);
		Assert.That(isDuplicate, Is.False);

		sourceDataThree.SDA_Status = StatusProvider.GetFINStatus();
		repo.SaveChanges();
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashOne);
		Assert.That(isDuplicate, Is.False);
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashTwo);
		Assert.That(isDuplicate, Is.True);

		sourceDataThree.SDA_Status = StatusProvider.GetMERStatus();
		repo.SaveChanges();
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashOne);
		Assert.That(isDuplicate, Is.False);
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashTwo);
		Assert.That(isDuplicate, Is.True);

		sourceDataThree.SDA_SubSource = "different-soruce-name";
		repo.SaveChanges();
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashOne);
		Assert.That(isDuplicate, Is.True);
		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName1, fileHashTwo);
		Assert.That(isDuplicate, Is.False);

		isDuplicate = sourceDataWriter.CheckDuplicateExists(sourceName2, fileHashTwo);
		Assert.That(isDuplicate, Is.False);
	}
}
