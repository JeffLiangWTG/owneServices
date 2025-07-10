using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CusPersonCountryLookups : ASYCUDA.Business.CusPersonCountryLookups
	{
		public CusPersonCountryLookups(CusPersonCountry parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList DataTypes => Parent.CPC_RN_NKCountry == Core.Constants.CountryCodes.SouthAfrica ? Factory.GetCachedValue<ZaDataTypes>() : new CodeDescriptionPairList();

		public override CodeDescriptionPairList DataValues
		{
			get
			{
				return Factory.GetCachedValue("ZACusPersonCountryLookups.DataValues." + Parent.CPC_Type, delegate
				{
					switch (Parent.CPC_Type)
					{
						case ZaDataTypes.Codes.ReasonForMovement:
							return new ZaReasonForMovement();

						case ZaDataTypes.Codes.TravellerType:
							return new ZaTravellerTypes();

						case ZaDataTypes.Codes.Occupation:
							return new ZaOccupations();

						case ZaDataTypes.Codes.TravelDocumentType:
							return new ZaTravelDocumentTypes();

						default:
							return new CodeDescriptionPairList();
					}
				});
			}
		}

		protected new CusPersonCountry Parent => (CusPersonCountry)base.Parent;
	}
}
