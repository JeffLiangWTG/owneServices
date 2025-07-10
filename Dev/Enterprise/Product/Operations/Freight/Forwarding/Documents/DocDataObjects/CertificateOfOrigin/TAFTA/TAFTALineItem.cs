using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class TAFTALineItem : CertificateOfOriginLineItemDocDataObject
	{
		public TAFTALineItem(object id) : base(id)
		{
		}

		#region ProductNumber

		public ZString ProductNumber
		{
			get => productNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ProductNumberInfo, ref productNumber, value))
				{
					Validate(ProductNumberInfo);
				}
			}
		}

		ZString productNumber;

		public ZPropertyInfo ProductNumberInfo => GetZPropertyInfo(nameof(ProductNumber));

		#endregion
	}
}
