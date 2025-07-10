using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomsNumberViewStmNumsCompanyUserControl : CustomsNumberViewStmNumsUserControl
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public CustomsNumberViewStmNumsCompanyUserControl()
		{
			InitializeComponent();
		}

		public CustomsNumberViewStmNumsCompanyUserControl(CustomsNumberViewStmNumsWrapperCollection collection)
			: base(collection)
		{
			InitializeComponent();
		}

		void AddIsBranchLevelColumn()
		{
			var isBranchLevelColumn = new ZCheckBoxColumnStyleInfo();
			isBranchLevelColumn.ColumnName = "IsBranchLevel";
			isBranchLevelColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(51);
			this.NumberRangesGrid.ColumnStyles.Insert(0, isBranchLevelColumn);
		}

		ZButton CreateAddBranchButton()
		{
			var addBranchButton = new ZButton();
			addBranchButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5dac982d-8976-42c5-8e52-c948e71cfb05", "Add &Branch");
			addBranchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			addBranchButton.Name = "NumberRangesAddBranchButton";
			addBranchButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			addBranchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			addBranchButton.TabIndex = 0;
			addBranchButton.ToolTipCaption = null;
			addBranchButton.Click += NumberRangesAddBranchButton_Click;
			NumberRangesAdditionalButtonsPanel.Controls.Add(addBranchButton);
			return addBranchButton;
		}

		void NumberRangesAddBranchButton_Click(object sender, EventArgs e)
		{
			if (CheckThatStmNumsParentHasNoChanges())
			{
				var newWrapper = LoadOrCreateWrapperInAStandAloneFactory();
				((CustomsNumberViewStmNumsCompanyWrapper)newWrapper).IsBranchLevel = true;
				newWrapper.SN_Owner = ((GlbCompany)(Provider.Parent)).Branches.Where(x => x.GB_IsActive).OrderBy(x => x.GB_Code).Select(x => x.PK).FirstOrDefault();
				var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(GuiProvider.GetEditorForm(newWrapper));
				if (editorDialogResult == DialogResult.OK && !newWrapper.HasErrors)
				{
					SaveStmNumsStandAloneFactory(newWrapper.Factory);
				}
			}
		}

		protected GlbCompany Company => (GlbCompany)StmNumsParent;
	}
}
