using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCommodityLineCollection : NonPersistentBusinessObjectCollection<DISCommodityLine>
	{
		public DISCommodityLineCollection(DISDocument disDocument)
			: base(disDocument.Factory)
		{
			this.disDocument = disDocument;
		}

		readonly DISDocument disDocument;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DISCommodityLine(disDocument);
		}
	}
}
