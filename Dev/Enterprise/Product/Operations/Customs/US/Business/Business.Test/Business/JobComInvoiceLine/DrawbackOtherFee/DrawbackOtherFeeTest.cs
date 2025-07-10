using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackOtherFee))]
	public class DrawbackOtherFeeTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DrawbackOtherFee>
	{
		public void TestDrawbackOtherFeeMembers()
		{
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity = 100m;
			InvoiceLine.DRWImportUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.DRWExportQuantity = 1000m;
			InvoiceLine.DRWExportUQ = Core.Constants.Weight.Kilograms;

			var otherFee = InvoiceLine.DrawbackOtherFees.AddNew();
			otherFee.US_FeeType = DrawbackOtherFeeTypesList.Codes.OtherFee;
			otherFee.DeclaredAmount = 100m;
			AssertEquals(DrawbackOtherFeeTypesList.Codes.OtherFee, otherFee.US_FeeType);
			AssertEquals(100m, otherFee.DeclaredAmount);
			AssertEquals(1m, otherFee.FeeAmountPerUnit);
			AssertEquals(1000m, otherFee.ClaimedAmount);
			AssertEquals(990m, otherFee._99ClaimedAmount);
			AssertEquals(990m, otherFee.CalculatedAmount);

			otherFee.CalculatedAmount = 980m;
			AssertEquals(DrawbackOtherFeeTypesList.Codes.OtherFee, otherFee.US_FeeType);
			AssertEquals(100m, otherFee.DeclaredAmount);
			AssertEquals(1m, otherFee.FeeAmountPerUnit);
			AssertEquals(1000m, otherFee.ClaimedAmount);
			AssertEquals(990m, otherFee._99ClaimedAmount);
			AssertEquals(980m, otherFee.CalculatedAmount);

			otherFee._99ClaimedAmount = 970m;
			AssertEquals(DrawbackOtherFeeTypesList.Codes.OtherFee, otherFee.US_FeeType);
			AssertEquals(100m, otherFee.DeclaredAmount);
			AssertEquals(1m, otherFee.FeeAmountPerUnit);
			AssertEquals(1000m, otherFee.ClaimedAmount);
			AssertEquals(970m, otherFee._99ClaimedAmount);
			AssertEquals(980m, otherFee.CalculatedAmount);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var otherFee = invoiceLine.DrawbackOtherFees.AddNew();
			otherFee.US_FeeType = DrawbackOtherFeeTypesList.Codes.BeefFee;
			return otherFee;
		}

		protected override IEnumerable<DrawbackOtherFee> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var otherFee = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().DrawbackOtherFees.AddNew();
			otherFee.US_FeeType = DrawbackOtherFeeTypesList.Codes.OtherFee;
			otherFee.DeclaredAmount = 100m;
			yield return otherFee;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.DrawbackOtherFees.AddNew();
		}

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
				}

				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
