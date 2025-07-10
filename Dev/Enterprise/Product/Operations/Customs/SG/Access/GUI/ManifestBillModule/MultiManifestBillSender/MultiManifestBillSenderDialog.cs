using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public partial class MultiManifestBillSenderDialog : ZChildForm
	{
		public MultiManifestBillSenderDialog(MultiManifestBillSender sender)
			: base(sender)
		{
			BuildTreeView();
		}

		public new MultiManifestBillSender BusinessEntity => (MultiManifestBillSender)base.BusinessEntity;

		public override string FormVerb => string.Empty;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void BuildTreeView()
		{
			foreach (var data in BusinessEntity.MasterDatas)
			{
				var root = new TreeNode(data.DisplayValue);
				foreach (var billData in data.BillDatas)
				{
					root.Nodes.Add(new TreeNode(billData.DisplayValue));
				}
				ShipmentTreeView.Nodes.Add(root);
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.RunPreSaveValidation();

				if (BusinessEntity.HasMessageErrors)
				{
					Globals.Message.ShowError(Res.GetString("{C631C62F-896E-4221-AECC-D4AFFCFF335B}", "Please fix all the errors before proceeding."));
					return;
				}

				using (var progressForm = CreateProgressForm())
				{
					progressForm.Status = "Sending Message To Customs";
					progressForm.ShowCancelButton = true;
					progressForm.ShowModalTo(this);
					try
					{
						progressForm.Cancelled += BusinessEntity.CancelSendingTheRest;
						BusinessEntity.UpdateProgress = progressForm.SetStatusAndPercentComplete;
						var sendResult = BusinessEntity.Send();
						progressForm.SetStatusAndPercentComplete("Sending Message To Customs Completed", 100);
						Globals.Message.ShowInformation(sendResult);
					}
					finally
					{
						progressForm.Cancelled -= BusinessEntity.CancelSendingTheRest;
						BusinessEntity.UpdateProgress = null;
					}
				}

				DialogResult = DialogResult.OK;
			}

			Close();
		}

#if DEBUG
		protected virtual
#endif
		ProgressForm CreateProgressForm()
		{
			return new ProgressForm();
		}

		void ButtonCancel_Click(object sender, EventArgs e)
		{
			Close();
			DialogResult = DialogResult.Cancel;
		}
	}
}
