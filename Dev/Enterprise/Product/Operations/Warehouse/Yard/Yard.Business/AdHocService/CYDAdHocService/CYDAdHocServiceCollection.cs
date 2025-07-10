using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDAdHocServiceCollection : ActiveBusinessObjectCollection<CYDAdHocService>
	{
		public CYDAdHocServiceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public CYDAdHocServiceCollection(BusinessObjectFactory factory, BusinessObject master)
			: base(factory, master, new ZQuery(), CYDAdHocServiceSchema.YAS_YAO_ServiceOrder)
		{
		}
	}
}
