using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class CusEntryNumberCollection : Customs.Business.CusEntryNumCollection
	{
		public CusEntryNumberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new CusEntryNumber this[int index]
		{
			get { return (CusEntryNumber)Elements[index]; }
		}

		public new CusEntryNumber AddNew()
		{
			return (CusEntryNumber)base.AddNew();
		}
	}
}
