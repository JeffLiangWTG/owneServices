using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Repository;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public class Scanner : IScanner
	{
		readonly IContentParser _contentParser;
		readonly IFileTrace _fileTrace;
		readonly IStagingRepositoryWrapper _stagingRepositoryWrapper;
		readonly IUniversalXmlSchemaHandler _universalXmlSchemaHandler;
		readonly IStagingRepository _stagingRepository;
		readonly IConfigProvider _configProvider;

		public Scanner(IContentParser contentParser, IStagingRepository stagingRepository, IUniversalXmlSchemaHandler universalXmlSchemaHandler, IFileTrace fileTrace, IStagingRepositoryWrapper stagingRepositoryWrapper, IConfigProvider configProvider)
		{
			Argument.NotNull(contentParser, nameof(contentParser));
			Argument.NotNull(stagingRepository, nameof(stagingRepository));
			Argument.NotNull(universalXmlSchemaHandler, nameof(universalXmlSchemaHandler));
			Argument.NotNull(fileTrace, nameof(fileTrace));
			Argument.NotNull(stagingRepositoryWrapper, nameof(stagingRepositoryWrapper));

			_contentParser = contentParser;
			_fileTrace = fileTrace;
			_stagingRepositoryWrapper = stagingRepositoryWrapper;
			_universalXmlSchemaHandler = universalXmlSchemaHandler;
			_stagingRepository = stagingRepository;
			_configProvider = configProvider;
		}

		public async Task Scan()
		{

			var dataPK = Guid.Empty;
			var sourceData = FindDataInQUEStatus();
			while (sourceData != null && sourceData.SDA_PK != dataPK)
			{
				try
				{
					dataPK = sourceData.SDA_PK;
					UpdateNotProcessUntil(sourceData);
					_stagingRepositoryWrapper.BulkInsertExecuting = () => UpdateNotProcessUntil(sourceData);
					var sourceDataWriter = new SourceDataWriter(_stagingRepository, sourceData);
					var appName = Path.GetFileName(Assembly.GetExecutingAssembly()?.Location);
					var uxmlParser = new UniversalXmlParser.UniversalXmlParser(_universalXmlSchemaHandler, _stagingRepositoryWrapper, sourceDataWriter, _fileTrace, appName);

					if (sourceData.SDA_Filetype == "XML")
					{
						await _contentParser.XmlContent(sourceData, uxmlParser, _stagingRepository);
					}
					else if (sourceData.SDA_Filetype == "COM")
					{
						await _contentParser.CompressedContent(sourceData, uxmlParser, _stagingRepository);
					}
					//validate if status is still Queued after parsing.
					if (sourceData.IsSourceDataQueuedStatus())
					{
						ValidateSourceData(sourceData, sourceDataWriter);
					}
					sourceData = FindDataInQUEStatus();
				}
				catch (NotProcessUntilException ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		void ValidateSourceData(SourceData sourceData, ISourceDataWriter sourceDataWriter)
		{
			var hasDuplicated = sourceDataWriter.CheckDuplicateExists(sourceData.SDA_SubSource, sourceData.SDA_FileHash);
			if (hasDuplicated)
			{
				sourceDataWriter.SetFlagDuplicated();
				return;
			}
			var timeOutInHours = _configProvider.ExecutionTimeOutInHours;
			if (sourceData.SDA_CreatedTime < DateTime.UtcNow.AddHours(-timeOutInHours))
			{
				var errorMessage = $"Source {sourceData.SDA_SubSource} error: Exceed timeout limit of {timeOutInHours} hours";
				Console.Error.WriteLine(errorMessage);
				sourceDataWriter.SetFlagErrorOnRootLevel();
				return;
			}
		}

		void UpdateNotProcessUntil(SourceData data)
		{
			var newNotProcessUntil = DateTime.UtcNow.AddMinutes(5);
			var effected = _stagingRepository.ExecuteSqlCommand($@"
UPDATE {nameof(SourceData)} SET {nameof(SourceData.SDA_NotProcessedUntil)} = @p0
WHERE {nameof(SourceData.SDA_PK)} = @p1
AND ISNULL({nameof(SourceData.SDA_NotProcessedUntil)}, '') = ISNULL(@p2, '')",
new SqlParameter("@p0", System.Data.SqlDbType.DateTime2) { Value = newNotProcessUntil },
new SqlParameter("@p1", System.Data.SqlDbType.UniqueIdentifier) { Value = data.SDA_PK },
new SqlParameter("@p2", System.Data.SqlDbType.DateTime2) { Value = (object)data.SDA_NotProcessedUntil ?? DBNull.Value });
			if (effected > 0)
			{
				data.SDA_NotProcessedUntil = newNotProcessUntil;
			}
			else
			{
				throw new NotProcessUntilException();
			}
		}

		public SourceData FindDataInQUEStatus()
		{
			return _stagingRepository.Get<SourceData>().Where(s => s.SDA_Source == DataSourceConstants.Source.Upload
			&& (s.SDA_Filetype == DataSourceConstants.FileType.XML.ToString() || s.SDA_Filetype == DataSourceConstants.FileType.COM.ToString())
			&& s.SDA_Status == StatusProvider.GetQUEStatus() && (!s.SDA_NotProcessedUntil.HasValue || s.SDA_NotProcessedUntil < DateTime.UtcNow)).FirstOrDefault();
		}
	}
}
