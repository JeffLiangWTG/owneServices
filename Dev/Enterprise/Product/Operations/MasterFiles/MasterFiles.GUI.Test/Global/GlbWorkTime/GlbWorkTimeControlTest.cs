using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbWorkTimeControlTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestControlKeysAreNotSuppressed()
		{
			using (var form = new ZForm())
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				form.Controls.Add(workTimeControl);
				form.Show();

				var source = workTimeControl.WorkingHoursTextBoxes.First();
				var destination = workTimeControl.WorkingHoursTextBoxes.Last();
				Assert("PRE: Source cannot equal destination", !ReferenceEquals(source, destination));

				source.Text = "    *****";

				//Really annoying. Form takes several DoEvents calls to reach a point where we can send keys and copy
				var timeBeforeGivingUp = TimeSpan.FromSeconds(5);

				var stopWatch = Stopwatch.StartNew();
				while (source.Text != SafeClipboard.GetText() && stopWatch.Elapsed < timeBeforeGivingUp)
				{
					Application.DoEvents();
					source.Focus();
					source.SelectAll();
					Application.DoEvents();

					SendKeys.SendWait("^(c)");
					Application.DoEvents();
				}

				AssertEquals("PRE: Clipboard should have data", source.Text, SafeClipboard.GetText());

				destination.Focus();
				SendKeys.SendWait("^(v)");

				AssertEquals(source.Text, destination.Text);
			}
		}

		public void TestWorkHoursTextboxFont()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			using (var form = new ZForm(department))
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				workTimeControl.SetDataBinding(department, "WorkTimeViewModel");
				form.Controls.Add(workTimeControl);
				form.Show();
				Application.DoEvents();
#if WINZOR
				var expectedFont = new Font("Courier New", 10F);
#else
				var expectedFont = new Font("Courier New", 10.5F);
#endif
				Assert("Should have a monospaced font", workTimeControl.WorkingHoursTextBoxes.All(textBox => textBox.Font.Equals(expectedFont)));
			}
		}

		public void TestWorkHoursTextboxMaxLength()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			using (var form = new ZForm(department))
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				workTimeControl.SetDataBinding(department, "WorkTimeViewModel");
				form.Controls.Add(workTimeControl);
				form.Show();

				Application.DoEvents();

				var elementsWithWrongMaxLength = workTimeControl.WorkingHoursTextBoxes
					.Where(tb => tb.MaxLength != 48)
					.Select(tb => $"{tb.Name}: {tb.MaxLength}");

				AssertContainsExactElementsInAnyOrder("Should have a max length of 48", Enumerable.Empty<string>(), elementsWithWrongMaxLength);
			}
		}

		public void TestWorkHoursTextChange()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			using (var form = new ZForm(department))
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				workTimeControl.SetDataBinding(department, "WorkTimeViewModel");
				form.Controls.Add(workTimeControl);
				form.Show();
				Application.DoEvents();

				foreach (var textBox in workTimeControl.WorkingHoursTextBoxes)
				{
					AssertTextChange(textBox);
				}
			}
		}

		void AssertTextChange(ZTextBox textBox)
		{
			textBox.Text = "   AA    ";
			textBox.SelectionStart = 3;
			textBox.Text = "aAs  vvv**$$$$$";

			AssertEquals(textBox.Name + " should allow only '*' and ' ' chars", textBox.Text, "  **");
		}

		public void TestTextChangeDoesNotRemoveCarat()
		{
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				var textBox = workTimeControl.WorkingHoursTextBoxes.First();

				textBox.Text = "   ***   ";
				textBox.SelectionStart = 4;
				textBox.Text = "   *Z**   ";

				AssertEquals(3, textBox.SelectionStart);
			}
		}

		[RequiresSTA]
		public void TestWorkHoursTextboxes()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			using (var form = new ZForm(department))
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				workTimeControl.SetDataBinding(department, "WorkTimeViewModel");
				form.Controls.Add(workTimeControl);
				form.Show();
				Application.DoEvents();

				foreach (var textBox in workTimeControl.WorkingHoursTextBoxes)
				{
					AssertKeyPress(textBox);
				}
			}
		}

		void AssertKeyPress(ZTextBox textBox)
		{
			textBox.SelectionStart = 0;
			textBox.Text = "";

			var keys = new[] { 'a', 'B', ' ', 'F', 'f', 'A', '*', ' ', '1', '^', (char)8 };
			foreach (var key in keys)
			{
				KeySender.SendKeyPress(textBox, textBox.Handle, key);
			}

			AssertEquals(textBox.Name + " should allow only '*' and ' ' chars", " *", textBox.Text);
		}

		public void TestPatternExistsLabelInvisible()
		{
			var staff = Factory.New<GlbStaff>();

			using (var form = new ZForm(staff))
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				workTimeControl.SetDataBinding(staff, "WorkTimeViewModel");
				form.Controls.Add(workTimeControl);
				form.Show();
				Application.DoEvents();

				AssertPatternReadonlyLabel(workTimeControl, false);
			}
		}

		[RequiresSTA]
		public void TestPatternExistsLabelVisible()
		{
			var staff = Factory.New<GlbStaff>();
			var pattern = Factory.New<GlbWorkPattern>();
			pattern.GWP_GS_Staff = staff.PK;
			pattern.GWP_EffectiveDate = ZDateTimeOffset.UtcNow.AddDays(-1);

			using (var form = new ZForm(staff))
			using (var workTimeControl = new GlbWorkTimeControl())
			{
				workTimeControl.SetDataBinding(staff, "WorkTimeViewModel");
				form.Controls.Add(workTimeControl);
				form.Show();
				Application.DoEvents();

				AssertPatternReadonlyLabel(workTimeControl, true);
			}
		}

		void AssertPatternReadonlyLabel(GlbWorkTimeControl workTimeControl, bool readOnly)
		{
			AssertLabelVisibility(workTimeControl, "PatternExistsLabel", readOnly);
			AssertLabelVisibility(workTimeControl, "AMLabel", !readOnly);
			AssertLabelVisibility(workTimeControl, "PMLabel", !readOnly);
		}

		void AssertLabelVisibility(GlbWorkTimeControl control, string labelName, bool visible)
		{
			var label = control.Controls.Find(labelName, true).Single();
			AssertEquals(labelName + ".Visible", visible, label.Visible);
		}
	}
}
