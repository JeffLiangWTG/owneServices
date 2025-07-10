
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USPGAAddInfoLookups : AutoUSPGAAddInfoLookups
	{
		public USPGAAddInfoLookups(AutoUSPGAAddInfo parent) : base(parent)
		{
		}

		public ACELaceyUnitsOfMeasureList UnitOfMeasureList
		{
			get { return Factory.GetCachedValue<ACELaceyUnitsOfMeasureList>(); }
		}

		public CodeDescriptionPairList CertifyingIndividualList
		{
			get
			{
				return Factory.GetCachedValue("CertifyingIndividualList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(PartyTypeList.Codes.CustomsBroker, PartyTypeList.Descriptions.CustomsBroker);
					result.AddPair(PartyTypeList.Codes.Importer, PartyTypeList.Descriptions.Importer);
					return result;
				});
			}
		}
	}
}
