using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFEquipCollection : ActiveBusinessObjectCollection<CusISFEquip>, ICusISFEquipCollection<CusISFEquip>
	{
		public CusISFEquipCollection(CusISFHeader iSFHeader)
			: base(iSFHeader)
		{
		}

		public CusISFEquip this[ZString equipmentNumber]
		{
			get
			{
				if (!equipmentNumber.IsEmpty)
				{
					foreach (CusISFEquip equipment in this)
					{
						if (equipment.BE_ContainerNum == equipmentNumber)
						{
							return equipment;
						}
					}
				}
				return null;
			}
		}
	}
}
