using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DuplicateAlertControl : ZUserControl
	{
		public DuplicateAlertControl(DuplicationEventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				this.master = e.Master;
				InitializeComponent();
				this.targets = e.TargetObjects;
				this.results = e.Results;
				this.resultModels = e.ResultsModels;

				HookEvents();
			}

			this.MainPanel.Controls.Add(this.ResultsListPanel, 0, 1);
		}

		readonly object master;
		readonly object targets;
		readonly IEnumerable<ScoringResult> results;
		readonly IEnumerable<PatternMatchingResultModel> resultModels;
		public event EventHandler<DuplicationEventArgs> DisplayDetailsAndFixes;
		public bool IsManualHide { get; set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Size = master is GlbPerson ?
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 214) :
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 206);

			if (!this.IsDesignMode())
			{
				SetDataSource();
			}
		}

		void SetDataSource()
		{
			var deduplicationResultListUserControl = ObjectFactory.Get<IMasterDataProviderGUI>().SetDeduplicationPopupDataSource(master, results, (code) =>
			{
				DisplayDetailsAndFixes?.Invoke(this, new DuplicationEventArgs(master, targets, results, resultModels, code));
				Close();
			});

			if (deduplicationResultListUserControl != null)
			{
				ResultsListPanel.Controls.Add(deduplicationResultListUserControl);
			}
			else
			{
				Close();
			}
		}

		void HookEvents()
		{
			DisplayDetailsAndFixesButton.Click += DisplayDetailsAndFixesButton_Click;
			CloseButton.Click += (s, e) => { IsManualHide = true; Hide(); };
			CloseButton.MouseHover += (sender, e) => CloseButton.ForeColor = Color.White;
			CloseButton.MouseEnter += (sender, e) => CloseButton.ForeColor = Color.White;
			CloseButton.MouseLeave += (sender, e) => CloseButton.ForeColor = Color.Black;
		}

		#region Methods

		public void Close()
		{
			DisplayDetailsAndFixesButton.Click -= DisplayDetailsAndFixesButton_Click;

			Parent?.Controls.Remove(this);

			if (IsHandleCreated)
			{
				// Don't Dispose synchronously since we could be in the middle of Control.WmMouseDown
				BeginInvoke(new MethodInvoker(Dispose));
			}
			else
			{
				// Will almost certainly only get here during unit tests
				Dispose();
			}
		}

		#endregion

		#region Event Handlers

		void DisplayDetailsAndFixesButton_Click(object sender, EventArgs e)
		{
			DisplayDetailsAndFixes?.Invoke(this, new DuplicationEventArgs(master, targets, results, resultModels));
			Close();
		}

		#endregion
	}
}
