using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class AESMessageHandlerTest : TestCaseWithFactory
	{
		public void TestAESMessageRestrictedByDeniedPartyScreening()
		{
			var registryItem = OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions;
			var registryValue = DPSFreightMovementRestrictionsOptions.Codes.All;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			declaration.DoMerge();
			declaration.RunPreSaveValidation();
			Factory.Save();
			declaration.JE_ScreeningStatus = "UNK";
			Factory.Save();
			var filerID = new ExportEntryFilerID
			{
				EntryFilerID = "123456789"
			};
			DataRegistry.Business.USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, filerID);
			var creditControlledDeclaration = declaration as MasterFiles.CreditControl.Business.ICreditControlledDocumentDelivery;
			Assert(creditControlledDeclaration != null);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals("Reg option is All. Declaration should be DPSFreightMovementRestricted:", true, creditControlledDeclaration.IsDPSFreightMovementRestricted);
			AESMessageHandler.SendMessage(declaration, null);
			AssertContains("Organization which may be on the Denied Party Screening list", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
		}
	}
}
