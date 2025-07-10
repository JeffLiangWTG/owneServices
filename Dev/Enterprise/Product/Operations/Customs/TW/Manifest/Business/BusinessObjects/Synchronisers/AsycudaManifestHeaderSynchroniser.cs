using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(ASYCUDA.Business.AsycudaManifestHeader destination, Freight.Forwarding.Business.ForwardingConsol sourceConsol) : base(destination, sourceConsol)
		{
		}

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser() => new AsycudaBillCollectionSynchroniser(Destination);

		protected override IZType GetCarrier() => Source.JK_OA_ShippingLineAddress;

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingCarrier() => new[] { Source.JK_OA_ShippingLineAddressInfo };
	}
}
