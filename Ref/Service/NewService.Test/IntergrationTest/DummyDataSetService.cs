using System;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.NewService.Controllers;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class DummyDataSetService : OneTableUpdateService<RefDataSet, RefDataSet>
	{
		public DummyDataSetService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
		}
	}

	class DummyDataSetController : DataSetControllerBase<RefDataSet>
	{
		public DummyDataSetController(
			IReferenceDataService<RefDataSet> service,
			IDataAdaptor adaptor,
			IDataBlockCacheHelper dataBlockCacheHelper,
			IClientRecord clientRecord,
			ICacheWrapper cacheWrapper,
			ILogHelper logHelper)
			: base(service, adaptor, dataBlockCacheHelper, clientRecord, cacheWrapper, logHelper)
		{
		}

		[HttpGet]
		public string GetString(int id)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
	}
}
