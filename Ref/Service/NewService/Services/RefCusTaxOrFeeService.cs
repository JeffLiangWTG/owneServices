using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusTaxOrFeeService : IReferenceDataService<Models.RefCusTaxOrFee>
	{
		public RefCusTaxOrFeeService(IReferenceDataService<Models.RefCusTaxOrFeeType> refCusTaxOrFeeTypeService)
		{
			Argument.NotNull(refCusTaxOrFeeTypeService, nameof(refCusTaxOrFeeTypeService));
			_refCusTaxOrFeeTypeService = refCusTaxOrFeeTypeService;
		}
		readonly IReferenceDataService<Models.RefCusTaxOrFeeType> _refCusTaxOrFeeTypeService;

		public IEnumerable<Models.RefCusTaxOrFee> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var types = _refCusTaxOrFeeTypeService.GetData(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
			foreach (var type in types)
			{
				foreach (var tax in type.RefCusTaxOrFees)
				{
					tax.Deleted = type.Deleted;
					yield return tax;
				}
			}
		}

		public DateTime GetServerTimestamp()
		{
			return _refCusTaxOrFeeTypeService.GetServerTimestamp();
		}

		public IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps(IEnumerable<Tuple<string, IEnumerable<DataSetVersion>>> dataSetVersions)
		{
			return new DataSetVersion[] {
				new DataSetVersion(typeof(RefCusTaxOrFee).Name, GetServerTimestamp())
			};
		}

		public short GetDataSetId(string dataSetName)
		{
			return _refCusTaxOrFeeTypeService.GetDataSetId(dataSetName);
		}

		public IEnumerable<Tuple<string, IEnumerable<DataSetVersion>>> GetAllDataSetTimestamps()
		{
			return _refCusTaxOrFeeTypeService.GetAllDataSetTimestamps();
		}
	}
}
