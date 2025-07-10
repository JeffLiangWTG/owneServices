using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedContainersGenPivotCollection : Customs.Business.CustomsGenPivotCollection<FDARelatedContainersGenPivot, BusinessObject, Customs.Business.CusContainerInvoiceLinePivot>
	{
		public FDARelatedContainersGenPivotCollection(IFDARelatedContainer master)
			: base((BusinessObject)master)
		{
		}

		internal void AddMissingPivotIfOnlyOneContainer()
		{
			if (Count == 0)
			{
				var master = (IFDARelatedContainer)Master;
				var invoiceLine = master.InvoiceLine;
				if (invoiceLine != null && invoiceLine.ContainersPivot.Count == 1)
				{
					AddPivotFor((CusContainerInvoiceLinePivot)invoiceLine.ContainersPivot[0]);
				}
			}
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			if (!runDefault)
			{
				runDefault = true;
				AddMissingPivotIfOnlyOneContainer();
			}
		}
		bool runDefault;

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.FDARelatedContainersGenPivot; }
		}
	}
}
