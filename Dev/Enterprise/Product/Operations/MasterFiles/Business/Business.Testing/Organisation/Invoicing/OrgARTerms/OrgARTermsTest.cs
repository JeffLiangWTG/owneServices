using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgARTerms))]
	sealed class OrgARTermsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeleteWithTermsCycles()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			var cycles = arTerms.ARTermsCycles.AddNew();
			arTerms.Delete();

			AssertEquals(true, cycles.IsDeleted);
		}

		public void TestDeleteWithPaymentCycles()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			var cycles = arTerms.ARPaymentCycles.AddNew();
			arTerms.Delete();

			AssertEquals(true, cycles.IsDeleted);
		}

		public void TestDeleteWithMultipleInstallments()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			var installments = arTerms.ARTermsInstallments.AddNew();
			arTerms.Delete();

			AssertEquals(true, installments.IsDeleted);
		}

		public void TestDefaultValues()
		{
			AssertEquals(defaultTerms.PY_InvoiceTerm, Constants.InvoiceTerms.CashOnDelivery);
		}

		public void TestCanDelete()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var cmpnyData = org1.CompanyData;
			cmpnyData.OB_IsDebtor = true;
			var arTerm = cmpnyData.ARTerms[0];
			SetupTermsInfo(arTerm, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			AssertEquals("One ARTerms record in a list can't be deleted.", false, arTerm.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", @"At least one term settings row with Job Type: ALL and Invoice Type: ALL must exist.", arTerm.ReasonForNotAbleToDelete);

			cmpnyData.ARTerms.AddNew();
			AssertEquals("Precondition:", 2, cmpnyData.ARTerms.Count);
			AssertEquals("ARTerms record can be deleted if it's not last in a list.", true, arTerm.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", arTerm.ReasonForNotAbleToDelete);
		}

		public void TestPY_JobType()
		{
			arTerms.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertEquals("PY_JobType", JobInvoicingConsumerTypes.ShipmentCode, arTerms.PY_JobType);
			AssertEquals("PY_Direction", "ALL", arTerms.PY_Direction);
			AssertEquals("PY_TransportMode", "ALL", arTerms.PY_TransportMode);
			Assert("PY_DirectionInfo not readonly", !arTerms.PY_DirectionInfo.ReadOnly);
			Assert("PY_TransportModeInfo not readonly", !arTerms.PY_TransportModeInfo.ReadOnly);

			arTerms.PY_JobType = "BRK";
			AssertEquals("PY_JobType", "BRK", arTerms.PY_JobType);
			AssertEquals("PY_Direction", "ALL", arTerms.PY_Direction);
			AssertEquals("PY_TransportMode", "ALL", arTerms.PY_TransportMode);
			Assert("PY_DirectionInfo not readonly", !arTerms.PY_DirectionInfo.ReadOnly);
			Assert("PY_TransportModeInfo not readonly", !arTerms.PY_TransportModeInfo.ReadOnly);
		}

		public void TestPY_InvoiceTerm()
		{
			arTerms.PY_InvoiceDays = 5;
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals("PY_InvoiceTerm", (short)5, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays not readonly", !arTerms.PY_InvoiceDaysInfo.ReadOnly);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals("PY_InvoiceTerm", (short)0, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays is readonly", arTerms.PY_InvoiceDaysInfo.ReadOnly);

			arTerms.PY_InvoiceDays = 15;
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
			AssertEquals("PY_InvoiceTerm", (short)15, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays not readonly", !arTerms.PY_InvoiceDaysInfo.ReadOnly);

			arTerms.PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals("PY_InvoiceTerm", (short)0, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays is readonly", arTerms.PY_InvoiceDaysInfo.ReadOnly);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			arTerms.ARTermsInstallments.AddNew();
			AssertEquals("PY_InvoiceTerm", (short)0, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays is readonly", arTerms.PY_InvoiceDaysInfo.ReadOnly);
		}

		public void TestPY_InvoiceTermWhenCreditNotApproved()
		{
			companyData.OB_AROnCreditHold = false;
			companyData.OB_ARCreditApproved = false;
			arTerms.PY_InvoiceDays = 0;
			arTerms.PY_InvoiceTerm = "COD";
			AssertNoErrors("invoice terms is 'COD', no errors", arTerms.PY_InvoiceTermInfo);

			arTerms.PY_InvoiceTerm = "INV";
			AssertEquals("PY_InvoiceTerm", "INV", arTerms.PY_InvoiceTerm);
			AssertNoErrors("invoice terms has no errors", arTerms.PY_InvoiceTermInfo);
			arTerms.PY_InvoiceDays = 5;
			AssertEquals("PY_InvoiceDays", (short)5, arTerms.PY_InvoiceDays);
			AssertNoErrors("invoice days has no errors", arTerms.PY_InvoiceDaysInfo);
		}

		public void TestPY_InvoiceTermWhenCreditOnHold()
		{
			companyData.OB_ARCreditApproved = true;
			companyData.OB_AROnCreditHold = true;
			arTerms.PY_InvoiceDays = 0;
			arTerms.PY_InvoiceTerm = "COD";
			AssertNoErrors("invoice terms is 'COD', no errors", arTerms.PY_InvoiceTermInfo);

			arTerms.PY_InvoiceTerm = "INV";
			AssertEquals("PY_InvoiceTerm", "INV", arTerms.PY_InvoiceTerm);
			AssertNoErrors("invoice terms has no errors", arTerms.PY_InvoiceTermInfo);
			arTerms.PY_InvoiceDays = 5;
			AssertEquals("PY_InvoiceDays", (short)5, arTerms.PY_InvoiceDays);
			AssertNoErrors("invoice days has no errors", arTerms.PY_InvoiceDaysInfo);
		}

		public void TestPY_InvoiceTermWhenCashInAdvance()
		{
			arTerms.PY_InvoiceDays = 5;
			arTerms.PY_InvoiceTerm = "INV";
			AssertEquals("PY_InvoiceTerm", (short)5, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays not readonly", !arTerms.PY_InvoiceDaysInfo.ReadOnly);

			arTerms.PY_InvoiceTerm = "PIA";
			AssertEquals("PY_InvoiceTerm", (short)0, arTerms.PY_InvoiceDays);
			Assert("PY_InvoiceDays is readonly", arTerms.PY_InvoiceDaysInfo.ReadOnly);
		}

		#region TestReadOnlyIfOrgReceivablesModifyPaymentTermsIsNotAllowed

		public void TestReadOnlysecurity()
		{
			arTerms.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			AssertReadOnlySecurity();

			Factory.Save();

			AssertReadOnlySecurity();
		}

		void AssertReadOnlySecurity()
		{
			companyData.OB_ARCreditApproved = true;
			companyData.OB_AROnCreditHold = false;

			bool originalModifyCreditControl = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;
			bool originalModifyPaymentTerms = Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;

				companyData.OB_ARCreditApproved = true;
				companyData.OB_AROnCreditHold = false;

				AssertPropertiesReadOnly(arTerms, true);

				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				AssertPropertiesReadOnly(arTerms, true);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				AssertPropertiesReadOnly(arTerms, false);

				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				AssertPropertiesReadOnly(arTerms, false);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				AssertPropertiesReadOnly(arTerms, true);

				companyData.OB_ARCreditApproved = false;
				AssertPropertiesReadOnly(arTerms, true);

				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				AssertPropertiesReadOnly(arTerms, false);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				AssertPropertiesReadOnly(arTerms, false);

				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				AssertPropertiesReadOnly(arTerms, false);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				AssertPropertiesReadOnly(arTerms, true);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = originalModifyCreditControl;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = originalModifyPaymentTerms;
			}
		}

		void AssertPropertiesReadOnly(OrgARTerms bizo, bool isReadOnly)
		{
			var propertiesWithIncorrectReadOnlyState =
					from ZPropertyInfo property in bizo.ZPropertyInfoHash
					where property.ReadOnly != isReadOnly && property.HasSetter
					select property;
			AssertEquals(string.Format("All properties must be {0} readonly.", isReadOnly ? "" : "not"), 0, propertiesWithIncorrectReadOnlyState.Count());
		}

		#endregion

		public void TestMarkAsNeedingValidationOnDeleting()
		{
			if (arTerms.Lookups.InvoiceTypeList.Count != new InvoiceTypesList().Count + 2)
			{
				Assert("This test can't be run correctly until InvoiceTypeList don't contain all invoice types", true);
			}
			else
			{
				Assert("This if..else MUST be deleted when InvoiceTypeList start to contain all invoice types!!!!", false);

				OrgARTerms allTerm = companyData.LoadARTermForAllInvoiceTypes();
				allTerm.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				allTerm.RunPreSaveValidation();
				OrgARTerms finTerm = companyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice);
				finTerm.RunPreSaveValidation();
				OrgARTerms disbTerm = companyData.CreateOrLoadDisbursementARTerm();
				disbTerm.RunPreSaveValidation();

				AssertEquals("Precondition: Term should be already validated.", false, finTerm.ShouldValidateOnSave);
				AssertNoErrors("Precondition: term should be valid.", finTerm.PY_InvoiceClassInfo);

				disbTerm.Delete();
				AssertEquals("Term must be not be validated on save after deleting any term except with 'ALL' invoice type.", false, finTerm.ShouldValidateOnSave);

				allTerm.Delete();
				AssertEquals("Term must be validated on save after deleting term with 'ALL' invoice type.", true, finTerm.ShouldValidateOnSave);

				finTerm.RunPreSaveValidation();
				AssertHasError("Term should be validated on save.", finTerm.PY_InvoiceClassInfo, "Term settings for invoice type 'ALL' are mandatory.");
			}
		}

		public void TestARTermsInstallmentReadyOnly_InvoiceTerm()
		{
			AssertEquals("ARTermsInstallment.ReadOnly", true, arTerms.ARTermsInstallments.ReadOnly);
			AssertEquals("ARTermsInstallment does not have Elements", false, arTerms.ARTermsInstallments.Any());
			AssertEquals("ARTermsInstallment.NewElement.ReadOnly", true, arTerms.ARTermsInstallments.AddNew().ReadOnly);

			arTerms.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			AssertEquals("ARTermsInstallment.ReadOnly", false, arTerms.ARTermsInstallments.ReadOnly);

			foreach (CodeDescriptionPair term in arTerms.Lookups.InvoiceTermList)
			{
				if (term.Code != InvoiceTermsList.FromInvoiceDate.Code)
				{
					arTerms.PY_InvoiceTerm = term.Code;
					AssertEquals("ARTermsInstallment.ReadOnly", true, arTerms.ARTermsInstallments.ReadOnly);
					AssertEquals("ARTermsInstallment.Count must be zero if it set to readonly.", 0, arTerms.ARTermsInstallments.Count);
					AssertEquals("ARTermsInstallment.Element.ReadOnly", true, arTerms.ARTermsInstallments.AddNew().ReadOnly);
				}
			}
		}

		public void TestARTermsCycleReadOnly_InvoiceTerm()
		{
			var initialValue = Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed = true;
				var termsCycle = Factory.New<OrgARTermsCycle>();
				termsCycle.P5_PY = arTerms.PK;

				AssertEquals("ARTermsCycle.ReadOnly", true, arTerms.ARTermsCycles.ReadOnly);
				AssertEquals("ARTermsCycle does not have Elements", false, arTerms.ARTermsCycles.Any());
				AssertEquals("ARTermsCycle.NewElement.ReadOnly", true, arTerms.ARTermsCycles.AddNew().ReadOnly);
				AssertEquals("ARTermsCycle does not have Elements", false, arTerms.ARTermsCycles.Any());

				arTerms.PY_InvoiceTerm = InvoiceTermsList.MonthsFromInvoiceCycleDate.Code;
				AssertEquals("ARTermsCycle.ReadOnly", false, arTerms.ARTermsCycles.ReadOnly);
				var cycle = arTerms.ARTermsCycles.Cast<OrgARTermsCycle>().FirstOrDefault(s => s.PK == termsCycle.PK);
				AssertNotNull(cycle);
				AssertEquals("ARTermsCycle.Element.ReadOnly", false, cycle.ReadOnly);

				foreach (CodeDescriptionPair term in arTerms.Lookups.InvoiceTermList)
				{
					if (term.Code != InvoiceTermsList.MonthsFromInvoiceCycleDate.Code)
					{
						arTerms.PY_InvoiceTerm = term.Code;
						AssertEquals("ARTermsCycle.ReadOnly", true, arTerms.ARTermsCycles.ReadOnly);
						AssertEquals("ARTermsCycle.Count must be zero if it set to readonly.", 0, arTerms.ARTermsCycles.Count);
						AssertEquals("ARTermsCycle.Element.ReadOnly", true, arTerms.ARTermsCycles.AddNew().ReadOnly);
					}
				}
			}
			finally
			{
				Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed = initialValue;
			}
		}

		public void TestARTermsCycleReadOnly_OrgReceivablesModifyInvoiceCycle_ReadOnlyIsFalse()
		{
			var initialValue = Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed = true;

				var termsCycle = Factory.New<OrgARTermsCycle>();
				termsCycle.P5_PY = arTerms.PK;
				arTerms.PY_InvoiceTerm = InvoiceTermsList.MonthsFromInvoiceCycleDate.Code;

				AssertEquals("ARTermsCycle.ReadOnly", false, arTerms.ARTermsCycles.ReadOnly);
				AssertEquals("ARTermsCycle.Element.ReadOnly", false, arTerms.ARTermsCycles[0].ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed = initialValue;
			}
		}

		public void TestARTermsCycleReadOnly_OrgReceivablesModifyInvoiceCycle_ReadOnlyIsTrue()
		{
			var initialValue = Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed = false;

				var termsCycle = Factory.NewWithValidTestData<OrgARTermsCycle>();
				termsCycle.P5_PY = arTerms.PK;
				termsCycle.P5_ToDay = 20;
				arTerms.PY_InvoiceTerm = InvoiceTermsList.MonthsFromInvoiceCycleDate.Code;

				AssertEquals("ARTermsCycle.ReadOnly", true, arTerms.ARTermsCycles.ReadOnly);
				AssertEquals("Can get term data when collection is read only.", (ZByte)20, arTerms.ARTermsCycles.Single().P5_ToDay);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyInvoiceCycle.IsAllowed = initialValue;
			}
		}

		public void TestARPaymentCycleReadOnly_InvoiceTerm()
		{
			var initialValue = Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed = true;
				var paymentCycle = Factory.New<OrgARPaymentCycle>();
				paymentCycle.P5_PY = arTerms.PK;

				AssertEquals("ARPaymentCycles.ReadOnly", true, arTerms.ARPaymentCycles.ReadOnly);
				AssertEquals("ARPaymentCycles does not have Elements", false, arTerms.ARPaymentCycles.Any());
				AssertEquals("ARPaymentCycles.NewElement.ReadOnly", true, arTerms.ARPaymentCycles.AddNew().ReadOnly);
				AssertEquals("ARPaymentCycles does not have Elements", false, arTerms.ARPaymentCycles.Any());

				arTerms.PY_InvoiceTerm = InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code;
				AssertEquals("ARPaymentCycles.ReadOnly", false, arTerms.ARPaymentCycles.ReadOnly);
				var cycle = arTerms.ARPaymentCycles.Cast<OrgARPaymentCycle>().FirstOrDefault(s => s.PK == paymentCycle.PK);
				AssertNotNull(cycle);
				AssertEquals("ARPaymentCycles.Element.ReadOnly", false, cycle.ReadOnly);

				foreach (CodeDescriptionPair term in arTerms.Lookups.InvoiceTermList)
				{
					if (term.Code != InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code)
					{
						arTerms.PY_InvoiceTerm = term.Code;
						AssertEquals("ARPaymentCycles.ReadOnly", true, arTerms.ARPaymentCycles.ReadOnly);
						AssertEquals("ARPaymentCycles.Count must be zero if it set to readonly.", 0, arTerms.ARPaymentCycles.Count);
						AssertEquals("ARPaymentCycles.Element.ReadOnly", true, arTerms.ARPaymentCycles.AddNew().ReadOnly);
					}
				}
			}
			finally
			{
				Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed = initialValue;
			}
		}

		public void TestARPaymentCycleReadOnly_OrgReceivablesModifyPaymentCycle_ReadOnlyIsFalse()
		{
			var initialValue = Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed = true;

				var paymentCycle = Factory.New<OrgARPaymentCycle>();
				paymentCycle.P5_PY = arTerms.PK;
				arTerms.PY_InvoiceTerm = InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code;

				AssertEquals("ARPaymentCycles.ReadOnly", false, arTerms.ARPaymentCycles.ReadOnly);
				AssertEquals("ARPaymentCycles.Element.ReadOnly", false, arTerms.ARPaymentCycles[0].ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed = initialValue;
			}
		}

		public void TestARPaymentCycleReadOnly_OrgReceivablesModifyPaymentCycle_ReadOnlyIsTrue()
		{
			var initialValue = Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed = false;

				var paymentCycle = Factory.NewWithValidTestData<OrgARPaymentCycle>();
				paymentCycle.P5_PY = arTerms.PK;
				paymentCycle.P5_PaymentDay = 20;
				arTerms.PY_InvoiceTerm = InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code;

				AssertEquals("ARPaymentCycles.ReadOnly", true, arTerms.ARPaymentCycles.ReadOnly);
				AssertEquals("Can get term data when collection is read only.", (ZByte)20, arTerms.ARPaymentCycles.Single().P5_PaymentDay);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyPaymentCycle.IsAllowed = initialValue;
			}
		}

		public void TestARTermsCyclesIsRegisteredChild()
		{
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerms.ARTermsCycles.AddNew();
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			arTerms.ARTermsCycles[0].P5_ToDay = 0;
			arTerms.RunPreSaveValidation();
			AssertHasErrors("Precondition: ", arTerms.ARTermsCycles[0].P5_ToDayInfo);
			AssertEquals("arTerms.HasErrors if any ARTermsCycles record has errors.", true, arTerms.HasErrors);
		}

		public void TestARPaymentCyclesIsRegisteredChild()
		{
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			arTerms.ARPaymentCycles.AddNew();
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			arTerms.ARPaymentCycles[0].P5_ToDay = 0;
			arTerms.RunPreSaveValidation();
			AssertHasErrors("Precondition: ", arTerms.ARPaymentCycles[0].P5_ToDayInfo);
			AssertEquals("arTerms.HasErrors if any ARTermsCycles record has errors.", true, arTerms.HasErrors);
		}

		public void TestGetARTermsCycleDueDate()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerms.PY_InvoiceDays = 0;
			AssertEquals("Precondition: ", 0, arTerms.ARTermsCycles.Count);
			AssertEquals(ZDateTime.Empty, arTerms.GetARTermsCycleDueDate(ZDateTime.BrettsBirthday, 0));

			OrgARTermsCycle arTermsCycle = arTerms.ARTermsCycles.AddNew();
			OrgARTermsCycle arTermsCycle2 = arTerms.ARTermsCycles.AddNew();
			OrgARTermsCycle arTermsCycle3 = arTerms.ARTermsCycles.AddNew();

			arTermsCycle.P5_ToDay = 31;
			arTermsCycle2.P5_ToDay = 10;
			arTermsCycle3.P5_ToDay = 20;

			arTermsCycle.P5_PaymentDay = 5;
			arTermsCycle2.P5_PaymentDay = 10;
			arTermsCycle3.P5_PaymentDay = 15;

			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);

			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 21), 0));
			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 30), 0));
			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 31), 0));

			AssertEquals(new ZDateTime(2011, 1, 10), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 1), 0));
			AssertEquals(new ZDateTime(2011, 1, 10), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 3), 0));
			AssertEquals(new ZDateTime(2011, 1, 10), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 10), 0));

			AssertEquals(new ZDateTime(2011, 2, 15), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 11), 0));
			AssertEquals(new ZDateTime(2011, 2, 15), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 15), 0));
			AssertEquals(new ZDateTime(2011, 2, 15), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 20), 0));

			arTerms.ARTermsCycles.DeleteAll();
			arTermsCycle = arTerms.ARTermsCycles.AddNew();
			arTermsCycle.P5_PaymentDay = 31;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 1, 31), arTerms.GetARTermsCycleDueDate(new ZDateTime(2010, 01, 01), 0));

			arTermsCycle.P5_PaymentDay = 30;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), arTerms.GetARTermsCycleDueDate(new ZDateTime(2010, 01, 01), 0));

			arTermsCycle.P5_PaymentDay = 29;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), arTerms.GetARTermsCycleDueDate(new ZDateTime(2010, 01, 01), 0));

			arTermsCycle.P5_PaymentDay = 28;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), arTerms.GetARTermsCycleDueDate(new ZDateTime(2010, 01, 01), 0));

			arTermsCycle.P5_PaymentDay = 27;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2011, 2, 27), arTerms.GetARTermsCycleDueDate(new ZDateTime(2010, 01, 01), 12));
		}

		public void TestGetARTermsCycleDueDateYusenCases()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerms.PY_InvoiceDays = 0;
			AssertEquals("Precondition: ", 0, arTerms.ARTermsCycles.Count);
			OrgARTermsCycle arTermsCycle = arTerms.ARTermsCycles.AddNew();
			OrgARTermsCycle arTermsCycle2 = arTerms.ARTermsCycles.AddNew();
			OrgARTermsCycle arTermsCycle3 = arTerms.ARTermsCycles.AddNew();

			arTermsCycle.P5_ToDay = 15;
			arTermsCycle2.P5_ToDay = 25;
			arTermsCycle3.P5_ToDay = 5;
			AssertEquals("arTermsCycle.P5_FromDayCalculated", (ZByte)6, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("arTermsCycle2.P5_FromDayCalculated", (ZByte)16, arTermsCycle2.P5_FromDayCalculated);
			AssertEquals("arTermsCycle3.P5_FromDayCalculated", (ZByte)26, arTermsCycle3.P5_FromDayCalculated);

			arTermsCycle.P5_PaymentDay = 11;
			arTermsCycle2.P5_PaymentDay = 21;
			arTermsCycle3.P5_PaymentDay = 1;
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			AssertEquals(new ZDateTime(2011, 5, 11), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 10), 2));
			AssertEquals(new ZDateTime(2011, 5, 21), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 19), 2));
			AssertEquals(new ZDateTime(2011, 5, 01), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 02), 2));
			AssertEquals(new ZDateTime(2011, 5, 01), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 1, 28), 2));
			AssertEquals(new ZDateTime(2011, 6, 01), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 28), 2));

			arTermsCycle.P5_PaymentDay = 3;
			arTermsCycle2.P5_PaymentDay = 20;
			arTermsCycle3.P5_PaymentDay = 23;
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			AssertEquals(new ZDateTime(2011, 5, 03), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 10), 2));
			AssertEquals(new ZDateTime(2011, 5, 20), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 19), 2));
			AssertEquals(new ZDateTime(2011, 4, 23), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 02), 2));
			AssertEquals(new ZDateTime(2011, 5, 23), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 28), 2));

			arTermsCycle.P5_ToDay = 5;
			arTermsCycle2.P5_ToDay = 15;
			arTermsCycle3.P5_ToDay = 25;
			AssertEquals("arTermsCycle.P5_FromDayCalculated", (ZByte)26, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("arTermsCycle2.P5_FromDayCalculated", (ZByte)6, arTermsCycle2.P5_FromDayCalculated);
			AssertEquals("arTermsCycle3.P5_FromDayCalculated", (ZByte)16, arTermsCycle3.P5_FromDayCalculated);

			arTermsCycle.P5_PaymentDay = 23;
			arTermsCycle2.P5_PaymentDay = 3;
			arTermsCycle3.P5_PaymentDay = 20;
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			AssertEquals(new ZDateTime(2011, 5, 03), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 10), 2));
			AssertEquals(new ZDateTime(2011, 5, 20), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 19), 2));
			AssertEquals(new ZDateTime(2011, 4, 23), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 02), 2));
			AssertEquals(new ZDateTime(2011, 5, 23), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 28), 2));

			arTermsCycle.P5_ToDay = 10;
			arTermsCycle2.P5_ToDay = 20;
			arTermsCycle3.P5_ToDay = 30;
			AssertEquals("arTermsCycle.P5_FromDayCalculated", (ZByte)31, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("arTermsCycle2.P5_FromDayCalculated", (ZByte)11, arTermsCycle2.P5_FromDayCalculated);
			AssertEquals("arTermsCycle3.P5_FromDayCalculated", (ZByte)21, arTermsCycle3.P5_FromDayCalculated);

			arTermsCycle.P5_PaymentDay = 15;
			arTermsCycle2.P5_PaymentDay = 25;
			arTermsCycle3.P5_PaymentDay = 5;
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			AssertEquals(new ZDateTime(2011, 5, 15), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 3, 02), 2));
			AssertEquals(new ZDateTime(2011, 4, 25), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 19), 2));
			AssertEquals(new ZDateTime(2011, 5, 05), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 28), 2));

			arTermsCycle3.Delete();
			arTermsCycle.P5_ToDay = 15;
			arTermsCycle2.P5_ToDay = 31;
			AssertEquals("arTermsCycle.P5_FromDayCalculated", (ZByte)1, arTermsCycle.P5_FromDayCalculated);
			AssertEquals("arTermsCycle2.P5_FromDayCalculated", (ZByte)16, arTermsCycle2.P5_FromDayCalculated);

			arTermsCycle.P5_PaymentDay = 31;
			arTermsCycle2.P5_PaymentDay = 15;
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			AssertEquals(new ZDateTime(2011, 4, 30), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 02), 2));
			AssertEquals(new ZDateTime(2011, 5, 15), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 19), 2));

			arTermsCycle2.Delete();
			arTermsCycle.P5_ToDay = 25;
			AssertEquals("arTermsCycle.P5_FromDayCalculated", (ZByte)26, arTermsCycle.P5_FromDayCalculated);

			arTermsCycle.P5_PaymentDay = 31;
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms);

			AssertEquals(new ZDateTime(2011, 4, 30), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 02), 2));
			AssertEquals(new ZDateTime(2011, 5, 31), arTerms.GetARTermsCycleDueDate(new ZDateTime(2011, 2, 28), 2));
		}

		public void TestGetARPaymentCycleDueDate()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertEquals("Precondition: ", 0, arTerms.ARPaymentCycles.Count);
			AssertEquals(ZDateTime.Empty, arTerms.GetARPaymentCycleDueDate(ZDateTime.BrettsBirthday, 0));

			var arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
			var arPaymentCycle2 = arTerms.ARPaymentCycles.AddNew();
			var arPaymentCycle3 = arTerms.ARPaymentCycles.AddNew();

			arPaymentCycle.P5_PaymentDay = 5;
			arPaymentCycle2.P5_PaymentDay = 10;
			arPaymentCycle3.P5_PaymentDay = 15;

			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);

			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 21), 0));
			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 30), 0));
			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 31), 0));

			AssertEquals(new ZDateTime(2011, 1, 5), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 1), 0));
			AssertEquals(new ZDateTime(2011, 1, 5), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 3), 0));
			AssertEquals(new ZDateTime(2011, 1, 10), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 10), 0));

			AssertEquals(new ZDateTime(2011, 1, 15), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 11), 0));
			AssertEquals(new ZDateTime(2011, 1, 15), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 15), 0));
			AssertEquals(new ZDateTime(2011, 2, 5), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2011, 1, 20), 0));

			arTerms.ARPaymentCycles.DeleteAll();
			arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
			arPaymentCycle.P5_PaymentDay = 31;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 3, 31), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2010, 02, 01), 0));

			arPaymentCycle.P5_PaymentDay = 30;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 3, 30), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2010, 01, 31), 0));

			arPaymentCycle.P5_PaymentDay = 29;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 3, 29), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2010, 02, 01), 0));

			arPaymentCycle.P5_PaymentDay = 28;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2010, 02, 01), 0));

			arPaymentCycle.P5_PaymentDay = 27;
			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			AssertEquals("Due Date", new DateTime(2010, 3, 27), arTerms.GetARPaymentCycleDueDate(new ZDateTime(2010, 02, 01), 27));
		}

		public void TestGetARPaymentCycleDueDateIsReportedWhenNotCalculable()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertEquals("Precondition: ", 0, arTerms.ARPaymentCycles.Count);
			AssertEquals(ZDateTime.Empty, arTerms.GetARPaymentCycleDueDate(ZDateTime.BrettsBirthday, 0));

			var arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
			var arPaymentCycle2 = arTerms.ARPaymentCycles.AddNew();
			arPaymentCycle.P5_PaymentDay = 5;
			arPaymentCycle2.P5_PaymentDay = 10;

			arTerms.RunPreSaveValidation();
			AssertNoErrors("Precondition: ", arTerms);
			ErrorReporter.Clear();
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);

			var result = arTerms.GetARPaymentCycleDueDate(new ZDateTime(DateTime.MaxValue), 0);
			AssertEquals("Due Date should be ZDateTime.Empty", ZDateTime.Empty, result);
			AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("OrgARTerms_ARPaymentCycleDueDateIsAnUnrepresentableDateTime key was reported", "OrgARTerms_ARPaymentCycleDueDateIsAnUnrepresentableDateTime", ErrorReporter.LastKeyReported);
			AssertContains("Should contain payment cycle and parameter info", "Calculating Due Date using the following info:\r\nInvoice Date: 31-Dec-99 23:59:59\r\nTerm Days: 0\r\n\r\nPayment Cycles Info:\r\n"
				+ "Payment Cycle: 1, Payment Day: 5\r\nPayment Cycle: 2, Payment Day: 10\r\n", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2018, 9, 11)]
		public void TestGetARPaymentCycleDueDateShouldReturnEmptyDateTimeWhenHasInvalidCycles()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;

			AssertARPaymentCycleDueDate(ZDateTime.Empty, (0, 0));
			AssertARPaymentCycleDueDate(ZDateTime.Empty, (0, 10));
			AssertARPaymentCycleDueDate(ZDateTime.Empty, (10, 0));
			AssertARPaymentCycleDueDate(ZDateTime.Empty, (10, 35));
			AssertARPaymentCycleDueDate(new ZDateTime(2018, 10, 5), (50, 5));

			var cycles = new (ZByte ToDay, ZByte PaymentDay)[] { (1, 5), (2, 10), (3, 0) };
			var expectDueDate = new ZDateTime(2018, 10, 5);
			AssertARPaymentCycleDueDate(expectDueDate, cycles);
		}

		void AssertARPaymentCycleDueDate(ZDateTime expectDueDate, (ZByte ToDay, ZByte PaymentDay) cycle)
		{
			var cycles = new (ZByte ToDay, ZByte PaymentDay)[] { cycle };
			AssertARPaymentCycleDueDate(expectDueDate, cycles);
		}

		void AssertARPaymentCycleDueDate(ZDateTime expectDueDate, (ZByte ToDay, ZByte PaymentDay)[] cycles)
		{
			arTerms.ARPaymentCycles.DeleteAll();
			foreach (var cycle in cycles)
			{
				var arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
				arPaymentCycle.P5_PaymentDay = cycle.PaymentDay;
				arPaymentCycle.P5_ToDay = cycle.ToDay;
			}

			try
			{
				var actualDueDate = arTerms.GetARPaymentCycleDueDate(ZDateTime.Now, 0);
				AssertEquals(expectDueDate, actualDueDate);
			}
			finally
			{
				arTerms.ARPaymentCycles.DeleteAll();
			}
		}

		[TestDate(2018, 9, 11)]
		public void TestGetARTermsCycleDueDateShouldReturnEmptyDateTimeWhenHasInvalidCycles()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;

			AssertARTermsCycleDueDate(ZDateTime.Empty, (0, 0));
			AssertARTermsCycleDueDate(ZDateTime.Empty, (0, 10));
			AssertARTermsCycleDueDate(ZDateTime.Empty, (10, 0));
			AssertARTermsCycleDueDate(ZDateTime.Empty, (35, 10));
			AssertARTermsCycleDueDate(ZDateTime.Empty, (35, 35));

			var cycles = new (ZByte ToDay, ZByte PaymentDay)[] { (10, 5), (20, 15), (31, 0) };
			var expectDueDate = new ZDateTime(2018, 10, 15);
			AssertARTermsCycleDueDate(expectDueDate, cycles);
			cycles = new (ZByte ToDay, ZByte PaymentDay)[] { (10, 5), (20, 15), (0, 25) };
			AssertARTermsCycleDueDate(expectDueDate, cycles);
		}

		void AssertARTermsCycleDueDate(ZDateTime expectDueDate, (ZByte ToDay, ZByte PaymentDay) cycle)
		{
			var cycles = new (ZByte ToDay, ZByte PaymentDay)[] { cycle };
			AssertARTermsCycleDueDate(expectDueDate, cycles);
		}

		void AssertARTermsCycleDueDate(ZDateTime expectDueDate, (ZByte ToDay, ZByte PaymentDay)[] cycles)
		{
			arTerms.ARTermsCycles.DeleteAll();
			foreach (var cycle in cycles)
			{
				var arTermsCycle = arTerms.ARTermsCycles.AddNew();
				arTermsCycle.P5_PaymentDay = cycle.PaymentDay;
				arTermsCycle.P5_ToDay = cycle.ToDay;
			}

			try
			{
				var actualDueDate = arTerms.GetARTermsCycleDueDate(ZDateTime.Now, 0);
				AssertEquals(expectDueDate, actualDueDate);
			}
			finally
			{
				arTerms.ARTermsCycles.DeleteAll();
			}
		}

		public void TestARTermCyclesNotDeletedOnLoading()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertEquals("Precondition: ", 0, arTerms.ARTermsCycles.Count);
			var arTermsCycle = arTerms.ARTermsCycles.AddNew();
			var arTermsCycle2 = arTerms.ARTermsCycles.AddNew();
			var arTermsCycle3 = arTerms.ARTermsCycles.AddNew();

			arTermsCycle.P5_ToDay = 15;
			arTermsCycle2.P5_ToDay = 25;
			arTermsCycle3.P5_ToDay = 5;

			Factory.Save();

			var loadedARTerms = new BusinessObjectFactory().Load<OrgARTerms>(arTerms.PK);
			AssertNotNull("Access other collection first", loadedARTerms.ARPaymentCycles);
			AssertEquals("Term cycles should be all there", 3, loadedARTerms.ARTermsCycles.Count);
		}

		public void TestARPaymentCyclesNotDeletedOnLoading()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertEquals("Precondition: ", 0, arTerms.ARPaymentCycles.Count);
			var arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
			var arPaymentCycle2 = arTerms.ARPaymentCycles.AddNew();
			var arPaymentCycle3 = arTerms.ARPaymentCycles.AddNew();

			arPaymentCycle.P5_PaymentDay = 5;
			arPaymentCycle2.P5_PaymentDay = 10;
			arPaymentCycle3.P5_PaymentDay = 15;

			Factory.Save();

			var loadedARTerms = new BusinessObjectFactory().Load<OrgARTerms>(arTerms.PK);
			AssertNotNull("Access other collection first", loadedARTerms.ARTermsCycles);
			AssertEquals("Payment cycles should be all there", 3, loadedARTerms.ARPaymentCycles.Count);
		}

		public void TestIsDefaultTerm()
		{
			SetupTermsInfo(arTerms, JobInvoicingConsumerTypes.ShipmentCode, ZGuid.Empty, ZGuid.Empty, "IMP", "AIR", "FID");
			arTerms.PY_InvoiceTerm = "MTH";
			arTerms.PY_InvoiceDays = 25;
			AssertEquals(arTerms.ToString(), false, arTerms.IsDefaultTerm);

			SetupTermsInfo(arTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");
			arTerms.PY_InvoiceTerm = "MTH";
			arTerms.PY_InvoiceDays = 25;
			AssertEquals(arTerms.ToString(), true, arTerms.IsDefaultTerm);
		}

		public void TestIsDisbursementTerm()
		{
			var lookups = new OrgARTermsLookups(arTerms);
			foreach (CodeDescriptionPair jobType in lookups.JobTypeList)
			{
				SetupTermsInfo(arTerms, jobType.Code, ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");

				foreach (CodeDescriptionPair invoiceType in lookups.InvoiceTypeList)
				{
					arTerms.PY_InvoiceClass = invoiceType.Code;
					arTerms.PY_InvoiceTerm = "MTH";
					arTerms.PY_InvoiceDays = 25;
					AssertEquals(arTerms.ToString(), new ZString[] {
																																								OrgARTermsLookups.InvoiceTypes.DSB.Code,
																																								InvoiceTypesList.Codes.DisbursementInForeignCurrency,
																																			InvoiceTypesList.Codes.DisbursementInvoice,
																																			InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
																																			InvoiceTypesList.Codes.DisbursementInvoice_Batching
																																						}.Contains(invoiceType.Code), arTerms.IsDisbursementTerm);
				}
			}
		}

		public void TestLongTermsTextTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", new ResourceStringData("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", "月"));
				mockChs.Put("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", new ResourceStringData("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", "日"));
				mockChs.Put("MasterFiles|InvoiceTerms|MonthsFromInvoiceCycleDate", new ResourceStringData("MasterFiles|InvoiceTerms|MonthsFromInvoiceCycleDate", "反正不是Month开头"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					defaultTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
					var converter = new OrgARTerms.ToStringConverter(defaultTerms);
					AssertEquals("DAYS should be translated", "0 日 FROM END OF MONTH", converter.LongTermsText);

					defaultTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
					converter = new OrgARTerms.ToStringConverter(defaultTerms);
					AssertEquals("MONTHS should be translated", "0 月 反正不是MONTH开头", converter.LongTermsText);
				}
			}
		}

		public void TestToString()
		{
			defaultTerms.PY_InvoiceTerm = "COD";
			AssertEquals("COD", defaultTerms.ToString());

			defaultTerms.PY_InvoiceTerm = "INV";
			defaultTerms.PY_InvoiceDays = 20;
			AssertEquals("20/INV", defaultTerms.ToString());

			arTerms.PY_InvoiceTerm = "COD";
			AssertEquals("(SHP-DSB)->COD", arTerms.ToString());

			arTerms.PY_InvoiceTerm = "INV";
			arTerms.PY_InvoiceDays = 20;
			AssertEquals("(SHP-DSB)->20/INV", arTerms.ToString());

			SetupTermsInfo(arTerms, JobInvoicingConsumerTypes.ShipmentCode, ZGuid.Empty, ZGuid.Empty, "IMP", "AIR", "FID");
			arTerms.PY_InvoiceTerm = "MTH";
			arTerms.PY_InvoiceDays = 25;
			AssertEquals("(SHP-IMP-AIR-FID)->25/MTH", arTerms.ToString());

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var dept = Factory.NewWithValidTestData<GlbDepartment>();

			SetupTermsInfo(arTerms, JobInvoicingConsumerTypes.ShipmentCode, branch.PK, dept.PK, "IMP", "AIR", "FIN");
			arTerms.PY_InvoiceTerm = "INV";
			arTerms.PY_InvoiceDays = 35;
			AssertEquals("(SHP-" + branch.GB_Code + "-" + dept.GE_Code + "-IMP-AIR-FIN)->35/INV", arTerms.ToString());
		}

		OrgHeader organisation;
		OrgCompanyData companyData;
		OrgARTerms arTerms, defaultTerms;

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			companyData = organisation.CompanyData;
			companyData.OB_IsDebtor = true;
			arTerms = companyData.ARTerms[0];

			defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");
			SetupTermsInfo(arTerms, JobInvoicingConsumerTypes.ShipmentCode, ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "DSB");
		}

		void SetupTermsInfo(OrgARTerms term, ZString jobType, ZGuid branchPK, ZGuid deptPK, ZString direction, ZString transportMode, ZString invoiceType)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
			}
		}
	}
}
