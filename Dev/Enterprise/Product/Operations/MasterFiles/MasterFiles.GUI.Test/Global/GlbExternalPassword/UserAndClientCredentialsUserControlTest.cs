using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public abstract class UserAndClientCredentialsUserControlTest : BasherTest
	{
		public override Form GetFormToBash()
		{
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = Activator.CreateInstance(BashType) as UserAndClientCredentialsUserControl;
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(GetDatabindingObject(), "");
			return form;
		}

		[RequiresSTA]
		public void TestPasswordStatusVisibility_WhenDataSourceIsNull()
		{
			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(null, "");
				form.Show();

				var userControl = GetFormControl<UserAndClientCredentialsUserControl>(form);
				AssertNull("Precondition", userControl.BindingSource.DataSource);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				AssertNotNull("Should find control", textBox1);
				AssertNotNull("Should find control", textBox2);
				Assert("Should be visible by default.", textBox1.Visible);
				Assert("Should be visible by default.", textBox2.Visible);
			}
		}

		public void TestPasswordStatusVisibility_WhenDataSourceIsUserAndClientCredentials()
		{
			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(GetDatabindingObject(), "");
				form.Show();

				var userControl = GetFormControl<UserAndClientCredentialsUserControl>(form);
				Assert("Precondition", userControl.BindingSource.DataSource is UserAndClientCredentials);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				AssertNotNull("Should find control", textBox1);
				AssertNotNull("Should find control", textBox2);
				Assert("Should be hidden when there is no error message.", !textBox1.Visible);
				Assert("Should be hidden when there is no error message.", !textBox2.Visible);
			}
		}

		[RequiresSTA]
		public void TestPasswordStatusVisibility_WhenDataSourceIsUserAndClientCredentialsWithStatusReasonError()
		{
			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(GetDatabindingObjectWithStatusReasonError(), "");
				form.Show();

				var userControl = GetFormControl<UserAndClientCredentialsUserControl>(form);
				Assert("Precondition", userControl.BindingSource.DataSource is UserAndClientCredentials);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				Assert("Should be visible when there is an error message.", textBox1.Visible);
				Assert("Should be visible when there is an error message.", textBox2.Visible);
			}
		}

		protected virtual UserAndClientCredentials GetDatabindingObject()
		{
			var userCredential = Factory.New<GlbExternalPassword>();
			var clientCredential = Factory.New<GlbExternalPassword>();
			var dataSource = new DummyUserAndClientCredentials(userCredential, clientCredential);

			AssertEquals(true, dataSource.UserCredentialPasswordStatusReason.IsEmpty);
			AssertEquals(true, dataSource.ClientCredentialPasswordStatusReason.IsEmpty);

			return dataSource;
		}

		protected virtual UserAndClientCredentials GetDatabindingObjectWithStatusReasonError()
		{
			var userCredential = Factory.NewWithValidTestData<GlbExternalPassword>();
			userCredential.GP_StatusReason = "taxpayer error";
			userCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;

			var clientCredential = Factory.NewWithValidTestData<GlbExternalPassword>();
			clientCredential.GP_StatusReason = "service provider error";
			clientCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;

			var dataSource = new DummyUserAndClientCredentials(userCredential, clientCredential);

			AssertEquals(false, dataSource.UserCredentialPasswordStatusReason.IsEmpty);
			AssertEquals(false, dataSource.ClientCredentialPasswordStatusReason.IsEmpty);

			return dataSource;
		}

		protected virtual T GetFormControl<T>(ZChildForm form) where T : UserAndClientCredentialsUserControl
			=> form.Controls.OfType<UserAndClientCredentialsUserControl>().FirstOrDefault() as T;

		class DummyUserAndClientCredentials : UserAndClientCredentials
		{
			public DummyUserAndClientCredentials(GlbExternalPassword userCredential, GlbExternalPassword clientCredential) : base(userCredential, clientCredential)
			{
			}
		}
	}
}
