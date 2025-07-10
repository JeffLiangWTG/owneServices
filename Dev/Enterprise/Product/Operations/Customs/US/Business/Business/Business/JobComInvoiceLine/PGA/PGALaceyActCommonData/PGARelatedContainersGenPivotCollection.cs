using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class PGARelatedContainersGenPivotCollection : Customs.Business.CustomsGenPivotCollection<PGARelatedContainersGenPivot, PGA, CusContainerInvoiceLinePivot>
	{
		public PGARelatedContainersGenPivotCollection(PGA master)
			: base(master)
		{
		}

		internal void AddMissingPivotIfOnlyOneContainer()
		{
			if (Count == 0)
			{
				var pga = Master;
				var invoiceLine = pga.InvoiceLine;
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
			get { return GenPivotTypeDecider.Types.PGARelatedContainersGenPivot; }
		}
	}
}
