using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateDynamicPickFaceAreaLookups : WarehouseClientLookups
	{
		public UpdateDynamicPickFaceAreaLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region DynamicPickAreas

		public IWhsDynamicAreaCollection DynamicPickAreas
		{
			get
			{
				var warehousePK = ((UpdateDynamicPickFaceAreaMethodApplicator)Parent).WarehousePK;
				return Factory.GetCachedValue("UpdateDynamicPickFaceAreaLookups|DynamicPickAreas" + warehousePK, () => ObjectFactory.Get<IWhsDynamicAreaCollection>(nameof(IWhsDynamicAreaCollection), Factory, new ZQuery(WhsAreaSchema.WA_WW_Whs, warehousePK)));
			}
		}

		#endregion
	}
}
