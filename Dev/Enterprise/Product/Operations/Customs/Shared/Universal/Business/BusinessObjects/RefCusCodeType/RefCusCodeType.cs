using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusCodeType.Schema.ZZK_CodeType), DescriptionProperty(RefCusCodeType.Schema.ZZK_Description)]
	public sealed class RefCusCodeType : AutoRefCusCodeType, ITranslatableZZBusinessObject
	{
		public RefCusCodeType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static int GetMaxLength(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, int defaultValue, ZString? fallBackDataGrouping = null)
		{
			return factory.GetCachedValue(string.Join("_", "RefCusCodeType_ZZK_MaxLength", dataGrouping, codeType, defaultValue, fallBackDataGrouping.HasValue ? (string)fallBackDataGrouping.Value : "NULL"),
				() =>
				{
					var result = defaultValue;
					var loader = new Loader(factory);
					var cusCodeType = loader.Load(dataGrouping, codeType);
					if (cusCodeType != null)
					{
						result = cusCodeType.ZZK_MaxLength;
					}
					else if (fallBackDataGrouping.HasValue && dataGrouping != fallBackDataGrouping.Value)
					{
						cusCodeType = loader.Load(fallBackDataGrouping.Value, codeType);
						if (cusCodeType != null)
						{
							result = cusCodeType.ZZK_MaxLength;
						}
					}
					return result;
				});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusCodeType.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static ZQuery GetQuery(ZString dataGrouping, ZString codeType)
			{
				var query = new ZQuery(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, dataGrouping);
				query.AddToFilter(RefCusCodeTypeSchema.ZZK_CodeType, codeType);
				return query;
			}

			public RefCusCodeType Load(ZString dataGrouping, ZString codeType)
			{
				return Factory.LoadTop1<RefCusCodeType>(GetQuery(dataGrouping, codeType));
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusCodeType);
		}

		public override ZString ZZK_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZK_Description, RefCusCodeTypeLanguageSchema.ZXI_Description);
			set => base.ZZK_Description = value;
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusCodeTypeLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusCodeTypeLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusCodeType>(this);
		}
	}
}
