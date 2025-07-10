using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AIM.Messaging
{
	public static class AIMDispositionCodesHelper
	{
		public const string _11 = "11";
		public const string _12 = "12";
		public const string _1G = "1G";
		public const string _83 = "83";
		public const string _95 = "95";
		public const string CBPLocalTransferAuthorized = "1F";
		public const string InbondMovementAuthorized = "1D";
		public const string InbondTransferNotAuthorized = "1E";
		public const string P3 = "P3";

		public static CodeDescriptionPairList GetCustomsStatusList(BusinessObjectFactory factory)
		{
			var country = Core.Constants.CountryCodes.UnitedStates;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode;
			var date = ZDateTime.Today;
			return factory.GetCachedValue(
				string.Join("_", country, codeType, date),
				() =>
				{
					var result = new CodeDescriptionPairList();
					var codes = ZZRefCusCodeListCombined.Loader.Load(factory, country, codeType, date);
					result.AddRange(codes);
					result.Sort();
					return result;
				});
		}
	}
}
