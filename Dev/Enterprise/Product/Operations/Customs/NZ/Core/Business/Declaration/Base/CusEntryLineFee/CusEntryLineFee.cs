using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[DependentBusinessObject(typeof(CusEntryLine), "Fees")]
	public class CusEntryLineFee : Customs.Business.CusEntryLineFee, Integration.Customs.NZ.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
