using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFLineCollection : ActiveBusinessObjectCollection<CusISFLine>, ICusISFLineCollection<CusISFLine>
	{
		public CusISFLineCollection(CusISFHeader iSFHeader)
			: base(iSFHeader)
		{
		}

		protected override void SetDefaultsForNewElementCore(CusISFLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			CusISFHeader header = Relationship.Master as CusISFHeader;
			if (header != null && header.ManufacturerAddresses.Count == 1)
			{
				newElement.BL_ManufacturerDocAddressPK = header.ManufacturerAddresses[0].PK;
			}
		}
	}
}
