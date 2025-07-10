using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SecurityFindBox_Test : TransactionedTestCase
	{
		public void TestTypeDescriptionProviderNotUsed()
		{
			TypeDescriptionProviderAttribute attr = (TypeDescriptionProviderAttribute)TypeDescriptor.GetAttributes(typeof(SecurityFindBox))[typeof(TypeDescriptionProviderAttribute)];
			AssertEquals("TypeDescriptionProviderAttribute causes the 'Security Rights' find box on the staff module to clear when 'Find' is clicked for some reason", null, attr);
		}

		public void TestBinding()
		{
			DummyBusinessObject businessObject = new DummyBusinessObject();

			using (DummyZForm form = new DummyZForm(businessObject))
			{
				form.Show();

				form.FindBox.CodeTextBox.Focus();
				form.FindBox.CodeTextBox.Text = "X";
				form.DummyControl.Focus();
				AssertEquals("businessObject.LookupKey.Code", "X", businessObject.SecurityFilterContainer.LookupKey.Code);
				AssertEquals("FindBox.Notifications", "Please select a valid Security Right.", form.FindBox.Extensions.Get<INotificationExtension>().Notifications.ToUniqueMessageListString());

				form.FindBox.CodeTextBox.Focus();
				form.FindBox.CodeTextBox.Text = Env.Security.Operations.Code;
				form.DummyControl.Focus();
				AssertEquals("businessObject.LookupKey.Code", Env.Security.Operations.Code.ToUpperInvariant(), businessObject.SecurityFilterContainer.LookupKey.Code);
				AssertEquals("FindBox.Notifications empty", "", form.FindBox.Extensions.Get<INotificationExtension>().Notifications.ToUniqueMessageListString());
			}
		}

		public void TestResize()
		{
			using (SecurityFindBox findBox = new SecurityFindBox())
			{
				findBox.Size = new Size(300, 20);
				AssertEquals(findBox.CodeTextBox.Width, ControlDpiScalingHelper.ScaleToCurrentDpiX(90));

				findBox.Size = new Size(600, 20);
				AssertEquals(findBox.CodeTextBox.Width, ControlDpiScalingHelper.ScaleToCurrentDpiX(180));
			}
		}

		public void TestSelectButtonFont()
		{
			using (SecurityFindBox findBox = new SecurityFindBox())
			{
				AssertEquals("SelectButton.Font", OFont.GetFontBold(), findBox.SelectButton.Font);
			}
		}

		public void TestSelectSecurity()
		{
			DummyBusinessObject businessObject = new DummyBusinessObject();
			using (DummyZForm form = new DummyZForm(businessObject))
			{
				form.Show();

				form.FindBox.Focus();
				form.FindBox.CodeTextBox.Text = Env.Security.Forwarding.Code;
				form.ValidateChildren();

				AssertEquals(Env.Security.Forwarding.HumanReadableName.ToString().ToUpperInvariant(), form.FindBox.HumanReadableNameTextBox.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				form.FindBox.SelectButton.PerformClick();
				using (SecuritySelectionForm selectionForm = (SecuritySelectionForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("selectionForm.SecurityTreeView.SelectedNode.CheckPoint", Env.Security.Forwarding, ((ZSecurityPointNode)selectionForm.SecurityTreeView_Exposed.SelectedNode).Checkpoint);
					AssertEquals("FindBox.LookupKey.Code", Env.Security.Forwarding.Code.ToUpperInvariant(), form.FindBox.LookupKey.Code);
					AssertEquals("FindBox.CodeTextBox.Text", Env.Security.Forwarding.Code.ToUpperInvariant(), form.FindBox.CodeTextBox.Text);
				}

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FindBox.SelectButton.PerformClick();
				using (SecuritySelectionForm selectionForm = (SecuritySelectionForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("selectionForm.SecurityTreeView.SelectedNode.CheckPoint", Env.Security.Forwarding, ((ZSecurityPointNode)selectionForm.SecurityTreeView_Exposed.SelectedNode).Checkpoint);
					AssertEquals("FindBox.LookupKey.Code", "", form.FindBox.LookupKey.Code);
					AssertEquals("FindBox.CodeTextBox.Text", "", form.FindBox.CodeTextBox.Text);
				}
			}
		}

		#region class DummyBusinessObject

		class DummyBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
		{
			SecurityFilterContainer securityFilterContainer;

			public DummyBusinessObject()
			{
			}

			public SecurityFilterContainer SecurityFilterContainer
			{
				get
				{
					if (securityFilterContainer == null)
					{
						securityFilterContainer = new SecurityFilterContainer();
						RegisterEditableChildObject(securityFilterContainer);
					}
					return securityFilterContainer;
				}
			}
		}

		#endregion

		#region class DummyZForm

		class DummyZForm : ZForm
		{
			KTextBox dummyControl;
			SecurityFindBox findBox;

			public DummyZForm(DummyBusinessObject businessEntity)
				: base(businessEntity)
			{
			}

			public TextBox DummyControl
			{
				get { return dummyControl; }
			}

			public SecurityFindBox FindBox
			{
				get { return findBox; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				dummyControl = new KTextBox();
				findBox = new SecurityFindBox();
				BindingSource.SetBindingMember(findBox, "SecurityFilterContainer.LookupKey");
				Controls.Add(dummyControl);
				Controls.Add(findBox);
			}
		}

		#endregion
	}
}
