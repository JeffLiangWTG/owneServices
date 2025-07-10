
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USPSTLineAddInfoLookups : AutoUSPSTLineAddInfoLookups
	{
		public USPSTLineAddInfoLookups(AutoUSPSTLineAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProductQualifiers
		{
			get
			{
				return Factory.GetCachedValue("ProductQualifiers", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber, ProductCodeQualifiersList.Descriptions.ChemicalAbstractServicesNumber);
					result.AddPair(ProductCodeQualifiersList.Codes.PCCode, ProductCodeQualifiersList.Descriptions.PCCode);
					return result;
				});
			}
		}
	}
}
