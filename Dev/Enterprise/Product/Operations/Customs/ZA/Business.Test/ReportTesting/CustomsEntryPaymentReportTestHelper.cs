using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class CustomsEntryPaymentReportTestHelper
	{
		public static List<string> ParametersValuesList
		{
			get
			{
				return new List<string>() { string.Format(CultureInfo.InvariantCulture, "'{0}'", GlbCompany.CurrentCompany.PK), // @CurrentCompany							
														ReportFunctionalTestCase.QuoteParameter(ZDateTime.Today.AddMonths(-6).ToISO8601String()), 	// 1 @FromDate
														ReportFunctionalTestCase.QuoteParameter(ZDateTime.Today.AddDays(1).ToISO8601String()),	// 2 @ToDate
														"'DEF'",	// 3  @PaymentType
													};
			}
		}
	}
}
