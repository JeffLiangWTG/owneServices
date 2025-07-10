using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ExtensionSuspensionCodeList : CodeDescriptionPairList
	{
		public ExtensionSuspensionCodeList(BusinessObjectFactory factory)
		{
			var codes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LiquidationExtensionSuspensionCode, ZDateTime.Today);
			codes.OrderBy(x => ZInt.ParseSafe(x.ZZD_Code, 0)).ForEach(code => AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description));
		}
	}
}
