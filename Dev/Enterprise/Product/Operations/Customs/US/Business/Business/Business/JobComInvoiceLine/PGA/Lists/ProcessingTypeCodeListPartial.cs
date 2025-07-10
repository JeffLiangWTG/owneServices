using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class ProcessingTypeCodeList
	{
		public static ICodeDescriptionPairList GetListForNMFS(BusinessObjectFactory factory, ZString programCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("ProcessingTypeCodeList_" + programCode, () =>
			{
				var result = new CodeDescriptionPairList();
				if (programCode == NMFSProgramCodeList.Codes.SIM)
				{
					result.AddPair(Codes.NmfsDressed, Descriptions.NmfsDressed);
					result.AddPair(Codes.NmfsFillet, Descriptions.NmfsFillet);
					result.AddPair(Codes.NmfsGilledAndGutted, Descriptions.NmfsGilledAndGutted);
					result.AddPair(Codes.NmfsOther, Descriptions.NmfsOther);
					result.AddPair(Codes.NmfsRound, Descriptions.NmfsRound);
					result.AddPair(Codes.NmfsSteak, Descriptions.NmfsSteak);
					result.AddPair(Codes.NmfsRadiationSterilized, Descriptions.NmfsRadiationSterilized);
				}
				return result;
			});
		}
	}
}
