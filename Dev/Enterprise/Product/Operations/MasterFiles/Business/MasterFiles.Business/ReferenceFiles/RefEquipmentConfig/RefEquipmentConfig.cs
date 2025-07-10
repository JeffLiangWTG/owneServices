using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentConfig : AutoRefEquipmentConfig
	{
		public RefEquipmentConfig(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
