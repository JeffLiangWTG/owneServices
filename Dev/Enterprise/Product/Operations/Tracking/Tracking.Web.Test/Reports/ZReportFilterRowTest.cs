using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ZReportFilterRowTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			var filter = new TextField(Factory);
			filter.Value = "ABC";
			var testrow = new ZReportFilterRowForTest(filter);

			AssertEquals("ID", filter.PK.ToString(), testrow.ID);
			AssertSame("Filter", filter, testrow.FilterExposedForTest);
		}

		public void TestCreateFilterControls()
		{
			var filter = new TextField(Factory);
			filter.Value = "ABC";
			filter.DisplayName = "Test";

			using (var mockRes = Res.UseMockData())
			using (var testrow = new ZReportFilterRowForTest(filter))
			{
				const string displayNameLocalized = "测试";
				mockRes.SetResourceGetter(k => new ResourceStringData(k, displayNameLocalized));
				filter.DisplayNameLocalizedData = mockRes.Get("");

				testrow.CreateFilterControls();
				AssertEquals("Cells Count", 2, testrow.CellsExposedForTest.Count);

				AssertEquals("First Cell: Controls in Cell", 1, testrow.CellsExposedForTest[0].Controls.Count);
				AssertEquals("First Cell: Control should always be a Label", typeof(Label), testrow.CellsExposedForTest[0].Controls[0].GetType());

				AssertEquals("Second Cell: Controls in Cell", 1, testrow.CellsExposedForTest[1].Controls.Count);
				AssertEquals("Second Cell: Control Type", typeof(ZTextBox), testrow.CellsExposedForTest[1].Controls[0].GetType());

				var textBox = (ZTextBox)testrow.CellsExposedForTest[1].Controls[0];
				AssertEquals("textBox.Text", "", textBox.Text);
				testrow.BindFilterControls();
				AssertEquals("textBox.Text", filter.Value, textBox.Text);

				var label = (Label)testrow.CellsExposedForTest[0].Controls[0];
				AssertEquals("label.Text", "测试", label.Text);
			}
		}

		public void TestRegisterAsyncPostBackForUpdatingFilterControls()
		{
			var filter = new CodeListMultipleChoice(Factory);
			// Hack to have this filter considered for registering postback controls
			typeof(FilterField).GetProperty("IsUsedToDetermineReadOnlyOfRelatedFilter", BindingFlags.Public | BindingFlags.Instance).SetValue(filter, true, null);

			using (var page = new ZPage())
			{
				var manager = new ScriptManager();
				page.Items[typeof(ScriptManager)] = manager; // set script manager for the page

				var testRow = new ZReportFilterRowForTest(filter);
				page.Controls.Add(testRow);
				testRow.CreateFilterControls();

				bool onSelectedIndexChangedOfListControlHit = false;
				testRow.OnSelectedIndexChangedOfListControl += (sender, e) => onSelectedIndexChangedOfListControlHit = true;

				foreach (var control in testRow.CellsExposedForTest[1].Controls)
				{
					var listControl = control as ListControl;
					if (listControl != null)
					{
						AssertEquals(true, listControl.AutoPostBack);
						listControl.Items.Add(new ListItem("Test", "123"));
						((IPostBackDataHandler)listControl).RaisePostDataChangedEvent();
					}
				}

				AssertEquals(true, onSelectedIndexChangedOfListControlHit);
			}
		}

		#region Implementation

		class ZReportFilterRowForTest : ZReportFilterRow
		{
			public ZReportFilterRowForTest(FilterField filter)
				: base(filter)
			{
			}

			public FilterField FilterExposedForTest
			{
				get { return base.Filter; }
			}

			public HtmlTableCellCollection CellsExposedForTest
			{
				get { return base.Cells; }
			}
		}

		#endregion
	}
}
