using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainerInvoiceLinePivotBaseOnlyTest : TestCaseWithFactory
	{
		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusContainerInvoiceLinePivot>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "CONT32";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				var pivot = invoiceLine.ContainersPivot[0];
				AssertEquals("From Declaration", "NZ", (pivot as ITypeDeciderContext).Country);
			});
		}
	}
}
