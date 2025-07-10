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
	[CodeProperty(RefCusTariffType.Schema.ZZI_TariffType), DescriptionProperty(RefCusTariffType.Schema.ZZI_Description)]
	public sealed class RefCusTariffType : AutoRefCusTariffType, ITranslatableZZBusinessObject
	{
		public RefCusTariffType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusTariffType|ZZI_Description", Caption = "Description", MediumCaption = "Description", ShortCaption = "Desc.")]
		public override ZString ZZI_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZI_Description, RefCusTariffTypeLanguageSchema.ZXK_Description);
			set => base.ZZI_Description = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusTariffType|ZZI_TariffType", Caption = "Tariff Type", MediumCaption = "Type", ShortCaption = "Type")]
		public override ZString ZZI_TariffType { get => base.ZZI_TariffType; set => base.ZZI_TariffType = value; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusTariffType.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public static RefCusTariffType Load(BusinessObjectFactory factory, string dataGroupingCode, string tariffType)
			{
				var query = new ZQuery(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, dataGroupingCode));
				query.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, tariffType);
				return factory.LoadTop1<RefCusTariffType>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCusTariffType);
			}
		}

		public RefDataGrouping DataGrouping => Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZI_ZZZ_NKDataGrouping);

		#region ITranslatableZZBusinessObject
		ITableSchema ITranslatableZZBusinessObject.LanguageTableSchema => RefCusTariffTypeLanguageSchema.Instance;

		Type ITranslatableZZBusinessObject.LanguageTableType => typeof(RefCusTariffTypeLanguage);
		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusTariffType>(this);
		}
	}
}
