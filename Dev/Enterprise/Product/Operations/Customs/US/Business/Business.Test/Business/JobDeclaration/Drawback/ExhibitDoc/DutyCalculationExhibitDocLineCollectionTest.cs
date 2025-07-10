using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DutyCalculationExhibitDocLineCollection))]
	class DutyCalculationExhibitDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DutyCalculationExhibitDocLineCollection>
	{
		protected override DutyCalculationExhibitDocLineCollection GetCollectionToTest()
		{
			var invoiceLine = Drawback.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			var supporter = new JobDeclarationDrawbackSupporter(Drawback, US.Messaging.Business.UpdateActionCode.Add);
			return new DutyCalculationExhibitDocLineCollection(supporter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Drawback.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			return new DutyCalculationExhibitDocLine(invoiceLine.Claims.DutyClaim);
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
