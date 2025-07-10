using System;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Organisation.Registry.Testing
{
	sealed class OrganisationGuiStateTest : TransactionedTestCase
	{
		public void TestSaveInteger()
		{
			using (var form = new ZForm())
			using (var control = new ZUserControl())
			{
				form.Controls.Add(control);
				form.Name = "MyTestForm";
				control.Name = "MyTestControl";

				OrganisationGuiState.SaveInteger(control, "MyState", 1);
			}

			var reg = OrganisationRegistry.Instance.GuiStateIntegers;
			reg.Name = "MyTestForm.MyTestControl.MyState";
			AssertEquals("Should be the same as what was saved", 1, reg.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestLoadInteger()
		{
			var reg = OrganisationRegistry.Instance.GuiStateIntegers;
			reg.Name = "MyTestForm.MyTestControl.MyState";
			reg.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, 2);

			using (var form = new ZForm())
			using (var control = new ZUserControl())
			{
				form.Controls.Add(control);
				form.Name = "MyTestForm";
				control.Name = "MyTestControl";

				AssertEquals("Should be the same as what was saved", 2, OrganisationGuiState.LoadInteger(control, "MyState"));
			}
		}

		public void TestDeleteInteger()
		{
			using (var form = new ZForm())
			using (var control = new ZUserControl())
			{
				form.Controls.Add(control);
				form.Name = "MyTestForm";
				control.Name = "MyTestControl";

				OrganisationGuiState.SaveInteger(control, "MyState", 3);
				AssertEquals("Should be the same as what was saved", 3, OrganisationGuiState.LoadInteger(control, "MyState"));
				AssertEquals("Here", 1, (int)Db.Connection.ExecuteScalar(string.Format("select count(*) from dbo.stmdata where SD_Name = '{0}'", "MyTestForm.MyTestControl.MyState")));
				OrganisationGuiState.SaveInteger(control, "MyState", 0);
				AssertEquals("Gone", 0, (int)Db.Connection.ExecuteScalar(string.Format("select count(*) from dbo.stmdata where SD_Name = '{0}'", "MyTestForm.MyTestControl.MyState")));
				AssertEquals("Back to 0", 0, OrganisationGuiState.LoadInteger(control, "MyState"));
			}
		}

		public void TestSaveIntegerWithALargeKey()
		{
			using (var form = new ZForm())
			using (var control = new ZUserControl())
			{
				form.Controls.Add(control);
				form.Name = "MyTestForm";
				control.Name = "MyTestControl.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters";

				OrganisationGuiState.SaveInteger(control, "MyState", 1);
			}

			var reg = OrganisationRegistry.Instance.GuiStateIntegers;
			reg.Name = "...haracters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.300Characters.MyState";
			AssertEquals("Should be the same as what was saved", 1, reg.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestSaveIntegerWithNullNameValue()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => OrganisationGuiState.SaveInteger(null, null, 1));
		}
	}
}
