using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentTrackingBulkUpdateForm : ZForm
	{
		public DocumentTrackingBulkUpdateForm(DocumentTrackingBulkUpdateBusinessObject bizO) : base(bizO)
		{
			InitializeComponent();
			this.BusinessEntity = bizO;
			MinimumSize = Size;
			UpdateButton.TextChanged += new EventHandler(UpdateButton_TextChanged);
			ZFormPostingButtonsStrategy.SetupPosting(this, UpdateButton, CancelButton);
			bizO.SaveSucceeded += new EventHandler(BizO_SaveSucceeded);
		}

		public new readonly DocumentTrackingBulkUpdateBusinessObject BusinessEntity;

		#region Form Caption

		public override string FormVerb
		{
			get
			{
				return ZString.Empty;
			}
		}

		#endregion

		#region Implementation

		protected EmbeddedModulePopup LastShownAttachPopup;

		protected void AttachButton_Click(object sender, EventArgs e)
		{
			ZFilterGridModule documentTrackingModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.DocumentTracking);
			documentTrackingModule.SetupAndGetGrid();

			SelectFromCollectionHelper helper = new SelectFromCollectionHelper(BusinessEntity.SelectedDocuments, documentTrackingModule);
			helper.BusinessObjectSelected += new SelectFromCollectionHelper.BusinessObjectSelectedHandler(OnBusinessObjectSelected);
			helper.Select(this);
			this.LastShownAttachPopup = helper.LastShownPopup;
		}

		void OnBusinessObjectSelected(SelectFromCollectionHelper sender, BusinessObject[] selectedDocuments)
		{
			foreach (JobRequiredDocument document in selectedDocuments)
			{
				BusinessEntity.SelectedDocuments.AddDocToBulkUpdate(document.PK, document.ParentType);
			}
		}

		protected void DetachButton_Click(object sender, EventArgs e)
		{
			if (SelectedDocumentsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("7aa8ddc8-8641-4683-a547-f5ce8f8b4b3d", "Please select a document in the grid to detach."));
			}

			foreach (RequiredDocToBulkUpdate doc in SelectedDocumentsGrid.SelectedElements)
			{
				BusinessEntity.SelectedDocuments.RemoveAndDelete(doc);
			}
		}

		void BizO_SaveSucceeded(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(Res.GetString("b9673c2e-50b2-4d3b-920c-56768fd94537", "Document statuses updated successfully"), Res.GetString("534f440c-f716-48d5-b8ae-585af544cf2d", "Success"));
		}

		void UpdateButton_TextChanged(object sender, EventArgs e)
		{
			this.UpdateButton.Text = Res.GetString("Forwading|DocumentTrackingBulkUpdateForm|UpdateButton", "Update");
		}

		#endregion
	}
}
