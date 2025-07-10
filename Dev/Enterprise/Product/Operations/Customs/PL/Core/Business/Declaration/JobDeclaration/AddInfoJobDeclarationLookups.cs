using System.Collections;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobDeclarationLookups
{
	public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<PLMethodOfPaymentList>();

	public override ICollection AgreedPlaceCodeList => Factory.GetAgreedPlaceCodeList(Parent.CountryCode);

	public override CodeDescriptionPairList BorderTransportMeansList
	{
		get
		{
			var transportMode = Declaration.JE_TransportMode;
			return Factory.GetCachedValue($"PL_AddInfoJobDeclarationLookups_BorderTransportMeansList_{transportMode}", () =>
			{
				var result = new CodeDescriptionPairList();
				return GetBorderTransportMeansListForExport(transportMode, result);
			});
		}
	}
}
