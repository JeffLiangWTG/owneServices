using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class NonPersistentCusContainer : Customs.Business.NonPersistentCusContainer
	{
		public NonPersistentCusContainer(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public new CusContainerInvoiceLinePivot Pivot
		{
			get { return (CusContainerInvoiceLinePivot)base.Pivot; }
		}

		public override ZBool IsForInvoiceLine
		{
			get { return base.IsForInvoiceLine; }
			set
			{
				bool oldValue = IsForInvoiceLine;
				base.IsForInvoiceLine = value;
				if (!IsCopying && oldValue != IsForInvoiceLine)
				{
					var pivot = Pivot;
					if (pivot != null)
					{
						foreach (PGA pga in InvoiceLine.LaceyActLines)
						{
							if (!value)
							{
								pga.ContainersForPGALine.DeletePivotFor(pivot);
							}
						}

						foreach (FDA fda in InvoiceLine.FDAs)
						{
							if (value)
							{
								fda.ContainersForFDALine.AddPivotFor(pivot);
							}
							else
							{
								fda.ContainersForFDALine.DeletePivotFor(pivot);
							}
						}
					}
				}
			}
		}

		public ZDecimal EffectiveNetWeightInKG
		{
			get { return InvoiceLine.IsSingleAssociationWithContainer ? InvoiceLine.NetWeightInKG : NetWeightInKG; }
		}

		public ZInt EffectivePackQty
		{
			get { return InvoiceLine.IsSingleAssociationWithContainer ? InvoiceLine.US_ManifestQty : PackQty; }
		}

		public ZDecimal EffectiveCustomsValue
		{
			get { return InvoiceLine.IsSingleAssociationWithContainer ? InvoiceLine.JI_CustomsValue : CustomsValue; }
		}

		protected new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}
	}
}
