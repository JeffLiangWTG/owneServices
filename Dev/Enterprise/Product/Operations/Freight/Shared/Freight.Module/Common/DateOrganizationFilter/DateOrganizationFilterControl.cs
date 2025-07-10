using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Module
{
	public partial class DateOrganizationFilterControl : ZDateRangeControl
	{
		public DateOrganizationFilterControl()
		{
			InitializeComponent();
		}

		public DateOrganizationFilterControl(ZFilterStrip parentStrip)
			: base(parentStrip)
		{
			InitializeComponent();
			InitializeControl(parentStrip);
		}

		void InitializeControl(ZFilterStrip parentStrip)
		{
			SetupOrganizationDropEdit(parentStrip);
			MoveOrganizationFieldsToBottom();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Filter != null && Filter.OrganizationLabel != null)
			{
				SetupOrganizationLabels();
			}
		}

		void SetupOrganizationLabels()
		{
			OrganizationComparisonOperator.CaptionResourceString = Filter.OrganizationLabel;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(PropertySearchDropEdit, true);
		}

		void SetupOrganizationDropEdit(ZFilterStrip parentStrip)
		{
			OrganizationComparisonOperator.CharacterCasing = CharacterCasing.Normal;
			OrganizationComparisonOperator.CodeBox.Font = parentStrip.ComparisonOperatorFont;
			OrganizationComparisonOperator.CodeBox.TextAlign = HorizontalAlignment.Center;
			OrganizationComparisonOperator.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
		}

		void MoveOrganizationFieldsToBottom()
		{
			ControlDpiScalingHelper.SetTop(ref OrganizationComparisonOperator, ParentStrip.FilterControlTop + ControlDpiScalingHelper.ScaleToCurrentDpiY(26), false);
			ControlDpiScalingHelper.SetLeft(ref OrganizationComparisonOperator, ParentStrip.FilterControlsStart, true);

			ControlDpiScalingHelper.SetTop(ref OrganizationFindBox, ParentStrip.FilterControlTop + ControlDpiScalingHelper.ScaleToCurrentDpiY(26), false);
			ControlDpiScalingHelper.SetLeft(ref OrganizationFindBox, OrganizationComparisonOperator.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);

			ControlDpiScalingHelper.SetWidth(ref OrganizationFindBox, PropertySearchDropEdit.Right - OrganizationFindBox.Left, false);
		}

		protected new DateOrganizationFilter Filter
		{
			get => DataSource as DateOrganizationFilter;
		}
	}
}
