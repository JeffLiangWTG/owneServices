using System;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class VoyageAccountingForm : ZForm
	{
		public VoyageAccountingForm(VoyageAccount bo)
			: base(bo)
		{
			InitializeComponent();

			PlugIns.AddJobInvoicing(bo.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtons);
			HookEvents();
		}

		#region Hook / UnHook Events

		void HookEvents()
		{
			Account.NA_JVInfo.ValueChanged += new EventHandler(NA_JVInfo_ValueChanged);
		}

		void UnHookEvents()
		{
			Account.NA_JVInfo.ValueChanged -= new EventHandler(NA_JVInfo_ValueChanged);
		}

		void NA_JVInfo_ValueChanged(object sender, EventArgs e)
		{
			PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).OnGUIShown();
		}

		#endregion

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Account != null)
				{
					UnHookEvents();
				}
			}
			base.Dispose(disposing);
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return mainTabControl; }
		}

		public override bool IsResizableByTabPageAllowed => true;

		void selectVoyageButton_Click(object sender, EventArgs e)
		{
			if (Account.NA_JVInfo.ReadOnly)
			{
				Globals.Message.Show(Res.GetString("b5f21d38-134b-4856-90f6-40f226d43404", "Unable to choose another Sailing Schedule because Voyage Accounting job has been invoiced."));
			}
			else
			{
				VoyageIFindBox.SelectSeaVoyage(this, Account.NA_JVInfo);
			}
		}

		VoyageAccount Account
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VoyageAccount)base.BusinessEntity; }
		}

		#endregion
	}
}


