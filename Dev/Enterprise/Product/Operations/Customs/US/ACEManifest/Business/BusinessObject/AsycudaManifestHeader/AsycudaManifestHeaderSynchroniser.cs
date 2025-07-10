using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol source)
			: base(destination, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(((AsycudaManifestHeader)Destination).IsExpressCourierInfo, GetIsExpressCourier, GetSourceInfosAffectingIsExpressCourier));
		}

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser((AsycudaManifestHeader)Destination);
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingIsExpressCourier()
		{
			yield return Source.JK_AgentTypeInfo;
		}

		IZType GetIsExpressCourier()
		{
			return (ZBool)(Source.JK_AgentType == Core.Constants.AgentType.Courier);
		}
	}
}
