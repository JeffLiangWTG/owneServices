using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
	{
		public AddInfoCusEntryInstructionLookups(EU.Business.Declaration.AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		public CodeDescriptionPairList UnionSecretaryCodesList => Factory.GetCachedValue<UnionSecretaryCodesList>();

		public CodeDescriptionPairList UnionCodesList => Factory.GetCachedValue<UnionCodesList>();

		public ICodeDescriptionPairList ExportUnionCountryCodeList
		{
			get
			{
				var dataGroupingCode = Core.Constants.CountryCodes.Turkey;
				return Factory.GetCachedValue("ExportUnionCountryCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory,
						dataGroupingCode,
						TRExportUnionCountryCode,
						ZDateTime.Today));
					result.Sort();
					return result;
				});
			}
		}
		const string TRExportUnionCountryCode = "TREUC";

		public CodeDescriptionPairList TransportModeInland => Factory.GetCachedValue<TRTransportModeInland>();
	}
}
