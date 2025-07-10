using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class LongTextControlTest : TestCaseWithFactory
	{
		public void TestCaptionsWhenCurrentDataItemNotNull()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummy))
			using (var mockData = Res.UseMockData())
			using (var editControl = new LongTextControl())
			using (var testPanel = new ZUserControl())
			{
				mockData.Put("DummyBizo|Z0_Description", new ResourceStringData("DummyBizo|Z0_Description", "", "", "Data Caption", "Caption set as data level resource string"));
				testPanel.Size = new Size(200, 100);
				testPanel.Location = new Point(200, 10);
				testPanel.Controls.Add(editControl);
				editControl.Location = new Point(100, 10);
				editControl.SetDataBinding(dummy, "Z0_Description");
				testForm.Size = new Size(300, 100);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(testPanel);
				testForm.Show();
				testPanel.BindingSource.SetBindingMember(editControl, "Collection.Z0_Description");
				((Button)editControl.Controls["MoreButton"]).PerformClick();
				Application.DoEvents();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					AssertContains("Data Caption", popup.Text);
					AssertEquals("Data Caption", popup.Controls["LongTextGroupBox"].Text);
					AssertEquals("Caption set as data level resource string", ((StatusBar)(popup.Controls["MainStatusBar"])).Panels[0].Text);
				}
			}
		}

		public void TestReadOnlyWhenSetBeforeBinding()
		{
			using (var testForm = new ZChildForm(invoiceLine))
			{
				var testControl = new LongTextControl();
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(testControl, LongTextInfo.Name);
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();
				AssertEquals("Control ReadOnly", true, testControl.ReadOnly);
				AssertEquals("LongTextTextBox ReadOnly", true, testControl.LongTextTextBox.ReadOnly);
				invoiceLine.JI_Tariff = "1010101010";
				AssertEquals("Control ReadOnly", false, testControl.ReadOnly);
				AssertEquals("LongTextTextBox ReadOnly", false, testControl.LongTextTextBox.ReadOnly);
				invoiceLine.JI_Tariff = "";
				AssertEquals("Control ReadOnly", true, testControl.ReadOnly);
				AssertEquals("LongTextTextBox ReadOnly", true, testControl.LongTextTextBox.ReadOnly);
				invoiceLine.JI_Tariff = "1010101010";
				AssertEquals("Control ReadOnly", false, testControl.ReadOnly);
				AssertEquals("LongTextTextBox ReadOnly", false, testControl.LongTextTextBox.ReadOnly);
			}
		}

		public void TestBindingWithNormalProperty()
		{
			using (var testForm = new ZChildForm(invoiceLine))
			{
				var testControl = new LongTextControl();
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(testControl, LongTextInfo.Name);
				testForm.Controls.Add(testControl);
				button = new ZButton();
				button.Location = new Point(0, 30);
				testForm.Controls.Add(button);
				testForm.Show();

				var testValue = new ZString("Test Description");
				LongTextInfo.Value = testValue;
				ChangeFocusToInvokeBinding();
				AssertEquals("Business Object has the changed value", testValue.ToUpper(), testControl.LongTextTextBox.Text);
			}
		}

		public void TestLongTextFormDataBinding_PropertyOnCurrentItem()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Description = "AAA";
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Description = "BBB";
			using (var testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();
				var control = testForm.CustomsBrokerageUserControl;
				var tabPage = control.InvoiceLinesTabPage;
				control.MainTabControl.SelectedTab = tabPage;
				var grid = (ZGrid)FindControl(tabPage, "CustomsInvoiceLinesBoundGrid");
				grid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				var moreButton = (Button)FindControl(testForm, "MoreButton");
				moreButton.PerformClick();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					AssertEquals("AAA", FindControl(popup, "LongTextTextBox").Text);
				}

				grid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				moreButton.PerformClick();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					AssertEquals("BBB", FindControl(popup, "LongTextTextBox").Text);
				}
			}
		}

		public void TestLongTextFormDataBinding_PropertyOnDataSource()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.TopGroupInvoice.JZ_Remarks = "AAA";
			using (var testForm = new ZForm(declaration))
			using (var longTextControl = new LongTextControl())
			{
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(longTextControl, "TopGroupInvoice+JZ_Remarks");
				testForm.Controls.Add(longTextControl);
				testForm.Show();
				var moreButton = (Button)FindControl(testForm, "MoreButton");
				moreButton.PerformClick();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					AssertEquals("AAA", FindControl(popup, "LongTextTextBox").Text);
				}
			}
		}

		public void TestReadOnly_SetValueWhenNoRowInGrid()
		{
			using (var control = new LongTextControl())
			{
				var readOnlyProperty = TypeDescriptor.GetProperties(typeof(LongTextControl))["ReadOnly"];
				readOnlyProperty.SetValue(control, null); // simulating what happens when there is no row in the parent grid
				AssertEquals("Control read only when no row in grid", true, control.ReadOnly);
			}
		}

		public void TestCaptionsFromDataLevelResourceString()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var mockData = Res.UseMockData())
			using (var testForm = new ZForm(dummy))
			using (var editControl = new LongTextControl())
			{
				mockData.Put("DummyBizo|Z0_Description", new ResourceStringData("DummyBizo|Z0_Description", "", "", "Data Caption", "Caption set as data level resource string"));
				editControl.Location = new Point(200, 10);
				editControl.SetBindingMember(DummyBusinessObject.Schema.Z0_Description);
				testForm.Size = new Size(300, 100);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(editControl);
				testForm.Show();
				((Button)editControl.Controls["MoreButton"]).PerformClick();
				Application.DoEvents();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					AssertContains("Data Caption", popup.Text);
					AssertEquals("Data Caption", popup.Controls["LongTextGroupBox"].Text);
					AssertEquals("Caption set as data level resource string", ((StatusBar)(popup.Controls["MainStatusBar"])).Panels[0].Text);
				}
			}
		}

		public void TestCaptionsFromGUILevelResourceString()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (Res.UseMockData())
			using (var testForm = new ZForm(dummy))
			using (var editControl = new LongTextControl())
			{
				editControl.GetExtension<ZLabelCaptionRenderer>().CopyCaptionToPropertyHumanReadableNameForTest = true;
				editControl.Location = new Point(200, 10);
				editControl.SetBindingMember(DummyBusinessObject.Schema.Z0_Description);
				editControl.CaptionResourceString = NoResourceStringData.GetData("GUI Caption", "Caption set as GUI level resource string");
				testForm.Size = new Size(300, 100);
				testForm.CaptionRenderingEnabled = true;
				testForm.Controls.Add(editControl);
				testForm.Show();
				((Button)editControl.Controls["MoreButton"]).PerformClick();
				Application.DoEvents();
				using (var popup = ZFormModaliser.ActiveForm)
				{
					AssertContains("GUI Caption", popup.Text);
					AssertEquals("GUI Caption", popup.Controls["LongTextGroupBox"].Text);
					AssertEquals("Caption set as GUI level resource string", ((StatusBar)(popup.Controls["MainStatusBar"])).Panels[0].Text);
				}
			}
		}

		public void TestNotificationsShown()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (dummy.SuspendValidationTesting())
			using (var testForm = new ZForm(dummy))
			using (var editControl = new LongTextControl())
			{
				editControl.Location = new Point(100, 10);
				editControl.SetBindingMember(DummyBusinessObject.Schema.Z0_Description);
				testForm.Size = new Size(300, 100);
				testForm.Controls.Add(editControl);
				testForm.Show();
				MandatoryValidation.MessageErrorIfIsEntered(dummy.Z0_DescriptionInfo);
				var notificationExtension = editControl.Extensions.Get<NotificationExtension>();
				AssertNotNull("Should have notification extension", notificationExtension);
				var errorMessage = notificationExtension.Notifications.GetMessageErrors().GetFirstMessage();
				AssertContains(MandatoryValidation.DoNotEntered, errorMessage);
			}
		}

		public void TestCharacterCasing()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummy))
			{
				var editControl = new LongTextControl();
				editControl.Location = new Point(200, 10);
				editControl.SetBindingMember(DummyBusinessObject.Schema.Z0_Description);
				testForm.Size = new Size(300, 100);
				testForm.Controls.Add(editControl);
				testForm.Show();
				editControl.Focus();
				Application.DoEvents();
				var longTextTextBox = editControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "LongTextTextBox");
				longTextTextBox.Focus();
				Application.DoEvents();
				editControl.CharacterCasing = CharacterCasing.Normal;
				AssertEquals(CharacterCasing.Normal, longTextTextBox.CharacterCasing);
				longTextTextBox.Text = "r";
				AssertEquals("r", longTextTextBox.Text);
				longTextTextBox.Text = "R";
				AssertEquals("R", longTextTextBox.Text);
				editControl.CharacterCasing = CharacterCasing.Upper;
				AssertEquals(CharacterCasing.Upper, longTextTextBox.CharacterCasing);
				longTextTextBox.Text = "r";
				AssertEquals("R", longTextTextBox.Text);
				editControl.CharacterCasing = CharacterCasing.Lower;
				AssertEquals(CharacterCasing.Lower, longTextTextBox.CharacterCasing);
				longTextTextBox.Text = "R";
				AssertEquals("r", longTextTextBox.Text);
			}
		}

		Control FindControl(Control control, string name)
		{
			Control result = null;
			if (control.Name == name)
			{
				result = control;
			}
			else
			{
				foreach (Control childControl in control.Controls)
				{
					result = FindControl(childControl, name);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		ZButton button;
		BaseJobDeclaration declaration;
		BaseJobComInvoiceLine invoiceLine;

		ZPropertyInfo LongTextInfo => invoiceLine.JI_DescriptionInfo;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = Factory.New<JobComInvoiceLineForTesting>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			LongTextInfo.Value = ZString.Empty;
		}

		void ChangeFocusToInvokeBinding()
		{
			button.Focus();
			Application.DoEvents();
		}

		class JobComInvoiceLineForTesting : BaseJobComInvoiceLine
		{
			public JobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ReadOnlyMember(nameof(JI_Description_ReadOnly))]
			public override ZString JI_Description { get => base.JI_Description; set => base.JI_Description = value; }

			public ZBool JI_Description_ReadOnly => JI_Tariff.IsEmpty;
		}
	}
}
