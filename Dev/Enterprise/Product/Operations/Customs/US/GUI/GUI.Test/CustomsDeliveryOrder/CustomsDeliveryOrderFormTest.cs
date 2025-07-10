using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(CustomsDeliveryOrderForm))]
	sealed class CustomsDeliveryOrderFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestCanclButton_Click()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			header.US_ShouldPrint = true;
			header.DeliveryOrderLines.AddNew();
			using (CustomsDeliveryOrderForm form = new CustomsDeliveryOrderForm(declaration))
			{
				form.DialogResult = DialogResult.None;
				AssertEquals(false, declaration.IsInDatabase);
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
				AssertEquals(false, declaration.IsInDatabase);
				Assert("should have been closed", form.IsDisposed);
			}
		}

		public void TestDoNotSetUpPostingButton()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.DeliveryOrderHeaders.AddNew();
			header.DeliveryOrderLines.AddNew();
			Factory.Save();
			using (CustomsDeliveryOrderForm form = new CustomsDeliveryOrderForm(declaration))
			{
				form.Show();
				header.US_ShouldPrint = true;
				form.DialogResult = DialogResult.None;
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Browse;
				AssertNoExceptionThrown(delegate
				{
					form.SaveButton.PerformClick();
				});
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		public void TestPrintButton_Click()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("ZSD");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			header.US_ShouldPrint = true;
			header.US_DeliveryInstructions = "BLAH";
			header.DeliveryOrderLines.AddNew();
			header.US_OH_InlandCarrier = ZGuid.Invalid;
			using (CustomsDeliveryOrderForm form = new CustomsDeliveryOrderForm(declaration))
			{
				form.DialogResult = DialogResult.None;
				AssertEquals(false, declaration.IsInDatabase);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PrintButton.PerformClick();
				AssertNotEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(false, declaration.IsInDatabase);
				AssertEquals("Errors!", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.US_OH_InlandCarrier = ZGuid.Empty;
				form.PrintButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(true, declaration.IsInDatabase);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestConcurrencyErrorHandlingWhenPrint()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("ZSD");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageStatus = "AER";
			Factory.Save();
			var factory2 = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			declaration.JE_MessageStatus = "CER";
			declaration.JE_SystemCreateUser = "~BP"; //wont be overridden once it is saved to the database
			declaration2.JE_HouseBill = "HB1";
			declaration2.JE_SystemCreateUser = "ABC";
			var header = declaration2.DeliveryOrderHeaders.AddNew();
			header.US_ShouldPrint = true;
			header.US_DeliveryInstructions = "BLAH";
			using (CustomsDeliveryOrderForm form = new CustomsDeliveryOrderForm(declaration2))
			{
				form.Show();
				Factory.Save();
				AssertNoExceptionThrown(delegate
				{
					form.PrintButton.PerformClick();
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			header.US_ShouldPrint = true;
			header.US_LastFreeDay = ZDateTime.Now;
			header.US_PrepaidCollect = "C";
			header.ForDeliveryToAddress.E2_AddressOverride = true;
			header.ForDeliveryToAddress.E2_CompanyName = "BOB THE BUILDER";
			header.ForDeliveryToAddress.E2_Address1 = "ADDRESS 1";
			DeliveryOrderLine line = header.DeliveryOrderLines.AddNew();
			line.US_GoodsDescription = "GOODS";
			line.US_WeightInKilograms = 100;
			Factory.Save();
			return new CustomsDeliveryOrderForm(declaration);
		}
	}
}
