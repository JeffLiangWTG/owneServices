using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ReportingBookAccountingJournalPrintOptionLookups
	{
		public ReportingBookAccountingJournalPrintOptionLookups(ZGuid companyPK)
		{
			CompanyPK = companyPK;
		}

		public AccReportingBookCollection AccReportingBookList
		{
			get
			{
				if (accReportingBookList == null)
				{
					accReportingBookList = new AccReportingBookCollection(new BusinessObjectFactory(), CompanyPK);
				}
				return accReportingBookList;
			}
		}
		AccReportingBookCollection accReportingBookList;

		public ZGuid CompanyPK { get; }
	}
}
