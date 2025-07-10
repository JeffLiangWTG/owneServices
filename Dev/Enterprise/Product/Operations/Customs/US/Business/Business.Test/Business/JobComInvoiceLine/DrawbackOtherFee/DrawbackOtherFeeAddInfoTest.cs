using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackOtherFeeAddInfo))]
	sealed class DrawbackOtherFeeAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._51;

			var drawbackInvoice = drawback.Invoices.AddNew();
			var drawbackInvoiceLine = drawbackInvoice.InvoiceLines.AddNew();
			var otherFee = drawbackInvoiceLine.DrawbackOtherFees.AddNew();
			return new DrawbackOtherFeeAddInfo(otherFee.B7_AddInfoDataInfo);
		}
	}
}
