using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using ContractModel = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefCusTaxOrFeeController : DataSetControllerBase<ContractModel.RefCusTaxOrFee>
{
	public RefCusTaxOrFeeController(IReferenceDataService<ContractModel.RefCusTaxOrFee> service, IDataAdaptor adaptor, IDataBlockCacheHelper dataBlockCacheHelper, IClientRecord clientRecord, ICacheWrapper cacheWrapper, ILogHelper logHelper)
		: base(service, adaptor, dataBlockCacheHelper, clientRecord, cacheWrapper, logHelper)
	{
		Argument.NotNull(service, nameof(service));
		Argument.NotNull(adaptor, nameof(adaptor));
		Argument.NotNull(dataBlockCacheHelper, nameof(dataBlockCacheHelper));
		Argument.NotNull(clientRecord, nameof(clientRecord));
		Argument.NotNull(cacheWrapper, nameof(cacheWrapper));
		Argument.NotNull(logHelper, nameof(logHelper));
	}
}
