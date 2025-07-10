using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefCountryStatesForm))]
	sealed class RefCountryStatesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new RefCountryStatesForm(Factory.New<RefCountryStates>());

		public void TestFormIsActiveExists()
		{
			using (var form = new RefCountryStatesForm(Factory.New<RefCountryStates>()))
			{
				var isActiveCheckBox = (ZCheckBox)form.Controls.Find("IsActiveCheckBox", true).Single();
				isActiveCheckBox.Focus();
				AssertEquals("Control needs focus", false, isActiveCheckBox.Focused);
			}
		}

		public void TestAllowEnterLowerCaseInDescription_WhenOrgAllowMixedCaseRegistryIsTrue()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			using (var form = new RefCountryStatesForm(Factory.New<RefCountryStates>()))
			{
				AssertEquals("DescriptionTextBox.CharacterCasing", CharacterCasing.Normal, GetCharacterCasing(form, "DescriptionTextBox"));
			}
		}

		public void TestDontAllowEnterLowerCaseInDescription_WhenOrgAllowMixedCaseRegistryIsFalse()
		{
			Env.Registry.SetOrgAllowMixedCase(false);
			using (var form = new RefCountryStatesForm(Factory.New<RefCountryStates>()))
			{
				AssertEquals("DescriptionTextBox.CharacterCasing", CharacterCasing.Upper, GetCharacterCasing(form, "DescriptionTextBox"));
			}
		}

		public void TestTabNonWorkingDaysExists()
		{
			using (var form = new RefCountryStatesForm(Factory.New<RefCountryStates>()))
			{
				var nonWorkingDayTab = (ZTabPage)form.Controls.Find("zNonWorkingDaysTabPage", true).SingleOrDefault();
				AssertNotNull(nonWorkingDayTab);
			}
		}

		public void TestTabNonWorkingDaysFields()
		{
			using (var form = new RefCountryStatesForm(Factory.New<RefCountryStates>()))
			{
				var nonWorkingDayTab = (ZTabPage)form.Controls.Find("zNonWorkingDaysTabPage", true).SingleOrDefault();
				AssertEquals(2, nonWorkingDayTab.Controls.Count);
				AssertNotNull(nonWorkingDayTab.Controls.Find("WeekendsGroupBox", false));
				AssertNotNull(nonWorkingDayTab.Controls.Find("HolidaysGridGroupBox", false));
			}
		}

		public void TestDoubleClickHolidayOnStateModuleToHolidayModule()
		{
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = countryStates.PK;
			holiday.GH_ParentTableCode = countryStates.TablePrefix;
			Factory.Save();
			using (var form = new RefCountryStatesForm(countryStates))
			{
				form.Show();
				var nonWorkingDayTab = (ZTabPage)form.Controls.Find("zNonWorkingDaysTabPage", true).SingleOrDefault();
				AssertEquals(2, nonWorkingDayTab.Controls.Count);
				AssertNotNull(nonWorkingDayTab.Controls.Find("WeekendsGroupBox", false));
				AssertNotNull(nonWorkingDayTab.Controls.Find("HolidaysGridGroupBox", false));
				var holidaysGridGroupBox = (ZGroupBox)nonWorkingDayTab.Controls.Find("HolidaysGridGroupBox", true).SingleOrDefault();
				holidaysGridGroupBox.Show();
				var holidaysGrid = (ZGrid)holidaysGridGroupBox.Controls.Find("ZGridNonWorkingDayHolidays", true).SingleOrDefault();
				holidaysGrid.Show();
				holidaysGrid.PerformDoubleClickForTest();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		static CharacterCasing GetCharacterCasing(ZForm form, string controlName)
		{
			CharacterCasing casing = CharacterCasing.Lower;
			var mainTabPage = form.Controls.Find("MainTabPage", true).SingleOrDefault();
			if (mainTabPage != null)
			{
				foreach (Control control in mainTabPage.Controls)
				{
					if (control.Name == controlName)
					{
						casing = ((ZTextBox)control).CharacterCasing;
					}
				}
			}
			return casing;
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefCountryStatesForm)GetFormToBashCore())
			{
				AssertNotNull("RefCountryStatesForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
