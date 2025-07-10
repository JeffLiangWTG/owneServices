using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class LineMerger : EU.Business.Declaration.LineMerger
	{
		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var invoiceLines = entryLine.InvoiceLines;
					var uniqueNDescriptions = invoiceLines.Select(il => il.JI_NDescription).ToHashSet();
					ZString description = string.Join(", ", uniqueNDescriptions);
					entryLine.CL_Description = description.Trim().SubstringSafe(0, entryLine.CL_DescriptionInfo.MaxLength);
				}
			}
		}
	}
}
