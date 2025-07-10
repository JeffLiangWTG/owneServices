using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityCodeCollection))]
	public class CommodityCodeCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CommodityCode>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		#region Implementation

		protected override Customs.Business.CusCodeDataCollection<CommodityCode> GetCusCodeDataCollection()
		{
			return CommodityCodes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommodityCode result = Factory.New<CommodityCode>();
			result.CY_ParentID = InvoiceLine.CommodityLines[0].PK;
			result.CY_ParentTableCode = "B7";
			return result;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					fInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					fInvoiceLine.CommodityLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		CommodityLine CommodityLine
		{
			get { return InvoiceLine.CommodityLines[0]; }
		}

		CommodityCodeCollection CommodityCodes
		{
			get { return CommodityLine.CommodityCodes; }
		}

		#endregion
	}
}
