using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class FDACollection : DependentCusAddInfoCollection<FDA, BusinessObject>
	{
		public FDACollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USFDA)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (FDA)child;
			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				if (invoiceLine.CopyLastFDADetailsToNewLine && Count > 0)
				{
					FDA previousFDALine = this[Count - 1];
					previousFDALine.UpdateAddInfoProperties();
					newElement.CopyPersistentValuesFrom(previousFDALine);
					newElement.US_InvCurrFDAValue = ValueRemaining(invoiceLine);
				}
				else
				{
					DefaultValuesFromInvoiceLine(newElement);
				}
				Factory.AddFetchHint(Enterprise.ZArchitecture.Schema.CusAddInfoSchema.PK, newElement.PK);
				newElement.SetDefaultFDARelatedBill();
				newElement.SetFDARelatedContainers();
				newElement.SetFDADefaultValueForBaseQty();
			}
		}

		internal void DefaultValuesFromInvoiceLine(FDA newElement)
		{
			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				if (newElement.US_FDACommercialDesc.IsEmpty)
				{
					newElement.US_FDACommercialDesc = invoiceLine.JI_Description.Left(newElement.US_FDACommercialDescInfo.MaxLength);
				}

				if (newElement.US_InvCurrFDAValue.IsEmpty)
				{
					var apportionmentDirtySuspender = invoiceLine.Declaration != null ? invoiceLine.Declaration.SuspendMarkApportionmentDirty() : null;

					newElement.US_InvCurrFDAValue = ValueRemaining(invoiceLine);
					if (apportionmentDirtySuspender != null)
					{
						apportionmentDirtySuspender.Dispose();
					}
				}

				newElement.SetManufacturerDefaults();
				newElement.SetShipperDefaults();
			}
		}

		ZDecimal ValueRemaining(JobComInvoiceLine invoiceLine)
		{
			ZDecimal valueRemaining = invoiceLine.JI_LinePrice - invoiceLine.FDAValueInvCurrRunningTotal;
			return valueRemaining > ZDecimal.Zero ? valueRemaining : ZDecimal.Zero;
		}

		void RefreshInvoiceLinesWithPGAIndicators()
		{
			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			RefreshInvoiceLinesWithPGAIndicators();
		}

		public override void RemoveAndDeleteAll()
		{
			base.RemoveAndDeleteAll();
			RefreshInvoiceLinesWithPGAIndicators();
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);

			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				if (invoiceLine.Declaration != null)
				{
					var fdaAdded = (FDA)bizOAdded;
					if (fdaAdded.US_InvCurrFDAValue > 0)
					{
						invoiceLine.Declaration.MarkApportionmentDirty();
					}
				}

				RefreshInvoiceLinesWithPGAIndicators();
			}
		}
	}
}
