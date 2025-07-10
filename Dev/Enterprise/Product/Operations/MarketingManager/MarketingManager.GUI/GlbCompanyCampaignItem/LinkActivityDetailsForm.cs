using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class LinkActivityDetailsForm : ZChildForm
	{
		internal GlbCompanyCampaignClickModule FilterItemModule;
		internal GlbCompanyCampaignClickFilterControl FilterStripControl;

		public LinkActivityDetailsForm(CampaignItemClickStatModel statModel)
			: base(statModel)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			FilterStripControl.FirePerformSearch();
		}

		void AddFilterGrid(CampaignItemClickStatModel statModel)
		{
			filterGridAdded = true;

			FilterItemModule = (GlbCompanyCampaignClickModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbCompanyCampaignClick);
			((IGlbCompanyCampaignClickModule)FilterItemModule).CampaignItem = statModel.BusinessEntity;

			FilterStripControl = (GlbCompanyCampaignClickFilterControl)FilterItemModule.EmbeddedControl;
			FilterStripControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			FilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 290, true);
			FilterStripControl.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			FilterStripControl.FireValidationRequest += FilterStripControl_FireValidationRequest;

			this.Controls.Add(FilterStripControl);
		}

		void FilterStripControl_FireValidationRequest(object sender, System.EventArgs e)
		{
			foreach (FilterStrip strip in FilterStripControl.FilterBusinessObject.FilterStrips)
			{
				strip.Validation.ValidateAll();
			}
		}

		bool filterGridAdded;

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!filterGridAdded)
			{
				var statModel = dataSource as CampaignItemClickStatModel;
				if (statModel != null)
				{
					AddFilterGrid(statModel);
				}
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return Res.GetString("7887f0a1-fa64-41dc-986e-6b29b08e6659", "Campaign Link Activity Details"); }
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (FilterItemModule != null)
				{
					FilterItemModule.Dispose();
				}
				if (FilterStripControl != null)
				{
					FilterStripControl.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
