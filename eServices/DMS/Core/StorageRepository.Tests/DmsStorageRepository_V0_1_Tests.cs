using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace eServices.Dms.Core.StorageRepository.Tests;

[Property("DAT:CapabilityRequirements", "SQL")]
public class DmsStorageRepository_V0_1_Tests
{
	private IConfiguration configuration = null!;
	private DbContextOptions<DmsStorageContext> storageOptions = null!;
	private DmsStorageContext storageContext = null!;
	private DmsStorageRepository_V0_1 storageRepository = null!;

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		configuration = new ConfigurationBuilder()
			.AddJsonFile(@"appSettings.json", false, false)
			.Build();

		storageOptions = new DbContextOptionsBuilder<DmsStorageContext>()
			.UseSqlServer(configuration.GetConnectionString("DmsStorage")
				?? throw new InvalidOperationException("Missing connection string: DmsStorage"))
			.Options;
		using var setupContext = new DmsStorageContext(storageOptions);
#if !DEBUG
		await setupContext.Database.EnsureDeletedAsync();
#endif
		await setupContext.Database.EnsureCreatedAsync();

		await setupContext.Database.ExecuteSqlRawAsync("""
			IF NOT EXISTS(SELECT * FROM [sys].[database_principals] WHERE [name] = 'APP')
				EXEC('CREATE USER [APP] WITHOUT LOGIN')
			""");
		await setupContext.Database.ExecuteSqlRawAsync("""
			IF NOT EXISTS(SELECT * FROM [sys].[schemas] WHERE [name] = 'APP')
				EXEC('CREATE SCHEMA [APP] AUTHORIZATION [APP]')
			""");
		await setupContext.Database.ExecuteSqlRawAsync("ALTER USER [APP] WITH DEFAULT_SCHEMA=[APP]");
	}

#if !DEBUG
	[OneTimeTearDown]
	public async Task OneTimeTearDown()
	{
		using var tearDownContext = new DmsStorageContext(storageOptions);
		await tearDownContext.Database.EnsureDeletedAsync();
	}
