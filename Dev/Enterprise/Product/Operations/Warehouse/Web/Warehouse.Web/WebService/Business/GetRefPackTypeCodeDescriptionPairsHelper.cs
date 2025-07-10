using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class GetRefPackTypeCodeDescriptionPairsHelper
	{
		public static CodeDescriptionPairList GetAsCodeDescriptionPairWithStandardUnits(bool isAndroidDevice, BusinessObjectFactory factory)
		{
			var list = new CodeDescriptionPairList();
			if (isAndroidDevice || WinCESupportedLanguages.List.Contains(Res.CurrentLanguage))
			{
				return factory.GetCachedValue("WhsDocketLineLookups|RefPackTypeCollection",
				() =>
				{
					return new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits();
				});
			}
			return list;
		}

		public static CodeDescriptionPair[] GetAsCodeDescriptionPair(bool isAndroidDevice, BusinessObjectFactory factory)
		{
			var supported = isAndroidDevice || WinCESupportedLanguages.List.Contains(Res.CurrentLanguage);
			return new RefPackTypeCollection(factory).
				Select(p => new CodeDescriptionPair(p.F3_Code.ToString(), (supported ? p.F3_DescriptionMultilingual : p.F3_Code).ToString())).ToArray();
		}
	}
}
