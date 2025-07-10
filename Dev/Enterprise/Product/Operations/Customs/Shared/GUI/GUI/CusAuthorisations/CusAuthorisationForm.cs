using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusAuthorisationForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CusAuthorisationForm() : base() { }

		public CusAuthorisationForm(CusAuthorisationHeader header) : base(header)
		{
			this.header = header;
			InitializeComponent();
			HookEvents();
			if (!DesignModeFinder.IsDesigning)
			{
				var provider = header.CustomsNumberProvider;
				NumberRangesTabPage.TabVisible = provider != null;
				provider?.SetupRelatedDataAndNotification();
			}
			UpdateAuthorisationNumberCharacterCasing(header.Provider);
		}

		void UpdateAuthorisationNumberCharacterCasing(CusAuthorisationHeaderProvider provider)
		{
			AuthorisationNumberZTextBox.CharacterCasing = provider.AllowMixedCaseAuthorisationNumbers(header) ? System.Windows.Forms.CharacterCasing.Normal : System.Windows.Forms.CharacterCasing.Upper;
			header.CPH_NumberInfo.RefreshBinding();
		}

		readonly CusAuthorisationHeader header;

		public override string FormCaption => header.CPH_IsAdHoc ? Res.GetString("34C71362-F308-4819-A1F0-4DA836A59A35", "Temporary Authorization") : Res.GetString("EDDE7A9B-3330-43B1-AF8C-6C59290EB41E", "Authorization");

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		CusAuthorisationRule currentAuthorizationRule;

		void HookEvents()
		{
			Load += CusAuthorisationForm_Load;
			header.CPH_TypeInfo.ValueChanged += CPH_TypeInfo_ValueChanged;
			header.CPH_IsAdHocInfo.ValueChanged += CPH_IsAdHocInfo_ValueChanged;

			AuthorisationRuleGrid.AfterBind += delegate
			{
				if (AuthorisationRuleGrid.ListManager != null)
				{
					AuthorisationRuleGrid.ListManager.CurrentChanged += AuthorizationRuleGrid_CurrentChanged;
				}
				AuthorizationRuleGrid_CurrentChanged(null, EventArgs.Empty);
			};
		}

		void UnhookEvents()
		{
			Load -= CusAuthorisationForm_Load;
			header.CPH_TypeInfo.ValueChanged -= CPH_TypeInfo_ValueChanged;
			header.CPH_IsAdHocInfo.ValueChanged -= CPH_IsAdHocInfo_ValueChanged;

			if (AuthorisationRuleGrid.ListManager != null)
			{
				AuthorisationRuleGrid.ListManager.CurrentChanged -= AuthorizationRuleGrid_CurrentChanged;
			}
			if (currentAuthorizationRule != null)
			{
				currentAuthorizationRule.CPR_RuleCodeInfo.ValueChanged -= RuleCode_ValueChanged;
			}
		}

		void CusAuthorisationForm_Load(object sender, EventArgs e)
		{
			AdHocCheckBox.Visible = header.Provider?.EnableAdHoc ?? false;
			SetupTypeRelatedControls();
		}

		void CPH_IsAdHocInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaption();
		}

		void CPH_TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupTypeRelatedControls();
			UpdateAuthorisationNumberCharacterCasing(header.Provider);
		}

		void AuthorizationRuleGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (currentAuthorizationRule != null)
			{
				currentAuthorizationRule.CPR_RuleCodeInfo.ValueChanged -= RuleCode_ValueChanged;
			}
			currentAuthorizationRule = (CusAuthorisationRule)AuthorisationRuleGrid.ListManager?.GetCurrent();
			if (currentAuthorizationRule != null)
			{
				currentAuthorizationRule.CPR_RuleCodeInfo.ValueChanged += RuleCode_ValueChanged;
			}
			RuleCode_ValueChanged(null, EventArgs.Empty);
		}

		void RuleCode_ValueChanged(object sender, EventArgs e)
		{
			LinkedAuthorisationRuleGroupBox.Visible = currentAuthorizationRule?.AllowLinkedRules ?? false;
		}

		void SetupTypeRelatedControls()
		{
			var numberRangesVisible = header.CustomsNumberProvider != null;
			var numberRangesTabPageTabVisible = NumberRangesTabPage.TabVisible;
			if (numberRangesTabPageTabVisible)
			{
				if (numberRangesVisible)
				{
					customsNumberViewStmNumsTabPageUserControl.LoadOrReloadUserControl();
				}
				else
				{
					NumberRangesTabPage.TabVisible = false;
				}
			}
			else
			{
				if (numberRangesVisible)
				{
					NumberRangesTabPage.TabVisible = true;
				}
			}
		}
	}
}
