using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MiningInformation))]
	sealed class MiningInformationTest : Customs.Business.Testing.CusCodeDataTest<MiningInformation>
	{
		public void TestCY_TypeIsSet()
		{
			var miningInformation = Factory.New<MiningInformation>();
			AssertEquals(CusCodeDataTypeList.Codes.MiningInformationForSanctions, miningInformation.CY_Type);
		}

		public void TestParent()
		{
			var miningInformation = Factory.New<MiningInformation>();
			AssertNull(miningInformation.Parent);
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			miningInformation.CY_ParentID = invoiceLine.PK;
			miningInformation.CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			AssertEquals(invoiceLine, miningInformation.Parent);
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.MiningInformations.AddNew();
			Factory.Save();
			AssertEquals(0, invoiceLine.MiningInformations.Count);

			var miningInformation = invoiceLine.MiningInformations.AddNew();
			miningInformation.CY_Data = "US";
			Factory.Save();
			AssertEquals(1, invoiceLine.MiningInformations.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var result = invoiceLine.MiningInformations.AddNew();
			result.CY_Data = "US";
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var result = invoiceLine.MiningInformations.AddNew();
			result.CY_Data = "US";
			return result;
		}

		protected override IEnumerable<MiningInformation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			var result = invoiceLine.MiningInformations.AddNew();
			result.CY_Data = "US";
			yield return result;
		}
	}
}
