using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ReportingBookAccountingJournalPrintOptionValidation
	{
		public ReportingBookAccountingJournalPrintOptionValidation(ReportingBookAccountingJournalPrintOption parent)
		{
			Parent = parent;
		}
		readonly ReportingBookAccountingJournalPrintOption Parent;

		public void ValidateAll()
		{
			ValidateReportingBookPK();
			ValidateDefault();
		}

		public void ValidateReportingBookPK()
		{
			Parent.ReportingBookInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.ReportingBookInfo);
			ListValidation.ErrorIfInvalidPK(Parent.ReportingBookInfo, Parent.Lookups.AccReportingBookList);

			if (!Parent.ReportingBookInfo.HasErrors()
				&& ParentCollection.Count(x => x.ReportingBook == Parent.ReportingBook) > 1)
			{
				Parent.ReportingBookInfo.AddError(Res.GetString("E5593D1F-EE43-403D-B171-896301262462", "At least one record has been set for the same reporting book."));
			}
		}

		public void ValidateDefault()
		{
			Parent.DefaultInfo.ClearAllNotifications();

			if (ParentCollection != null)
			{
				if (!ParentCollection.Any(x => x.Default))
				{
					Parent.DefaultInfo.AddError(Res.GetString("1844ABDE-AB81-480C-84C9-41BF543810AF", "At least one Reporting Book must be marked as 'Default'."));
				}

				if (ParentCollection.Count(x => x.Default) > 1)
				{
					Parent.DefaultInfo.AddError(Res.GetString("A249604A-2995-4055-9477-C35698F22F38", "Only one default reporting book can be set."));
				}
			}
		}

		public IEnumerable<ReportingBookAccountingJournalPrintOption> ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)Parent).ParentCollections.Length > 0)
				{
					return ((IBusinessObjectInternals)Parent).ParentCollections[0].Cast<ReportingBookAccountingJournalPrintOption>();
				}
				else
				{
					return null;
				}
			}
		}
	}
}
