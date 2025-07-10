using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class ManifestForm : ZEditForm
	{
		public ManifestForm(Trip trip)
			: base(trip)
		{
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), new MessagingMenu(trip));
			createNewDeclaration = new ZMenuItem(GetCreateNewDeclarationMenuCaption, new EventHandler(CreateNewDeclaration_Click));
			ZFormMenuStrategy.AddActionsMenuItem(this, createNewDeclaration);

			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)trip).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			TopLevelTabControl.ControlAdded += TopLevelTabControl_ControlAdded;

			WorkflowTabPage.Initialize(trip);
		}

		protected override void PerformValidation()
		{
			using ((BusinessEntity as Trip)?.SuspendValidationOnChildren())
			{
				base.PerformValidation();
			}
		}

		protected MenuItem createNewDeclaration;
		public string GetCreateNewDeclarationMenuCaption => Res.GetString("USManifestForm|CreateANewDeclaration", "Create A New Declaration");

		void CreateNewDeclaration_Click(object sender, EventArgs e)
		{
			var selectedTrip = base.BusinessEntity as Trip;
			if (selectedTrip != null)
			{
				ManifestFormhelper.CreateANewDeclaration(selectedTrip);
			}
		}

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("USManifestForm|FormCaption", "e-Manifest");
				if (!this.IsDesignMode())
				{
					caption += " - " + BusinessEntity.HumanReadableName;
				}
				return caption;
			}
		}

		protected override void InitializeFormCore()
		{
			base.InitializeFormCore();
			InitializeComponent();
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		protected override sealed ZTabControl TopLevelTabControl
		{
			get { return ManifestUserControl.TabControl; }
		}

		void TopLevelTabControl_ControlAdded(object sender, ControlEventArgs e)
		{
			if (!movingTabs)
			{
				movingTabs = true;
				TopLevelTabControl.Controls.Remove(StmNoteTabPage);
				TopLevelTabControl.Controls.Remove(LogsTabPage);

				TopLevelTabControl.Controls.Add(StmNoteTabPage);
				TopLevelTabControl.Controls.Add(LogsTabPage);
				movingTabs = false;
			}
		}

		bool movingTabs;
		protected override bool ExecuteAllFetchHintsBeforeValidateAll => false;
	}
}
