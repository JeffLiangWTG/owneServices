using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(AviationFuelType))]
	public class AviationFuelTypeTest : Customs.Business.Testing.CusSupportingInfoTest<AviationFuelType>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().AviationFuelTypeCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<AviationFuelType>();
			AssertEquals(CusSupportingInfoTypeList.Codes.AviationFuelType, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<AviationFuelType> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var aviationFuelType = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().AviationFuelTypeCollection.AddNew();
			aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
			aviationFuelType.CSI_Value = 1234567890123.12m;
			aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
			aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
			aviationFuelType.CSI_Description = "testtest";
			yield return aviationFuelType;
		}

		public void TestAviationFuelTypeCusSupportingSettingValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var aviationFuelType = invoiceLine.AviationFuelTypeCollection.AddNew();
			aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
			aviationFuelType.CSI_Value = 1234567890123.12m;
			aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
			aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
			aviationFuelType.CSI_Description = "testtest";

			CombineAssertions(() =>
			{
				AssertEquals("12345678901234567890", aviationFuelType.CSI_ReferenceNumber);
				AssertEquals(1234567890123.12m, aviationFuelType.CSI_Value);
				AssertEquals("12345678901", aviationFuelType.CSI_ReferenceNumber2);
				AssertEquals("testtest", aviationFuelType.CSI_Description);
			});
		}
	}
}
