using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AdditionalMessageInformationLookups : ZLookups
	{
		public AdditionalMessageInformationLookups(AdditionalMessageInformation additionalMessageInformation) : base(additionalMessageInformation)
		{
		}

		public CodeDescriptionPairList BoardedWeightUQList
		{
			get
			{
				var codePairList = new CodeDescriptionPairList();
				codePairList.AddPair("L", "Pounds");
				codePairList.AddPair("K", "Kilograms");
				return codePairList;
			}
		}
	}
}
