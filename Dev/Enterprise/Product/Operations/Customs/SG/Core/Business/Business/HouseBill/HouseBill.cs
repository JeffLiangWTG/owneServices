using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class Bill : TypeSafeBill, Integration.Customs.SG.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
		{
			return new CusDecHouseBillLookups(this);
		}
	}
}
