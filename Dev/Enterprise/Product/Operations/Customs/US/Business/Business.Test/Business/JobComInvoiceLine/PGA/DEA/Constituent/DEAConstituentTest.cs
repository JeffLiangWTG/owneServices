using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DEAConstituent))]
	internal class DEAConstituentTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DEAConstituent>
	{
		public void TestIDEAConstituentMembers()
		{
			var constituent = Factory.New<DEAConstituent>();
			constituent.US_Weight = 1m;
			constituent.US_WeightUQ = "G";
			constituent.US_ProductCode = "PRO";

			var deaConstituent = (IDEAConstituent)constituent;
			AssertEquals(1m, deaConstituent.Weight);
			AssertEquals("G", deaConstituent.WeightUQ);
			AssertEquals("PRO", deaConstituent.ProductCode);

			constituent.US_Weight = 9999999999.9999m;
			AssertEquals(0m, deaConstituent.Weight);
		}

		public void TestUS_WeightDecimalPlaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var constituent = invoiceLine.DEAHeaders.AddNew().Constituents.AddNew();
			AssertEquals(4, MetaData.GetDecimalPlaces(constituent, constituent.US_WeightInfo.PropertyDescriptor));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(2, MetaData.GetDecimalPlaces(constituent, constituent.US_WeightInfo.PropertyDescriptor));
		}

		protected override IEnumerable<DEAConstituent> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			yield return deaHeader.FirstConstituent;
		}
	}
}
