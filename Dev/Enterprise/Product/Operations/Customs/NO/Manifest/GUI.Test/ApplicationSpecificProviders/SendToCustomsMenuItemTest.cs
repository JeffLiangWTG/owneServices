using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(SendToCustomsMenuItem))]
sealed class SendToCustomsMenuItemTest : TestCaseWithFactory
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>("When header is null", () => new SendToCustomsMenuItem(null));

	public void TestCaption()
	{
		using var menuItem = new SendToCustomsMenuItem(NewManifestHeaderWithDefault());
		var captionResourceString = menuItem.CaptionResourceString;
		AssertNotNull("Caption Resource String Object", captionResourceString);
		AssertEquals("Caption", "Send to Customs", captionResourceString.Caption);
	}

	public void TestClick()
	{
		// This test would be modified once DMOMessageSendingObjectParent.CreateAndSaveMessage() is ready
		const string confirmSaveMsg = "You need to save first. Would you like to save now and proceed?";
		const string messageNotSent = "There are errors - can't save.";

		var header = NewManifestHeaderWithDefault();
		using var form = new ZForm(header);
		header.AMA_GB = ZGuid.Invalid;

		CombineAssertions(() =>
		{
			ClickSendToCustoms(form, header, saveToDb: DialogResult.No);
			AssertEquals("Confirmation message to save first", confirmSaveMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Changes not saved, data not is database", expected: false, header.IsInDatabase);

			ClickSendToCustoms(form, header);
			AssertEquals("Changes saved to database", expected: false, header.IsInDatabase);
			AssertContains("Message not sent due to missing manifest details", messageNotSent, UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	static void ClickSendToCustoms(ZForm manifestForm, AsycudaManifestHeader header, DialogResult saveToDb = DialogResult.Yes)
	{
		using var menuItem = new SendToCustomsMenuItem(header);
		manifestForm.Menu.MenuItems.Add(menuItem);
		manifestForm.Show();

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(saveToDb);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		menuItem.PerformClick();
	}

	AsycudaManifestHeader NewManifestHeaderWithDefault()
	{
		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Norway;
		manifestHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
		manifestHeader.AMA_CustomsProfile = GlbCompany.CurrentCompany.GC_Code;
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "123";

		return manifestHeader;
	}
}
