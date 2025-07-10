using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class CusInBondEquipmentCollection : ActiveBusinessObjectCollection<CusInBondEquipment>
	{
		public CusInBondEquipmentCollection(CusInBondHeader master) : base(master.Factory, master, null, CusInBondEquipmentSchema.BJ_BH_Header)
		{
		}
	}
}
