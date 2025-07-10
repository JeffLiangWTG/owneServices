using CargoWise.EntityFramework;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class InspectionStatusList
	{
		public static ICodeDescriptionPairList GetListForAPHIS(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("InspectionStatusListForAPHIS", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.PreviouslyPerformed, Descriptions.PreviouslyPerformed);
					result.AddPair(Codes.BTAAnticipatedArrivalInformation, BTAAnticipatedArrivalInformationDescriptionForAPHIS);
					return result;
				});
		}

		public const string BTAAnticipatedArrivalInformationDescriptionForAPHIS = "Anticipated arrival information (For FDA Prior Notice or other agency needs)";
	}
}
