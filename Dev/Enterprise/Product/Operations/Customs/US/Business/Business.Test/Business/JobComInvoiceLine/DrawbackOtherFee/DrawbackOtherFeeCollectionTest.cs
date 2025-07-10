using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackOtherFeeCollection))]
	public class DrawbackOtherFeeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasDuplicateFeeType()
		{
			OtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OtherFee, ZDecimal.Zero);
			AssertEquals(false, OtherFees.HasDuplicateFeeType(DrawbackOtherFeeTypesList.Codes.OtherFee));

			var otherFee = OtherFees.AddNew();
			otherFee.US_FeeType = DrawbackOtherFeeTypesList.Codes.OtherFee;
			AssertEquals(true, OtherFees.HasDuplicateFeeType(DrawbackOtherFeeTypesList.Codes.OtherFee));
		}

		public void TestAddNewOrUpdate()
		{
			OtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OtherFee, 100m);
			AssertEquals(1, OtherFees.Count);
			var otherFee = OtherFees[DrawbackOtherFeeTypesList.Codes.OtherFee];
			AssertNotNull(otherFee);
			AssertEquals(100m, otherFee.DeclaredAmount);

			OtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OtherFee, 200m);
			AssertEquals(1, OtherFees.Count);
			otherFee = OtherFees[DrawbackOtherFeeTypesList.Codes.OtherFee];
			AssertNotNull(otherFee);
			AssertEquals(200m, otherFee.DeclaredAmount);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return OtherFees;
		}

		DrawbackOtherFeeCollection OtherFees
		{
			get { return otherFees ?? (otherFees = InvoiceLine.DrawbackOtherFees); }
		}
		DrawbackOtherFeeCollection otherFees;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine.US_DRWClaimAmountOverriden_New = true;
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
