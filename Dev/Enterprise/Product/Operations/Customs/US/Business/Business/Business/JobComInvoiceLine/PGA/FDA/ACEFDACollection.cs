using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDACollection : DependentCusAddInfoCollection<ACEFDA, BusinessObject>, IPGADataCorrectionCollection
	{
		public ACEFDACollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USACEFDA)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as ACEFDA;
			var invoiceLine = Master as JobComInvoiceLine;

			if (newElement != null && invoiceLine != null)
			{
				if (!invoiceLine.CopyLastPGADetailsToNewLine)
				{
					newElement.SetFDADefaultValueForBaseQty();

					if (invoiceLine.Declaration == null || !invoiceLine.Declaration.IsACEStandalonePNWithoutENSAndCRL)
					{
						newElement.US_InvCurrValue = ValueRemaining(invoiceLine);
					}
				}

				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					ACEFDA previousLine = this[Count - 1];
					newElement.UpdateAddInfoProperties();
					newElement.CopyPersistentValuesFrom(previousLine);

					newElement.CloneChildren(previousLine);
					newElement.US_InvCurrValue = decimal.Zero;
				}
			}
		}

		decimal ValueRemaining(JobComInvoiceLine invoiceLine)
		{
			var valueRemaining = invoiceLine.JI_LinePrice - invoiceLine.ACE_FDAValueInvCurrRunningTotal;
			return valueRemaining > decimal.Zero ? valueRemaining : decimal.Zero;
		}

		void RefreshInvoiceLinesWithPGAIndicators()
		{
			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			}
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			RefreshInvoiceLinesWithPGAIndicators();
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

		protected override bool AllowNewCore
		{
			get { return AllowAddNewPGALines && base.AllowNewCore; }
		}

		public bool AllowAddNewPGALines
		{
			get
			{
				if (!fAllowAddNewPGALines.HasValue)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					fAllowAddNewPGALines = invoiceLine?.AllowAddNewLineToPGACollection() ?? true;
				}
				return fAllowAddNewPGALines.Value;
			}
			set
			{
				fAllowAddNewPGALines = value;
				if (value)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					invoiceLine?.Declaration?.UpdatePGADataReplacementUpdateRequired();
				}
			}
		}
		bool? fAllowAddNewPGALines;

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
