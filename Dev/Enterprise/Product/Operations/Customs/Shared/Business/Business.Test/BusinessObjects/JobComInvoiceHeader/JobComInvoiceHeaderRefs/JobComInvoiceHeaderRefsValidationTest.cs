using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class JobComInvoiceHeaderRefsValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckJ2_ReferenceType()
		{
			var ref1 = RefCollection.AddNew();
			ref1.J2_ReferenceType = "SS";
			AssertHasWarningContaining(ref1.J2_ReferenceTypeInfo, ListValidation.InvalidCodeMessage);

			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			AssertNoWarningContaining(ref1.J2_ReferenceTypeInfo, ListValidation.InvalidCodeMessage);

			var ref2 = RefCollection.AddNew();
			ref2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.RP;
			AssertNoErrorContaining(ref2.J2_ReferenceTypeInfo, JobComInvoiceHeaderRefsValidation.OnlyOneRPAllowedMessage);

			var ref3 = RefCollection.AddNew();
			ref3.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.RP;
			AssertHasErrorContaining(ref3.J2_ReferenceTypeInfo, JobComInvoiceHeaderRefsValidation.OnlyOneRPAllowedMessage);

			ref3.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			AssertNoErrorContaining(ref3.J2_ReferenceTypeInfo, JobComInvoiceHeaderRefsValidation.OnlyOneRPAllowedMessage);
		}

		public virtual void TestCheckJ2_ReferenceNumber()
		{
			var ref1 = RefCollection.AddNew();
			ref1.J2_ReferenceNumber = "";
			AssertHasWarning(ref1.J2_ReferenceNumberInfo, JobComInvoiceHeaderRefsValidation.NumberIsEmpty);

			ref1.J2_ReferenceNumber = "FTR45634";
			AssertNoWarning(ref1.J2_ReferenceNumberInfo, JobComInvoiceHeaderRefsValidation.NumberIsEmpty);
		}

		InvoiceHeaderRefsCollection RefCollection => refCollection ??= new InvoiceHeaderRefsCollection(Factory.New<BaseJobComInvoiceHeader>());
		InvoiceHeaderRefsCollection refCollection;
	}
}
