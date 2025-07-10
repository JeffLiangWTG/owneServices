using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CombineTranslatedAddressForm))]
	sealed class CombineTranslatedAddressFormTest : ZFormBasherTest
	{
		OrgAddress GetNewAddressForTesting(OrgHeader org)
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = Guid.NewGuid().ToString().Substring(0, 25);
			return address;
		}

		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = GetNewAddressForTesting(org);
			var addressB = GetNewAddressForTesting(org);

			var addressCombiner = new AddressCombiner(Factory, addressA, addressB);
			return new CombineTranslatedAddressForm(addressCombiner);
		}

		public void TestUserControlsLoadedwithAddressEntities()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = GetNewAddressForTesting(org);
			var addressB = GetNewAddressForTesting(org);

			using (var testForm = new TestForm(new AddressCombiner(Factory, addressA, addressB)))
			{
				testForm.Show();
				testForm.UserControlA.SetDataBinding(addressA, "");
				testForm.UserControlB.SetDataBinding(addressB, "");
				AssertEquals(addressA.PK, testForm.UserControlA.CurrentAddress.PK);
				AssertEquals(addressB.PK, testForm.UserControlB.CurrentAddress.PK);
			}
		}

		public void TestWarningMessageDialog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = GetNewAddressForTesting(org);
			var addressB = GetNewAddressForTesting(org);
			Factory.Save();

			using (var testForm = new TestForm(new AddressCombiner(Factory, addressA, addressB)))
			{
				testForm.Show();
				testForm.UserControlA.SetDataBinding(addressA, "");
				testForm.UserControlB.SetDataBinding(addressB, "");
				Application.DoEvents();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				testForm.SaveAsButton1Exposed.PerformClick();
				using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals(string.Format("By proceeding, {0} will be changed to a new translated address for {1}. All jobs referencing {0} will be changed to {1} Are you sure you want to proceed?", addressA.DisplayText, addressB.DisplayText), (lastShownForm as ZMessageBox).MessageMultilingual.ToString());
				}
			}

			ZFormModaliser.LastFormShownForTest = null;
			using (var testForm = new TestForm(new AddressCombiner(Factory, addressB, addressA)))
			{
				testForm.Show();
				testForm.UserControlA.SetDataBinding(addressA, "");
				testForm.UserControlB.SetDataBinding(addressB, "");
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				testForm.SaveAsButton2Exposed.PerformClick();
				using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals(string.Format("By proceeding, {0} will be changed to a new translated address for {1}. All jobs referencing {0} will be changed to {1} Are you sure you want to proceed?", addressA.DisplayText, addressB.DisplayText), (lastShownForm as ZMessageBox).MessageMultilingual.ToString());
				}
			}
		}

		public void TestErrorDialogShowsWhenSourceAddressContainsATranslatedAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = GetNewAddressForTesting(org);
			var addressB = GetNewAddressForTesting(org);
			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			addressA.TranslatedAddresses.Add(translatedAddress);
			Factory.Save();

			using (var testForm = new TestForm(new AddressCombiner(Factory, addressA, addressB)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Show();
				testForm.UserControlA.SetDataBinding(addressA, "");
				testForm.UserControlB.SetDataBinding(addressB, "");
				Application.DoEvents();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				testForm.SaveAsButton1Exposed.PerformClick();
				Assert("Error Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error Message Text", "Unable to combine. The Dissolved address has one or more existing translated addresses.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveButton1ClickSetsAddressAAsTranslatedAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = GetNewAddressForTesting(org);
			var addressB = GetNewAddressForTesting(org);
			Factory.Save();

			using (var testForm = new TestForm(new AddressCombiner(Factory, addressA, addressB)))
			{
				testForm.Show();
				Assert(addressB.ReadOnly);
				testForm.UserControlA.SetDataBinding(addressA, "");
				testForm.UserControlB.SetDataBinding(addressB, "");
				Application.DoEvents();
				testForm.SaveAsButton1Exposed.PerformClick();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertEquals(0, addressB.TranslatedAddresses.Count);
				var address1 = addressA.Address1;
				testForm.SaveAsButton1Exposed.PerformClick();
				Assert(addressA.IsDeleted);
				AssertEquals(1, addressB.TranslatedAddresses.Count);
				testForm.Close();
			}
			Assert(!addressB.ReadOnly);
		}

		public void TestSaveButton2ClickSetsAddressBAsTranslatedAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = GetNewAddressForTesting(org);
			var addressB = GetNewAddressForTesting(org);
			Factory.Save();

			using (var testForm = new TestForm(new AddressCombiner(Factory, addressA, addressB)))
			{
				testForm.Show();
				Assert(addressA.ReadOnly);
				testForm.UserControlA.SetDataBinding(addressA, "");
				testForm.UserControlB.SetDataBinding(addressB, "");
				Application.DoEvents();
				testForm.SaveAsButton2Exposed.PerformClick();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertEquals(0, addressA.TranslatedAddresses.Count);
				var address2 = addressB.Address1;
				testForm.SaveAsButton2Exposed.PerformClick();
				Assert(addressB.IsDeleted);
				AssertEquals(1, addressA.TranslatedAddresses.Count);
				testForm.Close();
			}
			Assert(!addressB.ReadOnly);
		}

		public class TestForm : CombineTranslatedAddressForm
		{
			public TestForm(AddressCombiner businessEntity) : base(businessEntity)
			{
				BusinessEntity = businessEntity;
			}
			public override IBusiness BusinessEntity { get; }

			public ZButton SaveAsButton1Exposed { get { return SaveAsButton1; } }

			public ZButton SaveAsButton2Exposed { get { return SaveAsButton2; } }

			public CombineTranslatedAddressUserControl UserControlA
			{
				get
				{
					return CombineTranslatedAddressAUserControl;
				}
			}

			public CombineTranslatedAddressUserControl UserControlB
			{
				get
				{
					return CombineTranslatedAddressBUserControl;
				}
			}
		}
	}
}
