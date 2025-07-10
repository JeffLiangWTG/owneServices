using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.NewService.UploadDownloadService
{
	public class SourceDataProvider : ISourceDataProvider
	{
		readonly string _dbConnectionString;

		public SourceDataProvider(string stagingConnectionString)
		{
			Argument.NotNull(stagingConnectionString, nameof(stagingConnectionString));
			_dbConnectionString = stagingConnectionString;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "UploadController.GetFile need to access the stream")]
		public async Task<(string, Stream)> GetFileFromServer(Guid sourceDataPK)
		{
			Argument.IsTrue(sourceDataPK != Guid.Empty, nameof(sourceDataPK));
			var sqlCmd = $@"SELECT SDA_FileName, SDA_Content FROM SourceData WHERE SDA_PK = @pk";

			var dbConnection = new SqlConnection(_dbConnectionString);
			await dbConnection.OpenAsync();
			var cmd = new SqlCommand(sqlCmd, dbConnection);
			{
				var pkParam = cmd.CreateParameter();
				pkParam.ParameterName = "@pk";
				pkParam.Value = sourceDataPK;
				cmd.Parameters.Add(pkParam);
				var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
				{
					if (await reader.ReadAsync() && !await reader.IsDBNullAsync(0))
					{
						var fileName = reader.GetString(0);
						var stream = reader.GetStream(1);
						return (fileName, stream);
					}
				}
			}

			return (string.Empty, null);
		}

		public async Task UploadFileToServer(string source, string fileType, string contentType, string contacts, string fileName, FileMultipartSection fileSection)
		{
			Argument.NotNullOrEmpty(source, nameof(source));
			Argument.NotNullOrEmpty(fileType, nameof(fileType));
			Argument.NotNullOrEmpty(contentType, nameof(contentType));
			Argument.NotNullOrEmpty(fileName, nameof(fileName));
			Argument.NotNull(fileSection, nameof(fileSection));

			using (var dbConnection = new SqlConnection(_dbConnectionString))
			{
				await dbConnection.OpenAsync();
				using (var dbTransaction = dbConnection.BeginTransaction())
				using (var sourceDataStream = new SourceDataContentWriterStream(dbConnection, dbTransaction, fileName, contentType, fileType, source, contacts))
				{
					var buffer = new byte[102400];
					var bytesRead = 0;
					do
					{
						bytesRead = await fileSection.FileStream.ReadAsync(buffer);
						if (bytesRead > 0)
						{
							sourceDataStream.Write(buffer, 0, bytesRead);
						}
					}
					while (bytesRead > 0);

					sourceDataStream.Flush();
				}
			}
		}
	}
}
