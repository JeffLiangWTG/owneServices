using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	sealed class GlobalRateDescriptionTranslatableFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public GlobalRateDescriptionTranslatableFieldAttribute(string tableName, string columnName)
			: base(tableName, columnName)
		{
		}

		public override IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			return new ResourceString[]
			{
				CustomizableDataResourceStrings.GetMultilingualString(this, null, Costing.StandardGlobalRateDescription)
			};
		}

		public override IEnumerable<IResString> GetRuntimeCaptions(IResString userCaption = null, object context = null)
		{
			var result = base.GetRuntimeCaptions(userCaption, context);
			if (!result.Any(res => res.EnglishText == Costing.StandardGlobalRateDescription))
			{
				result = result.Concat(new[] { CustomizableDataResourceStrings.GetMultilingualString(this, null, Costing.StandardGlobalRateDescription) });
			}
			return result;
		}

#if DEBUG
		public override BusinessObject[] GetSystemDefinedParentObjectsForTest(BusinessObjectFactory factory, Type type)
		{
			return new BusinessObject[] { factory.New<Costing>() };
		}
#endif
	}
}
