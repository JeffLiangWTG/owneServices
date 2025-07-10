using System.Globalization;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.NL.Business.Declaration;

public class NLTaxLookupsCommon : TaxLookupsCommon
{
	public NLTaxLookupsCommon(EU.Business.Declaration.IEuTax parent) : base(parent)
	{
	}

	public override RateCodeDescriptionPairList TypeList
	{
		get
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "NL.TypeList_{0}_{1}", parent.ImportExportParent.IsImport, parent.ImportExportParent.IsExport);
			return parent.Factory.GetCachedValue(cacheKey, delegate
			{
				var result = base.TypeList;
				var nlTypeList = new RateCodeDescriptionPairList();
				nlTypeList.AddRange(result);
				if (parent.ImportExportParent.IsImport)
				{
					nlTypeList.RemoveCode("087");
				}
				return nlTypeList;
			});
		}
	}
}
