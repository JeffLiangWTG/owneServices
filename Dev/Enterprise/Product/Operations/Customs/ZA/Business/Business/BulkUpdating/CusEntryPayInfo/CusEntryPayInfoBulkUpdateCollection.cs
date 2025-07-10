using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryPayInfoBulkUpdateCollection : BusinessObjectCollection<CusEntryPayInfo>
	{
		public CusEntryPayInfoBulkUpdateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore => false;
	}
}
