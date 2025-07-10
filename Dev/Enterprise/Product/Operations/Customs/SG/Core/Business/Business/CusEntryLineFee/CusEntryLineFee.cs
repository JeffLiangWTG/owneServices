using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusEntryLineFee : TypeSafeCusEntryLineFee, Integration.Customs.SG.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
