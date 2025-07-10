using System.Collections;
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
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class TariffFindBox : ZCodeFindBox, ITariffFindBox
	{
		public TariffFindBox() : base()
		{
			PartialDescriptionMinLengthForSearch = 3;
		}

		public string EffectiveTariffCountry => GetCountryCode?.Invoke() is string countryCode && !string.IsNullOrEmpty(countryCode) ? countryCode : Env.CurrentCompany.Country.Code;

		[DefaultValue("")]
		public string TariffType { get; set; }

		[DefaultValue(3)]
		public int PartialDescriptionMinLengthForSearch { get; set; }

		public string ErrorForUnsupportedCountry { get; set; }

		public List<SelectionStyle> SelectNomenclatureModes { get; set; }
		public System.Func<List<SelectionStyle>> GetSelectNomenclatureModes;

		public System.Func<ZDateTime> GetEffectiveDate { get; set; }

		public bool ShowDescriptionFilterOnNonNomenclatureTariffModule { get; set; }

		public System.Func<ZString> GetTariffType { get; set; }

		public System.Func<ZString> GetDataGrouping { get; set; }

		public bool NeedLoadParentDataGroup { get; set; } = true;

		public bool NeedLoadNomenclatureWhenTariffNotFound { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZQuery NomenclatureGroupAdditionalFilter => GetNomenclatureGroupAdditionalFilter();

		protected virtual ZQuery GetNomenclatureGroupAdditionalFilter() => null;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZQuery TariffAdditionalFilter => GetTariffAdditionalFilter();

		protected virtual ZQuery GetTariffAdditionalFilter() => null;

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			var bo = DataSource as BusinessObject;
			if (bo != null)
			{
				if (!string.IsNullOrEmpty(ErrorForUnsupportedCountry)
					&& !TariffSearchHelper.HasValidDataGroupingAndTariffType(bo.Factory, EffectiveDataGrouping, TariffType))
				{
					Globals.Message.Show(ErrorForUnsupportedCountry, Res.GetString("d2682daf-a43c-4d36-9dd0-22f14e6c1817", "Tariff Search"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
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
				var dataSource = (BusinessObject)DataSource;

				var borderWiseTariffCodeFindBox = BorderWiseTariffFindBoxWrapperProvider.GetFindBoxWrapper(dataSource, EffectiveTariffCountry, tariffType, effectiveDate, EffectiveDataGrouping);

				if (borderWiseTariffCodeFindBox != null)
				{
					return borderWiseTariffCodeFindBox;
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

		void LanguageInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var helper = sender as TariffSearchHelper;
			if (helper != null)
			{
				helper.PartialDescriptionMinLength = helper.GetPartialDescriptionMinLength(PartialDescriptionMinLengthForSearch);
			}
		}

		protected virtual ITariffFormatter GetTariffFormatter()
		{
			var tariffFormatProvider = CurrentItem as ITariffFormatProvider;
			return tariffFormatProvider?.TariffFormatter;
		}

		protected override void ShowEditOrViewForm()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.Customs.Universal.RefCusTariff))
			{
				module.SetFormsModalTo(FindForm());
				if (module is ITariffViewFilterDataSupporter supporter)
				{
					supporter.TariffViewFilterData = (CurrentItem as ITariffViewFilterDataProvider)?.TariffViewFilterData;
				}
				ShowViewForm(module);
			}
		}

		protected override IFindBoxListProvider ListProvider =>
			new TariffFindBoxListProvider(EffectiveDataGrouping, GetTariffType?.Invoke() ?? TariffType, GetEffectiveDate, TariffAdditionalFilter, GetTariffFormatter());

		protected override IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}

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

		protected override bool RequiresList => false;
	}
}
