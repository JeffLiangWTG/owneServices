using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DutyCalculationExhibitDocLine))]
	class DutyCalculationExhibitDocLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDutyCalculationExhibitDocLineMembers()
		{
			var supporterMock = new Mock<IDutyCalculationExhibitsSupporter>();
			supporterMock.Setup(m => m.ImportEntryOrCMDNo).Returns("XJ5-700306690-1");
			supporterMock.Setup(m => m.InvoiceNo).Returns("WI23100-TC34");
			supporterMock.Setup(m => m.PartNo).Returns("1");
			supporterMock.Setup(m => m.ImportQuantity).Returns(1500m);
			supporterMock.Setup(m => m.ExportQuantity).Returns(1400m);
			supporterMock.Setup(m => m.Description).Returns("FISH");
			supporterMock.Setup(m => m.PerUnit).Returns(10m);
			supporterMock.Setup(m => m.AmountPaid).Returns(1000m);
			supporterMock.Setup(m => m.AmountClaimed).Returns(900m);
			supporterMock.Setup(m => m.ExportValue).Returns(2600m);
			supporterMock.Setup(m => m.DutyRate).Returns(0.25m);
			supporterMock.Setup(m => m.WeightedRatio).Returns(0.89125m);
			supporterMock.Setup(m => m.LineAmount).Returns(49.50m);
			supporterMock.Setup(m => m.LineAmountEligible).Returns(48m);
			supporterMock.Setup(m => m.CalculatedAmountForQtyUsed).Returns(8m);
			supporterMock.Setup(m => m.TotalLineValue).Returns(1200m);
			supporterMock.Setup(m => m.IndividualValue).Returns(1.25m);
			supporterMock.Setup(m => m.LineDutyRateDesc).Returns("25%");

			var docLine = new DutyCalculationExhibitDocLine(supporterMock.Object);
			AssertEquals(2600m, docLine.ExportValue);
			AssertEquals(25m, docLine.DutyRateInPercentage);
			AssertEquals(0.89125m, docLine.WeightedRatio);
			AssertEquals(49.50m, docLine.LineAmount);
			AssertEquals(48m, docLine.LineAmountEligible);
			AssertEquals(8m, docLine.CalculatedAmountForQtyUsed);
			AssertEquals(1200m, docLine.TotalLineValue);
			AssertEquals(1.25m, docLine.IndividualValue);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			return new DutyCalculationExhibitDocLine(invoiceLine.Claims.DutyClaim);
		}
	}
}
