using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedBill))]
	public class FDARelatedBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadOnlyRightType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_HouseBill = "HB10020023";
			declaration.JE_MasterBill = "12599675660";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUZ432809";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.FDAs.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			fda.BillsForFDALine.AddPivotFor(declaration.PrimaryHouseBill);
			fda.ContainersForFDALine.AddPivotFor(invoiceLine.ContainersPivot[0]);

			Factory.Save();

			var declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			AssertEquals(1, declarationLoaded.InvoiceLines[0].FDAs[0].BillsForFDALine.Count);
			AssertEquals(1, declarationLoaded.InvoiceLines[0].FDAs[0].ContainersForFDALine.Count);
		}

		public void TestNotAddedToFactoryCache()
		{
			var relatedBill = SetRelatedBill();
			AssertEquals("Data should not be cached in the factory", 0, Factory.GetBizOsForPK(relatedBill.PK.ToGuid()).Length);
		}

		public void TestSetBillAndGenPivot()
		{
			FDARelatedBill relatedBill = SetRelatedBill();
			AssertEquals(false, relatedBill.IsForFDALine);
			AssertEquals(":9999", relatedBill.BillNumber);
			AssertNull(relatedBill.Pivot);

			relatedBill.IsForFDALine = true;
			AssertNotNull(relatedBill.Pivot);
		}

		public void TestIsForFDALine()
		{
			FDARelatedBill relatedBill = SetRelatedBill();

			FDA.BillsAvailable.Add(relatedBill);
			relatedBill.IsForFDALine = true;
			AssertEquals(1, FDA.BillsForFDALine.Count);

			relatedBill.IsForFDALine = false;
			AssertEquals(0, FDA.BillsForFDALine.Count);
		}

		public void TestValidateIsForFDALine()
		{
			Bill bill = Factory.New<Bill>();
			bill.CU_BillNum = "9999";

			FDARelatedBill relatedBill = new FDARelatedBill(FDA);
			relatedBill.SetBill(bill);
			FDA.ContainersForInvoiceLine.Add(relatedBill);
			FDARelatedBillsGenPivot pivot1 = FDA.BillsForFDALine.AddNew();
			pivot1.Relation2Object = bill;
			FDARelatedBillsGenPivot pivot2 = FDA.BillsForFDALine.AddNew();
			pivot2.Relation2Object = bill;
			relatedBill.IsForFDALine = true;
			AssertHasErrorContaining(relatedBill.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related bill.");
			AssertEquals(pivot1, relatedBill.Pivot);

			relatedBill.IsForFDALine = false;
			AssertNoErrorContaining(relatedBill.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related bill.");
			AssertEquals(pivot2, relatedBill.Pivot);
			AssertEquals(true, pivot1.IsDeleted);

			relatedBill.IsForFDALine = true;
			AssertNoErrorContaining(relatedBill.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related bill.");
			AssertEquals(pivot2, relatedBill.Pivot);

			relatedBill.IsForFDALine = false;
			AssertNoErrorContaining(relatedBill.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related bill.");
			AssertNull(relatedBill.Pivot);
			AssertEquals(true, pivot2.IsDeleted);
		}

		public void TestValidateIsForFDALineForSeveralBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_HouseBill = "HB10020023";
			declaration.JE_MasterBill = "12599675660";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillNum = "HB2";
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillNum = "MB10050";
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.FDAs.AddNew();
			var fdaBillsAvailable = fda.BillsAvailable;
			AssertEquals("Precondition: 4 Bills available for selection in FDA line", 4, fdaBillsAvailable.Count);
			AssertEquals("Precondition: no Bills selected for FDA Line", 0, fda.BillsForFDALine.Count);
			fdaBillsAvailable[2].IsForFDALine = true;
			AssertNoMessageError("No Notifications expected, only one Bill selected for FDA Line",
								fdaBillsAvailable[2].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);

			fdaBillsAvailable[1].IsForFDALine = true;
			AssertNoMessageError("No Notifications expected, even second Bill selected for FDA Line, because Stand Alone Prior Notice not selected",
								fdaBillsAvailable[1].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);

			declaration.US_EnableSPN = true;

			fdaBillsAvailable[1].ValidateIsForFDALine();
			AssertHasMessageError(fdaBillsAvailable[1].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);

			fdaBillsAvailable[2].ValidateIsForFDALine();
			AssertHasMessageError(fdaBillsAvailable[2].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
		}

		FDARelatedBill SetRelatedBill()
		{
			Bill bill = Factory.New<Bill>();
			bill.CU_BillNum = "9999";

			FDARelatedBill result = new FDARelatedBill(FDA);
			result.SetBill(bill);

			return result;
		}

		FDA FDA
		{
			get
			{
				if (fda == null)
				{
					fda = InvoiceLine.FDAs.AddNew();
				}
				return fda;
			}
		}
		FDA fda;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableCRL = true;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FDARelatedBill(Factory.New<FDA>());
		}
	}
}
