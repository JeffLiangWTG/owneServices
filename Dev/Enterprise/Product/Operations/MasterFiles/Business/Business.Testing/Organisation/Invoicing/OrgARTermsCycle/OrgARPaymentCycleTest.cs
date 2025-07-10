using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgARPaymentCycle))]
	sealed class OrgARPaymentCycleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals((ZByte)1, arPaymentCycle.P5_ToDay);
			AssertEquals((ZByte)1, arPaymentCycle.P5_PaymentDay);
		}

		public void TestCycle()
		{
			AssertEquals("1st row ToDay", (ZByte)1, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)1, arPaymentCycle.P5_Cycle);

			OrgARPaymentCycle arPaymentCycle2 = arTerms.ARPaymentCycles.AddNew();
			AssertEquals("2nd row ToDay", (ZByte)1, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)1, arPaymentCycle2.P5_Cycle);

			arPaymentCycle2.P5_PaymentDay = 10;
			AssertEquals("1st row ToDay", (ZByte)1, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)1, arPaymentCycle.P5_Cycle);
			AssertEquals("2nd row ToDay", (ZByte)2, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)2, arPaymentCycle2.P5_Cycle);

			Factory.Save();

			arPaymentCycle.P5_PaymentDay = 15;
			AssertEquals("1st row ToDay", (ZByte)2, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)2, arPaymentCycle.P5_Cycle);
			AssertEquals("2nd row ToDay", (ZByte)1, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)1, arPaymentCycle2.P5_Cycle);

			arPaymentCycle2.P5_PaymentDay = 31;
			AssertEquals("1st row ToDay", (ZByte)1, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)1, arPaymentCycle.P5_Cycle);
			AssertEquals("2nd row ToDay", (ZByte)2, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)2, arPaymentCycle2.P5_Cycle);

			Factory.Save();

			OrgARPaymentCycle arPaymentCycle3 = arTerms.ARPaymentCycles.AddNew();
			AssertEquals("1st row ToDay", (ZByte)1, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)1, arPaymentCycle.P5_Cycle);
			AssertEquals("2nd row ToDay", (ZByte)2, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)2, arPaymentCycle2.P5_Cycle);
			AssertEquals("3rd row ToDay", (ZByte)1, arPaymentCycle3.P5_ToDay);
			AssertEquals("3rd row Cycle", (ZByte)1, arPaymentCycle3.P5_Cycle);

			arPaymentCycle3.P5_PaymentDay = 1;
			AssertEquals("1st row ToDay", (ZByte)2, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)2, arPaymentCycle.P5_Cycle);
			AssertEquals("2nd row ToDay", (ZByte)3, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)3, arPaymentCycle2.P5_Cycle);
			AssertEquals("3rd row ToDay", (ZByte)1, arPaymentCycle3.P5_ToDay);
			AssertEquals("3rd row Cycle", (ZByte)1, arPaymentCycle3.P5_Cycle);

			arPaymentCycle3.P5_PaymentDay = 5;
			AssertEquals("1st row ToDay", (ZByte)2, arPaymentCycle.P5_ToDay);
			AssertEquals("1st row Cycle", (ZByte)2, arPaymentCycle.P5_Cycle);
			AssertEquals("2nd row ToDay", (ZByte)3, arPaymentCycle2.P5_ToDay);
			AssertEquals("2nd row Cycle", (ZByte)3, arPaymentCycle2.P5_Cycle);
			AssertEquals("3rd row ToDay", (ZByte)1, arPaymentCycle3.P5_ToDay);
			AssertEquals("3rd row Cycle", (ZByte)1, arPaymentCycle3.P5_Cycle);
		}

		public void TestMarkAsNeedValidationOnP5_PaymentDayChange()
		{
			OrgARPaymentCycle arPaymentCycle2 = arTerms.ARPaymentCycles.AddNew();
			OrgARPaymentCycle arPaymentCycle3 = arTerms.ARPaymentCycles.AddNew();
			arPaymentCycle.P5_PaymentDay = 10;
			arPaymentCycle2.P5_PaymentDay = 20;
			arPaymentCycle3.P5_PaymentDay = 31;
			AssertNoErrors(arPaymentCycle.P5_PaymentDayInfo);
			AssertNoErrors(arPaymentCycle2.P5_PaymentDayInfo);
			AssertNoErrors(arPaymentCycle3.P5_PaymentDayInfo);

			arPaymentCycle3.P5_PaymentDay = 20;
			AssertHasErrors("Precondition: ", arPaymentCycle3.P5_PaymentDayInfo);
			AssertNoErrors(arPaymentCycle.P5_PaymentDayInfo);
			AssertHasErrors("Terms cycle with the same 'Payment Day' value already exists.", arPaymentCycle2.P5_PaymentDayInfo);
		}

		#region TestP5_FromDateRefreshBindingOnP5_ToDayChange

		public void TestP5_ToDayRefreshBindingOnP5_PaymentDayChange()
		{
			OrgARPaymentCycle arPaymentCycle2 = arTerms.ARPaymentCycles.AddNew();
			arPaymentCycle2.P5_PaymentDay = 10;
			arPaymentCycle2.P5_ToDayInfo.ValueChanged += new EventHandler(P5_ToDayCalculatedInfo_ValueChanged);

			wasP5_ToDayInfo_ValueChangedCalled = false;
			arPaymentCycle.P5_PaymentDay = 20;
			AssertEquals("Refresh binding was called.", true, wasP5_ToDayInfo_ValueChangedCalled);
		}

		void P5_ToDayCalculatedInfo_ValueChanged(object sender, EventArgs e)
		{
			wasP5_ToDayInfo_ValueChangedCalled = true;
		}
		bool wasP5_ToDayInfo_ValueChangedCalled;

		#endregion

		public void TestMarkARTermAsNeedValidationOnDeleting()
		{
			arTerms.FillWithValidTestData();
			arTerms.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, arTerms.ShouldValidateOnSave);

			arPaymentCycle.Delete();
			AssertEquals("On deleting ARTermCycle parent ARTerm must be marked as need validation.", true, arTerms.ShouldValidateOnSave);
		}

		#region TestReadOnlyIfOrgReceivablesModifyPaymentTermsIsNotAllowed

		public void TestReadOnlyIfOrgReceivablesModifyPaymentTermsIsNotAllowed()
		{
			OrgHeader header = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			arPaymentCycle = header.CompanyData.LoadARTermForAllInvoiceTypes().ARPaymentCycles.AddNew();
			Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
			AssertPropertiesReadOnly(arPaymentCycle, false);

			header.Factory.Save();
			AssertPropertiesReadOnly(arPaymentCycle, true);

			Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
			AssertPropertiesReadOnly(arPaymentCycle, false);
		}

		void AssertPropertiesReadOnly(OrgARPaymentCycle bizo, bool isReadOnly)
		{
			var propertiesWithIncorrectReadOnlyState =
				from ZPropertyInfo property in bizo.ZPropertyInfoHash
				where property.ReadOnly != isReadOnly && property.HasSetter
				select property;
			AssertEquals(string.Format("All properties must be {0} readonly.", isReadOnly ? "" : "not"), 0, propertiesWithIncorrectReadOnlyState.Count());
		}

		#endregion

		OrgARTerms arTerms;
		OrgARPaymentCycle arPaymentCycle;

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			arTerms = org.CompanyData.ARTerms.AddNew();
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
		}
	}
}
