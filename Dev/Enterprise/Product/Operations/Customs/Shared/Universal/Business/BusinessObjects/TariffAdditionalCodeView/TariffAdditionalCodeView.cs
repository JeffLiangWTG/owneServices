using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(TariffAdditionalCodeView.Schema.ZY2_AdditionalCode), DescriptionProperty(TariffAdditionalCodeView.Schema.ZY2_Description)]
	public sealed class TariffAdditionalCodeView : AutoTariffAdditionalCodeView, ITranslatableZZBusinessObject, ITariffDataGroupingRelatedBusinessObject
	{
		public TariffAdditionalCodeView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTariffAdditionalCodeView.Schema
		{
			public const string ZY2_CategoryDescription = "ZY2_CategoryDescription";
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffAdditionalCodeView|ZY2_CategoryDescription", Caption = "Category Description", ShortCaption = "Description")]
		public ZString ZY2_CategoryDescription
		{
			get
			{
				if (zY2_CategoryDescriptionCached == null)
				{
					zY2_CategoryDescriptionCached = new CachedProperty<ZString>(Factory, () => Category?.ZY3_Description ?? ZString.Empty);
				}
				return zY2_CategoryDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> zY2_CategoryDescriptionCached;

		public ZPropertyInfo ZY2_CategoryDescriptionInfo => GetZPropertyInfo(Schema.ZY2_CategoryDescription);

		[ResourceStringData("Enterprise.Customs.Universal.TariffAdditionalCodeView|ZY2_AdditionalCode", Caption = "Additional Code")]
		public override ZString ZY2_AdditionalCode { get => base.ZY2_AdditionalCode; set => base.ZY2_AdditionalCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffAdditionalCodeView|ZY2_Description", Caption = "Description")]
		public override ZString ZY2_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZY2_Description, RefCusTariffAdditionalCodeLanguageSchema.ZY4_Description);
			set => base.ZY2_Description = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffAdditionalCodeView|ZY2_IsMandatory", Caption = "Is Mandatory")]
		public override ZBool ZY2_IsMandatory { get => base.ZY2_IsMandatory; set => base.ZY2_IsMandatory = value; }

		#region Related Business Objects

		[RelatedBusinessObject(nameof(CusTariff))]
		public override ZGuid ZY2_ZZ1_ParentTariffOrNationalCode
		{
			get { return base.ZY2_ZZ1_ParentTariffOrNationalCode; }
			set { base.ZY2_ZZ1_ParentTariffOrNationalCode = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZY2_ZZ1_ParentTariffOrNationalCode);

		[RelatedBusinessObject(nameof(DataGrouping))]
		[ResourceStringData("Enterprise.Customs.Universal.TariffAdditionalCodeView|ZY2_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping")]
		public override ZString ZY2_ZZZ_NKDataGrouping
		{
			get { return base.ZY2_ZZZ_NKDataGrouping; }
			set { base.ZY2_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping => Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZY2_ZZZ_NKDataGrouping);

		[ResourceStringData("Enterprise.Customs.Universal.TariffAdditionalCodeView|ZY2_ZY3_NKCategory", Caption = "Category")]
		[RelatedBusinessObject(nameof(Category))]
		public override ZString ZY2_ZY3_NKCategory
		{
			get { return base.ZY2_ZY3_NKCategory; }
			set { base.ZY2_ZY3_NKCategory = value; }
		}

		public RefCusTariffAdditionalCodeCategory Category => RefCusTariffAdditionalCodeCategory.Loader.Load(Factory, ZY2_ZZZ_NKDataGrouping, ZY2_ZY3_NKCategory);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : TranslatableZZBusinessObjectFetchStrategy<TariffAdditionalCodeView>
		{
			public Strategy(TariffAdditionalCodeView tariffAdditionalCode)
				: base(tariffAdditionalCode)
			{
			}

			protected new TariffAdditionalCodeView BusinessObject => (TariffAdditionalCodeView)base.BusinessObject;
		}

		#region ITranslatableZZBusinessObject
		ITableSchema ITranslatableZZBusinessObject.LanguageTableSchema => RefCusTariffAdditionalCodeLanguageSchema.Instance;

		Type ITranslatableZZBusinessObject.LanguageTableType => typeof(RefCusTariffAdditionalCodeLanguage);

		#endregion

		#region ITariffDataGroupingRelatedBusinessObject Members
		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => ZY2_ZZZ_NKDataGrouping;
		#endregion

		#region Applicabilities

		[ChildEditable]
		public CusRefApplicabilityViewCollection Applicabilities
		{
			get
			{
				if (applicabilities == null)
				{
					applicabilities = new CusRefApplicabilityViewCollection(this);
					RegisterEditableChildObject(applicabilities);
				}
				return applicabilities;
			}
		}

		CusRefApplicabilityViewCollection applicabilities;

		[ChildEditable]
		public FilteredCusRefApplicabilityViewCollection FilteredApplicabilities
		{
			get
			{
				if (filteredApplicabilities == null)
				{
					filteredApplicabilities = new FilteredCusRefApplicabilityViewCollection(this, CusTariff?.IsParentDataGrouping ?? false);
					RegisterEditableChildObject(filteredApplicabilities);
				}
				return filteredApplicabilities;
			}
		}
		FilteredCusRefApplicabilityViewCollection filteredApplicabilities;

		#endregion
	}
}
