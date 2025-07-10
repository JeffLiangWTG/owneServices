using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusConditionType.Schema.ZX2_ConditionType), DescriptionProperty(RefCusConditionType.Schema.ZX2_Description)]
	public sealed class RefCusConditionType : AutoRefCusConditionType, ITranslatableZZBusinessObject
	{
		public RefCusConditionType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZX2_ZZZ_NKDataGrouping
		{
			get => base.ZX2_ZZZ_NKDataGrouping;
			set => base.ZX2_ZZZ_NKDataGrouping = value;
		}

		public RefDataGrouping DataGrouping => Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZX2_ZZZ_NKDataGrouping);

		public override ZString ZX2_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZX2_Description, RefCusConditionTypeLanguageSchema.ZXW_Description);
			set => base.ZX2_Description = value;
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusConditionTypeLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusConditionTypeLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusConditionType>(this);
		}
	}
}
