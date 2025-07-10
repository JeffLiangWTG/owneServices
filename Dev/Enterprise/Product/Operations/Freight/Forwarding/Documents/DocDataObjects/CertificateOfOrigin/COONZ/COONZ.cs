using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed public class COONZ : CertificateOfOriginDocDataObject<COONZLineItem>
	{
		public COONZ(ZString sourceType, ZString sourceID) : base(sourceType, sourceID) { }

		#region LineItemOriginCountries

		public ZString LineItemOriginCountries
		{
			get => lineItemOriginCountries;
			set
			{
				if (SetNonPersistentPropertyValue(LineItemOriginCountriesInfo, ref lineItemOriginCountries, value))
				{
					Validate(LineItemOriginCountriesInfo);
				}
			}
		}

		ZString lineItemOriginCountries;

		public ZPropertyInfo LineItemOriginCountriesInfo => GetZPropertyInfo(nameof(LineItemOriginCountries));

		#endregion
	}
}
