using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	public abstract class BaseBrokeragePlugInAbstractTest : ZPlugInGenericTest
	{
		public virtual void TestClearHasChanges()
		{
			MakeThisDeclarationPackingRelevant(JobDeclaration);
			JobDeclaration.JE_OverrideFreightDefaults = false;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(JobDeclaration.MergeManager);

			PackLine packline = Shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			Shipment.JS_HouseBill = "HBL1";
			Shipment.JS_INCO = "FOB";

			if (JobDeclaration.Forwarder != null)
			{
				JobDeclaration.Forwarder.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			}
			Factory.Save();//to clear HasChanges for all children

			using (BaseBrokeragePlugIn plugIn = GetPlugInToTest() as BaseBrokeragePlugIn)
			{
				AssertEquals(false, JobDeclaration.HasChanges);
			}
		}

		protected virtual void MakeThisDeclarationPackingRelevant(BaseJobDeclaration declaration)
		{
		}

		protected BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (fTestDeclaration == null)
				{
					PrepareShipmentAndMergedDeclaration();
				}
				return fTestDeclaration;
			}
		}

		protected ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					PrepareShipmentAndMergedDeclaration();
				}
				return fShipment;
			}
		}

		protected virtual void PrepareShipmentAndMergedDeclaration()
		{
			fShipment = Factory.New<ForwardingShipment>();
			fTestDeclaration = GetDeclaration();
			fTestDeclaration.JE_JS = fShipment.PK;
			fTestDeclaration.JE_OverrideFreightDefaults = true;

			var invoice = fTestDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			if (fTestDeclaration.IsEntryInstructionRequired)
			{
				var instruction = fTestDeclaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
			}

			fTestDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			fTestDeclaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		}

		protected virtual BaseJobDeclaration GetDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		ForwardingShipment fShipment;
		BaseJobDeclaration fTestDeclaration;

		//Issue 00022244
		public void TestWhenUsersClickMenuToCreateDeclarationMessageInitiatorIsSet()
		{
			using (BaseBrokeragePlugIn plugIn = GetPlugInToTest() as BaseBrokeragePlugIn)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //Yes a job declaration will be created
				plugIn.OnMenuShown();
				AssertNotNull("JobDeclaration is created", plugIn.JobDeclaration);
				AssertNotNull("MessageInitiator should be set and will throw an exception if a sub-menu requiring message initiator is clicked", JobDeclaration.MessageInitiator);
			}
		}

		public void TestProductCreationConfirmationFormWithConsigneeSetting()
		{
			using (var plugIn = GetPlugInToTest() as BaseBrokeragePlugIn)
			{
				Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed = true;
				var invoice = fTestDeclaration.Invoices.First();
				var invoiceLine = (BaseJobComInvoiceLine)invoice.JobComInvoiceLines.First();
				var declarationSupplier = Factory.New<OrgHeader>();
				declarationSupplier.OH_Code = "ABC";
				declarationSupplier.OH_IsConsignor = true;
				declarationSupplier.MainAddress.OA_Address1 = "Add1";
				var declarationImporter = Factory.New<OrgHeader>();
				declarationImporter.OH_Code = "Importer";
				declarationImporter.OH_RL_NKClosestPort = "AUSYD";
				declarationImporter.OH_IsConsignee = true;
				declarationImporter.OH_IsConsignor = true;
				fTestDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				fTestDeclaration.JE_OH_Supplier = declarationSupplier.PK;
				invoiceLine.JI_Tariff = "123";
				invoiceLine.JI_PartNo = "AAA";
				invoiceLine.JI_Description = "QQQ";
				invoiceLine.JI_InvoiceUQ = "NO";

				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					fTestDeclaration.JE_OH_Importer = ZGuid.Empty;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					fTestDeclaration.JE_OH_Importer = declarationImporter.PK;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					fTestDeclaration.JE_OH_Importer = ZGuid.Empty;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
					fTestDeclaration.JE_OH_Importer = declarationImporter.PK;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					fTestDeclaration.JE_OH_Importer = ZGuid.Empty;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
					fTestDeclaration.JE_OH_Importer = declarationImporter.PK;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					fTestDeclaration.JE_OA_ImporterAddress = ZGuid.Empty;
					fTestDeclaration.JE_OH_Importer = ZGuid.Empty;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					fTestDeclaration.JE_OH_Importer = declarationImporter.PK;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
			}
		}

		public void TestProductCreationConfirmationFormWithPromptToCreateProducts()
		{
			using (var plugIn = GetPlugInToTest() as BaseBrokeragePlugIn)
			{
				Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed = true;
				var invoice = fTestDeclaration.Invoices.First();
				var invoiceLine = (BaseJobComInvoiceLine)invoice.JobComInvoiceLines.First();
				var declarationSupplier = Factory.New<OrgHeader>();
				declarationSupplier.OH_Code = "ABC";
				declarationSupplier.OH_IsConsignor = true;
				declarationSupplier.MainAddress.OA_Address1 = "Add1";
				fTestDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				fTestDeclaration.JE_OH_Supplier = declarationSupplier.PK;
				invoiceLine.JI_Tariff = "123";
				invoiceLine.JI_PartNo = "AAA";
				invoiceLine.JI_Description = "QQQ";
				invoiceLine.JI_InvoiceUQ = "NO";

				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					plugIn.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					plugIn.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}
}
