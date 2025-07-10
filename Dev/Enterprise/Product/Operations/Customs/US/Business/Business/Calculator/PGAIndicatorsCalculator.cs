using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PGAIndicatorsCalculator
	{
		public PGAIndicatorsCalculator(Func<ZString, ZBool> indicatorRelevantFunc)
		{
			this.indicatorRelevantFunc = indicatorRelevantFunc;
		}
		readonly Func<ZString, ZBool> indicatorRelevantFunc;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZBool ShouldRemoveIrrelevantIndicator(string columnName)
		{
			var shouldRemoveIndicator = false;
			var usColumnName = columnName.StartsWith("US_", StringComparison.OrdinalIgnoreCase) ? columnName : "US_" + columnName;

			switch (usColumnName)
			{
				case JobComInvoiceLine.Schema.US_ATFInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.ATF);
					break;
				case JobComInvoiceLine.Schema.US_FDAIndicator:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.FDA);
					break;
				case JobComInvoiceLine.Schema.US_NMFS370Ind:
				case JobComInvoiceLine.Schema.US_NMFSAMRInd:
				case JobComInvoiceLine.Schema.US_NMFSHMSInd:
				case JobComInvoiceLine.Schema.US_NMFSSIMPInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.NMFS);
					break;
				case JobComInvoiceLine.Schema.US_VNEInd:
				case JobComInvoiceLine.Schema.US_PSTIndicator:
				case JobComInvoiceLine.Schema.US_ODSInd:
				case JobComInvoiceLine.Schema.US_TSCAInd:
				case JobComInvoiceLine.Schema.US_HFCInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.EPA);
					break;
				case JobComInvoiceLine.Schema.US_AMSInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.AMS);
					break;
				case JobComInvoiceLine.Schema.US_NOPInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.NOP);
					break;
				case JobComInvoiceLine.Schema.US_APHISInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.APHIS);
					break;
				case JobComInvoiceLine.Schema.US_CPSCInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.CPSC);
					break;
				case JobComInvoiceLine.Schema.US_DDTCInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.DDTC);
					break;
				case JobComInvoiceLine.Schema.US_DEAInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.DEA);
					break;
				case JobComInvoiceLine.Schema.US_FSISInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.FSIS);
					break;
				case JobComInvoiceLine.Schema.US_FWSInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.FWS);
					break;
				case JobComInvoiceLine.Schema.US_LaceyIndicator:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.Lacey);
					break;
				case JobComInvoiceLine.Schema.US_NHTSAIndicator:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.NHTSA);
					break;
				case JobComInvoiceLine.Schema.US_OMCInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.OMC);
					break;
				case JobComInvoiceLine.Schema.US_TTBInd:
					shouldRemoveIndicator = !indicatorRelevantFunc(GovernmentAgencyProgramCodeList.Codes.TTB);
					break;
			}

			return shouldRemoveIndicator;
		}
	}
}
