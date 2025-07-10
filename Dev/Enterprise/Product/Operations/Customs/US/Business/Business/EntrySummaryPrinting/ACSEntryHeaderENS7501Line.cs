// -----------------------------------------------------------------------
// <copyright file="ACSEntryHeaderENS7501Line.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Schema;

	public class ACSEntryHeaderENS7501Line : EntryHeaderENS7501Line
	{
		public ACSEntryHeaderENS7501Line(CusEntryLine line, bool printInvoiceHeading, bool printInvoiceDetails)
			: base(line, printInvoiceHeading, printInvoiceDetails)
		{
		}

		protected override string GetADCVDRateDescriptionFromCaseRecord(ZString caseNo, string rateType, ZDecimal depositRateOverride)
		{
			var adCase = (USCACCase)GetADCVDCaseRecord(caseNo);
			return DepositRateIndicatorList.GetRateDescriptionFromCaseRecord(adCase, DateForAD_CVD, rateType);
		}

		protected override IACCase GetADCVDCaseRecord(ZString caseNo)
		{
			return Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, caseNo);
		}
	}
}
