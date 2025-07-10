using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.ZA.Business.InterfaceImplementations
{
	public class CusEntryHeaderCustomsCharges : Customs.Business.InterfaceImplementations.CusEntryHeaderCustomsCharges
	{
		public CusEntryHeaderCustomsCharges(ICustomsChargeEntry entryHeader) : base(entryHeader)
		{
		}

		protected override ZGuid GetDefaultCreditorPK(EntryChargeType chargeType)
		{
			return (entryHeader as CusEntryHeader)?.GetDefaultCreditorPK(chargeType) ?? ZGuid.Empty;
		}
	}
}
