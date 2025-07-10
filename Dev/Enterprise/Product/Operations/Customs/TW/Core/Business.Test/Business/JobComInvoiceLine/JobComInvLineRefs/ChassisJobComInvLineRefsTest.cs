using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ChassisJobComInvLineRefs))]
	sealed class ChassisJobComInvLineRefsTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestJI_HasCatalystConverter_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(LineRefs.JG_ReferenceNumberInfo, "Chassis No", "The identification number of the car at the time of manufacture.");
		}

		public void TestSetDefaultValues()
		{
			var jobComInvLineRefs = Factory.New<ChassisJobComInvLineRefs>();
			AssertEquals(JobComInvLineRefsType.Codes.Chassis, jobComInvLineRefs.JG_ReferenceType);
		}

		public void TestJG_ReferenceNumberMaxLength()
		{
			AssertEquals(18, LineRefs.JG_ReferenceNumberInfo.MaxLength);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(ChassisJobComInvLineRefsValidation), LineRefs.Validation.GetType());
		}

		public void TestInvoiceLine()
		{
			AssertSame(InvoiceLine, LineRefs.InvoiceLine);
		}

		public void TestDeleteIfReferenceNumberIsEmpty()
		{
			ChassisJobComInvLineRefs lineRefsNo1 = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			ChassisJobComInvLineRefs lineRefsNo2 = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			lineRefsNo2.JG_ReferenceNumber = "REF-NUMBER2";
			Factory.Save();
			AssertEquals("chassis with empty reference is Deleted", true, lineRefsNo1.IsDeleted);
			AssertEquals("chassis with reference stays", false, lineRefsNo2.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return LineRefs;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var lineRefs = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			lineRefs.JG_ReferenceNumber = "REF";
			return lineRefs;
		}

		ChassisJobComInvLineRefs LineRefs
		{
			get
			{
				if (lineRefs == null)
				{
					lineRefs = InvoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				}

				return lineRefs;
			}
		}

		ChassisJobComInvLineRefs lineRefs;
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
