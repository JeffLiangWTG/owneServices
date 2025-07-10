using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class FCCCollection : DependentCusAddInfoCollection<FCC, BusinessObject>
	{
		public FCCCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USFCC)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as FCC;

			var invoiceLine = Master as JobComInvoiceLine;
			if (newElement != null && invoiceLine != null)
			{
				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					FCC previousElement = this[Count - 1];

					newElement.US_FCCImpCondNo = previousElement.US_FCCImpCondNo;
					newElement.US_FCCImpCondNoQtyAppr = previousElement.US_FCCImpCondNoQtyAppr;
					newElement.US_FCCModel = previousElement.US_FCCModel;
					newElement.US_FCCQty = previousElement.US_FCCQty;
					newElement.US_FCCTradeName = previousElement.US_FCCTradeName;
					newElement.US_FCCWithhold = previousElement.US_FCCWithhold;
					newElement.US_FCCID = previousElement.US_FCCID;
				}

				newElement.US_FCCCommercialDesc = invoiceLine.JI_Description.Left(newElement.US_FCCCommercialDescInfo.MaxLength);
			}
		}
	}
}
