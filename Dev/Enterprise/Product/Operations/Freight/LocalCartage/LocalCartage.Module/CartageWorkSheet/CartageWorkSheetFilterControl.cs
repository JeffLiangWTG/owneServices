using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.Module
{
	public partial class CartageWorkSheetFilterControl : ZFilterStripControl
	{
		public CartageWorkSheetFilterControl()
		{
			InitializeComponent();
		}

		public CartageWorkSheetFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			HookEvents();
		}

		ModuleCartageRunSheetCollection RunSheetCollection
		{
			get { return (ModuleCartageRunSheetCollection)GridCollection; }
		}

		void HookEvents()
		{
			RunSheetCollection.CountChanged += new CollectionCountChangedEventHandler(RunSheetCollection_CountChanged);
			HookRunSheets();
		}

		void UnHookEvents()
		{
			RunSheetCollection.CountChanged -= new CollectionCountChangedEventHandler(RunSheetCollection_CountChanged);
			UnHookAllRunSheets();
		}

		void RunSheetCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			HookRunSheets();
		}

		void HookRunSheets()
		{
			foreach (CommonWorkSheet runSheet in RunSheets_Listening.ToArray())
			{
				if (!RunSheetCollection.Contains(runSheet))
				{
					runSheet.OnGetCartageLegsToPrint -= new System.EventHandler<DocumentCartageLegEventArgs>(runSheet_OnGetCartageLegsToPrint);
					RunSheets_Listening.Remove(runSheet);
				}
			}

			foreach (CommonWorkSheet runSheet in RunSheetCollection)
			{
				if (!RunSheets_Listening.Contains(runSheet))
				{
					runSheet.OnGetCartageLegsToPrint += new System.EventHandler<DocumentCartageLegEventArgs>(runSheet_OnGetCartageLegsToPrint);
					RunSheets_Listening.Add(runSheet);
				}
			}
		}

		void UnHookAllRunSheets()
		{
			foreach (CommonWorkSheet runSheet in RunSheets_Listening.ToArray())
			{
				runSheet.OnGetCartageLegsToPrint -= new System.EventHandler<DocumentCartageLegEventArgs>(runSheet_OnGetCartageLegsToPrint);
				RunSheets_Listening.Remove(runSheet);
			}
		}

		void runSheet_OnGetCartageLegsToPrint(object sender, DocumentCartageLegEventArgs e)
		{
#if DEBUG
			runSheet_OnGetCartageLegsToPrintHitCountTest++;
#endif

			if (e.DocumentCartageLegOptions.CartageLegs.Count == 0)
			{
				e.ContinueToPrint = false;
				Globals.Message.ShowInformation(Res.GetString("692dd943-8e9e-4c72-9ad9-f888c76c285a", "There are no Port Transport Legs to print."), Res.GetString("cac4600d-ee15-48f4-baf1-58d565718904", "No Port Transport Legs"));
			}
			else
			{
				using (var form = new DocumentCartageLegsForm(e.DocumentCartageLegOptions))
				{
					form.ShowDialog();
					e.ContinueToPrint = form.DialogResult == DialogResult.Yes;
				}
			}
		}

		List<CommonWorkSheet> RunSheets_Listening
		{
			get { return runSheets_Listening ?? (runSheets_Listening = new List<CommonWorkSheet>()); }
		}
		List<CommonWorkSheet> runSheets_Listening;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				UnHookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public int runSheet_OnGetCartageLegsToPrintHitCountTest;
	}
}