using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Manifest.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		public void TestSendButtonEnabled()
		{
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("TransportMode Add Message Error");
			};

			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				Assert(!sendButton.Enabled);
				var messageSendingObject = sendingObjParent.SendingObjectsCollection.Cast<MessageSendingObjectForTestSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				Assert(!sendButton.Enabled);
				sendingObjParent.AllowSendWithError = true;
				Assert(sendButton.Enabled);
				messageSendingObject.ShouldSend = false;
				Assert(!sendButton.Enabled);
			}
		}

		public void TestValidationErrorsTextBox()
		{
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Add Header Message Error");
			};
			bill.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Add Bill Message Error");
			};

			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var validationErrorsTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");
				var messageSendingObject = sendingObjParent.SendingObjectsCollection.Cast<MessageSendingObjectForTestSendingObject>().FirstOrDefault();
				messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
				{
					x.AddMessageError("Sending Object Message Error");
				};

				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(validationErrorsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("Add Header Message Error", validationErrorsTextBox.Text);
				AssertContains("Add Bill Message Error", validationErrorsTextBox.Text);
				AssertContains("Sending Object Message Error", validationErrorsTextBox.Text);
			}
		}

		public void TestAdditionalWarningsTextBox()
		{
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("Add Header Warning");
			};
			bill.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("Add Bill Warning");
			};

			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var additionalWarningsTextBox = form.FindSingle<ZTextBox>("AdditionalWarningsTextBox");
				var messageSendingObject = sendingObjParent.SendingObjectsCollection.Cast<MessageSendingObjectForTestSendingObject>().FirstOrDefault();
				messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
				{
					x.AddWarning("Sending Object Warning");
				};

				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(additionalWarningsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("Add Header Warning", additionalWarningsTextBox.Text);
				AssertContains("Add Bill Warning", additionalWarningsTextBox.Text);
				AssertContains("Sending Object Warning", additionalWarningsTextBox.Text);
			}
		}

		public void TestContinueToSendCheckBoxShouldNotBeVisibleWhenNoErrors()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var locationCode = "ANP0060D";
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, locationCode, "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeaderForTestSendingObject>();
			header.AMA_GoodsLocationFromMasterBill = locationCode;
			header.Bills.AddNew();
			var sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var continueToSendCheckBox = form.FindSingle<ZCheckBox>("ContinueToSendCheckBox");
				Assert(!continueToSendCheckBox.Visible);
			}

			header = Factory.New<AsycudaManifestHeaderForTestSendingObject>();
			header.AMA_GoodsLocationFromMasterBill = locationCode;
			header.Bills.AddNew();
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("Header Add Warning");
			};
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var continueToSendCheckBox = form.FindSingle<ZCheckBox>("ContinueToSendCheckBox");
				Assert(!continueToSendCheckBox.Visible);
			}

			header = Factory.New<AsycudaManifestHeaderForTestSendingObject>();
			header.AMA_GoodsLocationFromMasterBill = locationCode;
			header.Bills.AddNew();
			header.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("Header Add Message Error");
			};
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var continueToSendCheckBox = form.FindSingle<ZCheckBox>("ContinueToSendCheckBox");
				Assert(continueToSendCheckBox.Visible);
			}
		}

		public void TestIsFinalManifestCheckBox()
		{
			var header = Factory.New<AsycudaManifestHeaderForTestSendingObject>();
			header.Bills.AddNew();
			var sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
			using (var form = new MessageSendingForm(sendingObjParent))
			{
				form.Show();
				var continueToSendCheckBox = form.FindSingle<ZCheckBox>("IsFinalManifestCheckBox");
				CombineAssertions(() =>
				{
					Assert("Visible", continueToSendCheckBox.Visible);
					AssertEquals("Full Description", "By checking this checkbox, you confirm that this is the final manifest for the master bill number, and all manifests under the master bill number have been submitted to customs.", continueToSendCheckBox.CaptionResourceString.FullDescription);
					AssertEquals("Caption", "Final Manifest?", continueToSendCheckBox.CaptionResourceString.Caption);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MessageSendingForm(sendingObjParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeaderForTestSendingObject>();
			bill = header.Bills.AddNew();
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
		}

		AsycudaManifestHeaderForTestSendingObject header;
		AsycudaBillForTestSendingObject bill;
		MessageSendingObjectParentForTestSendingObject sendingObjParent;
	}
}
