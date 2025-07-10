using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System.Collections.Generic;
	using Enterprise.Customs.NZ.Business.Declaration;
	using NUnit.Framework;

	[TestedType(typeof(CommodityCode))]
	public class CommodityCodeTest : Customs.Business.Testing.CusCodeDataTest<CommodityCode>
	{
		public void TestHumanReadableNameForCY_Data()
		{
			CommodityCode uscAffirmCode = Factory.New<CommodityCode>();
			uscAffirmCode.CY_Code = "AAA";
			AssertEquals("TSW (AAA)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "";
			AssertEquals("CommodityLine", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "BBB";
			AssertEquals("TSW (BBB)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			CommodityCode copyCode = Factory.New<CommodityCode>();
			copyCode.CopyPersistentValuesFrom(uscAffirmCode);
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);

			copyCode = (CommodityCode)uscAffirmCode.Clone();
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);
		}

		public void TestCY_Data()
		{
			CommodityCode commodityCode = Factory.New<CommodityCode>();
			commodityCode.CY_Data = "dsfsdf";
			AssertEquals("DSFSDF", commodityCode.CY_Data);
		}

		public void TestSetDefaultValues()
		{
			CommodityCode commodityCode = Factory.New<CommodityCode>();
			AssertEquals("NZTSWCommodityData Type", "TCD", commodityCode.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityCodes.AddNew();
		}

		protected override IEnumerable<CommodityCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var commodityLine = invoiceLine.CommodityLines.AddNew();
			yield return commodityLine.CommodityCodes.AddNew();

			var commodityConstituent = invoiceLine.CommodityConstituents.AddNew();
			yield return commodityConstituent.CommodityCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var commodityLine = invoiceLine.CommodityLines.AddNew();
			return commodityLine.CommodityCodes.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					fInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					fInvoiceLine.CommodityLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		CommodityCodeCollection CommodityCodes
		{
			get
			{
				if (fCommodityCodes == null)
				{
					fCommodityCodes = new CommodityCodeCollection(InvoiceLine.CommodityLines.AddNew());
				}
				return fCommodityCodes;
			}
		}
		CommodityCodeCollection fCommodityCodes;
	}
}
