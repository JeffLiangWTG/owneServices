using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.NL.Module
{
	public class EntryStatusListProvider : EU.Module.EntryStatusListProvider
	{
		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => new CodeDescriptionPairList(Universal.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Netherlands, RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today));
	}
}
