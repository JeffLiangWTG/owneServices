using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DOTCollection : DependentCusAddInfoCollection<DOT, BusinessObject>
	{
		public DOTCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDOT)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (DOT)child;
			JobComInvoiceLine invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				newElement.US_DOTCommercialDesc = invoiceLine.JI_Description.Left(newElement.US_DOTCommercialDescInfo.MaxLength);
			}
		}
	}
}