#endif

	[SetUp]
	public async Task Setup()
	{
		storageContext = new DmsStorageContext(storageOptions);
		storageRepository = new DmsStorageRepository_V0_1(storageContext);
		await storageRepository.BeginTransactionAsync();
	}

	[TearDown]
	public async Task TearDown()
	{
		if (storageRepository is not null)
			await storageRepository.DisposeAsync();
		if (storageContext is not null)
			await storageContext.DisposeAsync();
	}

	[Test]
	public async Task RegisterOwnerAsyncTest()
	{
		await storageRepository.RegisterOwnerAsync("TST");

		Assert.That(await storageContext.Database.SqlQueryRaw<string>(
			"SELECT [name] AS [Value] FROM [sys].[database_principals]").ToListAsync(), Contains.Item("TST"));
		Assert.That(await storageContext.Database.SqlQueryRaw<string>(
			"SELECT [name] AS [Value] FROM [sys].[schemas]").ToListAsync(), Contains.Item("TST"));
	}

	[Test]
	public async Task CreateAndDeleteTableAsyncTest()
	{
		var created = await storageRepository.CreateTableAsync(new DmsStorageCatalog
		{
			SC_Schema = "APP",
			SC_Name = "Test",
			SC_Type = DmsTableTypes.Document.ToString(),
			SC_Version = "0.1"
		});

		storageContext.ChangeTracker.Clear();
		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await storageContext.DmsStorageCatalog.SingleOrDefaultAsync(c
				=> c.SC_Schema == "APP"
				&& c.SC_Name == "Test"
				&& c.SC_Type == DmsTableTypes.Document.ToString()
				&& c.SC_Version == "0.1"
				&& c.SC_DeleteTime == null), Is.Not.Null);
			Assert.That(await storageRepository.TableExistsAsync("APP", "Test"), Is.True);
			Assert.That(await storageContext.Database.SqlQueryRaw<string>(
				"SELECT CONCAT(SCHEMA_NAME([schema_id]),'.',[name]) AS [Value] FROM [sys].[tables]").ToListAsync(),
				Does.Contain("APP.Test"));
		});

		var deleted = await storageRepository.DeleteTableAsync(new DmsStorageCatalog
		{
			SC_Schema = "APP",
			SC_Name = "Test",
			SC_Type = DmsTableTypes.Document.ToString(),
			SC_Version = "0.1",
			SC_DeleteTime = DateTime.UtcNow,
			SC_DeleteUser = "Me"
		});

		storageContext.ChangeTracker.Clear();
		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await storageContext.DmsStorageCatalog.SingleOrDefaultAsync(c
				=> c.SC_Schema == "APP"
				&& c.SC_Name == "Test"
				&& c.SC_Type == DmsTableTypes.Document.ToString()
				&& c.SC_Version == "0.1"
				&& c.SC_DeleteTime != null), Is.Not.Null);
			Assert.That(await storageRepository.TableExistsAsync("APP", "Test"), Is.False);
			Assert.That(await storageContext.Database.SqlQueryRaw<string>(
				"SELECT CONCAT(SCHEMA_NAME([schema_id]),'.',[name]) AS [Value] FROM [sys].[tables]").ToListAsync(),
				Does.Not.Contain("APP.Test"));
		});
	}

	[Test()]
	public async Task PutAndGetDocumentAsJsonTest()
	{
		await storageRepository.CreateTableAsync(
			new DmsStorageCatalog { SC_Schema = "APP", SC_Name = "Test", SC_Type = DmsTableTypes.Document.ToString(), SC_Version = "0.1" });

		var document = new JsonObject { ["Test"] = true };
		await storageRepository.PutDocumentAsJsonAsync("APP", "Test", "Key", document, "Me");
		var result = await storageRepository.GetDocumentAsJsonAsync("APP", "Test", "Key");

		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await storageContext.Database.SqlQueryRaw<DmsStorageTable>("SELECT * From APP.Test").ToListAsync(), Has.Count.EqualTo(1));
			Assert.That(result, Is.Not.Null);
			Assert.That(JsonNode.DeepEquals(document, result), Is.True);
			Assert.That(result?["Test"]?.GetValue<bool>(), Is.True);
		});

		document["Test"] = false;
		await storageRepository.PutDocumentAsJsonAsync("APP", "Test", "Key", document, "Me");
		result = await storageRepository.GetDocumentAsJsonAsync("APP", "Test", "Key");

		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await storageContext.Database.SqlQueryRaw<DmsStorageTable>("SELECT * From APP.Test").ToListAsync(), Has.Count.EqualTo(1));
			Assert.That(result, Is.Not.Null);
			Assert.That(JsonNode.DeepEquals(document, result), Is.True);
			Assert.That(result?["Test"]?.GetValue<bool>(), Is.False);
		});
	}

	[Test()]
	public async Task PutAndGetObjectAsStreamTest()
	{
		await storageRepository.CreateTableAsync(
			new DmsStorageCatalog { SC_Schema = "APP", SC_Name = "Test", SC_Type = DmsTableTypes.Object.ToString(), SC_Version = "0.1" });

		byte[] blob1 = Guid.NewGuid().ToByteArray();
		using var blobStream1 = new MemoryStream(blob1);
		await storageRepository.PutObjectAsStreamAsync("APP", "Test", "Key", blobStream1, "Me");
		using var result1 = await storageRepository.GetObjectAsStreamAsync("APP", "Test", "Key");

		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await storageContext.Database.SqlQueryRaw<DmsStorageTable>("SELECT * From APP.Test").ToListAsync(), Has.Count.EqualTo(1));
			Assert.That(result1, Is.Not.Null);
			var buffer = new byte[result1!.Length];
			await result1.ReadAsync(buffer, 0, buffer.Length);
			Assert.That(buffer, Is.EquivalentTo(blob1));
		});

		byte[] blob2 = Guid.NewGuid().ToByteArray();
		using var blobStream2 = new MemoryStream(blob2);
		await storageRepository.PutObjectAsStreamAsync("APP", "Test", "Key", blobStream2, "Me");
		using var result2 = await storageRepository.GetObjectAsStreamAsync("APP", "Test", "Key");

		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await storageContext.Database.SqlQueryRaw<DmsStorageTable>("SELECT * From APP.Test").ToListAsync(), Has.Count.EqualTo(1));
			Assert.That(result2, Is.Not.Null);
			var buffer = new byte[result2!.Length];
			await result2.ReadAsync(buffer, 0, buffer.Length);
			Assert.That(buffer, Is.EquivalentTo(blob2));
		});
	}
}