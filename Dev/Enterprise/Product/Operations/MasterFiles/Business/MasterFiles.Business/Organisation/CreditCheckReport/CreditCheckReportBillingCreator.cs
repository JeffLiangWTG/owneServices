using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;

namespace Enterprise.MasterFiles.Business
{
	public static class CreditCheckReportBillingCreator
	{
		public static void CreateBillingTransaction(DbConnection dbConnection, string priceItemCode, string category, string reportSource, OrgHeader orgHeader, string duns)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var billingTransaction = new BillingTransaction();
			billingTransaction.ReportingSource = BillingManager.ReportingSource;
			billingTransaction.Category = category;
			billingTransaction.PriceItemCode = priceItemCode;
			billingTransaction.BillableCount = 1;
			billingTransaction.ReportingSource = reportSource;
			billingTransaction.ServiceOccuredUTC = ZDateTime.UtcNow.ToDateTime();
			billingTransaction.ClientID = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			billingTransaction.Branch = GlbBranch.CurrentBranch.GB_Code;
			billingTransaction.ClientNumber = registrationKey.SystemId + "." + GlbCompany.CurrentCompany.GC_Code;
			billingTransaction.ClientStaffCode = GlbStaff.CurrentUser.GS_Code;

			billingTransaction.Reference1 = orgHeader.PK.ToString();
			billingTransaction.Reference2 = duns;
			billingTransaction.Reference3 = orgHeader.CountryCode;
			billingTransaction.Reference4 = Env.Instance.CurrentCompany.Country.Code;
			billingTransaction.Reference5 = orgHeader.OH_FullName;

			billingTransaction.Version = 1;

			new BillingManager().AddTransactions(new[] { billingTransaction }, dbConnection);
		}
	}
}
