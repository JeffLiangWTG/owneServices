using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : OrgSupplierPartFormCustomsControl
	{
		public OrgSupplierPartFormCustomsControlGlobal() : this(true)
		{
		}

		public OrgSupplierPartFormCustomsControlGlobal(bool initialise = true)
		{
			if (initialise)
			{
				InitializeComponent();
			}
		}

		protected override void InitializeForm()
		{
			if (!this.IsDesignMode())
			{
				AddTariffColumn();
			}
			base.InitializeForm();
		}

		protected void PivotGrid_AfterBind(object sender, EventArgs e)
		{
			PivotGrid.ListManager.ItemChanged -= PivotGridListManager_ItemChanged;
			PivotGrid.ListManager.ItemChanged += PivotGridListManager_ItemChanged;
			PivotGrid.ListManager.PositionChanged -= ListManager_PositionChanged;
			PivotGrid.ListManager.PositionChanged += ListManager_PositionChanged;
			PivotGridListManager_ItemChanged(null, null);
		}

		protected void ListManager_PositionChanged(object sender, EventArgs e)
		{
			UpdateCurrentPartPivot();
			ChangeControlsVisibility();
		}

		protected void PivotGridListManager_ItemChanged(object sender, ItemChangedEventArgs e)
		{
			UpdateCurrentPartPivot();
			ChangeControlsVisibility();
		}

		void UpdateCurrentPartPivot()
		{
			BaseCusClassPartPivot partPivot = null;
			var listManager = PivotGrid.ListManager;
			if (listManager != null)
			{
				partPivot = (BaseCusClassPartPivot)listManager.GetCurrent();
				if (partPivot != null && partPivot.IsDeleted)
				{
					partPivot = null;
				}
			}

			if (currentPartPivot != partPivot)
			{
				UnHookPartPivotEvents(currentPartPivot);
				currentPartPivot = partPivot;
				HookPartPivotEvents(partPivot);
			}
		}

		protected virtual void HookPartPivotEvents(BaseCusClassPartPivot partPivot)
		{
			if (partPivot != null)
			{
				partPivot.CI_ChildTypeInfo.ValueChanged += CI_ChildType_ValueChanged;
			}
			CI_ChildType_ValueChanged(null, null);
		}

		protected virtual void UnHookPartPivotEvents(BaseCusClassPartPivot partPivot)
		{
			if (partPivot != null)
			{
				partPivot.CI_ChildTypeInfo.ValueChanged -= CI_ChildType_ValueChanged;
			}
		}

		void CI_ChildType_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlsVisibility();
		}

		protected BaseCusClassPartPivot currentPartPivot;

		protected virtual void ChangeControlsVisibility()
		{
		}

		void AddTariffColumn()
		{
			var tariffColumnStyleInfo = CreateTariffColumn();

			var ouInfo = PivotGrid.GetColumnStyle(BaseCusClassPartPivot.Schema.CI_OH);
			if (ouInfo != null)
			{
				var index = PivotGrid.ColumnStyles.IndexOf(ouInfo);
				PivotGrid.ColumnStyles.Insert(index + 1, tariffColumnStyleInfo);
			}
			else
			{
				PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo);
			}
		}

		protected virtual ZBaseFindBoxColumnStyleInfo CreateTariffColumn()
		{
			var tariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
			tariffColumnStyleInfo.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|F4B56DFB-1CE4-49E8-A8E0-4702765BCC06", "Tariff");
			tariffColumnStyleInfo.ColumnName = TariffColumnName;
			tariffColumnStyleInfo.GetCountryCode = GetCustomsCountryCode;
			tariffColumnStyleInfo.GetDataGrouping = GetDataGroupingForUniversalTariff;
			tariffColumnStyleInfo.TariffType = UniversalTariffType;
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			return tariffColumnStyleInfo;
		}

		public string TariffColumnName => TariffColumnNameCore;
		protected virtual string TariffColumnNameCore => BaseCusClassPartPivot.Schema.CI_FormattedTariffNum;
		public string DateStartColumnName => BaseCusClassPartPivot.Schema.CI_DateStart;
		public string DateEndColumnName => BaseCusClassPartPivot.Schema.CI_DateEnd;
		protected virtual string UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;
		protected virtual string GetCustomsCountryCode()
		{
			string result = null;
			if (!this.IsDesignMode())
			{
				var pivot = currentPartPivot;
				result = pivot == null || pivot.IsDeleted ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : pivot.CI_RN_NKCountry;
			}
			return result;
		}

		protected virtual ZString GetDataGroupingForUniversalTariff()
		{
			var pivot = currentPartPivot;
			return pivot == null || pivot.IsDeleted ? Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GetCustomsCountryCode())
				: pivot.DefaultDataGroupingForTariffs;
		}

		protected void DisableUnneededPivotColumns()
		{
			// Currently used only in AU/NZ/SG
			this.PivotGrid.RemoveFromAvailableColumns(DateStartColumnName, DateEndColumnName);
		}
	}
}
