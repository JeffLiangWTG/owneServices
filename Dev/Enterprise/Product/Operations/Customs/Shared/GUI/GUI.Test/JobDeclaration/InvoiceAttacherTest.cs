using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceAttacherTest : TestCaseForAttachGUI
	{
		public void TestGoodsDescriptionRecalculated_WhenInvoiceAttachedToDec()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invoiceLine1 = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoice.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_Tariff = "123456789";

			var invoiceLine2 = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoice.InvoiceLines.Add(invoiceLine2);
			invoiceLine2.JI_Tariff = "123456789";
			invoiceLine2.JI_Description = ZString.Empty;

			var invoiceLine3 = Factory.New<BaseJobComInvoiceLineForTesting>();
			invoice.InvoiceLines.Add(invoiceLine3);
			invoiceLine3.JI_Tariff = "123456789";
			invoiceLine3.JI_Description = "XXX";
			Factory.Save();

			var invoicesToAttach = new InvoiceHeaderWithNoDeclarationCollection(Factory);
			var invoices = new List<BaseJobComInvoiceHeader>();
			invoices.Add(invoice);

			using (CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(true)))
			{
				var attacher = new InvoiceAttacher(declaration.Invoices, invoicesToAttach);
				attacher.AttachItemsCoreInternal(declaration.Invoices, invoices);

				AssertEquals("When EnableCustomsDeclaration is true, default tariff description should be kept when attached to dec.", "TEST DESCRIPTION", invoiceLine1.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is true, empty JI_Description should be updated with default tariff description when attached to dec.", "TEST DESCRIPTION", invoiceLine2.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is true, manually entered JI_Description should be kept when attached to dec.", "XXX", invoiceLine3.JI_Description);
			}

			invoiceLine2.JI_Description = ZString.Empty;
			using (CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, GetConfiguredRegistry(false)))
			{
				var attacher = new InvoiceAttacher(declaration.Invoices, invoicesToAttach);
				attacher.AttachItemsCoreInternal(declaration.Invoices, invoices);

				AssertEquals("When EnableCustomsDeclaration is false, default tariff description should be removed when attached to dec.", string.Empty, invoiceLine1.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is false, empty JI_Description should be kept when attached to dec.", string.Empty, invoiceLine2.JI_Description);
				AssertEquals("When EnableCustomsDeclaration is false, manually entered JI_Description should be kept when attached to dec.", "XXX", invoiceLine3.JI_Description);
			}

			AutomatedTariffDescriptionPopulation GetConfiguredRegistry(bool value)
			{
				var registryValue = new AutomatedTariffDescriptionPopulation();
				registryValue.EnableCustomsDeclaration = value;
				return registryValue;
			}
		}

		public void TestCannotAttachLockedInvoices()
		{
			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			invoice1.JZ_InvoiceNumber = "INV1";
			((ICustomsFileParent)invoice1).LockFile("TEST");
			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_InvoiceNumber = "INV2";
			var invoice3 = Factory.New<BaseJobComInvoiceHeader>();
			invoice3.JZ_InvoiceNumber = "INV3";
			var invoice4 = Factory.New<BaseJobComInvoiceHeader>();
			invoice4.JZ_InvoiceNumber = "INV4";
			((ICustomsFileParent)invoice4).LockFile("TEST");
			Factory.Save();
			var declaration = Factory.New<BaseJobDeclaration>();
			var attacher = new InvoiceAttacher(declaration.Invoices, declaration.Lookups.InvoicesToAttach);
			using (var form = new ZForm(declaration))
			{
				attacher.Show(form);
				attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { invoice1, invoice2, invoice3, invoice4 });
				AssertEquals("Correct message shown", @"The following commercial invoices are locked and cannot be attached to the declaration until they have been Unlocked:
Invoice INV1
Invoice INV4", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Correct message shown", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				attacher.LastShownAttachPopupForTesting.Dispose();
			}
		}

		public void TestCannotAttachWhenDestinationCollectionIsReadOnly()
		{
			var declaration = GetNewDeclaration();
			InvoiceAttacher attacher = new InvoiceAttacher(declaration.Invoices, declaration.Lookups.InvoicesToAttach);
			declaration.Invoices.SetReadOnlyIncludingChildren(true);
			attacher.Show(null);
			AssertEquals("Correct message shown", "Sorry, Commercial Invoices can be viewed but not attached.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Correct message shown", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertNull("Should not have shown attach popup", attacher.LastShownAttachPopupForTesting);
		}

		public void TestCanAttachWhenDestinationCollectionIsNotReadOnly()
		{
			var declaration = GetNewDeclaration();
			InvoiceAttacher attacher = new InvoiceAttacher(declaration.Invoices, declaration.Lookups.InvoicesToAttach);
			attacher.DisableShowingGUIForTesting = true;
			declaration.Invoices.SetReadOnlyIncludingChildren(false);
			attacher.Show(null);
			AssertEquals("Should have called through to base Show", 1, attacher.BaseShowCoreCallCountForTesting);
			AssertEquals("No message shown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestEffectOfShouldShowOnPopup()
		{
			var declaration = GetNewDeclaration();
			InvoiceAttacher attacher = new InvoiceAttacher(declaration.Invoices, declaration.Lookups.InvoicesToAttach);
			attacher.DisableShowingGUIForTesting = true;
			declaration.Invoices.AddNew();
			declaration.Invoices[0].JZ_InvoiceNumber = "Blah";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (ZForm form = new ZForm(declaration))
			{
				attacher.Show(form);
				AssertEquals("Should not have called through to base Show", 0, attacher.BaseShowCoreCallCountForTesting);
				((IBusinessObjectCollection)declaration.Invoices).ClearHasChangesIncludingChildren();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				attacher.Show(form);
				AssertEquals("Should have called through to base Show", 1, attacher.BaseShowCoreCallCountForTesting);
			}
		}

		public void TestShouldShow()
		{
			var declaration = GetNewDeclaration();
			var mockForm = new Mock<ZForm>(declaration);
			mockForm.CallBase = true;
			ZForm form = mockForm.Object;
			try
			{
				InvoiceAttacher attacher = new InvoiceAttacher(declaration.Invoices, declaration.Lookups.InvoicesToAttach);
				((IBusinessObjectCollection)declaration.Invoices).ClearHasChangesIncludingChildren();
				Assert("No changes, should show", attacher.ShouldShow(form));
				((IBusinessObjectCollectionInternals)declaration.Invoices).HasChangesFromDelete = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert("User cancelled, don't show", !attacher.ShouldShow(form));
				Assert("Right message", UnitTestUserNotification.Instance.LastMessage.Text.EndsWith("save the form?"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				mockForm.Setup(m => m.FireSaveButton(It.IsAny<object>())).Returns(ContinueWithSave.No);
				Assert("Don't show, form says no", !attacher.ShouldShow(form));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				mockForm.Setup(m => m.FireSaveButton(It.IsAny<object>())).Returns(ContinueWithSave.Yes);
				Assert("All OK, should show", attacher.ShouldShow(form));
			}
			finally
			{
				form.Dispose();
			}
		}

		public void TestAttachCore()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			InvoiceAttacher attacher = new InvoiceAttacher(testDec.Invoices, testDec.Lookups.InvoicesToAttach);
			attacher.AttachCoreInternal(invoice, new List<BusinessObject>());
			Assert("RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired should have been set", invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired);
		}
	}
}
