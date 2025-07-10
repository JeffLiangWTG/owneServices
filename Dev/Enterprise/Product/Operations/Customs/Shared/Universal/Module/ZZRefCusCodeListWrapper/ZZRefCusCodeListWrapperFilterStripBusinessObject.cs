using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusCodeListWrapperFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(CodeCaption, ZZRefCusCodeListCombinedSchema.ZZD_Code);
			result.AddTextFilter(DescriptionCaption, ZZRefCusCodeListCombinedSchema.ZZD_Description);

			foreach (var filter in AttributeFilters)
			{
				result.AddFilter(new ZZRefCusCodeListAttributeTextFilter(filter.Key, filter.Value));
			}

			return result;
		}

		#region SuppressResourceStringsCheckRegion 

		protected virtual ZString CodeCaption => "Code";
		protected virtual ZString DescriptionCaption => "Description";

		#endregion

		protected virtual Dictionary<string, string> AttributeFilters => new Dictionary<string, string>();
	}
}
