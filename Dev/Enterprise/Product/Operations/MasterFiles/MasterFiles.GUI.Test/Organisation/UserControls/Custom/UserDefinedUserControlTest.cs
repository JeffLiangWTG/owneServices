using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UserDefinedUserControl))]
	sealed class UserDefinedUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		public void TestReadonlyWhenControlIsReadOnly()
		{
			var testHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			using (var testForm = new ZForm(testHeader))
			using (var testControl = new UserDefinedUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();

				var readOnlyToggleControl = testControl as IReadOnlyToggleControl;
				AssertNotNull("UserDefinedUserControl should implement IReadOnlyToggleControl", readOnlyToggleControl);
				AssertEquals("Precondition: User control is not readonly", false, testControl.ReadOnly);

				readOnlyToggleControl.ReadOnly = true;
				AssertEquals("User control is readonly", true, testControl.ReadOnly);
				AssertEquals("DocumentLogoButton.ReadOnly", true, testControl.DocumentLogoButton.ReadOnly);
				AssertEquals("OrderStatusListEditButton.ReadOnly", true, testControl.OrderStatusListEditButton.ReadOnly);
				AssertEquals("OrderLineStatusListEditButton.IsEditable", true, testControl.OrderLineStatusListEditButton.ReadOnly);
			}
		}

		public void TestReadOnlySecurity()
		{
			Env.Security.OrgCustomModify.IsAllowed = true;
			AssertControlButtonsEnabledState(false);

			Env.Security.OrgCustomModify.IsAllowed = false;
			AssertControlButtonsEnabledState(true);
		}

		void AssertControlButtonsEnabledState(bool shouldBeReadOnly)
		{
			var testHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			using (var testForm = new ZForm(testHeader))
			using (var testControl = new UserDefinedUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Width = testControl.Width + 10;
				testForm.Height = testControl.Height + 10;
				testForm.Show();
				AssertEquals("Testing the enabled state of DocumentLogo button", !shouldBeReadOnly, testControl.DocumentLogoButton.Enabled);
				AssertEquals("Testing the enabled state of OrderStatusList button", !shouldBeReadOnly, testControl.OrderStatusListEditButton.Enabled);
				AssertEquals("Testing the enabled state of OrderLineStatusList button", !shouldBeReadOnly, testControl.OrderLineStatusListEditButton.Enabled);
			}
		}

		#region implemetation

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new UserDefinedUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyCustom" }; }
		}

		#endregion
	}
}
