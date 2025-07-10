using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.NewService
{
	public interface IReferenceDataService
	{
		DateTime GetServerTimestamp();
		IEnumerable<Tuple<string, IEnumerable<DataSetVersion>>> GetAllDataSetTimestamps();
		IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps(IEnumerable<Tuple<string, IEnumerable<DataSetVersion>>> dataSetVersions);
	}

	public interface IReferenceDataService<T> : IReferenceDataService
	{
		IEnumerable<T> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId);
		short GetDataSetId(string dataSetName);
	}
}
