using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CalculationExhibitDocLineCollection))]
	class CalculationExhibitDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CalculationExhibitDocLineCollection>
	{
		protected override CalculationExhibitDocLineCollection GetCollectionToTest()
		{
			var invoiceLine = Drawback.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			var supporter = new JobDeclarationDrawbackSupporter(Drawback, US.Messaging.Business.UpdateActionCode.Add);
			return new CalculationExhibitDocLineCollection(supporter, x => x.Claims.HMFClaim);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Drawback.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			return new CalculationExhibitDocLine(invoiceLine.Claims.HMFClaim);
		}

		JobDeclaration Drawback
		{
			get
			{
				if (fDrawback == null)
				{
					fDrawback = Factory.New<JobDeclaration>();
					fDrawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					fDrawback.Invoices.AddNew();
				}
				return fDrawback;
			}
		}
		JobDeclaration fDrawback;
	}
}
