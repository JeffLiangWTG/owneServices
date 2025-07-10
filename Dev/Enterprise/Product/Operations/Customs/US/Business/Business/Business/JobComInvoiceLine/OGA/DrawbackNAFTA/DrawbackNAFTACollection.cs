using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackNAFTACollection : DependentCusAddInfoCollection<DrawbackNAFTA, JobComInvoiceLine>
	{
		public DrawbackNAFTACollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDrawbackNAFTA)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var drawbackNAFTA = child as DrawbackNAFTA;
			if (drawbackNAFTA != null)
			{
				var declaration = drawbackNAFTA.InvoiceLine != null ? drawbackNAFTA.InvoiceLine.Declaration : null;
				if (declaration != null && declaration.IsACEDrawback)
				{
					drawbackNAFTA.US_DRWNAFTACountryOfExport = declaration.US_NAFTADrawbackCountry;
				}
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			if (Master is JobComInvoiceLine invoiceline && invoiceline.ShouldReCalculateDrawbackData)
			{
				invoiceline.Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
			}
		}
	}
}
