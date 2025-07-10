using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.PlugIn.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class BrokeragePlugInTest : BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestCheckDA63NeedsRecalculationWhenSaving()
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new ZAUniversalReferenceTestDataHelper(newFactory);
			testHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var testShipment = newFactory.New<ForwardingShipment>();
			var testDeclaration = newFactory.New<JobDeclaration>();
			testDeclaration.JE_JS = testShipment.PK;
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			newFactory.Save();
			using (var testPlugin = new BrokeragePlugIn(Factory.Load<ForwardingShipment>(testShipment.PK)))
			{
				var invoiceLine = testPlugin.JobDeclaration.InvoiceLines[0] as JobComInvoiceLine;
				AssertEquals(false, (invoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
				invoiceLine.JI_PreviousEntryLineNumber = 5;
				AssertEquals(true, (invoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
				var notificationInstanse = UnitTestUserNotification.Instance;
				(testPlugin as IShowPreSaveDialog).ShowPreSaveDialogs();
				AssertEquals(DA63PreSaveDialogStrategy.ReCalculateConfimation, notificationInstanse.LastMessage.Text);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
		}
	}
}
