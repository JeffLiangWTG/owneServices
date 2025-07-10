using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class COOUS : CertificateOfOriginDocDataObject<COOUSLineItem>
	{
		public COOUS(ZString sourceType, ZString sourceId)
			: base(sourceType, sourceId)
		{
		}

		#region Shipment Number

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentNumberInfo);
				}
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

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
