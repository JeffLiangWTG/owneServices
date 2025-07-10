using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class UXMLMatchedListUserControlTest : TestCase
	{
		public void TestUXMLMatchedResultUserControl()
		{
			using (var form = new ZForm())
			{
				var orgCodes = new[] { "a", "b", "c", "d" };
				var dataSource = new List<UXMLMatchingDiagnosticModel>()
				{
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: orgCodes[0],
						orgScore: 0.7,
						true,
						orgPK: new ZGuid("CAC9EC20-2B11-4C22-8543-000058F5B58E"),
						result: "Pass"),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: orgCodes[2],
						orgScore: 0.9,
						true,
						orgPK: new ZGuid("F77C505B-183B-4451-84ED-0007FBE7761F"),
						result: "Pass"),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: orgCodes[3],
						orgScore: 0.2,
						true,
						orgPK: new ZGuid("2A3A06EE-988D-4C78-8870-000C5E5663C9"),
						result: "Not Pass"),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: orgCodes[1],
						orgScore: 0.3,
						true,
						orgPK: new ZGuid("2A3A06EE-988D-4C78-8870-000C5E5663C9"),
						result: "Not Pass")
				};

				var uxmlMatchedListUserControl = new UXMLMatchedListUserControl();
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				uxmlMatchedListUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(uxmlMatchedListUserControl);
				form.Show();

				var tableLayout = (KTableLayoutPanel)uxmlMatchedListUserControl.Controls.Find("controlTableLayout", true).Single();
				var uxmlResultControls = tableLayout.Controls.Cast<UXMLMatchedResultUserControl>().ToList();
				var positions = orgCodes.Select(name => tableLayout.GetRow(FindUXMLResultControlByName(uxmlResultControls, name)));

				CombineAssertions(() =>
				{
					AssertArrayEqualsByElements("UXMLMatchedListUserControl should preserve element order", new[] { 0, 3, 1, 2 }, positions.ToArray());
					AssertEquals("UXMLMatchedListUserControl should have AutoScroll", true, (form.Controls.Find("UXMLMatchedListUserControl", true)[0] as UXMLMatchedListUserControl).AutoScroll);
				});
			}
		}

		UXMLMatchedResultUserControl FindUXMLResultControlByName(IEnumerable<UXMLMatchedResultUserControl> controls, string name) => controls.Single(c => c.Name.Replace("uxmlMatchedResult", string.Empty) == name);

		[RequiresSTA]
		public void TestUXMLMatchedResultUserControl_WithEmptyList()
		{
			using (var form = new ZForm())
			{
				var dataSource = new List<UXMLMatchingDiagnosticModel>() { };

				var itemControlNames = dataSource.Select(o => "uxmlMatchedResult" + o.MatchCode);

				var uxmlMatchedListUserControl = new UXMLMatchedListUserControl();
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				form.Controls.Add(uxmlMatchedListUserControl);
				form.Show();
				var matchedItemControls = itemControlNames.Select(o => form.Controls.Find(o, true)[0]);
				CombineAssertions(() =>
				{
					AssertEquals(0, matchedItemControls.Count());
					AssertEquals("UXMLMatchedListUserControl should have AutoScroll", true, (form.Controls.Find("UXMLMatchedListUserControl", true)[0] as UXMLMatchedListUserControl).AutoScroll);
				});
			}
		}

		public void TestClearPanel()
		{
			using (var form = new ZForm())
			{
				var dataSource = new List<UXMLMatchingDiagnosticModel>()
				{
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "asdfghj",
						orgScore: 0.7,
						true,
						orgPK: new ZGuid("CAC9EC20-2B11-4C22-8543-000058F5B58E"),
						result: "Pass"),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "ghrtsh",
						orgScore: 0.9,
						true,
						orgPK: new ZGuid("F77C505B-183B-4451-84ED-0007FBE7761F"),
						result: "Pass"),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "tmyer",
						orgScore: 0.3,
						true,
						orgPK: new ZGuid("2A3A06EE-988D-4C78-8870-000C5E5663C9"),
						result: "Not Pass"),
				};

				var itemControlNames = dataSource.Select(o => "uxmlMatchedResult" + o.MatchCode);

				var uxmlMatchedListUserControl = new UXMLMatchedListUserControl();
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				uxmlMatchedListUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(uxmlMatchedListUserControl);
				form.Show();

				var matchedItemControls = itemControlNames.Select(o => form.Controls.Find(o, true)[0]);
				AssertEquals("Pre-condition: current UXMLMatchedListUserControl should have 3 uxmlMatchedResult controls", 3, matchedItemControls.Count());

				uxmlMatchedListUserControl.ClearPanel();

				var tableLayout = uxmlMatchedListUserControl.Controls.Find("controlTableLayout", true)[0];
				AssertEquals(0, tableLayout.Controls.Count);
			}
		}

		[RequiresSTA]
		public void TestBackColor_Matched()
		{
			using (var form = new ZForm())
			{
				var dataSource = new List<UXMLMatchingDiagnosticModel>()
				{
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "ABCDEF",
						orgScore: 0.7,
						true,
						orgPK: new ZGuid("CAC9EC20-2B11-4C22-8543-000058F5B58E"),
						result: UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "PQRTUV",
						orgScore: 0.2,
						true,
						orgPK: new ZGuid("F77C505B-183B-4451-84ED-0007FBE7761F"),
						result: UXMLMatchingDiagnosticUtils.Constants.NotMatch),
				};

				var itemControlNames = dataSource.Select(o => "uxmlMatchedResult" + o.MatchCode);

				var uxmlMatchedListUserControl = new UXMLMatchedListUserControl();
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				uxmlMatchedListUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(uxmlMatchedListUserControl);
				form.Show();

				var matchedItemControls = itemControlNames.Select(o => form.Controls.Find(o, true)[0]).ToList();
				AssertEquals("Pre-condition: current UXMLMatchedListUserControl should have 2 uxmlMatchedResult controls", 2, matchedItemControls.Count);

				CombineAssertions(() =>
				{
					AssertEquals(UXMLMatchedListUserControl.Colors.Matched, matchedItemControls[0].BackColor);
					AssertEquals(UXMLMatchedListUserControl.Colors.Default, matchedItemControls[1].BackColor);
				});
			}
		}

		[RequiresSTA]
		public void TestBackColor_NotMatched()
		{
			using (var form = new ZForm())
			{
				var dataSource = new List<UXMLMatchingDiagnosticModel>()
				{
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "ABCDEF",
						orgScore: 0.5,
						true,
						orgPK: new ZGuid("CAC9EC20-2B11-4C22-8543-000058F5B58E"),
						result: UXMLMatchingDiagnosticUtils.Constants.NotMatch),
					new UXMLMatchingDiagnosticModel(
						matchedOrgCode: "PQRTUV",
						orgScore: 0.2,
						true,
						orgPK: new ZGuid("F77C505B-183B-4451-84ED-0007FBE7761F"),
						result: UXMLMatchingDiagnosticUtils.Constants.NotMatch),
				};

				var itemControlNames = dataSource.Select(o => "uxmlMatchedResult" + o.MatchCode);

				var uxmlMatchedListUserControl = new UXMLMatchedListUserControl();
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				uxmlMatchedListUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(uxmlMatchedListUserControl);
				form.Show();

				var matchedItemControls = itemControlNames.Select(o => form.Controls.Find(o, true)[0]).ToList();
				AssertEquals("Pre-condition: current UXMLMatchedListUserControl should have 2 uxmlMatchedResult controls", 2, matchedItemControls.Count);

				CombineAssertions(() =>
				{
					AssertEquals(UXMLMatchedListUserControl.Colors.NotMatched, matchedItemControls[0].BackColor);
					AssertEquals(UXMLMatchedListUserControl.Colors.Default, matchedItemControls[1].BackColor);
				});
			}
		}
	}
}
