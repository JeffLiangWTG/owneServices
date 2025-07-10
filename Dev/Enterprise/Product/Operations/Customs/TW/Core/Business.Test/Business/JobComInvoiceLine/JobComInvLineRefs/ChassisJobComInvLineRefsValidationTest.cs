using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ChassisJobComInvLineRefsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ChassisJobComInvLineRefsCollection.AddNew();
			AssertEquals("Parent", parent, parent.Validation.Parent);
		}

		public void TestCheckJG_ReferenceNumber()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var lineRefs = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			lineRefs.JG_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(lineRefs.JG_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			lineRefs.JG_ReferenceNumber = "no123";
			AssertNoMessageErrorContaining(lineRefs.JG_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJG_ReferenceNumberMustBeUnique()
		{
			const string duplicateChassisNumberWarning = "This chassis number already exists in this invoice line";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "InvoiceNumber1";
				var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				var lineRefs1 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				lineRefs1.JG_ReferenceNumber = "1234";
				var lineRefs2 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				lineRefs2.JG_ReferenceNumber = "AAAA";
				AssertNoWarningContaining(lineRefs2.JG_ReferenceNumberInfo, duplicateChassisNumberWarning);
				var lineRefsEqual1 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				lineRefsEqual1.JG_ReferenceNumber = "1234";
				AssertHasWarningContaining(lineRefsEqual1.JG_ReferenceNumberInfo, duplicateChassisNumberWarning);
				declaration.RunPreSaveValidation();
				Factory.Save();
				AssertHasWarningContaining(lineRefs1.JG_ReferenceNumberInfo, duplicateChassisNumberWarning);
				lineRefsEqual1.JG_ReferenceNumber = "12345";
				declaration.RunPreSaveValidation();
				Factory.Save();
				AssertNoWarningContaining(lineRefs1.JG_ReferenceNumberInfo, duplicateChassisNumberWarning);
			}
		}

		public void TestChassisNumberBaseOnInvoiceQuantity()
		{
			string chassisNumberMessageError = "The number of chassis number must be equals to invoice quantity.";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				invoiceLine.JI_InvoiceQuantity = 2;
				var chassisNumber1 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				var chassisNumber2 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				chassisNumber1.JG_ReferenceNumber = "22";
				chassisNumber2.JG_ReferenceNumber = "23";
				var lastChassisNumber = invoiceLine.ChassisJobComInvLineRefsCollection[1];
				AssertNoMessageErrorContaining(lastChassisNumber.JG_ReferenceNumberInfo, chassisNumberMessageError);
				var chassisNumber3 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
				chassisNumber3.JG_ReferenceNumber = "AAA";
				lastChassisNumber = invoiceLine.ChassisJobComInvLineRefsCollection[2];
				AssertHasMessageErrorContaining(lastChassisNumber.JG_ReferenceNumberInfo, chassisNumberMessageError);
				invoiceLine.JI_InvoiceQuantity = 3;
				lastChassisNumber.JG_ReferenceNumber = "XXX";
				AssertNoMessageErrorContaining(lastChassisNumber.JG_ReferenceNumberInfo, chassisNumberMessageError);
				invoiceLine.JI_InvoiceQuantity = 4;
				lastChassisNumber = invoiceLine.ChassisJobComInvLineRefsCollection[2];
				lastChassisNumber.JG_ReferenceNumber = "123456XXX";
				AssertHasMessageErrorContaining(lastChassisNumber.JG_ReferenceNumberInfo, chassisNumberMessageError);
				lastChassisNumber.JG_ReferenceNumber = ZString.Empty;
				AssertNoMessageErrorContaining(lastChassisNumber.JG_ReferenceNumberInfo, chassisNumberMessageError);
			}
		}
	}
}
