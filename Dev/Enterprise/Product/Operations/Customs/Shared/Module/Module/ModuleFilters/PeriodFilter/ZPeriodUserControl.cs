using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	[DefaultDataSourceBindingMember(null)]
	public partial class ZPeriodUserControl : ZUserControl
	{
		#region Construction

		public ZPeriodUserControl()
			: this(null)
		{
		}

		public ZPeriodUserControl(ZFilterStrip parentStrip)
		{
			this.parentStrip = parentStrip;
			InitializeComponent();
			InitializeControls();
		}

		void InitializeControls()
		{
			if (parentStrip != null)
			{
				OperatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
				OperatorDropEdit.CodeBox.Font = parentStrip.ComparisonOperatorFont;
				OperatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			}
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Filter != null)
			{
				Filter.ComparisonOperatorInfo.ValueChanged -= new EventHandler(ComparisonOperatorInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Filter != null)
			{
				Filter.ComparisonOperatorInfo.ValueChanged += new EventHandler(ComparisonOperatorInfo_ValueChanged);
				UpdateControls();
			}
		}

		#endregion

		#region Showing / Hiding the period controls

		void ComparisonOperatorInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControls();
		}

		void UpdateControls()
		{
			ShowPeriodControls = (Filter != null &&
				(Filter.SqlComparisonOperator != SpecialComparisonOperator.IsBlank && Filter.SqlComparisonOperator != SpecialComparisonOperator.IsNotBlank));
		}

		protected bool ShowPeriodControls
		{
			get { return fShowPeriodControls; }
			set
			{
				fShowPeriodControls = value;

				SuspendLayout();
				try
				{
					PeriodMonthEdit.Visible = value;
					PeriodYearEdit.Visible = value;
					PeriodLabel.Visible = value;
				}
				finally
				{
					ResumeLayout(true);
				}
			}
		}

		bool fShowPeriodControls;
		protected ZFilterStrip parentStrip;

		protected PeriodFilter Filter
		{
			get { return (PeriodFilter)CurrentDataItem; }
		}

		#endregion
	}
}
