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
	[TestedType(typeof(OrgARTermsCycle))]
	sealed class OrgARTermsCycleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals((ZByte)31, arTermsCycle.P5_ToDay);
			AssertEquals((ZByte)1, arTermsCycle.P5_PaymentDay);
		}

		public void TestFromDay()
		{
			AssertEquals("1st row ToDay", (ZByte)1, arTermsCycle.P5_FromDayCalculated);

			OrgARTermsCycle arTermsCycle2 = arTerms.ARTermsCycles.AddNew();
			AssertEquals("2nd row ToDay", (ZByte)31, arTermsCycle2.P5_ToDay);
			AssertEquals("1st row FromDay", (ZByte)1, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("2nd row FromDay", (ZByte)1, arTermsCycle2.P5_FromDayCalculated);

			arTermsCycle.P5_ToDay = 4;
			AssertEquals("1st row FromDay", (ZByte)1, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("2nd row FromDay", (ZByte)5, arTermsCycle2.P5_FromDayCalculated);

			arTermsCycle2.P5_ToDay = 1;
			AssertEquals("1st row FromDay", (ZByte)2, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("2nd row FromDay", (ZByte)5, arTermsCycle2.P5_FromDayCalculated);

			arTermsCycle2.P5_ToDay = 4;
			AssertEquals("1st row FromDay", (ZByte)5, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("2nd row FromDay", (ZByte)5, arTermsCycle2.P5_FromDayCalculated);
		}

		public void TestMarkAsNeedValidationOnP5_ToDayChange()
		{
			OrgARTermsCycle arTermsCycle2 = arTerms.ARTermsCycles.AddNew();
			OrgARTermsCycle arTermsCycle3 = arTerms.ARTermsCycles.AddNew();
			arTermsCycle.P5_ToDay = 10;
			arTermsCycle2.P5_ToDay = 20;
			arTermsCycle3.P5_ToDay = 31;
			AssertNoErrors(arTermsCycle.P5_ToDayInfo);
			AssertNoErrors(arTermsCycle2.P5_ToDayInfo);
			AssertNoErrors(arTermsCycle3.P5_ToDayInfo);

			arTermsCycle3.P5_ToDay = 20;
			AssertHasErrors("Precondition: ", arTermsCycle3.P5_ToDayInfo);
			AssertNoErrors(arTermsCycle.P5_ToDayInfo);
			AssertHasErrors("Terms cycle with the same 'To Day' value already exists.", arTermsCycle2.P5_ToDayInfo);
		}

		#region TestP5_FromDateRefreshBindingOnP5_ToDayChange

		public void TestP5_FromDateRefreshBindingOnP5_ToDayChange()
		{
			OrgARTermsCycle arTermsCycle2 = arTerms.ARTermsCycles.AddNew();
			arTermsCycle2.P5_FromDayCalculatedInfo.ValueChanged += new EventHandler(P5_FromDayCalculatedInfo_ValueChanged);

			wasP5_FromDayCalculatedInfo_ValueChangedCalled = false;
			arTermsCycle.P5_ToDay = 10;
			AssertEquals("Refresh binding was called.", true, wasP5_FromDayCalculatedInfo_ValueChangedCalled);
		}

		void P5_FromDayCalculatedInfo_ValueChanged(object sender, EventArgs e)
		{
			wasP5_FromDayCalculatedInfo_ValueChangedCalled = true;
		}
		bool wasP5_FromDayCalculatedInfo_ValueChangedCalled;

		#endregion

		public void TestMarkARTermAsNeedValidationOnDeleting()
		{
			arTerms.FillWithValidTestData();
			arTerms.RunPreSaveValidation();
			AssertEquals("Precondition: ", false, arTerms.ShouldValidateOnSave);

			arTermsCycle.Delete();
			AssertEquals("On deleting ARTermCycle parent ARTerm must be marked as need validation.", true, arTerms.ShouldValidateOnSave);
		}

		#region TestReadOnlyIfOrgReceivablesModifyPaymentTermsIsNotAllowed

		public void TestReadOnlyIfOrgReceivablesModifyPaymentTermsIsNotAllowed()
		{
			OrgHeader header = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTermsCycle = header.CompanyData.LoadARTermForAllInvoiceTypes().ARTermsCycles.AddNew();
			Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
			AssertPropertiesReadOnly(arTermsCycle, false);

			header.Factory.Save();
			AssertPropertiesReadOnly(arTermsCycle, true);

			Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
			AssertPropertiesReadOnly(arTermsCycle, false);
		}

		void AssertPropertiesReadOnly(AutoOrgARTermsCycle bizo, bool isReadOnly)
		{
			var propertiesWithIncorrectReadOnlyState =
				from ZPropertyInfo property in bizo.ZPropertyInfoHash
				where property.ReadOnly != isReadOnly && property.HasSetter
				select property;
			AssertEquals(string.Format("All properties must be {0} readonly.", isReadOnly ? "" : "not"), 0, propertiesWithIncorrectReadOnlyState.Count());
		}

		#endregion

		OrgARTerms arTerms;

		OrgARTermsCycle arTermsCycle;

		protected override void SetUp()
		{
			base.SetUp();
			arTerms = Factory.New<OrgARTerms>();
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTermsCycle = arTerms.ARTermsCycles.AddNew();
		}
	}
}
