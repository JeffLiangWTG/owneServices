using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceTermTest : TestCaseWithFactory
	{
		#region TestInvoiceTerm

		public void TestInvoiceTerm()
		{
			AssertInvoiceTerm(new InvoiceTerm(), "", "", "", 0, "no value");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.CashOnDelivery, "Desc", 0), "", "COD", "Desc", 0, "COD");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.PaymentInAdvance, "Desc", 0), "", "PIA", "Desc", 0, "PIA");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "Desc", 0), "", "INV", "Desc", 0, "INV, 0 days");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "Desc", 1), "", "INV", "Desc", 1, "INV, 1 day");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.FromMonthEnd, "Desc", 5), "", "MTH", "Desc", 5, "MTH, 5 days");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "Desc", 0), "", "EWK", "Desc", 0, "EWK, 0 days");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "Desc", 5), "", "EWK", "Desc", 5, "EWK, 5 days");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, "Desc", 5), "", "MIC", "Desc", 5, "MIC, 5 months");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, "Desc", 1), "", "MIC", "Desc", 1, "MIC, 1 month");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, "Desc", 5), "", "DPC", "Desc", 5, "DPC, 5 days");
			AssertInvoiceTerm(new InvoiceTerm(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, "Desc", 1), "", "DPC", "Desc", 1, "DPC, 1 day");

			OrgARTerms arTerm = Factory.New<OrgARTerms>();
			arTerm.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			arTerm.PY_InvoiceDays = 3;
			AssertInvoiceTerm(new InvoiceTerm(arTerm), "ALL", "SHP", InvoiceTermsList.FromShipmentDate.Description, 3, "SHP, 3 days");

			arTerm.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			arTerm.PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertInvoiceTerm(new InvoiceTerm(arTerm), "DSB", "DEF", "", 0, "DEF, 0 days");
			AssertEquals("Description for DEF term is empty because to get description we must use a list without DEF term to avoid cyclic references on InvoiceTermList generation.", "", new InvoiceTerm(arTerm).TermDescription);
		}

		void AssertInvoiceTerm(InvoiceTerm invoiceTerm, ZString invoiceClass, ZString term, ZString termDescription, ZByte days, string toStringValue)
		{
			AssertEquals("InvoiceClass", invoiceClass, invoiceTerm.InvoiceClass);
			AssertEquals("Term", term, invoiceTerm.Term);
			AssertEquals("TermDescription", termDescription, invoiceTerm.TermDescription);
			AssertEquals("Days", days, invoiceTerm.Days);
			AssertEquals("ToString value", toStringValue, invoiceTerm.ToString());
		}

		#endregion

		public void TestIsEmpty()
		{
			AssertEquals(true, new InvoiceTerm().IsEmpty);
			AssertEquals(true, new InvoiceTerm("", "Desc", 10).IsEmpty);
			AssertEquals(false, new InvoiceTerm("XXX", "", 0).IsEmpty);
		}

		public void TestGetIsTermWithoutDays()
		{
			AssertEquals(true, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.CashOnDelivery));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.FromInvoiceDate));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.FromMonthEnd));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.FromWeekEnd));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.FromPeriodEnd));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.FromShipmentDate));
			AssertEquals(true, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.PaymentInAdvance));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate));
			AssertEquals(false, InvoiceTerm.GetIsTermWithoutDays(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle));
		}

		public void TestIsTermWithoutDays()
		{
			AssertEquals(true, new InvoiceTerm(Constants.InvoiceTerms.CashOnDelivery, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromMonthEnd, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromPeriodEnd, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromShipmentDate, "", 0).IsTermWithoutDays);
			AssertEquals(true, new InvoiceTerm(Constants.InvoiceTerms.PaymentInAdvance, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, "", 0).IsTermWithoutDays);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, "", 0).IsTermWithoutDays);
		}

		public void TestGetIsTermWitMonths()
		{
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.CashOnDelivery));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.FromInvoiceDate));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.FromMonthEnd));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.FromWeekEnd));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.FromPeriodEnd));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.FromShipmentDate));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.PaymentInAdvance));
			AssertEquals(true, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate));
			AssertEquals(false, InvoiceTerm.GetIsTermWitMonths(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle));
		}

		public void TestIsTermWithMonths()
		{
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.CashOnDelivery, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromMonthEnd, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromPeriodEnd, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.FromShipmentDate, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.PaymentInAdvance, "", 0).IsTermWithMonths);
			AssertEquals(true, new InvoiceTerm(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, "", 0).IsTermWithMonths);
			AssertEquals(false, new InvoiceTerm(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, "", 0).IsTermWithMonths);
		}

		public void TestGetARTermsCycleDueDate()
		{
			OrgARTerms arTerm = Factory.New<OrgARTerms>();
			arTerm.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			arTerm.PY_InvoiceDays = 5;
			arTerm.ARTermsCycles.AddNew().P5_PaymentDay = 3;
			InvoiceTerm invoiceTerm = new InvoiceTerm(arTerm);
			AssertEquals("Days", (ZByte)5, invoiceTerm.Days);
			AssertEquals("GetARTermsCycleDueDate doesn't return value if term is not 'MonthsFromInvoiceCycleDate'", ZDateTime.Empty, invoiceTerm.GetARTermsCycleDueDate(ZDateTime.BrettsBirthday, 0));

			arTerm.ARTermsCycles.DeleteAll();
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerm.ARTermsCycles.AddNew().P5_PaymentDay = 3;
			arTerm.PY_InvoiceDays = 2;
			AssertEquals("InvoiceTerm doesn't update term values, new InvoiceTerm have to be created.", (ZByte)5, invoiceTerm.Days);
			AssertEquals("InvoiceTerm doesn't update term values, new InvoiceTerm have to be created.", ZDateTime.Empty, invoiceTerm.GetARTermsCycleDueDate(ZDateTime.BrettsBirthday, 0));

			invoiceTerm = new InvoiceTerm(arTerm);
			AssertEquals("Days", (ZByte)2, invoiceTerm.Days);
			ZDateTime expectedMonth = ZDateTime.BrettsBirthday.AddMonths(1);
			AssertEquals(new ZDateTime(expectedMonth.Year, expectedMonth.Month, 3), invoiceTerm.GetARTermsCycleDueDate(ZDateTime.BrettsBirthday, 0));

			arTerm.Delete();
			AssertEquals("Days", (ZByte)2, invoiceTerm.Days);
			AssertEquals("GetARTermsCycleDueDate work correctly if OrgARTerm object was deleted.", ZDateTime.Empty, invoiceTerm.GetARTermsCycleDueDate(ZDateTime.BrettsBirthday, 0));
		}

		public void TestGetARPaymentCycleDueDate()
		{
			OrgARTerms arTerm = Factory.New<OrgARTerms>();
			arTerm.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			arTerm.PY_InvoiceDays = 5;
			arTerm.ARPaymentCycles.AddNew().P5_PaymentDay = 3;
			InvoiceTerm invoiceTerm = new InvoiceTerm(arTerm);
			AssertEquals("Days", (ZByte)5, invoiceTerm.Days);
			AssertEquals("GetARPaymentCycleDueDate doesn't return value if term is not 'TermDaysAndDebtorPaymentCycle'", ZDateTime.Empty, invoiceTerm.GetARPaymentCycleDueDate(ZDateTime.BrettsBirthday, 0));

			arTerm.ARPaymentCycles.DeleteAll();
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			arTerm.ARPaymentCycles.AddNew().P5_PaymentDay = 3;
			arTerm.PY_InvoiceDays = 2;
			AssertEquals("InvoiceTerm doesn't update term values, new InvoiceTerm have to be created.", (ZByte)5, invoiceTerm.Days);
			AssertEquals("InvoiceTerm doesn't update term values, new InvoiceTerm have to be created.", ZDateTime.Empty, invoiceTerm.GetARPaymentCycleDueDate(ZDateTime.BrettsBirthday, 0));

			invoiceTerm = new InvoiceTerm(arTerm);
			AssertEquals("Days", (ZByte)2, invoiceTerm.Days);
			ZDateTime expectedMonth = ZDateTime.BrettsBirthday.AddMonths(1);
			AssertEquals(new ZDateTime(expectedMonth.Year, expectedMonth.Month, 3), invoiceTerm.GetARPaymentCycleDueDate(ZDateTime.BrettsBirthday, 0));

			arTerm.Delete();
			AssertEquals("Days", (ZByte)2, invoiceTerm.Days);
			AssertEquals("GetARPaymentCycleDueDate work correctly if OrgARTerm object was deleted.", ZDateTime.Empty, invoiceTerm.GetARPaymentCycleDueDate(ZDateTime.BrettsBirthday, 0));
		}
	}
}
