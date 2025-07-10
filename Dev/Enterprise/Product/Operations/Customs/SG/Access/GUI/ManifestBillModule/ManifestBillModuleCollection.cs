using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestBillModuleCollection : ASYCUDA.Module.ASYCUDAManifestBillModuleCollection<AsycudaBill>
	{
		public ManifestBillModuleCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Singapore)
		{
		}

		#region Fetch Strategy
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ASYCUDAManifestBillModuleCollectionFetchStrategy(this);
		}
		#endregion
	}

	public class ASYCUDAManifestBillModuleCollectionFetchStrategy : ASYCUDA.Module.ASYCUDAManifestBillModuleCollectionFetchStrategy<AsycudaBill>
	{
		public ASYCUDAManifestBillModuleCollectionFetchStrategy(ManifestBillModuleCollection collection)
			: base(collection)
		{
		}

		protected override bool IsBillCountryGenAddOnColumnRelatedColumn(ZString columnName)
		{
			return billCountryGenAddOnColumnList.Contains(columnName);
		}

		readonly ZString[] billCountryGenAddOnColumnList = {
			AsycudaBill.Schema.CycleDate,
			AsycudaBill.Schema.CycleNumber,
			AsycudaBill.Schema.BatchDate,
			AsycudaBill.Schema.BatchNumber,
			AsycudaBill.Schema.CustomsJobNumber
		};
	}
}
