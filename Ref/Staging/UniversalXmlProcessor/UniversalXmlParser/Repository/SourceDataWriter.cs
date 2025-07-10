using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Repository
{
	public class SourceDataWriter : ISourceDataWriter
	{
		readonly IStagingRepository repository;
		readonly SourceContent sourceContent;
		readonly SourceData sourceData;

		public SourceDataWriter(IStagingRepository stagingRepository, SourceData sourceData)
		{
			Argument.NotNull(stagingRepository, nameof(stagingRepository));

			repository = stagingRepository;
			sourceContent = new SourceContent(Constants.UniversalXml.RootElement, new UniversalXmlTransformer(new UniversalXmlTransformerValidator()));
			this.sourceData = sourceData;
		}

		public Guid AddOrUpdateSourceDataAndGetPK(string subSource, DateTime sourceTime, string fileHash)
		{
			sourceData.SDA_SourceTime = sourceTime;
			sourceData.SDA_SubSource = subSource;
			sourceData.SDA_FileHash = fileHash;

			repository.AddOrUpdate(sourceData);
			repository.SaveChanges();

			return sourceData.SDA_PK;
		}

		public string GetSourceName()
		{
			return sourceData.SDA_Filename;
		}

		public void AddContent(string key, string value)
		{
			Argument.NotNullOrEmpty(key, nameof(key));
			if (!string.IsNullOrEmpty(key))
			{
				sourceContent.AddContent(key, value);
				sourceData.SDA_ContentText = sourceContent.ToXml();

				repository.Update(sourceData);
				repository.SaveChanges();
			}
		}

		public void AddXmlContent(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}

			sourceContent.AddXmlContent(value);
			sourceData.SDA_ContentText = sourceContent.ToXml();

			repository.Update(sourceData);
			repository.SaveChanges();
		}

		public void SetFlagAllRecordsAreProcessed()
		{
			SetFlag(StatusProvider.GetPRSStatus());
		}

		public void SetFlagErrorOnRootLevel()
		{
			SetFlag(StatusProvider.GetERRStatus());
		}

		public void SetFlagDuplicated()
		{
			SetFlag(StatusProvider.GetDUPStatus());
		}

		void SetFlag(string status)
		{
			sourceData.SDA_Status = status;
			repository.SaveChanges();
		}

		public bool CheckDuplicateExists(string subSource, string fileHash)
		{
			var lastProcessed = repository.Get<SourceData>().Where(src =>
				src.SDA_SubSource == subSource
				&& src.SDA_Status != StatusProvider.GetQUEStatus()
			).OrderByDescending(src => src.SDA_CreatedTime).FirstOrDefault();

			return lastProcessed != null &&
				   lastProcessed.SDA_FileHash == fileHash &&
				   lastProcessed.IsSourceDataMergedStatus();
		}

		public DataProcessingInformation CreateDataProcessingInformationError(string message)
		{
			Argument.NotNullOrEmpty(message, nameof(message));
			var dataProcessingInformation = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_Status = StatusProvider.GetERRStatus(),
				DPI_SourceId = sourceData.SDA_PK,
				DPI_Message = message,
				DPI_ParentTableCode = sourceData.SDA_Source,
				DPI_ParentPk = null
			};

			repository.Add(dataProcessingInformation);
			repository.SaveChanges();

			return dataProcessingInformation;
		}
	}
}
