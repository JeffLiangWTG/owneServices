using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PermitNumberToSelectFromForPrinting : NonPersistentBusinessObject
	{
		public PermitNumberToSelectFromForPrinting(ZString permitNumber)
		{
			this.permitNumber = permitNumber;
		}
		readonly ZString permitNumber;

		public ZString PermitNumber => permitNumber;

		public ZBool NeedPrint
		{
			get
			{
				return needPrint;
			}
			set
			{
				needPrint = value;
				NeedPrintInfo.RefreshBinding();
			}
		}
		ZBool needPrint = true;

		public ZPropertyInfo NeedPrintInfo
		{
			get { return GetZPropertyInfo(nameof(NeedPrint)); }
		}
	}
}
