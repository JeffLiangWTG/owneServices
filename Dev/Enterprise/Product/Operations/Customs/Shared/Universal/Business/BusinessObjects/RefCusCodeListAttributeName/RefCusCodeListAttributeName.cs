using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusCodeListAttributeName.Schema.TranslatedName), DescriptionProperty(RefCusCodeListAttributeName.Schema.ZXE_Description)]
	public sealed class RefCusCodeListAttributeName : AutoRefCusCodeListAttributeName, ITranslatableZZBusinessObject
	{
		#region Schema

		public new class Schema : AutoRefCusCodeListAttributeName.Schema
		{
			public const string TranslatedName = "TranslatedName";
		}

		#endregion

		public RefCusCodeListAttributeName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString ZXE_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZXE_Description, RefCusCodeListAttributeNameLanguageSchema.ZXH_Description);
			set => base.ZXE_Description = value;
		}

		public ZString TranslatedName => TranslationHelper.GetTranslatedValue(this, ZXE_Name, RefCusCodeListAttributeNameLanguageSchema.ZXH_Name);

		public override ZString ZXE_ColumnCaption
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZXE_ColumnCaption, RefCusCodeListAttributeNameLanguageSchema.ZXH_ColumnCaption);
			set => base.ZXE_ColumnCaption = value;
		}

		public ZString OriginalLanguageDescription => base.ZXE_Description;

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusCodeListAttributeNameLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusCodeListAttributeNameLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusCodeListAttributeName>(this);
		}
	}
}
