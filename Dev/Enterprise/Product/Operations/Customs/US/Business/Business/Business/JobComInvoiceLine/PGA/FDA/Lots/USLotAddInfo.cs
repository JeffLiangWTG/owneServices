using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USLot)]
	public class USLotAddInfo : AutoUSFDALotAddInfo, Integration.Customs.US.IUSLotAddInfo
	{
		public USLotAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override USFDALotAddInfoValidation GetNewValidation()
		{
			if (Lot?.Parent is CPSCHeader)
			{
				return new PSCLotValidation(this);
			}
			else if (Lot?.Parent is ACEFDA)
			{
				return new FDALotValidation(this);
			}
			else
			{
				return base.GetNewValidation();
			}
		}

		public Lot Lot => Parent as Lot;
	}
}
