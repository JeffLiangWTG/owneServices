using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceChargeCollection))]
	public class InvoiceChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<InvoiceChargeCollection, InvoiceCharge>
	{
		protected override InvoiceChargeCollection GetCollectionToTest() => (InvoiceChargeCollection)invoiceHeader.Charges;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<InvoiceCharge>();

		public void TestAdditionalFilter()
		{
			var testCollection = new InvoiceChargeCollection(invoiceHeader, charge => charge.J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalCultureCharge);
			invoiceHeader.Charges.RemoveAll();

			var testCharge = invoiceHeader.Charges.AddNew(TRIncotermChargeCodeList.Codes.LocalCultureCharge);
			((ICommonInvoice)invoiceHeader).AllCharges.Load();
			testCollection.Add(testCharge);
			testCollection.Rebuild();
			AssertSame("The item that fits the additional filter will be in the collection.", testCollection.First(), testCharge);

			testCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge;
			testCollection.Rebuild();
			AssertNull("When the item does not fit the additional filter any more it will be removed from the collection.", testCollection.Cast<object>().FirstOrDefault());
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobDeclaration>().Invoices.AddNew();
		}

		JobComInvoiceHeader invoiceHeader;
	}
}
