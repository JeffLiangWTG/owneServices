using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	public class TariffGridFindBox : ZGridFindBox, ITariffFindBox
	{
		public string EffectiveTariffCountry => GetCountryCode?.Invoke() is string countryCode && !string.IsNullOrEmpty(countryCode) ? countryCode : Env.CurrentCompany.Country.Code;

		public Func<ZDateTime> GetEffectiveDate { get; set; }

		public bool ShowDescriptionFilterOnNonNomenclatureTariffModule { get; set; }

		public Func<ZString> GetTariffType { get; set; }

		public Func<ZString> GetDataGrouping { get; set; }

		public bool NeedLoadParentDataGroup { get; set; } = true;

		public bool NeedLoadNomenclatureWhenTariffNotFound { get; set; }

		public TariffGridFindBox()
		{
			PartialDescriptionMinLengthForSearch = 3;
		}

		[DefaultValue("")]
		public string TariffType { get; set; }

		[DefaultValue(3)]
		public int PartialDescriptionMinLengthForSearch { get; set; }

		public ZString ErrorForUnsupportedCountry { get; set; }

		public List<SelectionStyle> SelectNomenclatureModes { get; set; }
		public Func<List<SelectionStyle>> GetSelectNomenclatureModes;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZQuery NomenclatureGroupAdditionalFilter { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZQuery TariffAdditionalFilter { get; set; }

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			var grid = (ZArchitecture.ZGrid)Parent;
			if (grid != null)
			{
				var bo = grid.DataSource as BusinessObject;
				if (bo != null)
				{
					if (!ErrorForUnsupportedCountry.IsEmpty
						&& !TariffSearchHelper.HasValidDataGroupingAndTariffType(bo.Factory, EffectiveDataGrouping, TariffType))
					{
						Globals.Message.Show(ErrorForUnsupportedCountry, Res.GetString("85392c07-9994-4acb-b8b1-6e906e1ad746", "Tariff Search"), MessageBoxButtons.OK, MessageBoxIcon.Information);
						return;
					}
				}
			}

			if (PopupForm is FindBoxWrapperForBorderWise borderWiseFindBoxWrapper && borderWiseFindBoxWrapper.BorderWiseLauncher.CountryCodeOverride != EffectiveDataGrouping)
			{
				ClosePopupForm();
			}

			base.SelectFromPopupForm(autoSelect);
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			var tariffType = GetTariffType?.Invoke() ?? TariffType;
			var effectiveDate = GetEffectiveDate?.Invoke() ?? ZDateTime.Today;
			if (!effectiveDate.IsValid)
			{
				effectiveDate = ZDateTime.Today;
			}

			if (Env.Registry.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.BorderWiseWeb)
			{
				var grid = (ZArchitecture.ZGrid)Parent;
				if (grid != null)
				{
					var bo = grid.DataSource as BusinessObject;
					var borderWiseTariffCodeFindBox = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(bo, EffectiveTariffCountry, tariffType, effectiveDate, EffectiveDataGrouping);

					if (borderWiseTariffCodeFindBox != null)
					{
						return borderWiseTariffCodeFindBox;
					}
				}
			}

			var tariffSearchHelper = new TariffSearchHelper(EffectiveDataGrouping, tariffType, NomenclatureGroupAdditionalFilter, TariffAdditionalFilter, GetSelectNomenclatureModes?.Invoke() ?? SelectNomenclatureModes, NeedLoadParentDataGroup, NeedLoadNomenclatureWhenTariffNotFound);
			if (tariffSearchHelper.HasNomenclatureGroup)
			{
				using (tariffSearchHelper.GetValidationSuspender())
				{
					tariffSearchHelper.EffectiveDate = effectiveDate;
					tariffSearchHelper.TariffFormatter = GetTariffFormatter();
					tariffSearchHelper.ChapterHeadingTariff = CodeBox.Text;
					tariffSearchHelper.PartialDescriptionMinLength = tariffSearchHelper.GetPartialDescriptionMinLength(PartialDescriptionMinLengthForSearch);
					tariffSearchHelper.LanguageInfo.ValueChanged += LanguageInfo_ValueChanged;
				}
				return new TariffFindBoxTreeViewForm(tariffSearchHelper);
			}
			else
			{
				return NonNomenclatureTariffModuleProvider.Show(this, EffectiveDataGrouping, tariffSearchHelper.TariffType);
			}
		}

		void LanguageInfo_ValueChanged(object sender, EventArgs e)
		{
			var helper = sender as TariffSearchHelper;
			if (helper != null)
			{
				helper.PartialDescriptionMinLength = helper.GetPartialDescriptionMinLength(PartialDescriptionMinLengthForSearch);
			}
		}

		protected ITariffFormatter GetTariffFormatter()
		{
			ITariffFormatter result = null;
			var grid = (ZArchitecture.ZGrid)Parent;
			if (grid != null)
			{
				var tariffFormatProvider = BindingContext[grid.DataSource, grid.DataMember].GetCurrent() as ITariffFormatProvider;
				result = tariffFormatProvider?.TariffFormatter;
			}
			return result;
		}

		protected override void ShowEditOrViewForm()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.Customs.Universal.RefCusTariff))
			{
				module.SetFormsModalTo(FindForm());
				if (module is ITariffViewFilterDataSupporter supporter)
				{
					supporter.TariffViewFilterData = GetTariffViewFilterData();
				}
				ShowViewForm(module);
			}
		}

		ITariffViewFilterData GetTariffViewFilterData()
		{
			var grid = (ZArchitecture.ZGrid)Parent;
			return (grid?.GetCurrent() as ITariffViewFilterDataProvider)?.TariffViewFilterData;
		}

		protected override IFindBoxListProvider ListProvider =>
			new TariffFindBoxListProvider(EffectiveDataGrouping, GetTariffType?.Invoke() ?? TariffType, GetEffectiveDate, TariffAdditionalFilter, GetTariffFormatter());

		public ZString EffectiveDataGrouping
		{
			get
			{
				var result = ZString.Empty;
				if (GetDataGrouping != null)
				{
					result = GetDataGrouping();
				}
				if (result.IsEmpty)
				{
					result = DesignMode ? string.Empty : Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return result;
			}
		}
	}
}
