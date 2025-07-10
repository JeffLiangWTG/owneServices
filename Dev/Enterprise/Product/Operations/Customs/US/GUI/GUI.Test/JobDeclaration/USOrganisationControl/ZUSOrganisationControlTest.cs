using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.GUI.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ZUSOrganisationControlTest : TestCaseWithDummy
	{
		public void TestControlSize()
		{
			using (var form = new FormWithUSOrganisationControl(dummyParent))
			{
				form.Show();
				AssertEquals("Precondition - initial control Height should be " + ZUSOrganisationControl.ControlHeight + ".", ZUSOrganisationControl.ControlHeight, form.USOrganisationControl.Height);
				AssertEquals("Precondition - initial control Width should be " + ZUSOrganisationControl.ControlWidth + ".", ZUSOrganisationControl.ControlWidth, form.USOrganisationControl.Width);
				form.USOrganisationControl.Height = 10;
				form.USOrganisationControl.Width = 5;
				AssertEquals("Control Height should be fixed at " + ZUSOrganisationControl.ControlHeight + ".", ZUSOrganisationControl.ControlHeight, form.USOrganisationControl.Height);
				AssertEquals("Control Width should be fixed at " + ZUSOrganisationControl.ControlWidth + ".", ZUSOrganisationControl.ControlWidth, form.USOrganisationControl.Width);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteBoundUSOrganisation()
		{
			var collection = new USOrganisationTest.DummyWithUSOrganisationCollection(Factory);
			using (var form = new FormWithUSOrganisationControl(collection))
			{
				form.Size = new Size(600, 300);
				form.Show();
				var dummy = collection.AddNew();
				dummy.Delete();
			}
		}

		public void TestSelectFromPopup()
		{
			using (var form = new FormWithUSOrganisationControl(dummyParent))
			{
				form.Show();
				AssertEquals("Popup not shown yet", false, form.USOrganisationControl.PopupShown);
				dummyParent.OrgPK = orgDummy.PK;
				form.USOrganisationControl.SelectFromPopupForm();
				AssertEquals("Popup should have been shown", true, form.USOrganisationControl.PopupShown);
				dummyParent.OrgPK = ZGuid.Invalid;
				form.USOrganisationControl.PopupShown = false;
				form.USOrganisationControl.SelectFromPopupForm();
				AssertEquals("Popup should have been shown", true, form.USOrganisationControl.PopupShown);
			}
		}

		public void TestShowSelectedAddress()
		{
			orgDummy.OH_FullName = "TEST ORG DUMMY";
			orgDummy.OH_Code = "ORG1";
			var mainAddress = orgDummy.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			mainAddress.OA_Address2 = "11 Street";
			mainAddress.OA_RL_NKRelatedPortCode = "US";
			var address2 = orgDummy.Addresses.AddNew();
			address2.OA_Address1 = "ADDRESS 2";
			address2.OA_Address2 = "22 Street";
			address2.OA_CompanyNameOverride = "NEW COMPANY NAME";
			address2.OA_RL_NKRelatedPortCode = "US";
			using (var form = new FormWithUSOrganisationControl(dummyParent))
			{
				form.Show();
				dummyParent.OrgPK = orgDummy.PK;
				dummyParent.Organisation.ZO_OA_Address = mainAddress.PK;
				var control = form.FindSingle<ZUSOrganisationWrappedControl>();
				AssertEquals(mainAddress.PK, control.USOrganisation.Address.PK);
				var addressLabel = control.FindSingle<ZLabel>("AddressLabel");
				var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "ADDRESS 1", "11 Street", "United States").ToUpper();
				AssertMultilineASCIIEquals(expectedOrgAddressFormatted, addressLabel.Text);
				dummyParent.Organisation.ZO_OA_Address = address2.PK;
				AssertEquals(address2.PK, control.USOrganisation.Address.PK);
				expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "ADDRESS 2", "22 Street", "United States").ToUpper();
				AssertMultilineASCIIEquals(expectedOrgAddressFormatted, addressLabel.Text);
			}
		}

		public void TestDataSourceType()
		{
			using (var control = new ZUSOrganisationControl())
			{
				AssertEquals(typeof(USOrganisation), control.DataSourceType);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgDummy = Factory.NewWithValidTestData<OrgHeader>();
			dummyParent = new USOrganisationTest.DummyWithUSOrganisation(Factory);
		}

		USOrganisationTest.DummyWithUSOrganisation dummyParent;
		OrgHeader orgDummy;

		sealed class FormWithUSOrganisationControl : ZChildForm
		{
			internal FormWithUSOrganisationControl(USOrganisationTest.DummyWithUSOrganisation dummy) : base(dummy)
			{
			}

			internal FormWithUSOrganisationControl(USOrganisationTest.DummyWithUSOrganisationCollection dummyCollection) : base(dummyCollection)
			{
			}

			internal ZUSOrganisationControl USOrganisationControl;
			protected override void InitializeComponent()
			{
				USOrganisationControl = new ZUSOrganisationControl();
				USOrganisationControl.BindTo = "Organisation";
				USOrganisationControl.BindToContacts = "Contacts";
				USOrganisationControl.BindToOrganisations = "Organisations";
				USOrganisationControl.Caption = "US Organisation";
				Controls.Add(USOrganisationControl);
				base.InitializeComponent();
			}
		}
	}
}
