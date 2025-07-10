using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	class RowEntryFormTest : WhsGuiTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var row = Factory.New<WhsRow>();
			using (var form = new RowEntryFormForTest(row))
			{
				AssertEquals("Posting not setup", true, form.GetSetupPostingCalled());
			}
		}

		public void TestFormCaption()
		{
			WhsRow row = Factory.New<WhsRow>();
			using (RowEntryForm form = new RowEntryForm(row))
			{
				form.Show();
				AssertEquals("Row", form.FormCaption);
			}
		}

		public void TestLocationsButton()
		{
			WhsWarehouse warehouse = Helper.CreateWarehouse("1", "A", 1, 1);
			WhsRow row = warehouse.Rows[0];

			using (var form = new RowEntryFormForTest(row))
			{
				form.Show();
				form.ClickLocationsButton();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("This row must be saved before locations can be edited."));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				form.ClickLocationsButton();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains("This row must be saved before locations can be edited."));

				LocationForm locationForm = (LocationForm)form.GetController().LastShownForm;
				AssertNotNull(locationForm);
				AssertEquals(row.PK, ((BusinessObject)locationForm.BusinessEntity).PK);
				AssertEquals(true, locationForm.Visible);
				locationForm.Dispose();
			}
		}

		public void TestColor()
		{
			var row = Factory.New<WhsRow>();
			using (var form = new RowEntryFormForTest(row))
			{
				// int GetColor(Color ColorObj)
				AssertEquals(System.Drawing.Color.HotPink.ToArgb(), form.GetFormColor(System.Drawing.Color.HotPink));

				// Color GetColor(int ColorInt)
				AssertEquals(System.Drawing.Color.HotPink.ToArgb(), form.GetFormColor(System.Drawing.Color.HotPink.ToArgb()).ToArgb());
			}
		}
	}

	class RowEntryFormForTest : RowEntryForm
	{
		public RowEntryFormForTest(WhsRow row)
			: base(row)
		{
		}

		public bool GetSetupPostingCalled() => SetupPostingCalled;

		public void ClickLocationsButton() => LocationsButton_Click(null, null);

		public ZController GetController() => ControllerForTest;

		public int GetFormColor(System.Drawing.Color color) => GetColorForTest(color);

		public System.Drawing.Color GetFormColor(int colorInt) => GetColorForTest(colorInt);
	}
}
