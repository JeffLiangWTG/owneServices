using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineViewCollection : InvoiceLineViewCollection<JobComInvoiceLine>
	{
		public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void CopyLastLineDetailsToNewLinesIfEnabled(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			base.CopyLastLineDetailsToNewLinesIfEnabled(newLine, previousLine);
			if (CopyLastLineDetailsToNewLines)
			{
				CopyInvoiceLineTaxIfNeeded(newLine, previousLine);

				newLine.PreviousBondedEntryNumber = previousLine.PreviousBondedEntryNumber;
				newLine.CertificateOfOriginNumber = previousLine.CertificateOfOriginNumber;
				if (previousLine.IsExport)
				{
					newLine.JI_BondedGoodsCode = previousLine.JI_BondedGoodsCode;
				}
				newLine.JI_Group = ZString.Empty;
				newLine.TrademarkStorageDocsGuid = previousLine.TrademarkStorageDocsGuid;
				try
				{
					((IBusinessObjectInternals)newLine).IsCopying = true;
					newLine.JI_EnteredUnitPrice = previousLine.JI_EnteredUnitPrice;
				}
				finally
				{
					((IBusinessObjectInternals)newLine).IsCopying = false;
				}

				newLine.JI_DtyPymntMthd = previousLine.JI_DtyPymntMthd;
				newLine.JI_TpfPymntMthd = previousLine.JI_TpfPymntMthd;
				newLine.JI_VatPymntMthd = previousLine.JI_VatPymntMthd;
				previousLine.PermitCusSupportingCollection.CloneElementsTo(newLine.PermitCusSupportingCollection);
				previousLine.ExemptionOfControllingAgenciesCusSupportings.CloneElementsTo(newLine.ExemptionOfControllingAgenciesCusSupportings);

				if (previousLine.IsRAPOrROR)
				{
					newLine.JI_RAPCurr = previousLine.JI_RAPCurr;
					newLine.JI_Calc_RAPRORUnitPrice = previousLine.JI_Calc_RAPRORUnitPrice;
					newLine.JI_RAPPrice = previousLine.JI_RAPPrice;
					newLine.JI_UseOneTenthCV = previousLine.JI_UseOneTenthCV;
				}
			}
		}

		void CopyInvoiceLineTaxIfNeeded(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			if (previousLine.IsImport && !newLine.JI_Tariff.IsEmpty)
			{
				newLine.ClearAndDefaultInvoiceLineTax();
			}
		}
	}
}
