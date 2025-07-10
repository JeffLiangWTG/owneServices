using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI.Protest
{
	public partial class ProtestForm : ZTemplateForm
	{
		public ProtestForm()
		{
			InitializeComponent();
		}

		public ProtestForm(Business.Protest.Protest protest)
			: base(protest)
		{
			InitializeComponent();
			this.protest = protest;
			AddPlugIns();
		}

		readonly Business.Protest.Protest protest;

		void AddPlugIns()
		{
			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			WorkflowTabPage.Initialize(protest);
		}

		public override string FormCaption
		{
			get { return "Protest" + (protest != null && protest.Declaration.IsInDatabase ? " - " + protest.Declaration.JE_DeclarationReference : ""); }
		}

		protected override bool SupportsEDocs
		{
			get { return true; }
		}

		public override bool IsResizableByTabPageAllowed => true;

		#region Application Questions

		void ApplicationQuestion1ExplainButton_Click(object sender, System.EventArgs e)
		{
			Globals.Message.ShowInformation("A code indicating if a prior request of Trade Compliance Process Owner for a further review of the same claim with respect to the same or substantially similar merchandise was made.");
		}

		void ApplicationQuestion2ExplainButton_Click(object sender, System.EventArgs e)
		{
			Globals.Message.ShowInformation("A code indicating if a final adverse decision from the U.S. Court of International Trade on the same claim with respect to the same category of merchandise or if action involving such a claim pending before the U.S. Court of International Trade is pending.");
		}

		void ApplicationQuestion3ExplainButton_Click(object sender, System.EventArgs e)
		{
			Globals.Message.ShowInformation("A code indicating an adverse administrative decision from the Commissioner of CBP or designee.  Or an administrative decision on the same claim with respect to the same category of merchandise is pending.");
		}

		#endregion

		void AllocateImportEntryNumberButton_Click(object sender, System.EventArgs e)
		{
			ZForm mainForm = FindForm() as ZForm;
			if (mainForm != null)
			{
				var args = new AllocateEventHandlerArgs();
				args.FireSaveButton = () => mainForm.FireSaveButton();
				args.TopBizObjForFormHasChanges = mainForm.BusinessEntityForHasChanges;
				args.PerformActionBeforeShowingForm = form => form.SetCaptions(Enterprise.Customs.Common.CusEntryNumberTypes.UnitedStates.Protest);

				new AllocateNumberButtonClickEventHandler().Allocate(protest, args);
			}
		}
	}
}
