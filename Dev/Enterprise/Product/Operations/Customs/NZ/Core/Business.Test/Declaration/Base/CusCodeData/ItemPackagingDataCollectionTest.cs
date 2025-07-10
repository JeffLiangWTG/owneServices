using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(ItemPackagingDataCollection))]
	public class ItemPackagingtDataCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ItemPackagingData>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		#region Implementation

		protected override Customs.Business.CusCodeDataCollection<ItemPackagingData> GetCusCodeDataCollection()
		{
			return ItemPackages;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ItemPackagingData>();
			result.CY_ParentID = InvoiceLine.ItemPackages[0].PK;
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
					var invoice = Declaration.Invoices.AddNew();
					fInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					fInvoiceLine.ItemPackages.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		ItemPackaging ItemPackaging
		{
			get { return InvoiceLine.ItemPackages[0]; }
		}

		ItemPackagingDataCollection ItemPackages
		{
			get { return ItemPackaging.ItemPackages; }
		}

		#endregion
	}
}
