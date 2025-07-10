using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	sealed class DeliveryOrderPrintManagerTest : TestCaseWithFactory
	{
		public void TestIsOkToPrint()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("ZSD");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var manager = new DeliveryOrderPrintManager(declaration);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZFormModaliser.LastFormShownDialogForTest = null;
			AssertEquals(false, manager.IsOkToPrint());
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			declaration.DeliveryOrderHeaders.Load();
			AssertEquals(0, declaration.DeliveryOrderHeaders.Count);
			Factory.Save();
			AssertEquals(false, manager.IsOkToPrint());
			AssertEquals(typeof(CustomsDeliveryOrderForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			declaration.DeliveryOrderHeaders.Load();
			AssertEquals(0, declaration.DeliveryOrderHeaders.Count);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.LastFormShownDialogForTest = null;
			AssertEquals(true, manager.IsOkToPrint());
			AssertEquals(typeof(CustomsDeliveryOrderForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			declaration.DeliveryOrderHeaders.Load();
			AssertEquals(0, declaration.DeliveryOrderHeaders.Count);
			try
			{
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(x =>
				{
					var form = x as CustomsDeliveryOrderForm;
					if (form != null)
					{
						declaration = form.BusinessEntity;
					}
				});
				ZFormModaliser.LastFormShownDialogForTest = null;
				AssertEquals(true, manager.IsOkToPrint());
				AssertEquals(typeof(CustomsDeliveryOrderForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				declaration.DeliveryOrderHeaders.Load();
				AssertEquals(1, declaration.DeliveryOrderHeaders.Count);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		public void TestDeliveryOrderSupportCreditControllFeature()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			consignor.OH_IsDebtor = true;
			consignor.CompanyData.OB_AROnCreditHold = true;
			dec.JE_OH_Supplier = consignor.PK;
			var deliveryOrder = dec.DeliveryOrderHeaders.AddNew();
			Factory.Save();
			consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			var command = Factory.New<DocumentCommand>();
			command.Parent = dec;
			command.SU_MenuName = DocumentNames.CustomsDeliveryOrder;
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			var supporter = dec.DocumentSupporter;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var state = supporter.GetDataStateBeforeRun(command);
			AssertEquals(false, state.IsValid);
			AssertEquals(@"Delivery of this document is restricted because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.", state.ErrorMessage);
		}
	}
}
