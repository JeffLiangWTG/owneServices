using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZOrgAddressControlTest : ZControlBaseTestCase<ZOrgAddressControl>
	{
		[RequiresSTA]
		public void TestConstructor()
		{
			using (ZOrgAddressControl control = new ZOrgAddressControl())
			{
				AssertNotNull(control);
			}
		}

		public void TestReadOnlyWhenNoCurrent()
		{
			using (TestZOrgAddressControlForm testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AddressControl.Focus();

				AssertNotNull("ZOrgAddressControl should be instantiated.", testForm.AddressControl);
				AssertEquals("ReadOnly value when No Current", true, testForm.AddressControl.ReadOnly);

				DummyWithAddy.Dummies.AddNew();
				AssertEquals("ReadOnly value when Current", false, testForm.AddressControl.ReadOnly);

				DummyWithAddy.Dummies.RemoveAll();
				AssertEquals("ReadOnly value when No Current", true, testForm.AddressControl.ReadOnly);
			}
		}

		public void TestReadOnlyWhenCurrentIsReadOnly()
		{
			using (var testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AddressControl.Focus();
				var dummy = DummyWithAddy.Dummies.AddNew();
				AssertEquals("Pre-Condition", false, testForm.AddressControl.ReadOnly);

				dummy.ReadOnly = true;
				AssertEquals("ReadOnly value when Current is read only", true, testForm.AddressControl.ReadOnly);

				dummy.ReadOnly = false;
				AssertEquals("ReadOnly value when Current is not read only", false, testForm.AddressControl.ReadOnly);
			}
		}

		[ExpectNoExceptions()]
		public void TestClickOnLinksDoesNotThrowException()
		{
			using (TestZOrgAddressControlForm testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AddressControl.Focus();

				// current BizO is NOT selected
				AssertNotNull("ZOrgAddressControl should be instantiated.", testForm.AddressControl);
				AssertEquals("ReadOnly value when No Current", true, testForm.AddressControl.ReadOnly);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.AddressControl.AddressesLink_LinkClicked(null, null);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Organization form could not be opened, because valid Organization has not been selected."));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.AddressControl.ContactsLink_LinkClicked(null, null);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Organization form could not be opened, because valid Organization has not been selected."));
			}
		}

		ZAddressFindBox.Bare GetOrganisationFindBox(ZOrgAddressControl instance)
		{
			var field = typeof(ZOrgAddressControl).GetField("OrganisationFindBox", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			return (ZAddressFindBox.Bare)field.GetValue(instance);
		}

		public void TestOrganisationFindBoxWidth()
		{
			using (TestZOrgAddressControlForm testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AddressControl.Focus();
				var findbox = GetOrganisationFindBox(testForm.AddressControl);
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(106, 20, true).Width, findbox.Width);
			}
		}

		public void TestSettingViaBusinessLayer()
		{
			DummyWithAddy.Dummies.AddNew();
			using (TestZOrgAddressControlForm testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				DummyWithAddy.Dummies[0].Z0_Guid = new ZGuid("131A8DEE-603D-47FA-98E9-2E69BDBBCCF5");
				AssertEquals("Label Test",
					"MIDWAY METALS\nCNR ROM AND BINARY STREETS\nYATALA, QLD\n4207\nAUSTRALIA",
					testForm.AddressControl.AddressLabel.Text);
			}
		}

		public void TestChangingDataSource()
		{
			DummyWithZAddress dummy1 = DummyWithAddy.Dummies.AddNew();
			DummyWithZAddress dummy2 = DummyWithAddy.Dummies.AddNew();

			dummy1.Z0_Guid = new ZGuid("131A8DEE-603D-47FA-98E9-2E69BDBBCCF5");
			dummy2.Z0_Guid = new ZGuid("B681B902-685A-4AFC-85E2-C312C0B29F4B");

			string dummy1Address = "MIDWAY METALS\nCNR ROM AND BINARY STREETS\nYATALA, QLD\n4207\nAUSTRALIA";
			string dummy2Address = "DEMO ORGANISATION\n111 DEMO ST\nDEMOVILLE NSW 2000\nAUSTRALIA";

			using (TestZOrgAddressControlForm testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();

				AssertEquals("AddressTextBox.Text", dummy1Address, testForm.AddressControl.AddressLabel.Text);
				AssertEquals("dummy1.Z0_Guid_ZAddress.IsOrgVisible", true, dummy1.Z0_Guid_ZAddress.IsOrgVisible);
				AssertEquals("dummy2.Z0_Guid_ZAddress.IsOrgVisible", false, dummy2.Z0_Guid_ZAddress.IsOrgVisible);

				testForm.Grid.ListManager.Position = 1;
				AssertEquals("AddressTextBox.Text", dummy2Address, testForm.AddressControl.AddressLabel.Text.ToUpper());
				AssertEquals("dummy1.Z0_Guid_ZAddress.IsOrgVisible", true, dummy1.Z0_Guid_ZAddress.IsOrgVisible);
				AssertEquals("dummy2.Z0_Guid_ZAddress.IsOrgVisible", true, dummy2.Z0_Guid_ZAddress.IsOrgVisible);
			}
		}

		public void TestValidation()
		{
			DummyWithConditionalSetter dummy = Factory.New<DummyWithConditionalSetter>();
			DummyWithAddy.Dummies.Add(dummy);

			using (TestZOrgAddressControlForm testForm = new TestZOrgAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				ZAddressFindBox orgFindBox = GetOrganisationFindBox(testForm.AddressControl);
				Control codeBox = orgFindBox.Controls.Find("CodeBox", true)[0];

				orgFindBox.Focus();
				codeBox.Text = "ZZZ";
				testForm.Grid.Focus();
				AssertEquals("dummy.Z0_Guid_ZAddress.OrgPK", ZGuid.Invalid, dummy.Z0_Guid_ZAddress.OrgPK);
				TestCaseWithFactory.AssertHasError(dummy.Z0_GuidInfo, "Enter a valid Organization.");

				orgFindBox.Focus();
				codeBox.Text = "";
				testForm.Grid.Focus();
				AssertEquals("dummy.Z0_Guid_ZAddress.OrgPK", ZGuid.Empty, dummy.Z0_Guid_ZAddress.OrgPK);
				TestCaseWithFactory.AssertNoErrors(dummy.Z0_GuidInfo);

				orgFindBox.Focus();
				BusinessObject org = (BusinessObject)Factory.LoadTop1<IOrgHeader>(new ZQuery());
				codeBox.Text = org[OrgHeaderSchema.Constants.OH_Code].ToString();
				testForm.Grid.Focus();
				AssertEquals("dummy.Z0_Guid_ZAddress.OrgPK", org.PK, dummy.Z0_Guid_ZAddress.OrgPK);
				TestCaseWithFactory.AssertNoErrors(dummy.Z0_GuidInfo);
			}
		}

		public void TestDataSourceType()
		{
			using (ZOrgAddressControl control = new ZOrgAddressControl())
			{
				AssertEquals(typeof(ZAddress), control.DataSourceType);
			}
		}

		public void TestLinkImagesSetImmediatly()
		{
			using (var control = new ZOrgAddressControl())
			{
				AssertNotNull(control.ContactsLink.Image);
				AssertNotNull(control.AddressesLink.Image);
			}
		}

		#region DummyWithConditionalSetter

		class DummyWithConditionalSetter : DummyWithZAddress
		{
			public DummyWithConditionalSetter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[List("Organisations")]
			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set
				{
					if (base.Z0_Guid != value)
					{
						base.Z0_Guid = value;
					}
				}
			}
		}

		#endregion

		#region TestZOrgAddressControlForm

		class TestZOrgAddressControlForm : ZForm
		{
			ZOrgAddressControl addressControl;
			ZGrid grid;

			public TestZOrgAddressControlForm(DummyWithZAddress businessEntity)
				: base(businessEntity)
			{
			}

			public ZOrgAddressControl AddressControl
			{
				get { return addressControl; }
			}

			public ZGrid Grid
			{
				get { return grid; }
			}

			#region Windows Form Designer generated code

			protected sealed override void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				this.addressControl = new ZOrgAddressControl();
				this.grid = new ZGrid();
				((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
				this.SuspendLayout();

				this.BindingSource.DataSourceType = typeof(DummyWithZAddress);
				// 
				// MainStatusBar
				// 
				this.MainStatusBar.Location = new System.Drawing.Point(0, 91);
				this.MainStatusBar.Name = "MainStatusBar";
				this.MainStatusBar.Size = new System.Drawing.Size(420, 24);
				// 
				// MessageStatusBarPanel
				// 
				this.MessageStatusBarPanel.Width = 204;
				// 
				// ErrorStatusBarPanel
				// 
				this.ErrorStatusBarPanel.Width = 205;
				// 
				// AddressControl
				// 
				BindingSource.SetBindingMember(this.addressControl, "Dummies.Z0_Guid_ZAddress");
				this.addressControl.Location = new System.Drawing.Point(20, 16);
				this.addressControl.Name = "AddressControl";
				this.addressControl.PopupCaption = "";
				this.addressControl.Size = new System.Drawing.Size(383, 61);
				this.addressControl.TabIndex = 2;
				//
				// Grid
				//
				this.grid.BindTo = "Dummies";
				zTextBoxColumnStyleInfo1.Caption = "Text";
				zTextBoxColumnStyleInfo1.ColumnName = "Z0_VarCharMax";
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.grid.Name = "grid";
				// 
				// TestZOrgAddressControlForm
				// 

				this.ClientSize = new System.Drawing.Size(420, 115);
				this.Controls.Add(this.addressControl);
				this.Controls.Add(this.grid);
				this.Name = "TestZOrgAddressControlForm";
				this.Text = "TestZOrgAddressControlForm";
				this.Controls.SetChildIndex(this.MainStatusBar, 0);
				this.Controls.SetChildIndex(this.addressControl, 0);
				((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
				this.ResumeLayout(false);
			}

			#endregion
		}

		#endregion

		#region Implementation

		DummyWithZAddress dummyWithAddy;

		DummyWithZAddress DummyWithAddy
		{
			get { return dummyWithAddy ?? (dummyWithAddy = Factory.New<DummyWithZAddress>()); }
		}

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		#endregion
	}
}
