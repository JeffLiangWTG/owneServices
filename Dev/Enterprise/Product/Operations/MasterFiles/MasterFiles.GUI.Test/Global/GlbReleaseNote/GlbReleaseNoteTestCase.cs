using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Global.Testing
{
	internal abstract class GlbReleaseNoteTestCase : ZFormBasherTest
	{
		public void TestNotesSortedByDateDescending()
		{
			PrepareTestData();

			using (GlbReleaseNoteForm form = GetFormToBash())
			{
				form.Show();

				Manager.DateToFilterAfter = ZDateTime.Today.AddMonths(-3);
				Manager.DateToFilterBefore = ZDateTime.Empty;

				AssertEquals("ReleaseNotesGrid.List[0].PK", Note1.PK, ((GlbReleaseNote)form.ReleaseNotesGrid.List[0]).PK);
				AssertEquals("ReleaseNotesGrid.List[1].PK", Note2.PK, ((GlbReleaseNote)form.ReleaseNotesGrid.List[1]).PK);
				AssertEquals("ReleaseNotesGrid.List[2].PK", Note3.PK, ((GlbReleaseNote)form.ReleaseNotesGrid.List[2]).PK);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new GlbReleaseNoteForm(Manager);
		}

		protected new GlbReleaseNoteForm GetFormToBash()
		{
			return (GlbReleaseNoteForm)base.GetFormToBash();
		}

		protected void PrepareTestData()
		{
			Manager.ReleaseNotes.DeleteAll();

			Note1 = Manager.ReleaseNotes.AddNew();
			Note2 = Manager.ReleaseNotes.AddNew();
			Note3 = Manager.ReleaseNotes.AddNew();

			Note1.GF_Summary = "Note1";
			Note2.GF_Summary = "Note2";
			Note3.GF_Summary = "Note3";

			Note1.GF_Section = "C1U";
			Note2.GF_Section = "C1U";
			Note3.GF_Section = "C1U";

			Note1.GF_URL = "Note1";
			Note2.GF_URL = "Note2";
			Note3.GF_URL = "Note3";

			Note1.GF_RN_NKCountryForReleaseNote = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Note2.GF_RN_NKCountryForReleaseNote = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Note3.GF_RN_NKCountryForReleaseNote = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Note1.GF_ReleaseNoteDate = ZDateTime.Now.AddDays(3);
			Note2.GF_ReleaseNoteDate = ZDateTime.Now.AddDays(2);
			Note3.GF_ReleaseNoteDate = ZDateTime.Now.AddDays(1);

			Factory.Save();
			((IActiveBusinessObjectCollection)Manager.ReleaseNotes).Refresh();
		}

		protected GlbReleaseNote Note1;
		protected GlbReleaseNote Note2;
		protected GlbReleaseNote Note3;

		#region Manager

		protected GlbReleaseNoteManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = GetNewManager();
				}

				return fManager;
			}
		}

		protected virtual GlbReleaseNoteManager GetNewManager()
		{
			for (int i = 0; i < 3; i++)
			{
				Factory.NewWithValidTestData<GlbReleaseNote>();
			}
			Factory.Save();

			var manager = new GlbReleaseNoteManager(Factory);
			manager.ReleaseNotes.AdditionalFilter = new ZQuery(GlbReleaseNoteSchema.GF_ReleaseNoteDate, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddDays(-14));
			return manager;
		}
		GlbReleaseNoteManager fManager;

		#endregion

		#endregion
	}
}
