using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZCompetitorIntelligenceForm : BaseOrganisationsForm
	{
		ZTabPage CompetitorTabPage;
		CompetitorUserControl CompetitorControl;

		public ZCompetitorIntelligenceForm()
		{
		}

		public ZCompetitorIntelligenceForm(OrgHeader organisation) : base(organisation)
		{
			SetupForm();
		}

		public override string FormCaption
		{
			get { return Res.GetString("ZCompetitorIntelligenceForm|FormCaption", "Competitor Intelligence") + base.FormCaption; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			Organisation.OH_IsCompetitor = true;
			return base.ShowPreSaveDialogs();
		}

		#region Implementation

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}

		protected void SetupForm()
		{
			SetOrderOfTabPages();
		}

		protected void SetOrderOfTabPages()
		{
			OrganisationsTabControl.TabPages.Remove(ContactsTabPage);
			OrganisationsTabControl.TabPages.Insert(ContactsTabPage, 3);

			OrganisationsTabControl.TabPages.Remove(AddressesTabPage);
			OrganisationsTabControl.TabPages.Insert(AddressesTabPage, 2);
		}

		#endregion
	}
}
