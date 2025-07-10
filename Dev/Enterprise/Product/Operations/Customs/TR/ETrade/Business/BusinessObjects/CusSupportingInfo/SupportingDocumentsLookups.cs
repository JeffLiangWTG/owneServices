using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class SupportingDocumentsLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentsLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}
		public override CodeDescriptionPairList StatusList => new SupportingDocumentStatusList();

		public override ICollection CodeList
		{
			get
			{
				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, TRSupportingDocumentsCodeType, ZDateTime.Today);
				return list;
			}
		}

		const string TRSupportingDocumentsCodeType = "TRDOC";
	}
}
