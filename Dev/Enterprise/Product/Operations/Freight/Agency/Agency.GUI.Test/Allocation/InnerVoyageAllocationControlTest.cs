using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI
{
	internal class InnerVoyageAllocationControlTest : BaseAgencyTest
	{
		[GuiTest]
		public void TestAllocationMethodChanged()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.Principals.Add(principal1);
			schedule.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			Factory.Save();
			using (TestFormWithExistingTab form = new TestFormWithExistingTab(schedule))
			{
				form.Show();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				Application.DoEvents();
				form.TabControl.SelectedIndex = 1;
				Label label = form.TabControl.TabPages[1].Controls.Find("allocationMethodNotSetLabel", true)[0] as Label;
				AssertNotNull("Should find Label 'allocationMethodNotSetLabel'", label);
				AssertEquals("allocationMethodNotSetLabel: Should be Visible", true, label.Visible);
				GroupBox groupBox = form.TabControl.TabPages[1].Controls.Find("countryGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'countryGroupBox'", groupBox);
				AssertEquals("countryGroupBox: Should NOT be Visible", false, groupBox.Visible);
				groupBox = form.TabControl.TabPages[1].Controls.Find("originGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'originGroupBox'", groupBox);
				AssertEquals("originGroupBox: Should NOT be Visible", false, groupBox.Visible);
				Panel panel = form.TabControl.TabPages[1].Controls.Find("refreshPanel", true)[0] as Panel;
				AssertNotNull("Should find Panel 'refreshPanel'", panel);
				AssertEquals("refreshPanel: Should be Visible", true, panel.Visible);
				groupBox = form.TabControl.TabPages[1].Controls.Find("usageBreakDownBySailingGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'usageBreakDownBySailingGroupBox'", groupBox);
				AssertEquals("usageBreakDownBySailingGroupBox: Should be Visible", true, groupBox.Visible);
				schedule.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Country;
				schedule.Principals.Add(principal2);
				label = form.TabControl.TabPages[1].Controls.Find("allocationMethodNotSetLabel", true)[0] as Label;
				AssertNotNull("Should find Label 'allocationMethodNotSetLabel'", label);
				AssertEquals("allocationMethodNotSetLabel: Should NOT be Visible", false, label.Visible);
				groupBox = form.TabControl.TabPages[1].Controls.Find("countryGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'countryGroupBox'", groupBox);
				AssertEquals("countryGroupBox: Should be Visible", true, groupBox.Visible);
				form.TabControl.SelectedIndex = 2;
				label = form.TabControl.TabPages[2].Controls.Find("allocationMethodNotSetLabel", true)[0] as Label;
				AssertNotNull("Should find Label 'allocationMethodNotSetLabel'", label);
				AssertEquals("allocationMethodNotSetLabel: Should NOT be Visible", false, label.Visible);
				groupBox = form.TabControl.TabPages[2].Controls.Find("countryGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'countryGroupBox'", groupBox);
				AssertEquals("countryGroupBox: Should be Visible", true, groupBox.Visible);
				schedule.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
				form.TabControl.SelectedIndex = 1;
				groupBox = form.TabControl.TabPages[1].Controls.Find("countryGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'countryGroupBox'", groupBox);
				AssertEquals("countryGroupBox: Should NOT be Visible", false, groupBox.Visible);
				groupBox = form.TabControl.TabPages[1].Controls.Find("sailingsGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'sailingsGroupBox'", groupBox);
				AssertEquals("sailingsGroupBox: Should be Visible", true, groupBox.Visible);
				groupBox = form.TabControl.TabPages[1].Controls.Find("usageBreakDownBySailingGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'usageBreakDownBySailingGroupBox'", groupBox);
				AssertEquals("usageBreakDownBySailingGroupBox: Should NOT be Visible", false, groupBox.Visible);
				form.TabControl.SelectedIndex = 2;
				groupBox = form.TabControl.TabPages[2].Controls.Find("countryGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'countryGroupBox'", groupBox);
				AssertEquals("countryGroupBox: Should NOT be Visible", false, groupBox.Visible);
				groupBox = form.TabControl.TabPages[2].Controls.Find("sailingsGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'sailingsGroupBox'", groupBox);
				AssertEquals("sailingsGroupBox: Should be Visible", true, groupBox.Visible);
				groupBox = form.TabControl.TabPages[1].Controls.Find("usageBreakDownBySailingGroupBox", true)[0] as GroupBox;
				AssertNotNull("Should find GroupBox 'usageBreakDownBySailingGroupBox'", groupBox);
				AssertEquals("usageBreakDownBySailingGroupBox: Should NOT be Visible", false, groupBox.Visible);
			}
		}

		#region Implementation
		class TestForm : ZForm
		{
			public AgencyCountry Schedule;
			public BindingTabControl TabControl;
			public TestForm(AgencyCountry country) : base(country)
			{
				this.Schedule = country;
			}

			protected virtual bool ExistingTab
			{
				get
				{
					return false;
				}
			}

			protected override void InitialiseForm()
			{
				Schedule = (AgencyCountry)BusinessEntity;
				base.InitialiseForm();
				TabControl = new BindingTabControl();
				if (ExistingTab)
				{
					ZTabPage tab = new ZTabPage();
					TabControl.TabPages.Add(tab);
				}

				TabControl.DataCollection = Schedule.WrappedPrincipals;
				TabControl.Dock = DockStyle.Fill;
				this.Controls.Add(TabControl);
				this.Height = 500;
				this.Width = 500;
			}
		}

		class TestFormWithExistingTab : TestForm
		{
			public TestFormWithExistingTab(AgencyCountry country) : base(country)
			{
			}

			protected override bool ExistingTab
			{
				get
				{
					return true;
				}
			}
		}
		#endregion
	}
}
