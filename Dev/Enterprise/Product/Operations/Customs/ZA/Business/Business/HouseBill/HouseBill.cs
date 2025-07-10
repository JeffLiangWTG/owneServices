using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class Bill : TypeSafeBill, Integration.Customs.ZA.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
		{
			return new CusDecHouseBillLookups(this);
		}

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			return new CusDecHouseBillValidation(this);
		}

		public override ZDateTime CU_IssueDate
		{
			get => base.CU_IssueDate;
			set
			{
				var originalValue = base.CU_IssueDate;
				base.CU_IssueDate = value;
				if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled && Declaration != null && !IsCopying && originalValue != value && IsHouseBill && Declaration.Bills.PrimaryHouseBill == this && Declaration.IsImport)
				{
					Declaration.JE_ValuationDate = base.CU_IssueDate.Date;
				}
			}
		}
	}
}
