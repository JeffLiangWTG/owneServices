using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class TTBUserControl : ZUserControl
	{
		public TTBUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(TTBLineGrid).AddPGALineEditMenu();
		}

		public void RemoveUnavailableComlumnsForProduct()
		{
			var columnsShouldBeRemoved = new[]
			{
				"US_QuantityInPCS", // This is a column name to be removed
				TTBLine.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate"
			};

			TTBLineGrid.RemoveFromAvailableColumns(columnsShouldBeRemoved);
			CigarsGrid.RemoveFromAvailableColumns(new[] { "US_Quantity", "US_UnitPrice" }); // This is a column name to be removed
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			TTBLineGridListManager_PositionChanged(null, null);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookProgramCodeChange();
			}
			base.Dispose(disposing);
		}

		void TTBLineGrid_AfterBind(object sender, EventArgs e)
		{
			TTBLineGrid.ListManager.PositionChanged += TTBLineGridListManager_PositionChanged;
			TTBLineGridListManager_PositionChanged(null, null);
		}

		void TTBLineGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var currentProgramType = CurrentProgramCode;
			var isQuantityRequired = TTBTOBProcessingCodeList.IsQuantityRequired(CurrentProcessingCode);
			UnHookProgramCodeChange();

			currentLine = null;
			var listManager = TTBLineGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				currentLine = (TTBLine)listManager.GetCurrent();
			}
			if (currentLine == null || currentLine.US_ProgramCode != currentProgramType || isQuantityRequired != currentLine.IsQuantityRequired)
			{
				ChangeTTBVisibility();
			}

			HookProgramCodeChange();
		}

		void HookProgramCodeChange()
		{
			if (currentLine != null)
			{
				currentLine.US_ProgramCodeInfo.ValueChanged += TTBRelated_ValueChanged;
				currentLine.US_ProcessingCodeInfo.ValueChanged += TTBRelated_ValueChanged;
			}
		}

		void UnHookProgramCodeChange()
		{
			if (currentLine != null)
			{
				currentLine.US_ProgramCodeInfo.ValueChanged -= TTBRelated_ValueChanged;
				currentLine.US_ProcessingCodeInfo.ValueChanged -= TTBRelated_ValueChanged;
			}
		}

		void TTBRelated_ValueChanged(object sender, EventArgs e)
		{
			ChangeTTBVisibility();
		}

		string CurrentProcessingCode
		{
			get
			{
				var result = "";
				var lookups = currentLine != null && !currentLine.IsDeleted ? currentLine.AddInfoLookups : null;
				if (lookups != null && lookups.ProcessingCodes.ContainsCode(currentLine.US_ProcessingCode))
				{
					result = currentLine.US_ProcessingCode;
				}
				return result;
			}
		}

		string CurrentProgramCode
		{
			get
			{
				var result = "";
				var lookups = currentLine != null && !currentLine.IsDeleted ? currentLine.AddInfoLookups : null;
				if (lookups != null && lookups.ProgramCodes.ContainsCode(currentLine.US_ProgramCode))
				{
					result = currentLine.US_ProgramCode;
				}
				return result;
			}
		}
		TTBLine currentLine;

		void ChangeTTBVisibility()
		{
			if (!this.IsDesignMode())
			{
				var isTobacco = CurrentProgramCode == TTBProgramCodeList.Codes.Tobacco;
				this.COLAAndCertificatesGroupBox.Visible = !isTobacco;
				this.COLAAndCertificatesGroupBox.Enabled = !isTobacco;
				this.CigarsGroupBox.Visible = isTobacco;
				this.CigarsGroupBox.Enabled = isTobacco;
				if (currentLine != null)
				{
					if (isTobacco)
					{
						currentLine.Cigars.RefreshBinding();
					}
					else
					{
						currentLine.COLAAndCertificates.RefreshBinding();
					}
				}
			}
		}
	}
}
