using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AssignedJobComInvLineRefs))]
	sealed class AssignedJobComInvLineRefsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var jobComInvLineRefs = Factory.New<AssignedJobComInvLineRefs>();
			AssertEquals(JobComInvLineRefsType.Codes.AssignedNumber, jobComInvLineRefs.JG_ReferenceType);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(AssignedJobComInvLineRefsValidation), LineRefs.Validation.GetType());
		}

		public void TestInvoiceLine()
		{
			AssertSame(InvoiceLine, LineRefs.InvoiceLine);
		}

		public void TestSupportsClone()
		{
			Assert("Should support clone", LineRefs.SupportsClone());
		}

		[ExpectNoExceptions]
		public void TestJG_ReferenceNumber_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(LineRefs.JG_ReferenceNumberInfo, "Assigned Number", "The assigned number by the authority for the specific goods.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return LineRefs;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return LineRefs;
		}

		AssignedJobComInvLineRefs LineRefs
		{
			get
			{
				if (lineRefs == null)
				{
					lineRefs = InvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
					lineRefs.JG_ReferenceNumber = "1";
				}

				return lineRefs;
			}
		}

		AssignedJobComInvLineRefs lineRefs;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
