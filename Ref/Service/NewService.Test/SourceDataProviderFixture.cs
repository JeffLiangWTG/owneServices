using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.NewService.UploadDownloadService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
[TransactionedTestCase]
class SourceDataProviderFixture
{
	[Test]
	public async Task GetFileFromServer()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		var sourceDataPK = Guid.NewGuid();
		var sourceFileName = "anyfile.xml";
		CreateAndInsertRecordToSourceData(sourceDataPK, sourceFileName, dbName);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		var sourceDataProvider = new SourceDataProvider(connectionString);
		var (fileName, fileStream) = await sourceDataProvider.GetFileFromServer(sourceDataPK);
		Assert.AreEqual(sourceFileName, fileName);
		using var streamReader = new StreamReader(fileStream);
		Assert.AreEqual("<abcd 1234 />", await streamReader.ReadToEndAsync());
	}

	[Test]
	public async Task UploadFileToServer()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		var sourceDataProvider = new SourceDataProvider(connectionString);

		var fileName = "testfile.xml";
		using (var stream = new MemoryStream(GetContentBytes(fileName)))
		{
			var request = new Mock<HttpRequest>();
			request.SetupGet(x => x.Body).Returns(stream);
			request.SetupGet(x => x.ContentType).Returns("multipart/form-data; boundary=Ax8x8");
			request.SetupGet(x => x.Headers).Returns(new DefaultHttpContext().Request.Headers);
			request.Object.Headers.ContentType = "multipart/form-data; boundary=Ax8x8";
			request.Object.Headers.ContentDisposition = $"form-data; name=tariff; filename={fileName}";
			var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(request.Object.ContentType).Boundary).Value;
			var reader = new MultipartReader(boundary, request.Object.Body);
			var section = await reader.ReadNextSectionAsync();
			await sourceDataProvider.UploadFileToServer("any", "any", "any", "any", fileName, section.AsFileSection());
		}

		await using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
		{
			conn.Open();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $"SELECT COUNT(*) FROM SourceData WHERE SDA_Filename = '{fileName}'";
				var result = (int)(await cmd.ExecuteScalarAsync())!;
				Assert.AreEqual(1, result);
			}
		}
	}

	byte[] GetContentBytes(string fileName)
	{
		var bodyString = $@"--Ax8x8
Content-Disposition: form-data; name=tariff; filename={fileName}

default text
--Ax8x8--
";
		return Encoding.UTF8.GetBytes(bodyString);
	}

	void CreateAndInsertRecordToSourceData(Guid pk, string fileName, string dbName)
	{
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		using var cmdInsert = conn.CreateCommand();
		cmdInsert.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='SourceData')
BEGIN
CREATE TABLE [dbo].[SourceData](
	[SDA_PK] [uniqueidentifier] NOT NULL,
	[SDA_Source] [char](3) NOT NULL,
	[SDA_Filename] [varchar](max) NULL,
	[SDA_Filetype] [varchar](3) NULL,
	[SDA_Content] [varbinary](max) NULL,
	[SDA_ContentText] [varchar](max) NOT NULL,
	[SDA_Status] [char](3) NULL,
	[SDA_CreatedTime] [datetime2](7) NOT NULL,
	[SDA_ContentType] [char](3) NULL,
	[SDA_SourceTime] [datetime2](7) NULL,
	[SDA_SubSource] [varchar](75) NOT NULL,
	[SDA_NotProcessedUntil] [datetime2](7) NULL,
	[SDA_Contacts] [varchar](1000) NULL,
 CONSTRAINT [PK_SourceData] PRIMARY KEY NONCLUSTERED 
(
	[SDA_PK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_SDA_PK]  DEFAULT (newid()) FOR [SDA_PK];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_SDA_ContentText]  DEFAULT ('') FOR [SDA_ContentText];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_SDA_Status]  DEFAULT ('QUE') FOR [SDA_Status];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_SDA_CreatedTime]  DEFAULT (sysutcdatetime()) FOR [SDA_CreatedTime];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_SDA_SubSource]  DEFAULT ('') FOR [SDA_SubSource];
END

INSERT INTO dbo.SourceData (SDA_PK, SDA_Source, SDA_FileName, SDA_FileType, SDA_ContentType, SDA_Content, SDA_Status)
VALUES (@pk, 'any', @fileName, 'XML', 'any', @content, 'QUE')";
		var pkParam = cmdInsert.CreateParameter();
		pkParam.ParameterName = "@pk";
		pkParam.Value = pk;
		var fileNameParam = cmdInsert.CreateParameter();
		fileNameParam.ParameterName = "@fileName";
		fileNameParam.Value = fileName;
		var contentParam = cmdInsert.CreateParameter();
		contentParam.ParameterName = "@content";
		contentParam.Value = Encoding.UTF8.GetBytes("<abcd 1234 />");

		cmdInsert.Parameters.Add(pkParam);
		cmdInsert.Parameters.Add(fileNameParam);
		cmdInsert.Parameters.Add(contentParam);
		cmdInsert.ExecuteNonQuery();
	}
}
