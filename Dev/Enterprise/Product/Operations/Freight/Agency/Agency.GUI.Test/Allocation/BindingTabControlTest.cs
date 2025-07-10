using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	internal class BindingTabControlTest : BaseAgencyTest
	{
		public void TestPrincipalAccessSecurityCheck()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			Factory.Save();
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			((IOrgsAndWarehousesAccessProvider)GlbStaff.CurrentUser).AddSecurityToAccessOrgOrWarehouse(principal1.OH_Code);
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			using (TestForm form = new TestForm(schedule))
			{
				form.Show();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				schedule.Principals.Add(principal1);
				Control control1 = GetControlFromTabPage(form, 0);
				AssertEquals("Have permitions for principal1, should use the allocation control", typeof(InnerVoyageAllocationControl), control1.GetType());
				schedule.Principals.Add(principal2);
				Control control2 = GetControlFromTabPage(form, 1);
				AssertEquals("Dont have permitions for principal2, should use a label", typeof(Label), control2.GetType());
			}
		}

		public void TestTabPagesStayInSync()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			Factory.Save();
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.Principals.Add(principal1);
			using (TestForm form = new TestForm(schedule))
			{
				form.Show();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				schedule.Principals.Add(principal2);
				AssertEquals("Should have 2 tabs", 2, form.TabControl.TabPages.Count);
				AssertSame("The first tab should be for Principal1", principal1, ((AgencyPrincipal)((BindingTab)form.TabControl.TabPages[0]).BizObj).Principal);
				AssertSame("The second tab should be for Principal2", principal2, ((AgencyPrincipal)((BindingTab)form.TabControl.TabPages[1]).BizObj).Principal);
				schedule.Principals.Remove(principal1);
				AssertEquals("Should have 1 tab", 1, form.TabControl.TabPages.Count);
				AssertSame("The first tab should be for Principal2", principal2, ((AgencyPrincipal)((BindingTab)form.TabControl.TabPages[0]).BizObj).Principal);
			}
		}

		public void TestRemovingATabPageDontChangeTheCurrentTabPage()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			OrgHeader principal3 = NewPrincipal();
			Factory.Save();
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.Principals.Add(principal1);
			schedule.Principals.Add(principal2);
			schedule.Principals.Add(principal3);
			using (TestFormWithExistingTab form = new TestFormWithExistingTab(schedule))
			{
				form.Show();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				Application.DoEvents();
				form.TabControl.SelectedIndex = 0;
				AssertEquals("Setting the selected index should have worked", 0, form.TabControl.SelectedIndex);
				schedule.Principals.Remove(principal2);
				AssertEquals("The first tab should still be selected.", 0, form.TabControl.SelectedIndex);
			}
		}

		#region Implementation
		Control GetControlFromTabPage(TestForm form, int index)
		{
			BindingTab tab = (BindingTab)form.TabControl.TabPages[index];
			AssertEquals("expecting tab to only have one control", 1, tab.Controls.Count);
			return tab.Controls[0];
		}

		class TestForm : ZForm
		{
			public BindingTabControl TabControl;
			public TestForm(AgencyCountry country) : base(country)
			{
			}

			public AgencyCountry Schedule
			{
				get
				{
					return (AgencyCountry)DataSource;
				}
			}

			protected virtual bool ExistingTab
			{
				get
				{
					return false;
				}
			}

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				base.SetDataBinding(dataSource, dataMember);
				if (dataSource != null)
				{
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
