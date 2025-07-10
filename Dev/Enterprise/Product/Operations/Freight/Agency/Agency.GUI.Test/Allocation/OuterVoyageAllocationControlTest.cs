using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal class OuterVoyageAllocationControlTest : BaseAgencyTest
	{
		public void TestAllocationsByPrincipalChanged()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.Principals.Add(principal1);
			schedule.Principals.Add(principal2);
			schedule.VoyageCountry.J0_AllocationsByPrincipal = true;
			Factory.Save();
			using (TestForm form = new TestForm(schedule))
			{
				form.Show();
				AssertEquals("Should have 2 principals", 2, schedule.Principals.Count);
				AssertEquals("Should have 3 pages", 3, form.TabControl.TabPages.Count);
				AssertNotEquals("Should NOT have page 'Details'", "Details", form.TabControl.TabPages[1].Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				schedule.VoyageCountry.J0_AllocationsByPrincipal = false;
				AssertEquals("Should have asked", "Question Are you sure you want to remove all Principals with their allocations?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should have 2 principals", 2, schedule.Principals.Count);
				AssertEquals("Should have 3 pages", 3, form.TabControl.TabPages.Count);
				AssertNotEquals("Should NOT have page 'Details'", "Details", form.TabControl.TabPages[1].Text);
				AssertEquals("J0_AllocationsByPrincipal Should be TRUE", true, schedule.VoyageCountry.J0_AllocationsByPrincipal);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				schedule.VoyageCountry.J0_AllocationsByPrincipal = false;
				AssertEquals("Should have asked", "Question Are you sure you want to remove all Principals with their allocations?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should NOT have principals", 0, schedule.Principals.Count);
				AssertEquals("Should have 2 pages", 2, form.TabControl.TabPages.Count);
				AssertEquals("Should have page 'Details'", "Details", form.TabControl.TabPages[1].Text);
				AssertEquals("J0_AllocationsByPrincipal Should be FALSE", false, schedule.VoyageCountry.J0_AllocationsByPrincipal);
				schedule.VoyageCountry.J0_AllocationsByPrincipal = true;
				AssertEquals("Should NOT have principals", 0, schedule.Principals.Count);
				AssertEquals("Should have 1 pages", 1, form.TabControl.TabPages.Count);
			}
		}

		#region Implementation
		class TestForm : ZForm
		{
			public AgencyCountry Schedule;
			public BindingTabControl TabControl;
			public TestForm(AgencyCountry country) : base(country)
			{
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				Schedule = (AgencyCountry)BusinessEntity;
				OuterVoyageAllocationControl outerControl = new OuterVoyageAllocationControl();
				OuterVoyageAllocationControl.TestHelper helper = new OuterVoyageAllocationControl.TestHelper(outerControl);
				this.TabControl = helper.TabControl;
				this.Controls.Add(outerControl);
				this.Height = 500;
				this.Width = 500;
			}
		}
		#endregion
	}
}
