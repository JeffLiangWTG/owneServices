using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	class AreaEntryFormTest : WhsGuiTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var area = Factory.New<WhsArea>();
			using (var form = new AreaEntryFormForTest(area))
			{
				AssertEquals("Posting not setup", true, form.GetSetupPostingCalled());
			}
		}

		public void TestFormCaption()
		{
			var area = Factory.New<WhsArea>();
			using (var form = new AreaEntryForm(area))
			{
				form.Show();
				AssertEquals("Area", form.FormCaption);
			}
		}

		public void TestLocationsButton()
		{
			var warehouse = Helper.CreateWarehouse("1", "A", 1, 1);
			var area = warehouse.Areas[0];

			using (var form = new AreaEntryFormForTest(area))
			{
				form.Show();
				form.ClickLocationsButton();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("This Area must be saved before locations can be edited."));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				// #warning need to wait until locations form can be open for an area
				Assert("incomplete test", true);
				//	Factory.Save();
				//	Form.LocationsButton_Click(null, null);
				//	AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains("This Area must be saved before locations can be edited."));
				//	LocationForm LocationForm = (LocationForm) Form.Controller.LastShownForm;
				//	AssertNotNull(LocationForm);
				//	AssertEquals(true, LocationForm.Visible);
				//	LocationForm.Dispose();
			}
		}

		#region Pick Area tests

		public void TestPickAreaCheckboxes()
		{
			var warehouse = Helper.CreateWarehouse("1", "A", 1, 1);
			var area = warehouse.Areas[0];
			area.WA_IsPickingArea = false;
			area.WA_IsDefaultPickArea = false;

			using (var form = new AreaEntryFormForTest(area))
			{
				form.Show();

				Assert(GuiTestHelper.FindControl<ZCheckBox>(form.Controls, "PickingAreaCheckBox").Visible);
				Assert(GuiTestHelper.FindControl<ZCheckBox>(form.Controls, "PutawayAreaCheckBox").Visible);
				Assert(GuiTestHelper.FindControl<ZCheckBox>(form.Controls, "DefaultPickAreaCheckBox").Visible);
				Assert(GuiTestHelper.FindControl<ZCheckBox>(form.Controls, "DefaultPutawayAreaCheckBox").Visible);
			}
		}

		#endregion
	}

	class AreaEntryFormForTest : AreaEntryForm
	{
		public AreaEntryFormForTest(WhsArea area)
			: base(area)
		{
		}

		public bool GetSetupPostingCalled() => SetupPostingCalled;

		public void ClickLocationsButton() => LocationsButton_Click(null, null);
	}
}
