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
	public partial class GearTypeList
	{
		public static ICodeDescriptionPairList GetListFor370Program(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("370GearTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.LargeScaleDriftnetHighSeas, Descriptions.LargeScaleDriftnetHighSeas);
					result.AddPair(Codes.GillnetLessThan15Miles24KmInTotalLength, Descriptions.GillnetLessThan15Miles24KmInTotalLength);
					result.AddPair(Codes.Longline, Descriptions.Longline);
					result.AddPair(Codes.OtherType, Descriptions.OtherType);
					result.AddPair(Codes.PoleAndLineHookAndLine, Descriptions.PoleAndLineHookAndLine);
					result.AddPair(Codes.PurseSeineNet, Descriptions.PurseSeineNet);
					return result;
				});
		}
	}
}
