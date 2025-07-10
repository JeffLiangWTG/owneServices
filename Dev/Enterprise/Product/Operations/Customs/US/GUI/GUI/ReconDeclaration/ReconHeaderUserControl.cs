using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ReconHeaderUserControl : ZUserControl
	{
		public ReconHeaderUserControl()
		{
			InitializeComponent();
		}

		void AllocateButton_Click(object sender, EventArgs e)
		{
			ZForm mainForm = FindForm() as ZForm;
			if (mainForm != null)
			{
				var reconDeclaration = mainForm.BusinessEntityForHasChanges as ReconDeclaration;

				if (reconDeclaration != null)
				{
					var args = new AllocateEventHandlerArgs();
					args.FireSaveButton = () => mainForm.FireSaveButton();
					args.TopBizObjForFormHasChanges = reconDeclaration;
					args.PerformActionBeforeShowingForm = null;

					new AllocateNumberButtonClickEventHandler().Allocate(reconDeclaration, args);
				}
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (CurrentDataItem != null)
			{
				CurrentDataItem.US_IsAggregateInfo.ValueChanged -= new EventHandler(US_IsAggregateInfo_ValueChanged);
				CurrentDataItem.US_IssueCodeInfo.ValueChanged -= new EventHandler(US_IssueCodeInfo_ValueChanged);
				ChangeControlsVisibilityForAggregateRecon();
				ChangeControlsVisibilityForIssueCode();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.US_IsAggregateInfo.ValueChanged += new EventHandler(US_IsAggregateInfo_ValueChanged);
				CurrentDataItem.US_IssueCodeInfo.ValueChanged += new EventHandler(US_IssueCodeInfo_ValueChanged);
				ChangeControlsVisibilityForAggregateRecon();
				ChangeControlsVisibilityForIssueCode();
			}
		}

		public new ReconDeclaration CurrentDataItem
		{
			get { return (ReconDeclaration)base.CurrentDataItem; }
		}

		void US_IsAggregateInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlsVisibilityForAggregateRecon();
		}

		void ChangeControlsVisibilityForAggregateRecon()
		{
			if (CurrentDataItem != null)
			{
				NoChangeAggregateCheckBox.Visible = CurrentDataItem.US_IsAggregate;
				AggregateFeesGroupBox.Visible = CurrentDataItem.US_IsAggregate && !CurrentDataItem.IsACE;
				WaiveCheckBox.Visible = CurrentDataItem.US_IsAggregate;
			}
		}

		public void ChangeVisibilityForACE(bool isACE)
		{
			JobDocAddressTabControl.Visible = isACE;
			NotifyPartyGuidFindBox.Visible = isACE;
			teamNoDropEdit.Visible = !isACE;
			AggregateFeesGroupBox.Visible = !isACE && CurrentDataItem != null && CurrentDataItem.US_IsAggregate;
			if (isACE)
			{
				MainDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 263, true);
			}
			else
			{
				MainDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 208, true);
			}
		}

		void US_IssueCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlsVisibilityForIssueCode();
		}

		void ChangeControlsVisibilityForIssueCode()
		{
			QualifyingGoodsFTADecLabel.Visible = (CurrentDataItem.US_IssueCode == ReconIssueCodeList.Codes.FTA) && CurrentDataItem.IsACE;
		}
	}
}
