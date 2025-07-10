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
	public static class PGADispositionCodeList
	{
		public const string DataRejectedPerPGAReview = "04";
		public const string MayProceed = "07";
		public const string DocumentRequired = "10";
		public const string OneUSGCode = "01";
		public const string OneUSGDesc = "One USG";
		public const string MarkAsClosedCode = "MC";
		public const string MarkAsClosedCodeDesc = "Mark As Closed";

		public static bool IsDocRequiredPGADispositionCode(string code)
		{
			return code == DocumentRequired;
		}

		public static bool IsFinalCode(string code)
		{
			return code == MayProceed || code == MarkAsClosedCode;
		}

		public static CodeDescriptionPairList GetPGADispositionCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PGADispositionCodeList", () =>
			{
				var result = new CodeDescriptionPairList();
				var codeList = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus,
					ZDateTime.Today);
				codeList.OrderBy(x => x.ZZD_Code).ForEach(c => result.AddPairIfNotExist(c.ZZD_Code, c.ZZD_Description));

				result.AddPairIfNotExist(PGADispositionCodeList.MarkAsClosedCode, PGADispositionCodeList.MarkAsClosedCodeDesc);

				return result;
			});
		}
	}
}
