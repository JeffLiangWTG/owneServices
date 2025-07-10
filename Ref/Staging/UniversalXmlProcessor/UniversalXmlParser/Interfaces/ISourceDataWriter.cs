using System;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface ISourceDataWriter
	{
		Guid AddOrUpdateSourceDataAndGetPK(string subSource, DateTime sourceTime, string fileHash);
		void AddContent(string key, string value);
		void AddXmlContent(string value);
		void SetFlagAllRecordsAreProcessed();
		bool CheckDuplicateExists(string subSource, string fileHash);
		void SetFlagErrorOnRootLevel();
		void SetFlagDuplicated();
		DataProcessingInformation CreateDataProcessingInformationError(string message);
		string GetSourceName();
	}
}
