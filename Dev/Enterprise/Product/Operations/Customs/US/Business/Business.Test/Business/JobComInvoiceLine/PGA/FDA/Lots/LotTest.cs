using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Lot))]
	public class LotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<Lot>
	{
		public void TestAddInfo()
		{
			var addInfo = new USLotAddInfo(Lot.B7_AddInfoDataInfo);
			AssertNotNull(addInfo);
			AssertEquals(addInfo.Parent.GetHashCode(), Lot.GetHashCode());
		}

		public void TestTemperature()
		{
			Lot.US_Temperature = 0.01m;
			AssertEquals("000001", ((IFDALot)Lot).Temperature);
			Lot.US_Temperature = 0.1m;
			AssertEquals("000010", ((IFDALot)Lot).Temperature);
			Lot.US_Temperature = 1.01m;
			AssertEquals("000101", ((IFDALot)Lot).Temperature);
			Lot.US_Temperature = 1010.01m;
			AssertEquals("101001", ((IFDALot)Lot).Temperature);
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			cpsc.Lots.AddNew();
			var lot = cpsc.Lots.AddNew();
			lot.US_LotNumber = "Test";
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.Lots.AddNew();
			Factory.Save();

			AssertEquals(1, cpsc.Lots.Count);
			AssertEquals("Test", cpsc.Lots[0].US_LotNumber);
			AssertEquals(1, fda.Lots.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Lot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Lot;
		}

		protected override IEnumerable<Lot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			yield return fda.Lots.AddNew();
		}

		Lot Lot
		{
			get
			{
				if (lot == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var fda = invoiceLine.ACE_FDALines.AddNew();
					lot = fda.Lots.AddNew();
				}
				return lot;
			}
		}
		Lot lot;
	}
}
