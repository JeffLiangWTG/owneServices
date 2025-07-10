using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public static class PartAttribListCreator
	{
		public static CodeDescriptionPairList GetPartAttribList(this OrgSupplierPart part, ZGuid importerPK, ZGuid supplierPK, string classificationType, GetAttributesDelegate getAttributes, ZString country)
		{
			var pivots = part?.GetPivots<BaseCusClassPartPivot>(country)
				.GetMatchesIgnoringAttributes(country, classificationType, importerPK, supplierPK)
				.ToList() ?? new List<BaseCusClassPartPivot>();

			var result = new CodeDescriptionPairList();
			pivots.Where(x => x.CI_OH.IsValid || x.CI_OH.IsEmpty)
				.SelectMany(pivot => getAttributes(pivot))
				.ToList()
				.ForEach(attribute => result.AddPairIfNotExist(attribute.BG_AttributeValue1, attribute.BG_AttributeValue1));

			result.Sort();
			return result;
		}

		public delegate CusAttributeFilterCollection GetAttributesDelegate(BaseCusClassPartPivot pivot);
	}
}
