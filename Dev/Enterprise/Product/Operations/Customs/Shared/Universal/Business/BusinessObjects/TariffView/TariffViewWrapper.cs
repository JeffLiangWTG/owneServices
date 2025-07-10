using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class TariffViewWrapper : NonPersistentBusinessObject
	{
		internal TariffViewWrapper(TariffView tariffView)
			: base(tariffView.Factory)
		{
			this.TariffView = tariffView;
		}
		public readonly TariffView TariffView;

		public static class Schema
		{
			public const string EffectiveDataGrouping = "EffectiveDataGrouping";
			public const string EffectiveDate = "EffectiveDate";
			public const string SelectedNomenclatureDescription = "SelectedNomenclatureDescription";
			public const string SelectedNomenclatureAlternateLanguageDescription = "SelectedNomenclatureAlternateLanguageDescription";
			public const string RatesApplyToCountry = "RatesApplyToCountry";
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|EffectiveDataGrouping", Caption = "Effective Country/Region or Grouping")]
		[List(nameof(ApplicableDataGroupingList))]
		public ZString EffectiveDataGrouping
		{
			get => effectiveDataGrouping;
			set => SetNonPersistentPropertyValue(EffectiveDataGroupingInfo, ref effectiveDataGrouping, value);
		}
		ZString effectiveDataGrouping;

		public ZPropertyInfo EffectiveDataGroupingInfo => GetZPropertyInfo(Schema.EffectiveDataGrouping);

		public ICodeDescriptionPairList ApplicableDataGroupingList
		{
			get
			{
				return TariffView.DataGrouping?.ApplicableDataGroupingList ?? new CodeDescriptionPairList();
			}
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|EffectiveDate", Caption = "Effective Date")]
		public ZDate EffectiveDate
		{
			get
			{
				if (!effectiveDate.HasValue)
				{
					var date = ZDate.Today;
					var startDate = TariffView.ZZ1_StartDate.Date;
					var endDate = TariffView.ZZ1_EndDate.Date;
					effectiveDate = startDate <= date && date <= endDate ? date : startDate > date ? startDate : endDate;
				}
				return effectiveDate.Value;
			}
			set
			{
				var oldValue = EffectiveDate;
				effectiveDate = value;
				EffectiveDateInfo.RefreshBinding(oldValue);
			}
		}
		ZDate? effectiveDate;

		public ZPropertyInfo EffectiveDateInfo => GetZPropertyInfo(Schema.EffectiveDate);

		[ReadOnly(true)]
		public ZString SelectedNomenclatureDescription
		{
			get => selectedNomenclatureDescription;
			set => SetNonPersistentPropertyValue(SelectedNomenclatureDescriptionInfo, ref selectedNomenclatureDescription, value);
		}
		ZString selectedNomenclatureDescription;

		public ZPropertyInfo SelectedNomenclatureDescriptionInfo => GetZPropertyInfo(Schema.SelectedNomenclatureDescription);

		[ReadOnly(true)]
		public ZString SelectedNomenclatureAlternateLanguageDescription
		{
			get => selectedNomenclatureAlternateLanguageDescription;
			set => SetNonPersistentPropertyValue(SelectedNomenclatureAlternateLanguageDescriptionInfo, ref selectedNomenclatureAlternateLanguageDescription, value);
		}
		ZString selectedNomenclatureAlternateLanguageDescription;

		public ZPropertyInfo SelectedNomenclatureAlternateLanguageDescriptionInfo => GetZPropertyInfo(Schema.SelectedNomenclatureAlternateLanguageDescription);

		public bool MatchEffectiveDataGrouping(ZString dataGrouping)
		{
			return dataGrouping.IsEmpty || EffectiveDataGrouping.IsEmpty || dataGrouping == EffectiveDataGrouping;
		}

		public bool IsWithInEffectiveDate(ZDateTime startDate, ZDateTime endDate)
		{
			var date = EffectiveDate;
			return date.IsEmpty || (startDate.Date <= date && date <= endDate.Date);
		}

		public RefCusNomenclatureGroupCollection NomenclatureGroups
		{
			get
			{
				if (nomenclatureGroups == null)
				{
					nomenclatureGroups = new RefCusNomenclatureGroupCollection(TariffView);
				}
				return nomenclatureGroups;
			}
		}
		RefCusNomenclatureGroupCollection nomenclatureGroups;

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|RatesApplyToCountry", Caption = "Show rates/conditions that apply to Country/Region")]
		[List(nameof(RatesApplyToCountryList))]
		[MaxLength(2)]
		public ZString RatesApplyToCountry
		{
			get => ratesApplyToCountry;
			set => SetNonPersistentPropertyValue(RatesApplyToCountryInfo, ref ratesApplyToCountry, value);
		}
		ZString ratesApplyToCountry;

		public ZPropertyInfo RatesApplyToCountryInfo => GetZPropertyInfo(Schema.RatesApplyToCountry);

		public RefCountryCollection RatesApplyToCountryList => new RefCountryCollection(Factory);
	}
}
