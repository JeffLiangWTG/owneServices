using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	public class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		public override void TestAddingNewPart()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusClassification cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "8431.49.01.00B";
			invoiceLine.JI_Tariff = "8431.49.05.00H";
			invoiceLine.JI_CC = cusClass.PK;
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			OrgSupplierPart part = collection.AddNew();
			AssertEquals(1, part.PivotsForBinding.Count);
			var pivot = part.PivotsForBinding[0];
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals(cusClass.PK, pivot.CI_CC);
			AssertEquals("", pivot.CI_TariffNum);

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8431.49.05.00H";
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			part = collection.AddNew();
			AssertEquals(1, part.PivotsForBinding.Count);
			pivot = part.PivotsForBinding[0];
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals(ZGuid.Empty, pivot.CI_CC);
			AssertEquals("8431.49.05.00H", pivot.CI_TariffNum);
		}
	}
}
