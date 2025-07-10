using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccTaxRateForm : ZForm
	{
		public AccTaxRateForm()
		{
		}

		public AccTaxRateForm(AccTaxRate taxRate)
			: base(taxRate)
		{
			SetupForm();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}

		protected AccTaxRate TaxRate
		{
			get { return (AccTaxRate)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SetupForm()
		{
			var isAuxiliaryRateControlsVisible = TaxRate.AT_ExtraTaxRateType != string.Empty;

			AT_ExtraRateBoundCalcEdit.Visible = isAuxiliaryRateControlsVisible;
			AuxiliaryTaxTypeDropEdit.Visible = isAuxiliaryRateControlsVisible;

			PostingGroupEdit.ReadOnly = !GlbStaff.CurrentUser.IsSupportUser;
			PostingGroupEdit.Visible = AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			ResizeFormBasedOnReferenceData();

			if (!Env.CurrentUser.IsSupportUser)
			{
				HideRefTypeColumns();
			}
		}

		void ResizeFormBasedOnReferenceData()
		{
			var clientHeight = ClientSize.Height - RefDataEmptyLabel.Height;

			if (!TaxRate.SystemTaxRate)
			{
				RefTaxRateGroupBox.Visible = RefExtraTaxRateGroupBox.Visible = false;
				clientHeight -= (RefTaxRateGroupBox.Height + RefExtraTaxRateGroupBox.Height);
			}
			else
			{
				if (TaxRate.RefTaxRateDataForUIBinding.Any())
				{
					if (!TaxRate.RefExtraTaxRateDataForUIBinding.Any())
					{
						RefExtraTaxRateGroupBox.Visible = false;
						clientHeight -= RefExtraTaxRateGroupBox.Height;
					}
				}
				else
				{
					RefTaxRateGroupBox.Visible = RefExtraTaxRateGroupBox.Visible = false;
					RefDataEmptyLabel.Visible = true;
					RefDataEmptyLabel.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, RefTaxRateGroupBox.Height + RefExtraTaxRateGroupBox.Height, false).Height;
				}
			}

			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(ClientSize.Width, clientHeight, false);
		}

		void HideRefTypeColumns()
		{
			TaxRateGrid.RemoveFromAvailableColumns("ZAT_ReferenceRateType");
			ExtraTaxRateGrid.RemoveFromAvailableColumns("ZAT_ReferenceRateType");
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return TaxRate.AT_TaxSystemCode.IsEmpty ? GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription + " " + Res.GetString("Accounting|AccTaxRateForm|FormCaptionSuffix", "Tax ID") : Res.GetString("Accounting|AccTaxRateForm|FormCaptionSuffix|TaxSystemTaxID", "Tax System Tax ID"); }
		}

		public override string FormVerb
		{
			get
			{
				string verb = base.FormVerb;

				if (BusinessEntityForHasChanges != null && DisplayMode == ODisplayMode.Delete)
				{
					verb = Res.GetString("f92f5bfc-847d-4df8-a8f4-c7f970628594", "Deactivate");
				}

				return verb;
			}
		}

		#endregion
	}
}
